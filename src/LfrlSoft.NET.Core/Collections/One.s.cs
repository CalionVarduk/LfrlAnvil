using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlSoft.NET.Core.Internal;

namespace LfrlSoft.NET.Core.Collections
{
    public static class One
    {
        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static One<T> Create<T>(T value)
        {
            return new One<T>( value );
        }

        [Pure]
        public static Type? GetUnderlyingType(Type? type)
        {
            var result = UnderlyingType.GetForType( type, typeof( One<> ) );
            return result.Length == 0 ? null : result[0];
        }
    }
}
