namespace Shimmering.Analyzers.NegatedTernaryCondition;

/// <summary>
/// Reports instances of a tenary expression that starts with a negation operator in the condition part.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
internal sealed class NegatedTernaryConditionAnalyzer : DiagnosticAnalyzer
{
	private const string Title = "Avoid negation in the ternary condition";
	private const string Message = "This ternary condition has a negation";
	private const string Category = "Style";

	private static readonly DiagnosticDescriptor Rule = new(
		DiagnosticIds.NegatedTernaryCondition,
		Title,
		Message,
		Category,
		DiagnosticSeverity.Info,
		isEnabledByDefault: true,
		helpLinkUri: $"https://github.com/Bartleby2718/Shimmering.Analyzers/blob/main/docs/{DiagnosticIds.NegatedTernaryCondition}.md");

	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

	public override void Initialize(AnalysisContext context)
	{
		context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
		context.EnableConcurrentExecution();
		context.RegisterSyntaxNodeAction(AnalyzeConditionalExpression, SyntaxKind.ConditionalExpression);
	}

	private static void AnalyzeConditionalExpression(SyntaxNodeAnalysisContext context)
	{
		var conditionalExpr = (ConditionalExpressionSyntax)context.Node;
		// Check if the condition is a negation (i.e. !expr)
		if (conditionalExpr.Condition is PrefixUnaryExpressionSyntax prefixUnary &&
			prefixUnary.IsKind(SyntaxKind.LogicalNotExpression))
		{
			// Report the diagnostic on the whole conditional expression.
			var diagnostic = Diagnostic.Create(Rule, conditionalExpr.GetLocation());
			context.ReportDiagnostic(diagnostic);
		}
	}
}
