using BenchmarkDotNet.Running;
using Shimmering.Analyzers.Benchmark;
using Shimmering.Analyzers.StyleRules.InlineSingleUseOutVariable;

Console.WriteLine("Let's start benchmarking!");
BenchmarkRunner.Run<ShimmeringAnalyzerBenchmark<InlineSingleUseOutVariableAnalyzer>>();
