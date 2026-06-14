using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

using Shimmering.Analyzers.Analyzers.Usage;

namespace Shimmering.Analyzers.CodeFixes.Usage;

/// <summary>
/// Removes .ToHashSet(), if reported by <see cref="HashSetOrHashSetReturningMethodFollowedByToHashSetAnalyzer"/>.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(HashSetOrHashSetReturningMethodFollowedByToHashSetCodeFixProvider))]
public sealed class HashSetOrHashSetReturningMethodFollowedByToHashSetCodeFixProvider : ShimmeringRedundantInvocationCodeFixProvider
{
	public sealed override string CodeFixTitle => "Remove redundant .ToHashSet()";

	public sealed override string CodeFixEquivalenceKey => nameof(HashSetOrHashSetReturningMethodFollowedByToHashSetCodeFixProvider);

	public sealed override ImmutableArray<string> FixableDiagnosticIds =>
		[DiagnosticIds.UsageRules.HashSetOrHashSetReturningMethodFollowedByToHashSet];

	public sealed override string SampleCodeFixed => """
		using System.Collections.Generic;
		using System.Linq;

		namespace Tests;

		class Test
		{
			void Do()
			{
				HashSet<int> MyHashSet = new HashSet<int>();
			}
		}
		""";
}
