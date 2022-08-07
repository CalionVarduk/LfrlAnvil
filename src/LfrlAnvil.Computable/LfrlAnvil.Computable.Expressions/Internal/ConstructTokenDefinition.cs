using System.Diagnostics.Contracts;
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
        ConstructTokenType type)
    {
        BinaryOperators = binaryOperators;
        PrefixUnaryOperators = prefixUnaryOperators;
        PostfixUnaryOperators = postfixUnaryOperators;
        PrefixTypeConverters = prefixTypeConverters;
        PostfixTypeConverters = postfixTypeConverters;
        Constant = constant;
        Type = type;
    }

    internal BinaryOperatorCollection BinaryOperators { get; }
    internal UnaryOperatorCollection PrefixUnaryOperators { get; }
    internal UnaryOperatorCollection PostfixUnaryOperators { get; }
    internal TypeConverterCollection PrefixTypeConverters { get; }
    internal TypeConverterCollection PostfixTypeConverters { get; }
    internal ParsedExpressionConstant? Constant { get; }
    internal ConstructTokenType Type { get; }

    [Pure]
    internal static ConstructTokenDefinition CreateOperator(
        BinaryOperatorCollection binary,
        UnaryOperatorCollection prefixUnary,
        UnaryOperatorCollection postfixUnary)
    {
        return new ConstructTokenDefinition(
            binary,
            prefixUnary,
            postfixUnary,
            TypeConverterCollection.Empty,
            TypeConverterCollection.Empty,
            constant: null,
            ConstructTokenType.Operator );
    }

    [Pure]
    internal static ConstructTokenDefinition CreateTypeConverter(
        TypeConverterCollection prefix,
        TypeConverterCollection postfix)
    {
        return new ConstructTokenDefinition(
            BinaryOperatorCollection.Empty,
            UnaryOperatorCollection.Empty,
            UnaryOperatorCollection.Empty,
            prefix,
            postfix,
            constant: null,
            ConstructTokenType.TypeConverter );
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
            ConstructTokenType.Constant );
    }
}
