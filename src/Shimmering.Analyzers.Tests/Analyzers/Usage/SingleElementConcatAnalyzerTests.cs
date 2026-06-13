namespace Shimmering.Analyzers.Tests.Analyzers.Usage;

using Shimmering.Analyzers.Analyzers.Usage;
using Shimmering.Analyzers.CodeFixes.Usage;

public class SingleElementConcatAnalyzerTests : ShimmeringAnalyzerTests<SingleElementConcatAnalyzer>
{
	[Test]
	public Task TestSpreadElementIsIgnored() => VerifyAnalyzerAsync(
		"""
		using System;
		using System.Collections.Generic;
		using System.Linq;

		namespace Tests
		{
			class Test
			{
				public Test()
				{
					List<int> list = [1];
					int[] array = new[] { 3, 4 }.Concat([..list]).ToArray();
				}
			}
		}
		""");

	[Test]
	public Task TestObjectInitializerIsIgnored() => VerifyAnalyzerAsync("""
		using System;
		using System.Collections;
		using System.Collections.Generic;
		using System.Linq;

		namespace Tests
		{
			class Test
			{
				private IEnumerable<int> ints = new[] { 1, 2 }.Concat(new MyCollection<int> { SettableProperty = 1 });
			}

			public class MyCollection<T> : IEnumerable<T>
			{
				public int SettableProperty { get; set; }
				public IEnumerator<T> GetEnumerator() => throw new NotImplementedException();
				IEnumerator IEnumerable.GetEnumerator() => throw new NotImplementedException();
			}
		}
		""");
}
