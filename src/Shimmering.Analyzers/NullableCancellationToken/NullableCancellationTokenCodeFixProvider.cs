namespace Shimmering.Analyzers.NullableCancellationToken;

/// <summary>
/// Makes a nullable <see cref="CancellationToken"/> non-nullable if reported by <see cref="NullableCancellationTokenAnalyzer"/>.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(NullableCancellationTokenCodeFixProvider))]
internal sealed class NullableCancellationTokenCodeFixProvider : ShimmeringCodeFixProvider
{
	private static readonly string Title = $"Make {nameof(CancellationToken)} non-nullable";

	public sealed override ImmutableArray<string> FixableDiagnosticIds =>
		[DiagnosticIds.NullableCancellationToken];

	public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
	{
		var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
		if (root == null) { return; }

		var diagnostic = context.Diagnostics.First();
		var diagnosticSpan = diagnostic.Location.SourceSpan;

		var node = root.FindNode(diagnosticSpan);
		if (node is not ParameterSyntax parameter) { return; }

		context.RegisterCodeFix(
			CodeAction.Create(
				Title,
				ct => MakeNonNullableAsync(context.Document, parameter, ct),
				nameof(NullableCancellationTokenCodeFixProvider)),
			diagnostic);
	}

	private static async Task<Document> MakeNonNullableAsync(Document document, ParameterSyntax parameter, CancellationToken cancellationToken)
	{
		var nonNullableCancellationToken = SyntaxFactory.IdentifierName(nameof(CancellationToken));
		var newParameter = parameter.WithType(nonNullableCancellationToken);

		// Handle optional parameters with default value
		if (parameter.Default != null)
		{
			var newDefault = SyntaxFactory.EqualsValueClause(SyntaxFactory.ParseExpression("default"));
			newParameter = newParameter.WithDefault(newDefault);
		}

		var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
		if (root == null) { return document; }

		var newRoot = root.ReplaceNode(parameter, newParameter.WithTriviaFrom(parameter));
		return document.WithSyntaxRoot(newRoot);
	}
}
