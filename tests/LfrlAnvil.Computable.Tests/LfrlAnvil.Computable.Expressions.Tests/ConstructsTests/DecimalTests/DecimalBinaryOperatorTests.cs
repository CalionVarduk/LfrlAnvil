using System.Linq;
using System.Linq.Expressions;
using LfrlAnvil.Computable.Expressions.Constructs.Decimal;

namespace LfrlAnvil.Computable.Expressions.Tests.ConstructsTests.DecimalTests;

public class DecimalBinaryOperatorTests : BinaryOperatorsTestsBase
{
    [Fact]
    public void AddOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable<decimal, decimal, decimal>(
            sut: new ParsedExpressionAddDecimalOperator(),
            expectedNodeType: ExpressionType.Add,
            DefaultNodeAssertion );
    }

    [Fact]
    public void AddOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant<decimal, decimal, decimal>(
            sut: new ParsedExpressionAddDecimalOperator(),
            expectedNodeType: ExpressionType.Constant,
            leftValue: 123,
            rightValue: 456,
            (_, _, result) =>
            {
                result.Should().BeAssignableTo<ConstantExpression>();
                if ( result is not ConstantExpression constantResult )
                    return;

                constantResult.Value.Should().Be( 579m );
            } );
    }

    [Fact]
    public void AddOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstantAndNotEqualToZero()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant<decimal, decimal, decimal>(
            sut: new ParsedExpressionAddDecimalOperator(),
            expectedNodeType: ExpressionType.Add,
            leftValue: Fixture.CreateNotDefault<decimal>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void AddOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstantAndEqualToZero()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant<decimal, decimal, decimal>(
            sut: new ParsedExpressionAddDecimalOperator(),
            expectedNodeType: ExpressionType.Parameter,
            leftValue: 0,
            (_, right, result) => result.Should().BeSameAs( right ) );
    }

    [Fact]
    public void AddOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstantAndNotEqualToZero()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant<decimal, decimal, decimal>(
            sut: new ParsedExpressionAddDecimalOperator(),
            expectedNodeType: ExpressionType.Add,
            rightValue: Fixture.CreateNotDefault<decimal>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void AddOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstantAndEqualToZero()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant<decimal, decimal, decimal>(
            sut: new ParsedExpressionAddDecimalOperator(),
            expectedNodeType: ExpressionType.Parameter,
            rightValue: 0,
            (left, _, result) => result.Should().BeSameAs( left ) );
    }

    [Fact]
    public void SubtractOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable<decimal, decimal, decimal>(
            sut: new ParsedExpressionSubtractDecimalOperator(),
            expectedNodeType: ExpressionType.Subtract,
            DefaultNodeAssertion );
    }

    [Fact]
    public void SubtractOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant<decimal, decimal, decimal>(
            sut: new ParsedExpressionSubtractDecimalOperator(),
            expectedNodeType: ExpressionType.Constant,
            leftValue: 123,
            rightValue: 456,
            (_, _, result) =>
            {
                result.Should().BeAssignableTo<ConstantExpression>();
                if ( result is not ConstantExpression constantResult )
                    return;

                constantResult.Value.Should().Be( -333m );
            } );
    }

    [Fact]
    public void SubtractOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstantAndNotEqualToZero()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant<decimal, decimal, decimal>(
            sut: new ParsedExpressionSubtractDecimalOperator(),
            expectedNodeType: ExpressionType.Subtract,
            leftValue: Fixture.CreateNotDefault<decimal>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void SubtractOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstantAndEqualToZero()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant<decimal, decimal, decimal>(
            sut: new ParsedExpressionSubtractDecimalOperator(),
            expectedNodeType: ExpressionType.Negate,
            leftValue: 0,
            (_, right, result) =>
            {
                result.Should().BeAssignableTo<UnaryExpression>();
                if ( result is not UnaryExpression unaryResult )
                    return;

                unaryResult.Operand.Should().BeSameAs( right );
            } );
    }

    [Fact]
    public void SubtractOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstantAndNotEqualToZero()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant<decimal, decimal, decimal>(
            sut: new ParsedExpressionSubtractDecimalOperator(),
            expectedNodeType: ExpressionType.Subtract,
            rightValue: Fixture.CreateNotDefault<decimal>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void SubtractOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstantAndEqualToZero()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant<decimal, decimal, decimal>(
            sut: new ParsedExpressionSubtractDecimalOperator(),
            expectedNodeType: ExpressionType.Parameter,
            rightValue: 0,
            (left, _, result) => result.Should().BeSameAs( left ) );
    }

    [Fact]
    public void MultiplyOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable<decimal, decimal, decimal>(
            sut: new ParsedExpressionMultiplyDecimalOperator(),
            expectedNodeType: ExpressionType.Multiply,
            DefaultNodeAssertion );
    }

    [Fact]
    public void MultiplyOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant<decimal, decimal, decimal>(
            sut: new ParsedExpressionMultiplyDecimalOperator(),
            expectedNodeType: ExpressionType.Constant,
            leftValue: 123,
            rightValue: 456,
            (_, _, result) =>
            {
                result.Should().BeAssignableTo<ConstantExpression>();
                if ( result is not ConstantExpression constantResult )
                    return;

                constantResult.Value.Should().Be( 56088m );
            } );
    }

    [Fact]
    public void MultiplyOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstantAndNotEqualToZeroOrOneOrMinusOne()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant<decimal, decimal, decimal>(
            sut: new ParsedExpressionMultiplyDecimalOperator(),
            expectedNodeType: ExpressionType.Multiply,
            leftValue: 123,
            DefaultNodeAssertion );
    }

    [Fact]
    public void MultiplyOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstantAndEqualToZero()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant<decimal, decimal, decimal>(
            sut: new ParsedExpressionMultiplyDecimalOperator(),
            expectedNodeType: ExpressionType.Constant,
            leftValue: 0,
            (_, _, result) =>
            {
                result.Should().BeAssignableTo<ConstantExpression>();
                if ( result is not ConstantExpression constantResult )
                    return;

                constantResult.Value.Should().Be( 0m );
            } );
    }

    [Fact]
    public void MultiplyOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstantAndEqualToOne()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant<decimal, decimal, decimal>(
            sut: new ParsedExpressionMultiplyDecimalOperator(),
            expectedNodeType: ExpressionType.Parameter,
            leftValue: 1,
            (_, right, result) => result.Should().BeSameAs( right ) );
    }

    [Fact]
    public void MultiplyOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstantAndEqualToMinusOne()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant<decimal, decimal, decimal>(
            sut: new ParsedExpressionMultiplyDecimalOperator(),
            expectedNodeType: ExpressionType.Negate,
            leftValue: -1,
            (_, right, result) =>
            {
                result.Should().BeAssignableTo<UnaryExpression>();
                if ( result is not UnaryExpression unaryResult )
                    return;

                unaryResult.Operand.Should().BeSameAs( right );
            } );
    }

    [Fact]
    public void AddOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstantAndNotEqualToZeroOrOneOrMinusOne()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant<decimal, decimal, decimal>(
            sut: new ParsedExpressionMultiplyDecimalOperator(),
            expectedNodeType: ExpressionType.Multiply,
            rightValue: 123,
            DefaultNodeAssertion );
    }

    [Fact]
    public void MultiplyOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstantAndEqualToZero()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant<decimal, decimal, decimal>(
            sut: new ParsedExpressionMultiplyDecimalOperator(),
            expectedNodeType: ExpressionType.Constant,
            rightValue: 0,
            (_, _, result) =>
            {
                result.Should().BeAssignableTo<ConstantExpression>();
                if ( result is not ConstantExpression constantResult )
                    return;

                constantResult.Value.Should().Be( 0m );
            } );
    }

    [Fact]
    public void MultiplyOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstantAndEqualToOne()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant<decimal, decimal, decimal>(
            sut: new ParsedExpressionMultiplyDecimalOperator(),
            expectedNodeType: ExpressionType.Parameter,
            rightValue: 1,
            (left, _, result) => result.Should().BeSameAs( left ) );
    }

    [Fact]
    public void MultiplyOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstantAndEqualToMinusOne()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant<decimal, decimal, decimal>(
            sut: new ParsedExpressionMultiplyDecimalOperator(),
            expectedNodeType: ExpressionType.Negate,
            rightValue: -1,
            (left, _, result) =>
            {
                result.Should().BeAssignableTo<UnaryExpression>();
                if ( result is not UnaryExpression unaryResult )
                    return;

                unaryResult.Operand.Should().BeSameAs( left );
            } );
    }

    [Fact]
    public void DivideOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable<decimal, decimal, decimal>(
            sut: new ParsedExpressionDivideDecimalOperator(),
            expectedNodeType: ExpressionType.Divide,
            DefaultNodeAssertion );
    }

    [Fact]
    public void DivideOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant<decimal, decimal, decimal>(
            sut: new ParsedExpressionDivideDecimalOperator(),
            expectedNodeType: ExpressionType.Constant,
            leftValue: 1236,
            rightValue: 4,
            (_, _, result) =>
            {
                result.Should().BeAssignableTo<ConstantExpression>();
                if ( result is not ConstantExpression constantResult )
                    return;

                constantResult.Value.Should().Be( 309m );
            } );
    }

    [Fact]
    public void DivideOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant<decimal, decimal, decimal>(
            sut: new ParsedExpressionDivideDecimalOperator(),
            expectedNodeType: ExpressionType.Divide,
            leftValue: Fixture.Create<decimal>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void DivideOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstantAndNotEqualToZeroOrOneOrMinusOne()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant<decimal, decimal, decimal>(
            sut: new ParsedExpressionDivideDecimalOperator(),
            expectedNodeType: ExpressionType.Divide,
            rightValue: 123,
            DefaultNodeAssertion );
    }

    [Fact]
    public void DivideOperatorProcess_ShouldThrowDivideByZeroException_WhenRightOperandIsConstantAndEqualToZero()
    {
        Process_ShouldThrowException_WhenAttemptingToResolveRightConstantValue<decimal, decimal, DivideByZeroException>(
            sut: new ParsedExpressionDivideDecimalOperator(),
            rightValue: 0 );
    }

    [Fact]
    public void DivideOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstantAndEqualToOne()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant<decimal, decimal, decimal>(
            sut: new ParsedExpressionDivideDecimalOperator(),
            expectedNodeType: ExpressionType.Parameter,
            rightValue: 1,
            (left, _, result) => result.Should().BeSameAs( left ) );
    }

    [Fact]
    public void DivideOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstantAndEqualToMinusOne()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant<decimal, decimal, decimal>(
            sut: new ParsedExpressionDivideDecimalOperator(),
            expectedNodeType: ExpressionType.Negate,
            rightValue: -1,
            (left, _, result) =>
            {
                result.Should().BeAssignableTo<UnaryExpression>();
                if ( result is not UnaryExpression unaryResult )
                    return;

                unaryResult.Operand.Should().BeSameAs( left );
            } );
    }

    [Fact]
    public void ModuloOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable<decimal, decimal, decimal>(
            sut: new ParsedExpressionModuloDecimalOperator(),
            expectedNodeType: ExpressionType.Modulo,
            DefaultNodeAssertion );
    }

    [Fact]
    public void ModuloOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant<decimal, decimal, decimal>(
            sut: new ParsedExpressionModuloDecimalOperator(),
            expectedNodeType: ExpressionType.Constant,
            leftValue: 456,
            rightValue: 123,
            (_, _, result) =>
            {
                result.Should().BeAssignableTo<ConstantExpression>();
                if ( result is not ConstantExpression constantResult )
                    return;

                constantResult.Value.Should().Be( 87m );
            } );
    }

    [Fact]
    public void ModuloOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant<decimal, decimal, decimal>(
            sut: new ParsedExpressionModuloDecimalOperator(),
            expectedNodeType: ExpressionType.Modulo,
            leftValue: Fixture.Create<decimal>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void ModuloOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstantAndNotEqualToZero()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant<decimal, decimal, decimal>(
            sut: new ParsedExpressionModuloDecimalOperator(),
            expectedNodeType: ExpressionType.Modulo,
            rightValue: Fixture.CreateNotDefault<decimal>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void ModuloOperatorProcess_ShouldThrowDivideByZeroException_WhenRightOperandIsConstantAndEqualToZero()
    {
        Process_ShouldThrowException_WhenAttemptingToResolveRightConstantValue<decimal, decimal, DivideByZeroException>(
            sut: new ParsedExpressionModuloDecimalOperator(),
            rightValue: 0 );
    }

    [Fact]
    public void EqualToOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable<decimal, decimal, bool>(
            sut: new ParsedExpressionEqualToDecimalOperator(),
            expectedNodeType: ExpressionType.Equal,
            DefaultNodeAssertion );
    }

    [Theory]
    [InlineData( 123, 456, false )]
    [InlineData( 123, 123, true )]
    public void EqualToOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant(
        int left,
        int right,
        bool expected)
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant<decimal, decimal, bool>(
            sut: new ParsedExpressionEqualToDecimalOperator(),
            expectedNodeType: ExpressionType.Constant,
            leftValue: left,
            rightValue: right,
            (_, _, result) =>
            {
                result.Should().BeAssignableTo<ConstantExpression>();
                if ( result is not ConstantExpression constantResult )
                    return;

                constantResult.Value.Should().Be( expected );
            } );
    }

    [Fact]
    public void EqualToOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant<decimal, decimal, bool>(
            sut: new ParsedExpressionEqualToDecimalOperator(),
            expectedNodeType: ExpressionType.Equal,
            leftValue: Fixture.Create<decimal>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void EqualToOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant<decimal, decimal, bool>(
            sut: new ParsedExpressionEqualToDecimalOperator(),
            expectedNodeType: ExpressionType.Equal,
            rightValue: Fixture.Create<decimal>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void NotEqualToOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable<decimal, decimal, bool>(
            sut: new ParsedExpressionNotEqualToDecimalOperator(),
            expectedNodeType: ExpressionType.NotEqual,
            DefaultNodeAssertion );
    }

    [Theory]
    [InlineData( 123, 456, true )]
    [InlineData( 123, 123, false )]
    public void NotEqualToOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant(
        int left,
        int right,
        bool expected)
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant<decimal, decimal, bool>(
            sut: new ParsedExpressionNotEqualToDecimalOperator(),
            expectedNodeType: ExpressionType.Constant,
            leftValue: left,
            rightValue: right,
            (_, _, result) =>
            {
                result.Should().BeAssignableTo<ConstantExpression>();
                if ( result is not ConstantExpression constantResult )
                    return;

                constantResult.Value.Should().Be( expected );
            } );
    }

    [Fact]
    public void NotEqualToOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant<decimal, decimal, bool>(
            sut: new ParsedExpressionNotEqualToDecimalOperator(),
            expectedNodeType: ExpressionType.NotEqual,
            leftValue: Fixture.Create<decimal>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void NotEqualToOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant<decimal, decimal, bool>(
            sut: new ParsedExpressionNotEqualToDecimalOperator(),
            expectedNodeType: ExpressionType.NotEqual,
            rightValue: Fixture.Create<decimal>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void GreaterThanOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable<decimal, decimal, bool>(
            sut: new ParsedExpressionGreaterThanDecimalOperator(),
            expectedNodeType: ExpressionType.GreaterThan,
            DefaultNodeAssertion );
    }

    [Theory]
    [InlineData( 123, 456, false )]
    [InlineData( 123, 123, false )]
    [InlineData( 456, 123, true )]
    public void GreaterThanOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant(
        int left,
        int right,
        bool expected)
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant<decimal, decimal, bool>(
            sut: new ParsedExpressionGreaterThanDecimalOperator(),
            expectedNodeType: ExpressionType.Constant,
            leftValue: left,
            rightValue: right,
            (_, _, result) =>
            {
                result.Should().BeAssignableTo<ConstantExpression>();
                if ( result is not ConstantExpression constantResult )
                    return;

                constantResult.Value.Should().Be( expected );
            } );
    }

    [Fact]
    public void GreaterThanOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant<decimal, decimal, bool>(
            sut: new ParsedExpressionGreaterThanDecimalOperator(),
            expectedNodeType: ExpressionType.GreaterThan,
            leftValue: Fixture.Create<decimal>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void GreaterThanOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant<decimal, decimal, bool>(
            sut: new ParsedExpressionGreaterThanDecimalOperator(),
            expectedNodeType: ExpressionType.GreaterThan,
            rightValue: Fixture.Create<decimal>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void LessThanOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable<decimal, decimal, bool>(
            sut: new ParsedExpressionLessThanDecimalOperator(),
            expectedNodeType: ExpressionType.LessThan,
            DefaultNodeAssertion );
    }

    [Theory]
    [InlineData( 123, 456, true )]
    [InlineData( 123, 123, false )]
    [InlineData( 456, 123, false )]
    public void LessThanOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant(
        int left,
        int right,
        bool expected)
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant<decimal, decimal, bool>(
            sut: new ParsedExpressionLessThanDecimalOperator(),
            expectedNodeType: ExpressionType.Constant,
            leftValue: left,
            rightValue: right,
            (_, _, result) =>
            {
                result.Should().BeAssignableTo<ConstantExpression>();
                if ( result is not ConstantExpression constantResult )
                    return;

                constantResult.Value.Should().Be( expected );
            } );
    }

    [Fact]
    public void LessThanOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant<decimal, decimal, bool>(
            sut: new ParsedExpressionLessThanDecimalOperator(),
            expectedNodeType: ExpressionType.LessThan,
            leftValue: Fixture.Create<decimal>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void LessThanOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant<decimal, decimal, bool>(
            sut: new ParsedExpressionLessThanDecimalOperator(),
            expectedNodeType: ExpressionType.LessThan,
            rightValue: Fixture.Create<decimal>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void GreaterThanOrEqualToOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable<decimal, decimal, bool>(
            sut: new ParsedExpressionGreaterThanOrEqualToDecimalOperator(),
            expectedNodeType: ExpressionType.GreaterThanOrEqual,
            DefaultNodeAssertion );
    }

    [Theory]
    [InlineData( 123, 456, false )]
    [InlineData( 123, 123, true )]
    [InlineData( 456, 123, true )]
    public void GreaterThanOrEqualToOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant(
        int left,
        int right,
        bool expected)
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant<decimal, decimal, bool>(
            sut: new ParsedExpressionGreaterThanOrEqualToDecimalOperator(),
            expectedNodeType: ExpressionType.Constant,
            leftValue: left,
            rightValue: right,
            (_, _, result) =>
            {
                result.Should().BeAssignableTo<ConstantExpression>();
                if ( result is not ConstantExpression constantResult )
                    return;

                constantResult.Value.Should().Be( expected );
            } );
    }

    [Fact]
    public void GreaterThanOrEqualToOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant<decimal, decimal, bool>(
            sut: new ParsedExpressionGreaterThanOrEqualToDecimalOperator(),
            expectedNodeType: ExpressionType.GreaterThanOrEqual,
            leftValue: Fixture.Create<decimal>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void GreaterThanOrEqualToOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant<decimal, decimal, bool>(
            sut: new ParsedExpressionGreaterThanOrEqualToDecimalOperator(),
            expectedNodeType: ExpressionType.GreaterThanOrEqual,
            rightValue: Fixture.Create<decimal>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void LessThanOrEqualToOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable<decimal, decimal, bool>(
            sut: new ParsedExpressionLessThanOrEqualToDecimalOperator(),
            expectedNodeType: ExpressionType.LessThanOrEqual,
            DefaultNodeAssertion );
    }

    [Theory]
    [InlineData( 123, 456, true )]
    [InlineData( 123, 123, true )]
    [InlineData( 456, 123, false )]
    public void LessThanOrEqualToOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant(
        int left,
        int right,
        bool expected)
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant<decimal, decimal, bool>(
            sut: new ParsedExpressionLessThanOrEqualToDecimalOperator(),
            expectedNodeType: ExpressionType.Constant,
            leftValue: left,
            rightValue: right,
            (_, _, result) =>
            {
                result.Should().BeAssignableTo<ConstantExpression>();
                if ( result is not ConstantExpression constantResult )
                    return;

                constantResult.Value.Should().Be( expected );
            } );
    }

    [Fact]
    public void LessThanOrEqualToOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant<decimal, decimal, bool>(
            sut: new ParsedExpressionLessThanOrEqualToDecimalOperator(),
            expectedNodeType: ExpressionType.LessThanOrEqual,
            leftValue: Fixture.Create<decimal>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void LessThanOrEqualToOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant<decimal, decimal, bool>(
            sut: new ParsedExpressionLessThanOrEqualToDecimalOperator(),
            expectedNodeType: ExpressionType.LessThanOrEqual,
            rightValue: Fixture.Create<decimal>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void CompareOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable<decimal, decimal, int>(
            sut: new ParsedExpressionCompareDecimalOperator(),
            expectedNodeType: ExpressionType.Call,
            (left, right, result) =>
            {
                result.Should().BeAssignableTo<MethodCallExpression>();
                if ( result is not MethodCallExpression methodCallResult )
                    return;

                methodCallResult.Object.Should().BeSameAs( left );
                methodCallResult.Arguments.Should().HaveCount( 1 ).And.Subject.First().Should().BeSameAs( right );
                methodCallResult.Method.Name.Should().Be( nameof( IComparable.CompareTo ) );
            } );
    }

    [Theory]
    [InlineData( 123, 456, -1 )]
    [InlineData( 123, 123, 0 )]
    [InlineData( 456, 123, 1 )]
    public void CompareOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant(
        int left,
        int right,
        int expected)
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant<decimal, decimal, int>(
            sut: new ParsedExpressionCompareDecimalOperator(),
            expectedNodeType: ExpressionType.Constant,
            leftValue: left,
            rightValue: right,
            (_, _, result) =>
            {
                result.Should().BeAssignableTo<ConstantExpression>();
                if ( result is not ConstantExpression constantResult )
                    return;

                constantResult.Value.Should().Be( expected );
            } );
    }

    [Fact]
    public void CompareOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant<decimal, decimal, int>(
            sut: new ParsedExpressionCompareDecimalOperator(),
            expectedNodeType: ExpressionType.Call,
            leftValue: Fixture.Create<decimal>(),
            (left, right, result) =>
            {
                result.Should().BeAssignableTo<MethodCallExpression>();
                if ( result is not MethodCallExpression methodCallResult )
                    return;

                methodCallResult.Object.Should().BeSameAs( left );
                methodCallResult.Arguments.Should().HaveCount( 1 ).And.Subject.First().Should().BeSameAs( right );
                methodCallResult.Method.Name.Should().Be( nameof( IComparable.CompareTo ) );
            } );
    }

    [Fact]
    public void CompareOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant<decimal, decimal, int>(
            sut: new ParsedExpressionCompareDecimalOperator(),
            expectedNodeType: ExpressionType.Call,
            rightValue: Fixture.Create<decimal>(),
            (left, right, result) =>
            {
                result.Should().BeAssignableTo<MethodCallExpression>();
                if ( result is not MethodCallExpression methodCallResult )
                    return;

                methodCallResult.Object.Should().BeSameAs( left );
                methodCallResult.Arguments.Should().HaveCount( 1 ).And.Subject.First().Should().BeSameAs( right );
                methodCallResult.Method.Name.Should().Be( nameof( IComparable.CompareTo ) );
            } );
    }
}
