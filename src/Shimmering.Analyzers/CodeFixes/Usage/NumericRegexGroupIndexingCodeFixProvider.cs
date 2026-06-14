using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Shimmering.Analyzers.Analyzers.Usage;

namespace Shimmering.Analyzers.CodeFixes.Usage;

/// <summary>
/// Replaces positional indexing on capture groups with named capture group accesses.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(NumericRegexGroupIndexingCodeFixProvider))]
public sealed class NumericRegexGroupIndexingCodeFixProvider : ShimmeringCodeFixProvider
{
	private const string Title = "Use named group to prevent silent breakage when pattern changes";

	public sealed override ImmutableArray<string> FixableDiagnosticIds =>
		[DiagnosticIds.UsageRules.NumericRegexGroupIndexing];

	public override string SampleCodeFixed => """
		using System.Text.RegularExpressions;

		class Test
		{
			void Do(Match match)
			{
				var group = match.Groups["group1"];
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

		var elementAccess = node.FirstAncestorOrSelf<ElementAccessExpressionSyntax>();
		if (elementAccess == null || elementAccess.ArgumentList.Arguments.Count != 1)
		{
			return;
		}

		var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);
		if (semanticModel == null)
		{
			return;
		}

		var indexArgument = elementAccess.ArgumentList.Arguments[0];
		var constantValue = semanticModel.GetConstantValue(indexArgument.Expression, context.CancellationToken);
		if (constantValue.HasValue && constantValue.Value is int index)
		{
			context.RegisterCodeFix(
				CodeAction.Create(
					Title,
					cancellationToken => FixGroupAccessAsync(context.Document, elementAccess, index, cancellationToken),
					nameof(NumericRegexGroupIndexingCodeFixProvider)),
				diagnostic);
		}
	}

	private static async Task<Document> FixGroupAccessAsync(
		Document document,
		ElementAccessExpressionSyntax elementAccess,
		int index,
		CancellationToken cancellationToken)
	{
		var resolvedGroupName = await ResolveGroupNameAsync(document, elementAccess, index, cancellationToken).ConfigureAwait(false);

		var argument = elementAccess.ArgumentList.Arguments[0];
		var newArgument = argument.WithExpression(
			SyntaxFactory.LiteralExpression(
				SyntaxKind.StringLiteralExpression,
				SyntaxFactory.Literal(resolvedGroupName)));

		var newElementAccess = elementAccess.WithArgumentList(
			elementAccess.ArgumentList.WithArguments(
				elementAccess.ArgumentList.Arguments.Replace(argument, newArgument)));

		var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
		if (root == null)
		{
			return document;
		}

		var newRoot = root.ReplaceNode(elementAccess, newElementAccess);
		return document.WithSyntaxRoot(newRoot);
	}

	private static async Task<string> ResolveGroupNameAsync(
		Document document,
		ElementAccessExpressionSyntax elementAccess,
		int index,
		CancellationToken cancellationToken)
	{
		var fallbackName = GetFallbackName(elementAccess, index);

		var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
		if (semanticModel == null)
		{
			return fallbackName;
		}

		if (elementAccess.Expression is not MemberAccessExpressionSyntax memberAccess || memberAccess.Name.Identifier.ValueText != "Groups")
		{
			return fallbackName;
		}

		var matchExpression = memberAccess.Expression;
		var matchSymbol = semanticModel.GetSymbolInfo(matchExpression, cancellationToken).Symbol;
		if (matchSymbol == null)
		{
			return fallbackName;
		}

		ExpressionSyntax? matchAssignExpression = null;

		if (matchSymbol is ILocalSymbol localSymbol)
		{
			var syntaxReference = localSymbol.DeclaringSyntaxReferences.FirstOrDefault();
			if (syntaxReference != null)
			{
				var localDeclarator = await syntaxReference.GetSyntaxAsync(cancellationToken).ConfigureAwait(false) as VariableDeclaratorSyntax;
				matchAssignExpression = localDeclarator?.Initializer?.Value;
			}
		}

		if (matchAssignExpression == null)
		{
			return fallbackName;
		}

		if (matchAssignExpression is not InvocationExpressionSyntax matchInvocation)
		{
			return fallbackName;
		}

		var matchMethodSymbol = semanticModel.GetSymbolInfo(matchInvocation, cancellationToken).Symbol as IMethodSymbol;
		if (matchMethodSymbol == null || matchMethodSymbol.ContainingType?.ToDisplayString() != "System.Text.RegularExpressions.Regex")
		{
			return fallbackName;
		}

		if (matchMethodSymbol.Name != "Match" && matchMethodSymbol.Name != "Matches")
		{
			return fallbackName;
		}

		string? pattern = null;
		RegexOptions options = RegexOptions.None;

		if (matchMethodSymbol.IsStatic)
		{
			var parameters = matchMethodSymbol.Parameters;
			if (matchInvocation.ArgumentList != null)
			{
				var arguments = matchInvocation.ArgumentList.Arguments;
				ExpressionSyntax? patternExpression = null;
				ExpressionSyntax? optionsExpression = null;

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
						if (parameter.Name == "pattern")
						{
							patternExpression = argument.Expression;
						}
						else if (parameter.Name == "options")
						{
							optionsExpression = argument.Expression;
						}
					}
				}

				if (patternExpression != null)
				{
					var patternConstant = semanticModel.GetConstantValue(patternExpression, cancellationToken);
					if (patternConstant.HasValue && patternConstant.Value is string patternString)
					{
						pattern = patternString;
					}

					if (optionsExpression != null)
					{
						var optionsConstant = semanticModel.GetConstantValue(optionsExpression, cancellationToken);
						if (optionsConstant.HasValue && optionsConstant.Value is int optionsValue)
						{
							options = (RegexOptions)optionsValue;
						}
					}
				}
			}
		}
		else
		{
			var receiver = matchInvocation.Expression;
			if (receiver is MemberAccessExpressionSyntax receiverMemberAccess)
			{
				receiver = receiverMemberAccess.Expression;
			}

			var regexSymbol = semanticModel.GetSymbolInfo(receiver, cancellationToken).Symbol;
			if (regexSymbol != null)
			{
				ExpressionSyntax? regexAssignExpression = null;

				if (regexSymbol is ILocalSymbol regexLocalSymbol)
				{
					var syntaxReference = regexLocalSymbol.DeclaringSyntaxReferences.FirstOrDefault();
					if (syntaxReference != null)
					{
						var regexDeclarator = await syntaxReference.GetSyntaxAsync(cancellationToken).ConfigureAwait(false) as VariableDeclaratorSyntax;
						regexAssignExpression = regexDeclarator?.Initializer?.Value;
					}
				}
				else if (regexSymbol is IFieldSymbol regexFieldSymbol)
				{
					var syntaxReference = regexFieldSymbol.DeclaringSyntaxReferences.FirstOrDefault();
					if (syntaxReference != null)
					{
						var regexDeclarator = await syntaxReference.GetSyntaxAsync(cancellationToken).ConfigureAwait(false) as VariableDeclaratorSyntax;
						regexAssignExpression = regexDeclarator?.Initializer?.Value;
					}
				}
				else if (regexSymbol is IPropertySymbol regexPropertySymbol)
				{
					var syntaxReference = regexPropertySymbol.DeclaringSyntaxReferences.FirstOrDefault();
					if (syntaxReference != null)
					{
						var regexDeclarator = await syntaxReference.GetSyntaxAsync(cancellationToken).ConfigureAwait(false) as PropertyDeclarationSyntax;
						regexAssignExpression = regexDeclarator?.ExpressionBody?.Expression;
					}
				}
				else if (regexSymbol is IMethodSymbol regexMethodSymbol)
				{
					var syntaxReference = regexMethodSymbol.DeclaringSyntaxReferences.FirstOrDefault();
					if (syntaxReference != null)
					{
						var methodDeclaration = await syntaxReference.GetSyntaxAsync(cancellationToken).ConfigureAwait(false) as MethodDeclarationSyntax;
						if (methodDeclaration != null)
						{
							foreach (var attributeList in methodDeclaration.AttributeLists)
							{
								foreach (var attribute in attributeList.Attributes)
								{
									var attributeSymbol = semanticModel.GetSymbolInfo(attribute, cancellationToken).Symbol as IMethodSymbol;
									if (attributeSymbol != null && attributeSymbol.ContainingType?.ToDisplayString() == "System.Text.RegularExpressions.GeneratedRegexAttribute")
									{
										if (attribute.ArgumentList != null)
										{
											var attributeArguments = attribute.ArgumentList.Arguments;
											if (attributeArguments.Count > 0)
											{
												var patternExpression = attributeArguments[0].Expression;
												var patternConstant = semanticModel.GetConstantValue(patternExpression, cancellationToken);
												if (patternConstant.HasValue && patternConstant.Value is string patternString)
												{
													pattern = patternString;
												}

												if (attributeArguments.Count > 1)
												{
													var optionsConstant = semanticModel.GetConstantValue(attributeArguments[1].Expression, cancellationToken);
													if (optionsConstant.HasValue && optionsConstant.Value is int optionsValue)
													{
														options = (RegexOptions)optionsValue;
													}
												}
											}
										}
									}
								}
							}
						}
					}
				}

				if (regexAssignExpression is ObjectCreationExpressionSyntax regexObjectCreation)
				{
					var regexConstructorSymbol = semanticModel.GetSymbolInfo(regexObjectCreation, cancellationToken).Symbol as IMethodSymbol;
					if (regexConstructorSymbol != null && regexConstructorSymbol.ContainingType?.ToDisplayString() == "System.Text.RegularExpressions.Regex")
					{
						var regexParameters = regexConstructorSymbol.Parameters;
						if (regexObjectCreation.ArgumentList != null)
						{
							var regexArguments = regexObjectCreation.ArgumentList.Arguments;
							ExpressionSyntax? patternExpression = null;
							ExpressionSyntax? optionsExpression = null;

							for (int i = 0; i < regexParameters.Length; i++)
							{
								var parameter = regexParameters[i];
								var argument = regexArguments.FirstOrDefault(arg => arg.NameColon?.Name.Identifier.ValueText == parameter.Name);
								if (argument == null && i < regexArguments.Count)
								{
									argument = regexArguments[i];
								}

								if (argument != null)
								{
									if (parameter.Name == "pattern")
									{
										patternExpression = argument.Expression;
									}
									else if (parameter.Name == "options")
									{
										optionsExpression = argument.Expression;
									}
								}
							}

							if (patternExpression != null)
							{
								var patternConstant = semanticModel.GetConstantValue(patternExpression, cancellationToken);
								if (patternConstant.HasValue && patternConstant.Value is string patternString)
								{
									pattern = patternString;
								}

								if (optionsExpression != null)
								{
									var optionsConstant = semanticModel.GetConstantValue(optionsExpression, cancellationToken);
									if (optionsConstant.HasValue && optionsConstant.Value is int optionsValue)
									{
										options = (RegexOptions)optionsValue;
									}
								}
							}
						}
					}
				}
			}
		}

		if (pattern == null)
		{
			return fallbackName;
		}

		try
		{
			var regex = new Regex(pattern, options);
			var groupNames = regex.GetGroupNames();
			if (index >= 0 && index < groupNames.Length)
			{
				var groupName = groupNames[index];
				if (int.TryParse(groupName, out _))
				{
					return fallbackName;
				}
				return groupName;
			}
		}
		catch (ArgumentException)
		{
			// Ignore invalid regex syntax
		}

		return fallbackName;
	}

	private static string GetFallbackName(ElementAccessExpressionSyntax elementAccess, int index)
	{
		SyntaxNode current = elementAccess;
		while (current.Parent is MemberAccessExpressionSyntax memberAccess && memberAccess.Expression == current)
		{
			current = memberAccess;
		}

		var parent = current.Parent;
		if (parent is EqualsValueClauseSyntax equalsValue && equalsValue.Parent is VariableDeclaratorSyntax declarator)
		{
			var name = declarator.Identifier.ValueText;
			if (IsValidGroupName(name))
			{
				return name;
			}
		}
		else if (parent is AssignmentExpressionSyntax assignment && assignment.Right == current)
		{
			if (assignment.Left is IdentifierNameSyntax identifier)
			{
				var name = identifier.Identifier.ValueText;
				if (IsValidGroupName(name))
				{
					return name;
				}
			}
		}

		return $"group{index}";
	}

	private static bool IsValidGroupName(string name)
	{
		if (string.IsNullOrEmpty(name))
		{
			return false;
		}

		if (string.Equals(name, "group", StringComparison.OrdinalIgnoreCase) ||
			string.Equals(name, "match", StringComparison.OrdinalIgnoreCase) ||
			string.Equals(name, "regex", StringComparison.OrdinalIgnoreCase))
		{
			return false;
		}

		if (!char.IsLetter(name[0]))
		{
			return false;
		}

		foreach (var character in name)
		{
			if (!char.IsLetterOrDigit(character))
			{
				return false;
			}
		}

		return true;
	}
}
