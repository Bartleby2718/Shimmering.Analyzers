using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Shimmering.Analyzers.Analyzers.Usage;
using Shimmering.Analyzers.Utilities;

namespace Shimmering.Analyzers.CodeFixes.Usage;

/// <summary>
/// Replaces mutable collection types for parameters with read-only interface types.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseReadOnlyCollectionParameterCodeFixProvider))]
public sealed class UseReadOnlyCollectionParameterCodeFixProvider : ShimmeringCodeFixProvider
{
	private const string Title = "Use read-only collection interface for parameter";

	private static readonly Dictionary<string, string> InterfaceNameMapping = new(StringComparer.Ordinal)
	{
		{ "List", "IReadOnlyList" },
		{ "IList", "IReadOnlyList" },
		{ "ICollection", "IReadOnlyCollection" },
		{ "Dictionary", "IReadOnlyDictionary" },
		{ "IDictionary", "IReadOnlyDictionary" },
		{ "HashSet", "IReadOnlySet" },
		{ "ISet", "IReadOnlySet" },
		{ "Collection", "IReadOnlyCollection" },
	};

	public sealed override ImmutableArray<string> FixableDiagnosticIds =>
		[DiagnosticIds.UsageRules.UseReadOnlyCollectionParameter];

	public override string SampleCodeFixed => """
		using System;
		using System.Collections.Generic;

		public class ReportGenerator
		{
			public void PrintReport(IReadOnlyList<string> items)
			{
				foreach (var item in items)
				{
					Console.WriteLine(item);
				}
			}
		}
		""";

	public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
	{
		var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
		if (root == null)
		{
			return;
		}

		var diagnostic = context.Diagnostics.First();
		if (!diagnostic.Properties.TryGetValue("IsFixable", out var isFixable) || isFixable != "true")
		{
			return;
		}

		var diagnosticSpan = diagnostic.Location.SourceSpan;
		var node = root.FindNode(diagnosticSpan);

		var parameterSyntax = node.FirstAncestorOrSelf<ParameterSyntax>();
		if (parameterSyntax == null)
		{
			return;
		}

		context.RegisterCodeFix(
			CodeAction.Create(
				Title,
				cancellationToken => ChangeTypeToReadOnlyAsync(context.Document, parameterSyntax, cancellationToken),
				nameof(UseReadOnlyCollectionParameterCodeFixProvider)),
			diagnostic);
	}

	private static async Task<Document> ChangeTypeToReadOnlyAsync(
		Document document,
		ParameterSyntax parameterSyntax,
		CancellationToken cancellationToken)
	{
		if (parameterSyntax.Type == null)
		{
			return document;
		}

		var genericName = parameterSyntax.Type.DescendantNodesAndSelf()
			.OfType<GenericNameSyntax>()
			.FirstOrDefault();

		if (genericName == null)
		{
			return document;
		}

		var originalName = genericName.Identifier.ValueText;
		if (!InterfaceNameMapping.TryGetValue(originalName, out var targetName))
		{
			return document;
		}

		var newGenericName = genericName.WithIdentifier(SyntaxFactory.Identifier(targetName));
		var newType = parameterSyntax.Type.ReplaceNode(genericName, newGenericName);
		var newParameter = parameterSyntax.WithType(newType);

		var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false) as CompilationUnitSyntax;
		if (root == null)
		{
			return document;
		}

		var newRoot = root.ReplaceNode(parameterSyntax, newParameter);
		var newRootWithUsing = CodeFixHelpers.EnsureUsingDirectivesExist(
			document,
			newRoot,
			[FullyQualifiedNamespaces.SystemCollectionsGeneric]);

		return document.WithSyntaxRoot(newRootWithUsing);
	}
}
