using Shimmering.Analyzers.Analyzers.Usage;
using Shimmering.Analyzers.CodeFixes.Usage;

namespace Shimmering.Analyzers.Tests.Analyzers.Usage;

public class ArrayOrArrayReturningMethodFollowedByToArrayAnalyzerTests : ShimmeringAnalyzerTests<ArrayOrArrayReturningMethodFollowedByToArrayAnalyzer>
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
					var list = "d".Split(' ').ToList(); // array followed by ToList
					var array = "d".ToList().ToArray(); // list-returning method followed by ToArray
				}
			}
		}
		""");
}
