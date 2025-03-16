namespace Shimmering.Analyzers.UsageRules.ArrayOrArrayReturningMethodFollowedByToArray;

/// <summary>
/// Removes .ToArray(), if reported by <see cref="ArrayOrArrayReturningMethodFollowedByToArrayAnalyzer"/>.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ArrayOrArrayReturningMethodFollowedByToArrayCodeFixProvider))]
public sealed class ArrayOrArrayReturningMethodFollowedByToArrayCodeFixProvider : ShimmeringCodeFixProvider
{
	private const string Title = "Remove redundant .ToArray()";

	public sealed override ImmutableArray<string> FixableDiagnosticIds =>
		[DiagnosticIds.UsageRules.ArrayOrArrayReturningMethodFollowedByToArray];

	public override string SampleCodeFixed => """
		using System.Linq;

		namespace Tests
		{
			class Test
			{
				void Do()
				{
					var array = "a".Split(' ');
				}
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
		var someExpression = node.DescendantNodesAndSelf()
			.OfType<InvocationExpressionSyntax>()
			.FirstOrDefault();
		if (someExpression == null) { return; }

		context.RegisterCodeFix(
			CodeAction.Create(
				Title,
				ct => RemoveToArrayAsync(context.Document, someExpression, ct),
				nameof(ArrayOrArrayReturningMethodFollowedByToArrayCodeFixProvider)),
			diagnostic);
	}

	private static async Task<Document> RemoveToArrayAsync(
		Document document,
		InvocationExpressionSyntax invocation,
		CancellationToken cancellationToken)
	{
		var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
		if (root == null) { return document; }

		var memberAccess = (MemberAccessExpressionSyntax)invocation.Expression;

		// do not use the trivia from 'invocation' because we want to keep the trivia for the original node
		var newRoot = root.ReplaceNode(invocation, memberAccess.Expression);
		return document.WithSyntaxRoot(newRoot);
	}
}
