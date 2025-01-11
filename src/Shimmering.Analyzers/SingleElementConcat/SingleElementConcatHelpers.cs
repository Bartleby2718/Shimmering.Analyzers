using System.Diagnostics.CodeAnalysis;

namespace Shimmering.Analyzers.SingleElementConcat;

internal static class SingleElementConcatHelpers
{
	public static bool TryGetSingleElement(LanguageVersion? csharpVersion, InvocationExpressionSyntax invocation, [NotNullWhen(true)] out ExpressionSyntax? expression)
	{
		expression = null;

		if (invocation.ArgumentList.Arguments.Count != 1) { return false; }

		var argument = invocation.ArgumentList.Arguments[0].Expression;

		// Case 1: array initializer, as in new[] { 1, 2 }
		if (argument is ImplicitArrayCreationExpressionSyntax arrayCreation
			&& arrayCreation.Initializer?.Expressions.Count == 1)
		{
			expression = arrayCreation.Initializer.Expressions[0];
			return true;
		}

		// Case 2: other collection initializer, as in new List<int>() { 1, 2 }
		if (argument is ObjectCreationExpressionSyntax objectCreation
			&& objectCreation.Initializer?.Expressions.Count == 1)
		{
			expression = objectCreation.Initializer.Expressions[0];
			// ensure that the the argument was a collection initializer, not an object initializer
			if (expression is not AssignmentExpressionSyntax)
			{
				return true;
			}
		}

		// collection expresion is supported in C# 12+
		if (csharpVersion >= LanguageVersion.CSharp12)
		{
			// Case 3: cast epxression, as in (int[])[1, 2]
			var collectionExpressionCandidate = argument is CastExpressionSyntax castExpression
				? castExpression.Expression
				// Case 4: collection expression, as in [1, 2]
				: argument;

			if (collectionExpressionCandidate is CollectionExpressionSyntax collectionExpression
				&& collectionExpression.Elements.Count == 1
				&& collectionExpression.Elements[0] is ExpressionElementSyntax element)
			{
				expression = element.Expression;
				return true;
			}
		}

		return false;
	}
}
