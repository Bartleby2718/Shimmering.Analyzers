using System.Threading.Tasks;

using NUnit.Framework;

using Shimmering.Analyzers.Analyzers.Usage;
using Shimmering.Analyzers.CodeFixes.Usage;

namespace Shimmering.Analyzers.Tests.Analyzers.Usage;

public class UseGetCultureInfoCodeFixProviderTests : ShimmeringCodeFixProviderTests<UseGetCultureInfoAnalyzer, UseGetCultureInfoCodeFixProvider>
{
	[Test]
	public Task TestCodeFixForStringConstructor() => VerifyCodeFixAsync(
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
		""",
		"""
		using System.Globalization;

		namespace Tests
		{
			class Test
			{
				void Do()
				{
					var culture = CultureInfo.GetCultureInfo("en-US");
				}
			}
		}
		""");

	[Test]
	public Task TestCodeFixForStringAndBoolOverrideConstructor() => VerifyCodeFixAsync(
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
		""",
		"""
		using System.Globalization;

		namespace Tests
		{
			class Test
			{
				void Do()
				{
					var culture = CultureInfo.GetCultureInfo("en-US");
				}
			}
		}
		""");

	[Test]
	public Task TestCodeFixForEmptyStringConstructor() => VerifyCodeFixAsync(
		"""
		using System.Globalization;

		namespace Tests
		{
			class Test
			{
				void Do()
				{
					var culture = [|new CultureInfo("")|];
				}
			}
		}
		""",
		"""
		using System.Globalization;

		namespace Tests
		{
			class Test
			{
				void Do()
				{
					var culture = CultureInfo.InvariantCulture;
				}
			}
		}
		""");

	[Test]
	public Task TestCodeFixForStringEmptyConstructor() => VerifyCodeFixAsync(
		"""
		using System;
		using System.Globalization;

		namespace Tests
		{
			class Test
			{
				void Do()
				{
					var culture = [|new CultureInfo(string.Empty)|];
				}
			}
		}
		""",
		"""
		using System;
		using System.Globalization;

		namespace Tests
		{
			class Test
			{
				void Do()
				{
					var culture = CultureInfo.InvariantCulture;
				}
			}
		}
		""");

	[Test]
	public Task TestCodeFixForInvariantLcidConstructor() => VerifyCodeFixAsync(
		"""
		using System.Globalization;

		namespace Tests
		{
			class Test
			{
				void Do()
				{
					var culture = [|new CultureInfo(127)|];
				}
			}
		}
		""",
		"""
		using System.Globalization;

		namespace Tests
		{
			class Test
			{
				void Do()
				{
					var culture = CultureInfo.InvariantCulture;
				}
			}
		}
		""");

	[Test]
	public Task TestCodeFixForInlineUsage() => VerifyCodeFixAsync(
		"""
		using System;
		using System.Globalization;

		namespace Tests
		{
			class Test
			{
				void Do()
				{
					var text = 123.ToString([|new CultureInfo("fr-FR")|]);
				}
			}
		}
		""",
		"""
		using System;
		using System.Globalization;

		namespace Tests
		{
			class Test
			{
				void Do()
				{
					var text = 123.ToString(CultureInfo.GetCultureInfo("fr-FR"));
				}
			}
		}
		""");
}
