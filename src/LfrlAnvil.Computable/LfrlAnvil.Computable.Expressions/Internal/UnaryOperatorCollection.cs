using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LfrlAnvil.Computable.Expressions.Constructs;

namespace LfrlAnvil.Computable.Expressions.Internal;

internal sealed class UnaryOperatorCollection
{
    internal static readonly UnaryOperatorCollection Empty = new UnaryOperatorCollection( null, null, int.MaxValue );

    private readonly ParsedExpressionUnaryOperator? _genericConstruct;
    private readonly Dictionary<Type, ParsedExpressionTypedUnaryOperator>? _specializedConstructs;

    internal UnaryOperatorCollection(
        ParsedExpressionUnaryOperator? genericConstruct,
        Dictionary<Type, ParsedExpressionTypedUnaryOperator>? specializedConstructs,
        int precedence)
    {
        _genericConstruct = genericConstruct;
        _specializedConstructs = specializedConstructs;
        Precedence = precedence;
    }

    internal int Precedence { get; }
    internal bool IsEmpty => _genericConstruct is null && _specializedConstructs is null;

    [Pure]
    internal ParsedExpressionUnaryOperator? FindConstruct(Type argumentType)
    {
        if ( _specializedConstructs is null )
            return _genericConstruct;

        var specialized = _specializedConstructs.GetValueOrDefault( argumentType );
        return specialized ?? _genericConstruct;
    }
}
