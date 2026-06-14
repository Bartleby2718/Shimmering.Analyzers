using Shimmering.Analyzers.Analyzers.Usage;

namespace Shimmering.Analyzers.Tests.Analyzers.Usage;

public class UniqueNonSetCollectionAnalyzerTests : ShimmeringAnalyzerTests<UniqueNonSetCollectionAnalyzer>
{
	[Test]
	public Task TestOverloadIsExcludedUntilIssue91IsDone() => VerifyAnalyzerAsync(
		"""
		using System;
		using System.Collections.Generic;
		using System.Linq;

		namespace Tests
		{
			class Test
			{
				string[] _field = new[] { "a" }.Distinct(StringComparer.Ordinal).ToArray();
			}
		}
		""");

	[Test]
	public Task TestIncompatibleTargetTypesAreNotFlagged() => VerifyAnalyzerAsync(
		"""
		using System;
		using System.Collections.Generic;
		using System.Linq;

		namespace Tests
		{
			class Test
			{
				List<int> concreteList = new[] { 1, 2 }.Distinct().ToList();
				int[] concreteArray = new[] { 1, 2 }.Distinct().ToArray();

				void Method()
				{
					List<int> localList = new[] { 1, 2 }.Distinct().ToList();
					int[] localArray = new[] { 1, 2 }.Distinct().ToArray();
					DummyMethod(new[] { 1, 2 }.Distinct().ToList());
				}

				static void DummyMethod(List<int> list) { }
			}
		}
		""");
}
