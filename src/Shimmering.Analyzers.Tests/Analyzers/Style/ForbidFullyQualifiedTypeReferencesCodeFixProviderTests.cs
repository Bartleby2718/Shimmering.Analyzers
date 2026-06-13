using System.Threading.Tasks;
using NUnit.Framework;
using Shimmering.Analyzers.Analyzers.Style;
using Shimmering.Analyzers.CodeFixes.Style;

namespace Shimmering.Analyzers.Tests.Analyzers.Style;

public class ForbidFullyQualifiedTypeReferencesCodeFixProviderTests : ShimmeringCodeFixProviderTests<ForbidFullyQualifiedTypeReferencesAnalyzer, ForbidFullyQualifiedTypeReferencesCodeFixProvider>
{
	[Test]
	public Task TestSimplifyStandardTypeReference() => VerifyCodeFixAsync(
		"namespace Tests\r\n{\r\n\tclass Test\r\n\t{\r\n\t\tvoid Method()\r\n\t\t{\r\n\t\t\tvar encoding = [|System.Text.Encoding|].UTF8;\r\n\t\t}\r\n\t}\r\n}",
		"using System.Text;\r\nnamespace Tests\r\n{\r\n\tclass Test\r\n\t{\r\n\t\tvoid Method()\r\n\t\t{\r\n\t\t\tvar encoding = Encoding.UTF8;\r\n\t\t}\r\n\t}\r\n}");

	[Test]
	public Task TestSimplifyNestedTypeReference() => VerifyCodeFixAsync(
		"namespace Tests\r\n{\r\n\tclass Test\r\n\t{\r\n\t\tvoid Method()\r\n\t\t{\r\n\t\t\tvar inner = new [|MyNamespace.Outer.Inner|]();\r\n\t\t}\r\n\t}\r\n}\r\n\r\nnamespace MyNamespace\r\n{\r\n\tpublic class Outer\r\n\t{\r\n\t\tpublic class Inner {}\r\n\t}\r\n}",
		"using MyNamespace;\r\nnamespace Tests\r\n{\r\n\tclass Test\r\n\t{\r\n\t\tvoid Method()\r\n\t\t{\r\n\t\t\tvar inner = new Outer.Inner();\r\n\t\t}\r\n\t}\r\n}\r\n\r\nnamespace MyNamespace\r\n{\r\n\tpublic class Outer\r\n\t{\r\n\t\tpublic class Inner {}\r\n\t}\r\n}");

	[Test]
	public Task TestSimplifyGenericTypeArgument() => VerifyCodeFixAsync(
		"using System.Collections.Generic;\r\n\r\nnamespace Tests\r\n{\r\n\tclass Test\r\n\t{\r\n\t\tvoid Method()\r\n\t\t{\r\n\t\t\tvar list = new List<[|System.Text.Encoding|]>();\r\n\t\t}\r\n\t}\r\n}",
		"using System.Collections.Generic;\r\nusing System.Text;\r\n\r\nnamespace Tests\r\n{\r\n\tclass Test\r\n\t{\r\n\t\tvoid Method()\r\n\t\t{\r\n\t\t\tvar list = new List<Encoding>();\r\n\t\t}\r\n\t}\r\n}");

	[Test]
	public Task TestSimplifyAttribute() => VerifyCodeFixAsync(
		"namespace Tests\r\n{\r\n\t[[|System.Diagnostics.CodeAnalysis.SuppressMessage|](\"Category\", \"Id\")]\r\n\tclass Test {}\r\n}",
		"using System.Diagnostics.CodeAnalysis;\r\nnamespace Tests\r\n{\r\n\t[SuppressMessage(\"Category\", \"Id\")]\r\n\tclass Test {}\r\n}");

	[Test]
	public Task TestSimplifyGlobalAliasQualified() => VerifyCodeFixAsync(
		"namespace Tests\r\n{\r\n\tclass Test\r\n\t{\r\n\t\tvoid Method()\r\n\t\t{\r\n\t\t\tvar encoding = [|global::System.Text.Encoding|].UTF8;\r\n\t\t}\r\n\t}\r\n}",
		"using System.Text;\r\nnamespace Tests\r\n{\r\n\tclass Test\r\n\t{\r\n\t\tvoid Method()\r\n\t\t{\r\n\t\t\tvar encoding = Encoding.UTF8;\r\n\t\t}\r\n\t}\r\n}");
}
