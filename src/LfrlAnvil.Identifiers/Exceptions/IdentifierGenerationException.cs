using System;

namespace LfrlAnvil.Identifiers.Exceptions;

public class IdentifierGenerationException : InvalidOperationException
{
    public IdentifierGenerationException()
        : base( Resources.IdentifierGenerationFailure ) { }
}
