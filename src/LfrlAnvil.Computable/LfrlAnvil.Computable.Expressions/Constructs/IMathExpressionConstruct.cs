using LfrlAnvil.Computable.Expressions.Internal;

namespace LfrlAnvil.Computable.Expressions.Constructs;

public interface IMathExpressionConstruct
{
    void Process(MathExpressionOperandStack operandStack);
}
