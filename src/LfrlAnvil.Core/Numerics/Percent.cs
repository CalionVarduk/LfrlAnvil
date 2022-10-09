using System;
using System.Diagnostics.Contracts;
using System.Globalization;

namespace LfrlAnvil.Numerics;

public readonly struct Percent : IEquatable<Percent>, IComparable<Percent>, IComparable, IFormattable
{
    public static readonly Percent Zero = new Percent( 0 );
    public static readonly Percent OneHundred = new Percent( 1 );

    private Percent(decimal normalizedValue)
    {
        NormalizedValue = normalizedValue;
    }

    public decimal Value => NormalizedValue * 100m;
    public decimal NormalizedValue { get; }

    [Pure]
    public static Percent Create(long value)
    {
        return Create( (decimal)value );
    }

    [Pure]
    public static Percent Create(double value)
    {
        return Create( (decimal)value );
    }

    [Pure]
    public static Percent Create(decimal value)
    {
        return CreateNormalized( value * 0.01m );
    }

    [Pure]
    public static Percent CreateNormalized(decimal normalizedValue)
    {
        return new Percent( normalizedValue );
    }

    [Pure]
    public override string ToString()
    {
        return ToString( "N2", NumberFormatInfo.CurrentInfo );
    }

    [Pure]
    public string ToString(string? format, IFormatProvider? formatProvider)
    {
        return $"{Value.ToString( format, formatProvider )}%";
    }

    [Pure]
    public override int GetHashCode()
    {
        return NormalizedValue.GetHashCode();
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
        return NormalizedValue == other.NormalizedValue;
    }

    [Pure]
    public int CompareTo(Percent other)
    {
        return NormalizedValue.CompareTo( other.NormalizedValue );
    }

    [Pure]
    public Percent Abs()
    {
        return new Percent( Math.Abs( NormalizedValue ) );
    }

    [Pure]
    public Percent Truncate()
    {
        return Create( Math.Truncate( Value ) );
    }

    [Pure]
    public Percent Round(int decimals, MidpointRounding mode = MidpointRounding.ToEven)
    {
        return Create( Math.Round( Value, decimals, mode ) );
    }

    [Pure]
    public Percent Negate()
    {
        return new Percent( -NormalizedValue );
    }

    [Pure]
    public Percent Offset(Percent other)
    {
        return new Percent( NormalizedValue + other.NormalizedValue );
    }

    [Pure]
    public Percent Add(Percent other)
    {
        return new Percent( NormalizedValue * (other.NormalizedValue + 1) );
    }

    [Pure]
    public Percent Subtract(Percent other)
    {
        return new Percent( NormalizedValue - NormalizedValue * other.NormalizedValue );
    }

    [Pure]
    public Percent Multiply(Percent other)
    {
        return new Percent( NormalizedValue * other.NormalizedValue );
    }

    [Pure]
    public Percent Divide(Percent other)
    {
        return new Percent( NormalizedValue / other.NormalizedValue );
    }

    [Pure]
    public Percent Modulo(Percent other)
    {
        return new Percent( NormalizedValue % other.NormalizedValue );
    }

    [Pure]
    public static Percent operator -(Percent percent)
    {
        return percent.Negate();
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
    public static decimal operator +(decimal left, Percent right)
    {
        return left * (right.NormalizedValue + 1);
    }

    [Pure]
    public static decimal operator -(decimal left, Percent right)
    {
        return left - left * right.NormalizedValue;
    }

    [Pure]
    public static decimal operator *(decimal left, Percent right)
    {
        return left * right.NormalizedValue;
    }

    [Pure]
    public static decimal operator /(decimal left, Percent right)
    {
        return left / right.NormalizedValue;
    }

    [Pure]
    public static double operator +(double left, Percent right)
    {
        return left * (double)(right.NormalizedValue + 1);
    }

    [Pure]
    public static double operator -(double left, Percent right)
    {
        return left - left * (double)right.NormalizedValue;
    }

    [Pure]
    public static double operator *(double left, Percent right)
    {
        return left * (double)right.NormalizedValue;
    }

    [Pure]
    public static double operator /(double left, Percent right)
    {
        return left / (double)right.NormalizedValue;
    }

    [Pure]
    public static double operator +(float left, Percent right)
    {
        return left * (float)(right.NormalizedValue + 1);
    }

    [Pure]
    public static double operator -(float left, Percent right)
    {
        return left - left * (float)right.NormalizedValue;
    }

    [Pure]
    public static double operator *(float left, Percent right)
    {
        return left * (float)right.NormalizedValue;
    }

    [Pure]
    public static double operator /(float left, Percent right)
    {
        return left / (float)right.NormalizedValue;
    }

    [Pure]
    public static long operator +(long left, Percent right)
    {
        return (long)Math.Round( (decimal)left + right, 0, MidpointRounding.AwayFromZero );
    }

    [Pure]
    public static long operator -(long left, Percent right)
    {
        return (long)Math.Round( (decimal)left - right, 0, MidpointRounding.AwayFromZero );
    }

    [Pure]
    public static long operator *(long left, Percent right)
    {
        return (long)Math.Round( (decimal)left * right, 0, MidpointRounding.AwayFromZero );
    }

    [Pure]
    public static long operator /(long left, Percent right)
    {
        return (long)Math.Round( (decimal)left / right, 0, MidpointRounding.AwayFromZero );
    }

    [Pure]
    public static TimeSpan operator +(TimeSpan left, Percent right)
    {
        return new TimeSpan( left.Ticks + right );
    }

    [Pure]
    public static TimeSpan operator -(TimeSpan left, Percent right)
    {
        return new TimeSpan( left.Ticks - right );
    }

    [Pure]
    public static TimeSpan operator *(TimeSpan left, Percent right)
    {
        return new TimeSpan( left.Ticks * right );
    }

    [Pure]
    public static TimeSpan operator /(TimeSpan left, Percent right)
    {
        return new TimeSpan( left.Ticks / right );
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
