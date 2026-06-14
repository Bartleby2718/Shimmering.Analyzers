using System.Threading.Tasks;
using NUnit.Framework;
using Shimmering.Analyzers.Analyzers.Usage;
using Shimmering.Analyzers.CodeFixes.Usage;

namespace Shimmering.Analyzers.Tests.Analyzers.Usage;

public class UnnamedRegexCaptureGroupCodeFixProviderTests : ShimmeringCodeFixProviderTests<UnnamedRegexCaptureGroupAnalyzer, UnnamedRegexCaptureGroupCodeFixProvider>
{
	[Test]
	public Task TestCodeFixForConstructor() => VerifyCodeFixAsync(
		"""
		using System.Text.RegularExpressions;

		class Test
		{
			void Do()
			{
				var regex = [|new Regex(@"(\d{4})-(\d{2})-(\d{2})")|];
			}
		}
		""",
		"""
		using System.Text.RegularExpressions;

		class Test
		{
			void Do()
			{
				var regex = new Regex(@"(?<group1>\d{4})-(?<group2>\d{2})-(?<group3>\d{2})");
			}
		}
		""");

	[Test]
	public Task TestCodeFixForStaticCall() => VerifyCodeFixAsync(
		"""
		using System.Text.RegularExpressions;

		class Test
		{
			void Do()
			{
				var match = [|Regex.Match("input", @"(\w+)\s+(\w+)")|];
			}
		}
		""",
		"""
		using System.Text.RegularExpressions;

		class Test
		{
			void Do()
			{
				var match = Regex.Match("input", @"(?<group1>\w+)\s+(?<group2>\w+)");
			}
		}
		""");
}
