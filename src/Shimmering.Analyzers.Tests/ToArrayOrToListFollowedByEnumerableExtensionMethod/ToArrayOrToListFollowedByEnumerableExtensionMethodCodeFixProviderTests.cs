using Shimmering.Analyzers.ToArrayOrToListFollowedByEnumerableExtensionMethod;

namespace Shimmering.Analyzers.Tests.ToArrayOrToListFollowedByEnumerableExtensionMethod;

using Verifier = CSharpCodeFixVerifier<
	ToArrayOrToListFollowedByEnumerableExtensionMethodAnalyzer,
	ToArrayOrToListFollowedByEnumerableExtensionMethodCodeFixProvider,
	DefaultVerifier>;

public class ToArrayOrToListFollowedByEnumerableExtensionMethodCodeFixProviderTests
{
	[Test]
	public Task TestUnsupportedCases() => Verifier.VerifyAnalyzerAsync(
		"""
		using System;
		using System.Linq;

		namespace Tests
		{
			class Test
			{
				public void Do()
				{
					int[] numbers = [];
					var greaterThanThree = numbers.ToHashSet().Where(x => x > 3); // ToHashSet
					numbers.ToList().Sort(); // Sort is not an Enumerable extension method
					var queryable = Enumerable.Empty<int>()
						.AsQueryable()
						.ToList()
						.Where(_ => true);
				}
			}
		}
		""");

	[Test]
	public Task TestToArrayIsRemoved() => Verifier.VerifyCodeFixAsync(
		"""
		using System;
		using System.Linq;

		namespace Tests
		{
			class Test
			{
				public void Do()
				{
					int[] numbers = [];
					var greaterThanThree = numbers.[|ToArray|]().Where(x => x > 3);
				}
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
				public void Do()
				{
					int[] numbers = [];
					var greaterThanThree = numbers.Where(x => x > 3);
				}
			}
		}
		""");

	[Test]
	public Task TestToListIsRemoved() => Verifier.VerifyCodeFixAsync(
		"""
		using System;
		using System.Linq;

		namespace Tests
		{
			class Test
			{
				public void Do()
				{
					int[] numbers = [];
					var squares = numbers.[|ToList|]().Select(x => x * x).ToArray();
				}
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
				public void Do()
				{
					int[] numbers = [];
					var squares = numbers.Select(x => x * x).ToArray();
				}
			}
		}
		""");

	[Test]
	public Task TestTrivia() => Verifier.VerifyCodeFixAsync(
		"""
		using System;
		using System.Linq;

		namespace Tests
		{
			class Test
			{
				public void Do()
				{
					int[] numbers = [];
					// line before source
					var squares = numbers // right after source
						// line before ToList
						.[|ToList|]() // right after ToList
						// line before Select
						.Select(x => x * x) // right after Select
						// line before ToArray
						.ToArray();
				}
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
				public void Do()
				{
					int[] numbers = [];
					// line before source
					var squares = numbers // right after source
						// line before Select
						.Select(x => x * x) // right after Select
						// line before ToArray
						.ToArray();
				}
			}
		}
		""");
}
