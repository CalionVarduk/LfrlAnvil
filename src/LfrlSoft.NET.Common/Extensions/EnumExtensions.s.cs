using System;

namespace LfrlSoft.NET.Common.Extensions
{
    public static class EnumExtensions
    {
        public static Bitmask<T> ToBitmask<T>(this T value)
            where T : struct, Enum
        {
            return new Bitmask<T>( value );
        }
    }
}
