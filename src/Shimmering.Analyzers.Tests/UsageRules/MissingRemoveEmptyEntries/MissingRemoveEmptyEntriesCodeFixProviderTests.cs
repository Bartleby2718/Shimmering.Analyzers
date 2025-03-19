using System;

using Shimmering.Analyzers.UsageRules.MissingRemoveEmptyEntries;

namespace Shimmering.Analyzers.Tests.UsageRules.MissingRemoveEmptyEntries;

using Verifier = CSharpCodeFixVerifier<
	MissingRemoveEmptyEntriesAnalyzer,
	MissingRemoveEmptyEntriesCodeFixProvider,
	DefaultVerifier>;

public class MissingRemoveEmptyEntriesCodeFixProviderTests : ShimmeringCodeFixProviderTests<MissingRemoveEmptyEntriesAnalyzer, MissingRemoveEmptyEntriesCodeFixProvider>
{
	[TestCase("x.Length > 0")]
	[TestCase("x.Length != 0")]
	[TestCase("x.Length >= 1")]
	[TestCase("x != \"\"")]
	[TestCase("x != string.Empty")]
	[TestCase("!string.IsNullOrEmpty(x)")]
	[TestCase("x.Any()")]
	[TestCase("x is not \"\"")]
	public Task TestWithoutToArray(string predicate) => Verifier.VerifyCodeFixAsync(
		$$"""
		using System;
		using System.Linq;
		
		namespace Tests
		{
			class Test
			{
				void Do(string input)
				{
					var x = [|input.Split(' ')
						.Where(x => {{predicate}})|];
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
				void Do(string input)
				{
					var x = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
				}
			}
		}
		""");

	[TestCase("x.Length > 0")]
	[TestCase("x.Length != 0")]
	[TestCase("x.Length >= 1")]
	[TestCase("x != \"\"")]
	[TestCase("x != string.Empty")]
	[TestCase("!string.IsNullOrEmpty(x)")]
	[TestCase("x.Any()")]
	[TestCase("x is not \"\"")]
	public Task TestWithToArray(string predicate) => Verifier.VerifyCodeFixAsync(
		$$"""
		using System;
		using System.Linq;
		
		namespace Tests
		{
			class Test
			{
				void Do(string input)
				{
					var x = [|input.Split(' ')
						.Where(x => {{predicate}})
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
				void Do(string input)
				{
					var x = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
				}
			}
		}
		""");

	[Test]
	public Task TestTrivia() => Verifier.VerifyCodeFixAsync(
		"""
		using System;
		using System.Linq;
		
		namespace Tests
		{
			class Test
			{
				void Do(string input)
				{
					var x = /* right before input */[|input/* right after input*/
						/* line before Split */
						.Split(' ') // right after Split
						// right before Where
						.Where(x => x.Length > 0) // right after Split
						// line before ToArray
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
				void Do(string input)
				{
					var x = /* right before input */input/* right after input*/
						/* line before Split */
						.Split(' ', StringSplitOptions.RemoveEmptyEntries);
				}
			}
		}
		""");
}
