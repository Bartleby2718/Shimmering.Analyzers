# SingleUseIEnumerableMaterialization

## Diagnostic Rule Overview

| Field                              | Value
|------------------------------------|-------
| ID                                 | SHIMMER1015
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

Materializing an `IEnumerable<T>` when it's only used once is wasteful, as it forces an unnecessary allocation. Therefore, this diagnostic flags an enumerable that is used only once and that usage is either in a `foreach` loop or followed by a LINQ method.

Currently, this diagnostic is triggered only for a local variable declaration with an implicit type (`var`), where materialization is done through either `.ToArray()` or `.ToList()`.

Note that this diagnostic is not triggered if:
1. the preceding enumerable implements `IQueryable<T>`, as materialization was likely an intentional decision
2. the enumerable is used in a lambda (e.g. `.Where(x => notFlaggedEnumerable.Contains(x))`)

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

While this is not a bug, this will slow down your code and increase memory usage with no benefits. As a reminder, the diagnostic doesn't flag if the preceding enumerable implements `IQueryable<T>` or if the enumerable is referenced multiple times, which are the main cases where you'd actually want to materialize an enumerable.

## When to Suppress

Suppress this diagnostic if the enumerable is actually used multiple times. As a reminder, this diagnostic is not triggered if the enumerable is used in a lambda.
