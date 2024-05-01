using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Functional.Extensions;

/// <summary>
/// Contains <see cref="Erratic{T}"/> extension methods.
/// </summary>
public static class ErraticExtensions
{
    /// <summary>
    /// Creates a new <see cref="Maybe{T}"/> instance.
    /// </summary>
    /// <param name="source">Source erratic.</param>
    /// <typeparam name="T">Value type.</typeparam>
    /// <returns>
    /// New <see cref="Maybe{T}"/> instance equivalent to the provided <paramref name="source"/>
    /// or <see cref="Maybe{T}.None"/> when <paramref name="source"/> does not have a value.
    /// </returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Maybe<T> ToMaybe<T>(this Erratic<T> source)
        where T : notnull
    {
        return source.IsOk ? source.Value : Maybe<T>.None;
    }

    /// <summary>
    /// Creates a new <see cref="Either{T1,T2}"/> instance.
    /// </summary>
    /// <param name="source">Source erratic.</param>
    /// <typeparam name="T">Value type.</typeparam>
    /// <returns>
    /// New <see cref="Either{T1,T2}"/> instance equivalent to the value of <paramref name="source"/>
    /// when <see cref="Erratic{T}.IsOk"/> of <paramref name="source"/> is equal to <b>true</b>
    /// otherwise a new <see cref="Either{T1,T2}"/> instance equivalent to the error of <paramref name="source"/>.
    /// </returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Either<T, Exception> ToEither<T>(this Erratic<T> source)
    {
        return source.IsOk ? new Either<T, Exception>( source.Value ) : new Either<T, Exception>( source.Error );
    }

    /// <summary>
    /// Creates a new <see cref="Erratic{T}"/> instance.
    /// </summary>
    /// <param name="source">Source erratic.</param>
    /// <typeparam name="T">Value type.</typeparam>
    /// <returns>
    /// New <see cref="Erratic{T}"/> instance equivalent to the value of <paramref name="source"/>
    /// when <see cref="Erratic{T}.IsOk"/> of <paramref name="source"/> is equal to <b>true</b>
    /// otherwise a new <see cref="Erratic{T}"/> instance equivalent to the error of <paramref name="source"/>.
    /// </returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Erratic<T> Reduce<T>(this Erratic<Erratic<T>> source)
    {
        if ( source.IsOk )
            return source.Value.IsOk ? new Erratic<T>( source.Value.Value ) : new Erratic<T>( source.Value.Error );

        return new Erratic<T>( source.Error );
    }
}
