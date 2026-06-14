namespace Shimmering.Analyzers.Core;

/// <summary>
/// A lightweight base class for all analyzers in this project.
/// </summary>
public abstract class ShimmeringAnalyzer : DiagnosticAnalyzer
{
	/// <summary>
	/// Gets the minimal code that this analyzer flags.
	/// </summary>
	public abstract string SampleCode { get; }

	public sealed override void Initialize(AnalysisContext context)
	{
		context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
		context.EnableConcurrentExecution();
		this.InitializeCore(context);
	}

	/// <summary>
	/// Initializes the core logic for the analyzer.
	/// </summary>
	protected abstract void InitializeCore(AnalysisContext context);
}
