namespace Shimmering.Analyzers;

internal static class DiagnosticIds
{
	public static class UsageRules
	{
		// 100X: asynchronous programming
		public const string NullableCancellationToken = "SHIMMER1000";
		public const string MissingCancellationToken = "SHIMMER1001";
		// 101X: avoiding unnecessary materialization
		public const string ToListForEach = "SHIMMER1010";
		public const string ToArrayOrToListFollowedByEnumerableExtensionMethod = "SHIMMER1011";
		public const string ArrayOrArrayReturningMethodFollowedByToArray = "SHIMMER1012";
		// 102X: collection expression
		public const string RedundantSpreadElement = "SHIMMER1020";
		public const string SingleUseIEnumerableMaterialization = "SHIMMER1021";
		// 104X: string.Split()
		public const string MissingRemoveEmptyEntries = "SHIMMER1040";

		// 110X: idiomatic use of enumerable extension methods
		public const string MisusedOrDefault = "SHIMMER1100";
		public const string SingleElementConcat = "SHIMMER1101";
		public const string UniqueNonSetCollection = "SHIMMER1102";
	}

	public static class StyleRules
	{
		// 200X: collection expression
		public const string VerboseLinqChain = "SHIMMER2000";
		// 201X: double negatives
		public const string NegatedTernaryCondition = "SHIMMER2010";
		// 202X: variable declaration/assignment
		public const string RedundantOutVariable = "SHIMMER2020";
		// 203X: primary constructor
		public const string PrimaryConstructorParameterReassignment = "SHIMMER2030";
	}
}
