using System.Reflection;

namespace Shimmering.Analyzers.Tests;

public class WatchmanTests
{
	[Test]
	public void TestDiagnosticIdsOnlyContainPublicConstStringFieldsStartingWithShimmerfFollowedByFourDigits()
	{
		var declaredMembers = typeof(DiagnosticIds).GetMembers(BindingFlags.DeclaredOnly);

		// every member should be a class
		var nonClassMembers = declaredMembers.Where(m => m is not Type type || !type.IsClass);
		Assert.That(nonClassMembers, Is.Empty);

		// every class should end with "Rules"
		var classes = declaredMembers.OfType<Type>();
		Assert.That(classes.Select(c => c.Name), Is.All.EndsWith("Rules"));

		// every member in those classes should be a field
		var nonFieldMembers = classes.SelectMany(t => t
			.GetMembers(BindingFlags.DeclaredOnly)
			.Where(m => m.MemberType != MemberTypes.Field));
		Assert.That(nonFieldMembers, Is.Empty);

		// all fields should be public const strings starting with "SHIMMER" followed by four digits
		var fields = classes.SelectMany(c => c.GetFields());
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
