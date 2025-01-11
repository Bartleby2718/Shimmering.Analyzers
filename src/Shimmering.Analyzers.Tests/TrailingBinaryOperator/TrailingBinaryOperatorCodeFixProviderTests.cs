using Shimmering.Analyzers.TrailingBinaryOperator;

namespace Shimmering.Analyzers.Tests.TrailingBinaryOperator;

using Verifier = CSharpCodeFixVerifier<
	TrailingBinaryOperatorAnalyzer,
	TrailingBinaryOperatorCodeFixProvider,
	DefaultVerifier>;

public class TrailingBinaryOperatorCodeFixProviderTests
{
	[Test]
	public Task TestUnsupportedCases() => Verifier.VerifyAnalyzerAsync("""
		namespace Tests
		{
			class Test
			{
				public Test()
				{
					int a = 1 - 2;
					double b = 3.0
						- 4;
					string c = "left operand"
						+
						"right operand";
				}
			}
		}
		""");

	/// <summary>
	/// Illustrates how trivias (i.e. whitespaces and comments) are affected by the *current* business logic.
	/// This doesn't necessarily mean this is the *right* logic, but it is reasonable and should help the maintainers
	/// understand what's happening to the trivias.
	/// </summary>
	[Test]
	public Task TestCurrentTriviaHandlingLogic() => Verifier.VerifyCodeFixAsync(
		"""
		namespace Tests
		{
			class Test
			{
				public Test()
				{
					int a = 9 /* before operand 1 */ [|+|] /* after operand 1 */
						// line comment
						/* before right */ 8 /* after right */ [|-|] /* after operand 2 */
						/*
							block comment
						*/
						/* before right 2 */ 7;
				}
			}
		}
		""",
		"""
		namespace Tests
		{
			class Test
			{
				public Test()
				{
					int a = 9 /* before operand 1 */
						+ /* after operand 1 */// line comment
						/* before right */ 8 /* after right */
						- /* after operand 2 *//*
							block comment
						*/
						/* before right 2 */ 7;
				}
			}
		}
		""");

	[Test]
	public Task TestBinaryOperators() => Verifier.VerifyCodeFixAsync(
		"""
		namespace Tests
		{
			class Test
			{
				public Test()
				{
					int a = 9 [|+|]
						8 [|-|]
						7 [|*|]
						6 [|/|]
						5 [|%|]
						4 [|<<|]
						3 [|>>|]
						2 [|>>>|]
						1;
					bool b = true [||||]
						true [|&&|]
						true;
					int c = 1 [|||]
						2 [|&|]
						3 [|^|]
						4;
					bool d = true [|==|]
						true [|!=|]
						true;
					bool e = 1 [|<|]
						2;
					bool f = 1 [|<=|]
						2;
					bool g = 1 [|>|]
						2;
					bool h = 1 [|>=|]
						2;
					bool i = 1 [|is|]
						int;
					string j = "a" [|as|]
						string;
					string k = "" [|??|]
						"";
				}
			}
		}
		""",
		"""
		namespace Tests
		{
			class Test
			{
				public Test()
				{
					int a = 9
						+ 8
						- 7
						* 6
						/ 5
						% 4
						<< 3
						>> 2
						>>> 1;
					bool b = true
						|| true
						&& true;
					int c = 1
						| 2
						& 3
						^ 4;
					bool d = true
						== true
						!= true;
					bool e = 1
						< 2;
					bool f = 1
						<= 2;
					bool g = 1
						> 2;
					bool h = 1
						>= 2;
					bool i = 1
						is int;
					string j = "a"
						as string;
					string k = ""
						?? "";
				}
			}
		}
		""");

	[Test]
	public Task TestBinaryExpressionAsArgument() => Verifier.VerifyCodeFixAsync(
		"""
		namespace Tests
		{
			class Test
			{
				public Test()
				{
					Method($"{1}" [|+|]
						$"{2}");
					Method($"{3}" [|+|]
						"4");
					Method("{5}" [|+|]
						$"{6}");
					Method("{7}" [|+|]
						"8");
				}

				static void Method(string x) { }
			}
		}
		""",
		"""
		namespace Tests
		{
			class Test
			{
				public Test()
				{
					Method($"{1}"
						+ $"{2}");
					Method($"{3}"
						+ "4");
					Method("{5}"
						+ $"{6}");
					Method("{7}"
						+ "8");
				}

				static void Method(string x) { }
			}
		}
		""");
}
