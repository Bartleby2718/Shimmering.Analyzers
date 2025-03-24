# SingleUseIEnumerableMaterialization

## Diagnostic Rule Overview

| Field                              | Value
|------------------------------------|-------
| ID                                 | SHIMMER1021
| Analyzer title                     | Avoid materializing a single-use IEnumerable
| Analyzer message                   | Avoid materializing an IEnumerable if it's used only once
| Code fix title                     | Remove unnecessary materialization
| Default severity                   | Info
| Minimum framework/language version | N/A
| Enabled by default?                | No
| Category                           | Usage
| Link to code                       | [SingleUseIEnumerableMaterializationAnalyzer.cs](../../src/Shimmering.Analyzers/UsageRules/SingleUseIEnumerableMaterialization/SingleUseIEnumerableMaterializationAnalyzer.cs)
| Code fix exists?                   | Yes

## Detailed Explanation

Materializing an `IEnumerable<T>` (e.g., using `.ToArray()` or `.ToList()`) when it's only used once is wasteful, as it forces an unnecessary allocation.

## Examples

Flagged code:
```cs
using System;
using System.Collections.Generic;
using System.Linq;

namespace Tests;
class Test
{
    void Do()
    {
        List<int> numbers = [];
        var oddNumbers = [|numbers.Where(n => n % 2 == 1).ToArray()|];
        foreach (var oddNumber in oddNumbers)
        {
            Console.WriteLine(oddNumber);
        }
    }
}
```

Fixed code:
```cs
using System;
using System.Collections.Generic;
using System.Linq;

namespace Tests;
class Test
{
    void Do()
    {
        List<int> numbers = [];
        var oddNumbers = numbers.Where(n => n % 2 == 1);
        foreach (var oddNumber in oddNumbers)
        {
            Console.WriteLine(oddNumber);
        }
    }
}
```

## Justification of the Severity

While this is not a bug, this will slow down your code and increase memory usage.

## When to Suppress

Suppress this diagnostic if the enumerable is actually used multiple times.
