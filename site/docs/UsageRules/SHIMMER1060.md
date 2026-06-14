---
sidebar_label: SHIMMER1060
---
# UseReadOnlyCollectionParameter

## Diagnostic Rule Overview

| Field                              | Value
|------------------------------------|-------
| ID                                 | SHIMMER1060
| Analyzer title                     | Use read-only collection interface for parameter
| Analyzer message                   | Parameter '{0}' is typed as '{1}' but is never mutated. Consider using '{2}' to broaden caller compatibility.
| Code fix title                     | Use read-only collection interface for parameter
| Default severity                   | Suggestion
| Minimum framework/language version | N/A
| Category                           | ShimmeringUsage
| Link to code                       | [UseReadOnlyCollectionParameterAnalyzer.cs](../../../src/Shimmering.Analyzers/Analyzers/Usage/UseReadOnlyCollectionParameterAnalyzer.cs)
| Code fix exists?                   | Yes

## Detailed Explanation

Postel's Law states: *"Be conservative in what you do, be liberal in what you accept from others."*
When a public or protected method accepts a mutable collection parameter (like `List<T>`, `IList<T>`, or `Dictionary<TKey, TValue>`) but never modifies its content, it unnecessarily restricts callers.

Callers holding read-only collections (e.g. `IReadOnlyList<T>` or `IReadOnlyDictionary<TKey, TValue>`) are forced to allocate and copy their data into mutable containers simply to satisfy the method signature.
Specifying a read-only collection interface (e.g. `IReadOnlyList<T>`) broadens caller compatibility, makes the method's behavior self-documenting, and prevents accidental future mutation bugs.

## Examples

Flagged code:
```csharp
public class ReportGenerator
{
    public void PrintReport(List<string> items)
    {
        foreach (var item in items)
        {
            Console.WriteLine(item);
        }
    }
}
```

Fixed code:
```csharp
using System.Collections.Generic;

public class ReportGenerator
{
    public void PrintReport(IReadOnlyList<string> items)
    {
        foreach (var item in items)
        {
            Console.WriteLine(item);
        }
    }
}
```

## Configuration via `.editorconfig`

### API Surface
By default, this rule analyzes parameters of `public` and `protected` methods. You can customize the API surface to be analyzed by specifying a comma-separated list of accessibility modifiers (e.g. `public`, `protected`, `internal`, `private`):

```ini
# .editorconfig
[*]
dotnet_code_quality.SHIMMER1060.api_surface = public, protected, internal
```
