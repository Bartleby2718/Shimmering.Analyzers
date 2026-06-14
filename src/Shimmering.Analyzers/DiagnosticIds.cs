namespace Shimmering.Analyzers;

public static class DiagnosticIds
{
	public static class UsageRules
	{
		// 100X: asynchronous programming
		public const string NullableCancellationToken = "SHIMMER1000";
		public const string MissingCancellationToken = "SHIMMER1001";
		// 101X: avoiding unnecessary materialization
		public const string ToListForEach = "SHIMMER1010";
		public const string ToArrayOrToListFollowedByLinqMethod = "SHIMMER1011";
		public const string ArrayOrArrayReturningMethodFollowedByToArray = "SHIMMER1012";
		public const string ListOrListReturningMethodFollowedByToList = "SHIMMER1013";
		public const string HashSetOrHashSetReturningMethodFollowedByToHashSet = "SHIMMER1014";
		public const string SingleUseIEnumerableMaterialization = "SHIMMER1015";
		// 102X: collection expression
		public const string RedundantSpreadElement = "SHIMMER1020";
		// 103X: string.Split()
		public const string MissingRemoveEmptyEntries = "SHIMMER1030";
		public const string UseTrimEntries = "SHIMMER1031";

		// 104X: CultureInfo construction
		public const string UseGetCultureInfo = "SHIMMER1040";

		// 105X: Obsolete API management
		public const string RemoveObsoleteMembersInMajorVersion = "SHIMMER1050";

		// 106X: read-only collection parameters
		public const string UseReadOnlyCollectionParameter = "SHIMMER1060";

		// 107X: regex capture groups
		public const string UnnamedRegexCaptureGroup = "SHIMMER1070";
		public const string NumericRegexGroupIndexing = "SHIMMER1071";

		// 110X: idiomatic use of LINQ methods
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
		// 204X: fully-qualified references
		public const string ForbidFullyQualifiedTypeReferences = "SHIMMER2040";
	}
}
