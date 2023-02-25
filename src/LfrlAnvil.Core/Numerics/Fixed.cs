using System;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Runtime.CompilerServices;
using LfrlAnvil.Exceptions;
using LfrlAnvil.Extensions;

namespace LfrlAnvil.Numerics;

// TODO: add Partition method & Partition by providing range of percentages?
public readonly struct Fixed : IEquatable<Fixed>, IComparable<Fixed>, IComparable, IFormattable
{
    public const byte MinPrecision = 0;
    public const byte MaxPrecision = 18;
    public static readonly Fixed Zero = new Fixed( 0, MinPrecision );
    public static readonly Fixed MaxValue = new Fixed( long.MaxValue, MinPrecision );
    public static readonly Fixed MinValue = new Fixed( long.MinValue, MinPrecision );
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
        Assume.IsLessThanOrEqualTo( precision, MaxPrecision, nameof( precision ) );
        RawValue = rawValue;
        Precision = precision;
    }

    public long RawValue { get; }
    public byte Precision { get; }

    [Pure]
    public static Fixed CreateZero(byte precision)
    {
        return CreateRaw( 0, precision );
    }

    [Pure]
    public static Fixed CreateMaxValue(byte precision)
    {
        return CreateRaw( long.MaxValue, precision );
    }

    [Pure]
    public static Fixed CreateMinValue(byte precision)
    {
        return CreateRaw( long.MinValue, precision );
    }

    [Pure]
    public static Fixed CreateEpsilon(byte precision)
    {
        return CreateRaw( 1, precision );
    }

    [Pure]
    public static Fixed CreateRaw(long rawValue, byte precision)
    {
        EnsureCorrectPrecision( precision );
        return new Fixed( rawValue, precision );
    }

    [Pure]
    public static Fixed Create(long value, byte precision = MinPrecision)
    {
        EnsureCorrectPrecision( precision );
        var rawValue = GetRawValue( value, precision );
        return new Fixed( rawValue, precision );
    }

    [Pure]
    public static Fixed Create(decimal value, byte precision)
    {
        EnsureCorrectPrecision( precision );
        var rawValue = GetRawValue( value, precision );
        return new Fixed( rawValue, precision );
    }

    [Pure]
    public static Fixed Create(double value, byte precision)
    {
        EnsureCorrectPrecision( precision );
        var rawValue = GetRawValue( value, precision );
        return new Fixed( rawValue, precision );
    }

    [Pure]
    public static long GetScale(byte precision)
    {
        return PowersOfTen[precision];
    }

    [Pure]
    public override string ToString()
    {
        return ToString( NumberFormatInfo.CurrentInfo );
    }

    [Pure]
    public string ToString(IFormatProvider? formatProvider)
    {
        return ToString( $"N{Precision}", formatProvider );
    }

    [Pure]
    public string ToString(string? format, IFormatProvider? formatProvider)
    {
        return ((decimal)this).ToString( format, formatProvider );
    }

    [Pure]
    public override int GetHashCode()
    {
        return RawValue.GetHashCode();
    }

    [Pure]
    public override bool Equals(object? obj)
    {
        return obj is Fixed f && Equals( f );
    }

    [Pure]
    public int CompareTo(object? obj)
    {
        return obj is Fixed f ? CompareTo( f ) : 1;
    }

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

    [Pure]
    public Fixed Abs()
    {
        return new Fixed( Math.Abs( RawValue ), Precision );
    }

    [Pure]
    public Fixed Truncate()
    {
        var fractionalPart = RawValue % PowersOfTen[Precision];
        return new Fixed( RawValue - fractionalPart, Precision );
    }

    [Pure]
    public Fixed Round(int precision)
    {
        if ( precision >= Precision )
            return this;

        Ensure.IsGreaterThanOrEqualTo( precision, 0, nameof( precision ) );
        var precisionDelta = Precision - precision;
        var rawValue = GetRoundedRawValue( RawValue, precisionDelta );
        return new Fixed( rawValue, Precision );
    }

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

    [Pure]
    public Fixed Negate()
    {
        return new Fixed( checked( -RawValue ), Precision );
    }

    [Pure]
    public Fixed SetRawValue(long rawValue)
    {
        return new Fixed( rawValue, Precision );
    }

    [Pure]
    public Fixed SetValue(long value)
    {
        var rawValue = GetRawValue( value, Precision );
        return new Fixed( rawValue, Precision );
    }

    [Pure]
    public Fixed SetValue(double value)
    {
        var rawValue = GetRawValue( value, Precision );
        return new Fixed( rawValue, Precision );
    }

    [Pure]
    public Fixed SetValue(decimal value)
    {
        var rawValue = GetRawValue( value, Precision );
        return new Fixed( rawValue, Precision );
    }

    [Pure]
    public Fixed SetPrecision(byte precision)
    {
        if ( Precision == precision )
            return this;

        EnsureCorrectPrecision( precision );
        return Precision > precision ? DecreasePrecision( precision ) : IncreasePrecision( precision );
    }

    [Pure]
    public Fixed AddRaw(long rawValue)
    {
        return new Fixed( checked( RawValue + rawValue ), Precision );
    }

    [Pure]
    public Fixed SubtractRaw(long rawValue)
    {
        return new Fixed( checked( RawValue - rawValue ), Precision );
    }

    [Pure]
    public Fixed MultiplyRaw(long rawValue)
    {
        return new Fixed( checked( RawValue * rawValue ), Precision );
    }

    [Pure]
    public Fixed DivideRaw(long rawValue)
    {
        return new Fixed( RawValue / rawValue, Precision );
    }

    [Pure]
    public Fixed ModuloRaw(long rawValue)
    {
        return new Fixed( RawValue % rawValue, Precision );
    }

    [Pure]
    public Fixed Increment()
    {
        return new Fixed( checked( RawValue + PowersOfTen[Precision] ), Precision );
    }

    [Pure]
    public Fixed Decrement()
    {
        return new Fixed( checked( RawValue - PowersOfTen[Precision] ), Precision );
    }

    [Pure]
    public Fixed Add(Fixed other)
    {
        if ( Precision == other.Precision )
            return AddRaw( other.RawValue );

        return Precision > other.Precision
            ? AddRaw( other.IncreasePrecisionRaw( Precision ) )
            : IncreasePrecision( other.Precision ).AddRaw( other.RawValue );
    }

    [Pure]
    public Fixed Subtract(Fixed other)
    {
        if ( Precision == other.Precision )
            return SubtractRaw( other.RawValue );

        return Precision > other.Precision
            ? SubtractRaw( other.IncreasePrecisionRaw( Precision ) )
            : IncreasePrecision( other.Precision ).SubtractRaw( other.RawValue );
    }

    [Pure]
    public Fixed Multiply(Fixed other)
    {
        if ( Precision == other.Precision )
            return MultiplyInternal( RawValue, other.RawValue, Precision );

        return Precision > other.Precision
            ? MultiplyInternal( RawValue, other.IncreasePrecisionRaw( Precision ), Precision )
            : MultiplyInternal( IncreasePrecisionRaw( other.Precision ), other.RawValue, other.Precision );
    }

    [Pure]
    public Fixed Divide(Fixed other)
    {
        if ( Precision == other.Precision )
            return DivideInternal( RawValue, other.RawValue, Precision );

        return Precision > other.Precision
            ? DivideInternal( RawValue, other.IncreasePrecisionRaw( Precision ), Precision )
            : DivideInternal( IncreasePrecisionRaw( other.Precision ), other.RawValue, other.Precision );
    }

    [Pure]
    public Fixed Modulo(Fixed other)
    {
        if ( Precision == other.Precision )
            return ModuloRaw( other.RawValue );

        return Precision > other.Precision
            ? ModuloRaw( other.IncreasePrecisionRaw( Precision ) )
            : IncreasePrecision( other.Precision ).ModuloRaw( other.RawValue );
    }

    [Pure]
    public static explicit operator decimal(Fixed f)
    {
        var result = (decimal)f.RawValue / PowersOfTen[f.Precision];
        return result;
    }

    [Pure]
    public static explicit operator double(Fixed f)
    {
        var result = (double)f.RawValue / PowersOfTen[f.Precision];
        return result;
    }

    [Pure]
    public static explicit operator long(Fixed f)
    {
        var result = f.RawValue / PowersOfTen[f.Precision];
        return result;
    }

    [Pure]
    public static Fixed operator -(Fixed f)
    {
        return f.Negate();
    }

    [Pure]
    public static Fixed operator ++(Fixed f)
    {
        return f.Increment();
    }

    [Pure]
    public static Fixed operator --(Fixed f)
    {
        return f.Decrement();
    }

    [Pure]
    public static Fixed operator +(Fixed left, Fixed right)
    {
        return left.Add( right );
    }

    [Pure]
    public static Fixed operator -(Fixed left, Fixed right)
    {
        return left.Subtract( right );
    }

    [Pure]
    public static Fixed operator *(Fixed left, Fixed right)
    {
        return left.Multiply( right );
    }

    [Pure]
    public static Fixed operator /(Fixed left, Fixed right)
    {
        return left.Divide( right );
    }

    [Pure]
    public static Fixed operator %(Fixed left, Fixed right)
    {
        return left.Modulo( right );
    }

    [Pure]
    public static Fixed operator +(Fixed left, Percent right)
    {
        return new Fixed( left.RawValue + right, left.Precision );
    }

    [Pure]
    public static Fixed operator -(Fixed left, Percent right)
    {
        return new Fixed( left.RawValue - right, left.Precision );
    }

    [Pure]
    public static Fixed operator *(Fixed left, Percent right)
    {
        return new Fixed( left.RawValue * right, left.Precision );
    }

    [Pure]
    public static Fixed operator /(Fixed left, Percent right)
    {
        return new Fixed( left.RawValue / right, left.Precision );
    }

    [Pure]
    public static bool operator ==(Fixed left, Fixed right)
    {
        return left.Equals( right );
    }

    [Pure]
    public static bool operator !=(Fixed left, Fixed right)
    {
        return ! left.Equals( right );
    }

    [Pure]
    public static bool operator <(Fixed left, Fixed right)
    {
        return left.CompareTo( right ) < 0;
    }

    [Pure]
    public static bool operator <=(Fixed left, Fixed right)
    {
        return left.CompareTo( right ) <= 0;
    }

    [Pure]
    public static bool operator >(Fixed left, Fixed right)
    {
        return left.CompareTo( right ) > 0;
    }

    [Pure]
    public static bool operator >=(Fixed left, Fixed right)
    {
        return left.CompareTo( right ) >= 0;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static void EnsureCorrectPrecision(byte precision)
    {
        Ensure.IsInRange( precision, MinPrecision, MaxPrecision, nameof( precision ) );
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
        Assume.IsInRange( precision, MinPrecision, Precision - 1, nameof( precision ) );
        var precisionDelta = Precision - precision;
        var rawValue = GetRoundedRawValue( RawValue, precisionDelta ) / PowersOfTen[precisionDelta];
        return rawValue;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private long IncreasePrecisionRaw(byte precision)
    {
        Assume.IsInRange( precision, Precision + 1, MaxPrecision, nameof( precision ) );
        var precisionDelta = precision - Precision;
        var rawValue = checked( RawValue * PowersOfTen[precisionDelta] );
        return rawValue;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static long GetRawValue(long value, byte precision)
    {
        Assume.IsLessThanOrEqualTo( precision, MaxPrecision, nameof( precision ) );
        var rawValue = checked( value * PowersOfTen[precision] );
        return rawValue;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static long GetRawValue(decimal value, byte precision)
    {
        Assume.IsLessThanOrEqualTo( precision, MaxPrecision, nameof( precision ) );
        var integerInputPart = Math.Truncate( value );
        var integerPart = checked( (long)integerInputPart * PowersOfTen[precision] );
        var fractionalInputPart = Math.Round( value - integerInputPart, precision, MidpointRounding.AwayFromZero );
        var fractionalPart = (long)(fractionalInputPart * PowersOfTen[precision]);
        var rawValue = checked( integerPart + fractionalPart );
        return rawValue;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static long GetRawValue(double value, byte precision)
    {
        Assume.IsLessThanOrEqualTo( precision, MaxPrecision, nameof( precision ) );
        var integerInputPart = Math.Truncate( value );
        var integerPart = checked( (long)integerInputPart * PowersOfTen[precision] );
        var fractionalInputPart = Math.Round( value - integerInputPart, precision, MidpointRounding.AwayFromZero );
        var fractionalPart = (long)(fractionalInputPart * PowersOfTen[precision]);
        var rawValue = checked( integerPart + fractionalPart );
        return rawValue;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static long GetRoundedRawValue(long rawValue, int precisionDelta)
    {
        Assume.IsGreaterThan( precisionDelta, 0, nameof( precisionDelta ) );
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
            (var resultOverflow, unsignedResult, remainder) = MathUtils.BigDivU128( resultHigh, resultLow, (ulong)PowersOfTen[precision] );
            if ( resultOverflow > 0 )
                ExceptionThrower.Throw( new OverflowException() );
        }
        else
            (unsignedResult, remainder) = Math.DivRem( resultLow, (ulong)PowersOfTen[precision] );

        if ( remainder > 0 && remainder >= (ulong)PowersOfTen[precision] >> 1 )
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
        var (leftHigh, leftLow) = MathUtils.BigMulU128( left, (ulong)PowersOfTen[precision] );

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
