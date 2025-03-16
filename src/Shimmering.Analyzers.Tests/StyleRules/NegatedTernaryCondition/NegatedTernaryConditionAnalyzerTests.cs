using Shimmering.Analyzers.StyleRules.NegatedTernaryCondition;

namespace Shimmering.Analyzers.Tests.StyleRules.NegatedTernaryCondition;

using Verifier = CSharpCodeFixVerifier<
	NegatedTernaryConditionAnalyzer,
	NegatedTernaryConditionCodeFixProvider,
	DefaultVerifier>;

public class NegatedTernaryConditionAnalyzerTests
{
	[Test]
	public Task TestIgnoreTernaryContainingTernary() => Verifier.VerifyAnalyzerAsync(
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
	public Task TestIgnoreTernaryWithoutNegationInCondition() => Verifier.VerifyAnalyzerAsync(
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
