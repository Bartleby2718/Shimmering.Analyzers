namespace Shimmering.Analyzers.Tests;

public abstract class ShimmeringCodeFixProviderTests<TAnalyzer, TCodeFixProvider>
	where TAnalyzer : ShimmeringSyntaxNodeAnalyzer, new()
	where TCodeFixProvider : ShimmeringCodeFixProvider, new()
{
	/// <summary>
	/// Tests if <typeparamref name="TAnalyzer.SampleCode"/> is replaced with <typeparamref name="TCodeFixProvider.SampleCodeFixed"/>.
	/// </summary>
	[Test]
	public Task TestSampleCode() => CSharpCodeFixVerifier<TAnalyzer, TCodeFixProvider, DefaultVerifier>.VerifyCodeFixAsync(
		new TAnalyzer().SampleCode,
		new TCodeFixProvider().SampleCodeFixed);
}
