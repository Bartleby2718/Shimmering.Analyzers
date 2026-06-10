using Shimmering.Analyzers.UsageRules.ListOrListReturningMethodFollowedByToList;

namespace Shimmering.Analyzers.Tests.UsageRules.ListOrListReturningMethodFollowedByToList;

using Verifier = CSharpAnalyzerVerifier<
	ListOrListReturningMethodFollowedByToListAnalyzer,
	DefaultVerifier>;

public class ListOrListReturningMethodFollowedByToListAnalyzerTests : ShimmeringAnalyzerTests<ListOrListReturningMethodFollowedByToListAnalyzer>
{
	[Test]
	public Task TestToListOnParameter() => Verifier.VerifyAnalyzerAsync(
		"""
		using System.Collections.Generic;
		using System.Linq;

		namespace Tests
		{
			class Test
			{
				public void Do(List<int> list)
				{
					var copy = list.ToList();
				}
			}
		}
		""");
}
