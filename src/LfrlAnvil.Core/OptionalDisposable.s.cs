using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Internal;

namespace LfrlAnvil;

public static class OptionalDisposable
{
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static OptionalDisposable<T> Create<T>(T value)
        where T : IDisposable
    {
        return new OptionalDisposable<T>( value );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static OptionalDisposable<T> TryCreate<T>(T? value)
        where T : IDisposable
    {
        return Generic<T>.IsNotNull( value ) ? Create( value ) : OptionalDisposable<T>.Empty;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static OptionalDisposable<T> TryCreate<T>(T? value)
        where T : struct, IDisposable
    {
        return value.HasValue ? Create( value.Value ) : OptionalDisposable<T>.Empty;
    }

    [Pure]
    public static Type? GetUnderlyingType(Type? type)
    {
        var result = UnderlyingType.GetForType( type, typeof( OptionalDisposable<> ) );
        return result.Length == 0 ? null : result[0];
    }
}
