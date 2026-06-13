---
sidebar_label: SHIMMER1014
---
# HashSetOrHashSetReturningMethodFollowedByToHashSet

## Diagnostic Rule Overview

| Field                              | Value
|------------------------------------|-------
| ID                                 | SHIMMER1014
| Analyzer title                     | A HashSet creation expression, identifier, or HashSet-returning method should not be followed by .ToHashSet()
| Analyzer message                   | .ToHashSet() is redundant
| Code fix title                     | Remove redundant .ToHashSet()
| Default severity                   | Warning
| Minimum framework/language version | N/A
| Category                           | ShimmeringUsage
| Link to code                       | [HashSetOrHashSetReturningMethodFollowedByToHashSetAnalyzer.cs](https://github.com/Bartleby2718/Shimmering.Analyzers/blob/main/src/Shimmering.Analyzers/Analyzers/Usage/HashSetOrHashSetReturningMethodFollowedByToHashSetAnalyzer.cs)
| Code fix exists?                   | Yes

## Detailed Explanation

Calling `.ToHashSet()` on an existing `HashSet<T>` is redundant and wastes memory because `.ToHashSet()` will always create and allocate a new `HashSet<T>` and populate it.

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
        HashSet<int> MyHashSet = new HashSet<int>().ToHashSet();
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
        HashSet<int> MyHashSet = new HashSet<int>();
    }
}
```

## Justification of the Severity

Calling `.ToHashSet()` on an expression that is already a `HashSet<T>` creates a completely redundant set object, generating useless heap allocations.

## Related Rules

- [SHIMMER1010: .ToList().ForEach() causes unnecessary memory allocation](./SHIMMER1010.md)
- [SHIMMER1011: Unnecessary materialization to array/list in LINQ chain](./SHIMMER1011.md)
- [SHIMMER1012: Redundant .ToArray() call](./SHIMMER1012.md)
- [SHIMMER1013: Redundant .ToList() call](./SHIMMER1013.md)
