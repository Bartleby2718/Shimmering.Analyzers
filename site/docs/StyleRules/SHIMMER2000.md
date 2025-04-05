---
sidebar_label: SHIMMER2000
---
# VerboseLinqChain

## Diagnostic Rule Overview

| Field                              | Value
|------------------------------------|-------
| ID                                 | SHIMMER2000
| Analyzer title                     | Simplify LINQ chain
| Analyzer message                   | Replace a verbose LINQ chain with a collection expression
| Code fix title                     | Replace with a collection expression
| Default severity                   | Info
| Minimum framework/language version | C# 12
| Category                           | ShimmeringStyle
| Link to code                       | [VerboseLinqChainAnalyzer.cs](https://github.com/Bartleby2718/Shimmering.Analyzers/blob/main/src/Shimmering.Analyzers/StyleRules/VerboseLinqChain/VerboseLinqChainAnalyzer.cs)
| Code fix exists?                   | Yes

## Detailed Explanation

Introduced C# 12, collection expressions provide a cleaner and more efficient way to construct collections compared to multiple LINQ method calls.

## Examples

Flagged code:
```cs
using System.Linq;

namespace Tests;
class Test
{
    static int[] array1 = [0, 1];
    static int[] array2 = [5];
    void Do()
    {
        var array3 = array1.Append(2).Prepend(3).Concat(array2).ToArray();
    }
}
```

Fixed code:
```cs
using System.Linq;

namespace Tests;
class Test
{
    static int[] array1 = [0, 1];
    static int[] array2 = [5];
    void Do()
    {
        int[] array3 = [3, .. array1, 2, .. array2];
    }
}
```

## Justification of the Severity

While using a collection expression improves readability, this is a stylistic suggestion that is not related to performance or buggy behavior.

## When to Suppress

Suppress this diagnostic if you find the existing code more readable.

## Related Rules

- [IDE0300: Use collection expression for array](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/style-rules/ide0300)
- [IDE0301: Use collection expression for empty](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/style-rules/ide0301)
- [IDE0302: Use collection expression for stackalloc](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/style-rules/ide0302)
- [IDE0303: Use collection expression for Create()](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/style-rules/ide0303)
- [IDE0304: Use collection expression for builder](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/style-rules/ide0304)
- [IDE0305: Use collection expression for fluent](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/style-rules/ide0305)
