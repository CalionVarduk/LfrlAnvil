using System;
using System.Globalization;
using System.Linq.Expressions;
using System.Numerics;
using FluentAssertions;
using LfrlAnvil.Functional;
using LfrlAnvil.Computable.Expressions.Constructs.String;
using Xunit;

namespace LfrlAnvil.Computable.Expressions.Tests.ConstructsTests.StringTests;

public class StringTypeConverterTests : TypeConvertersTestsBase
{
    [Fact]
    public static void TypeConverterProcess_ShouldPopOneOperandAndPushOneExpression_WhenOperandIsVariable()
    {
        Process_ShouldPopOneOperandAndPushOneExpression_WhenOperandIsVariable<string, int, string>(
            sut: new MathExpressionToStringTypeConverter(),
            (operand, result) =>
            {
                result.Should().BeAssignableTo<MethodCallExpression>();
                if ( result is not MethodCallExpression methodCallResult )
                    return;

                methodCallResult.Object.Should().BeSameAs( operand );
                methodCallResult.Arguments.Should().BeEmpty();
                methodCallResult.Method.Name.Should().Be( nameof( ToString ) );
            },
            expectedNodeType: ExpressionType.Call );
    }

    [Fact]
    public static void TypeConverterProcess_ShouldPopOneOperandAndPushOneExpression_WhenOperandIsConstantAndNotNull()
    {
        Process_ShouldPopOneOperandAndPushOneExpression_WhenOperandIsConstant<string, int, string>(
            sut: new MathExpressionToStringTypeConverter(),
            operandValue: 123,
            (_, result) =>
            {
                result.Should().BeAssignableTo<ConstantExpression>();
                if ( result is not ConstantExpression constantResult )
                    return;

                constantResult.Value.Should().Be( "123" );
            },
            expectedNodeType: ExpressionType.Constant );
    }

    [Fact]
    public static void TypeConverterProcess_ShouldThrowArgumentNullException_WhenOperandIsConstantAndNull()
    {
        var sut = new MathExpressionToStringTypeConverter();
        var operand = CreateConstantOperand( (object?)null );
        var stack = CreateStack( operand );

        var action = Lambda.Of( () => sut.Process( stack ) );

        action.Should().ThrowExactly<ArgumentNullException>();
    }

    [Fact]
    public static void FromDecimalTypeConverterProcess_ShouldPopOneOperandAndPushOneExpression_WhenOperandIsVariable()
    {
        var formatProvider = CultureInfo.InvariantCulture;

        Process_ShouldPopOneOperandAndPushOneExpression_WhenOperandIsVariable<string, decimal, string>(
            sut: new MathExpressionDecimalToStringTypeConverter( formatProvider ),
            (operand, result) =>
            {
                result.Should().BeAssignableTo<MethodCallExpression>();
                if ( result is not MethodCallExpression methodCallResult )
                    return;

                methodCallResult.Object.Should().BeSameAs( operand );
                methodCallResult.Method.Name.Should().Be( nameof( ToString ) );
                methodCallResult.Arguments.Should().HaveCount( 1 );
                if ( methodCallResult.Arguments.Count != 1 )
                    return;

                methodCallResult.Arguments[0].Should().BeAssignableTo<ConstantExpression>();
                if ( methodCallResult.Arguments[0] is not ConstantExpression constantArgument )
                    return;

                constantArgument.Value.Should().BeSameAs( formatProvider );
            },
            expectedNodeType: ExpressionType.Call );
    }

    [Fact]
    public static void FromDecimalTypeConverterProcess_ShouldPopOneOperandAndPushOneExpression_WhenOperandIsConstantAndNotNull()
    {
        Process_ShouldPopOneOperandAndPushOneExpression_WhenOperandIsConstant<string, decimal, string>(
            sut: new MathExpressionDecimalToStringTypeConverter( CultureInfo.InvariantCulture ),
            operandValue: 123,
            (_, result) =>
            {
                result.Should().BeAssignableTo<ConstantExpression>();
                if ( result is not ConstantExpression constantResult )
                    return;

                constantResult.Value.Should().Be( "123" );
            },
            expectedNodeType: ExpressionType.Constant );
    }

    [Fact]
    public static void FromDoubleTypeConverterProcess_ShouldPopOneOperandAndPushOneExpression_WhenOperandIsVariable()
    {
        var formatProvider = CultureInfo.InvariantCulture;

        Process_ShouldPopOneOperandAndPushOneExpression_WhenOperandIsVariable<string, double, string>(
            sut: new MathExpressionDoubleToStringTypeConverter( formatProvider ),
            (operand, result) =>
            {
                result.Should().BeAssignableTo<MethodCallExpression>();
                if ( result is not MethodCallExpression methodCallResult )
                    return;

                methodCallResult.Object.Should().BeSameAs( operand );
                methodCallResult.Method.Name.Should().Be( nameof( ToString ) );
                methodCallResult.Arguments.Should().HaveCount( 1 );
                if ( methodCallResult.Arguments.Count != 1 )
                    return;

                methodCallResult.Arguments[0].Should().BeAssignableTo<ConstantExpression>();
                if ( methodCallResult.Arguments[0] is not ConstantExpression constantArgument )
                    return;

                constantArgument.Value.Should().BeSameAs( formatProvider );
            },
            expectedNodeType: ExpressionType.Call );
    }

    [Fact]
    public static void FromDoubleTypeConverterProcess_ShouldPopOneOperandAndPushOneExpression_WhenOperandIsConstantAndNotNull()
    {
        Process_ShouldPopOneOperandAndPushOneExpression_WhenOperandIsConstant<string, double, string>(
            sut: new MathExpressionDoubleToStringTypeConverter( CultureInfo.InvariantCulture ),
            operandValue: 123,
            (_, result) =>
            {
                result.Should().BeAssignableTo<ConstantExpression>();
                if ( result is not ConstantExpression constantResult )
                    return;

                constantResult.Value.Should().Be( "123" );
            },
            expectedNodeType: ExpressionType.Constant );
    }

    [Fact]
    public static void FromFloatTypeConverterProcess_ShouldPopOneOperandAndPushOneExpression_WhenOperandIsVariable()
    {
        var formatProvider = CultureInfo.InvariantCulture;

        Process_ShouldPopOneOperandAndPushOneExpression_WhenOperandIsVariable<string, float, string>(
            sut: new MathExpressionFloatToStringTypeConverter( formatProvider ),
            (operand, result) =>
            {
                result.Should().BeAssignableTo<MethodCallExpression>();
                if ( result is not MethodCallExpression methodCallResult )
                    return;

                methodCallResult.Object.Should().BeSameAs( operand );
                methodCallResult.Method.Name.Should().Be( nameof( ToString ) );
                methodCallResult.Arguments.Should().HaveCount( 1 );
                if ( methodCallResult.Arguments.Count != 1 )
                    return;

                methodCallResult.Arguments[0].Should().BeAssignableTo<ConstantExpression>();
                if ( methodCallResult.Arguments[0] is not ConstantExpression constantArgument )
                    return;

                constantArgument.Value.Should().BeSameAs( formatProvider );
            },
            expectedNodeType: ExpressionType.Call );
    }

    [Fact]
    public static void FromFloatTypeConverterProcess_ShouldPopOneOperandAndPushOneExpression_WhenOperandIsConstantAndNotNull()
    {
        Process_ShouldPopOneOperandAndPushOneExpression_WhenOperandIsConstant<string, float, string>(
            sut: new MathExpressionFloatToStringTypeConverter( CultureInfo.InvariantCulture ),
            operandValue: 123,
            (_, result) =>
            {
                result.Should().BeAssignableTo<ConstantExpression>();
                if ( result is not ConstantExpression constantResult )
                    return;

                constantResult.Value.Should().Be( "123" );
            },
            expectedNodeType: ExpressionType.Constant );
    }

    [Fact]
    public static void FromBigIntegerTypeConverterProcess_ShouldPopOneOperandAndPushOneExpression_WhenOperandIsVariable()
    {
        var formatProvider = CultureInfo.InvariantCulture;

        Process_ShouldPopOneOperandAndPushOneExpression_WhenOperandIsVariable<string, BigInteger, string>(
            sut: new MathExpressionBigIntToStringTypeConverter( formatProvider ),
            (operand, result) =>
            {
                result.Should().BeAssignableTo<MethodCallExpression>();
                if ( result is not MethodCallExpression methodCallResult )
                    return;

                methodCallResult.Object.Should().BeSameAs( operand );
                methodCallResult.Method.Name.Should().Be( nameof( ToString ) );
                methodCallResult.Arguments.Should().HaveCount( 1 );
                if ( methodCallResult.Arguments.Count != 1 )
                    return;

                methodCallResult.Arguments[0].Should().BeAssignableTo<ConstantExpression>();
                if ( methodCallResult.Arguments[0] is not ConstantExpression constantArgument )
                    return;

                constantArgument.Value.Should().BeSameAs( formatProvider );
            },
            expectedNodeType: ExpressionType.Call );
    }

    [Fact]
    public static void FromBigIntegerTypeConverterProcess_ShouldPopOneOperandAndPushOneExpression_WhenOperandIsConstantAndNotNull()
    {
        Process_ShouldPopOneOperandAndPushOneExpression_WhenOperandIsConstant<string, BigInteger, string>(
            sut: new MathExpressionBigIntToStringTypeConverter( CultureInfo.InvariantCulture ),
            operandValue: 123,
            (_, result) =>
            {
                result.Should().BeAssignableTo<ConstantExpression>();
                if ( result is not ConstantExpression constantResult )
                    return;

                constantResult.Value.Should().Be( "123" );
            },
            expectedNodeType: ExpressionType.Constant );
    }

    [Fact]
    public static void FromInt64TypeConverterProcess_ShouldPopOneOperandAndPushOneExpression_WhenOperandIsVariable()
    {
        var formatProvider = CultureInfo.InvariantCulture;

        Process_ShouldPopOneOperandAndPushOneExpression_WhenOperandIsVariable<string, long, string>(
            sut: new MathExpressionInt64ToStringTypeConverter( formatProvider ),
            (operand, result) =>
            {
                result.Should().BeAssignableTo<MethodCallExpression>();
                if ( result is not MethodCallExpression methodCallResult )
                    return;

                methodCallResult.Object.Should().BeSameAs( operand );
                methodCallResult.Method.Name.Should().Be( nameof( ToString ) );
                methodCallResult.Arguments.Should().HaveCount( 1 );
                if ( methodCallResult.Arguments.Count != 1 )
                    return;

                methodCallResult.Arguments[0].Should().BeAssignableTo<ConstantExpression>();
                if ( methodCallResult.Arguments[0] is not ConstantExpression constantArgument )
                    return;

                constantArgument.Value.Should().BeSameAs( formatProvider );
            },
            expectedNodeType: ExpressionType.Call );
    }

    [Fact]
    public static void FromInt64TypeConverterProcess_ShouldPopOneOperandAndPushOneExpression_WhenOperandIsConstantAndNotNull()
    {
        Process_ShouldPopOneOperandAndPushOneExpression_WhenOperandIsConstant<string, long, string>(
            sut: new MathExpressionInt64ToStringTypeConverter( CultureInfo.InvariantCulture ),
            operandValue: 123,
            (_, result) =>
            {
                result.Should().BeAssignableTo<ConstantExpression>();
                if ( result is not ConstantExpression constantResult )
                    return;

                constantResult.Value.Should().Be( "123" );
            },
            expectedNodeType: ExpressionType.Constant );
    }

    [Fact]
    public static void FromInt32TypeConverterProcess_ShouldPopOneOperandAndPushOneExpression_WhenOperandIsVariable()
    {
        var formatProvider = CultureInfo.InvariantCulture;

        Process_ShouldPopOneOperandAndPushOneExpression_WhenOperandIsVariable<string, int, string>(
            sut: new MathExpressionInt32ToStringTypeConverter( formatProvider ),
            (operand, result) =>
            {
                result.Should().BeAssignableTo<MethodCallExpression>();
                if ( result is not MethodCallExpression methodCallResult )
                    return;

                methodCallResult.Object.Should().BeSameAs( operand );
                methodCallResult.Method.Name.Should().Be( nameof( ToString ) );
                methodCallResult.Arguments.Should().HaveCount( 1 );
                if ( methodCallResult.Arguments.Count != 1 )
                    return;

                methodCallResult.Arguments[0].Should().BeAssignableTo<ConstantExpression>();
                if ( methodCallResult.Arguments[0] is not ConstantExpression constantArgument )
                    return;

                constantArgument.Value.Should().BeSameAs( formatProvider );
            },
            expectedNodeType: ExpressionType.Call );
    }

    [Fact]
    public static void FromInt32TypeConverterProcess_ShouldPopOneOperandAndPushOneExpression_WhenOperandIsConstantAndNotNull()
    {
        Process_ShouldPopOneOperandAndPushOneExpression_WhenOperandIsConstant<string, int, string>(
            sut: new MathExpressionInt32ToStringTypeConverter( CultureInfo.InvariantCulture ),
            operandValue: 123,
            (_, result) =>
            {
                result.Should().BeAssignableTo<ConstantExpression>();
                if ( result is not ConstantExpression constantResult )
                    return;

                constantResult.Value.Should().Be( "123" );
            },
            expectedNodeType: ExpressionType.Constant );
    }

    [Fact]
    public static void FromBooleanTypeConverterProcess_ShouldPopOneOperandAndPushOneExpression_WhenOperandIsVariable()
    {
        var formatProvider = CultureInfo.InvariantCulture;

        Process_ShouldPopOneOperandAndPushOneExpression_WhenOperandIsVariable<string, bool, string>(
            sut: new MathExpressionBooleanToStringTypeConverter( formatProvider ),
            (operand, result) =>
            {
                result.Should().BeAssignableTo<MethodCallExpression>();
                if ( result is not MethodCallExpression methodCallResult )
                    return;

                methodCallResult.Object.Should().BeSameAs( operand );
                methodCallResult.Method.Name.Should().Be( nameof( ToString ) );
                methodCallResult.Arguments.Should().HaveCount( 1 );
                if ( methodCallResult.Arguments.Count != 1 )
                    return;

                methodCallResult.Arguments[0].Should().BeAssignableTo<ConstantExpression>();
                if ( methodCallResult.Arguments[0] is not ConstantExpression constantArgument )
                    return;

                constantArgument.Value.Should().BeSameAs( formatProvider );
            },
            expectedNodeType: ExpressionType.Call );
    }

    [Fact]
    public static void FromBooleanTypeConverterProcess_ShouldPopOneOperandAndPushOneExpression_WhenOperandIsConstantAndNotNull()
    {
        Process_ShouldPopOneOperandAndPushOneExpression_WhenOperandIsConstant<string, bool, string>(
            sut: new MathExpressionBooleanToStringTypeConverter( CultureInfo.InvariantCulture ),
            operandValue: true,
            (_, result) =>
            {
                result.Should().BeAssignableTo<ConstantExpression>();
                if ( result is not ConstantExpression constantResult )
                    return;

                constantResult.Value.Should().Be( "True" );
            },
            expectedNodeType: ExpressionType.Constant );
    }
}
