# Guidelines for AI Agents

Please adhere to the following rules and best practices:

## 1. Documentation & Workflow

- **File Links**: When mentioning repository files in markdown documentation, always use clickable relative links (e.g., [`README.md`](./README.md)).
- **Testing**: After adding a feature, always add tests covering both the happy paths and edge cases. When fixing a bug, follow the red-green-refactor approach. Every test name should be descriptive, explaining the business logic that's being enforced.
- **Documentation**: After adding a feature, ensure all relevant documentation is created, updated, or removed.

## 2. Roslyn Analyzer Architecture
- **Composition over Inheritance**: Use `ShimmeringAnalyzer` as the base class for all analyzers.
- **Metadata and Help Links**: Always construct `DiagnosticDescriptor` instances using `ShimmeringRuleFactory.Create`. Do not hardcode help link URLs.
- **Category Constants**: Use the constants defined in `Core.RuleCategories` for diagnostic categories (e.g., `RuleCategories.Style`, `RuleCategories.Usage`, `RuleCategories.Performance`). Do not use hardcoded strings for categories.
- **Directory Structure**: Put analyzers under `src/Shimmering.Analyzers/Analyzers/{Category}/` and code fixes under `src/Shimmering.Analyzers/CodeFixes/{Category}/`. Group tests correspondingly under `src/Shimmering.Analyzers.Tests/Analyzers/{Category}/`.

## 3. Commit Convention

Every commit should be a single logical piece of change, whether it's a new feature, a bug fix, or refactoring. When committing code, use the following format: `<model-used>: <changes written concisely>`

*(e.g., `gemini-3.1-pro: Refactor foundational docs and decouple validator`, not `antigravity: Refactor foundational docs and decouple validator`)*

Also, do not push commits unless instructed.

## 4. C# Coding Style

- **Strings**: Prefer raw string literals (`"""`) over regular string literals (`"..."`) or verbatim string literals (`@"..."`) for any multi-line strings, code snippets, or strings containing double quotes or newlines (such as expected outputs in unit tests).
  - **Spacing Convention**: Multi-line raw string literals must have their opening and closing quotes on their own dedicated lines. The closing quotes must align with the indentation of the enclosing statement. The string content must be indented at least as far as the closing quotes. 
    ```csharp
    var text = """
        Line 1
        Line 2
        """;
    ```
- **Variables**: CRITICAL: NEVER use abbreviated variable names (e.g., `ms`, `ctx`, `fbb`, `tok`, `ast`, `decl`, `stmt`). ALWAYS write out the full word (e.g., `memoryStream`, `context`, `builder`, `token`, `syntaxTree`, `declaration`, `statement`). However, using a single word is acceptable for a compound noun (e.g. `builder` instead of `stringBuilder`).
- **XML / Project Files**:
  - **Doc Comments**: Prefer `<paramref>` and `<see>` tags over regular strings or backticks for code references in XML doc comments.
  - Do not put newlines between XML attributes. Keep all attributes for a single element on the same line to prevent whitespace churning.
  - **No Redundancy**: Do not duplicate properties (e.g., `TargetFramework`, `Nullable`, `ImplicitUsings`) or transitive package references in `.csproj` files if they are already defined globally in `Directory.Build.props` or inherited via project references.
  - **Lock Files**: Whenever modifying a `.csproj` file, run `dotnet restore` or `dotnet build` to regenerate `packages.lock.json` and ensure the updated lock file is included in your commit.
- **Types**: Never use fully qualified types. Prefer `using` directives at the top of the file.
