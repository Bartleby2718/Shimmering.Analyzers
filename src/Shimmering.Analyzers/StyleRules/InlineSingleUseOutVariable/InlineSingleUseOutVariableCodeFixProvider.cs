using Microsoft.CodeAnalysis.Text;

namespace Shimmering.Analyzers.StyleRules.InlineSingleUseOutVariable;

/// <summary>
/// Inlines an out variable if it's only used in an assignment, if reported by <see cref="InlineSingleUseOutVariableAnalyzer"/>.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(InlineSingleUseOutVariableCodeFixProvider))]
public sealed class InlineSingleUseOutVariableCodeFixProvider : ShimmeringCodeFixProvider
{
	private const string Title = "Inline temporary variable";

	public sealed override ImmutableArray<string> FixableDiagnosticIds =>
		[DiagnosticIds.StyleRules.InlineSingleUseOutVariable];

	public override async Task RegisterCodeFixesAsync(CodeFixContext context)
	{
		var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
		if (root == null) { return; }

		var diagnostic = context.Diagnostics.First();

		context.RegisterCodeFix(
			CodeAction.Create(
				Title,
				ct => InlineOutVariableAsync(context.Document, diagnostic, ct),
				nameof(InlineSingleUseOutVariableCodeFixProvider)),
			diagnostic);
	}

	private static async Task<Document> InlineOutVariableAsync(
		Document document,
		Diagnostic diagnostic,
		CancellationToken cancellationToken)
	{
		var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
		if (root == null) { return document; }

		// Find the identifier token for the out variable.
		var diagnosticSpan = diagnostic.Location.SourceSpan;
		var identifierToken = root.FindToken(diagnosticSpan.Start);
		var identifierNode = identifierToken.Parent;
		if (identifierNode == null)
			return document;

		// Get the declaration expression ("out var value").
		var declarationExpression = identifierNode.FirstAncestorOrSelf<DeclarationExpressionSyntax>();
		if (declarationExpression == null)
			return document;

		// Get the argument node (the out argument).
		if (declarationExpression.Parent is not ArgumentSyntax argument)
			return document;

		// Retrieve helper data from the diagnostic properties.
		var properties = diagnostic.Properties;
		if (!properties.TryGetValue("targetName", out var targetName)
			|| targetName is null
			|| !properties.TryGetValue("isDeclaration", out var isDeclarationStr)
			|| isDeclarationStr is null
			|| !properties.TryGetValue("assignmentSpanStart", out var assignmentSpanStartStr)
			|| !properties.TryGetValue("assignmentSpanLength", out var assignmentSpanLengthStr))
		{
			return document;
		}
		var isDeclaration = bool.TryParse(isDeclarationStr, out var flag) && flag;
		if (!int.TryParse(assignmentSpanStartStr, out var spanStart)
			|| !int.TryParse(assignmentSpanLengthStr, out var spanLength))
		{
			return document;
		}
		var assignmentSpan = new TextSpan(spanStart, spanLength);

		// Locate the assignment statement node using the stored span.
		var assignmentStatement = root.FindNode(assignmentSpan);
		if (assignmentStatement == null)
			return document;

		// Build the new out argument expression.
		ExpressionSyntax newArgumentExpression = isDeclaration
			? SyntaxFactory.DeclarationExpression(
				declarationExpression.Type,
				SyntaxFactory.SingleVariableDesignation(SyntaxFactory.Identifier(targetName)))
			: SyntaxFactory.IdentifierName(targetName);

		var invocation = argument.FirstAncestorOrSelf<InvocationExpressionSyntax>();
		if (invocation == null)
			return document;

		var newArgument = argument.WithExpression(newArgumentExpression);
		var newInvocation = invocation.ReplaceNode(argument, newArgument);

		// track nodes to make sure we can both remove the next statement and replace the invocation
		var trackedRoot = root.TrackNodes(assignmentStatement, invocation);
		var trackedNextStatementNode = trackedRoot.GetCurrentNode(assignmentStatement);
		if (trackedNextStatementNode is null) { return document; }

		var rootAfterRemoval = trackedRoot.RemoveNode(trackedNextStatementNode, SyntaxRemoveOptions.KeepNoTrivia);
		if (rootAfterRemoval is null) { return document; }

		var trackedInvocationNode = rootAfterRemoval.GetCurrentNode(invocation);
		if (trackedInvocationNode is null) { return document; }

		var newRoot = rootAfterRemoval.ReplaceNode(trackedInvocationNode, newInvocation);
		return document.WithSyntaxRoot(newRoot);
	}
}
