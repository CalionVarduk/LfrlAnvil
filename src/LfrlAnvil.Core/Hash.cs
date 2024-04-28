using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Exceptions;

namespace LfrlAnvil;

/// <summary>
/// Represents a lightweight hash code computation result.
/// </summary>
public readonly struct Hash : IEquatable<Hash>, IComparable<Hash>, IComparable
{
    /// <summary>
    /// Represents a default hash code computation, equivalent to <b>0</b>.
    /// </summary>
    public static readonly Hash Default = new Hash( HashCode.Combine( 0 ) );

    /// <summary>
    /// Underlying hash code value.
    /// </summary>
    public readonly int Value;

    /// <summary>
    /// Creates a new <see cref="Hash"/> instance.
    /// </summary>
    /// <param name="value">Underlying hash code value.</param>
    public Hash(int value)
    {
        Value = value;
    }

    /// <summary>
    /// Returns a string representation of this <see cref="Hash"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return $"{nameof( Hash )}({Value})";
    }

    /// <inheritdoc />
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public override int GetHashCode()
    {
        return Value;
    }

    /// <inheritdoc />
    [Pure]
    public override bool Equals(object? obj)
    {
        return obj is Hash h && Equals( h );
    }

    /// <inheritdoc />
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public bool Equals(Hash other)
    {
        return Value.Equals( other.Value );
    }

    /// <inheritdoc />
    [Pure]
    public int CompareTo(object? obj)
    {
        return obj is Hash h ? CompareTo( h ) : throw new ArgumentException( ExceptionResources.InvalidType, nameof( obj ) );
    }

    /// <inheritdoc />
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public int CompareTo(Hash other)
    {
        return Value.CompareTo( other.Value );
    }

    /// <summary>
    /// Creates a new <see cref="Hash"/> instance by calculating a hash code of the provided <paramref name="obj"/>
    /// and including it in the hash code of this instance.
    /// </summary>
    /// <param name="obj">Object to add.</param>
    /// <typeparam name="T">Value type.</typeparam>
    /// <returns>New <see cref="Hash"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Hash Add<T>(T? obj)
    {
        return new Hash( HashCode.Combine( Value, obj ) );
    }

    /// <summary>
    /// Creates a new <see cref="Hash"/> instance by calculating hash codes of all objects provided in the <paramref name="range"/>
    /// and including them in the hash code of this instance.
    /// </summary>
    /// <param name="range">Range of objects to add.</param>
    /// <typeparam name="T">Element type.</typeparam>
    /// <returns>New <see cref="Hash"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Hash AddRange<T>(IEnumerable<T?> range)
    {
        var result = new Hash( Value );
        foreach ( var obj in range )
            result = result.Add( obj );

        return result;
    }

    /// <summary>
    /// Creates a new <see cref="Hash"/> instance by calculating hash codes of all objects provided in the <paramref name="range"/>
    /// and including them in the hash code of this instance.
    /// </summary>
    /// <param name="range">Range of objects to add.</param>
    /// <typeparam name="T">Element type.</typeparam>
    /// <returns>New <see cref="Hash"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Hash AddRange<T>(params T?[] range)
    {
        var result = new Hash( Value );
        foreach ( var obj in range )
            result = result.Add( obj );

        return result;
    }

    /// <summary>
    /// Returns the underlying <see cref="Value"/> from the provided <paramref name="h"/>.
    /// </summary>
    /// <param name="h">Object to convert.</param>
    /// <returns>Underlying <see cref="Value"/> from the provided <paramref name="h"/>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static implicit operator int(Hash h)
    {
        return h.Value;
    }

    /// <summary>
    /// Checks if <paramref name="a"/> is equal to <paramref name="b"/>.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns><b>true</b> when operands are equal, otherwise <b>false</b>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool operator ==(Hash a, Hash b)
    {
        return a.Value == b.Value;
    }

    /// <summary>
    /// Checks if <paramref name="a"/> is not equal to <paramref name="b"/>.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns><b>true</b> when operands are not equal, otherwise <b>false</b>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool operator !=(Hash a, Hash b)
    {
        return a.Value != b.Value;
    }

    /// <summary>
    /// Checks if <paramref name="a"/> is greater than <paramref name="b"/>.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns><b>true</b> when <paramref name="a"/> is greater than <paramref name="b"/>, otherwise <b>false</b>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool operator >(Hash a, Hash b)
    {
        return a.Value > b.Value;
    }

    /// <summary>
    /// Checks if <paramref name="a"/> is less than or equal to <paramref name="b"/>.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns><b>true</b> when <paramref name="a"/> is less than or equal to <paramref name="b"/>, otherwise <b>false</b>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool operator <=(Hash a, Hash b)
    {
        return a.Value <= b.Value;
    }

    /// <summary>
    /// Checks if <paramref name="a"/> is less than <paramref name="b"/>.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns><b>true</b> when <paramref name="a"/> is less than <paramref name="b"/>, otherwise <b>false</b>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool operator <(Hash a, Hash b)
    {
        return a.Value < b.Value;
    }

    /// <summary>
    /// Checks if <paramref name="a"/> is greater than or equal to <paramref name="b"/>.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns><b>true</b> when <paramref name="a"/> is greater than or equal to <paramref name="b"/>, otherwise <b>false</b>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool operator >=(Hash a, Hash b)
    {
        return a.Value >= b.Value;
    }
}
