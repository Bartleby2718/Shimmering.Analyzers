# MissingRemoveEmptyEntries

## Diagnostic Rule Overview

| Field                              | Value
|------------------------------------|-------
| ID                                 | SHIMMER1030
| Analyzer title                     | Use StringSplitOptions.RemoveEmptyEntries
| Analyzer message                   | Use the overload of String.Split with StringSplitOptions.RemoveEmptyEntries to remove empty entries
| Code fix title                     | Optimize string.Split()
| Default severity                   | Info
| Minimum framework/language version | N/A
| Category                           | ShimmeringUsage
| Link to code                       | [MissingRemoveEmptyEntriesAnalyzer.cs](../../../src/Shimmering.Analyzers/UsageRules/MissingRemoveEmptyEntries/MissingRemoveEmptyEntriesAnalyzer.cs)
| Code fix exists?                   | Yes

## Detailed Explanation

The `StringSplitOptions.RemoveEmptyEntries` parameter allows you to remove empty entries in a single [`String.Split()`](https://learn.microsoft.com/en-us/dotnet/api/system.string.split) call and therefore is more efficient than chaining with a LINQ call or two to do the same thing.

Currently, the checked patterns in the `.Where()` call are:
```cs
x => x.Length > 0
x => x.Length != 0
x => x.Length >= 1
x => x != ""
x => x != string.Empty
x => !string.IsNullOrEmpty(x)
x => x.Any()
x => x is not ""
```

Note that this diagnostic is not triggered if `StringSplitOptions` is already used.

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
        var x = [|input.Split(' ')
            .Where(x => x.Length > 0)|];
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
        var x = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
    }
}
```

## Justification of the Severity

The code fix improves performance while keeping the code readable, but the flagged code does not cause any bugs or errors.
