using System.Linq.Expressions;
using FluentAssertions.Execution;
using LfrlAnvil.Computable.Expressions.Constructs;
using LfrlAnvil.Functional;

namespace LfrlAnvil.Computable.Expressions.Tests.ConstructsTests;

public abstract class BinaryOperatorsTestsBase : ConstructsTestsBase
{
    protected static void Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable<TLeftArg, TRightArg, TResult>(
        ParsedExpressionBinaryOperator sut,
        ExpressionType expectedNodeType,
        Action<Expression, Expression, Expression> nodeAssertion)
    {
        var left = CreateVariableOperand<TLeftArg>( "left" );
        var right = CreateVariableOperand<TRightArg>( "right" );

        var result = sut.Process( left, right );

        using ( new AssertionScope() )
        {
            result.NodeType.Should().Be( expectedNodeType );
            result.Type.Should().Be( typeof( TResult ) );
            nodeAssertion( left, right, result );
        }
    }

    protected static void Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant<TLeftArg, TRightArg, TResult>(
        ParsedExpressionBinaryOperator sut,
        ExpressionType expectedNodeType,
        TLeftArg leftValue,
        TRightArg rightValue,
        Action<Expression, Expression, Expression> nodeAssertion)
    {
        var left = CreateConstantOperand( leftValue );
        var right = CreateConstantOperand( rightValue );

        var result = sut.Process( left, right );

        using ( new AssertionScope() )
        {
            result.NodeType.Should().Be( expectedNodeType );
            result.Type.Should().Be( typeof( TResult ) );
            nodeAssertion( left, right, result );
        }
    }

    protected static void Process_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant<TLeftArg, TRightArg, TResult>(
        ParsedExpressionBinaryOperator sut,
        ExpressionType expectedNodeType,
        TLeftArg leftValue,
        Action<Expression, Expression, Expression> nodeAssertion)
    {
        var left = CreateConstantOperand( leftValue );
        var right = CreateVariableOperand<TRightArg>( "right" );

        var result = sut.Process( left, right );

        using ( new AssertionScope() )
        {
            result.NodeType.Should().Be( expectedNodeType );
            result.Type.Should().Be( typeof( TResult ) );
            nodeAssertion( left, right, result );
        }
    }

    protected static void Process_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant<TLeftArg, TRightArg, TResult>(
        ParsedExpressionBinaryOperator sut,
        ExpressionType expectedNodeType,
        TRightArg rightValue,
        Action<Expression, Expression, Expression> nodeAssertion)
    {
        var left = CreateVariableOperand<TLeftArg>( "left" );
        var right = CreateConstantOperand( rightValue );

        var result = sut.Process( left, right );

        using ( new AssertionScope() )
        {
            result.NodeType.Should().Be( expectedNodeType );
            result.Type.Should().Be( typeof( TResult ) );
            nodeAssertion( left, right, result );
        }
    }

    protected static void Process_ShouldThrowException_WhenOperatorDoesNotExist<TLeftArg, TRightArg, TException>(
        ParsedExpressionBinaryOperator sut)
        where TException : Exception
    {
        var left = CreateVariableOperand<TLeftArg>( "left" );
        var right = CreateVariableOperand<TRightArg>( "right" );

        var action = Lambda.Of( () => sut.Process( left, right ) );

        action.Should().ThrowExactly<TException>();
    }

    protected static void Process_ShouldThrowException_WhenAttemptingToResolveRightConstantValue<TLeftArg, TRightArg, TException>(
        ParsedExpressionBinaryOperator sut,
        TRightArg rightValue)
        where TException : Exception
    {
        var left = CreateVariableOperand<TLeftArg>( "left" );
        var right = CreateConstantOperand( rightValue );

        var action = Lambda.Of( () => sut.Process( left, right ) );

        action.Should().ThrowExactly<TException>();
    }

    protected static void DefaultNodeAssertion(Expression left, Expression right, Expression result)
    {
        result.Should().BeAssignableTo<BinaryExpression>();
        if ( result is not BinaryExpression binaryResult )
            return;

        binaryResult.Left.Should().BeSameAs( left );
        binaryResult.Right.Should().BeSameAs( right );
    }
}
