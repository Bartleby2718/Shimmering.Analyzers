using System.Threading.Tasks;

using NUnit.Framework;

using Shimmering.Analyzers.Analyzers.Usage;
using Shimmering.Analyzers.CodeFixes.Usage;

namespace Shimmering.Analyzers.Tests.Analyzers.Usage;

public class NumericRegexGroupIndexingCodeFixProviderTests : ShimmeringCodeFixProviderTests<NumericRegexGroupIndexingAnalyzer, NumericRegexGroupIndexingCodeFixProvider>
{
	[Test]
	public Task TestCodeFixResolvingToNamedGroup() => VerifyCodeFixAsync(
		"""
		using System.Text.RegularExpressions;

		class Test
		{
			void Do()
			{
				var regex = new Regex(@"(?<year>\d{4})-(?<month>\d{2})");
				var match = regex.Match("2026-06");
				var group = [|match.Groups[1]|];
			}
		}
		""",
		"""
		using System.Text.RegularExpressions;

		class Test
		{
			void Do()
			{
				var regex = new Regex(@"(?<year>\d{4})-(?<month>\d{2})");
				var match = regex.Match("2026-06");
				var group = match.Groups["year"];
			}
		}
		""");

	[Test]
	public Task TestCodeFixFallbackToPlaceholder() => VerifyCodeFixAsync(
		"""
		using System.Text.RegularExpressions;

		class Test
		{
			void Do()
			{
				var regex = new Regex(@"(\d{4})-(\d{2})");
				var match = regex.Match("2026-06");
				var group = [|match.Groups[1]|];
			}
		}
		""",
		"""
		using System.Text.RegularExpressions;

		class Test
		{
			void Do()
			{
				var regex = new Regex(@"(\d{4})-(\d{2})");
				var match = regex.Match("2026-06");
				var group = match.Groups["group1"];
			}
		}
		""");

	[Test]
	public Task TestCodeFixFallbackToVariableName() => VerifyCodeFixAsync(
		"""
		using System.Text.RegularExpressions;

		class Test
		{
			void Do()
			{
				var regex = new Regex(@"(\d{4})-(\d{2})");
				var match = regex.Match("2026-06");
				var year = [|match.Groups[1]|].Value;
			}
		}
		""",
		"""
		using System.Text.RegularExpressions;

		class Test
		{
			void Do()
			{
				var regex = new Regex(@"(\d{4})-(\d{2})");
				var match = regex.Match("2026-06");
				var year = match.Groups["year"].Value;
			}
		}
		""");
}
