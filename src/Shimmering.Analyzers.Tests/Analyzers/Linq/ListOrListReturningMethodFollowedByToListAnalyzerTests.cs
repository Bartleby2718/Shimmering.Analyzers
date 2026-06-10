namespace Shimmering.Analyzers.Tests.Analyzers.Linq;

using Shimmering.Analyzers.Analyzers.Linq;
using Shimmering.Analyzers.CodeFixes.Linq;

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
