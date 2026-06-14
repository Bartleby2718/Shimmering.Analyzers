using Shimmering.Analyzers.Analyzers.Style;
using Shimmering.Analyzers.CodeFixes.Style;

namespace Shimmering.Analyzers.Tests.Analyzers.Style;

public class RedundantOutVariableCodeFixProviderTests : ShimmeringCodeFixProviderTests<RedundantOutVariableAnalyzer, RedundantOutVariableCodeFixProvider>
{
	[Test]
	public Task TestFieldAssignment() => VerifyCodeFixAsync(
		"""
		using System;
		using System.Collections.Generic;

		namespace Tests
		{
			class Test
			{
				private int _field;

				void Method()
				{
					var exists = new Dictionary<string, int>().TryGetValue("key", [|out var value|]);
					Console.WriteLine("assignment may not happen immediately");
					this._field = value;
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
				private int _field;

				void Method()
				{
					var exists = new Dictionary<string, int>().TryGetValue("key", out this._field);
					Console.WriteLine("assignment may not happen immediately");
				}
			}
		}
		""");

	[Test]
	public Task TestOutVariableIsImplicitAndAssignedVariableIsExplicit() => VerifyCodeFixAsync(
		"""
		using System;
		using System.Collections.Generic;

		namespace Tests
		{
			class Test
			{
				void Method()
				{
					var exists = new Dictionary<string, int>().TryGetValue("key", [|out var value|]);
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
	public Task TestOutVariableIsImplicitAndAssignedVariableIsImplicit() => VerifyCodeFixAsync(
		"""
		using System;
		using System.Collections.Generic;

		namespace Tests
		{
			class Test
			{
				void Method()
				{
					var exists = new Dictionary<string, int>().TryGetValue("key", [|out var value|]);
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
	public Task TestOutVariableIsExplicitAndAssignedVariableIsExplicit() => VerifyCodeFixAsync(
		"""
		using System;
		using System.Collections.Generic;

		namespace Tests
		{
			class Test
			{
				void Method()
				{
					var exists = new Dictionary<string, int>().TryGetValue("key", [|out int value|]);
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
	public Task TestOutVariableIsExplicitAndAssignedVariableIsImplicit() => VerifyCodeFixAsync(
		"""
		using System;
		using System.Collections.Generic;

		namespace Tests
		{
			class Test
			{
				void Method()
				{
					var exists = new Dictionary<string, int>().TryGetValue("key", [|out int value|]);
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
	public Task TestInlineSimpleAssignmentWithExplicitOutVariableType() => VerifyCodeFixAsync(
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
					var exists = new Dictionary<string, int>().TryGetValue("key", [|out int value|]);
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
	public Task TestInlineSimpleAssignmentWithImplicitOutVariableType() => VerifyCodeFixAsync(
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
					var exists = new Dictionary<string, int>().TryGetValue("key", [|out var value|]);
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
	public Task TestMultipleOutParameters() => VerifyCodeFixAsync(
		"""
		using System;

		namespace Tests
		{
			class Test
			{
				void Method()
				{
					Another([|out int value1|], [|out var value2|]);
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
					Another(out var first, out int second);
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
	public Task TestTrivia() => VerifyCodeFixAsync(
		"""
		using System;

		namespace Tests
		{
			class Test
			{
				void Method()
				{
					int second = 0;
					Another([|out /* before explicit type */int/* after explicit type */ /* before first variable */value1|]/* after first variable */, [|out /* before implicit type */var/* after explicit type */ /* before second variable */value2|]/* after second variable */);
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
					Another(out /* before explicit type */var/* after explicit type */ /* before first variable */first/* after first variable */, out /* before implicit type */second/* after second variable */);
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
