namespace Shimmering.Analyzers.CATEGORY_PLACEHOLDERRules.BadPractice;

/// <summary>
/// TODO.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class BadPracticeAnalyzer : ShimmeringSyntaxNodeAnalyzer
{
	private const string Title = "TODO:";
	private const string Message = "TODO:";
	private const string Category = "ShimmeringCATEGORY_PLACEHOLDER";

	private static readonly DiagnosticDescriptor Rule = CreateRule(
		DiagnosticIds.CATEGORY_PLACEHOLDERRules.BadPractice,
		Title,
		Message,
		Category,
		DiagnosticSeverity.Info);

	public override string SampleCode => """
		namespace Tests;
		class Test
		{
			void Do()
			{
				// [|the code to flag|] and the rest of the code
			}
		}
		""";

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
