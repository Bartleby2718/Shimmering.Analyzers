---
sidebar_label: SHIMMER1010
---
# ToListForEach

## Diagnostic Rule Overview

| Field                              | Value
|------------------------------------|-------
| ID                                 | SHIMMER1010
| Analyzer title                     | .ToList().ForEach() causes unnecessary memory allocation
| Analyzer message                   | Replace .ToList().ForEach() with a foreach loop to reduce memory usage
| Code fix title                     | Replace with a foreach loop
| Default severity                   | Warning
| Minimum framework/language version | N/A
| Category                           | ShimmeringUsage
| Link to code                       | [ToListForEachAnalyzer.cs](https://github.com/Bartleby2718/Shimmering.Analyzers/blob/main/src/Shimmering.Analyzers/UsageRules/ToListForEach/ToListForEachAnalyzer.cs)
| Code fix exists?                   | Yes

## Detailed Explanation

Avoid converting an `IEnumerable<T>` to a `List<T>` just to use the `List<T>.ForEach()` method. The unnecessary `.ToList()` call causes extra memory allocation, while a simple `foreach` loop achieves the same result without this overhead. If you _do_ want to a list, you can work around this diagnostic by using a plain `foreach` loop.

Note that this diagnostic is not triggered if the preceding enumerable implements `IQueryable<T>`, as using `.ToList()` for materialization is a valid use case. 

## Examples

Flagged code:
```cs
using System;
using System.Linq;

namespace Tests;
class Test
{
    void Do(int[] numbers)
    {
        numbers.ToList().ForEach(n => Console.WriteLine(n));
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
    void Do(int[] numbers)
    {
        foreach (var n in numbers)
        {
            Console.WriteLine(n);
        }
    }
}
```

## Justification of the Severity

While this is not a bug, this will slow down your code and increase memory usage with no benefits. As a reminder, the diagnostic doesn't flag if the preceding enumerable implements `IQueryable<T>`, which is likely the main case where you'd actually want to allocate a list.

## Related Rules

- [SHIMMER1011: Unnecessary materialization to array/list in LINQ chain](./SHIMMER1011.md)
- [SHIMMER1012: An array creation expression or array-returning method should not be followed by .ToArray()](./SHIMMER1012.md)

## Inspiration

This was inspired by [@Treit](https://github.com/Treit)'s blog post https://mtreit.com/programming,/.net/2024/07/30/ToList.html. The same material was also covered in [this YouTube video](https://www.youtube.com/watch?v=LaoRkzSE5tI).
