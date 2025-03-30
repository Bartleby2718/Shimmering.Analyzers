# ArrayOrArrayReturningMethodFollowedByToArray

## Diagnostic Rule Overview

| Field                              | Value
|------------------------------------|-------
| ID                                 | SHIMMER1012
| Analyzer title                     | An array creation expression or array-returning method should not be followed by .ToArray()
| Analyzer message                   | .ToArray() is redundant
| Code fix title                     | Remove redundant .ToArray()
| Default severity                   | Warning
| Minimum framework/language version | N/A
| Enabled by default?                | Ye
| Category                           | ShimmeringUsage
| Link to code                       | [ArrayOrArrayReturningMethodFollowedByToArrayAnalyzer.cs](../../src/Shimmering.Analyzers/UsageRules/ArrayOrArrayReturningMethodFollowedByToArray/ArrayOrArrayReturningMethodFollowedByToArrayAnalyzer.cs)
| Code fix exists?                   | Yes

## Detailed Explanation

Calling `.ToArray()` on an existing array is redundant and wastes memory because `.ToArray()` will always create/allocate a new array.

## Examples

Flagged code:
```cs
using System.Linq;

namespace Tests;
class Test
{
    void Do()
    {
        var array = [|"a b".Split(' ').ToArray()|];
    }
}
```

Fixed code:
```cs
using System.Linq;

namespace Tests;
class Test
{
    void Do()
    {
        var array = "a b".Split(' ');
    }
}
```

## Justification of the Severity

While this is not a bug, this will slow down your code and increase memory usage with no benefits. As a reminder, calling `.ToArray()` on an existing array is not a no-op but rather creates and allocates a new array, so it has costs but not benefits.

## Related Rules

- [SHIMMER1010: .ToList().ForEach() causes unnecessary memory allocation](./SHIMMER1010.md)
- [SHIMMER1011: Unnecessary materialization to array/list in LINQ chain](./SHIMMER1011.md)

## Inspiration

This was inspired by [@Treit](https://github.com/Treit)'s blog post https://mtreit.com/programming,/.net/2024/07/30/ToList.html. The same material was also covered in [this YouTube video](https://www.youtube.com/watch?v=LaoRkzSE5tI).
