# NullableCancellationToken

## Diagnostic Rule Overview

| Field                              | Value
|------------------------------------|-------
| ID                                 | SHIMMER1000
| Analyzer title                     | Do not use a nullable CancellationToken
| Analyzer message                   | CancellationToken should not be nullable
| Code fix title                     | Make CancellationToken non-nullable
| Default severity                   | Warning
| Minimum framework/language version | N/A
| Enabled by default?                | Yes
| Category                           | ShimmeringUsage
| Link to code                       | [NullableCancellationTokenAnalyzer.cs](../../src/Shimmering.Analyzers/UsageRules/NullableCancellationToken/NullableCancellationTokenAnalyzer.cs)
| Code fix exists?                   | Yes

## Detailed Explanation

`CancellationToken` is a `struct` that follows the [null object pattern](https://en.wikipedia.org/wiki/Null_object_pattern), and `default(CancellationToken)` is `CancellationToken.None`, which is a valid, non-null instance representing a non-cancellable token. See also: https://devblogs.microsoft.com/premier-developer/recommended-patterns-for-cancellationtoken/

## Examples

Flagged code:
```cs
using System.Threading;
using System.Threading.Tasks;

namespace Tests;
class Test
{
    Task DoAsync([|CancellationToken? cancellationToken = null|]) => Task.CompletedTask;
}
```

Fixed code:
```cs
using System.Threading;
using System.Threading.Tasks;

namespace Tests;
class Test
{
    Task DoAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
}
```

## Justification of the Severity

While a nullable CancellationToken (`CancellationToken?`) is syntactically valid, it promotes an anti-pattern that requires unnecessary null handling logic. There is no situation where you'd want to prefer `Cancellation?` token over `CancellationToken.None`.

## When to Suppress

Suppress this diagnostic only if it's unmanageable to update the corresponding tests. However, you should likely be able to leverage simple string replacement (e.g. `It.IsAny<CancellationToken?>()` to `It.IsAny<CancellationToken>()` if your tests use [Moq](https://www.nuget.org/packages/moq/)) to do this in tests as well.

## Related Rules

- [SHIMMER1001: Include a CancellationToken parameter in an asynchronous method](./SHIMMER1001.md)
- [CA1068: CancellationToken parameters must come last](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/ca1068)
- [CA2016: Forward the CancellationToken parameter to methods that take one](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/ca2016)

## Inspiration

This was inspired by Mike Cop, @madelson's internal documentation created during his time at @mastercard.
