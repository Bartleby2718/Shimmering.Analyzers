namespace Shimmering.Analyzers.NullableCancellationToken;

/// <summary>
/// Reports instances of nullable <see cref="CancellationToken"/>s in method signatures.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
internal sealed class NullableCancellationTokenAnalyzer : DiagnosticAnalyzer
{
	private const string Title = $"Do not use a nullable {nameof(CancellationToken)}";
	private const string Message = $"{nameof(CancellationToken)} should not be nullable";
	private const string Category = "CodeQuality";

	private static readonly DiagnosticDescriptor Rule = new(
		DiagnosticIds.NullableCancellationToken,
		Title,
		Message,
		Category,
		DiagnosticSeverity.Info,
		isEnabledByDefault: true,
		helpLinkUri: $"https://github.com/Bartleby2718/Shimmering.Analyzers/blob/main/docs/{DiagnosticIds.NullableCancellationToken}.md");

	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

	public override void Initialize(AnalysisContext context)
	{
		context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
		context.EnableConcurrentExecution();
		context.RegisterSyntaxNodeAction(AnalyzeParameter, SyntaxKind.Parameter);
	}

	private static void AnalyzeParameter(SyntaxNodeAnalysisContext context)
	{
		var parameter = (ParameterSyntax)context.Node;
		if (parameter.Type is not NullableTypeSyntax nullableType) { return; }
		if (context.SemanticModel.GetSymbolInfo(nullableType.ElementType).Symbol is not INamedTypeSymbol typeSymbol) { return; }
		var cancellationTokenSymbol = context.Compilation.GetTypeByMetadataName("System.Threading.CancellationToken");
		if (SymbolEqualityComparer.Default.Equals(typeSymbol, cancellationTokenSymbol))
		{
			// Technically, this can break consumers of the method, but still flagging to promote a best practice.
			context.ReportDiagnostic(Diagnostic.Create(Rule, parameter.GetLocation()));
		}
	}
}
