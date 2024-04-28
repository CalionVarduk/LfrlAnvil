using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Internal;

namespace LfrlAnvil;

/// <summary>
/// A lightweight representation of an equality comparison of two generic values.
/// </summary>
/// <typeparam name="T">Value type.</typeparam>
public readonly struct Equality<T> : IEquatable<Equality<T>>
{
    /// <summary>
    /// First value to compare.
    /// </summary>
    public readonly T? First;

    /// <summary>
    /// Second value to compare.
    /// </summary>
    public readonly T? Second;

    /// <summary>
    /// <see cref="First"/> and <see cref="Second"/> equality comparison result.
    /// </summary>
    public readonly bool Result;

    /// <summary>
    /// Creates a new <see cref="Equality{T}"/> instance.
    /// </summary>
    /// <param name="first">First value to compare.</param>
    /// <param name="second">Second value to compare.</param>
    public Equality(T? first, T? second)
    {
        First = first;
        Second = second;
        Result = Generic<T>.AreEqual( First, Second );
    }

    /// <summary>
    /// Returns a string representation of this <see cref="Equality{T}"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return $"{nameof( Equality )}({Generic<T>.ToString( First )}, {Generic<T>.ToString( Second )})";
    }

    /// <inheritdoc />
    [Pure]
    public override int GetHashCode()
    {
        return Hash.Default
            .Add( First )
            .Add( Second )
            .Add( Result )
            .Value;
    }

    /// <inheritdoc />
    [Pure]
    public override bool Equals(object? obj)
    {
        return obj is Equality<T> e && Equals( e );
    }

    /// <inheritdoc />
    [Pure]
    public bool Equals(Equality<T> other)
    {
        return Generic<T>.AreEqual( First, other.First ) && Generic<T>.AreEqual( Second, other.Second ) && Result == other.Result;
    }

    /// <summary>
    /// Returns <see cref="Result"/> of <paramref name="e"/>.
    /// </summary>
    /// <param name="e">Operand.</param>
    /// <returns><see cref="Result"/> of <paramref name="e"/>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static implicit operator bool(Equality<T> e)
    {
        return e.Result;
    }

    /// <summary>
    /// Returns negated <see cref="Result"/> of <paramref name="e"/>.
    /// </summary>
    /// <param name="e">Operand.</param>
    /// <returns>Negated <see cref="Result"/> of <paramref name="e"/>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool operator !(Equality<T> e)
    {
        return ! e.Result;
    }

    /// <summary>
    /// Checks if <paramref name="a"/> is equal to <paramref name="b"/>.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns><b>true</b> when operands are equal, otherwise <b>false</b>.</returns>
    [Pure]
    public static bool operator ==(Equality<T> a, Equality<T> b)
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
    public static bool operator !=(Equality<T> a, Equality<T> b)
    {
        return ! a.Equals( b );
    }
}
