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
		public const string ToArrayOrToListFollowedByLinqMethod = "SHIMMER1011";
		public const string ArrayOrArrayReturningMethodFollowedByToArray = "SHIMMER1012";
		// TODO: 1013 = https://github.com/Bartleby2718/Shimmering.Analyzers/issues/49
		// TODO: 1014 = https://github.com/Bartleby2718/Shimmering.Analyzers/issues/50
		public const string SingleUseIEnumerableMaterialization = "SHIMMER1015";
		// 102X: collection expression
		public const string RedundantSpreadElement = "SHIMMER1020";
		// 103X: string.Split()
		public const string MissingRemoveEmptyEntries = "SHIMMER1030";
		// TODO: 1031 = https://github.com/Bartleby2718/Shimmering.Analyzers/issues/

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
	}
}
