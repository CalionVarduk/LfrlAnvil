using System.Linq;
using System.Linq.Expressions;
using LfrlAnvil.Computable.Expressions.Constructs.String;

namespace LfrlAnvil.Computable.Expressions.Tests.ConstructsTests.StringTests;

public class StringBinaryOperatorTests : BinaryOperatorsTestsBase
{
    [Fact]
    public void AddOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable<string, string, string>(
            sut: new ParsedExpressionAddStringOperator(),
            expectedNodeType: ExpressionType.Call,
            (left, right, result) => result.TestType()
                .AssignableTo<MethodCallExpression>(
                    methodCallResult => Assertion.All(
                        "methodCallResult",
                        methodCallResult.Object.TestNull(),
                        methodCallResult.Arguments.Count.TestEquals( 2 ),
                        methodCallResult.Method.Name.TestEquals( nameof( string.Concat ) ),
                        methodCallResult.Arguments.ElementAtOrDefault( 0 ).TestRefEquals( left ),
                        methodCallResult.Arguments.ElementAtOrDefault( 1 ).TestRefEquals( right ) ) ) );
    }

    [Fact]
    public void AddOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant<string, string, string>(
            sut: new ParsedExpressionAddStringOperator(),
            expectedNodeType: ExpressionType.Constant,
            leftValue: "foo",
            rightValue: "bar",
            (_, _, result) =>
                result.TestType().AssignableTo<ConstantExpression>( constantResult => constantResult.Value.TestEquals( "foobar" ) ) );
    }

    [Fact]
    public void AddOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstantAndNotEmpty()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant<string, string, string>(
            sut: new ParsedExpressionAddStringOperator(),
            expectedNodeType: ExpressionType.Call,
            leftValue: "foo",
            (left, right, result) => result.TestType()
                .AssignableTo<MethodCallExpression>(
                    methodCallResult => Assertion.All(
                        "methodCallResult",
                        methodCallResult.Object.TestNull(),
                        methodCallResult.Arguments.Count.TestEquals( 2 ),
                        methodCallResult.Method.Name.TestEquals( nameof( string.Concat ) ),
                        methodCallResult.Arguments.ElementAtOrDefault( 0 ).TestRefEquals( left ),
                        methodCallResult.Arguments.ElementAtOrDefault( 1 ).TestRefEquals( right ) ) ) );
    }

    [Fact]
    public void AddOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstantAndEmpty()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant<string, string, string>(
            sut: new ParsedExpressionAddStringOperator(),
            expectedNodeType: ExpressionType.Parameter,
            leftValue: string.Empty,
            (_, right, result) => result.TestRefEquals( right ) );
    }

    [Fact]
    public void AddOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstantAndNotEmpty()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant<string, string, string>(
            sut: new ParsedExpressionAddStringOperator(),
            expectedNodeType: ExpressionType.Call,
            rightValue: "foo",
            (left, right, result) => result.TestType()
                .AssignableTo<MethodCallExpression>(
                    methodCallResult => Assertion.All(
                        "methodCallResult",
                        methodCallResult.Object.TestNull(),
                        methodCallResult.Arguments.Count.TestEquals( 2 ),
                        methodCallResult.Method.Name.TestEquals( nameof( string.Concat ) ),
                        methodCallResult.Arguments.ElementAtOrDefault( 0 ).TestRefEquals( left ),
                        methodCallResult.Arguments.ElementAtOrDefault( 1 ).TestRefEquals( right ) ) ) );
    }

    [Fact]
    public void AddOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstantAndEmpty()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant<string, string, string>(
            sut: new ParsedExpressionAddStringOperator(),
            expectedNodeType: ExpressionType.Parameter,
            rightValue: string.Empty,
            (left, _, result) => result.TestRefEquals( left ) );
    }

    [Fact]
    public void EqualToOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable<string, string, bool>(
            sut: new ParsedExpressionEqualToStringOperator(),
            expectedNodeType: ExpressionType.Equal,
            DefaultNodeAssertion );
    }

    [Theory]
    [InlineData( "foo", "bar", false )]
    [InlineData( "foo", "foo", true )]
    public void EqualToOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant(
        string left,
        string right,
        bool expected)
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant<string, string, bool>(
            sut: new ParsedExpressionEqualToStringOperator(),
            expectedNodeType: ExpressionType.Constant,
            leftValue: left,
            rightValue: right,
            (_, _, result) =>
                result.TestType().AssignableTo<ConstantExpression>( constantResult => constantResult.Value.TestEquals( expected ) ) );
    }

    [Fact]
    public void EqualToOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant<string, string, bool>(
            sut: new ParsedExpressionEqualToStringOperator(),
            expectedNodeType: ExpressionType.Equal,
            leftValue: Fixture.Create<string>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void EqualToOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant<string, string, bool>(
            sut: new ParsedExpressionEqualToStringOperator(),
            expectedNodeType: ExpressionType.Equal,
            rightValue: Fixture.Create<string>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void NotEqualToOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable<string, string, bool>(
            sut: new ParsedExpressionNotEqualToStringOperator(),
            expectedNodeType: ExpressionType.NotEqual,
            DefaultNodeAssertion );
    }

    [Theory]
    [InlineData( "foo", "bar", true )]
    [InlineData( "foo", "foo", false )]
    public void NotEqualToOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant(
        string left,
        string right,
        bool expected)
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant<string, string, bool>(
            sut: new ParsedExpressionNotEqualToStringOperator(),
            expectedNodeType: ExpressionType.Constant,
            leftValue: left,
            rightValue: right,
            (_, _, result) =>
                result.TestType().AssignableTo<ConstantExpression>( constantResult => constantResult.Value.TestEquals( expected ) ) );
    }

    [Fact]
    public void NotEqualToOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant<string, string, bool>(
            sut: new ParsedExpressionNotEqualToStringOperator(),
            expectedNodeType: ExpressionType.NotEqual,
            leftValue: Fixture.Create<string>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void NotEqualToOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant<string, string, bool>(
            sut: new ParsedExpressionNotEqualToStringOperator(),
            expectedNodeType: ExpressionType.NotEqual,
            rightValue: Fixture.Create<string>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void CompareOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable<string, string, int>(
            sut: new ParsedExpressionCompareStringOperator(),
            expectedNodeType: ExpressionType.Call,
            (left, right, result) => result.TestType()
                .AssignableTo<MethodCallExpression>(
                    methodCallResult => Assertion.All(
                        "methodCallResult",
                        methodCallResult.Object.TestNull(),
                        methodCallResult.Arguments.Count.TestEquals( 3 ),
                        methodCallResult.Method.Name.TestEquals( nameof( string.Compare ) ),
                        methodCallResult.Arguments.ElementAtOrDefault( 0 ).TestRefEquals( left ),
                        methodCallResult.Arguments.ElementAtOrDefault( 1 ).TestRefEquals( right ),
                        methodCallResult.Arguments.ElementAtOrDefault( 2 )
                            .TestType()
                            .AssignableTo<ConstantExpression>(
                                constantStringComparisonArgument =>
                                    constantStringComparisonArgument.Value.TestEquals( StringComparison.Ordinal ) ) ) ) );
    }

    [Theory]
    [InlineData( "a", "z", -1 )]
    [InlineData( "a", "a", 0 )]
    [InlineData( "z", "a", 1 )]
    public void CompareOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant(
        string left,
        string right,
        int expected)
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant<string, string, int>(
            sut: new ParsedExpressionCompareStringOperator(),
            expectedNodeType: ExpressionType.Constant,
            leftValue: left,
            rightValue: right,
            (_, _, result) =>
                result.TestType()
                    .AssignableTo<ConstantExpression>(
                        constantResult => Math.Sign( ( int )constantResult.Value! ).TestEquals( expected ) ) );
    }

    [Fact]
    public void CompareOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant<string, string, int>(
            sut: new ParsedExpressionCompareStringOperator(),
            expectedNodeType: ExpressionType.Call,
            leftValue: Fixture.Create<string>(),
            (left, right, result) => result.TestType()
                .AssignableTo<MethodCallExpression>(
                    methodCallResult => Assertion.All(
                        "methodCallResult",
                        methodCallResult.Object.TestNull(),
                        methodCallResult.Arguments.Count.TestEquals( 3 ),
                        methodCallResult.Method.Name.TestEquals( nameof( string.Compare ) ),
                        methodCallResult.Arguments.ElementAtOrDefault( 0 ).TestRefEquals( left ),
                        methodCallResult.Arguments.ElementAtOrDefault( 1 ).TestRefEquals( right ),
                        methodCallResult.Arguments.ElementAtOrDefault( 2 )
                            .TestType()
                            .AssignableTo<ConstantExpression>(
                                constantStringComparisonArgument =>
                                    constantStringComparisonArgument.Value.TestEquals( StringComparison.Ordinal ) ) ) ) );
    }

    [Fact]
    public void CompareOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant<string, string, int>(
            sut: new ParsedExpressionCompareStringOperator(),
            expectedNodeType: ExpressionType.Call,
            rightValue: Fixture.Create<string>(),
            (left, right, result) => result.TestType()
                .AssignableTo<MethodCallExpression>(
                    methodCallResult => Assertion.All(
                        "methodCallResult",
                        methodCallResult.Object.TestNull(),
                        methodCallResult.Arguments.Count.TestEquals( 3 ),
                        methodCallResult.Method.Name.TestEquals( nameof( string.Compare ) ),
                        methodCallResult.Arguments.ElementAtOrDefault( 0 ).TestRefEquals( left ),
                        methodCallResult.Arguments.ElementAtOrDefault( 1 ).TestRefEquals( right ),
                        methodCallResult.Arguments.ElementAtOrDefault( 2 )
                            .TestType()
                            .AssignableTo<ConstantExpression>(
                                constantStringComparisonArgument =>
                                    constantStringComparisonArgument.Value.TestEquals( StringComparison.Ordinal ) ) ) ) );
    }
}
