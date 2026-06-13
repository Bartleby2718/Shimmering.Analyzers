using System.Linq;
using Microsoft.CodeAnalysis;

using Shimmering.Analyzers.Core;
using Shimmering.Analyzers.Utilities;

namespace Shimmering.Analyzers.Analyzers.Usage;

/// <summary>
/// Reports instances of a HashSet or a HashSet-returning method immediately followed by Enumerable.ToHashSet, as in GetHashSet().ToHashSet().
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class HashSetOrHashSetReturningMethodFollowedByToHashSetAnalyzer : ShimmeringAnalyzer
{
	private const string Title = "A HashSet creation expression, identifier, or HashSet-returning method should not be followed by .ToHashSet()";
	private const string Message = ".ToHashSet() is redundant";
	private const string Category = RuleCategories.Usage;

	private static readonly DiagnosticDescriptor Rule = ShimmeringRuleFactory.Create(
		DiagnosticIds.UsageRules.HashSetOrHashSetReturningMethodFollowedByToHashSet,
		Title,
		Message,
		Category,
		DiagnosticSeverity.Warning);

	public override string SampleCode => """
		using System.Collections.Generic;
		using System.Linq;

		namespace Tests;
		class Test
		{
			void Do()
			{
				HashSet<int> MyHashSet = [|new HashSet<int>().ToHashSet()|];
			}
		}
		""";

	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

	protected override void InitializeCore(AnalysisContext context)
	{
		context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
	}

	private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
	{
		var invocation = (InvocationExpressionSyntax)context.Node;

		if (!EnumerableHelpers.IsLinqMethodCall(context.SemanticModel, invocation, context.CancellationToken, out var methodName)) { return; }
		if (methodName != "ToHashSet") { return; }
		if (invocation.ArgumentList.Arguments.Count > 0) { return; }
		if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess) { return; }
		var innerExpression = memberAccess.Expression;
		var typeSymbol = context.SemanticModel.GetTypeInfo(innerExpression, context.CancellationToken).Type;

		if (typeSymbol is INamedTypeSymbol namedType
			&& namedType.OriginalDefinition.ToDisplayString() == "System.Collections.Generic.HashSet<T>"
			&& namedType.TypeArguments.Length == 1)
		{
			context.ReportDiagnostic(Diagnostic.Create(Rule, invocation.GetLocation()));
		}
	}
}
