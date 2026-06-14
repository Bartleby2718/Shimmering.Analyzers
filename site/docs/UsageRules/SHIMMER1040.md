---
sidebar_label: SHIMMER1040
---
# UseGetCultureInfo

## Diagnostic Rule Overview

| Field                              | Value
|------------------------------------|-------
| ID                                 | SHIMMER1040
| Analyzer title                     | Use cached CultureInfo instead of allocating a new instance
| Analyzer message                   | Use cached CultureInfo instead of allocating a new instance
| Code fix title                     | Use cached CultureInfo
| Default severity                   | Warning
| Minimum framework/language version | N/A
| Category                           | ShimmeringUsage
| Link to code                       | [UseGetCultureInfoAnalyzer.cs](../../../src/Shimmering.Analyzers/Analyzers/Usage/UseGetCultureInfoAnalyzer.cs)
| Code fix exists?                   | Yes

## Detailed Explanation

Instantiating `CultureInfo` via its constructor (e.g., `new CultureInfo("en-US")`) allocates a new instance on the heap every time it is called. Since `CultureInfo` is a heavy object containing calendar, date, and number format settings, repeatedly allocating it creates unnecessary GC pressure.

Instead, calling `CultureInfo.GetCultureInfo("en-US")` returns a cached, read-only instance, which is both memory-efficient and thread-safe.

For invariant culture instances, using `CultureInfo.InvariantCulture` is preferred directly.

## Examples

Flagged code:
```cs
using System.Globalization;

namespace Tests;
class Test
{
    void Do()
    {
        var culture = new CultureInfo("en-US");
    }
}
```

Fixed code:
```cs
using System.Globalization;

namespace Tests;
class Test
{
    void Do()
    {
        var culture = CultureInfo.GetCultureInfo("en-US");
    }
}
```

