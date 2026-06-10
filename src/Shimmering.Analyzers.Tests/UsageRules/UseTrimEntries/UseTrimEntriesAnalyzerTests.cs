using Shimmering.Analyzers.UsageRules.UseTrimEntries;

namespace Shimmering.Analyzers.Tests.UsageRules.UseTrimEntries;

using Verifier = CSharpAnalyzerVerifier<
	UseTrimEntriesAnalyzer,
	DefaultVerifier>;

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
