using System;
using LfrlSoft.NET.Common.Internal;

namespace LfrlSoft.NET.Common.Collections
{
    public static class One
    {
        public static One<T> Create<T>(T value)
        {
            return new One<T>( value );
        }

        public static Type? GetUnderlyingType(Type? type)
        {
            var result = UnderlyingType.GetForType( type, typeof( One<> ) );
            return result.Length == 0 ? null : result[0];
        }
    }
}
