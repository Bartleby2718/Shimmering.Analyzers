using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Shimmering.Analyzers.StyleRules.InlineSingleUseOutVariable;

/// <summary>
/// Reports instances of an out variable that's only assigned to a variable and not used otherwise.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class InlineSingleUseOutVariableAnalyzer : ShimmeringSyntaxNodeAnalyzer
{
	private const string Title = "Inline single-use out variable";
	private const string Message = "Out variable '{0}' is used only once and can be inlined";
	private const string Category = "Style";

	private static readonly DiagnosticDescriptor Rule = CreateRule(
		DiagnosticIds.StyleRules.InlineSingleUseOutVariable,
		Title,
		Message,
		Category,
		DiagnosticSeverity.Info,
		isEnabledByDefault: true);

	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

	public override void RegisterSyntaxNodeAction(AnalysisContext context)
	{
		context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
	}

	private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
	{
		var invocation = (InvocationExpressionSyntax)context.Node;

		foreach (var argument in invocation.ArgumentList.Arguments)
		{
			// process only out arguments
			if (!argument.RefOrOutKeyword.IsKind(SyntaxKind.OutKeyword)) { continue; }
			// process only out declarations
			if (argument.Expression is not DeclarationExpressionSyntax declarationExpression) { continue; }
			// only handle simple out variable declarations
			if (declarationExpression.Designation is not SingleVariableDesignationSyntax singleDesignation) { continue; }

			// find the containing statement and its parent block
			var invocationStatement = invocation.FirstAncestorOrSelf<StatementSyntax>();
			if (invocationStatement == null) { continue; }
			if (invocationStatement.Parent is not BlockSyntax block) { continue; }

			var outVariableName = singleDesignation.Identifier.Text;
			// ensure the out variable is used only in this assignment.
			var usageCount = block.DescendantNodes()
				.OfType<IdentifierNameSyntax>()
				.Count(id => id.Identifier.Text == outVariableName);
			if (usageCount != 1) { continue; }

			// use the helper to locate the assignment statement that uses the out variable
			if (!TryAnalyzeOutParameter(invocation, outVariableName, block, out var analysisResult)) { continue; }

			// report diagnostic on the out variable identifier and include helper data in properties
			var properties = ImmutableDictionary<string, string?>.Empty
				.Add("targetName", analysisResult.TargetName)
				.Add("isDeclaration", analysisResult.IsDeclaration.ToString())
				.Add("assignmentSpanStart", analysisResult.AssignmentSpan.Start.ToString())
				.Add("assignmentSpanLength", analysisResult.AssignmentSpan.Length.ToString());

			var diagnostic = Diagnostic.Create(Rule, singleDesignation.Identifier.GetLocation(), properties, outVariableName);
			context.ReportDiagnostic(diagnostic);
		}
	}

	private static bool TryAnalyzeOutParameter(
		InvocationExpressionSyntax invocation,
		string outVariableName,
		BlockSyntax block,
		[NotNullWhen(returnValue: true)] out OutParameterAnalysisResult? result)
	{
		result = null;

		var statements = block.Statements;
		var invocationStatement = invocation.FirstAncestorOrSelf<StatementSyntax>();
		if (invocationStatement == null) { return false; }

		var index = statements.IndexOf(invocationStatement);
		if (index < 0) { return false; }

		// Look at all statements following the invocation.
		for (var i = index + 1; i < statements.Count; i++)
		{
			var statement = statements[i];
			// Case 1: local declaration assignment: e.g. "var value2 = value;"
			if (statement is LocalDeclarationStatementSyntax localDeclaration)
			{
				var variables = localDeclaration.Declaration.Variables;
				if (variables.Count == 1)
				{
					var variable = variables.First();
					if (variable.Initializer?.Value is IdentifierNameSyntax identifierNameSyntax
						&& identifierNameSyntax.Identifier.Text == outVariableName)
					{
						result = new OutParameterAnalysisResult(variable.Identifier.Text, IsDeclaration: true, statement.Span);
						return true;
					}
				}
			}
			// Case 2: simple assignment: e.g. "value2 = value;"
			else if (statement is ExpressionStatementSyntax expressionStatement
				&& expressionStatement.Expression is AssignmentExpressionSyntax assignment
				&& assignment.Right is IdentifierNameSyntax rightHandSide
				&& rightHandSide.Identifier.Text == outVariableName
				&& assignment.Left is IdentifierNameSyntax leftHandSide)
			{
				result = new OutParameterAnalysisResult(leftHandSide.Identifier.Text, IsDeclaration: false, statement.Span);
				return true;
			}
		}

		return false;
	}

	private record OutParameterAnalysisResult(string TargetName, bool IsDeclaration, TextSpan AssignmentSpan);
}
