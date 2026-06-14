using System.Threading.Tasks;

using NUnit.Framework;

using Shimmering.Analyzers.Analyzers.Usage;

namespace Shimmering.Analyzers.Tests.Analyzers.Usage;

public class HashSetOrHashSetReturningMethodFollowedByToHashSetAnalyzerTests : ShimmeringAnalyzerTests<HashSetOrHashSetReturningMethodFollowedByToHashSetAnalyzer>
{
	[Test]
	public Task TestIgnoreNoHashSet() => VerifyAnalyzerAsync(
		"""
		using System.Collections.Generic;
		using System.Linq;

		class Test
		{
			void Do()
			{
				var list = new List<int>().ToHashSet();
			}
		}
		""");

	[Test]
	public Task TestFlagToHashSet() => VerifyAnalyzerAsync(
		"""
		using System.Collections.Generic;
		using System.Linq;

		class Test
		{
			void Do(HashSet<int> set)
			{
				var other = [|set.ToHashSet()|];
			}
		}
		""");
}
