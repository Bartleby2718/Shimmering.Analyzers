using Shimmering.Analyzers.StyleRules.NegatedTernaryCondition;

namespace Shimmering.Analyzers.Tests.StyleRules.NegatedTernaryCondition;

using Verifier = CSharpCodeFixVerifier<
	NegatedTernaryConditionAnalyzer,
	NegatedTernaryConditionCodeFixProvider,
	DefaultVerifier>;

public class NegatedTernaryConditionCodeFixProviderTests : ShimmeringCodeFixProviderTests<NegatedTernaryConditionAnalyzer, NegatedTernaryConditionCodeFixProvider>
{
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
	public Task TestTriviaForLeadingOperators() => Verifier.VerifyCodeFixAsync(
		"""
        namespace Tests
        {
            class Test
            {
                public string MyString() =>
                    // before condition
                    [|!true // after condition
                        // before true
                        ? "1" // after true
                        // before false
                        : "2"|]; // after statement
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
                    true // after condition
                         // before false
                        ? "2"
                        // before true
                        : "1"; // after statement
            }
        }
        """);
#pragma warning restore SA1027 // Use tabs correctly

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
