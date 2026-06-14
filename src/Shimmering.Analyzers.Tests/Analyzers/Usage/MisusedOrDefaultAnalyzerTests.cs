using Shimmering.Analyzers.Analyzers.Usage;
using Shimmering.Analyzers.CodeFixes.Usage;

namespace Shimmering.Analyzers.Tests.Analyzers.Usage;

public class MisusedOrDefaultAnalyzerTests : ShimmeringAnalyzerTests<MisusedOrDefaultAnalyzer>
{
	[Test]
	public Task TestUnsupportedCases() => VerifyAnalyzerAsync(
		"""
		using System;
		using System.Linq;

		namespace Tests
		{
			class Test
			{
				public void Do()
				{
					var x = new[] { 1 }.Single()!; // no OrDefault
					var y = (new[] { 1 }.SingleOrDefault())!; // parenthesized expression
				}
			}
		}
		""");
}
