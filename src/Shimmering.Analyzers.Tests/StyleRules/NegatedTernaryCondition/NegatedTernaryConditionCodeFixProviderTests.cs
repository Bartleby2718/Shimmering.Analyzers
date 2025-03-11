using Shimmering.Analyzers.StyleRules.NegatedTernaryCondition;

namespace Shimmering.Analyzers.Tests.StyleRules.NegatedTernaryCondition;

using Verifier = CSharpCodeFixVerifier<
	NegatedTernaryConditionAnalyzer,
	NegatedTernaryConditionCodeFixProvider,
	DefaultVerifier>;

public class NegatedTernaryConditionCodeFixProviderTests
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

	[Test]
	public Task TestTernaryInOneLine() => Verifier.VerifyCodeFixAsync(
		"""
		namespace Tests
		{
			class Test
			{
				public string MyString() => [|!true ? "1" : "2"|];
			}
		}
		""",
		"""
		namespace Tests
		{
			class Test
			{
				public string MyString() => [|true ? "2" : "1"|];
			}
		}
		""");

	[Test]
#pragma warning disable SA1027 // Use tabs correctly
	public Task TestTriviaForTrailingOperators() => Verifier.VerifyCodeFixAsync(
		"""
        namespace Tests
        {
            class Test
            {
                public string MyString() =>
                    // before condition
                    [|!true ?
                    // before true
                    "1" :
                    // before false
                    "2"|];
            }
        }
        """,
		"""
        namespace Tests
        {
            class Test
            {
                public string MyString() =>
                    // before condition
                    [|true ?
                    // before false
                    "2" :
                    // before true
                    "1"|];
            }
        }
        """);
#pragma warning restore SA1027 // Use tabs correctly
}
