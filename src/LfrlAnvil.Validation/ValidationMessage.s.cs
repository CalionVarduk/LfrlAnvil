using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Internal;

namespace LfrlAnvil.Validation;

public static class ValidationMessage
{
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ValidationMessage<TResource> Create<TResource>(TResource resource, params object?[] parameters)
    {
        return new ValidationMessage<TResource>( resource, parameters );
    }

    [Pure]
    public static Type? GetUnderlyingType(Type? type)
    {
        var result = UnderlyingType.GetForType( type, typeof( ValidationMessage<> ) );
        return result.Length == 0 ? null : result[0];
    }
}
