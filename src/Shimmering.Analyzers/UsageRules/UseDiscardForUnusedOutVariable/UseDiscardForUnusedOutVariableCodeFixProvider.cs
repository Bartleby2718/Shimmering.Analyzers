namespace Shimmering.Analyzers.UsageRules.UseDiscardForUnusedOutVariable;

/// <summary>
/// Replaces an out variable with a discard, if reported by <see cref="UseDiscardForUnusedOutVariableAnalyzer"/>.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseDiscardForUnusedOutVariableCodeFixProvider))]
public sealed class UseDiscardForUnusedOutVariableCodeFixProvider : ShimmeringCodeFixProvider
{
	private const string Title = "Replace with discard";

	public sealed override ImmutableArray<string> FixableDiagnosticIds =>
		[DiagnosticIds.UsageRules.UseDiscardForUnusedOutVariable];

	public override string SampleCodeFixed => """
		using System;

		namespace Tests
		{
			class Test
			{
				void Method(string day)
				{
					if (Enum.TryParse<DayOfWeek>(day, out _))
					{
						Console.WriteLine($"{day} is a valid day of week.");
					}
				}
			}
		}
		""";

	public override async Task RegisterCodeFixesAsync(CodeFixContext context)
	{
		var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
		if (root == null) { return; }

		var diagnostic = context.Diagnostics.First();
		context.RegisterCodeFix(
			CodeAction.Create(
				Title,
				ct => ReplaceWithDiscardAsync(context.Document, diagnostic, ct),
				nameof(UseDiscardForUnusedOutVariableCodeFixProvider)),
			diagnostic);
	}

	private static async Task<Document> ReplaceWithDiscardAsync(
		Document document,
		Diagnostic diagnostic,
		CancellationToken cancellationToken)
	{
		var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
		if (root == null) { return document; }

		var diagnosticSpan = diagnostic.Location.SourceSpan;
		var identifierToken = root.FindToken(diagnosticSpan.Start);

		if (identifierToken.Parent is not SingleVariableDesignationSyntax singleVariable
			|| singleVariable.Parent is not DeclarationExpressionSyntax declarationExpression
			|| declarationExpression.Parent is not ArgumentSyntax argument)
		{
			return document;
		}

		var discardExpression = SyntaxFactory.IdentifierName("_").WithTriviaFrom(declarationExpression);
		var newArgument = argument.WithExpression(discardExpression);
		var newRoot = root.ReplaceNode(argument, newArgument);
		return document.WithSyntaxRoot(newRoot);
	}
}
