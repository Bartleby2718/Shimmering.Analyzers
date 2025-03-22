using Shimmering.Analyzers.Utilities;

namespace Shimmering.Analyzers.UsageRules.RedundantSpreadElement;

/// <summary>
/// Reports instances of redundant spread elements in a collection expression, like [1, .. new[] { 2, 3 }, 4].
/// </summary>
// See also: https://github.com/dotnet/roslyn/blob/main/src/Analyzers/CSharp/Analyzers/UseCollectionExpression/CSharpUseCollectionExpressionForArrayDiagnosticAnalyzer.cs
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class RedundantSpreadElementAnalyzer : ShimmeringSyntaxNodeAnalyzer
{
	private const string Title = "Inline spread element";
	private const string Message = "Inline spread element";
	private const string Category = "Usage";

	private static readonly DiagnosticDescriptor Rule = CreateRule(
		DiagnosticIds.UsageRules.RedundantSpreadElement,
		Title,
		Message,
		Category,
		DiagnosticSeverity.Info,
		isEnabledByDefault: true);

	public override string SampleCode => """
		namespace Tests;
		class Test
		{
			int[] Array => [1, [|.. new[] { 2, 3 }|], 4];
		}
		""";

	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

	public override void RegisterSyntaxNodeAction(AnalysisContext context)
	{
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
