using System.Linq.Expressions;
using FluentAssertions;
using LfrlAnvil.Computable.Expressions.Constructs.Double;
using Xunit;

namespace LfrlAnvil.Computable.Expressions.Tests.ConstructsTests.DoubleTests;

public class DoubleUnaryOperatorTests : UnaryOperatorsTestsBase
{
    [Fact]
    public void NegateOperatorProcess_ShouldPopOneOperandAndPushOneExpression_WhenOperandIsVariable()
    {
        Process_ShouldPopOneOperandAndPushOneExpression_WhenOperandIsVariable<double, double>(
            sut: new MathExpressionNegateDoubleOperator(),
            expectedNodeType: ExpressionType.Negate,
            DefaultNodeAssertion );
    }

    [Fact]
    public void NegateOperatorProcess_ShouldPopOneOperandAndPushOneExpression_WhenOperandIsConstant()
    {
        Process_ShouldPopOneOperandAndPushOneExpression_WhenOperandIsConstant<double, double>(
            sut: new MathExpressionNegateDoubleOperator(),
            expectedNodeType: ExpressionType.Constant,
            operandValue: 123,
            (_, result) =>
            {
                result.Should().BeAssignableTo<ConstantExpression>();
                if ( result is not ConstantExpression constantResult )
                    return;

                constantResult.Value.Should().Be( -123.0 );
            } );
    }
}
