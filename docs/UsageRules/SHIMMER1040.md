# MissingRemoveEmptyEntries

## Diagnostic Rule Overview

| Field                              | Value
|------------------------------------|-------
| ID                                 | SHIMMER1040
| Analyzer title                     | Use StringSplitOptions.RemoveEmptyEntries
| Analyzer message                   | Use the overload of String.Split with StringSplitOptions.RemoveEmptyEntries to remove empty entries
| Code fix title                     | Optimize string.Split()
| Default severity                   | Info
| Minimum framework/language version | N/A
| Enabled by default?                | Yes
| Category                           | Usage
| Link to code                       | [MissingRemoveEmptyEntriesAnalyzer.cs](../../src/Shimmering.Analyzers/UsageRules/MissingRemoveEmptyEntries/MissingRemoveEmptyEntriesAnalyzer.cs)
| Code fix exists?                   | Yes

## Detailed Explanation

There's an overload of [`String.Split()`](https://learn.microsoft.com/en-us/dotnet/api/system.string.split)Using `Enumerable.Concat()` to add a single element is semantically imprecise and may mislead readers about the intent of the code. The analyzer flags these instances because `.Concat()` is designed for merging two enumerables, while `.Append()` more clearly indicates the addition of one element to an existing enumerable.

## Examples

Flagged code:
```cs
using System;
using System.Linq;

namespace Tests
{
    class Test
    {
        void Do(string input)
        {
            var x = [|input.Split(' ')
                .Where(x => x.Length > 0)|];
        }
    }
}
```

Fixed code:
```cs
using System;
using System.Linq;

namespace Tests
{
    class Test
    {
        void Do(string input)
        {
            var x = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        }
    }
}
```

## Justification of the Severity

The overload that uses `StringSplitOptions` is more efficient because it can return the desired result in a single pass. However, the flagged code is also highly readable.
