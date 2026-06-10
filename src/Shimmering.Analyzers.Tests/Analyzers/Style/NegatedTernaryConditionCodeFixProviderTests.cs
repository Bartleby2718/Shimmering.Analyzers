namespace Shimmering.Analyzers.Tests.Analyzers.Style;

using Shimmering.Analyzers.Analyzers.Style;
using Shimmering.Analyzers.CodeFixes.Style;

public class NegatedTernaryConditionCodeFixProviderTests : ShimmeringCodeFixProviderTests<NegatedTernaryConditionAnalyzer, NegatedTernaryConditionCodeFixProvider>
{
	[Test]
	public Task TestTernaryInOneLine() => VerifyCodeFixAsync(
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
	public Task TestTriviaForLeadingOperators() => VerifyCodeFixAsync(
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
	public Task TestTriviaForTrailingOperators() => VerifyCodeFixAsync(
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

#pragma warning disable SA1027 // Use tabs correctly
	[Test]
	public Task TestBugReproNegatedTernary() => VerifyCodeFixAsync(
		"""
		class C {
			void M() {
				var x = [|!true // line 1
					? 0 // line 2
					: 1|]; // line 3
			}
		}
		""",
		"""
		class C {
			void M() {
				var x = true // line 1
		            ? 1
		            : 0; // line 3
			}
		}
		""");
#pragma warning restore SA1027 // Use tabs correctly
}
