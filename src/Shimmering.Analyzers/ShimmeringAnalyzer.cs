using System.Diagnostics;

namespace Shimmering.Analyzers;

/// <summary>
/// A base <see cref="DiagnosticAnalyzer"/> class in this project, used for <see cref="SyntaxNode"/> analysis.
/// </summary>
public abstract class ShimmeringAnalyzer : DiagnosticAnalyzer
{
	/// <summary>
	/// Gets the minimal code that the <see cref="ShimmeringSyntaxNodeAnalyzer"/> flags.
	/// </summary>
	public abstract string SampleCode { get; }

	public abstract override void Initialize(AnalysisContext context);
}
