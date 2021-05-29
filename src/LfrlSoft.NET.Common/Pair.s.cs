using System;
using LfrlSoft.NET.Common.Internal;

namespace LfrlSoft.NET.Common
{
    public static class Pair
    {
        public static Pair<T1, T2> Create<T1, T2>(T1 first, T2 second)
        {
            return new Pair<T1, T2>( first, second );
        }

        public static Type? GetUnderlyingFirstType(Type? type)
        {
            var result = UnderlyingType.GetForType( type, typeof( Pair<,> ) );
            return result.Length == 0 ? null : result[0];
        }

        public static Type? GetUnderlyingSecondType(Type? type)
        {
            var result = UnderlyingType.GetForType( type, typeof( Pair<,> ) );
            return result.Length == 0 ? null : result[1];
        }

        public static Pair<Type, Type>? GetUnderlyingTypes(Type? type)
        {
            var result = UnderlyingType.GetForType( type, typeof( Pair<,> ) );
            return result.Length == 0 ? null : Create( result[0], result[1] );
        }
    }
}
