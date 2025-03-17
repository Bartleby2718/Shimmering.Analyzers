using BenchmarkDotNet.Running;
using Shimmering.Analyzers.Benchmark;
using Shimmering.Analyzers.StyleRules.RedundantOutVariable;

Console.WriteLine("Let's start benchmarking!");

// typically takes ~250 us
BenchmarkRunner.Run<ShimmeringAnalyzerBenchmark<RedundantOutVariableAnalyzer>>();

// typically takes 1.5-2.5 ms
BenchmarkRunner.Run<ShimmeringCodeFixBenchmark<RedundantOutVariableAnalyzer, RedundantOutVariableCodeFixProvider>>();
