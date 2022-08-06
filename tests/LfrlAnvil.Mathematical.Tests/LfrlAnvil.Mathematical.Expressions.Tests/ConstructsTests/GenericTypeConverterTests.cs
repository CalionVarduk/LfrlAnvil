﻿using System;
using System.Linq.Expressions;
using AutoFixture;
using FluentAssertions;
using LfrlAnvil.Mathematical.Expressions.Constructs;
using LfrlAnvil.Mathematical.Expressions.Exceptions;
using Xunit;

namespace LfrlAnvil.Mathematical.Expressions.Tests.ConstructsTests;

public class GenericTypeConverterTests : TypeConvertersTestsBase
{
    [Fact]
    public void TypeConverterProcess_ShouldPopOneOperandAndPushOneExpression_WhenOperandIsVariable()
    {
        Process_ShouldPopOneOperandAndPushOneExpression_WhenOperandIsVariable<int, long, int>(
            sut: new MathExpressionTypeConverter<int>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void TypeConverterProcess_ShouldPopOneOperandAndPushOneExpression_WhenOperandIsConstant()
    {
        Process_ShouldPopOneOperandAndPushOneExpression_WhenOperandIsConstant<int, long, int>(
            sut: new MathExpressionTypeConverter<int>(),
            operandValue: Fixture.Create<long>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void TypeConverterProcess_ShouldPopOneOperandAndPushOneExpression_WhenOperandIsOfTargetType()
    {
        Process_ShouldPopOneOperandAndPushOneExpression_WhenOperandIsConstant<int, int, int>(
            sut: new MathExpressionTypeConverter<int>(),
            operandValue: Fixture.Create<int>(),
            (operand, result) => result.Should().BeSameAs( operand ),
            expectedNodeType: ExpressionType.Constant );
    }

    [Fact]
    public void TypeConverterProcess_ShouldPopOneOperandAndPushOneExpression_WhenConversionResultTypeIsDifferentButAssignableToTargetType()
    {
        Process_ShouldPopOneOperandAndPushOneExpression_WhenOperandIsConstant<object, string, object>(
            sut: new StringConverterWithObjectTargetType(),
            operandValue: Fixture.Create<string>(),
            (operand, result) =>
            {
                result.Should().BeAssignableTo<UnaryExpression>();
                if ( result is not UnaryExpression unaryResult )
                    return;

                unaryResult.Operand.Should().BeAssignableTo<UnaryExpression>();
                if ( unaryResult.Operand is not UnaryExpression intermediateOperand )
                    return;

                intermediateOperand.Operand.Should().BeSameAs( operand );
            } );
    }

    [Fact]
    public void TypeConverterProcess_ShouldThrowInvalidOperationException_WhenOperatorDoesNotExist()
    {
        Process_ShouldThrowException_WhenConversionDoesNotExist<int, string, InvalidOperationException>(
            sut: new MathExpressionTypeConverter<int>() );
    }

    [Fact]
    public void TypeConverterProcess_ShouldThrowMathExpressionTypeConverterException_WhenConversionResultIsNotAssignableToTargetType()
    {
        var converter = new StringConverterWithIntTargetType();
        Process_ShouldThrowException_WhenConversionDoesNotExist<int, string, MathExpressionTypeConverterException>(
            sut: converter,
            e => e.Converter == converter );
    }

    private sealed class StringConverterWithObjectTargetType : MathExpressionTypeConverter<object>
    {
        protected override Expression CreateConversionExpression(Expression operand)
        {
            return Expression.Convert( operand, typeof( string ) );
        }
    }

    private sealed class StringConverterWithIntTargetType : MathExpressionTypeConverter<int>
    {
        protected override Expression CreateConversionExpression(Expression operand)
        {
            return Expression.Convert( operand, typeof( string ) );
        }
    }
}
