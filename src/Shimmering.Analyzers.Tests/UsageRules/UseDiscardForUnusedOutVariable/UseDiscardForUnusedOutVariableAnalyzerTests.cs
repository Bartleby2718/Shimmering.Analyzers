using Shimmering.Analyzers.UsageRules.UseDiscardForUnusedOutVariable;

namespace Shimmering.Analyzers.Tests.UsageRules.UseDiscardForUnusedOutVariable;

using Verifier = CSharpAnalyzerVerifier<
	UseDiscardForUnusedOutVariableAnalyzer,
	DefaultVerifier>;

public class UseDiscardForUnusedOutVariableAnalyzerTests : ShimmeringAnalyzerTests<UseDiscardForUnusedOutVariableAnalyzer>
{
	[Test]
	public Task TestIgnoreIfOutVariableIsRead() => Verifier.VerifyAnalyzerAsync(
		"""
		using System;

		namespace Tests
		{
			class Test
			{
				void Method()
				{
					if (Enum.TryParse<DayOfWeek>("Sunday", out DayOfWeek dayOfWeek))
					{
						 Console.WriteLine(dayOfWeek);
					}
				}
			}
		}
		""");

	[Test]
	public Task TestIgnoreIfOutVariableIsWritten() => Verifier.VerifyAnalyzerAsync(
		"""
		using System;

		namespace Tests
		{
			class Test
			{
				void Method()
				{
					if (Enum.TryParse<DayOfWeek>("Sunday", out DayOfWeek dayOfWeek))
					{
						 dayOfWeek = DayOfWeek.Sunday;
					}
				}
			}
		}
		""");
}
