using System.Linq.Expressions;
using FluentAssertions;
using LfrlAnvil.Computable.Expressions.Constructs.Int64;
using Xunit;

namespace LfrlAnvil.Computable.Expressions.Tests.ConstructsTests.Int64Tests;

public class Int64UnaryOperatorTests : UnaryOperatorsTestsBase
{
    [Fact]
    public void NegateOperatorProcess_ShouldPopOneOperandAndPushOneExpression_WhenOperandIsVariable()
    {
        Process_ShouldPopOneOperandAndPushOneExpression_WhenOperandIsVariable<long, long>(
            sut: new MathExpressionNegateInt64Operator(),
            expectedNodeType: ExpressionType.Negate,
            DefaultNodeAssertion );
    }

    [Fact]
    public void NegateOperatorProcess_ShouldPopOneOperandAndPushOneExpression_WhenOperandIsConstant()
    {
        Process_ShouldPopOneOperandAndPushOneExpression_WhenOperandIsConstant<long, long>(
            sut: new MathExpressionNegateInt64Operator(),
            expectedNodeType: ExpressionType.Constant,
            operandValue: 123,
            (_, result) =>
            {
                result.Should().BeAssignableTo<ConstantExpression>();
                if ( result is not ConstantExpression constantResult )
                    return;

                constantResult.Value.Should().Be( -123L );
            } );
    }

    [Fact]
    public void BitwiseNotOperatorProcess_ShouldPopOneOperandAndPushOneExpression_WhenOperandIsVariable()
    {
        Process_ShouldPopOneOperandAndPushOneExpression_WhenOperandIsVariable<long, long>(
            sut: new MathExpressionBitwiseNotInt64Operator(),
            expectedNodeType: ExpressionType.Not,
            DefaultNodeAssertion );
    }

    [Fact]
    public void BitwiseNotOperatorProcess_ShouldPopOneOperandAndPushOneExpression_WhenOperandIsConstant()
    {
        Process_ShouldPopOneOperandAndPushOneExpression_WhenOperandIsConstant<long, long>(
            sut: new MathExpressionBitwiseNotInt64Operator(),
            expectedNodeType: ExpressionType.Constant,
            operandValue: 123,
            (_, result) =>
            {
                result.Should().BeAssignableTo<ConstantExpression>();
                if ( result is not ConstantExpression constantResult )
                    return;

                constantResult.Value.Should().Be( ~123L );
            } );
    }
}
