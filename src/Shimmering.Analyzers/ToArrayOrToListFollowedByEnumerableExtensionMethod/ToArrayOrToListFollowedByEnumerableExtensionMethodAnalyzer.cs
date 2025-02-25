using Shimmering.Analyzers.Utilities;

namespace Shimmering.Analyzers.ToArrayOrToListFollowedByEnumerableExtensionMethod;

/// <summary>
/// Reports instances of LINQ materialization immediately followed by another Enumerable extension method.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
internal sealed class ToArrayOrToListFollowedByEnumerableExtensionMethodAnalyzer : ShimmeringSyntaxNodeAnalyzer
{
	private const string Title = "Unnecessary materialization to array/list in LINQ chain";
	private const string Message = "Remove unnecessary materialization to an array or a list";
	private const string Category = "Usage";

	private static readonly DiagnosticDescriptor Rule = CreateRule(
		DiagnosticIds.ToArrayOrToListFollowedByEnumerableExtensionMethod,
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

		// Check if the invocation is a member access like "something.ToList()"
		if (!EnumerableHelpers.IsLinqExtensionMethodCall(context.SemanticModel, invocation, out var methodName)
			|| methodName is not (nameof(Enumerable.ToList) or nameof(Enumerable.ToArray)))
		{
			return;
		}

		// The materialization call must be immediately followed by another call.
		// For example, in: source.ToList().Where(...), the outer of the materialization invocation is a member access
		if (invocation.Parent is not MemberAccessExpressionSyntax outerMemberAccess
			|| outerMemberAccess.Parent is not InvocationExpressionSyntax outerInvocation)
		{
			return;
		}

		// Check if the outer invocation is also a LINQ extension method
		// TODO: What about List instance methods and Enumerable extension methods that have the same name? (Contains, Reverse, ToArray)
		if (!EnumerableHelpers.IsLinqExtensionMethodCall(context.SemanticModel, outerInvocation, out var parentMethodName))
		{
			return;
		}

		var memberAccess = (MemberAccessExpressionSyntax)invocation.Expression;
		var diagnostic = Diagnostic.Create(Rule, memberAccess.Name.GetLocation());
		context.ReportDiagnostic(diagnostic);
	}
}
