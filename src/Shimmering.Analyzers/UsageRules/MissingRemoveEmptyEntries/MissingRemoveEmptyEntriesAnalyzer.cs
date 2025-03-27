using Shimmering.Analyzers.Utilities;

namespace Shimmering.Analyzers.UsageRules.MissingRemoveEmptyEntries;

/// <summary>
/// Reports instances of string.Split() where StringSplitOptions.RemoveEmptyEntries could have been used but wasn't used.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class MissingRemoveEmptyEntriesAnalyzer : ShimmeringSyntaxNodeAnalyzer
{
	private const string Title = "Use StringSplitOptions.RemoveEmptyEntries";
	private const string Message = "Use the overload of String.Split with StringSplitOptions.RemoveEmptyEntries to remove empty entries";
	private const string Category = "Usage";

	private static readonly DiagnosticDescriptor Rule = CreateRule(
		DiagnosticIds.UsageRules.MissingRemoveEmptyEntries,
		Title,
		Message,
		Category,
		DiagnosticSeverity.Info,
		isEnabledByDefault: true);

	public override string SampleCode => """
		using System;
		using System.Linq;

		namespace Tests;
		class Test
		{
			void Do(string input)
			{
				var x = [|input.Split(' ')
					.Where(x => x.Length > 0)|];
			}
		}
		""";

	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

	public override void RegisterSyntaxNodeAction(AnalysisContext context)
	{
		context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
	}

	private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
	{
		var invocation = (InvocationExpressionSyntax)context.Node;
		var semanticModel = context.SemanticModel;
		var cancellationToken = context.CancellationToken;
		if (!EnumerableHelpers.IsLinqMethodCall(semanticModel, invocation, cancellationToken, out var methodName)
			|| methodName != nameof(Enumerable.Where))
		{
			return;
		}

		// Currently, there exists only one overload of Enumerable.Where that accepts a single argument, but checking to make sure we can call Single().
		if (invocation.ArgumentList.Arguments.Count != 1)
			return;

		var argument = invocation.ArgumentList.Arguments.Single();
		if (argument.Expression is not SimpleLambdaExpressionSyntax lambda
			|| !IsNonEmptyStringCheck(semanticModel, cancellationToken, lambda))
		{
			return;
		}

		if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess
			|| memberAccess.Expression is not InvocationExpressionSyntax splitInvocation
			|| !StringHelpers.IsStringMethodCall(semanticModel, splitInvocation, cancellationToken, isStatic: false, out var innerMethodName)
			|| innerMethodName != nameof(string.Split))
		{
			return;
		}

		var symbolInfo = context.SemanticModel.GetSymbolInfo(splitInvocation);
		if (symbolInfo.Symbol is not IMethodSymbol methodSymbol
			|| methodSymbol.ContainingType?.SpecialType != SpecialType.System_String)
		{
			return;
		}

		// Only trigger if the Split call does not already use an overload that accepts StringSplitOptions.
		if (splitInvocation.ArgumentList.Arguments.Count != 1)
			return;

		var invocationToFlag = invocation.Parent is MemberAccessExpressionSyntax outerMemberAccess
			&& outerMemberAccess.Parent is InvocationExpressionSyntax outerInvocation
			&& EnumerableHelpers.IsLinqMethodCall(semanticModel, outerInvocation, cancellationToken, out var outerMethodName)
			&& outerMethodName == nameof(Enumerable.ToArray)
			? outerInvocation
			: invocation;

		var diagnostic = Diagnostic.Create(Rule, invocationToFlag.GetLocation());
		context.ReportDiagnostic(diagnostic);
	}

	private static bool IsNonEmptyStringCheck(SemanticModel semanticModel, CancellationToken cancellationToken, SimpleLambdaExpressionSyntax lambda)
	{
		var parameterName = lambda.Parameter.Identifier.ValueText;
		var body = lambda.Body;

		if (body is BinaryExpressionSyntax binaryExpression)
		{
			// Pattern 1: x.Length
			if (binaryExpression.Left is MemberAccessExpressionSyntax leftMember
				&& leftMember.Name.Identifier.Text == nameof(string.Length)
				&& leftMember.Expression is IdentifierNameSyntax lengthIdentifier
				&& lengthIdentifier.Identifier.ValueText == parameterName
				&& binaryExpression.Right is LiteralExpressionSyntax rightLiteral)
			{
				// Pattern 1-1: x => x.Length != 0 or x => x.Length > 0
				if (rightLiteral.Token.ValueText == "0"
					&& (binaryExpression.OperatorToken.IsKind(SyntaxKind.GreaterThanToken)
						|| binaryExpression.OperatorToken.IsKind(SyntaxKind.ExclamationEqualsToken)))
				{
					return true;
				}
				// Pattern 1-2: x => x.Length >= 1
				if (rightLiteral.Token.ValueText == "1"
					&& binaryExpression.OperatorToken.IsKind(SyntaxKind.GreaterThanEqualsToken))
				{
					return true;
				}
			}
			// Pattern 2: x => x != "" or x => x != string.Empty
			else if (
				binaryExpression.OperatorToken.IsKind(SyntaxKind.ExclamationEqualsToken)
				&& binaryExpression.Left is IdentifierNameSyntax notEqualsIdentifier
				&& notEqualsIdentifier.Identifier.ValueText == parameterName)
			{
				if (binaryExpression.Right is LiteralExpressionSyntax notEqualsRightLiteral
					&& notEqualsRightLiteral.Token.ValueText == string.Empty)
				{
					return true;
				}
				if (binaryExpression.Right is MemberAccessExpressionSyntax notEqualsMemberAccess
					&& semanticModel.GetSymbolInfo(notEqualsMemberAccess, cancellationToken).Symbol is IFieldSymbol symbol
					&& symbol is { Name: nameof(string.Empty) }
					&& symbol.ContainingType.SpecialType == SpecialType.System_String)
				{
					return true;
				}
			}
		}
		// Pattern 3: x => !string.IsNullOrEmpty(x)
		else if (body is PrefixUnaryExpressionSyntax prefixUnary)
		{
			if (prefixUnary.OperatorToken.IsKind(SyntaxKind.ExclamationToken)
				&& prefixUnary.Operand is InvocationExpressionSyntax isNullOrEmptyInvocation
				&& isNullOrEmptyInvocation.ArgumentList.Arguments.Count == 1
				&& isNullOrEmptyInvocation.ArgumentList.Arguments[0].Expression is IdentifierNameSyntax isNullOrEmptyArgument
				&& isNullOrEmptyArgument.Identifier.ValueText == parameterName
				&& isNullOrEmptyInvocation.Expression is MemberAccessExpressionSyntax isNullOrEmptyMemberAccess
				&& StringHelpers.IsStringMethodCall(semanticModel, isNullOrEmptyInvocation, cancellationToken, isStatic: true, out var isNullOrEmptyMethodName)
				&& isNullOrEmptyMethodName == nameof(string.IsNullOrEmpty))
			{
				return true;
			}
		}
		// Pattern 4: x => x.Any()
		else if (body is InvocationExpressionSyntax anyInvocation
			&& anyInvocation.Expression is MemberAccessExpressionSyntax anyMemberAccess
			&& anyMemberAccess.Name.Identifier.Text == nameof(Enumerable.Any)
			&& anyMemberAccess.Expression is IdentifierNameSyntax anyIdentifier
			&& anyIdentifier.Identifier.ValueText == parameterName)
		{
			return true;
		}
		// Pattern 5: x => x is not ""
		else if (body is IsPatternExpressionSyntax isPatternExpression
			&& isPatternExpression.Expression is IdentifierNameSyntax patternIdentifier
			&& patternIdentifier.Identifier.ValueText == parameterName
			&& isPatternExpression.Pattern is UnaryPatternSyntax unaryPattern
			&& unaryPattern.Kind() == SyntaxKind.NotPattern
			&& unaryPattern.Pattern is ConstantPatternSyntax constantPattern
			&& constantPattern.Expression is LiteralExpressionSyntax literal
			&& literal.Token.ValueText == string.Empty)
		{
			return true;
		}

		return false;
	}
}
