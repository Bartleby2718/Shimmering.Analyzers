using Shimmering.Analyzers.UsageRules.MisusedOrDefault;

namespace Shimmering.Analyzers.Tests.UsageRules.MisusedOrDefault;

using Verifier = CSharpAnalyzerVerifier<
	MisusedOrDefaultAnalyzer,
	DefaultVerifier>;

public class MisusedOrDefaultAnalyzerTests
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
					var x = new[] { 1 }.Single()!; // no OrDefault
					var y = (new[] { 1 }.SingleOrDefault())!; // parenthesized expression
				}
			}
		}
		""");
}
