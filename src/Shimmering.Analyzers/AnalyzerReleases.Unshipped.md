; Unshipped analyzer release
; https://github.com/dotnet/roslyn-analyzers/blob/main/src/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md
### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
SHIMMER1013 | ShimmeringUsage | Warning | ListOrListReturningMethodFollowedByToListAnalyzer
SHIMMER1014 | ShimmeringUsage | Warning | HashSetOrHashSetReturningMethodFollowedByToHashSetAnalyzer
SHIMMER1031 | ShimmeringUsage | Info | UseTrimEntriesAnalyzer
SHIMMER1040 | ShimmeringUsage | Warning | UseGetCultureInfoAnalyzer
SHIMMER1050 | ShimmeringUsage | Warning | RemoveObsoleteMembersInMajorVersionAnalyzer
SHIMMER1060 | ShimmeringUsage | Info | UseReadOnlyCollectionParameterAnalyzer
SHIMMER2040 | ShimmeringStyle | Info | ForbidFullyQualifiedTypeReferencesAnalyzer