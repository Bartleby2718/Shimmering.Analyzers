using Shimmering.Analyzers.Utilities;

namespace Shimmering.Analyzers.MissingCancellationToken;

/// <summary>
/// Reports instances of asynchronous methods missing a <see cref="CancellationToken"/> parameter.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
internal sealed class MissingCancellationTokenAnalyzer : DiagnosticAnalyzer
{
	private const string Title = $"Include a {nameof(CancellationToken)} parameter";
	private const string Message = $"Missing a {nameof(CancellationToken)} parameter";
	private const string Category = "CodeQuality";

	private static readonly DiagnosticDescriptor Rule = new(
		DiagnosticIds.MissingCancellationToken,
		Title,
		Message,
		Category,
		DiagnosticSeverity.Info,
		isEnabledByDefault: true);

	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

	public override void Initialize(AnalysisContext context)
	{
		context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
		context.EnableConcurrentExecution();
		context.RegisterSymbolAction(AnalyzeMethod, SymbolKind.Method);
	}
	private static void AnalyzeMethod(SymbolAnalysisContext context)
	{
		if (context.Symbol is not IMethodSymbol methodSymbol
			|| methodSymbol.ReturnType is not INamedTypeSymbol returnType
			|| !IsTaskType(returnType, context.Compilation))
		{
			return;
		}

		// Check for existing CancellationToken or CancellationToken? parameter
		if (methodSymbol.Parameters.Any(p => IsCancellationTokenType(p.Type, context.Compilation)))
			return;

		var diagnostic = Diagnostic.Create(Rule, methodSymbol.Locations[0]);
		context.ReportDiagnostic(diagnostic);
	}

	private static bool IsTaskType(INamedTypeSymbol returnType, Compilation compilation)
	{
		var taskType = compilation.GetTypeByMetadataName(FullyQualifiedTypeNames.Task);
		var taskOfTType = compilation.GetTypeByMetadataName(FullyQualifiedTypeNames.TaskOfT);

		return SymbolEqualityComparer.Default.Equals(returnType.OriginalDefinition, taskType)
			|| SymbolEqualityComparer.Default.Equals(returnType.OriginalDefinition, taskOfTType);
	}

	private static bool IsCancellationTokenType(ITypeSymbol type, Compilation compilation)
	{
		var cancellationTokenType = compilation.GetTypeByMetadataName(FullyQualifiedTypeNames.CancellationToken);
		return type is INamedTypeSymbol namedType
			&& SymbolEqualityComparer.Default.Equals(namedType.OriginalDefinition, cancellationTokenType);
	}
}
