﻿using System;
using System.Linq.Expressions;
using FluentAssertions;
using FluentAssertions.Execution;
using LfrlAnvil.Functional;
using LfrlAnvil.Mathematical.Expressions.Constructs;

namespace LfrlAnvil.Mathematical.Expressions.Tests.ConstructsTests;

public abstract class UnaryOperatorsTestsBase : ConstructsTestsBase
{
    protected static void Process_ShouldPopOneOperandAndPushOneExpression_WhenOperandIsVariable<TArg, TResult>(
        MathExpressionUnaryOperator sut,
        ExpressionType expectedNodeType,
        Action<Expression, Expression> nodeAssertion)
    {
        var operand = CreateVariableOperand<TArg>( "value" );
        var stack = CreateStack( operand );

        sut.Process( stack );

        using ( new AssertionScope() )
        {
            stack.Count.Should().Be( 1 );
            if ( stack.Count == 0 )
                return;

            var result = stack[0];
            result.NodeType.Should().Be( expectedNodeType );
            result.Type.Should().Be( typeof( TResult ) );
            nodeAssertion( operand, result );
        }
    }

    protected static void Process_ShouldPopOneOperandAndPushOneExpression_WhenOperandIsConstant<TArg, TResult>(
        MathExpressionUnaryOperator sut,
        ExpressionType expectedNodeType,
        TArg operandValue,
        Action<Expression, Expression> nodeAssertion)
    {
        var operand = CreateConstantOperand( operandValue );
        var stack = CreateStack( operand );

        sut.Process( stack );

        using ( new AssertionScope() )
        {
            stack.Count.Should().Be( 1 );
            if ( stack.Count == 0 )
                return;

            var result = stack[0];
            result.NodeType.Should().Be( expectedNodeType );
            result.Type.Should().Be( typeof( TResult ) );
            nodeAssertion( operand, result );
        }
    }

    protected static void Process_ShouldThrowException_WhenOperatorDoesNotExist<TArg, TException>(MathExpressionUnaryOperator sut)
        where TException : Exception
    {
        var operand = CreateVariableOperand<TArg>( "value" );
        var stack = CreateStack( operand );

        var action = Lambda.Of( () => sut.Process( stack ) );

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
