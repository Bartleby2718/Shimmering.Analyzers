using Shimmering.Analyzers.Core;
using Shimmering.Analyzers.Utilities;

namespace Shimmering.Analyzers.UsageRules.SingleElementConcat;

/// <summary>
/// Reports instances of `.Concat(new[] { e })` or `.Concat(new List&lt;T&gt; { e })`.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class SingleElementConcatAnalyzer : Core.ShimmeringAnalyzer
{
	private const string Title = "Do not concat a single element";
	private const string Message = "Replace .Concat([e]) with .Append(e)";
	private const string Category = "ShimmeringUsage";

	private static readonly DiagnosticDescriptor Rule = RuleFactory.Create(
		DiagnosticIds.UsageRules.SingleElementConcat,
		Title,
		Message,
		Category,
		DiagnosticSeverity.Info);

	public override string SampleCode => """
		using System.Linq;

		namespace Tests;
		class Test
		{
			void Do()
			{
				_ = [|new[] { 1 }.Concat(new[] { 2 })|];
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
		if (context.Node is not InvocationExpressionSyntax invocation) { return; }

		if (!IsConcat(context.SemanticModel, invocation, context.CancellationToken)) { return; }

		var supportsCollectionExpressions = CsharpVersionHelpers.SupportsCollectionExpressions(context);
		if (SingleElementConcatHelpers.TryGetSingleElement(invocation, supportsCollectionExpressions, out _))
		{
			context.ReportDiagnostic(Diagnostic.Create(Rule, invocation.GetLocation()));
		}
	}

	private static bool IsConcat(SemanticModel semanticModel, InvocationExpressionSyntax invocation, CancellationToken cancellationToken)
	{
		if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
			return false;

		// validate name
		if (memberAccess.Name.Identifier.Text != nameof(Enumerable.Concat))
			return false;

		// validate that it's an extension method
		if (semanticModel.GetSymbolInfo(memberAccess, cancellationToken).Symbol is not IMethodSymbol methodSymbol)
			return false;
		if (methodSymbol.MethodKind != MethodKind.ReducedExtension)
			return false;

		// validate the containing class
		var containingClass = methodSymbol.ContainingType;
		if (containingClass.Name != nameof(Enumerable))
			return false;

		// validate the containing namespace
		return containingClass.ContainingNamespace.Name == nameof(System.Linq)
			&& containingClass.ContainingNamespace.ContainingNamespace.Name == nameof(System)
			&& containingClass.ContainingNamespace.ContainingNamespace.ContainingNamespace.IsGlobalNamespace;
	}
}
