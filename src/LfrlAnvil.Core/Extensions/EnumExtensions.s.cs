using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Extensions
{
    public static class EnumExtensions
    {
        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Bitmask<T> ToBitmask<T>(this T value)
            where T : struct, Enum
        {
            return new Bitmask<T>( value );
        }
    }
}
