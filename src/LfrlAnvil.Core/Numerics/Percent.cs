using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Runtime.CompilerServices;
using LfrlAnvil.Exceptions;

namespace LfrlAnvil.Numerics;

/// <summary>
/// Represents a lightweight percentage.
/// </summary>
public readonly struct Percent : IEquatable<Percent>, IComparable<Percent>, IComparable, IFormattable
{
    /// <summary>
    /// Represents <b>0%</b>.
    /// </summary>
    public static readonly Percent Zero = new Percent( 0 );

    /// <summary>
    /// Represents <b>1%</b>.
    /// </summary>
    public static readonly Percent One = new Percent( 0.01m );

    /// <summary>
    /// Represents <b>100%</b>.
    /// </summary>
    public static readonly Percent OneHundred = new Percent( 1 );

    /// <summary>
    /// Creates a new <see cref="Percent"/> instance.
    /// </summary>
    /// <param name="ratio">Percentage ratio. <b>1</b> is equivalent to <b>100%</b>.</param>
    public Percent(decimal ratio)
    {
        Ratio = ratio;
    }

    /// <summary>
    ///Percentage value. <b>100</b> is equivalent to <b>100%</b>.
    /// </summary>
    public decimal Value => Ratio * 100m;

    /// <summary>
    /// Percentage ratio. <b>1</b> is equivalent to <b>100%</b>.
    /// </summary>
    public decimal Ratio { get; }

    /// <summary>
    /// Creates a new <see cref="Percent"/> instance from an <see cref="Int64"/> value.
    /// </summary>
    /// <param name="value">Percentage value. <b>100</b> is equivalent to <b>100%</b>.</param>
    [Pure]
    public static Percent Normalize(long value)
    {
        return Normalize( ( decimal )value );
    }

    /// <summary>
    /// Creates a new <see cref="Percent"/> instance from a <see cref="Double"/> value.
    /// </summary>
    /// <param name="value">Percentage value. <b>100</b> is equivalent to <b>100%</b>.</param>
    [Pure]
    public static Percent Normalize(double value)
    {
        return Normalize( ( decimal )value );
    }

    /// <summary>
    /// Creates a new <see cref="Percent"/> instance from a <see cref="Decimal"/> value.
    /// </summary>
    /// <param name="value">Percentage value. <b>100</b> is equivalent to <b>100%</b>.</param>
    [Pure]
    public static Percent Normalize(decimal value)
    {
        return Create( value * 0.01m );
    }

    /// <summary>
    /// Creates a new <see cref="Percent"/> instance.
    /// </summary>
    /// <param name="ratio">Percentage ratio. <b>1</b> is equivalent to <b>100%</b>.</param>
    [Pure]
    public static Percent Create(decimal ratio)
    {
        return new Percent( ratio );
    }

    /// <summary>
    /// Returns a string representation of this <see cref="Percent"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return ToString( NumberFormatInfo.CurrentInfo );
    }

    /// <summary>
    /// Returns a string representation of this <see cref="Percent"/> instance.
    /// </summary>
    /// <param name="formatProvider">An optional format provider.</param>
    /// <returns>String representation.</returns>
    [Pure]
    public string ToString(IFormatProvider? formatProvider)
    {
        return ToString( "P2", formatProvider );
    }

    /// <summary>
    /// Returns a string representation of this <see cref="Percent"/> instance.
    /// </summary>
    /// <param name="format">An optional numeric format.</param>
    /// <param name="formatProvider">An optional format provider.</param>
    /// <returns>String representation.</returns>
    [Pure]
    public string ToString([StringSyntax( "NumericFormat" )] string? format, IFormatProvider? formatProvider)
    {
        return Ratio.ToString( format, formatProvider );
    }

    /// <inheritdoc />
    [Pure]
    public override int GetHashCode()
    {
        return Ratio.GetHashCode();
    }

    /// <inheritdoc />
    [Pure]
    public override bool Equals(object? obj)
    {
        return obj is Percent p && Equals( p );
    }

    /// <inheritdoc />
    [Pure]
    public int CompareTo(object? obj)
    {
        return obj is Percent p ? CompareTo( p ) : throw new ArgumentException( ExceptionResources.InvalidType, nameof( obj ) );
    }

    /// <inheritdoc />
    [Pure]
    public bool Equals(Percent other)
    {
        return Ratio == other.Ratio;
    }

    /// <inheritdoc />
    [Pure]
    public int CompareTo(Percent other)
    {
        return Ratio.CompareTo( other.Ratio );
    }

    /// <summary>
    /// Creates a new <see cref="Percent"/> instance by calculating an absolute value from this instance.
    /// </summary>
    /// <returns>New <see cref="Percent"/> instance.</returns>
    [Pure]
    public Percent Abs()
    {
        return new Percent( Math.Abs( Ratio ) );
    }

    /// <summary>
    /// Creates a new <see cref="Percent"/> instance by truncating the <see cref="Value"/> of this instance.
    /// </summary>
    /// <returns>New <see cref="Percent"/> instance.</returns>
    /// <remarks>See <see cref="Math.Truncate(Decimal)"/> for more information.</remarks>
    [Pure]
    public Percent Truncate()
    {
        return Normalize( Math.Truncate( Value ) );
    }

    /// <summary>
    /// Creates a new <see cref="Percent"/> instance by calculating the floor of the <see cref="Value"/> from this instance.
    /// </summary>
    /// <returns>New <see cref="Percent"/> instance.</returns>
    /// <remarks>See <see cref="Math.Floor(Decimal)"/> for more information.</remarks>
    [Pure]
    public Percent Floor()
    {
        return Normalize( Math.Floor( Value ) );
    }

    /// <summary>
    /// Creates a new <see cref="Percent"/> instance by calculating the ceiling of the <see cref="Value"/> from this instance.
    /// </summary>
    /// <returns>New <see cref="Percent"/> instance.</returns>
    /// <remarks>See <see cref="Math.Ceiling(Decimal)"/> for more information.</remarks>
    [Pure]
    public Percent Ceiling()
    {
        return Normalize( Math.Ceiling( Value ) );
    }

    /// <summary>
    /// Creates a new <see cref="Percent"/> instance by rounding the <see cref="Value"/> from this instance.
    /// </summary>
    /// <param name="decimals">Number of decimal places to round to.</param>
    /// <param name="mode">Optional rounding strategy. Equal to <see cref="MidpointRounding.AwayFromZero"/> by default.</param>
    /// <returns>New <see cref="Percent"/> instance.</returns>
    /// <remarks>See <see cref="Math.Round(Decimal,MidpointRounding)"/> for more information.</remarks>
    [Pure]
    public Percent Round(int decimals, MidpointRounding mode = MidpointRounding.AwayFromZero)
    {
        return Normalize( Math.Round( Value, decimals, mode ) );
    }

    /// <summary>
    /// Creates a new <see cref="Percent"/> instance by negating this instance.
    /// </summary>
    /// <returns>New <see cref="Percent"/> instance.</returns>
    [Pure]
    public Percent Negate()
    {
        return new Percent( -Ratio );
    }

    /// <summary>
    /// Creates a new <see cref="Percent"/> instance by adding <b>1%</b> to this instance.
    /// </summary>
    /// <returns>New <see cref="Percent"/> instance.</returns>
    [Pure]
    public Percent Increment()
    {
        return Add( One );
    }

    /// <summary>
    /// Creates a new <see cref="Percent"/> instance by subtracting <b>1%</b> from this instance.
    /// </summary>
    /// <returns>New <see cref="Percent"/> instance.</returns>
    [Pure]
    public Percent Decrement()
    {
        return Subtract( One );
    }

    /// <summary>
    /// Creates a new <see cref="Percent"/> instance by adding <paramref name="other"/> to this instance.
    /// </summary>
    /// <param name="other">Other instance to add.</param>
    /// <returns>New <see cref="Percent"/> instance.</returns>
    [Pure]
    public Percent Add(Percent other)
    {
        return new Percent( Ratio + other.Ratio );
    }

    /// <summary>
    /// Creates a new <see cref="Percent"/> instance by subtracting <paramref name="other"/> from this instance.
    /// </summary>
    /// <param name="other">Other instance to subtract.</param>
    /// <returns>New <see cref="Percent"/> instance.</returns>
    [Pure]
    public Percent Subtract(Percent other)
    {
        return new Percent( Ratio - other.Ratio );
    }

    /// <summary>
    /// Creates a new <see cref="Percent"/> instance by multiplying <paramref name="other"/> and this instance together.
    /// </summary>
    /// <param name="other">Other instance to multiply by.</param>
    /// <returns>New <see cref="Percent"/> instance.</returns>
    [Pure]
    public Percent Multiply(Percent other)
    {
        return new Percent( Ratio * other.Ratio );
    }

    /// <summary>
    /// Creates a new <see cref="Percent"/> instance by dividing this instance by <paramref name="other"/>.
    /// </summary>
    /// <param name="other">Other instance to divide by.</param>
    /// <returns>New <see cref="Percent"/> instance.</returns>
    /// <exception cref="DivideByZeroException">When <paramref name="other"/> is equal to <b>0%</b>.</exception>
    [Pure]
    public Percent Divide(Percent other)
    {
        return new Percent( Ratio / other.Ratio );
    }

    /// <summary>
    /// Creates a new <see cref="Percent"/> instance by calculating the remainder of division of this instance by <paramref name="other"/>.
    /// </summary>
    /// <param name="other">Other instance to divide by.</param>
    /// <returns>New <see cref="Percent"/> instance.</returns>
    /// <exception cref="DivideByZeroException">When <paramref name="other"/> is equal to <b>0%</b>.</exception>
    [Pure]
    public Percent Modulo(Percent other)
    {
        return new Percent( Ratio % other.Ratio );
    }

    /// <summary>
    /// Converts the provided <paramref name="percent"/> to <see cref="Decimal"/>. Returns <see cref="Ratio"/>.
    /// </summary>
    /// <param name="percent">Percent to convert.</param>
    /// <returns><see cref="Ratio"/> from the provided <paramref name="percent"/>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static implicit operator decimal(Percent percent)
    {
        return percent.Ratio;
    }

    /// <summary>
    /// Converts the provided <paramref name="percent"/> to <see cref="Double"/>. Returns <see cref="Ratio"/>.
    /// </summary>
    /// <param name="percent">Percent to convert.</param>
    /// <returns><see cref="Ratio"/> from the provided <paramref name="percent"/>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static explicit operator double(Percent percent)
    {
        return ( double )percent.Ratio;
    }

    /// <summary>
    /// Creates a new <see cref="Percent"/> instance by negating the provided <paramref name="percent"/>.
    /// </summary>
    /// <param name="percent">Operand.</param>
    /// <returns>New <see cref="Percent"/> instance.</returns>
    [Pure]
    public static Percent operator -(Percent percent)
    {
        return percent.Negate();
    }

    /// <summary>
    /// Creates a new <see cref="Percent"/> instance by incrementing the provided <paramref name="percent"/>.
    /// </summary>
    /// <param name="percent">Operand.</param>
    /// <returns>New <see cref="Percent"/> instance.</returns>
    [Pure]
    public static Percent operator ++(Percent percent)
    {
        return percent.Increment();
    }

    /// <summary>
    /// Creates a new <see cref="Percent"/> instance by decrementing the provided <paramref name="percent"/>.
    /// </summary>
    /// <param name="percent">Operand.</param>
    /// <returns>New <see cref="Percent"/> instance.</returns>
    [Pure]
    public static Percent operator --(Percent percent)
    {
        return percent.Decrement();
    }

    /// <summary>
    /// Creates a new <see cref="Percent"/> instance by adding <paramref name="left"/> and <paramref name="right"/> together.
    /// </summary>
    /// <param name="left">First operand.</param>
    /// <param name="right">Second operand.</param>
    /// <returns>New <see cref="Percent"/> instance.</returns>
    [Pure]
    public static Percent operator +(Percent left, Percent right)
    {
        return left.Add( right );
    }

    /// <summary>
    /// Creates a new <see cref="Percent"/> instance by subtracting <paramref name="right"/> from <paramref name="left"/>.
    /// </summary>
    /// <param name="left">First operand.</param>
    /// <param name="right">Second operand.</param>
    /// <returns>New <see cref="Percent"/> instance.</returns>
    [Pure]
    public static Percent operator -(Percent left, Percent right)
    {
        return left.Subtract( right );
    }

    /// <summary>
    /// Creates a new <see cref="Percent"/> instance by multiplying <paramref name="left"/> and <paramref name="right"/> together.
    /// </summary>
    /// <param name="left">First operand.</param>
    /// <param name="right">Second operand.</param>
    /// <returns>New <see cref="Percent"/> instance.</returns>
    [Pure]
    public static Percent operator *(Percent left, Percent right)
    {
        return left.Multiply( right );
    }

    /// <summary>
    /// Creates a new <see cref="Percent"/> instance by dividing <paramref name="left"/> by <paramref name="right"/>.
    /// </summary>
    /// <param name="left">First operand.</param>
    /// <param name="right">Second operand.</param>
    /// <returns>New <see cref="Percent"/> instance.</returns>
    /// <exception cref="DivideByZeroException">When <paramref name="right"/> is equal to <b>0%</b>.</exception>
    [Pure]
    public static Percent operator /(Percent left, Percent right)
    {
        return left.Divide( right );
    }

    /// <summary>
    /// Creates a new <see cref="Percent"/> instance by calculating the remainder of division of
    /// <paramref name="left"/> by <paramref name="right"/>.
    /// </summary>
    /// <param name="left">First operand.</param>
    /// <param name="right">Second operand.</param>
    /// <returns>New <see cref="Percent"/> instance.</returns>
    /// <exception cref="DivideByZeroException">When <paramref name="right"/> is equal to <b>0%</b>.</exception>
    [Pure]
    public static Percent operator %(Percent left, Percent right)
    {
        return left.Modulo( right );
    }

    /// <summary>
    /// Creates a new <see cref="Decimal"/> instance by multiplying <see cref="Ratio"/> of
    /// <paramref name="left"/> and <paramref name="right"/> together.
    /// </summary>
    /// <param name="left">First operand.</param>
    /// <param name="right">Second operand.</param>
    /// <returns>New <see cref="Decimal"/> instance.</returns>
    [Pure]
    public static decimal operator *(Percent left, decimal right)
    {
        return left.Ratio * right;
    }

    /// <summary>
    /// Creates a new <see cref="Decimal"/> instance by multiplying <paramref name="left"/> and
    /// <see cref="Ratio"/> of <paramref name="right"/> together.
    /// </summary>
    /// <param name="left">First operand.</param>
    /// <param name="right">Second operand.</param>
    /// <returns>New <see cref="Decimal"/> instance.</returns>
    [Pure]
    public static decimal operator *(decimal left, Percent right)
    {
        return left * right.Ratio;
    }

    /// <summary>
    /// Creates a new <see cref="Double"/> instance by multiplying <see cref="Ratio"/> of
    /// <paramref name="left"/> and <paramref name="right"/> together.
    /// </summary>
    /// <param name="left">First operand.</param>
    /// <param name="right">Second operand.</param>
    /// <returns>New <see cref="Double"/> instance.</returns>
    [Pure]
    public static double operator *(Percent left, double right)
    {
        return ( double )left.Ratio * right;
    }

    /// <summary>
    /// Creates a new <see cref="Double"/> instance by multiplying <paramref name="left"/> and
    /// <see cref="Ratio"/> of <paramref name="right"/> together.
    /// </summary>
    /// <param name="left">First operand.</param>
    /// <param name="right">Second operand.</param>
    /// <returns>New <see cref="Double"/> instance.</returns>
    [Pure]
    public static double operator *(double left, Percent right)
    {
        return left * ( double )right.Ratio;
    }

    /// <summary>
    /// Creates a new <see cref="Single"/> instance by multiplying <see cref="Ratio"/> of
    /// <paramref name="left"/> and <paramref name="right"/> together.
    /// </summary>
    /// <param name="left">First operand.</param>
    /// <param name="right">Second operand.</param>
    /// <returns>New <see cref="Single"/> instance.</returns>
    [Pure]
    public static float operator *(Percent left, float right)
    {
        return ( float )left.Ratio * right;
    }

    /// <summary>
    /// Creates a new <see cref="Single"/> instance by multiplying <paramref name="left"/> and
    /// <see cref="Ratio"/> of <paramref name="right"/> together.
    /// </summary>
    /// <param name="left">First operand.</param>
    /// <param name="right">Second operand.</param>
    /// <returns>New <see cref="Single"/> instance.</returns>
    [Pure]
    public static float operator *(float left, Percent right)
    {
        return left * ( float )right.Ratio;
    }

    /// <summary>
    /// Creates a new <see cref="Int64"/> instance by multiplying <see cref="Ratio"/> of
    /// <paramref name="left"/> and <paramref name="right"/> together and rounding the result
    /// using the <see cref="MidpointRounding.AwayFromZero"/> strategy.
    /// </summary>
    /// <param name="left">First operand.</param>
    /// <param name="right">Second operand.</param>
    /// <returns>New <see cref="Int64"/> instance.</returns>
    [Pure]
    public static long operator *(Percent left, long right)
    {
        return ( long )Math.Round( left.Ratio * right, 0, MidpointRounding.AwayFromZero );
    }

    /// <summary>
    /// Creates a new <see cref="Int64"/> instance by multiplying <paramref name="left"/> and
    /// <see cref="Ratio"/> of <paramref name="right"/> together and rounding the result
    /// using the <see cref="MidpointRounding.AwayFromZero"/> strategy.
    /// </summary>
    /// <param name="left">First operand.</param>
    /// <param name="right">Second operand.</param>
    /// <returns>New <see cref="Int64"/> instance.</returns>
    [Pure]
    public static long operator *(long left, Percent right)
    {
        return ( long )Math.Round( left * right.Ratio, 0, MidpointRounding.AwayFromZero );
    }

    /// <summary>
    /// Creates a new <see cref="TimeSpan"/> instance by multiplying <see cref="Ratio"/> of
    /// <paramref name="left"/> and <paramref name="right"/> together and rounding the resulting <see cref="TimeSpan.Ticks"/>
    /// using the <see cref="MidpointRounding.AwayFromZero"/> strategy.
    /// </summary>
    /// <param name="left">First operand.</param>
    /// <param name="right">Second operand.</param>
    /// <returns>New <see cref="TimeSpan"/> instance.</returns>
    [Pure]
    public static TimeSpan operator *(Percent left, TimeSpan right)
    {
        return new TimeSpan( left * right.Ticks );
    }

    /// <summary>
    /// Creates a new <see cref="TimeSpan"/> instance by multiplying <paramref name="left"/> and
    /// <see cref="Ratio"/> of <paramref name="right"/> together and rounding the resulting <see cref="TimeSpan.Ticks"/>
    /// using the <see cref="MidpointRounding.AwayFromZero"/> strategy.
    /// </summary>
    /// <param name="left">First operand.</param>
    /// <param name="right">Second operand.</param>
    /// <returns>New <see cref="TimeSpan"/> instance.</returns>
    [Pure]
    public static TimeSpan operator *(TimeSpan left, Percent right)
    {
        return new TimeSpan( left.Ticks * right );
    }

    /// <summary>
    /// Checks if <paramref name="left"/> is equal to <paramref name="right"/>.
    /// </summary>
    /// <param name="left">First operand.</param>
    /// <param name="right">Second operand.</param>
    /// <returns><b>true</b> when operands are equal, otherwise <b>false</b>.</returns>
    [Pure]
    public static bool operator ==(Percent left, Percent right)
    {
        return left.Equals( right );
    }

    /// <summary>
    /// Checks if <paramref name="left"/> is not equal to <paramref name="right"/>.
    /// </summary>
    /// <param name="left">First operand.</param>
    /// <param name="right">Second operand.</param>
    /// <returns><b>true</b> when operands are not equal, otherwise <b>false</b>.</returns>
    [Pure]
    public static bool operator !=(Percent left, Percent right)
    {
        return ! left.Equals( right );
    }

    /// <summary>
    /// Checks if <paramref name="left"/> is less than <paramref name="right"/>.
    /// </summary>
    /// <param name="left">First operand.</param>
    /// <param name="right">Second operand.</param>
    /// <returns><b>true</b> when <paramref name="left"/> is less than <paramref name="right"/>, otherwise <b>false</b>.</returns>
    [Pure]
    public static bool operator <(Percent left, Percent right)
    {
        return left.CompareTo( right ) < 0;
    }

    /// <summary>
    /// Checks if <paramref name="left"/> is less than or equal to <paramref name="right"/>.
    /// </summary>
    /// <param name="left">First operand.</param>
    /// <param name="right">Second operand.</param>
    /// <returns>
    /// <b>true</b> when <paramref name="left"/> is less than or equal to <paramref name="right"/>, otherwise <b>false</b>.
    /// </returns>
    [Pure]
    public static bool operator <=(Percent left, Percent right)
    {
        return left.CompareTo( right ) <= 0;
    }

    /// <summary>
    /// Checks if <paramref name="left"/> is greater than <paramref name="right"/>.
    /// </summary>
    /// <param name="left">First operand.</param>
    /// <param name="right">Second operand.</param>
    /// <returns><b>true</b> when <paramref name="left"/> is greater than <paramref name="right"/>, otherwise <b>false</b>.</returns>
    [Pure]
    public static bool operator >(Percent left, Percent right)
    {
        return left.CompareTo( right ) > 0;
    }

    /// <summary>
    /// Checks if <paramref name="left"/> is greater than or equal to <paramref name="right"/>.
    /// </summary>
    /// <param name="left">First operand.</param>
    /// <param name="right">Second operand.</param>
    /// <returns>
    /// <b>true</b> when <paramref name="left"/> is greater than or equal to <paramref name="right"/>, otherwise <b>false</b>.
    /// </returns>
    [Pure]
    public static bool operator >=(Percent left, Percent right)
    {
        return left.CompareTo( right ) >= 0;
    }
}
