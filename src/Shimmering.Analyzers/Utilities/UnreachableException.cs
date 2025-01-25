namespace System.Diagnostics;

/// <summary>
/// UnreachableException was introduced in .NET 7, so creating the same thing here.
/// </summary>
public sealed class UnreachableException(string message = "This code should not be reachable.") : Exception(message)
{
}
