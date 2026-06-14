using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Shimmering.Analyzers.Analyzers.Usage;

namespace Shimmering.Analyzers.CodeFixes.Usage;

/// <summary>
/// Replaces new CultureInfo(...) with CultureInfo.GetCultureInfo(...) or CultureInfo.InvariantCulture.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseGetCultureInfoCodeFixProvider))]
[Shared]
public sealed class UseGetCultureInfoCodeFixProvider : ShimmeringCodeFixProvider
{
	private const string Title = "Use cached CultureInfo";

	public sealed override ImmutableArray<string> FixableDiagnosticIds =>
		[DiagnosticIds.UsageRules.UseGetCultureInfo];

	public override string SampleCodeFixed => """
		using System.Globalization;

		class Test
		{
			void Do()
			{
				var culture = CultureInfo.GetCultureInfo("en-US");
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

		var objectCreation = node.DescendantNodesAndSelf().OfType<ObjectCreationExpressionSyntax>().FirstOrDefault();
		if (objectCreation == null)
		{
			return;
		}

		context.RegisterCodeFix(
			CodeAction.Create(
				Title,
				cancellationToken => ReplaceCultureInfoConstructorAsync(context.Document, objectCreation, cancellationToken),
				nameof(UseGetCultureInfoCodeFixProvider)),
			diagnostic);
	}

	private static async Task<Document> ReplaceCultureInfoConstructorAsync(
		Document document, ObjectCreationExpressionSyntax objectCreation, CancellationToken cancellationToken)
	{
		var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
		var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
		if (root == null || semanticModel == null || objectCreation.ArgumentList == null || objectCreation.ArgumentList.Arguments.Count == 0)
		{
			return document;
		}

		var firstArgumentExpression = objectCreation.ArgumentList.Arguments[0].Expression;

		ExpressionSyntax replacementNode;
		if (IsEmptyString(firstArgumentExpression, semanticModel, cancellationToken) || IsInvariantLcid(firstArgumentExpression, semanticModel, cancellationToken))
		{
			replacementNode = SyntaxFactory.MemberAccessExpression(
				SyntaxKind.SimpleMemberAccessExpression,
				SyntaxFactory.IdentifierName("CultureInfo"),
				SyntaxFactory.IdentifierName("InvariantCulture"));
		}
		else
		{
			var memberAccess = SyntaxFactory.MemberAccessExpression(
				SyntaxKind.SimpleMemberAccessExpression,
				SyntaxFactory.IdentifierName("CultureInfo"),
				SyntaxFactory.IdentifierName("GetCultureInfo"));

			var argumentList = SyntaxFactory.ArgumentList(
				SyntaxFactory.SingletonSeparatedList(
					SyntaxFactory.Argument(firstArgumentExpression.WithoutTrivia())));

			replacementNode = SyntaxFactory.InvocationExpression(memberAccess, argumentList);
		}

		replacementNode = replacementNode.WithTriviaFrom(objectCreation);
		var newRoot = root.ReplaceNode(objectCreation, replacementNode);
		return document.WithSyntaxRoot(newRoot);
	}

	private static bool IsEmptyString(ExpressionSyntax expression, SemanticModel semanticModel, CancellationToken cancellationToken)
	{
		if (expression is LiteralExpressionSyntax literal && literal.Token.ValueText == string.Empty)
		{
			return true;
		}

		var symbol = semanticModel.GetSymbolInfo(expression, cancellationToken).Symbol;
		if (symbol != null && symbol.Name == "Empty" && symbol.ContainingType?.ToDisplayString() == "string")
		{
			return true;
		}

		return false;
	}

	private static bool IsInvariantLcid(ExpressionSyntax expression, SemanticModel semanticModel, CancellationToken cancellationToken)
	{
		var constantValue = semanticModel.GetConstantValue(expression, cancellationToken);
		return constantValue.HasValue && constantValue.Value is 127;
	}
}
