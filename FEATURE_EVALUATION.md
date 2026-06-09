# Feature Evaluation

The following open enhancement issues (excluding `TBD`) have been critically evaluated and ranked in descending order of impact and value.

## 1. Issue #32: Use `StringSplitOptions.TrimEntries`
- **Impact & Value**: **High**
- **Evaluation**: The `string.Split()` method paired with a trailing `.Select(x => x.Trim())` call incurs redundant string allocations and LINQ overhead. Using `.Split(..., StringSplitOptions.TrimEntries)` is native, idiomatic, and significantly more performant. This is highly valuable.
- **Diagnostic ID**: Should be `SHIMMER1031`.

## 2. Issue #50: `HashSetOrHashSetReturningMethodFollowedByToHashSetAnalyzer`
- **Impact & Value**: **High**
- **Evaluation**: Mirroring the existing `ToArray` and `ToList` redundancy analyzers, flagging redundant `.ToHashSet()` invocations on an existing `HashSet` or HashSet-returning method removes unnecessary object creation, memory overhead, and iteration. This is a very robust performance optimization.
- **Diagnostic ID**: Should be `SHIMMER1014`.

## 3. Issue #91: `UniqueNonSetCollection` could also flag `.Distinct(IEqualityComparer<TSource>)`
- **Impact & Value**: **Medium**
- **Evaluation**: Swapping `.Distinct(comparer)` for `.ToHashSet(comparer)` avoids yielding elements through an intermediate LINQ enumerable when the consumer's goal is merely deduplication and the collection will be materialized anyway. While performant, it's a slightly more complex transformation because `ToHashSet` returns a concrete `HashSet<T>` while `Distinct` returns an `IEnumerable<T>`.

## 4. Issue #42: `UninitializedVariableAssignedInUsing`
- **Impact & Value**: **Medium-Low**
- **Evaluation**: Recommending local functions to ensure all branches within a `using` statement assign a value is a good safeguard against using default values like `0` or `null` unexpectedly. However, this is quite niche, structurally disruptive, and may conflict with developers' scoping preferences.

## 5. Issue #53: Sort attributes alphabetically
- **Impact & Value**: **Low**
- **Evaluation**: This is a purely stylistic change. Since attribute execution order is not guaranteed and style rules are highly subjective, forcing an alphabetical sort offers zero functional value.

## 6. Issue #24: Style: SquareBracketsInCollectionExpressionNotOnTheirOwnLines
- **Impact & Value**: **Low**
- **Evaluation**: This relates to enforcing a specific formatting preference for C# 12 collection expressions. It offers zero performance benefits and is best suited for formatting rules (like `dotnet format`) rather than a dedicated Roslyn analyzer.
