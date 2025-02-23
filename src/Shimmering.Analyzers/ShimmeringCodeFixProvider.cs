namespace Shimmering.Analyzers;

/// <summary>
/// A base <see cref="CodeFixProvider"/> class in this project.
/// </summary>
[Shared]
public abstract class ShimmeringCodeFixProvider : CodeFixProvider
{
	public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;
}
