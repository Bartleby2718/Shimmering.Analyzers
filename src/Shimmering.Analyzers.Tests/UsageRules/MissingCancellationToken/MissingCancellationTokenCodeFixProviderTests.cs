using Shimmering.Analyzers.UsageRules.MissingCancellationToken;

namespace Shimmering.Analyzers.Tests.UsageRules.MissingCancellationToken;

using Verifier = CSharpCodeFixVerifier<
	MissingCancellationTokenAnalyzer,
	MissingCancellationTokenCodeFixProvider,
	DefaultVerifier>;

public class MissingCancellationTokenCodeFixProviderTests : ShimmeringCodeFixProviderTests<MissingCancellationTokenAnalyzer, MissingCancellationTokenCodeFixProvider>
{
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
	public Task TestMethodWithOneParameter() => Verifier.VerifyCodeFixAsync(
		"""
		using System.Threading.Tasks;

		namespace Tests
		{
			class Test
			{
				public Task [|Do1Async|](string parameter) => Task.CompletedTask;
				public Task [|Do2Async|](int? parameter) => Task.CompletedTask;
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
				public Task Do1Async(string parameter, CancellationToken cancellationToken = default) => Task.CompletedTask;
				public Task Do2Async(int? parameter, CancellationToken cancellationToken = default) => Task.CompletedTask;
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
