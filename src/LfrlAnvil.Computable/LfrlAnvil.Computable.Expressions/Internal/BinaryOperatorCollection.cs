using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LfrlAnvil.Computable.Expressions.Constructs;

namespace LfrlAnvil.Computable.Expressions.Internal;

internal sealed class BinaryOperatorCollection
{
    internal static readonly BinaryOperatorCollection Empty = new BinaryOperatorCollection( null, null, int.MaxValue );

    internal BinaryOperatorCollection(
        ParsedExpressionBinaryOperator? genericConstruct,
        IReadOnlyDictionary<(Type Left, Type Right), ParsedExpressionTypedBinaryOperator>? specializedConstructs,
        int precedence)
    {
        GenericConstruct = genericConstruct;
        SpecializedConstructs = specializedConstructs;
        Precedence = precedence;
    }

    internal ParsedExpressionBinaryOperator? GenericConstruct { get; }
    internal IReadOnlyDictionary<(Type Left, Type Right), ParsedExpressionTypedBinaryOperator>? SpecializedConstructs { get; }
    internal int Precedence { get; }
    internal bool IsEmpty => GenericConstruct is null && SpecializedConstructs is null;

    [Pure]
    internal ParsedExpressionBinaryOperator? FindConstruct(Type leftArgumentType, Type rightArgumentType)
    {
        if ( SpecializedConstructs is null )
            return GenericConstruct;

        var specialized = SpecializedConstructs.GetValueOrDefault( (leftArgumentType, rightArgumentType) );
        return specialized ?? GenericConstruct;
    }
}
