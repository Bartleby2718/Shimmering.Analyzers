using Shimmering.Analyzers.UsageRules.ListOrListReturningMethodFollowedByToList;

namespace Shimmering.Analyzers.Tests.UsageRules.ListOrListReturningMethodFollowedByToList;

using Verifier = CSharpCodeFixVerifier<
	ListOrListReturningMethodFollowedByToListAnalyzer,
	ListOrListReturningMethodFollowedByToListCodeFixProvider,
	DefaultVerifier>;

public class ListOrListReturningMethodFollowedByToListCodeFixProviderTests : ShimmeringCodeFixProviderTests<ListOrListReturningMethodFollowedByToListAnalyzer, ListOrListReturningMethodFollowedByToListCodeFixProvider>
{
	[Test]
	public Task TestNewListWithoutInitializer() => Verifier.VerifyCodeFixAsync(
		"""
		using System.Collections.Generic;
		using System.Linq;

		namespace Tests
		{
			class Test
			{
				public void Do()
				{
					List<int> MyList = [|new List<int>().ToList()|];
				}
			}
		}
		""",
		"""
		using System.Collections.Generic;
		using System.Linq;
		
		namespace Tests
		{
			class Test
			{
				public void Do()
				{
					List<int> MyList = new List<int>();
				}
			}
		}
		""");

	[Test]
	public Task TestListReturningMethod() => Verifier.VerifyCodeFixAsync(
		"""
		using System.Collections.Generic;
		using System.Linq;

		namespace Tests
		{
			class Test
			{
				public void Do()
				{
					List<string> MyList = [|GetList().ToList()|];
				}

				static List<string> GetList() => [];
			}
		}
		""",
		"""
		using System.Collections.Generic;
		using System.Linq;
		
		namespace Tests
		{
			class Test
			{
				public void Do()
				{
					List<string> MyList = GetList();
				}
		
				static List<string> GetList() => [];
			}
		}
		""");

	[Test]
	public Task TestTrivia() => Verifier.VerifyCodeFixAsync(
		"""
		using System.Collections.Generic;
		using System.Linq;
		
		namespace Tests
		{
			class Test
			{
				public void Do()
				{
					List<string> MyList = [|GetList()
						.ToList()|];
				}
		
				static List<string> GetList() => [];
			}
		}
		""",
		"""
		using System.Collections.Generic;
		using System.Linq;
		
		namespace Tests
		{
			class Test
			{
				public void Do()
				{
					List<string> MyList = GetList();
				}
		
				static List<string> GetList() => [];
			}
		}
		""");
}
