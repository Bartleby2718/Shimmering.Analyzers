using System.Threading.Tasks;
using NUnit.Framework;
using Shimmering.Analyzers.Analyzers.Style;

namespace Shimmering.Analyzers.Tests.Analyzers.Style;

public class ForbidFullyQualifiedTypeReferencesAnalyzerTests : ShimmeringAnalyzerTests<ForbidFullyQualifiedTypeReferencesAnalyzer>
{
	[Test]
	public Task TestIgnoreWhenUsingDirectiveExists() => VerifyAnalyzerAsync(
		"""
		using System.Text;

		namespace Tests
		{
			class Test
			{
				void Method()
				{
					var encoding = System.Text.Encoding.UTF8;
				}
			}
		}
		""");

	[Test]
	public Task TestIgnoreWhenInSameNamespace() => VerifyAnalyzerAsync(
		"""
		namespace Tests
		{
			class ClassA {}

			class ClassB
			{
				void Method()
				{
					var reference = Tests.ClassA.Equals(null, null);
				}
			}
		}
		""");

	[Test]
	public Task TestIgnoreInsideUsingDirective() => VerifyAnalyzerAsync(
		"""
		using MyEncoding = System.Text.Encoding;

		namespace Tests
		{
			class Test {}
		}
		""");

	[Test]
	public Task TestFlagFullyQualifiedNestedType() => VerifyAnalyzerAsync(
		"""
		namespace Tests
		{
			class Test
			{
				void Method()
				{
					var inner = [|MyNamespace.Outer.Inner|].SomeProperty;
				}
			}
		}

		namespace MyNamespace
		{
			public class Outer
			{
				public class Inner
				{
					public static int SomeProperty = 42;
				}
			}
		}
		""");

	[Test]
	public Task TestFlagFullyQualifiedGenericType() => VerifyAnalyzerAsync(
		"""
		namespace Tests
		{
			class Test
			{
				void Method()
				{
					var list = new [|System.Collections.Generic.List<int>|]();
				}
			}
		}
		""");
}
