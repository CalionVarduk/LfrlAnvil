using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Functional.Extensions;

/// <summary>
/// Contains <see cref="Either{T1,T2}"/> extension methods.
/// </summary>
public static class EitherExtensions
{
    /// <summary>
    /// Creates a new <see cref="Maybe{T}"/> instance.
    /// </summary>
    /// <param name="source">Source either.</param>
    /// <typeparam name="T1">First either type.</typeparam>
    /// <typeparam name="T2">Second either type.</typeparam>
    /// <returns>
    /// New <see cref="Maybe{T}"/> instance equivalent to the first value of <paramref name="source"/>
    /// or <see cref="Maybe{T}.None"/> when <see cref="Either{T1,T2}.HasFirst"/> of <paramref name="source"/> is equal to <b>false</b>.
    /// </returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Maybe<T1> ToMaybe<T1, T2>(this Either<T1, T2> source)
        where T1 : notnull
    {
        return source.HasFirst ? source.First : Maybe<T1>.None;
    }

    /// <summary>
    /// Creates a new <see cref="Erratic{T}"/> instance.
    /// </summary>
    /// <param name="source">Source either.</param>
    /// <typeparam name="T">First either type.</typeparam>
    /// <returns>
    /// New <see cref="Erratic{T}"/> instance equivalent to the first value of <paramref name="source"/>
    /// when <see cref="Either{T1,T2}.HasFirst"/> of <paramref name="source"/> is equal to <b>true</b>
    /// otherwise a new <see cref="Erratic{T}"/> instance equivalent to the second value of <paramref name="source"/>.
    /// </returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Erratic<T> ToErratic<T>(this Either<T, Exception> source)
    {
        return source.HasFirst ? new Erratic<T>( source.First ) : new Erratic<T>( source.Second );
    }

    /// <summary>
    /// Creates a new <see cref="Either{T1,T2}"/> instance.
    /// </summary>
    /// <param name="source">Source either.</param>
    /// <typeparam name="T1">First either type.</typeparam>
    /// <typeparam name="T2">Second either type.</typeparam>
    /// <returns>
    /// New <see cref="Either{T1,T2}"/> instance equivalent to the first value of <paramref name="source"/>
    /// when <see cref="Either{T1,T2}.HasFirst"/> of <paramref name="source"/> is equal to <b>true</b>
    /// otherwise a new <see cref="Either{T1,T2}"/> instance equivalent to the second value of <paramref name="source"/>.
    /// </returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Either<T1, T2> Reduce<T1, T2>(this Either<Either<T1, T2>, Either<T1, T2>> source)
    {
        return source.HasFirst ? source.First : source.Second;
    }

    /// <summary>
    /// Creates a new <see cref="Either{T1,T2}"/> instance.
    /// </summary>
    /// <param name="source">Source either.</param>
    /// <typeparam name="T1">First either type.</typeparam>
    /// <typeparam name="T2">Second either type.</typeparam>
    /// <returns>
    /// New <see cref="Either{T1,T2}"/> instance equivalent to the first value of <paramref name="source"/>
    /// when <see cref="Either{T1,T2}.HasFirst"/> of <paramref name="source"/> is equal to <b>true</b>
    /// otherwise a new <see cref="Either{T1,T2}"/> instance equivalent to the second value of <paramref name="source"/>.
    /// </returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Either<T1, T2> Reduce<T1, T2>(this Either<Either<T1, T2>, T1> source)
    {
        return source.HasFirst ? source.First : source.Second;
    }

    /// <summary>
    /// Creates a new <see cref="Either{T1,T2}"/> instance.
    /// </summary>
    /// <param name="source">Source either.</param>
    /// <typeparam name="T1">First either type.</typeparam>
    /// <typeparam name="T2">Second either type.</typeparam>
    /// <returns>
    /// New <see cref="Either{T1,T2}"/> instance equivalent to the first value of <paramref name="source"/>
    /// when <see cref="Either{T1,T2}.HasFirst"/> of <paramref name="source"/> is equal to <b>true</b>
    /// otherwise a new <see cref="Either{T1,T2}"/> instance equivalent to the second value of <paramref name="source"/>.
    /// </returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Either<T1, T2> Reduce<T1, T2>(this Either<Either<T1, T2>, T2> source)
    {
        return source.HasFirst ? source.First : source.Second;
    }

    /// <summary>
    /// Creates a new <see cref="Either{T1,T2}"/> instance.
    /// </summary>
    /// <param name="source">Source either.</param>
    /// <typeparam name="T1">First either type.</typeparam>
    /// <typeparam name="T2">Second either type.</typeparam>
    /// <returns>
    /// New <see cref="Either{T1,T2}"/> instance equivalent to the first value of <paramref name="source"/>
    /// when <see cref="Either{T1,T2}.HasFirst"/> of <paramref name="source"/> is equal to <b>true</b>
    /// otherwise a new <see cref="Either{T1,T2}"/> instance equivalent to the second value of <paramref name="source"/>.
    /// </returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Either<T1, T2> Reduce<T1, T2>(this Either<T1, Either<T1, T2>> source)
    {
        return source.HasFirst ? source.First : source.Second;
    }

    /// <summary>
    /// Creates a new <see cref="Either{T1,T2}"/> instance.
    /// </summary>
    /// <param name="source">Source either.</param>
    /// <typeparam name="T1">First either type.</typeparam>
    /// <typeparam name="T2">Second either type.</typeparam>
    /// <returns>
    /// New <see cref="Either{T1,T2}"/> instance equivalent to the first value of <paramref name="source"/>
    /// when <see cref="Either{T1,T2}.HasFirst"/> of <paramref name="source"/> is equal to <b>true</b>
    /// otherwise a new <see cref="Either{T1,T2}"/> instance equivalent to the second value of <paramref name="source"/>.
    /// </returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Either<T1, T2> Reduce<T1, T2>(this Either<T2, Either<T1, T2>> source)
    {
        return source.HasFirst ? source.First : source.Second;
    }
}
