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

Note that this diagnostic is not triggered if the method:
1. is an override or implementation of an interface; or
2. already has a `CancellationToken` parameter (either nullable or non-nullable).

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

In some cases, a `CancellationToken` parameter should not be added to an asynchronous method in some cases. See the following section.

## When to Suppress

Suppress this diagnostic if:
1. cancellation is unnecessary because the operation is quick or does not perform I/O or long-running tasks.
2. it can cause breaking changes by breaking existing contracts
3. it affects reflection-based code
4. it impacts unit tests using `Moq` and there are too many of them
5. the method delegates to another API without cancellation support
6. the operation should not be cancelled based on your business logic

## Related Rules

- [SHIMMER1000: Do not use a nullable CancellationToken](./SHIMMER1000.md)
- [CA1068: CancellationToken parameters must come last](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/ca1068)
- [CA2016: Forward the CancellationToken parameter to methods that take one](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/ca2016)
