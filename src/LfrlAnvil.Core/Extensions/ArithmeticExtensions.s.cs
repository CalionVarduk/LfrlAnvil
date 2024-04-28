using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Exceptions;

namespace LfrlAnvil.Extensions;

/// <summary>
/// Contains arithmetic extension methods.
/// </summary>
public static class ArithmeticExtensions
{
    /// <summary>
    /// Calculates the remainder of a euclidean division of <paramref name="a"/> by <paramref name="b"/>.
    /// </summary>
    /// <param name="a">Dividend.</param>
    /// <param name="b">Divisor.</param>
    /// <returns><see cref="Single"/> result of euclidean <paramref name="a"/> / <paramref name="b"/>.</returns>
    /// <exception cref="DivideByZeroException">When <paramref name="b"/> is equal to <b>0</b>.</exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static float EuclidModulo(this float a, float b)
    {
        if ( b == 0 )
            ExceptionThrower.Throw( new DivideByZeroException( ExceptionResources.DividedByZero ) );

        var r = a % b;
        return r < 0 ? r + b : r;
    }

    /// <summary>
    /// Calculates the remainder of a euclidean division of <paramref name="a"/> by <paramref name="b"/>.
    /// </summary>
    /// <param name="a">Dividend.</param>
    /// <param name="b">Divisor.</param>
    /// <returns><see cref="Double"/> result of euclidean <paramref name="a"/> / <paramref name="b"/>.</returns>
    /// <exception cref="DivideByZeroException">When <paramref name="b"/> is equal to <b>0</b>.</exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static double EuclidModulo(this double a, double b)
    {
        if ( b == 0 )
            ExceptionThrower.Throw( new DivideByZeroException( ExceptionResources.DividedByZero ) );

        var r = a % b;
        return r < 0 ? r + b : r;
    }

    /// <summary>
    /// Calculates the remainder of a euclidean division of <paramref name="a"/> by <paramref name="b"/>.
    /// </summary>
    /// <param name="a">Dividend.</param>
    /// <param name="b">Divisor.</param>
    /// <returns><see cref="Decimal"/> result of euclidean <paramref name="a"/> / <paramref name="b"/>.</returns>
    /// <exception cref="DivideByZeroException">When <paramref name="b"/> is equal to <b>0</b>.</exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static decimal EuclidModulo(this decimal a, decimal b)
    {
        var r = a % b;
        return r < 0 ? r + b : r;
    }

    /// <summary>
    /// Calculates the remainder of a euclidean division of <paramref name="a"/> by <paramref name="b"/>.
    /// </summary>
    /// <param name="a">Dividend.</param>
    /// <param name="b">Divisor.</param>
    /// <returns><see cref="UInt64"/> result of euclidean <paramref name="a"/> / <paramref name="b"/>.</returns>
    /// <exception cref="DivideByZeroException">When <paramref name="b"/> is equal to <b>0</b>.</exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ulong EuclidModulo(this ulong a, ulong b)
    {
        return a % b;
    }

    /// <summary>
    /// Calculates the remainder of a euclidean division of <paramref name="a"/> by <paramref name="b"/>.
    /// </summary>
    /// <param name="a">Dividend.</param>
    /// <param name="b">Divisor.</param>
    /// <returns><see cref="Int64"/> result of euclidean <paramref name="a"/> / <paramref name="b"/>.</returns>
    /// <exception cref="DivideByZeroException">When <paramref name="b"/> is equal to <b>0</b>.</exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static long EuclidModulo(this long a, long b)
    {
        var r = a % b;
        return r < 0 ? r + b : r;
    }

    /// <summary>
    /// Calculates the remainder of a euclidean division of <paramref name="a"/> by <paramref name="b"/>.
    /// </summary>
    /// <param name="a">Dividend.</param>
    /// <param name="b">Divisor.</param>
    /// <returns><see cref="UInt32"/> result of euclidean <paramref name="a"/> / <paramref name="b"/>.</returns>
    /// <exception cref="DivideByZeroException">When <paramref name="b"/> is equal to <b>0</b>.</exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static uint EuclidModulo(this uint a, uint b)
    {
        return a % b;
    }

    /// <summary>
    /// Calculates the remainder of a euclidean division of <paramref name="a"/> by <paramref name="b"/>.
    /// </summary>
    /// <param name="a">Dividend.</param>
    /// <param name="b">Divisor.</param>
    /// <returns><see cref="Int32"/> result of euclidean <paramref name="a"/> / <paramref name="b"/>.</returns>
    /// <exception cref="DivideByZeroException">When <paramref name="b"/> is equal to <b>0</b>.</exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static int EuclidModulo(this int a, int b)
    {
        var r = a % b;
        return r < 0 ? r + b : r;
    }

    /// <summary>
    /// Calculates the remainder of a euclidean division of <paramref name="a"/> by <paramref name="b"/>.
    /// </summary>
    /// <param name="a">Dividend.</param>
    /// <param name="b">Divisor.</param>
    /// <returns><see cref="UInt16"/> result of euclidean <paramref name="a"/> / <paramref name="b"/>.</returns>
    /// <exception cref="DivideByZeroException">When <paramref name="b"/> is equal to <b>0</b>.</exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ushort EuclidModulo(this ushort a, ushort b)
    {
        return ( ushort )(( uint )a).EuclidModulo( b );
    }

    /// <summary>
    /// Calculates the remainder of a euclidean division of <paramref name="a"/> by <paramref name="b"/>.
    /// </summary>
    /// <param name="a">Dividend.</param>
    /// <param name="b">Divisor.</param>
    /// <returns><see cref="Int16"/> result of euclidean <paramref name="a"/> / <paramref name="b"/>.</returns>
    /// <exception cref="DivideByZeroException">When <paramref name="b"/> is equal to <b>0</b>.</exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static short EuclidModulo(this short a, short b)
    {
        return ( short )(( int )a).EuclidModulo( b );
    }

    /// <summary>
    /// Calculates the remainder of a euclidean division of <paramref name="a"/> by <paramref name="b"/>.
    /// </summary>
    /// <param name="a">Dividend.</param>
    /// <param name="b">Divisor.</param>
    /// <returns><see cref="Byte"/> result of euclidean <paramref name="a"/> / <paramref name="b"/>.</returns>
    /// <exception cref="DivideByZeroException">When <paramref name="b"/> is equal to <b>0</b>.</exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static byte EuclidModulo(this byte a, byte b)
    {
        return ( byte )(( uint )a).EuclidModulo( b );
    }

    /// <summary>
    /// Calculates the remainder of a euclidean division of <paramref name="a"/> by <paramref name="b"/>.
    /// </summary>
    /// <param name="a">Dividend.</param>
    /// <param name="b">Divisor.</param>
    /// <returns><see cref="SByte"/> result of euclidean <paramref name="a"/> / <paramref name="b"/>.</returns>
    /// <exception cref="DivideByZeroException">When <paramref name="b"/> is equal to <b>0</b>.</exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static sbyte EuclidModulo(this sbyte a, sbyte b)
    {
        return ( sbyte )(( int )a).EuclidModulo( b );
    }

    /// <summary>
    /// Checks if the provided <see cref="UInt64"/> value is even.
    /// </summary>
    /// <param name="x">Value to check.</param>
    /// <returns><b>true</b> when <paramref name="x"/> is even, otherwise <b>false</b>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool IsEven(this ulong x)
    {
        return (x & 1) == 0;
    }

    /// <summary>
    /// Checks if the provided <see cref="UInt64"/> value is odd.
    /// </summary>
    /// <param name="x">Value to check.</param>
    /// <returns><b>true</b> when <paramref name="x"/> is odd, otherwise <b>false</b>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool IsOdd(this ulong x)
    {
        return ! x.IsEven();
    }

    /// <summary>
    /// Checks if the provided <see cref="Int64"/> value is even.
    /// </summary>
    /// <param name="x">Value to check.</param>
    /// <returns><b>true</b> when <paramref name="x"/> is even, otherwise <b>false</b>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool IsEven(this long x)
    {
        return (x & 1) == 0;
    }

    /// <summary>
    /// Checks if the provided <see cref="Int64"/> value is odd.
    /// </summary>
    /// <param name="x">Value to check.</param>
    /// <returns><b>true</b> when <paramref name="x"/> is odd, otherwise <b>false</b>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool IsOdd(this long x)
    {
        return ! x.IsEven();
    }

    /// <summary>
    /// Checks if the provided <see cref="UInt32"/> value is even.
    /// </summary>
    /// <param name="x">Value to check.</param>
    /// <returns><b>true</b> when <paramref name="x"/> is even, otherwise <b>false</b>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool IsEven(this uint x)
    {
        return (x & 1) == 0;
    }

    /// <summary>
    /// Checks if the provided <see cref="UInt32"/> value is odd.
    /// </summary>
    /// <param name="x">Value to check.</param>
    /// <returns><b>true</b> when <paramref name="x"/> is odd, otherwise <b>false</b>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool IsOdd(this uint x)
    {
        return ! x.IsEven();
    }

    /// <summary>
    /// Checks if the provided <see cref="Int32"/> value is even.
    /// </summary>
    /// <param name="x">Value to check.</param>
    /// <returns><b>true</b> when <paramref name="x"/> is even, otherwise <b>false</b>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool IsEven(this int x)
    {
        return (x & 1) == 0;
    }

    /// <summary>
    /// Checks if the provided <see cref="Int32"/> value is odd.
    /// </summary>
    /// <param name="x">Value to check.</param>
    /// <returns><b>true</b> when <paramref name="x"/> is odd, otherwise <b>false</b>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool IsOdd(this int x)
    {
        return ! x.IsEven();
    }

    /// <summary>
    /// Checks if the provided <see cref="UInt16"/> value is even.
    /// </summary>
    /// <param name="x">Value to check.</param>
    /// <returns><b>true</b> when <paramref name="x"/> is even, otherwise <b>false</b>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool IsEven(this ushort x)
    {
        return (x & 1) == 0;
    }

    /// <summary>
    /// Checks if the provided <see cref="UInt16"/> value is odd.
    /// </summary>
    /// <param name="x">Value to check.</param>
    /// <returns><b>true</b> when <paramref name="x"/> is odd, otherwise <b>false</b>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool IsOdd(this ushort x)
    {
        return ! x.IsEven();
    }

    /// <summary>
    /// Checks if the provided <see cref="Int16"/> value is even.
    /// </summary>
    /// <param name="x">Value to check.</param>
    /// <returns><b>true</b> when <paramref name="x"/> is even, otherwise <b>false</b>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool IsEven(this short x)
    {
        return (x & 1) == 0;
    }

    /// <summary>
    /// Checks if the provided <see cref="Int16"/> value is odd.
    /// </summary>
    /// <param name="x">Value to check.</param>
    /// <returns><b>true</b> when <paramref name="x"/> is odd, otherwise <b>false</b>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool IsOdd(this short x)
    {
        return ! x.IsEven();
    }

    /// <summary>
    /// Checks if the provided <see cref="Byte"/> value is even.
    /// </summary>
    /// <param name="x">Value to check.</param>
    /// <returns><b>true</b> when <paramref name="x"/> is even, otherwise <b>false</b>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool IsEven(this byte x)
    {
        return (x & 1) == 0;
    }

    /// <summary>
    /// Checks if the provided <see cref="Byte"/> value is odd.
    /// </summary>
    /// <param name="x">Value to check.</param>
    /// <returns><b>true</b> when <paramref name="x"/> is odd, otherwise <b>false</b>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool IsOdd(this byte x)
    {
        return ! x.IsEven();
    }

    /// <summary>
    /// Checks if the provided <see cref="SByte"/> value is even.
    /// </summary>
    /// <param name="x">Value to check.</param>
    /// <returns><b>true</b> when <paramref name="x"/> is even, otherwise <b>false</b>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool IsEven(this sbyte x)
    {
        return (x & 1) == 0;
    }

    /// <summary>
    /// Checks if the provided <see cref="SByte"/> value is odd.
    /// </summary>
    /// <param name="x">Value to check.</param>
    /// <returns><b>true</b> when <paramref name="x"/> is odd, otherwise <b>false</b>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool IsOdd(this sbyte x)
    {
        return ! x.IsEven();
    }
}
