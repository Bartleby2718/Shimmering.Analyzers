namespace Shimmering.Analyzers.Utilities;

internal static class CodeFixHelpers
{
	/// <summary>
	/// Checks if <paramref name="compilationUnit"/> contains all <paramref name="namespaces"/>. If not, the missing namespaces are added at the end, in the same order as in <paramref name="namespaces"/>.
	/// </summary>
	public static CompilationUnitSyntax EnsureUsingDirectivesExist(CompilationUnitSyntax compilationUnit, IReadOnlyCollection<string> namespaces)
	{
		var missingNamespaces = namespaces
			.Where(@namespace => compilationUnit.Usings.Any(u => u.Name?.ToString() == @namespace) == false);
		if (missingNamespaces.Any() == false) { return compilationUnit; }

		var newUsings = missingNamespaces
			.Select(@namespace =>
				SyntaxFactory.UsingDirective(
					SyntaxFactory.ParseName(@namespace)
						.WithLeadingTrivia(SyntaxFactory.Space))
					.WithTrailingTrivia(SyntaxFactory.CarriageReturnLineFeed))
			.ToArray();
		return compilationUnit.AddUsings(newUsings);
	}
}
