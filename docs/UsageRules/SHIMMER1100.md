# MisusedOrDefault

## Diagnostic Rule Overview

| Field                              | Value
|------------------------------------|-------
| ID                                 | SHIMMER1100
| Analyzer title                     | OrDefault()! is redundant
| Analyzer message                   | Replace '{0}!' with '{1}'
| Code fix title                     | Simplify 'OrDefault()!' method call
| Default severity                   | Warning
| Minimum framework/language version | N/A
| Enabled by default?                | Yes
| Category                           | Usage
| Link to code                       | [MisusedOrDefaultAnalyzer.cs](../../src/Shimmering.Analyzers/UsageRules/MisusedOrDefault/MisusedOrDefaultAnalyzer.cs)
| Code fix exists?                   | Yes

## Detailed Explanation

This analyzer detects instances where a built-in LINQ `OrDefault` method is immediately followed by the null-forgiving operator (`!`). Such usage is redundant because the non-`OrDefault` counterpart of the method exists and should be used instead, when you expect there will _always_ be such an element.

If you believe the `OrDefault` method could return `null` but the null-forgiving operator (`!`) may have been added by mistake, you should remove the null-forgiving operator instead.

## Examples

Flagged code:
```cs
using System;
using System.Linq;

namespace Tests
{
    class Test
    {
        void Do()
        {
            var a = new[] { 1 }.SingleOrDefault()!;
        }
    }
}
```

Fixed code:
```cs
using System;
using System.Linq;

namespace Tests
{
    class Test
    {
        void Do()
        {
            var a = new[] { 1 }.Single();
        }
    }
}
```

## Justification of the Severity

Assuming that the null-forgiving operator (`!`) was intentional, updated code will always be clearer and shorter, while the behavior is exactly the same.

## When to suppress

This should not be suppressed, although you may fix instead by removing an unnecessary `!`.
