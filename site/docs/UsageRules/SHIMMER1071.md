---
sidebar_label: SHIMMER1071
---
# NumericRegexGroupIndexing

## Diagnostic Rule Overview

| Field                              | Value
|------------------------------------|-------
| ID                                 | SHIMMER1071
| Analyzer title                     | Regex group accessed by numeric index
| Analyzer message                   | Groups[{0}] accesses a capture group by position. Use a named group and Groups["{name}"] to prevent silent breakage when the pattern changes.
| Code fix title                     | Use named group to prevent silent breakage when pattern changes
| Default severity                   | Warning
| Minimum framework/language version | N/A
| Category                           | ShimmeringUsage
| Link to code                       | [NumericRegexGroupIndexingAnalyzer.cs](../../../src/Shimmering.Analyzers/Analyzers/Usage/NumericRegexGroupIndexingAnalyzer.cs)
| Code fix exists?                   | Yes

## Detailed Explanation

Accessing regular expression capture groups via integer indexes (e.g., `match.Groups[1]`) introduces positional fragility:
- Changing the regular expression pattern by adding or reordering capturing groups shifts the indices of other groups, silently breaking callsites at runtime.
- Accessing groups by name (e.g. `match.Groups["year"]`) increases code readability and protects callers against pattern edits.

## Examples

Flagged code:
```csharp
var match = regex.Match(text);
var year = match.Groups[1].Value;
```

Fixed code:
```csharp
var match = regex.Match(text);
var year = match.Groups["year"].Value;
```

## Code Fix Naming Heuristics

When applying the automated code fix:
1. If the underlying `Regex` pattern defines a name for the group at that index, the indexer is replaced with that name (e.g. `Groups["year"]`).
2. If the pattern is unnamed, the code fix traces if the group access is assigned to a local variable (e.g. `var year = match.Groups[1].Value;`) and uses the variable's name as the group name (e.g. `Groups["year"]`).
3. Otherwise, it falls back to a placeholder name like `Groups["group1"]`.

