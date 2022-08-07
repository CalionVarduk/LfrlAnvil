using System;
using System.Linq;
using System.Linq.Expressions;
using AutoFixture;
using FluentAssertions;
using LfrlAnvil.Computable.Expressions.Constructs.Boolean;
using Xunit;

namespace LfrlAnvil.Computable.Expressions.Tests.ConstructsTests.BooleanTests;

public class BooleanBinaryOperatorTests : BinaryOperatorsTestsBase
{
    [Fact]
    public void AndOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable<bool, bool, bool>(
            sut: new MathExpressionAndOperator(),
            expectedNodeType: ExpressionType.AndAlso,
            DefaultNodeAssertion );
    }

    [Theory]
    [InlineData( true, true, true )]
    [InlineData( true, false, false )]
    [InlineData( false, true, false )]
    [InlineData( false, false, false )]
    public void AndOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant(
        bool left,
        bool right,
        bool expected)
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant<bool, bool, bool>(
            sut: new MathExpressionAndOperator(),
            expectedNodeType: ExpressionType.Constant,
            leftValue: left,
            rightValue: right,
            (_, _, result) =>
            {
                result.Should().BeAssignableTo<ConstantExpression>();
                if ( result is not ConstantExpression constantExpression )
                    return;

                constantExpression.Value.Should().Be( expected );
            } );
    }

    [Fact]
    public void AndOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstantAndTrue()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant<bool, bool, bool>(
            sut: new MathExpressionAndOperator(),
            expectedNodeType: ExpressionType.Parameter,
            leftValue: true,
            (_, right, result) => result.Should().BeSameAs( right ) );
    }

    [Fact]
    public void AndOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstantAndFalse()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant<bool, bool, bool>(
            sut: new MathExpressionAndOperator(),
            expectedNodeType: ExpressionType.Constant,
            leftValue: false,
            (_, _, result) =>
            {
                result.Should().BeAssignableTo<ConstantExpression>();
                if ( result is not ConstantExpression constantExpression )
                    return;

                constantExpression.Value.Should().Be( false );
            } );
    }

    [Fact]
    public void AndOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstantAndTrue()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant<bool, bool, bool>(
            sut: new MathExpressionAndOperator(),
            expectedNodeType: ExpressionType.Parameter,
            rightValue: true,
            (left, _, result) => result.Should().BeSameAs( left ) );
    }

    [Fact]
    public void AndOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstantAndFalse()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant<bool, bool, bool>(
            sut: new MathExpressionAndOperator(),
            expectedNodeType: ExpressionType.Constant,
            rightValue: false,
            (_, _, result) =>
            {
                result.Should().BeAssignableTo<ConstantExpression>();
                if ( result is not ConstantExpression constantExpression )
                    return;

                constantExpression.Value.Should().Be( false );
            } );
    }

    [Fact]
    public void OrOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable<bool, bool, bool>(
            sut: new MathExpressionOrOperator(),
            expectedNodeType: ExpressionType.OrElse,
            DefaultNodeAssertion );
    }

    [Theory]
    [InlineData( true, true, true )]
    [InlineData( true, false, true )]
    [InlineData( false, true, true )]
    [InlineData( false, false, false )]
    public void OrOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant(
        bool left,
        bool right,
        bool expected)
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant<bool, bool, bool>(
            sut: new MathExpressionOrOperator(),
            expectedNodeType: ExpressionType.Constant,
            leftValue: left,
            rightValue: right,
            (_, _, result) =>
            {
                result.Should().BeAssignableTo<ConstantExpression>();
                if ( result is not ConstantExpression constantExpression )
                    return;

                constantExpression.Value.Should().Be( expected );
            } );
    }

    [Fact]
    public void OrOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstantAndTrue()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant<bool, bool, bool>(
            sut: new MathExpressionOrOperator(),
            expectedNodeType: ExpressionType.Constant,
            leftValue: true,
            (_, _, result) =>
            {
                result.Should().BeAssignableTo<ConstantExpression>();
                if ( result is not ConstantExpression constantExpression )
                    return;

                constantExpression.Value.Should().Be( true );
            } );
    }

    [Fact]
    public void OrOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstantAndFalse()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant<bool, bool, bool>(
            sut: new MathExpressionOrOperator(),
            expectedNodeType: ExpressionType.Parameter,
            leftValue: false,
            (_, right, result) => result.Should().BeSameAs( right ) );
    }

    [Fact]
    public void OrOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstantAndTrue()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant<bool, bool, bool>(
            sut: new MathExpressionOrOperator(),
            expectedNodeType: ExpressionType.Constant,
            rightValue: true,
            (_, _, result) =>
            {
                result.Should().BeAssignableTo<ConstantExpression>();
                if ( result is not ConstantExpression constantExpression )
                    return;

                constantExpression.Value.Should().Be( true );
            } );
    }

    [Fact]
    public void OrOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstantAndFalse()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant<bool, bool, bool>(
            sut: new MathExpressionOrOperator(),
            expectedNodeType: ExpressionType.Parameter,
            rightValue: false,
            (left, _, result) => result.Should().BeSameAs( left ) );
    }

    [Fact]
    public void BitwiseAndOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable<bool, bool, bool>(
            sut: new MathExpressionBitwiseAndBooleanOperator(),
            expectedNodeType: ExpressionType.And,
            DefaultNodeAssertion );
    }

    [Theory]
    [InlineData( true, true, true )]
    [InlineData( true, false, false )]
    [InlineData( false, true, false )]
    [InlineData( false, false, false )]
    public void BitwiseAndOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant(
        bool left,
        bool right,
        bool expected)
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant<bool, bool, bool>(
            sut: new MathExpressionBitwiseAndBooleanOperator(),
            expectedNodeType: ExpressionType.Constant,
            leftValue: left,
            rightValue: right,
            (_, _, result) =>
            {
                result.Should().BeAssignableTo<ConstantExpression>();
                if ( result is not ConstantExpression constantExpression )
                    return;

                constantExpression.Value.Should().Be( expected );
            } );
    }

    [Fact]
    public void BitwiseAndOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstantAndTrue()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant<bool, bool, bool>(
            sut: new MathExpressionBitwiseAndBooleanOperator(),
            expectedNodeType: ExpressionType.Parameter,
            leftValue: true,
            (_, right, result) => result.Should().BeSameAs( right ) );
    }

    [Fact]
    public void BitwiseAndOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstantAndFalse()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant<bool, bool, bool>(
            sut: new MathExpressionBitwiseAndBooleanOperator(),
            expectedNodeType: ExpressionType.Constant,
            leftValue: false,
            (_, _, result) =>
            {
                result.Should().BeAssignableTo<ConstantExpression>();
                if ( result is not ConstantExpression constantExpression )
                    return;

                constantExpression.Value.Should().Be( false );
            } );
    }

    [Fact]
    public void BitwiseAndOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstantAndTrue()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant<bool, bool, bool>(
            sut: new MathExpressionBitwiseAndBooleanOperator(),
            expectedNodeType: ExpressionType.Parameter,
            rightValue: true,
            (left, _, result) => result.Should().BeSameAs( left ) );
    }

    [Fact]
    public void BitwiseAndOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstantAndFalse()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant<bool, bool, bool>(
            sut: new MathExpressionBitwiseAndBooleanOperator(),
            expectedNodeType: ExpressionType.Constant,
            rightValue: false,
            (_, _, result) =>
            {
                result.Should().BeAssignableTo<ConstantExpression>();
                if ( result is not ConstantExpression constantExpression )
                    return;

                constantExpression.Value.Should().Be( false );
            } );
    }

    [Fact]
    public void BitwiseOrOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable<bool, bool, bool>(
            sut: new MathExpressionBitwiseOrBooleanOperator(),
            expectedNodeType: ExpressionType.Or,
            DefaultNodeAssertion );
    }

    [Theory]
    [InlineData( true, true, true )]
    [InlineData( true, false, true )]
    [InlineData( false, true, true )]
    [InlineData( false, false, false )]
    public void BitwiseOrOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant(
        bool left,
        bool right,
        bool expected)
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant<bool, bool, bool>(
            sut: new MathExpressionBitwiseOrBooleanOperator(),
            expectedNodeType: ExpressionType.Constant,
            leftValue: left,
            rightValue: right,
            (_, _, result) =>
            {
                result.Should().BeAssignableTo<ConstantExpression>();
                if ( result is not ConstantExpression constantExpression )
                    return;

                constantExpression.Value.Should().Be( expected );
            } );
    }

    [Fact]
    public void BitwiseOrOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstantAndTrue()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant<bool, bool, bool>(
            sut: new MathExpressionBitwiseOrBooleanOperator(),
            expectedNodeType: ExpressionType.Constant,
            leftValue: true,
            (_, _, result) =>
            {
                result.Should().BeAssignableTo<ConstantExpression>();
                if ( result is not ConstantExpression constantExpression )
                    return;

                constantExpression.Value.Should().Be( true );
            } );
    }

    [Fact]
    public void BitwiseOrOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstantAndFalse()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant<bool, bool, bool>(
            sut: new MathExpressionBitwiseOrBooleanOperator(),
            expectedNodeType: ExpressionType.Parameter,
            leftValue: false,
            (_, right, result) => result.Should().BeSameAs( right ) );
    }

    [Fact]
    public void BitwiseOrOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstantAndTrue()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant<bool, bool, bool>(
            sut: new MathExpressionBitwiseOrBooleanOperator(),
            expectedNodeType: ExpressionType.Constant,
            rightValue: true,
            (_, _, result) =>
            {
                result.Should().BeAssignableTo<ConstantExpression>();
                if ( result is not ConstantExpression constantExpression )
                    return;

                constantExpression.Value.Should().Be( true );
            } );
    }

    [Fact]
    public void BitwiseOrOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstantAndFalse()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant<bool, bool, bool>(
            sut: new MathExpressionBitwiseOrBooleanOperator(),
            expectedNodeType: ExpressionType.Parameter,
            rightValue: false,
            (left, _, result) => result.Should().BeSameAs( left ) );
    }

    [Fact]
    public void BitwiseXorOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable<bool, bool, bool>(
            sut: new MathExpressionBitwiseXorBooleanOperator(),
            expectedNodeType: ExpressionType.ExclusiveOr,
            DefaultNodeAssertion );
    }

    [Theory]
    [InlineData( true, true, false )]
    [InlineData( true, false, true )]
    [InlineData( false, true, true )]
    [InlineData( false, false, false )]
    public void BitwiseXorOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant(
        bool left,
        bool right,
        bool expected)
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant<bool, bool, bool>(
            sut: new MathExpressionBitwiseXorBooleanOperator(),
            expectedNodeType: ExpressionType.Constant,
            leftValue: left,
            rightValue: right,
            (_, _, result) =>
            {
                result.Should().BeAssignableTo<ConstantExpression>();
                if ( result is not ConstantExpression constantExpression )
                    return;

                constantExpression.Value.Should().Be( expected );
            } );
    }

    [Fact]
    public void BitwiseXorOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant<bool, bool, bool>(
            sut: new MathExpressionBitwiseXorBooleanOperator(),
            expectedNodeType: ExpressionType.ExclusiveOr,
            leftValue: Fixture.Create<bool>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void BitwiseXorOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant<bool, bool, bool>(
            sut: new MathExpressionBitwiseXorBooleanOperator(),
            expectedNodeType: ExpressionType.ExclusiveOr,
            rightValue: Fixture.Create<bool>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void EqualToOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable<bool, bool, bool>(
            sut: new MathExpressionEqualToBooleanOperator(),
            expectedNodeType: ExpressionType.Equal,
            DefaultNodeAssertion );
    }

    [Theory]
    [InlineData( true, true, true )]
    [InlineData( false, true, false )]
    [InlineData( true, false, false )]
    public void EqualToOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant(
        bool left,
        bool right,
        bool expected)
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant<bool, bool, bool>(
            sut: new MathExpressionEqualToBooleanOperator(),
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
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant<bool, bool, bool>(
            sut: new MathExpressionEqualToBooleanOperator(),
            expectedNodeType: ExpressionType.Equal,
            leftValue: Fixture.Create<bool>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void EqualToOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant<bool, bool, bool>(
            sut: new MathExpressionEqualToBooleanOperator(),
            expectedNodeType: ExpressionType.Equal,
            rightValue: Fixture.Create<bool>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void NotEqualToOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable<bool, bool, bool>(
            sut: new MathExpressionNotEqualToBooleanOperator(),
            expectedNodeType: ExpressionType.NotEqual,
            DefaultNodeAssertion );
    }

    [Theory]
    [InlineData( true, true, false )]
    [InlineData( false, true, true )]
    [InlineData( true, false, true )]
    public void NotEqualToOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant(
        bool left,
        bool right,
        bool expected)
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant<bool, bool, bool>(
            sut: new MathExpressionNotEqualToBooleanOperator(),
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
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant<bool, bool, bool>(
            sut: new MathExpressionNotEqualToBooleanOperator(),
            expectedNodeType: ExpressionType.NotEqual,
            leftValue: Fixture.Create<bool>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void NotEqualToOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant<bool, bool, bool>(
            sut: new MathExpressionNotEqualToBooleanOperator(),
            expectedNodeType: ExpressionType.NotEqual,
            rightValue: Fixture.Create<bool>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void CompareOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable<bool, bool, int>(
            sut: new MathExpressionCompareBooleanOperator(),
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
    [InlineData( false, true, -1 )]
    [InlineData( true, true, 0 )]
    [InlineData( false, false, 0 )]
    [InlineData( true, false, 1 )]
    public void CompareOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant(
        bool left,
        bool right,
        int expected)
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant<bool, bool, int>(
            sut: new MathExpressionCompareBooleanOperator(),
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
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant<bool, bool, int>(
            sut: new MathExpressionCompareBooleanOperator(),
            expectedNodeType: ExpressionType.Call,
            leftValue: Fixture.Create<bool>(),
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
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant<bool, bool, int>(
            sut: new MathExpressionCompareBooleanOperator(),
            expectedNodeType: ExpressionType.Call,
            rightValue: Fixture.Create<bool>(),
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
