using System;

namespace LfrlAnvil.Mapping.Exceptions;

public class UndefinedTypeMappingException : InvalidOperationException
{
    public UndefinedTypeMappingException(Type sourceType, Type destinationType)
        : base( Resources.UndefinedTypeMapping( sourceType, destinationType ) )
    {
        SourceType = sourceType;
        DestinationType = destinationType;
    }

    public Type SourceType { get; }
    public Type DestinationType { get; }
}