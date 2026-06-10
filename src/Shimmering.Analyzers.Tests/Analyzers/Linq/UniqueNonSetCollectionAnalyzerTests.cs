namespace Shimmering.Analyzers.Tests.Analyzers.Linq;

using Shimmering.Analyzers.Analyzers.Linq;
using Shimmering.Analyzers.CodeFixes.Linq;

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
