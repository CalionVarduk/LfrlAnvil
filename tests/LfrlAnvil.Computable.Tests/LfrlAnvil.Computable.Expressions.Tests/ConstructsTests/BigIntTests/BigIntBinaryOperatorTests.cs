﻿using System;
using System.Linq;
using System.Linq.Expressions;
using System.Numerics;
using AutoFixture;
using FluentAssertions;
using LfrlAnvil.Computable.Expressions.Constructs.BigInt;
using LfrlAnvil.TestExtensions;
using Xunit;

namespace LfrlAnvil.Computable.Expressions.Tests.ConstructsTests.BigIntTests;

public class BigIntBinaryOperatorTests : BinaryOperatorsTestsBase
{
    [Fact]
    public void AddOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable<BigInteger, BigInteger, BigInteger>(
            sut: new ParsedExpressionAddBigIntOperator(),
            expectedNodeType: ExpressionType.Add,
            DefaultNodeAssertion );
    }

    [Fact]
    public void AddOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant<BigInteger, BigInteger, BigInteger>(
            sut: new ParsedExpressionAddBigIntOperator(),
            expectedNodeType: ExpressionType.Constant,
            leftValue: 123,
            rightValue: 456,
            (_, _, result) =>
            {
                result.Should().BeAssignableTo<ConstantExpression>();
                if ( result is not ConstantExpression constantResult )
                    return;

                constantResult.Value.Should().Be( new BigInteger( 579 ) );
            } );
    }

    [Fact]
    public void AddOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstantAndNotEqualToZero()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant<BigInteger, BigInteger, BigInteger>(
            sut: new ParsedExpressionAddBigIntOperator(),
            expectedNodeType: ExpressionType.Add,
            leftValue: Fixture.CreateNotDefault<BigInteger>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void AddOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstantAndEqualToZero()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant<BigInteger, BigInteger, BigInteger>(
            sut: new ParsedExpressionAddBigIntOperator(),
            expectedNodeType: ExpressionType.Parameter,
            leftValue: 0,
            (_, right, result) => result.Should().BeSameAs( right ) );
    }

    [Fact]
    public void AddOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstantAndNotEqualToZero()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant<BigInteger, BigInteger, BigInteger>(
            sut: new ParsedExpressionAddBigIntOperator(),
            expectedNodeType: ExpressionType.Add,
            rightValue: Fixture.CreateNotDefault<BigInteger>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void AddOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstantAndEqualToZero()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant<BigInteger, BigInteger, BigInteger>(
            sut: new ParsedExpressionAddBigIntOperator(),
            expectedNodeType: ExpressionType.Parameter,
            rightValue: 0,
            (left, _, result) => result.Should().BeSameAs( left ) );
    }

    [Fact]
    public void SubtractOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable<BigInteger, BigInteger, BigInteger>(
            sut: new ParsedExpressionSubtractBigIntOperator(),
            expectedNodeType: ExpressionType.Subtract,
            DefaultNodeAssertion );
    }

    [Fact]
    public void SubtractOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant<BigInteger, BigInteger, BigInteger>(
            sut: new ParsedExpressionSubtractBigIntOperator(),
            expectedNodeType: ExpressionType.Constant,
            leftValue: 123,
            rightValue: 456,
            (_, _, result) =>
            {
                result.Should().BeAssignableTo<ConstantExpression>();
                if ( result is not ConstantExpression constantResult )
                    return;

                constantResult.Value.Should().Be( new BigInteger( -333 ) );
            } );
    }

    [Fact]
    public void SubtractOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstantAndNotEqualToZero()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant<BigInteger, BigInteger, BigInteger>(
            sut: new ParsedExpressionSubtractBigIntOperator(),
            expectedNodeType: ExpressionType.Subtract,
            leftValue: Fixture.CreateNotDefault<BigInteger>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void SubtractOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstantAndEqualToZero()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant<BigInteger, BigInteger, BigInteger>(
            sut: new ParsedExpressionSubtractBigIntOperator(),
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
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant<BigInteger, BigInteger, BigInteger>(
            sut: new ParsedExpressionSubtractBigIntOperator(),
            expectedNodeType: ExpressionType.Subtract,
            rightValue: Fixture.CreateNotDefault<BigInteger>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void SubtractOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstantAndEqualToZero()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant<BigInteger, BigInteger, BigInteger>(
            sut: new ParsedExpressionSubtractBigIntOperator(),
            expectedNodeType: ExpressionType.Parameter,
            rightValue: 0,
            (left, _, result) => result.Should().BeSameAs( left ) );
    }

    [Fact]
    public void MultiplyOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable<BigInteger, BigInteger, BigInteger>(
            sut: new ParsedExpressionMultiplyBigIntOperator(),
            expectedNodeType: ExpressionType.Multiply,
            DefaultNodeAssertion );
    }

    [Fact]
    public void MultiplyOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant<BigInteger, BigInteger, BigInteger>(
            sut: new ParsedExpressionMultiplyBigIntOperator(),
            expectedNodeType: ExpressionType.Constant,
            leftValue: 123,
            rightValue: 456,
            (_, _, result) =>
            {
                result.Should().BeAssignableTo<ConstantExpression>();
                if ( result is not ConstantExpression constantResult )
                    return;

                constantResult.Value.Should().Be( new BigInteger( 56088 ) );
            } );
    }

    [Fact]
    public void MultiplyOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstantAndNotEqualToZeroOrOneOrMinusOne()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant<BigInteger, BigInteger, BigInteger>(
            sut: new ParsedExpressionMultiplyBigIntOperator(),
            expectedNodeType: ExpressionType.Multiply,
            leftValue: 123,
            DefaultNodeAssertion );
    }

    [Fact]
    public void MultiplyOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstantAndEqualToZero()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant<BigInteger, BigInteger, BigInteger>(
            sut: new ParsedExpressionMultiplyBigIntOperator(),
            expectedNodeType: ExpressionType.Constant,
            leftValue: 0,
            (_, _, result) =>
            {
                result.Should().BeAssignableTo<ConstantExpression>();
                if ( result is not ConstantExpression constantResult )
                    return;

                constantResult.Value.Should().Be( BigInteger.Zero );
            } );
    }

    [Fact]
    public void MultiplyOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstantAndEqualToOne()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant<BigInteger, BigInteger, BigInteger>(
            sut: new ParsedExpressionMultiplyBigIntOperator(),
            expectedNodeType: ExpressionType.Parameter,
            leftValue: 1,
            (_, right, result) => result.Should().BeSameAs( right ) );
    }

    [Fact]
    public void MultiplyOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstantAndEqualToMinusOne()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant<BigInteger, BigInteger, BigInteger>(
            sut: new ParsedExpressionMultiplyBigIntOperator(),
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
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant<BigInteger, BigInteger, BigInteger>(
            sut: new ParsedExpressionMultiplyBigIntOperator(),
            expectedNodeType: ExpressionType.Multiply,
            rightValue: 123,
            DefaultNodeAssertion );
    }

    [Fact]
    public void MultiplyOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstantAndEqualToZero()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant<BigInteger, BigInteger, BigInteger>(
            sut: new ParsedExpressionMultiplyBigIntOperator(),
            expectedNodeType: ExpressionType.Constant,
            rightValue: 0,
            (_, _, result) =>
            {
                result.Should().BeAssignableTo<ConstantExpression>();
                if ( result is not ConstantExpression constantResult )
                    return;

                constantResult.Value.Should().Be( BigInteger.Zero );
            } );
    }

    [Fact]
    public void MultiplyOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstantAndEqualToOne()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant<BigInteger, BigInteger, BigInteger>(
            sut: new ParsedExpressionMultiplyBigIntOperator(),
            expectedNodeType: ExpressionType.Parameter,
            rightValue: 1,
            (left, _, result) => result.Should().BeSameAs( left ) );
    }

    [Fact]
    public void MultiplyOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstantAndEqualToMinusOne()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant<BigInteger, BigInteger, BigInteger>(
            sut: new ParsedExpressionMultiplyBigIntOperator(),
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
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable<BigInteger, BigInteger, BigInteger>(
            sut: new ParsedExpressionDivideBigIntOperator(),
            expectedNodeType: ExpressionType.Divide,
            DefaultNodeAssertion );
    }

    [Fact]
    public void DivideOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant<BigInteger, BigInteger, BigInteger>(
            sut: new ParsedExpressionDivideBigIntOperator(),
            expectedNodeType: ExpressionType.Constant,
            leftValue: 1236,
            rightValue: 4,
            (_, _, result) =>
            {
                result.Should().BeAssignableTo<ConstantExpression>();
                if ( result is not ConstantExpression constantResult )
                    return;

                constantResult.Value.Should().Be( new BigInteger( 309 ) );
            } );
    }

    [Fact]
    public void DivideOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant<BigInteger, BigInteger, BigInteger>(
            sut: new ParsedExpressionDivideBigIntOperator(),
            expectedNodeType: ExpressionType.Divide,
            leftValue: Fixture.Create<BigInteger>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void DivideOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstantAndNotEqualToZeroOrOneOrMinusOne()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant<BigInteger, BigInteger, BigInteger>(
            sut: new ParsedExpressionDivideBigIntOperator(),
            expectedNodeType: ExpressionType.Divide,
            rightValue: 123,
            DefaultNodeAssertion );
    }

    [Fact]
    public void DivideOperatorProcess_ShouldThrowDivideByZeroException_WhenRightOperandIsConstantAndEqualToZero()
    {
        Process_ShouldThrowException_WhenAttemptingToResolveRightConstantValue<BigInteger, BigInteger, DivideByZeroException>(
            sut: new ParsedExpressionDivideBigIntOperator(),
            rightValue: 0 );
    }

    [Fact]
    public void DivideOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstantAndEqualToOne()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant<BigInteger, BigInteger, BigInteger>(
            sut: new ParsedExpressionDivideBigIntOperator(),
            expectedNodeType: ExpressionType.Parameter,
            rightValue: 1,
            (left, _, result) => result.Should().BeSameAs( left ) );
    }

    [Fact]
    public void DivideOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstantAndEqualToMinusOne()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant<BigInteger, BigInteger, BigInteger>(
            sut: new ParsedExpressionDivideBigIntOperator(),
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
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable<BigInteger, BigInteger, BigInteger>(
            sut: new ParsedExpressionModuloBigIntOperator(),
            expectedNodeType: ExpressionType.Modulo,
            DefaultNodeAssertion );
    }

    [Fact]
    public void ModuloOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant<BigInteger, BigInteger, BigInteger>(
            sut: new ParsedExpressionModuloBigIntOperator(),
            expectedNodeType: ExpressionType.Constant,
            leftValue: 456,
            rightValue: 123,
            (_, _, result) =>
            {
                result.Should().BeAssignableTo<ConstantExpression>();
                if ( result is not ConstantExpression constantResult )
                    return;

                constantResult.Value.Should().Be( new BigInteger( 87 ) );
            } );
    }

    [Fact]
    public void ModuloOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant<BigInteger, BigInteger, BigInteger>(
            sut: new ParsedExpressionModuloBigIntOperator(),
            expectedNodeType: ExpressionType.Modulo,
            leftValue: Fixture.Create<BigInteger>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void ModuloOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstantAndNotEqualToZeroOrOneOrMinusOne()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant<BigInteger, BigInteger, BigInteger>(
            sut: new ParsedExpressionModuloBigIntOperator(),
            expectedNodeType: ExpressionType.Modulo,
            rightValue: 123,
            DefaultNodeAssertion );
    }

    [Fact]
    public void ModuloOperatorProcess_ShouldThrowDivideByZeroException_WhenRightOperandIsConstantAndEqualToZero()
    {
        Process_ShouldThrowException_WhenAttemptingToResolveRightConstantValue<BigInteger, BigInteger, DivideByZeroException>(
            sut: new ParsedExpressionModuloBigIntOperator(),
            rightValue: 0 );
    }

    [Theory]
    [InlineData( 1 )]
    [InlineData( -1 )]
    public void ModuloOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstantAndEqualToOneOrMinusOne(
        BigInteger right)
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant<BigInteger, BigInteger, BigInteger>(
            sut: new ParsedExpressionModuloBigIntOperator(),
            expectedNodeType: ExpressionType.Constant,
            rightValue: right,
            (_, _, result) =>
            {
                result.Should().BeAssignableTo<ConstantExpression>();
                if ( result is not ConstantExpression constantResult )
                    return;

                constantResult.Value.Should().Be( BigInteger.Zero );
            } );
    }

    [Fact]
    public void BitwiseAndOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable<BigInteger, BigInteger, BigInteger>(
            sut: new ParsedExpressionBitwiseAndBigIntOperator(),
            expectedNodeType: ExpressionType.And,
            DefaultNodeAssertion );
    }

    [Fact]
    public void BitwiseAndOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant<BigInteger, BigInteger, BigInteger>(
            sut: new ParsedExpressionBitwiseAndBigIntOperator(),
            expectedNodeType: ExpressionType.Constant,
            leftValue: 123,
            rightValue: 456,
            (_, _, result) =>
            {
                result.Should().BeAssignableTo<ConstantExpression>();
                if ( result is not ConstantExpression constantExpression )
                    return;

                constantExpression.Value.Should().Be( new BigInteger( 72 ) );
            } );
    }

    [Fact]
    public void BitwiseAndOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstantAndNotEqualToZero()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant<BigInteger, BigInteger, BigInteger>(
            sut: new ParsedExpressionBitwiseAndBigIntOperator(),
            expectedNodeType: ExpressionType.And,
            leftValue: Fixture.CreateNotDefault<BigInteger>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void BitwiseAndOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstantAndEqualToZero()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant<BigInteger, BigInteger, BigInteger>(
            sut: new ParsedExpressionBitwiseAndBigIntOperator(),
            expectedNodeType: ExpressionType.Constant,
            leftValue: 0,
            (_, _, result) =>
            {
                result.Should().BeAssignableTo<ConstantExpression>();
                if ( result is not ConstantExpression constantExpression )
                    return;

                constantExpression.Value.Should().Be( BigInteger.Zero );
            } );
    }

    [Fact]
    public void BitwiseAndOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstantAndNotEqualToZero()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant<BigInteger, BigInteger, BigInteger>(
            sut: new ParsedExpressionBitwiseAndBigIntOperator(),
            expectedNodeType: ExpressionType.And,
            rightValue: Fixture.CreateNotDefault<BigInteger>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void BitwiseAndOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstantAndEqualToZero()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant<BigInteger, BigInteger, BigInteger>(
            sut: new ParsedExpressionBitwiseAndBigIntOperator(),
            expectedNodeType: ExpressionType.Constant,
            rightValue: 0,
            (_, _, result) =>
            {
                result.Should().BeAssignableTo<ConstantExpression>();
                if ( result is not ConstantExpression constantExpression )
                    return;

                constantExpression.Value.Should().Be( BigInteger.Zero );
            } );
    }

    [Fact]
    public void BitwiseOrOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable<BigInteger, BigInteger, BigInteger>(
            sut: new ParsedExpressionBitwiseOrBigIntOperator(),
            expectedNodeType: ExpressionType.Or,
            DefaultNodeAssertion );
    }

    [Fact]
    public void BitwiseOrOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant<BigInteger, BigInteger, BigInteger>(
            sut: new ParsedExpressionBitwiseOrBigIntOperator(),
            expectedNodeType: ExpressionType.Constant,
            leftValue: 123,
            rightValue: 456,
            (_, _, result) =>
            {
                result.Should().BeAssignableTo<ConstantExpression>();
                if ( result is not ConstantExpression constantExpression )
                    return;

                constantExpression.Value.Should().Be( new BigInteger( 507 ) );
            } );
    }

    [Fact]
    public void BitwiseOrOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstantAndNotEqualToZero()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant<BigInteger, BigInteger, BigInteger>(
            sut: new ParsedExpressionBitwiseOrBigIntOperator(),
            expectedNodeType: ExpressionType.Or,
            leftValue: Fixture.CreateNotDefault<BigInteger>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void BitwiseOrOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstantAndEqualToZero()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant<BigInteger, BigInteger, BigInteger>(
            sut: new ParsedExpressionBitwiseOrBigIntOperator(),
            expectedNodeType: ExpressionType.Parameter,
            leftValue: 0,
            (_, right, result) => result.Should().BeSameAs( right ) );
    }

    [Fact]
    public void BitwiseOrOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstantAndNotEqualToZero()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant<BigInteger, BigInteger, BigInteger>(
            sut: new ParsedExpressionBitwiseOrBigIntOperator(),
            expectedNodeType: ExpressionType.Or,
            rightValue: Fixture.CreateNotDefault<BigInteger>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void BitwiseOrOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstantAndEqualToZero()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant<BigInteger, BigInteger, BigInteger>(
            sut: new ParsedExpressionBitwiseOrBigIntOperator(),
            expectedNodeType: ExpressionType.Parameter,
            rightValue: 0,
            (left, _, result) => result.Should().BeSameAs( left ) );
    }

    [Fact]
    public void BitwiseXorOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable<BigInteger, BigInteger, BigInteger>(
            sut: new ParsedExpressionBitwiseXorBigIntOperator(),
            expectedNodeType: ExpressionType.ExclusiveOr,
            DefaultNodeAssertion );
    }

    [Fact]
    public void BitwiseXorOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant<BigInteger, BigInteger, BigInteger>(
            sut: new ParsedExpressionBitwiseXorBigIntOperator(),
            expectedNodeType: ExpressionType.Constant,
            leftValue: 123,
            rightValue: 456,
            (_, _, result) =>
            {
                result.Should().BeAssignableTo<ConstantExpression>();
                if ( result is not ConstantExpression constantExpression )
                    return;

                constantExpression.Value.Should().Be( new BigInteger( 435 ) );
            } );
    }

    [Fact]
    public void BitwiseXorOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstantAndNotEqualToZero()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant<BigInteger, BigInteger, BigInteger>(
            sut: new ParsedExpressionBitwiseXorBigIntOperator(),
            expectedNodeType: ExpressionType.ExclusiveOr,
            leftValue: Fixture.CreateNotDefault<BigInteger>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void BitwiseXorOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstantAndEqualToZero()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant<BigInteger, BigInteger, BigInteger>(
            sut: new ParsedExpressionBitwiseXorBigIntOperator(),
            expectedNodeType: ExpressionType.Parameter,
            leftValue: 0,
            (_, right, result) => result.Should().BeSameAs( right ) );
    }

    [Fact]
    public void BitwiseXorOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstantAndNotEqualToZero()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant<BigInteger, BigInteger, BigInteger>(
            sut: new ParsedExpressionBitwiseXorBigIntOperator(),
            expectedNodeType: ExpressionType.ExclusiveOr,
            rightValue: Fixture.CreateNotDefault<BigInteger>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void BitwiseXorOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstantAndEqualToZero()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant<BigInteger, BigInteger, BigInteger>(
            sut: new ParsedExpressionBitwiseXorBigIntOperator(),
            expectedNodeType: ExpressionType.Parameter,
            rightValue: 0,
            (left, _, result) => result.Should().BeSameAs( left ) );
    }

    [Fact]
    public void BitwiseLeftShiftOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable<BigInteger, int, BigInteger>(
            sut: new ParsedExpressionBitwiseLeftShiftBigIntOperator(),
            expectedNodeType: ExpressionType.LeftShift,
            DefaultNodeAssertion );
    }

    [Fact]
    public void BitwiseLeftShiftOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant<BigInteger, int, BigInteger>(
            sut: new ParsedExpressionBitwiseLeftShiftBigIntOperator(),
            expectedNodeType: ExpressionType.Constant,
            leftValue: 456,
            rightValue: 12,
            (_, _, result) =>
            {
                result.Should().BeAssignableTo<ConstantExpression>();
                if ( result is not ConstantExpression constantExpression )
                    return;

                constantExpression.Value.Should().Be( new BigInteger( 1867776 ) );
            } );
    }

    [Fact]
    public void BitwiseLeftShiftOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstantAndNotEqualToZero()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant<BigInteger, int, BigInteger>(
            sut: new ParsedExpressionBitwiseLeftShiftBigIntOperator(),
            expectedNodeType: ExpressionType.LeftShift,
            leftValue: Fixture.CreateNotDefault<BigInteger>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void BitwiseLeftShiftOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstantAndEqualToZero()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant<BigInteger, int, BigInteger>(
            sut: new ParsedExpressionBitwiseLeftShiftBigIntOperator(),
            expectedNodeType: ExpressionType.Constant,
            leftValue: 0,
            (_, _, result) =>
            {
                result.Should().BeAssignableTo<ConstantExpression>();
                if ( result is not ConstantExpression constantResult )
                    return;

                constantResult.Value.Should().Be( BigInteger.Zero );
            } );
    }

    [Theory]
    [InlineData( 1 )]
    [InlineData( 63 )]
    [InlineData( 65 )]
    [InlineData( -1 )]
    [InlineData( -63 )]
    [InlineData( -65 )]
    public void BitwiseLeftShiftOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstantAndNotEqualToZero(
        int shift)
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant<BigInteger, int, BigInteger>(
            sut: new ParsedExpressionBitwiseLeftShiftBigIntOperator(),
            expectedNodeType: ExpressionType.LeftShift,
            rightValue: shift,
            DefaultNodeAssertion );
    }

    [Fact]
    public void BitwiseLeftShiftOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstantAndEqualToZero()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant<BigInteger, int, BigInteger>(
            sut: new ParsedExpressionBitwiseLeftShiftBigIntOperator(),
            expectedNodeType: ExpressionType.Parameter,
            rightValue: 0,
            (left, _, result) => result.Should().BeSameAs( left ) );
    }

    [Fact]
    public void BitwiseRightShiftOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable<BigInteger, int, BigInteger>(
            sut: new ParsedExpressionBitwiseRightShiftBigIntOperator(),
            expectedNodeType: ExpressionType.RightShift,
            DefaultNodeAssertion );
    }

    [Fact]
    public void BitwiseRightShiftOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant<BigInteger, int, BigInteger>(
            sut: new ParsedExpressionBitwiseRightShiftBigIntOperator(),
            expectedNodeType: ExpressionType.Constant,
            leftValue: 1867776,
            rightValue: 12,
            (_, _, result) =>
            {
                result.Should().BeAssignableTo<ConstantExpression>();
                if ( result is not ConstantExpression constantExpression )
                    return;

                constantExpression.Value.Should().Be( new BigInteger( 456 ) );
            } );
    }

    [Fact]
    public void BitwiseRightShiftOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstantAndNotEqualToZero()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant<BigInteger, int, BigInteger>(
            sut: new ParsedExpressionBitwiseRightShiftBigIntOperator(),
            expectedNodeType: ExpressionType.RightShift,
            leftValue: Fixture.CreateNotDefault<BigInteger>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void BitwiseRightShiftOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstantAndEqualToZero()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant<BigInteger, int, BigInteger>(
            sut: new ParsedExpressionBitwiseRightShiftBigIntOperator(),
            expectedNodeType: ExpressionType.Constant,
            leftValue: 0,
            (_, _, result) =>
            {
                result.Should().BeAssignableTo<ConstantExpression>();
                if ( result is not ConstantExpression constantResult )
                    return;

                constantResult.Value.Should().Be( BigInteger.Zero );
            } );
    }

    [Theory]
    [InlineData( 1 )]
    [InlineData( 63 )]
    [InlineData( 65 )]
    [InlineData( -1 )]
    [InlineData( -63 )]
    [InlineData( -65 )]
    public void BitwiseRightShiftOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstantAndNotEqualToZero(
        int shift)
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant<BigInteger, int, BigInteger>(
            sut: new ParsedExpressionBitwiseRightShiftBigIntOperator(),
            expectedNodeType: ExpressionType.RightShift,
            rightValue: shift,
            DefaultNodeAssertion );
    }

    [Fact]
    public void BitwiseRightShiftOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstantAndEqualToZero()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant<BigInteger, int, BigInteger>(
            sut: new ParsedExpressionBitwiseRightShiftBigIntOperator(),
            expectedNodeType: ExpressionType.Parameter,
            rightValue: 0,
            (left, _, result) => result.Should().BeSameAs( left ) );
    }

    [Fact]
    public void EqualToOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable<BigInteger, BigInteger, bool>(
            sut: new ParsedExpressionEqualToBigIntOperator(),
            expectedNodeType: ExpressionType.Equal,
            DefaultNodeAssertion );
    }

    [Theory]
    [InlineData( 123, 456, false )]
    [InlineData( 123, 123, true )]
    public void EqualToOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant(
        BigInteger left,
        BigInteger right,
        bool expected)
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant<BigInteger, BigInteger, bool>(
            sut: new ParsedExpressionEqualToBigIntOperator(),
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
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant<BigInteger, BigInteger, bool>(
            sut: new ParsedExpressionEqualToBigIntOperator(),
            expectedNodeType: ExpressionType.Equal,
            leftValue: Fixture.Create<BigInteger>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void EqualToOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant<BigInteger, BigInteger, bool>(
            sut: new ParsedExpressionEqualToBigIntOperator(),
            expectedNodeType: ExpressionType.Equal,
            rightValue: Fixture.Create<BigInteger>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void NotEqualToOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable<BigInteger, BigInteger, bool>(
            sut: new ParsedExpressionNotEqualToBigIntOperator(),
            expectedNodeType: ExpressionType.NotEqual,
            DefaultNodeAssertion );
    }

    [Theory]
    [InlineData( 123, 456, true )]
    [InlineData( 123, 123, false )]
    public void NotEqualToOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant(
        BigInteger left,
        BigInteger right,
        bool expected)
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant<BigInteger, BigInteger, bool>(
            sut: new ParsedExpressionNotEqualToBigIntOperator(),
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
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant<BigInteger, BigInteger, bool>(
            sut: new ParsedExpressionNotEqualToBigIntOperator(),
            expectedNodeType: ExpressionType.NotEqual,
            leftValue: Fixture.Create<BigInteger>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void NotEqualToOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant<BigInteger, BigInteger, bool>(
            sut: new ParsedExpressionNotEqualToBigIntOperator(),
            expectedNodeType: ExpressionType.NotEqual,
            rightValue: Fixture.Create<BigInteger>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void GreaterThanOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable<BigInteger, BigInteger, bool>(
            sut: new ParsedExpressionGreaterThanBigIntOperator(),
            expectedNodeType: ExpressionType.GreaterThan,
            DefaultNodeAssertion );
    }

    [Theory]
    [InlineData( 123, 456, false )]
    [InlineData( 123, 123, false )]
    [InlineData( 456, 123, true )]
    public void GreaterThanOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant(
        BigInteger left,
        BigInteger right,
        bool expected)
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant<BigInteger, BigInteger, bool>(
            sut: new ParsedExpressionGreaterThanBigIntOperator(),
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
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant<BigInteger, BigInteger, bool>(
            sut: new ParsedExpressionGreaterThanBigIntOperator(),
            expectedNodeType: ExpressionType.GreaterThan,
            leftValue: Fixture.Create<BigInteger>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void GreaterThanOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant<BigInteger, BigInteger, bool>(
            sut: new ParsedExpressionGreaterThanBigIntOperator(),
            expectedNodeType: ExpressionType.GreaterThan,
            rightValue: Fixture.Create<BigInteger>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void LessThanOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable<BigInteger, BigInteger, bool>(
            sut: new ParsedExpressionLessThanBigIntOperator(),
            expectedNodeType: ExpressionType.LessThan,
            DefaultNodeAssertion );
    }

    [Theory]
    [InlineData( 123, 456, true )]
    [InlineData( 123, 123, false )]
    [InlineData( 456, 123, false )]
    public void LessThanOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant(
        BigInteger left,
        BigInteger right,
        bool expected)
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant<BigInteger, BigInteger, bool>(
            sut: new ParsedExpressionLessThanBigIntOperator(),
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
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant<BigInteger, BigInteger, bool>(
            sut: new ParsedExpressionLessThanBigIntOperator(),
            expectedNodeType: ExpressionType.LessThan,
            leftValue: Fixture.Create<BigInteger>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void LessThanOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant<BigInteger, BigInteger, bool>(
            sut: new ParsedExpressionLessThanBigIntOperator(),
            expectedNodeType: ExpressionType.LessThan,
            rightValue: Fixture.Create<BigInteger>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void GreaterThanOrEqualToOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable<BigInteger, BigInteger, bool>(
            sut: new ParsedExpressionGreaterThanOrEqualToBigIntOperator(),
            expectedNodeType: ExpressionType.GreaterThanOrEqual,
            DefaultNodeAssertion );
    }

    [Theory]
    [InlineData( 123, 456, false )]
    [InlineData( 123, 123, true )]
    [InlineData( 456, 123, true )]
    public void GreaterThanOrEqualToOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant(
        BigInteger left,
        BigInteger right,
        bool expected)
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant<BigInteger, BigInteger, bool>(
            sut: new ParsedExpressionGreaterThanOrEqualToBigIntOperator(),
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
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant<BigInteger, BigInteger, bool>(
            sut: new ParsedExpressionGreaterThanOrEqualToBigIntOperator(),
            expectedNodeType: ExpressionType.GreaterThanOrEqual,
            leftValue: Fixture.Create<BigInteger>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void GreaterThanOrEqualToOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant<BigInteger, BigInteger, bool>(
            sut: new ParsedExpressionGreaterThanOrEqualToBigIntOperator(),
            expectedNodeType: ExpressionType.GreaterThanOrEqual,
            rightValue: Fixture.Create<BigInteger>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void LessThanOrEqualToOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable<BigInteger, BigInteger, bool>(
            sut: new ParsedExpressionLessThanOrEqualToBigIntOperator(),
            expectedNodeType: ExpressionType.LessThanOrEqual,
            DefaultNodeAssertion );
    }

    [Theory]
    [InlineData( 123, 456, true )]
    [InlineData( 123, 123, true )]
    [InlineData( 456, 123, false )]
    public void LessThanOrEqualToOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant(
        BigInteger left,
        BigInteger right,
        bool expected)
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant<BigInteger, BigInteger, bool>(
            sut: new ParsedExpressionLessThanOrEqualToBigIntOperator(),
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
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant<BigInteger, BigInteger, bool>(
            sut: new ParsedExpressionLessThanOrEqualToBigIntOperator(),
            expectedNodeType: ExpressionType.LessThanOrEqual,
            leftValue: Fixture.Create<BigInteger>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void LessThanOrEqualToOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant<BigInteger, BigInteger, bool>(
            sut: new ParsedExpressionLessThanOrEqualToBigIntOperator(),
            expectedNodeType: ExpressionType.LessThanOrEqual,
            rightValue: Fixture.Create<BigInteger>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void CompareOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable<BigInteger, BigInteger, int>(
            sut: new ParsedExpressionCompareBigIntOperator(),
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
        BigInteger left,
        BigInteger right,
        int expected)
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant<BigInteger, BigInteger, int>(
            sut: new ParsedExpressionCompareBigIntOperator(),
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
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant<BigInteger, BigInteger, int>(
            sut: new ParsedExpressionCompareBigIntOperator(),
            expectedNodeType: ExpressionType.Call,
            leftValue: Fixture.Create<BigInteger>(),
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
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant<BigInteger, BigInteger, int>(
            sut: new ParsedExpressionCompareBigIntOperator(),
            expectedNodeType: ExpressionType.Call,
            rightValue: Fixture.Create<BigInteger>(),
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
