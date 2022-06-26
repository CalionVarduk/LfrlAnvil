using System.Collections.Generic;
using System.Linq.Expressions;

namespace LfrlAnvil.Mathematical.Expressions.Tokens;

public abstract class MathExpressionOperator : IMathExpressionToken
{
    protected MathExpressionOperator(string symbol)
    {
        Symbol = symbol; // TODO: validate (depends on math expression builder configuration)
    }

    public string Symbol { get; }

    // TODO: specializations for specific input types, that can override the default generic behavior
    public abstract void Process(Stack<Expression> operandStack);
}
