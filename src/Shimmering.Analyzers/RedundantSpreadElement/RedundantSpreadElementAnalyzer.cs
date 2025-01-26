using Shimmering.Analyzers.Utilities;

namespace Shimmering.Analyzers.RedundantSpreadElement;

/// <summary>
/// Reports instances of redundant nonempty spread elements in a collection expression, like [1, .. new[] { 2, 3 }, 4].
/// </summary>
// See also: https://github.com/dotnet/roslyn/blob/main/src/Analyzers/CSharp/Analyzers/UseCollectionExpression/CSharpUseCollectionExpressionForArrayDiagnosticAnalyzer.cs
[DiagnosticAnalyzer(LanguageNames.CSharp)]
internal sealed class RedundantSpreadElementAnalyzer : DiagnosticAnalyzer
{
	private const string Title = "Inline spread element";
	private const string Message = "Inline nonempty spread element";
	private const string Category = "Style";

	private static readonly DiagnosticDescriptor Rule = new(
		DiagnosticIds.RedundantSpreadElement,
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
		context.RegisterSyntaxNodeAction(AnalyzeCollectionExpression, SyntaxKind.CollectionExpression);
	}

	private static void AnalyzeCollectionExpression(SyntaxNodeAnalysisContext context)
	{
		if (!CsharpVersionHelpers.SupportsCollectionExpressions(context)) { return; }

		var collectionExpression = (CollectionExpressionSyntax)context.Node;
		foreach (var collectionElement in collectionExpression.Elements)
		{
			if (collectionElement is not SpreadElementSyntax spreadElement) { continue; }

			if (RedundantSpreadElementHelpers.TryGetInnerElementsOfSpreadElement(spreadElement, out _))
			{
				context.ReportDiagnostic(Diagnostic.Create(Rule, spreadElement.GetLocation()));
			}
		}
	}
}
