# Guidelines for AI Agents

Please adhere to the following rules and best practices:

## 1. Test Coverage
- **Mandatory test coverage**: Whenever you add a new analyzer, a new code fix, or a new feature, you must add corresponding tests to the test suite (located under `src/Shimmering.Analyzers.Tests/`).
- **Bug Fixes**: When fixing a bug, first add a reproducing test case, verify that it fails, and then ensure it passes after your fix.

## 2. Roslyn Analyzer Architecture
- **Composition over Inheritance**: Use `Core.ShimmeringAnalyzer` as the base class for all analyzers.
- **Metadata and Help Links**: Always construct `DiagnosticDescriptor` instances using `Core.RuleFactory.Create`. Do not hardcode help link URLs.
- **Category Constants**: Use the constants defined in `Core.RuleCategories` for diagnostic categories (e.g., `RuleCategories.Style`, `RuleCategories.Usage`, `RuleCategories.Performance`). Do not use hardcoded strings for categories.
- **Directory Structure**: Put analyzers under `src/Shimmering.Analyzers/Analyzers/{Category}/` and code fixes under `src/Shimmering.Analyzers/CodeFixes/{Category}/`. Group tests correspondingly under `src/Shimmering.Analyzers.Tests/Analyzers/{Category}/`.
