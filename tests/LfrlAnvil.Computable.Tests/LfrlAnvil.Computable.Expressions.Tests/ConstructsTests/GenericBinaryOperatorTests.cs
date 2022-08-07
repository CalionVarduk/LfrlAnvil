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
            sut: new ParsedExpressionAddOperator(),
            expectedNodeType: ExpressionType.Add,
            DefaultNodeAssertion );
    }

    [Fact]
    public void AddOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant<int, int, int>(
            sut: new ParsedExpressionAddOperator(),
            expectedNodeType: ExpressionType.Add,
            leftValue: Fixture.Create<int>(),
            rightValue: Fixture.Create<int>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void AddOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant<int, int, int>(
            sut: new ParsedExpressionAddOperator(),
            expectedNodeType: ExpressionType.Add,
            leftValue: Fixture.Create<int>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void AddOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant<int, int, int>(
            sut: new ParsedExpressionAddOperator(),
            expectedNodeType: ExpressionType.Add,
            rightValue: Fixture.Create<int>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void AddOperatorProcess_ShouldThrowInvalidOperationException_WhenOperatorDoesNotExist()
    {
        Process_ShouldThrowException_WhenOperatorDoesNotExist<int, string, InvalidOperationException>(
            sut: new ParsedExpressionAddOperator() );
    }

    [Fact]
    public void SubtractOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable<int, int, int>(
            sut: new ParsedExpressionSubtractOperator(),
            expectedNodeType: ExpressionType.Subtract,
            DefaultNodeAssertion );
    }

    [Fact]
    public void SubtractOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant<int, int, int>(
            sut: new ParsedExpressionSubtractOperator(),
            expectedNodeType: ExpressionType.Subtract,
            leftValue: Fixture.Create<int>(),
            rightValue: Fixture.Create<int>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void SubtractOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant<int, int, int>(
            sut: new ParsedExpressionSubtractOperator(),
            expectedNodeType: ExpressionType.Subtract,
            leftValue: Fixture.Create<int>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void SubtractOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant<int, int, int>(
            sut: new ParsedExpressionSubtractOperator(),
            expectedNodeType: ExpressionType.Subtract,
            rightValue: Fixture.Create<int>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void SubtractOperatorProcess_ShouldThrowInvalidOperationException_WhenOperatorDoesNotExist()
    {
        Process_ShouldThrowException_WhenOperatorDoesNotExist<int, string, InvalidOperationException>(
            sut: new ParsedExpressionSubtractOperator() );
    }

    [Fact]
    public void MultiplyOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable<int, int, int>(
            sut: new ParsedExpressionMultiplyOperator(),
            expectedNodeType: ExpressionType.Multiply,
            DefaultNodeAssertion );
    }

    [Fact]
    public void MultiplyOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant<int, int, int>(
            sut: new ParsedExpressionMultiplyOperator(),
            expectedNodeType: ExpressionType.Multiply,
            leftValue: Fixture.Create<int>(),
            rightValue: Fixture.Create<int>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void MultiplyOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant<int, int, int>(
            sut: new ParsedExpressionMultiplyOperator(),
            expectedNodeType: ExpressionType.Multiply,
            leftValue: Fixture.Create<int>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void MultiplyOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant<int, int, int>(
            sut: new ParsedExpressionMultiplyOperator(),
            expectedNodeType: ExpressionType.Multiply,
            rightValue: Fixture.Create<int>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void MultiplyOperatorProcess_ShouldThrowInvalidOperationException_WhenOperatorDoesNotExist()
    {
        Process_ShouldThrowException_WhenOperatorDoesNotExist<int, string, InvalidOperationException>(
            sut: new ParsedExpressionMultiplyOperator() );
    }

    [Fact]
    public void DivideOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable<int, int, int>(
            sut: new ParsedExpressionDivideOperator(),
            expectedNodeType: ExpressionType.Divide,
            DefaultNodeAssertion );
    }

    [Fact]
    public void DivideOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant<int, int, int>(
            sut: new ParsedExpressionDivideOperator(),
            expectedNodeType: ExpressionType.Divide,
            leftValue: Fixture.Create<int>(),
            rightValue: Fixture.Create<int>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void DivideOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant<int, int, int>(
            sut: new ParsedExpressionDivideOperator(),
            expectedNodeType: ExpressionType.Divide,
            leftValue: Fixture.Create<int>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void DivideOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant<int, int, int>(
            sut: new ParsedExpressionDivideOperator(),
            expectedNodeType: ExpressionType.Divide,
            rightValue: Fixture.Create<int>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void DivideOperatorProcess_ShouldThrowInvalidOperationException_WhenOperatorDoesNotExist()
    {
        Process_ShouldThrowException_WhenOperatorDoesNotExist<int, string, InvalidOperationException>(
            sut: new ParsedExpressionDivideOperator() );
    }

    [Fact]
    public void ModuloOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable<int, int, int>(
            sut: new ParsedExpressionModuloOperator(),
            expectedNodeType: ExpressionType.Modulo,
            DefaultNodeAssertion );
    }

    [Fact]
    public void ModuloOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant<int, int, int>(
            sut: new ParsedExpressionModuloOperator(),
            expectedNodeType: ExpressionType.Modulo,
            leftValue: Fixture.Create<int>(),
            rightValue: Fixture.Create<int>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void ModuloOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant<int, int, int>(
            sut: new ParsedExpressionModuloOperator(),
            expectedNodeType: ExpressionType.Modulo,
            leftValue: Fixture.Create<int>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void ModuloOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant<int, int, int>(
            sut: new ParsedExpressionModuloOperator(),
            expectedNodeType: ExpressionType.Modulo,
            rightValue: Fixture.Create<int>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void ModuloOperatorProcess_ShouldThrowInvalidOperationException_WhenOperatorDoesNotExist()
    {
        Process_ShouldThrowException_WhenOperatorDoesNotExist<int, string, InvalidOperationException>(
            sut: new ParsedExpressionModuloOperator() );
    }

    [Fact]
    public void EqualToOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable<int, int, bool>(
            sut: new ParsedExpressionEqualToOperator(),
            expectedNodeType: ExpressionType.Equal,
            DefaultNodeAssertion );
    }

    [Fact]
    public void EqualToOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant<int, int, bool>(
            sut: new ParsedExpressionEqualToOperator(),
            expectedNodeType: ExpressionType.Equal,
            leftValue: Fixture.Create<int>(),
            rightValue: Fixture.Create<int>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void EqualToOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant<int, int, bool>(
            sut: new ParsedExpressionEqualToOperator(),
            expectedNodeType: ExpressionType.Equal,
            leftValue: Fixture.Create<int>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void EqualToOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant<int, int, bool>(
            sut: new ParsedExpressionEqualToOperator(),
            expectedNodeType: ExpressionType.Equal,
            rightValue: Fixture.Create<int>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void EqualToOperatorProcess_ShouldThrowInvalidOperationException_WhenOperatorDoesNotExist()
    {
        Process_ShouldThrowException_WhenOperatorDoesNotExist<int, string, InvalidOperationException>(
            sut: new ParsedExpressionEqualToOperator() );
    }

    [Fact]
    public void NotEqualToOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable<int, int, bool>(
            sut: new ParsedExpressionNotEqualToOperator(),
            expectedNodeType: ExpressionType.NotEqual,
            DefaultNodeAssertion );
    }

    [Fact]
    public void NotEqualToOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant<int, int, bool>(
            sut: new ParsedExpressionNotEqualToOperator(),
            expectedNodeType: ExpressionType.NotEqual,
            leftValue: Fixture.Create<int>(),
            rightValue: Fixture.Create<int>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void NotEqualToOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant<int, int, bool>(
            sut: new ParsedExpressionNotEqualToOperator(),
            expectedNodeType: ExpressionType.NotEqual,
            leftValue: Fixture.Create<int>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void NotEqualToOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant<int, int, bool>(
            sut: new ParsedExpressionNotEqualToOperator(),
            expectedNodeType: ExpressionType.NotEqual,
            rightValue: Fixture.Create<int>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void NotEqualToOperatorProcess_ShouldThrowInvalidOperationException_WhenOperatorDoesNotExist()
    {
        Process_ShouldThrowException_WhenOperatorDoesNotExist<int, string, InvalidOperationException>(
            sut: new ParsedExpressionNotEqualToOperator() );
    }

    [Fact]
    public void GreaterThanOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable<int, int, bool>(
            sut: new ParsedExpressionGreaterThanOperator(),
            expectedNodeType: ExpressionType.GreaterThan,
            DefaultNodeAssertion );
    }

    [Fact]
    public void GreaterThanOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant<int, int, bool>(
            sut: new ParsedExpressionGreaterThanOperator(),
            expectedNodeType: ExpressionType.GreaterThan,
            leftValue: Fixture.Create<int>(),
            rightValue: Fixture.Create<int>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void GreaterThanOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant<int, int, bool>(
            sut: new ParsedExpressionGreaterThanOperator(),
            expectedNodeType: ExpressionType.GreaterThan,
            leftValue: Fixture.Create<int>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void GreaterThanOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant<int, int, bool>(
            sut: new ParsedExpressionGreaterThanOperator(),
            expectedNodeType: ExpressionType.GreaterThan,
            rightValue: Fixture.Create<int>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void GreaterThanOperatorProcess_ShouldThrowInvalidOperationException_WhenOperatorDoesNotExist()
    {
        Process_ShouldThrowException_WhenOperatorDoesNotExist<int, string, InvalidOperationException>(
            sut: new ParsedExpressionGreaterThanOperator() );
    }

    [Fact]
    public void LessThanOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable<int, int, bool>(
            sut: new ParsedExpressionLessThanOperator(),
            expectedNodeType: ExpressionType.LessThan,
            DefaultNodeAssertion );
    }

    [Fact]
    public void LessThanOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant<int, int, bool>(
            sut: new ParsedExpressionLessThanOperator(),
            expectedNodeType: ExpressionType.LessThan,
            leftValue: Fixture.Create<int>(),
            rightValue: Fixture.Create<int>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void LessThanOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant<int, int, bool>(
            sut: new ParsedExpressionLessThanOperator(),
            expectedNodeType: ExpressionType.LessThan,
            leftValue: Fixture.Create<int>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void LessThanOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant<int, int, bool>(
            sut: new ParsedExpressionLessThanOperator(),
            expectedNodeType: ExpressionType.LessThan,
            rightValue: Fixture.Create<int>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void LessThanOperatorProcess_ShouldThrowInvalidOperationException_WhenOperatorDoesNotExist()
    {
        Process_ShouldThrowException_WhenOperatorDoesNotExist<int, string, InvalidOperationException>(
            sut: new ParsedExpressionLessThanOperator() );
    }

    [Fact]
    public void GreaterThanOrEqualToOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable<int, int, bool>(
            sut: new ParsedExpressionGreaterThanOrEqualToOperator(),
            expectedNodeType: ExpressionType.GreaterThanOrEqual,
            DefaultNodeAssertion );
    }

    [Fact]
    public void GreaterThanOrEqualToOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant<int, int, bool>(
            sut: new ParsedExpressionGreaterThanOrEqualToOperator(),
            expectedNodeType: ExpressionType.GreaterThanOrEqual,
            leftValue: Fixture.Create<int>(),
            rightValue: Fixture.Create<int>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void GreaterThanOrEqualToOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant<int, int, bool>(
            sut: new ParsedExpressionGreaterThanOrEqualToOperator(),
            expectedNodeType: ExpressionType.GreaterThanOrEqual,
            leftValue: Fixture.Create<int>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void GreaterThanOrEqualToOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant<int, int, bool>(
            sut: new ParsedExpressionGreaterThanOrEqualToOperator(),
            expectedNodeType: ExpressionType.GreaterThanOrEqual,
            rightValue: Fixture.Create<int>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void GreaterThanOrEqualToOperatorProcess_ShouldThrowInvalidOperationException_WhenOperatorDoesNotExist()
    {
        Process_ShouldThrowException_WhenOperatorDoesNotExist<int, string, InvalidOperationException>(
            sut: new ParsedExpressionGreaterThanOrEqualToOperator() );
    }

    [Fact]
    public void LessThanOrEqualToOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable<int, int, bool>(
            sut: new ParsedExpressionLessThanOrEqualToOperator(),
            expectedNodeType: ExpressionType.LessThanOrEqual,
            DefaultNodeAssertion );
    }

    [Fact]
    public void LessThanOrEqualToOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant<int, int, bool>(
            sut: new ParsedExpressionLessThanOrEqualToOperator(),
            expectedNodeType: ExpressionType.LessThanOrEqual,
            leftValue: Fixture.Create<int>(),
            rightValue: Fixture.Create<int>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void LessThanOrEqualToOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant<int, int, bool>(
            sut: new ParsedExpressionLessThanOrEqualToOperator(),
            expectedNodeType: ExpressionType.LessThanOrEqual,
            leftValue: Fixture.Create<int>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void LessThanOrEqualToOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant<int, int, bool>(
            sut: new ParsedExpressionLessThanOrEqualToOperator(),
            expectedNodeType: ExpressionType.LessThanOrEqual,
            rightValue: Fixture.Create<int>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void LessThanOrEqualToOperatorProcess_ShouldThrowInvalidOperationException_WhenOperatorDoesNotExist()
    {
        Process_ShouldThrowException_WhenOperatorDoesNotExist<int, string, InvalidOperationException>(
            sut: new ParsedExpressionLessThanOrEqualToOperator() );
    }

    [Fact]
    public void BitwiseAndOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable<int, int, int>(
            sut: new ParsedExpressionBitwiseAndOperator(),
            expectedNodeType: ExpressionType.And,
            DefaultNodeAssertion );
    }

    [Fact]
    public void BitwiseAndOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant<int, int, int>(
            sut: new ParsedExpressionBitwiseAndOperator(),
            expectedNodeType: ExpressionType.And,
            leftValue: Fixture.Create<int>(),
            rightValue: Fixture.Create<int>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void BitwiseAndOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant<int, int, int>(
            sut: new ParsedExpressionBitwiseAndOperator(),
            expectedNodeType: ExpressionType.And,
            leftValue: Fixture.Create<int>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void BitwiseAndOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant<int, int, int>(
            sut: new ParsedExpressionBitwiseAndOperator(),
            expectedNodeType: ExpressionType.And,
            rightValue: Fixture.Create<int>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void BitwiseAndOperatorProcess_ShouldThrowInvalidOperationException_WhenOperatorDoesNotExist()
    {
        Process_ShouldThrowException_WhenOperatorDoesNotExist<int, string, InvalidOperationException>(
            sut: new ParsedExpressionBitwiseAndOperator() );
    }

    [Fact]
    public void BitwiseOrOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable<int, int, int>(
            sut: new ParsedExpressionBitwiseOrOperator(),
            expectedNodeType: ExpressionType.Or,
            DefaultNodeAssertion );
    }

    [Fact]
    public void BitwiseOrOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant<int, int, int>(
            sut: new ParsedExpressionBitwiseOrOperator(),
            expectedNodeType: ExpressionType.Or,
            leftValue: Fixture.Create<int>(),
            rightValue: Fixture.Create<int>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void BitwiseOrOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant<int, int, int>(
            sut: new ParsedExpressionBitwiseOrOperator(),
            expectedNodeType: ExpressionType.Or,
            leftValue: Fixture.Create<int>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void BitwiseOrOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant<int, int, int>(
            sut: new ParsedExpressionBitwiseOrOperator(),
            expectedNodeType: ExpressionType.Or,
            rightValue: Fixture.Create<int>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void BitwiseOrOperatorProcess_ShouldThrowInvalidOperationException_WhenOperatorDoesNotExist()
    {
        Process_ShouldThrowException_WhenOperatorDoesNotExist<int, string, InvalidOperationException>(
            sut: new ParsedExpressionBitwiseOrOperator() );
    }

    [Fact]
    public void BitwiseXorOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable<int, int, int>(
            sut: new ParsedExpressionBitwiseXorOperator(),
            expectedNodeType: ExpressionType.ExclusiveOr,
            DefaultNodeAssertion );
    }

    [Fact]
    public void BitwiseXorOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant<int, int, int>(
            sut: new ParsedExpressionBitwiseXorOperator(),
            expectedNodeType: ExpressionType.ExclusiveOr,
            leftValue: Fixture.Create<int>(),
            rightValue: Fixture.Create<int>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void BitwiseXorOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant<int, int, int>(
            sut: new ParsedExpressionBitwiseXorOperator(),
            expectedNodeType: ExpressionType.ExclusiveOr,
            leftValue: Fixture.Create<int>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void BitwiseXorOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant<int, int, int>(
            sut: new ParsedExpressionBitwiseXorOperator(),
            expectedNodeType: ExpressionType.ExclusiveOr,
            rightValue: Fixture.Create<int>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void BitwiseXorOperatorProcess_ShouldThrowInvalidOperationException_WhenOperatorDoesNotExist()
    {
        Process_ShouldThrowException_WhenOperatorDoesNotExist<int, string, InvalidOperationException>(
            sut: new ParsedExpressionBitwiseXorOperator() );
    }

    [Fact]
    public void BitwiseLeftShiftOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable<int, int, int>(
            sut: new ParsedExpressionBitwiseLeftShiftOperator(),
            expectedNodeType: ExpressionType.LeftShift,
            DefaultNodeAssertion );
    }

    [Fact]
    public void BitwiseLeftShiftOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant<int, int, int>(
            sut: new ParsedExpressionBitwiseLeftShiftOperator(),
            expectedNodeType: ExpressionType.LeftShift,
            leftValue: Fixture.Create<int>(),
            rightValue: Fixture.Create<int>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void BitwiseLeftShiftOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant<int, int, int>(
            sut: new ParsedExpressionBitwiseLeftShiftOperator(),
            expectedNodeType: ExpressionType.LeftShift,
            leftValue: Fixture.Create<int>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void BitwiseLeftShiftOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant<int, int, int>(
            sut: new ParsedExpressionBitwiseLeftShiftOperator(),
            expectedNodeType: ExpressionType.LeftShift,
            rightValue: Fixture.Create<int>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void BitwiseLeftShiftOperatorProcess_ShouldThrowInvalidOperationException_WhenOperatorDoesNotExist()
    {
        Process_ShouldThrowException_WhenOperatorDoesNotExist<int, string, InvalidOperationException>(
            sut: new ParsedExpressionBitwiseLeftShiftOperator() );
    }

    [Fact]
    public void BitwiseRightShiftOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable<int, int, int>(
            sut: new ParsedExpressionBitwiseRightShiftOperator(),
            expectedNodeType: ExpressionType.RightShift,
            DefaultNodeAssertion );
    }

    [Fact]
    public void BitwiseRightShiftOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant<int, int, int>(
            sut: new ParsedExpressionBitwiseRightShiftOperator(),
            expectedNodeType: ExpressionType.RightShift,
            leftValue: Fixture.Create<int>(),
            rightValue: Fixture.Create<int>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void BitwiseRightShiftOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant<int, int, int>(
            sut: new ParsedExpressionBitwiseRightShiftOperator(),
            expectedNodeType: ExpressionType.RightShift,
            leftValue: Fixture.Create<int>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void BitwiseRightShiftOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant<int, int, int>(
            sut: new ParsedExpressionBitwiseRightShiftOperator(),
            expectedNodeType: ExpressionType.RightShift,
            rightValue: Fixture.Create<int>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void BitwiseRightShiftOperatorProcess_ShouldThrowInvalidOperationException_WhenOperatorDoesNotExist()
    {
        Process_ShouldThrowException_WhenOperatorDoesNotExist<int, string, InvalidOperationException>(
            sut: new ParsedExpressionBitwiseRightShiftOperator() );
    }

    [Fact]
    public void CompareOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable<string, string, int>(
            sut: new ParsedExpressionCompareOperator(),
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
            sut: new ParsedExpressionCompareOperator(),
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
            sut: new ParsedExpressionCompareOperator(),
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
            sut: new ParsedExpressionCompareOperator(),
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
        Process_ShouldThrowException_WhenOperatorDoesNotExist<int, string, ParsedExpressionConstructException>(
            sut: new ParsedExpressionCompareOperator() );
    }

    [Fact]
    public void CoalesceOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable<string, string, string>(
            sut: new ParsedExpressionCoalesceOperator(),
            expectedNodeType: ExpressionType.Coalesce,
            DefaultNodeAssertion );
    }

    [Fact]
    public void CoalesceOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant<string, string, string>(
            sut: new ParsedExpressionCoalesceOperator(),
            expectedNodeType: ExpressionType.Coalesce,
            leftValue: Fixture.Create<string>(),
            rightValue: Fixture.Create<string>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void CoalesceOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant<string, string, string>(
            sut: new ParsedExpressionCoalesceOperator(),
            expectedNodeType: ExpressionType.Coalesce,
            leftValue: Fixture.Create<string>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void CoalesceOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant<string, string, string>(
            sut: new ParsedExpressionCoalesceOperator(),
            expectedNodeType: ExpressionType.Coalesce,
            rightValue: Fixture.Create<string>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void CoalesceOperatorProcess_ShouldThrowInvalidOperationException_WhenOperatorDoesNotExist()
    {
        Process_ShouldThrowException_WhenOperatorDoesNotExist<int, string, InvalidOperationException>(
            sut: new ParsedExpressionCoalesceOperator() );
    }
}
