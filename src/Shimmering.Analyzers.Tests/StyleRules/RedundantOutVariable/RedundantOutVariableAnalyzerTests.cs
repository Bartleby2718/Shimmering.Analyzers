using Shimmering.Analyzers.StyleRules.RedundantOutVariable;

namespace Shimmering.Analyzers.Tests.StyleRules.RedundantOutVariable;

using Verifier = CSharpAnalyzerVerifier<
	RedundantOutVariableAnalyzer,
	DefaultVerifier>;

public class RedundantOutVariableAnalyzerTests : ShimmeringAnalyzerTests<RedundantOutVariableAnalyzer>
{
	[Test]
	public Task TestShouldNotFlagIfNotUsedForAssignment() => Verifier.VerifyAnalyzerAsync(
		"""
		using System;

		namespace Tests
		{
			class Test
			{
				void Method(string dayOfWeekString)
				{
					if (Enum.TryParse<DayOfWeek>(dayOfWeekString, out DayOfWeek dayOfWeek1))
					{
						// not an assignment
						Console.WriteLine(dayOfWeek1);
					}
				}
			}
		}
		""");

	[Test]
	public Task TestShouldNotFlagMultipleDeclaration() => Verifier.VerifyAnalyzerAsync(
		"""
		using System;

		namespace Tests
		{
			class Test
			{
				void Method(string dayOfWeekString)
				{
					if (Enum.TryParse<DayOfWeek>(dayOfWeekString, out DayOfWeek dayOfWeek1))
					{
						DayOfWeek first = dayOfWeek1, second = DayOfWeek.Monday;
					}
				}
			}
		}
		""");

	[Test]
	public Task TestShouldNotFlagTupleAssignment() => Verifier.VerifyAnalyzerAsync(
		"""
		using System;

		namespace Tests
		{
			class Test
			{
				void Method(string dayOfWeekString)
				{
					if (Enum.TryParse<DayOfWeek>(dayOfWeekString, out DayOfWeek dayOfWeek1))
					{
						var (first, second) = (dayOfWeek1, DayOfWeek.Monday);
					}
				}
			}
		}
		""");

	[Test]
	public Task TestShouldNotFlagChainedAssignment() => Verifier.VerifyAnalyzerAsync(
		"""
		using System;

		namespace Tests
		{
			class Test
			{
				void Method(string dayOfWeekString)
				{
					if (Enum.TryParse<DayOfWeek>(dayOfWeekString, out DayOfWeek dayOfWeek1))
					{
						DayOfWeek b;
						DayOfWeek a = (b = dayOfWeek1);

					}
				}
			}
		}
		""");
}
