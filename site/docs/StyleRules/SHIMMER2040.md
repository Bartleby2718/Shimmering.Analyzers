---
sidebar_label: SHIMMER2040
---
# ForbidFullyQualifiedTypeReferences

## Diagnostic Rule Overview

| Field                              | Value
|------------------------------------|-------
| ID                                 | SHIMMER2040
| Analyzer title                     | Avoid fully-qualified type references
| Analyzer message                   | Type reference '{0}' should not be fully-qualified; add 'using {1};' instead
| Code fix title                     | Use using directive and simplify type name
| Default severity                   | Info
| Minimum framework/language version | N/A
| Category                           | ShimmeringStyle
| Link to code                       | [ForbidFullyQualifiedTypeReferencesAnalyzer.cs](https://github.com/Bartleby2718/Shimmering.Analyzers/blob/main/src/Shimmering.Analyzers/Analyzers/Style/ForbidFullyQualifiedTypeReferencesAnalyzer.cs)
| Code fix exists?                   | Yes

## Detailed Explanation

Using fully-qualified type references in C# expressions and type syntax clutter the code and make it harder to read. This analyzer forbids fully-qualified type references when their containing namespace is not already in scope, requiring developers to add a `using` directive instead and use the simplified type name.

Unlike Microsoft's IDE0001 (Simplify Name), this analyzer fires only when the required `using` directive does not yet exist in the file (either locally or globally), and its code fix automatically inserts the necessary `using` directive.

## Examples

Flagged code:
```cs
namespace Tests
{
    class Test
    {
        void Method()
        {
            System.Text.Encoding.UTF8.GetBytes("test");
        }
    }
}
```

Fixed code:
```cs
using System.Text;

namespace Tests
{
    class Test
    {
        void Method()
        {
            Encoding.UTF8.GetBytes("test");
        }
    }
}
```

## Justification of the Severity

This rule is a style suggestion designed to keep C# code clean and uniform by avoiding verbose type paths. Because it is purely cosmetic and does not affect correctness or performance, its default severity is Info.

## When to Suppress

Suppress this diagnostic if you explicitly want to keep type names fully qualified to prevent potential naming confusion or conflicts, or if you prefer using fully-qualified names in a particular file.
