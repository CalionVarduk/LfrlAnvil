using System.Linq.Expressions;

namespace LfrlAnvil.Computable.Expressions.Tests.ConstructsTests;

public abstract class ConstructsTestsBase : TestsBase
{
    protected static Expression CreateConstantOperand<T>(T value)
    {
        return Expression.Constant( value, typeof( T ) );
    }

    protected static Expression CreateVariableOperand<T>(string name)
    {
        return Expression.Parameter( typeof( T ), name );
    }
}
