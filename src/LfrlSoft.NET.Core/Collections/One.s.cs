using System;
using LfrlSoft.NET.Core.Internal;

namespace LfrlSoft.NET.Core.Collections
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
