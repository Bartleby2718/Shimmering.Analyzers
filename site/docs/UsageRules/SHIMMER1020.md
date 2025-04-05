# RedundantSpreadElement

## Diagnostic Rule Overview

| Field                              | Value
|------------------------------------|-------
| ID                                 | SHIMMER1020
| Analyzer title                     | Inline spread element
| Analyzer message                   | Inline spread element
| Code fix title                     | Flatten spread element
| Default severity                   | Warning
| Minimum framework/language version | [C# 12](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-version-history#c-version-12)
| Category                           | ShimmeringUsage
| Link to code                       | [RedundantSpreadElementAnalyzer.cs](https://github.com/Bartleby2718/Shimmering.Analyzers/blob/main/src/Shimmering.Analyzers/UsageRules/RedundantSpreadElement/RedundantSpreadElementAnalyzer.cs)
| Code fix exists?                   | Yes

## Detailed Explanation

Explicitly creating an array before spreading it in a collection expression is redundant and should be inlined.

## Examples

Flagged code:
```cs
namespace Tests;
class Test
{
    int[] Array => [1, [|.. new[] { 2, 3 }|], 4];
}
```

Fixed code:
```cs
namespace Tests;
class Test
{
    int[] Array => [1, 2, 3, 4];
}
```

## Justification of the Severity

While this is not a bug, this will slow down your code and increase memory usage.

## Related Rules

- [SHIMMER2000: Simplify LINQ chain](../StyleRules/SHIMMER2000.md)
