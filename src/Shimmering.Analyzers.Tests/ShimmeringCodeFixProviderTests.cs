using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

using NUnit.Framework;

using Shimmering.Analyzers;
using Shimmering.Analyzers.Core;

namespace Shimmering.Analyzers.Tests;

/// <summary>
/// A base class for code fix tests.
/// </summary>
/// <typeparam name="TAnalyzer">The type of the analyzer to test.</typeparam>
/// <typeparam name="TCodeFixProvider">The type of the code fix provider to test.</typeparam>
public abstract class ShimmeringCodeFixProviderTests<TAnalyzer, TCodeFixProvider>
	where TAnalyzer : ShimmeringAnalyzer, new()
	where TCodeFixProvider : ShimmeringCodeFixProvider, new()
{
	protected static Task VerifyCodeFixAsync(string source, string fixedSource)
	{
		var test = new CSharpCodeFixTest<TAnalyzer, TCodeFixProvider, DefaultVerifier>
		{
			TestCode = source.Replace("\r\n", "\n").Replace("\n", Environment.NewLine),
			FixedCode = fixedSource.Replace("\r\n", "\n").Replace("\n", Environment.NewLine),
			ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
		};

		return test.RunAsync();
	}

	protected static Task VerifyAnalyzerAsync(string source, params DiagnosticResult[] expected)
	{
		var test = new CSharpAnalyzerTest<TAnalyzer, DefaultVerifier>
		{
			TestCode = source.Replace("\r\n", "\n").Replace("\n", Environment.NewLine),
			ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
		};

		test.ExpectedDiagnostics.AddRange(expected);
		return test.RunAsync();
	}
}
