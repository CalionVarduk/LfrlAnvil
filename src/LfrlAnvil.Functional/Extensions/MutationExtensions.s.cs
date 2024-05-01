using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Functional.Extensions;

/// <summary>
/// Contains <see cref="Mutation{T}"/> extension methods.
/// </summary>
public static class MutationExtensions
{
    /// <summary>
    /// Creates a new <see cref="Mutation{T}"/> instance.
    /// </summary>
    /// <param name="source">Source mutation.</param>
    /// <typeparam name="T">Value type.</typeparam>
    /// <returns>
    /// New <see cref="Mutation{T}"/> instance with <see cref="Mutation{T}.OldValue"/> equal to
    /// nested <see cref="Mutation{T}.OldValue"/> and <see cref="Mutation{T}.Value"/> equal to nested <see cref="Mutation{T}.Value"/>.
    /// </returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Mutation<T> Reduce<T>(this Mutation<Mutation<T>> source)
    {
        return new Mutation<T>( source.OldValue.OldValue, source.Value.Value );
    }
}
