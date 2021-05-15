using LfrlSoft.NET.Common.Internal;
using System;

namespace LfrlSoft.NET.Common
{
    public static class Ref
    {
        public static Ref<T> Create<T>(T value)
             where T : struct
        {
            return new Ref<T>( value );
        }

        public static Type GetUnderlyingType(Type type)
        {
            var result = UnderlyingType.GetForType( type, typeof( Ref<> ) );
            return result.Length == 0 ? null : result[0];
        }
    }
}
