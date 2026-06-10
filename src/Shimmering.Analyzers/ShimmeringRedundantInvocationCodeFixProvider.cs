namespace Shimmering.Analyzers;

/// <summary>
/// A <see cref="ShimmeringCodeFixProvider"/> that simply removes an <see cref="InvocationExpressionSyntax"/> from a <see cref="SyntaxNode"/>.
/// </summary>
public abstract class ShimmeringRedundantInvocationCodeFixProvider : ShimmeringCodeFixProvider
{
	public abstract string CodeFixTitle { get; }

	public abstract string CodeFixEquivalenceKey { get; }

	public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
	{
		var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
		if (root == null) { return; }

		var diagnostic = context.Diagnostics.First();
		var diagnosticSpan = diagnostic.Location.SourceSpan;

		var node = root.FindNode(diagnosticSpan);
		var invocation = node.DescendantNodesAndSelf()
			.OfType<InvocationExpressionSyntax>()
			.FirstOrDefault();
		if (invocation == null) { return; }

		context.RegisterCodeFix(
			CodeAction.Create(
				this.CodeFixTitle,
				ct => RemoveInvocationAsync(context.Document, invocation, ct),
				this.CodeFixEquivalenceKey),
			diagnostic);
	}

	// This isn't yet perfect, but make best effort trying to update trivia in a reasonable manner
	private static async Task<Document> RemoveInvocationAsync(
		Document document,
		InvocationExpressionSyntax invocation,
		CancellationToken cancellationToken)
	{
		var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
		if (root == null) { return document; }

		var memberAccess = (MemberAccessExpressionSyntax)invocation.Expression;
		var innerNode = memberAccess.Expression;
		var innerNodeTrailingTrivia = innerNode.GetTrailingTrivia();
		var invocationTrailingTrivia = invocation.GetTrailingTrivia();

		SyntaxTriviaList replacementTrailingTrivia;
		if (innerNodeTrailingTrivia.All(t => t.IsKind(SyntaxKind.WhitespaceTrivia) || t.IsKind(SyntaxKind.EndOfLineTrivia)))
		{
			replacementTrailingTrivia = invocationTrailingTrivia;
		}
		else
		{
			replacementTrailingTrivia = innerNodeTrailingTrivia.AddRange(invocationTrailingTrivia);
		}

		var replacementNode = innerNode
			.WithLeadingTrivia(invocation.GetLeadingTrivia())
			.WithTrailingTrivia(replacementTrailingTrivia);

		return document.WithSyntaxRoot(root.ReplaceNode(invocation, replacementNode));
	}
}
