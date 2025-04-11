using Microsoft.CodeAnalysis.Formatting;

namespace Shimmering.Analyzers.StyleRules.NegatedTernaryCondition;

/// <summary>
/// Inverts a ternary expression, if reported by <see cref="NegatedTernaryConditionAnalyzer"/>.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(NegatedTernaryConditionCodeFixProvider))]
public sealed class NegatedTernaryConditionCodeFixProvider : ShimmeringCodeFixProvider
{
	private const string Title = "Invert the ternary for clarity";

	public sealed override ImmutableArray<string> FixableDiagnosticIds =>
		[DiagnosticIds.StyleRules.NegatedTernaryCondition];

	public override string SampleCodeFixed => """
		namespace Tests;
		class Test
		{
			string Do(bool condition) => condition ? "when true" : "when false";
		}
		""";

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
				ct => InvertTernary(context.Document, conditionalExpression, ct),
				nameof(NegatedTernaryConditionCodeFixProvider)),
			diagnostic);
	}

	private static async Task<Document> InvertTernary(
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
		// If the old true branch had a trailing newline but the old false branch didn't, we'd want to preserve the style.
		if (conditionalExpression.WhenFalse.GetTrailingTrivia().LastOrDefault().IsKind(SyntaxKind.None))
		{
			var lastSyntaxTriviaFromWhenFalse = conditionalExpression.WhenTrue.GetTrailingTrivia().LastOrDefault();
			if (lastSyntaxTriviaFromWhenFalse.IsKind(SyntaxKind.EndOfLineTrivia))
			{
				newWhenTrue = newWhenTrue.WithTrailingTrivia(newWhenTrue.GetTrailingTrivia().Add(lastSyntaxTriviaFromWhenFalse));
			}
		}

		var newConditionalExpression = SyntaxFactory.ConditionalExpression(
				newCondition,
				conditionalExpression.QuestionToken.WithTriviaFrom(conditionalExpression.ColonToken),
				newWhenTrue,
				conditionalExpression.ColonToken.WithTriviaFrom(conditionalExpression.QuestionToken),
				newWhenFalse)
			.WithTriviaFrom(conditionalExpression)
			.WithAdditionalAnnotations(Formatter.Annotation);

		var newRoot = root.ReplaceNode(conditionalExpression, newConditionalExpression);
		return document.WithSyntaxRoot(newRoot);
	}
}
