using Shimmering.Analyzers.Analyzers.Usage;
using Shimmering.Analyzers.CodeFixes.Usage;

namespace Shimmering.Analyzers.Tests.Analyzers.Usage;

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
	public Task TestWithoutToArray(string predicate) => VerifyCodeFixAsync(
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
	public Task TestWithToArray(string predicate) => VerifyCodeFixAsync(
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
	public Task TestTrivia() => VerifyCodeFixAsync(
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
