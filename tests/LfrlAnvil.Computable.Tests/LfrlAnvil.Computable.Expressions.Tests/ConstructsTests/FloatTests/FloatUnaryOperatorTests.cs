using System.Linq.Expressions;
using LfrlAnvil.Computable.Expressions.Constructs.Float;

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
            (_, result) => Assertion.All(
                result.TestType().AssignableTo<ConstantExpression>(),
                result.TestIf()
                    .OfType<ConstantExpression>(
                        constantResult =>
                            constantResult.Value.TestEquals( -123.0F ) ) ) );
    }
}
