using System;

namespace LfrlAnvil.Exceptions;

/// <summary>
/// Represents an error that occurred during an attempt to generate a value.
/// </summary>
public class ValueGenerationException : InvalidOperationException
{
    /// <summary>
    /// Creates a new <see cref="ValueGenerationException"/> instance.
    /// </summary>
    public ValueGenerationException()
        : base( ExceptionResources.FailedToGenerateNextValue ) { }
}
