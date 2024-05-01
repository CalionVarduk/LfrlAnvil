using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Functional.Extensions;

/// <summary>
/// Contains <see cref="TypeCast{TSource,TDestination}"/> extension methods.
/// </summary>
public static class TypeCastExtensions
{
    /// <summary>
    /// Creates a new <see cref="Maybe{T}"/> instance.
    /// </summary>
    /// <param name="source">Source type cast.</param>
    /// <typeparam name="TSource">Source object type.</typeparam>
    /// <typeparam name="TDestination">Destination object type.</typeparam>
    /// <returns>
    /// New <see cref="Maybe{T}"/> instance equivalent to the result of <paramref name="source"/> or <see cref="Maybe{T}.None"/>
    /// when <see cref="TypeCast{TSource,TDestination}.IsValid"/> of <paramref name="source"/> is equal to <b>false</b>.
    /// </returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Maybe<TDestination> ToMaybe<TSource, TDestination>(this TypeCast<TSource, TDestination> source)
        where TDestination : notnull
    {
        return source.IsValid ? new Maybe<TDestination>( source.Result ) : Maybe<TDestination>.None;
    }
}
