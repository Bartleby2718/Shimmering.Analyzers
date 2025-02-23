namespace Shimmering.Analyzers;

internal static class DiagnosticIds
{
	public const string SingleElementConcat = "SHIMMER1001";
	public const string TrailingBinaryOperator = "SHIMMER1002";
	public const string UniqueNonSetCollection = "SHIMMER1003";
	public const string RedundantSpreadElement = "SHIMMER1004";
	public const string VerboseLinqChain = "SHIMMER1005";
	public const string NullableCancellationToken = "SHIMMER1006";
	public const string MissingCancellationToken = "SHIMMER1007";
	public const string SingleUseIEnumerableMaterialization = "SHIMMER1008";
	public const string NegatedTernaryCondition = "SHIMMER1009";
	public const string MisusedOrDefault = "SHIMMER1010";
	public const string NonStaticClassWithStaticMembersOnly = "SHIMMER1011";
	public const string ArrayOrArrayReturningMethodFollowedByToArray = "SHIMMER1012";
	public const string ToListForEach = "SHIMMER1013";
	public const string ToArrayOrToListFollowedByEnumerableExtensionMethod = "SHIMMER1014";
}
