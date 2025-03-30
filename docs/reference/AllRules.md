# Categorization
Currently, there are two categories in `Shimmering.Analyzers`: **ShimmeringUsage** and **ShimmeringStyle**. The `Shimmering` prefix allows you to easily set the same severity for all diagnostics in this NuGet package.
1. **ShimmeringUsage**: These rules have ID `SHIMMER1XXX` and aim to enforce better usage of .NET APIs and C# constructs. If your code is flagged, it means there's likely a more idiomatic, efficient, or modern way to achieve the same result in .NET.
2. **ShimmeringStyle**: These rules have ID `SHIMMER2XXX` and aim to enforce a consistent, but subjective, style throughout the code base. enforce the correct usage of .NET APIs and C# constructs.

All **Shimmering** rules are _enabled_ by default, but some are _hidden_ by default. See [here](https://learn.microsoft.com/en-us/visualstudio/code-quality/use-roslyn-analyzers?view=vs-2022#silent-vs-none-severity) to understand the differences.

# Table
  Rule ID   |    Category     | Severity | Since | Notes
------------|-----------------|----------|-------|-------
SHIMMER1000 | ShimmeringUsage | Warning  | 1.0.0 | NullableCancellationTokenAnalyzer, [Documentation](UsageRules/SHIMMER1000.md)
SHIMMER1001 | ShimmeringUsage |   Info   | 1.0.0 | MissingCancellationTokenAnalyzer, [Documentation](UsageRules/SHIMMER1001.md)
SHIMMER1010 | ShimmeringUsage | Warning  | 1.0.0 | ToListForEachAnalyzer, [Documentation](UsageRules/SHIMMER1010.md)
SHIMMER1011 | ShimmeringUsage | Warning  | 1.0.0 | ToArrayOrToListFollowedByLinqMethodAnalyzer, [Documentation](UsageRules/SHIMMER1011.md)
SHIMMER1012 | ShimmeringUsage | Warning  | 1.0.0 | ArrayOrArrayReturningMethodFollowedByToArrayAnalyzer, [Documentation](UsageRules/SHIMMER1012.md)
SHIMMER1015 | ShimmeringUsage |  Hidden  | 1.0.0 | SingleUseIEnumerableMaterializationAnalyzer, [Documentation](UsageRules/SHIMMER1015.md)
SHIMMER1020 | ShimmeringUsage | Warning  | 1.0.0 | RedundantSpreadElementAnalyzer, [Documentation](UsageRules/SHIMMER1020.md)
SHIMMER1030 | ShimmeringUsage |   Info   | 1.0.0 | MissingRemoveEmptyEntriesAnalyzer, [Documentation](UsageRules/SHIMMER1030.md)
SHIMMER1100 | ShimmeringUsage | Warning  | 1.0.0 | MisusedOrDefaultAnalyzer, [Documentation](UsageRules/SHIMMER1100.md)
SHIMMER1101 | ShimmeringUsage |   Info   | 1.0.0 | SingleElementConcatAnalyzer, [Documentation](UsageRules/SHIMMER1101.md)
SHIMMER1102 | ShimmeringUsage |  Hidden  | 1.0.0 | UniqueNonSetCollectionAnalyzer, [Documentation](UsageRules/SHIMMER1102.md)
SHIMMER2000 | ShimmeringStyle |   Info   | 1.0.0 | VerboseLinqChainAnalyzer, [Documentation](StyleRules/SHIMMER2000.md)
SHIMMER2010 | ShimmeringStyle |   Info   | 1.0.0 | NegatedTernaryConditionAnalyzer, [Documentation](StyleRules/SHIMMER2010.md)
SHIMMER2020 | ShimmeringStyle |   Info   | 1.0.0 | RedundantOutVariableAnalyzer, [Documentation](StyleRules/SHIMMER2020.md)
SHIMMER2030 | ShimmeringStyle |  Hidden  | 1.0.0 | PrimaryConstructorParameterReassignmentAnalyzer, [Documentation](StyleRules/SHIMMER2030.md)