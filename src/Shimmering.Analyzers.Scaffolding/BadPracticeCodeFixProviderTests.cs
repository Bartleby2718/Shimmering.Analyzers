using Shimmering.Analyzers.CATEGORY_PLACEHOLDERRules.BadPractice;

namespace Shimmering.Analyzers.Tests.CATEGORY_PLACEHOLDERRules.BadPractice;

using Verifier = CSharpCodeFixVerifier<
	BadPracticeAnalyzer,
	BadPracticeCodeFixProvider,
	DefaultVerifier>;

public class BadPracticeCodeFixProviderTests
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

	[Test]
	public Task TestSomethingThatShouldBeFixed() => Verifier.VerifyCodeFixAsync(
		"""
		using System.Threading.Tasks;

		namespace Tests
		{
			class Test
			{
				public Task [|DoAsync|]() => Task.CompletedTask;
			}
		}
		""",
		"""
		using System.Threading.Tasks;
		using System.Threading;

		namespace Tests
		{
			class Test
			{
				public Task DoAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
			}
		}
		""");

	[Test]
	public Task TestTrivia() => Verifier.VerifyCodeFixAsync(
		"""
		using System.Threading.Tasks;

		namespace Tests
		{
			class Test
			{
				public Task [|DoAsync|](int a, string b) => Task.CompletedTask;
			}
		}
		""",
		"""
		using System.Threading.Tasks;
		using System.Threading;

		namespace Tests
		{
			class Test
			{
				public Task DoAsync(int a, string b, CancellationToken cancellationToken = default) => Task.CompletedTask;
			}
		}
		""");
}
