using Shimmering.Analyzers.UsageRules.ArrayOrArrayReturningMethodFollowedByToArray;

namespace Shimmering.Analyzers.Tests.UsageRules.ArrayOrArrayReturningMethodFollowedByToArray;

using Verifier = CSharpAnalyzerVerifier<
	ArrayOrArrayReturningMethodFollowedByToArrayAnalyzer,
	DefaultVerifier>;

public class ArrayOrArrayReturningMethodFollowedByToArrayAnalyzerTests : ShimmeringAnalyzerTests<ArrayOrArrayReturningMethodFollowedByToArrayAnalyzer>
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
					var list = "d".Split(' ').ToList(); // array followed by ToList
					var array = "d".ToList().ToArray(); // list-returning method followed by ToArray
				}
			}
		}
		""");
}
