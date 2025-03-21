# UseDiscardForUnusedOutVariable

## Diagnostic Rule Overview

| Field                              | Value
|------------------------------------|-------
| ID                                 | SHIMMER1030
| Analyzer title                     | Use discard for unused out variable
| Analyzer message                   | Unused out variable '{0}' can be replaced with discard '_'
| Code fix title                     | Replace with discard
| Default severity                   | Info
| Minimum framework/language version | N/A
| Enabled by default?                | Yes
| Category                           | Usage
| Link to code                       | [UseDiscardForUnusedOutVariableAnalyzer.cs](../../src/Shimmering.Analyzers/UsageRules/UseDiscardForUnusedOutVariable/UseDiscardForUnusedOutVariableAnalyzer.cs)
| Code fix exists?                   | Yes

## Detailed Explanation

When an `out` parameter is unused, it should be assigned to discard (`_`) to avoid unnecessary variables and indicate intent to the reader and the compiler.

## Examples

Flagged code:
```cs
using System;
using System.Linq;

namespace Tests
{
    class Test
    {
        void Do(string input)
        {
            var x = [|input.Split(' ')
                .Where(x => x.Length > 0)|];
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
        void Do(string input)
        {
            var x = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        }
    }
}
```

## Justification of the Severity

The overload that uses `StringSplitOptions` is more efficient because it can return the desired result in a single pass. However, the flagged code is also highly readable.
