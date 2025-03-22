namespace Shimmering.Analyzers.UsageRules.MissingRemoveEmptyEntries;

/// <summary>
/// Replaces string.Split() with the overload that has <see cref="StringSplitOptions.RemoveEmptyEntries"/> as the second argument, if reported by <see cref="MissingRemoveEmptyEntriesAnalyzer"/>.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(MissingRemoveEmptyEntriesCodeFixProvider))]
public sealed class MissingRemoveEmptyEntriesCodeFixProvider : ShimmeringCodeFixProvider
{
	private const string Title = "Simplify string.Split()";

	public sealed override ImmutableArray<string> FixableDiagnosticIds =>
		[DiagnosticIds.UsageRules.MissingRemoveEmptyEntries];

	public override string SampleCodeFixed => """
		using System;
		using System.Linq;

		namespace Tests;
		class Test
		{
			void Do(string input)
			{
				var x = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
			}
		}
		""";

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
				ct => UseStringSplitOptionsAsync(context.Document, invocation, ct),
				nameof(MissingRemoveEmptyEntriesCodeFixProvider)),
			diagnostic);
	}

	private static async Task<Document> UseStringSplitOptionsAsync(
		Document document,
		InvocationExpressionSyntax invocation,
		CancellationToken cancellationToken)
	{
		var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
		if (root == null) { return document; }

		if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
			return document;
		var methodName = memberAccess.Name.Identifier.Text;
		if (methodName is not (nameof(Enumerable.Where) or nameof(Enumerable.ToArray)))
			return document;

		var whereInvocation = methodName == nameof(Enumerable.ToArray)
			? (InvocationExpressionSyntax)memberAccess.Expression
			: invocation;
		if (whereInvocation.Expression is not MemberAccessExpressionSyntax whereMemberAccess
			|| whereMemberAccess.Expression is not InvocationExpressionSyntax splitInvocation)
		{
			return document;
		}

		var newArgument = SyntaxFactory.Argument(
			SyntaxFactory.MemberAccessExpression(
				SyntaxKind.SimpleMemberAccessExpression,
				SyntaxFactory.IdentifierName(nameof(StringSplitOptions)),
				SyntaxFactory.IdentifierName(nameof(StringSplitOptions.RemoveEmptyEntries))));

		var newArguments = splitInvocation.ArgumentList.Arguments.Add(newArgument);
		var newArgumentList = splitInvocation.ArgumentList.WithArguments(newArguments);
		var newSplitInvocation = splitInvocation.WithArgumentList(newArgumentList);

		var newRoot = root.ReplaceNode(invocation, newSplitInvocation.WithTriviaFrom(invocation));
		return document.WithSyntaxRoot(newRoot);
	}
}
