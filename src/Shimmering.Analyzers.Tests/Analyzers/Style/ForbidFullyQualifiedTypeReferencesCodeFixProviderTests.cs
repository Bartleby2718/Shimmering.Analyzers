using Shimmering.Analyzers.Analyzers.Style;
using Shimmering.Analyzers.CodeFixes.Style;

namespace Shimmering.Analyzers.Tests.Analyzers.Style;

public class ForbidFullyQualifiedTypeReferencesCodeFixProviderTests : ShimmeringCodeFixProviderTests<ForbidFullyQualifiedTypeReferencesAnalyzer, ForbidFullyQualifiedTypeReferencesCodeFixProvider>
{
	[Test]
	public Task TestSimplifyStandardTypeReference() => VerifyCodeFixAsync(
		"""
		namespace Tests
		{
			class Test
			{
				void Method()
				{
					var encoding = [|System.Text.Encoding|].UTF8;
				}
			}
		}
		""",
		"""
		using System.Text;
		namespace Tests
		{
			class Test
			{
				void Method()
				{
					var encoding = Encoding.UTF8;
				}
			}
		}
		""");

	[Test]
	public Task TestSimplifyNestedTypeReference() => VerifyCodeFixAsync(
		"""
		namespace Tests
		{
			class Test
			{
				void Method()
				{
					var inner = new [|MyNamespace.Outer.Inner|]();
				}
			}
		}

		namespace MyNamespace
		{
			public class Outer
			{
				public class Inner {}
			}
		}
		""",
		"""
		using MyNamespace;
		namespace Tests
		{
			class Test
			{
				void Method()
				{
					var inner = new Outer.Inner();
				}
			}
		}

		namespace MyNamespace
		{
			public class Outer
			{
				public class Inner {}
			}
		}
		""");

	[Test]
	public Task TestSimplifyGenericTypeArgument() => VerifyCodeFixAsync(
		"""
		using System.Collections.Generic;

		namespace Tests
		{
			class Test
			{
				void Method()
				{
					var list = new List<[|System.Text.Encoding|]>();
				}
			}
		}
		""",
		"""
		using System.Collections.Generic;
		using System.Text;

		namespace Tests
		{
			class Test
			{
				void Method()
				{
					var list = new List<Encoding>();
				}
			}
		}
		""");

	[Test]
	public Task TestSimplifyAttribute() => VerifyCodeFixAsync(
		"""
		namespace Tests
		{
			[[|System.Diagnostics.CodeAnalysis.SuppressMessage|]("Category", "Id")]
			class Test {}
		}
		""",
		"""
		using System.Diagnostics.CodeAnalysis;
		namespace Tests
		{
			[SuppressMessage("Category", "Id")]
			class Test {}
		}
		""");

	[Test]
	public Task TestSimplifyGlobalAliasQualified() => VerifyCodeFixAsync(
		"""
		namespace Tests
		{
			class Test
			{
				void Method()
				{
					var encoding = [|global::System.Text.Encoding|].UTF8;
				}
			}
		}
		""",
		"""
		using System.Text;
		namespace Tests
		{
			class Test
			{
				void Method()
				{
					var encoding = Encoding.UTF8;
				}
			}
		}
		""");
}
