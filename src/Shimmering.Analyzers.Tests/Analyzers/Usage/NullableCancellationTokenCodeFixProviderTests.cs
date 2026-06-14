using Shimmering.Analyzers.Analyzers.Usage;
using Shimmering.Analyzers.CodeFixes.Usage;

namespace Shimmering.Analyzers.Tests.Analyzers.Usage;

public class NullableCancellationTokenCodeFixProviderTests : ShimmeringCodeFixProviderTests<NullableCancellationTokenAnalyzer, NullableCancellationTokenCodeFixProvider>
{
	[Test]
	public Task TestNullableCancellationTokenWithDefault() => VerifyCodeFixAsync(
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
	public Task TestNullableCancellationTokenWithoutDefault() => VerifyCodeFixAsync(
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
	public Task TestTriviaWhenParametersStartOnSameColumn() => VerifyCodeFixAsync(
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
	public Task TestTriviaWhenParametersStartOnDifferentColumns() => VerifyCodeFixAsync(
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
