---
sidebar_label: SHIMMER1050
---
# RemoveObsoleteMembersInMajorVersion

## Diagnostic Rule Overview

| Field                              | Value
|------------------------------------|-------
| ID                                 | SHIMMER1050
| Analyzer title                     | Remove obsolete members in major version releases
| Analyzer message                   | Remove obsolete members in major version releases
| Code fix title                     | N/A
| Default severity                   | Warning
| Minimum framework/language version | N/A
| Category                           | ShimmeringUsage
| Link to code                       | [RemoveObsoleteMembersInMajorVersionAnalyzer.cs](../../../src/Shimmering.Analyzers/Analyzers/Usage/RemoveObsoleteMembersInMajorVersionAnalyzer.cs)
| Code fix exists?                   | No

## Detailed Explanation

Under Semantic Versioning (SemVer), major version updates (e.g., transitioning from `1.x.x` to `2.0.0`) are the designated boundaries for introducing breaking changes. 
* Elements that were deprecated and marked `[Obsolete]` in previous minor versions (e.g., `1.2.0`) should be completely removed when publishing a new stable major version (e.g., `2.0.0`).
* Carrying obsolete members into a new major lifecycle introduces unnecessary technical debt, pollutes the public API surface, and delays API modernization.
* This analyzer ensures that stable `*.0.0` releases contain a clean, updated public API contract free of deprecated members.

## Examples

Flagged code:
```cs
using System;

namespace MyLibrary;

public class Calculator
{
    [Obsolete("Use Add(int, int) instead.")]
    public int AddOld(int first, int second) => first + second;

    public int Add(int first, int second) => first + second;
}
```

## Configuration via `.editorconfig`

### Excluded Symbol Names
You can exclude specific namespaces, types, or members from this rule using the following option:

```ini
# .editorconfig
[*]
dotnet_code_quality.SHIMMER1050.excluded_symbol_names = N:MyLib.Compat|T:MyLib.Legacy.LegacyHelper
```

### Apply to Non-Packable Projects
By default, this rule is only active for projects where `IsPackable` is true (e.g. library packages). You can opt-in to analyze non-packable projects (such as applications or test suites) using:

```ini
# .editorconfig
[*]
dotnet_code_quality.SHIMMER1050.apply_to_non_packable_projects = true
```

