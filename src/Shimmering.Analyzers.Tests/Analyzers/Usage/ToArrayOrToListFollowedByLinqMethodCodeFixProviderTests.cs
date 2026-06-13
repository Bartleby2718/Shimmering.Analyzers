using Shimmering.Analyzers.Analyzers.Usage;
using Shimmering.Analyzers.CodeFixes.Usage;

namespace Shimmering.Analyzers.Tests.Analyzers.Usage;

public class ToArrayOrToListFollowedByLinqMethodCodeFixProviderTests : ShimmeringCodeFixProviderTests<ToArrayOrToListFollowedByLinqMethodAnalyzer, ToArrayOrToListFollowedByLinqMethodCodeFixProvider>
{
	[Test]
	public Task TestToArrayIsRemoved() => VerifyCodeFixAsync(
		"""
		using System;
		using System.Linq;

		namespace Tests
		{
			class Test
			{
				public void Do()
				{
					int[] numbers = [];
					var greaterThanThree = [|numbers.ToArray()|].Where(x => x > 3);
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
				public void Do()
				{
					int[] numbers = [];
					var greaterThanThree = numbers.Where(x => x > 3);
				}
			}
		}
		""");

	[Test]
	public Task TestToListIsRemoved() => VerifyCodeFixAsync(
		"""
		using System;
		using System.Linq;

		namespace Tests
		{
			class Test
			{
				public void Do()
				{
					int[] numbers = [];
					var squares = [|numbers.ToList()|].Select(x => x * x).ToArray();
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
				public void Do()
				{
					int[] numbers = [];
					var squares = numbers.Select(x => x * x).ToArray();
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
				public void Do()
				{
					int[] numbers = [];
					// line before source
					var squares = [|numbers // right after source
						.ToList()|]
						// line before Select
						.Select(x => x * x) // right after Select
						// line before ToArray
						.ToArray();
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
				public void Do()
				{
					int[] numbers = [];
					// line before source
					var squares = numbers // right after source
						// line before Select
						.Select(x => x * x) // right after Select
						// line before ToArray
						.ToArray();
				}
			}
		}
		""");

	[Test]
	public Task TestRemoveRedundantToListInChainedLinqOperations() => VerifyCodeFixAsync(
		"""
		using System.Linq;
		using System.Collections.Generic;
		class C {
			void M(IEnumerable<int> a) {
				[|a.Union(new[] {1}).ToList()|]
					.Union(new[] {2});
			}
		}
		""",
		"""
		using System.Linq;
		using System.Collections.Generic;
		class C {
			void M(IEnumerable<int> a) {
				a.Union(new[] {1})
					.Union(new[] {2});
			}
		}
		""");

	[Test]
	public Task TestToListContainsIsRemoved() => VerifyCodeFixAsync(
		"""
		using System;
		using System.Linq;
		using System.Collections.Generic;

		namespace Tests
		{
			class Test
			{
				public void Do(IEnumerable<int> numbers)
				{
					var hasThree = [|numbers.ToList()|].Contains(3);
				}
			}
		}
		""",
		"""
		using System;
		using System.Linq;
		using System.Collections.Generic;

		namespace Tests
		{
			class Test
			{
				public void Do(IEnumerable<int> numbers)
				{
					var hasThree = numbers.Contains(3);
				}
			}
		}
		""");

	[Test]
	public Task TestToListReverseIsRemoved() => VerifyCodeFixAsync(
		"""
		using System;
		using System.Linq;
		using System.Collections.Generic;

		namespace Tests
		{
			class Test
			{
				public void Do(IEnumerable<int> numbers)
				{
					[|numbers.ToList()|].Reverse();
				}
			}
		}
		""",
		"""
		using System;
		using System.Linq;
		using System.Collections.Generic;

		namespace Tests
		{
			class Test
			{
				public void Do(IEnumerable<int> numbers)
				{
					numbers.Reverse();
				}
			}
		}
		""");
}
