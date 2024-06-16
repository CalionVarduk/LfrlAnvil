// Copyright 2024 Łukasz Furlepa
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using LfrlAnvil.Exceptions;
using LfrlAnvil.Extensions;

namespace LfrlAnvil.Numerics;

/// <summary>
/// Contains helper math-related methods.
/// </summary>
public static class MathUtils
{
    /// <summary>
    /// Calculates an absolute value from the given parameter and converts it to <see cref="UInt64"/>.
    /// </summary>
    /// <param name="value">Value to calculate an absolute value from.</param>
    /// <returns>Absolute value as <see cref="UInt64"/>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ulong UnsignedAbs(long value)
    {
        if ( value < 0 )
            value = unchecked( -value );

        return unchecked( ( ulong )value );
    }

    /// <summary>
    /// Converts the provided <see cref="Int64"/> <paramref name="value"/> to <see cref="UInt64"/>
    /// and stores its <paramref name="sign"/> in a separate variable.
    /// </summary>
    /// <param name="value">Value to convert.</param>
    /// <param name="sign"><b>ref</b> parameter that gets negated when the provided <paramref name="value"/> is negative.</param>
    /// <returns>Absolute value of the provided parameter as <see cref="UInt64"/>.</returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ulong ToUnsigned(long value, ref int sign)
    {
        if ( value < 0 )
        {
            sign = -sign;
            value = unchecked( -value );
        }

        return unchecked( ( ulong )value );
    }

    /// <summary>
    /// Converts the provided <see cref="UInt64"/> <paramref name="value"/> to <see cref="Int64"/>
    /// by using the <paramref name="sign"/> stored in a separate variable.
    /// </summary>
    /// <param name="value">Value to convert.</param>
    /// <param name="sign">Expected result's sign.</param>
    /// <returns>
    /// Provided <paramref name="value"/> converted to <see cref="Int64"/>.
    /// Result will be negated when the provided <paramref name="sign"/> is less than <b>0</b>.
    /// </returns>
    /// <exception cref="OverflowException">When the result causes an arithmetic overflow.</exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static long ToSigned(ulong value, int sign)
    {
        if ( sign >= 0 )
            return checked( ( long )value );

        if ( value > ( ulong )long.MaxValue + 1 )
            ExceptionThrower.Throw( new OverflowException() );

        return unchecked( -( long )value );
    }

    /// <summary>
    /// Multiples two <see cref="UInt64"/> values.
    /// </summary>
    /// <param name="left">First operand.</param>
    /// <param name="right">Second operand.</param>
    /// <returns>Tuple representing multiplication result.</returns>
    /// <remarks>See <see cref="Math.BigMul(UInt64,UInt64,out UInt64)"/> for more information.</remarks>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static (ulong High, ulong Low) BigMulU128(ulong left, ulong right)
    {
        var high = Math.BigMul( left, right, out var low );
        return (high, low);
    }

    /// <summary>
    /// Divides a 128-bit unsigned value by <see cref="UInt32"/> value.
    /// </summary>
    /// <param name="leftHigh">High 64 bits of the dividend.</param>
    /// <param name="leftLow">Low 64 bits of the dividend.</param>
    /// <param name="right">32-bit divisor.</param>
    /// <returns>Tuple representing division result as quotient and remainder.</returns>
    /// <exception cref="DivideByZeroException">When the divisor is equal to <b>0</b>.</exception>
    [Pure]
    public static (ulong QuotientHigh, ulong QuotientLow, ulong Remainder) BigDivU128(ulong leftHigh, ulong leftLow, uint right)
    {
        if ( right == 0 )
            ExceptionThrower.Throw( new DivideByZeroException( ExceptionResources.DividedByZero ) );

        var left0 = unchecked( ( uint )leftLow );
        var left1 = unchecked( ( uint )(leftLow >> 32) );
        var left2 = unchecked( ( uint )leftHigh );
        var left3 = unchecked( ( uint )(leftHigh >> 32) );

        var shift = BitOperations.LeadingZeroCount( right );
        var backShift = 32 - shift;
        var div = right << shift;

        var val = (( ulong )left3 << shift) | (( ulong )left2 >> backShift);
        var digit = Math.Min( val / div, uint.MaxValue );
        while ( div * digit > val )
            --digit;

        left3 = GetFixedDividendDigit( left3, right, 0, ref digit );
        var quotientHigh = digit << 32;

        val = (((( ulong )left3 << 32) | left2) << shift) | (( ulong )left1 >> backShift);
        digit = Math.Min( val / div, uint.MaxValue );
        while ( div * digit > val )
            --digit;

        left2 = GetFixedDividendDigit( left2, right, left3, ref digit );
        quotientHigh |= unchecked( ( uint )digit );

        val = (((( ulong )left2 << 32) | left1) << shift) | (( ulong )left0 >> backShift);
        digit = Math.Min( val / div, uint.MaxValue );
        while ( div * digit > val )
            --digit;

        left1 = GetFixedDividendDigit( left1, right, left2, ref digit );
        var quotientLow = digit << 32;

        val = ((( ulong )left1 << 32) | left0) << shift;
        digit = Math.Min( val / div, uint.MaxValue );
        while ( div * digit > val )
            --digit;

        left0 = GetFixedDividendDigit( left0, right, left1, ref digit );
        quotientLow |= unchecked( ( uint )digit );

        return (quotientHigh, quotientLow, left0);

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        static uint GetFixedDividendDigit(uint left, uint right, uint prevLeft, ref ulong digit)
        {
            if ( digit == 0 )
                return left;

            var carry = right * digit;
            var subDigit = unchecked( ( uint )carry );
            carry >>= 32;
            if ( left < subDigit )
                ++carry;

            left = unchecked( left - subDigit );
            carry = unchecked( ( uint )carry );

            if ( carry == prevLeft )
                return left;

            Assume.Equals( carry, prevLeft + 1 );

            var addDigit = ( ulong )left + right;
            left = unchecked( ( uint )addDigit );
            carry = unchecked( ( uint )(addDigit >> 32) );

            Assume.Equals( carry, 1U );

            --digit;
            return left;
        }
    }

    /// <summary>
    /// Divides a 128-bit unsigned value by <see cref="UInt64"/> value.
    /// </summary>
    /// <param name="leftHigh">High 64 bits of the dividend.</param>
    /// <param name="leftLow">Low 64 bits of the dividend.</param>
    /// <param name="right">64-bit divisor.</param>
    /// <returns>Tuple representing division result as quotient and remainder.</returns>
    /// <exception cref="DivideByZeroException">When the divisor is equal to <b>0</b>.</exception>
    [Pure]
    public static (ulong QuotientHigh, ulong QuotientLow, ulong Remainder) BigDivU128(ulong leftHigh, ulong leftLow, ulong right)
    {
        // NOTE: adapted from System.Numerics.BigInteger division implementation
        var right1 = unchecked( ( uint )(right >> 32) );

        if ( right1 == 0 )
            return BigDivU128( leftHigh, leftLow, unchecked( ( uint )right ) );

        var right0 = unchecked( ( uint )right );

        var left0 = unchecked( ( uint )leftLow );
        var left1 = unchecked( ( uint )(leftLow >> 32) );
        var left2 = unchecked( ( uint )leftHigh );
        var left3 = unchecked( ( uint )(leftHigh >> 32) );

        var shift = BitOperations.LeadingZeroCount( right1 );
        var backShift = 32 - shift;

        var divHigh = (right1 << shift) | unchecked( ( uint )(( ulong )right0 >> backShift) );
        var divLow = right0 << shift;

        var valHigh = (( ulong )left3 << shift) | (( ulong )left2 >> backShift);
        var valLow = (left2 << shift) | unchecked( ( uint )(( ulong )left1 >> backShift) );
        var digit = Math.Min( valHigh / divHigh, uint.MaxValue );
        while ( IsQuotientDigitTooBig( digit, valHigh, valLow, divHigh, divLow ) )
            --digit;

        FixDividendDigits( ref left2, ref left3, right0, right1, 0, ref digit );
        var quotientHigh = ( ulong )unchecked( ( uint )digit );

        valHigh = (((( ulong )left3 << 32) | left2) << shift) | (( ulong )left1 >> backShift);
        valLow = (left1 << shift) | unchecked( ( uint )(( ulong )left0 >> backShift) );
        digit = Math.Min( valHigh / divHigh, uint.MaxValue );
        while ( IsQuotientDigitTooBig( digit, valHigh, valLow, divHigh, divLow ) )
            --digit;

        FixDividendDigits( ref left1, ref left2, right0, right1, left3, ref digit );
        var quotientLow = digit << 32;

        valHigh = (((( ulong )left2 << 32) | left1) << shift) | (( ulong )left0 >> backShift);
        valLow = left0 << shift;
        digit = Math.Min( valHigh / divHigh, uint.MaxValue );
        while ( IsQuotientDigitTooBig( digit, valHigh, valLow, divHigh, divLow ) )
            --digit;

        FixDividendDigits( ref left0, ref left1, right0, right1, left2, ref digit );
        quotientLow |= unchecked( ( uint )digit );

        var remainder = (( ulong )left1 << 32) | left0;
        return (quotientHigh, quotientLow, remainder);

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        static bool IsQuotientDigitTooBig(ulong digit, ulong valHigh, uint valLow, uint divHigh, uint divLow)
        {
            Assume.IsLessThanOrEqualTo( digit, uint.MaxValue );

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
            var subDigit = unchecked( ( uint )carry );
            carry >>= 32;
            if ( left0 < subDigit )
                ++carry;

            left0 = unchecked( left0 - subDigit );

            carry += right1 * digit;
            subDigit = unchecked( ( uint )carry );
            carry >>= 32;
            if ( left1 < subDigit )
                ++carry;

            left1 = unchecked( left1 - subDigit );
            carry = unchecked( ( uint )carry );

            if ( carry == prevLeft )
                return;

            Assume.Equals( carry, prevLeft + 1 );

            var addDigit = ( ulong )left0 + right0;
            left0 = unchecked( ( uint )addDigit );

            addDigit = left1 + (addDigit >> 32) + right1;
            left1 = unchecked( ( uint )addDigit );
            carry = unchecked( ( uint )(addDigit >> 32) );

            Assume.Equals( carry, 1U );
            --digit;
        }
    }

    /// <summary>
    /// Calculates the GCD (greatest common divisor) of two <see cref="UInt64"/> values.
    /// </summary>
    /// <param name="a">First value.</param>
    /// <param name="b">Second value.</param>
    /// <returns>Greatest common divisor.</returns>
    /// <remarks>When the second value is equal to <b>0</b>, then this method will return the first value.</remarks>
    [Pure]
    public static ulong Gcd(ulong a, ulong b)
    {
        while ( b != 0 )
        {
            var t = a % b;
            a = b;
            b = t;
        }

        return a;
    }

    /// <summary>
    /// Calculates the LCM (least common multiple) of two <see cref="UInt64"/> values.
    /// </summary>
    /// <param name="a">First value.</param>
    /// <param name="b">Second value.</param>
    /// <returns>Least common multiple.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ulong Lcm(ulong a, ulong b)
    {
        return checked( a * (b / Gcd( a, b )) );
    }

    /// <summary>
    /// Converts the provided range of <paramref name="percentages"/> into an equivalent range of fractions,
    /// whose sum is equal to the specified <paramref name="targetSum"/> and whose denominators
    /// are all equal to the <paramref name="targetSum"/> <see cref="Fraction.Denominator"/>.
    /// </summary>
    /// <param name="percentages">Range of percentages to convert into fractions.</param>
    /// <param name="targetSum">Expected sum of resulting fractions and their <see cref="Fraction.Denominator"/> definition.</param>
    /// <returns>
    /// Range of fractions whose number is equal to the number of the provided <paramref name="percentages"/>
    /// and whose sum is equal to the <paramref name="targetSum"/>.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// When <paramref name="targetSum"/> is less than <b>0</b>
    /// or any of the provided <paramref name="percentages"/> is less than or equal to <b>0</b>.
    /// </exception>
    [Pure]
    public static Fraction[] ConvertToFractions(IEnumerable<Percent> percentages, Fraction targetSum)
    {
        Ensure.IsGreaterThanOrEqualTo( targetSum, Fraction.Zero );

        var materializedPercentages = percentages.Materialize();
        if ( materializedPercentages.Count == 0 )
            return Array.Empty<Fraction>();

        var percentageSum = Percent.Zero;
        foreach ( var percent in materializedPercentages )
        {
            Ensure.IsGreaterThan( percent, Percent.Zero );
            percentageSum += percent;
        }

        var fractions = new Fraction[materializedPercentages.Count];
        if ( targetSum.Numerator == 0 )
        {
            Array.Fill( fractions, targetSum );
            return fractions;
        }

        var index = 0;
        var numeratorSum = 0L;
        var ratioMultiplier = ( decimal )targetSum / percentageSum.Ratio;

        foreach ( var percent in materializedPercentages )
        {
            fractions[index] = Fraction.Create( percent.Ratio * ratioMultiplier, targetSum.Denominator );
            numeratorSum = checked( numeratorSum + fractions[index++].Numerator );
        }

        var roundingError = unchecked( targetSum.Numerator - numeratorSum );
        if ( roundingError < 0 )
        {
            for ( var i = 0; i < fractions.Length; ++i )
            {
                var numerator = fractions[i].Numerator;
                if ( numerator > 0 )
                {
                    fractions[i] = fractions[i].SetNumerator( numerator - 1 );
                    ++roundingError;
                }
            }
        }

        Assume.IsGreaterThanOrEqualTo( roundingError, 0 );
        if ( roundingError > 0 )
        {
            index = 0;
            var fixedPartition = new IntegerFixedPartition( unchecked( ( ulong )roundingError ), fractions.Length );
            foreach ( var part in fixedPartition )
            {
                var signedPartition = unchecked( ( long )part );
                var numerator = fractions[index].Numerator;
                numerator = checked( numerator + signedPartition );
                fractions[index] = fractions[index].SetNumerator( numerator );
                ++index;
            }
        }

        Assume.Equals( targetSum, fractions.Aggregate( Fraction.Zero, (a, b) => a + b ) );
        return fractions;
    }
}
