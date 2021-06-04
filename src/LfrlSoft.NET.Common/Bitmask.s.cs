using System;
using LfrlSoft.NET.Common.Internal;

namespace LfrlSoft.NET.Common
{
    public static class Bitmask
    {
        public static Bitmask<T> Create<T>(T value)
            where T : struct, IConvertible, IComparable
        {
            return new Bitmask<T>( value );
        }

        public static Type? GetUnderlyingType(Type? type)
        {
            var result = UnderlyingType.GetForType( type, typeof( Bitmask<> ) );
            return result.Length == 0 ? null : result[0];
        }
    }
}
