using Shimmering.Analyzers.RedundantSpreadElement;

namespace Shimmering.Analyzers.Tests.RedundantSpreadElement;

using Verifier = CSharpCodeFixVerifier<
	RedundantSpreadElementAnalyzer,
	RedundantSpreadElementCodeFixProvider,
	DefaultVerifier>;

public class RedundantSpreadElementCodeFixProviderTests
{
	[Test]
	public async Task TestUnsupportedCases()
	{
#if NET462
		await new CSharpAnalyzerTest<RedundantSpreadElementAnalyzer, DefaultVerifier>
		{
			TestState =
			{
				Sources =
				{
					"""
					using System;
					using System.Collections.Generic;
					using System.Collections.Immutable;

					namespace Tests
					{
						class Test
						{
							int[] Array1 => [1, .. new List<int>(), 4];                  // object creation without initializer (because a collection may not be empty by default)
							int[] Array2 => [1, .. new List<int>() { Capacity = 2 }, 4]; // object creation with an initializer assigning a property
							int[] Array3 => [1, .. new int[3], 4];                       // array creation without initializer
							int[] Array4 => [1, .. (List<int>)[], 2];                    // empty collection expression casted to a non-array collection
						}
					}
					""",
				},
				AdditionalReferences =
				{
					MetadataReference.CreateFromFile(System.Reflection.Assembly.Load("System.Collections.Immutable").Location),
				},
			},
		}.RunAsync();
#elif NET
		await Verifier.VerifyAnalyzerAsync(
		"""
		using System;
		using System.Collections.Generic;
		using System.Collections.Immutable;

		namespace Tests
		{
			class Test
			{
				int[] Array1 => [1, .. new List<int>(), 4];                  // object creation without initializer (because a collection may not be empty by default)
				int[] Array2 => [1, .. new List<int>() { Capacity = 2 }, 4]; // object creation with an initializer assigning a property
				int[] Array3 => [1, .. new int[3], 4];                       // array creation without initializer
				int[] Array4 => [1, .. (List<int>)[], 2];                    // empty collection expression casted to a non-array collection
			}
		}
		""");
#else
		throw new InvalidOperationException("Remember to update the preprocessor directives!");
#endif
	}

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
	public async Task TestEmptySingletonAsProperty()
	{
#if NET462
		await new CSharpCodeFixTest<
			RedundantSpreadElementAnalyzer,
			RedundantSpreadElementCodeFixProvider,
			DefaultVerifier>
		{
			TestState =
			{
				Sources =
				{
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
				},
				AdditionalReferences =
				{
					MetadataReference.CreateFromFile(System.Reflection.Assembly.Load("System.Collections.Immutable").Location),
				},
				ReferenceAssemblies = ReferenceAssemblies.Default.AddAssemblies(["System.Linq"]),
			},
			FixedCode = """
				using System.Collections.Immutable;

				namespace Tests
				{
					class Test
					{
						int[] Array => [1, 4];
					}
				}
				""",
		}.RunAsync();
#elif NET
		await Verifier.VerifyCodeFixAsync(
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
#else
		throw new InvalidOperationException("Remember to update the preprocessor directives!");
#endif
	}
}
