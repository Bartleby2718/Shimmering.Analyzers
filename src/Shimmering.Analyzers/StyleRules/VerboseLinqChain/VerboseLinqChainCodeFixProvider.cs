using Shimmering.Analyzers.Utilities;

namespace Shimmering.Analyzers.StyleRules.VerboseLinqChain;

/// <summary>
/// Converts a chain of LINQ calls with a collection expression.if reported by <see cref="VerboseLinqChainAnalyzer"/>.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(VerboseLinqChainCodeFixProvider))]
public sealed class VerboseLinqChainCodeFixProvider : ShimmeringCodeFixProvider
{
	private const string Title = "Replace a verbose LINQ chain with a collection expression";

	public sealed override ImmutableArray<string> FixableDiagnosticIds =>
		[DiagnosticIds.StyleRules.VerboseLinqChain];

	public override string SampleCodeFixed => """
		using System.Linq;

		namespace Tests;
		class Test
		{
			static int[] array1 = [0, 1];
			void Do()
			{
				int[] array2 = [3, .. array1, 2];
			}
		}
		""";

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

		if (!VerboseLinqChainHelpers.TryConstructCollectionExpression(semanticModel, lastInvocation, cancellationToken, out var collectionExpression)
			|| collectionExpression == null)
		{
			return document;
		}

		var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
		if (root is not CompilationUnitSyntax compilationUnit) { return document; }

		var parent = lastInvocation.Parent;

		// Case 1: If the invocation is an argument, no special treatment is needed.
		if (parent is ArgumentSyntax argument
			&& argument.Parent is ArgumentListSyntax argumentList
			&& argumentList.Parent is InvocationExpressionSyntax)
		{
			var newArgument = argument.WithExpression(collectionExpression)
				.WithTriviaFrom(argument);
			var newRoot = root.ReplaceNode(argument, newArgument);
			return document.WithSyntaxRoot(newRoot);
		}
		// If the invocation is a variable declaration, we need to check whether the type is explicit or implicit.
		else if (parent is EqualsValueClauseSyntax equalsValueClause
			&& equalsValueClause.Parent is VariableDeclaratorSyntax variableDeclarator
			&& variableDeclarator.Parent is VariableDeclarationSyntax variableDeclaration)
		{
			var newEqualsValueClause = equalsValueClause.WithValue(collectionExpression)
				 .WithTriviaFrom(equalsValueClause);

			// Case 2: If the variable declaration uses an explicit type, no special treatment is needed.
			if (!variableDeclaration.Type.IsVar)
			{
				var newRoot = root.ReplaceNode(equalsValueClause, newEqualsValueClause);
				return document.WithSyntaxRoot(newRoot);
			}

			// Case 3: If the variable declaration uses an implicit
			// Replace 'var' with the explicit type.
			var declaredTypeSymbol = semanticModel.GetTypeInfo(variableDeclaration.Type, cancellationToken).Type;
			if (declaredTypeSymbol == null) { return document; }

			var resolvedTypeSyntax = SyntaxFactory.ParseTypeName(
				declaredTypeSymbol.WithNullableAnnotation(NullableAnnotation.NotAnnotated)
					.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat))
				.WithTriviaFrom(variableDeclaration.Type);
			// Safe to handle just the first variable, considering error CS0819 (Implicitly-typed variables cannot have multiple declarators)
			var newVariableDeclaration = variableDeclaration
				.WithType(resolvedTypeSyntax)
				.WithVariables(SyntaxFactory.SingletonSeparatedList(
					variableDeclaration.Variables[0].WithInitializer(newEqualsValueClause)));

			var newRootWithUpdatedVariableDeclaration = compilationUnit.ReplaceNode(variableDeclaration, newVariableDeclaration);

			var containingNamespace = declaredTypeSymbol?.ContainingNamespace?.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat);
			// No need to add the using directive if the type is not System.Collections.Generic (e.g. array)
			var newRootWithUsingDirectives = containingNamespace != FullyQualifiedNamespaces.SystemCollectionsGeneric
				// or if the using directive already exists
				|| compilationUnit.Usings.Any(u => u.Name?.ToString() == FullyQualifiedNamespaces.SystemCollectionsGeneric)
				? newRootWithUpdatedVariableDeclaration
				: CodeFixHelpers.EnsureUsingDirectivesExist(
					document,
					newRootWithUpdatedVariableDeclaration,
					namespaces: [FullyQualifiedNamespaces.SystemCollectionsGeneric]);

			return document.WithSyntaxRoot(newRootWithUsingDirectives);
		}

		// Shouldn't have got here, but return the same document just in case.
		return document;
	}

	private static bool IsVariableDeclaration(InvocationExpressionSyntax invocation)
	{
		return invocation.Parent is EqualsValueClauseSyntax equalsValueClause
			&& equalsValueClause.Parent is VariableDeclaratorSyntax variableDeclarator
			&& variableDeclarator.Parent is VariableDeclarationSyntax;
	}
}
