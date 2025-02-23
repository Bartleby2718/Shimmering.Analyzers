using Microsoft.CodeAnalysis.Formatting;

namespace Shimmering.Analyzers.NegatedTernaryCondition;

/// <summary>
/// TODO.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(NegatedTernaryConditionCodeFixProvider))]
internal sealed class NegatedTernaryConditionCodeFixProvider : ShimmeringCodeFixProvider
{
	private const string Title = "Invert the ternary for clarity";

	public sealed override ImmutableArray<string> FixableDiagnosticIds =>
		[DiagnosticIds.NegatedTernaryCondition];

	public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
	{
		var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
		if (root == null) { return; }

		var diagnostic = context.Diagnostics.First();
		var diagnosticSpan = diagnostic.Location.SourceSpan;

		var node = root.FindNode(diagnosticSpan);
		var conditionalExpression = node.DescendantNodesAndSelf()
			.OfType<ConditionalExpressionSyntax>()
			.FirstOrDefault();
		if (conditionalExpression == null) { return; }

		context.RegisterCodeFix(
			CodeAction.Create(
				Title,
				ct => DoSomethingAsync(context.Document, conditionalExpression, ct),
				nameof(NegatedTernaryConditionCodeFixProvider)),
			diagnostic);
	}

	private static async Task<Document> DoSomethingAsync(
		Document document,
		ConditionalExpressionSyntax conditionalExpression,
		CancellationToken cancellationToken)
	{
		var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
		if (root == null) { return document; }

		if (conditionalExpression.Condition is not PrefixUnaryExpressionSyntax prefixUnary
			|| prefixUnary.IsKind(SyntaxKind.LogicalNotExpression) == false)
		{
			return document;
		}

		var newCondition = prefixUnary.Operand.WithTriviaFrom(prefixUnary);

		var newWhenTrue = conditionalExpression.WhenFalse;
		var newWhenFalse = conditionalExpression.WhenTrue;

		var newConditionalExpression = SyntaxFactory.ConditionalExpression(
				newCondition,
				conditionalExpression.QuestionToken,
				newWhenTrue,
				conditionalExpression.ColonToken,
				newWhenFalse)
			.WithTriviaFrom(conditionalExpression)
			.WithAdditionalAnnotations(Formatter.Annotation);

		var newRoot = root.ReplaceNode(conditionalExpression, newConditionalExpression);
		return document.WithSyntaxRoot(newRoot);
	}
}
