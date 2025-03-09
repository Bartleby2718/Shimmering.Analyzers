using Shimmering.Analyzers.StyleRules.VerboseLinqChain;

namespace Shimmering.Analyzers.Tests.StyleRules.VerboseLinqChain;

using Verifier = CSharpCodeFixVerifier<
	VerboseLinqChainAnalyzer,
	VerboseLinqChainCodeFixProvider,
	DefaultVerifier>;

public class VerboseLinqChainCodeFixProviderTests
{
	[Test]
	public Task TestUnsupportedCases() => Verifier.VerifyAnalyzerAsync(
		"""
		using System.Linq;

		namespace Tests
		{
			class Test
			{
				void Method()
				{
					// no LINQ calls before ToArray
					var x1 = new[] { 1 }.ToArray();

					// irrelevant LINQ calls only
					var x2 = new[] { 1 }.Distinct().ToArray();
					var x3 = new[] { 1 }.Except(new[] { 2 }).ToArray();
					var x4 = new[] { 1 }.Skip(0).ToArray();

					// relevant LINQ calls followed by irrelevant LINQ calls
					var x5 = new[] { 1 }.Append(2).Intersect(new[] { 2 }).ToArray();
					var x6 = new[] { 1 }.Concat(new[] { 2 }).OfType<int>().ToArray();
					var x7 = new[] { 1 }.Prepend(2).Take(1).ToArray();

					// tuple declaration is not supported
					var (x8, x9) = (new[] { 1 }.Append(2).ToArray(), new[] { 1 }.Append(2).ToArray());
				}
			}
		}
		""");

	[Test]
	public Task TestVariableDeclarationWithExplicitType() => Verifier.VerifyCodeFixAsync(
		"""
		using System.Linq;

		namespace Tests
		{
			class Test
			{
				void Method()
				{
					int[] array = [|new[] { 1 }.Prepend(2).Prepend(3).ToArray()|];
				}
			}
		}
		""",
		"""
		using System.Linq;

		namespace Tests
		{
			class Test
			{
				void Method()
				{
					int[] array = [3, 2, .. new[] { 1 }];
				}
			}
		}
		""");

	[Test]
	public Task TestArgumentOfInterfaceType() => Verifier.VerifyCodeFixAsync(
		"""
		using System.Collections.Generic;
		using System.Linq;

		namespace Tests
		{
			class Test
			{
				void Method()
				{
					AnotherMethod([|new[] { 1 }.Prepend(2).Prepend(3).ToArray()|]);
				}

				void AnotherMethod(IReadOnlyCollection<int> numbers) { }
			}
		}
		""",
		"""
		using System.Collections.Generic;
		using System.Linq;

		namespace Tests
		{
			class Test
			{
				void Method()
				{
					AnotherMethod([3, 2, .. new[] { 1 }]);
				}

				void AnotherMethod(IReadOnlyCollection<int> numbers) { }
			}
		}
		""");

	[Test]
	public Task TestArrayOfBuiltInType() => Verifier.VerifyCodeFixAsync(
		"""
		using System.Linq;

		namespace Tests
		{
			class Test
			{
				void Method()
				{
					var x = [|new[] { 1 }.Prepend(2).Prepend(3).ToArray()|];
				}
			}
		}
		""",
		"""
		using System.Linq;

		namespace Tests
		{
			class Test
			{
				void Method()
				{
					int[] x = [3, 2, .. new[] { 1 }];
				}
			}
		}
		""");

	[Test]
	public Task TestListOfBuiltInType() => Verifier.VerifyCodeFixAsync(
		"""
		using System.Linq;

		namespace Tests
		{
			class Test
			{
				void Method()
				{
					var x = [|new[] { 1 }.Append(2).Concat(new[] { 3 }).Append(4).Append(5).Concat(new int[] { 6 }).ToList()|];
				}
			}
		}
		""",
		"""
		using System.Linq;
		using System.Collections.Generic;

		namespace Tests
		{
			class Test
			{
				void Method()
				{
					List<int> x = [.. new[] { 1 }, 2, .. new[] { 3 }, 4, 5, .. new int[] { 6 }];
				}
			}
		}
		""");

	[Test]
	public Task TestHashSetOfBuiltInType() => Verifier.VerifyCodeFixAsync(
		"""
		using System.Linq;

		namespace Tests
		{
			class Test
			{
				void Method()
				{
					var x = [|new[] { 1 }.Concat(new int[] { 2, 3 }).Concat(new[] { 4, 5 }).ToHashSet()|];
				}
			}
		}
		""",
		"""
		using System.Linq;
		using System.Collections.Generic;

		namespace Tests
		{
			class Test
			{
				void Method()
				{
					HashSet<int> x = [.. new[] { 1 }, .. new int[] { 2, 3 }, .. new[] { 4, 5 }];
				}
			}
		}
		""");

	[Test]
	public Task TestUsingDirectiveIsAddedIfSystemCollectionsGenericIsMissing() => Verifier.VerifyCodeFixAsync(
		"""
		using System.Linq;

		namespace Tests
		{
			class Test
			{
				void Method()
				{
					var x = [|new[] { 1 }.Append(2).Append(3).ToList()|];
				}
			}
		}
		""",
		"""
		using System.Linq;
		using System.Collections.Generic;

		namespace Tests
		{
			class Test
			{
				void Method()
				{
					List<int> x = [.. new[] { 1 }, 2, 3];
				}
			}
		}
		""");

	[Test]
	public Task TestInvocationAsFirstElementInTheLinqChain() => Verifier.VerifyCodeFixAsync(
		"""
		using System.Linq;

		namespace Tests
		{
			class Test
			{
				void Method()
				{
					var x = [|Enumerable.Empty<int>().Append(123).ToArray()|];
				}
			}
		}
		""",
		"""
		using System.Linq;

		namespace Tests
		{
			class Test
			{
				void Method()
				{
					int[] x = [.. Enumerable.Empty<int>(), 123];
				}
			}
		}
		""");

	[Test]
#pragma warning disable SA1027 // Use tabs correctly
	public Task TestTriviaForArgument() => Verifier.VerifyCodeFixAsync(
		"""
		using System;
		using System.Linq;

		namespace Tests
		{
		    class Test
		    {
		        void Method()
		        {
		            AnotherMethod(/* before argument */[|Enumerable.Empty<int>()/* after first element */
		                // line before Append
		                .Append(123) // right after Append
		                // line before Concat
		                .Concat(Array.Empty<int>()) // right after Concat
		                // line before ToArray
		                .ToArray()|]/* after argument */);
		        }

		        void AnotherMethod(int[] numbers) { }
		    }
		}
		""",
		"""
		using System;
		using System.Linq;

		namespace Tests
		{
		    class Test
		    {
		        void Method()
		        {
		            AnotherMethod(/* before argument */[.. Enumerable.Empty<int>(),/* after first element */
		                // line before Append
		                123, // right after Append
		                // line before Concat
		                .. Array.Empty<int>()]/* after argument */);
		        }

		        void AnotherMethod(int[] numbers) { }
		    }
		}
		""");
#pragma warning restore SA1027 // Use tabs correctly

	[Test]
#pragma warning disable SA1027 // Use tabs correctly
	public Task TestTriviaForExplicitType() => Verifier.VerifyCodeFixAsync(
		"""
		using System.Linq;

		namespace Tests
		{
		    class Test
		    {
		        void Method()
		        {
		            // line before variable declaration
		            /* before explicit type*/ int[] /* between type and variable */ x = /* before invocation */[|Enumerable.Empty<int>()
		                // line before Append
		                .Append(123) // right after Append
		                // line before ToArray
		                .ToArray()|]/* after invocation */; // right after variable declaration
		        }
		    }
		}
		""",
		"""
		using System.Linq;

		namespace Tests
		{
		    class Test
		    {
		        void Method()
		        {
		            // line before variable declaration
		            /* before explicit type*/ int[] /* between type and variable */ x = /* before invocation */[.. Enumerable.Empty<int>(),
		                                          // line before Append
		                                          123]/* after invocation */; // right after variable declaration
		        }
		    }
		}
		""");
#pragma warning restore SA1027 // Use tabs correctly

	[Test]
#pragma warning disable SA1027 // Use tabs correctly
	public Task TestTriviaForImplicitType() => Verifier.VerifyCodeFixAsync(
		"""
		using System.Linq;

		namespace Tests
		{
		    class Test
		    {
		        void Method()
		        {
		            // line before variable declaration
		            /* before implicit type*/ var /* between type and variable */ x = /* before invocation */[|Enumerable.Empty<int>()
		                // line before Append
		                .Append(123) // right after Append
		                // line before ToArray
		                .ToArray()|]/* after invocation */; // right after variable declaration
		        }
		    }
		}
		""",
		"""
		using System.Linq;

		namespace Tests
		{
		    class Test
		    {
		        void Method()
		        {
		            // line before variable declaration
		            /* before implicit type*/ int[] /* between type and variable */ x = /* before invocation */[.. Enumerable.Empty<int>(),
		                                          // line before Append
		                                          123]/* after invocation */; // right after variable declaration
		        }
		    }
		}
		""");
#pragma warning restore SA1027 // Use tabs correctly
}
