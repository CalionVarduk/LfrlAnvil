using System.Linq.Expressions;
using LfrlAnvil.Computable.Expressions.Constructs;

namespace LfrlAnvil.Computable.Expressions.Tests.ConstructsTests;

public class GenericUnaryOperatorTests : UnaryOperatorsTestsBase
{
    [Fact]
    public void NegateOperatorProcess_ShouldPopOneOperandAndPushOneExpression_WhenOperandIsVariable()
    {
        Process_ShouldPopOneOperandAndPushOneExpression_WhenOperandIsVariable<int, int>(
            sut: new ParsedExpressionNegateOperator(),
            expectedNodeType: ExpressionType.Negate,
            DefaultNodeAssertion );
    }

    [Fact]
    public void NegateOperatorProcess_ShouldPopOneOperandAndPushOneExpression_WhenOperandIsConstant()
    {
        Process_ShouldPopOneOperandAndPushOneExpression_WhenOperandIsConstant<int, int>(
            sut: new ParsedExpressionNegateOperator(),
            expectedNodeType: ExpressionType.Negate,
            operandValue: Fixture.Create<int>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void NegateOperatorProcess_ShouldThrowInvalidOperationException_WhenOperatorDoesNotExist()
    {
        Process_ShouldThrowException_WhenOperatorDoesNotExist<string, InvalidOperationException>(
            sut: new ParsedExpressionNegateOperator() );
    }

    [Fact]
    public void BitwiseNotOperatorProcess_ShouldPopOneOperandAndPushOneExpression_WhenOperandIsVariable()
    {
        Process_ShouldPopOneOperandAndPushOneExpression_WhenOperandIsVariable<int, int>(
            sut: new ParsedExpressionBitwiseNotOperator(),
            expectedNodeType: ExpressionType.Not,
            DefaultNodeAssertion );
    }

    [Fact]
    public void BitwiseNotOperatorProcess_ShouldPopOneOperandAndPushOneExpression_WhenOperandIsConstant()
    {
        Process_ShouldPopOneOperandAndPushOneExpression_WhenOperandIsConstant<int, int>(
            sut: new ParsedExpressionBitwiseNotOperator(),
            expectedNodeType: ExpressionType.Not,
            operandValue: Fixture.Create<int>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void BitwiseNotOperatorProcess_ShouldThrowInvalidOperationException_WhenOperatorDoesNotExist()
    {
        Process_ShouldThrowException_WhenOperatorDoesNotExist<string, InvalidOperationException>(
            sut: new ParsedExpressionBitwiseNotOperator() );
    }
}
