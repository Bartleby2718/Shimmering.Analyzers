// UnreachableException was introduced in .NET 7, so adapted from the .NET Source Browser
// https://source.dot.net/#System.Private.CoreLib/src/libraries/System.Private.CoreLib/src/System/Diagnostics/UnreachableException.cs
#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace System.Diagnostics;
#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>
/// Exception thrown when the program executes an instruction that was thought to be unreachable.
/// </summary>
public sealed class UnreachableException : Exception
{
	private const string DefaultMessage = "This code should not be reachable.";

	/// <summary>
	/// Initializes a new instance of the <see cref="UnreachableException"/> class with the default error message.
	/// </summary>
	public UnreachableException()
		: base(DefaultMessage)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="UnreachableException"/>
	/// class with a specified error message.
	/// </summary>
	/// <param name="message">The error message that explains the reason for the exception.</param>
	public UnreachableException(string? message)
		: base(message ?? DefaultMessage)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="UnreachableException"/>
	/// class with a specified error message and a reference to the inner exception that is the cause of
	/// this exception.
	/// </summary>
	/// <param name="message">The error message that explains the reason for the exception.</param>
	/// <param name="innerException">The exception that is the cause of the current exception.</param>
	public UnreachableException(string? message, Exception? innerException)
		: base(message ?? DefaultMessage, innerException)
	{
	}
}
