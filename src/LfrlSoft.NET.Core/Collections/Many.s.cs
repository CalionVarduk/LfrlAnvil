using System;
using System.Diagnostics.Contracts;
using LfrlSoft.NET.Core.Internal;

namespace LfrlSoft.NET.Core.Collections
{
    public static class Many
    {
        [Pure]
        public static Many<T> Create<T>(params T[] values)
        {
            return new Many<T>( values );
        }

        [Pure]
        public static Type? GetUnderlyingType(Type? type)
        {
            var result = UnderlyingType.GetForType( type, typeof( Many<> ) );
            return result.Length == 0 ? null : result[0];
        }
    }
}
