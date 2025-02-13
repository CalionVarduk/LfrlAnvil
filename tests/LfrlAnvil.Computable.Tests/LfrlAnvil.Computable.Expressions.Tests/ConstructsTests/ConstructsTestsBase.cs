using System.Diagnostics.Contracts;
using System.Linq.Expressions;

namespace LfrlAnvil.Computable.Expressions.Tests.ConstructsTests;

public abstract class ConstructsTestsBase : TestsBase
{
    [Pure]
    protected static Expression CreateConstantOperand<T>(T value)
    {
        return Expression.Constant( value, typeof( T ) );
    }

    [Pure]
    protected static Expression CreateVariableOperand<T>(string name)
    {
        return Expression.Parameter( typeof( T ), name );
    }
}
