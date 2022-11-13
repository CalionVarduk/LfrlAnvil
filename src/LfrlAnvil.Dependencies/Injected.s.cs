using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Internal;

namespace LfrlAnvil.Dependencies;

public static class Injected
{
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Injected<T> Create<T>(T instance)
    {
        return new Injected<T>( instance );
    }

    [Pure]
    public static Type? GetUnderlyingType(Type? type)
    {
        var result = UnderlyingType.GetForType( type, typeof( Injected<> ) );
        return result.Length == 0 ? null : result[0];
    }
}
