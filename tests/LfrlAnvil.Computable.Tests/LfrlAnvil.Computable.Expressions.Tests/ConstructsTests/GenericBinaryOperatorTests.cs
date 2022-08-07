using System;
using System.Linq;
using System.Linq.Expressions;
using AutoFixture;
using FluentAssertions;
using LfrlAnvil.Computable.Expressions.Constructs;
using LfrlAnvil.Computable.Expressions.Exceptions;
using Xunit;

namespace LfrlAnvil.Computable.Expressions.Tests.ConstructsTests;

public class GenericBinaryOperatorTests : BinaryOperatorsTestsBase
{
    [Fact]
    public void AddOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable<int, int, int>(
            sut: new MathExpressionAddOperator(),
            expectedNodeType: ExpressionType.Add,
            DefaultNodeAssertion );
    }

    [Fact]
    public void AddOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant<int, int, int>(
            sut: new MathExpressionAddOperator(),
            expectedNodeType: ExpressionType.Add,
            leftValue: Fixture.Create<int>(),
            rightValue: Fixture.Create<int>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void AddOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant<int, int, int>(
            sut: new MathExpressionAddOperator(),
            expectedNodeType: ExpressionType.Add,
            leftValue: Fixture.Create<int>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void AddOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant<int, int, int>(
            sut: new MathExpressionAddOperator(),
            expectedNodeType: ExpressionType.Add,
            rightValue: Fixture.Create<int>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void AddOperatorProcess_ShouldThrowInvalidOperationException_WhenOperatorDoesNotExist()
    {
        Process_ShouldThrowException_WhenOperatorDoesNotExist<int, string, InvalidOperationException>(
            sut: new MathExpressionAddOperator() );
    }

    [Fact]
    public void SubtractOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable<int, int, int>(
            sut: new MathExpressionSubtractOperator(),
            expectedNodeType: ExpressionType.Subtract,
            DefaultNodeAssertion );
    }

    [Fact]
    public void SubtractOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant<int, int, int>(
            sut: new MathExpressionSubtractOperator(),
            expectedNodeType: ExpressionType.Subtract,
            leftValue: Fixture.Create<int>(),
            rightValue: Fixture.Create<int>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void SubtractOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant<int, int, int>(
            sut: new MathExpressionSubtractOperator(),
            expectedNodeType: ExpressionType.Subtract,
            leftValue: Fixture.Create<int>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void SubtractOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant<int, int, int>(
            sut: new MathExpressionSubtractOperator(),
            expectedNodeType: ExpressionType.Subtract,
            rightValue: Fixture.Create<int>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void SubtractOperatorProcess_ShouldThrowInvalidOperationException_WhenOperatorDoesNotExist()
    {
        Process_ShouldThrowException_WhenOperatorDoesNotExist<int, string, InvalidOperationException>(
            sut: new MathExpressionSubtractOperator() );
    }

    [Fact]
    public void MultiplyOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable<int, int, int>(
            sut: new MathExpressionMultiplyOperator(),
            expectedNodeType: ExpressionType.Multiply,
            DefaultNodeAssertion );
    }

    [Fact]
    public void MultiplyOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant<int, int, int>(
            sut: new MathExpressionMultiplyOperator(),
            expectedNodeType: ExpressionType.Multiply,
            leftValue: Fixture.Create<int>(),
            rightValue: Fixture.Create<int>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void MultiplyOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant<int, int, int>(
            sut: new MathExpressionMultiplyOperator(),
            expectedNodeType: ExpressionType.Multiply,
            leftValue: Fixture.Create<int>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void MultiplyOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant<int, int, int>(
            sut: new MathExpressionMultiplyOperator(),
            expectedNodeType: ExpressionType.Multiply,
            rightValue: Fixture.Create<int>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void MultiplyOperatorProcess_ShouldThrowInvalidOperationException_WhenOperatorDoesNotExist()
    {
        Process_ShouldThrowException_WhenOperatorDoesNotExist<int, string, InvalidOperationException>(
            sut: new MathExpressionMultiplyOperator() );
    }

    [Fact]
    public void DivideOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable<int, int, int>(
            sut: new MathExpressionDivideOperator(),
            expectedNodeType: ExpressionType.Divide,
            DefaultNodeAssertion );
    }

    [Fact]
    public void DivideOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant<int, int, int>(
            sut: new MathExpressionDivideOperator(),
            expectedNodeType: ExpressionType.Divide,
            leftValue: Fixture.Create<int>(),
            rightValue: Fixture.Create<int>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void DivideOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant<int, int, int>(
            sut: new MathExpressionDivideOperator(),
            expectedNodeType: ExpressionType.Divide,
            leftValue: Fixture.Create<int>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void DivideOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant<int, int, int>(
            sut: new MathExpressionDivideOperator(),
            expectedNodeType: ExpressionType.Divide,
            rightValue: Fixture.Create<int>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void DivideOperatorProcess_ShouldThrowInvalidOperationException_WhenOperatorDoesNotExist()
    {
        Process_ShouldThrowException_WhenOperatorDoesNotExist<int, string, InvalidOperationException>(
            sut: new MathExpressionDivideOperator() );
    }

    [Fact]
    public void ModuloOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable<int, int, int>(
            sut: new MathExpressionModuloOperator(),
            expectedNodeType: ExpressionType.Modulo,
            DefaultNodeAssertion );
    }

    [Fact]
    public void ModuloOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant<int, int, int>(
            sut: new MathExpressionModuloOperator(),
            expectedNodeType: ExpressionType.Modulo,
            leftValue: Fixture.Create<int>(),
            rightValue: Fixture.Create<int>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void ModuloOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant<int, int, int>(
            sut: new MathExpressionModuloOperator(),
            expectedNodeType: ExpressionType.Modulo,
            leftValue: Fixture.Create<int>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void ModuloOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant<int, int, int>(
            sut: new MathExpressionModuloOperator(),
            expectedNodeType: ExpressionType.Modulo,
            rightValue: Fixture.Create<int>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void ModuloOperatorProcess_ShouldThrowInvalidOperationException_WhenOperatorDoesNotExist()
    {
        Process_ShouldThrowException_WhenOperatorDoesNotExist<int, string, InvalidOperationException>(
            sut: new MathExpressionModuloOperator() );
    }

    [Fact]
    public void EqualToOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable<int, int, bool>(
            sut: new MathExpressionEqualToOperator(),
            expectedNodeType: ExpressionType.Equal,
            DefaultNodeAssertion );
    }

    [Fact]
    public void EqualToOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant<int, int, bool>(
            sut: new MathExpressionEqualToOperator(),
            expectedNodeType: ExpressionType.Equal,
            leftValue: Fixture.Create<int>(),
            rightValue: Fixture.Create<int>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void EqualToOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant<int, int, bool>(
            sut: new MathExpressionEqualToOperator(),
            expectedNodeType: ExpressionType.Equal,
            leftValue: Fixture.Create<int>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void EqualToOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant<int, int, bool>(
            sut: new MathExpressionEqualToOperator(),
            expectedNodeType: ExpressionType.Equal,
            rightValue: Fixture.Create<int>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void EqualToOperatorProcess_ShouldThrowInvalidOperationException_WhenOperatorDoesNotExist()
    {
        Process_ShouldThrowException_WhenOperatorDoesNotExist<int, string, InvalidOperationException>(
            sut: new MathExpressionEqualToOperator() );
    }

    [Fact]
    public void NotEqualToOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable<int, int, bool>(
            sut: new MathExpressionNotEqualToOperator(),
            expectedNodeType: ExpressionType.NotEqual,
            DefaultNodeAssertion );
    }

    [Fact]
    public void NotEqualToOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant<int, int, bool>(
            sut: new MathExpressionNotEqualToOperator(),
            expectedNodeType: ExpressionType.NotEqual,
            leftValue: Fixture.Create<int>(),
            rightValue: Fixture.Create<int>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void NotEqualToOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant<int, int, bool>(
            sut: new MathExpressionNotEqualToOperator(),
            expectedNodeType: ExpressionType.NotEqual,
            leftValue: Fixture.Create<int>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void NotEqualToOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant<int, int, bool>(
            sut: new MathExpressionNotEqualToOperator(),
            expectedNodeType: ExpressionType.NotEqual,
            rightValue: Fixture.Create<int>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void NotEqualToOperatorProcess_ShouldThrowInvalidOperationException_WhenOperatorDoesNotExist()
    {
        Process_ShouldThrowException_WhenOperatorDoesNotExist<int, string, InvalidOperationException>(
            sut: new MathExpressionNotEqualToOperator() );
    }

    [Fact]
    public void GreaterThanOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable<int, int, bool>(
            sut: new MathExpressionGreaterThanOperator(),
            expectedNodeType: ExpressionType.GreaterThan,
            DefaultNodeAssertion );
    }

    [Fact]
    public void GreaterThanOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant<int, int, bool>(
            sut: new MathExpressionGreaterThanOperator(),
            expectedNodeType: ExpressionType.GreaterThan,
            leftValue: Fixture.Create<int>(),
            rightValue: Fixture.Create<int>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void GreaterThanOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant<int, int, bool>(
            sut: new MathExpressionGreaterThanOperator(),
            expectedNodeType: ExpressionType.GreaterThan,
            leftValue: Fixture.Create<int>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void GreaterThanOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant<int, int, bool>(
            sut: new MathExpressionGreaterThanOperator(),
            expectedNodeType: ExpressionType.GreaterThan,
            rightValue: Fixture.Create<int>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void GreaterThanOperatorProcess_ShouldThrowInvalidOperationException_WhenOperatorDoesNotExist()
    {
        Process_ShouldThrowException_WhenOperatorDoesNotExist<int, string, InvalidOperationException>(
            sut: new MathExpressionGreaterThanOperator() );
    }

    [Fact]
    public void LessThanOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable<int, int, bool>(
            sut: new MathExpressionLessThanOperator(),
            expectedNodeType: ExpressionType.LessThan,
            DefaultNodeAssertion );
    }

    [Fact]
    public void LessThanOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant<int, int, bool>(
            sut: new MathExpressionLessThanOperator(),
            expectedNodeType: ExpressionType.LessThan,
            leftValue: Fixture.Create<int>(),
            rightValue: Fixture.Create<int>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void LessThanOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant<int, int, bool>(
            sut: new MathExpressionLessThanOperator(),
            expectedNodeType: ExpressionType.LessThan,
            leftValue: Fixture.Create<int>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void LessThanOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant<int, int, bool>(
            sut: new MathExpressionLessThanOperator(),
            expectedNodeType: ExpressionType.LessThan,
            rightValue: Fixture.Create<int>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void LessThanOperatorProcess_ShouldThrowInvalidOperationException_WhenOperatorDoesNotExist()
    {
        Process_ShouldThrowException_WhenOperatorDoesNotExist<int, string, InvalidOperationException>(
            sut: new MathExpressionLessThanOperator() );
    }

    [Fact]
    public void GreaterThanOrEqualToOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable<int, int, bool>(
            sut: new MathExpressionGreaterThanOrEqualToOperator(),
            expectedNodeType: ExpressionType.GreaterThanOrEqual,
            DefaultNodeAssertion );
    }

    [Fact]
    public void GreaterThanOrEqualToOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant<int, int, bool>(
            sut: new MathExpressionGreaterThanOrEqualToOperator(),
            expectedNodeType: ExpressionType.GreaterThanOrEqual,
            leftValue: Fixture.Create<int>(),
            rightValue: Fixture.Create<int>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void GreaterThanOrEqualToOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant<int, int, bool>(
            sut: new MathExpressionGreaterThanOrEqualToOperator(),
            expectedNodeType: ExpressionType.GreaterThanOrEqual,
            leftValue: Fixture.Create<int>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void GreaterThanOrEqualToOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant<int, int, bool>(
            sut: new MathExpressionGreaterThanOrEqualToOperator(),
            expectedNodeType: ExpressionType.GreaterThanOrEqual,
            rightValue: Fixture.Create<int>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void GreaterThanOrEqualToOperatorProcess_ShouldThrowInvalidOperationException_WhenOperatorDoesNotExist()
    {
        Process_ShouldThrowException_WhenOperatorDoesNotExist<int, string, InvalidOperationException>(
            sut: new MathExpressionGreaterThanOrEqualToOperator() );
    }

    [Fact]
    public void LessThanOrEqualToOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable<int, int, bool>(
            sut: new MathExpressionLessThanOrEqualToOperator(),
            expectedNodeType: ExpressionType.LessThanOrEqual,
            DefaultNodeAssertion );
    }

    [Fact]
    public void LessThanOrEqualToOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant<int, int, bool>(
            sut: new MathExpressionLessThanOrEqualToOperator(),
            expectedNodeType: ExpressionType.LessThanOrEqual,
            leftValue: Fixture.Create<int>(),
            rightValue: Fixture.Create<int>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void LessThanOrEqualToOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant<int, int, bool>(
            sut: new MathExpressionLessThanOrEqualToOperator(),
            expectedNodeType: ExpressionType.LessThanOrEqual,
            leftValue: Fixture.Create<int>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void LessThanOrEqualToOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant<int, int, bool>(
            sut: new MathExpressionLessThanOrEqualToOperator(),
            expectedNodeType: ExpressionType.LessThanOrEqual,
            rightValue: Fixture.Create<int>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void LessThanOrEqualToOperatorProcess_ShouldThrowInvalidOperationException_WhenOperatorDoesNotExist()
    {
        Process_ShouldThrowException_WhenOperatorDoesNotExist<int, string, InvalidOperationException>(
            sut: new MathExpressionLessThanOrEqualToOperator() );
    }

    [Fact]
    public void BitwiseAndOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable<int, int, int>(
            sut: new MathExpressionBitwiseAndOperator(),
            expectedNodeType: ExpressionType.And,
            DefaultNodeAssertion );
    }

    [Fact]
    public void BitwiseAndOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant<int, int, int>(
            sut: new MathExpressionBitwiseAndOperator(),
            expectedNodeType: ExpressionType.And,
            leftValue: Fixture.Create<int>(),
            rightValue: Fixture.Create<int>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void BitwiseAndOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant<int, int, int>(
            sut: new MathExpressionBitwiseAndOperator(),
            expectedNodeType: ExpressionType.And,
            leftValue: Fixture.Create<int>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void BitwiseAndOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant<int, int, int>(
            sut: new MathExpressionBitwiseAndOperator(),
            expectedNodeType: ExpressionType.And,
            rightValue: Fixture.Create<int>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void BitwiseAndOperatorProcess_ShouldThrowInvalidOperationException_WhenOperatorDoesNotExist()
    {
        Process_ShouldThrowException_WhenOperatorDoesNotExist<int, string, InvalidOperationException>(
            sut: new MathExpressionBitwiseAndOperator() );
    }

    [Fact]
    public void BitwiseOrOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable<int, int, int>(
            sut: new MathExpressionBitwiseOrOperator(),
            expectedNodeType: ExpressionType.Or,
            DefaultNodeAssertion );
    }

    [Fact]
    public void BitwiseOrOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant<int, int, int>(
            sut: new MathExpressionBitwiseOrOperator(),
            expectedNodeType: ExpressionType.Or,
            leftValue: Fixture.Create<int>(),
            rightValue: Fixture.Create<int>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void BitwiseOrOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant<int, int, int>(
            sut: new MathExpressionBitwiseOrOperator(),
            expectedNodeType: ExpressionType.Or,
            leftValue: Fixture.Create<int>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void BitwiseOrOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant<int, int, int>(
            sut: new MathExpressionBitwiseOrOperator(),
            expectedNodeType: ExpressionType.Or,
            rightValue: Fixture.Create<int>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void BitwiseOrOperatorProcess_ShouldThrowInvalidOperationException_WhenOperatorDoesNotExist()
    {
        Process_ShouldThrowException_WhenOperatorDoesNotExist<int, string, InvalidOperationException>(
            sut: new MathExpressionBitwiseOrOperator() );
    }

    [Fact]
    public void BitwiseXorOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable<int, int, int>(
            sut: new MathExpressionBitwiseXorOperator(),
            expectedNodeType: ExpressionType.ExclusiveOr,
            DefaultNodeAssertion );
    }

    [Fact]
    public void BitwiseXorOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant<int, int, int>(
            sut: new MathExpressionBitwiseXorOperator(),
            expectedNodeType: ExpressionType.ExclusiveOr,
            leftValue: Fixture.Create<int>(),
            rightValue: Fixture.Create<int>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void BitwiseXorOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant<int, int, int>(
            sut: new MathExpressionBitwiseXorOperator(),
            expectedNodeType: ExpressionType.ExclusiveOr,
            leftValue: Fixture.Create<int>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void BitwiseXorOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant<int, int, int>(
            sut: new MathExpressionBitwiseXorOperator(),
            expectedNodeType: ExpressionType.ExclusiveOr,
            rightValue: Fixture.Create<int>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void BitwiseXorOperatorProcess_ShouldThrowInvalidOperationException_WhenOperatorDoesNotExist()
    {
        Process_ShouldThrowException_WhenOperatorDoesNotExist<int, string, InvalidOperationException>(
            sut: new MathExpressionBitwiseXorOperator() );
    }

    [Fact]
    public void BitwiseLeftShiftOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable<int, int, int>(
            sut: new MathExpressionBitwiseLeftShiftOperator(),
            expectedNodeType: ExpressionType.LeftShift,
            DefaultNodeAssertion );
    }

    [Fact]
    public void BitwiseLeftShiftOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant<int, int, int>(
            sut: new MathExpressionBitwiseLeftShiftOperator(),
            expectedNodeType: ExpressionType.LeftShift,
            leftValue: Fixture.Create<int>(),
            rightValue: Fixture.Create<int>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void BitwiseLeftShiftOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant<int, int, int>(
            sut: new MathExpressionBitwiseLeftShiftOperator(),
            expectedNodeType: ExpressionType.LeftShift,
            leftValue: Fixture.Create<int>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void BitwiseLeftShiftOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant<int, int, int>(
            sut: new MathExpressionBitwiseLeftShiftOperator(),
            expectedNodeType: ExpressionType.LeftShift,
            rightValue: Fixture.Create<int>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void BitwiseLeftShiftOperatorProcess_ShouldThrowInvalidOperationException_WhenOperatorDoesNotExist()
    {
        Process_ShouldThrowException_WhenOperatorDoesNotExist<int, string, InvalidOperationException>(
            sut: new MathExpressionBitwiseLeftShiftOperator() );
    }

    [Fact]
    public void BitwiseRightShiftOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable<int, int, int>(
            sut: new MathExpressionBitwiseRightShiftOperator(),
            expectedNodeType: ExpressionType.RightShift,
            DefaultNodeAssertion );
    }

    [Fact]
    public void BitwiseRightShiftOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant<int, int, int>(
            sut: new MathExpressionBitwiseRightShiftOperator(),
            expectedNodeType: ExpressionType.RightShift,
            leftValue: Fixture.Create<int>(),
            rightValue: Fixture.Create<int>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void BitwiseRightShiftOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant<int, int, int>(
            sut: new MathExpressionBitwiseRightShiftOperator(),
            expectedNodeType: ExpressionType.RightShift,
            leftValue: Fixture.Create<int>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void BitwiseRightShiftOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant<int, int, int>(
            sut: new MathExpressionBitwiseRightShiftOperator(),
            expectedNodeType: ExpressionType.RightShift,
            rightValue: Fixture.Create<int>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void BitwiseRightShiftOperatorProcess_ShouldThrowInvalidOperationException_WhenOperatorDoesNotExist()
    {
        Process_ShouldThrowException_WhenOperatorDoesNotExist<int, string, InvalidOperationException>(
            sut: new MathExpressionBitwiseRightShiftOperator() );
    }

    [Fact]
    public void CompareOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable<string, string, int>(
            sut: new MathExpressionCompareOperator(),
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

    [Fact]
    public void CompareOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant<string, string, int>(
            sut: new MathExpressionCompareOperator(),
            expectedNodeType: ExpressionType.Call,
            leftValue: Fixture.Create<string>(),
            rightValue: Fixture.Create<string>(),
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
    public void CompareOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant<string, string, int>(
            sut: new MathExpressionCompareOperator(),
            expectedNodeType: ExpressionType.Call,
            leftValue: Fixture.Create<string>(),
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
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant<string, string, int>(
            sut: new MathExpressionCompareOperator(),
            expectedNodeType: ExpressionType.Call,
            rightValue: Fixture.Create<string>(),
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
    public void CompareOperatorProcess_ShouldThrowMathExpressionConstructException_WhenCorrectCompareToMethodDoesNotExist()
    {
        Process_ShouldThrowException_WhenOperatorDoesNotExist<int, string, MathExpressionConstructException>(
            sut: new MathExpressionCompareOperator() );
    }

    [Fact]
    public void CoalesceOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable<string, string, string>(
            sut: new MathExpressionCoalesceOperator(),
            expectedNodeType: ExpressionType.Coalesce,
            DefaultNodeAssertion );
    }

    [Fact]
    public void CoalesceOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant<string, string, string>(
            sut: new MathExpressionCoalesceOperator(),
            expectedNodeType: ExpressionType.Coalesce,
            leftValue: Fixture.Create<string>(),
            rightValue: Fixture.Create<string>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void CoalesceOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant<string, string, string>(
            sut: new MathExpressionCoalesceOperator(),
            expectedNodeType: ExpressionType.Coalesce,
            leftValue: Fixture.Create<string>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void CoalesceOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant<string, string, string>(
            sut: new MathExpressionCoalesceOperator(),
            expectedNodeType: ExpressionType.Coalesce,
            rightValue: Fixture.Create<string>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void CoalesceOperatorProcess_ShouldThrowInvalidOperationException_WhenOperatorDoesNotExist()
    {
        Process_ShouldThrowException_WhenOperatorDoesNotExist<int, string, InvalidOperationException>(
            sut: new MathExpressionCoalesceOperator() );
    }
}
