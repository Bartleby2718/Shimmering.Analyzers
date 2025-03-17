using Shimmering.Analyzers.Utilities;

namespace Shimmering.Analyzers.UsageRules.ArrayOrArrayReturningMethodFollowedByToArray;

/// <summary>
/// Reports instances of an array or an array-returning method immediately followed by <see cref="Enumerable.ToArray"/>, as in myString.Split(...).ToArray().
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ArrayOrArrayReturningMethodFollowedByToArrayAnalyzer : ShimmeringSyntaxNodeAnalyzer
{
	private const string Title = "An array creation like new[] { 1 } or array-returning method like String.Split() must not be followed by .ToArray()";
	private const string Message = ".ToArray() is redundant";
	private const string Category = "Usage";

	private static readonly DiagnosticDescriptor Rule = CreateRule(
		DiagnosticIds.UsageRules.ArrayOrArrayReturningMethodFollowedByToArray,
		Title,
		Message,
		Category,
		DiagnosticSeverity.Warning,
		isEnabledByDefault: true);

	public override string SampleCode => """
		using System.Linq;

		namespace Tests
		{
			class Test
			{
				void Do()
				{
					var array = [|"a".Split(' ').ToArray()|];
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
		var invocation = (InvocationExpressionSyntax)context.Node;

		if (!EnumerableHelpers.IsLinqExtensionMethodCall(context.SemanticModel, invocation, context.CancellationToken, out var methodName)) { return; }
		if (methodName != nameof(Enumerable.ToArray)) { return; }

		var memberAccess = (MemberAccessExpressionSyntax)invocation.Expression;
		var innerExpression = memberAccess.Expression;
		if (innerExpression is ArrayCreationExpressionSyntax or ImplicitArrayCreationExpressionSyntax)
		{
			context.ReportDiagnostic(Diagnostic.Create(Rule, invocation.GetLocation()));
		}
		else if (innerExpression is InvocationExpressionSyntax innerInvocation)
		{
			var innerSymbolInfo = context.SemanticModel.GetSymbolInfo(innerInvocation, context.CancellationToken);
			if (innerSymbolInfo.Symbol is not IMethodSymbol methodSymbol) { return; }

			if (methodSymbol.ReturnType is IArrayTypeSymbol)
			{
				context.ReportDiagnostic(Diagnostic.Create(Rule, invocation.GetLocation()));
			}
		}
	}
}
