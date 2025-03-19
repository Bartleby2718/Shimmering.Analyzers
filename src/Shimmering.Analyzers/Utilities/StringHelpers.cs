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

		var containingNamespace = methodSymbol.ContainingNamespace?.ToDisplayString();
		if (containingNamespace != FullyQualifiedNamespaces.System) { return false; }

		if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess) { return false; }

		methodName = memberAccess.Name.Identifier.Text;
		return true;
	}
}
