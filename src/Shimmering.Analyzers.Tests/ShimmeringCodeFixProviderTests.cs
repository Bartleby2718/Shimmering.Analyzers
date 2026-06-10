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
	protected static Task VerifyCodeFixAsync(string source, string fixedSource)
	{
		var test = new CSharpCodeFixTest<TAnalyzer, TCodeFixProvider, DefaultVerifier>
		{
			TestCode = source,
			FixedCode = fixedSource,
			ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
		};

		return test.RunAsync();
	}

	protected static Task VerifyAnalyzerAsync(string source, params DiagnosticResult[] expected)
	{
		var test = new CSharpAnalyzerTest<TAnalyzer, DefaultVerifier>
		{
			TestCode = source,
			ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
		};

		test.ExpectedDiagnostics.AddRange(expected);
		return test.RunAsync();
	}
}
