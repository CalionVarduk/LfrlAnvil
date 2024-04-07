using System;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Numerics;

public readonly struct Percent : IEquatable<Percent>, IComparable<Percent>, IComparable, IFormattable
{
    public static readonly Percent Zero = new Percent( 0 );
    public static readonly Percent One = new Percent( 0.01m );
    public static readonly Percent OneHundred = new Percent( 1 );

    public Percent(decimal ratio)
    {
        Ratio = ratio;
    }

    public decimal Value => Ratio * 100m;
    public decimal Ratio { get; }

    [Pure]
    public static Percent Normalize(long value)
    {
        return Normalize( ( decimal )value );
    }

    [Pure]
    public static Percent Normalize(double value)
    {
        return Normalize( ( decimal )value );
    }

    [Pure]
    public static Percent Normalize(decimal value)
    {
        return Create( value * 0.01m );
    }

    [Pure]
    public static Percent Create(decimal ratio)
    {
        return new Percent( ratio );
    }

    [Pure]
    public override string ToString()
    {
        return ToString( NumberFormatInfo.CurrentInfo );
    }

    [Pure]
    public string ToString(IFormatProvider? formatProvider)
    {
        return ToString( "P2", formatProvider );
    }

    [Pure]
    public string ToString(string? format, IFormatProvider? formatProvider)
    {
        return Ratio.ToString( format, formatProvider );
    }

    [Pure]
    public override int GetHashCode()
    {
        return Ratio.GetHashCode();
    }

    [Pure]
    public override bool Equals(object? obj)
    {
        return obj is Percent p && Equals( p );
    }

    [Pure]
    public int CompareTo(object? obj)
    {
        return obj is Percent p ? CompareTo( p ) : 1;
    }

    [Pure]
    public bool Equals(Percent other)
    {
        return Ratio == other.Ratio;
    }

    [Pure]
    public int CompareTo(Percent other)
    {
        return Ratio.CompareTo( other.Ratio );
    }

    [Pure]
    public Percent Abs()
    {
        return new Percent( Math.Abs( Ratio ) );
    }

    [Pure]
    public Percent Truncate()
    {
        return Normalize( Math.Truncate( Value ) );
    }

    [Pure]
    public Percent Floor()
    {
        return Normalize( Math.Floor( Value ) );
    }

    [Pure]
    public Percent Ceiling()
    {
        return Normalize( Math.Ceiling( Value ) );
    }

    [Pure]
    public Percent Round(int decimals, MidpointRounding mode = MidpointRounding.ToEven)
    {
        return Normalize( Math.Round( Value, decimals, mode ) );
    }

    [Pure]
    public Percent Negate()
    {
        return new Percent( -Ratio );
    }

    [Pure]
    public Percent Increment()
    {
        return Add( One );
    }

    [Pure]
    public Percent Decrement()
    {
        return Subtract( One );
    }

    [Pure]
    public Percent Add(Percent other)
    {
        return new Percent( Ratio + other.Ratio );
    }

    [Pure]
    public Percent Subtract(Percent other)
    {
        return new Percent( Ratio - other.Ratio );
    }

    [Pure]
    public Percent Multiply(Percent other)
    {
        return new Percent( Ratio * other.Ratio );
    }

    [Pure]
    public Percent Divide(Percent other)
    {
        return new Percent( Ratio / other.Ratio );
    }

    [Pure]
    public Percent Modulo(Percent other)
    {
        return new Percent( Ratio % other.Ratio );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static implicit operator decimal(Percent percent)
    {
        return percent.Ratio;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static explicit operator double(Percent percent)
    {
        return ( double )percent.Ratio;
    }

    [Pure]
    public static Percent operator -(Percent percent)
    {
        return percent.Negate();
    }

    [Pure]
    public static Percent operator ++(Percent percent)
    {
        return percent.Increment();
    }

    [Pure]
    public static Percent operator --(Percent percent)
    {
        return percent.Decrement();
    }

    [Pure]
    public static Percent operator +(Percent left, Percent right)
    {
        return left.Add( right );
    }

    [Pure]
    public static Percent operator -(Percent left, Percent right)
    {
        return left.Subtract( right );
    }

    [Pure]
    public static Percent operator *(Percent left, Percent right)
    {
        return left.Multiply( right );
    }

    [Pure]
    public static Percent operator /(Percent left, Percent right)
    {
        return left.Divide( right );
    }

    [Pure]
    public static Percent operator %(Percent left, Percent right)
    {
        return left.Modulo( right );
    }

    [Pure]
    public static decimal operator *(Percent left, decimal right)
    {
        return left.Ratio * right;
    }

    [Pure]
    public static decimal operator *(decimal left, Percent right)
    {
        return left * right.Ratio;
    }

    [Pure]
    public static double operator *(Percent left, double right)
    {
        return ( double )left.Ratio * right;
    }

    [Pure]
    public static double operator *(double left, Percent right)
    {
        return left * ( double )right.Ratio;
    }

    [Pure]
    public static float operator *(Percent left, float right)
    {
        return ( float )left.Ratio * right;
    }

    [Pure]
    public static float operator *(float left, Percent right)
    {
        return left * ( float )right.Ratio;
    }

    [Pure]
    public static long operator *(Percent left, long right)
    {
        return ( long )Math.Round( left.Ratio * right, 0, MidpointRounding.AwayFromZero );
    }

    [Pure]
    public static long operator *(long left, Percent right)
    {
        return ( long )Math.Round( left * right.Ratio, 0, MidpointRounding.AwayFromZero );
    }

    [Pure]
    public static TimeSpan operator *(Percent left, TimeSpan right)
    {
        return new TimeSpan( left * right.Ticks );
    }

    [Pure]
    public static TimeSpan operator *(TimeSpan left, Percent right)
    {
        return new TimeSpan( left.Ticks * right );
    }

    [Pure]
    public static bool operator ==(Percent left, Percent right)
    {
        return left.Equals( right );
    }

    [Pure]
    public static bool operator !=(Percent left, Percent right)
    {
        return ! left.Equals( right );
    }

    [Pure]
    public static bool operator <(Percent left, Percent right)
    {
        return left.CompareTo( right ) < 0;
    }

    [Pure]
    public static bool operator <=(Percent left, Percent right)
    {
        return left.CompareTo( right ) <= 0;
    }

    [Pure]
    public static bool operator >(Percent left, Percent right)
    {
        return left.CompareTo( right ) > 0;
    }

    [Pure]
    public static bool operator >=(Percent left, Percent right)
    {
        return left.CompareTo( right ) >= 0;
    }
}
