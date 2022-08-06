using LfrlAnvil.Mathematical.Expressions.Internal;

namespace LfrlAnvil.Mathematical.Expressions.Constructs;

public interface IMathExpressionConstruct
{
    void Process(MathExpressionOperandStack operandStack);
}
