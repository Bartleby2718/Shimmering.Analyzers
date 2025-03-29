// Empirically, tests take 50% longer to run without parallelization
[assembly: Parallelizable(ParallelScope.Children)]
