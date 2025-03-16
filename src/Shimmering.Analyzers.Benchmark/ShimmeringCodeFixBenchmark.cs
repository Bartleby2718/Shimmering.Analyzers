using BenchmarkDotNet.Attributes;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace Shimmering.Analyzers.Benchmark;

[MemoryDiagnoser]
public abstract class ShimmeringCodeFixBenchmark<TAnalyzer, TCodeFixProvider>
	where TAnalyzer : ShimmeringSyntaxNodeAnalyzer, new()
	where TCodeFixProvider : ShimmeringCodeFixProvider, new()
{
	private AdhocWorkspace _workspace = default!;
	private Project _project = default!;
	private Document _document = default!;
	private readonly TAnalyzer _analyzer = new();
	private readonly TCodeFixProvider _codeFixProvider = new();

	[GlobalSetup]
	public void Setup()
	{
		// Create an in-memory _workspace and _project.
		this._workspace = new();
		var projectId = ProjectId.CreateNewId();
		var versionStamp = VersionStamp.Create();

		// Otherwise, I had to manually add assembly references to the project
		var tpa = (string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!;
		var assemblyPaths = tpa.Split(Path.PathSeparator);
		var references = assemblyPaths.Select(path => MetadataReference.CreateFromFile(path));

		this._project = this._workspace.AddProject(
			ProjectInfo.Create(projectId, versionStamp, "TestProject", "TestProject", LanguageNames.CSharp)
			.WithCompilationOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
			.WithMetadataReferences(references));

		// Add a document with the sample code that triggers the diagnostic.
        var sourceCode = this._analyzer.SampleCode
            .Replace("[|", string.Empty)
            .Replace("|]", string.Empty);
		this._document = this._workspace.AddDocument(this._project.Id, "TestDocument.cs", SourceText.From(sourceCode));
	}

	[Benchmark]
	public async Task<Document> ApplyCodeFix()
	{
		// Run the _analyzer to obtain diagnostics.
		var compilation = await this._document.Project.GetCompilationAsync();
		ArgumentNullException.ThrowIfNull(compilation);

		var compilationWithAnalyzers = compilation.WithAnalyzers([this._analyzer]);
		foreach (var analyzer in compilationWithAnalyzers.Analyzers)
		{
			Console.WriteLine(analyzer);
		}
		Console.WriteLine("all diagnostics:");
		foreach (var d in await compilationWithAnalyzers.GetAllDiagnosticsAsync(default))
		{
			Console.WriteLine(d);
		}
		Console.WriteLine("all analyzer diagnostics:");
		foreach (var ad in await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync(default))
		{
			Console.WriteLine(ad);
		}
		Console.WriteLine(this._analyzer.SampleCode);
		var diagnostics = await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync();

		if (diagnostics.Length != 1)
		{
			throw new InvalidOperationException($"The number of diagnostics should be exactly 1, but was {diagnostics.Length}");
		}

		// Prepare to collect code fix actions.
		var diagnostic = diagnostics.Single();
		List<CodeAction> actions = [];

		var context = new CodeFixContext(this._document, diagnostic,
			(a, d) => actions.Add(a),
			CancellationToken.None);

		await _codeFixProvider.RegisterCodeFixesAsync(context);

		if (actions.Count != 1)
		{
			throw new InvalidOperationException($"The number of code fix actions should be exactly 1, but was {actions.Count}");
		}

		var action = actions.Single();
		var operations = await action.GetOperationsAsync(CancellationToken.None);
		foreach (var operation in operations)
		{
			operation.Apply(this._workspace, CancellationToken.None);
		}

		// Update and return the _document from the workspace after the fix.
		var document = this._workspace.CurrentSolution.GetDocument(this._document.Id);
		ArgumentNullException.ThrowIfNull(document);
		return document;
	}
}
