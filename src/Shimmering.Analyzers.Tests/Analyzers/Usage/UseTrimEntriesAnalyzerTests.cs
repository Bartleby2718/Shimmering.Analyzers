using Shimmering.Analyzers.Analyzers.Usage;

namespace Shimmering.Analyzers.Tests.Analyzers.Usage;

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

	[Test]
	public Task TestFlagSplitSelectTrim() => VerifyAnalyzerAsync(
		"""
		using System;
		using System.Linq;

		namespace Tests
		{
			class Test
			{
				void Do(string input)
				{
					var x = [|input.Split(',').Select(x => x.Trim())|];
				}
			}
		}
		""");

	[Test]
	public Task TestFlagSplitSelectTrimToArray() => VerifyAnalyzerAsync(
		"""
		using System;
		using System.Linq;

		namespace Tests
		{
			class Test
			{
				void Do(string input)
				{
					var x = [|input.Split(',').Select(x => x.Trim()).ToArray()|];
				}
			}
		}
		""");

	[Test]
	public Task TestIgnoreDifferentSelectLambda() => VerifyAnalyzerAsync(
		"""
		using System;
		using System.Linq;

		namespace Tests
		{
			class Test
			{
				void Do(string input)
				{
					var x = input.Split(',').Select(x => x.ToUpper());
				}
			}
		}
		""");
}
