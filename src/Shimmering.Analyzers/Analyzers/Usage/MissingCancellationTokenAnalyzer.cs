using System.Collections.Immutable;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

using Shimmering.Analyzers.Core;
using Shimmering.Analyzers.Utilities;

namespace Shimmering.Analyzers.Analyzers.Usage;

/// <summary>
/// Reports instances of asynchronous methods missing a <see cref="CancellationToken"/> parameter.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class MissingCancellationTokenAnalyzer : Core.ShimmeringAnalyzer
{
	private static readonly DiagnosticDescriptor Rule = RuleFactory.Create(
		DiagnosticIds.UsageRules.MissingCancellationToken,
		"Include a CancellationToken parameter in an asynchronous method",
		"An asynchronous method is missing a CancellationToken parameter",
		RuleCategories.Usage,
		DiagnosticSeverity.Info);

	public override string SampleCode => """
		using System.Threading.Tasks;

		namespace Tests;
		class Test
		{
			async Task [|DoAsync|]()
			{
				await Task.CompletedTask;
			}
		}
		""";

	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

	protected override void InitializeCore(AnalysisContext context)
	{
		context.RegisterSymbolAction(AnalyzeMethod, SymbolKind.Method);
		context.RegisterOperationAction(AnalyzeInvocation, OperationKind.Invocation);
	}

	private static void AnalyzeMethod(SymbolAnalysisContext context)
	{
		var methodSymbol = (IMethodSymbol)context.Symbol;

		if (!IsAwaitable(methodSymbol.ReturnType, context.Compilation)
			|| IsInterfaceOrOverrideImplementation(methodSymbol))
		{
			return;
		}

		// Check for existing CancellationToken or CancellationToken? parameter
		if (methodSymbol.Parameters.Any(p => IsCancellationTokenType(p.Type, context.Compilation)))
		{
			return;
		}

		var diagnostic = Diagnostic.Create(Rule, methodSymbol.Locations[0]);
		context.ReportDiagnostic(diagnostic);
	}

	private static void AnalyzeInvocation(OperationAnalysisContext context)
	{
		var invocation = (Microsoft.CodeAnalysis.Operations.IInvocationOperation)context.Operation;

		foreach (var argument in invocation.Arguments)
		{
			if (argument.Parameter != null && IsCancellationTokenType(argument.Parameter.Type, context.Compilation))
			{
				var value = argument.Value;
				if (value is Microsoft.CodeAnalysis.Operations.IConversionOperation conversion)
				{
					value = conversion.Operand;
				}

				if (argument.IsImplicit)
				{
					context.ReportDiagnostic(Diagnostic.Create(Rule, invocation.Syntax.GetLocation()));
					return;
				}

				if (value is Microsoft.CodeAnalysis.Operations.IDefaultValueOperation)
				{
					context.ReportDiagnostic(Diagnostic.Create(Rule, argument.Syntax.GetLocation()));
					return;
				}

				if (value is Microsoft.CodeAnalysis.Operations.IPropertyReferenceOperation propertyRef &&
					propertyRef.Property.Name == "None" &&
					IsCancellationTokenType(propertyRef.Property.ContainingType, context.Compilation))
				{
					context.ReportDiagnostic(Diagnostic.Create(Rule, argument.Syntax.GetLocation()));
					return;
				}
			}
		}
	}

	private static bool IsAwaitable(ITypeSymbol returnType, Compilation compilation)
	{
		if (returnType is not INamedTypeSymbol namedType)
		{
			return false;
		}

		var originalDefinition = namedType.OriginalDefinition;

		// Task, Task<T>
		var taskType = compilation.GetTypeByMetadataName(FullyQualifiedTypeNames.Task);
		var taskOfTType = compilation.GetTypeByMetadataName(FullyQualifiedTypeNames.TaskOfT);
		if (SymbolEqualityComparer.Default.Equals(originalDefinition, taskType) ||
			SymbolEqualityComparer.Default.Equals(originalDefinition, taskOfTType))
		{
			return true;
		}

		// ValueTask, ValueTask<T>
		var valueTaskType = compilation.GetTypeByMetadataName("System.Threading.Tasks.ValueTask");
		var valueTaskOfTType = compilation.GetTypeByMetadataName("System.Threading.Tasks.ValueTask`1");
		if (SymbolEqualityComparer.Default.Equals(originalDefinition, valueTaskType) ||
			SymbolEqualityComparer.Default.Equals(originalDefinition, valueTaskOfTType))
		{
			return true;
		}

		// IAsyncEnumerable<T>
		var asyncEnumerableType = compilation.GetTypeByMetadataName("System.Collections.Generic.IAsyncEnumerable`1");
		if (SymbolEqualityComparer.Default.Equals(originalDefinition, asyncEnumerableType))
		{
			return true;
		}

		return false;
	}

	private static bool IsCancellationTokenType(ITypeSymbol type, Compilation compilation)
	{
		var cancellationTokenType = compilation.GetTypeByMetadataName(FullyQualifiedTypeNames.CancellationToken);
		if (cancellationTokenType is null)
		{
			return false;
		}

		// Exact match
		if (SymbolEqualityComparer.Default.Equals(type, cancellationTokenType))
		{
			return true;
		}

		// Nullable<CancellationToken>
		if (type is INamedTypeSymbol namedType &&
			namedType.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T &&
			SymbolEqualityComparer.Default.Equals(namedType.TypeArguments[0], cancellationTokenType))
		{
			return true;
		}

		return false;
	}

	private static bool IsInterfaceOrOverrideImplementation(IMethodSymbol methodSymbol)
	{
		if (methodSymbol.IsOverride)
		{
			return true;
		}

		if (methodSymbol.ExplicitInterfaceImplementations.Length > 0)
		{
			return true;
		}

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
