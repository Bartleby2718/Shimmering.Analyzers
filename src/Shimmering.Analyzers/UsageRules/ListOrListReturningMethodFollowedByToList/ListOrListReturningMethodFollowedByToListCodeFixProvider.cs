namespace Shimmering.Analyzers.UsageRules.ListOrListReturningMethodFollowedByToList;

/// <summary>
/// Removes .ToList(), if reported by <see cref="ListOrListReturningMethodFollowedByToListAnalyzer"/>.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ListOrListReturningMethodFollowedByToListCodeFixProvider))]
public sealed class ListOrListReturningMethodFollowedByToListCodeFixProvider : ShimmeringRedundantInvocationCodeFixProvider
{
	public sealed override string CodeFixTitle => "Remove redundant .ToList()";

	public sealed override string CodeFixEquivalenceKey => nameof(ListOrListReturningMethodFollowedByToListCodeFixProvider);

	public sealed override ImmutableArray<string> FixableDiagnosticIds =>
		[DiagnosticIds.UsageRules.ListOrListReturningMethodFollowedByToList];

	public sealed override string SampleCodeFixed => """
		using System.Collections.Generic;
		using System.Linq;

		namespace Tests;
		class Test
		{
			void Do()
			{
				List<int> MyList = new List<int>();
			}
		}
		""";
}
