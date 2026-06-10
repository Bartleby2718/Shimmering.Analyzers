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
		if (!missingNamespaces.Any()) { return compilationUnit; }

		var newLine = GetNewLine(document);
		var newCompilationUnit = compilationUnit;

		foreach (var missing in missingNamespaces)
		{
			var newUsing = SyntaxFactory.UsingDirective(
					SyntaxFactory.ParseName(missing).WithLeadingTrivia(SyntaxFactory.Space))
				.WithTrailingTrivia(newLine)
				.WithAdditionalAnnotations(Formatter.Annotation);

			bool isSystem = missing == "System" || missing.StartsWith("System.");
			int insertIndex = 0;

			for (int i = 0; i < newCompilationUnit.Usings.Count; i++)
			{
				var u = newCompilationUnit.Usings[i];
				if (u.Alias != null || u.StaticKeyword.IsKind(SyntaxKind.StaticKeyword))
				{
					insertIndex = i;
					break;
				}

				var name = u.Name?.ToString() ?? string.Empty;
				bool uIsSystem = name == "System" || name.StartsWith("System.");

				if (isSystem && !uIsSystem)
				{
					insertIndex = i;
					break;
				}
				if (isSystem == uIsSystem)
				{
					if (string.Compare(missing, name, StringComparison.OrdinalIgnoreCase) < 0)
					{
						insertIndex = i;
						break;
					}
				}
				insertIndex = i + 1;
			}

			if (insertIndex == 0 && newCompilationUnit.Usings.Count > 0)
			{
				var oldFirst = newCompilationUnit.Usings[0];
				var leadingTrivia = oldFirst.GetLeadingTrivia();
				newUsing = newUsing.WithLeadingTrivia(leadingTrivia);
				var updatedOldFirst = oldFirst.WithLeadingTrivia(SyntaxFactory.TriviaList());
				newCompilationUnit = newCompilationUnit.WithUsings(
					newCompilationUnit.Usings.Replace(oldFirst, updatedOldFirst).Insert(0, newUsing));
			}
			else
			{
				newCompilationUnit = newCompilationUnit.WithUsings(newCompilationUnit.Usings.Insert(insertIndex, newUsing));
			}
		}

		return newCompilationUnit;
	}

	private static SyntaxTrivia GetNewLine(Document document)
	{
		var newLine = document.Project.Solution.Options.GetOption(FormattingOptions.NewLine, document.Project.Language);
		return SyntaxFactory.EndOfLine(newLine);
	}
}
