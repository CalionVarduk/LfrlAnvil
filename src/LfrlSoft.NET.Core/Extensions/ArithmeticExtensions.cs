using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlSoft.NET.Core.Extensions
{
    public static class ArithmeticExtensions
    {
        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static long EuclidModulo(this long a, long b)
        {
            var r = a % b;
            return r < 0 ? r + b : r;
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
        public static short EuclidModulo(this short a, short b)
        {
            return (short) ((int) a).EuclidModulo( b );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static sbyte EuclidModulo(this sbyte a, sbyte b)
        {
            return (sbyte) ((int) a).EuclidModulo( b );
        }
    }
}
