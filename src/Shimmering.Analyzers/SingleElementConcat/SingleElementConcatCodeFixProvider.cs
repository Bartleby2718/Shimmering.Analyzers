using Shimmering.Analyzers.Utilities;

namespace Shimmering.Analyzers.SingleElementConcat;

/// <summary>
/// Converts an <see cref="Enumerable.Concat"/> to an <see cref="Enumerable.Append"/> if reported by <see cref="SingleElementConcatAnalyzer"/>.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(SingleElementConcatCodeFixProvider)), Shared]
internal sealed class SingleElementConcatCodeFixProvider : CodeFixProvider
{
	private const string Title = "Replace .Concat([e]) with .Append(e)";

	public sealed override ImmutableArray<string> FixableDiagnosticIds =>
		[DiagnosticIds.SingleElementConcat];

	public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

	public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
	{
		var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
		if (root == null) { return; }

		var diagnostic = context.Diagnostics.First();
		var diagnosticSpan = diagnostic.Location.SourceSpan;

		var node = root.FindNode(diagnosticSpan);
		var invocation = node
			.DescendantNodesAndSelf()
			.OfType<InvocationExpressionSyntax>()
			.FirstOrDefault();
		if (invocation == null) { return; }

		context.RegisterCodeFix(
			CodeAction.Create(
				Title,
				ct => ReplaceInvocationWithCollectionExpressionAsync(context.Document, invocation, ct),
				nameof(SingleElementConcatCodeFixProvider)),
			diagnostic);
	}

	private static async Task<Document> ReplaceInvocationWithCollectionExpressionAsync(
		Document document, InvocationExpressionSyntax invocation, CancellationToken cancellationToken)
	{
		var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
		if (semanticModel == null) { return document; }

		var supportsCollectionExpressions = document.Project.ParseOptions is CSharpParseOptions { LanguageVersion: >= LanguageVersion.CSharp12 };
		if (!SingleElementConcatHelpers.TryGetSingleElement(invocation, supportsCollectionExpressions, out var expression)) { return document; }

		var memberAccess = (MemberAccessExpressionSyntax)invocation.Expression;
		var newMemberAccess = memberAccess.WithName(SyntaxFactory.IdentifierName(nameof(Enumerable.Append)));

		var newArgumentList = SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList([SyntaxFactory.Argument(expression)]));
		var newNode = invocation.WithExpression(newMemberAccess)
			.WithArgumentList(newArgumentList);

		var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
		if (root == null) { return document; }

		var newRoot = root.ReplaceNode(invocation, newNode);
		return document.WithSyntaxRoot(newRoot);
	}
}
