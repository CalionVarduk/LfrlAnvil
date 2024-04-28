using System;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Internal;

/// <summary>
/// An internal helper class for extracting underlying types from generic classes and structs.
/// </summary>
public static class UnderlyingType
{
    /// <summary>
    /// Returns generic arguments of the provided <paramref name="type"/>,
    /// if it is a generic type closed over open generic <paramref name="targetType"/>.
    /// </summary>
    /// <param name="type">Type to extract generic arguments from.</param>
    /// <param name="targetType">Open generic type that the provided <paramref name="type"/> should close over.</param>
    /// <returns>
    /// Generic arguments of <paramref name="type"/>,
    /// or an empty array when <paramref name="type"/> is null or does not close over <paramref name="targetType"/>.
    /// </returns>
    [Pure]
    public static Type[] GetForType(Type? type, Type? targetType)
    {
        if ( type is null || ! type.IsGenericType )
            return Type.EmptyTypes;

        var openType = type.IsGenericTypeDefinition ? type : type.GetGenericTypeDefinition();
        return openType == targetType ? type.GetGenericArguments() : Type.EmptyTypes;
    }
}
