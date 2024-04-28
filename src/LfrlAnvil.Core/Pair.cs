using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Internal;

namespace LfrlAnvil;

/// <summary>
/// A lightweight generic representation of a pair of items.
/// </summary>
/// <typeparam name="T1">First item type.</typeparam>
/// <typeparam name="T2">Second item type.</typeparam>
public readonly struct Pair<T1, T2> : IEquatable<Pair<T1, T2>>
{
    /// <summary>
    /// First item.
    /// </summary>
    public readonly T1 First;

    /// <summary>
    /// Second item.
    /// </summary>
    public readonly T2 Second;

    /// <summary>
    /// Creates a new <see cref="Pair{T1,T2}"/> instance.
    /// </summary>
    /// <param name="first">First item.</param>
    /// <param name="second">Second item.</param>
    public Pair(T1 first, T2 second)
    {
        First = first;
        Second = second;
    }

    /// <summary>
    /// Returns a string representation of this <see cref="Pair{T1,T2}"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return $"{nameof( Pair )}({Generic<T1>.ToString( First )}, {Generic<T2>.ToString( Second )})";
    }

    /// <inheritdoc />
    [Pure]
    public override int GetHashCode()
    {
        return Hash.Default
            .Add( First )
            .Add( Second )
            .Value;
    }

    /// <inheritdoc />
    [Pure]
    public override bool Equals(object? obj)
    {
        return obj is Pair<T1, T2> p && Equals( p );
    }

    /// <inheritdoc />
    [Pure]
    public bool Equals(Pair<T1, T2> other)
    {
        return Equality.Create( First, other.First ).Result && Equality.Create( Second, other.Second ).Result;
    }

    /// <summary>
    /// Creates a new <see cref="Pair{T1,T2}"/> instance with changed <see cref="First"/> item.
    /// </summary>
    /// <param name="first">First item.</param>
    /// <typeparam name="T">First item type.</typeparam>
    /// <returns>New <see cref="Pair{T1,T2}"/> instance with unchanged <see cref="Second"/> item.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Pair<T, T2> SetFirst<T>(T first)
    {
        return new Pair<T, T2>( first, Second );
    }

    /// <summary>
    /// Creates a new <see cref="Pair{T1,T2}"/> instance with changed <see cref="Second"/> item.
    /// </summary>
    /// <param name="second">Second item.</param>
    /// <typeparam name="T">Second item type.</typeparam>
    /// <returns>New <see cref="Pair{T1,T2}"/> instance with unchanged <see cref="First"/> item.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Pair<T1, T> SetSecond<T>(T second)
    {
        return new Pair<T1, T>( First, second );
    }

    /// <summary>
    /// Checks if <paramref name="a"/> is equal to <paramref name="b"/>.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns><b>true</b> when operands are equal, otherwise <b>false</b>.</returns>
    [Pure]
    public static bool operator ==(Pair<T1, T2> a, Pair<T1, T2> b)
    {
        return a.Equals( b );
    }

    /// <summary>
    /// Checks if <paramref name="a"/> is not equal to <paramref name="b"/>.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns><b>true</b> when operands are not equal, otherwise <b>false</b>.</returns>
    [Pure]
    public static bool operator !=(Pair<T1, T2> a, Pair<T1, T2> b)
    {
        return ! a.Equals( b );
    }
}
