using Shimmering.Analyzers.UsageRules.RedundantSpreadElement;

namespace Shimmering.Analyzers.Tests.UsageRules.RedundantSpreadElement;

using Verifier = CSharpAnalyzerVerifier<
	RedundantSpreadElementAnalyzer,
	DefaultVerifier>;

public class RedundantSpreadElementAnalyzerTests : ShimmeringAnalyzerTests<RedundantSpreadElementAnalyzer>
{
	[Test]
	public Task TestUnsupportedCases() => Verifier.VerifyAnalyzerAsync(
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
}
