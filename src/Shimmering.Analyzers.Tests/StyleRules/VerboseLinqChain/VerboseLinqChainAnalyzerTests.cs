using Shimmering.Analyzers.StyleRules.VerboseLinqChain;

namespace Shimmering.Analyzers.Tests.StyleRules.VerboseLinqChain;

using Verifier = CSharpAnalyzerVerifier<
	VerboseLinqChainAnalyzer,
	DefaultVerifier>;

public class VerboseLinqChainAnalyzerTests
{
	[Test]
	public Task TestUnsupportedCases() => Verifier.VerifyAnalyzerAsync(
		"""
		using System.Linq;

		namespace Tests
		{
			class Test
			{
				void Method()
				{
					// no LINQ calls before ToArray
					var x1 = new[] { 1 }.ToArray();

					// irrelevant LINQ calls only
					var x2 = new[] { 1 }.Distinct().ToArray();
					var x3 = new[] { 1 }.Except(new[] { 2 }).ToArray();
					var x4 = new[] { 1 }.Skip(0).ToArray();

					// relevant LINQ calls followed by irrelevant LINQ calls
					var x5 = new[] { 1 }.Append(2).Intersect(new[] { 2 }).ToArray();
					var x6 = new[] { 1 }.Concat(new[] { 2 }).OfType<int>().ToArray();
					var x7 = new[] { 1 }.Prepend(2).Take(1).ToArray();

					// tuple declaration is not supported
					var (x8, x9) = (new[] { 1 }.Append(2).ToArray(), new[] { 1 }.Append(2).ToArray());
				}
			}
		}
		""");
}
