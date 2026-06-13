using Microsoft.CodeAnalysis.Operations;
using Shimmering.Analyzers.Core;
using Shimmering.Analyzers.Utilities;

namespace Shimmering.Analyzers.Analyzers.Usage;

/// <summary>
/// Reports instances of string.Split() followed by Enumerable.Select(x => x.Trim()) where StringSplitOptions.TrimEntries could have been used instead.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UseTrimEntriesAnalyzer : ShimmeringAnalyzer
{
	private const string Title = "Use StringSplitOptions.TrimEntries";
	private const string Message = "Use the overload of string.Split with StringSplitOptions.TrimEntries to trim entries";
	private const string Category = RuleCategories.Usage;

	private static readonly DiagnosticDescriptor Rule = ShimmeringRuleFactory.Create(
		DiagnosticIds.UsageRules.UseTrimEntries,
		Title,
		Message,
		Category,
		DiagnosticSeverity.Info);

	public override string SampleCode => """
		using System;
		using System.Linq;

		namespace Tests;
		class Test
		{
			void Do(string input)
			{
				var x = [|input.Split(',')
					.Select(x => x.Trim())|];
			}
		}
		""";

	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

	protected override void InitializeCore(AnalysisContext context)
	{
		context.RegisterOperationAction(AnalyzeInvocation, OperationKind.Invocation);
	}

	private static void AnalyzeInvocation(OperationAnalysisContext context)
	{
		var invocation = (IInvocationOperation)context.Operation;
		var method = invocation.TargetMethod;

		if (method == null
			|| method.Name != nameof(Enumerable.Select)
			|| method.ContainingType?.ToDisplayString() != "System.Linq.Enumerable"
			|| invocation.Arguments.Length != 2)
		{
			return;
		}

		// 2. The source enumerable should be the first argument to Select (as an extension method, it is argument 0)
		var sourceArgument = invocation.Arguments[0].Value;
		if (sourceArgument is IConversionOperation conversion)
		{
			sourceArgument = conversion.Operand;
		}

		if (sourceArgument is not IInvocationOperation sourceInvocation)
		{
			return;
		}

		var sourceMethod = sourceInvocation.TargetMethod;
		if (sourceMethod.Name != nameof(string.Split)
			|| sourceMethod.ContainingType?.SpecialType != SpecialType.System_String
			|| sourceInvocation.Syntax is not InvocationExpressionSyntax splitSyntax
			|| splitSyntax.ArgumentList.Arguments.Count != 1)
		{
			return;
		}

		// 3. Check if the compilation has StringSplitOptions.TrimEntries
		var stringSplitOptionsType = context.Compilation.GetTypeByMetadataName("System.StringSplitOptions");
		if (stringSplitOptionsType == null || !stringSplitOptionsType.MemberNames.Contains("TrimEntries"))
		{
			return;
		}

		// 4. Check if the second argument to Select is x => x.Trim()
		var delegateArgument = invocation.Arguments[1].Value;
		if (delegateArgument is IConversionOperation delegateConversion)
		{
			delegateArgument = delegateConversion.Operand;
		}

		if (delegateArgument is not IDelegateCreationOperation delegateCreation)
		{
			return;
		}

		if (delegateCreation.Target is not IAnonymousFunctionOperation anonymousFunction)
		{
			return;
		}

		// We expect the body of the lambda to be exactly `return parameter.Trim();` or an expression `parameter.Trim()`.
		// In IOperation, an expression body lambda has a Block with a single IReturnOperation.
		if (anonymousFunction.Body.Operations.Length != 1)
		{
			return;
		}

		var operationInLambda = anonymousFunction.Body.Operations[0];
		if (operationInLambda is not IReturnOperation returnOperation
			|| returnOperation.ReturnedValue is not IInvocationOperation trimInvocation)
		{
			return;
		}

		var trimMethod = trimInvocation.TargetMethod;
		if (trimMethod.Name != nameof(string.Trim)
			|| trimMethod.ContainingType?.SpecialType != SpecialType.System_String
			|| trimInvocation.Arguments.Length != 0) // Trim() with no arguments
		{
			return;
		}

		var trimInstance = trimInvocation.Instance;
		if (trimInstance is not IParameterReferenceOperation parameterReference
			|| anonymousFunction.Symbol.Parameters.Length != 1
			|| !SymbolEqualityComparer.Default.Equals(parameterReference.Parameter, anonymousFunction.Symbol.Parameters[0]))
		{
			return;
		}

		// 5. If it is `.ToArray()`, we want to flag the ToArray call as well, similar to MissingRemoveEmptyEntriesAnalyzer.
		var invocationToFlag = invocation.Syntax;
		if (invocation.Parent is IArgumentOperation parentArgument
			&& parentArgument.Parent is IInvocationOperation parentInvocation
			&& parentInvocation.TargetMethod.Name == nameof(Enumerable.ToArray)
			&& parentInvocation.TargetMethod.ContainingType?.ToDisplayString() == "System.Linq.Enumerable")
		{
			invocationToFlag = parentInvocation.Syntax;
		}

		var diagnostic = Diagnostic.Create(Rule, invocationToFlag.GetLocation());
		context.ReportDiagnostic(diagnostic);
	}
}
