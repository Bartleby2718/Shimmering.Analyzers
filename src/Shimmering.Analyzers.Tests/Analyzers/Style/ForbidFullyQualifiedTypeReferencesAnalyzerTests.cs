using System.Threading.Tasks;
using NUnit.Framework;
using Shimmering.Analyzers.Analyzers.Style;

namespace Shimmering.Analyzers.Tests.Analyzers.Style;

public class ForbidFullyQualifiedTypeReferencesAnalyzerTests : ShimmeringAnalyzerTests<ForbidFullyQualifiedTypeReferencesAnalyzer>
{
	[Test]
	public Task TestIgnoreWhenUsingDirectiveExists() => VerifyAnalyzerAsync(
		"using System.Text;\r\n\r\nnamespace Tests\r\n{\r\n\tclass Test\r\n\t{\r\n\t\tvoid Method()\r\n\t\t{\r\n\t\t\tvar encoding = System.Text.Encoding.UTF8;\r\n\t\t}\r\n\t}\r\n}");

	[Test]
	public Task TestIgnoreWhenInSameNamespace() => VerifyAnalyzerAsync(
		"namespace Tests\r\n{\r\n\tclass ClassA {}\r\n\r\n\tclass ClassB\r\n\t{\r\n\t\tvoid Method()\r\n\t\t{\r\n\t\t\tvar reference = Tests.ClassA.Equals(null, null);\r\n\t\t}\r\n\t}\r\n}");

	[Test]
	public Task TestIgnoreInsideUsingDirective() => VerifyAnalyzerAsync(
		"using MyEncoding = System.Text.Encoding;\r\n\r\nnamespace Tests\r\n{\r\n\tclass Test {}\r\n}");

	[Test]
	public Task TestFlagFullyQualifiedNestedType() => VerifyAnalyzerAsync(
		"namespace Tests\r\n{\r\n\tclass Test\r\n\t{\r\n\t\tvoid Method()\r\n\t\t{\r\n\t\t\tvar inner = [|MyNamespace.Outer.Inner|].SomeProperty;\r\n\t\t}\r\n\t}\r\n}\r\n\r\nnamespace MyNamespace\r\n{\r\n\tpublic class Outer\r\n\t{\r\n\t\tpublic class Inner\r\n\t\t{\r\n\t\t\tpublic static int SomeProperty = 42;\r\n\t\t}\r\n\t}\r\n}");

	[Test]
	public Task TestFlagFullyQualifiedGenericType() => VerifyAnalyzerAsync(
		"namespace Tests\r\n{\r\n\tclass Test\r\n\t{\r\n\t\tvoid Method()\r\n\t\t{\r\n\t\t\tvar list = new [|System.Collections.Generic.List<int>|]();\r\n\t\t}\r\n\t}\r\n}");
}
