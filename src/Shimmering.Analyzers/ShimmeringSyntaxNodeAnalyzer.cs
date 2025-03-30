using System.Diagnostics;

namespace Shimmering.Analyzers;

/// <summary>
/// A base <see cref="DiagnosticAnalyzer"/> class in this project, used for <see cref="SyntaxNode"/> analysis.
/// </summary>
public abstract class ShimmeringSyntaxNodeAnalyzer : ShimmeringAnalyzer
{
	public sealed override void Initialize(AnalysisContext context)
	{
		context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
		context.EnableConcurrentExecution();
		this.RegisterSyntaxNodeAction(context);
	}

	public abstract void RegisterSyntaxNodeAction(AnalysisContext context);

	protected static DiagnosticDescriptor CreateRule(
		string id,
		LocalizableString title,
		LocalizableString messageFormat,
		string category,
		DiagnosticSeverity defaultSeverity,
		LocalizableString? description = null) => new(
		id,
		title,
		messageFormat,
		category,
		defaultSeverity,
		isEnabledByDefault: true,
		description,
		helpLinkUri: category is "ShimmeringUsage" or "ShimmeringStyle"
			? $"https://github.com/Bartleby2718/Shimmering.Analyzers/blob/main/docs/{category.Substring(10)}Rules/{id}.md"
			: throw new UnreachableException($"The only supported categories are 'ShimmeringUsage' and 'ShimmeringStyle', but received '{category}'"));
}
