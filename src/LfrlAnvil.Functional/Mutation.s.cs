using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Internal;

namespace LfrlAnvil.Functional;

public static class Mutation
{
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Mutation<T> Create<T>(T oldValue, T value)
    {
        return new Mutation<T>( oldValue, value );
    }

    [Pure]
    public static Type? GetUnderlyingType(Type? type)
    {
        var result = UnderlyingType.GetForType( type, typeof( Mutation<> ) );
        return result.Length == 0 ? null : result[0];
    }
}