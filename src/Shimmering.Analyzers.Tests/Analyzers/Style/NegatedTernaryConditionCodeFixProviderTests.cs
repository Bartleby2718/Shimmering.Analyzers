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
		"""
		namespace Tests
		{
			class Test
			{
				public string MyString() =>
					// before condition
					true ?
					// before true
					"2" :
					// before false
					"1";
			}
		}
		""");

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
					? 1 // line 2
					: 0; // line 3
			}
		}
		""");
}
