using System.Text.RegularExpressions;

namespace Shimmering.Analyzers.CodeFixes.Usage;

/// <summary>
/// Replaces unnamed capturing groups in regex patterns with named capturing groups.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UnnamedRegexCaptureGroupCodeFixProvider))]
public sealed class UnnamedRegexCaptureGroupCodeFixProvider : ShimmeringCodeFixProvider
{
	private const string Title = "Use (?<name>...) to prevent brittle positional indexing";

	public sealed override ImmutableArray<string> FixableDiagnosticIds =>
		[DiagnosticIds.UsageRules.UnnamedRegexCaptureGroup];

	public override string SampleCodeFixed => """
		using System.Text.RegularExpressions;

		class Test
		{
			void Do()
			{
				var regex = new Regex(@"(?<group1>\d{4})-(?<group2>\d{2})-(?<group3>\d{2})");
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

		var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);
		if (semanticModel == null)
		{
			return;
		}

		var diagnostic = context.Diagnostics.First();
		var diagnosticSpan = diagnostic.Location.SourceSpan;
		var node = root.FindNode(diagnosticSpan);

		FindPatternAndOptionsExpressions(
			node,
			semanticModel,
			context.CancellationToken,
			out var patternExpression,
			out var optionsExpression);

		if (patternExpression is LiteralExpressionSyntax literalExpression && literalExpression.IsKind(SyntaxKind.StringLiteralExpression))
		{
			context.RegisterCodeFix(
				CodeAction.Create(
					Title,
					cancellationToken => FixRegexPatternAsync(context.Document, literalExpression, optionsExpression, cancellationToken),
					nameof(UnnamedRegexCaptureGroupCodeFixProvider)),
				diagnostic);
		}
	}

	private static void FindPatternAndOptionsExpressions(
		SyntaxNode node,
		SemanticModel semanticModel,
		CancellationToken cancellationToken,
		out ExpressionSyntax? patternExpression,
		out ExpressionSyntax? optionsExpression)
	{
		patternExpression = null;
		optionsExpression = null;

		if (node is ObjectCreationExpressionSyntax objectCreation)
		{
			var constructorSymbol = semanticModel.GetSymbolInfo(objectCreation, cancellationToken).Symbol as IMethodSymbol;
			if (constructorSymbol == null || constructorSymbol.ContainingType?.ToDisplayString() != "System.Text.RegularExpressions.Regex")
			{
				return;
			}

			var parameters = constructorSymbol.Parameters;
			if (objectCreation.ArgumentList == null)
			{
				return;
			}

			var arguments = objectCreation.ArgumentList.Arguments;
			for (int i = 0; i < parameters.Length; i++)
			{
				var parameter = parameters[i];
				var argument = arguments.FirstOrDefault(arg => arg.NameColon?.Name.Identifier.ValueText == parameter.Name);
				if (argument == null && i < arguments.Count)
				{
					argument = arguments[i];
				}

				if (argument != null)
				{
					if (string.Equals(parameter.Name, "pattern", StringComparison.Ordinal))
					{
						patternExpression = argument.Expression;
					}
					else if (string.Equals(parameter.Name, "options", StringComparison.Ordinal))
					{
						optionsExpression = argument.Expression;
					}
				}
			}
		}
		else if (node is InvocationExpressionSyntax invocation)
		{
			var methodSymbol = semanticModel.GetSymbolInfo(invocation, cancellationToken).Symbol as IMethodSymbol;
			if (methodSymbol == null || !methodSymbol.IsStatic || methodSymbol.ContainingType?.ToDisplayString() != "System.Text.RegularExpressions.Regex")
			{
				return;
			}

			var parameters = methodSymbol.Parameters;
			if (invocation.ArgumentList == null)
			{
				return;
			}

			var arguments = invocation.ArgumentList.Arguments;
			for (int i = 0; i < parameters.Length; i++)
			{
				var parameter = parameters[i];
				var argument = arguments.FirstOrDefault(arg => arg.NameColon?.Name.Identifier.ValueText == parameter.Name);
				if (argument == null && i < arguments.Count)
				{
					argument = arguments[i];
				}

				if (argument != null)
				{
					if (string.Equals(parameter.Name, "pattern", StringComparison.Ordinal))
					{
						patternExpression = argument.Expression;
					}
					else if (string.Equals(parameter.Name, "options", StringComparison.Ordinal))
					{
						optionsExpression = argument.Expression;
					}
				}
			}
		}
		else if (node is AttributeSyntax attribute)
		{
			var attributeSymbol = semanticModel.GetSymbolInfo(attribute, cancellationToken).Symbol as IMethodSymbol;
			if (attributeSymbol == null || attributeSymbol.ContainingType?.ToDisplayString() != "System.Text.RegularExpressions.GeneratedRegexAttribute")
			{
				return;
			}

			var parameters = attributeSymbol.Parameters;
			if (attribute.ArgumentList == null)
			{
				return;
			}

			var arguments = attribute.ArgumentList.Arguments;
			for (int i = 0; i < parameters.Length; i++)
			{
				var parameter = parameters[i];
				var argument = arguments.FirstOrDefault(arg => arg.NameColon?.Name.Identifier.ValueText == parameter.Name);
				if (argument == null && i < arguments.Count)
				{
					argument = arguments[i];
				}

				if (argument != null)
				{
					if (string.Equals(parameter.Name, "pattern", StringComparison.Ordinal))
					{
						patternExpression = argument.Expression;
					}
					else if (string.Equals(parameter.Name, "options", StringComparison.Ordinal))
					{
						optionsExpression = argument.Expression;
					}
				}
			}
		}
	}

	private static async Task<Document> FixRegexPatternAsync(
		Document document,
		LiteralExpressionSyntax literalExpression,
		ExpressionSyntax? optionsExpression,
		CancellationToken cancellationToken)
	{
		var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
		if (semanticModel == null)
		{
			return document;
		}

		var patternConstant = semanticModel.GetConstantValue(literalExpression, cancellationToken);
		if (!patternConstant.HasValue || patternConstant.Value is not string pattern)
		{
			return document;
		}

		var options = RegexOptions.None;
		if (optionsExpression != null)
		{
			var optionsConstant = semanticModel.GetConstantValue(optionsExpression, cancellationToken);
			if (optionsConstant.HasValue && optionsConstant.Value is int intValue)
			{
				options = (RegexOptions)intValue;
			}
		}

		try
		{
			var regex = new Regex(pattern, options);
			var existingNames = new HashSet<string>(regex.GetGroupNames(), StringComparer.Ordinal);

			var insertions = new List<(int Position, string Name)>();
			bool inEscape = false;
			bool inCharacterClass = false;
			int capturingGroupCount = 0;
			bool hasComplexConstructs = false;

			for (int i = 0; i < pattern.Length; i++)
			{
				var c = pattern[i];

				if (inEscape)
				{
					inEscape = false;
					continue;
				}

				if (c == '\\')
				{
					inEscape = true;
					continue;
				}

				if (inCharacterClass)
				{
					if (c == ']')
					{
						inCharacterClass = false;
					}
					continue;
				}

				if (c == '[')
				{
					inCharacterClass = true;
					continue;
				}

				if (c == '(')
				{
					if (i + 1 < pattern.Length)
					{
						var next = pattern[i + 1];
						if (next == '?')
						{
							if (i + 2 < pattern.Length)
							{
								var nextNext = pattern[i + 2];
								if (nextNext == '<' || nextNext == '\'')
								{
									capturingGroupCount++;
								}
								else if (nextNext == '(')
								{
									hasComplexConstructs = true;
								}
							}
						}
						else
						{
							capturingGroupCount++;
							var name = $"group{capturingGroupCount}";
							while (existingNames.Contains(name))
							{
								name += "_";
							}
							insertions.Add((i + 1, name));
						}
					}
					else
					{
						capturingGroupCount++;
						var name = $"group{capturingGroupCount}";
						while (existingNames.Contains(name))
						{
							name += "_";
						}
						insertions.Add((i + 1, name));
					}
				}
			}

			if (hasComplexConstructs || insertions.Count == 0)
			{
				return document;
			}

			var stringBuilder = new System.Text.StringBuilder(pattern);
			for (int i = insertions.Count - 1; i >= 0; i--)
			{
				var insertion = insertions[i];
				stringBuilder.Insert(insertion.Position, $"?<{insertion.Name}>");
			}

			var newPattern = stringBuilder.ToString();
			var tokenText = literalExpression.Token.Text;
			string newText;

			if (tokenText.StartsWith("@", StringComparison.Ordinal))
			{
				newText = "@\"" + newPattern.Replace("\"", "\"\"") + "\"";
			}
			else if (tokenText.StartsWith("\"\"\"", StringComparison.Ordinal))
			{
				newText = "\"\"\"" + newPattern + "\"\"\"";
			}
			else
			{
				newText = SymbolDisplay.FormatLiteral(newPattern, true);
			}

			var newLiteralExpression = SyntaxFactory.ParseExpression(newText);
			var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
			if (root == null)
			{
				return document;
			}

			var newRoot = root.ReplaceNode(literalExpression, newLiteralExpression.WithTriviaFrom(literalExpression));
			return document.WithSyntaxRoot(newRoot);
		}
		catch (ArgumentException)
		{
			// Ignore invalid regex syntax
		}

		return document;
	}
}
