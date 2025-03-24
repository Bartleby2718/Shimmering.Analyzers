# PrimaryConstructorParameterReassignment

## Diagnostic Rule Overview

| Field                              | Value
|------------------------------------|-------
| ID                                 | SHIMMER2030
| Analyzer title                     | Avoid reassigning primary constructor parameter
| Analyzer message                   | Primary constructor parameter '{0}' shouldn't be reassigned
| Code fix title                     | N/A
| Default severity                   | Hidden
| Minimum framework/language version | [C# 12](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-version-history#c-version-12)
| Enabled by default?                | No
| Category                           | Style
| Link to code                       | [PrimaryConstructorParameterReassignmentAnalyzer.cs](../../src/Shimmering.Analyzers/StyleRules/PrimaryConstructorParameterReassignment/PrimaryConstructorParameterReassignmentAnalyzer.cs)
| Code fix exists?                   | No

## Detailed Explanation

[Introduced in C# 12, primary constructors provide a concise syntax to declare constructors whose parameters are available anywhere in the body of the type.](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/tutorials/primary-constructors) A primary constructor can _almost_ allow you to remove all boilerplate fields that are created just to store the constructor parameters, except that a primary constructor parameter cannot be `readonly` like a field is. With this diagnostic enabled, you can assume that another method will not reassign the primary constructor parameter and therefore can treat it like a `readonly` field and safely remove the boilerplate fields. One of the code fixes for [IDE0290](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/style-rules/ide0290) (`Use primary constructor (and remove fields)`) can help you with this.

Caveats:
1. This diagnostic prevents _reassignments_, not _mutations_. For example, you cannot prevent another method from updating the first element of a primary constructor parameter that's an array.
2. [CS9105: Cannot use primary constructor parameter in this context](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/compiler-messages/constructor-errors#primary-constructor-declaration) prevents you from updating the primary constructor parameter in a regular constructor. If you do want to use the same thing throughout the class but want to do some preprocessing, either disable this diagnostic or rely on good old fields.

## Examples

Flagged code:
```cs
namespace Tests;
class Test(int x)
{
    void Do()
    {
        [|x|] = x / 2;
    }
}
```

## Justification of the Severity

This diagnostic prevents one of the valid use cases unlocked through primary constructors and therefore is disabled by default. See [Create mutable state](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/tutorials/primary-constructors#create-mutable-state).

## When to Suppress

Suppress this diagnostic if you want to maintain a mutable state using a primary constructor parameter, as mentioned above.

## Related Rules

- [IDE0290: Use primary constructor](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/style-rules/ide0290)
