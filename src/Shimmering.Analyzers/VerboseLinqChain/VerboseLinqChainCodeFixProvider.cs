using Shimmering.Analyzers.Utilities;

namespace Shimmering.Analyzers.VerboseLinqChain;

/// <summary>
/// Converts a chain of LINQ calls with a collection expression.if reported by <see cref="VerboseLinqChainAnalyzer"/>.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(VerboseLinqChainCodeFixProvider)), Shared]
internal sealed class VerboseLinqChainCodeFixProvider : CodeFixProvider
{
	private const string Title = "Replace a verbose LINQ chain with a collection expression";

	public sealed override ImmutableArray<string> FixableDiagnosticIds =>
		[DiagnosticIds.VerboseLinqChain];

	public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

	public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
	{
		var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
		if (root == null) { return; }

		var diagnostic = context.Diagnostics.First();
		var diagnosticSpan = diagnostic.Location.SourceSpan;
		var node = root.FindNode(diagnosticSpan);

		var invocation = node
			.DescendantNodesAndSelf()
			.OfType<InvocationExpressionSyntax>()
			.FirstOrDefault();
		if (invocation == null) { return; }

		context.RegisterCodeFix(
			CodeAction.Create(
				Title,
				ct => ConvertToCollectionExpressionAsync(context.Document, invocation, ct),
				nameof(VerboseLinqChainCodeFixProvider)),
			diagnostic);
	}

	private static async Task<Document> ConvertToCollectionExpressionAsync(
		Document document, InvocationExpressionSyntax lastInvocation, CancellationToken cancellationToken)
	{
		var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
		if (semanticModel == null) { return document; }

		if (!VerboseLinqChainHelpers.TryConstructCollectionExpression(semanticModel, lastInvocation, out var collectionExpression)
			|| collectionExpression == null)
		{
			return document;
		}

		var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
		if (root is not CompilationUnitSyntax compilationUnit) { return document; }

		if (lastInvocation.Parent?.Parent?.Parent is not VariableDeclarationSyntax variableDeclaration) { return document; }

		var declaredTypeSymbol = semanticModel.GetTypeInfo(variableDeclaration.Type, cancellationToken).Type;
		if (declaredTypeSymbol == null) { return document; }

		var resolvedTypeSyntax = SyntaxFactory.ParseTypeName(
			declaredTypeSymbol.WithNullableAnnotation(NullableAnnotation.NotAnnotated)
				.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat))
			.WithTriviaFrom(variableDeclaration.Type);
		var newVariableDeclaration = variableDeclaration
			.WithType(resolvedTypeSyntax)
			.WithVariables(SyntaxFactory.SingletonSeparatedList(
		variableDeclaration.Variables[0].WithInitializer(SyntaxFactory.EqualsValueClause(collectionExpression))));

		var newRootWithUpdatedVariableDeclaration = compilationUnit.ReplaceNode(variableDeclaration, newVariableDeclaration);
		var newRootWithUsingDirectives = EnsureNecessaryUsingDirectivesExist(newRootWithUpdatedVariableDeclaration, declaredTypeSymbol);
		return document.WithSyntaxRoot(newRootWithUsingDirectives);
	}

	private static CompilationUnitSyntax EnsureNecessaryUsingDirectivesExist(CompilationUnitSyntax compilationUnit, ITypeSymbol? typeSymbol)
	{
		var containingNamespace = typeSymbol?.ContainingNamespace?.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat);
		// No need to add the using directive if the type is not System.Collections.Generic (e.g. array)
		if (containingNamespace != FullyQualifiedNamespaces.SystemCollectionsGeneric
			// or if the using directive already exists
			|| compilationUnit.Usings.Any(u => u.Name?.ToString() == FullyQualifiedNamespaces.SystemCollectionsGeneric))
		{
			return compilationUnit;
		}

		var newUsing = SyntaxFactory.UsingDirective(
			SyntaxFactory.ParseName(FullyQualifiedNamespaces.SystemCollectionsGeneric)
				.WithLeadingTrivia(SyntaxFactory.Space))
				.WithTrailingTrivia(SyntaxFactory.CarriageReturnLineFeed);
		return compilationUnit.AddUsings(newUsing);
	}
}
