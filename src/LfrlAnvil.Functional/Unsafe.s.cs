using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Internal;

namespace LfrlAnvil.Functional;

public static class Unsafe
{
    [Pure]
    public static Type? GetUnderlyingType(Type? type)
    {
        var result = UnderlyingType.GetForType( type, typeof( Unsafe<> ) );
        return result.Length == 0 ? null : result[0];
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Unsafe<T> Try<T>(Func<T> func)
    {
        try
        {
            return new Unsafe<T>( func() );
        }
        catch ( Exception exc )
        {
            return new Unsafe<T>( exc );
        }
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Unsafe<Nil> Try(Action action)
    {
        try
        {
            action();
            return new Unsafe<Nil>( Nil.Instance );
        }
        catch ( Exception exc )
        {
            return new Unsafe<Nil>( exc );
        }
    }
}