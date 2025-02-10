namespace Shimmering.Analyzers.BadPractice;

/// <summary>
/// TODO.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
internal sealed class BadPracticeAnalyzer : DiagnosticAnalyzer
{
	private const string Title = "TODO:";
	private const string Message = "TODO:";
	private const string Category = "TODO:";

	private static readonly DiagnosticDescriptor Rule = new(
		DiagnosticIds.BadPractice,
		Title,
		Message,
		Category,
		DiagnosticSeverity.Info,
		isEnabledByDefault: true);

	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

	public override void Initialize(AnalysisContext context)
	{
		context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
		context.EnableConcurrentExecution();
		// TODO: context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.MyExpression);
	}

	private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
	{
		// TODO:
	}
}
