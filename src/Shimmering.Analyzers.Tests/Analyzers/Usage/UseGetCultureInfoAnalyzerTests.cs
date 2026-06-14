using System.Threading.Tasks;

using NUnit.Framework;

using Shimmering.Analyzers.Analyzers.Usage;

namespace Shimmering.Analyzers.Tests.Analyzers.Usage;

public class UseGetCultureInfoAnalyzerTests : ShimmeringAnalyzerTests<UseGetCultureInfoAnalyzer>
{
	[Test]
	public Task TestFlagCultureInfoStringConstructor() => VerifyAnalyzerAsync(
		"""
		using System.Globalization;

		namespace Tests
		{
			class Test
			{
				void Do()
				{
					var culture = [|new CultureInfo("en-US")|];
				}
			}
		}
		""");

	[Test]
	public Task TestFlagCultureInfoLcidConstructor() => VerifyAnalyzerAsync(
		"""
		using System.Globalization;

		namespace Tests
		{
			class Test
			{
				void Do()
				{
					var culture = [|new CultureInfo(1033)|];
				}
			}
		}
		""");

	[Test]
	public Task TestFlagCultureInfoWithOverrideFalse() => VerifyAnalyzerAsync(
		"""
		using System.Globalization;

		namespace Tests
		{
			class Test
			{
				void Do()
				{
					var culture = [|new CultureInfo("en-US", false)|];
				}
			}
		}
		""");

	[Test]
	public Task TestIgnoreCultureInfoWithOverrideTrue() => VerifyAnalyzerAsync(
		"""
		using System.Globalization;

		namespace Tests
		{
			class Test
			{
				void Do()
				{
					var culture = new CultureInfo("en-US", true);
				}
			}
		}
		""");

	[Test]
	public Task TestIgnoreWhenMutatedLocally() => VerifyAnalyzerAsync(
		"""
		using System.Globalization;

		namespace Tests
		{
			class Test
			{
				void Do()
				{
					var culture = new CultureInfo("en-US");
					culture.DateTimeFormat.ShortDatePattern = "dd-MM-yyyy";
				}
			}
		}
		""");

	[Test]
	public Task TestIgnoreWhenEscapingViaMethodCallArgument() => VerifyAnalyzerAsync(
		"""
		using System.Globalization;

		namespace Tests
		{
			class Test
			{
				void Consume(CultureInfo culture) {}

				void Do()
				{
					var culture = new CultureInfo("en-US");
					Consume(culture);
				}
			}
		}
		""");

	[Test]
	public Task TestIgnoreWhenEscapingViaReturn() => VerifyAnalyzerAsync(
		"""
		using System.Globalization;

		namespace Tests
		{
			class Test
			{
				CultureInfo Do()
				{
					var culture = new CultureInfo("en-US");
					return culture;
				}
			}
		}
		""");

	[Test]
	public Task TestIgnoreWhenEscapingViaFieldAssignment() => VerifyAnalyzerAsync(
		"""
		using System.Globalization;

		namespace Tests
		{
			class Test
			{
				private CultureInfo field;

				void Do()
				{
					var culture = new CultureInfo("en-US");
					field = culture;
				}
			}
		}
		""");

	[Test]
	public Task TestIgnoreWhenEscapingViaLambdaCapture() => VerifyAnalyzerAsync(
		"""
		using System.Globalization;
		using System.Threading.Tasks;

		namespace Tests
		{
			class Test
			{
				void Do()
				{
					var culture = new CultureInfo("en-US");
					Task.Run(() => {
						var name = culture.Name;
					});
				}
			}
		}
		""");

	[Test]
	public Task TestFlagInlineUsageAsArgument() => VerifyAnalyzerAsync(
		"""
		using System;
		using System.Globalization;

		namespace Tests
		{
			class Test
			{
				void Do()
				{
					var text = 123.ToString([|new CultureInfo("en-US")|]);
				}
			}
		}
		""");
}
