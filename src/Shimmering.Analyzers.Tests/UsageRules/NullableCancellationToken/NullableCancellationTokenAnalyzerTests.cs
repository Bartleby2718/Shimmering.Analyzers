using Shimmering.Analyzers.UsageRules.NullableCancellationToken;

namespace Shimmering.Analyzers.Tests.UsageRules.NullableCancellationToken;

using Verifier = CSharpAnalyzerVerifier<
	NullableCancellationTokenAnalyzer,
	DefaultVerifier>;

public class NullableCancellationTokenAnalyzerTests : ShimmeringAnalyzerTests<NullableCancellationTokenAnalyzer>
{
	[Test]
	public Task TestNonNullableCancellationTokens() => Verifier.VerifyAnalyzerAsync(
		"""
		using System;
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
}
