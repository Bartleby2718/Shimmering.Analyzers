using Shimmering.Analyzers.Utilities;

namespace Shimmering.Analyzers.CodeFixes.Style;

/// <summary>
/// Replaces fully-qualified type references with their simple/nested names and adds the corresponding using directive.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ForbidFullyQualifiedTypeReferencesCodeFixProvider))]
[Shared]
public sealed class ForbidFullyQualifiedTypeReferencesCodeFixProvider : ShimmeringCodeFixProvider
{
	private const string Title = "Use using directive and simplify type name";

	public sealed override ImmutableArray<string> FixableDiagnosticIds =>
		[DiagnosticIds.StyleRules.ForbidFullyQualifiedTypeReferences];

	public override string SampleCodeFixed => """
		using System.Text;

		namespace Tests
		{
			class Test
			{
				void Method()
				{
					Encoding.UTF8.GetBytes("test");
				}
			}
		}
		""";

	public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
	{
		var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
		if (root == null)
		{
			return;
		}

		var diagnostic = context.Diagnostics.First();
		var diagnosticSpan = diagnostic.Location.SourceSpan;

		var node = root.FindNode(diagnosticSpan);
		var targetNode = node.DescendantNodesAndSelf()
			.FirstOrDefault(descendant => descendant is QualifiedNameSyntax || descendant is MemberAccessExpressionSyntax || descendant is AliasQualifiedNameSyntax);

		if (targetNode == null)
		{
			return;
		}

		context.RegisterCodeFix(
			CodeAction.Create(
				Title,
				cancellationToken => SimplifyFullyQualifiedReference(context.Document, targetNode, diagnostic, cancellationToken),
				nameof(ForbidFullyQualifiedTypeReferencesCodeFixProvider)),
			diagnostic);
	}

	private static async Task<Document> SimplifyFullyQualifiedReference(
		Document document,
		SyntaxNode targetNode,
		Diagnostic diagnostic,
		CancellationToken cancellationToken)
	{
		var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
		if (root == null)
		{
			return document;
		}

		var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
		if (semanticModel == null)
		{
			return document;
		}

		// 1. Get the type symbol for targetNode.
		var symbolInfo = semanticModel.GetSymbolInfo(targetNode, cancellationToken);
		var symbol = symbolInfo.Symbol;

		if (symbol is IAliasSymbol aliasSymbol)
		{
			symbol = aliasSymbol.Target;
		}

		if (symbol is IMethodSymbol methodSymbol && methodSymbol.MethodKind == MethodKind.Constructor)
		{
			symbol = methodSymbol.ContainingType;
		}

		var typeSymbol = symbol as ITypeSymbol;
		if (typeSymbol == null)
		{
			return document;
		}

		// 2. Find the top-level containing type (for nested types).
		var topLevelType = typeSymbol;
		while (topLevelType.ContainingType != null)
		{
			topLevelType = topLevelType.ContainingType;
		}

		// 3. Find the sub-node representing the top-level type.
		var symbolNode = FindNodeForSymbol(targetNode, topLevelType, semanticModel, cancellationToken);
		if (symbolNode == null)
		{
			return document;
		}

		// 4. Simplify the symbolNode.
		SyntaxNode? simplifiedNode = null;
		if (symbolNode is QualifiedNameSyntax qualifiedName)
		{
			simplifiedNode = qualifiedName.Right.WithTriviaFrom(symbolNode);
		}
		else if (symbolNode is MemberAccessExpressionSyntax memberAccess)
		{
			simplifiedNode = memberAccess.Name.WithTriviaFrom(symbolNode);
		}
		else if (symbolNode is AliasQualifiedNameSyntax aliasQualifiedName)
		{
			simplifiedNode = aliasQualifiedName.Name.WithTriviaFrom(symbolNode);
		}

		if (simplifiedNode == null)
		{
			return document;
		}

		var newRoot = root.ReplaceNode(symbolNode, simplifiedNode);

		// 5. Add the using directive.
		if (diagnostic.Properties.TryGetValue("Namespace", out var namespaceName) && namespaceName != null)
		{
			if (newRoot is CompilationUnitSyntax compilationUnit)
			{
				newRoot = CodeFixHelpers.EnsureUsingDirectivesExist(document, compilationUnit, [namespaceName]);
			}
		}

		return document.WithSyntaxRoot(newRoot);
	}

	private static SyntaxNode? FindNodeForSymbol(
		SyntaxNode rootNode,
		ISymbol targetSymbol,
		SemanticModel semanticModel,
		CancellationToken cancellationToken)
	{
		var current = rootNode;
		while (current != null)
		{
			var symbolInfo = semanticModel.GetSymbolInfo(current, cancellationToken);
			var symbol = symbolInfo.Symbol;

			if (symbol is IAliasSymbol aliasSymbol)
			{
				symbol = aliasSymbol.Target;
			}

			if (symbol is IMethodSymbol methodSymbol && methodSymbol.MethodKind == MethodKind.Constructor)
			{
				symbol = methodSymbol.ContainingType;
			}

			if (symbol != null && SymbolEqualityComparer.Default.Equals(symbol.OriginalDefinition, targetSymbol.OriginalDefinition))
			{
				return current;
			}

			if (current is QualifiedNameSyntax qualifiedName)
			{
				current = qualifiedName.Left;
			}
			else if (current is MemberAccessExpressionSyntax memberAccess)
			{
				current = memberAccess.Expression;
			}
			else if (current is AliasQualifiedNameSyntax aliasQualifiedName)
			{
				current = aliasQualifiedName.Name;
			}
			else
			{
				break;
			}
		}

		return null;
	}
}
