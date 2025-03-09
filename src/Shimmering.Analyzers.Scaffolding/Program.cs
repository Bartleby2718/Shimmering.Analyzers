﻿using Shimmering.Analyzers.CATEGORY_PLACEHOLDERRules.BadPractice;
using Shimmering.Analyzers.Tests.CATEGORY_PLACEHOLDERRules.BadPractice;

if (args.Length != 2)
{
	Console.Error.WriteLine("Expected two arguments: category as the first one and rule name as the second.");
	return;
}

var category = args[0];
if (category is not "Usage" or "Style")
{
	Console.Error.WriteLine("The category must be either Usage or Style.");
	return;
}

var ruleName = args[1];

var analyzersDirectory = Path.Combine("Shimmering.Analyzers", $"{category}Rules", ruleName);
Directory.CreateDirectory(analyzersDirectory);

var testDirectory = Path.Combine("Shimmering.Analyzers.Tests", $"{category}Rules", ruleName);
Directory.CreateDirectory(testDirectory);

const string scaffoldingProjectName = "Shimmering.Analyzers.Scaffolding";
var analyzerTemplateFilePath = Path.Combine(scaffoldingProjectName, $"{nameof(BadPracticeAnalyzer)}.cs");
var analyzerFileContent = File.ReadAllText(analyzerTemplateFilePath)
	.Replace("CATEGORY_PLACEHOLDER", category)
	.Replace("BadPractice", ruleName);
CreateFile(
	Path.Combine(analyzersDirectory, $"{ruleName}Analyzer.cs"),
	analyzerFileContent);

var codeFixProviderTemplateFilePath = Path.Combine(scaffoldingProjectName, $"{nameof(BadPracticeCodeFixProvider)}.cs");
var codeFixProviderFileContent = File.ReadAllText(codeFixProviderTemplateFilePath)
	.Replace("CATEGORY_PLACEHOLDER", category)
	.Replace("BadPractice", ruleName);
CreateFile(
	Path.Combine(analyzersDirectory, $"{ruleName}CodeFixProvider.cs"),
	codeFixProviderFileContent);

var testTemplateFilePath = Path.Combine(scaffoldingProjectName, $"{nameof(BadPracticeCodeFixProviderTests)}.cs");
var testFileContent = File.ReadAllText(testTemplateFilePath)
	.Replace("CATEGORY_PLACEHOLDER", category)
	.Replace("BadPractice", ruleName);
CreateFile(
	Path.Combine(testDirectory, $"{ruleName}CodeFixProviderTests.cs"),
	testFileContent);

Console.WriteLine($"Analyzer rule {ruleName} created successfully.");

static void CreateFile(string path, string content)
{
	if (File.Exists(path))
	{
		throw new InvalidOperationException($"{path} Already exists!");
	}
	else
	{
		File.WriteAllText(path, content);
		Console.WriteLine($"Created: {path}");
	}
}
