using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using LfrlAnvil.Exceptions;
using LfrlAnvil.Extensions;

namespace LfrlAnvil;

/// <summary>
/// Represents a lightweight generic bitmask.
/// </summary>
/// <typeparam name="T">Value type.</typeparam>
/// <remarks>
/// Type initialization may throw exceptions of <see cref="BitmaskTypeInitializationException"/> type
/// when the value type is not convertible to and from <see cref="UInt64"/> or when value type is an <see cref="Enum"/>
/// without the <see cref="FlagsAttribute"/> or does not have a member with its underlying value equal to <b>0</b>.
/// </remarks>
public readonly struct Bitmask<T> : IEquatable<Bitmask<T>>, IComparable<Bitmask<T>>, IComparable, IReadOnlyCollection<T>
    where T : struct, IConvertible, IComparable
{
    /// <summary>
    /// Represents an empty bitmask with all bits set to <b>0</b>.
    /// </summary>
    public static readonly Bitmask<T> Empty = new Bitmask<T>();

    /// <summary>
    /// Represents a bitmask with all available bits set to <b>1</b>.
    /// </summary>
    public static readonly Bitmask<T> All;

    /// <summary>
    /// Specifies whether or not the underlying value type is an <see cref="Enum"/>.
    /// </summary>
    public static readonly bool IsEnum = typeof( T ).IsEnum;

    /// <summary>
    /// Specifies the base type of the underlying value type.
    /// </summary>
    public static readonly Type BaseType = IsEnum ? Enum.GetUnderlyingType( typeof( T ) ) : typeof( T );

    /// <summary>
    /// Specifies the size of the bitmask in bits.
    /// </summary>
    public static readonly int BitCount = Marshal.SizeOf( BaseType ) << 3;

    /// <summary>
    /// Delegate used for conversion of the underlying value type to <see cref="UInt64"/>.
    /// </summary>
    public static readonly Func<T, ulong> ToLongValue;

    /// <summary>
    /// Delegate used for conversion of <see cref="UInt64"/> to the underlying value type.
    /// </summary>
    public static readonly Func<ulong, T> FromLongValue;

    static Bitmask()
    {
        var toLongValueExpr = BuildToLongValueExpr();
        var fromLongValueExpr = BuildFromLongValueExpr();
        ToLongValue = toLongValueExpr.Compile();
        FromLongValue = fromLongValueExpr.Compile();

        TryAssertEnumType();

        All = new Bitmask<T>( FromLongValue( ~0UL ) );
        if ( ! IsEnum )
            return;

        var availableValues = All
            .Where( static v => Enum.IsDefined( typeof( T ), v ) )
            .Select( ToLongValue )
            .Aggregate( 0UL, static (p, c) => p | c );

        All = new Bitmask<T>( FromLongValue( availableValues ) );
    }

    /// <summary>
    /// Creates a new <see cref="Bitmask{T}"/> instance.
    /// </summary>
    /// <param name="value">Bitmask value.</param>
    public Bitmask(T value)
    {
        Value = value;
    }

    /// <summary>
    /// Underlying value.
    /// </summary>
    public T Value { get; }

    /// <summary>
    /// Specifies the number of set bits in this instance.
    /// </summary>
    public int Count
    {
        get
        {
            var result = 0;
            foreach ( var _ in this )
                ++result;

            return result;
        }
    }

    /// <summary>
    /// Returns a string representation of this <see cref="Bitmask{T}"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return $"{nameof( Bitmask )}({Value})";
    }

    /// <inheritdoc />
    [Pure]
    public override bool Equals(object? obj)
    {
        return obj is Bitmask<T> b && Equals( b );
    }

    /// <inheritdoc />
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public override int GetHashCode()
    {
        return EqualityComparer<T>.Default.GetHashCode( Value );
    }

    /// <inheritdoc />
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public bool Equals(Bitmask<T> other)
    {
        return EqualityComparer<T>.Default.Equals( Value, other.Value );
    }

    /// <inheritdoc />
    [Pure]
    public int CompareTo(object? obj)
    {
        return obj is Bitmask<T> b ? CompareTo( b ) : throw new ArgumentException( ExceptionResources.InvalidType, nameof( obj ) );
    }

    /// <inheritdoc />
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public int CompareTo(Bitmask<T> other)
    {
        return Comparer<T>.Default.Compare( Value, other.Value );
    }

    /// <summary>
    /// Checks whether or not the <see cref="Value"/> of this instance
    /// contains at least one set bit specified by the provided <paramref name="value"/>.
    /// </summary>
    /// <param name="value">Bits to verify.</param>
    /// <returns>
    /// <b>true</b> when this instance contains at least one set bit specified by the provided <paramref name="value"/>,
    /// otherwise <b>false</b>.
    /// </returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public bool ContainsAny(T value)
    {
        var longValue = ToLongValue( value );
        return (ToLongValue( Value ) & longValue) != 0 || longValue == 0;
    }

    /// <summary>
    /// Checks whether or not the <see cref="Value"/> of this instance
    /// contains at least one set bit specified by the provided <paramref name="other"/>.
    /// </summary>
    /// <param name="other">Bits to verify.</param>
    /// <returns>
    /// <b>true</b> when this instance contains at least one set bit specified by the provided <paramref name="other"/>,
    /// otherwise <b>false</b>.
    /// </returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public bool ContainsAny(Bitmask<T> other)
    {
        return ContainsAny( other.Value );
    }

    /// <summary>
    /// Checks whether or not the <see cref="Value"/> of this instance
    /// contains all set bits specified by the provided <paramref name="value"/>.
    /// </summary>
    /// <param name="value">Bits to verify.</param>
    /// <returns>
    /// <b>true</b> when this instance contains all set bits specified by the provided <paramref name="value"/>, otherwise <b>false</b>.
    /// </returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public bool ContainsAll(T value)
    {
        var longValue = ToLongValue( value );
        return (ToLongValue( Value ) & longValue) == longValue;
    }

    /// <summary>
    /// Checks whether or not the <see cref="Value"/> of this instance
    /// contains all set bits specified by the provided <paramref name="other"/>.
    /// </summary>
    /// <param name="other">Bits to verify.</param>
    /// <returns>
    /// <b>true</b> when this instance contains all set bits specified by the provided <paramref name="other"/>, otherwise <b>false</b>.
    /// </returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public bool ContainsAll(Bitmask<T> other)
    {
        return ContainsAll( other.Value );
    }

    /// <summary>
    /// Checks whether or not the <see cref="Value"/> of this instance
    /// contains set bit at the specified 0-based <paramref name="bitIndex"/> position.
    /// </summary>
    /// <param name="bitIndex">0-based bit position to verify.</param>
    /// <returns>
    /// <b>true</b> when this instance contains set bit at the specified 0-based <paramref name="bitIndex"/> position,
    /// otherwise <b>false</b>.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// When <paramref name="bitIndex"/> is less than <b>0</b> or greater than or equal to <see cref="BitCount"/>.
    /// </exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public bool ContainsBit(int bitIndex)
    {
        Ensure.IsInIndexRange( bitIndex, BitCount );
        return ContainsAll( FromLongValue( 1UL << bitIndex ) );
    }

    /// <summary>
    /// Creates a new <see cref="Bitmask{T}"/> instance by calculating bitwise or
    /// on this instance and the provided <paramref name="value"/>.
    /// </summary>
    /// <param name="value">Value to bitwise or.</param>
    /// <returns>New <see cref="Bitmask{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Bitmask<T> Set(T value)
    {
        var result = ToLongValue( Value ) | ToLongValue( value );
        return new Bitmask<T>( FromLongValue( result ) );
    }

    /// <summary>
    /// Creates a new <see cref="Bitmask{T}"/> instance by calculating bitwise or
    /// on this instance and the provided <paramref name="other"/>.
    /// </summary>
    /// <param name="other">Value to bitwise or.</param>
    /// <returns>New <see cref="Bitmask{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Bitmask<T> Set(Bitmask<T> other)
    {
        return Set( other.Value );
    }

    /// <summary>
    /// Creates a new <see cref="Bitmask{T}"/> instance by setting a bit at the specified 0-based <paramref name="bitIndex"/> position
    /// in the <see cref="Value"/> of this instance.
    /// </summary>
    /// <param name="bitIndex">0-based position of a bit to set.</param>
    /// <returns>New <see cref="Bitmask{T}"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// When <paramref name="bitIndex"/> is less than <b>0</b> or greater than or equal to <see cref="BitCount"/>.
    /// </exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Bitmask<T> SetBit(int bitIndex)
    {
        Ensure.IsInIndexRange( bitIndex, BitCount );
        return Set( FromLongValue( 1UL << bitIndex ) );
    }

    /// <summary>
    /// Creates a new <see cref="Bitmask{T}"/> instance by unsetting provided bits in the <see cref="Value"/> of this instance.
    /// </summary>
    /// <param name="value">Bits to unset.</param>
    /// <returns>New <see cref="Bitmask{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Bitmask<T> Unset(T value)
    {
        var result = ToLongValue( Value ) & ~ToLongValue( value );
        return new Bitmask<T>( FromLongValue( result ) );
    }

    /// <summary>
    /// Creates a new <see cref="Bitmask{T}"/> instance by unsetting provided bits in the <see cref="Value"/> of this instance.
    /// </summary>
    /// <param name="other">Bits to unset.</param>
    /// <returns>New <see cref="Bitmask{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Bitmask<T> Unset(Bitmask<T> other)
    {
        return Unset( other.Value );
    }

    /// <summary>
    /// Creates a new <see cref="Bitmask{T}"/> instance by unsetting a bit at the specified 0-based <paramref name="bitIndex"/> position
    /// in the <see cref="Value"/> of this instance.
    /// </summary>
    /// <param name="bitIndex">0-based position of a bit to unset.</param>
    /// <returns>New <see cref="Bitmask{T}"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// When <paramref name="bitIndex"/> is less than <b>0</b> or greater than or equal to <see cref="BitCount"/>.
    /// </exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Bitmask<T> UnsetBit(int bitIndex)
    {
        Ensure.IsInIndexRange( bitIndex, BitCount );
        return Unset( FromLongValue( 1UL << bitIndex ) );
    }

    /// <summary>
    /// Creates a new <see cref="Bitmask{T}"/> instance by calculating bitwise and
    /// on this instance and the provided <paramref name="value"/>.
    /// </summary>
    /// <param name="value">Value to bitwise and.</param>
    /// <returns>New <see cref="Bitmask{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Bitmask<T> Intersect(T value)
    {
        var result = ToLongValue( Value ) & ToLongValue( value );
        return new Bitmask<T>( FromLongValue( result ) );
    }

    /// <summary>
    /// Creates a new <see cref="Bitmask{T}"/> instance by calculating bitwise and
    /// on this instance and the provided <paramref name="other"/>.
    /// </summary>
    /// <param name="other">Value to bitwise and.</param>
    /// <returns>New <see cref="Bitmask{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Bitmask<T> Intersect(Bitmask<T> other)
    {
        return Intersect( other.Value );
    }

    /// <summary>
    /// Creates a new <see cref="Bitmask{T}"/> instance by calculating bitwise xor
    /// on this instance and the provided <paramref name="value"/>.
    /// </summary>
    /// <param name="value">Value to bitwise xor.</param>
    /// <returns>New <see cref="Bitmask{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Bitmask<T> Alternate(T value)
    {
        var result = ToLongValue( Value ) ^ ToLongValue( value );
        return new Bitmask<T>( FromLongValue( result ) );
    }

    /// <summary>
    /// Creates a new <see cref="Bitmask{T}"/> instance by calculating bitwise xor
    /// on this instance and the provided <paramref name="other"/>.
    /// </summary>
    /// <param name="other">Value to bitwise xor.</param>
    /// <returns>New <see cref="Bitmask{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Bitmask<T> Alternate(Bitmask<T> other)
    {
        return Alternate( other.Value );
    }

    /// <summary>
    /// Creates a new <see cref="Bitmask{T}"/> instance by calculating bitwise not on this instance.
    /// </summary>
    /// <returns>New <see cref="Bitmask{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Bitmask<T> Negate()
    {
        var result = ~ToLongValue( Value );
        return new Bitmask<T>( FromLongValue( result ) );
    }

    /// <summary>
    /// Creates a new <see cref="Bitmask{T}"/> instance by sanitizing this instance
    /// by calculating bitwise and with <see cref="All"/> available values.
    /// </summary>
    /// <returns>New <see cref="Bitmask{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Bitmask<T> Sanitize()
    {
        return Intersect( All.Value );
    }

    /// <summary>
    /// Creates a new <see cref="Bitmask{T}"/> instance with all bits unset.
    /// </summary>
    /// <returns>New <see cref="Bitmask{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Bitmask<T> Clear()
    {
        return new Bitmask<T>( FromLongValue( 0 ) );
    }

    /// <summary>
    /// Creates a new <see cref="Enumerator"/> instance for this bitmask.
    /// </summary>
    /// <returns>New <see cref="Enumerator"/> instance.</returns>
    [Pure]
    public Enumerator GetEnumerator()
    {
        return new Enumerator( Value );
    }

    /// <summary>
    /// Converts the provided <paramref name="b"/> to the underlying value type. Returns <see cref="Value"/>.
    /// </summary>
    /// <param name="b">Bitmask to convert.</param>
    /// <returns><see cref="Value"/> from the provided <paramref name="b"/>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static implicit operator T(Bitmask<T> b)
    {
        return b.Value;
    }

    /// <summary>
    /// Converts the provided <paramref name="v"/> to <see cref="Bitmask{T}"/> type.
    /// </summary>
    /// <param name="v">Value to convert.</param>
    /// <returns>New <see cref="Bitmask{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static implicit operator Bitmask<T>(T v)
    {
        return new Bitmask<T>( v );
    }

    /// <summary>
    /// Checks if <paramref name="a"/> is equal to <paramref name="b"/>.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns><b>true</b> when operands are equal, otherwise <b>false</b>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool operator ==(Bitmask<T> a, Bitmask<T> b)
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
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool operator !=(Bitmask<T> a, Bitmask<T> b)
    {
        return ! a.Equals( b );
    }

    /// <summary>
    /// Checks if <paramref name="a"/> is greater than <paramref name="b"/>.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns><b>true</b> when <paramref name="a"/> is greater than <paramref name="b"/>, otherwise <b>false</b>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool operator >(Bitmask<T> a, Bitmask<T> b)
    {
        return a.CompareTo( b ) > 0;
    }

    /// <summary>
    /// Checks if <paramref name="a"/> is less than or equal to <paramref name="b"/>.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns><b>true</b> when <paramref name="a"/> is less than or equal to <paramref name="b"/>, otherwise <b>false</b>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool operator <=(Bitmask<T> a, Bitmask<T> b)
    {
        return a.CompareTo( b ) <= 0;
    }

    /// <summary>
    /// Checks if <paramref name="a"/> is less than <paramref name="b"/>.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns><b>true</b> when <paramref name="a"/> is less than <paramref name="b"/>, otherwise <b>false</b>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool operator <(Bitmask<T> a, Bitmask<T> b)
    {
        return a.CompareTo( b ) < 0;
    }

    /// <summary>
    /// Checks if <paramref name="a"/> is greater than or equal to <paramref name="b"/>.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns><b>true</b> when <paramref name="a"/> is greater than or equal to <paramref name="b"/>, otherwise <b>false</b>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool operator >=(Bitmask<T> a, Bitmask<T> b)
    {
        return a.CompareTo( b ) >= 0;
    }

    /// <summary>
    /// Creates a new <see cref="Bitmask{T}"/> instance by performing bitwise or on <paramref name="a"/> and <paramref name="b"/>.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns>New <see cref="Bitmask{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Bitmask<T> operator |(Bitmask<T> a, Bitmask<T> b)
    {
        return a.Set( b );
    }

    /// <summary>
    /// Creates a new <see cref="Bitmask{T}"/> instance by performing bitwise or on <paramref name="a"/> and <paramref name="b"/>.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns>New <see cref="Bitmask{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Bitmask<T> operator |(Bitmask<T> a, T b)
    {
        return a.Set( b );
    }

    /// <summary>
    /// Creates a new <see cref="Bitmask{T}"/> instance by performing bitwise or on <paramref name="a"/> and <paramref name="b"/>.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns>New <see cref="Bitmask{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Bitmask<T> operator |(T a, Bitmask<T> b)
    {
        return new Bitmask<T>( a ).Set( b );
    }

    /// <summary>
    /// Creates a new <see cref="Bitmask{T}"/> instance by performing bitwise and on <paramref name="a"/> and <paramref name="b"/>.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns>New <see cref="Bitmask{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Bitmask<T> operator &(Bitmask<T> a, Bitmask<T> b)
    {
        return a.Intersect( b );
    }

    /// <summary>
    /// Creates a new <see cref="Bitmask{T}"/> instance by performing bitwise and on <paramref name="a"/> and <paramref name="b"/>.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns>New <see cref="Bitmask{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Bitmask<T> operator &(Bitmask<T> a, T b)
    {
        return a.Intersect( b );
    }

    /// <summary>
    /// Creates a new <see cref="Bitmask{T}"/> instance by performing bitwise and on <paramref name="a"/> and <paramref name="b"/>.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns>New <see cref="Bitmask{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Bitmask<T> operator &(T a, Bitmask<T> b)
    {
        return new Bitmask<T>( a ).Intersect( b );
    }

    /// <summary>
    /// Creates a new <see cref="Bitmask{T}"/> instance by performing bitwise xor on <paramref name="a"/> and <paramref name="b"/>.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns>New <see cref="Bitmask{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Bitmask<T> operator ^(Bitmask<T> a, Bitmask<T> b)
    {
        return a.Alternate( b );
    }

    /// <summary>
    /// Creates a new <see cref="Bitmask{T}"/> instance by performing bitwise xor on <paramref name="a"/> and <paramref name="b"/>.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns>New <see cref="Bitmask{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Bitmask<T> operator ^(Bitmask<T> a, T b)
    {
        return a.Alternate( b );
    }

    /// <summary>
    /// Creates a new <see cref="Bitmask{T}"/> instance by performing bitwise xor on <paramref name="a"/> and <paramref name="b"/>.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns>New <see cref="Bitmask{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Bitmask<T> operator ^(T a, Bitmask<T> b)
    {
        return new Bitmask<T>( a ).Alternate( b );
    }

    /// <summary>
    /// Creates a new <see cref="Bitmask{T}"/> instance by performing bitwise not on <paramref name="a"/>.
    /// </summary>
    /// <param name="a">Operand.</param>
    /// <returns>New <see cref="Bitmask{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Bitmask<T> operator ~(Bitmask<T> a)
    {
        return a.Negate();
    }

    /// <summary>
    /// Lightweight enumerator implementation for <see cref="Bitmask{T}"/>.
    /// </summary>
    public struct Enumerator : IEnumerator<T>
    {
        private readonly ulong _value;
        private int _index;

        internal Enumerator(T value)
        {
            _value = ToLongValue( value );
            _index = -1;
        }

        /// <inheritdoc />
        public T Current => FromLongValue( 1UL << _index );

        object IEnumerator.Current => Current;

        /// <inheritdoc />
        public void Dispose() { }

        /// <inheritdoc />
        public bool MoveNext()
        {
            ++_index;
            while ( true )
            {
                if ( _index >= BitCount )
                {
                    _index = BitCount;
                    return false;
                }

                if ( ((_value >> _index) & 1UL) == 1UL )
                    break;

                ++_index;
            }

            return true;
        }

        void IEnumerator.Reset()
        {
            _index = -1;
        }
    }

    private static void TryAssertEnumType()
    {
        if ( ! IsEnum )
            return;

        if ( ! typeof( T ).HasAttribute<FlagsAttribute>() )
            throw new BitmaskTypeInitializationException( typeof( T ), ExceptionResources.MissingEnumFlagsAttribute<T>() );

        if ( ! Enum.IsDefined( typeof( T ), FromLongValue( 0 ) ) )
            throw new BitmaskTypeInitializationException( typeof( T ), ExceptionResources.MissingEnumZeroValueMember<T>() );
    }

    private static Expression<Func<T, ulong>> BuildToLongValueExpr()
    {
        try
        {
            var parameterExpr = Expression.Parameter( typeof( T ), "value" );

            if ( IsEnum )
            {
                var underlyingTypeConvertExpr = Expression.Convert( parameterExpr, BaseType );
                var enumUlongConvertExpr = Expression.Convert( underlyingTypeConvertExpr, typeof( ulong ) );

                return Expression.Lambda<Func<T, ulong>>( enumUlongConvertExpr, parameterExpr );
            }

            var ulongConvertExpr = Expression.Convert( parameterExpr, typeof( ulong ) );
            return Expression.Lambda<Func<T, ulong>>( ulongConvertExpr, parameterExpr );
        }
        catch ( Exception exc )
        {
            throw new BitmaskTypeInitializationException(
                typeof( T ),
                ExceptionResources.FailedToCreateConverter<T>( nameof( ToLongValue ) ),
                exc );
        }
    }

    private static Expression<Func<ulong, T>> BuildFromLongValueExpr()
    {
        try
        {
            var parameterExpr = Expression.Parameter( typeof( ulong ), "value" );

            if ( IsEnum )
            {
                var underlyingTypeConvertExpr = Expression.Convert( parameterExpr, BaseType );
                var enumTypeConvertExpr = Expression.Convert( underlyingTypeConvertExpr, typeof( T ) );

                return Expression.Lambda<Func<ulong, T>>( enumTypeConvertExpr, parameterExpr );
            }

            var typeConvertExpr = Expression.Convert( parameterExpr, typeof( T ) );
            return Expression.Lambda<Func<ulong, T>>( typeConvertExpr, parameterExpr );
        }
        catch ( Exception exc )
        {
            throw new BitmaskTypeInitializationException(
                typeof( T ),
                ExceptionResources.FailedToCreateConverter<T>( nameof( FromLongValue ) ),
                exc );
        }
    }

    [Pure]
    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
        return GetEnumerator();
    }

    [Pure]
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
