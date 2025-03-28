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

	private static async Task<Document> RemoveInvocationAsync(
		Document document,
		InvocationExpressionSyntax invocation,
		CancellationToken cancellationToken)
	{
		var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
		if (root == null) { return document; }

		var memberAccess = (MemberAccessExpressionSyntax)invocation.Expression;

		// do not use the trivia from 'invocation' because we want to keep the trivia for the original node
		var newRoot = root.ReplaceNode(invocation, memberAccess.Expression);
		return document.WithSyntaxRoot(newRoot);
	}
}
