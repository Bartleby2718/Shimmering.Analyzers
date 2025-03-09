using Shimmering.Analyzers.Utilities;

namespace Shimmering.Analyzers.UsageRules.SingleUseIEnumerableMaterialization;

/// <summary>
/// Reports instances of a single-use IEnumerable that is materialized.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
internal sealed class SingleUseIEnumerableMaterializationAnalyzer : ShimmeringSyntaxNodeAnalyzer
{
	private const string Title = "Avoid materializing a single-use IEnumerable";
	private const string Message = "Avoid materializing an IEnumerable if it's used only once";
	private const string Category = "Usage";

	private static readonly DiagnosticDescriptor Rule = CreateRule(
		DiagnosticIds.UsageRules.SingleUseIEnumerableMaterialization,
		Title,
		Message,
		Category,
		DiagnosticSeverity.Info,
		isEnabledByDefault: false);

	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

	public override void RegisterSyntaxNodeAction(AnalysisContext context)
	{
		context.RegisterSyntaxNodeAction(AnalyzeLocalDeclaration, SyntaxKind.LocalDeclarationStatement);
	}

	private static void AnalyzeLocalDeclaration(SyntaxNodeAnalysisContext context)
	{
		var localDeclaration = (LocalDeclarationStatementSyntax)context.Node;

		// For now, handle only single variable declarations for simplicity.
		if (localDeclaration.Declaration.Variables.Count != 1) { return; }

		if (!localDeclaration.Declaration.Type.IsVar) { return; }

		var variableDeclarator = localDeclaration.Declaration.Variables.First();
		if (variableDeclarator.Initializer == null) { return; }

		// Look for an invocation expression in the initializer.
		if (variableDeclarator.Initializer.Value is not InvocationExpressionSyntax invocation) { return; }

		// Check that the invocation is a member access
		if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess) { return; }

		// Only proceed if the method is "ToList" or "ToArray"
		if (!EnumerableHelpers.IsLinqExtensionMethodCall(context.SemanticModel, invocation, out var methodName)
			|| methodName is not (nameof(Enumerable.ToList) or nameof(Enumerable.ToArray)))
		{
			return;
		}

		// Bail out if the receiver is an IQueryable<T> because removing materialization affects business logic
		if (AnalyzerHelpers.IsOrImplementsInterface(context, memberAccess.Expression, FullyQualifiedTypeNames.IQueryableOfT))
		{
			return;
		}

		// Get the symbol for the variable.
		var variableSymbol = context.SemanticModel.GetDeclaredSymbol(variableDeclarator);
		if (variableSymbol == null) { return; }

		// Find the enclosing block.
		if (localDeclaration.Parent is not BlockSyntax parentBlock) { return; }

		// Find all identifier usages of this variable (excluding the declaration itself).
		var identifierUsages = parentBlock.DescendantNodes()
			.OfType<IdentifierNameSyntax>()
			.Where(id => SymbolEqualityComparer.Default.Equals(variableSymbol, context.SemanticModel.GetSymbolInfo(id).Symbol))
			.Where(id => id.SpanStart != variableDeclarator.Identifier.SpanStart)
			.ToList();

		if (identifierUsages.Count != 1) { return; }
		var identifierUsage = identifierUsages.Single();
		// The identifier should not be in a lambda, as it's actually "used" multiple times
		// in a lambda like .Where(x => identifier.Contains(x))
		if (!IsInLambda(identifierUsage)
			&& IsSafeToUseIEnumerableWithoutMaterializing(identifierUsage, context.SemanticModel))
		{
			var diagnostic = Diagnostic.Create(Rule, invocation.GetLocation(), variableSymbol.Name);
			context.ReportDiagnostic(diagnostic);
		}
	}

	private static bool IsInLambda(IdentifierNameSyntax identifier)
	{
		return identifier.Ancestors().Any(a => a is LambdaExpressionSyntax);
	}

	private static bool IsSafeToUseIEnumerableWithoutMaterializing(IdentifierNameSyntax identifier, SemanticModel semanticModel) =>
		IsUsedInForeach(identifier) || IsFollowedByLinqMethod(identifier, semanticModel);

	private static bool IsUsedInForeach(IdentifierNameSyntax identifier)
	{
		return identifier.Parent is ForEachStatementSyntax foreachStatement
			&& foreachStatement.Expression == identifier;
	}

	private static bool IsFollowedByLinqMethod(IdentifierNameSyntax identifier, SemanticModel semanticModel)
	{
		return identifier.Parent is MemberAccessExpressionSyntax memberAccess
			&& memberAccess.Parent is InvocationExpressionSyntax invocation
			&& EnumerableHelpers.IsLinqExtensionMethodCall(semanticModel, invocation, out _);
	}
}
