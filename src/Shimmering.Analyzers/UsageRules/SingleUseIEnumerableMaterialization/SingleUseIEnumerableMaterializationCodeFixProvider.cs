namespace Shimmering.Analyzers.UsageRules.SingleUseIEnumerableMaterialization;

/// <summary>
/// Removes an unnecessary materialization of an IEnumerable if reported by <see cref="SingleUseIEnumerableMaterializationAnalyzer"/>.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(SingleUseIEnumerableMaterializationCodeFixProvider))]
public sealed class SingleUseIEnumerableMaterializationCodeFixProvider : ShimmeringCodeFixProvider
{
	private static readonly string Title = "Remove unnecessary materialization";

	public sealed override ImmutableArray<string> FixableDiagnosticIds =>
		[DiagnosticIds.UsageRules.SingleUseIEnumerableMaterialization];

	public override string SampleCodeFixed => """
		using System;
		using System.Collections.Generic;
		using System.Linq;

		namespace Tests;
		class Test
		{
			void Do()
			{
				List<int> numbers = [];
				var oddNumbers = numbers.Where(n => n % 2 == 1);
				foreach (var oddNumber in oddNumbers)
				{
					Console.WriteLine(oddNumber);
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
		if (node is not InvocationExpressionSyntax invocation) { return; }

		context.RegisterCodeFix(
			CodeAction.Create(
				Title,
				ct => RemoveMaterializationAsync(context.Document, invocation, ct),
				nameof(SingleUseIEnumerableMaterializationCodeFixProvider)),
			diagnostic);
	}

	private static async Task<Document> RemoveMaterializationAsync(Document document, InvocationExpressionSyntax invocation, CancellationToken cancellationToken)
	{
		var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
		if (root == null) { return document; }

		var memberAccess = (MemberAccessExpressionSyntax)invocation.Expression;
		var newRoot = root.ReplaceNode(
			invocation,
			memberAccess.Expression.WithTrailingTrivia(invocation.GetTrailingTrivia()));
		return document.WithSyntaxRoot(newRoot);
	}
}
