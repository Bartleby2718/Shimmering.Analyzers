using Shimmering.Analyzers.UsageRules.RedundantSpreadElement;

namespace Shimmering.Analyzers.Tests.UsageRules.RedundantSpreadElement;

using Verifier = CSharpCodeFixVerifier<
	RedundantSpreadElementAnalyzer,
	RedundantSpreadElementCodeFixProvider,
	DefaultVerifier>;

public class RedundantSpreadElementCodeFixProviderTests : ShimmeringCodeFixProviderTests<RedundantSpreadElementAnalyzer, RedundantSpreadElementCodeFixProvider>
{
	[Test]
	public Task TestImplicitArrayCreationExpression() => Verifier.VerifyCodeFixAsync(
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
	public Task TestExplicitArrayCreationExpression() => Verifier.VerifyCodeFixAsync(
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
	public Task TestExplicitArrayCreationExpressionWithEmptyInitializer() => Verifier.VerifyCodeFixAsync(
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
	public Task TestObjectCreationWithEmptyInitializer() => Verifier.VerifyCodeFixAsync(
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
	public Task TestObjectCreationWithNonemptyInitializer() => Verifier.VerifyCodeFixAsync(
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
	public Task TestEmptyCollectionExpressionCastedToArray() => Verifier.VerifyCodeFixAsync(
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
	public Task TestNonemptyCollectionExpressionCastedToArray() => Verifier.VerifyCodeFixAsync(
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
	public Task TestEmptySingletonAsMethod() => Verifier.VerifyCodeFixAsync(
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
	public Task TestEmptySingletonAsProperty() => Verifier.VerifyCodeFixAsync(
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
