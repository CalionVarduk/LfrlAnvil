using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LfrlAnvil.Computable.Expressions.Constructs;

namespace LfrlAnvil.Computable.Expressions.Internal;

internal sealed class TypeConverterCollection
{
    internal static readonly TypeConverterCollection Empty = new TypeConverterCollection( null, null, null, int.MaxValue );

    private readonly ParsedExpressionTypeConverter? _genericConstruct;
    private readonly Dictionary<Type, ParsedExpressionTypeConverter>? _specializedConstructs;

    internal TypeConverterCollection(
        Type? targetType,
        ParsedExpressionTypeConverter? genericConstruct,
        Dictionary<Type, ParsedExpressionTypeConverter>? specializedConstructs,
        int precedence)
    {
        TargetType = targetType;
        _genericConstruct = genericConstruct;
        _specializedConstructs = specializedConstructs;
        Precedence = precedence;
    }

    internal Type? TargetType { get; }
    internal int Precedence { get; }
    internal bool IsEmpty => TargetType is null;

    [Pure]
    internal ParsedExpressionTypeConverter? FindConstruct(Type sourceType)
    {
        if ( _specializedConstructs is null )
            return _genericConstruct;

        var specialized = _specializedConstructs.GetValueOrDefault( sourceType );
        return specialized ?? _genericConstruct;
    }
}
