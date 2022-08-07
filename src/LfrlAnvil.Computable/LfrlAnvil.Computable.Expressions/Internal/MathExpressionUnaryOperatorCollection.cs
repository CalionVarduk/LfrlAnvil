using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LfrlAnvil.Computable.Expressions.Constructs;

namespace LfrlAnvil.Computable.Expressions.Internal;

internal sealed class MathExpressionUnaryOperatorCollection
{
    internal static readonly MathExpressionUnaryOperatorCollection Empty =
        new MathExpressionUnaryOperatorCollection( null, null, int.MaxValue );

    private readonly MathExpressionUnaryOperator? _genericConstruct;
    private readonly Dictionary<Type, MathExpressionTypedUnaryOperator>? _specializedConstructs;

    internal MathExpressionUnaryOperatorCollection(
        MathExpressionUnaryOperator? genericConstruct,
        Dictionary<Type, MathExpressionTypedUnaryOperator>? specializedConstructs,
        int precedence)
    {
        _genericConstruct = genericConstruct;
        _specializedConstructs = specializedConstructs;
        Precedence = precedence;
    }

    internal int Precedence { get; }
    internal bool IsEmpty => _genericConstruct is null && _specializedConstructs is null;

    [Pure]
    internal MathExpressionUnaryOperator? FindConstruct(Type argumentType)
    {
        if ( _specializedConstructs is null )
            return _genericConstruct;

        var specialized = _specializedConstructs.GetValueOrDefault( argumentType );
        return specialized ?? _genericConstruct;
    }
}
