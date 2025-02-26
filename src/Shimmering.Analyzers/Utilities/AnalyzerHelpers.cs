namespace Shimmering.Analyzers.Utilities;

internal static class AnalyzerHelpers
{
	public static bool IsOrImplementsInterface(SyntaxNodeAnalysisContext context, ExpressionSyntax expression, string interfaceMetadataName)
	{
		var semanticModel = context.SemanticModel;
		var receiverType = semanticModel.GetTypeInfo(expression, context.CancellationToken).Type;
		if (receiverType is null) { return false; }

		var interfaceMetadata = semanticModel.Compilation.GetTypeByMetadataName(interfaceMetadataName);
		if (interfaceMetadata is null) { return false; }

		return receiverType.OriginalDefinition.Equals(interfaceMetadata, SymbolEqualityComparer.Default)
			|| receiverType.AllInterfaces.Any(i => i.OriginalDefinition.Equals(interfaceMetadata, SymbolEqualityComparer.Default));
	}
}
