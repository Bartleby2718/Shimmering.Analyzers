namespace Shimmering.Analyzers.UsageRules.UniqueNonSetCollection;

/// <summary>
/// Converts .Distinct().ToList() or .Distinct().ToArray() with .ToHashSet() if reported by <see cref="UniqueNonSetCollectionAnalyzer"/>.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UniqueNonSetCollectionCodeFixProvider))]
public sealed class UniqueNonSetCollectionCodeFixProvider : ShimmeringCodeFixProvider
{
	private static readonly string Title = "Replace .Distinct().ToList() or .Distinct().ToArray() with .ToHashSet()";

	public sealed override ImmutableArray<string> FixableDiagnosticIds =>
		[DiagnosticIds.UsageRules.UniqueNonSetCollection];

	public override string SampleCodeFixed => """
		using System;
		using System.Collections.Generic;
		using System.Linq;

		namespace Tests
		{
			class Test
			{
				IReadOnlyCollection<int> Do()
				{
					List<int> numbers = [];
					return numbers.ToHashSet();
				}
			}
		}
		""";

	public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
	{
		var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
		if (root == null) { return; }

		var diagnostic = context.Diagnostics.First();
		var diagnosticSpan = diagnostic.Location.SourceSpan;

		var node = root.FindNode(diagnosticSpan);
		var invocation = node
			.DescendantNodesAndSelf()
			.OfType<InvocationExpressionSyntax>()
			.FirstOrDefault();
		if (invocation == null) { return; }

		context.RegisterCodeFix(
			CodeAction.Create(
				Title,
				ct => ReplaceWithToHashSetAsync(context.Document, invocation, ct),
				nameof(UniqueNonSetCollectionCodeFixProvider)),
			diagnostic);
	}

	private static async Task<Document> ReplaceWithToHashSetAsync(Document document, InvocationExpressionSyntax invocation, CancellationToken cancellationToken)
	{
		var toSomething = (MemberAccessExpressionSyntax)invocation.Expression;
		var distinctCall = (InvocationExpressionSyntax)toSomething.Expression;
		var distinctNode = (MemberAccessExpressionSyntax)distinctCall.Expression;

		/*
			Dealing with trivia is tricky here because these are typically on different lines, like this:

			IEnumerable<int> _field = new[] { 1, 2 }
				// a
				.Distinct() // b
				// c
				.ToArray(); // d

			Most of the code below is about getting a, b, c, and d right.
		*/

		var leadingTabs = toSomething.OperatorToken.LeadingTrivia
			.Where(trivia => trivia.IsKind(SyntaxKind.WhitespaceTrivia))
			.ToSyntaxTriviaList();

		// corresponds to the trivia containing '// a' in the previous block comment
		var newOperatorTokenLeadingTrivia = distinctNode.OperatorToken.LeadingTrivia;
		// corresponds to the trivia containing '// b' in the previous block comment
		if (distinctCall.HasTrailingTrivia)
		{
			var trailingTrivia = distinctCall.GetTrailingTrivia();
			// add leading tabs unless the trailing trivia only consists of a single newline
			if (trailingTrivia.Count != 1 || !trailingTrivia[0].IsKind(SyntaxKind.EndOfLineTrivia))
			{
				newOperatorTokenLeadingTrivia = newOperatorTokenLeadingTrivia
					.AddRange(leadingTabs)
					.AddRange(trailingTrivia);
			}
		}

		// corresponds to the trivia containing '// c' in the previous block comment
		if (toSomething.OperatorToken.HasLeadingTrivia
			// however, this is duplicative if it only contains whitespaces
			&& toSomething.OperatorToken.LeadingTrivia.Any(t => !t.IsKind(SyntaxKind.WhitespaceTrivia)))
		{
			newOperatorTokenLeadingTrivia = newOperatorTokenLeadingTrivia
				.AddRange(toSomething.OperatorToken.LeadingTrivia);
		}

		var operatorToken = SyntaxFactory.Token(SyntaxKind.DotToken)
			.WithLeadingTrivia(newOperatorTokenLeadingTrivia);
		var name = SyntaxFactory.IdentifierName("ToHashSet");

		var toHashSetMemberAccess = SyntaxFactory.MemberAccessExpression(
			SyntaxKind.SimpleMemberAccessExpression,
			distinctNode.Expression,
			operatorToken,
			name);

		var toHashSetInvocation = SyntaxFactory.InvocationExpression(toHashSetMemberAccess)
			.WithTrailingTrivia(invocation.GetTrailingTrivia());

		var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
		if (root == null) { return document; }

		var newRoot = root.ReplaceNode(invocation, toHashSetInvocation);
		var newDocument = document.WithSyntaxRoot(newRoot);
		return newDocument;
	}
}
