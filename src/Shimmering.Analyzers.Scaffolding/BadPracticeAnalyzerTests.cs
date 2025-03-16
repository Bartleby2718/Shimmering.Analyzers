using Shimmering.Analyzers.CATEGORY_PLACEHOLDERRules.BadPractice;

namespace Shimmering.Analyzers.Tests.CATEGORY_PLACEHOLDERRules.BadPractice;

using Verifier = CSharpAnalyzerVerifier<
	BadPracticeAnalyzer,
	DefaultVerifier>;

public class BadPracticeAnalyzerTests
{
	[Test]
	public Task TestSomethingThatShouldNotBeFlagged() => Verifier.VerifyAnalyzerAsync(
		"""
		using System.Threading;
		using System.Threading.Tasks;

		namespace Tests
		{
			class Test
			{
				public Task DoAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
			}
		}
		""");
}
