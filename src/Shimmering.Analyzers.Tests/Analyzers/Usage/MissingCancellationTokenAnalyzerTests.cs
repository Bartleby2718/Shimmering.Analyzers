using Shimmering.Analyzers.Analyzers.Usage;

namespace Shimmering.Analyzers.Tests.Analyzers.Usage;

public class MissingCancellationTokenAnalyzerTests : ShimmeringAnalyzerTests<MissingCancellationTokenAnalyzer>
{
	[Test]
	public Task TestMethodsWithCancellationTokens() => VerifyAnalyzerAsync(
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

				// CancellationToken? already exists
				public Task Do7Async(CancellationToken? cancellationToken) => Task.CompletedTask;
			}
		}
		""");

	[Test]
	public Task TestExplicitInterfaceImplementation() => VerifyAnalyzerAsync(
		"""
		using System.Threading;
		using System.Threading.Tasks;

		namespace Tests
		{
			class Test : ITest
			{
				Task ITest.DoAsync() => Task.CompletedTask;
			}

			interface ITest
			{
				Task [|DoAsync|]();
			}
		}
		""");

	[Test]
	public Task TestImplicitInterfaceImplementation() => VerifyAnalyzerAsync(
		"""
		using System.Threading;
		using System.Threading.Tasks;

		namespace Tests
		{
			class Test : ITest
			{
				public Task DoAsync() => Task.CompletedTask;
			}

			interface ITest
			{
				Task [|DoAsync|]();
			}
		}
		""");

	[Test]
	public Task TestOverride() => VerifyAnalyzerAsync(
		"""
		using System.Threading;
		using System.Threading.Tasks;

		namespace Tests
		{
			class Test : Parent
			{
				public override Task DoAsync() => Task.CompletedTask;
			}

			class Parent
			{
				public virtual Task [|DoAsync|]() => Task.CompletedTask;
			}
		}
		""");
}
