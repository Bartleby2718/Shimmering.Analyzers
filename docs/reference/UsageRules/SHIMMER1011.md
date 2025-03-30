# ToArrayOrToListFollowedByLinqMethod

## Diagnostic Rule Overview

| Field                              | Value
|------------------------------------|-------
| ID                                 | SHIMMER1011
| Analyzer title                     | Unnecessary materialization to array/list in LINQ chain
| Analyzer message                   | Remove unnecessary materialization to an array or a list
| Code fix title                     | Remove unnecessary materialization
| Default severity                   | Warning
| Minimum framework/language version | N/A
| Enabled by default?                | Yes
| Category                           | ShimmeringUsage
| Link to code                       | [ToArrayOrToListFollowedByLinqMethodAnalyzer.cs](../../src/Shimmering.Analyzers/UsageRules/ToArrayOrToListFollowedByLinqMethod/ToArrayOrToListFollowedByLinqMethodAnalyzer.cs)
| Code fix exists?                   | Yes

## Detailed Explanation

Materializing an `IEnumerable<T>` before further processing adds unnecessary overhead. LINQ operations should be performed before materialization.

Note that this diagnostic is not triggered if the preceding enumerable implements `IQueryable<T>`, as using materializing may be required when, for example, talking to the database.

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
        int[] numbers = [];
        var greaterThanThree = [|numbers.ToArray|]().Where(x => x > 3);
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
        int[] numbers = [];
        var greaterThanThree = numbers.Where(x => x > 3);
    }
}
```

## Justification of the Severity

While this is not a bug, this will slow down your code and increase memory usage with no benefits. As a reminder, the diagnostic doesn't flag if the preceding enumerable implements `IQueryable<T>`, which is likely the main case where you'd actually want to allocate a list.

## Related Rules

- [SHIMMER1010: .ToList().ForEach() causes unnecessary memory allocation](./SHIMMER1010.md)
- [SHIMMER1012: An array creation expression or array-returning method should not be followed by .ToArray()](./SHIMMER1012.md)

## Inspiration

This was inspired by [@Treit](https://github.com/Treit)'s blog post https://mtreit.com/programming,/.net/2024/07/30/ToList.html. The same material was also covered in [this YouTube video](https://www.youtube.com/watch?v=LaoRkzSE5tI).
