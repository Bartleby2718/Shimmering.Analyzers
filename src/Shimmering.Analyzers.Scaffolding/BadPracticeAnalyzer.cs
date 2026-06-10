using Shimmering.Analyzers.Core;
using Shimmering.Analyzers.Utilities;

namespace Shimmering.Analyzers.CATEGORY_PLACEHOLDERRules.BadPractice;

/// <summary>
/// TODO.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class BadPracticeAnalyzer : Core.ShimmeringAnalyzer
{
	private const string Title = "TODO:";
	private const string Message = "TODO:";
	private const string Category = "ShimmeringCATEGORY_PLACEHOLDER";

	private static readonly DiagnosticDescriptor Rule = RuleFactory.Create(
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

	protected override void InitializeCore(AnalysisContext context)
	{
		// TODO: context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.MyExpression);
	}

	private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
	{
		// TODO:
	}
}
