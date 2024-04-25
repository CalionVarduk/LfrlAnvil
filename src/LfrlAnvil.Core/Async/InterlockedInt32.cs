using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Threading;

namespace LfrlAnvil.Async;

/// <summary>
/// A lightweight representation of an <see cref="Interlocked"/> (atomic) <see cref="Int32"/>.
/// </summary>
public struct InterlockedInt32 : IEquatable<InterlockedInt32>, IComparable<InterlockedInt32>, IComparable
{
    private int _value;

    /// <summary>
    /// Creates a new <see cref="InterlockedInt32"/> instance.
    /// </summary>
    /// <param name="value">Initial value.</param>
    public InterlockedInt32(int value)
    {
        _value = value;
    }

    /// <summary>
    /// Current value.
    /// </summary>
    public int Value => Interlocked.Add( ref _value, 0 );

    [Pure]
    public override string ToString()
    {
        return Value.ToString();
    }

    [Pure]
    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    [Pure]
    public override bool Equals(object? obj)
    {
        return obj is InterlockedInt32 b && Equals( b );
    }

    [Pure]
    public int CompareTo(object? obj)
    {
        return obj is InterlockedInt32 b ? CompareTo( b ) : 1;
    }

    [Pure]
    public bool Equals(InterlockedInt32 other)
    {
        return Value == other.Value;
    }

    [Pure]
    public int CompareTo(InterlockedInt32 other)
    {
        return Value.CompareTo( other.Value );
    }

    /// <summary>
    /// Sets <see cref="Value"/> to the provided <paramref name="value"/> and returns the old value.
    /// </summary>
    /// <param name="value">Value to set.</param>
    /// <returns><see cref="Value"/> before the change.</returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public int Exchange(int value)
    {
        return Interlocked.Exchange( ref _value, value );
    }

    /// <summary>
    /// Sets <see cref="Value"/> to the provided <paramref name="value"/>
    /// only if the current <see cref="Value"/> is equal to the provided <paramref name="comparand"/>
    /// and returns the old value.
    /// </summary>
    /// <param name="value">Value to set.</param>
    /// <param name="comparand">Value used for <see cref="Value"/> comparison.</param>
    /// <returns><see cref="Value"/> before the change.</returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public int CompareExchange(int value, int comparand)
    {
        return Interlocked.CompareExchange( ref _value, value, comparand );
    }

    /// <summary>
    /// Sets <see cref="Value"/> to the provided <paramref name="value"/>.
    /// </summary>
    /// <param name="value">Value to set.</param>
    /// <returns><b>true</b> when value has changed, otherwise <b>false</b>.</returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public bool Write(int value)
    {
        return Exchange( value ) != value;
    }

    /// <summary>
    /// Sets <see cref="Value"/> to the provided <paramref name="value"/>
    /// only if the current <see cref="Value"/> is equal to the provided <paramref name="expected"/>.
    /// </summary>
    /// <param name="value">Value to set.</param>
    /// <param name="expected">Value used for <see cref="Value"/> comparison.</param>
    /// <returns><b>true</b> when value has changed, otherwise <b>false</b>.</returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public bool Write(int value, int expected)
    {
        return CompareExchange( value, expected ) == expected;
    }

    /// <summary>
    /// Increments the current <see cref="Value"/> by <b>1</b>.
    /// </summary>
    /// <returns><see cref="Value"/> after the change.</returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public int Increment()
    {
        return Interlocked.Increment( ref _value );
    }

    /// <summary>
    /// Decrements the current <see cref="Value"/> by <b>1</b>.
    /// </summary>
    /// <returns><see cref="Value"/> after the change.</returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public int Decrement()
    {
        return Interlocked.Decrement( ref _value );
    }

    /// <summary>
    /// Adds provided <paramref name="value"/> to the current <see cref="Value"/>.
    /// </summary>
    /// <param name="value">Value to add.</param>
    /// <returns><see cref="Value"/> after the change.</returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public int Add(int value)
    {
        return Interlocked.Add( ref _value, value );
    }

    /// <summary>
    /// Subtracts provided <paramref name="value"/> from the current <see cref="Value"/>.
    /// </summary>
    /// <param name="value">Value to subtract.</param>
    /// <returns><see cref="Value"/> after the change.</returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public int Subtract(int value)
    {
        return Add( unchecked( -value ) );
    }

    /// <summary>
    /// Performs a bitwise and operation on the current <see cref="Value"/> and the provided <paramref name="value"/>
    /// and stores the result in <see cref="Value"/>.
    /// </summary>
    /// <param name="value">Value to bitwise and.</param>
    /// <returns><see cref="Value"/> before the change.</returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public int And(int value)
    {
        return Interlocked.And( ref _value, value );
    }

    /// <summary>
    /// Performs a bitwise or operation on the current <see cref="Value"/> and the provided <paramref name="value"/>
    /// and stores the result in <see cref="Value"/>.
    /// </summary>
    /// <param name="value">Value to bitwise or.</param>
    /// <returns><see cref="Value"/> before the change.</returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public int Or(int value)
    {
        return Interlocked.Or( ref _value, value );
    }

    /// <summary>
    /// Checks if <paramref name="a"/> is equal to <paramref name="b"/>.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns><b>true</b> when operands are equal, otherwise <b>false</b>.</returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool operator ==(InterlockedInt32 a, InterlockedInt32 b)
    {
        return a.Equals( b );
    }

    /// <summary>
    /// Checks if <paramref name="a"/> is not equal to <paramref name="b"/>.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns><b>true</b> when operands are not equal, otherwise <b>false</b>.</returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool operator !=(InterlockedInt32 a, InterlockedInt32 b)
    {
        return ! a.Equals( b );
    }

    /// <summary>
    /// Checks if <paramref name="a"/> is greater than or equal to <paramref name="b"/>.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns><b>true</b> when <paramref name="a"/> is greater than or equal to <paramref name="b"/>, otherwise <b>false</b>.</returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool operator >=(InterlockedInt32 a, InterlockedInt32 b)
    {
        return a.CompareTo( b ) >= 0;
    }

    /// <summary>
    /// Checks if <paramref name="a"/> is less than or equal to <paramref name="b"/>.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns><b>true</b> when <paramref name="a"/> is less than or equal to <paramref name="b"/>, otherwise <b>false</b>.</returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool operator <=(InterlockedInt32 a, InterlockedInt32 b)
    {
        return a.CompareTo( b ) <= 0;
    }

    /// <summary>
    /// Checks if <paramref name="a"/> is greater than <paramref name="b"/>.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns><b>true</b> when <paramref name="a"/> is greater than <paramref name="b"/>, otherwise <b>false</b>.</returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool operator >(InterlockedInt32 a, InterlockedInt32 b)
    {
        return a.CompareTo( b ) > 0;
    }

    /// <summary>
    /// Checks if <paramref name="a"/> is less than <paramref name="b"/>.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns><b>true</b> when <paramref name="a"/> is less than <paramref name="b"/>, otherwise <b>false</b>.</returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool operator <(InterlockedInt32 a, InterlockedInt32 b)
    {
        return a.CompareTo( b ) < 0;
    }
}
