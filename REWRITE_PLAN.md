# Shimmering.Analyzers Rewrite Plan

This document outlines the current state of the repository, the proposed architectural changes, and the roadmap for a complete rewrite to a modern, `IOperation`-based analyzer library.

## 1. Status Quo Analysis

### Current Architecture
- **Inheritance-Based**: Most analyzers inherit from `ShimmeringSyntaxNodeAnalyzer`.
- **Syntax-Centric**: Logic relies on parsing specific C# `SyntaxNode` types (e.g., `InvocationExpressionSyntax`).
- **Rigid Metadata**: Help links and categories are hardcoded or derived via string manipulation in the base class.

### Pros & Cons
| Feature | Pro | Con |
| :--- | :--- | :--- |
| **Documentation** | Automatic `helpLinkUri` generation ensures docs are accessible. | The logic is fragile and tied to specific string prefixes. |
| **Consistency** | All analyzers follow the same initialization pattern. | Inheritance prevents using multiple analysis actions (e.g., Compilation + Syntax). |
| **Analysis** | Easy to implement for simple syntax patterns. | Fragile against modern C# features (e.g., parenthesized expressions, `var`). |

---

## 2. The Re-architecture Plan

### Vision: Composition over Inheritance
We will move away from a "Base Analyzer" that does everything. Instead, we will use a **Rule Factory** for metadata and focus on **Semantic Analysis (`IOperation`)**.

### Proposed Directory Structure
```text
src/Shimmering.Analyzers/
├── Core/                       # Core infrastructure
│   ├── RuleFactory.cs          # Descriptor creation with auto-links
│   └── RuleCategories.cs       # Shared category constants
├── Analyzers/                  # IOperation-based logic
│   ├── Async/                  # CancellationToken rules
│   ├── Linq/                   # MisusedOrDefault, materialization rules
│   └── Style/                  # Modern C# idioms
├── CodeFixes/                  # Matching fixes
└── Utilities/                  # Semantic/Operation extensions
```

---

## 3. Rule Evaluation (Exhaustive List)

Each rule has been evaluated for its technical value and modern C# relevance.

| ID | Rule Name | Fate | Reason |
| :--- | :--- | :--- | :--- |
| **SHIMMER1000** | NullableCancellationToken | **Keep/Refine** | Basic async safety. |
| **SHIMMER1001** | MissingCancellationToken | **Keep (High)** | Critical for async robustness. |
| **SHIMMER1010** | ToListForEach | **Keep** | Performance: avoid unnecessary materialization. |
| **SHIMMER1011** | ToArrayOrToListFollowedByLinq | **Keep (High)** | Major performance win. |
| **SHIMMER1012** | ArrayFollowedByToArray | **Refine** | Check for redundancy with IDE rules. |
| **SHIMMER1013** | ListFollowedByToList | **Refine** | Check for redundancy with IDE rules. |
| **SHIMMER1015** | SingleUseMaterialization | **Keep** | Performance: materializing lazy IEnumerables too early. |
| **SHIMMER1020** | RedundantSpreadElement | **Keep** | Modern C# (12+) idiom cleanup. |
| **SHIMMER1030** | MissingRemoveEmptyEntries | **Keep** | Best practice for robust string splitting. |
| **SHIMMER1100** | MisusedOrDefault | **Keep (High)** | Prevents logical bugs in LINQ usage. |
| **SHIMMER1101** | SingleElementConcat | **Keep** | Micro-optimization. |
| **SHIMMER1102** | UniqueNonSetCollection | **Keep** | Micro-optimization. |
| **SHIMMER2000** | VerboseLinqChain | **Keep** | Readability: `Where().First()` -> `First()`. |
| **SHIMMER2010** | NegatedTernaryCondition | **Keep** | Readability: avoid double-negatives. |
| **SHIMMER2020** | RedundantOutVariable | **Refine** | Avoid overlap with `IDE0018`. |
| **SHIMMER2030** | PrimaryConstructorReassignment| **Keep (High)** | Prevents bugs in C# 12+ primary constructors. |

---

## 4. Documentation Strategy

The `site/docs/` directory must stay in sync with the codebase.

1.  **Orphaned File Cleanup**: Any rule marked as **Drop** will have its `.md` file removed from `site/docs/`.
2.  **Semantic Examples**: Update documentation examples to include cases caught by `IOperation` but missed by `SyntaxNode` analysis (e.g., nested expressions).
3.  **URL Validation**: The `RuleFactory` will generate URLs pointing to `.../docs/{Category}Rules/{Id}`. We must ensure the file structure in `site/docs/` reflects this.
4.  **AllRules.md Update**: The aggregate rules list in `site/docs/AllRules.md` will be updated to reflect the final list, including 9XXX disabled-by-default rules.

---

## 5. Roadmap & Checkpoints

We will use these checkpoints to ensure we are on the right track. **Each checkpoint must be verified before moving to the next.**

### Checkpoint 1: Foundation (Infrastructure)
- [x] Implement `Core/RuleCategories.cs`.
- [x] Implement `Core/RuleFactory.cs` (verified by unit test for URL generation).
- [x] Update `ShimmeringAnalyzerTests.cs` to support the new independent analyzer structure.

### Checkpoint 2: Semantic Proof
- [x] Rewrite `SHIMMER1001` (MissingCancellationToken) using `IOperation`.
- [x] **Enhancement**: Use `IInvocationOperation` to enforce that cancellation tokens are actually passed to underlying async invocations.
- [x] **Validation**: It must catch cases that `SyntaxNode` analysis missed (e.g., `(await task).SomeAsync(token: default)`).
- [x] Pass all existing tests for `SHIMMER1001`.

### Checkpoint 3: Migration & Cleanup
- [x] Migrate all "Keep" rules to the new structure.
- [x] Maintain original diagnostic IDs and default severities for now to avoid breaking changes.
- [x] Remove `ShimmeringSyntaxNodeAnalyzer` and `ShimmeringAnalyzer` (old).

### Checkpoint 4: Final Integration
- [x] Update Scaffolding project to match the new patterns.
- [x] Verify Docusaurus help links resolve for all migrated rules.
- [x] Run Benchmarks to ensure no performance regression in analysis time.

---

## 5. How to Stay on Track

1.  **Test-Driven Development (TDD)**: Every rewritten rule starts with its existing test suite.
2.  **Semantic Rigor**: Prefer `IOperation` over `SyntaxNode` for 100% of new logic.
3.  **Strict Review**: Ensure no hardcoded strings for metadata; use the `RuleFactory`.
