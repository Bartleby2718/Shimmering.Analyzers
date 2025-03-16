using Shimmering.Analyzers.Utilities;

namespace Shimmering.Analyzers.UsageRules.MissingCancellationToken;

/// <summary>
/// Reports instances of asynchronous methods missing a <see cref="CancellationToken"/> parameter.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class MissingCancellationTokenAnalyzer : ShimmeringSyntaxNodeAnalyzer
{
	private const string Title = $"Include a {nameof(CancellationToken)} parameter";
	private const string Message = $"An asynchronous method is missing a {nameof(CancellationToken)} parameter";
	private const string Category = "Usage";

	private static readonly DiagnosticDescriptor Rule = CreateRule(
		DiagnosticIds.UsageRules.MissingCancellationToken,
		Title,
		Message,
		Category,
		DiagnosticSeverity.Info,
		isEnabledByDefault: true);

#pragma warning disable SA1027 // Use tabs correctly
	public override string SampleCode => """
		using System.Threading.Tasks;

		namespace Tests
		{
		    class Test
		    {
		        async Task [|DoAsync|]()
		        {
		            await Task.CompletedTask;
		        }
		    }
		}
		"""
#pragma warning restore SA1027 // Use tabs correctly
;

	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

	public override void RegisterSyntaxNodeAction(AnalysisContext context)
	{
		context.RegisterSymbolAction(AnalyzeMethod, SymbolKind.Method);
	}
	private static void AnalyzeMethod(SymbolAnalysisContext context)
	{
		if (context.Symbol is not IMethodSymbol methodSymbol
			|| methodSymbol.ReturnType is not INamedTypeSymbol returnType
			|| !IsTaskType(returnType, context.Compilation)
			|| IsInterfaceOrOverrideImplementation(methodSymbol))
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
		if (cancellationTokenType is null) { return false; }

		if (type is not INamedTypeSymbol namedType) { return false; }

		var isNonNullCancellationToken = SymbolEqualityComparer.Default.Equals(namedType.OriginalDefinition, cancellationTokenType);
		if (isNonNullCancellationToken) { return true; }

		return namedType.NullableAnnotation == NullableAnnotation.Annotated
			&& SymbolEqualityComparer.Default.Equals(namedType.TypeArguments[0], cancellationTokenType);
	}

	/// <summary>
	/// Returns true if and only if the method symbol represents either:
	/// - An interface implementation (explicit or implicit), or
	/// - An overridden implementation of a base class member.
	/// </summary>
	private static bool IsInterfaceOrOverrideImplementation(IMethodSymbol methodSymbol)
	{
		// Check if the method is an override of a base class member.
		if (methodSymbol.IsOverride) { return true; }

		// Check for explicit interface implementations.
		if (methodSymbol.ExplicitInterfaceImplementations.Length > 0) { return true; }

		// Check for implicit interface implementations.
		var containingType = methodSymbol.ContainingType;
		if (containingType != null)
		{
			foreach (var @interface in containingType.AllInterfaces)
			{
				foreach (var interfaceMember in @interface.GetMembers())
				{
					var implementation = containingType.FindImplementationForInterfaceMember(interfaceMember);
					if (SymbolEqualityComparer.Default.Equals(implementation, methodSymbol))
					{
						return true;
					}
				}
			}
		}

		return false;
	}
}
