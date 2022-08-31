using System;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Internal;

public static class UnderlyingType
{
    [Pure]
    public static Type[] GetForType(Type? type, Type? targetType)
    {
        if ( type is null || ! type.IsGenericType )
            return Type.EmptyTypes;

        var openType = type.IsGenericTypeDefinition ? type : type.GetGenericTypeDefinition();
        return openType == targetType ? type.GetGenericArguments() : Type.EmptyTypes;
    }
}
