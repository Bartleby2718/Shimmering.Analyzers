using Shimmering.Analyzers.Analyzers.Usage;
using Shimmering.Analyzers.CodeFixes.Usage;

namespace Shimmering.Analyzers.Tests.Analyzers.Usage;

public class ToListForEachAnalyzerTests : ShimmeringAnalyzerTests<ToListForEachAnalyzer>
{
	[Test]
#pragma warning disable SA1027 // Use tabs correctly
	public Task TestCustomToListExtensionMethodIsNotFlagged() => VerifyAnalyzerAsync(
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

	[Test]
	public Task TestIQueryableIsNotFlagged() => VerifyAnalyzerAsync(
		"""
		using System;
		using System.Linq;

		namespace Tests
		{
			class Test
			{
				public void Do()
				{
					var iqueryable = Enumerable.Empty<int>().AsQueryable();
					iqueryable.ToList().ForEach(n => Console.WriteLine(n));
				}
			}
		}
		""");
}
