namespace Shimmering.Analyzers.UsageRules.UseTrimEntries;

/// <summary>
/// TODO.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseTrimEntriesCodeFixProvider))]
public sealed class UseTrimEntriesCodeFixProvider : ShimmeringCodeFixProvider
{
	private const string Title = "Use StringSplitOptions.TrimEntries";

	public sealed override ImmutableArray<string> FixableDiagnosticIds =>
		[DiagnosticIds.UsageRules.UseTrimEntries];

	public override string SampleCodeFixed => """
		using System;
		using System.Linq;

		namespace Tests;
		class Test
		{
			void Do(string input)
			{
				var x = input.Split(',', StringSplitOptions.TrimEntries);
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
				nameof(UseTrimEntriesCodeFixProvider)),
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
		if (methodName is not (nameof(Enumerable.Select) or nameof(Enumerable.ToArray)))
			return document;

		var selectInvocation = methodName == nameof(Enumerable.ToArray)
			? (InvocationExpressionSyntax)memberAccess.Expression
			: invocation;
		if (selectInvocation.Expression is not MemberAccessExpressionSyntax selectMemberAccess
			|| selectMemberAccess.Expression is not InvocationExpressionSyntax splitInvocation)
		{
			return document;
		}

		var newArgument = SyntaxFactory.Argument(
			SyntaxFactory.MemberAccessExpression(
				SyntaxKind.SimpleMemberAccessExpression,
				SyntaxFactory.IdentifierName(nameof(StringSplitOptions)),
				SyntaxFactory.IdentifierName("TrimEntries")));

		var newArguments = splitInvocation.ArgumentList.Arguments.Add(newArgument);
		var newArgumentList = splitInvocation.ArgumentList.WithArguments(newArguments);
		var newSplitInvocation = splitInvocation.WithArgumentList(newArgumentList);

		var newRoot = root.ReplaceNode(invocation, newSplitInvocation.WithTriviaFrom(invocation));
		return document.WithSyntaxRoot(newRoot);
	}
}
