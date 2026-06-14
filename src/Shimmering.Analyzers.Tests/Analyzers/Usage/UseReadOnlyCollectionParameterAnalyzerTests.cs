using Shimmering.Analyzers.Analyzers.Usage;

namespace Shimmering.Analyzers.Tests.Analyzers.Usage;

public class UseReadOnlyCollectionParameterAnalyzerTests : ShimmeringAnalyzerTests<UseReadOnlyCollectionParameterAnalyzer>
{
	[Test]
	public Task TestFlagListParameter() => VerifyAnalyzerAsync(
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
		""");

	[Test]
	public Task TestFlagIListParameter() => VerifyAnalyzerAsync(
		"""
		using System.Collections.Generic;

		public class ReportGenerator
		{
			public void PrintReport([|IList<string> items|])
			{
				foreach (var item in items)
				{
				}
			}
		}
		""");

	[Test]
	public Task TestFlagHashSetParameter() => VerifyAnalyzerAsync(
		"""
		using System.Collections.Generic;

		public class ReportGenerator
		{
			public void PrintReport([|HashSet<string> items|])
			{
				foreach (var item in items)
				{
				}
			}
		}
		""");

	[Test]
	public Task TestIgnoreOverrideMethod() => VerifyAnalyzerAsync(
		"""
		using System.Collections.Generic;

		public class BaseClass
		{
			public virtual void PrintReport(List<string> items) {}
		}

		public class ReportGenerator : BaseClass
		{
			public override void PrintReport(List<string> items)
			{
				foreach (var item in items)
				{
				}
			}
		}
		""");

	[Test]
	public Task TestIgnoreInterfaceMethod() => VerifyAnalyzerAsync(
		"""
		using System.Collections.Generic;

		public interface IReportGenerator
		{
			void PrintReport(List<string> items);
		}
		""");

	[Test]
	public Task TestIgnoreImplicitInterfaceImplementation() => VerifyAnalyzerAsync(
		"""
		using System.Collections.Generic;

		public interface IReportGenerator
		{
			void PrintReport(List<string> items);
		}

		public class ReportGenerator : IReportGenerator
		{
			public void PrintReport(List<string> items)
			{
				foreach (var item in items)
				{
				}
			}
		}
		""");

	[Test]
	public Task TestIgnoreWhenMutatedViaAdd() => VerifyAnalyzerAsync(
		"""
		using System.Collections.Generic;

		public class ReportGenerator
		{
			public void PrintReport(List<string> items)
			{
				items.Add("new item");
			}
		}
		""");

	[Test]
	public Task TestIgnoreWhenMutatedViaIndexerWrite() => VerifyAnalyzerAsync(
		"""
		using System.Collections.Generic;

		public class ReportGenerator
		{
			public void PrintReport(List<string> items)
			{
				items[0] = "new item";
			}
		}
		""");

	[Test]
	public Task TestIgnoreWhenMutatedViaPropertyWrite() => VerifyAnalyzerAsync(
		"""
		using System.Collections.Generic;

		public class ReportGenerator
		{
			public void PrintReport(List<string> items)
			{
				items.Capacity = 10;
			}
		}
		""");

	[Test]
	public Task TestIgnoreWhenReassigned() => VerifyAnalyzerAsync(
		"""
		using System.Collections.Generic;

		public class ReportGenerator
		{
			public void PrintReport(List<string> items)
			{
				items = new List<string>();
			}
		}
		""");

	[Test]
	public Task TestIgnoreWhenEscapingAsMutableArgument() => VerifyAnalyzerAsync(
		"""
		using System.Collections.Generic;

		public class ReportGenerator
		{
			public void Consume(IList<string> list) { list.Clear(); }

			public void PrintReport(List<string> items)
			{
				Consume(items);
			}
		}
		""");
}
