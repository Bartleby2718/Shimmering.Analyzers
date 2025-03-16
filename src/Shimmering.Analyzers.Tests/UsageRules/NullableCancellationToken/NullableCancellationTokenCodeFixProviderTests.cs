using Shimmering.Analyzers.UsageRules.NullableCancellationToken;

namespace Shimmering.Analyzers.Tests.UsageRules.NullableCancellationToken;

using Verifier = CSharpCodeFixVerifier<
	NullableCancellationTokenAnalyzer,
	NullableCancellationTokenCodeFixProvider,
	DefaultVerifier>;

public class NullableCancellationTokenCodeFixProviderTests : ShimmeringCodeFixProviderTests<NullableCancellationTokenAnalyzer, NullableCancellationTokenCodeFixProvider>
{
	[Test]
	public Task TestNullableCancellationTokenWithDefault() => Verifier.VerifyCodeFixAsync(
		"""
		using System.Threading;
		using System.Threading.Tasks;

		namespace Tests
		{
			class Test
			{
				public Task Do1Async([|CancellationToken? cancellationToken = null|]) => Task.CompletedTask;
				public Task Do2Async([|CancellationToken? cancellationToken = default|]) => Task.CompletedTask;
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
				public Task Do1Async(CancellationToken cancellationToken = default) => Task.CompletedTask;
				public Task Do2Async(CancellationToken cancellationToken = default) => Task.CompletedTask;
			}
		}
		""");

	[Test]
	public Task TestNullableCancellationTokenWithoutDefault() => Verifier.VerifyCodeFixAsync(
		"""
		using System.Threading;
		using System.Threading.Tasks;

		namespace Tests
		{
			class Test
			{
				public Task DoAsync([|CancellationToken? cancellationToken|]) => Task.CompletedTask;
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
				public Task DoAsync(CancellationToken cancellationToken) => Task.CompletedTask;
			}
		}
		""");

	[Test]
	public Task TestTriviaWhenParametersStartOnSameColumn() => Verifier.VerifyCodeFixAsync(
#pragma warning disable SA1027 // Use tabs correctly
		"""
        using System.Threading;
        using System.Threading.Tasks;
        
        namespace Tests
        {
            class Test
            {
                public Task DoAsync(int number,
                               [|CancellationToken? cancellationToken|]) => Task.CompletedTask;
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
                               CancellationToken cancellationToken) => Task.CompletedTask;
            }
        }
        """);
#pragma warning restore SA1027 // Use tabs correctly

	[Test]
	public Task TestTriviaWhenParametersStartOnDifferentColumns() => Verifier.VerifyCodeFixAsync(
#pragma warning disable SA1027 // Use tabs correctly
		"""
        using System.Threading;
        using System.Threading.Tasks;
        
        namespace Tests
        {
            class Test
            {
                public Task DoAsync(
                    int number,
                    [|CancellationToken? cancellationToken|]) => Task.CompletedTask;
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
                    CancellationToken cancellationToken) => Task.CompletedTask;
            }
        }
        """);
#pragma warning restore SA1027 // Use tabs correctly
}
