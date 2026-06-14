# Proposal: Use cached `CultureInfo` instead of allocating a new instance (`SHIMMER1040`)

## 1. Rule Overview

| Field | Value |
|---|---|
| **Rule ID** | `SHIMMER1040` |
| **Title** | Use cached `CultureInfo` instead of allocating a new instance |
| **Category** | `ShimmeringUsage` |
| **Default Severity** | `Warning` |
| **Enabled by Default** | Yes |
| **Code Fix Provider** | Yes (with safety heuristics) |

---

## 2. Motivation

Instantiating `CultureInfo` via its constructor (e.g. `new CultureInfo(string)` or `new CultureInfo(int)`) allocates a new instance on the heap each time it is executed. `CultureInfo` is a heavy object because it resolves and loads culture data (date-time formats, number formats, calendars) from the OS/runtime. 

Replacing constructor calls with cached alternatives:
1. **Reduces memory allocations and GC pressure**, especially in loops or high-throughput sections.
2. **Prevents unintended mutation of shared state**: The returned cached instances from `CultureInfo.GetCultureInfo` are read-only (`IsReadOnly = true`), preventing bugs where one part of the code mutates a shared culture instance and affects other components.

---

## 3. Flagged vs. Fixed Examples

### Case A: Specific Named Culture (String Literal)
**Flagged Code:**
```csharp
using System.Globalization;
var culture = new CultureInfo("en-US");
```
**Fixed Code:**
```csharp
using System.Globalization;
var culture = CultureInfo.GetCultureInfo("en-US");
```

### Case B: Invariant Culture (Empty String or LCID 127)
**Flagged Code:**
```csharp
using System.Globalization;
var c1 = new CultureInfo("");
var c2 = new CultureInfo(127); // Invariant LCID
```
**Fixed Code:**
```csharp
using System.Globalization;
var c1 = CultureInfo.InvariantCulture;
var c2 = CultureInfo.InvariantCulture;
```

### Case C: Inline Usage (Method Arguments)
**Flagged Code:**
```csharp
using System.Globalization;
var text = 123.45.ToString(new CultureInfo("fr-FR"));
```
**Fixed Code:**
```csharp
using System.Globalization;
var text = 123.45.ToString(CultureInfo.GetCultureInfo("fr-FR"));
```

---

## 4. Detailed Specification

### A. Supported Constructor Signatures
The analyzer flags the following constructor invocations:
1. `new CultureInfo(string name)`
2. `new CultureInfo(string name, bool useUserOverride)`: Only if `useUserOverride` is resolved to a compile-time boolean literal `false`.
3. `new CultureInfo(int culture)`
4. `new CultureInfo(int culture, bool useUserOverride)`: Only if `useUserOverride` is resolved to a compile-time boolean literal `false`.

### B. Resolution Mappings
### B. Resolution Mappings
* **Empty/Invariant Targets**: If `name` resolves to `""` or `string.Empty`, or if
  `culture` resolves to `127` (`0x7F`), the replacement is **`CultureInfo.InvariantCulture`**.
  > Note: LCID `0` (LOCALE_NEUTRAL) is **not** an alias for the invariant culture and must
  > not be mapped to `CultureInfo.InvariantCulture`.
* **Other Targets**: The replacement is **`CultureInfo.GetCultureInfo(arg)`**.

### C. Detecting `string.Empty` and Empty Strings
Because `string.Empty` is a `static readonly` field and not a compile-time constant, the analyzer must check:
1. If the node is a `LiteralExpressionSyntax` representing `""`.
2. If the node is a `MemberAccessExpressionSyntax` resolving to the `System.String.Empty` symbol.

---

## 5. Flow Analysis & Safety Heuristics

To prevent replacing mutable instances that are subsequently modified (which would cause runtime `InvalidOperationException` on the read-only cached instances), the analyzer enforces these rules:

### 1. Inline Construction
If `new CultureInfo(...)` is passed directly as a method argument or used in any
expression that is not stored in a local variable, field, or property, no local mutation
can be detected by the analyzer.

**Policy**: The diagnostic is always reported and the code fix is always offered in this
case. If the receiving method mutates the `CultureInfo` argument (e.g., sets
`NumberFormat.NumberDecimalDigits`), the fix will introduce a runtime
`InvalidOperationException`. This is treated as a defect in the callee — accepting a
`CultureInfo` parameter and mutating it is itself an anti-pattern — but consumers who
cannot change the callee should suppress this diagnostic at the call site.

### 2. Local Variable Flow Analysis
If the created instance is assigned to a local variable, the analyzer performs a
control-flow check before reporting a diagnostic.

#### Mutation Check (checked first)
The following patterns constitute mutation and cause the **diagnostic to be suppressed**
entirely:

* Direct property assignment on the variable:
  `culture.NumberFormat = new NumberFormatInfo();`
* Member-of-member write (chained access):
  `culture.DateTimeFormat.ShortDatePattern = "dd/MM/yyyy";`
  `culture.NumberFormat.NumberDecimalDigits = 4;`

The check must walk assignments whose left-hand side begins with the tracked local
symbol, regardless of chain depth.

#### Escape Check (checked if no mutation detected)
A local variable **escapes** if it is:
* Returned from the enclosing method.
* Assigned to a field, property, or static variable.
* Captured in a lambda or anonymous method.
* Passed as any argument to any method call (treated conservatively as a potential
  mutation site, since the analyzer cannot inspect the callee).

> Removing the prior "known read-only formatting method" carve-out: determining
> whether a callee is safe requires either an attribute convention (e.g., a
> `[CultureInfoReadOnly]` parameter annotation) or a hardcoded allowlist, neither of
> which is specified here. Until such a mechanism is defined, all method-call escapes
> are treated uniformly.

**If the variable escapes and no local mutation was detected: suppress both the
diagnostic and the code fix.** Reporting a warning without a fix in this case produces
noise the user cannot resolve automatically, and applying the suggestion manually may
break the callee. The correct guidance is to address the callee's design separately.
