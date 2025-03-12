using Shimmering.Analyzers.UsageRules.SingleUseIEnumerableMaterialization;

namespace Shimmering.Analyzers.Tests.UsageRules.SingleUseIEnumerableMaterialization;

using Verifier = CSharpCodeFixVerifier<
	SingleUseIEnumerableMaterializationAnalyzer,
	SingleUseIEnumerableMaterializationCodeFixProvider,
	DefaultVerifier>;

public class SingleUseIEnumerableMaterializationCodeFixProviderTests
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

	[Test]
	public Task TestSingleUseIEnumerableMaterialization() => Verifier.VerifyCodeFixAsync(
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
					var oddNumbers = [|numbers.Where(n => n % 2 == 1).ToArray()|];
					foreach (var oddNumber in oddNumbers)
					{
						Console.WriteLine(oddNumber);
					}
					var evenNumbers = [|numbers.Where(n => n % 2 == 0).ToList()|];
					Console.WriteLine(evenNumbers.Last());
				}
			}
		}
		""",
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
					var oddNumbers = numbers.Where(n => n % 2 == 1);
					foreach (var oddNumber in oddNumbers)
					{
						Console.WriteLine(oddNumber);
					}
					var evenNumbers = numbers.Where(n => n % 2 == 0);
					Console.WriteLine(evenNumbers.Last());
				}
			}
		}
		""");

	[Test]
	public Task TestTrivia() => Verifier.VerifyCodeFixAsync(
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
					var oddNumbers = [|numbers
						// line before previous
						.Where(n => n % 2 == 1) // right after previous
						// line before current
						.ToArray()|]; // right after current
					foreach (var oddNumber in oddNumbers)
					{
						Console.WriteLine(oddNumber);
					}
				}
			}
		}
		""",
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
					var oddNumbers = numbers
						// line before previous
						.Where(n => n % 2 == 1); // right after current
					foreach (var oddNumber in oddNumbers)
					{
						Console.WriteLine(oddNumber);
					}
				}
			}
		}
		""");
}
