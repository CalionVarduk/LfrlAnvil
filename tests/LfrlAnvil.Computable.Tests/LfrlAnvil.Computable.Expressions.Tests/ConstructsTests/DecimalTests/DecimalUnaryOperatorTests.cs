using System.Linq.Expressions;
using LfrlAnvil.Computable.Expressions.Constructs.Decimal;

namespace LfrlAnvil.Computable.Expressions.Tests.ConstructsTests.DecimalTests;

public class DecimalUnaryOperatorTests : UnaryOperatorsTestsBase
{
    [Fact]
    public void NegateOperatorProcess_ShouldPopOneOperandAndPushOneExpression_WhenOperandIsVariable()
    {
        Process_ShouldPopOneOperandAndPushOneExpression_WhenOperandIsVariable<decimal, decimal>(
            sut: new ParsedExpressionNegateDecimalOperator(),
            expectedNodeType: ExpressionType.Negate,
            DefaultNodeAssertion );
    }

    [Fact]
    public void NegateOperatorProcess_ShouldPopOneOperandAndPushOneExpression_WhenOperandIsConstant()
    {
        Process_ShouldPopOneOperandAndPushOneExpression_WhenOperandIsConstant<decimal, decimal>(
            sut: new ParsedExpressionNegateDecimalOperator(),
            expectedNodeType: ExpressionType.Constant,
            operandValue: 123,
            (_, result) => result.TestType()
                .AssignableTo<ConstantExpression>(
                    constantResult =>
                        constantResult.Value.TestEquals( -123m ) ) );
    }
}
