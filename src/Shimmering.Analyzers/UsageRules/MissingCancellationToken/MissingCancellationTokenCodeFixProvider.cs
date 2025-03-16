using System.Diagnostics;
using Shimmering.Analyzers.Utilities;

namespace Shimmering.Analyzers.UsageRules.MissingCancellationToken;

/// <summary>
/// Adds a <see cref="CancellationToken"/> parameter if reported by <see cref="MissingCancellationTokenAnalyzer"/>.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(MissingCancellationTokenCodeFixProvider))]
public sealed class MissingCancellationTokenCodeFixProvider : ShimmeringCodeFixProvider
{
	private static readonly string Title = $"Add a {nameof(CancellationToken)} parameter";

	public sealed override ImmutableArray<string> FixableDiagnosticIds =>
		[DiagnosticIds.UsageRules.MissingCancellationToken];

#pragma warning disable SA1027 // Use tabs correctly
	public override string SampleCodeFixed => """
		using System.Threading.Tasks;
		using System.Threading;

		namespace Tests
		{
		    class Test
		    {
		        async Task DoAsync(CancellationToken cancellationToken = default)
		        {
		            await Task.CompletedTask;
		        }
		    }
		}
		"""
#pragma warning restore SA1027 // Use tabs correctly
;

	public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
	{
		var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
		if (root == null) { return; }

		var diagnostic = context.Diagnostics.First();
		var diagnosticSpan = diagnostic.Location.SourceSpan;

		var node = root.FindNode(diagnosticSpan);
		if (node is not MethodDeclarationSyntax methodDeclaration) { return; }

		context.RegisterCodeFix(
			CodeAction.Create(
				Title,
				ct => AddCancellationTokenParameterAsync(context.Document, methodDeclaration, ct),
				nameof(MissingCancellationTokenCodeFixProvider)),
			diagnostic);
	}

	private static async Task<Document> AddCancellationTokenParameterAsync(
		Document document,
		MethodDeclarationSyntax methodDeclaration,
		CancellationToken cancellationToken)
	{
		var parameterNames = methodDeclaration.ParameterList.Parameters
			.Select(p => p.Identifier.Text)
			.ToArray();

		var parameters = methodDeclaration.ParameterList.Parameters;

		// Create a new parameter for CancellationToken
		var cancellationTokenParameterName = GetCancellationTokenParameterName(methodDeclaration);
		var cancellationTokenParameter = SyntaxFactory.Parameter(SyntaxFactory.Identifier(cancellationTokenParameterName))
			.WithType(SyntaxFactory.ParseTypeName(nameof(CancellationToken)))
			.WithDefault(SyntaxFactory.EqualsValueClause(SyntaxFactory.LiteralExpression(SyntaxKind.DefaultLiteralExpression)))
			.NormalizeWhitespace();
		if (parameters.Count != 0)
		{
			var previousParameterLeadingTrivia = parameters[parameters.Count - 1].GetLeadingTrivia();
			cancellationTokenParameter = cancellationTokenParameter.WithLeadingTrivia(previousParameterLeadingTrivia);
		}

		var originalParameterList = methodDeclaration.ParameterList;
		var lastCommaToken = methodDeclaration.ParameterList
			.Parameters
			.GetSeparators()
			.LastOrDefault();
		var newParameterList = ConstructNewParameterList(originalParameterList, cancellationTokenParameter, lastCommaToken)
			.WithTriviaFrom(originalParameterList)
			.WithOpenParenToken(originalParameterList.OpenParenToken);
		var newMethodDeclaration = methodDeclaration.WithParameterList(newParameterList);

		if (await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false) is not CompilationUnitSyntax root) { return document; }

		var newRoot = root.ReplaceNode(methodDeclaration, newMethodDeclaration);
		var newRootWithUsingDirective = CodeFixHelpers.EnsureUsingDirectivesExist(document, newRoot, namespaces: [FullyQualifiedNamespaces.SystemThreading]);
		return document.WithSyntaxRoot(newRootWithUsingDirective);
	}

	private static ParameterListSyntax ConstructNewParameterList(ParameterListSyntax originalParameterList, ParameterSyntax newParameter, SyntaxToken lastCommaToken)
	{
		if (lastCommaToken == default)
		{
			return SyntaxFactory.ParameterList(originalParameterList.Parameters.Add(newParameter));
		}

		var newCommaToken = SyntaxFactory.Token(SyntaxKind.CommaToken)
			.WithTrailingTrivia(lastCommaToken.TrailingTrivia);
		var newParameters = originalParameterList.Parameters.Add(newParameter);
		var separators = originalParameterList
			.Parameters
			.GetSeparators()
			.Append(newCommaToken);

		return SyntaxFactory.ParameterList(SyntaxFactory.SeparatedList(newParameters, separators));
	}

	private static string GetCancellationTokenParameterName(MethodDeclarationSyntax methodDeclaration)
	{
		const string defaultParameterName = "cancellationToken";

		var parameters = methodDeclaration.ParameterList.Parameters;
		if (parameters.Count == 0) { return defaultParameterName; }

		var parameterNames = parameters
			.Select(p => p.Identifier.Text)
			// use a HashSet because we'll be checking membership below
			.ToImmutableHashSet();

		if (parameterNames.Contains(defaultParameterName) == false) { return defaultParameterName; }

		// By the pigeonhole principle, we don't need to try more than X+1 suffixes,
		// where X is the number of parameters in this method.
		int suffix = 0;
		while (suffix <= parameterNames.Count)
		{
			var candidate = $"{defaultParameterName}{suffix}";
			if (!parameterNames.Contains(candidate))
			{
				return candidate;
			}

			suffix++;
		}

		throw new UnreachableException();
	}
}
