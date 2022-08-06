﻿using System.Linq.Expressions;
using FluentAssertions;
using LfrlAnvil.Mathematical.Expressions.Constructs.Boolean;
using Xunit;

namespace LfrlAnvil.Mathematical.Expressions.Tests.ConstructsTests.BooleanTests;

public class BooleanUnaryOperatorTests : UnaryOperatorsTestsBase
{
    [Fact]
    public void NotOperatorProcess_ShouldPopOneOperandAndPushOneExpression_WhenOperandIsVariable()
    {
        Process_ShouldPopOneOperandAndPushOneExpression_WhenOperandIsVariable<bool, bool>(
            sut: new MathExpressionNotOperator(),
            expectedNodeType: ExpressionType.Not,
            DefaultNodeAssertion );
    }

    [Theory]
    [InlineData( true, false )]
    [InlineData( false, true )]
    public void NotOperatorProcess_ShouldPopOneOperandAndPushOneExpression_WhenOperandIsConstant(bool operand, bool expected)
    {
        Process_ShouldPopOneOperandAndPushOneExpression_WhenOperandIsConstant<bool, bool>(
            sut: new MathExpressionNotOperator(),
            expectedNodeType: ExpressionType.Constant,
            operandValue: operand,
            (_, result) =>
            {
                result.Should().BeAssignableTo<ConstantExpression>();
                if ( result is not ConstantExpression constantResult )
                    return;

                constantResult.Value.Should().Be( expected );
            } );
    }
}
