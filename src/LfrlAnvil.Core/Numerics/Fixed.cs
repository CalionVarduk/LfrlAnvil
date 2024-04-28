using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Runtime.CompilerServices;
using LfrlAnvil.Exceptions;
using LfrlAnvil.Extensions;

namespace LfrlAnvil.Numerics;

/// <summary>
/// Represents a lightweight base-10 real number with fixed decimal <see cref="Precision"/>.
/// </summary>
public readonly struct Fixed : IEquatable<Fixed>, IComparable<Fixed>, IComparable, IFormattable
{
    /// <summary>
    /// Minimum allowed precision. Used for integers.
    /// </summary>
    public const byte MinPrecision = 0;

    /// <summary>
    /// Maximum allowed precision.
    /// </summary>
    public const byte MaxPrecision = 18;

    /// <summary>
    /// Represents an integer equal to <b>0</b>.
    /// </summary>
    public static readonly Fixed Zero = new Fixed( 0, MinPrecision );

    /// <summary>
    /// Represents an integer equal to <see cref="Int64.MaxValue"/>.
    /// </summary>
    public static readonly Fixed MaxValue = new Fixed( long.MaxValue, MinPrecision );

    /// <summary>
    /// Represents an integer equal to <see cref="Int64.MinValue"/>.
    /// </summary>
    public static readonly Fixed MinValue = new Fixed( long.MinValue, MinPrecision );

    /// <summary>
    /// Represents smallest possible number greater than <b>0</b>.
    /// </summary>
    public static readonly Fixed Epsilon = new Fixed( 1, MaxPrecision );

    private static readonly long[] PowersOfTen = new[]
    {
        1L,
        10L,
        100L,
        1000L,
        10000L,
        100000L,
        1000000L,
        10000000L,
        100000000L,
        1000000000L,
        10000000000L,
        100000000000L,
        1000000000000L,
        10000000000000L,
        100000000000000L,
        1000000000000000L,
        10000000000000000L,
        100000000000000000L,
        1000000000000000000L
    };

    private Fixed(long rawValue, byte precision)
    {
        Assume.IsLessThanOrEqualTo( precision, MaxPrecision );
        RawValue = rawValue;
        Precision = precision;
    }

    /// <summary>
    /// Represents the raw underlying value of this instance.
    /// The actual value is equal to raw value divided by <b>10^</b><see cref="Precision"/>.
    /// </summary>
    public long RawValue { get; }

    /// <summary>
    /// Represents the decimal precision of this instance.
    /// </summary>
    public byte Precision { get; }

    /// <summary>
    /// Creates a new <see cref="Fixed"/> instance equivalent to <b>0</b> with the provided <paramref name="precision"/>.
    /// </summary>
    /// <param name="precision">Decimal precision.</param>
    /// <returns>New <see cref="Fixed"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// When <paramref name="precision"/> is less than <see cref="MinPrecision"/> or greater than <see cref="MaxPrecision"/>.
    /// </exception>
    [Pure]
    public static Fixed CreateZero(byte precision)
    {
        return CreateRaw( 0, precision );
    }

    /// <summary>
    /// Creates a new <see cref="Fixed"/> instance equivalent to maximum possible value with the provided <paramref name="precision"/>.
    /// </summary>
    /// <param name="precision">Decimal precision.</param>
    /// <returns>New <see cref="Fixed"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// When <paramref name="precision"/> is less than <see cref="MinPrecision"/> or greater than <see cref="MaxPrecision"/>.
    /// </exception>
    [Pure]
    public static Fixed CreateMaxValue(byte precision)
    {
        return CreateRaw( long.MaxValue, precision );
    }

    /// <summary>
    /// Creates a new <see cref="Fixed"/> instance equivalent to minimum possible value with the provided <paramref name="precision"/>.
    /// </summary>
    /// <param name="precision">Decimal precision.</param>
    /// <returns>New <see cref="Fixed"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// When <paramref name="precision"/> is less than <see cref="MinPrecision"/> or greater than <see cref="MaxPrecision"/>.
    /// </exception>
    [Pure]
    public static Fixed CreateMinValue(byte precision)
    {
        return CreateRaw( long.MinValue, precision );
    }

    /// <summary>
    /// Creates a new <see cref="Fixed"/> instance equivalent to the smallest possible value greater than <b>0</b>
    /// with the provided <paramref name="precision"/>.
    /// </summary>
    /// <param name="precision">Decimal precision.</param>
    /// <returns>New <see cref="Fixed"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// When <paramref name="precision"/> is less than <see cref="MinPrecision"/> or greater than <see cref="MaxPrecision"/>.
    /// </exception>
    [Pure]
    public static Fixed CreateEpsilon(byte precision)
    {
        return CreateRaw( 1, precision );
    }

    /// <summary>
    /// Creates a new <see cref="Fixed"/> instance with the provided <paramref name="rawValue"/> and <paramref name="precision"/>.
    /// </summary>
    /// <param name="rawValue">Raw value.</param>
    /// <param name="precision">Decimal precision.</param>
    /// <returns>New <see cref="Fixed"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// When <paramref name="precision"/> is less than <see cref="MinPrecision"/> or greater than <see cref="MaxPrecision"/>.
    /// </exception>
    [Pure]
    public static Fixed CreateRaw(long rawValue, byte precision)
    {
        EnsureCorrectPrecision( precision );
        return new Fixed( rawValue, precision );
    }

    /// <summary>
    /// Creates a new <see cref="Fixed"/> instance equivalent to the provided <see cref="Int64"/> <paramref name="value"/>
    /// with the given <paramref name="precision"/>.
    /// </summary>
    /// <param name="value">Integer value.</param>
    /// <param name="precision">Decimal precision. Equal to <see cref="MinPrecision"/> by default.</param>
    /// <returns>New <see cref="Fixed"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// When <paramref name="precision"/> is less than <see cref="MinPrecision"/> or greater than <see cref="MaxPrecision"/>.
    /// </exception>
    [Pure]
    public static Fixed Create(long value, byte precision = MinPrecision)
    {
        EnsureCorrectPrecision( precision );
        var rawValue = GetRawValue( value, precision );
        return new Fixed( rawValue, precision );
    }

    /// <summary>
    /// Creates a new <see cref="Fixed"/> instance equivalent to the provided <see cref="Decimal"/> <paramref name="value"/>
    /// with the given <paramref name="precision"/>.
    /// </summary>
    /// <param name="value">Decimal value.</param>
    /// <param name="precision">Decimal precision.</param>
    /// <param name="rounding">
    /// Optional <paramref name="value"/> rounding strategy. Equal to <see cref="MidpointRounding.AwayFromZero"/> by default.
    /// </param>
    /// <returns>New <see cref="Fixed"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// When <paramref name="precision"/> is less than <see cref="MinPrecision"/> or greater than <see cref="MaxPrecision"/>.
    /// </exception>
    [Pure]
    public static Fixed Create(decimal value, byte precision, MidpointRounding rounding = MidpointRounding.AwayFromZero)
    {
        EnsureCorrectPrecision( precision );
        var rawValue = GetRawValue( value, precision, rounding );
        return new Fixed( rawValue, precision );
    }

    /// <summary>
    /// Creates a new <see cref="Fixed"/> instance equivalent to the provided <see cref="Double"/> <paramref name="value"/>
    /// with the given <paramref name="precision"/>.
    /// </summary>
    /// <param name="value">Double value.</param>
    /// <param name="precision">Decimal precision.</param>
    /// <param name="rounding">
    /// Optional <paramref name="value"/> rounding strategy. Equal to <see cref="MidpointRounding.AwayFromZero"/> by default.
    /// </param>
    /// <returns>New <see cref="Fixed"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// When <paramref name="precision"/> is less than <see cref="MinPrecision"/> or greater than <see cref="MaxPrecision"/>.
    /// </exception>
    [Pure]
    public static Fixed Create(double value, byte precision, MidpointRounding rounding = MidpointRounding.AwayFromZero)
    {
        EnsureCorrectPrecision( precision );
        var rawValue = GetRawValue( value, precision, rounding );
        return new Fixed( rawValue, precision );
    }

    /// <summary>
    /// Returns <b>10^</b><paramref name="precision"/>.
    /// </summary>
    /// <param name="precision">Decimal precision to calculate scale for.</param>
    /// <returns><b>10^</b><paramref name="precision"/>.</returns>
    /// <exception cref="IndexOutOfRangeException">
    /// When <paramref name="precision"/> is less than <see cref="MinPrecision"/> or greater than <see cref="MaxPrecision"/>.
    /// </exception>
    [Pure]
    public static long GetScale(byte precision)
    {
        return PowersOfTen[precision];
    }

    /// <summary>
    /// Returns a string representation of this <see cref="Fixed"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return ToString( NumberFormatInfo.CurrentInfo );
    }

    /// <summary>
    /// Returns a string representation of this <see cref="Fixed"/> instance.
    /// </summary>
    /// <param name="formatProvider">An optional format provider.</param>
    /// <returns>String representation.</returns>
    [Pure]
    public string ToString(IFormatProvider? formatProvider)
    {
        return ToString( $"N{Precision}", formatProvider );
    }

    /// <summary>
    /// Returns a string representation of this <see cref="Fixed"/> instance.
    /// </summary>
    /// <param name="format">An optional numeric format.</param>
    /// <param name="formatProvider">An optional format provider.</param>
    /// <returns>String representation.</returns>
    [Pure]
    public string ToString([StringSyntax( "NumericFormat" )] string? format, IFormatProvider? formatProvider)
    {
        return (( decimal )this).ToString( format, formatProvider );
    }

    /// <inheritdoc />
    [Pure]
    public override int GetHashCode()
    {
        return (( decimal )this).GetHashCode();
    }

    /// <inheritdoc />
    [Pure]
    public override bool Equals(object? obj)
    {
        return obj is Fixed f && Equals( f );
    }

    /// <inheritdoc />
    [Pure]
    public int CompareTo(object? obj)
    {
        return obj is Fixed f ? CompareTo( f ) : throw new ArgumentException( ExceptionResources.InvalidType, nameof( obj ) );
    }

    /// <inheritdoc />
    [Pure]
    public bool Equals(Fixed other)
    {
        if ( Precision == other.Precision )
            return RawValue == other.RawValue;

        var integer = RawValue / PowersOfTen[Precision];
        var otherInteger = other.RawValue / PowersOfTen[other.Precision];

        if ( integer != otherInteger )
            return false;

        var fractionalPart = RawValue - integer * PowersOfTen[Precision];
        var otherFractionalPart = other.RawValue - integer * PowersOfTen[other.Precision];

        if ( Precision > other.Precision )
            otherFractionalPart *= PowersOfTen[Precision - other.Precision];
        else
            fractionalPart *= PowersOfTen[other.Precision - Precision];

        return fractionalPart == otherFractionalPart;
    }

    /// <inheritdoc />
    [Pure]
    public int CompareTo(Fixed other)
    {
        if ( Precision == other.Precision )
            return RawValue.CompareTo( other.RawValue );

        var integer = RawValue / PowersOfTen[Precision];
        var otherInteger = other.RawValue / PowersOfTen[other.Precision];
        var intComparisonResult = integer.CompareTo( otherInteger );

        if ( intComparisonResult != 0 )
            return intComparisonResult;

        var fractionalPart = RawValue - integer * PowersOfTen[Precision];
        var otherFractionalPart = other.RawValue - integer * PowersOfTen[other.Precision];

        if ( Precision > other.Precision )
            otherFractionalPart *= PowersOfTen[Precision - other.Precision];
        else
            fractionalPart *= PowersOfTen[other.Precision - Precision];

        return fractionalPart.CompareTo( otherFractionalPart );
    }

    /// <summary>
    /// Creates a new <see cref="Fixed"/> instance by calculating an absolute value from this instance.
    /// </summary>
    /// <returns>New <see cref="Fixed"/> instance with unchanged <see cref="Precision"/>.</returns>
    [Pure]
    public Fixed Abs()
    {
        return new Fixed( Math.Abs( RawValue ), Precision );
    }

    /// <summary>
    /// Creates a new <see cref="Fixed"/> instance by truncating this instance.
    /// </summary>
    /// <returns>New <see cref="Fixed"/> instance with unchanged <see cref="Precision"/>.</returns>
    [Pure]
    public Fixed Truncate()
    {
        var fractionalPart = RawValue % PowersOfTen[Precision];
        return new Fixed( RawValue - fractionalPart, Precision );
    }

    /// <summary>
    /// Creates a new <see cref="Fixed"/> instance by rounding this instance to the provided <paramref name="precision"/>.
    /// </summary>
    /// <param name="precision">Decimal precision to round to.</param>
    /// <returns>New <see cref="Fixed"/> instance with unchanged <see cref="Precision"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="precision"/> is less than <b>0</b>.</exception>
    /// <remarks>Uses <see cref="MidpointRounding.AwayFromZero"/> rounding strategy.</remarks>
    [Pure]
    public Fixed Round(int precision)
    {
        if ( precision >= Precision )
            return this;

        Ensure.IsGreaterThanOrEqualTo( precision, 0 );
        var precisionDelta = Precision - precision;
        var rawValue = GetRoundedRawValue( RawValue, precisionDelta );
        return new Fixed( rawValue, Precision );
    }

    /// <summary>
    /// Creates a new <see cref="Fixed"/> instance by calculating the floor of this instance.
    /// </summary>
    /// <returns>New <see cref="Fixed"/> instance with unchanged <see cref="Precision"/>.</returns>
    [Pure]
    public Fixed Floor()
    {
        if ( RawValue >= 0 )
            return Truncate();

        var fractionalPart = RawValue % PowersOfTen[Precision];
        if ( fractionalPart == 0 )
            return this;

        var integerPart = RawValue - fractionalPart;
        var rawValue = checked( integerPart - PowersOfTen[Precision] );
        return new Fixed( rawValue, Precision );
    }

    /// <summary>
    /// Creates a new <see cref="Fixed"/> instance by calculating the ceiling of this instance.
    /// </summary>
    /// <returns>New <see cref="Fixed"/> instance with unchanged <see cref="Precision"/>.</returns>
    [Pure]
    public Fixed Ceiling()
    {
        if ( RawValue <= 0 )
            return Truncate();

        var fractionalPart = RawValue % PowersOfTen[Precision];
        if ( fractionalPart == 0 )
            return this;

        var integerPart = RawValue - fractionalPart;
        var rawValue = checked( integerPart + PowersOfTen[Precision] );
        return new Fixed( rawValue, Precision );
    }

    /// <summary>
    /// Creates a new <see cref="Fixed"/> instance by negating this instance.
    /// </summary>
    /// <returns>New <see cref="Fixed"/> instance with unchanged <see cref="Precision"/>.</returns>
    [Pure]
    public Fixed Negate()
    {
        return new Fixed( checked( -RawValue ), Precision );
    }

    /// <summary>
    /// Creates a new <see cref="Fixed"/> instance with changed <see cref="RawValue"/>.
    /// </summary>
    /// <param name="rawValue">Raw value.</param>
    /// <returns>New <see cref="Fixed"/> instance with unchanged <see cref="Precision"/>.</returns>
    [Pure]
    public Fixed SetRawValue(long rawValue)
    {
        return new Fixed( rawValue, Precision );
    }

    /// <summary>
    /// Creates a new <see cref="Fixed"/> instance equivalent to the provided <see cref="Int64"/> <paramref name="value"/>.
    /// </summary>
    /// <param name="value">Integer value.</param>
    /// <returns>New <see cref="Fixed"/> instance with unchanged <see cref="Precision"/>.</returns>
    [Pure]
    public Fixed SetValue(long value)
    {
        var rawValue = GetRawValue( value, Precision );
        return new Fixed( rawValue, Precision );
    }

    /// <summary>
    /// Creates a new <see cref="Fixed"/> instance equivalent to the provided <see cref="Double"/> <paramref name="value"/>.
    /// </summary>
    /// <param name="value">Double value.</param>
    /// <param name="rounding">
    /// Optional <paramref name="value"/> rounding strategy. Equal to <see cref="MidpointRounding.AwayFromZero"/> by default.
    /// </param>
    /// <returns>New <see cref="Fixed"/> instance with unchanged <see cref="Precision"/>.</returns>
    [Pure]
    public Fixed SetValue(double value, MidpointRounding rounding = MidpointRounding.AwayFromZero)
    {
        var rawValue = GetRawValue( value, Precision, rounding );
        return new Fixed( rawValue, Precision );
    }

    /// <summary>
    /// Creates a new <see cref="Fixed"/> instance equivalent to the provided <see cref="Decimal"/> <paramref name="value"/>.
    /// </summary>
    /// <param name="value">Decimal value.</param>
    /// <param name="rounding">
    /// Optional <paramref name="value"/> rounding strategy. Equal to <see cref="MidpointRounding.AwayFromZero"/> by default.
    /// </param>
    /// <returns>New <see cref="Fixed"/> instance with unchanged <see cref="Precision"/>.</returns>
    [Pure]
    public Fixed SetValue(decimal value, MidpointRounding rounding = MidpointRounding.AwayFromZero)
    {
        var rawValue = GetRawValue( value, Precision, rounding );
        return new Fixed( rawValue, Precision );
    }

    /// <summary>
    /// Creates a new <see cref="Fixed"/> instance with changed <see cref="Precision"/>.
    /// </summary>
    /// <param name="precision">Decimal precision.</param>
    /// <returns>New <see cref="Fixed"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// When <paramref name="precision"/> is less than <see cref="MinPrecision"/> or greater than <see cref="MaxPrecision"/>.
    /// </exception>
    /// <remarks>
    /// Resulting value will be rounded using <see cref="MidpointRounding.AwayFromZero"/> strategy
    /// when new precision is less than the current one.
    /// </remarks>
    [Pure]
    public Fixed SetPrecision(byte precision)
    {
        if ( Precision == precision )
            return this;

        EnsureCorrectPrecision( precision );
        return Precision > precision ? DecreasePrecision( precision ) : IncreasePrecision( precision );
    }

    /// <summary>
    /// Creates a new <see cref="Fixed"/> instance by adding <paramref name="rawValue"/> to the <see cref="RawValue"/> of this instance.
    /// </summary>
    /// <param name="rawValue">Raw value to add.</param>
    /// <returns>New <see cref="Fixed"/> instance with unchanged <see cref="Precision"/>.</returns>
    [Pure]
    public Fixed AddRaw(long rawValue)
    {
        return new Fixed( checked( RawValue + rawValue ), Precision );
    }

    /// <summary>
    /// Creates a new <see cref="Fixed"/> instance by subtracting <paramref name="rawValue"/>
    /// from the <see cref="RawValue"/> of this instance.
    /// </summary>
    /// <param name="rawValue">Raw value to subtract.</param>
    /// <returns>New <see cref="Fixed"/> instance with unchanged <see cref="Precision"/>.</returns>
    [Pure]
    public Fixed SubtractRaw(long rawValue)
    {
        return new Fixed( checked( RawValue - rawValue ), Precision );
    }

    /// <summary>
    /// Creates a new <see cref="Fixed"/> instance by multiplying <paramref name="rawValue"/>
    /// by the <see cref="RawValue"/> of this instance.
    /// </summary>
    /// <param name="rawValue">Raw value to multiply by.</param>
    /// <returns>New <see cref="Fixed"/> instance with unchanged <see cref="Precision"/>.</returns>
    [Pure]
    public Fixed MultiplyRaw(long rawValue)
    {
        return new Fixed( checked( RawValue * rawValue ), Precision );
    }

    /// <summary>
    /// Creates a new <see cref="Fixed"/> instance by dividing the <see cref="RawValue"/> of this instance by <paramref name="rawValue"/>.
    /// </summary>
    /// <param name="rawValue">Raw value to divide by.</param>
    /// <returns>New <see cref="Fixed"/> instance with unchanged <see cref="Precision"/>.</returns>
    /// <exception cref="DivideByZeroException">When <paramref name="rawValue"/> is equal to <b>0</b>.</exception>
    [Pure]
    public Fixed DivideRaw(long rawValue)
    {
        return new Fixed( RawValue / rawValue, Precision );
    }

    /// <summary>
    /// Creates a new <see cref="Fixed"/> instance by calculating the remainder of division of
    /// the <see cref="RawValue"/> of this instance by <paramref name="rawValue"/>.
    /// </summary>
    /// <param name="rawValue">Raw value to divide by.</param>
    /// <returns>New <see cref="Fixed"/> instance with unchanged <see cref="Precision"/>.</returns>
    /// <exception cref="DivideByZeroException">When <paramref name="rawValue"/> is equal to <b>0</b>.</exception>
    [Pure]
    public Fixed ModuloRaw(long rawValue)
    {
        return new Fixed( RawValue % rawValue, Precision );
    }

    /// <summary>
    /// Creates a new <see cref="Fixed"/> instance by adding <b>1</b> to this instance.
    /// </summary>
    /// <returns>New <see cref="Fixed"/> instance with unchanged <see cref="Precision"/>.</returns>
    [Pure]
    public Fixed Increment()
    {
        return new Fixed( checked( RawValue + PowersOfTen[Precision] ), Precision );
    }

    /// <summary>
    /// Creates a new <see cref="Fixed"/> instance by subtracting <b>1</b> from this instance.
    /// </summary>
    /// <returns>New <see cref="Fixed"/> instance with unchanged <see cref="Precision"/>.</returns>
    [Pure]
    public Fixed Decrement()
    {
        return new Fixed( checked( RawValue - PowersOfTen[Precision] ), Precision );
    }

    /// <summary>
    /// Creates a new <see cref="Fixed"/> instance by adding <paramref name="other"/> to this instance.
    /// </summary>
    /// <param name="other">Other instance to add.</param>
    /// <returns>
    /// New <see cref="Fixed"/> instance with <see cref="Precision"/> being the greater precision out of the two operands.
    /// </returns>
    [Pure]
    public Fixed Add(Fixed other)
    {
        if ( Precision == other.Precision )
            return AddRaw( other.RawValue );

        return Precision > other.Precision
            ? AddRaw( other.IncreasePrecisionRaw( Precision ) )
            : IncreasePrecision( other.Precision ).AddRaw( other.RawValue );
    }

    /// <summary>
    /// Creates a new <see cref="Fixed"/> instance by subtracting <paramref name="other"/> from this instance.
    /// </summary>
    /// <param name="other">Other instance to subtract.</param>
    /// <returns>
    /// New <see cref="Fixed"/> instance with <see cref="Precision"/> being the greater precision out of the two operands.
    /// </returns>
    [Pure]
    public Fixed Subtract(Fixed other)
    {
        if ( Precision == other.Precision )
            return SubtractRaw( other.RawValue );

        return Precision > other.Precision
            ? SubtractRaw( other.IncreasePrecisionRaw( Precision ) )
            : IncreasePrecision( other.Precision ).SubtractRaw( other.RawValue );
    }

    /// <summary>
    /// Creates a new <see cref="Fixed"/> instance by multiplying <paramref name="other"/> and this instance together.
    /// </summary>
    /// <param name="other">Other instance to multiply by.</param>
    /// <returns>
    /// New <see cref="Fixed"/> instance with <see cref="Precision"/> being the greater precision out of the two operands.
    /// </returns>
    [Pure]
    public Fixed Multiply(Fixed other)
    {
        if ( Precision == other.Precision )
            return MultiplyInternal( RawValue, other.RawValue, Precision );

        return Precision > other.Precision
            ? MultiplyInternal( RawValue, other.IncreasePrecisionRaw( Precision ), Precision )
            : MultiplyInternal( IncreasePrecisionRaw( other.Precision ), other.RawValue, other.Precision );
    }

    /// <summary>
    /// Creates a new <see cref="Fixed"/> instance by multiplying <paramref name="percent"/> and this instance together.
    /// </summary>
    /// <param name="percent"><see cref="Percent"/> to multiply by.</param>
    /// <returns>New <see cref="Fixed"/> instance with unchanged <see cref="Precision"/>.</returns>
    [Pure]
    public Fixed Multiply(Percent percent)
    {
        return new Fixed( RawValue * percent, Precision );
    }

    /// <summary>
    /// Creates a new <see cref="Fixed"/> instance by dividing this instance by <paramref name="other"/>.
    /// </summary>
    /// <param name="other">Other instance to divide by.</param>
    /// <returns>
    /// New <see cref="Fixed"/> instance with <see cref="Precision"/> being the greater precision out of the two operands.
    /// </returns>
    /// <exception cref="DivideByZeroException">When <paramref name="other"/> is equal to <b>0</b>.</exception>
    [Pure]
    public Fixed Divide(Fixed other)
    {
        if ( Precision == other.Precision )
            return DivideInternal( RawValue, other.RawValue, Precision );

        return Precision > other.Precision
            ? DivideInternal( RawValue, other.IncreasePrecisionRaw( Precision ), Precision )
            : DivideInternal( IncreasePrecisionRaw( other.Precision ), other.RawValue, other.Precision );
    }

    /// <summary>
    /// Creates a new <see cref="Fixed"/> instance by calculating the remainder of division of this instance by <paramref name="other"/>.
    /// </summary>
    /// <param name="other">Other instance to divide by.</param>
    /// <returns>
    /// New <see cref="Fixed"/> instance with <see cref="Precision"/> being the greater precision out of the two operands.
    /// </returns>
    /// <exception cref="DivideByZeroException">When <paramref name="other"/> is equal to <b>0</b>.</exception>
    [Pure]
    public Fixed Modulo(Fixed other)
    {
        if ( Precision == other.Precision )
            return ModuloRaw( other.RawValue );

        return Precision > other.Precision
            ? ModuloRaw( other.IncreasePrecisionRaw( Precision ) )
            : IncreasePrecision( other.Precision ).ModuloRaw( other.RawValue );
    }

    /// <summary>
    /// Converts the provided <paramref name="f"/> to <see cref="Decimal"/>.
    /// </summary>
    /// <param name="f">Number to convert.</param>
    /// <returns>New <see cref="Decimal"/> instance.</returns>
    [Pure]
    public static implicit operator decimal(Fixed f)
    {
        var result = ( decimal )f.RawValue / PowersOfTen[f.Precision];
        return result;
    }

    /// <summary>
    /// Converts the provided <paramref name="f"/> to <see cref="Double"/>.
    /// </summary>
    /// <param name="f">Number to convert.</param>
    /// <returns>New <see cref="Double"/> instance.</returns>
    [Pure]
    public static explicit operator double(Fixed f)
    {
        var result = ( double )f.RawValue / PowersOfTen[f.Precision];
        return result;
    }

    /// <summary>
    /// Converts the provided <paramref name="f"/> to <see cref="Int64"/> through truncation.
    /// </summary>
    /// <param name="f">Number to convert.</param>
    /// <returns>New <see cref="Int64"/> instance.</returns>
    [Pure]
    public static explicit operator long(Fixed f)
    {
        var result = f.RawValue / PowersOfTen[f.Precision];
        return result;
    }

    /// <summary>
    /// Creates a new <see cref="Fixed"/> instance by negating the provided <paramref name="f"/>.
    /// </summary>
    /// <param name="f">Operand.</param>
    /// <returns>New <see cref="Fixed"/> instance.</returns>
    [Pure]
    public static Fixed operator -(Fixed f)
    {
        return f.Negate();
    }

    /// <summary>
    /// Creates a new <see cref="Fixed"/> instance by incrementing the provided <paramref name="f"/>.
    /// </summary>
    /// <param name="f">Operand.</param>
    /// <returns>New <see cref="Fixed"/> instance.</returns>
    [Pure]
    public static Fixed operator ++(Fixed f)
    {
        return f.Increment();
    }

    /// <summary>
    /// Creates a new <see cref="Fixed"/> instance by decrementing the provided <paramref name="f"/>.
    /// </summary>
    /// <param name="f">Operand.</param>
    /// <returns>New <see cref="Fixed"/> instance.</returns>
    [Pure]
    public static Fixed operator --(Fixed f)
    {
        return f.Decrement();
    }

    /// <summary>
    /// Creates a new <see cref="Fixed"/> instance by adding <paramref name="left"/> and <paramref name="right"/> together.
    /// </summary>
    /// <param name="left">First operand.</param>
    /// <param name="right">Second operand.</param>
    /// <returns>New <see cref="Fixed"/> instance.</returns>
    [Pure]
    public static Fixed operator +(Fixed left, Fixed right)
    {
        return left.Add( right );
    }

    /// <summary>
    /// Creates a new <see cref="Fixed"/> instance by subtracting <paramref name="right"/> from <paramref name="left"/>.
    /// </summary>
    /// <param name="left">First operand.</param>
    /// <param name="right">Second operand.</param>
    /// <returns>New <see cref="Fixed"/> instance.</returns>
    [Pure]
    public static Fixed operator -(Fixed left, Fixed right)
    {
        return left.Subtract( right );
    }

    /// <summary>
    /// Creates a new <see cref="Fixed"/> instance by multiplying <paramref name="left"/> and <paramref name="right"/> together.
    /// </summary>
    /// <param name="left">First operand.</param>
    /// <param name="right">Second operand.</param>
    /// <returns>New <see cref="Fixed"/> instance.</returns>
    [Pure]
    public static Fixed operator *(Fixed left, Fixed right)
    {
        return left.Multiply( right );
    }

    /// <summary>
    /// Creates a new <see cref="Fixed"/> instance by dividing <paramref name="left"/> by <paramref name="right"/>.
    /// </summary>
    /// <param name="left">First operand.</param>
    /// <param name="right">Second operand.</param>
    /// <returns>New <see cref="Fixed"/> instance.</returns>
    /// <exception cref="DivideByZeroException">When <paramref name="right"/> is equal to <b>0</b>.</exception>
    [Pure]
    public static Fixed operator /(Fixed left, Fixed right)
    {
        return left.Divide( right );
    }

    /// <summary>
    /// Creates a new <see cref="Fixed"/> instance by calculating the remainder of division of
    /// <paramref name="left"/> by <paramref name="right"/>.
    /// </summary>
    /// <param name="left">First operand.</param>
    /// <param name="right">Second operand.</param>
    /// <returns>New <see cref="Fixed"/> instance.</returns>
    /// <exception cref="DivideByZeroException">When <paramref name="right"/> is equal to <b>0</b>.</exception>
    [Pure]
    public static Fixed operator %(Fixed left, Fixed right)
    {
        return left.Modulo( right );
    }

    /// <summary>
    /// Creates a new <see cref="Fixed"/> instance by multiplying <paramref name="left"/> and <paramref name="right"/> together.
    /// </summary>
    /// <param name="left">First operand.</param>
    /// <param name="right">Second operand.</param>
    /// <returns>New <see cref="Fixed"/> instance.</returns>
    [Pure]
    public static Fixed operator *(Fixed left, Percent right)
    {
        return left.Multiply( right );
    }

    /// <summary>
    /// Creates a new <see cref="Fixed"/> instance by multiplying <paramref name="left"/> and <paramref name="right"/> together.
    /// </summary>
    /// <param name="left">First operand.</param>
    /// <param name="right">Second operand.</param>
    /// <returns>New <see cref="Fixed"/> instance.</returns>
    [Pure]
    public static Fixed operator *(Percent left, Fixed right)
    {
        return right.Multiply( left );
    }

    /// <summary>
    /// Checks if <paramref name="left"/> is equal to <paramref name="right"/>.
    /// </summary>
    /// <param name="left">First operand.</param>
    /// <param name="right">Second operand.</param>
    /// <returns><b>true</b> when operands are equal, otherwise <b>false</b>.</returns>
    [Pure]
    public static bool operator ==(Fixed left, Fixed right)
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
    public static bool operator !=(Fixed left, Fixed right)
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
    public static bool operator <(Fixed left, Fixed right)
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
    public static bool operator <=(Fixed left, Fixed right)
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
    public static bool operator >(Fixed left, Fixed right)
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
    public static bool operator >=(Fixed left, Fixed right)
    {
        return left.CompareTo( right ) >= 0;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static void EnsureCorrectPrecision(byte precision)
    {
        Ensure.IsInRange( precision, MinPrecision, MaxPrecision );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private Fixed DecreasePrecision(byte precision)
    {
        return new Fixed( DecreasePrecisionRaw( precision ), precision );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private Fixed IncreasePrecision(byte precision)
    {
        return new Fixed( IncreasePrecisionRaw( precision ), precision );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private long DecreasePrecisionRaw(byte precision)
    {
        Assume.IsInRange( precision, MinPrecision, Precision - 1 );
        var precisionDelta = Precision - precision;
        var rawValue = GetRoundedRawValue( RawValue, precisionDelta ) / PowersOfTen[precisionDelta];
        return rawValue;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private long IncreasePrecisionRaw(byte precision)
    {
        Assume.IsInRange( precision, Precision + 1, MaxPrecision );
        var precisionDelta = precision - Precision;
        var rawValue = checked( RawValue * PowersOfTen[precisionDelta] );
        return rawValue;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static long GetRawValue(long value, byte precision)
    {
        Assume.IsLessThanOrEqualTo( precision, MaxPrecision );
        var rawValue = checked( value * PowersOfTen[precision] );
        return rawValue;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static long GetRawValue(decimal value, byte precision, MidpointRounding rounding)
    {
        Assume.IsLessThanOrEqualTo( precision, MaxPrecision );
        var integerInputPart = Math.Truncate( value );
        var integerPart = checked( ( long )integerInputPart * PowersOfTen[precision] );
        var fractionalInputPart = Math.Round( value - integerInputPart, precision, rounding );
        var fractionalPart = ( long )(fractionalInputPart * PowersOfTen[precision]);
        var rawValue = checked( integerPart + fractionalPart );
        return rawValue;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static long GetRawValue(double value, byte precision, MidpointRounding rounding)
    {
        Assume.IsLessThanOrEqualTo( precision, MaxPrecision );
        var integerInputPart = Math.Truncate( value );
        var integerPart = checked( ( long )integerInputPart * PowersOfTen[precision] );
        var fractionalInputPart = Math.Round( value - integerInputPart, precision, rounding );
        var fractionalPart = ( long )(fractionalInputPart * PowersOfTen[precision]);
        var rawValue = checked( integerPart + fractionalPart );
        return rawValue;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static long GetRoundedRawValue(long rawValue, int precisionDelta)
    {
        Assume.IsGreaterThan( precisionDelta, 0 );
        var discardedFractionalPart = rawValue % PowersOfTen[precisionDelta];
        var result = rawValue - discardedFractionalPart;

        if ( Math.Abs( discardedFractionalPart ) >= PowersOfTen[precisionDelta] >> 1 )
            result += rawValue < 0 ? -PowersOfTen[precisionDelta] : PowersOfTen[precisionDelta];

        return result;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static Fixed MultiplyInternal(long rawLeft, long rawRight, byte precision)
    {
        var sign = 1;
        var left = MathUtils.ToUnsigned( rawLeft, ref sign );
        var right = MathUtils.ToUnsigned( rawRight, ref sign );

        ulong unsignedResult, remainder;
        var (resultHigh, resultLow) = MathUtils.BigMulU128( left, right );

        if ( resultHigh > 0 )
        {
            (var resultOverflow, unsignedResult, remainder)
                = MathUtils.BigDivU128( resultHigh, resultLow, ( ulong )PowersOfTen[precision] );

            if ( resultOverflow > 0 )
                ExceptionThrower.Throw( new OverflowException() );
        }
        else
            (unsignedResult, remainder) = Math.DivRem( resultLow, ( ulong )PowersOfTen[precision] );

        if ( remainder > 0 && remainder >= ( ulong )PowersOfTen[precision] >> 1 )
            unsignedResult = checked( unsignedResult + 1 );

        var result = MathUtils.ToSigned( unsignedResult, sign );
        return new Fixed( result, precision );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static Fixed DivideInternal(long rawLeft, long rawRight, byte precision)
    {
        var sign = 1;
        var left = MathUtils.ToUnsigned( rawLeft, ref sign );
        var right = MathUtils.ToUnsigned( rawRight, ref sign );

        ulong unsignedResult, remainder;
        var (leftHigh, leftLow) = MathUtils.BigMulU128( left, ( ulong )PowersOfTen[precision] );

        if ( leftHigh > 0 )
        {
            (var resultOverflow, unsignedResult, remainder) = MathUtils.BigDivU128( leftHigh, leftLow, right );
            if ( resultOverflow > 0 )
                ExceptionThrower.Throw( new OverflowException() );
        }
        else
            (unsignedResult, remainder) = Math.DivRem( leftLow, right );

        if ( remainder >= (right.IsEven() ? right >> 1 : (right >> 1) + 1) )
            unsignedResult = checked( unsignedResult + 1 );

        var result = MathUtils.ToSigned( unsignedResult, sign );
        return new Fixed( result, precision );
    }
}
