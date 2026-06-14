using Shimmering.Analyzers.Analyzers.Style;

namespace Shimmering.Analyzers.Tests.Analyzers.Style;

public class PrimaryConstructorParameterReassignmentAnalyzerTests : ShimmeringAnalyzerTests<PrimaryConstructorParameterReassignmentAnalyzer>
{
	[Test]
	public Task TestNonAssignmentIsNotFlagged() => VerifyAnalyzerAsync(
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
	public Task TestMutationIsNotFlagged() => VerifyAnalyzerAsync(
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
	public Task TestRegularConstructorIsNotFlagged() => VerifyAnalyzerAsync(
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
	public Task TestSimpleAssignment() => VerifyAnalyzerAsync(
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
	public Task TestChainedAssignment() => VerifyAnalyzerAsync(
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
	public Task TestCompoundAssignment() => VerifyAnalyzerAsync(
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
	public Task TestIncrement() => VerifyAnalyzerAsync(
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
