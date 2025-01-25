using Shimmering.Analyzers.MissingCancellationToken;

namespace Shimmering.Analyzers.Tests.MissingCancellationToken;

using Verifier = CSharpCodeFixVerifier<
	MissingCancellationTokenAnalyzer,
	MissingCancellationTokenCodeFixProvider,
	DefaultVerifier>;

public class MissingCancellationTokenCodeFixProviderTests
{
	[Test]
	public Task TestMethodsWithCancellationTokens() => Verifier.VerifyAnalyzerAsync(
		"""
		using System.Threading;
		using System.Threading.Tasks;

		namespace Tests
		{
			class Test
			{
				// CancellationToken is the only parameter
				public Task Do1Async(CancellationToken cancellationToken = default) => Task.CompletedTask;
				public Task Do2Async(CancellationToken cancellationToken) => Task.CompletedTask;

				// CancellationToken is the last parameter
				public Task Do3Async(int number, CancellationToken cancellationToken = default) => Task.CompletedTask;
				public Task Do4Async(int number, CancellationToken cancellationToken) => Task.CompletedTask;

				// another parameter after CancellationToken
				public Task Do5Async(CancellationToken cancellationToken = default, int number = 1) => Task.CompletedTask;
				public Task Do6Async(CancellationToken cancellationToken, int number) => Task.CompletedTask;
			}
		}
		""");

	[Test]
	public Task TestMethodWithoutParameters() => Verifier.VerifyCodeFixAsync(
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
	public Task TestMethodWithParameters() => Verifier.VerifyCodeFixAsync(
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

	[Test]
	public Task TestTriviaWhenParametersStartOnSameColumn() => Verifier.VerifyCodeFixAsync(
#pragma warning disable SA1027 // Use tabs correctly
		"""
        using System.Threading.Tasks;

        namespace Tests
        {
            class Test
            {
                public Task [|DoAsync|](int number,
                                    string word) => Task.CompletedTask;
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
                public Task DoAsync(int number,
                                    string word,
                                    CancellationToken cancellationToken = default) => Task.CompletedTask;
            }
        }
        """);
#pragma warning restore SA1027 // Use tabs correctly

	[Test]
	public Task TestTriviaWhenParametersStartOnDifferentColumns() => Verifier.VerifyCodeFixAsync(
#pragma warning disable SA1027 // Use tabs correctly
		"""
        using System.Threading.Tasks;

        namespace Tests
        {
            class Test
            {
                public Task [|DoAsync|](
                    int number,
                    string word) => Task.CompletedTask;
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
                public Task DoAsync(
                    int number,
                    string word,
                    CancellationToken cancellationToken = default) => Task.CompletedTask;
            }
        }
        """);
#pragma warning restore SA1027 // Use tabs correctly
}
