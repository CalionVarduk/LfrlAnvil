using System;

namespace LfrlAnvil.Identifiers.Exceptions;

/// <summary>
/// Represents an error that occurred during an attempt to generate an <see cref="Identifier"/>.
/// </summary>
public class IdentifierGenerationException : InvalidOperationException
{
    /// <summary>
    /// Creates a new <see cref="IdentifierGenerationException"/> instance.
    /// </summary>
    public IdentifierGenerationException()
        : base( Resources.IdentifierGenerationFailure ) { }
}
