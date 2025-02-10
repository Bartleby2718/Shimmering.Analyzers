﻿using System.Diagnostics.CodeAnalysis;

using Shimmering.Analyzers.Utilities;

namespace Shimmering.Analyzers.SingleUseIEnumerableMaterialization;

/// <summary>
/// Reports instances of a single-use IEnumerable that is materialized.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
internal sealed class SingleUseIEnumerableMaterializationAnalyzer : DiagnosticAnalyzer
{
	private const string Title = "Avoid materializing a single-use IEnumerable";
	private const string Message = "Avoid materializing an IEnumerable if it's used only once";
	private const string Category = "CodeQuality";

	private static readonly DiagnosticDescriptor Rule = new(
		DiagnosticIds.SingleUseIEnumerableMaterialization,
		Title,
		Message,
		Category,
		DiagnosticSeverity.Info,
		isEnabledByDefault: true,
		helpLinkUri: $"https://github.com/Bartleby2718/Shimmering.Analyzers/blob/main/docs/{DiagnosticIds.SingleUseIEnumerableMaterialization}.md");

	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

	public override void Initialize(AnalysisContext context)
	{
		context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
		context.EnableConcurrentExecution();
		context.RegisterSyntaxNodeAction(AnalyzeLocalDeclaration, SyntaxKind.LocalDeclarationStatement);
	}

	private static void AnalyzeLocalDeclaration(SyntaxNodeAnalysisContext context)
	{
		var localDeclaration = (LocalDeclarationStatementSyntax)context.Node;

		// For now, handle only single variable declarations for simplicity.
		if (localDeclaration.Declaration.Variables.Count != 1) { return; }

		// TODO: handle explicit types
		if (!localDeclaration.Declaration.Type.IsVar) { return; }

		var variableDeclarator = localDeclaration.Declaration.Variables.First();
		if (variableDeclarator.Initializer == null) { return; }

		// Look for an invocation expression in the initializer.
		if (variableDeclarator.Initializer.Value is not InvocationExpressionSyntax invocation) { return; }

		// Check that the invocation is a member access
		if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess) { return; }

		// Only proceed if the method is "ToList" or "ToArray"
		// TODO: use EnumerableHelpers
		var methodName = memberAccess.Name.Identifier.Text;
		if (methodName is not (nameof(Enumerable.ToList) or nameof(Enumerable.ToArray))) { return; }

		// Get the symbol for the variable.
		var variableSymbol = context.SemanticModel.GetDeclaredSymbol(variableDeclarator);
		if (variableSymbol == null) { return; }

		// Find the enclosing block.
		if (localDeclaration.Parent is not BlockSyntax parentBlock) { return; }

		// Find all identifier usages of this variable (excluding the declaration itself).
		var identifierUsages = parentBlock.DescendantNodes()
			.OfType<IdentifierNameSyntax>()
			.Where(id => SymbolEqualityComparer.Default.Equals(variableSymbol, context.SemanticModel.GetSymbolInfo(id).Symbol))
			.Where(id => id.SpanStart != variableDeclarator.Identifier.SpanStart)
			.ToList();

		if (identifierUsages.Count != 1) { return; }
		var identifierUsage = identifierUsages.Single();
		if (IsUsedInForeach(identifierUsage) || IsFollowedByLinqMethod(identifierUsage, context.SemanticModel))
		{
			var diagnostic = Diagnostic.Create(Rule, invocation.GetLocation(), variableSymbol.Name);
			context.ReportDiagnostic(diagnostic);
		}
	}

	private static bool IsUsedInForeach(IdentifierNameSyntax identifier)
	{
		return identifier.Parent is ForEachStatementSyntax foreachStatement
			&& foreachStatement.Expression == identifier;
	}

	private static bool IsFollowedByLinqMethod(IdentifierNameSyntax identifier, SemanticModel semanticModel)
	{
		return identifier.Parent is MemberAccessExpressionSyntax memberAccess
			&& memberAccess.Parent is InvocationExpressionSyntax invocation
			&& EnumerableHelpers.IsLinqExtensionMethodCall(semanticModel, invocation, out _);
	}
}
