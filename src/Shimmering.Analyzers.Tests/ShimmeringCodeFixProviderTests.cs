using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;

namespace Shimmering.Analyzers.Tests;

/// <summary>
/// A base class for code fix tests.
/// </summary>
/// <typeparam name="TAnalyzer">The type of the analyzer to test.</typeparam>
/// <typeparam name="TCodeFixProvider">The type of the code fix provider to test.</typeparam>
public abstract class ShimmeringCodeFixProviderTests<TAnalyzer, TCodeFixProvider>
	where TAnalyzer : DiagnosticAnalyzer, new()
	where TCodeFixProvider : CodeFixProvider, new()
{
	protected static Task VerifyCodeFixAsync(string source, string fixedSource) =>
		CSharpCodeFixVerifier<TAnalyzer, TCodeFixProvider, DefaultVerifier>.VerifyCodeFixAsync(source, fixedSource);
}
