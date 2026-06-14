using Shimmering.Analyzers.Analyzers.Usage;

namespace Shimmering.Analyzers.CodeFixes.Usage;

/// <summary>
/// Removes .ToArray(), if reported by <see cref="ArrayOrArrayReturningMethodFollowedByToArrayAnalyzer"/>.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ArrayOrArrayReturningMethodFollowedByToArrayCodeFixProvider))]
public sealed class ArrayOrArrayReturningMethodFollowedByToArrayCodeFixProvider : ShimmeringRedundantInvocationCodeFixProvider
{
	public sealed override string CodeFixTitle => "Remove redundant .ToArray()";

	public sealed override string CodeFixEquivalenceKey => nameof(ArrayOrArrayReturningMethodFollowedByToArrayCodeFixProvider);

	public sealed override ImmutableArray<string> FixableDiagnosticIds =>
		[DiagnosticIds.UsageRules.ArrayOrArrayReturningMethodFollowedByToArray];

	public sealed override string SampleCodeFixed => """
		using System.Linq;

		namespace Tests;

		class Test
		{
			void Do()
			{
				var array = "a b".Split(' ');
			}
		}
		""";
}
