using System;
using System.Diagnostics.Contracts;
using LfrlAnvil.Internal;

namespace LfrlAnvil
{
    public static class Pair
    {
        [Pure]
        public static Pair<T1, T2> Create<T1, T2>(T1 first, T2 second)
        {
            return new Pair<T1, T2>( first, second );
        }

        [Pure]
        public static Type? GetUnderlyingFirstType(Type? type)
        {
            var result = UnderlyingType.GetForType( type, typeof( Pair<,> ) );
            return result.Length == 0 ? null : result[0];
        }

        [Pure]
        public static Type? GetUnderlyingSecondType(Type? type)
        {
            var result = UnderlyingType.GetForType( type, typeof( Pair<,> ) );
            return result.Length == 0 ? null : result[1];
        }

        [Pure]
        public static Pair<Type, Type>? GetUnderlyingTypes(Type? type)
        {
            var result = UnderlyingType.GetForType( type, typeof( Pair<,> ) );
            return result.Length == 0 ? null : Create( result[0], result[1] );
        }
    }
}
