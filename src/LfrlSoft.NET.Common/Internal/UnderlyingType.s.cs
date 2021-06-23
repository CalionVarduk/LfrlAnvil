using System;

namespace LfrlSoft.NET.Common.Internal
{
    public static class UnderlyingType
    {
        public static Type[] GetForType(Type? type, Type? targetType)
        {
            if ( type is null || ! type.IsGenericType )
                return Array.Empty<Type>();

            var openType = type.IsGenericTypeDefinition ? type : type.GetGenericTypeDefinition();
            return openType == targetType ? type.GetGenericArguments() : Array.Empty<Type>();
        }
    }
}
