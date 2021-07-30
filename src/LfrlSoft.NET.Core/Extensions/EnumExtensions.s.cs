using System;
using System.Diagnostics.Contracts;

namespace LfrlSoft.NET.Core.Extensions
{
    public static class EnumExtensions
    {
        [Pure]
        public static Bitmask<T> ToBitmask<T>(this T value)
            where T : struct, Enum
        {
            return new Bitmask<T>( value );
        }
    }
}
