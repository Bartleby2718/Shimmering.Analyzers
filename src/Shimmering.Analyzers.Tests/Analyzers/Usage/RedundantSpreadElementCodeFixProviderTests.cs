using Shimmering.Analyzers.Analyzers.Usage;
using Shimmering.Analyzers.CodeFixes.Usage;

namespace Shimmering.Analyzers.Tests.Analyzers.Usage;

public class RedundantSpreadElementCodeFixProviderTests : ShimmeringCodeFixProviderTests<RedundantSpreadElementAnalyzer, RedundantSpreadElementCodeFixProvider>
{
	[Test]
	public Task TestImplicitArrayCreationExpression() => VerifyCodeFixAsync(
		"""
		namespace Tests
		{
			class Test
			{
				int[] Array => [1, [|..new[] { 2, 3 }|], 4];
			}
		}
		""",
		"""
		namespace Tests
		{
			class Test
			{
				int[] Array => [1, 2, 3, 4];
			}
		}
		""");

	[Test]
	public Task TestExplicitArrayCreationExpression() => VerifyCodeFixAsync(
		"""
		namespace Tests
		{
			class Test
			{
				int[] Array => [1, [|..new int[] { 2, 3 }|], 4];
			}
		}
		""",
		"""
		namespace Tests
		{
			class Test
			{
				int[] Array => [1, 2, 3, 4];
			}
		}
		""");

	[Test]
	public Task TestExplicitArrayCreationExpressionWithEmptyInitializer() => VerifyCodeFixAsync(
		"""
		namespace Tests
		{
			class Test
			{
				int[] Array => [1, [|..new int[0] { }|], 4];
			}
		}
		""",
		"""
		namespace Tests
		{
			class Test
			{
				int[] Array => [1, 4];
			}
		}
		""");

	[Test]
	public Task TestObjectCreationWithEmptyInitializer() => VerifyCodeFixAsync(
		"""
		using System.Collections.Generic;

		namespace Tests
		{
			class Test
			{
				int[] Array => [1, [|.. new List<int> { }|], [|.. new List<int>() { }|], 4];
			}
		}
		""",
		"""
		using System.Collections.Generic;

		namespace Tests
		{
			class Test
			{
				int[] Array => [1, 4];
			}
		}
		""");

	[Test]
	public Task TestObjectCreationWithNonemptyInitializer() => VerifyCodeFixAsync(
		"""
		using System.Collections.Generic;

		namespace Tests
		{
			class Test
			{
				int[] Array => [1, [|.. new List<int>() { 2, 3 }|], 4];
			}
		}
		""",
		"""
		using System.Collections.Generic;

		namespace Tests
		{
			class Test
			{
				int[] Array => [1, 2, 3, 4];
			}
		}
		""");

	[Test]
	public Task TestEmptyCollectionExpressionCastedToArray() => VerifyCodeFixAsync(
		"""
		namespace Tests
		{
			class Test
			{
				int[] Array => [1, [|..(int[])[]|], 4];
			}
		}
		""",
		"""
		namespace Tests
		{
			class Test
			{
				int[] Array => [1, 4];
			}
		}
		""");

	[Test]
	public Task TestNonemptyCollectionExpressionCastedToArray() => VerifyCodeFixAsync(
		"""
		namespace Tests
		{
			class Test
			{
				int[] Array => [1, [|..(int[])[2, 3]|], 4];
			}
		}
		""",
		"""
		namespace Tests
		{
			class Test
			{
				int[] Array => [1, 2, 3, 4];
			}
		}
		""");

	[Test]
	public Task TestEmptySingletonAsMethod() => VerifyCodeFixAsync(
		"""
		using System.Linq;

		namespace Tests
		{
			class Test
			{
				int[] Array => [1, [|..Enumerable.Empty<int>()|], 4];
			}
		}
		""",
		"""
		using System.Linq;

		namespace Tests
		{
			class Test
			{
				int[] Array => [1, 4];
			}
		}
		""");

	[Test]
	public Task TestEmptySingletonAsProperty() => VerifyCodeFixAsync(
		"""
		using System.Collections.Immutable;

		namespace Tests
		{
			class Test
			{
				int[] Array => [1, [|..ImmutableList<int>.Empty|], 4];
			}
		}
		""",
		"""
		using System.Collections.Immutable;

		namespace Tests
		{
			class Test
			{
				int[] Array => [1, 4];
			}
		}
		""");
}
