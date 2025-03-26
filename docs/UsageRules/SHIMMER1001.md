# MissingCancellationToken

## Diagnostic Rule Overview

| Field                              | Value
|------------------------------------|-------
| ID                                 | SHIMMER1001
| Analyzer title                     | Include a CancellationToken parameter in an asynchronous method
| Analyzer message                   | An asynchronous method is missing a CancellationToken parameter
| Code fix title                     | Add a CancellationToken parameter
| Default severity                   | Info
| Minimum framework/language version | N/A
| Enabled by default?                | Yes
| Category                           | Usage
| Link to code                       | [MissingCancellationTokenAnalyzer.cs](../../src/Shimmering.Analyzers/UsageRules/MissingCancellationToken/MissingCancellationTokenAnalyzer.cs)
| Code fix exists?                   | Yes

## Detailed Explanation

[A `CancellationToken` enables cooperative cancellation between threads, thread pool work items, or `Task` objects.](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken) Asynchronous methods that do not accept a `CancellationToken` limit the caller's ability to cancel the operation, causing unnecessary resource consumption. See also: https://learn.microsoft.com/en-us/dotnet/standard/threading/cancellation-in-managed-threads

## Examples

Flagged code:
```cs
using System.Threading.Tasks;

namespace Tests;
class Test
{
    async Task [|DoAsync|]()
    {
        await Task.CompletedTask;
    }
}
```

Fixed code:
```cs
using System.Threading.Tasks;
using System.Threading;

namespace Tests;
class Test
{
    async Task DoAsync(CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
    }
}
```

## Justification of the Severity

Any suggestions?

## When to Suppress

Suppress this diagnostic if it's unmanageable to update the corresponding tests.

## Related Rules

- [SHIMMER1000: Do not use a nullable CancellationToken](./SHIMMER1000.md)
- [CA1068: CancellationToken parameters must come last](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/ca1068)
- [CA2016: Forward the CancellationToken parameter to methods that take one](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/ca2016)
