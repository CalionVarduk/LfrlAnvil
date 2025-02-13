using System.Linq.Expressions;
using LfrlAnvil.Computable.Expressions.Constructs.Double;

namespace LfrlAnvil.Computable.Expressions.Tests.ConstructsTests.DoubleTests;

public class DoubleUnaryOperatorTests : UnaryOperatorsTestsBase
{
    [Fact]
    public void NegateOperatorProcess_ShouldPopOneOperandAndPushOneExpression_WhenOperandIsVariable()
    {
        Process_ShouldPopOneOperandAndPushOneExpression_WhenOperandIsVariable<double, double>(
            sut: new ParsedExpressionNegateDoubleOperator(),
            expectedNodeType: ExpressionType.Negate,
            DefaultNodeAssertion );
    }

    [Fact]
    public void NegateOperatorProcess_ShouldPopOneOperandAndPushOneExpression_WhenOperandIsConstant()
    {
        Process_ShouldPopOneOperandAndPushOneExpression_WhenOperandIsConstant<double, double>(
            sut: new ParsedExpressionNegateDoubleOperator(),
            expectedNodeType: ExpressionType.Constant,
            operandValue: 123,
            (_, result) => result.TestType()
                .AssignableTo<ConstantExpression>(
                    constantResult =>
                        constantResult.Value.TestEquals( -123.0 ) ) );
    }
}
