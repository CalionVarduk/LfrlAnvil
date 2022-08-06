using System.Linq.Expressions;
using LfrlAnvil.Mathematical.Expressions.Internal;
using LfrlAnvil.TestExtensions;

namespace LfrlAnvil.Mathematical.Expressions.Tests.ConstructsTests;

public abstract class ConstructsTestsBase : TestsBase
{
    protected static MathExpressionOperandStack CreateStack(params Expression[] operands)
    {
        var stack = new MathExpressionOperandStack();
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
