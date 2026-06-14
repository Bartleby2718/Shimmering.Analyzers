using Shimmering.Analyzers.Analyzers.Usage;
using Shimmering.Analyzers.CodeFixes.Usage;

namespace Shimmering.Analyzers.Tests.Analyzers.Usage;

public class ToListForEachCodeFixProviderTests : ShimmeringCodeFixProviderTests<ToListForEachAnalyzer, ToListForEachCodeFixProvider>
{
	[Test]
#pragma warning disable SA1027 // Use tabs correctly
	public Task TestLambda() => VerifyCodeFixAsync(
		"""
		using System;
		using System.Linq;

		class Test
		{
		    void Do()
		    {
		        int[] numbers = [];
		        [|numbers.ToList().ForEach(n => Console.WriteLine(n))|];
		    }
		}
		""",
		"""
		using System;
		using System.Linq;
		
		class Test
		{
		    void Do()
		    {
		        int[] numbers = [];
		        foreach (var n in numbers)
		        {
		            Console.WriteLine(n);
		        }
		    }
		}
		""");
#pragma warning restore SA1027 // Use tabs correctly

	[Test]
#pragma warning disable SA1027 // Use tabs correctly
	public Task TestActionInvocation() => VerifyCodeFixAsync(
		"""
		using System;
		using System.Linq;

		class Test
		{
		    void Do()
		    {
		        Action[] actions = [];
		        [|actions.ToList().ForEach(a => a())|];
		    }
		}
		""",
		"""
		using System;
		using System.Linq;
		
		class Test
		{
		    void Do()
		    {
		        Action[] actions = [];
		        foreach (var a in actions)
		        {
		            a();
		        }
		    }
		}
		""");
#pragma warning restore SA1027 // Use tabs correctly

	[Test]
#pragma warning disable SA1027 // Use tabs correctly
	public Task TestMethodGroup() => VerifyCodeFixAsync(
		"""
		using System;
		using System.Linq;

		class Test
		{
		    void Do()
		    {
		        int[] numbers = [];
		        [|numbers.ToList().ForEach(Console.WriteLine)|]; // after statement
		    }
		}
		""",
		"""
		using System;
		using System.Linq;
		
		class Test
		{
		    void Do()
		    {
		        int[] numbers = [];
		        foreach (var item in numbers)
		        {
		            Console.WriteLine(item);
		        } // after statement
		    }
		}
		""");
#pragma warning restore SA1027 // Use tabs correctly

	[Test]
#pragma warning disable SA1027 // Use tabs correctly
	public Task TestTriviaForLambda() => VerifyCodeFixAsync(
		"""
		using System;
		using System.Linq;

		class Test
		{
		    void Do()
		    {
		        int[] numbers = [];
		        // line before enumerable
		        [|numbers // right after enumerable
		            // line before ToList
		            .ToList() // right after ToList
		            .ForEach(/* before lambda */n => Console.WriteLine(n)/* after lambda */)|]/* right after ForEach */; // after statement
		    }
		}
		""",
		"""
		using System;
		using System.Linq;
		
		class Test
		{
		    void Do()
		    {
		        int[] numbers = [];
		        // line before enumerable
		        foreach (var n in numbers)
		        {
		            Console.WriteLine(n)/* after lambda */;
		        }/* right after ForEach */ // after statement
		    }
		}
		""");
#pragma warning restore SA1027 // Use tabs correctly

	[Test]
#pragma warning disable SA1027 // Use tabs correctly
	public Task TestTriviaForActionInvocation() => VerifyCodeFixAsync(
		"""
		using System;
		using System.Linq;
		
		class Test
		{
		    void Do()
		    {
		        Action[] actions = [];
		        // line before enumerable
		        [|actions // right after enumerable
		            // line before ToList
		            .ToList() // right after ToList
		            .ForEach(/* before action */a => a()/* after action */)|]/* right after ForEach */; // after statement
		    }
		}
		""",
		"""
		using System;
		using System.Linq;
		
		class Test
		{
		    void Do()
		    {
		        Action[] actions = [];
		        // line before enumerable
		        foreach (var a in actions)
		        {
		            a()/* after action */;
		        }/* right after ForEach */ // after statement
		    }
		}
		""");
#pragma warning restore SA1027 // Use tabs correctly

	[Test]
#pragma warning disable SA1027 // Use tabs correctly
	public Task TestTriviaForMethodGroup() => VerifyCodeFixAsync(
		"""
		using System;
		using System.Linq;
		
		class Test
		{
		    void Do()
		    {
		        int[] numbers = [];
		        // line before enumerable
		        [|numbers // right after enumerable
		            // line before ToList
		            .ToList() // right after ToList
		            .ForEach(/* before method group */Console.WriteLine/* after method group */)|]/* right after ForEach */; // after statement
		    }
		}
		""",
		"""
		using System;
		using System.Linq;
		
		class Test
		{
		    void Do()
		    {
		        int[] numbers = [];
		        // line before enumerable
		        foreach (var item in numbers)
		        {
		            Console.WriteLine(item)/* after method group */;
		        }/* right after ForEach */ // after statement
		    }
		}
		""");
#pragma warning restore SA1027 // Use tabs correctly

	[Test]
#pragma warning disable SA1027 // Use tabs correctly
	public Task TestTriviaForIndentation() => VerifyCodeFixAsync(
		"""
		using System;
		using System.Collections.Generic;
		using System.Linq;
		
		class Test
		{
		    void Do(IEnumerable<int> myEnumerable)
		    {
		        [|myEnumerable
		            .ToList()
		            .ForEach(x =>
		            {
		                Console.WriteLine(x);
		            })|];
		    }
		}
		""",
		"""
		using System;
		using System.Collections.Generic;
		using System.Linq;
		
		class Test
		{
		    void Do(IEnumerable<int> myEnumerable)
		    {
		        foreach (var x in myEnumerable)
		        {
		            Console.WriteLine(x);
		        }
		    }
		}
		""");
#pragma warning restore SA1027 // Use tabs correctly

#pragma warning disable SA1027 // Use tabs correctly
	[Test]
	public Task TestBugReproToListForEach() => VerifyCodeFixAsync(
		"""
		using System;
		using System.Linq;
		using System.Collections.Generic;
		class C {
			void M(IEnumerable<int> myEnumerable) {
				[|myEnumerable
					.ToList()
					.ForEach(x =>
					{
						Console.WriteLine(x);
					})|];
			}
		}
		""",
		"""
		using System;
		using System.Linq;
		using System.Collections.Generic;
		class C {
			void M(IEnumerable<int> myEnumerable) {
		        foreach (var x in myEnumerable)
		        {
		            Console.WriteLine(x);
		        }
		    }
		}
		""");
#pragma warning restore SA1027 // Use tabs correctly
}
