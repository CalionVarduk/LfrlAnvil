using System;
using System.Diagnostics.Contracts;
using LfrlAnvil.Internal;

namespace LfrlAnvil.Functional
{
    public static class Either
    {
        [Pure]
        public static Type? GetUnderlyingFirstType(Type? type)
        {
            var result = UnderlyingType.GetForType( type, typeof( Either<,> ) );
            return result.Length == 0 ? null : result[0];
        }

        [Pure]
        public static Type? GetUnderlyingSecondType(Type? type)
        {
            var result = UnderlyingType.GetForType( type, typeof( Either<,> ) );
            return result.Length == 0 ? null : result[1];
        }

        [Pure]
        public static Pair<Type, Type>? GetUnderlyingTypes(Type? type)
        {
            var result = UnderlyingType.GetForType( type, typeof( Either<,> ) );
            return result.Length == 0 ? null : Pair.Create( result[0], result[1] );
        }
    }
}
