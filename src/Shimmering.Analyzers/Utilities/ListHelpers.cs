using System.Diagnostics.CodeAnalysis;

namespace Shimmering.Analyzers.Utilities;

internal static class ListHelpers
{
	public static bool IsListInstanceMethodCall(
		SemanticModel semanticModel, InvocationExpressionSyntax invocation, CancellationToken cancellationToken, [NotNullWhen(returnValue: true)] out string? methodName)
	{
		methodName = null;

		if (semanticModel.GetSymbolInfo(invocation, cancellationToken).Symbol is not IMethodSymbol methodSymbol) { return false; }
		if (methodSymbol.IsStatic) { return false; }

		var containingClass = methodSymbol.ContainingType;
		if (containingClass is null || containingClass.Name != nameof(List<int>)) { return false; }

		var containingNamespace = methodSymbol.ContainingNamespace;
		if (containingNamespace is null || containingNamespace.Name != "Generic") { return false; }
		var parentNamespace = containingNamespace.ContainingNamespace;
		if (parentNamespace is null || parentNamespace.Name != "Collections") { return false; }
		var grandParentNamespace = parentNamespace.ContainingNamespace;
		if (grandParentNamespace is null || grandParentNamespace.Name != "System") { return false; }
		if (grandParentNamespace.ContainingNamespace is null || !grandParentNamespace.ContainingNamespace.IsGlobalNamespace) { return false; }

		methodName = methodSymbol.Name;
		return true;
	}
}
