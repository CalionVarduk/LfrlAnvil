using System;
using System.Linq.Expressions;
using FluentAssertions;
using FluentAssertions.Execution;
using LfrlAnvil.Functional;
using LfrlAnvil.Computable.Expressions.Constructs;

namespace LfrlAnvil.Computable.Expressions.Tests.ConstructsTests;

public abstract class BinaryOperatorsTestsBase : ConstructsTestsBase
{
    protected static void Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable<TLeftArg, TRightArg, TResult>(
        MathExpressionBinaryOperator sut,
        ExpressionType expectedNodeType,
        Action<Expression, Expression, Expression> nodeAssertion)
    {
        var left = CreateVariableOperand<TLeftArg>( "left" );
        var right = CreateVariableOperand<TRightArg>( "right" );
        var stack = CreateStack( left, right );

        sut.Process( stack );

        using ( new AssertionScope() )
        {
            stack.Count.Should().Be( 1 );
            if ( stack.Count == 0 )
                return;

            var result = stack[0];
            result.NodeType.Should().Be( expectedNodeType );
            result.Type.Should().Be( typeof( TResult ) );
            nodeAssertion( left, right, result );
        }
    }

    protected static void Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant<TLeftArg, TRightArg, TResult>(
        MathExpressionBinaryOperator sut,
        ExpressionType expectedNodeType,
        TLeftArg leftValue,
        TRightArg rightValue,
        Action<Expression, Expression, Expression> nodeAssertion)
    {
        var left = CreateConstantOperand( leftValue );
        var right = CreateConstantOperand( rightValue );
        var stack = CreateStack( left, right );

        sut.Process( stack );

        using ( new AssertionScope() )
        {
            stack.Count.Should().Be( 1 );
            if ( stack.Count == 0 )
                return;

            var result = stack[0];
            result.NodeType.Should().Be( expectedNodeType );
            result.Type.Should().Be( typeof( TResult ) );
            nodeAssertion( left, right, result );
        }
    }

    protected static void Process_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant<TLeftArg, TRightArg, TResult>(
        MathExpressionBinaryOperator sut,
        ExpressionType expectedNodeType,
        TLeftArg leftValue,
        Action<Expression, Expression, Expression> nodeAssertion)
    {
        var left = CreateConstantOperand( leftValue );
        var right = CreateVariableOperand<TRightArg>( "right" );
        var stack = CreateStack( left, right );

        sut.Process( stack );

        using ( new AssertionScope() )
        {
            stack.Count.Should().Be( 1 );
            if ( stack.Count == 0 )
                return;

            var result = stack[0];
            result.NodeType.Should().Be( expectedNodeType );
            result.Type.Should().Be( typeof( TResult ) );
            nodeAssertion( left, right, result );
        }
    }

    protected static void Process_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant<TLeftArg, TRightArg, TResult>(
        MathExpressionBinaryOperator sut,
        ExpressionType expectedNodeType,
        TRightArg rightValue,
        Action<Expression, Expression, Expression> nodeAssertion)
    {
        var left = CreateVariableOperand<TLeftArg>( "left" );
        var right = CreateConstantOperand( rightValue );
        var stack = CreateStack( left, right );

        sut.Process( stack );

        using ( new AssertionScope() )
        {
            stack.Count.Should().Be( 1 );
            if ( stack.Count == 0 )
                return;

            var result = stack[0];
            result.NodeType.Should().Be( expectedNodeType );
            result.Type.Should().Be( typeof( TResult ) );
            nodeAssertion( left, right, result );
        }
    }

    protected static void Process_ShouldThrowException_WhenOperatorDoesNotExist<TLeftArg, TRightArg, TException>(
        MathExpressionBinaryOperator sut)
        where TException : Exception
    {
        var left = CreateVariableOperand<TLeftArg>( "left" );
        var right = CreateVariableOperand<TRightArg>( "right" );
        var stack = CreateStack( left, right );

        var action = Lambda.Of( () => sut.Process( stack ) );

        action.Should().ThrowExactly<TException>();
    }

    protected static void Process_ShouldThrowException_WhenAttemptingToResolveRightConstantValue<TLeftArg, TRightArg, TException>(
        MathExpressionBinaryOperator sut,
        TRightArg rightValue)
        where TException : Exception
    {
        var left = CreateVariableOperand<TLeftArg>( "left" );
        var right = CreateConstantOperand( rightValue );
        var stack = CreateStack( left, right );

        var action = Lambda.Of( () => sut.Process( stack ) );

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
