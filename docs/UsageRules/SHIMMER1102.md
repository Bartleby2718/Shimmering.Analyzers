# UniqueNonSetCollection

## Diagnostic Rule Overview

| Field                              | Value
|------------------------------------|-------
| ID                                 | SHIMMER1102
| Analyzer title                     | Use a set instead
| Analyzer message                   | Prefer sets when uniqueness is required
| Code fix title                     | Replace .Distinct().ToList() or .Distinct().ToArray() with .ToHashSet()
| Default severity                   | Info
| Minimum framework/language version | .NET	Core 2.0+, .NET Framework 4.7.2+, .NET Standard 2.1
| Enabled by default?                | No
| Category                           | Usage
| Link to code                       | [UniqueNonSetCollectionAnalyzer.cs](../../src/Shimmering.Analyzers/UsageRules/UniqueNonSetCollection/UniqueNonSetCollectionAnalyzer.cs)
| Code fix exists?                   | Yes

## Detailed Explanation

Using `.Distinct().ToArray()` or `.Distinct().ToList()` suggests that the goal is to remove duplicates. A `HashSet<T>` achieves the same outcome more efficiently while allowing a faster lookup.

## Examples

Flagged code:
```cs
using System.Collections.Generic;
using System.Linq;

namespace Tests;
class Test
{
    void Do()
    {
        List<int> numbers = [];
        var distinctNumbers = [|numbers.Distinct().ToArray()|];
    }
}
```

Fixed code:
```cs
using System.Collections.Generic;
using System.Linq;

namespace Tests;
class Test
{
    void Do()
    {
        List<int> numbers = [];
        var distinctNumbers = numbers.ToHashSet();
    }
}
```

## Justification of the Severity

This diagnostic can cause compilation errors, so it's disabled by default.

## When to Suppress

Suppress this diagnostic if you need the collection to be ordered or indexed.

## Inspiration

Suggested by [@matthew-elgart](https://github.com/matthew-elgart).
