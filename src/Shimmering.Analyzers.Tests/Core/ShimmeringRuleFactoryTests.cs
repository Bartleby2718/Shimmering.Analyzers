using Microsoft.CodeAnalysis;

using NUnit.Framework;

using Shimmering.Analyzers.Core;

namespace Shimmering.Analyzers.Tests.Core;

[TestFixture]
public class ShimmeringRuleFactoryTests
{
	[Test]
	public void Create_GeneratesCorrectHelpLink_ForUsageRule()
	{
		var descriptor = ShimmeringRuleFactory.Create(
			"SHIMMER1000",
			"Title",
			"Message",
			RuleCategories.Usage,
			DiagnosticSeverity.Warning);

		Assert.That(descriptor.HelpLinkUri, Is.EqualTo("https://bartleby2718.github.io/Shimmering.Analyzers/docs/UsageRules/SHIMMER1000"));
	}

	[Test]
	public void Create_GeneratesCorrectHelpLink_ForStyleRule()
	{
		var descriptor = ShimmeringRuleFactory.Create(
			"SHIMMER2000",
			"Title",
			"Message",
			RuleCategories.Style,
			DiagnosticSeverity.Warning);

		Assert.That(descriptor.HelpLinkUri, Is.EqualTo("https://bartleby2718.github.io/Shimmering.Analyzers/docs/StyleRules/SHIMMER2000"));
	}

	[Test]
	public void Create_GeneratesCorrectHelpLink_ForPerformanceRule()
	{
		var descriptor = ShimmeringRuleFactory.Create(
			"SHIMMER1010",
			"Title",
			"Message",
			RuleCategories.Performance,
			DiagnosticSeverity.Warning);

		Assert.That(descriptor.HelpLinkUri, Is.EqualTo("https://bartleby2718.github.io/Shimmering.Analyzers/docs/UsageRules/SHIMMER1010"));
	}
}
