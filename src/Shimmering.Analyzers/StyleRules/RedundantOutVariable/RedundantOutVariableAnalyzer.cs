using System.Diagnostics.CodeAnalysis;

namespace Shimmering.Analyzers.StyleRules.RedundantOutVariable;

/// <summary>
/// Reports instances of an out variable that's only assigned to a variable and not used otherwise.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class RedundantOutVariableAnalyzer : ShimmeringSyntaxNodeAnalyzer
{
	private const string Title = "Redundant out variable";
	private const string Message = "Out variable '{0}' is used exactly once and for assignment and therefore can be inlined";
	private const string Category = "Style";

	private static readonly DiagnosticDescriptor Rule = CreateRule(
		DiagnosticIds.StyleRules.RedundantOutVariable,
		Title,
		Message,
		Category,
		DiagnosticSeverity.Info);

	public override string SampleCode => """
		using System;

		namespace Tests;
		class Test
		{
			void Do(string dayOfWeekString)
			{
				if (Enum.TryParse(dayOfWeekString, [|out DayOfWeek dayOfWeek1|]))
				{
					DayOfWeek dayOfWeek2 = dayOfWeek1;
				}
			}
		}
		""";

	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

	public override void RegisterSyntaxNodeAction(AnalysisContext context)
	{
		context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
	}

	private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
	{
		var invocation = (InvocationExpressionSyntax)context.Node;
		if (invocation.ArgumentList == null)
			return;

		// Process each argument that uses the 'out' keyword.
		foreach (var argument in invocation.ArgumentList.Arguments)
		{
			if (!argument.RefOrOutKeyword.IsKind(SyntaxKind.OutKeyword))
				continue;

			// Obtain the symbol for the out variable from either a declaration or an identifier.
			if (!TryParseOutArgument(argument, context.SemanticModel, context.CancellationToken, out var outVariableSymbol, out var declarationNode))
			{
				continue;
			}

			// Find the nearest enclosing block.
			if (declarationNode.FirstAncestorOrSelf<BlockSyntax>() is not BlockSyntax containingBlock)
				continue;

			// Only consider usages within the same executable scope.
			var references = containingBlock.DescendantNodes()
				.OfType<IdentifierNameSyntax>()
				.Where(id =>
				{
					var symbol = context.SemanticModel.GetSymbolInfo(id, context.CancellationToken).Symbol;
					if (!SymbolEqualityComparer.Default.Equals(symbol, outVariableSymbol))
						return false;
					if (declarationNode.Span.Contains(id.SpanStart))
						return false;
					return InSameLocalScope(declarationNode, id);
				})
				.ToList();

			// Require exactly one usage.
			if (references.Count != 1)
				continue;

			var singleUsage = references.Single();
			var parent = singleUsage.Parent;
			if (parent is AssignmentExpressionSyntax assignment)
			{
				if (!assignment.Right.Contains(singleUsage))
					continue;
				if (assignment.Parent is not ExpressionStatementSyntax)
					continue;
				// Verify that the left-hand side is not a property.
				var leftSymbol = context.SemanticModel.GetSymbolInfo(assignment.Left, context.CancellationToken).Symbol;
				if (leftSymbol != null && leftSymbol.Kind == SymbolKind.Property)
					continue;
			}
			else if (parent is EqualsValueClauseSyntax equalsClause)
			{
				if (!equalsClause.Value.Contains(singleUsage))
					continue;
				if (equalsClause.Parent is not VariableDeclaratorSyntax variableDeclarator
					|| variableDeclarator.Parent is not VariableDeclarationSyntax variableDeclaration
					|| variableDeclaration.Parent is not LocalDeclarationStatementSyntax localDeclarationStatement
					// for now, bail out if there are more than one variables
					|| localDeclarationStatement.Declaration.Variables.Count != 1)
				{
					continue;
				}
			}
			else
			{
				continue;
			}

			var properties = ImmutableDictionary<string, string?>.Empty
				.Add("isVariableDeclaration", (singleUsage.Parent is EqualsValueClauseSyntax).ToString());
			var diagnostic = Diagnostic.Create(Rule, argument.GetLocation(), properties, outVariableSymbol.Name);
			context.ReportDiagnostic(diagnostic);
		}
	}

	/// <summary>
	/// Determines whether the reference is in the same local scope (method, lambda, or local function)
	/// as the declaration.
	/// </summary>
	private static bool InSameLocalScope(SyntaxNode declaration, SyntaxNode reference)
	{
		var declarationScope = declaration.FirstAncestorOrSelf<MethodDeclarationSyntax>() as SyntaxNode
			?? declaration.FirstAncestorOrSelf<AnonymousFunctionExpressionSyntax>() as SyntaxNode
			?? declaration.FirstAncestorOrSelf<LocalFunctionStatementSyntax>();
		var referenceScope = reference.FirstAncestorOrSelf<MethodDeclarationSyntax>() as SyntaxNode
			?? reference.FirstAncestorOrSelf<AnonymousFunctionExpressionSyntax>() as SyntaxNode
			?? reference.FirstAncestorOrSelf<LocalFunctionStatementSyntax>();

		return declarationScope != null
			&& ReferenceEquals(declarationScope, referenceScope);
	}

	private static bool TryParseOutArgument(
		ArgumentSyntax argument,
		SemanticModel semanticModel,
		CancellationToken cancellationToken,
		[NotNullWhen(returnValue: true)] out ISymbol? outVariableSymbol,
		[NotNullWhen(returnValue: true)] out SyntaxNode? declarationNode)
	{
		outVariableSymbol = null;
		declarationNode = null;

		if (argument.Expression is DeclarationExpressionSyntax declarationExpression
			&& declarationExpression.Designation is SingleVariableDesignationSyntax designation)
		{
			outVariableSymbol = semanticModel.GetDeclaredSymbol(designation, cancellationToken);
			if (outVariableSymbol != null)
			{
				declarationNode = designation;
				return true;
			}
		}
		else if (argument.Expression is IdentifierNameSyntax identifierName)
		{
			outVariableSymbol = semanticModel.GetSymbolInfo(identifierName, cancellationToken).Symbol;
			if (outVariableSymbol != null)
			{
				declarationNode = identifierName;
				return true;
			}
		}

		return false;
	}
}
