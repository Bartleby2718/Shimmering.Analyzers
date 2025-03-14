; Unshipped analyzer release
; https://github.com/dotnet/roslyn-analyzers/blob/main/src/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
SHIMMER1000 |  Usage   | Warning  | NullableCancellationTokenAnalyzer, [Documentation](https://github.com/Bartleby2718/Shimmering.Analyzers/blob/main/docs/UsageRules/SHIMMER1000.md)
SHIMMER1001 |  Usage   |  Info    | MissingCancellationTokenAnalyzer, [Documentation](https://github.com/Bartleby2718/Shimmering.Analyzers/blob/main/docs/UsageRules/SHIMMER1001.md)
SHIMMER1010 |  Usage   | Warning  | ToListForEachAnalyzer, [Documentation](https://github.com/Bartleby2718/Shimmering.Analyzers/blob/main/docs/UsageRules/SHIMMER1010.md)
SHIMMER1011 |  Usage   | Warning  | ToArrayOrToListFollowedByEnumerableExtensionMethodAEnalyzer, [Documentation](https://github.com/Bartleby2718/Shimmering.Analyzers/blob/main/docs/UsageRules/SHIMMER1011.md)
SHIMMER1012 |  Usage   | Warning  | ArrayOrArrayReturningMethodFollowedByToArrayAnalyzer, [Documentation](https://github.com/Bartleby2718/Shimmering.Analyzers/blob/main/docs/UsageRules/SHIMMER1012.md)
SHIMMER1020 |  Usage   |  Info    | RedundantSpreadElementAnalyzer, [Documentation](https://github.com/Bartleby2718/Shimmering.Analyzers/blob/main/docs/UsageRules/SHIMMER1020.md)
SHIMMER1021 |  Usage   | Disabled | SingleUseIEnumerableMaterializationAnalyzer, [Documentation](https://github.com/Bartleby2718/Shimmering.Analyzers/blob/main/docs/UsageRules/SHIMMER1021.md)
SHIMMER1030 |  Usage   | Warning  | UseDiscardForUnusedOutVariableAnalyzer, [Documentation](https://github.com/Bartleby2718/Shimmering.Analyzers/blob/main/docs/UsageRules/SHIMMER1030.md)
SHIMMER1100 |  Usage   | Warning  | MisusedOrDefaultAnalyzer, [Documentation](https://github.com/Bartleby2718/Shimmering.Analyzers/blob/main/docs/UsageRules/SHIMMER1100.md)
SHIMMER1101 |  Usage   |  Info    | SingleElementConcatAnalyzer, [Documentation](https://github.com/Bartleby2718/Shimmering.Analyzers/blob/main/docs/UsageRules/SHIMMER1101.md)
SHIMMER1102 |  Usage   | Disabled | UniqueNonSetCollectionAnalyzer, [Documentation](https://github.com/Bartleby2718/Shimmering.Analyzers/blob/main/docs/UsageRules/SHIMMER1102.md)
SHIMMER2000 |  Style   |  Info    | VerboseLinqChainAnalyzer, [Documentation](https://github.com/Bartleby2718/Shimmering.Analyzers/blob/main/docs/StyleRules/SHIMMER2000.md)
SHIMMER2010 |  Style   |  Info    | NegatedTernaryConditionAnalyzer, [Documentation](https://github.com/Bartleby2718/Shimmering.Analyzers/blob/main/docs/StyleRules/SHIMMER2010.md)
SHIMMER2020 |  Style   |  Info    | InlineSingleUseOutVariableAnalyzer, [Documentation](https://github.com/Bartleby2718/Shimmering.Analyzers/blob/main/docs/StyleRules/SHIMMER2020.md)
SHIMMER2030 |  Style   | Disabled | PrimaryConstructorParameterReassignmentAnalyzer, [Documentation](https://github.com/Bartleby2718/Shimmering.Analyzers/blob/main/docs/StyleRules/SHIMMER2030.md)