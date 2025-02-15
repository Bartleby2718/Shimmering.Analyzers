namespace Shimmering.Analyzers.MisusedOrDefault;

internal static class MisusedOrDefaultHelpers
{
	// unfortunately, there's no API yet to create an immutable dictionary without first creating a mutable dictionary in a single expression
	public static readonly ImmutableDictionary<string, string> MethodMapping = ImmutableDictionary.CreateRange(new Dictionary<string, string>
	{
		{ nameof(Enumerable.ElementAtOrDefault), nameof(Enumerable.ElementAt) },
		{ nameof(Enumerable.SingleOrDefault), nameof(Enumerable.Single) },
		{ nameof(Enumerable.FirstOrDefault), nameof(Enumerable.First) },
		{ nameof(Enumerable.LastOrDefault), nameof(Enumerable.Last) },
	});
}
