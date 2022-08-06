using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LfrlAnvil.Mathematical.Expressions.Constructs;

namespace LfrlAnvil.Mathematical.Expressions.Internal;

internal sealed class MathExpressionTypeConverterCollection
{
    internal static readonly MathExpressionTypeConverterCollection Empty =
        new MathExpressionTypeConverterCollection( null, null, null, int.MaxValue );

    private readonly MathExpressionTypeConverter? _genericConstruct;
    private readonly Dictionary<Type, MathExpressionTypeConverter>? _specializedConstructs;

    internal MathExpressionTypeConverterCollection(
        Type? targetType,
        MathExpressionTypeConverter? genericConstruct,
        Dictionary<Type, MathExpressionTypeConverter>? specializedConstructs,
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
    internal MathExpressionTypeConverter? FindConstruct(Type sourceType)
    {
        if ( _specializedConstructs is null )
            return _genericConstruct;

        var specialized = _specializedConstructs.GetValueOrDefault( sourceType );
        return specialized ?? _genericConstruct;
    }
}
