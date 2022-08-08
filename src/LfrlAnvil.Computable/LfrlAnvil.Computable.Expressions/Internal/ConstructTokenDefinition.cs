using System;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using LfrlAnvil.Computable.Expressions.Constructs;

namespace LfrlAnvil.Computable.Expressions.Internal;

internal sealed class ConstructTokenDefinition
{
    private ConstructTokenDefinition(
        BinaryOperatorCollection binaryOperators,
        UnaryOperatorCollection prefixUnaryOperators,
        UnaryOperatorCollection postfixUnaryOperators,
        TypeConverterCollection prefixTypeConverters,
        TypeConverterCollection postfixTypeConverters,
        ParsedExpressionConstant? constant,
        Type? typeDeclaration,
        ConstructTokenType type)
    {
        BinaryOperators = binaryOperators;
        PrefixUnaryOperators = prefixUnaryOperators;
        PostfixUnaryOperators = postfixUnaryOperators;
        PrefixTypeConverters = prefixTypeConverters;
        PostfixTypeConverters = postfixTypeConverters;
        Constant = constant?.Expression;
        TypeDeclaration = typeDeclaration;
        Type = type;
    }

    internal BinaryOperatorCollection BinaryOperators { get; }
    internal UnaryOperatorCollection PrefixUnaryOperators { get; }
    internal UnaryOperatorCollection PostfixUnaryOperators { get; }
    internal TypeConverterCollection PrefixTypeConverters { get; }
    internal TypeConverterCollection PostfixTypeConverters { get; }
    internal ConstantExpression? Constant { get; }
    internal Type? TypeDeclaration { get; }
    internal ConstructTokenType Type { get; }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal bool IsAny(ConstructTokenType type)
    {
        return (Type & type) != ConstructTokenType.None;
    }

    [Pure]
    internal static ConstructTokenDefinition CreateOperator(
        BinaryOperatorCollection binary,
        UnaryOperatorCollection prefixUnary,
        UnaryOperatorCollection postfixUnary)
    {
        var type = ConstructTokenType.None;

        if ( ! binary.IsEmpty )
            type |= ConstructTokenType.BinaryOperator;

        if ( ! prefixUnary.IsEmpty )
            type |= ConstructTokenType.PrefixUnaryOperator;

        if ( ! postfixUnary.IsEmpty )
            type |= ConstructTokenType.PostfixUnaryOperator;

        return new ConstructTokenDefinition(
            binary,
            prefixUnary,
            postfixUnary,
            TypeConverterCollection.Empty,
            TypeConverterCollection.Empty,
            constant: null,
            typeDeclaration: null,
            type );
    }

    [Pure]
    internal static ConstructTokenDefinition CreateTypeConverter(
        TypeConverterCollection prefix,
        TypeConverterCollection postfix)
    {
        var type = ConstructTokenType.None;

        if ( ! prefix.IsEmpty )
            type |= ConstructTokenType.PrefixTypeConverter;

        if ( ! postfix.IsEmpty )
            type |= ConstructTokenType.PostfixTypeConverter;

        return new ConstructTokenDefinition(
            BinaryOperatorCollection.Empty,
            UnaryOperatorCollection.Empty,
            UnaryOperatorCollection.Empty,
            prefix,
            postfix,
            constant: null,
            typeDeclaration: null,
            type );
    }

    [Pure]
    internal static ConstructTokenDefinition CreateConstant(ParsedExpressionConstant? constant)
    {
        return new ConstructTokenDefinition(
            BinaryOperatorCollection.Empty,
            UnaryOperatorCollection.Empty,
            UnaryOperatorCollection.Empty,
            TypeConverterCollection.Empty,
            TypeConverterCollection.Empty,
            constant,
            typeDeclaration: null,
            ConstructTokenType.Constant );
    }

    [Pure]
    internal static ConstructTokenDefinition CreateTypeDeclaration(Type? type)
    {
        return new ConstructTokenDefinition(
            BinaryOperatorCollection.Empty,
            UnaryOperatorCollection.Empty,
            UnaryOperatorCollection.Empty,
            TypeConverterCollection.Empty,
            TypeConverterCollection.Empty,
            constant: null,
            typeDeclaration: type,
            ConstructTokenType.TypeDeclaration );
    }
}
