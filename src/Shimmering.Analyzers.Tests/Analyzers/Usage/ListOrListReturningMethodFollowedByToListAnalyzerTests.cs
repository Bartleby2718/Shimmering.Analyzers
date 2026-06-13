namespace Shimmering.Analyzers.Tests.Analyzers.Usage;

using Shimmering.Analyzers.Analyzers.Usage;
using Shimmering.Analyzers.CodeFixes.Usage;

public class ListOrListReturningMethodFollowedByToListAnalyzerTests : ShimmeringAnalyzerTests<ListOrListReturningMethodFollowedByToListAnalyzer>
{
	[Test]
	public Task TestToListOnParameter() => VerifyAnalyzerAsync(
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
