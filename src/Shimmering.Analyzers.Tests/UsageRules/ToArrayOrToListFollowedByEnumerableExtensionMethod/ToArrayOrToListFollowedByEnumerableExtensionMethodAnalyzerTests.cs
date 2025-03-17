using Shimmering.Analyzers.UsageRules.ToArrayOrToListFollowedByEnumerableExtensionMethod;

namespace Shimmering.Analyzers.Tests.UsageRules.ToArrayOrToListFollowedByEnumerableExtensionMethod;

using Verifier = CSharpAnalyzerVerifier<
	ToArrayOrToListFollowedByEnumerableExtensionMethodAnalyzer,
	DefaultVerifier>;

public class ToArrayOrToListFollowedByEnumerableExtensionMethodAnalyzerTests : ShimmeringAnalyzerTests<ToArrayOrToListFollowedByEnumerableExtensionMethodAnalyzer>
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
}
