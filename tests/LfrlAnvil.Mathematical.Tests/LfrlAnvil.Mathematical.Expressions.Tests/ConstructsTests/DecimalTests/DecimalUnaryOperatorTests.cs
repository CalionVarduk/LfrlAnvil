using System.Linq.Expressions;
using FluentAssertions;
using LfrlAnvil.Mathematical.Expressions.Constructs.Decimal;
using Xunit;

namespace LfrlAnvil.Mathematical.Expressions.Tests.ConstructsTests.DecimalTests;

public class DecimalUnaryOperatorTests : UnaryOperatorsTestsBase
{
    [Fact]
    public void NegateOperatorProcess_ShouldPopOneOperandAndPushOneExpression_WhenOperandIsVariable()
    {
        Process_ShouldPopOneOperandAndPushOneExpression_WhenOperandIsVariable<decimal, decimal>(
            sut: new MathExpressionNegateDecimalOperator(),
            expectedNodeType: ExpressionType.Negate,
            DefaultNodeAssertion );
    }

    [Fact]
    public void NegateOperatorProcess_ShouldPopOneOperandAndPushOneExpression_WhenOperandIsConstant()
    {
        Process_ShouldPopOneOperandAndPushOneExpression_WhenOperandIsConstant<decimal, decimal>(
            sut: new MathExpressionNegateDecimalOperator(),
            expectedNodeType: ExpressionType.Constant,
            operandValue: 123,
            (_, result) =>
            {
                result.Should().BeAssignableTo<ConstantExpression>();
                if ( result is not ConstantExpression constantResult )
                    return;

                constantResult.Value.Should().Be( -123m );
            } );
    }
}
