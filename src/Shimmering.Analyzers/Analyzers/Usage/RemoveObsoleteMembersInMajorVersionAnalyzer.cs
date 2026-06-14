using System;
using System.Collections.Immutable;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

using Shimmering.Analyzers.Core;

namespace Shimmering.Analyzers.Analyzers.Usage;

/// <summary>
/// Flags obsolete public/protected members when building a stable major version release.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class RemoveObsoleteMembersInMajorVersionAnalyzer : ShimmeringAnalyzer
{
	private const string Title = "Remove obsolete members in major version releases";
	private const string Message = "Remove obsolete members in major version releases";
	private const string Category = RuleCategories.Usage;

	private static readonly DiagnosticDescriptor Rule = ShimmeringRuleFactory.Create(
		DiagnosticIds.UsageRules.RemoveObsoleteMembersInMajorVersion,
		Title,
		Message,
		Category,
		DiagnosticSeverity.Warning);

	public override string SampleCode => """
		using System;

		namespace Tests
		{
			public class Test
			{
				[Obsolete]
				public void [|OldMethod|]() {}
			}
		}
		""";

	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

	protected override void InitializeCore(AnalysisContext context)
	{
		context.RegisterCompilationStartAction(compilationContext =>
		{
			compilationContext.Options.AnalyzerConfigOptionsProvider.GlobalOptions
				.TryGetValue("build_property.IsPackable", out var isPackableString);

			compilationContext.Options.AnalyzerConfigOptionsProvider.GlobalOptions
				.TryGetValue("dotnet_code_quality.SHIMMER1050.apply_to_non_packable_projects", out var applyToNonPackableProjectsString);

			var isPackable = string.Equals(isPackableString, "true", StringComparison.OrdinalIgnoreCase);
			var optedIn = string.Equals(applyToNonPackableProjectsString, "true", StringComparison.OrdinalIgnoreCase);

			if (!isPackable && !optedIn)
			{
				return;
			}

			if (!TryGetPackageVersion(compilationContext.Compilation, compilationContext.Options, out var versionString))
			{
				return;
			}

			if (!IsStableMajorVersion(versionString))
			{
				return;
			}

			var exclusions = ReadExclusions(compilationContext.Options);
			compilationContext.RegisterSymbolAction(
				symbolContext => AnalyzeSymbol(symbolContext, exclusions),
				SymbolKind.NamedType,
				SymbolKind.Method,
				SymbolKind.Property,
				SymbolKind.Field,
				SymbolKind.Event);
		});
	}

	private static bool TryGetPackageVersion(Compilation compilation, AnalyzerOptions options, out string versionString)
	{
		versionString = string.Empty;

		if (options.AnalyzerConfigOptionsProvider.GlobalOptions.TryGetValue("build_property.PackageVersion", out var packageVersion)
			&& !string.IsNullOrWhiteSpace(packageVersion))
		{
			versionString = packageVersion.Trim();
			return true;
		}

		if (options.AnalyzerConfigOptionsProvider.GlobalOptions.TryGetValue("build_property.Version", out var version)
			&& !string.IsNullOrWhiteSpace(version))
		{
			versionString = version.Trim();
			return true;
		}

		var assembly = compilation.Assembly;
		var informationalVersionAttribute = assembly.GetAttributes().FirstOrDefault(attribute =>
			attribute.AttributeClass?.ToDisplayString() == "System.Reflection.AssemblyInformationalVersionAttribute");

		if (informationalVersionAttribute != null && informationalVersionAttribute.ConstructorArguments.Length > 0)
		{
			var value = informationalVersionAttribute.ConstructorArguments[0].Value?.ToString();
			if (!string.IsNullOrWhiteSpace(value))
			{
				versionString = value!.Trim();
				return true;
			}
		}

		return false;
	}

	private static bool IsStableMajorVersion(string versionString)
	{
		if (!System.Version.TryParse(versionString, out var version))
		{
			return false;
		}

		return version.Minor == 0 && version.Build == 0;
	}

	private static void AnalyzeSymbol(SymbolAnalysisContext context, ImmutableHashSet<string> exclusions)
	{
		var symbol = context.Symbol;

		if (!IsPubliclyReachable(symbol))
		{
			return;
		}

		if (IsExcluded(symbol, exclusions))
		{
			return;
		}

		var obsoleteAttribute = symbol.GetAttributes().FirstOrDefault(attribute =>
			attribute.AttributeClass?.ToDisplayString() == "System.ObsoleteAttribute");

		if (obsoleteAttribute != null)
		{
			foreach (var location in symbol.Locations)
			{
				if (location.IsInSource)
				{
					context.ReportDiagnostic(Diagnostic.Create(Rule, location));
				}
			}
		}
	}

	private static bool IsPubliclyReachable(ISymbol symbol)
	{
		var current = symbol;
		while (current != null)
		{
			if (current is INamespaceSymbol)
			{
				break;
			}

			var accessibility = current.DeclaredAccessibility;
			if (accessibility != Accessibility.Public
				&& accessibility != Accessibility.Protected
				&& accessibility != Accessibility.ProtectedOrInternal)
			{
				return false;
			}

			current = current.ContainingType;
		}

		return true;
	}

	private static ImmutableHashSet<string> ReadExclusions(AnalyzerOptions options)
	{
		if (options.AnalyzerConfigOptionsProvider.GlobalOptions.TryGetValue("dotnet_code_quality.SHIMMER1050.excluded_symbol_names", out var raw)
			&& !string.IsNullOrWhiteSpace(raw))
		{
			return raw.Split('|')
				.Select(symbolName => symbolName.Trim())
				.Where(symbolName => symbolName.Length > 0)
				.ToImmutableHashSet(StringComparer.Ordinal);
		}

		return ImmutableHashSet<string>.Empty;
	}

	private static bool IsExcluded(ISymbol symbol, ImmutableHashSet<string> exclusions)
	{
		if (exclusions.IsEmpty)
		{
			return false;
		}

		var containingNamespace = symbol.ContainingNamespace?.ToDisplayString() ?? string.Empty;
		foreach (var exclusion in exclusions)
		{
			if (exclusion.StartsWith("N:", StringComparison.Ordinal))
			{
				var excludedNamespace = exclusion.Substring(2);
				if (containingNamespace == excludedNamespace || containingNamespace.StartsWith(excludedNamespace + ".", StringComparison.Ordinal))
				{
					return true;
				}
			}
			else
			{
				var documentationCommentId = symbol.GetDocumentationCommentId();
				if (documentationCommentId != null && exclusion == documentationCommentId)
				{
					return true;
				}

				var currentType = symbol.ContainingType;
				while (currentType != null)
				{
					var typeId = currentType.GetDocumentationCommentId();
					if (typeId != null && exclusion == typeId)
					{
						return true;
					}
					currentType = currentType.ContainingType;
				}
			}
		}

		return false;
	}
}
