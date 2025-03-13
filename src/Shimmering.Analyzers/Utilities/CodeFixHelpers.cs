using Microsoft.CodeAnalysis.Formatting;

namespace Shimmering.Analyzers.Utilities;

internal static class CodeFixHelpers
{
	/// <summary>
	/// Checks if <paramref name="compilationUnit"/> contains all <paramref name="namespaces"/>. If not, the missing namespaces are added at the end, in the same order as in <paramref name="namespaces"/>.
	/// </summary>
	public static CompilationUnitSyntax EnsureUsingDirectivesExist(Document document, CompilationUnitSyntax compilationUnit, IReadOnlyCollection<string> namespaces)
	{
		var missingNamespaces = namespaces
			.Where(@namespace => compilationUnit.Usings.Any(u => u.Name?.ToString() == @namespace) == false);
		if (missingNamespaces.Any() == false) { return compilationUnit; }

		var newLine = GetNewLine(document);
		var newUsings = missingNamespaces
			.Select(@namespace =>
				SyntaxFactory.UsingDirective(
					SyntaxFactory.ParseName(@namespace)
						.WithLeadingTrivia(SyntaxFactory.Space))
					.WithTrailingTrivia(newLine))
			.ToArray();

		return compilationUnit.AddUsings(newUsings);
	}

	private static SyntaxTrivia GetNewLine(Document document)
	{
		var newLine = document.Project.Solution.Options.GetOption(FormattingOptions.NewLine, document.Project.Language);
		return SyntaxFactory.EndOfLine(newLine);
	}
}
