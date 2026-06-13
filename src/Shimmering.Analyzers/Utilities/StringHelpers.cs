using System.Diagnostics.CodeAnalysis;

namespace Shimmering.Analyzers.Utilities;

internal static class StringHelpers
{
	public static bool IsStringMethodCall(
		SemanticModel semanticModel, InvocationExpressionSyntax invocation, CancellationToken cancellationToken, bool isStatic, [NotNullWhen(returnValue: true)] out string? methodName)
	{
		methodName = null;

		if (semanticModel.GetSymbolInfo(invocation, cancellationToken).Symbol is not IMethodSymbol methodSymbol) { return false; }
		if (methodSymbol.IsStatic != isStatic) { return false; }

		var containingNamespace = methodSymbol.ContainingNamespace;
		if (containingNamespace is null || containingNamespace.Name != "System") { return false; }
		if (containingNamespace.ContainingNamespace is null || !containingNamespace.ContainingNamespace.IsGlobalNamespace) { return false; }

		methodName = methodSymbol.Name;
		return true;
	}
}
