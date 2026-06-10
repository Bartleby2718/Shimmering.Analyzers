namespace Shimmering.Analyzers.Tests.Analyzers.Linq;

using Shimmering.Analyzers.Analyzers.Linq;
using Shimmering.Analyzers.CodeFixes.Linq;

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
