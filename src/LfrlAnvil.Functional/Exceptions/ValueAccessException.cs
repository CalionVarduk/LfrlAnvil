using System;

namespace LfrlAnvil.Functional.Exceptions;

/// <summary>
/// Represents an error that occurred during an invalid value access attempt.
/// </summary>
public class ValueAccessException : InvalidOperationException
{
    /// <summary>
    /// Creates a new <see cref="ValueAccessException"/> instance.
    /// </summary>
    /// <param name="message">Exception's <see cref="Exception.Message"/>.</param>
    /// <param name="memberName">Name of the accessed member.</param>
    public ValueAccessException(string message, string memberName)
        : base( message )
    {
        MemberName = memberName;
    }

    /// <summary>
    /// Name of the accessed member.
    /// </summary>
    public string MemberName { get; }
}
