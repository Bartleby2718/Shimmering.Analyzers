using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;

using NUnit.Framework;

using Shimmering.Analyzers.Core;

namespace Shimmering.Analyzers.Tests;

/// <summary>
/// A base class for analyzer tests.
/// </summary>
/// <typeparam name="TAnalyzer">The type of the analyzer to test.</typeparam>
public abstract class ShimmeringAnalyzerTests<TAnalyzer>
	where TAnalyzer : ShimmeringAnalyzer, new()
{
	/// <summary>
	/// Tests if <see cref="ShimmeringAnalyzer.SampleCode"/> is flagged by <typeparamref name="TAnalyzer"/>.
	/// </summary>
	[Test]
	public Task TestSampleCode()
	{
		return VerifyAnalyzerAsync(new TAnalyzer().SampleCode);
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
