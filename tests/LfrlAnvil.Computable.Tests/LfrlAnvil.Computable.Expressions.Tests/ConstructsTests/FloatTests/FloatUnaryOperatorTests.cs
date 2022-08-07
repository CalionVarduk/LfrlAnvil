using System.Linq.Expressions;
using FluentAssertions;
using LfrlAnvil.Computable.Expressions.Constructs.Float;
using Xunit;

namespace LfrlAnvil.Computable.Expressions.Tests.ConstructsTests.FloatTests;

public class FloatUnaryOperatorTests : UnaryOperatorsTestsBase
{
    [Fact]
    public void NegateOperatorProcess_ShouldPopOneOperandAndPushOneExpression_WhenOperandIsVariable()
    {
        Process_ShouldPopOneOperandAndPushOneExpression_WhenOperandIsVariable<float, float>(
            sut: new ParsedExpressionNegateFloatOperator(),
            expectedNodeType: ExpressionType.Negate,
            DefaultNodeAssertion );
    }

    [Fact]
    public void NegateOperatorProcess_ShouldPopOneOperandAndPushOneExpression_WhenOperandIsConstant()
    {
        Process_ShouldPopOneOperandAndPushOneExpression_WhenOperandIsConstant<float, float>(
            sut: new ParsedExpressionNegateFloatOperator(),
            expectedNodeType: ExpressionType.Constant,
            operandValue: 123,
            (_, result) =>
            {
                result.Should().BeAssignableTo<ConstantExpression>();
                if ( result is not ConstantExpression constantResult )
                    return;

                constantResult.Value.Should().Be( -123.0F );
            } );
    }
}
