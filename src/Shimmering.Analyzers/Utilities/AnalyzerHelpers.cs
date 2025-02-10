namespace Shimmering.Analyzers.Utilities;

internal static class AnalyzerHelpers
{
	public static bool IsOrImplementsInterface(Compilation compilation, ITypeSymbol type, string interfaceMetadataName)
	{
		var interfaceType = compilation.GetTypeByMetadataName(interfaceMetadataName);
		return SymbolEqualityComparer.Default.Equals(type.OriginalDefinition, interfaceType)
			|| type.AllInterfaces.Any(i => SymbolEqualityComparer.Default.Equals(i.OriginalDefinition, interfaceType));
	}
}
