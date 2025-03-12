using Microsoft.CodeAnalysis.Formatting;
using Shimmering.Analyzers.Utilities;

namespace Shimmering.Analyzers.UsageRules.MisusedOrDefault;

/// <summary>
/// Replaces an 'OrDefault' LINQ method with its non-OrDefault counterpart, if reported by <see cref="MisusedOrDefaultAnalyzer"/>.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(MisusedOrDefaultCodeFixProvider))]
internal sealed class MisusedOrDefaultCodeFixProvider : ShimmeringCodeFixProvider
{
	private const string Title = "Simplify 'OrDefault' method call";

	public sealed override ImmutableArray<string> FixableDiagnosticIds =>
		[DiagnosticIds.UsageRules.MisusedOrDefault];

	public override async Task RegisterCodeFixesAsync(CodeFixContext context)
	{
		var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
		if (root == null) { return; }

		var diagnostic = context.Diagnostics.First();
		var diagnosticSpan = diagnostic.Location.SourceSpan;

		var node = root.FindNode(diagnosticSpan);
		var someExpression = node.DescendantNodesAndSelf()
			.OfType<PostfixUnaryExpressionSyntax>()
			.FirstOrDefault();
		if (someExpression == null) { return; }

		context.RegisterCodeFix(
			CodeAction.Create(
				Title,
				ct => ReplaceOrDefaultAsync(context.Document, someExpression, ct),
				nameof(MisusedOrDefaultCodeFixProvider)),
			diagnostic);
	}

	private static async Task<Document> ReplaceOrDefaultAsync(
		Document document,
		PostfixUnaryExpressionSyntax suppressNode,
		CancellationToken cancellationToken)
	{
		var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
		if (root == null) { return document; }

		var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
		if (semanticModel == null) { return document; }

		var invocation = (InvocationExpressionSyntax)suppressNode.Operand;
		var memberAccess = (MemberAccessExpressionSyntax)invocation.Expression;
		if (!EnumerableHelpers.IsLinqExtensionMethodCall(semanticModel, invocation, out var oldMethodName)) { return document; }

		var newMethodName = MisusedOrDefaultHelpers.MethodMapping[oldMethodName];
		var newIdentifier = SyntaxFactory.IdentifierName(newMethodName);
		var newMemberAccess = memberAccess.WithName(newIdentifier)
			.WithTrailingTrivia(memberAccess.GetTrailingTrivia());
		var newInvocation = invocation.WithExpression(newMemberAccess)
			.WithLeadingTrivia(invocation.GetLeadingTrivia())
			.WithTrailingTrivia(invocation.GetTrailingTrivia().Concat(suppressNode.GetTrailingTrivia()));

		var newRoot = root.ReplaceNode(suppressNode, newInvocation);
		return document.WithSyntaxRoot(newRoot);
	}
}
