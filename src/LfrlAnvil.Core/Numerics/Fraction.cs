using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Exceptions;

namespace LfrlAnvil.Numerics;

/// <summary>
/// Represents a lightweight fraction, that is a number with separate <see cref="Numerator"/> and <see cref="Denominator"/>.
/// </summary>
public readonly struct Fraction : IEquatable<Fraction>, IComparable<Fraction>, IComparable
{
    /// <summary>
    /// Represents <b>0/1</b> fraction.
    /// </summary>
    public static readonly Fraction Zero = new Fraction( 0 );

    /// <summary>
    /// Represents <b>1/1</b> fraction.
    /// </summary>
    public static readonly Fraction One = new Fraction( 1 );

    /// <summary>
    /// Represents the maximum fraction value equivalent to <see cref="Int64.MaxValue"/>.
    /// </summary>
    public static readonly Fraction MaxValue = new Fraction( long.MaxValue );

    /// <summary>
    /// Represents the minimum fraction value equivalent to <see cref="Int64.MinValue"/>.
    /// </summary>
    public static readonly Fraction MinValue = new Fraction( long.MinValue );

    /// <summary>
    /// Represents smallest possible fraction greater than <b>0</b>, with <see cref="Numerator"/> equal to <b>1</b>
    /// and <see cref="Denominator"/> equal to <see cref="UInt64.MaxValue"/>.
    /// </summary>
    public static readonly Fraction Epsilon = new Fraction( 1, ulong.MaxValue );

    private readonly ulong _denominator;

    /// <summary>
    /// Creates a new <see cref="Fraction"/> instance.
    /// </summary>
    /// <param name="numerator">Fraction's numerator.</param>
    /// <param name="denominator">Fraction's denominator. Equal to <b>1</b> by default.</param>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="denominator"/> is equal to <b>0</b>.</exception>
    public Fraction(long numerator, ulong denominator = 1)
    {
        Ensure.IsGreaterThan( denominator, 0UL );
        Numerator = numerator;
        _denominator = denominator;
    }

    /// <summary>
    /// Fraction's numerator.
    /// </summary>
    public long Numerator { get; }

    /// <summary>
    /// Fraction's denominator.
    /// </summary>
    public ulong Denominator => Math.Max( _denominator, 1 );

    /// <summary>
    /// Creates a new <see cref="Fraction"/> instance from the provided <see cref="Decimal"/> and <paramref name="denominator"/>.
    /// </summary>
    /// <param name="value">Value to convert to a fraction.</param>
    /// <param name="denominator">Fraction's denominator.</param>
    /// <param name="rounding">
    /// Optional <paramref name="value"/> rounding strategy. Equal to <see cref="MidpointRounding.AwayFromZero"/> by default.
    /// </param>
    /// <returns>New <see cref="Fraction"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="denominator"/> is equal to <b>0</b>.</exception>
    [Pure]
    public static Fraction Create(decimal value, ulong denominator, MidpointRounding rounding = MidpointRounding.AwayFromZero)
    {
        var numerator = Math.Round( value * denominator, rounding );
        return new Fraction( ( long )numerator, denominator );
    }

    /// <summary>
    /// Creates a new <see cref="Fraction"/> instance from the provided <see cref="Double"/> and <paramref name="denominator"/>.
    /// </summary>
    /// <param name="value">Value to convert to a fraction.</param>
    /// <param name="denominator">Fraction's denominator.</param>
    /// <param name="rounding">
    /// Optional <paramref name="value"/> rounding strategy. Equal to <see cref="MidpointRounding.AwayFromZero"/> by default.
    /// </param>
    /// <returns>New <see cref="Fraction"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="denominator"/> is equal to <b>0</b>.</exception>
    [Pure]
    public static Fraction Create(double value, ulong denominator, MidpointRounding rounding = MidpointRounding.AwayFromZero)
    {
        var numerator = Math.Round( value * denominator, rounding );
        return new Fraction( checked( ( long )numerator ), denominator );
    }

    /// <summary>
    /// Creates a new <see cref="Fraction"/> instance from the provided <see cref="Fixed"/>.
    /// </summary>
    /// <param name="value">Value to convert to a fraction.</param>
    /// <returns>New <see cref="Fraction"/> instance.</returns>
    /// <remarks>
    /// Resulting <see cref="Denominator"/> will be equal to <b>10^p</b>,
    /// where <b>p</b> represents the <see cref="Fixed.Precision"/> of the provided <paramref name="value"/>.
    /// </remarks>
    [Pure]
    public static Fraction Create(Fixed value)
    {
        return new Fraction( value.RawValue, unchecked( ( ulong )Fixed.GetScale( value.Precision ) ) );
    }

    /// <summary>
    /// Returns a string representation of this <see cref="Fraction"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return $"{Numerator} / {Denominator}";
    }

    /// <inheritdoc />
    [Pure]
    public override int GetHashCode()
    {
        var f = Simplify();
        return HashCode.Combine( f.Numerator, f._denominator );
    }

    /// <inheritdoc />
    [Pure]
    public override bool Equals(object? obj)
    {
        return obj is Fraction f && Equals( f );
    }

    /// <inheritdoc />
    [Pure]
    public int CompareTo(object? obj)
    {
        return obj is Fraction f ? CompareTo( f ) : throw new ArgumentException( ExceptionResources.InvalidType, nameof( obj ) );
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
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

    /// <summary>
    /// Creates a new <see cref="Fraction"/> instance by negating this instance.
    /// </summary>
    /// <returns>New <see cref="Fraction"/> instance with unchanged <see cref="Denominator"/>.</returns>
    [Pure]
    public Fraction Negate()
    {
        return new Fraction( checked( -Numerator ), Denominator );
    }

    /// <summary>
    /// Creates a new <see cref="Fraction"/> instance by calculating an absolute value from this instance.
    /// </summary>
    /// <returns>New <see cref="Fraction"/> instance with unchanged <see cref="Denominator"/>.</returns>
    [Pure]
    public Fraction Abs()
    {
        return new Fraction( Math.Abs( Numerator ), Denominator );
    }

    /// <summary>
    /// Creates a new <see cref="Fraction"/> instance by truncating this instance.
    /// </summary>
    /// <returns>New <see cref="Fraction"/> instance with <see cref="Denominator"/> equal to <b>1</b>.</returns>
    [Pure]
    public Fraction Truncate()
    {
        var sign = 1;
        var numerator = MathUtils.ToUnsigned( Numerator, ref sign );
        var denominator = Denominator;
        var fractionalPart = numerator % denominator;
        return new Fraction( unchecked( ( long )(numerator - fractionalPart) * sign ) / unchecked( ( long )denominator ) );
    }

    /// <summary>
    /// Creates a new <see cref="Fraction"/> instance by calculating the floor of this instance.
    /// </summary>
    /// <returns>New <see cref="Fraction"/> instance with <see cref="Denominator"/> equal to <b>1</b>.</returns>
    [Pure]
    public Fraction Floor()
    {
        if ( Numerator >= 0 )
            return Truncate();

        var absNumerator = unchecked( ( ulong )-Numerator );
        var denominator = Denominator;
        var fractionalPart = absNumerator % denominator;
        if ( fractionalPart == 0 )
            return new Fraction( Numerator / unchecked( ( long )denominator ) );

        var absIntegerPart = unchecked( absNumerator - fractionalPart );
        return new Fraction( checked( -( long )(absIntegerPart + denominator) ) / unchecked( ( long )denominator ) );
    }

    /// <summary>
    /// Creates a new <see cref="Fraction"/> instance by calculating the ceiling of this instance.
    /// </summary>
    /// <returns>New <see cref="Fraction"/> instance with <see cref="Denominator"/> equal to <b>1</b>.</returns>
    [Pure]
    public Fraction Ceiling()
    {
        if ( Numerator <= 0 )
            return Truncate();

        var denominator = Denominator;
        var fractionalPart = unchecked( ( ulong )Numerator ) % denominator;
        if ( fractionalPart == 0 )
            return new Fraction( Numerator / unchecked( ( long )denominator ) );

        var integerPart = unchecked( ( ulong )Numerator - fractionalPart );
        return new Fraction( checked( ( long )(integerPart + denominator) ) / unchecked( ( long )denominator ) );
    }

    /// <summary>
    /// Creates a new <see cref="Fraction"/> instance that represents a reciprocal of this instance.
    /// </summary>
    /// <returns>New <see cref="Fraction"/> instance.</returns>
    [Pure]
    public Fraction Reciprocal()
    {
        var sign = 1;
        var denominator = Denominator;
        var nextNominator = checked( ( long )denominator );
        var nextDenominator = MathUtils.ToUnsigned( Numerator, ref sign );
        return new Fraction( unchecked( nextNominator * sign ), nextDenominator );
    }

    /// <summary>
    /// Creates a new <see cref="Fraction"/> instance equivalent to this instance,
    /// with its <see cref="Numerator"/> and <see cref="Denominator"/> divided by their GCD (greatest common divisor).
    /// </summary>
    /// <returns>New <see cref="Fraction"/> instance.</returns>
    [Pure]
    public Fraction Simplify()
    {
        var denominator = Denominator;
        var gcd = MathUtils.Gcd( MathUtils.UnsignedAbs( Numerator ), denominator );
        return new Fraction( Numerator / unchecked( ( long )gcd ), denominator / gcd );
    }

    /// <summary>
    /// Creates a new <see cref="Fraction"/> instance with changed <see cref="Numerator"/>.
    /// </summary>
    /// <param name="value">Fraction's numerator.</param>
    /// <returns>New <see cref="Fraction"/> instance with unchanged <see cref="Denominator"/>.</returns>
    [Pure]
    public Fraction SetNumerator(long value)
    {
        return new Fraction( value, Denominator );
    }

    /// <summary>
    /// Creates a new <see cref="Fraction"/> instance with changed <see cref="Denominator"/>.
    /// </summary>
    /// <param name="value">Fraction's denominator.</param>
    /// <returns>New <see cref="Fraction"/> instance with unchanged <see cref="Numerator"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="value"/> is equal to <b>0</b>.</exception>
    [Pure]
    public Fraction SetDenominator(ulong value)
    {
        return new Fraction( Numerator, value );
    }

    /// <summary>
    /// Creates a new <see cref="Fraction"/> instance by rounding this instance to the provided <paramref name="denominator"/>.
    /// </summary>
    /// <param name="denominator">Fraction's denominator.</param>
    /// <param name="rounding">
    /// Optional numerator rounding strategy. Equal to <see cref="MidpointRounding.AwayFromZero"/> by default.
    /// </param>
    /// <returns>New <see cref="Fraction"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="denominator"/> is equal to <b>0</b>.</exception>
    /// <remarks>See <see cref="Math.Round(Decimal,MidpointRounding)"/> for more information.</remarks>
    [Pure]
    public Fraction Round(ulong denominator, MidpointRounding rounding = MidpointRounding.AwayFromZero)
    {
        var current = Denominator;
        if ( current == denominator )
            return this;

        var ratio = ( decimal )denominator / current;
        return new Fraction( ( long )Math.Round( Numerator * ratio, rounding ), denominator );
    }

    /// <summary>
    /// Creates a new <see cref="Fraction"/> instance by adding <b>1</b> to this instance.
    /// </summary>
    /// <returns>New <see cref="Fraction"/> instance with unchanged <see cref="Denominator"/>.</returns>
    [Pure]
    public Fraction Increment()
    {
        var denominator = Denominator;
        return new Fraction( checked( Numerator + ( long )denominator ), denominator );
    }

    /// <summary>
    /// Creates a new <see cref="Fraction"/> instance by subtracting <b>1</b> from this instance.
    /// </summary>
    /// <returns>New <see cref="Fraction"/> instance with unchanged <see cref="Denominator"/>.</returns>
    [Pure]
    public Fraction Decrement()
    {
        var denominator = Denominator;
        return new Fraction( checked( Numerator - ( long )denominator ), denominator );
    }

    /// <summary>
    /// Creates a new <see cref="Fraction"/> instance by adding <paramref name="other"/> to this instance.
    /// </summary>
    /// <param name="other">Other instance to add.</param>
    /// <returns>New <see cref="Fraction"/> instance.</returns>
    [Pure]
    public Fraction Add(Fraction other)
    {
        return Add( Numerator, Denominator, other.Numerator, other.Denominator );
    }

    /// <summary>
    /// Creates a new <see cref="Fraction"/> instance by subtracting <paramref name="other"/> from this instance.
    /// </summary>
    /// <param name="other">Other instance to subtract.</param>
    /// <returns>New <see cref="Fraction"/> instance.</returns>
    [Pure]
    public Fraction Subtract(Fraction other)
    {
        return Subtract( Numerator, Denominator, other.Numerator, other.Denominator );
    }

    /// <summary>
    /// Creates a new <see cref="Fraction"/> instance by multiplying <paramref name="other"/> and this instance together.
    /// </summary>
    /// <param name="other">Other instance to multiply by.</param>
    /// <returns>New <see cref="Fraction"/> instance.</returns>
    [Pure]
    public Fraction Multiply(Fraction other)
    {
        return Multiply( Numerator, Denominator, other.Numerator, other.Denominator );
    }

    /// <summary>
    /// Creates a new <see cref="Fraction"/> instance by multiplying <paramref name="percent"/> and this instance together.
    /// </summary>
    /// <param name="percent"><see cref="Percent"/> to multiply by.</param>
    /// <returns>New <see cref="Fraction"/> instance with unchanged <see cref="Denominator"/>.</returns>
    [Pure]
    public Fraction Multiply(Percent percent)
    {
        return new Fraction( Numerator * percent, Denominator );
    }

    /// <summary>
    /// Creates a new <see cref="Fraction"/> instance by dividing this instance by <paramref name="other"/>.
    /// </summary>
    /// <param name="other">Other instance to divide by.</param>
    /// <returns>New <see cref="Fraction"/> instance.</returns>
    /// <exception cref="DivideByZeroException">When <paramref name="other"/> is equal to <b>0</b>.</exception>
    [Pure]
    public Fraction Divide(Fraction other)
    {
        if ( other.Numerator == 0 )
            throw new DivideByZeroException( ExceptionResources.DividedByZero );

        return Multiply( other.Reciprocal() );
    }

    /// <summary>
    /// Creates a new <see cref="Fraction"/> instance by calculating the remainder of division of this instance by <paramref name="other"/>.
    /// </summary>
    /// <param name="other">Other instance to divide by.</param>
    /// <returns>New <see cref="Fraction"/> instance.</returns>
    /// <exception cref="DivideByZeroException">When <paramref name="other"/> is equal to <b>0</b>.</exception>
    [Pure]
    public Fraction Modulo(Fraction other)
    {
        if ( other.Numerator == 0 )
            throw new DivideByZeroException( ExceptionResources.DividedByZero );

        var q = Divide( other ).Floor();
        return Subtract( other.Multiply( q ) );
    }

    /// <summary>
    /// Converts the provided <paramref name="f"/> to <see cref="Fraction"/>.
    /// </summary>
    /// <param name="f"><see cref="Fixed"/> to convert.</param>
    /// <returns>New <see cref="Fraction"/> instance.</returns>
    [Pure]
    public static implicit operator Fraction(Fixed f)
    {
        return Create( f );
    }

    /// <summary>
    /// Converts the provided <paramref name="f"/> to <see cref="Double"/>.
    /// </summary>
    /// <param name="f">Fraction to convert.</param>
    /// <returns>New <see cref="Double"/> instance.</returns>
    [Pure]
    public static explicit operator double(Fraction f)
    {
        return ( double )f.Numerator / f.Denominator;
    }

    /// <summary>
    /// Converts the provided <paramref name="f"/> to <see cref="Decimal"/>.
    /// </summary>
    /// <param name="f">Fraction to convert.</param>
    /// <returns>New <see cref="Decimal"/> instance.</returns>
    [Pure]
    public static explicit operator decimal(Fraction f)
    {
        return ( decimal )f.Numerator / f.Denominator;
    }

    /// <summary>
    /// Creates a new <see cref="Fraction"/> instance by negating the provided <paramref name="f"/>.
    /// </summary>
    /// <param name="f">Operand.</param>
    /// <returns>New <see cref="Fraction"/> instance.</returns>
    [Pure]
    public static Fraction operator -(Fraction f)
    {
        return f.Negate();
    }

    /// <summary>
    /// Creates a new <see cref="Fraction"/> instance by incrementing the provided <paramref name="f"/>.
    /// </summary>
    /// <param name="f">Operand.</param>
    /// <returns>New <see cref="Fraction"/> instance.</returns>
    [Pure]
    public static Fraction operator ++(Fraction f)
    {
        return f.Increment();
    }

    /// <summary>
    /// Creates a new <see cref="Fraction"/> instance by decrementing the provided <paramref name="f"/>.
    /// </summary>
    /// <param name="f">Operand.</param>
    /// <returns>New <see cref="Fraction"/> instance.</returns>
    [Pure]
    public static Fraction operator --(Fraction f)
    {
        return f.Decrement();
    }

    /// <summary>
    /// Creates a new <see cref="Fraction"/> instance by adding <paramref name="a"/> and <paramref name="b"/> together.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns>New <see cref="Fraction"/> instance.</returns>
    [Pure]
    public static Fraction operator +(Fraction a, Fraction b)
    {
        return a.Add( b );
    }

    /// <summary>
    /// Creates a new <see cref="Fraction"/> instance by subtracting <paramref name="b"/> from <paramref name="a"/>.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns>New <see cref="Fraction"/> instance.</returns>
    [Pure]
    public static Fraction operator -(Fraction a, Fraction b)
    {
        return a.Subtract( b );
    }

    /// <summary>
    /// Creates a new <see cref="Fraction"/> instance by multiplying <paramref name="a"/> and <paramref name="b"/> together.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns>New <see cref="Fraction"/> instance.</returns>
    [Pure]
    public static Fraction operator *(Fraction a, Fraction b)
    {
        return a.Multiply( b );
    }

    /// <summary>
    /// Creates a new <see cref="Fraction"/> instance by multiplying <paramref name="a"/> and <paramref name="b"/> together.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns>New <see cref="Fraction"/> instance.</returns>
    [Pure]
    public static Fraction operator *(Fraction a, Percent b)
    {
        return a.Multiply( b );
    }

    /// <summary>
    /// Creates a new <see cref="Fraction"/> instance by multiplying <paramref name="a"/> and <paramref name="b"/> together.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns>New <see cref="Fraction"/> instance.</returns>
    [Pure]
    public static Fraction operator *(Percent a, Fraction b)
    {
        return b.Multiply( a );
    }

    /// <summary>
    /// Creates a new <see cref="Fraction"/> instance by dividing <paramref name="a"/> by <paramref name="b"/>.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns>New <see cref="Fraction"/> instance.</returns>
    /// <exception cref="DivideByZeroException">When <paramref name="b"/> is equal to <b>0</b>.</exception>
    [Pure]
    public static Fraction operator /(Fraction a, Fraction b)
    {
        return a.Divide( b );
    }

    /// <summary>
    /// Creates a new <see cref="Fraction"/> instance by calculating the remainder of division of
    /// <paramref name="a"/> by <paramref name="b"/>.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns>New <see cref="Fraction"/> instance.</returns>
    /// <exception cref="DivideByZeroException">When <paramref name="b"/> is equal to <b>0</b>.</exception>
    [Pure]
    public static Fraction operator %(Fraction a, Fraction b)
    {
        return a.Modulo( b );
    }

    /// <summary>
    /// Checks if <paramref name="a"/> is equal to <paramref name="b"/>.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns><b>true</b> when operands are equal, otherwise <b>false</b>.</returns>
    [Pure]
    public static bool operator ==(Fraction a, Fraction b)
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
    public static bool operator !=(Fraction a, Fraction b)
    {
        return ! a.Equals( b );
    }

    /// <summary>
    /// Checks if <paramref name="a"/> is greater than or equal to <paramref name="b"/>.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns><b>true</b> when <paramref name="a"/> is greater than or equal to <paramref name="b"/>, otherwise <b>false</b>.</returns>
    [Pure]
    public static bool operator >=(Fraction a, Fraction b)
    {
        return a.CompareTo( b ) >= 0;
    }

    /// <summary>
    /// Checks if <paramref name="a"/> is less than <paramref name="b"/>.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns><b>true</b> when <paramref name="a"/> is less than <paramref name="b"/>, otherwise <b>false</b>.</returns>
    [Pure]
    public static bool operator <(Fraction a, Fraction b)
    {
        return a.CompareTo( b ) < 0;
    }

    /// <summary>
    /// Checks if <paramref name="a"/> is less than or equal to <paramref name="b"/>.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns><b>true</b> when <paramref name="a"/> is less than or equal to <paramref name="b"/>, otherwise <b>false</b>.</returns>
    [Pure]
    public static bool operator <=(Fraction a, Fraction b)
    {
        return a.CompareTo( b ) <= 0;
    }

    /// <summary>
    /// Checks if <paramref name="a"/> is greater than <paramref name="b"/>.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns><b>true</b> when <paramref name="a"/> is greater than <paramref name="b"/>, otherwise <b>false</b>.</returns>
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
        return new Fraction( checked( n1 * ( long )t + n2 * ( long )(d1 / gcd) ), checked( d1 * t ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static Fraction Subtract(long n1, ulong d1, long n2, ulong d2)
    {
        if ( d1 == d2 )
            return new Fraction( checked( n1 - n2 ), d1 );

        var gcd = MathUtils.Gcd( d1, d2 );
        var t = d2 / gcd;
        return new Fraction( checked( n1 * ( long )t - n2 * ( long )(d1 / gcd) ), checked( d1 * t ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static Fraction Multiply(long n1, ulong d1, long n2, ulong d2)
    {
        var gcd1 = MathUtils.Gcd( MathUtils.UnsignedAbs( n1 ), d2 );
        var gcd2 = MathUtils.Gcd( MathUtils.UnsignedAbs( n2 ), d1 );
        return new Fraction( checked( n1 / ( long )gcd1 * (n2 / ( long )gcd2) ), checked( d1 / gcd2 * (d2 / gcd1) ) );
    }
}
