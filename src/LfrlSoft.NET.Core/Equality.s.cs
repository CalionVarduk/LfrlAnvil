using System;
using System.Diagnostics.Contracts;
using LfrlSoft.NET.Core.Internal;

namespace LfrlSoft.NET.Core
{
    public static class Equality
    {
        [Pure]
        public static Equality<T> Create<T>(T? first, T? second)
        {
            return new Equality<T>( first, second );
        }

        [Pure]
        public static Type? GetUnderlyingType(Type? type)
        {
            var result = UnderlyingType.GetForType( type, typeof( Equality<> ) );
            return result.Length == 0 ? null : result[0];
        }
    }
}
