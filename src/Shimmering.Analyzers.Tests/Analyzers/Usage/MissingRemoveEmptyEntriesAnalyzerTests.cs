namespace Shimmering.Analyzers.Tests.Analyzers.Usage;

using Shimmering.Analyzers.Analyzers.Usage;
using Shimmering.Analyzers.CodeFixes.Usage;

public class MissingRemoveEmptyEntriesAnalyzerTests : ShimmeringAnalyzerTests<MissingRemoveEmptyEntriesAnalyzer>
{
	[Test]
	public Task TestShouldNotFlagRemoveEmptyEntries() => VerifyAnalyzerAsync(
		"""
		using System;
		using System.Linq;
		
		namespace Tests
		{
			class Test
			{
				void Do(string input)
				{
					var x = input.Split(' ', StringSplitOptions.RemoveEmptyEntries)
						.Where(x => x.Length > 0);
				}
			}
		}
		""");
}
