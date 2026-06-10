# SHIMMER1014: A HashSet creation expression, identifier, or HashSet-returning method should not be followed by .ToHashSet()

## Cause
A `HashSet<T>` creation expression, identifier, or method call that returns `HashSet<T>` is immediately followed by a call to `Enumerable.ToHashSet()`.

## Rule description
Since the expression is already a `HashSet<T>`, calling `.ToHashSet()` is redundant and creates an unnecessary allocation.

## How to fix violations
Remove the redundant `.ToHashSet()` call.

## Example

### Violates
```csharp
HashSet<int> MyHashSet = new HashSet<int>().ToHashSet();
```

### Fixed
```csharp
HashSet<int> MyHashSet = new HashSet<int>();
```
