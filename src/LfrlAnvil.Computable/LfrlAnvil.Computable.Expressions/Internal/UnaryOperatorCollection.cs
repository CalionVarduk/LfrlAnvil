using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LfrlAnvil.Computable.Expressions.Constructs;

namespace LfrlAnvil.Computable.Expressions.Internal;

internal sealed class UnaryOperatorCollection
{
    internal static readonly UnaryOperatorCollection Empty = new UnaryOperatorCollection( null, null, int.MaxValue );

    internal UnaryOperatorCollection(
        ParsedExpressionUnaryOperator? genericConstruct,
        IReadOnlyDictionary<Type, ParsedExpressionTypedUnaryOperator>? specializedConstructs,
        int precedence)
    {
        GenericConstruct = genericConstruct;
        SpecializedConstructs = specializedConstructs;
        Precedence = precedence;
    }

    internal ParsedExpressionUnaryOperator? GenericConstruct { get; }
    internal IReadOnlyDictionary<Type, ParsedExpressionTypedUnaryOperator>? SpecializedConstructs { get; }
    internal int Precedence { get; }
    internal bool IsEmpty => GenericConstruct is null && SpecializedConstructs is null;

    [Pure]
    internal ParsedExpressionUnaryOperator? FindConstruct(Type argumentType)
    {
        if ( SpecializedConstructs is null )
            return GenericConstruct;

        var specialized = SpecializedConstructs.GetValueOrDefault( argumentType );
        return specialized ?? GenericConstruct;
    }
}
