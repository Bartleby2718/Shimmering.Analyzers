namespace Shimmering.Analyzers.StyleRules.VerboseLinqChain;

/// <summary>
/// Reports instances of a verbose chain of <see cref="Enumerable.Concat{TSource}(IEnumerable{TSource}, IEnumerable{TSource})"/>s and <see cref="Enumerable.Append"/>s, ending with with a <see cref="Enumerable.ToArray{TSource}(IEnumerable{TSource})"/> or a <see cref="Enumerable.ToList{TSource}(IEnumerable{TSource})"/>.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class VerboseLinqChainAnalyzer : ShimmeringSyntaxNodeAnalyzer
{
	private const string Title = "Simplify LINQ chain";
	private const string Message = "Replace a verbose LINQ chain with a collection expression";
	private const string Category = "ShimmeringStyle";

	private static readonly DiagnosticDescriptor Rule = CreateRule(
		DiagnosticIds.StyleRules.VerboseLinqChain,
		Title,
		Message,
		Category,
		DiagnosticSeverity.Info);

	public override string SampleCode => """
		using System.Linq;

		namespace Tests;
		class Test
		{
			static int[] array1 = [0, 1];
			static int[] array2 = [5];
			void Do()
			{
				var array3 = [|array1.Append(2).Prepend(3).Concat(array2).ToArray()|];
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

		var parent = invocation.Parent;
		var isArgumentToInvocation = parent is ArgumentSyntax argument
			&& argument.Parent is ArgumentListSyntax argumentList
			&& argumentList.Parent is InvocationExpressionSyntax;
		var isVariableDeclaration = parent is EqualsValueClauseSyntax equalsValueClause
			&& equalsValueClause.Parent is VariableDeclaratorSyntax variableDeclarator
			&& variableDeclarator.Parent is VariableDeclarationSyntax;
		// Only argument and variable declaration are supported. (e.g. tuple expression is not touched)
		if (!isArgumentToInvocation && !isVariableDeclaration) { return; }

		if (!VerboseLinqChainHelpers.TryConstructCollectionExpression(semanticModel, invocation, context.CancellationToken, out _)) { return; }

		var diagnostic = Diagnostic.Create(Rule, invocation.GetLocation());
		context.ReportDiagnostic(diagnostic);
	}
}
