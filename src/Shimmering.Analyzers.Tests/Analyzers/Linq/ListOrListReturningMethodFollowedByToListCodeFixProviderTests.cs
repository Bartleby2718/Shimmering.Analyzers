namespace Shimmering.Analyzers.Tests.Analyzers.Linq;

using Shimmering.Analyzers.Analyzers.Linq;
using Shimmering.Analyzers.CodeFixes.Linq;

public class ListOrListReturningMethodFollowedByToListCodeFixProviderTests : ShimmeringCodeFixProviderTests<ListOrListReturningMethodFollowedByToListAnalyzer, ListOrListReturningMethodFollowedByToListCodeFixProvider>
{
	[Test]
	public Task TestNewListWithoutInitializer() => VerifyCodeFixAsync(
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
	public Task TestListReturningMethod() => VerifyCodeFixAsync(
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
	public Task TestTrivia() => VerifyCodeFixAsync(
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
