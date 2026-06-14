using System.Threading.Tasks;
using NUnit.Framework;
using Shimmering.Analyzers.Analyzers.Usage;

namespace Shimmering.Analyzers.Tests.Analyzers.Usage;

public class NumericRegexGroupIndexingAnalyzerTests : ShimmeringAnalyzerTests<NumericRegexGroupIndexingAnalyzer>
{
	[Test]
	public Task TestFlagNumericIndexerAccess() => VerifyAnalyzerAsync(
		"""
		using System.Text.RegularExpressions;

		class Test
		{
			void Do(Match match)
			{
				var group = [|match.Groups[1]|];
			}
		}
		""");

	[Test]
	public Task TestIgnoreIndexZeroAccess() => VerifyAnalyzerAsync(
		"""
		using System.Text.RegularExpressions;

		class Test
		{
			void Do(Match match)
			{
				var group = match.Groups[0];
			}
		}
		""");

	[Test]
	public Task TestIgnoreStringIndexerAccess() => VerifyAnalyzerAsync(
		"""
		using System.Text.RegularExpressions;

		class Test
		{
			void Do(Match match)
			{
				var group = match.Groups["year"];
			}
		}
		""");
}
