using System;
using System.Diagnostics.Contracts;
using System.Numerics;
using System.Runtime.CompilerServices;
using LfrlAnvil.Exceptions;

namespace LfrlAnvil.Numerics;

public static class MathUtils
{
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ulong ToUnsigned(long value, ref int sign)
    {
        if ( value < 0 )
        {
            sign = -sign;
            value = unchecked( -value );
        }

        return unchecked( (ulong)value );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static long ToSigned(ulong value, int sign)
    {
        if ( sign >= 0 )
            return checked( (long)value );

        if ( value > (ulong)long.MaxValue + 1 )
            ExceptionThrower.Throw( new OverflowException() );

        return unchecked( -(long)value );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static (ulong High, ulong Low) BigMulU128(ulong left, ulong right)
    {
        var high = Math.BigMul( left, right, out var low );
        return (high, low);
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static (ulong High, ulong Low, int Sign) BigMul128(long left, long right)
    {
        var sign = 1;
        var l = ToUnsigned( left, ref sign );
        var r = ToUnsigned( right, ref sign );
        var high = Math.BigMul( l, r, out var low );
        return (high, low, sign);
    }

    [Pure]
    public static (ulong QuotientHigh, ulong QuotientLow, ulong Remainder) BigDivU128(ulong leftHigh, ulong leftLow, uint right)
    {
        if ( right == 0 )
            ExceptionThrower.Throw( new DivideByZeroException( ExceptionResources.DividedByZero ) );

        var left0 = unchecked( (uint)leftLow );
        var left1 = unchecked( (uint)(leftLow >> 32) );
        var left2 = unchecked( (uint)leftHigh );
        var left3 = unchecked( (uint)(leftHigh >> 32) );

        var shift = BitOperations.LeadingZeroCount( right );
        var backShift = 32 - shift;
        var div = right << shift;

        var val = ((ulong)left3 << shift) | ((ulong)left2 >> backShift);
        var digit = Math.Min( val / div, uint.MaxValue );
        while ( div * digit > val )
            --digit;

        left3 = GetFixedDividendDigit( left3, right, 0, ref digit );
        var quotientHigh = digit << 32;

        val = ((((ulong)left3 << 32) | left2) << shift) | ((ulong)left1 >> backShift);
        digit = Math.Min( val / div, uint.MaxValue );
        while ( div * digit > val )
            --digit;

        left2 = GetFixedDividendDigit( left2, right, left3, ref digit );
        quotientHigh |= unchecked( (uint)digit );

        val = ((((ulong)left2 << 32) | left1) << shift) | ((ulong)left0 >> backShift);
        digit = Math.Min( val / div, uint.MaxValue );
        while ( div * digit > val )
            --digit;

        left1 = GetFixedDividendDigit( left1, right, left2, ref digit );
        var quotientLow = digit << 32;

        val = (((ulong)left1 << 32) | left0) << shift;
        digit = Math.Min( val / div, uint.MaxValue );
        while ( div * digit > val )
            --digit;

        left0 = GetFixedDividendDigit( left0, right, left1, ref digit );
        quotientLow |= unchecked( (uint)digit );

        return (quotientHigh, quotientLow, left0);

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        static uint GetFixedDividendDigit(uint left, uint right, uint prevLeft, ref ulong digit)
        {
            if ( digit == 0 )
                return left;

            var carry = right * digit;
            var subDigit = unchecked( (uint)carry );
            carry >>= 32;
            if ( left < subDigit )
                ++carry;

            left = unchecked( left - subDigit );
            carry = unchecked( (uint)carry );

            if ( carry == prevLeft )
                return left;

            Assume.Equals( carry, prevLeft + 1, nameof( carry ) );

            var addDigit = (ulong)left + right;
            left = unchecked( (uint)addDigit );
            carry = unchecked( (uint)(addDigit >> 32) );

            Assume.Equals( carry, 1U, nameof( carry ) );

            --digit;
            return left;
        }
    }

    [Pure]
    public static (ulong QuotientHigh, ulong QuotientLow, ulong Remainder) BigDivU128(ulong leftHigh, ulong leftLow, ulong right)
    {
        // NOTE: adapted from System.Numerics.BigInteger division implementation
        var right1 = unchecked( (uint)(right >> 32) );

        if ( right1 == 0 )
            return BigDivU128( leftHigh, leftLow, unchecked( (uint)right ) );

        var right0 = unchecked( (uint)right );

        var left0 = unchecked( (uint)leftLow );
        var left1 = unchecked( (uint)(leftLow >> 32) );
        var left2 = unchecked( (uint)leftHigh );
        var left3 = unchecked( (uint)(leftHigh >> 32) );

        var shift = BitOperations.LeadingZeroCount( right1 );
        var backShift = 32 - shift;

        var divHigh = (right1 << shift) | unchecked( (uint)((ulong)right0 >> backShift) );
        var divLow = right0 << shift;

        var valHigh = ((ulong)left3 << shift) | ((ulong)left2 >> backShift);
        var valLow = (left2 << shift) | unchecked( (uint)((ulong)left1 >> backShift) );
        var digit = Math.Min( valHigh / divHigh, uint.MaxValue );
        while ( IsQuotientDigitTooBig( digit, valHigh, valLow, divHigh, divLow ) )
            --digit;

        FixDividendDigits( ref left2, ref left3, right0, right1, 0, ref digit );
        var quotientHigh = (ulong)unchecked( (uint)digit );

        valHigh = ((((ulong)left3 << 32) | left2) << shift) | ((ulong)left1 >> backShift);
        valLow = (left1 << shift) | unchecked( (uint)((ulong)left0 >> backShift) );
        digit = Math.Min( valHigh / divHigh, uint.MaxValue );
        while ( IsQuotientDigitTooBig( digit, valHigh, valLow, divHigh, divLow ) )
            --digit;

        FixDividendDigits( ref left1, ref left2, right0, right1, left3, ref digit );
        var quotientLow = digit << 32;

        valHigh = ((((ulong)left2 << 32) | left1) << shift) | ((ulong)left0 >> backShift);
        valLow = left0 << shift;
        digit = Math.Min( valHigh / divHigh, uint.MaxValue );
        while ( IsQuotientDigitTooBig( digit, valHigh, valLow, divHigh, divLow ) )
            --digit;

        FixDividendDigits( ref left0, ref left1, right0, right1, left2, ref digit );
        quotientLow |= unchecked( (uint)digit );

        var remainder = ((ulong)left1 << 32) | left0;
        return (quotientHigh, quotientLow, remainder);

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        static bool IsQuotientDigitTooBig(ulong digit, ulong valHigh, uint valLow, uint divHigh, uint divLow)
        {
            Assume.IsLessThanOrEqualTo( digit, uint.MaxValue, nameof( digit ) );

            var checkHigh = divHigh * digit;
            var checkLow = divLow * digit;

            checkHigh += checkLow >> 32;
            checkLow &= uint.MaxValue;

            if ( checkHigh < valHigh )
                return false;

            return checkHigh > valHigh || checkLow > valLow;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        static void FixDividendDigits(ref uint left0, ref uint left1, uint right0, uint right1, uint prevLeft, ref ulong digit)
        {
            if ( digit == 0 )
                return;

            var carry = right0 * digit;
            var subDigit = unchecked( (uint)carry );
            carry >>= 32;
            if ( left0 < subDigit )
                ++carry;

            left0 = unchecked( left0 - subDigit );

            carry += right1 * digit;
            subDigit = unchecked( (uint)carry );
            carry >>= 32;
            if ( left1 < subDigit )
                ++carry;

            left1 = unchecked( left1 - subDigit );
            carry = unchecked( (uint)carry );

            if ( carry == prevLeft )
                return;

            Assume.Equals( carry, prevLeft + 1, nameof( carry ) );

            var addDigit = (ulong)left0 + right0;
            left0 = unchecked( (uint)addDigit );

            addDigit = left1 + (addDigit >> 32) + right1;
            left1 = unchecked( (uint)addDigit );
            carry = unchecked( (uint)(addDigit >> 32) );

            Assume.Equals( carry, 1U, nameof( carry ) );
            --digit;
        }
    }
}
