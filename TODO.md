# TODO List

## 1. Feature Refinements (To be squashed)
- [x] Rename `SHIMMER1080` (Numeric Regex Group Indexing) to `SHIMMER1071` everywhere.
- [x] Implement and document the `group*` variable-name-based fallback heuristic for `SHIMMER1071`.
- [x] Remove fully qualified types in `VerifyAnalyzerWithNoCompilerErrorsAsync` within `UnnamedRegexCaptureGroupAnalyzerTests.cs`.
- [x] Document the `api_surface` configuration option in `SHIMMER1060.md`.
- [x] Simplify `IsInvariantLcid` in `UseGetCultureInfoCodeFixProvider.cs` (SHIMMER1040).
- [x] Remove the misleading `Fixed code:` section in `SHIMMER1050.md`.
- [x] Clarify the link between `SHIMMER1040.md` and `SHIMMER1031.md` (SHIMMER1040).
- [x] Optimize `UseReadOnlyCollectionParameterAnalyzer.cs` to lookup mutable types and target interfaces once per compilation start action (avoiding `ToDisplayString()` on symbols and namespace concatenation in syntax actions) (SHIMMER1060).

## 2. History Squashing
- [x] Run interactive `git rebase -i` to squash all of the above refinements into their respective atomic feature commits (`SHIMMER1040`, `SHIMMER1050`, `SHIMMER1060`, `SHIMMER1070`, `SHIMMER1071`).

## 3. General Cleanups (Separate commits)
- [x] Remove `PLAN.md` from the repository root.

## 4. Repository-wide Audits & Invasive Changes (To be done last)
- [x] Perform a library-wide performance audit.
- [x] Replace `ContainingType?.ToDisplayString() == ...` with direct type symbol comparisons in regex analyzers.
- [x] Audit and improve cancellation support (ensure `context.CancellationToken` is passed to all Roslyn semantic model and compilation actions).
- [x] Add blank lines between file-scoped namespace declarations and class declarations in test raw string literals repository-wide.
- [x] Update all `SHIMMER*.md` files to align with the standard `AnalyzerDocumentationTemplate.md` structure.
- [x] Fix StyleCop rule SA1027 globally to enforce consistent tab-based indentation.
- [ ] Enforce IDE0005 (Remove unnecessary usings) globally in `.editorconfig` as warning/error.
