using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LfrlAnvil.Computable.Expressions.Constructs;

namespace LfrlAnvil.Computable.Expressions.Internal;

internal sealed class BinaryOperatorCollection
{
    internal static readonly BinaryOperatorCollection Empty = new BinaryOperatorCollection( null, null, int.MaxValue );

    private readonly ParsedExpressionBinaryOperator? _genericConstruct;
    private readonly IReadOnlyDictionary<(Type Left, Type Right), ParsedExpressionTypedBinaryOperator>? _specializedConstructs;

    internal BinaryOperatorCollection(
        ParsedExpressionBinaryOperator? genericConstruct,
        IReadOnlyDictionary<(Type Left, Type Right), ParsedExpressionTypedBinaryOperator>? specializedConstructs,
        int precedence)
    {
        _genericConstruct = genericConstruct;
        _specializedConstructs = specializedConstructs;
        Precedence = precedence;
    }

    internal int Precedence { get; }
    internal bool IsEmpty => _genericConstruct is null && _specializedConstructs is null;

    [Pure]
    internal ParsedExpressionBinaryOperator? FindConstruct(Type leftArgumentType, Type rightArgumentType)
    {
        if ( _specializedConstructs is null )
            return _genericConstruct;

        var specialized = _specializedConstructs.GetValueOrDefault( (leftArgumentType, rightArgumentType) );
        return specialized ?? _genericConstruct;
    }
}
