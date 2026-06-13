using Shimmering.Analyzers.Analyzers.Style;
using Shimmering.Analyzers.CodeFixes.Style;

namespace Shimmering.Analyzers.Tests.Analyzers.Style;

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
                         // before true
                        ? "2" // after true
                              // before false
                        : "1"; // after statement
            }
        }
        """);
#pragma warning restore SA1027 // Use tabs correctly

	[Test]
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
		"namespace Tests\r\n{\r\n\tclass Test\r\n\t{\r\n\t\tpublic string MyString() =>\r\n            // before condition\r\n            true ?\r\n            // before true\r\n            \"2\" :\r\n            // before false\r\n            \"1\";\r\n\t}\r\n}");

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
		"class C {\r\n\tvoid M() {\r\n\t\tvar x = true // line 1\r\n            ? 1 // line 2\r\n            : 0; // line 3\r\n\t}\r\n}");
}
