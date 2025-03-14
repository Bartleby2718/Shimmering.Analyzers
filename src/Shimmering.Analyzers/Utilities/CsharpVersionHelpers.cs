namespace Shimmering.Analyzers.Utilities;

internal static class CsharpVersionHelpers
{
	public static bool SupportsCollectionExpressions(SyntaxNodeAnalysisContext context) =>
		context.Node.SyntaxTree.Options is CSharpParseOptions { LanguageVersion: >= LanguageVersion.CSharp12 };

	public static bool SupportsPrimaryConstructors(SyntaxNodeAnalysisContext context) =>
		context.Node.SyntaxTree.Options is CSharpParseOptions { LanguageVersion: >= LanguageVersion.CSharp12 };
}
