using System.Diagnostics.Contracts;

namespace LfrlAnvil.Computable.Expressions.Internal;

internal sealed class MathExpressionConstructTokenDefinition
{
    private MathExpressionConstructTokenDefinition(
        MathExpressionBinaryOperatorCollection binaryOperators,
        MathExpressionUnaryOperatorCollection prefixUnaryOperators,
        MathExpressionUnaryOperatorCollection postfixUnaryOperators,
        MathExpressionTypeConverterCollection prefixTypeConverters,
        MathExpressionTypeConverterCollection postfixTypeConverters,
        MathExpressionConstructTokenType type)
    {
        BinaryOperators = binaryOperators;
        PrefixUnaryOperators = prefixUnaryOperators;
        PostfixUnaryOperators = postfixUnaryOperators;
        PrefixTypeConverters = prefixTypeConverters;
        PostfixTypeConverters = postfixTypeConverters;
        Type = type;
    }

    internal MathExpressionBinaryOperatorCollection BinaryOperators { get; }
    internal MathExpressionUnaryOperatorCollection PrefixUnaryOperators { get; }
    internal MathExpressionUnaryOperatorCollection PostfixUnaryOperators { get; }
    internal MathExpressionTypeConverterCollection PrefixTypeConverters { get; }
    internal MathExpressionTypeConverterCollection PostfixTypeConverters { get; }
    internal MathExpressionConstructTokenType Type { get; }

    [Pure]
    internal static MathExpressionConstructTokenDefinition CreateOperator(
        MathExpressionBinaryOperatorCollection binary,
        MathExpressionUnaryOperatorCollection prefixUnary,
        MathExpressionUnaryOperatorCollection postfixUnary)
    {
        return new MathExpressionConstructTokenDefinition(
            binary,
            prefixUnary,
            postfixUnary,
            MathExpressionTypeConverterCollection.Empty,
            MathExpressionTypeConverterCollection.Empty,
            MathExpressionConstructTokenType.Operator );
    }

    [Pure]
    internal static MathExpressionConstructTokenDefinition CreateTypeConverter(
        MathExpressionTypeConverterCollection prefix,
        MathExpressionTypeConverterCollection postfix)
    {
        return new MathExpressionConstructTokenDefinition(
            MathExpressionBinaryOperatorCollection.Empty,
            MathExpressionUnaryOperatorCollection.Empty,
            MathExpressionUnaryOperatorCollection.Empty,
            prefix,
            postfix,
            MathExpressionConstructTokenType.TypeConverter );
    }
}
