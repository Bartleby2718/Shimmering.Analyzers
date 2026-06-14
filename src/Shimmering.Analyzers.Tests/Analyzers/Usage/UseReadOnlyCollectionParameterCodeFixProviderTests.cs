using Shimmering.Analyzers.Analyzers.Usage;
using Shimmering.Analyzers.CodeFixes.Usage;

namespace Shimmering.Analyzers.Tests.Analyzers.Usage;

public class UseReadOnlyCollectionParameterCodeFixProviderTests : ShimmeringCodeFixProviderTests<UseReadOnlyCollectionParameterAnalyzer, UseReadOnlyCollectionParameterCodeFixProvider>
{
	[Test]
	public Task TestCodeFixForListParameter() => VerifyCodeFixAsync(
		"""
		using System.Collections.Generic;

		public class ReportGenerator
		{
			public void PrintReport([|List<string> items|])
			{
				foreach (var item in items)
				{
				}
			}
		}
		""",
		"""
		using System.Collections.Generic;

		public class ReportGenerator
		{
			public void PrintReport(IReadOnlyList<string> items)
			{
				foreach (var item in items)
				{
				}
			}
		}
		""");

	[Test]
	public Task TestCodeFixForDictionaryParameter() => VerifyCodeFixAsync(
		"""
		using System.Collections.Generic;

		public class ReportGenerator
		{
			public void PrintReport([|Dictionary<string, int> items|])
			{
				foreach (var item in items)
				{
				}
			}
		}
		""",
		"""
		using System.Collections.Generic;

		public class ReportGenerator
		{
			public void PrintReport(IReadOnlyDictionary<string, int> items)
			{
				foreach (var item in items)
				{
				}
			}
		}
		""");
}
