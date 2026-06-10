using Shimmering.Analyzers.UsageRules.HashSetOrHashSetReturningMethodFollowedByToHashSet;

namespace Shimmering.Analyzers.Tests.UsageRules.HashSetOrHashSetReturningMethodFollowedByToHashSet;

using Verifier = CSharpCodeFixVerifier<
	HashSetOrHashSetReturningMethodFollowedByToHashSetAnalyzer,
	HashSetOrHashSetReturningMethodFollowedByToHashSetCodeFixProvider,
	DefaultVerifier>;

public class HashSetOrHashSetReturningMethodFollowedByToHashSetCodeFixProviderTests : ShimmeringCodeFixProviderTests<HashSetOrHashSetReturningMethodFollowedByToHashSetAnalyzer, HashSetOrHashSetReturningMethodFollowedByToHashSetCodeFixProvider>
{
	[Test]
	public Task TestHashSetCreationIsFlagged() => Verifier.VerifyCodeFixAsync(
		"""
		using System.Collections.Generic;
		using System.Linq;

		namespace Tests
		{
			class Test
			{
				public void Do()
				{
					HashSet<int> MyHashSet = [|new HashSet<int>().ToHashSet()|];
				}
			}
		}
		""",
		"""
		using System.Collections.Generic;
		using System.Linq;

		namespace Tests
		{
			class Test
			{
				public void Do()
				{
					HashSet<int> MyHashSet = new HashSet<int>();
				}
			}
		}
		""");

	[Test]
	public Task TestHashSetIdentifierIsFlagged() => Verifier.VerifyCodeFixAsync(
		"""
		using System.Collections.Generic;
		using System.Linq;

		namespace Tests
		{
			class Test
			{
				public void Do(HashSet<int> input)
				{
					var x = [|input.ToHashSet()|];
				}
			}
		}
		""",
		"""
		using System.Collections.Generic;
		using System.Linq;

		namespace Tests
		{
			class Test
			{
				public void Do(HashSet<int> input)
				{
					var x = input;
				}
			}
		}
		""");

	[Test]
	public Task TestHashSetReturningMethodIsFlagged() => Verifier.VerifyCodeFixAsync(
		"""
		using System.Collections.Generic;
		using System.Linq;

		namespace Tests
		{
			class Test
			{
				public void Do()
				{
					var x = [|GetHashSet().ToHashSet()|];
				}

				static HashSet<int> GetHashSet() => [];
			}
		}
		""",
		"""
		using System.Collections.Generic;
		using System.Linq;

		namespace Tests
		{
			class Test
			{
				public void Do()
				{
					var x = GetHashSet();
				}

				static HashSet<int> GetHashSet() => [];
			}
		}
		""");

	[Test]
	public Task TestOtherMethodsAreNotFlagged() => Verifier.VerifyAnalyzerAsync(
		"""
		using System.Collections.Generic;
		using System.Linq;

		namespace Tests
		{
			class Test
			{
				public void Do()
				{
					List<int> myList = [];
					var x = myList.ToHashSet();
				}
			}
		}
		""");
}
