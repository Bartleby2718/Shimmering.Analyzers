using Shimmering.Analyzers.StyleRules.PrimaryConstructorParameterReassignment;

namespace Shimmering.Analyzers.Tests.StyleRules.PrimaryConstructorParameterReassignment;

using Verifier = CSharpAnalyzerVerifier<
	PrimaryConstructorParameterReassignmentAnalyzer,
	DefaultVerifier>;

public class PrimaryConstructorParameterReassignmentAnalyzerTests : ShimmeringAnalyzerTests<PrimaryConstructorParameterReassignmentAnalyzer>
{
	[Test]
	public Task TestNonAssignmentIsNotFlagged() => Verifier.VerifyAnalyzerAsync(
		"""
		namespace Tests
		{
			class Test(int x)
			{
				public void HalfX()
				{
					var y = x / 2;
				}
			}
		}
		""");

	[Test]
	public Task TestMutationIsNotFlagged() => Verifier.VerifyAnalyzerAsync(
		"""
		namespace Tests
		{
			class Test(int[] x)
			{
				public void Method()
				{
					x[0] = 1;
				}
			}
		}
		""");

	[Test]
	public Task TestRegularConstructorIsNotFlagged() => Verifier.VerifyAnalyzerAsync(
		"""
		namespace Tests
		{
			class Test
			{
				public Test(int x)
				{
					x = 1;
				}
			}
		}
		""");

	[Test]
	public Task TestSimpleAssignment() => Verifier.VerifyAnalyzerAsync(
		"""
		namespace Tests
		{
			class Test(int x)
			{
				public void HalfX()
				{
					[|x|] = x / 2;
				}
			}
		}
		""");

	[Test]
	public Task TestChainedAssignment() => Verifier.VerifyAnalyzerAsync(
		"""
		namespace Tests
		{
			class Test(int x)
			{
				public void Two()
				{
					var y = [|x|] = 2;
				}
			}
		}
		""");

	[Test]
	public Task TestCompoundAssignment() => Verifier.VerifyAnalyzerAsync(
		"""
		namespace Tests
		{
			class Test(int x)
			{
				public void HalfX()
				{
					[|x|] /= 2;
				}
			}
		}
		""");

	[Test]
	public Task TestIncrement() => Verifier.VerifyAnalyzerAsync(
		"""
		namespace Tests
		{
			class Test(int x)
			{
				public void Increment()
				{
					++[|x|];
				}
			}
		}
		""");
}
