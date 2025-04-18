using Shimmering.Analyzers.Utilities;

namespace Shimmering.Analyzers.UsageRules.ToListForEach;

/// <summary>
/// Reports stances of .ToList().ForEach().
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ToListForEachAnalyzer : ShimmeringSyntaxNodeAnalyzer
{
	private const string Title = ".ToList().ForEach() causes unnecessary memory allocation";
	private const string Message = "Replace .ToList().ForEach() with a foreach loop to reduce memory usage";
	private const string Category = "ShimmeringUsage";

	private static readonly DiagnosticDescriptor Rule = CreateRule(
		DiagnosticIds.UsageRules.ToListForEach,
		Title,
		Message,
		Category,
		DiagnosticSeverity.Warning);

#pragma warning disable SA1027 // Use tabs correctly
	public override string SampleCode => """
        using System;
        using System.Linq;

        namespace Tests;
        class Test
        {
            void Do(int[] numbers)
            {
                [|numbers.ToList().ForEach(n => Console.WriteLine(n))|];
            }
        }
        """
#pragma warning restore SA1027 // Use tabs correctly
;

	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

	public override void RegisterSyntaxNodeAction(AnalysisContext context)
	{
		context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
	}

	private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
	{
		var invocation = (InvocationExpressionSyntax)context.Node;

		// the last invocation should be .ForEach()
		if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess
			|| !ListHelpers.IsListInstanceMethodCall(context.SemanticModel, invocation, context.CancellationToken, out var methodName)
			|| methodName != nameof(List<int>.ForEach))
		{
			return;
		}

		// the target of .ForEach should be .ToList()
		if (memberAccess.Expression is not InvocationExpressionSyntax toListInvocation
			|| !EnumerableHelpers.IsLinqMethodCall(context.SemanticModel, toListInvocation, context.CancellationToken, out var innerMethodName)
			|| innerMethodName != nameof(Enumerable.ToList))
		{
			return;
		}

		// Bail out if the receiver is an IQueryable<T> because removing materialization affects business logic
		if (toListInvocation.Expression is not MemberAccessExpressionSyntax innerMemberAccess
			|| AnalyzerHelpers.IsOrImplementsInterface(context, innerMemberAccess.Expression, FullyQualifiedTypeNames.IQueryableOfT))
		{
			return;
		}

		if (invocation.FirstAncestorOrSelf<ExpressionStatementSyntax>() is null) { return; }

		var diagnostic = Diagnostic.Create(Rule, invocation.GetLocation());
		context.ReportDiagnostic(diagnostic);
	}
}
