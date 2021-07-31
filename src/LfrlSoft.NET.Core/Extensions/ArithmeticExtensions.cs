using System.Diagnostics.Contracts;

namespace LfrlSoft.NET.Core.Extensions
{
    public static class ArithmeticExtensions
    {
        [Pure]
        public static long EuclidModulo(this long a, long b)
        {
            var r = a % b;
            return r < 0 ? r + b : r;
        }

        [Pure]
        public static int EuclidModulo(this int a, int b)
        {
            var r = a % b;
            return r < 0 ? r + b : r;
        }

        [Pure]
        public static short EuclidModulo(this short a, short b)
        {
            return (short) ((int) a).EuclidModulo( b );
        }

        [Pure]
        public static sbyte EuclidModulo(this sbyte a, sbyte b)
        {
            return (sbyte) ((int) a).EuclidModulo( b );
        }
    }
}
