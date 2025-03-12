using Shimmering.Analyzers.UsageRules.MisusedOrDefault;

namespace Shimmering.Analyzers.Tests.UsageRules.MisusedOrDefault;

using Verifier = CSharpCodeFixVerifier<
	MisusedOrDefaultAnalyzer,
	MisusedOrDefaultCodeFixProvider,
	DefaultVerifier>;

public class MisusedOrDefaultCodeFixProviderTests
{
	[Test]
	public Task TestUnsupportedCases() => Verifier.VerifyAnalyzerAsync(
		"""
		using System;
		using System.Linq;

		namespace Tests
		{
			class Test
			{
				public void Do()
				{
					var x = new[] { 1 }.Single()!; // no OrDefault
					var y = (new[] { 1 }.SingleOrDefault())!; // parenthesized expression
				}
			}
		}
		""");

	[Test]
	public Task TestOrDefaultIsReplaced() => Verifier.VerifyCodeFixAsync(
		"""
		using System;
		using System.Linq;

		namespace Tests
		{
			class Test
			{
				public void Do()
				{
					var a = [|new[] { 1 }.SingleOrDefault()!|];
					var b = [|new[] { 1 }.FirstOrDefault(_ => true)!|];
					var c = [|new[] { 1 }.LastOrDefault()!|];
					var d = [|new[] { 1 }.ElementAtOrDefault(0)!|];
				}
			}
		}
		""",
		"""
		using System;
		using System.Linq;

		namespace Tests
		{
			class Test
			{
				public void Do()
				{
					var a = new[] { 1 }.Single();
					var b = new[] { 1 }.First(_ => true);
					var c = new[] { 1 }.Last();
					var d = new[] { 1 }.ElementAt(0);
				}
			}
		}
		""");

	[Test]
#pragma warning disable SA1027 // Use tabs correctly
	public Task TestTrivia() => Verifier.VerifyCodeFixAsync(
		"""
        using System;
        using System.Linq;

        namespace Tests
        {
            class Test
            {
                public void Do()
                {
                    var x = // line before receiver
                        /* right before receiver */ [|new[] { 1 } // right after receiver
                        // line before OrDefault
                        ./* right before member access */SingleOrDefault/* right before member access */()/* between OrDefault and operator*/!|]// right after operator
                        // line after operator
                        ;
                }
            }
        }
        """,
		"""
        using System;
        using System.Linq;

        namespace Tests
        {
            class Test
            {
                public void Do()
                {
                    var x = // line before receiver
                        /* right before receiver */ new[] { 1 } // right after receiver
                        // line before OrDefault
                        ./* right before member access */Single/* right before member access */()/* between OrDefault and operator*/// right after operator
                        // line after operator
                        ;
                }
            }
        }
        """);
#pragma warning restore SA1027 // Use tabs correctly
}
