using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Framework;
using Shimmering.Analyzers.Analyzers.Usage;

namespace Shimmering.Analyzers.Tests.Analyzers.Usage;

public class UnnamedRegexCaptureGroupAnalyzerTests : ShimmeringAnalyzerTests<UnnamedRegexCaptureGroupAnalyzer>
{
	[Test]
	public Task TestFlagRegexConstructor() => VerifyAnalyzerAsync(
		"""
		using System.Text.RegularExpressions;

		class Test
		{
			void Do()
			{
				var regex = [|new Regex(@"(\d{4})-(\d{2})-(\d{2})")|];
			}
		}
		""");

	[Test]
	public Task TestFlagStaticRegexCall() => VerifyAnalyzerAsync(
		"""
		using System.Text.RegularExpressions;

		class Test
		{
			void Do()
			{
				var match = [|Regex.Match("input", @"(\w+)\s+(\w+)")|];
			}
		}
		""");

	[Test]
	public Task TestFlagGeneratedRegex() => VerifyAnalyzerWithNoCompilerErrorsAsync(
		"""
		using System.Text.RegularExpressions;

		partial class Test
		{
			[[|GeneratedRegex(@"(\d+)")|]]
			private partial Regex GetRegex();
		}
		""");

	[Test]
	public Task TestIgnoreExplicitCaptureOption() => VerifyAnalyzerAsync(
		"""
		using System.Text.RegularExpressions;

		class Test
		{
			void Do()
			{
				var regex = new Regex(@"(\d{4})-(\d{2})-(\d{2})", RegexOptions.ExplicitCapture);
			}
		}
		""");

	[Test]
	public Task TestIgnoreAlreadyNamedGroups() => VerifyAnalyzerAsync(
		"""
		using System.Text.RegularExpressions;

		class Test
		{
			void Do()
			{
				var regex = new Regex(@"(?<year>\d{4})-(?<month>\d{2})-(?<day>\d{2})");
			}
		}
		""");

	private static Task VerifyAnalyzerWithNoCompilerErrorsAsync(string source, params DiagnosticResult[] expected)
	{
		var test = new CSharpAnalyzerTest<UnnamedRegexCaptureGroupAnalyzer, DefaultVerifier>
		{
			TestCode = source.Replace("\r\n", "\n").Replace("\n", Environment.NewLine),
			ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
			CompilerDiagnostics = CompilerDiagnostics.None,
		};

		test.ExpectedDiagnostics.AddRange(expected);
		return test.RunAsync();
	}
}
