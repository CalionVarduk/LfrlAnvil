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
            if ( !UnderlyingTypeCheck.IsValidForType( type, typeof( Ref<> ) ) )
                return null;

            return type.GetGenericArguments()[0];
        }
    }
}
