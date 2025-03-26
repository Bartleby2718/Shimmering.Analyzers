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
| Enabled by default?                | Yes
| Category                           | Usage
| Link to code                       | [ToListForEachAnalyzer.cs](../../src/Shimmering.Analyzers/UsageRules/ToListForEach/ToListForEachAnalyzer.cs)
| Code fix exists?                   | Yes

## Detailed Explanation

Calling `.ToList().ForEach()` is wasteful when a simple `foreach` loop would achieve the same result without unnecessary allocation.

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
        [|numbers.ToList().ForEach(n => Console.WriteLine(n))|];
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

While this is not a bug, this will slow down your code and increase memory usage.

## Related Rules

- [SHIMMER1011: Unnecessary materialization to array/list in LINQ chain](./SHIMMER1011.md)
- [SHIMMER1012: Do not use a nullable CancellationToken](./SHIMMER1012.md)

## Inspiration

This is from [@Treit](https://github.com/Treit)'s blog post https://mtreit.com/programming,/.net/2024/07/30/ToList.html. The same material was also covered in [this YouTube video](https://www.youtube.com/watch?v=LaoRkzSE5tI).
