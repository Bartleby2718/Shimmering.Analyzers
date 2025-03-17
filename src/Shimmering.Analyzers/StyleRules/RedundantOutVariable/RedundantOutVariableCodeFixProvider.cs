using System.Diagnostics.CodeAnalysis;

namespace Shimmering.Analyzers.StyleRules.RedundantOutVariable;

/// <summary>
/// Inlines an out variable if it's only used in an assignment, if reported by <see cref="RedundantOutVariableAnalyzer"/>.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RedundantOutVariableCodeFixProvider))]
public sealed class RedundantOutVariableCodeFixProvider : ShimmeringCodeFixProvider
{
	private const string Title = "Inline temporary variable";

	public sealed override ImmutableArray<string> FixableDiagnosticIds =>
		[DiagnosticIds.StyleRules.RedundantOutVariable];

	public override string SampleCodeFixed => """
		using System;

		namespace Tests
		{
			class Test
			{
				void Do(string dayOfWeekString)
				{
					if (Enum.TryParse<DayOfWeek>(dayOfWeekString, out DayOfWeek dayOfWeek2))
					{
					}
				}
			}
		}
		""";

	public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
	{
		var diagnostic = context.Diagnostics.First();
		var cancellationToken = context.CancellationToken;

		var root = await context.Document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
		if (root == null)
			return;

		// Locate the out argument node that triggered the diagnostic.
		var diagnosticSpan = diagnostic.Location.SourceSpan;
		if (root.FindNode(diagnosticSpan) is not ArgumentSyntax outArgument)
			return;

		// Determine the usage kind from the diagnostic property.
		if (!diagnostic.Properties.TryGetValue("isVariableDeclaration", out var value))
			return;
		var isVariableDeclaration = bool.Parse(value);

		context.RegisterCodeFix(
			CodeAction.Create(
				title: "Inline out parameter",
				ct => InlineOutParameterAsync(context.Document, outArgument, isVariableDeclaration, ct),
				equivalenceKey: "InlineOutParameter"),
			diagnostic);
	}

	private static async Task<Document> InlineOutParameterAsync(Document document, ArgumentSyntax outArgument, bool isVariableDeclaration, CancellationToken cancellationToken)
	{
		var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
		if (root == null) { return document; }

		var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
		if (semanticModel == null) { return document; }

		// Depending on the usage kind, delegate to the appropriate helper.
		return TryConstructNewRoot(isVariableDeclaration, outArgument, root, out var newRoot)
			? document.WithSyntaxRoot(newRoot)
			: document;
	}

	private static bool TryConstructNewRoot(bool isVariableDeclaration, ArgumentSyntax outArgument, SyntaxNode root, [NotNullWhen(true)] out SyntaxNode? newRoot)
	{
		newRoot = null;

		if (isVariableDeclaration && TryConstructNewArgumentFromVariableDeclaration(outArgument) is { } variableDeclarationData)
		{
			var (targetExpression, nodeToRemove) = variableDeclarationData;
			// track nodes to make sure we can both remove the next statement and replace the invocation
			var trackedRoot = root.TrackNodes(outArgument, nodeToRemove);
			var trackedNodeToRemove = trackedRoot.GetCurrentNode(nodeToRemove);
			if (trackedNodeToRemove is null) { return false; }

			var rootAfterRemoval = trackedRoot.RemoveNode(trackedNodeToRemove, SyntaxRemoveOptions.KeepNoTrivia);
			if (rootAfterRemoval is null) { return false; }

			var trackedInvocationNode = rootAfterRemoval.GetCurrentNode(outArgument);
			if (trackedInvocationNode is null) { return false; }

			var newArgument = outArgument.WithExpression(targetExpression)
				.WithTriviaFrom(outArgument);

			newRoot = rootAfterRemoval.ReplaceNode(trackedInvocationNode, newArgument);
			return true;
		}
		else if (!isVariableDeclaration && TryConstructNewArgumentFromSimpleAssignment(outArgument) is { } simpleAssignmentData)
		{
			var (targetExpression, nodeToRemove) = simpleAssignmentData;
			// track nodes to make sure we can both remove the next statement and replace the invocation
			var trackedRoot = root.TrackNodes(outArgument, nodeToRemove);
			var trackedNodeToRemove = trackedRoot.GetCurrentNode(nodeToRemove);
			if (trackedNodeToRemove is null) { return false; }

			var rootAfterRemoval = trackedRoot.RemoveNode(trackedNodeToRemove, SyntaxRemoveOptions.KeepNoTrivia);
			if (rootAfterRemoval is null) { return false; }

			var trackedInvocationNode = rootAfterRemoval.GetCurrentNode(outArgument);
			if (trackedInvocationNode is null) { return false; }

			var newArgument = outArgument.WithExpression(targetExpression)
				.WithTriviaFrom(outArgument);

			newRoot = rootAfterRemoval.ReplaceNode(trackedInvocationNode, newArgument);
			return true;
		}

		return false;
	}

	private static (ExpressionSyntax TargetExpression, StatementSyntax NodeToRemove)? TryConstructNewArgumentFromVariableDeclaration(ArgumentSyntax outArgument)
	{
		// Get the out variable's declaration node.
		if (outArgument.Expression is not DeclarationExpressionSyntax declarationExpression
			|| declarationExpression.Designation is not SingleVariableDesignationSyntax declarationNode)
		{
			return null;
		}

		var containingBlock = declarationNode.FirstAncestorOrSelf<BlockSyntax>();
		if (containingBlock == null)
			return null;

		// Find the equals-value clause that uses the declared out variable.
		var equalsClause = containingBlock.DescendantNodes()
			.OfType<EqualsValueClauseSyntax>()
			.FirstOrDefault(eq => eq.Value.DescendantNodesAndSelf()
				.OfType<IdentifierNameSyntax>()
				.Any(id => id.SpanStart > declarationNode.Span.End
					&& id.Identifier.Text == declarationNode.Identifier.Text));
		if (equalsClause == null)
			return null;

		if (equalsClause.Parent is not VariableDeclaratorSyntax declarator
			|| declarator.Parent is not VariableDeclarationSyntax declaration)
			return null;

		// The target expression is the declared variable identifier.
		ExpressionSyntax? targetExpression = outArgument.Expression is DeclarationExpressionSyntax originalDeclaration
			// Rebuild the declaration expression with the same type.
			? originalDeclaration
				.WithType(declaration.Type.WithTriviaFrom(originalDeclaration.Type))
				.WithDesignation(
					SyntaxFactory.SingleVariableDesignation(declarator.Identifier)
						.WithTriviaFrom(originalDeclaration.Designation))
			// Fallback: use just the identifier.
			: targetExpression = SyntaxFactory.IdentifierName(declarator.Identifier)
				.WithTriviaFrom(declarator);

		// Remove the entire local declaration statement.
		return declaration.Parent is StatementSyntax nodeToRemove
			? (targetExpression, nodeToRemove)
			: null;
	}

	private static (ExpressionSyntax TargetExpression, StatementSyntax NodeToRemove)? TryConstructNewArgumentFromSimpleAssignment(ArgumentSyntax outArgument)
	{
		// Get the out variable's declaration node.
		SyntaxNode? declarationNode = null;
		string? variableName = null;
		if (outArgument.Expression is DeclarationExpressionSyntax declExpr
			&& declExpr.Designation is SingleVariableDesignationSyntax designation)
		{
			declarationNode = designation;
			variableName = designation.Identifier.Text;
		}
		else if (outArgument.Expression is IdentifierNameSyntax identifierName)
		{
			declarationNode = identifierName;
			variableName = identifierName.Identifier.Text;
		}
		if (declarationNode == null || variableName == null)
			return null;

		if (declarationNode.FirstAncestorOrSelf<BlockSyntax>() is not BlockSyntax containingBlock)
			return null;

		var assignment = containingBlock.DescendantNodes()
			.OfType<AssignmentExpressionSyntax>()
			.FirstOrDefault(a => a.Right
				.DescendantNodesAndSelf()
				.OfType<IdentifierNameSyntax>()
				.Any(id => id.SpanStart > declarationNode.Span.End
					&& id.Identifier.Text == variableName));
		if (assignment == null)
			return null;

		// The target expression is the left-hand side of the assignment.
		var targetExpression = assignment.Left.WithoutTrivia();
		return assignment.Parent is StatementSyntax nodeToRemove
			? (targetExpression, nodeToRemove)
			: null;
	}
}
