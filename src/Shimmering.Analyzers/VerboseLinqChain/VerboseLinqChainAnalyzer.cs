namespace Shimmering.Analyzers.VerboseLinqChain;

/// <summary>
/// Reports instances of a verbose chain of <see cref="Enumerable.Concat{TSource}(IEnumerable{TSource}, IEnumerable{TSource})"/>s and <see cref="Enumerable.Append"/>s, ending with with a <see cref="Enumerable.ToArray{TSource}(IEnumerable{TSource})"/> or a <see cref="Enumerable.ToList{TSource}(IEnumerable{TSource})"/>.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
internal sealed class VerboseLinqChainAnalyzer : DiagnosticAnalyzer
{
	private const string Title = "Simplify LINQ chain";
	private const string Message = "Replace a verbose LINQ chain with a collection expression";
	private const string Category = "Refactoring";

	private static readonly DiagnosticDescriptor Rule = new(
		DiagnosticIds.VerboseLinqChain,
		Title,
		Message,
		Category,
		DiagnosticSeverity.Info,
		isEnabledByDefault: true,
		helpLinkUri: $"https://github.com/Bartleby2718/Shimmering.Analyzers/blob/main/docs/{DiagnosticIds.VerboseLinqChain}.md");

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

		// look for declarations of the form:
		//    var x = <chain of Concats and Appends and Prepends).ToArray(); (or ToList() or ToHashSet())
		if (!IsVariableDeclaration(invocation)) { return; }

		if (!VerboseLinqChainHelpers.TryConstructCollectionExpression(semanticModel, invocation, doConstructCollectionExpression: false, out _)) { return; }

		var diagnostic = Diagnostic.Create(Rule, invocation.GetLocation());
		context.ReportDiagnostic(diagnostic);
	}

	private static bool IsVariableDeclaration(InvocationExpressionSyntax invocation)
	{
		return invocation.Parent is EqualsValueClauseSyntax equalsValueClause
			&& equalsValueClause.Parent is VariableDeclaratorSyntax variableDeclarator
			&& variableDeclarator.Parent is VariableDeclarationSyntax;
	}
}
