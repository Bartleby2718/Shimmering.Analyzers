namespace Shimmering.Analyzers.BadPractice;

/// <summary>
/// TODO.
/// </summary>
internal sealed class BadPracticeAnalyzer : ShimmeringSyntaxNodeAnalyzer
{
	private const string Title = "TODO:";
	private const string Message = "TODO:";
	private const string Category = "TODO:";

	private static readonly DiagnosticDescriptor Rule = CreateRule(
		DiagnosticIds.BadPractice,
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
