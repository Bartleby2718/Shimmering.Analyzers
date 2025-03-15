using Shimmering.Analyzers.Utilities;

namespace Shimmering.Analyzers.StyleRules.PrimaryConstructorParameterReassignment;

/// <summary>
/// Reports instances of primary constructor parameters that are reassigned.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class PrimaryConstructorParameterReassignmentAnalyzer : ShimmeringSyntaxNodeAnalyzer
{
	private const string Title = "Avoid reassigning primary constructor parameter";
	private const string Message = "Primary constructor parameter '{0}' shouldn't be reassigned";
	private const string Category = "Style";

	private static readonly DiagnosticDescriptor Rule = CreateRule(
		DiagnosticIds.StyleRules.PrimaryConstructorParameterReassignment,
		Title,
		Message,
		Category,
		DiagnosticSeverity.Info,
		isEnabledByDefault: false);

	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

	public override void RegisterSyntaxNodeAction(AnalysisContext context)
	{
		// assignment expressions
		context.RegisterSyntaxNodeAction(
			AnalyzeAssignment,
			SyntaxKind.SimpleAssignmentExpression,
			// compound assignments
			SyntaxKind.AddAssignmentExpression,
			SyntaxKind.SubtractAssignmentExpression,
			SyntaxKind.MultiplyAssignmentExpression,
			SyntaxKind.DivideAssignmentExpression,
			SyntaxKind.ModuloAssignmentExpression,
			SyntaxKind.AndAssignmentExpression,
			SyntaxKind.ExclusiveOrAssignmentExpression,
			SyntaxKind.OrAssignmentExpression,
			SyntaxKind.LeftShiftAssignmentExpression,
			SyntaxKind.RightShiftAssignmentExpression,
			SyntaxKind.UnsignedRightShiftAssignmentExpression,
			SyntaxKind.CoalesceAssignmentExpression);

		// prefix increment/decrement expressions
		context.RegisterSyntaxNodeAction(
			AnalyzePrefixIncrementDecrement,
			SyntaxKind.PreIncrementExpression,
			SyntaxKind.PreDecrementExpression);

		// postfix increment/decrement expressions
		context.RegisterSyntaxNodeAction(
			AnalyzePostfixIncrementDecrement,
			SyntaxKind.PostIncrementExpression,
			SyntaxKind.PostDecrementExpression);
	}

	private static void AnalyzeAssignment(SyntaxNodeAnalysisContext context)
	{
		if (!CsharpVersionHelpers.SupportsPrimaryConstructors(context)) { return; }

		var assignment = (AssignmentExpressionSyntax)context.Node;
		CheckAndReport(context, assignment.Left, context.CancellationToken);
	}

	private static void AnalyzePrefixIncrementDecrement(SyntaxNodeAnalysisContext context)
	{
		if (!CsharpVersionHelpers.SupportsPrimaryConstructors(context)) { return; }

		var operand = (PrefixUnaryExpressionSyntax)context.Node;
		CheckAndReport(context, operand.Operand, context.CancellationToken);
	}

	private static void AnalyzePostfixIncrementDecrement(SyntaxNodeAnalysisContext context)
	{
		if (!CsharpVersionHelpers.SupportsPrimaryConstructors(context)) { return; }

		var operand = (PostfixUnaryExpressionSyntax)context.Node;
		CheckAndReport(context, operand.Operand, context.CancellationToken);
	}

	private static void CheckAndReport(SyntaxNodeAnalysisContext context, ExpressionSyntax expression, CancellationToken cancellationToken)
	{
		// We only care about identifier names.
		if (expression is not IdentifierNameSyntax identifier) { return; }

		if (context.SemanticModel.GetSymbolInfo(identifier, cancellationToken).Symbol is IParameterSymbol symbol
			&& IsPrimaryConstructorParameter(symbol))
		{
			var diagnostic = Diagnostic.Create(Rule, identifier.GetLocation(), symbol.Name);
			context.ReportDiagnostic(diagnostic);
		}
	}

	private static bool IsPrimaryConstructorParameter(IParameterSymbol parameter)
	{
		// Check if the parameter's declaration appears in a type’s parameter list.
		foreach (var reference in parameter.DeclaringSyntaxReferences)
		{
			var syntax = reference.GetSyntax();
			if (syntax.Parent is ParameterListSyntax parameterList
				// This part checks for primary constructors. In comparison:
				// - Method parameter -> MethodDeclarationSyntax
				// - Local function parameter -> LocalFunctionStatementSyntax
				// - Regular constructor parameter -> ConstructorDeclarationSyntax
				&& parameterList.Parent is TypeDeclarationSyntax)
			{
				return true;
			}
		}
		return false;
	}
}
