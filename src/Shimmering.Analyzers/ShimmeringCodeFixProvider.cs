namespace Shimmering.Analyzers;

/// <summary>
/// A base <see cref="CodeFixProvider"/> class in this project.
/// </summary>
[Shared]
public abstract class ShimmeringCodeFixProvider : CodeFixProvider
{
	/// <summary>
	/// Gets the minimal code that the <see cref="ShimmeringCodeFixProvider"/> creates from the corresponding analyzer's SampleCode.
	/// </summary>
	public abstract string SampleCodeFixed { get; }

	public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;
}
