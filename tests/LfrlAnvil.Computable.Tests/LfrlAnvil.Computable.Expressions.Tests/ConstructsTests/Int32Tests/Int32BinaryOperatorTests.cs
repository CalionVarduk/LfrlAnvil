﻿using System.Linq;
using System.Linq.Expressions;
using LfrlAnvil.Computable.Expressions.Constructs.Int32;

namespace LfrlAnvil.Computable.Expressions.Tests.ConstructsTests.Int32Tests;

public class Int32BinaryOperatorTests : BinaryOperatorsTestsBase
{
    [Fact]
    public void AddOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable<int, int, int>(
            sut: new ParsedExpressionAddInt32Operator(),
            expectedNodeType: ExpressionType.Add,
            DefaultNodeAssertion );
    }

    [Fact]
    public void AddOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant<int, int, int>(
            sut: new ParsedExpressionAddInt32Operator(),
            expectedNodeType: ExpressionType.Constant,
            leftValue: 123,
            rightValue: 456,
            (_, _, result) =>
            {
                result.Should().BeAssignableTo<ConstantExpression>();
                if ( result is not ConstantExpression constantResult )
                    return;

                constantResult.Value.Should().Be( 579 );
            } );
    }

    [Fact]
    public void AddOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstantAndNotEqualToZero()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant<int, int, int>(
            sut: new ParsedExpressionAddInt32Operator(),
            expectedNodeType: ExpressionType.Add,
            leftValue: Fixture.CreateNotDefault<int>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void AddOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstantAndEqualToZero()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant<int, int, int>(
            sut: new ParsedExpressionAddInt32Operator(),
            expectedNodeType: ExpressionType.Parameter,
            leftValue: 0,
            (_, right, result) => result.Should().BeSameAs( right ) );
    }

    [Fact]
    public void AddOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstantAndNotEqualToZero()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant<int, int, int>(
            sut: new ParsedExpressionAddInt32Operator(),
            expectedNodeType: ExpressionType.Add,
            rightValue: Fixture.CreateNotDefault<int>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void AddOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstantAndEqualToZero()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant<int, int, int>(
            sut: new ParsedExpressionAddInt32Operator(),
            expectedNodeType: ExpressionType.Parameter,
            rightValue: 0,
            (left, _, result) => result.Should().BeSameAs( left ) );
    }

    [Fact]
    public void SubtractOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable<int, int, int>(
            sut: new ParsedExpressionSubtractInt32Operator(),
            expectedNodeType: ExpressionType.Subtract,
            DefaultNodeAssertion );
    }

    [Fact]
    public void SubtractOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant<int, int, int>(
            sut: new ParsedExpressionSubtractInt32Operator(),
            expectedNodeType: ExpressionType.Constant,
            leftValue: 123,
            rightValue: 456,
            (_, _, result) =>
            {
                result.Should().BeAssignableTo<ConstantExpression>();
                if ( result is not ConstantExpression constantResult )
                    return;

                constantResult.Value.Should().Be( -333 );
            } );
    }

    [Fact]
    public void SubtractOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstantAndNotEqualToZero()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant<int, int, int>(
            sut: new ParsedExpressionSubtractInt32Operator(),
            expectedNodeType: ExpressionType.Subtract,
            leftValue: Fixture.CreateNotDefault<int>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void SubtractOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstantAndEqualToZero()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant<int, int, int>(
            sut: new ParsedExpressionSubtractInt32Operator(),
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
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant<int, int, int>(
            sut: new ParsedExpressionSubtractInt32Operator(),
            expectedNodeType: ExpressionType.Subtract,
            rightValue: Fixture.CreateNotDefault<int>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void SubtractOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstantAndEqualToZero()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant<int, int, int>(
            sut: new ParsedExpressionSubtractInt32Operator(),
            expectedNodeType: ExpressionType.Parameter,
            rightValue: 0,
            (left, _, result) => result.Should().BeSameAs( left ) );
    }

    [Fact]
    public void MultiplyOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable<int, int, int>(
            sut: new ParsedExpressionMultiplyInt32Operator(),
            expectedNodeType: ExpressionType.Multiply,
            DefaultNodeAssertion );
    }

    [Fact]
    public void MultiplyOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant<int, int, int>(
            sut: new ParsedExpressionMultiplyInt32Operator(),
            expectedNodeType: ExpressionType.Constant,
            leftValue: 123,
            rightValue: 456,
            (_, _, result) =>
            {
                result.Should().BeAssignableTo<ConstantExpression>();
                if ( result is not ConstantExpression constantResult )
                    return;

                constantResult.Value.Should().Be( 56088 );
            } );
    }

    [Fact]
    public void MultiplyOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstantAndNotEqualToZeroOrOneOrMinusOne()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant<int, int, int>(
            sut: new ParsedExpressionMultiplyInt32Operator(),
            expectedNodeType: ExpressionType.Multiply,
            leftValue: 123,
            DefaultNodeAssertion );
    }

    [Fact]
    public void MultiplyOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstantAndEqualToZero()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant<int, int, int>(
            sut: new ParsedExpressionMultiplyInt32Operator(),
            expectedNodeType: ExpressionType.Constant,
            leftValue: 0,
            (_, _, result) =>
            {
                result.Should().BeAssignableTo<ConstantExpression>();
                if ( result is not ConstantExpression constantResult )
                    return;

                constantResult.Value.Should().Be( 0 );
            } );
    }

    [Fact]
    public void MultiplyOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstantAndEqualToOne()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant<int, int, int>(
            sut: new ParsedExpressionMultiplyInt32Operator(),
            expectedNodeType: ExpressionType.Parameter,
            leftValue: 1,
            (_, right, result) => result.Should().BeSameAs( right ) );
    }

    [Fact]
    public void MultiplyOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstantAndEqualToMinusOne()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant<int, int, int>(
            sut: new ParsedExpressionMultiplyInt32Operator(),
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
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant<int, int, int>(
            sut: new ParsedExpressionMultiplyInt32Operator(),
            expectedNodeType: ExpressionType.Multiply,
            rightValue: 123,
            DefaultNodeAssertion );
    }

    [Fact]
    public void MultiplyOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstantAndEqualToZero()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant<int, int, int>(
            sut: new ParsedExpressionMultiplyInt32Operator(),
            expectedNodeType: ExpressionType.Constant,
            rightValue: 0,
            (_, _, result) =>
            {
                result.Should().BeAssignableTo<ConstantExpression>();
                if ( result is not ConstantExpression constantResult )
                    return;

                constantResult.Value.Should().Be( 0 );
            } );
    }

    [Fact]
    public void MultiplyOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstantAndEqualToOne()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant<int, int, int>(
            sut: new ParsedExpressionMultiplyInt32Operator(),
            expectedNodeType: ExpressionType.Parameter,
            rightValue: 1,
            (left, _, result) => result.Should().BeSameAs( left ) );
    }

    [Fact]
    public void MultiplyOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstantAndEqualToMinusOne()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant<int, int, int>(
            sut: new ParsedExpressionMultiplyInt32Operator(),
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
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable<int, int, int>(
            sut: new ParsedExpressionDivideInt32Operator(),
            expectedNodeType: ExpressionType.Divide,
            DefaultNodeAssertion );
    }

    [Fact]
    public void DivideOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant<int, int, int>(
            sut: new ParsedExpressionDivideInt32Operator(),
            expectedNodeType: ExpressionType.Constant,
            leftValue: 1236,
            rightValue: 4,
            (_, _, result) =>
            {
                result.Should().BeAssignableTo<ConstantExpression>();
                if ( result is not ConstantExpression constantResult )
                    return;

                constantResult.Value.Should().Be( 309 );
            } );
    }

    [Fact]
    public void DivideOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant<int, int, int>(
            sut: new ParsedExpressionDivideInt32Operator(),
            expectedNodeType: ExpressionType.Divide,
            leftValue: Fixture.Create<int>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void DivideOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstantAndNotEqualToZeroOrOneOrMinusOne()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant<int, int, int>(
            sut: new ParsedExpressionDivideInt32Operator(),
            expectedNodeType: ExpressionType.Divide,
            rightValue: 123,
            DefaultNodeAssertion );
    }

    [Fact]
    public void DivideOperatorProcess_ShouldThrowDivideByZeroException_WhenRightOperandIsConstantAndEqualToZero()
    {
        Process_ShouldThrowException_WhenAttemptingToResolveRightConstantValue<int, int, DivideByZeroException>(
            sut: new ParsedExpressionDivideInt32Operator(),
            rightValue: 0 );
    }

    [Fact]
    public void DivideOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstantAndEqualToOne()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant<int, int, int>(
            sut: new ParsedExpressionDivideInt32Operator(),
            expectedNodeType: ExpressionType.Parameter,
            rightValue: 1,
            (left, _, result) => result.Should().BeSameAs( left ) );
    }

    [Fact]
    public void DivideOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstantAndEqualToMinusOne()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant<int, int, int>(
            sut: new ParsedExpressionDivideInt32Operator(),
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
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable<int, int, int>(
            sut: new ParsedExpressionModuloInt32Operator(),
            expectedNodeType: ExpressionType.Modulo,
            DefaultNodeAssertion );
    }

    [Fact]
    public void ModuloOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant<int, int, int>(
            sut: new ParsedExpressionModuloInt32Operator(),
            expectedNodeType: ExpressionType.Constant,
            leftValue: 456,
            rightValue: 123,
            (_, _, result) =>
            {
                result.Should().BeAssignableTo<ConstantExpression>();
                if ( result is not ConstantExpression constantResult )
                    return;

                constantResult.Value.Should().Be( 87 );
            } );
    }

    [Fact]
    public void ModuloOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant<int, int, int>(
            sut: new ParsedExpressionModuloInt32Operator(),
            expectedNodeType: ExpressionType.Modulo,
            leftValue: Fixture.Create<int>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void ModuloOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstantAndNotEqualToZeroOrOneOrMinusOne()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant<int, int, int>(
            sut: new ParsedExpressionModuloInt32Operator(),
            expectedNodeType: ExpressionType.Modulo,
            rightValue: 123,
            DefaultNodeAssertion );
    }

    [Fact]
    public void ModuloOperatorProcess_ShouldThrowDivideByZeroException_WhenRightOperandIsConstantAndEqualToZero()
    {
        Process_ShouldThrowException_WhenAttemptingToResolveRightConstantValue<int, int, DivideByZeroException>(
            sut: new ParsedExpressionModuloInt32Operator(),
            rightValue: 0 );
    }

    [Theory]
    [InlineData( 1 )]
    [InlineData( -1 )]
    public void ModuloOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstantAndEqualToOneOrMinusOne(int right)
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant<int, int, int>(
            sut: new ParsedExpressionModuloInt32Operator(),
            expectedNodeType: ExpressionType.Constant,
            rightValue: right,
            (_, _, result) =>
            {
                result.Should().BeAssignableTo<ConstantExpression>();
                if ( result is not ConstantExpression constantResult )
                    return;

                constantResult.Value.Should().Be( 0 );
            } );
    }

    [Fact]
    public void BitwiseAndOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable<int, int, int>(
            sut: new ParsedExpressionBitwiseAndInt32Operator(),
            expectedNodeType: ExpressionType.And,
            DefaultNodeAssertion );
    }

    [Fact]
    public void BitwiseAndOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant<int, int, int>(
            sut: new ParsedExpressionBitwiseAndInt32Operator(),
            expectedNodeType: ExpressionType.Constant,
            leftValue: 123,
            rightValue: 456,
            (_, _, result) =>
            {
                result.Should().BeAssignableTo<ConstantExpression>();
                if ( result is not ConstantExpression constantExpression )
                    return;

                constantExpression.Value.Should().Be( 72 );
            } );
    }

    [Fact]
    public void BitwiseAndOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstantAndNotEqualToZero()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant<int, int, int>(
            sut: new ParsedExpressionBitwiseAndInt32Operator(),
            expectedNodeType: ExpressionType.And,
            leftValue: Fixture.CreateNotDefault<int>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void BitwiseAndOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstantAndEqualToZero()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant<int, int, int>(
            sut: new ParsedExpressionBitwiseAndInt32Operator(),
            expectedNodeType: ExpressionType.Constant,
            leftValue: 0,
            (_, _, result) =>
            {
                result.Should().BeAssignableTo<ConstantExpression>();
                if ( result is not ConstantExpression constantExpression )
                    return;

                constantExpression.Value.Should().Be( 0 );
            } );
    }

    [Fact]
    public void BitwiseAndOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstantAndNotEqualToZero()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant<int, int, int>(
            sut: new ParsedExpressionBitwiseAndInt32Operator(),
            expectedNodeType: ExpressionType.And,
            rightValue: Fixture.CreateNotDefault<int>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void BitwiseAndOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstantAndEqualToZero()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant<int, int, int>(
            sut: new ParsedExpressionBitwiseAndInt32Operator(),
            expectedNodeType: ExpressionType.Constant,
            rightValue: 0,
            (_, _, result) =>
            {
                result.Should().BeAssignableTo<ConstantExpression>();
                if ( result is not ConstantExpression constantExpression )
                    return;

                constantExpression.Value.Should().Be( 0 );
            } );
    }

    [Fact]
    public void BitwiseOrOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable<int, int, int>(
            sut: new ParsedExpressionBitwiseOrInt32Operator(),
            expectedNodeType: ExpressionType.Or,
            DefaultNodeAssertion );
    }

    [Fact]
    public void BitwiseOrOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant<int, int, int>(
            sut: new ParsedExpressionBitwiseOrInt32Operator(),
            expectedNodeType: ExpressionType.Constant,
            leftValue: 123,
            rightValue: 456,
            (_, _, result) =>
            {
                result.Should().BeAssignableTo<ConstantExpression>();
                if ( result is not ConstantExpression constantExpression )
                    return;

                constantExpression.Value.Should().Be( 507 );
            } );
    }

    [Fact]
    public void BitwiseOrOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstantAndNotEqualToZero()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant<int, int, int>(
            sut: new ParsedExpressionBitwiseOrInt32Operator(),
            expectedNodeType: ExpressionType.Or,
            leftValue: Fixture.CreateNotDefault<int>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void BitwiseOrOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstantAndEqualToZero()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant<int, int, int>(
            sut: new ParsedExpressionBitwiseOrInt32Operator(),
            expectedNodeType: ExpressionType.Parameter,
            leftValue: 0,
            (_, right, result) => result.Should().BeSameAs( right ) );
    }

    [Fact]
    public void BitwiseOrOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstantAndNotEqualToZero()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant<int, int, int>(
            sut: new ParsedExpressionBitwiseOrInt32Operator(),
            expectedNodeType: ExpressionType.Or,
            rightValue: Fixture.CreateNotDefault<int>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void BitwiseOrOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstantAndEqualToZero()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant<int, int, int>(
            sut: new ParsedExpressionBitwiseOrInt32Operator(),
            expectedNodeType: ExpressionType.Parameter,
            rightValue: 0,
            (left, _, result) => result.Should().BeSameAs( left ) );
    }

    [Fact]
    public void BitwiseXorOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable<int, int, int>(
            sut: new ParsedExpressionBitwiseXorInt32Operator(),
            expectedNodeType: ExpressionType.ExclusiveOr,
            DefaultNodeAssertion );
    }

    [Fact]
    public void BitwiseXorOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant<int, int, int>(
            sut: new ParsedExpressionBitwiseXorInt32Operator(),
            expectedNodeType: ExpressionType.Constant,
            leftValue: 123,
            rightValue: 456,
            (_, _, result) =>
            {
                result.Should().BeAssignableTo<ConstantExpression>();
                if ( result is not ConstantExpression constantExpression )
                    return;

                constantExpression.Value.Should().Be( 435 );
            } );
    }

    [Fact]
    public void BitwiseXorOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstantAndNotEqualToZero()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant<int, int, int>(
            sut: new ParsedExpressionBitwiseXorInt32Operator(),
            expectedNodeType: ExpressionType.ExclusiveOr,
            leftValue: Fixture.CreateNotDefault<int>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void BitwiseXorOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstantAndEqualToZero()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant<int, int, int>(
            sut: new ParsedExpressionBitwiseXorInt32Operator(),
            expectedNodeType: ExpressionType.Parameter,
            leftValue: 0,
            (_, right, result) => result.Should().BeSameAs( right ) );
    }

    [Fact]
    public void BitwiseXorOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstantAndNotEqualToZero()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant<int, int, int>(
            sut: new ParsedExpressionBitwiseXorInt32Operator(),
            expectedNodeType: ExpressionType.ExclusiveOr,
            rightValue: Fixture.CreateNotDefault<int>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void BitwiseXorOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstantAndEqualToZero()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant<int, int, int>(
            sut: new ParsedExpressionBitwiseXorInt32Operator(),
            expectedNodeType: ExpressionType.Parameter,
            rightValue: 0,
            (left, _, result) => result.Should().BeSameAs( left ) );
    }

    [Fact]
    public void BitwiseLeftShiftOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable<int, int, int>(
            sut: new ParsedExpressionBitwiseLeftShiftInt32Operator(),
            expectedNodeType: ExpressionType.LeftShift,
            DefaultNodeAssertion );
    }

    [Fact]
    public void BitwiseLeftShiftOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant<int, int, int>(
            sut: new ParsedExpressionBitwiseLeftShiftInt32Operator(),
            expectedNodeType: ExpressionType.Constant,
            leftValue: 456,
            rightValue: 12,
            (_, _, result) =>
            {
                result.Should().BeAssignableTo<ConstantExpression>();
                if ( result is not ConstantExpression constantExpression )
                    return;

                constantExpression.Value.Should().Be( 1867776 );
            } );
    }

    [Fact]
    public void BitwiseLeftShiftOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstantAndNotEqualToZero()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant<int, int, int>(
            sut: new ParsedExpressionBitwiseLeftShiftInt32Operator(),
            expectedNodeType: ExpressionType.LeftShift,
            leftValue: Fixture.CreateNotDefault<int>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void BitwiseLeftShiftOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstantAndEqualToZero()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant<int, int, int>(
            sut: new ParsedExpressionBitwiseLeftShiftInt32Operator(),
            expectedNodeType: ExpressionType.Constant,
            leftValue: 0,
            (_, _, result) =>
            {
                result.Should().BeAssignableTo<ConstantExpression>();
                if ( result is not ConstantExpression constantResult )
                    return;

                constantResult.Value.Should().Be( 0 );
            } );
    }

    [Theory]
    [InlineData( 1 )]
    [InlineData( 31 )]
    [InlineData( 33 )]
    [InlineData( -1 )]
    [InlineData( -31 )]
    [InlineData( -33 )]
    public void BitwiseLeftShiftOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstantAndNotEqualToZeroShift(
        int shift)
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant<int, int, int>(
            sut: new ParsedExpressionBitwiseLeftShiftInt32Operator(),
            expectedNodeType: ExpressionType.LeftShift,
            rightValue: shift,
            DefaultNodeAssertion );
    }

    [Theory]
    [InlineData( 0 )]
    [InlineData( -32 )]
    [InlineData( -64 )]
    [InlineData( 32 )]
    [InlineData( 64 )]
    public void BitwiseLeftShiftOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstantAndEqualToZeroShift(
        int shift)
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant<int, int, int>(
            sut: new ParsedExpressionBitwiseLeftShiftInt32Operator(),
            expectedNodeType: ExpressionType.Parameter,
            rightValue: shift,
            (left, _, result) => result.Should().BeSameAs( left ) );
    }

    [Fact]
    public void BitwiseRightShiftOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable<int, int, int>(
            sut: new ParsedExpressionBitwiseRightShiftInt32Operator(),
            expectedNodeType: ExpressionType.RightShift,
            DefaultNodeAssertion );
    }

    [Fact]
    public void BitwiseRightShiftOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant<int, int, int>(
            sut: new ParsedExpressionBitwiseRightShiftInt32Operator(),
            expectedNodeType: ExpressionType.Constant,
            leftValue: 1867776,
            rightValue: 12,
            (_, _, result) =>
            {
                result.Should().BeAssignableTo<ConstantExpression>();
                if ( result is not ConstantExpression constantExpression )
                    return;

                constantExpression.Value.Should().Be( 456 );
            } );
    }

    [Fact]
    public void BitwiseRightShiftOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstantAndNotEqualToZero()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant<int, int, int>(
            sut: new ParsedExpressionBitwiseRightShiftInt32Operator(),
            expectedNodeType: ExpressionType.RightShift,
            leftValue: Fixture.CreateNotDefault<int>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void BitwiseRightShiftOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstantAndEqualToZero()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant<int, int, int>(
            sut: new ParsedExpressionBitwiseRightShiftInt32Operator(),
            expectedNodeType: ExpressionType.Constant,
            leftValue: 0,
            (_, _, result) =>
            {
                result.Should().BeAssignableTo<ConstantExpression>();
                if ( result is not ConstantExpression constantResult )
                    return;

                constantResult.Value.Should().Be( 0 );
            } );
    }

    [Theory]
    [InlineData( 1 )]
    [InlineData( 31 )]
    [InlineData( 33 )]
    [InlineData( -1 )]
    [InlineData( -31 )]
    [InlineData( -33 )]
    public void BitwiseRightShiftOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstantAndNotEqualToZeroShift(
        int shift)
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant<int, int, int>(
            sut: new ParsedExpressionBitwiseRightShiftInt32Operator(),
            expectedNodeType: ExpressionType.RightShift,
            rightValue: shift,
            DefaultNodeAssertion );
    }

    [Theory]
    [InlineData( 0 )]
    [InlineData( -32 )]
    [InlineData( -64 )]
    [InlineData( 32 )]
    [InlineData( 64 )]
    public void BitwiseRightShiftOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstantAndEqualToZeroShift(
        int shift)
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant<int, int, int>(
            sut: new ParsedExpressionBitwiseRightShiftInt32Operator(),
            expectedNodeType: ExpressionType.Parameter,
            rightValue: shift,
            (left, _, result) => result.Should().BeSameAs( left ) );
    }

    [Fact]
    public void EqualToOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable<int, int, bool>(
            sut: new ParsedExpressionEqualToInt32Operator(),
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
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant<int, int, bool>(
            sut: new ParsedExpressionEqualToInt32Operator(),
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
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant<int, int, bool>(
            sut: new ParsedExpressionEqualToInt32Operator(),
            expectedNodeType: ExpressionType.Equal,
            leftValue: Fixture.Create<int>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void EqualToOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant<int, int, bool>(
            sut: new ParsedExpressionEqualToInt32Operator(),
            expectedNodeType: ExpressionType.Equal,
            rightValue: Fixture.Create<int>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void NotEqualToOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable<int, int, bool>(
            sut: new ParsedExpressionNotEqualToInt32Operator(),
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
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant<int, int, bool>(
            sut: new ParsedExpressionNotEqualToInt32Operator(),
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
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant<int, int, bool>(
            sut: new ParsedExpressionNotEqualToInt32Operator(),
            expectedNodeType: ExpressionType.NotEqual,
            leftValue: Fixture.Create<int>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void NotEqualToOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant<int, int, bool>(
            sut: new ParsedExpressionNotEqualToInt32Operator(),
            expectedNodeType: ExpressionType.NotEqual,
            rightValue: Fixture.Create<int>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void GreaterThanOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable<int, int, bool>(
            sut: new ParsedExpressionGreaterThanInt32Operator(),
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
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant<int, int, bool>(
            sut: new ParsedExpressionGreaterThanInt32Operator(),
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
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant<int, int, bool>(
            sut: new ParsedExpressionGreaterThanInt32Operator(),
            expectedNodeType: ExpressionType.GreaterThan,
            leftValue: Fixture.Create<int>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void GreaterThanOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant<int, int, bool>(
            sut: new ParsedExpressionGreaterThanInt32Operator(),
            expectedNodeType: ExpressionType.GreaterThan,
            rightValue: Fixture.Create<int>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void LessThanOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable<int, int, bool>(
            sut: new ParsedExpressionLessThanInt32Operator(),
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
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant<int, int, bool>(
            sut: new ParsedExpressionLessThanInt32Operator(),
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
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant<int, int, bool>(
            sut: new ParsedExpressionLessThanInt32Operator(),
            expectedNodeType: ExpressionType.LessThan,
            leftValue: Fixture.Create<int>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void LessThanOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant<int, int, bool>(
            sut: new ParsedExpressionLessThanInt32Operator(),
            expectedNodeType: ExpressionType.LessThan,
            rightValue: Fixture.Create<int>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void GreaterThanOrEqualToOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable<int, int, bool>(
            sut: new ParsedExpressionGreaterThanOrEqualToInt32Operator(),
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
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant<int, int, bool>(
            sut: new ParsedExpressionGreaterThanOrEqualToInt32Operator(),
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
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant<int, int, bool>(
            sut: new ParsedExpressionGreaterThanOrEqualToInt32Operator(),
            expectedNodeType: ExpressionType.GreaterThanOrEqual,
            leftValue: Fixture.Create<int>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void GreaterThanOrEqualToOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant<int, int, bool>(
            sut: new ParsedExpressionGreaterThanOrEqualToInt32Operator(),
            expectedNodeType: ExpressionType.GreaterThanOrEqual,
            rightValue: Fixture.Create<int>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void LessThanOrEqualToOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable<int, int, bool>(
            sut: new ParsedExpressionLessThanOrEqualToInt32Operator(),
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
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant<int, int, bool>(
            sut: new ParsedExpressionLessThanOrEqualToInt32Operator(),
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
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant<int, int, bool>(
            sut: new ParsedExpressionLessThanOrEqualToInt32Operator(),
            expectedNodeType: ExpressionType.LessThanOrEqual,
            leftValue: Fixture.Create<int>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void LessThanOrEqualToOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant<int, int, bool>(
            sut: new ParsedExpressionLessThanOrEqualToInt32Operator(),
            expectedNodeType: ExpressionType.LessThanOrEqual,
            rightValue: Fixture.Create<int>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void CompareOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable<int, int, int>(
            sut: new ParsedExpressionCompareInt32Operator(),
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
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant<int, int, int>(
            sut: new ParsedExpressionCompareInt32Operator(),
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
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant<int, int, int>(
            sut: new ParsedExpressionCompareInt32Operator(),
            expectedNodeType: ExpressionType.Call,
            leftValue: Fixture.Create<int>(),
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
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant<int, int, int>(
            sut: new ParsedExpressionCompareInt32Operator(),
            expectedNodeType: ExpressionType.Call,
            rightValue: Fixture.Create<int>(),
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
