# Purpose
This project is used to create a set of a analyzer, a code fix provider, and a test file.

# Steps
1. Run `dotnet run --project Shimmering.Analyzers.Scaffolding -- <Category> <PascalCaseBadPracticeYouWantToFix>` at `/src` to execute this project, where `<Category>` is either `Usage` or `Style` (without the `Shimmering` prefix).
2. Open `DiagnosticIds` in `Shimmering.Analyzers` to add a new diagnostic ID in the right class
3. Open the newly generated analyzer file and fix `RS2010`
4. Confirm that build is successful