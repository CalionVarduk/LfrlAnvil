using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Exceptions;

namespace LfrlAnvil.Numerics;

public readonly struct Fraction : IEquatable<Fraction>, IComparable<Fraction>, IComparable
{
    public static readonly Fraction Zero = new Fraction( 0 );
    public static readonly Fraction One = new Fraction( 1 );
    public static readonly Fraction MaxValue = new Fraction( long.MaxValue );
    public static readonly Fraction MinValue = new Fraction( long.MinValue );
    public static readonly Fraction Epsilon = new Fraction( 1, ulong.MaxValue );

    private readonly ulong _denominator;

    public Fraction(long value)
        : this( value, 1 ) { }

    public Fraction(long numerator, ulong denominator)
    {
        Ensure.IsGreaterThan( denominator, 0UL, nameof( denominator ) );
        Numerator = numerator;
        _denominator = denominator;
    }

    public long Numerator { get; }
    public ulong Denominator => Math.Max( _denominator, 1 );

    [Pure]
    public static Fraction Create(decimal value, ulong denominator)
    {
        var numerator = Math.Round( value * denominator, MidpointRounding.AwayFromZero );
        return new Fraction( (long)numerator, denominator );
    }

    [Pure]
    public static Fraction Create(double value, ulong denominator)
    {
        var numerator = Math.Round( value * denominator, MidpointRounding.AwayFromZero );
        return new Fraction( checked( (long)numerator ), denominator );
    }

    [Pure]
    public static Fraction Create(Fixed value)
    {
        return new Fraction( value.RawValue, unchecked( (ulong)Fixed.GetScale( value.Precision ) ) );
    }

    [Pure]
    public override string ToString()
    {
        return $"{Numerator} / {Denominator}";
    }

    [Pure]
    public override int GetHashCode()
    {
        var f = Simplify();
        return HashCode.Combine( f.Numerator, f._denominator );
    }

    [Pure]
    public override bool Equals(object? obj)
    {
        return obj is Fraction f && Equals( f );
    }

    [Pure]
    public int CompareTo(object? obj)
    {
        return obj is Fraction f ? CompareTo( f ) : 1;
    }

    [Pure]
    public bool Equals(Fraction other)
    {
        var denominator = Denominator;
        var otherDenominator = other.Denominator;

        if ( denominator == otherDenominator )
            return Numerator == other.Numerator;

        var sign = Math.Sign( Numerator );
        if ( sign != Math.Sign( other.Numerator ) )
            return false;

        if ( sign == 0 )
            return true;

        var (hi1, lo1) = MathUtils.BigMulU128( MathUtils.UnsignedAbs( Numerator ), otherDenominator );
        var (hi2, lo2) = MathUtils.BigMulU128( MathUtils.UnsignedAbs( other.Numerator ), denominator );
        return hi1 == hi2 && lo1 == lo2;
    }

    [Pure]
    public int CompareTo(Fraction other)
    {
        var denominator = Denominator;
        var otherDenominator = other.Denominator;

        if ( denominator == otherDenominator )
            return Numerator.CompareTo( other.Numerator );

        var sign = Math.Sign( Numerator );
        var otherSign = Math.Sign( other.Numerator );

        if ( sign != otherSign )
            return sign.CompareTo( otherSign );

        if ( sign == 0 )
            return 0;

        var (hi1, lo1) = MathUtils.BigMulU128( MathUtils.UnsignedAbs( Numerator ), otherDenominator );
        var (hi2, lo2) = MathUtils.BigMulU128( MathUtils.UnsignedAbs( other.Numerator ), denominator );

        var hiCmp = hi1.CompareTo( hi2 );
        if ( hiCmp != 0 )
            return sign > 0 ? hiCmp : -hiCmp;

        return sign > 0 ? lo1.CompareTo( lo2 ) : -lo1.CompareTo( lo2 );
    }

    [Pure]
    public Fraction Negate()
    {
        return new Fraction( checked( -Numerator ), Denominator );
    }

    [Pure]
    public Fraction Abs()
    {
        return new Fraction( Math.Abs( Numerator ), Denominator );
    }

    [Pure]
    public Fraction Truncate()
    {
        var sign = 1;
        var numerator = MathUtils.ToUnsigned( Numerator, ref sign );
        var denominator = Denominator;
        var fractionalPart = numerator % denominator;
        return new Fraction( unchecked( (long)(numerator - fractionalPart) * sign ) / unchecked( (long)denominator ) );
    }

    [Pure]
    public Fraction Floor()
    {
        if ( Numerator >= 0 )
            return Truncate();

        var absNumerator = unchecked( (ulong)-Numerator );
        var denominator = Denominator;
        var fractionalPart = absNumerator % denominator;
        if ( fractionalPart == 0 )
            return new Fraction( Numerator / unchecked( (long)denominator ) );

        var absIntegerPart = unchecked( absNumerator - fractionalPart );
        return new Fraction( checked( -(long)(absIntegerPart + denominator) ) / unchecked( (long)denominator ) );
    }

    [Pure]
    public Fraction Ceiling()
    {
        if ( Numerator <= 0 )
            return Truncate();

        var denominator = Denominator;
        var fractionalPart = unchecked( (ulong)Numerator ) % denominator;
        if ( fractionalPart == 0 )
            return new Fraction( Numerator / unchecked( (long)denominator ) );

        var integerPart = unchecked( (ulong)Numerator - fractionalPart );
        return new Fraction( checked( (long)(integerPart + denominator) ) / unchecked( (long)denominator ) );
    }

    [Pure]
    public Fraction Reciprocal()
    {
        var sign = 1;
        var denominator = Denominator;
        var nextNominator = checked( (long)denominator );
        var nextDenominator = MathUtils.ToUnsigned( Numerator, ref sign );
        return new Fraction( unchecked( nextNominator * sign ), nextDenominator );
    }

    [Pure]
    public Fraction Simplify()
    {
        var denominator = Denominator;
        var gcd = MathUtils.Gcd( MathUtils.UnsignedAbs( Numerator ), denominator );
        return new Fraction( Numerator / unchecked( (long)gcd ), denominator / gcd );
    }

    [Pure]
    public Fraction SetNumerator(long value)
    {
        return new Fraction( value, Denominator );
    }

    [Pure]
    public Fraction SetDenominator(ulong value)
    {
        return new Fraction( Numerator, value );
    }

    [Pure]
    public Fraction Round(ulong denominator)
    {
        var current = Denominator;
        if ( current == denominator )
            return this;

        var ratio = (decimal)denominator / current;
        return new Fraction( (long)Math.Round( Numerator * ratio, MidpointRounding.AwayFromZero ), denominator );
    }

    [Pure]
    public Fraction Increment()
    {
        var denominator = Denominator;
        return new Fraction( checked( Numerator + (long)denominator ), denominator );
    }

    [Pure]
    public Fraction Decrement()
    {
        var denominator = Denominator;
        return new Fraction( checked( Numerator - (long)denominator ), denominator );
    }

    [Pure]
    public Fraction Add(Fraction other)
    {
        return Add( Numerator, Denominator, other.Numerator, other.Denominator );
    }

    [Pure]
    public Fraction Subtract(Fraction other)
    {
        return Subtract( Numerator, Denominator, other.Numerator, other.Denominator );
    }

    [Pure]
    public Fraction Multiply(Fraction other)
    {
        return Multiply( Numerator, Denominator, other.Numerator, other.Denominator );
    }

    [Pure]
    public Fraction Multiply(Percent percent)
    {
        return new Fraction( Numerator * percent, Denominator );
    }

    [Pure]
    public Fraction Divide(Fraction other)
    {
        if ( other.Numerator == 0 )
            throw new DivideByZeroException( ExceptionResources.DividedByZero );

        return Multiply( other.Reciprocal() );
    }

    [Pure]
    public Fraction Modulo(Fraction other)
    {
        if ( other.Numerator == 0 )
            throw new DivideByZeroException( ExceptionResources.DividedByZero );

        var q = Divide( other ).Floor();
        return Subtract( other.Multiply( q ) );
    }

    [Pure]
    public static implicit operator Fraction(Fixed f)
    {
        return Create( f );
    }

    [Pure]
    public static explicit operator double(Fraction f)
    {
        return (double)f.Numerator / f.Denominator;
    }

    [Pure]
    public static explicit operator decimal(Fraction f)
    {
        return (decimal)f.Numerator / f.Denominator;
    }

    [Pure]
    public static Fraction operator -(Fraction f)
    {
        return f.Negate();
    }

    [Pure]
    public static Fraction operator ++(Fraction f)
    {
        return f.Increment();
    }

    [Pure]
    public static Fraction operator --(Fraction f)
    {
        return f.Decrement();
    }

    [Pure]
    public static Fraction operator +(Fraction a, Fraction b)
    {
        return a.Add( b );
    }

    [Pure]
    public static Fraction operator -(Fraction a, Fraction b)
    {
        return a.Subtract( b );
    }

    [Pure]
    public static Fraction operator *(Fraction a, Fraction b)
    {
        return a.Multiply( b );
    }

    [Pure]
    public static Fraction operator *(Fraction a, Percent b)
    {
        return a.Multiply( b );
    }

    [Pure]
    public static Fraction operator *(Percent a, Fraction b)
    {
        return b.Multiply( a );
    }

    [Pure]
    public static Fraction operator /(Fraction a, Fraction b)
    {
        return a.Divide( b );
    }

    [Pure]
    public static Fraction operator %(Fraction a, Fraction b)
    {
        return a.Modulo( b );
    }

    [Pure]
    public static bool operator ==(Fraction a, Fraction b)
    {
        return a.Equals( b );
    }

    [Pure]
    public static bool operator !=(Fraction a, Fraction b)
    {
        return ! a.Equals( b );
    }

    [Pure]
    public static bool operator >=(Fraction a, Fraction b)
    {
        return a.CompareTo( b ) >= 0;
    }

    [Pure]
    public static bool operator <(Fraction a, Fraction b)
    {
        return a.CompareTo( b ) < 0;
    }

    [Pure]
    public static bool operator <=(Fraction a, Fraction b)
    {
        return a.CompareTo( b ) <= 0;
    }

    [Pure]
    public static bool operator >(Fraction a, Fraction b)
    {
        return a.CompareTo( b ) > 0;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static Fraction Add(long n1, ulong d1, long n2, ulong d2)
    {
        if ( d1 == d2 )
            return new Fraction( checked( n1 + n2 ), d1 );

        var gcd = MathUtils.Gcd( d1, d2 );
        var t = d2 / gcd;
        return new Fraction( checked( n1 * (long)t + n2 * (long)(d1 / gcd) ), checked( d1 * t ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static Fraction Subtract(long n1, ulong d1, long n2, ulong d2)
    {
        if ( d1 == d2 )
            return new Fraction( checked( n1 - n2 ), d1 );

        var gcd = MathUtils.Gcd( d1, d2 );
        var t = d2 / gcd;
        return new Fraction( checked( n1 * (long)t - n2 * (long)(d1 / gcd) ), checked( d1 * t ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static Fraction Multiply(long n1, ulong d1, long n2, ulong d2)
    {
        var gcd1 = MathUtils.Gcd( MathUtils.UnsignedAbs( n1 ), d2 );
        var gcd2 = MathUtils.Gcd( MathUtils.UnsignedAbs( n2 ), d1 );
        return new Fraction( checked( n1 / (long)gcd1 * (n2 / (long)gcd2) ), checked( d1 / gcd2 * (d2 / gcd1) ) );
    }
}
