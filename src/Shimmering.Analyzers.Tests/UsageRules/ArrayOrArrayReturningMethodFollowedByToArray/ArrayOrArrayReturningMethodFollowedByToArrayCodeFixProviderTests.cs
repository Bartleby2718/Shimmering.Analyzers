using Shimmering.Analyzers.UsageRules.ArrayOrArrayReturningMethodFollowedByToArray;

namespace Shimmering.Analyzers.Tests.UsageRules.ArrayOrArrayReturningMethodFollowedByToArray;

using Verifier = CSharpCodeFixVerifier<
	ArrayOrArrayReturningMethodFollowedByToArrayAnalyzer,
	ArrayOrArrayReturningMethodFollowedByToArrayCodeFixProvider,
	DefaultVerifier>;

public class ArrayOrArrayReturningMethodFollowedByToArrayCodeFixProviderTests : ShimmeringCodeFixProviderTests<ArrayOrArrayReturningMethodFollowedByToArrayAnalyzer, ArrayOrArrayReturningMethodFollowedByToArrayCodeFixProvider>
{
	[Test]
	public Task TestToArrayNotChainedBySomethingElse() => Verifier.VerifyCodeFixAsync(
		"""
		using System;
		using System.Linq;

		namespace Tests
		{
			class Test
			{
				public void Do()
				{
					var implicitArrayCreation = [|new[] { 1 }.ToArray()|];
					var explicitArrayCreation = [|new int[] { 1 }.ToArray()|];
					var array = [|"a".Split(' ').ToArray()|];
					MyMethod([|"b".ToCharArray().ToArray()|]);

					void MyMethod(char[] input) { }
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
					var implicitArrayCreation = new[] { 1 };
					var explicitArrayCreation = new int[] { 1 };
					var array = "a".Split(' ');
					MyMethod("b".ToCharArray());

					void MyMethod(char[] input) { }
				}
			}
		}
		""");

	[Test]
	public Task TestToArrayChainedBySomethingElse() => Verifier.VerifyCodeFixAsync(
		"""
		using System;
		using System.Linq;

		namespace Tests
		{
			class Test
			{
				public void Do()
				{
					var arrayLength = [|"a".Split(' ').ToArray()|].Length;
					MyMethod([|"b".ToCharArray().ToArray()|].Length);

					void MyMethod(int length) { }
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
					var arrayLength = "a".Split(' ').Length;
					MyMethod("b".ToCharArray().Length);

					void MyMethod(int length) { }
				}
			}
		}
		""");

	// TODO: should be fixed in #85
	[Test]
	public Task TestTriviaWhenToArrayComesLastWithComments() => Verifier.VerifyCodeFixAsync(
		"""
		using System;
		using System.Linq;

		namespace Tests
		{
			class Test
			{
				public void Do()
				{
					// line before declaration
					var arrayLength = [|"a"
						// line before Split
						.Split(' ') // right after Split
						// line before ToArray
						.ToArray()|]; // right after ToArray
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
					// line before declaration
					var arrayLength = "a"
						// line before Split
						.Split(' ') // right after Split
		; // right after ToArray
				}
			}
		}
		""");

	[Test]
	public Task TestTriviaWhenToArrayComesLastWithoutComments() => Verifier.VerifyCodeFixAsync(
		"""
		using System;
		using System.Linq;

		namespace Tests
		{
			class Test
			{
				public void Do()
				{
					var arrayLength = [|"a"
						.Split(' ')
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
				public void Do()
				{
					var arrayLength = "a"
						.Split(' ');
				}
			}
		}
		""");

	[Test]
	public Task TestTriviaWhenToArrayIsFollowedBySomethingElse() => Verifier.VerifyCodeFixAsync(
		"""
		using System;
		using System.Linq;

		namespace Tests
		{
			class Test
			{
				public void Do()
				{
					// line before declaration
					var arrayLength = [|"a"
						// line before Split
						.Split(' ') // right after Split
						// line before ToArray
						.ToArray()|] // right after ToArray
						// line before Length
						.Length; // right after declaration
					MyMethod([|"b"
						// line before ToCharArray
						.ToCharArray() // right after ToCharArray
						// line before ToArray
						.ToArray()|] // right after ToArray
						// line before Length
						.Length); // right after invocation

					void MyMethod(int length) { }
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
					// line before declaration
					var arrayLength = "a"
						// line before Split
						.Split(' ') // right after Split
						// line before Length
						.Length; // right after declaration
					MyMethod("b"
						// line before ToCharArray
						.ToCharArray() // right after ToCharArray
						// line before Length
						.Length); // right after invocation

					void MyMethod(int length) { }
				}
			}
		}
		""");
}
