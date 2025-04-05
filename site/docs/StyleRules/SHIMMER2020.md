---
sidebar_label: SHIMMER2020
---
# RedundantOutVariable

## Diagnostic Rule Overview

| Field                              | Value
|------------------------------------|-------
| ID                                 | SHIMMER2020
| Analyzer title                     | Redundant out variable
| Analyzer message                   | Out variable '{0}' is used exactly once and for assignment and therefore can be inlined
| Code fix title                     | Inline temporary variable
| Default severity                   | Info
| Minimum framework/language version | [C# 7](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-version-history#c-version-70)
| Category                           | ShimmeringStyle
| Link to code                       | [RedundantOutVariableAnalyzer.cs](https://github.com/Bartleby2718/Shimmering.Analyzers/blob/main/src/Shimmering.Analyzers/StyleRules/RedundantOutVariable/RedundantOutVariableAnalyzer.cs)
| Code fix exists?                   | Yes

## Detailed Explanation

You can directly assign the value of an `out` parameter to a variable or a field without needing to declare a variable and then assign it.

## Examples

Flagged code:
```cs
using System;

namespace Tests;
class Test
{
    void Do(string dayOfWeekString)
    {
        if (Enum.TryParse(dayOfWeekString, [|out DayOfWeek dayOfWeek1|]))
        {
            DayOfWeek dayOfWeek2 = dayOfWeek1;
        }
    }
}
```

Fixed code:
```cs
using System;

namespace Tests;
class Test
{
    void Do(string dayOfWeekString)
    {
        if (Enum.TryParse(dayOfWeekString, out DayOfWeek dayOfWeek2))
        {
        }
    }
}
```

## Justification of the Severity

This is a stylistic suggestion that is not related to performance or buggy behaviors.

## When to Suppress

Suppress this diagnostic if you find the existing code more readable.

## Related Rules

- [IDE0059: Remove unnecessary value assignment](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/style-rules/ide0059)
