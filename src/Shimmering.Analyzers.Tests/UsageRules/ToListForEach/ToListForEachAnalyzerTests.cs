using Shimmering.Analyzers.UsageRules.ToListForEach;

namespace Shimmering.Analyzers.Tests.UsageRules.ToListForEach;

using Verifier = CSharpAnalyzerVerifier<
	ToListForEachAnalyzer,
	DefaultVerifier>;

public class ToListForEachAnalyzerTests : ShimmeringAnalyzerTests<ToListForEachAnalyzer>
{
	[Test]
#pragma warning disable SA1027 // Use tabs correctly
	public Task TestCustomToListExtensionMethodIsNotFlagged() => Verifier.VerifyAnalyzerAsync(
		"""
		using System;
		using System.Collections.Generic;
		using System.Linq;

		class Test
		{
		    void Do()
		    {
		        int[] numbers = [];
		        numbers.ToList().ForEach(n => Console.WriteLine(n));
		    }
		}

		internal static class CustomExtensions
		{
			internal static List<T> ToList<T>(this IEnumerable<T> items) => items.ToList();
		}
		""");
#pragma warning restore SA1027 // Use tabs correctly
}
