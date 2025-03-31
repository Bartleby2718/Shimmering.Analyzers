# NegatedTernaryCondition

## Diagnostic Rule Overview

| Field                              | Value
|------------------------------------|-------
| ID                                 | SHIMMER2010
| Analyzer title                     | Avoid negation in ternary condition
| Analyzer message                   | This ternary condition has a negation
| Code fix title                     | Invert the ternary for clarity
| Default severity                   | Info
| Minimum framework/language version | N/A
| Category                           | ShimmeringStyle
| Link to code                       | [NegatedTernaryConditionAnalyzer.cs](../../../src/Shimmering.Analyzers/StyleRules/NegatedTernaryCondition/NegatedTernaryConditionAnalyzer.cs)
| Code fix exists?                   | Yes

## Detailed Explanation

Using negation (`!`) in a ternary condition reduces readability because it forces the reader to mentally invert the condition. Instead, swapping the order of the true/false branches often makes the code more intuitive.

Note that this diagnostic is not triggered if either of the branches or the condition contains another ternary expression.

## Examples

Flagged code:
```cs
namespace Tests;
class Test
{
    string Do(bool condition) => [|!condition ? "when false" : "when true"|];
}
```

Fixed code:
```cs
namespace Tests;
class Test
{
    string Do(bool condition) => condition ? "when true" : "when false";
}
```

## Justification of the Severity

While removing negation in the ternary condition often improves readability, this is a stylistic suggestion that is not related to performance or buggy behaviors. Some people prefer to handle common cases first, and others prefer to handle exceptional cases first, regardless of the existence of negation in the ternary condition.

## When to Suppress

Suppress this diagnostic if you find the existing code more readable.
