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
        var syntaxTree = CSharpSyntaxTree.ParseText(new TAnalyzer().SampleCode);
        PortableExecutableReference[] references = [MetadataReference.CreateFromFile(typeof(object).Assembly.Location)];
        this._compilation = CSharpCompilation.Create("AssemblyName", [syntaxTree], references);
    }

    [Benchmark]
    public void RunAnalyzer()
    {
        var compilationWithAnalyzers = _compilation.WithAnalyzers([new TAnalyzer()]);
        // Force evaluation of diagnostics.
        compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync().Wait();
    }
}

