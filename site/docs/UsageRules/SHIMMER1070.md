---
sidebar_label: SHIMMER1070
---
# UnnamedRegexCaptureGroup

## Diagnostic Rule Overview

| Field                              | Value
|------------------------------------|-------
| ID                                 | SHIMMER1070
| Analyzer title                     | Regex pattern contains unnamed capturing group
| Analyzer message                   | Regex pattern contains {0} unnamed capturing group(s). Use `(?<name>...)` to prevent brittle positional indexing.
| Code fix title                     | Use `(?<name>...)` to prevent brittle positional indexing
| Default severity                   | Warning
| Minimum framework/language version | N/A
| Category                           | ShimmeringUsage
| Link to code                       | [UnnamedRegexCaptureGroupAnalyzer.cs](../../../src/Shimmering.Analyzers/Analyzers/Usage/UnnamedRegexCaptureGroupAnalyzer.cs)
| Code fix exists?                   | Yes

## Detailed Explanation

Regex patterns frequently use parentheses `(...)` for capturing groups. However, accessing groups by their numeric index (e.g. `match.Groups[1]`) makes code brittle:
- Inserting, removing, or reordering groups in the pattern shifts the index of every subsequent group, leading to silent runtime bugs.
- Named groups (`(?<name>...)`) make patterns self-documenting and decouple call-site parsing logic from the physical layout of the regular expression.

## Examples

Flagged code:
```csharp
using System.Text.RegularExpressions;
var regex = new Regex(@"(\d{4})-(\d{2})-(\d{2})");
```

Fixed code:
```csharp
using System.Text.RegularExpressions;
var regex = new Regex(@"(?<group1>\d{4})-(?<group2>\d{2})-(?<group3>\d{2})");
```

## Justification of the Severity

Using unnamed capture groups makes accessing parsed values fragile: inserting, deleting, or reordering capturing groups in the pattern shifts all subsequent numeric group indices, leading to silent bugs at runtime. Named capture groups prevent this coupling.

## When to Suppress

Suppress this diagnostic if the regular expression is extremely simple or transient, if you never access capturing groups by index (e.g. you only use the entire matched string `match.Groups[0]`), or if you explicitly prefer positional indices.
