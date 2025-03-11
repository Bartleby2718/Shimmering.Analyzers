namespace Shimmering.Analyzers.StyleRules.NegatedTernaryCondition;

/// <summary>
/// Reports instances of a tenary expression that starts with a negation operator in the condition part.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
internal sealed class NegatedTernaryConditionAnalyzer : ShimmeringSyntaxNodeAnalyzer
{
	private const string Title = "Avoid negation in the ternary condition";
	private const string Message = "This ternary condition has a negation";
	private const string Category = "Style";

	private static readonly DiagnosticDescriptor Rule = CreateRule(
		DiagnosticIds.StyleRules.NegatedTernaryCondition,
		Title,
		Message,
		Category,
		DiagnosticSeverity.Info,
		isEnabledByDefault: true);

	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

	public override void RegisterSyntaxNodeAction(AnalysisContext context)
	{
		context.RegisterSyntaxNodeAction(AnalyzeConditionalExpression, SyntaxKind.ConditionalExpression);
	}

	private static void AnalyzeConditionalExpression(SyntaxNodeAnalysisContext context)
	{
		var conditionalExpression = (ConditionalExpressionSyntax)context.Node;

		// Ignore ternary expressions that are nested inside the condition, as it may be an intentional choice for readability.
		if (conditionalExpression.Condition is ConditionalExpressionSyntax
			|| conditionalExpression.WhenTrue is ConditionalExpressionSyntax
			|| conditionalExpression.WhenFalse is ConditionalExpressionSyntax)
		{
			return;
		}

		// Check if the condition is a negation (i.e. !expression)
		if (conditionalExpression.Condition is PrefixUnaryExpressionSyntax prefixUnary &&
			prefixUnary.IsKind(SyntaxKind.LogicalNotExpression))
		{
			// Report the diagnostic on the whole conditional expression.
			var diagnostic = Diagnostic.Create(Rule, conditionalExpression.GetLocation());
			context.ReportDiagnostic(diagnostic);
		}
	}
}
