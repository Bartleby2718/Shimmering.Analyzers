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
		if (containingClass.Name != nameof(List<int>)) { return false; }

		var containingNamespace = methodSymbol.ContainingNamespace?.ToDisplayString();
		if (containingNamespace != FullyQualifiedNamespaces.SystemCollectionsGeneric) { return false; }

		if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess) { return false; }

		methodName = memberAccess.Name.Identifier.Text;
		return true;
	}
}
