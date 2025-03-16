using Shimmering.Analyzers.UsageRules.UseDiscardForUnusedOutVariable;

namespace Shimmering.Analyzers.Tests.UsageRules.UseDiscardForUnusedOutVariable;

using Verifier = CSharpCodeFixVerifier<
	UseDiscardForUnusedOutVariableAnalyzer,
	UseDiscardForUnusedOutVariableCodeFixProvider,
	DefaultVerifier>;

public class UseDiscardForUnusedOutVariableCodeFixProviderTests : ShimmeringCodeFixProviderTests<UseDiscardForUnusedOutVariableAnalyzer, UseDiscardForUnusedOutVariableCodeFixProvider>
{
	[Test]
	public Task TestExplicitlyTypedOutVariable() => Verifier.VerifyCodeFixAsync(
		"""
		using System;

		namespace Tests
		{
			class Test
			{
				void Method()
				{
					if (Enum.TryParse<DayOfWeek>("Sunday", out DayOfWeek [|dayOfWeek|]))
					{
						 // dayOfWeek is not used
					}
				}
			}
		}
		""",
		"""
		using System;

		namespace Tests
		{
			class Test
			{
				void Method()
				{
					if (Enum.TryParse<DayOfWeek>("Sunday", out _))
					{
						 // dayOfWeek is not used
					}
				}
			}
		}
		""");

	[Test]
	public Task TestImplicitlyTypedOutVariable() => Verifier.VerifyCodeFixAsync(
		"""
		using System;

		namespace Tests
		{
			class Test
			{
				void Method()
				{
					if (Enum.TryParse<DayOfWeek>("Sunday", out var [|dayOfWeek|]))
					{
						 // dayOfWeek is not used
					}
				}
			}
		}
		""",
		"""
		using System;

		namespace Tests
		{
			class Test
			{
				void Method()
				{
					if (Enum.TryParse<DayOfWeek>("Sunday", out _))
					{
						 // dayOfWeek is not used
					}
				}
			}
		}
		""");

	[Test]
	public Task TestTrivia() => Verifier.VerifyCodeFixAsync(
		"""
		using System;

		namespace Tests
		{
			class Test
			{
				void Method()
				{
					if (Enum.TryParse<DayOfWeek>("Sunday", out /* before var */var/* after var */ /* before parameter */[|dayOfWeek|]/* after variable */))
					{
						 // dayOfWeek is not used
					}
				}
			}
		}
		""",
		"""
		using System;

		namespace Tests
		{
			class Test
			{
				void Method()
				{
					if (Enum.TryParse<DayOfWeek>("Sunday", out /* before var */_/* after variable */))
					{
						 // dayOfWeek is not used
					}
				}
			}
		}
		""");
}
