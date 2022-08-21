using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LfrlAnvil.Computable.Expressions.Constructs;

namespace LfrlAnvil.Computable.Expressions.Internal;

internal sealed class TypeConverterCollection
{
    internal static readonly TypeConverterCollection Empty = new TypeConverterCollection( null, null, null, int.MaxValue );

    internal TypeConverterCollection(
        Type? targetType,
        ParsedExpressionTypeConverter? genericConstruct,
        IReadOnlyDictionary<Type, ParsedExpressionTypeConverter>? specializedConstructs,
        int precedence)
    {
        TargetType = targetType;
        GenericConstruct = genericConstruct;
        SpecializedConstructs = specializedConstructs;
        Precedence = precedence;
    }

    internal ParsedExpressionTypeConverter? GenericConstruct { get; }
    internal IReadOnlyDictionary<Type, ParsedExpressionTypeConverter>? SpecializedConstructs { get; }
    internal Type? TargetType { get; }
    internal int Precedence { get; }
    internal bool IsEmpty => TargetType is null;

    [Pure]
    internal ParsedExpressionTypeConverter? FindConstruct(Type sourceType)
    {
        if ( SpecializedConstructs is null )
            return GenericConstruct;

        var specialized = SpecializedConstructs.GetValueOrDefault( sourceType );
        return specialized ?? GenericConstruct;
    }
}
