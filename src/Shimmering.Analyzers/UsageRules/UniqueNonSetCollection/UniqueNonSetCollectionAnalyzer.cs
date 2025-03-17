using Shimmering.Analyzers.Utilities;

namespace Shimmering.Analyzers.UsageRules.UniqueNonSetCollection;

/// <summary>
/// Reports instances of .Distinct().ToList() and .Distinct().ToArray() that can be replaced with .ToHashSet().
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UniqueNonSetCollectionAnalyzer : ShimmeringSyntaxNodeAnalyzer
{
	private const string Title = "Use a set instead";
	private const string Message = "Prefer sets when uniqueness is required";
	private const string Category = "Usage";

	private static readonly DiagnosticDescriptor Rule = CreateRule(
		DiagnosticIds.UsageRules.UniqueNonSetCollection,
		Title,
		Message,
		Category,
		DiagnosticSeverity.Info,
		isEnabledByDefault: false);

	public override string SampleCode => """
		using System;
		using System.Collections.Generic;
		using System.Linq;

		namespace Tests
		{
			class Test
			{
				void Do()
				{
					List<int> numbers = [];
					var distinctNumbers = [|numbers.Distinct().ToArray()|];
				}
			}
		}
		""";

	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

	public override void RegisterSyntaxNodeAction(AnalysisContext context)
	{
		context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
	}

	private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
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
		if (!EnumerableHelpers.IsLinqExtensionMethodCall(semanticModel, invocation, context.CancellationToken, out var methodName)) { return; }
		if (methodName is not (nameof(Enumerable.ToArray) or nameof(Enumerable.ToList))) { return; }
		if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess) { return; }

		// the previous invocation must be .Distinct()
		if (memberAccess.Expression is not InvocationExpressionSyntax innerInvocation) { return; }
		if (!EnumerableHelpers.IsLinqExtensionMethodCall(semanticModel, innerInvocation, context.CancellationToken, out var innerMethodName)) { return; }
		if (innerMethodName is not nameof(Enumerable.Distinct)) { return; }

		// Technically, we shouldn't flag when ToHashSet() can cause a compilation failure.
		// However, still flagging to promote a best practice

		context.ReportDiagnostic(Diagnostic.Create(Rule, invocation.GetLocation()));
	}
}
