namespace Shimmering.Analyzers.Tests.Analyzers.Usage;

using Shimmering.Analyzers.Analyzers.Usage;
using Shimmering.Analyzers.CodeFixes.Usage;

public class UseTrimEntriesAnalyzerTests : ShimmeringAnalyzerTests<UseTrimEntriesAnalyzer>
{
	[Test]
	public Task TestNoSplitNoSelect() => VerifyAnalyzerAsync(
		"""
		using System.Linq;

		namespace Tests
		{
			class Test
			{
				public void Do(string[] input)
				{
					var x = input.Select(x => x.Trim());
				}
			}
		}
		""");
}
