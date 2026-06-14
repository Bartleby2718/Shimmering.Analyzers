using Shimmering.Analyzers.Core;

namespace Shimmering.Analyzers.Analyzers.Usage;

/// <summary>
/// Replaces new CultureInfo(...) constructor calls with cached alternatives like CultureInfo.GetCultureInfo(...) or CultureInfo.InvariantCulture.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UseGetCultureInfoAnalyzer : ShimmeringAnalyzer
{
	private const string Title = "Use cached CultureInfo instead of allocating a new instance";
	private const string Message = "Use cached CultureInfo instead of allocating a new instance";
	private const string Category = RuleCategories.Usage;

	private static readonly DiagnosticDescriptor Rule = ShimmeringRuleFactory.Create(
		DiagnosticIds.UsageRules.UseGetCultureInfo,
		Title,
		Message,
		Category,
		DiagnosticSeverity.Warning);

	public override string SampleCode => """
		using System.Globalization;

		class Test
		{
			void Do()
			{
				var culture = [|new CultureInfo("en-US")|];
			}
		}
		""";

	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

	protected override void InitializeCore(AnalysisContext context)
	{
		context.RegisterSyntaxNodeAction(AnalyzeObjectCreation, SyntaxKind.ObjectCreationExpression);
	}

	private static void AnalyzeObjectCreation(SyntaxNodeAnalysisContext context)
	{
		var objectCreation = (ObjectCreationExpressionSyntax)context.Node;
		if (objectCreation.ArgumentList == null)
		{
			return;
		}

		var constructorSymbol = context.SemanticModel.GetSymbolInfo(objectCreation, context.CancellationToken).Symbol as IMethodSymbol;
		if (constructorSymbol == null)
		{
			return;
		}

		var containingType = constructorSymbol.ContainingType;
		if (containingType == null || containingType.ToDisplayString() != "System.Globalization.CultureInfo")
		{
			return;
		}

		var parameters = constructorSymbol.Parameters;
		var arguments = objectCreation.ArgumentList.Arguments;
		if (parameters.Length == 0 || arguments.Count == 0)
		{
			return;
		}

		var firstParameter = parameters[0];
		var firstArgument = arguments[0];

		bool isValidSignature = false;
		if (parameters.Length == 1)
		{
			if (firstParameter.Type.SpecialType == SpecialType.System_String || firstParameter.Type.SpecialType == SpecialType.System_Int32)
			{
				isValidSignature = true;
			}
		}
		else if (parameters.Length == 2)
		{
			var secondParameter = parameters[1];
			var secondArgument = arguments[1];
			if ((firstParameter.Type.SpecialType == SpecialType.System_String || firstParameter.Type.SpecialType == SpecialType.System_Int32)
				&& secondParameter.Type.SpecialType == SpecialType.System_Boolean)
			{
				var constantValue = context.SemanticModel.GetConstantValue(secondArgument.Expression, context.CancellationToken);
				if (constantValue.HasValue && constantValue.Value is false)
				{
					isValidSignature = true;
				}
			}
		}

		if (!isValidSignature)
		{
			return;
		}

		if (IsAssignedToLocalAndEscapesOrMutates(objectCreation, context))
		{
			return;
		}

		context.ReportDiagnostic(Diagnostic.Create(Rule, objectCreation.GetLocation()));
	}

	private static bool IsAssignedToLocalAndEscapesOrMutates(ObjectCreationExpressionSyntax objectCreation, SyntaxNodeAnalysisContext context)
	{
		var parent = objectCreation.Parent;
		ILocalSymbol? localSymbol = null;

		if (parent is EqualsValueClauseSyntax && parent.Parent is VariableDeclaratorSyntax declarator)
		{
			localSymbol = context.SemanticModel.GetDeclaredSymbol(declarator, context.CancellationToken) as ILocalSymbol;
		}
		else if (parent is AssignmentExpressionSyntax assignment && assignment.Right == objectCreation)
		{
			localSymbol = context.SemanticModel.GetSymbolInfo(assignment.Left, context.CancellationToken).Symbol as ILocalSymbol;
		}
		else
		{
			if (IsEscapingParentNode(parent))
			{
				return true;
			}
			return false;
		}

		if (localSymbol == null)
		{
			return false;
		}

		var enclosingNode = objectCreation.FirstAncestorOrSelf<MethodDeclarationSyntax>() as SyntaxNode
			?? objectCreation.FirstAncestorOrSelf<LocalFunctionStatementSyntax>() as SyntaxNode
			?? objectCreation.FirstAncestorOrSelf<AccessorDeclarationSyntax>() as SyntaxNode;

		if (enclosingNode == null)
		{
			return false;
		}

		var references = enclosingNode.DescendantNodes()
			.OfType<IdentifierNameSyntax>()
			.Where(identifier => SymbolEqualityComparer.Default.Equals(context.SemanticModel.GetSymbolInfo(identifier, context.CancellationToken).Symbol, localSymbol))
			.ToList();

		foreach (var reference in references)
		{
			if (IsMutationReference(reference))
			{
				return true;
			}
			if (IsEscapeReference(reference, enclosingNode, context))
			{
				return true;
			}
		}

		return false;
	}

	private static bool IsEscapingParentNode(SyntaxNode? parent)
	{
		if (parent == null)
		{
			return false;
		}

		if (parent is ReturnStatementSyntax || parent is ArrowExpressionClauseSyntax || parent is FieldDeclarationSyntax || parent is PropertyDeclarationSyntax)
		{
			return true;
		}

		if (parent is AssignmentExpressionSyntax)
		{
			return true;
		}

		return false;
	}

	private static bool IsMutationReference(IdentifierNameSyntax reference)
	{
		SyntaxNode current = reference;
		while (current.Parent is MemberAccessExpressionSyntax memberAccess && memberAccess.Expression == current)
		{
			current = memberAccess;
		}

		if (current.Parent is AssignmentExpressionSyntax assignment && assignment.Left == current)
		{
			return true;
		}

		return false;
	}

	private static bool IsEscapeReference(IdentifierNameSyntax reference, SyntaxNode enclosingNode, SyntaxNodeAnalysisContext context)
	{
		var parent = reference.Parent;
		if (parent == null)
		{
			return false;
		}

		if (parent is ReturnStatementSyntax)
		{
			return true;
		}

		if (parent is ArgumentSyntax)
		{
			return true;
		}

		var hasLambdaAncestor = false;
		var current = parent;
		while (current != null && current != enclosingNode)
		{
			if (current is LambdaExpressionSyntax || current is AnonymousMethodExpressionSyntax)
			{
				hasLambdaAncestor = true;
				break;
			}
			current = current.Parent;
		}

		if (hasLambdaAncestor)
		{
			return true;
		}

		if (parent is AssignmentExpressionSyntax assignment && assignment.Right == reference)
		{
			var leftHandSideSymbol = context.SemanticModel.GetSymbolInfo(assignment.Left, context.CancellationToken).Symbol;
			if (leftHandSideSymbol is not ILocalSymbol)
			{
				return true;
			}
		}

		return false;
	}
}
