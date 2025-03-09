using System.Diagnostics.CodeAnalysis;

namespace Shimmering.Analyzers.UsageRules.ToListForEach;

/// <summary>
/// Replaces a .ToList().ForEach() with a foreach loop without materialization, if reported by <see cref="ToListForEachAnalyzer"/>.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ToListForEachCodeFixProvider))]
internal sealed class ToListForEachCodeFixProvider : ShimmeringCodeFixProvider
{
	private const string Title = "Replace with a foreach loop";

	public sealed override ImmutableArray<string> FixableDiagnosticIds =>
		[DiagnosticIds.UsageRules.ToListForEach];

	public override async Task RegisterCodeFixesAsync(CodeFixContext context)
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
				Title,
				ct => DoSomethingAsync(context.Document, invocation, ct),
				nameof(ToListForEachCodeFixProvider)),
			diagnostic);
	}

	private static async Task<Document> DoSomethingAsync(
		Document document,
		InvocationExpressionSyntax invocation,
		CancellationToken cancellationToken)
	{
		var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
		if (root == null) { return document; }

		var forEachMemberAccess = (MemberAccessExpressionSyntax)invocation.Expression;

		// Get the argument which can be a lambda or a method group.
		var argumentExpression = invocation.ArgumentList.Arguments.First().Expression;

		var loopVariableName = GetLoopVariableName(argumentExpression);
		if (!TryBuildLoopBody(argumentExpression, loopVariableName, out var loopBody))
		{
			return document;
		}

		// Get the original enumerable from the ToList() call.
		var toListInvocation = (InvocationExpressionSyntax)forEachMemberAccess.Expression;
		var toListMemberAccess = (MemberAccessExpressionSyntax)toListInvocation.Expression;
		// remove trivia, as you probably wouldn't want trivia in the middle of foreach
		var enumerableExpression = toListMemberAccess.Expression.WithoutTrivia();

		var invocationStatement = invocation.FirstAncestorOrSelf<ExpressionStatementSyntax>()!;
		var foreachTrailingTrivia = invocation.GetTrailingTrivia().Concat(invocationStatement.GetTrailingTrivia());

		// Build the foreach statement.
		var foreachStatement = SyntaxFactory.ForEachStatement(
			SyntaxFactory.Token(SyntaxKind.ForEachKeyword),
			SyntaxFactory.Token(SyntaxKind.OpenParenToken),
			SyntaxFactory.IdentifierName("var"),
			SyntaxFactory.Identifier(loopVariableName),
			SyntaxFactory.Token(SyntaxKind.InKeyword),
			enumerableExpression,
			SyntaxFactory.Token(SyntaxKind.CloseParenToken),
			loopBody)
			.WithLeadingTrivia(invocation.GetLeadingTrivia())
			.WithTrailingTrivia(foreachTrailingTrivia);

		var newRoot = root.ReplaceNode(invocationStatement, foreachStatement);
		return document.WithSyntaxRoot(newRoot);
	}

	private static string GetLoopVariableName(ExpressionSyntax argumentExpression)
	{
		if (argumentExpression is SimpleLambdaExpressionSyntax simpleLambda)
		{
			return simpleLambda.Parameter.Identifier.Text;
		}

		if (argumentExpression is ParenthesizedLambdaExpressionSyntax parenthesizedLambda
			&& parenthesizedLambda.ParameterList.Parameters.Count == 1)
		{
			return parenthesizedLambda.ParameterList.Parameters[0].Identifier.Text;
		}

		return "item";
	}

	private static bool TryBuildLoopBody(
		ExpressionSyntax argumentExpression,
		string loopVariableName,
		[NotNullWhen(true)] out BlockSyntax? loopBody)
	{
		if (argumentExpression is LambdaExpressionSyntax lambda)
		{
			loopBody = lambda.Body is BlockSyntax block
				? block
				: lambda.Body is ExpressionSyntax expression
					? SyntaxFactory.Block(SyntaxFactory.ExpressionStatement(expression))
					: SyntaxFactory.Block();
			return true;
		}

		if (argumentExpression is IdentifierNameSyntax or MemberAccessExpressionSyntax)
		{
			// Convert single-parameter static method group to a call: Method(loopVariableName);
			var newInvocation = SyntaxFactory.InvocationExpression(
				argumentExpression.WithoutTrivia(),
				SyntaxFactory.ArgumentList(
					SyntaxFactory.SingletonSeparatedList(
						SyntaxFactory.Argument(SyntaxFactory.IdentifierName(loopVariableName)))))
				.WithTriviaFrom(argumentExpression);
			loopBody = SyntaxFactory.Block(SyntaxFactory.ExpressionStatement(newInvocation));
			return true;
		}

		loopBody = null;
		return false;
	}
}
