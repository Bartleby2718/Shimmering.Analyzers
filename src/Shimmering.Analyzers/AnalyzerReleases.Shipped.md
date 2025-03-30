; To see the table containing all rules and the version each was added, please see [`AllRules.md`](AllRules.md).

## Release 1.0.0

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
SHIMMER1000 | ShimmeringUsage | Warning  | NullableCancellationTokenAnalyzer, [Documentation](../../docs/reference/UsageRules/SHIMMER1000.md)
SHIMMER1001 | ShimmeringUsage |  Info    | MissingCancellationTokenAnalyzer, [Documentation](../../docs/reference/UsageRules/SHIMMER1001.md)
SHIMMER1010 | ShimmeringUsage | Warning  | ToListForEachAnalyzer, [Documentation](../../docs/reference/UsageRules/SHIMMER1010.md)
SHIMMER1011 | ShimmeringUsage | Warning  | ToArrayOrToListFollowedByLinqMethodAnalyzer, [Documentation](../../docs/reference/UsageRules/SHIMMER1011.md)
SHIMMER1012 | ShimmeringUsage | Warning  | ArrayOrArrayReturningMethodFollowedByToArrayAnalyzer, [Documentation](../../docs/reference/UsageRules/SHIMMER1012.md)
SHIMMER1015 | ShimmeringUsage |  Hidden  | SingleUseIEnumerableMaterializationAnalyzer, [Documentation](../../docs/reference/UsageRules/SHIMMER1015.md)
SHIMMER1020 | ShimmeringUsage | Warning  | RedundantSpreadElementAnalyzer, [Documentation](../../docs/reference/UsageRules/SHIMMER1020.md)
SHIMMER1030 | ShimmeringUsage |  Info    | MissingRemoveEmptyEntriesAnalyzer, [Documentation](../../docs/reference/UsageRules/SHIMMER1030.md)
SHIMMER1100 | ShimmeringUsage | Warning  | MisusedOrDefaultAnalyzer, [Documentation](../../docs/reference/UsageRules/SHIMMER1100.md)
SHIMMER1101 | ShimmeringUsage |  Info    | SingleElementConcatAnalyzer, [Documentation](../../docs/reference/UsageRules/SHIMMER1101.md)
SHIMMER1102 | ShimmeringUsage |  Hidden  | UniqueNonSetCollectionAnalyzer, [Documentation](../../docs/reference/UsageRules/SHIMMER1102.md)
SHIMMER2000 | ShimmeringStyle |  Info    | VerboseLinqChainAnalyzer, [Documentation](../../docs/reference/StyleRules/SHIMMER2000.md)
SHIMMER2010 | ShimmeringStyle |  Info    | NegatedTernaryConditionAnalyzer, [Documentation](../../docs/reference/StyleRules/SHIMMER2010.md)
SHIMMER2020 | ShimmeringStyle |  Info    | RedundantOutVariableAnalyzer, [Documentation](../../docs/reference/StyleRules/SHIMMER2020.md)
SHIMMER2030 | ShimmeringStyle |  Hidden  | PrimaryConstructorParameterReassignmentAnalyzer, [Documentation](../../docs/reference/StyleRules/SHIMMER2030.md)