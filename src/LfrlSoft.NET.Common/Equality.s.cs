using LfrlSoft.NET.Common.Internal;
using System;

namespace LfrlSoft.NET.Common
{
    public static class Equality
    {
        public static Equality<T> Create<T>(T first, T second)
        {
            return new Equality<T>( first, second );
        }

        public static Type GetUnderlyingType(Type type)
        {
            var result = UnderlyingType.GetForType( type, typeof( Equality<> ) );
            return result.Length == 0 ? null : result[0];
        }
    }
}
