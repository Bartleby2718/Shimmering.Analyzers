using Shimmering.Analyzers.UsageRules.UniqueNonSetCollection;

namespace Shimmering.Analyzers.Tests.UsageRules.UniqueNonSetCollection;

using Verifier = CSharpAnalyzerVerifier<
	UniqueNonSetCollectionAnalyzer,
	DefaultVerifier>;

public class UniqueNonSetCollectionAnalyzerTests : ShimmeringAnalyzerTests<UniqueNonSetCollectionAnalyzer>
{
	[Test]
	public Task TestOverloadIsExcludedUntilIssue91IsDone() => Verifier.VerifyAnalyzerAsync(
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
