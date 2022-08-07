using System.Linq.Expressions;
using LfrlAnvil.Computable.Expressions.Internal;
using LfrlAnvil.TestExtensions;

namespace LfrlAnvil.Computable.Expressions.Tests.ConstructsTests;

public abstract class ConstructsTestsBase : TestsBase
{
    protected static ParsedExpressionOperandStack CreateStack(params Expression[] operands)
    {
        var stack = new ParsedExpressionOperandStack();
        foreach ( var o in operands )
            stack.Push( o );

        return stack;
    }

    protected static Expression CreateConstantOperand<T>(T value)
    {
        return Expression.Constant( value, typeof( T ) );
    }

    protected static Expression CreateVariableOperand<T>(string name)
    {
        return Expression.Parameter( typeof( T ), name );
    }
}
