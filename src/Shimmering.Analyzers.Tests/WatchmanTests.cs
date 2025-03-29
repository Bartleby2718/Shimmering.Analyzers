using System.Reflection;

namespace Shimmering.Analyzers.Tests;

[Parallelizable]
public class WatchmanTests
{
	[Test]
	public void TestDiagnosticIdsOnlyContainPublicConstStringFieldsStartingWithShimmerfFollowedByFourDigits()
	{
		var declaredMembers = typeof(DiagnosticIds).GetMembers(BindingFlags.DeclaredOnly);

		// every member should be a class
		var nonClassMembers = declaredMembers.Where(m => m is not Type type || !type.IsClass);
		Assert.That(nonClassMembers, Is.Empty);

		// every class should end with "Rules"
		var classes = declaredMembers.OfType<Type>();
		Assert.That(classes.Select(c => c.Name), Is.All.EndsWith("Rules"));

		// every member in those classes should be a field
		var nonFieldMembers = classes.SelectMany(t => t
			.GetMembers(BindingFlags.DeclaredOnly)
			.Where(m => m.MemberType != MemberTypes.Field));
		Assert.That(nonFieldMembers, Is.Empty);

		// all fields should be public const strings starting with "SHIMMER" followed by four digits
		var fields = classes.SelectMany(c => c.GetFields());
		foreach (var field in fields)
		{
			Assert.Multiple(() =>
			{
				// public const
				Assert.That(field.IsPublic && field.IsLiteral);
				// string
				Assert.That(field.FieldType, Is.EqualTo(typeof(string)));
				// non-null
				var value = field.GetRawConstantValue() as string;
				Assert.That(value, Is.Not.Null);
				// SHIMMMER####
				Assert.That(value, Does.Match(@"SHIMMER\d{4}"));
			});
		}
	}

	[Test]
	public void TestAllAbstractClassesStartWithShimmering()
	{
		var abstractClasses = typeof(ShimmeringAnalyzer).Assembly
			.GetTypes()
			.Where(t => t.IsClass && t.IsAbstract && !t.IsSealed);

		var invalidClasses = abstractClasses
			.Where(t => !t.Name.StartsWith("Shimmering", StringComparison.Ordinal))
			.Select(t => t.Name)
			.ToArray();

		Assert.That(
			invalidClasses,
			Is.Empty,
			$"The following abstract classes do not start with 'Abc': {string.Join(", ", invalidClasses)}");
	}

	[Test]
	public void TestDocumentationFilesExist()
	{
		var diagnosticIds = typeof(DiagnosticIds)
			.GetNestedTypes()
			.SelectMany(nestedType => nestedType.GetFields())
			.Select(field => field.GetValue(null) as string)
			.OfType<string>()
			.ToArray();

		var docsDirectory = Path.GetFullPath(Path.Combine(TestContext.CurrentContext.TestDirectory, "docs"));
		var existingMarkdownFiles = Directory.GetFiles(docsDirectory, "*.md", SearchOption.AllDirectories)
			.Select(path => Path.GetRelativePath(docsDirectory, path))
			.ToArray();

		var expectedRelativePaths = diagnosticIds
			.Select(id => Path.Combine(
				id.StartsWith("SHIMMER1") ? "UsageRules"
					: id.StartsWith("SHIMMER2") ? "StyleRules"
					: throw new InvalidOperationException(),
				$"{id}.md"))
			.ToArray();

		string[] expectedMarkdownFiles =
		[
			"AllRules.md",
			"AnalyzerDocumentationTemplate.md",
			"CONTRIBUTING.md",
			.. expectedRelativePaths,
		];

		Assert.That(existingMarkdownFiles, Is.EquivalentTo(expectedMarkdownFiles));
	}

	[Test]
	public void TestAllRulesTableIsFormattedCorrectly()
	{
		var analyzers = GetAnalyzers();

		// sort by Id in ascending order
		analyzers.Sort((a1, a2) => a1.Id.CompareTo(a2.Id));

		var allRulesFile = Path.GetFullPath(Path.Combine(TestContext.CurrentContext.TestDirectory, "docs", "AllRules.md"));
		var lines = File.ReadAllLines(allRulesFile)
			.SkipWhile(line => !line.StartsWith("# Table"))
			.ToArray();

		Assert.Multiple(() =>
		{
			Assert.That(lines[0], Is.EqualTo("# Table"));
			Assert.That(lines[1], Is.EqualTo("  Rule ID   | Category | Severity | Since | Notes"));
			Assert.That(lines[2], Is.EqualTo("------------|----------|----------|-------|-------"));
			Assert.That(lines, Has.Length.EqualTo(analyzers.Count + 3));
		});

		Assert.Multiple(() =>
		{
			for (int i = 3; i < lines.Length; ++i)
			{
				Assert.DoesNotThrow(
					() => Assert.Multiple(() =>
						{
							var line = lines[i];
							var columns = line.Split("|");
							Assert.That(columns, Has.Length.EqualTo(5));

							var analyzer = analyzers[i - 3];

							Assert.That(columns[0], Is.EqualTo($"{analyzer.Id} "));

							Assert.That(columns[1], Is.EqualTo($"  {analyzer.Category}   "));

							var severity = analyzer.Severity.ToString();
							Assert.That(columns[2], Is.EqualTo(CenterAlign(severity, totalWidth: 10)));

							// skip validation on the Since column
							var link = $"[Documentation]({analyzer.Category}Rules/{analyzer.Id}.md)";
							Assert.That(columns[4], Is.EqualTo($" {analyzer.Name}, {link}"));
						}),
					message: lines[i]);
			}
		});
	}

	private static List<AnalyzerInfo> GetAnalyzers()
	{
		var analyzerTypes = typeof(ShimmeringAnalyzer).Assembly
			.GetTypes()
			.Where(t => t.IsClass
				&& !t.IsAbstract
				&& typeof(ShimmeringAnalyzer).IsAssignableFrom(t))
			.ToArray();

		List<AnalyzerInfo> analyzers = [];
		foreach (var type in analyzerTypes)
		{
			if (Activator.CreateInstance(type) is not ShimmeringAnalyzer instance) { continue; }

			// So far, all analyzers support a single diagnostic each.
			Assert.That(instance.SupportedDiagnostics, Has.Length.EqualTo(1));

			var descriptor = instance.SupportedDiagnostics.Single();
			var analyzerName = instance.GetType().Name;
			analyzers.Add(new(descriptor.Id, analyzerName, descriptor.Category, descriptor.DefaultSeverity));
		}

		return analyzers;
	}

	private static string CenterAlign(string text, int totalWidth)
	{
		int totalPadding = totalWidth - text.Length;
		int leftPadding = totalPadding / 2;  // Floor division
		int rightPadding = totalPadding - leftPadding; // The rest goes to the right

		return new string(' ', leftPadding) + text + new string(' ', rightPadding);
	}

	private record AnalyzerInfo(string Id, string Name, string Category, DiagnosticSeverity Severity);
}
