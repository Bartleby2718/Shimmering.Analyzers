namespace Shimmering.Analyzers.Tests.Analyzers.Linq;

using Shimmering.Analyzers.Analyzers.Linq;
using Shimmering.Analyzers.CodeFixes.Linq;

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
