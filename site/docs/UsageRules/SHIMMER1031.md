---
sidebar_label: SHIMMER1031
---
# UseTrimEntries

## Diagnostic Rule Overview

| Field                              | Value
|------------------------------------|-------
| ID                                 | SHIMMER1031
| Analyzer title                     | Use StringSplitOptions.TrimEntries
| Analyzer message                   | Use the overload of string.Split with StringSplitOptions.TrimEntries to trim entries
| Code fix title                     | Use StringSplitOptions.TrimEntries
| Default severity                   | Info
| Minimum framework/language version | .NET 5.0
| Category                           | ShimmeringUsage
| Link to code                       | [UseTrimEntriesAnalyzer.cs](https://github.com/Bartleby2718/Shimmering.Analyzers/blob/main/src/Shimmering.Analyzers/Analyzers/Usage/UseTrimEntriesAnalyzer.cs)
| Code fix exists?                   | Yes

## Detailed Explanation

In .NET 5.0 and later, `string.Split` supports `StringSplitOptions.TrimEntries` which trims individual substrings as they are parsed, avoiding intermediate allocations. Calling `string.Split()` followed by `Select(x => x.Trim())` (and optionally `ToArray()`) is less efficient and more verbose.

## Examples

Flagged code:
```cs
using System;
using System.Linq;

namespace Tests;
class Test
{
    void Do(string input)
    {
        var x = input.Split(',').Select(x => x.Trim());
    }
}
```

Fixed code:
```cs
using System;
using System.Linq;

namespace Tests;
class Test
{
    void Do(string input)
    {
        var x = input.Split(',', StringSplitOptions.TrimEntries);
    }
}
```

## Justification of the Severity

Using `StringSplitOptions.TrimEntries` avoids creating intermediate strings and allocations from the split operation before they are trimmed, leading to cleaner code and better performance. Because it is a recommendation and requires .NET 5.0+, the severity is set to Info.

## Related Rules

- [SHIMMER1030: Missing StringSplitOptions.RemoveEmptyEntries](./SHIMMER1030.md)
