﻿using Shimmering.Analyzers.Utilities;

namespace Shimmering.Analyzers.UniqueNonSetCollection;

/// <summary>
/// Reports instances of .Distinct().ToList() and .Distinct().ToArray() that can be replaced with .ToHashSet().
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UniqueNonSetCollectionAnalyzer : DiagnosticAnalyzer
{
	private const string Title = "Use a set instead";
	private const string Message = "Prefer sets when uniqueness is required";
	private const string Category = "Refactoring";

	private static readonly DiagnosticDescriptor Rule = new(
		DiagnosticIds.UniqueNonSetCollection,
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
		if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess) { return; }
		if (!EnumerableHelpers.IsEnumerableMethodInSystemLinq(semanticModel, memberAccess, out var methodName)) { return; }
		if (methodName is not (nameof(Enumerable.ToArray) or nameof(Enumerable.ToList))) { return; }

		// the previous invocation must be .Distinct()
		if (memberAccess.Expression is not InvocationExpressionSyntax innerInvocation) { return; }
		if (innerInvocation.Expression is not MemberAccessExpressionSyntax innerMemberAccess) { return; }
		if (!EnumerableHelpers.IsEnumerableMethodInSystemLinq(semanticModel, innerMemberAccess, out var innerMethodName)) { return; }
		if (innerMethodName is not nameof(Enumerable.Distinct)) { return; }

		// Technically, we shouldn't flag when ToHashSet() can cause a compilation failure.
		// However, still flagging to promote a best practice

		context.ReportDiagnostic(Diagnostic.Create(Rule, invocation.GetLocation()));
	}
}
