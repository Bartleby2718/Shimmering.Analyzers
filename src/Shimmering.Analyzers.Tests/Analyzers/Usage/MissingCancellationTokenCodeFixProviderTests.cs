using Shimmering.Analyzers.Analyzers.Usage;
using Shimmering.Analyzers.CodeFixes.Usage;

namespace Shimmering.Analyzers.Tests.Analyzers.Usage;

public class MissingCancellationTokenCodeFixProviderTests : ShimmeringCodeFixProviderTests<MissingCancellationTokenAnalyzer, MissingCancellationTokenCodeFixProvider>
{
	[Test]
	public Task TestMethodWithoutParameters() => VerifyCodeFixAsync(
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
	public Task TestMethodWithOneParameter() => VerifyCodeFixAsync(
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
		using System.Threading;
		using System.Threading.Tasks;

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
	public Task TestMethodWithParameters() => VerifyCodeFixAsync(
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
		using System.Threading;
		using System.Threading.Tasks;

		namespace Tests
		{
			class Test
			{
				public Task DoAsync(int a, string b, CancellationToken cancellationToken = default) => Task.CompletedTask;
			}
		}
		""");

	[Test]
	public Task TestTriviaWhenParametersStartOnSameColumn() => VerifyCodeFixAsync(
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
        using System.Threading;
        using System.Threading.Tasks;

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
	public Task TestTriviaWhenParametersStartOnDifferentColumns() => VerifyCodeFixAsync(
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
        using System.Threading;
        using System.Threading.Tasks;

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
