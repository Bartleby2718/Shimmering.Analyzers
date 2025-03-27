using Shimmering.Analyzers.UsageRules.ToArrayOrToListFollowedByLinqMethod;

namespace Shimmering.Analyzers.Tests.UsageRules.ToArrayOrToListFollowedByLinqMethod;

using Verifier = CSharpAnalyzerVerifier<
	ToArrayOrToListFollowedByLinqMethodAnalyzer,
	DefaultVerifier>;

public class ToArrayOrToListFollowedByLinqMethodAnalyzerTests : ShimmeringAnalyzerTests<ToArrayOrToListFollowedByLinqMethodAnalyzer>
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
					numbers.ToList().Sort(); // Sort is not an LINQ method
					var queryable = Enumerable.Empty<int>()
						.AsQueryable()
						.ToList()
						.Where(_ => true);
				}
			}
		}
		""");
}
