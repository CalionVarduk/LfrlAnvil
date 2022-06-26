using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Mathematical.Expressions.Tokens
{
    public static class MathExpressionTokenExtensions
    {
        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static MathExpressionOperator? AsOperator(this IMathExpressionToken token)
        {
            return token as MathExpressionOperator;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static MathExpressionUnaryOperator? AsUnaryOperator(this IMathExpressionToken token)
        {
            return token as MathExpressionUnaryOperator;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static MathExpressionBinaryOperator? AsBinaryOperator(this IMathExpressionToken token)
        {
            return token as MathExpressionBinaryOperator;
        }
    }
}
