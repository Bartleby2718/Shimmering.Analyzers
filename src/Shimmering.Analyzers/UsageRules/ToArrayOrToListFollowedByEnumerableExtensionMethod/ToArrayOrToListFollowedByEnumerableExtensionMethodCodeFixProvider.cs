namespace Shimmering.Analyzers.UsageRules.ToArrayOrToListFollowedByEnumerableExtensionMethod;

/// <summary>
/// Replaces an unnecessary materialization, if reported by <see cref="ToArrayOrToListFollowedByEnumerableExtensionMethodAnalyzer"/>.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ToArrayOrToListFollowedByEnumerableExtensionMethodCodeFixProvider))]
internal sealed class ToArrayOrToListFollowedByEnumerableExtensionMethodCodeFixProvider : ShimmeringCodeFixProvider
{
	private const string Title = "Remove unnecessary materialization";

	public sealed override ImmutableArray<string> FixableDiagnosticIds =>
		[DiagnosticIds.UsageRules.ToArrayOrToListFollowedByEnumerableExtensionMethod];

	public override async Task RegisterCodeFixesAsync(CodeFixContext context)
	{
		var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
		if (root == null) { return; }

		var diagnostic = context.Diagnostics.First();
		context.RegisterCodeFix(
			CodeAction.Create(
				Title,
				ct => RemoveMaterializationAsync(context.Document, diagnostic, ct),
				nameof(ToArrayOrToListFollowedByEnumerableExtensionMethodCodeFixProvider)),
			diagnostic);
	}

	private static async Task<Document> RemoveMaterializationAsync(Document document, Diagnostic diagnostic, CancellationToken cancellationToken)
	{
		var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
		if (root == null) { return document; }

		// locate the method name in the materialization call
		var diagnosticSpan = diagnostic.Location.SourceSpan;
		var token = root.FindToken(diagnosticSpan.Start);
		if (token.Parent is not SimpleNameSyntax materializationName
			|| materializationName.Parent is not MemberAccessExpressionSyntax memberAccess
			|| memberAccess.Parent is not InvocationExpressionSyntax invocation)
		{
			return document;
		}

		// We want to remove the materialization call by replacing it with its receiver ("source")
		var newExpression = memberAccess.Expression;

		// do not use the trivia from invocation, as it's likely meant for the materialization call
		var newRoot = root.ReplaceNode(invocation, newExpression);
		return document.WithSyntaxRoot(newRoot);
	}
}
