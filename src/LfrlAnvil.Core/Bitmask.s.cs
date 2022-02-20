using System;
using System.Diagnostics.Contracts;
using LfrlAnvil.Internal;

namespace LfrlAnvil
{
    public static class Bitmask
    {
        [Pure]
        public static Bitmask<T> Create<T>(T value)
            where T : struct, IConvertible, IComparable
        {
            return new Bitmask<T>( value );
        }

        [Pure]
        public static Type? GetUnderlyingType(Type? type)
        {
            var result = UnderlyingType.GetForType( type, typeof( Bitmask<> ) );
            return result.Length == 0 ? null : result[0];
        }
    }
}
