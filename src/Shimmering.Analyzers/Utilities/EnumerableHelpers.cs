using System.Diagnostics.CodeAnalysis;

namespace Shimmering.Analyzers.Utilities;

internal static class EnumerableHelpers
{
	public static bool IsLinqMethodCall(
		SemanticModel semanticModel, InvocationExpressionSyntax invocation, CancellationToken cancellationToken, [NotNullWhen(returnValue: true)] out string? methodName)
	{
		methodName = null;

		if (semanticModel.GetSymbolInfo(invocation, cancellationToken).Symbol is not IMethodSymbol methodSymbol) { return false; }
		if (!methodSymbol.IsExtensionMethod) { return false; }

		var containingClass = methodSymbol.ContainingType;
		if (containingClass is null || containingClass.Name != nameof(Enumerable)) { return false; }

		var containingNamespace = methodSymbol.ContainingNamespace;
		if (containingNamespace is null || containingNamespace.Name != "Linq") { return false; }
		var parentNamespace = containingNamespace.ContainingNamespace;
		if (parentNamespace is null || parentNamespace.Name != "System") { return false; }
		if (parentNamespace.ContainingNamespace is null || !parentNamespace.ContainingNamespace.IsGlobalNamespace) { return false; }

		methodName = methodSymbol.Name;
		return true;
	}
}
