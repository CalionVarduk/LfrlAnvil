using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Functional.Extensions;

/// <summary>
/// Contains various generic object extensions.
/// </summary>
public static class ObjectExtensions
{
    /// <summary>
    /// Creates a new <see cref="Maybe{T}"/> instance.
    /// </summary>
    /// <param name="source">Source object.</param>
    /// <typeparam name="T">Object type.</typeparam>
    /// <returns>
    /// New <see cref="Maybe{T}"/> instance equivalent to <paramref name="source"/>
    /// or <see cref="Maybe{T}.None"/> when <paramref name="source"/> is null.
    /// </returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Maybe<T> ToMaybe<T>(this T? source)
        where T : notnull
    {
        return source;
    }

    /// <summary>
    /// Creates a new <see cref="PartialEither{T1}"/> instance that can be used to create an <see cref="Either{T1,T2}"/> instance.
    /// </summary>
    /// <param name="source">Source object.</param>
    /// <typeparam name="T">Object type.</typeparam>
    /// <returns>New <see cref="PartialEither{T1}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static PartialEither<T> ToEither<T>(this T source)
    {
        return new PartialEither<T>( source );
    }

    /// <summary>
    /// Creates a new <see cref="Erratic{T}"/> instance.
    /// </summary>
    /// <param name="source">Source object.</param>
    /// <typeparam name="T">Object type.</typeparam>
    /// <returns>
    /// New <see cref="Erratic{T}"/> instance without an error, equivalent to the provided <paramref name="source"/>.
    /// </returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Erratic<T> ToErratic<T>(this T source)
    {
        return new Erratic<T>( source );
    }

    /// <summary>
    /// Creates a new <see cref="Erratic{T}"/> instance.
    /// </summary>
    /// <param name="source">Source exception.</param>
    /// <typeparam name="T">Object type.</typeparam>
    /// <returns>
    /// New <see cref="Erratic{T}"/> instance with an error equal to the provided <paramref name="source"/>.
    /// </returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Erratic<T> ToErratic<T>(this Exception source)
    {
        return new Erratic<T>( source );
    }

    /// <summary>
    /// Creates a new <see cref="Erratic{T}"/> instance with <see cref="Nil"/> value type.
    /// </summary>
    /// <param name="source">Source exception.</param>
    /// <returns>
    /// New <see cref="Erratic{T}"/> instance with an error equal to the provided <paramref name="source"/>.
    /// </returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Erratic<Nil> ToErratic(this Exception source)
    {
        return source.ToErratic<Nil>();
    }

    /// <summary>
    /// Creates a new <see cref="PartialTypeCast{TSource}"/> instance that can be used to create
    /// a <see cref="TypeCast{TSource,TDestination}"/> instance.
    /// </summary>
    /// <param name="source">Source object.</param>
    /// <typeparam name="T">Source object type.</typeparam>
    /// <returns>New <see cref="PartialTypeCast{TSource}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static PartialTypeCast<T> TypeCast<T>(this T source)
    {
        return new PartialTypeCast<T>( source );
    }

    /// <summary>
    /// Creates a new <see cref="Mutation{T}"/> instance.
    /// </summary>
    /// <param name="source">Source object.</param>
    /// <typeparam name="T">Object type.</typeparam>
    /// <returns>
    /// New <see cref="Mutation{T}"/> instance with <see cref="Mutation{T}.OldValue"/> and <see cref="Mutation{T}.Value"/>
    /// equal to the provided <paramref name="source"/>.
    /// </returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Mutation<T> ToMutation<T>(this T source)
    {
        return source.Mutate( source );
    }

    /// <summary>
    /// Creates a new <see cref="Mutation{T}"/> instance.
    /// </summary>
    /// <param name="source">Source object.</param>
    /// <param name="newValue">New value object.</param>
    /// <typeparam name="T">Object type.</typeparam>
    /// <returns>
    /// New <see cref="Mutation{T}"/> instance with <see cref="Mutation{T}.OldValue"/> equal to the provided <paramref name="source"/>
    /// and <see cref="Mutation{T}.Value"/> equal to the provided <paramref name="newValue"/>.
    /// </returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Mutation<T> Mutate<T>(this T source, T newValue)
    {
        return new Mutation<T>( source, newValue );
    }
}
