using Shimmering.Analyzers.NonStaticClassWithStaticMembersOnly;

namespace Shimmering.Analyzers.Tests.NonStaticClassWithStaticMembersOnly;

using Verifier = CSharpCodeFixVerifier<
	NonStaticClassWithStaticMembersOnlyAnalyzer,
	NonStaticClassWithStaticMembersOnlyCodeFixProvider,
	DefaultVerifier>;

public class NonStaticClassWithStaticMembersOnlyCodeFixProviderTests
{
	[Test]
	public Task TestAlreadyStaticClass() => Verifier.VerifyAnalyzerAsync(
		"""
		namespace Tests
		{
			static class Test
			{
				public const int One = 1;
			}
		}
		""");

	[Test]
	public Task TestPartialClass() => Verifier.VerifyAnalyzerAsync(
		"""
		namespace Tests
		{
			public partial class Test
			{
				public const int One = 1;
			}
		}
		""");

	[Test]
	public Task TestSealedClass() => Verifier.VerifyAnalyzerAsync(
		"""
		namespace Tests
		{
			public sealed class Test
			{
				public const int One = 1;
			}
		}
		""");

	[Test]
	public Task TestClassImplementingInterface() => Verifier.VerifyAnalyzerAsync(
		"""
		namespace Tests
		{
			class Test : ITest
			{
				public const int One = 1;
			}

			interface ITest { }
		}
		""");

	[Test]
	public Task TestClassInheritingNonObject() => Verifier.VerifyAnalyzerAsync(
		"""
		namespace Tests
		{
			class Test : BaseTest
			{
				public const int One = 1;
			}

			class BaseTest
			{
				public void InstanceMethod() { }
			}
		}
		""");

	[Test]
	public Task TestClassWithInstanceMember() => Verifier.VerifyAnalyzerAsync(
		"""
		namespace Tests
		{
			class Test
			{
				public int Value { get; }
			}
		}
		""");

	[Test]
#pragma warning disable SA1027 // Use tabs correctly
	public Task TestClassWithConstField() => Verifier.VerifyCodeFixAsync(
		"""
		namespace Tests
		{
		    class [|Test|]
		    {
		        public const int One = 1;
		    }
		}
		""",
		"""
		namespace Tests
		{
		    static class Test
		    {
		        public const int One = 1;
		    }
		}
		""");
#pragma warning restore SA1027 // Use tabs correctly

	[Test]
#pragma warning disable SA1027 // Use tabs correctly
	public Task TestClassWithStaticMember() => Verifier.VerifyCodeFixAsync(
		"""
		namespace Tests
		{
		    class [|Test|]
		    {
		        public static readonly int One = 1;
		    }
		}
		""",
		"""
		namespace Tests
		{
		    static class Test
		    {
		        public static readonly int One = 1;
		    }
		}
		""");
#pragma warning restore SA1027 // Use tabs correctly

	[Test]
#pragma warning disable SA1027 // Use tabs correctly
	public Task TestObjectInheritingClassWithStaticMember() => Verifier.VerifyCodeFixAsync(
		"""
		namespace Tests
		{
		    class [|Test|] : object
		    {
		        public static readonly int One = 1;
		    }
		}
		""",
		"""
		namespace Tests
		{
		    static class Test : object
		    {
		        public static readonly int One = 1;
		    }
		}
		""");
#pragma warning restore SA1027 // Use tabs correctly

	[Test]
#pragma warning disable SA1027 // Use tabs correctly
	public Task TestTrivia() => Verifier.VerifyCodeFixAsync(
		"""
		namespace Tests
		{
		    /* before access modifier */
		    internal
		    /* after access modifier */
		    class
		    /* after class */[|Test|]
		    {
		        public const int One = 1;
		    }
		}
		""",
		"""
		namespace Tests
		{
		    /* before access modifier */
		    internal static
		    /* after access modifier */
		    class
		    /* after class */[|Test|]
		    {
		        public const int One = 1;
		    }
		}
		""");
#pragma warning restore SA1027 // Use tabs correctly
}
