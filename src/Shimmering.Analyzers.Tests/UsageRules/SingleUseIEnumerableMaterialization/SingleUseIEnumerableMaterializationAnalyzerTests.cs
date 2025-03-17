using Shimmering.Analyzers.UsageRules.SingleUseIEnumerableMaterialization;

namespace Shimmering.Analyzers.Tests.UsageRules.SingleUseIEnumerableMaterialization;

using Verifier = CSharpAnalyzerVerifier<
	SingleUseIEnumerableMaterializationAnalyzer,
	DefaultVerifier>;

public class SingleUseIEnumerableMaterializationAnalyzerTests : ShimmeringAnalyzerTests<SingleUseIEnumerableMaterializationAnalyzer>
{
	[Test]
	public Task TestExplicitTypeIsNotFlagged() => Verifier.VerifyAnalyzerAsync(
		"""
		using System;
		using System.Linq;
		
		namespace Tests
		{
			class Test
			{
				public void Do()
				{
					int[] numbers = [1, 2, 3, 4, 5];
					int[] oddNumbers = numbers.Where(n => n % 2 == 1).ToArray();
					foreach (var oddNumber in oddNumbers)
					{
						Console.WriteLine(oddNumber);
					}
				}
			}
		}
		""");

	[Test]
	public Task TestUsageInLambdaIsNotFlagged() => Verifier.VerifyAnalyzerAsync(
		"""
		using System;
		using System.Linq;
		
		namespace Tests
		{
			class Test
			{
				public void Do()
				{
					int[] numbers = [1, 2, 3, 4, 5];
					var oddNumbers = numbers.Where(n => n % 2 == 1).ToArray();
					var evenNumbers = numbers.Where(n => !oddNumbers.Contains(n));
				}
			}
		}
		""");

	[Test]
	public Task TestMultiUseIEnumerableIsNotFlagged() => Verifier.VerifyAnalyzerAsync(
		"""
		using System;
		using System.Collections.Generic;
		using System.Linq;

		namespace Tests
		{
			class Test
			{
				public void Do()
				{
					List<int> numbers = [1, 2, 3, 4, 5];
					var oddNumbers = numbers.Where(n => n % 2 == 1).ToArray();
					foreach (var oddNumber in oddNumbers)
					{
						Console.WriteLine(oddNumber);
					}
					Console.WriteLine(oddNumbers.First());
				}
			}
		}
		""");

	[Test]
	public Task TestIQueryableIsNotFlagged() => Verifier.VerifyAnalyzerAsync(
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
					var emptyArray = iqueryable.ToArray();
					foreach (var value in emptyArray)
					{
						Console.WriteLine(value);
					}
				}
			}
		}
		""");
}
