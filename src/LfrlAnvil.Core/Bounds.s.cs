using System;
using System.Diagnostics.Contracts;
using LfrlAnvil.Internal;

namespace LfrlAnvil
{
    public static class Bounds
    {
        [Pure]
        public static Bounds<T> Create<T>(T min, T max)
            where T : IComparable<T>
        {
            return new Bounds<T>( min, max );
        }

        [Pure]
        public static Type? GetUnderlyingType(Type? type)
        {
            var result = UnderlyingType.GetForType( type, typeof( Bounds<> ) );
            return result.Length == 0 ? null : result[0];
        }
    }
}
