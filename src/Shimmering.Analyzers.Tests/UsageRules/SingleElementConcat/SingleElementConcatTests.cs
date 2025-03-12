using Shimmering.Analyzers.UsageRules.SingleElementConcat;

namespace Shimmering.Analyzers.Tests.UsageRules.SingleElementConcat;

using Verifier = CSharpCodeFixVerifier<
	SingleElementConcatAnalyzer,
	SingleElementConcatCodeFixProvider,
	DefaultVerifier>;

public class SingleElementConcatTests
{
	[Test]
	public Task TestSpreadElementIsIgnored() => Verifier.VerifyAnalyzerAsync(
		"""
		using System;
		using System.Collections.Generic;
		using System.Linq;

		namespace Tests
		{
			class Test
			{
				public Test()
				{
					List<int> list = [1];
					int[] array = new[] { 3, 4 }.Concat([..list]).ToArray();
				}
			}
		}
		""");

	[Test]
	public Task TestObjectInitializerIsIgnored() => Verifier.VerifyAnalyzerAsync("""
		using System;
		using System.Collections;
		using System.Collections.Generic;
		using System.Linq;

		namespace Tests
		{
			class Test
			{
				private IEnumerable<int> ints = new[] { 1, 2 }.Concat(new MyCollection<int> { SettableProperty = 1 });
			}

			public class MyCollection<T> : IEnumerable<T>
			{
				public int SettableProperty { get; set; }
				public IEnumerator<T> GetEnumerator() => throw new NotImplementedException();
				IEnumerator IEnumerable.GetEnumerator() => throw new NotImplementedException();
			}
		}
		""");

	[Test]
	public Task TestCollectionExpression() => Verifier.VerifyCodeFixAsync(
		"""
		using System;
		using System.Collections.Generic;
		using System.Linq;

		namespace Tests
		{
			class Test
			{
				private IEnumerable<int> _field;
				public IEnumerable<string>? Property { get; set; }
				public Test()
				{
					this._field = [|new[] { 1, 2 }.Concat([3])|];
					this.Property = [|new[] { "a", "b" }.Concat(["c"])|];
					IEnumerable<double> d = default!;
					d = [|new[] { 1.0, 2 }.Concat([3.0])|];
				}
			}
		}
		""",
		"""
		using System;
		using System.Collections.Generic;
		using System.Linq;

		namespace Tests
		{
			class Test
			{
				private IEnumerable<int> _field;
				public IEnumerable<string>? Property { get; set; }
				public Test()
				{
					this._field = new[] { 1, 2 }.Append(3);
					this.Property = new[] { "a", "b" }.Append("c");
					IEnumerable<double> d = default!;
					d = new[] { 1.0, 2 }.Append(3.0);
				}
			}
		}
		""");

	[Test]
	public Task TestCastExpressionContainingCollectionExpression() => Verifier.VerifyCodeFixAsync(
		"""
		using System;
		using System.Collections.Generic;
		using System.Linq;

		namespace Tests
		{
			class Test
			{
				private IEnumerable<int> _field;
				public IEnumerable<string>? Property { get; set; }
				public Test()
				{
					this._field = [|new[] { 1, 2 }.Concat((int[])[3])|];
					this.Property = [|new[] { "a", "b" }.Concat((List<string>)["c"])|];
					IEnumerable<double> d = default!;
					d = [|new[] { 1.0, 2 }.Concat((IReadOnlyCollection<double>)[3.0])|];
				}
			}
		}
		""",
		"""
		using System;
		using System.Collections.Generic;
		using System.Linq;

		namespace Tests
		{
			class Test
			{
				private IEnumerable<int> _field;
				public IEnumerable<string>? Property { get; set; }
				public Test()
				{
					this._field = new[] { 1, 2 }.Append(3);
					this.Property = new[] { "a", "b" }.Append("c");
					IEnumerable<double> d = default!;
					d = new[] { 1.0, 2 }.Append(3.0);
				}
			}
		}
		""");

	[Test]
	public Task TestTestCollectionInitializerWithArray() => Verifier.VerifyCodeFixAsync(
		"""
		using System;
		using System.Collections.Generic;
		using System.Linq;

		namespace Tests
		{
			class Test
			{
				IEnumerable<int> _field = [|new[] { 1, 2 }.Concat(new[] { 3 })|];
				IEnumerable<double> ReadOnlyProperty => [|new[] { 1.0, 2 }.Concat(new[] { 3.0 })|].ToList();
				IEnumerable<string> MethodWithFatArrow() => [|new[] { "a", "b" }.Concat(new[] { "c" })|].ToArray();
				IEnumerable<char> MethodWithReturn() { return [|new[] { 'a', 'b' }.Concat(new[] { 'c' })|].ToHashSet(); }
			}
		}
		""",
		"""
		using System;
		using System.Collections.Generic;
		using System.Linq;

		namespace Tests
		{
			class Test
			{
				IEnumerable<int> _field = new[] { 1, 2 }.Append(3);
				IEnumerable<double> ReadOnlyProperty => new[] { 1.0, 2 }.Append(3.0).ToList();
				IEnumerable<string> MethodWithFatArrow() => new[] { "a", "b" }.Append("c").ToArray();
				IEnumerable<char> MethodWithReturn() { return new[] { 'a', 'b' }.Append('c').ToHashSet(); }
			}
		}
		""");

	[Test]
	public Task TestCollectionInitializerWithList() => Verifier.VerifyCodeFixAsync(
		"""
		using System;
		using System.Collections.Generic;
		using System.Linq;

		namespace Tests
		{
			class Test
			{
				IEnumerable<int> _field = [|new[] { 1, 2 }.Concat(new List<int>() { 3 })|];
				IEnumerable<double> ReadOnlyProperty => [|new[] { 1.0, 2 }.Concat(new List<double>() { 3.0 })|];
				IEnumerable<string> MethodWithFatArrow() => [|new[] { "a", "b" }.Concat(new List<string>() { "c" })|];
				IEnumerable<char> MethodWithReturn() { return [|new[] { 'a', 'b' }.Concat(new List<char>() { 'c' })|]; }
			}
		}
		""",
		"""
		using System;
		using System.Collections.Generic;
		using System.Linq;

		namespace Tests
		{
			class Test
			{
				IEnumerable<int> _field = new[] { 1, 2 }.Append(3);
				IEnumerable<double> ReadOnlyProperty => new[] { 1.0, 2 }.Append(3.0);
				IEnumerable<string> MethodWithFatArrow() => new[] { "a", "b" }.Append("c");
				IEnumerable<char> MethodWithReturn() { return new[] { 'a', 'b' }.Append('c'); }
			}
		}
		""");
}
