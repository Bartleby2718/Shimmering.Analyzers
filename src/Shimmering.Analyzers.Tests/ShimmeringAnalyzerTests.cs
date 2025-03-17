namespace Shimmering.Analyzers.Tests;

public abstract class ShimmeringAnalyzerTests<TAnalyzer>
	where TAnalyzer : ShimmeringSyntaxNodeAnalyzer, new()
{
	/// <summary>
	/// Tests if <typeparamref name="TAnalyzer.SampleCode"/> is flagged by <typeparamref name="TAnalyzer"/>.
	/// </summary>
	[Test]
	public Task TestSampleCode() => CSharpAnalyzerVerifier<TAnalyzer, DefaultVerifier>.VerifyAnalyzerAsync(new TAnalyzer().SampleCode);
}
