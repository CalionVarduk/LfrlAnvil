using LfrlAnvil.Mathematical.Expressions.Tokens;

namespace LfrlAnvil.Mathematical.Expressions.Internal
{
    internal sealed class MathExpressionTokenSet
    {
        public MathExpressionTokenSet(
            MathExpressionBinaryOperator? binaryOperator,
            MathExpressionUnaryOperator? prefixUnaryOperator,
            MathExpressionUnaryOperator? postfixUnaryOperator)
        {
            BinaryOperator = binaryOperator;
            PrefixUnaryOperator = prefixUnaryOperator;
            PostfixUnaryOperator = postfixUnaryOperator;
        }

        internal MathExpressionBinaryOperator? BinaryOperator { get; }
        internal MathExpressionUnaryOperator? PrefixUnaryOperator { get; }
        internal MathExpressionUnaryOperator? PostfixUnaryOperator { get; }
    }
}
