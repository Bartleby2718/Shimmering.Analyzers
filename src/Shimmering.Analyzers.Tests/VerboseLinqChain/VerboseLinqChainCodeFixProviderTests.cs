using Shimmering.Analyzers.VerboseLinqChain;

namespace Shimmering.Analyzers.Tests.VerboseLinqChain;

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
				}
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
}
