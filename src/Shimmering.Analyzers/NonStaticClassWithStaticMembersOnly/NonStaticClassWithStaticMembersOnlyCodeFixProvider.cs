namespace Shimmering.Analyzers.NonStaticClassWithStaticMembersOnly;

/// <summary>
/// Makes a non-static class static, if reported by <see cref="NonStaticClassWithStaticMembersOnlyAnalyzer"/>.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(NonStaticClassWithStaticMembersOnlyCodeFixProvider)), Shared]
internal sealed class NonStaticClassWithStaticMembersOnlyCodeFixProvider : CodeFixProvider
{
	private const string Title = "Make static class static";

	private static readonly SyntaxKind[] AccessModifiers =
	[
		SyntaxKind.PublicKeyword,
		SyntaxKind.PrivateKeyword,
		SyntaxKind.ProtectedKeyword,
		SyntaxKind.InternalKeyword,
		SyntaxKind.FileKeyword,
	];

	public sealed override ImmutableArray<string> FixableDiagnosticIds =>
		[DiagnosticIds.NonStaticClassWithStaticMembersOnly];

	public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

	public override async Task RegisterCodeFixesAsync(CodeFixContext context)
	{
		var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
		if (root == null) { return; }

		var diagnostic = context.Diagnostics.First();
		var diagnosticSpan = diagnostic.Location.SourceSpan;

		var node = root.FindNode(diagnosticSpan);
		var classDeclaration = node.DescendantNodesAndSelf()
			.OfType<ClassDeclarationSyntax>()
			.FirstOrDefault();
		if (classDeclaration == null) { return; }

		context.RegisterCodeFix(
			CodeAction.Create(
				Title,
				ct => MakeClassStaticAsync(context.Document, classDeclaration, ct),
				nameof(NonStaticClassWithStaticMembersOnlyCodeFixProvider)),
			diagnostic);
	}

	private static async Task<Document> MakeClassStaticAsync(
		Document document,
		ClassDeclarationSyntax classDeclaration,
		CancellationToken cancellationToken)
	{
		// Insert the static keyword after the access modifier
		var accessModifierIndex = GetAccessModifierIndex(classDeclaration.Modifiers);
		var staticModifierIndex = accessModifierIndex + 1;

		// copy over the trailing trivia, just in case the access modifier has some trivia
		var staticModifierTrailingTrivia = accessModifierIndex == -1
			? [SyntaxFactory.Space]
			: classDeclaration.Modifiers[accessModifierIndex].TrailingTrivia;
		var staticModifier = SyntaxFactory.Token(SyntaxKind.StaticKeyword)
			.WithTrailingTrivia(staticModifierTrailingTrivia);

		var newModifiers = classDeclaration.Modifiers.Insert(staticModifierIndex, staticModifier);
		var newClassDeclaration = classDeclaration.WithModifiers(newModifiers);

		var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
		if (root == null) { return document; }

		var newRoot = root.ReplaceNode(classDeclaration, newClassDeclaration);
		return document.WithSyntaxRoot(newRoot);
	}

	private static int GetAccessModifierIndex(SyntaxTokenList modifiers)
	{
		foreach (var accessModifier in AccessModifiers)
		{
			var index = modifiers.IndexOf(accessModifier);
			if (index != -1)
			{
				return index;
			}
		}

		return -1;
	}
}
