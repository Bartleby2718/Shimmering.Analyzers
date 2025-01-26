using System.Diagnostics.CodeAnalysis;

namespace Shimmering.Analyzers.RedundantSpreadElement;

internal static class RedundantSpreadElementHelpers
{
	public static bool TryGetInnerElementsOfSpreadElement(
		SpreadElementSyntax spreadElement,
		[NotNullWhen(returnValue: true)] out IEnumerable<CollectionElementSyntax>? innerElements)
	{
		innerElements = spreadElement.Expression switch
		{
			// case 1: new[] { 1, 2 }
			ImplicitArrayCreationExpressionSyntax implicitArrayCreation =>
				// an implicit array creation must have a nonempty initializer, so we don't do additional filtering here
				implicitArrayCreation.Initializer.Expressions
					.Select(e => SyntaxFactory.ExpressionElement(e).WithoutTrivia()),

			// case 2: new int[] { 1, 2 } or new int[2] { 1, 2 }
			ArrayCreationExpressionSyntax { Initializer: var initializer }
				// rule out arrays without initializers entirely, as it may be more concise/readable to do new int[n] than write n zeros
				// (of course we could remove new T[0], but we don't bother with this case and document the decision in a TestUnsupportedCases)
				when initializer is not null =>
				initializer.Expressions.Select(e => SyntaxFactory.ExpressionElement(e).WithoutTrivia()),

			// case 3: (int[])[1, 2]
			CastExpressionSyntax { Type: ArrayTypeSyntax, Expression: CollectionExpressionSyntax collectionExpression } =>
				collectionExpression.Elements,

			// case 4: new List<int>() { 1, 2 }, but also edge cases like new List<int> { } or new List<int>() { }
			ObjectCreationExpressionSyntax { Initializer: InitializerExpressionSyntax initializer }
				// rule out initializers that assign properties
				when initializer.Expressions.All(e => e is not AssignmentExpressionSyntax) =>
				initializer.Expressions.Select(e => SyntaxFactory.ExpressionElement(e).WithoutTrivia()),

			// case 5: ImmutableArray<T>.Empty, ImmutableList<T>.Empty, etc
			MemberAccessExpressionSyntax { Name.Identifier.Text: "Empty" } =>
				[],

			// case 6: Array.Empty<T>(), Enumerable.Empty<T>(), etc
			InvocationExpressionSyntax { Expression: MemberAccessExpressionSyntax memberAccess }
				when memberAccess.Name.Identifier.Text == "Empty" =>
				[],

			// default
			_ => null,
		};
		return innerElements != null;
	}
}
