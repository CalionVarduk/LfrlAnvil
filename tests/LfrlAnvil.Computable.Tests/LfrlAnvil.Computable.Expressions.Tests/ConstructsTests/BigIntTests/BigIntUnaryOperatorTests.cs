using System.Linq.Expressions;
using System.Numerics;
using FluentAssertions;
using LfrlAnvil.Computable.Expressions.Constructs.BigInt;
using Xunit;

namespace LfrlAnvil.Computable.Expressions.Tests.ConstructsTests.BigIntTests;

public class BigIntUnaryOperatorTests : UnaryOperatorsTestsBase
{
    [Fact]
    public void NegateOperatorProcess_ShouldPopOneOperandAndPushOneExpression_WhenOperandIsVariable()
    {
        Process_ShouldPopOneOperandAndPushOneExpression_WhenOperandIsVariable<BigInteger, BigInteger>(
            sut: new ParsedExpressionNegateBigIntOperator(),
            expectedNodeType: ExpressionType.Negate,
            DefaultNodeAssertion );
    }

    [Fact]
    public void NegateOperatorProcess_ShouldPopOneOperandAndPushOneExpression_WhenOperandIsConstant()
    {
        Process_ShouldPopOneOperandAndPushOneExpression_WhenOperandIsConstant<BigInteger, BigInteger>(
            sut: new ParsedExpressionNegateBigIntOperator(),
            expectedNodeType: ExpressionType.Constant,
            operandValue: 123,
            (_, result) =>
            {
                result.Should().BeAssignableTo<ConstantExpression>();
                if ( result is not ConstantExpression constantResult )
                    return;

                constantResult.Value.Should().Be( new BigInteger( -123 ) );
            } );
    }

    [Fact]
    public void BitwiseNotOperatorProcess_ShouldPopOneOperandAndPushOneExpression_WhenOperandIsVariable()
    {
        Process_ShouldPopOneOperandAndPushOneExpression_WhenOperandIsVariable<BigInteger, BigInteger>(
            sut: new ParsedExpressionBitwiseNotBigIntOperator(),
            expectedNodeType: ExpressionType.Not,
            DefaultNodeAssertion );
    }

    [Fact]
    public void BitwiseNotOperatorProcess_ShouldPopOneOperandAndPushOneExpression_WhenOperandIsConstant()
    {
        Process_ShouldPopOneOperandAndPushOneExpression_WhenOperandIsConstant<BigInteger, BigInteger>(
            sut: new ParsedExpressionBitwiseNotBigIntOperator(),
            expectedNodeType: ExpressionType.Constant,
            operandValue: 123,
            (_, result) =>
            {
                result.Should().BeAssignableTo<ConstantExpression>();
                if ( result is not ConstantExpression constantResult )
                    return;

                constantResult.Value.Should().Be( ~new BigInteger( 123 ) );
            } );
    }
}
