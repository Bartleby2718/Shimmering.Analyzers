namespace Shimmering.Analyzers.UsageRules.ToArrayOrToListFollowedByLinqMethod;

/// <summary>
/// Replaces an unnecessary materialization, if reported by <see cref="ToArrayOrToListFollowedByLinqMethodAnalyzer"/>.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ToArrayOrToListFollowedByLinqMethodCodeFixProvider))]
public sealed class ToArrayOrToListFollowedByLinqMethodCodeFixProvider : ShimmeringRedundantInvocationCodeFixProvider
{
	public sealed override string CodeFixTitle => "Remove unnecessary materialization";

	public sealed override string CodeFixEquivalenceKey => nameof(ToArrayOrToListFollowedByLinqMethodCodeFixProvider);

	public sealed override ImmutableArray<string> FixableDiagnosticIds =>
		[DiagnosticIds.UsageRules.ToArrayOrToListFollowedByLinqMethod];

	public sealed override string SampleCodeFixed => """
		using System.Collections.Generic;
		using System.Linq;

		namespace Tests;
		class Test
		{
			void Do()
			{
				int[] numbers = [];
				var greaterThanThree = numbers.Where(x => x > 3);
			}
		}
		""";
}
