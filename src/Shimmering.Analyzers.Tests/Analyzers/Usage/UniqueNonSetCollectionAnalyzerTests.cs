namespace Shimmering.Analyzers.Tests.Analyzers.Usage;

using Shimmering.Analyzers.Analyzers.Usage;
using Shimmering.Analyzers.CodeFixes.Usage;

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
}
