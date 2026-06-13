using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Shimmering.Analyzers.Core;

namespace Shimmering.Analyzers.Analyzers.Style;

/// <summary>
/// Forbids fully-qualified type references in expressions and type syntax, requiring using directives instead.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ForbidFullyQualifiedTypeReferencesAnalyzer : ShimmeringAnalyzer
{
	private const string Title = "Avoid fully-qualified type references";
	private const string Message = "Type reference '{0}' should not be fully-qualified; add 'using {1};' instead";
	private const string Category = RuleCategories.Style;

	private static readonly DiagnosticDescriptor Rule = ShimmeringRuleFactory.Create(
		DiagnosticIds.StyleRules.ForbidFullyQualifiedTypeReferences,
		Title,
		Message,
		Category,
		DiagnosticSeverity.Info);

	public override string SampleCode => """
		namespace Tests
		{
			class Test
			{
				void Method()
				{
					[|System.Text.Encoding|].UTF8.GetBytes("test");
				}
			}
		}
		""";

	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

	protected override void InitializeCore(AnalysisContext context)
	{
		context.RegisterSyntaxNodeAction(AnalyzeSyntaxNode, SyntaxKind.QualifiedName, SyntaxKind.SimpleMemberAccessExpression, SyntaxKind.AliasQualifiedName);
	}

	private static void AnalyzeSyntaxNode(SyntaxNodeAnalysisContext context)
	{
		var syntaxNode = context.Node;

		// 1. Get the type symbol for the current node.
		var typeSymbol = GetTypeSymbol(syntaxNode, context.SemanticModel, context.CancellationToken);
		if (typeSymbol == null)
		{
			return;
		}

		// 2. Ensure it is the outermost qualified type reference.
		if (!IsOutermostTypeReference(syntaxNode, context.SemanticModel, context.CancellationToken))
		{
			return;
		}

		// 3. Find its containing namespace. If it's the global namespace or null, ignore it.
		var containingNamespace = typeSymbol.ContainingNamespace;
		if (containingNamespace == null || containingNamespace.IsGlobalNamespace)
		{
			return;
		}

		var namespaceName = containingNamespace.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat);

		// 4. Find the top-level containing type (for nested types).
		var topLevelType = typeSymbol;
		while (topLevelType.ContainingType != null)
		{
			topLevelType = topLevelType.ContainingType;
		}

		// 5. Check if the top-level type is already in scope (imported via using, global using, or in same namespace).
		var position = syntaxNode.SpanStart;
		var symbols = context.SemanticModel.LookupSymbols(position, name: topLevelType.Name);
		bool isAlreadyInScope = false;

		foreach (var symbol in symbols)
		{
			if (SymbolEqualityComparer.Default.Equals(symbol.OriginalDefinition, topLevelType.OriginalDefinition))
			{
				isAlreadyInScope = true;
				break;
			}
		}

		if (isAlreadyInScope)
		{
			return;
		}

		// 6. Report the diagnostic, passing the namespace name and top-level type name for the code fix.
		var properties = ImmutableDictionary<string, string?>.Empty
			.Add("Namespace", namespaceName)
			.Add("TypeName", topLevelType.Name);

		var diagnostic = Diagnostic.Create(
			Rule,
			syntaxNode.GetLocation(),
			properties,
			syntaxNode.ToString(),
			namespaceName);

		context.ReportDiagnostic(diagnostic);
	}

	private static ITypeSymbol? GetTypeSymbol(SyntaxNode syntaxNode, SemanticModel semanticModel, CancellationToken cancellationToken)
	{
		// Skip nodes inside a using directive itself.
		if (syntaxNode.Ancestors().Any(ancestor => ancestor is UsingDirectiveSyntax))
		{
			return null;
		}

		var symbolInfo = semanticModel.GetSymbolInfo(syntaxNode, cancellationToken);
		var symbol = symbolInfo.Symbol;

		if (symbol is IAliasSymbol aliasSymbol)
		{
			symbol = aliasSymbol.Target;
		}

		if (symbol is IMethodSymbol methodSymbol && methodSymbol.MethodKind == MethodKind.Constructor)
		{
			symbol = methodSymbol.ContainingType;
		}

		if (symbol is ITypeSymbol typeSymbol)
		{
			return typeSymbol;
		}

		return null;
	}

	private static bool IsOutermostTypeReference(SyntaxNode syntaxNode, SemanticModel semanticModel, CancellationToken cancellationToken)
	{
		var parent = syntaxNode.Parent;
		if (parent == null)
		{
			return true;
		}

		if (parent is QualifiedNameSyntax qualifiedName && qualifiedName.Left == syntaxNode)
		{
			if (GetTypeSymbol(parent, semanticModel, cancellationToken) != null)
			{
				return false;
			}
		}
		else if (parent is MemberAccessExpressionSyntax memberAccess && memberAccess.Expression == syntaxNode && memberAccess.IsKind(SyntaxKind.SimpleMemberAccessExpression))
		{
			if (GetTypeSymbol(parent, semanticModel, cancellationToken) != null)
			{
				return false;
			}
		}

		return true;
	}
}
