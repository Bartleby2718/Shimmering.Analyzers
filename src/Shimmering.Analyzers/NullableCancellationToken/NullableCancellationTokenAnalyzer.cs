using Shimmering.Analyzers.Utilities;

namespace Shimmering.Analyzers.NullableCancellationToken;

/// <summary>
/// Reports instances of nullable <see cref="CancellationToken"/>s in method signatures.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
internal sealed class NullableCancellationTokenAnalyzer : ShimmeringSyntaxNodeAnalyzer
{
	private const string Title = $"Do not use a nullable {nameof(CancellationToken)}";
	private const string Message = $"{nameof(CancellationToken)} should not be nullable";
	private const string Category = "CodeQuality";

	private static readonly DiagnosticDescriptor Rule = CreateRule(
		DiagnosticIds.NullableCancellationToken,
		Title,
		Message,
		Category,
		DiagnosticSeverity.Info,
		isEnabledByDefault: true);

	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

	public override void RegisterSyntaxNodeAction(AnalysisContext context)
	{
		context.RegisterSyntaxNodeAction(AnalyzeParameter, SyntaxKind.Parameter);
	}

	private static void AnalyzeParameter(SyntaxNodeAnalysisContext context)
	{
		var parameter = (ParameterSyntax)context.Node;
		if (parameter.Type is not NullableTypeSyntax nullableType) { return; }
		if (context.SemanticModel.GetSymbolInfo(nullableType.ElementType).Symbol is not INamedTypeSymbol typeSymbol) { return; }
		var cancellationTokenSymbol = context.Compilation.GetTypeByMetadataName(FullyQualifiedTypeNames.CancellationToken);
		if (SymbolEqualityComparer.Default.Equals(typeSymbol, cancellationTokenSymbol))
		{
			// Technically, this can break consumers of the method, but still flagging to promote a best practice.
			context.ReportDiagnostic(Diagnostic.Create(Rule, parameter.GetLocation()));
		}
	}
}
