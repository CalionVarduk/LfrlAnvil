using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Functional.Extensions;

public static class ObjectExtensions
{
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Maybe<T> ToMaybe<T>(this T? source)
        where T : notnull
    {
        return source;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static PartialEither<T> ToEither<T>(this T source)
    {
        return new PartialEither<T>( source );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Unsafe<T> ToUnsafe<T>(this T source)
    {
        return new Unsafe<T>( source );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Unsafe<T> ToUnsafe<T>(this Exception source)
    {
        return new Unsafe<T>( source );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Unsafe<Nil> ToUnsafe(this Exception source)
    {
        return source.ToUnsafe<Nil>();
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static PartialTypeCast<T> TypeCast<T>(this T source)
    {
        return new PartialTypeCast<T>( source );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Mutation<T> ToMutation<T>(this T source)
    {
        return source.Mutate( source );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Mutation<T> Mutate<T>(this T source, T newValue)
    {
        return new Mutation<T>( source, newValue );
    }
}
