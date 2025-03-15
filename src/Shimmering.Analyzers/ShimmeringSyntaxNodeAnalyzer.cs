using System.Diagnostics;

namespace Shimmering.Analyzers;

/// <summary>
/// A base <see cref="DiagnosticAnalyzer"/> class in this project, used for <see cref="SyntaxNode"/> analysis.
/// </summary>
public abstract class ShimmeringSyntaxNodeAnalyzer : DiagnosticAnalyzer
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
		bool isEnabledByDefault = true,
		LocalizableString? description = null) => new(
		id,
		title,
		messageFormat,
		category,
		defaultSeverity,
		isEnabledByDefault,
		description,
		helpLinkUri: category is "Usage" or "Style"
			? $"https://github.com/Bartleby2718/Shimmering.Analyzers/blob/main/docs/{category}Rules/{id}.md"
			: throw new UnreachableException($"The only supported categories are 'Usage' and 'Style', but received '{category}'"));
}
