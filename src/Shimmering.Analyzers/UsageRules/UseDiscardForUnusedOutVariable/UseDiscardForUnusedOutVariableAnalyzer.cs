namespace Shimmering.Analyzers.UsageRules.UseDiscardForUnusedOutVariable;

/// <summary>
/// Reports instances of an out variable that's not used anywhere.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UseDiscardForUnusedOutVariableAnalyzer : ShimmeringSyntaxNodeAnalyzer
{
	private const string Title = "Use discard for unused out variable";
	private const string Message = "Unused out variable '{0}' can be replaced with discard '_'";
	private const string Category = "Usage";

	private static readonly DiagnosticDescriptor Rule = CreateRule(
		DiagnosticIds.UsageRules.UseDiscardForUnusedOutVariable,
		Title,
		Message,
		Category,
		DiagnosticSeverity.Warning,
		isEnabledByDefault: true);

	public override string SampleCode => """
		using System;

		namespace Tests
		{
			class Test
			{
				void Method(string day)
				{
					if (Enum.TryParse<DayOfWeek>(day, out DayOfWeek dayOfWeek))
					{
						Console.WriteLine($"{day} is a valid day of week.");
					}
				}
			}
		}
		""";

	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

	public override void RegisterSyntaxNodeAction(AnalysisContext context)
	{
		context.RegisterSyntaxNodeAction(AnalyzeDeclarationExpression, SyntaxKind.DeclarationExpression);
	}

	private static void AnalyzeDeclarationExpression(SyntaxNodeAnalysisContext context)
	{
		var declarationExpression = (DeclarationExpressionSyntax)context.Node;

		// the declaration should be part of an out argument
		if (declarationExpression.Parent is not ArgumentSyntax argument)
			return;
		if (!argument.RefOrOutKeyword.IsKind(SyntaxKind.OutKeyword))
			return;

		// only consider single variable designations
		if (declarationExpression.Designation is not SingleVariableDesignationSyntax singleVariable)
			return;

		// get the symbol for the declared variable
		var variableSymbol = context.SemanticModel.GetDeclaredSymbol(singleVariable, context.CancellationToken);
		if (variableSymbol == null)
			return;

		// locate the containing containingBlock.
		var containingBlock = declarationExpression.FirstAncestorOrSelf<BlockSyntax>();
		if (containingBlock == null)
			return;

		if (!IsUsedElsewhere(variableSymbol, singleVariable, containingBlock, context.SemanticModel, context.CancellationToken))
		{
			var diagnostic = Diagnostic.Create(Rule, singleVariable.Identifier.GetLocation(), singleVariable.Identifier.Text);
			context.ReportDiagnostic(diagnostic);
		}
	}

	private static bool IsUsedElsewhere(
		ISymbol variableSymbol,
		SingleVariableDesignationSyntax declaration,
		BlockSyntax containingBlock,
		SemanticModel semanticModel,
		CancellationToken cancellationToken)
	{
		// Iterate over all identifier names in the containing containingBlock.
		var identifiers = containingBlock.DescendantNodes().OfType<IdentifierNameSyntax>();
		foreach (var identifier in identifiers)
		{
			// Only consider identifiers that match the variable's name.
			if (identifier.Identifier.ValueText != variableSymbol.Name)
				continue;

			// Ensure the symbol really is the same.
			var symbol = semanticModel.GetSymbolInfo(identifier, cancellationToken).Symbol;
			if (!SymbolEqualityComparer.Default.Equals(variableSymbol, symbol))
				continue;

			// Skip the original declaration identifier.
			if (identifier.Span.Equals(declaration.Identifier.Span))
				continue;

			// Any other occurrence means the variable is used.
			return true;
		}

		return false;
	}
}
