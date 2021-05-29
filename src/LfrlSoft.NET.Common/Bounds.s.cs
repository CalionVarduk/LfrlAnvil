using System;
using LfrlSoft.NET.Common.Internal;

namespace LfrlSoft.NET.Common
{
    public static class Bounds
    {
        public static Bounds<T> Create<T>(T min, T max)
            where T : IComparable<T>
        {
            return new Bounds<T>( min, max );
        }

        public static Type? GetUnderlyingType(Type? type)
        {
            var result = UnderlyingType.GetForType( type, typeof( Bounds<> ) );
            return result.Length == 0 ? null : result[0];
        }
    }
}
