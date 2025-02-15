using System.Reflection;

namespace Shimmering.Analyzers.Tests;

public class WatchmanTests
{
	[Test]
	public void TestDiagnosticIdsOnlyContainPublicConstStringFieldsStartingWithShimmerfFollowedByFourDigits()
	{
		var type = typeof(DiagnosticIds);

		// get all members
		var declaredMembers = type.GetMembers(
			BindingFlags.DeclaredOnly
			| BindingFlags.Public
			| BindingFlags.NonPublic
			| BindingFlags.Static
			| BindingFlags.Instance);

		// every member should be a field
		var nonFieldMembers = declaredMembers.Where(m => m.MemberType != MemberTypes.Field);
		Assert.That(nonFieldMembers, Is.Empty);

		// get all fields
		var fields = type.GetFields(
			BindingFlags.DeclaredOnly
			| BindingFlags.Public
			| BindingFlags.NonPublic
			| BindingFlags.Static
			| BindingFlags.Instance);

		foreach (var field in fields)
		{
			Assert.Multiple(() =>
			{
				// public const
				Assert.That(field.IsPublic && field.IsLiteral);
				// string
				Assert.That(field.FieldType, Is.EqualTo(typeof(string)));
				// non-null
				var value = field.GetRawConstantValue() as string;
				Assert.That(value, Is.Not.Null);
				// SHIMMMER####
				Assert.That(value, Does.Match(@"SHIMMER\d{4}"));
			});
		}
	}
}
