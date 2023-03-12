using System.Linq.Expressions;
using LfrlAnvil.Computable.Expressions.Constructs;
using LfrlAnvil.Functional;

namespace LfrlAnvil.Computable.Expressions.Tests.ConstructsTests;

public abstract class UnaryOperatorsTestsBase : ConstructsTestsBase
{
    protected static void Process_ShouldPopOneOperandAndPushOneExpression_WhenOperandIsVariable<TArg, TResult>(
        ParsedExpressionUnaryOperator sut,
        ExpressionType expectedNodeType,
        Action<Expression, Expression> nodeAssertion)
    {
        var operand = CreateVariableOperand<TArg>( "value" );

        var result = sut.Process( operand );

        using ( new AssertionScope() )
        {
            result.NodeType.Should().Be( expectedNodeType );
            result.Type.Should().Be( typeof( TResult ) );
            nodeAssertion( operand, result );
        }
    }

    protected static void Process_ShouldPopOneOperandAndPushOneExpression_WhenOperandIsConstant<TArg, TResult>(
        ParsedExpressionUnaryOperator sut,
        ExpressionType expectedNodeType,
        TArg operandValue,
        Action<Expression, Expression> nodeAssertion)
    {
        var operand = CreateConstantOperand( operandValue );

        var result = sut.Process( operand );

        using ( new AssertionScope() )
        {
            result.NodeType.Should().Be( expectedNodeType );
            result.Type.Should().Be( typeof( TResult ) );
            nodeAssertion( operand, result );
        }
    }

    protected static void Process_ShouldThrowException_WhenOperatorDoesNotExist<TArg, TException>(ParsedExpressionUnaryOperator sut)
        where TException : Exception
    {
        var operand = CreateVariableOperand<TArg>( "value" );
        var action = Lambda.Of( () => sut.Process( operand ) );
        action.Should().ThrowExactly<TException>();
    }

    protected static void DefaultNodeAssertion(Expression operand, Expression result)
    {
        result.Should().BeAssignableTo<UnaryExpression>();
        if ( result is not UnaryExpression unaryResult )
            return;

        unaryResult.Operand.Should().BeSameAs( operand );
    }
}
