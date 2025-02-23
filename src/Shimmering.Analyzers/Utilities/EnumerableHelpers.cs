using System.Diagnostics.CodeAnalysis;

namespace Shimmering.Analyzers.Utilities;

internal static class EnumerableHelpers
{
	public static bool IsLinqExtensionMethodCall(
		SemanticModel semanticModel, InvocationExpressionSyntax invocation, [NotNullWhen(returnValue: true)] out string? methodName)
	{
		methodName = null;

		if (semanticModel.GetSymbolInfo(invocation).Symbol is not IMethodSymbol methodSymbol) { return false; }
		if (!methodSymbol.IsExtensionMethod) { return false; }

		var containingClass = methodSymbol.ContainingType;
		if (containingClass.Name != nameof(Enumerable)) { return false; }

		var containingNamespace = methodSymbol.ContainingNamespace?.ToDisplayString();
		if (containingNamespace != FullyQualifiedNamespaces.SystemLinq) { return false; }

		var assemblyName = methodSymbol.ContainingAssembly?.Name;
		// TODO: add netstandard?
		if (assemblyName != "System.Linq") { return false; }

		if (!containingClass.ContainingNamespace.ContainingNamespace.ContainingNamespace.IsGlobalNamespace) { return false; }

		if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess) { return false; }

		methodName = memberAccess.Name.Identifier.Text;
		return true;
	}
}
