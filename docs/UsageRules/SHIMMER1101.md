# SingleElementConcat

## Diagnostic Rule Overview

| Field                              | Value
|------------------------------------|-------
| ID                                 | SHIMMER1101
| Analyzer title                     | Simplify .Concat()
| Analyzer message                   | Replace .Concat([e]) with .Append(e)
| Code fix title                     | Replace .Concat([e]) with .Append(e)
| Default severity                   | Info
| Minimum framework/language version | N/A
| Enabled by default?                | Yes
| Category                           | Usage
| Link to code                       | SingleElementConcatAnalyzer.cs
| Code fix exists?                   | Yes

## Detailed Explanation

Using `Enumerable.Concat()` to add a single element is semantically imprecise and may mislead readers about the intent of the code. The analyzer flags these instances because `.Concat()` is designed for merging two enumerables, while `.Append()` more clearly indicates the addition of one element to an existing enumerable.

## Examples

Flagged code:
```cs
var result = new[] { 1, 2 }.Concat(new[] { 3 });
```

Fixed code:
```cs
var result = new[] { 1, 2 }.Append(3);
```

Another scenario involving a cast:

Flagged code:
```cs
this._field = new[] { 1, 2 }.Concat((int[])[3]);
```
Fixed code:

```cs
this._field = new[] { 1, 2 }.Append(3);
```

## When to Suppress

Suppress this diagnostic when you want to use `.Concat()` consistently.
