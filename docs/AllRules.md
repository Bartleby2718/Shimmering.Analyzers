# Categorization
Currently, there are two categories in `Shimmering.Analyzers`:
1. **Usage**: These rules have ID `SHIMMER1XXX` and aim to enforce better usage of .NET APIs and C# constructs. If your code is flagged, it means there's likely a more idiomatic, efficient, or modern way to achieve the same result in .NET.
2. **Style**: These rules have ID `SHIMMER2XXX` and aim to enforce a consistent, but subjective, style throughout the code base. enforce the correct usage of .NET APIs and C# constructs.

Some **Usage** rules are disabled by default. This is because they enforce a best practice that does not apply in all cases. Savvy users can consider enabling such rules by default and suppressing them on a case-by-case basis in source code.

# Table
  Rule ID   | Category | Severity | Since | Notes
------------|----------|----------|-------|-------
SHIMMER1000 |  Usage   | Warning  | 1.0.0 | NullableCancellationTokenAnalyzer, [Documentation](UsageRules/SHIMMER1000.md)
SHIMMER1001 |  Usage   |   Info   | 1.0.0 | MissingCancellationTokenAnalyzer, [Documentation](UsageRules/SHIMMER1001.md)
SHIMMER1010 |  Usage   | Warning  | 1.0.0 | ToListForEachAnalyzer, [Documentation](UsageRules/SHIMMER1010.md)
SHIMMER1011 |  Usage   | Warning  | 1.0.0 | ToArrayOrToListFollowedByLinqMethodAEnalyzer, [Documentation](UsageRules/SHIMMER1011.md)
SHIMMER1012 |  Usage   | Warning  | 1.0.0 | ArrayOrArrayReturningMethodFollowedByToArrayAnalyzer, [Documentation](UsageRules/SHIMMER1012.md)
SHIMMER1020 |  Usage   |   Info   | 1.0.0 | RedundantSpreadElementAnalyzer, [Documentation](UsageRules/SHIMMER1020.md)
SHIMMER1021 |  Usage   | Disabled | 1.0.0 | SingleUseIEnumerableMaterializationAnalyzer, [Documentation](UsageRules/SHIMMER1021.md)
SHIMMER1030 |  Usage   |   Info   | 1.0.0 | MissingRemoveEmptyEntriesAnalyzer, [Documentation](UsageRules/SHIMMER1030.md)
SHIMMER1100 |  Usage   |  Warning | 1.0.0 | MisusedOrDefaultAnalyzer, [Documentation](UsageRules/SHIMMER1100.md)
SHIMMER1101 |  Usage   |   Info   | 1.0.0 | SingleElementConcatAnalyzer, [Documentation](UsageRules/SHIMMER1101.md)
SHIMMER1102 |  Usage   | Disabled | 1.0.0 | UniqueNonSetCollectionAnalyzer, [Documentation](UsageRules/SHIMMER1102.md)
SHIMMER2000 |  Style   |   Info   | 1.0.0 | VerboseLinqChainAnalyzer, [Documentation](StyleRules/SHIMMER2000.md)
SHIMMER2010 |  Style   |   Info   | 1.0.0 | NegatedTernaryConditionAnalyzer, [Documentation](StyleRules/SHIMMER2010.md)
SHIMMER2020 |  Style   |   Info   | 1.0.0 | RedundantOutVariableAnalyzer, [Documentation](StyleRules/SHIMMER2020.md)
SHIMMER2030 |  Style   | Disabled | 1.0.0 | PrimaryConstructorParameterReassignmentAnalyzer, [Documentation](StyleRules/SHIMMER2030.md)