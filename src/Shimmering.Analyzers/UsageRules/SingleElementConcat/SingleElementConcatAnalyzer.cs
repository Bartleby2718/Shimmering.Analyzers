using Shimmering.Analyzers.Utilities;

namespace Shimmering.Analyzers.UsageRules.SingleElementConcat;

/// <summary>
/// Reports instances of calling <see cref="Enumerable.Concat"/> against a single-element collection that can be replaced with <see cref="Enumerable.Append"/>.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class SingleElementConcatAnalyzer : ShimmeringSyntaxNodeAnalyzer
{
	private const string Title = "Simplify .Concat()";
	private const string Message = "Replace .Concat([e]) with .Append(e)";
	private const string Category = "Usage";

	private static readonly DiagnosticDescriptor Rule = CreateRule(
		DiagnosticIds.UsageRules.SingleElementConcat,
		Title,
		Message,
		Category,
		DiagnosticSeverity.Info,
		isEnabledByDefault: true);

	public override string SampleCode => """
		using System;
		using System.Linq;

		namespace Tests
		{
			class Test
			{
				void Do()
				{
					var result = [|new[] { 1, 2 }.Concat(new[] { 3 })|];
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
