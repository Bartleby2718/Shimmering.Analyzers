using Shimmering.Analyzers.Utilities;

namespace Shimmering.Analyzers.VerboseLinqChain;

internal static class VerboseLinqChainHelpers
{
	/// <summary>
	/// Checks if a collection expression can be built from <paramref name="lastInvocation"/>.
	/// However, if you don't need the actual collection expression, set <paramref name="doConstructCollectionExpression"/> to false to avoid unnecessary work.
	/// </summary>
	public static bool TryConstructCollectionExpression(
		SemanticModel semanticModel,
		InvocationExpressionSyntax lastInvocation,
		bool doConstructCollectionExpression,
		out CollectionExpressionSyntax? collectionElements)
	{
		if (!EnumerableHelpers.IsLinqExtensionMethodCall(semanticModel, lastInvocation, out var lastMethodName)
			|| lastMethodName is not (nameof(Enumerable.ToArray) or nameof(Enumerable.ToList) or "ToHashSet")
			|| lastInvocation.Expression is not MemberAccessExpressionSyntax lastMemberAccess
			|| lastMemberAccess.Expression is not InvocationExpressionSyntax invocation)
		{
			collectionElements = null;
			return false;
		}

		var foundRelevantLinqCalls = false;
		Stack<(bool IsPrepend, CollectionElementSyntax Element)> elements = [];

		while (invocation is not null)
		{
			if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
			{
				collectionElements = null;
				return false;
			}

			(bool IsPrepend, CollectionElementSyntax CollectionElement)? result = memberAccess.Name.Identifier.Text switch
			{
				nameof(Enumerable.Append) => (false, SyntaxFactory.ExpressionElement(invocation.ArgumentList.Arguments[0].Expression)),
				nameof(Enumerable.Prepend) => (true, SyntaxFactory.ExpressionElement(invocation.ArgumentList.Arguments[0].Expression)),
				nameof(Enumerable.Concat) => (false, SyntaxFactory.SpreadElement(invocation.ArgumentList.Arguments[0].Expression)),
				_ => null,
			};
			if (result.HasValue)
			{
				foundRelevantLinqCalls = true;
				if (doConstructCollectionExpression)
				{
					elements.Push(result.Value);
				}

				if (memberAccess.Expression is InvocationExpressionSyntax previousInvocation)
				{
					invocation = previousInvocation;
					continue;
				}
			}

			// although this one was not a relevant LINQ call, we have already found some
			if (foundRelevantLinqCalls)
			{
				if (doConstructCollectionExpression)
				{
					var spreadElement = SyntaxFactory.SpreadElement(memberAccess.Expression);
					elements.Push((IsPrepend: false, spreadElement));
				}
				break;
			}
			// we have found no relevant LINQ calls, so we cannot construct a collection expression
			else
			{
				collectionElements = null;
				return false;
			}
		}

		if (!doConstructCollectionExpression)
		{
			collectionElements = null;
			return true;
		}

		List<CollectionElementSyntax> finalElements = [];
		// no need to reverse because we used a stack in the first place
		foreach (var (isPrepend, element) in elements)
		{
			if (isPrepend)
			{
				finalElements.Insert(0, element);
			}
			else
			{
				finalElements.Add(element);
			}
		}

		collectionElements = SyntaxFactory.CollectionExpression([.. finalElements]);
		return true;
	}
}
