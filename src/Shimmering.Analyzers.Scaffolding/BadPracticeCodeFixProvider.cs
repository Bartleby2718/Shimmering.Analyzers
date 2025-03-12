namespace Shimmering.Analyzers.CATEGORY_PLACEHOLDERRules.BadPractice;

/// <summary>
/// TODO.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(BadPracticeCodeFixProvider))]
internal sealed class BadPracticeCodeFixProvider : ShimmeringCodeFixProvider
{
	private const string Title = "TODO:";

	public sealed override ImmutableArray<string> FixableDiagnosticIds =>
		[DiagnosticIds.CATEGORY_PLACEHOLDERRules.BadPractice];

	public override async Task RegisterCodeFixesAsync(CodeFixContext context)
	{
		var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
		if (root == null) { return; }

		var diagnostic = context.Diagnostics.First();
		var diagnosticSpan = diagnostic.Location.SourceSpan;

		// TODO:
		var node = root.FindNode(diagnosticSpan);
		var someExpression = node.DescendantNodesAndSelf()
			.OfType<BinaryExpressionSyntax>() // TODO: replace
			.FirstOrDefault();
		if (someExpression == null) { return; }

		context.RegisterCodeFix(
			CodeAction.Create(
				Title,
				ct => DoSomethingAsync(context.Document, someExpression, ct),
				nameof(BadPracticeCodeFixProvider)),
			diagnostic);
	}

	private static async Task<Document> DoSomethingAsync(
		Document document,
		BinaryExpressionSyntax someExpression, // TODO: replace
		CancellationToken cancellationToken)
	{
		var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
		if (root == null) { return document; }

		SyntaxNode newExpression = default!; // TODO:
		var newRoot = root.ReplaceNode(someExpression, newExpression);
		return document.WithSyntaxRoot(newRoot);
	}
}
