using Shimmering.Analyzers.Utilities;

namespace Shimmering.Analyzers.UsageRules.ToListForEach;

/// <summary>
/// Reports stances of ToList().ForEach().
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ToListForEachAnalyzer : ShimmeringSyntaxNodeAnalyzer
{
	private const string Title = "ToList().ForEach() causes unnecessary memory allocation";
	private const string Message = "Replace ToList().ForEach() with a foreach loop to reduce memory usage";
	private const string Category = "Usage";

	private static readonly DiagnosticDescriptor Rule = CreateRule(
		DiagnosticIds.UsageRules.ToListForEach,
		Title,
		Message,
		Category,
		DiagnosticSeverity.Warning,
		isEnabledByDefault: true);

	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

	public override void RegisterSyntaxNodeAction(AnalysisContext context)
	{
		context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
	}

	private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
	{
		var invocation = (InvocationExpressionSyntax)context.Node;

		// the last invocation should be ForEach()
		if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess
			|| !ListHelpers.IsListInstanceMethodCall(context.SemanticModel, invocation, out var methodName)
			|| methodName != nameof(List<int>.ForEach))
		{
			return;
		}

		// the target of ForEach should be ToList()
		if (memberAccess.Expression is not InvocationExpressionSyntax toListInvocation
			|| !EnumerableHelpers.IsLinqExtensionMethodCall(context.SemanticModel, toListInvocation, out var innerMethodName)
			|| innerMethodName != nameof(Enumerable.ToList))
		{
			return;
		}

		if (invocation.FirstAncestorOrSelf<ExpressionStatementSyntax>() is null) { return; }

		var diagnostic = Diagnostic.Create(Rule, invocation.GetLocation());
		context.ReportDiagnostic(diagnostic);
	}
}
