using System.Linq.Expressions;
using LfrlAnvil.Computable.Expressions.Constructs.Boolean;

namespace LfrlAnvil.Computable.Expressions.Tests.ConstructsTests.BooleanTests;

public class BooleanUnaryOperatorTests : UnaryOperatorsTestsBase
{
    [Fact]
    public void NotOperatorProcess_ShouldPopOneOperandAndPushOneExpression_WhenOperandIsVariable()
    {
        Process_ShouldPopOneOperandAndPushOneExpression_WhenOperandIsVariable<bool, bool>(
            sut: new ParsedExpressionNotOperator(),
            expectedNodeType: ExpressionType.Not,
            DefaultNodeAssertion );
    }

    [Theory]
    [InlineData( true, false )]
    [InlineData( false, true )]
    public void NotOperatorProcess_ShouldPopOneOperandAndPushOneExpression_WhenOperandIsConstant(bool operand, bool expected)
    {
        Process_ShouldPopOneOperandAndPushOneExpression_WhenOperandIsConstant<bool, bool>(
            sut: new ParsedExpressionNotOperator(),
            expectedNodeType: ExpressionType.Constant,
            operandValue: operand,
            (_, result) => Assertion.All(
                result.TestType().AssignableTo<ConstantExpression>(),
                result.TestIf()
                    .OfType<ConstantExpression>(
                        constantResult =>
                            constantResult.Value.TestEquals( expected ) ) ) );
    }
}
