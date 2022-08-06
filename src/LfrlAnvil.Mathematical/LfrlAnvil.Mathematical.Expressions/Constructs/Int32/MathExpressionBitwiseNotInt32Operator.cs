using System.Linq.Expressions;

namespace LfrlAnvil.Mathematical.Expressions.Constructs.Int32;

public sealed class MathExpressionBitwiseNotInt32Operator : MathExpressionUnaryOperator<int>
{
    protected override Expression? TryCreateFromConstant(ConstantExpression operand)
    {
        return TryGetArgumentValue( operand, out var value )
            ? Expression.Constant( ~value )
            : null;
    }

    protected override Expression CreateUnaryExpression(Expression operand)
    {
        return Expression.Not( operand );
    }
}
