using BenchmarkDotNet.Attributes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Shimmering.Analyzers.Benchmark;

/// <summary>
/// A generic benchmark for any <see cref="ShimmeringSyntaxNodeAnalyzer"/> 
/// </summary>
public abstract class ShimmeringAnalyzerBenchmark<TAnalyzer>
    where TAnalyzer : ShimmeringSyntaxNodeAnalyzer, new() // ensures no reflection is needed.
{
    private Compilation _compilation = default!;

	[GlobalSetup]
    public void Setup()
    {
        var sourceCode = new TAnalyzer().SampleCode
            .Replace("[|", string.Empty)
            .Replace("|]", string.Empty);
        var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);
        PortableExecutableReference[] references = [MetadataReference.CreateFromFile(typeof(object).Assembly.Location)];
        this._compilation = CSharpCompilation.Create("AssemblyName", [syntaxTree], references);
    }

    [Benchmark]
    public void RunAnalyzer()
    {
        var compilationWithAnalyzers = this._compilation.WithAnalyzers([new TAnalyzer()]);
        // Force evaluation of diagnostics.
        compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync().Wait();
    }
}

