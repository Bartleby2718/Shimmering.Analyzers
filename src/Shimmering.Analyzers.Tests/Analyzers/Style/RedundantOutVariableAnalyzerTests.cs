using Shimmering.Analyzers.Analyzers.Style;

namespace Shimmering.Analyzers.Tests.Analyzers.Style;

public class RedundantOutVariableAnalyzerTests : ShimmeringAnalyzerTests<RedundantOutVariableAnalyzer>
{
	[Test]
	public Task TestShouldNotFlagIfNotUsedForAssignment() => VerifyAnalyzerAsync(
		"""
		using System;

		namespace Tests
		{
			class Test
			{
				void Method(string dayOfWeekString)
				{
					if (Enum.TryParse(dayOfWeekString, out DayOfWeek dayOfWeek1))
					{
						// not an assignment
						Console.WriteLine(dayOfWeek1);
					}
				}
			}
		}
		""");

	[Test]
	public Task TestShouldNotFlagMultipleDeclaration() => VerifyAnalyzerAsync(
		"""
		using System;

		namespace Tests
		{
			class Test
			{
				void Method(string dayOfWeekString)
				{
					if (Enum.TryParse(dayOfWeekString, out DayOfWeek dayOfWeek1))
					{
						DayOfWeek first = dayOfWeek1, second = DayOfWeek.Monday;
					}
				}
			}
		}
		""");

	[Test]
	public Task TestShouldNotFlagTupleAssignment() => VerifyAnalyzerAsync(
		"""
		using System;

		namespace Tests
		{
			class Test
			{
				void Method(string dayOfWeekString)
				{
					if (Enum.TryParse(dayOfWeekString, out DayOfWeek dayOfWeek1))
					{
						var (first, second) = (dayOfWeek1, DayOfWeek.Monday);
					}
				}
			}
		}
		""");

	[Test]
	public Task TestShouldNotFlagChainedAssignment() => VerifyAnalyzerAsync(
		"""
		using System;

		namespace Tests
		{
			class Test
			{
				void Method(string dayOfWeekString)
				{
					if (Enum.TryParse(dayOfWeekString, out DayOfWeek dayOfWeek1))
					{
						DayOfWeek b;
						DayOfWeek a = (b = dayOfWeek1);

					}
				}
			}
		}
		""");
}
