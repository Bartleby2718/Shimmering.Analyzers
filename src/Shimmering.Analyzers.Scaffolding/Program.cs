using Shimmering.Analyzers.BadPractice;
using Shimmering.Analyzers.Tests.BadPractice;

if (args.Length != 1)
{
	Console.WriteLine("Expected a single parameter for the rule name");
	return;
}

var ruleName = args[0];

var analyzersDirectory = Path.Combine("Shimmering.Analyzers", ruleName);
Directory.CreateDirectory(analyzersDirectory);

var testDirectory = Path.Combine("Shimmering.Analyzers.Tests", ruleName);
Directory.CreateDirectory(testDirectory);

const string scaffoldingProjectName = "Shimmering.Analyzers.Scaffolding";
var analyzerTemplateFilePath = Path.Combine(scaffoldingProjectName, $"{nameof(BadPracticeAnalyzer)}.cs");
var analyzerFileContent = File.ReadAllText(analyzerTemplateFilePath)
	.Replace("BadPractice", ruleName);
CreateFile(
	Path.Combine(analyzersDirectory, $"{ruleName}Analyzer.cs"),
	analyzerFileContent);

var codeFixProviderTemplateFilePath = Path.Combine(scaffoldingProjectName, $"{nameof(BadPracticeCodeFixProvider)}.cs");
var codeFixProviderFileContent = File.ReadAllText(codeFixProviderTemplateFilePath)
	.Replace("BadPractice", ruleName);
CreateFile(
	Path.Combine(analyzersDirectory, $"{ruleName}CodeFixProvider.cs"),
	codeFixProviderFileContent);

var testTemplateFilePath = Path.Combine(scaffoldingProjectName, $"{nameof(BadPracticeCodeFixProviderTests)}.cs");
var testFileContent = File.ReadAllText(testTemplateFilePath)
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
