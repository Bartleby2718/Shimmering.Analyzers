using System.Collections.Immutable;
using System.Linq;
using System.Threading;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Shimmering.Analyzers.Core;
using Shimmering.Analyzers.Utilities;

namespace Shimmering.Analyzers.Analyzers.Usage;

/// <summary>
/// Reports instances of .Distinct().ToList() and .Distinct().ToArray() that can be replaced with .ToHashSet().
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UniqueNonSetCollectionAnalyzer : ShimmeringAnalyzer
{
	private const string Title = "Use a set instead";
	private const string Message = "Prefer sets when uniqueness is required";
	private const string Category = RuleCategories.Usage;

	private static readonly DiagnosticDescriptor Rule = ShimmeringRuleFactory.Create(
		DiagnosticIds.UsageRules.UniqueNonSetCollection,
		Title,
		Message,
		Category,
		DiagnosticSeverity.Hidden);

	public override string SampleCode => """
		using System.Collections.Generic;
		using System.Linq;

		namespace Tests;

		class Test
		{
			void Do()
			{
				List<int> numbers = [];
				var distinctNumbers = [|numbers.Distinct().ToArray()|];
			}
		}
		""";

	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

	protected override void InitializeCore(AnalysisContext context)
	{
		context.RegisterCompilationStartAction(compilationContext =>
		{
			var hashSetSymbol = compilationContext.Compilation.GetTypeByMetadataName("System.Collections.Generic.HashSet`1");
			if (hashSetSymbol == null)
			{
				return;
			}

			compilationContext.RegisterSyntaxNodeAction(syntaxContext => AnalyzeInvocation(syntaxContext, hashSetSymbol), SyntaxKind.InvocationExpression);
		});
	}

	private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context, INamedTypeSymbol hashSetSymbol)
	{
		var semanticModel = context.SemanticModel;
		var invocation = (InvocationExpressionSyntax)context.Node;

		// bail out if it's not a terminal node
		if (invocation.Parent is MemberAccessExpressionSyntax
			or InvocationExpressionSyntax
			or ConditionalAccessExpressionSyntax)
		{
			return;
		}

		// the invocation must be .ToArray() or .ToList()
		if (!EnumerableHelpers.IsLinqMethodCall(semanticModel, invocation, context.CancellationToken, out var methodName))
		{
			return;
		}

		if (methodName is not (nameof(Enumerable.ToArray) or nameof(Enumerable.ToList)))
		{
			return;
		}

		if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
		{
			return;
		}

		// the previous invocation must be .Distinct()
		if (memberAccess.Expression is not InvocationExpressionSyntax innerInvocation)
		{
			return;
		}

		if (!EnumerableHelpers.IsLinqMethodCall(semanticModel, innerInvocation, context.CancellationToken, out var innerMethodName))
		{
			return;
		}

		if (innerMethodName is not nameof(Enumerable.Distinct))
		{
			return;
		}

		// Exclude the overload that accepts a comparer argument until https://github.com/Bartleby2718/Shimmering.Analyzers/issues/91 is done
		if (innerInvocation.ArgumentList.Arguments.Count != 0)
		{
			return;
		}

		if (semanticModel.GetSymbolInfo(invocation, context.CancellationToken).Symbol is not IMethodSymbol methodSymbol)
		{
			return;
		}

		var elementType = methodSymbol.TypeArguments.FirstOrDefault();
		if (elementType == null)
		{
			return;
		}

		if (!IsTargetTypeCompatible(semanticModel, invocation, elementType, hashSetSymbol, context.CancellationToken))
		{
			return;
		}

		context.ReportDiagnostic(Diagnostic.Create(Rule, invocation.GetLocation()));
	}

	private static bool IsTargetTypeCompatible(
		SemanticModel semanticModel,
		InvocationExpressionSyntax invocation,
		ITypeSymbol elementType,
		INamedTypeSymbol hashSetSymbol,
		CancellationToken cancellationToken)
	{
		var typeInfo = semanticModel.GetTypeInfo(invocation, cancellationToken);
		var convertedType = typeInfo.ConvertedType;
		if (convertedType == null || convertedType is IErrorTypeSymbol)
		{
			return false;
		}

		// If the parent is a var variable declaration, it is always compatible.
		var variableDeclaration = invocation.Parent?.Parent?.Parent as VariableDeclarationSyntax;
		if (variableDeclaration != null && variableDeclaration.Type.IsVar)
		{
			return true;
		}

		var constructedHashSetType = hashSetSymbol.Construct(elementType);
		var conversion = semanticModel.Compilation.ClassifyConversion(constructedHashSetType, convertedType);
		return conversion.Exists && (conversion.IsImplicit || conversion.IsIdentity);
	}
}
