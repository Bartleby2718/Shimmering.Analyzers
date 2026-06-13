using System.Diagnostics;

using Microsoft.CodeAnalysis;

namespace Shimmering.Analyzers.Core;

/// <summary>
/// A factory for creating <see cref="DiagnosticDescriptor"/> instances with standardized metadata and help links.
/// </summary>
public static class ShimmeringRuleFactory
{
	/// <summary>
	/// Creates a <see cref="DiagnosticDescriptor"/> for a rule.
	/// </summary>
	/// <param name="id">The diagnostic ID (e.g., SHIMMER1001).</param>
	/// <param name="title">The rule title.</param>
	/// <param name="messageFormat">The message format used when reporting the diagnostic.</param>
	/// <param name="category">The rule category (use <see cref="RuleCategories"/>).</param>
	/// <param name="defaultSeverity">The default severity of the rule.</param>
	/// <param name="description">An optional detailed description.</param>
	/// <param name="isEnabledByDefault">Whether the rule is enabled by default. Defaults to <see langword="true"/>.</param>
	/// <returns>A configured <see cref="DiagnosticDescriptor"/>.</returns>
	/// <exception cref="UnreachableException">Thrown if the category is not recognized.</exception>
	public static DiagnosticDescriptor Create(
		string id,
		LocalizableString title,
		LocalizableString messageFormat,
		string category,
		DiagnosticSeverity defaultSeverity,
		LocalizableString? description = null,
		bool isEnabledByDefault = true)
	{
		string helpLinkUri = category switch
		{
			RuleCategories.Usage => $"https://bartleby2718.github.io/Shimmering.Analyzers/docs/UsageRules/{id}",
			RuleCategories.Performance => $"https://bartleby2718.github.io/Shimmering.Analyzers/docs/UsageRules/{id}", // Performance rules are documented under UsageRules for now to match status quo
			RuleCategories.Style => $"https://bartleby2718.github.io/Shimmering.Analyzers/docs/StyleRules/{id}",
			_ => throw new UnreachableException($"The category '{category}' is not supported for help link generation."),
		};

		return new DiagnosticDescriptor(
			id,
			title,
			messageFormat,
			category,
			defaultSeverity,
			isEnabledByDefault,
			description,
			helpLinkUri);
	}
}
