using Microsoft.CodeAnalysis.Formatting;

using Shimmering.Analyzers.UsageRules.UniqueNonSetCollection;

namespace Shimmering.Analyzers.Tests.UsageRules.UniqueNonSetCollection;

using Verifier = CSharpCodeFixVerifier<
	UniqueNonSetCollectionAnalyzer,
	UniqueNonSetCollectionCodeFixProvider,
	DefaultVerifier>;

public class UniqueNonSetCollectionCodeFixProviderTests : ShimmeringCodeFixProviderTests<UniqueNonSetCollectionAnalyzer, UniqueNonSetCollectionCodeFixProvider>
{
	[Test]
	public Task TestToArray() => Verifier.VerifyCodeFixAsync(
		"""
		using System;
		using System.Collections.Generic;
		using System.Linq;

		namespace Tests
		{
			class Test
			{
				IEnumerable<int> _field = [|new[] { 1, 2 }.Distinct().ToArray()|];
				ICollection<double> ReadOnlyProperty => [|new[] { 1.0, 2 }.Distinct().ToArray()|];
				IEnumerable<string> MethodWithFatArrow() => [|new[] { "a", "b" }.Distinct().ToArray()|];
				IReadOnlyCollection<char> MethodWithReturn() { return [|new[] { 'a', 'b' }.Distinct().ToArray()|]; }
				void Method()
				{
					var collection = [|new List<bool>().Distinct().ToArray()|];
					DummyMethod(collection);
				}

				static void DummyMethod<T>(IReadOnlyCollection<T> collection) { }
			}
		}
		""",
		"""
		using System;
		using System.Collections.Generic;
		using System.Linq;

		namespace Tests
		{
			class Test
			{
				IEnumerable<int> _field = new[] { 1, 2 }.ToHashSet();
				ICollection<double> ReadOnlyProperty => new[] { 1.0, 2 }.ToHashSet();
				IEnumerable<string> MethodWithFatArrow() => new[] { "a", "b" }.ToHashSet();
				IReadOnlyCollection<char> MethodWithReturn() { return new[] { 'a', 'b' }.ToHashSet(); }
				void Method()
				{
					var collection = new List<bool>().ToHashSet();
					DummyMethod(collection);
				}

				static void DummyMethod<T>(IReadOnlyCollection<T> collection) { }
			}
		}
		""");

	[Test]
	public Task TestToList() => Verifier.VerifyCodeFixAsync(
		"""
		using System;
		using System.Collections.Generic;
		using System.Linq;

		namespace Tests
		{
			class Test
			{
				IEnumerable<int> _field = [|new[] { 1, 2 }.Distinct().ToList()|];
				ICollection<double> ReadOnlyProperty => [|new[] { 1.0, 2 }.Distinct().ToList()|];
				IEnumerable<string> MethodWithFatArrow() => [|new[] { "a", "b" }.Distinct().ToList()|];
				IReadOnlyCollection<char> MethodWithReturn() { return [|new[] { 'a', 'b' }.Distinct().ToList()|]; }
				void Method()
				{
					var collection = [|new List<int>().Distinct().ToList()|];
					DummyMethod(collection);
				}

				static void DummyMethod<T>(IReadOnlyCollection<T> collection) { }
			}
		}
		""",
		"""
		using System;
		using System.Collections.Generic;
		using System.Linq;

		namespace Tests
		{
			class Test
			{
				IEnumerable<int> _field = new[] { 1, 2 }.ToHashSet();
				ICollection<double> ReadOnlyProperty => new[] { 1.0, 2 }.ToHashSet();
				IEnumerable<string> MethodWithFatArrow() => new[] { "a", "b" }.ToHashSet();
				IReadOnlyCollection<char> MethodWithReturn() { return new[] { 'a', 'b' }.ToHashSet(); }
				void Method()
				{
					var collection = new List<int>().ToHashSet();
					DummyMethod(collection);
				}

				static void DummyMethod<T>(IReadOnlyCollection<T> collection) { }
			}
		}
		""");

	/// <summary>
	/// Spaces are used by default in Roslyn, so tabs are being converted to spaces in this test.
	/// To test  <see cref="CSharpCodeFixTest{TAnalyzer, TCodeFix, TVerifier}"/> instead of <see cref="CSharpCodeFixVerifier{TAnalyzer, TCodeFix, TVerifier}"/>.
	/// </summary>
	[Test]
	public async Task TestLeadingTabsArePreservedWhenInvocationsAreOnDifferentLines()
	{
		var test = new CSharpCodeFixTest<UniqueNonSetCollectionAnalyzer, UniqueNonSetCollectionCodeFixProvider, DefaultVerifier>
		{
			TestCode = """
				using System;
				using System.Collections.Generic;
				using System.Linq;

				namespace Tests
				{
					class Test
					{
						IEnumerable<int> _fieldWithComments = new[] { 1, 2 }
							// a
							.Distinct() // b
							// c
							.ToArray(); // d

						IReadOnlyCollection<int> _fieldWithoutComments = new[] { 1, 2 }
							.Distinct()
							.ToList();
					}
				}
				""",
			FixedCode = """
				using System;
				using System.Collections.Generic;
				using System.Linq;

				namespace Tests
				{
					class Test
					{
						IEnumerable<int> _fieldWithComments = new[] { 1, 2 }
							// a
							// b
							// c
							.ToHashSet(); // d
				
						IReadOnlyCollection<int> _fieldWithoutComments = new[] { 1, 2 }
							.ToHashSet();
					}
				}
				""",
		};

		test.TestState.ExpectedDiagnostics.Add(new DiagnosticResult(
			DiagnosticIds.UsageRules.UniqueNonSetCollection,
			DiagnosticSeverity.Hidden)
			.WithSpan(9, 41, 13, 14));
		test.TestState.ExpectedDiagnostics.Add(new DiagnosticResult(
			DiagnosticIds.UsageRules.UniqueNonSetCollection,
			DiagnosticSeverity.Hidden)
			.WithSpan(15, 52, 17, 13));

		test.SolutionTransforms.Add((solution, projectId) =>
		{
			var project = solution.GetProject(projectId)!;
			var options = project.Solution.Workspace.Options
				.WithChangedOption(FormattingOptions.UseTabs, LanguageNames.CSharp, value: true);
			solution.Workspace.TryApplyChanges(solution.Workspace.CurrentSolution.WithOptions(options));
			return solution;
		});
		await test.RunAsync();
	}

	[Test]
#pragma warning disable SA1027 // Use tabs correctly
	public Task TestLeadingSpacesArePreservedWhenInvocationsAreOnDifferentLines() => Verifier.VerifyCodeFixAsync(
		"""
        using System;
        using System.Collections.Generic;
        using System.Linq;
        
        namespace Tests
        {
            class Test
            {
                IEnumerable<int> _field = [|new[] { 1, 2 }
                                          .Distinct()
                                          .ToList()|];
            }
        }
        """,
		"""
        using System;
        using System.Collections.Generic;
        using System.Linq;

        namespace Tests
        {
            class Test
            {
                IEnumerable<int> _field = new[] { 1, 2 }
                                          .ToHashSet();
            }
        }
        """);
#pragma warning restore SA1027 // Use tabs correctly
}
