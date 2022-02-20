using System;
using System.Diagnostics.Contracts;
using LfrlAnvil.Internal;

namespace LfrlAnvil.Functional
{
    public static class TypeCast
    {
        [Pure]
        public static Type? GetUnderlyingSourceType(Type? type)
        {
            var result = UnderlyingType.GetForType( type, typeof( TypeCast<,> ) );
            return result.Length == 0 ? null : result[0];
        }

        [Pure]
        public static Type? GetUnderlyingDestinationType(Type? type)
        {
            var result = UnderlyingType.GetForType( type, typeof( TypeCast<,> ) );
            return result.Length == 0 ? null : result[1];
        }

        [Pure]
        public static Pair<Type, Type>? GetUnderlyingTypes(Type? type)
        {
            var result = UnderlyingType.GetForType( type, typeof( TypeCast<,> ) );
            return result.Length == 0 ? null : Pair.Create( result[0], result[1] );
        }
    }
}
