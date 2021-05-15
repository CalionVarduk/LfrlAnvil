using System;

namespace LfrlSoft.NET.Common.Internal
{
    public static class UnderlyingTypeCheck
    {
        public static bool IsValidForType(Type type, Type targetType)
        {
            if ( type is null || !type.IsGenericType )
                return false;

            if ( type.IsGenericTypeDefinition )
                return type == targetType;

            return type.GetGenericTypeDefinition() == targetType;
        }
    }
}
