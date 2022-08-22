using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using LfrlAnvil.Computable.Expressions.Exceptions;

namespace LfrlAnvil.Computable.Expressions.Constructs.Variadic;

public sealed class ParsedExpressionIf : ParsedExpressionVariadicFunction
{
    [Pure]
    protected internal override Expression Process(IReadOnlyList<Expression> parameters)
    {
        Ensure.ContainsExactly( parameters, count: 3, nameof( parameters ) );

        var test = parameters[0];
        var ifTrue = parameters[1];
        var ifFalse = parameters[2];

        var result = test.NodeType == ExpressionType.Constant
            ? CreateFromConstantTest( (ConstantExpression)test, ifTrue, ifFalse )
            : Expression.Condition( test, ifTrue, ifFalse );

        return result;
    }

    [Pure]
    private static Expression CreateFromConstantTest(ConstantExpression test, Expression ifTrue, Expression ifFalse)
    {
        if ( test.Type != typeof( bool ) )
            throw new ArgumentException( Resources.IfTestMustBeOfBooleanType( test.Type ), nameof( test ) );

        var testValue = (bool)test.Value!;
        return testValue ? ifTrue : ifFalse;
    }
}
