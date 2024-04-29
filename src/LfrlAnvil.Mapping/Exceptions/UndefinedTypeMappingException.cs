using System;

namespace LfrlAnvil.Mapping.Exceptions;

/// <summary>
/// Represents an error that occurred due to an undefined mapping from <see cref="SourceType"/> to <see cref="DestinationType"/>.
/// </summary>
public class UndefinedTypeMappingException : InvalidOperationException
{
    /// <summary>
    /// Creates a new <see cref="UndefinedTypeMappingException"/> instance.
    /// </summary>
    /// <param name="sourceType">Source type.</param>
    /// <param name="destinationType">Destination type.</param>
    public UndefinedTypeMappingException(Type sourceType, Type destinationType)
        : base( Resources.UndefinedTypeMapping( sourceType, destinationType ) )
    {
        SourceType = sourceType;
        DestinationType = destinationType;
    }

    /// <summary>
    /// Source type.
    /// </summary>
    public Type SourceType { get; }

    /// <summary>
    /// Destination type.
    /// </summary>
    public Type DestinationType { get; }
}
