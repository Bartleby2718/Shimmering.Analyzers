using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Shimmering.Analyzers.Core;

namespace Shimmering.Analyzers.Analyzers.Usage;

/// <summary>
/// Flags unnamed capturing groups in Regex patterns.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UnnamedRegexCaptureGroupAnalyzer : ShimmeringAnalyzer
{
	private const string Title = "Regex pattern contains unnamed capturing group";
	private const string MessageFormat = "Regex pattern contains {0} unnamed capturing group(s). Use (?<name>...) to prevent brittle positional indexing.";
	private const string Category = RuleCategories.Usage;

	private static readonly DiagnosticDescriptor Rule = ShimmeringRuleFactory.Create(
		DiagnosticIds.UsageRules.UnnamedRegexCaptureGroup,
		Title,
		MessageFormat,
		Category,
		DiagnosticSeverity.Warning);

	public override string SampleCode => """
		using System.Text.RegularExpressions;

		class Test
		{
			void Do()
			{
				var regex = [|new Regex(@"(\d{4})-(\d{2})-(\d{2})")|];
			}
		}
		""";

	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

	protected override void InitializeCore(AnalysisContext context)
	{
		context.RegisterCompilationStartAction(compilationContext =>
		{
			var compilation = compilationContext.Compilation;
			var regexType = compilation.GetTypeByMetadataName("System.Text.RegularExpressions.Regex");
			var generatedRegexAttributeType = compilation.GetTypeByMetadataName("System.Text.RegularExpressions.GeneratedRegexAttribute");

			var compilationState = new CompilationState(regexType, generatedRegexAttributeType);

			compilationContext.RegisterSyntaxNodeAction(syntaxContext => AnalyzeObjectCreation(syntaxContext, compilationState), SyntaxKind.ObjectCreationExpression);
			compilationContext.RegisterSyntaxNodeAction(syntaxContext => AnalyzeInvocation(syntaxContext, compilationState), SyntaxKind.InvocationExpression);
			compilationContext.RegisterSyntaxNodeAction(syntaxContext => AnalyzeAttribute(syntaxContext, compilationState), SyntaxKind.Attribute);
		});
	}

	private static void AnalyzeObjectCreation(SyntaxNodeAnalysisContext context, CompilationState compilationState)
	{
		var objectCreation = (ObjectCreationExpressionSyntax)context.Node;
		var constructorSymbol = context.SemanticModel.GetSymbolInfo(objectCreation, context.CancellationToken).Symbol as IMethodSymbol;
		if (constructorSymbol == null || !SymbolEqualityComparer.Default.Equals(constructorSymbol.ContainingType, compilationState.RegexType))
		{
			return;
		}

		var parameters = constructorSymbol.Parameters;
		ExpressionSyntax? patternExpression = null;
		ExpressionSyntax? optionsExpression = null;

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

		if (patternExpression != null)
		{
			AnalyzePatternArgument(context, patternExpression, objectCreation.GetLocation(), optionsExpression);
		}
	}

	private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context, CompilationState compilationState)
	{
		var invocation = (InvocationExpressionSyntax)context.Node;
		var methodSymbol = context.SemanticModel.GetSymbolInfo(invocation, context.CancellationToken).Symbol as IMethodSymbol;
		if (methodSymbol == null || !methodSymbol.IsStatic || !SymbolEqualityComparer.Default.Equals(methodSymbol.ContainingType, compilationState.RegexType))
		{
			return;
		}

		var parameters = methodSymbol.Parameters;
		ExpressionSyntax? patternExpression = null;
		ExpressionSyntax? optionsExpression = null;

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

		if (patternExpression != null)
		{
			AnalyzePatternArgument(context, patternExpression, invocation.GetLocation(), optionsExpression);
		}
	}

	private static void AnalyzeAttribute(SyntaxNodeAnalysisContext context, CompilationState compilationState)
	{
		var attribute = (AttributeSyntax)context.Node;
		var attributeSymbol = context.SemanticModel.GetSymbolInfo(attribute, context.CancellationToken).Symbol as IMethodSymbol;
		if (attributeSymbol == null || !SymbolEqualityComparer.Default.Equals(attributeSymbol.ContainingType, compilationState.GeneratedRegexAttributeType))
		{
			return;
		}

		var parameters = attributeSymbol.Parameters;
		ExpressionSyntax? patternExpression = null;
		ExpressionSyntax? optionsExpression = null;

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

		if (patternExpression != null)
		{
			AnalyzePatternArgument(context, patternExpression, attribute.GetLocation(), optionsExpression);
		}
	}

	private static void AnalyzePatternArgument(
		SyntaxNodeAnalysisContext context,
		ExpressionSyntax patternExpression,
		Location location,
		ExpressionSyntax? optionsExpression)
	{
		var patternConstant = context.SemanticModel.GetConstantValue(patternExpression, context.CancellationToken);
		if (!patternConstant.HasValue || patternConstant.Value is not string pattern)
		{
			return;
		}

		var options = RegexOptions.None;
		if (optionsExpression != null)
		{
			var optionsConstant = context.SemanticModel.GetConstantValue(optionsExpression, context.CancellationToken);
			if (optionsConstant.HasValue && optionsConstant.Value is int intValue)
			{
				options = (RegexOptions)intValue;
			}
		}

		try
		{
			var regex = new Regex(pattern, options);
			var groupNames = regex.GetGroupNames();
			int unnamedCount = 0;

			foreach (var name in groupNames)
			{
				if (int.TryParse(name, out var index) && index > 0)
				{
					unnamedCount++;
				}
			}

			if (unnamedCount > 0)
			{
				var diagnostic = Diagnostic.Create(
					Rule,
					location,
					unnamedCount.ToString());
				context.ReportDiagnostic(diagnostic);
			}
		}
		catch (ArgumentException)
		{
			// Ignore invalid regex syntax
		}
	}

	private sealed class CompilationState
	{
		public CompilationState(INamedTypeSymbol? regexType, INamedTypeSymbol? generatedRegexAttributeType)
		{
			this.RegexType = regexType;
			this.GeneratedRegexAttributeType = generatedRegexAttributeType;
		}

		public INamedTypeSymbol? RegexType { get; }
		public INamedTypeSymbol? GeneratedRegexAttributeType { get; }
	}
}
