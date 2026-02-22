using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Numerics;
using LfrlAnvil.Computable.Expressions.Constructs.String;
using LfrlAnvil.Functional;

namespace LfrlAnvil.Computable.Expressions.Tests.ConstructsTests.StringTests;

public class StringTypeConverterTests : TypeConvertersTestsBase
{
    [Fact]
    public void TypeConverterProcess_ShouldPopOneOperandAndPushOneExpression_WhenOperandIsVariable()
    {
        Process_ShouldPopOneOperandAndPushOneExpression_WhenOperandIsVariable<string, int, string>(
            sut: new ParsedExpressionToStringTypeConverter(),
            (operand, result) => result.TestType()
                .AssignableTo<MethodCallExpression>( methodCallResult => Assertion.All(
                    "methodCallResult",
                    methodCallResult.Object.TestRefEquals( operand ),
                    methodCallResult.Arguments.TestEmpty(),
                    methodCallResult.Method.Name.TestEquals( nameof( ToString ) ) ) ),
            expectedNodeType: ExpressionType.Call );
    }

    [Fact]
    public void TypeConverterProcess_ShouldPopOneOperandAndPushOneExpression_WhenOperandIsConstantAndNotNull()
    {
        Process_ShouldPopOneOperandAndPushOneExpression_WhenOperandIsConstant<string, int, string>(
            sut: new ParsedExpressionToStringTypeConverter(),
            operandValue: 123,
            (_, result) => result.TestType()
                .AssignableTo<ConstantExpression>( constantResult =>
                    constantResult.Value.TestEquals( "123" ) ),
            expectedNodeType: ExpressionType.Constant );
    }

    [Fact]
    public void TypeConverterProcess_ShouldThrowArgumentNullException_WhenOperandIsConstantAndNull()
    {
        var sut = new ParsedExpressionToStringTypeConverter();
        var operand = CreateConstantOperand( ( object? )null );

        var action = Lambda.Of( () => sut.Process( operand ) );

        action.Test( exc => exc.TestType().Exact<ArgumentNullException>() ).Go();
    }

    [Fact]
    public void FromDecimalTypeConverterProcess_ShouldPopOneOperandAndPushOneExpression_WhenOperandIsVariable()
    {
        var formatProvider = CultureInfo.InvariantCulture;

        Process_ShouldPopOneOperandAndPushOneExpression_WhenOperandIsVariable<string, decimal, string>(
            sut: new ParsedExpressionDecimalToStringTypeConverter( formatProvider ),
            (operand, result) => result.TestType()
                .AssignableTo<MethodCallExpression>( methodCallResult => Assertion.All(
                    "methodCallResult",
                    methodCallResult.Object.TestRefEquals( operand ),
                    methodCallResult.Method.Name.TestEquals( nameof( ToString ) ),
                    methodCallResult.Arguments.Count.TestEquals( 1 ),
                    methodCallResult.Arguments.FirstOrDefault()
                        .TestType()
                        .AssignableTo<ConstantExpression>( constantArgument =>
                            constantArgument.Value.TestRefEquals( formatProvider ) ) ) ),
            expectedNodeType: ExpressionType.Call );
    }

    [Fact]
    public void FromDecimalTypeConverterProcess_ShouldPopOneOperandAndPushOneExpression_WhenOperandIsConstantAndNotNull()
    {
        Process_ShouldPopOneOperandAndPushOneExpression_WhenOperandIsConstant<string, decimal, string>(
            sut: new ParsedExpressionDecimalToStringTypeConverter( CultureInfo.InvariantCulture ),
            operandValue: 123,
            (_, result) => result.TestType()
                .AssignableTo<ConstantExpression>( constantResult =>
                    constantResult.Value.TestEquals( "123" ) ),
            expectedNodeType: ExpressionType.Constant );
    }

    [Fact]
    public void FromDoubleTypeConverterProcess_ShouldPopOneOperandAndPushOneExpression_WhenOperandIsVariable()
    {
        var formatProvider = CultureInfo.InvariantCulture;

        Process_ShouldPopOneOperandAndPushOneExpression_WhenOperandIsVariable<string, double, string>(
            sut: new ParsedExpressionDoubleToStringTypeConverter( formatProvider ),
            (operand, result) => result.TestType()
                .AssignableTo<MethodCallExpression>( methodCallResult => Assertion.All(
                    "methodCallResult",
                    methodCallResult.Object.TestRefEquals( operand ),
                    methodCallResult.Method.Name.TestEquals( nameof( ToString ) ),
                    methodCallResult.Arguments.Count.TestEquals( 1 ),
                    methodCallResult.Arguments.FirstOrDefault()
                        .TestType()
                        .AssignableTo<ConstantExpression>( constantArgument =>
                            constantArgument.Value.TestRefEquals( formatProvider ) ) ) ),
            expectedNodeType: ExpressionType.Call );
    }

    [Fact]
    public void FromDoubleTypeConverterProcess_ShouldPopOneOperandAndPushOneExpression_WhenOperandIsConstantAndNotNull()
    {
        Process_ShouldPopOneOperandAndPushOneExpression_WhenOperandIsConstant<string, double, string>(
            sut: new ParsedExpressionDoubleToStringTypeConverter( CultureInfo.InvariantCulture ),
            operandValue: 123,
            (_, result) => result.TestType()
                .AssignableTo<ConstantExpression>( constantResult =>
                    constantResult.Value.TestEquals( "123" ) ),
            expectedNodeType: ExpressionType.Constant );
    }

    [Fact]
    public void FromFloatTypeConverterProcess_ShouldPopOneOperandAndPushOneExpression_WhenOperandIsVariable()
    {
        var formatProvider = CultureInfo.InvariantCulture;

        Process_ShouldPopOneOperandAndPushOneExpression_WhenOperandIsVariable<string, float, string>(
            sut: new ParsedExpressionFloatToStringTypeConverter( formatProvider ),
            (operand, result) => result.TestType()
                .AssignableTo<MethodCallExpression>( methodCallResult => Assertion.All(
                    "methodCallResult",
                    methodCallResult.Object.TestRefEquals( operand ),
                    methodCallResult.Method.Name.TestEquals( nameof( ToString ) ),
                    methodCallResult.Arguments.Count.TestEquals( 1 ),
                    methodCallResult.Arguments.FirstOrDefault()
                        .TestType()
                        .AssignableTo<ConstantExpression>( constantArgument =>
                            constantArgument.Value.TestRefEquals( formatProvider ) ) ) ),
            expectedNodeType: ExpressionType.Call );
    }

    [Fact]
    public void FromFloatTypeConverterProcess_ShouldPopOneOperandAndPushOneExpression_WhenOperandIsConstantAndNotNull()
    {
        Process_ShouldPopOneOperandAndPushOneExpression_WhenOperandIsConstant<string, float, string>(
            sut: new ParsedExpressionFloatToStringTypeConverter( CultureInfo.InvariantCulture ),
            operandValue: 123,
            (_, result) => result.TestType()
                .AssignableTo<ConstantExpression>( constantResult =>
                    constantResult.Value.TestEquals( "123" ) ),
            expectedNodeType: ExpressionType.Constant );
    }

    [Fact]
    public void FromBigIntegerTypeConverterProcess_ShouldPopOneOperandAndPushOneExpression_WhenOperandIsVariable()
    {
        var formatProvider = CultureInfo.InvariantCulture;

        Process_ShouldPopOneOperandAndPushOneExpression_WhenOperandIsVariable<string, BigInteger, string>(
            sut: new ParsedExpressionBigIntToStringTypeConverter( formatProvider ),
            (operand, result) => result.TestType()
                .AssignableTo<MethodCallExpression>( methodCallResult => Assertion.All(
                    "methodCallResult",
                    methodCallResult.Object.TestRefEquals( operand ),
                    methodCallResult.Method.Name.TestEquals( nameof( ToString ) ),
                    methodCallResult.Arguments.Count.TestEquals( 1 ),
                    methodCallResult.Arguments.FirstOrDefault()
                        .TestType()
                        .AssignableTo<ConstantExpression>( constantArgument =>
                            constantArgument.Value.TestRefEquals( formatProvider ) ) ) ),
            expectedNodeType: ExpressionType.Call );
    }

    [Fact]
    public void FromBigIntegerTypeConverterProcess_ShouldPopOneOperandAndPushOneExpression_WhenOperandIsConstantAndNotNull()
    {
        Process_ShouldPopOneOperandAndPushOneExpression_WhenOperandIsConstant<string, BigInteger, string>(
            sut: new ParsedExpressionBigIntToStringTypeConverter( CultureInfo.InvariantCulture ),
            operandValue: 123,
            (_, result) => result.TestType()
                .AssignableTo<ConstantExpression>( constantResult =>
                    constantResult.Value.TestEquals( "123" ) ),
            expectedNodeType: ExpressionType.Constant );
    }

    [Fact]
    public void FromInt64TypeConverterProcess_ShouldPopOneOperandAndPushOneExpression_WhenOperandIsVariable()
    {
        var formatProvider = CultureInfo.InvariantCulture;

        Process_ShouldPopOneOperandAndPushOneExpression_WhenOperandIsVariable<string, long, string>(
            sut: new ParsedExpressionInt64ToStringTypeConverter( formatProvider ),
            (operand, result) => result.TestType()
                .AssignableTo<MethodCallExpression>( methodCallResult => Assertion.All(
                    "methodCallResult",
                    methodCallResult.Object.TestRefEquals( operand ),
                    methodCallResult.Method.Name.TestEquals( nameof( ToString ) ),
                    methodCallResult.Arguments.Count.TestEquals( 1 ),
                    methodCallResult.Arguments.FirstOrDefault()
                        .TestType()
                        .AssignableTo<ConstantExpression>( constantArgument =>
                            constantArgument.Value.TestRefEquals( formatProvider ) ) ) ),
            expectedNodeType: ExpressionType.Call );
    }

    [Fact]
    public void FromInt64TypeConverterProcess_ShouldPopOneOperandAndPushOneExpression_WhenOperandIsConstantAndNotNull()
    {
        Process_ShouldPopOneOperandAndPushOneExpression_WhenOperandIsConstant<string, long, string>(
            sut: new ParsedExpressionInt64ToStringTypeConverter( CultureInfo.InvariantCulture ),
            operandValue: 123,
            (_, result) => result.TestType()
                .AssignableTo<ConstantExpression>( constantResult =>
                    constantResult.Value.TestEquals( "123" ) ),
            expectedNodeType: ExpressionType.Constant );
    }

    [Fact]
    public void FromInt32TypeConverterProcess_ShouldPopOneOperandAndPushOneExpression_WhenOperandIsVariable()
    {
        var formatProvider = CultureInfo.InvariantCulture;

        Process_ShouldPopOneOperandAndPushOneExpression_WhenOperandIsVariable<string, int, string>(
            sut: new ParsedExpressionInt32ToStringTypeConverter( formatProvider ),
            (operand, result) => result.TestType()
                .AssignableTo<MethodCallExpression>( methodCallResult => Assertion.All(
                    "methodCallResult",
                    methodCallResult.Object.TestRefEquals( operand ),
                    methodCallResult.Method.Name.TestEquals( nameof( ToString ) ),
                    methodCallResult.Arguments.Count.TestEquals( 1 ),
                    methodCallResult.Arguments.FirstOrDefault()
                        .TestType()
                        .AssignableTo<ConstantExpression>( constantArgument =>
                            constantArgument.Value.TestRefEquals( formatProvider ) ) ) ),
            expectedNodeType: ExpressionType.Call );
    }

    [Fact]
    public void FromInt32TypeConverterProcess_ShouldPopOneOperandAndPushOneExpression_WhenOperandIsConstantAndNotNull()
    {
        Process_ShouldPopOneOperandAndPushOneExpression_WhenOperandIsConstant<string, int, string>(
            sut: new ParsedExpressionInt32ToStringTypeConverter( CultureInfo.InvariantCulture ),
            operandValue: 123,
            (_, result) => result.TestType()
                .AssignableTo<ConstantExpression>( constantResult =>
                    constantResult.Value.TestEquals( "123" ) ),
            expectedNodeType: ExpressionType.Constant );
    }

    [Fact]
    public void FromBooleanTypeConverterProcess_ShouldPopOneOperandAndPushOneExpression_WhenOperandIsVariable()
    {
        var formatProvider = CultureInfo.InvariantCulture;

        Process_ShouldPopOneOperandAndPushOneExpression_WhenOperandIsVariable<string, bool, string>(
            sut: new ParsedExpressionBooleanToStringTypeConverter( formatProvider ),
            (operand, result) => result.TestType()
                .AssignableTo<MethodCallExpression>( methodCallResult => Assertion.All(
                    "methodCallResult",
                    methodCallResult.Object.TestRefEquals( operand ),
                    methodCallResult.Method.Name.TestEquals( nameof( ToString ) ),
                    methodCallResult.Arguments.Count.TestEquals( 1 ),
                    methodCallResult.Arguments.FirstOrDefault()
                        .TestType()
                        .AssignableTo<ConstantExpression>( constantArgument =>
                            constantArgument.Value.TestRefEquals( formatProvider ) ) ) ),
            expectedNodeType: ExpressionType.Call );
    }

    [Fact]
    public void FromBooleanTypeConverterProcess_ShouldPopOneOperandAndPushOneExpression_WhenOperandIsConstantAndNotNull()
    {
        Process_ShouldPopOneOperandAndPushOneExpression_WhenOperandIsConstant<string, bool, string>(
            sut: new ParsedExpressionBooleanToStringTypeConverter( CultureInfo.InvariantCulture ),
            operandValue: true,
            (_, result) => result.TestType()
                .AssignableTo<ConstantExpression>( constantResult =>
                    constantResult.Value.TestEquals( "True" ) ),
            expectedNodeType: ExpressionType.Constant );
    }
}
