using Shimmering.Analyzers.Analyzers.Style;
using Shimmering.Analyzers.CodeFixes.Style;

namespace Shimmering.Analyzers.Tests.Analyzers.Style;

public class NegatedTernaryConditionAnalyzerTests : ShimmeringAnalyzerTests<NegatedTernaryConditionAnalyzer>
{
	[Test]
	public Task TestIgnoreTernaryContainingTernary() => VerifyAnalyzerAsync(
		"""
		namespace Tests
		{
			class Test
			{
				public int Method1(int? number1, int? number2) => !number1.HasValue ? 1
					: number2.HasValue ? number2.Value
					: number1.Value;
				public int Method2(int? number1, int? number2) => !number1.HasValue ? 1
					: number2.HasValue ? number2.Value
					: number1.Value;
			}
		}
		""");

	[Test]
	public Task TestIgnoreTernaryWithoutNegationInCondition() => VerifyAnalyzerAsync(
		"""
		namespace Tests
		{
			class Test
			{
				public string MyString() => true ? "true" : "false";
			}
		}
		""");
}
