using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Shimmering.Analyzers.Core;

namespace Shimmering.Analyzers.Analyzers.Usage;

/// <summary>
/// Recommends using a read-only collection interface instead of a mutable collection type for parameters when the collection is not mutated.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UseReadOnlyCollectionParameterAnalyzer : ShimmeringAnalyzer
{
	private const string Title = "Use read-only collection interface for parameter";
	private const string MessageFormat = "Parameter '{0}' is typed as '{1}' but is never mutated. Consider using '{2}' to broaden caller compatibility.";
	private const string Category = RuleCategories.Usage;

	private static readonly DiagnosticDescriptor Rule = ShimmeringRuleFactory.Create(
		DiagnosticIds.UsageRules.UseReadOnlyCollectionParameter,
		Title,
		MessageFormat,
		Category,
		DiagnosticSeverity.Info);

	private static readonly HashSet<string> MutatingMethodNames = new(StringComparer.Ordinal)
	{
		"Add", "AddRange", "Clear", "Insert", "InsertRange", "Remove", "RemoveAll", "RemoveAt",
		"RemoveRange", "RemoveWhere", "Reverse", "Sort", "TrimExcess", "TryAdd", "TryRemove", "TryUpdate",
		"Enqueue", "Dequeue", "Push", "Pop",
	};

	public override string SampleCode => """
		using System;
		using System.Collections.Generic;

		public class ReportGenerator
		{
			public void PrintReport([|List<string> items|])
			{
				foreach (var item in items)
				{
					Console.WriteLine(item);
				}
			}
		}
		""";

	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

	protected override void InitializeCore(AnalysisContext context)
	{
		context.RegisterCompilationStartAction(compilationContext =>
		{
			var compilation = compilationContext.Compilation;

			var listSymbol = compilation.GetTypeByMetadataName("System.Collections.Generic.List`1");
			var interfaceListSymbol = compilation.GetTypeByMetadataName("System.Collections.Generic.IList`1");
			var interfaceCollectionSymbol = compilation.GetTypeByMetadataName("System.Collections.Generic.ICollection`1");
			var dictionarySymbol = compilation.GetTypeByMetadataName("System.Collections.Generic.Dictionary`2");
			var interfaceDictionarySymbol = compilation.GetTypeByMetadataName("System.Collections.Generic.IDictionary`2");
			var hashSetSymbol = compilation.GetTypeByMetadataName("System.Collections.Generic.HashSet`1");
			var interfaceSetSymbol = compilation.GetTypeByMetadataName("System.Collections.Generic.ISet`1");
			var collectionSymbol = compilation.GetTypeByMetadataName("System.Collections.ObjectModel.Collection`1");

			var readOnlyListSymbol = compilation.GetTypeByMetadataName("System.Collections.Generic.IReadOnlyList`1");
			var readOnlyCollectionSymbol = compilation.GetTypeByMetadataName("System.Collections.Generic.IReadOnlyCollection`1");
			var readOnlyDictionarySymbol = compilation.GetTypeByMetadataName("System.Collections.Generic.IReadOnlyDictionary`2");
			var readOnlySetSymbol = compilation.GetTypeByMetadataName("System.Collections.Generic.IReadOnlySet`1");

			var targetInterfaceMap = new Dictionary<ISymbol, INamedTypeSymbol>(SymbolEqualityComparer.Default);
			if (listSymbol != null && readOnlyListSymbol != null)
			{
				targetInterfaceMap[listSymbol] = readOnlyListSymbol;
			}
			if (interfaceListSymbol != null && readOnlyListSymbol != null)
			{
				targetInterfaceMap[interfaceListSymbol] = readOnlyListSymbol;
			}
			if (interfaceCollectionSymbol != null && readOnlyCollectionSymbol != null)
			{
				targetInterfaceMap[interfaceCollectionSymbol] = readOnlyCollectionSymbol;
			}
			if (dictionarySymbol != null && readOnlyDictionarySymbol != null)
			{
				targetInterfaceMap[dictionarySymbol] = readOnlyDictionarySymbol;
			}
			if (interfaceDictionarySymbol != null && readOnlyDictionarySymbol != null)
			{
				targetInterfaceMap[interfaceDictionarySymbol] = readOnlyDictionarySymbol;
			}
			if (hashSetSymbol != null && readOnlySetSymbol != null)
			{
				targetInterfaceMap[hashSetSymbol] = readOnlySetSymbol;
			}
			if (interfaceSetSymbol != null && readOnlySetSymbol != null)
			{
				targetInterfaceMap[interfaceSetSymbol] = readOnlySetSymbol;
			}
			if (collectionSymbol != null && readOnlyCollectionSymbol != null)
			{
				targetInterfaceMap[collectionSymbol] = readOnlyCollectionSymbol;
			}

			var mutableInterfacesBuilder = ImmutableArray.CreateBuilder<INamedTypeSymbol>();
			if (interfaceCollectionSymbol != null)
			{
				mutableInterfacesBuilder.Add(interfaceCollectionSymbol);
			}
			if (interfaceListSymbol != null)
			{
				mutableInterfacesBuilder.Add(interfaceListSymbol);
			}
			if (interfaceDictionarySymbol != null)
			{
				mutableInterfacesBuilder.Add(interfaceDictionarySymbol);
			}
			if (interfaceSetSymbol != null)
			{
				mutableInterfacesBuilder.Add(interfaceSetSymbol);
			}
			var mutableInterfaces = mutableInterfacesBuilder.ToImmutable();

			var compilationState = new CompilationState(targetInterfaceMap, mutableInterfaces);

			compilationContext.RegisterSyntaxNodeAction(syntaxContext => AnalyzeMethod(syntaxContext, compilationState), SyntaxKind.MethodDeclaration);
			compilationContext.RegisterSyntaxNodeAction(syntaxContext => AnalyzeConstructor(syntaxContext, compilationState), SyntaxKind.ConstructorDeclaration);
		});
	}

	private static void AnalyzeMethod(SyntaxNodeAnalysisContext context, CompilationState compilationState)
	{
		var methodDeclaration = (MethodDeclarationSyntax)context.Node;
		var methodSymbol = context.SemanticModel.GetDeclaredSymbol(methodDeclaration, context.CancellationToken);
		if (methodSymbol == null)
		{
			return;
		}

		if (methodSymbol.IsOverride || methodSymbol.IsVirtual || methodSymbol.IsAbstract)
		{
			return;
		}

		if (methodSymbol.ExplicitInterfaceImplementations.Any())
		{
			return;
		}

		if (methodSymbol.ContainingType?.TypeKind == TypeKind.Interface)
		{
			return;
		}

		var containingType = methodSymbol.ContainingType;
		if (containingType != null)
		{
			foreach (var interfaceSymbol in containingType.AllInterfaces)
			{
				foreach (var member in interfaceSymbol.GetMembers())
				{
					var implementation = containingType.FindImplementationForInterfaceMember(member);
					if (SymbolEqualityComparer.Default.Equals(implementation, methodSymbol))
					{
						return;
					}
				}
			}
		}

		var body = (SyntaxNode?)methodDeclaration.Body ?? methodDeclaration.ExpressionBody;
		if (body == null)
		{
			return;
		}

		AnalyzeParameterList(context, methodSymbol, methodDeclaration.ParameterList, body, compilationState);
	}

	private static void AnalyzeConstructor(SyntaxNodeAnalysisContext context, CompilationState compilationState)
	{
		var constructorDeclaration = (ConstructorDeclarationSyntax)context.Node;
		var constructorSymbol = context.SemanticModel.GetDeclaredSymbol(constructorDeclaration, context.CancellationToken);
		if (constructorSymbol == null)
		{
			return;
		}

		var body = (SyntaxNode?)constructorDeclaration.Body ?? constructorDeclaration.ExpressionBody;
		if (body == null)
		{
			return;
		}

		AnalyzeParameterList(context, constructorSymbol, constructorDeclaration.ParameterList, body, compilationState);
	}

	private static void AnalyzeParameterList(
		SyntaxNodeAnalysisContext context,
		IMethodSymbol methodSymbol,
		ParameterListSyntax parameterList,
		SyntaxNode body,
		CompilationState compilationState)
	{
		var options = context.Options.AnalyzerConfigOptionsProvider.GetOptions(context.Node.SyntaxTree);
		var allowedAccessibilities = GetAllowedAccessibilities(options);

		if (!allowedAccessibilities.Contains(methodSymbol.DeclaredAccessibility))
		{
			return;
		}

		var compilation = context.Compilation;
		var dataFlowAnalysis = context.SemanticModel.AnalyzeDataFlow(body);
		if (dataFlowAnalysis == null || !dataFlowAnalysis.Succeeded)
		{
			return;
		}

		foreach (var parameterSyntax in parameterList.Parameters)
		{
			var parameterSymbol = context.SemanticModel.GetDeclaredSymbol(parameterSyntax, context.CancellationToken);
			if (parameterSymbol == null)
			{
				continue;
			}

			if (parameterSymbol.Type is not INamedTypeSymbol parameterType)
			{
				continue;
			}

			var originalDefinition = parameterType.OriginalDefinition;
			if (!compilationState.TargetInterfaceMap.TryGetValue(originalDefinition, out var targetInterfaceSymbol))
			{
				continue;
			}

			var concreteTargetInterface = targetInterfaceSymbol.Construct(parameterType.TypeArguments.ToArray());

			if (dataFlowAnalysis.WrittenInside.Contains(parameterSymbol) || dataFlowAnalysis.CapturedInside.Contains(parameterSymbol))
			{
				continue;
			}

			var references = body.DescendantNodes()
				.OfType<IdentifierNameSyntax>()
				.Where(identifier => SymbolEqualityComparer.Default.Equals(context.SemanticModel.GetSymbolInfo(identifier, context.CancellationToken).Symbol, parameterSymbol))
				.ToList();

			bool isMutatedOrEscaped = false;
			bool isFixable = true;

			foreach (var reference in references)
			{
				if (IsMutationOrEscape(reference, parameterSymbol, context, compilationState, out var isEscape))
				{
					isMutatedOrEscaped = true;
					break;
				}

				if (reference.Parent is MemberAccessExpressionSyntax memberAccess && memberAccess.Expression == reference)
				{
					if (!IsMemberAccessCompatible(memberAccess, concreteTargetInterface, context.SemanticModel, compilation, context.CancellationToken))
					{
						isFixable = false;
					}
				}
				else if (reference.Parent is ElementAccessExpressionSyntax elementAccess && elementAccess.Expression == reference)
				{
					if (!HasIndexer(concreteTargetInterface))
					{
						isFixable = false;
					}
				}
			}

			if (isMutatedOrEscaped)
			{
				continue;
			}

			var properties = ImmutableDictionary<string, string?>.Empty;
			if (isFixable)
			{
				properties = properties.Add("IsFixable", "true");
			}

			var location = parameterSyntax.GetLocation();
			var diagnostic = Diagnostic.Create(
				Rule,
				location,
				properties,
				parameterSymbol.Name,
				parameterType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat),
				concreteTargetInterface.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat));

			context.ReportDiagnostic(diagnostic);
		}
	}

	private static HashSet<Accessibility> GetAllowedAccessibilities(AnalyzerConfigOptions options)
	{
		var allowedAccessibilities = new HashSet<Accessibility>();
		if (options.TryGetValue("dotnet_code_quality.SHIMMER1060.api_surface", out var apiSurface) ||
			options.TryGetValue("dotnet_diagnostic.SHIMMER1060.api_surface", out apiSurface))
		{
			var parts = apiSurface.Split(',');
			foreach (var part in parts)
			{
				var trimmed = part.Trim();
				if (string.Equals(trimmed, "public", StringComparison.OrdinalIgnoreCase))
				{
					allowedAccessibilities.Add(Accessibility.Public);
				}
				else if (string.Equals(trimmed, "protected", StringComparison.OrdinalIgnoreCase))
				{
					allowedAccessibilities.Add(Accessibility.Protected);
					allowedAccessibilities.Add(Accessibility.ProtectedOrInternal);
					allowedAccessibilities.Add(Accessibility.ProtectedAndInternal);
				}
				else if (string.Equals(trimmed, "internal", StringComparison.OrdinalIgnoreCase))
				{
					allowedAccessibilities.Add(Accessibility.Internal);
					allowedAccessibilities.Add(Accessibility.ProtectedOrInternal);
				}
				else if (string.Equals(trimmed, "private", StringComparison.OrdinalIgnoreCase))
				{
					allowedAccessibilities.Add(Accessibility.Private);
				}
			}
		}
		else
		{
			allowedAccessibilities.Add(Accessibility.Public);
			allowedAccessibilities.Add(Accessibility.Protected);
			allowedAccessibilities.Add(Accessibility.ProtectedOrInternal);
			allowedAccessibilities.Add(Accessibility.ProtectedAndInternal);
		}

		return allowedAccessibilities;
	}

	private static bool IsMutationOrEscape(
		IdentifierNameSyntax reference,
		IParameterSymbol parameterSymbol,
		SyntaxNodeAnalysisContext context,
		CompilationState compilationState,
		out bool isEscape)
	{
		isEscape = false;
		var parent = reference.Parent;
		if (parent == null)
		{
			return false;
		}

		if (IsLeftOfAssignmentOrIncrementDecrement(reference))
		{
			return true;
		}

		if (parent is MemberAccessExpressionSyntax memberAccess && memberAccess.Expression == reference)
		{
			if (IsLeftOfAssignmentOrIncrementDecrement(memberAccess))
			{
				return true;
			}

			var methodName = memberAccess.Name.Identifier.ValueText;
			if (MutatingMethodNames.Contains(methodName))
			{
				return true;
			}
		}

		if (parent is ElementAccessExpressionSyntax elementAccess && elementAccess.Expression == reference)
		{
			if (IsLeftOfAssignmentOrIncrementDecrement(elementAccess))
			{
				return true;
			}
		}

		if (parent is ArgumentSyntax argument)
		{
			var correspondingParameter = FindCorrespondingParameter(argument, context.SemanticModel, context.CancellationToken);
			if (correspondingParameter != null && IsMutableCollectionType(correspondingParameter.Type, compilationState))
			{
				isEscape = true;
				return true;
			}
		}

		return false;
	}

	private static bool IsLeftOfAssignmentOrIncrementDecrement(ExpressionSyntax expression)
	{
		var parent = expression.Parent;
		if (parent is AssignmentExpressionSyntax assignment && assignment.Left == expression)
		{
			return true;
		}
		if (parent is PostfixUnaryExpressionSyntax postfix &&
			(postfix.IsKind(SyntaxKind.PostIncrementExpression) || postfix.IsKind(SyntaxKind.PostDecrementExpression)) &&
			postfix.Operand == expression)
		{
			return true;
		}
		if (parent is PrefixUnaryExpressionSyntax prefix &&
			(prefix.IsKind(SyntaxKind.PreIncrementExpression) || prefix.IsKind(SyntaxKind.PreDecrementExpression)) &&
			prefix.Operand == expression)
		{
			return true;
		}
		return false;
	}

	private static IParameterSymbol? FindCorrespondingParameter(
		ArgumentSyntax argument,
		SemanticModel semanticModel,
		CancellationToken cancellationToken)
	{
		if (argument.Parent is not ArgumentListSyntax argumentList)
		{
			return null;
		}

		var parentNode = argumentList.Parent;
		if (parentNode == null)
		{
			return null;
		}

		ISymbol? memberSymbol = null;
		if (parentNode is InvocationExpressionSyntax invocation)
		{
			memberSymbol = semanticModel.GetSymbolInfo(invocation, cancellationToken).Symbol;
		}
		else if (parentNode is ObjectCreationExpressionSyntax objectCreation)
		{
			memberSymbol = semanticModel.GetSymbolInfo(objectCreation, cancellationToken).Symbol;
		}
		else if (parentNode is ElementAccessExpressionSyntax elementAccess)
		{
			memberSymbol = semanticModel.GetSymbolInfo(elementAccess, cancellationToken).Symbol;
		}

		if (memberSymbol is IMethodSymbol methodSymbol)
		{
			return GetParameterForArgument(argument, argumentList, methodSymbol.Parameters);
		}
		else if (memberSymbol is IPropertySymbol propertySymbol)
		{
			return GetParameterForArgument(argument, argumentList, propertySymbol.Parameters);
		}

		return null;
	}

	private static IParameterSymbol? GetParameterForArgument(
		ArgumentSyntax argument,
		ArgumentListSyntax argumentList,
		ImmutableArray<IParameterSymbol> parameters)
	{
		if (argument.NameColon != null)
		{
			var name = argument.NameColon.Name.Identifier.ValueText;
			return parameters.FirstOrDefault(parameter => string.Equals(parameter.Name, name, StringComparison.Ordinal));
		}

		var index = argumentList.Arguments.IndexOf(argument);
		if (index < 0)
		{
			return null;
		}

		if (index < parameters.Length)
		{
			return parameters[index];
		}

		if (parameters.Length > 0 && parameters[parameters.Length - 1].IsParams)
		{
			return parameters[parameters.Length - 1];
		}

		return null;
	}

	private static bool IsMutableCollectionType(ITypeSymbol typeSymbol, CompilationState compilationState)
	{
		if (typeSymbol is not INamedTypeSymbol namedType)
		{
			return false;
		}

		var originalDefinition = namedType.OriginalDefinition;

		foreach (var mutableInterface in compilationState.MutableInterfaces)
		{
			if (SymbolEqualityComparer.Default.Equals(originalDefinition, mutableInterface))
			{
				return true;
			}

			if (namedType.AllInterfaces.Any(interfaceSymbol => SymbolEqualityComparer.Default.Equals(interfaceSymbol.OriginalDefinition, mutableInterface)))
			{
				return true;
			}
		}

		return false;
	}

	private static bool IsMemberAccessCompatible(
		MemberAccessExpressionSyntax memberAccess,
		INamedTypeSymbol targetInterface,
		SemanticModel semanticModel,
		Compilation compilation,
		CancellationToken cancellationToken)
	{
		var symbol = semanticModel.GetSymbolInfo(memberAccess, cancellationToken).Symbol;
		if (symbol == null)
		{
			return false;
		}

		if (symbol is IMethodSymbol methodSymbol && methodSymbol.IsExtensionMethod)
		{
			if (methodSymbol.Parameters.Length > 0)
			{
				var receiverType = methodSymbol.Parameters[0].Type;
				var conversion = compilation.ClassifyConversion(targetInterface, receiverType);
				return conversion.IsImplicit;
			}
			return false;
		}

		var memberName = memberAccess.Name.Identifier.ValueText;
		return TargetInterfaceHasMember(targetInterface, memberName);
	}

	private static bool TargetInterfaceHasMember(INamedTypeSymbol targetInterface, string memberName)
	{
		if (targetInterface.GetMembers(memberName).Any())
		{
			return true;
		}

		foreach (var interfaceSymbol in targetInterface.AllInterfaces)
		{
			if (interfaceSymbol.GetMembers(memberName).Any())
			{
				return true;
			}
		}

		return false;
	}

	private static bool HasIndexer(INamedTypeSymbol typeSymbol)
	{
		if (typeSymbol.GetMembers().OfType<IPropertySymbol>().Any(property => property.IsIndexer))
		{
			return true;
		}
		foreach (var interfaceSymbol in typeSymbol.AllInterfaces)
		{
			if (interfaceSymbol.GetMembers().OfType<IPropertySymbol>().Any(property => property.IsIndexer))
			{
				return true;
			}
		}
		return false;
	}

	private sealed class CompilationState
	{
		public CompilationState(
			Dictionary<ISymbol, INamedTypeSymbol> targetInterfaceMap,
			ImmutableArray<INamedTypeSymbol> mutableInterfaces)
		{
			this.TargetInterfaceMap = targetInterfaceMap;
			this.MutableInterfaces = mutableInterfaces;
		}

		public Dictionary<ISymbol, INamedTypeSymbol> TargetInterfaceMap { get; }
		public ImmutableArray<INamedTypeSymbol> MutableInterfaces { get; }
	}
}
