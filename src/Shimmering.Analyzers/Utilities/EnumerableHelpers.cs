using System.Diagnostics.CodeAnalysis;

namespace Shimmering.Analyzers.Utilities;

internal static class EnumerableHelpers
{
	/// <summary>
	/// If you have an <see cref="InvocationExpressionSyntax"/>, you can pass its Expression as <paramref name="memberAccess"/>.
	/// </summary>
	public static bool IsEnumerableMethodInSystemLinq(
		SemanticModel semanticModel, MemberAccessExpressionSyntax memberAccess, [NotNullWhen(returnValue: true)] out string? methodName)
	{
		methodName = null;

		if (semanticModel.GetSymbolInfo(memberAccess).Symbol is not IMethodSymbol methodSymbol) { return false; }

		var containingClass = methodSymbol.ContainingType;
		if (containingClass.Name != nameof(Enumerable)) { return false; }

		if (containingClass.ContainingNamespace.Name != nameof(System.Linq)) { return false; }

		if (containingClass.ContainingNamespace.ContainingNamespace is not INamespaceSymbol namespaceSymbol) { return false; }
		if (namespaceSymbol.Name != nameof(System)) { return false; }
		if (!containingClass.ContainingNamespace.ContainingNamespace.ContainingNamespace.IsGlobalNamespace) { return false; }

		methodName = memberAccess.Name.Identifier.Text;
		return true;
	}
}
