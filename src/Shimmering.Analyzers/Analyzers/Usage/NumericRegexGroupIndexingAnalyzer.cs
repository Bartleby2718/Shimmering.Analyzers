using Microsoft.CodeAnalysis.Operations;

using Shimmering.Analyzers.Core;

namespace Shimmering.Analyzers.Analyzers.Usage;

/// <summary>
/// Flags accesses of regular expression capture groups by numeric index.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class NumericRegexGroupIndexingAnalyzer : ShimmeringAnalyzer
{
	private const string Title = "Regex group accessed by numeric index";
	private const string MessageFormat = "Groups[{0}] accesses a capture group by position. Use a named group and Groups[\"{name}\"] to prevent silent breakage when the pattern changes.";
	private const string Category = RuleCategories.Usage;

	private static readonly DiagnosticDescriptor Rule = ShimmeringRuleFactory.Create(
		DiagnosticIds.UsageRules.NumericRegexGroupIndexing,
		Title,
		MessageFormat,
		Category,
		DiagnosticSeverity.Warning);

	public override string SampleCode => """
		using System.Text.RegularExpressions;

		class Test
		{
			void Do(Match match)
			{
				var group = [|match.Groups[1]|];
			}
		}
		""";

	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

	protected override void InitializeCore(AnalysisContext context)
	{
		context.RegisterOperationAction(AnalyzePropertyReference, OperationKind.PropertyReference);
	}

	private static void AnalyzePropertyReference(OperationAnalysisContext context)
	{
		var propertyReference = (IPropertyReferenceOperation)context.Operation;
		var property = propertyReference.Property;
		if (property == null || !property.IsIndexer)
		{
			return;
		}

		var containingType = property.ContainingType;
		if (containingType == null || containingType.ToDisplayString() != "System.Text.RegularExpressions.GroupCollection")
		{
			return;
		}

		var arguments = propertyReference.Arguments;
		if (arguments.Length != 1)
		{
			return;
		}

		var argumentValue = arguments[0].Value;
		if (argumentValue == null)
		{
			return;
		}

		var constantValue = argumentValue.ConstantValue;
		if (constantValue.HasValue && constantValue.Value is int index)
		{
			if (index > 0)
			{
				var location = propertyReference.Syntax.GetLocation();
				var diagnostic = Diagnostic.Create(
					Rule,
					location,
					index.ToString());
				context.ReportDiagnostic(diagnostic);
			}
		}
	}
}
