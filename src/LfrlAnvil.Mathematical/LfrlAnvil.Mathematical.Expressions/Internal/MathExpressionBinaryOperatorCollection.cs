using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LfrlAnvil.Mathematical.Expressions.Constructs;

namespace LfrlAnvil.Mathematical.Expressions.Internal;

internal sealed class MathExpressionBinaryOperatorCollection
{
    internal static readonly MathExpressionBinaryOperatorCollection Empty =
        new MathExpressionBinaryOperatorCollection( null, null, int.MaxValue );

    private readonly MathExpressionBinaryOperator? _genericConstruct;
    private readonly Dictionary<(Type Left, Type Right), MathExpressionTypedBinaryOperator>? _specializedConstructs;

    internal MathExpressionBinaryOperatorCollection(
        MathExpressionBinaryOperator? genericConstruct,
        Dictionary<(Type Left, Type Right), MathExpressionTypedBinaryOperator>? specializedConstructs,
        int precedence)
    {
        _genericConstruct = genericConstruct;
        _specializedConstructs = specializedConstructs;
        Precedence = precedence;
    }

    internal int Precedence { get; }
    internal bool IsEmpty => _genericConstruct is null && _specializedConstructs is null;

    [Pure]
    internal MathExpressionBinaryOperator? FindConstruct(Type leftArgumentType, Type rightArgumentType)
    {
        if ( _specializedConstructs is null )
            return _genericConstruct;

        var specialized = _specializedConstructs.GetValueOrDefault( (leftArgumentType, rightArgumentType) );
        return specialized ?? _genericConstruct;
    }
}
