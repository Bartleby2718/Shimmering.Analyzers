namespace Shimmering.Analyzers.TrailingBinaryOperator;

/// <summary>
/// Moves trailing binary operators to the beginning of the line.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(TrailingBinaryOperatorCodeFixProvider)), Shared]
public sealed class TrailingBinaryOperatorCodeFixProvider : CodeFixProvider
{
	private const string Title = "Make trailing binary operators leading";

	private static readonly SyntaxTrivia SingleWhitespaceTrivia = SyntaxFactory.Whitespace(" ");

	public sealed override ImmutableArray<string> FixableDiagnosticIds =>
		[DiagnosticIds.TrailingBinaryOperator];

	public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

	public override async Task RegisterCodeFixesAsync(CodeFixContext context)
	{
		var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
		if (root == null) { return; }

		var diagnostic = context.Diagnostics.First();
		var diagnosticSpan = diagnostic.Location.SourceSpan;

		var node = root.FindNode(diagnosticSpan);
		var binaryExpression = node.DescendantNodesAndSelf()
			.OfType<BinaryExpressionSyntax>()
			.FirstOrDefault();
		if (binaryExpression == null) { return; }

		context.RegisterCodeFix(
			CodeAction.Create(
				Title,
				ct => MoveBinaryOperatorAsync(context.Document, binaryExpression, ct),
				nameof(TrailingBinaryOperatorCodeFixProvider)),
			diagnostic);
	}

	private static async Task<Document> MoveBinaryOperatorAsync(Document document, BinaryExpressionSyntax binaryExpression, CancellationToken cancellationToken)
	{
		var operatorToken = binaryExpression.OperatorToken;
		var leftOperand = binaryExpression.Left;
		var rightOperand = binaryExpression.Right;

		// From the trailing trivia of the left operand, remove the final newline and any whitespaces before it
		var newline = operatorToken.TrailingTrivia[operatorToken.TrailingTrivia.Count - 1];
		var newLeftOperandTrailingTrivia = leftOperand.GetTrailingTrivia()
			// reverse to remove trailing whitespaces
			.Reverse()
			.SkipWhile(trivia => trivia.IsKind(SyntaxKind.WhitespaceTrivia))
			.Reverse()
			.Append(newline);

		// Ensure there's a space before the new right operand
		var rightOperandLeadingTrivia = rightOperand.GetLeadingTrivia();
		var newRightOperandLeadingTrivia = rightOperandLeadingTrivia.SkipWhile(t => t.IsKind(SyntaxKind.WhitespaceTrivia));
		if (!newRightOperandLeadingTrivia.Any())
		{
			newRightOperandLeadingTrivia = [SingleWhitespaceTrivia];
		}

		// From the leading trivia of the right operand, copy over leading whitespaces to the new operator token
		var newOperatorTokenLeadingTrivia = rightOperandLeadingTrivia.TakeWhile(t => t.IsKind(SyntaxKind.WhitespaceTrivia));
		var updatedOperatorToken = operatorToken
			.WithLeadingTrivia(newOperatorTokenLeadingTrivia)
			.WithTrailingTrivia(operatorToken.TrailingTrivia.Remove(newline));

		var updatedBinaryExpression = binaryExpression
			.WithLeft(leftOperand.WithTrailingTrivia(newLeftOperandTrailingTrivia))
			.WithOperatorToken(updatedOperatorToken)
			.WithRight(rightOperand.WithLeadingTrivia(newRightOperandLeadingTrivia));

		var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
		if (root == null) { return document; }

		var newRoot = root.ReplaceNode(binaryExpression, updatedBinaryExpression);
		return document.WithSyntaxRoot(newRoot);
	}
}
