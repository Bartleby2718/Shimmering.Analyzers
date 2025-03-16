using Shimmering.Analyzers.StyleRules.InlineSingleUseOutVariable;

namespace Shimmering.Analyzers.Tests.StyleRules.InlineSingleUseOutVariable;

using Verifier = CSharpCodeFixVerifier<
	InlineSingleUseOutVariableAnalyzer,
	InlineSingleUseOutVariableCodeFixProvider,
	DefaultVerifier>;

public class InlineSingleUseOutVariableCodeFixProviderTests : ShimmeringCodeFixProviderTests<InlineSingleUseOutVariableAnalyzer, InlineSingleUseOutVariableCodeFixProvider>
{
	[Test]
	public Task TestOutVariableIsImplicitAndAssignedVariableIsExplicit() => Verifier.VerifyCodeFixAsync(
		"""
		using System;
		using System.Collections.Generic;

		namespace Tests
		{
			class Test
			{
				void Method()
				{
					var exists = new Dictionary<string, int>().TryGetValue("key", out var [|value|]);
					Console.WriteLine("assignment may not happen immediately");
					int value2 = value;
				}
			}
		}
		""",
		"""
		using System;
		using System.Collections.Generic;

		namespace Tests
		{
			class Test
			{
				void Method()
				{
					var exists = new Dictionary<string, int>().TryGetValue("key", out var value2);
					Console.WriteLine("assignment may not happen immediately");
				}
			}
		}
		""");

	[Test]
	public Task TestOutVariableIsImplicitAndAssignedVariableIsImplicit() => Verifier.VerifyCodeFixAsync(
		"""
		using System;
		using System.Collections.Generic;

		namespace Tests
		{
			class Test
			{
				void Method()
				{
					var exists = new Dictionary<string, int>().TryGetValue("key", out var [|value|]);
					Console.WriteLine("assignment may not happen immediately");
					var value2 = value;
				}
			}
		}
		""",
		"""
		using System;
		using System.Collections.Generic;

		namespace Tests
		{
			class Test
			{
				void Method()
				{
					var exists = new Dictionary<string, int>().TryGetValue("key", out var value2);
					Console.WriteLine("assignment may not happen immediately");
				}
			}
		}
		""");

	[Test]
	public Task TestOutVariableIsExplicitAndAssignedVariableIsExplicit() => Verifier.VerifyCodeFixAsync(
		"""
		using System;
		using System.Collections.Generic;

		namespace Tests
		{
			class Test
			{
				void Method()
				{
					var exists = new Dictionary<string, int>().TryGetValue("key", out int [|value|]);
					Console.WriteLine("assignment may not happen immediately");
					int value2 = value;
				}
			}
		}
		""",
		"""
		using System;
		using System.Collections.Generic;

		namespace Tests
		{
			class Test
			{
				void Method()
				{
					var exists = new Dictionary<string, int>().TryGetValue("key", out int value2);
					Console.WriteLine("assignment may not happen immediately");
				}
			}
		}
		""");

	[Test]
	public Task TestOutVariableIsExplicitAndAssignedVariableIsImplicit() => Verifier.VerifyCodeFixAsync(
		"""
		using System;
		using System.Collections.Generic;

		namespace Tests
		{
			class Test
			{
				void Method()
				{
					var exists = new Dictionary<string, int>().TryGetValue("key", out int [|value|]);
					Console.WriteLine("assignment may not happen immediately");
					var value2 = value;
				}
			}
		}
		""",
		"""
		using System;
		using System.Collections.Generic;

		namespace Tests
		{
			class Test
			{
				void Method()
				{
					var exists = new Dictionary<string, int>().TryGetValue("key", out int value2);
					Console.WriteLine("assignment may not happen immediately");
				}
			}
		}
		""");

	[Test]
	public Task TestInlineSimplAssignmentWithExplicitOutVariableType() => Verifier.VerifyCodeFixAsync(
		"""
		using System;
		using System.Collections.Generic;

		namespace Tests
		{
			class Test
			{
				void Method()
				{
					int value2 = 0;
					var exists = new Dictionary<string, int>().TryGetValue("key", out int [|value|]);
					Console.WriteLine("assignment may not happen immediately");
					value2 = value;
				}
			}
		}
		""",
		"""
		using System;
		using System.Collections.Generic;

		namespace Tests
		{
			class Test
			{
				void Method()
				{
					int value2 = 0;
					var exists = new Dictionary<string, int>().TryGetValue("key", out value2);
					Console.WriteLine("assignment may not happen immediately");
				}
			}
		}
		""");

	[Test]
	public Task TestInlineSimplAssignmentWithImplicitOutVariableType() => Verifier.VerifyCodeFixAsync(
		"""
		using System;
		using System.Collections.Generic;

		namespace Tests
		{
			class Test
			{
				void Method()
				{
					int value2 = 0;
					var exists = new Dictionary<string, int>().TryGetValue("key", out var [|value|]);
					Console.WriteLine("assignment may not happen immediately");
					value2 = value;
				}
			}
		}
		""",
		"""
		using System;
		using System.Collections.Generic;

		namespace Tests
		{
			class Test
			{
				void Method()
				{
					int value2 = 0;
					var exists = new Dictionary<string, int>().TryGetValue("key", out value2);
					Console.WriteLine("assignment may not happen immediately");
				}
			}
		}
		""");

	[Test]
	public Task TestMultipleOutParameters() => Verifier.VerifyCodeFixAsync(
		"""
		using System;

		namespace Tests
		{
			class Test
			{
				void Method()
				{
					Another(out int [|value1|], out var [|value2|]);
					Console.WriteLine("assignment may not happen immediately");
					var first = value1;
					int second = value2;
				}

				static void Another(out int value1, out int value2)
				{
					value1 = 1;
					value2 = 2;
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
					Another(out int first, out var second);
					Console.WriteLine("assignment may not happen immediately");
				}

				static void Another(out int value1, out int value2)
				{
					value1 = 1;
					value2 = 2;
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
					int second = 0;
					Another(out /* before explicit type */int/* after explicit type */ /* before first variable */[|value1|]/* after first variable */, out /* before implicit type */var/* after explicit type */ /* before second variable */[|value2|]/* after second variable */);
					Console.WriteLine("assignment may not happen immediately");
					var first = value1;
					second = value2;
				}

				static void Another(out int value1, out int value2)
				{
					value1 = 1;
					value2 = 2;
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
					int second = 0;
					Another(out /* before explicit type */int/* after explicit type */ /* before first variable */first, out /* before implicit type */second);
					Console.WriteLine("assignment may not happen immediately");
				}

				static void Another(out int value1, out int value2)
				{
					value1 = 1;
					value2 = 2;
				}
			}
		}
		""");
}
