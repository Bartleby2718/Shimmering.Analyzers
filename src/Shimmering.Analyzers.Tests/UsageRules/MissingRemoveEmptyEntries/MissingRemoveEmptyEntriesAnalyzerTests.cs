using Shimmering.Analyzers.UsageRules.MissingRemoveEmptyEntries;

namespace Shimmering.Analyzers.Tests.UsageRules.MissingRemoveEmptyEntries;

using Verifier = CSharpAnalyzerVerifier<
	MissingRemoveEmptyEntriesAnalyzer,
	DefaultVerifier>;

public class MissingRemoveEmptyEntriesAnalyzerTests : ShimmeringAnalyzerTests<MissingRemoveEmptyEntriesAnalyzer>
{
	[Test]
	public Task TestShouldNotFlagRemoveEmptyEntries() => Verifier.VerifyAnalyzerAsync(
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
