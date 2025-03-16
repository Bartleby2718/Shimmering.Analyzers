using BenchmarkDotNet.Running;
using Shimmering.Analyzers.Benchmark;
using Shimmering.Analyzers.StyleRules.InlineSingleUseOutVariable;

Console.WriteLine("Let's start benchmarking!");

// typically takes ~250 us
BenchmarkRunner.Run<ShimmeringAnalyzerBenchmark<InlineSingleUseOutVariableAnalyzer>>();
