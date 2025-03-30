using Shimmering.Analyzers.Utilities;

namespace Shimmering.Analyzers.UsageRules.MisusedOrDefault;

/// <summary>
/// Reports instances of a 'OrDefault' LINQ methods that are followed by the null-forgiving operator (!).
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class MisusedOrDefaultAnalyzer : ShimmeringSyntaxNodeAnalyzer
{
	private const string Title = "OrDefault()! is redundant";
	private const string Message = "Replace '{0}!' with '{1}'";
	private const string Category = "ShimmeringUsage";

	private static readonly DiagnosticDescriptor Rule = CreateRule(
		DiagnosticIds.UsageRules.MisusedOrDefault,
		Title,
		Message,
		Category,
		DiagnosticSeverity.Warning);

	public override string SampleCode => """
		using System.Linq;

		namespace Tests;
		class Test
		{
			static int[] array = [1];
			void Do()
			{
				var a = [|array.SingleOrDefault()!|];
			}
		}
		""";

	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

	public override void RegisterSyntaxNodeAction(AnalysisContext context)
	{
		context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.SuppressNullableWarningExpression);
	}

	private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
	{
		var suppressNode = (PostfixUnaryExpressionSyntax)context.Node;
		if (suppressNode.Operand is not InvocationExpressionSyntax invocation) { return; }
		if (!EnumerableHelpers.IsLinqMethodCall(context.SemanticModel, invocation, context.CancellationToken, out var methodName)) { return; }
		if (!MisusedOrDefaultHelpers.MethodMapping.TryGetValue(methodName, out var replacementMethodName)) { return; }

		var diagnostic = Diagnostic.Create(Rule, suppressNode.GetLocation(), methodName, replacementMethodName);
		context.ReportDiagnostic(diagnostic);
	}
}
