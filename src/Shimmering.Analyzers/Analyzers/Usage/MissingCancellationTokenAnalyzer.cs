using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
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
public sealed class MissingCancellationTokenAnalyzer : ShimmeringAnalyzer
{
	private static readonly DiagnosticDescriptor Rule = ShimmeringRuleFactory.Create(
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

	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

	protected override void InitializeCore(AnalysisContext context)
	{
		context.RegisterCompilationStartAction(compilationContext =>
		{
			var compilation = compilationContext.Compilation;
			var taskType = compilation.GetTypeByMetadataName(FullyQualifiedTypeNames.Task);
			var taskOfTType = compilation.GetTypeByMetadataName(FullyQualifiedTypeNames.TaskOfT);
			var valueTaskType = compilation.GetTypeByMetadataName("System.Threading.Tasks.ValueTask");
			var valueTaskOfTType = compilation.GetTypeByMetadataName("System.Threading.Tasks.ValueTask`1");
			var asyncEnumerableType = compilation.GetTypeByMetadataName("System.Collections.Generic.IAsyncEnumerable`1");
			var cancellationTokenType = compilation.GetTypeByMetadataName(FullyQualifiedTypeNames.CancellationToken);

			// If fundamental types aren't available, we don't need to run.
			if (taskType == null && valueTaskType == null && asyncEnumerableType == null)
			{
				return;
			}

			var compilationState = new CompilationState(
				taskType,
				taskOfTType,
				valueTaskType,
				valueTaskOfTType,
				asyncEnumerableType,
				cancellationTokenType);

			compilationContext.RegisterSymbolAction(symbolContext => AnalyzeMethod(symbolContext, compilationState), SymbolKind.Method);
		});
	}

	private static void AnalyzeMethod(SymbolAnalysisContext context, CompilationState compilationState)
	{
		var methodSymbol = (IMethodSymbol)context.Symbol;

		if (!IsAwaitable(methodSymbol.ReturnType, compilationState)
			|| IsInterfaceOrOverrideImplementation(methodSymbol))
		{
			return;
		}

		// Check for existing CancellationToken or CancellationToken? parameter
		if (methodSymbol.Parameters.Any(parameter => IsCancellationTokenType(parameter.Type, compilationState)))
		{
			return;
		}

		var diagnostic = Diagnostic.Create(Rule, methodSymbol.Locations[0]);
		context.ReportDiagnostic(diagnostic);
	}

	private static bool IsAwaitable(ITypeSymbol returnType, CompilationState compilationState)
	{
		if (returnType is not INamedTypeSymbol namedType)
		{
			return false;
		}

		var originalDefinition = namedType.OriginalDefinition;

		// Task, Task<T>
		if (SymbolEqualityComparer.Default.Equals(originalDefinition, compilationState.TaskType) ||
			SymbolEqualityComparer.Default.Equals(originalDefinition, compilationState.TaskOfTType))
		{
			return true;
		}

		// ValueTask, ValueTask<T>
		if (SymbolEqualityComparer.Default.Equals(originalDefinition, compilationState.ValueTaskType) ||
			SymbolEqualityComparer.Default.Equals(originalDefinition, compilationState.ValueTaskOfTType))
		{
			return true;
		}

		// IAsyncEnumerable<T>
		if (SymbolEqualityComparer.Default.Equals(originalDefinition, compilationState.AsyncEnumerableType))
		{
			return true;
		}

		return false;
	}

	private static bool IsCancellationTokenType(ITypeSymbol type, CompilationState compilationState)
	{
		var cancellationTokenType = compilationState.CancellationTokenType;
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

	private sealed class CompilationState
	{
		public CompilationState(
			INamedTypeSymbol? taskType,
			INamedTypeSymbol? taskOfTType,
			INamedTypeSymbol? valueTaskType,
			INamedTypeSymbol? valueTaskOfTType,
			INamedTypeSymbol? asyncEnumerableType,
			INamedTypeSymbol? cancellationTokenType)
		{
			this.TaskType = taskType;
			this.TaskOfTType = taskOfTType;
			this.ValueTaskType = valueTaskType;
			this.ValueTaskOfTType = valueTaskOfTType;
			this.AsyncEnumerableType = asyncEnumerableType;
			this.CancellationTokenType = cancellationTokenType;
		}

		public INamedTypeSymbol? TaskType { get; }
		public INamedTypeSymbol? TaskOfTType { get; }
		public INamedTypeSymbol? ValueTaskType { get; }
		public INamedTypeSymbol? ValueTaskOfTType { get; }
		public INamedTypeSymbol? AsyncEnumerableType { get; }
		public INamedTypeSymbol? CancellationTokenType { get; }
	}
}
