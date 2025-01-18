namespace Shimmering.Analyzers.Utilities;

internal static class CsharpVersionHelpers
{
	public static bool SupportsCollectionExpressions(SyntaxNodeAnalysisContext context) =>
		context.Node.SyntaxTree.Options is CSharpParseOptions { LanguageVersion: >= LanguageVersion.CSharp12 };
}
