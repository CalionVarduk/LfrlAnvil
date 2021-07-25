using System;

namespace LfrlSoft.NET.Core.Extensions
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
