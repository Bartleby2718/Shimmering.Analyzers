namespace Shimmering.Analyzers.CATEGORY_PLACEHOLDERRules.BadPractice;

/// <summary>
/// TODO.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
internal sealed class BadPracticeAnalyzer : ShimmeringSyntaxNodeAnalyzer
{
	private const string Title = "TODO:";
	private const string Message = "TODO:";
	private const string Category = "CATEGORY_PLACEHOLDER";

	private static readonly DiagnosticDescriptor Rule = CreateRule(
		DiagnosticIds.CATEGORY_PLACEHOLDERRules.BadPractice,
		Title,
		Message,
		Category,
		DiagnosticSeverity.Info,
		isEnabledByDefault: true);

	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

	public override void RegisterSyntaxNodeAction(AnalysisContext context)
	{
		// TODO: context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.MyExpression);
	}

	private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
	{
		// TODO:
	}
}
