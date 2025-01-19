namespace Shimmering.Analyzers.RedundantSpreadElement;

/// <summary>
/// Flattens a spread element (e.g. [1, .. new[] { 2, 3 }, 4] to [1, 2, 3, 4]) if reported by <see cref="RedundantSpreadElementAnalyzer"/>.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RedundantSpreadElementCodeFixProvider)), Shared]
internal sealed class RedundantSpreadElementCodeFixProvider : CodeFixProvider
{
	private static readonly string Title = "Replace Enumerate() with collection expression";

	public sealed override ImmutableArray<string> FixableDiagnosticIds =>
		[DiagnosticIds.RedundantSpreadElement];

	public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

	public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
	{
		var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
		if (root == null) { return; }

		var diagnostic = context.Diagnostics.First();
		var diagnosticSpan = diagnostic.Location.SourceSpan;

		var node = root.FindNode(diagnosticSpan);
		var spreadElement = node
			.DescendantNodesAndSelf()
			.OfType<SpreadElementSyntax>()
			.FirstOrDefault();
		if (spreadElement is null) { return; }

		context.RegisterCodeFix(
			CodeAction.Create(
				Title,
				ct => ReplaceSpreadElementWithCollectionElementsAsync(context.Document, spreadElement, ct),
				nameof(RedundantSpreadElementCodeFixProvider)),
			diagnostic);
	}

	private static async Task<Document> ReplaceSpreadElementWithCollectionElementsAsync(
		Document document, SpreadElementSyntax spreadElement, CancellationToken cancellationToken)
	{
		var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
		if (root != null
			&& RedundantSpreadElementHelpers.TryGetInnerElementsOfNonemptySpreadElement(spreadElement, out var innerElements))
		{
			var newRoot = root.ReplaceNode(spreadElement, innerElements);
			return document.WithSyntaxRoot(newRoot);
		}

		return document;
	}
}
