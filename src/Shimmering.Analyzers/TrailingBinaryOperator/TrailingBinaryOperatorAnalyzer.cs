namespace Shimmering.Analyzers.TrailingBinaryOperator;

/// <summary>
/// Reports instances of trailing binary operators.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
internal sealed class TrailingBinaryOperatorAnalyzer : ShimmeringSyntaxNodeAnalyzer
{
	private const string Title = "Binary operators should be leading, not trailing";
	private const string Message = "Move binary operator to the beginning of the line";
	private const string Category = "Style";

	private static readonly DiagnosticDescriptor Rule = CreateRule(
		DiagnosticIds.TrailingBinaryOperator,
		Title,
		Message,
		Category,
		DiagnosticSeverity.Info,
		isEnabledByDefault: false); // this is a matter of taste

	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

	public override void RegisterSyntaxNodeAction(AnalysisContext context)
	{
		context.RegisterSyntaxNodeAction(
			AnalyzeBinaryExpression,
			// The list of expressions covered by BinaryExpressionSyntax, according to its doc comment at the time of writing.
			// This can get outdated as C# adds more binary operators, but this should be sufficiently exhaustive.
			SyntaxKind.AddExpression,
			SyntaxKind.SubtractExpression,
			SyntaxKind.MultiplyExpression,
			SyntaxKind.DivideExpression,
			SyntaxKind.ModuloExpression,
			SyntaxKind.LeftShiftExpression,
			SyntaxKind.RightShiftExpression,
			SyntaxKind.UnsignedRightShiftExpression,
			SyntaxKind.LogicalOrExpression,
			SyntaxKind.LogicalAndExpression,
			SyntaxKind.BitwiseOrExpression,
			SyntaxKind.BitwiseAndExpression,
			SyntaxKind.ExclusiveOrExpression,
			SyntaxKind.EqualsExpression,
			SyntaxKind.NotEqualsExpression,
			SyntaxKind.LessThanExpression,
			SyntaxKind.LessThanOrEqualExpression,
			SyntaxKind.GreaterThanExpression,
			SyntaxKind.GreaterThanOrEqualExpression,
			SyntaxKind.IsExpression,
			SyntaxKind.AsExpression,
			SyntaxKind.CoalesceExpression);
	}

	private static void AnalyzeBinaryExpression(SyntaxNodeAnalysisContext context)
	{
		var binaryExpression = (BinaryExpressionSyntax)context.Node;

		var leftOperand = binaryExpression.Left.GetLocation().GetLineSpan();
		var endLineOfLeftOperand = leftOperand.EndLinePosition.Line;

		var rightOperand = binaryExpression.Right.GetLocation().GetLineSpan();
		var startLineOfRightOperand = rightOperand.StartLinePosition.Line;

		// skip if both operands are on the same line
		if (endLineOfLeftOperand == startLineOfRightOperand) { return; }

		var operatorToken = binaryExpression.OperatorToken;
		var operatorLine = operatorToken.GetLocation().GetLineSpan().StartLinePosition.Line;
		// if the operator is trailing, then the operator is on the same line as the left operand
		if (endLineOfLeftOperand == operatorLine)
		{
			context.ReportDiagnostic(Diagnostic.Create(Rule, operatorToken.GetLocation()));
		}
	}
}
