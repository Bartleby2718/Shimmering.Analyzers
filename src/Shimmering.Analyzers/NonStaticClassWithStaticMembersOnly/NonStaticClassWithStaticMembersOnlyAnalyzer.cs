namespace Shimmering.Analyzers.NonStaticClassWithStaticMembersOnly;

/// <summary>
/// Reports instances of a non-static class that can be made static.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
internal sealed class NonStaticClassWithStaticMembersOnlyAnalyzer : ShimmeringSyntaxNodeAnalyzer
{
	private const string Title = "Make static class static";
	private const string Message = "Non-static class '{0}' can be made static";
	private const string Category = "Usage";

	private static readonly DiagnosticDescriptor Rule = CreateRule(
		DiagnosticIds.NonStaticClassWithStaticMembersOnly,
		Title,
		Message,
		Category,
		DiagnosticSeverity.Info,
		isEnabledByDefault: true);

	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

	public override void RegisterSyntaxNodeAction(AnalysisContext context)
	{
		context.RegisterSyntaxNodeAction(AnalyzeClassDeclaration, SyntaxKind.ClassDeclaration);
	}

	private static void AnalyzeClassDeclaration(SyntaxNodeAnalysisContext context)
	{
		var classDeclaration = (ClassDeclarationSyntax)context.Node;

		// ignore already static classes
		if (classDeclaration.Modifiers.Any(SyntaxKind.StaticKeyword)
			// a partial class may be spread out across multiple files, so bail out
			|| classDeclaration.Modifiers.Any(SyntaxKind.PartialKeyword)
			// a sealed class was likely sealed for a reason, so bail out
			|| classDeclaration.Modifiers.Any(SyntaxKind.SealedKeyword))
		{
			return;
		}

		// a static class can only inherit from object
		if (!ImplementsObjectAndNothingElse(classDeclaration, context.SemanticModel)) { return; }

		if (AllMembersAreEitherConstFieldsOrStatic(classDeclaration))
		{
			var diagnostic = Diagnostic.Create(Rule, classDeclaration.Identifier.GetLocation(), classDeclaration.Identifier.Text);
			context.ReportDiagnostic(diagnostic);
		}
	}

	private static bool ImplementsObjectAndNothingElse(ClassDeclarationSyntax classDeclaration, SemanticModel semanticModel)
	{
		if (classDeclaration.BaseList == null) { return true; }

		foreach (var baseType in classDeclaration.BaseList.Types)
		{
			var typeSymbol = semanticModel.GetTypeInfo(baseType.Type).Type;
			if (typeSymbol == null || typeSymbol.SpecialType != SpecialType.System_Object)
			{
				return false;
			}
		}

		return true;
	}

	private static bool AllMembersAreEitherConstFieldsOrStatic(ClassDeclarationSyntax classDeclaration)
	{
		foreach (var member in classDeclaration.Members)
		{
			if (member is FieldDeclarationSyntax field)
			{
				if (!field.Modifiers.Any(SyntaxKind.StaticKeyword) && !field.Modifiers.Any(SyntaxKind.ConstKeyword))
				{
					return false;
				}
			}
			else if (!member.Modifiers.Any(SyntaxKind.StaticKeyword))
			{
				return false;
			}
		}

		return true;
	}
}
