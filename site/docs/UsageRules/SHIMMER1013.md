---
sidebar_label: SHIMMER1013
---
# ListOrListReturningMethodFollowedByToList

## Diagnostic Rule Overview

| Field                              | Value
|------------------------------------|-------
| ID                                 | SHIMMER1013
| Analyzer title                     | A list creation expression or list-returning method should not be followed by .ToList()
| Analyzer message                   | .ToList() is redundant
| Code fix title                     | Remove redundant .ToList()
| Default severity                   | Warning
| Minimum framework/language version | N/A
| Category                           | ShimmeringUsage
| Link to code                       | [ListOrListReturningMethodFollowedByToListAnalyzer.cs](https://github.com/Bartleby2718/Shimmering.Analyzers/blob/main/src/Shimmering.Analyzers/Analyzers/Usage/ListOrListReturningMethodFollowedByToListAnalyzer.cs)
| Code fix exists?                   | Yes

## Detailed Explanation

Calling `.ToList()` on an existing list or a method call that returns a `List<T>` is redundant and wastes memory because `.ToList()` will always create and allocate a new list, copying the elements.

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
        List<int> MyList = new List<int>().ToList();
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
        List<int> MyList = new List<int>();
    }
}
```

## Justification of the Severity

While this does not cause a crash or correctness issue, calling `.ToList()` on an expression that is already a `List<T>` creates an unnecessary allocation and loop, which degrades performance without any benefit.

## Related Rules

- [SHIMMER1010: .ToList().ForEach() causes unnecessary memory allocation](./SHIMMER1010.md)
- [SHIMMER1011: Unnecessary materialization to array/list in LINQ chain](./SHIMMER1011.md)
- [SHIMMER1012: Redundant .ToArray() call](./SHIMMER1012.md)
