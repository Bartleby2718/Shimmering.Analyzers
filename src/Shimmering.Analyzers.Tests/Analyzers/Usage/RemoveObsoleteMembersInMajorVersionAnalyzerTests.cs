using System.Text;

using Shimmering.Analyzers.Analyzers.Usage;

namespace Shimmering.Analyzers.Tests.Analyzers.Usage;

public class RemoveObsoleteMembersInMajorVersionAnalyzerTests : ShimmeringAnalyzerTests<RemoveObsoleteMembersInMajorVersionAnalyzer>
{
	[Test]
	public override Task TestSampleCode() => VerifyAnalyzerWithConfigAsync(
		new RemoveObsoleteMembersInMajorVersionAnalyzer().SampleCode,
		version: "2.0.0",
		packageVersion: null,
		exclusions: null);

	[Test]
	public Task TestFlagObsoleteMethodInMajorVersion() => VerifyAnalyzerWithConfigAsync(
		"""
		using System;

		namespace Tests
		{
			public class Test
			{
				[Obsolete]
				public void [|OldMethod|]() {}
			}
		}
		""",
		version: "2.0.0",
		packageVersion: null,
		exclusions: null);

	[Test]
	public Task TestFlagObsoleteClassInMajorVersion() => VerifyAnalyzerWithConfigAsync(
		"""
		using System;

		namespace Tests
		{
			[Obsolete]
			public class [|OldClass|] {}
		}
		""",
		version: "3.0.0",
		packageVersion: null,
		exclusions: null);

	[Test]
	public Task TestIgnoreObsoleteMethodInMinorVersion() => VerifyAnalyzerWithConfigAsync(
		"""
		using System;

		namespace Tests
		{
			public class Test
			{
				[Obsolete]
				public void OldMethod() {}
			}
		}
		""",
		version: "2.1.0",
		packageVersion: null,
		exclusions: null);

	[Test]
	public Task TestIgnoreObsoleteMethodInPatchVersion() => VerifyAnalyzerWithConfigAsync(
		"""
		using System;

		namespace Tests
		{
			public class Test
			{
				[Obsolete]
				public void OldMethod() {}
			}
		}
		""",
		version: "2.0.1",
		packageVersion: null,
		exclusions: null);

	[Test]
	public Task TestIgnoreObsoleteMethodInPrereleaseMajorVersion() => VerifyAnalyzerWithConfigAsync(
		"""
		using System;

		namespace Tests
		{
			public class Test
			{
				[Obsolete]
				public void OldMethod() {}
			}
		}
		""",
		version: "2.0.0-beta1",
		packageVersion: null,
		exclusions: null);

	[Test]
	public Task TestIgnoreObsoleteMethodInInternalClass() => VerifyAnalyzerWithConfigAsync(
		"""
		using System;

		namespace Tests
		{
			internal class Test
			{
				[Obsolete]
				public void OldMethod() {}
			}
		}
		""",
		version: "2.0.0",
		packageVersion: null,
		exclusions: null);

	[Test]
	public Task TestIgnoreObsoleteInternalMethod() => VerifyAnalyzerWithConfigAsync(
		"""
		using System;

		namespace Tests
		{
			public class Test
			{
				[Obsolete]
				internal void OldMethod() {}
			}
		}
		""",
		version: "2.0.0",
		packageVersion: null,
		exclusions: null);

	[Test]
	public Task TestFlagObsoleteProtectedMethod() => VerifyAnalyzerWithConfigAsync(
		"""
		using System;

		namespace Tests
		{
			public class Test
			{
				[Obsolete]
				protected void [|OldMethod|]() {}

				[Obsolete]
				protected internal void [|OldMethod2|]() {}
			}
		}
		""",
		version: "2.0.0",
		packageVersion: null,
		exclusions: null);

	[Test]
	public Task TestAssemblyInformationalVersionFallback() => VerifyAnalyzerWithConfigAsync(
		"""
		using System;

		[assembly: System.Reflection.AssemblyInformationalVersion("2.0.0")]

		namespace Tests
		{
			public class Test
			{
				[Obsolete]
				public void [|OldMethod|]() {}
			}
		}
		""",
		version: null,
		packageVersion: null,
		exclusions: null);

	[Test]
	public Task TestAssemblyInformationalVersionPrereleaseIsIgnored() => VerifyAnalyzerWithConfigAsync(
		"""
		using System;

		[assembly: System.Reflection.AssemblyInformationalVersion("2.0.0-beta1")]

		namespace Tests
		{
			public class Test
			{
				[Obsolete]
				public void OldMethod() {}
			}
		}
		""",
		version: null,
		packageVersion: null,
		exclusions: null);

	[Test]
	public Task TestExcludedNamespaceConfiguration() => VerifyAnalyzerWithConfigAsync(
		"""
		using System;

		namespace Tests.Compat
		{
			public class Test
			{
				[Obsolete]
				public void OldMethod() {}
			}
		}

		namespace Tests.Compat.Sub
		{
			public class Test2
			{
				[Obsolete]
				public void OldMethod2() {}
			}
		}

		namespace Tests.Other
		{
			public class Test3
			{
				[Obsolete]
				public void [|OldMethod3|]() {}
			}
		}
		""",
		version: "2.0.0",
		packageVersion: null,
		exclusions: "N:Tests.Compat");

	[Test]
	public Task TestExcludedTypeConfiguration() => VerifyAnalyzerWithConfigAsync(
		"""
		using System;

		namespace Tests
		{
			public class LegacyHelper
			{
				[Obsolete]
				public void OldMethod() {}
			}

			public class OtherHelper
			{
				[Obsolete]
				public void [|OldMethod|]() {}
			}
		}
		""",
		version: "2.0.0",
		packageVersion: null,
		exclusions: "T:Tests.LegacyHelper");

	[Test]
	public Task TestExcludedMethodConfiguration() => VerifyAnalyzerWithConfigAsync(
		"""
		using System;

		namespace Tests
		{
			public class Test
			{
				[Obsolete]
				public void OldMethod(int value) {}

				[Obsolete]
				public void [|OldMethod|](string value) {}
			}
		}
		""",
		version: "2.0.0",
		packageVersion: null,
		exclusions: "M:Tests.Test.OldMethod(System.Int32)");

	[Test]
	public Task TestIgnoreWhenNotPackableAndNotOptedIn() => VerifyAnalyzerWithConfigAsync(
		"""
		using System;

		namespace Tests
		{
			public class Test
			{
				[Obsolete]
				public void OldMethod() {}
			}
		}
		""",
		version: "2.0.0",
		packageVersion: null,
		exclusions: null,
		isPackable: "false",
		applyToNonPackableProjects: null);

	[Test]
	public Task TestFlagWhenNotPackableButOptedIn() => VerifyAnalyzerWithConfigAsync(
		"""
		using System;

		namespace Tests
		{
			public class Test
			{
				[Obsolete]
				public void [|OldMethod|]() {}
			}
		}
		""",
		version: "2.0.0",
		packageVersion: null,
		exclusions: null,
		isPackable: "false",
		applyToNonPackableProjects: "true");

	private static Task VerifyAnalyzerWithConfigAsync(
		string source,
		string? version,
		string? packageVersion,
		string? exclusions,
		string? isPackable = "true",
		string? applyToNonPackableProjects = null,
		params DiagnosticResult[] expected)
	{
		var test = new CSharpAnalyzerTest<RemoveObsoleteMembersInMajorVersionAnalyzer, DefaultVerifier>
		{
			TestCode = source.Replace("\r\n", "\n").Replace("\n", Environment.NewLine),
			ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
		};

		var globalConfigBuilder = new StringBuilder();
		globalConfigBuilder.AppendLine("is_global = true");
		if (version != null)
		{
			globalConfigBuilder.AppendLine($"build_property.Version = {version}");
		}
		if (packageVersion != null)
		{
			globalConfigBuilder.AppendLine($"build_property.PackageVersion = {packageVersion}");
		}
		if (exclusions != null)
		{
			globalConfigBuilder.AppendLine($"dotnet_code_quality.SHIMMER1050.excluded_symbol_names = {exclusions}");
		}
		if (isPackable != null)
		{
			globalConfigBuilder.AppendLine($"build_property.IsPackable = {isPackable}");
		}
		if (applyToNonPackableProjects != null)
		{
			globalConfigBuilder.AppendLine($"dotnet_code_quality.SHIMMER1050.apply_to_non_packable_projects = {applyToNonPackableProjects}");
		}

		test.TestState.AnalyzerConfigFiles.Add(("/.globalconfig", globalConfigBuilder.ToString()));
		test.ExpectedDiagnostics.AddRange(expected);
		return test.RunAsync();
	}
}
