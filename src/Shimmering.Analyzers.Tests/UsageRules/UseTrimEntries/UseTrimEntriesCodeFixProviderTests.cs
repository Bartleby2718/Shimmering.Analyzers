using Shimmering.Analyzers.UsageRules.UseTrimEntries;

namespace Shimmering.Analyzers.Tests.UsageRules.UseTrimEntries;

using Verifier = CSharpCodeFixVerifier<
	UseTrimEntriesAnalyzer,
	UseTrimEntriesCodeFixProvider,
	DefaultVerifier>;

public class UseTrimEntriesCodeFixProviderTests : ShimmeringCodeFixProviderTests<UseTrimEntriesAnalyzer, UseTrimEntriesCodeFixProvider>
{
	[Test]
	public Task TestSomethingThatShouldBeFixed() => VerifyCodeFixAsync(
		"""
		using System;
		using System.Linq;

		namespace Tests
		{
			class Test
			{
				public void Do(string input)
				{
					var x = [|input.Split(',')
						.Select(x => x.Trim())|];
				}
			}
		}
		""",
		"""
		using System;
		using System.Linq;

		namespace Tests
		{
			class Test
			{
				public void Do(string input)
				{
					var x = input.Split(',', StringSplitOptions.TrimEntries);
				}
			}
		}
		""");

	[Test]
	public Task TestWithToArray() => VerifyCodeFixAsync(
		"""
		using System;
		using System.Linq;

		namespace Tests
		{
			class Test
			{
				public void Do(string input)
				{
					var x = [|input.Split(',')
						.Select(x => x.Trim())
						.ToArray()|];
				}
			}
		}
		""",
		"""
		using System;
		using System.Linq;

		namespace Tests
		{
			class Test
			{
				public void Do(string input)
				{
					var x = input.Split(',', StringSplitOptions.TrimEntries);
				}
			}
		}
		""");
}
