using Shimmering.Analyzers.Core;
using Shimmering.Analyzers.Utilities;

namespace Shimmering.Analyzers.Analyzers.Usage;

/// <summary>
/// Reports instances of a list or a list-returning method immediately followed by <see cref="Enumerable.ToList"/>, as in myString.Split(...).ToList().
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ListOrListReturningMethodFollowedByToListAnalyzer : ShimmeringAnalyzer
{
	private const string Title = "A list creation expression or list-returning method should not be followed by .ToList()";
	private const string Message = ".ToList() is redundant";
	private const string Category = RuleCategories.Usage;

	private static readonly DiagnosticDescriptor Rule = ShimmeringRuleFactory.Create(
		DiagnosticIds.UsageRules.ListOrListReturningMethodFollowedByToList,
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
				List<int> MyList = [|new List<int>().ToList()|];
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
		if (methodName != nameof(Enumerable.ToList)) { return; }
		if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess) { return; }
		var innerExpression = memberAccess.Expression;
		if (innerExpression is ObjectCreationExpressionSyntax objectCreationExpressionSyntax)
		{
			if (objectCreationExpressionSyntax.Type is GenericNameSyntax genericName
				&& genericName.Identifier.Text == nameof(List<int>)
				&& genericName.TypeArgumentList.Arguments.Count == 1)
			{
				context.ReportDiagnostic(Diagnostic.Create(Rule, invocation.GetLocation()));
			}
		}
		else if (innerExpression is InvocationExpressionSyntax innerInvocation)
		{
			var typeSymbol = context.SemanticModel.GetTypeInfo(innerInvocation, context.CancellationToken).Type;

			if (typeSymbol is INamedTypeSymbol namedType
				&& namedType.OriginalDefinition.ToDisplayString() == "System.Collections.Generic.List<T>"
				&& namedType.TypeArguments.Length == 1)
			{
				context.ReportDiagnostic(Diagnostic.Create(Rule, invocation.GetLocation()));
			}
		}
	}
}
