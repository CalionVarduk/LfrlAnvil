using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Functional.Extensions;

/// <summary>
/// Contains <see cref="Maybe{T}"/> extension methods.
/// </summary>
public static class MaybeExtensions
{
    /// <summary>
    /// Creates a new <see cref="Either{T1,T2}"/> instance with <see cref="Nil"/> second type.
    /// </summary>
    /// <param name="source">Source maybe.</param>
    /// <typeparam name="T">Value type.</typeparam>
    /// <returns>
    /// New <see cref="Either{T1,T2}"/> instance equivalent to the provided <paramref name="source"/>
    /// or <see cref="Nil"/> when <paramref name="source"/> does not have a value.
    /// </returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Either<T, Nil> ToEither<T>(this Maybe<T> source)
        where T : notnull
    {
        return source.HasValue ? new Either<T, Nil>( source.Value ) : new Either<T, Nil>( Nil.Instance );
    }

    /// <summary>
    /// Creates a new <see cref="Maybe{T}"/> instance.
    /// </summary>
    /// <param name="source">Source maybe.</param>
    /// <typeparam name="T">Value type.</typeparam>
    /// <returns>Nested <see cref="Maybe{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Maybe<T> Reduce<T>(this Maybe<Maybe<T>> source)
        where T : notnull
    {
        return source.Value;
    }

    /// <summary>
    /// Matches two <see cref="Maybe{T}"/> instances together and invokes a correct delegate based on which values are set.
    /// </summary>
    /// <param name="source">First maybe.</param>
    /// <param name="other">Second maybe.</param>
    /// <param name="both">Delegate to invoke when both maybe's have a value.</param>
    /// <param name="first">Delegate to invoke when only the first maybe has a value.</param>
    /// <param name="second">Delegate to invoke when only the second maybe has a value.</param>
    /// <param name="none">Delegate to invoke when none of the maybe's has a value.</param>
    /// <typeparam name="T1">First maybe's value type.</typeparam>
    /// <typeparam name="T2">Second maybe's value type.</typeparam>
    /// <typeparam name="T3">Result type.</typeparam>
    /// <returns>Result returned by the invoked delegate.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static T3 MatchWith<T1, T2, T3>(
        this Maybe<T1> source,
        Maybe<T2> other,
        Func<T1, T2, T3> both,
        Func<T1, T3> first,
        Func<T2, T3> second,
        Func<T3> none)
        where T1 : notnull
        where T2 : notnull
    {
        if ( source.HasValue )
            return other.HasValue ? both( source.Value, other.Value ) : first( source.Value );

        return other.HasValue ? second( other.Value ) : none();
    }

    /// <summary>
    /// Matches two <see cref="Maybe{T}"/> instances together and invokes a correct delegate based on which values are set.
    /// </summary>
    /// <param name="source">First maybe.</param>
    /// <param name="other">Second maybe.</param>
    /// <param name="both">Delegate to invoke when both maybe's have a value.</param>
    /// <param name="first">Delegate to invoke when only the first maybe has a value.</param>
    /// <param name="second">Delegate to invoke when only the second maybe has a value.</param>
    /// <param name="none">Delegate to invoke when none of the maybe's has a value.</param>
    /// <typeparam name="T1">First maybe's value type.</typeparam>
    /// <typeparam name="T2">Second maybe's value type.</typeparam>
    /// <returns><see cref="Nil"/>.</returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Nil MatchWith<T1, T2>(
        this Maybe<T1> source,
        Maybe<T2> other,
        Action<T1, T2> both,
        Action<T1> first,
        Action<T2> second,
        Action none)
        where T1 : notnull
        where T2 : notnull
    {
        if ( source.HasValue )
        {
            if ( other.HasValue )
                both( source.Value, other.Value );
            else
                first( source.Value );
        }
        else if ( other.HasValue )
            second( other.Value );
        else
            none();

        return Nil.Instance;
    }
}
