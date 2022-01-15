using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlSoft.NET.Core.Extensions
{
    public static class ArithmeticExtensions
    {
        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static float EuclidModulo(this float a, float b)
        {
            if ( b == 0 )
                throw new DivideByZeroException( "Attempted to divide by zero." );

            var r = a % b;
            return r < 0 ? r + b : r;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static double EuclidModulo(this double a, double b)
        {
            if ( b == 0 )
                throw new DivideByZeroException( "Attempted to divide by zero." );

            var r = a % b;
            return r < 0 ? r + b : r;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static decimal EuclidModulo(this decimal a, decimal b)
        {
            var r = a % b;
            return r < 0 ? r + b : r;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static ulong EuclidModulo(this ulong a, ulong b)
        {
            return a % b;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static long EuclidModulo(this long a, long b)
        {
            var r = a % b;
            return r < 0 ? r + b : r;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static uint EuclidModulo(this uint a, uint b)
        {
            return a % b;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static int EuclidModulo(this int a, int b)
        {
            var r = a % b;
            return r < 0 ? r + b : r;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static ushort EuclidModulo(this ushort a, ushort b)
        {
            return (ushort)((uint)a).EuclidModulo( b );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static short EuclidModulo(this short a, short b)
        {
            return (short)((int)a).EuclidModulo( b );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static byte EuclidModulo(this byte a, byte b)
        {
            return (byte)((uint)a).EuclidModulo( b );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static sbyte EuclidModulo(this sbyte a, sbyte b)
        {
            return (sbyte)((int)a).EuclidModulo( b );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static bool IsEven(this ulong x)
        {
            return (x & 1) == 0;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static bool IsOdd(this ulong x)
        {
            return ! x.IsEven();
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static bool IsEven(this long x)
        {
            return (x & 1) == 0;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static bool IsOdd(this long x)
        {
            return ! x.IsEven();
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static bool IsEven(this uint x)
        {
            return (x & 1) == 0;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static bool IsOdd(this uint x)
        {
            return ! x.IsEven();
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static bool IsEven(this int x)
        {
            return (x & 1) == 0;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static bool IsOdd(this int x)
        {
            return ! x.IsEven();
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static bool IsEven(this ushort x)
        {
            return (x & 1) == 0;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static bool IsOdd(this ushort x)
        {
            return ! x.IsEven();
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static bool IsEven(this short x)
        {
            return (x & 1) == 0;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static bool IsOdd(this short x)
        {
            return ! x.IsEven();
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static bool IsEven(this byte x)
        {
            return (x & 1) == 0;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static bool IsOdd(this byte x)
        {
            return ! x.IsEven();
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static bool IsEven(this sbyte x)
        {
            return (x & 1) == 0;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static bool IsOdd(this sbyte x)
        {
            return ! x.IsEven();
        }
    }
}
