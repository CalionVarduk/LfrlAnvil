using LfrlAnvil.Computable.Expressions.Internal;

namespace LfrlAnvil.Computable.Expressions.Constructs;

public interface IParsedExpressionConstruct
{
    void Process(ParsedExpressionOperandStack operandStack);
}
