# Purpose
This project is used to create a set of a analyzer, a code fix provider, and a test file.

# Steps
1. Run `dotnet run --project Shimmering.Analyzers.Scaffolding -- <PascalCaseBadPracticeYouWantToFix>` at `/src` to execute this project.
2. Open `DiagnosticIds` in `Shimmering.Analyzers` to add a new diagnostic ID
3. Open the newly generated analyzer file and fix `RS2000`
4. Confirm that build is successful