using System;
using System.Linq;
using System.Linq.Expressions;
using AutoFixture;
using FluentAssertions;
using LfrlAnvil.Computable.Expressions.Constructs.String;
using Xunit;

namespace LfrlAnvil.Computable.Expressions.Tests.ConstructsTests.StringTests;

public class StringBinaryOperatorTests : BinaryOperatorsTestsBase
{
    [Fact]
    public void AddOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable<string, string, string>(
            sut: new MathExpressionAddStringOperator(),
            expectedNodeType: ExpressionType.Call,
            (left, right, result) =>
            {
                result.Should().BeAssignableTo<MethodCallExpression>();
                if ( result is not MethodCallExpression methodCallResult )
                    return;

                methodCallResult.Object.Should().BeNull();
                methodCallResult.Arguments.Should().HaveCount( 2 );
                methodCallResult.Method.Name.Should().Be( nameof( string.Concat ) );

                if ( methodCallResult.Arguments.Count != 2 )
                    return;

                methodCallResult.Arguments.ElementAt( 0 ).Should().BeSameAs( left );
                methodCallResult.Arguments.ElementAt( 1 ).Should().BeSameAs( right );
            } );
    }

    [Fact]
    public void AddOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant<string, string, string>(
            sut: new MathExpressionAddStringOperator(),
            expectedNodeType: ExpressionType.Constant,
            leftValue: "foo",
            rightValue: "bar",
            (_, _, result) =>
            {
                result.Should().BeAssignableTo<ConstantExpression>();
                if ( result is not ConstantExpression constantResult )
                    return;

                constantResult.Value.Should().Be( "foobar" );
            } );
    }

    [Fact]
    public void AddOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstantAndNotEmpty()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant<string, string, string>(
            sut: new MathExpressionAddStringOperator(),
            expectedNodeType: ExpressionType.Call,
            leftValue: "foo",
            (left, right, result) =>
            {
                result.Should().BeAssignableTo<MethodCallExpression>();
                if ( result is not MethodCallExpression methodCallResult )
                    return;

                methodCallResult.Object.Should().BeNull();
                methodCallResult.Arguments.Should().HaveCount( 2 );
                methodCallResult.Method.Name.Should().Be( nameof( string.Concat ) );

                if ( methodCallResult.Arguments.Count != 2 )
                    return;

                methodCallResult.Arguments.ElementAt( 0 ).Should().BeSameAs( left );
                methodCallResult.Arguments.ElementAt( 1 ).Should().BeSameAs( right );
            } );
    }

    [Fact]
    public void AddOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstantAndEmpty()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant<string, string, string>(
            sut: new MathExpressionAddStringOperator(),
            expectedNodeType: ExpressionType.Parameter,
            leftValue: string.Empty,
            (_, right, result) => result.Should().BeSameAs( right ) );
    }

    [Fact]
    public void AddOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstantAndNotEmpty()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant<string, string, string>(
            sut: new MathExpressionAddStringOperator(),
            expectedNodeType: ExpressionType.Call,
            rightValue: "foo",
            (left, right, result) =>
            {
                result.Should().BeAssignableTo<MethodCallExpression>();
                if ( result is not MethodCallExpression methodCallResult )
                    return;

                methodCallResult.Object.Should().BeNull();
                methodCallResult.Arguments.Should().HaveCount( 2 );
                methodCallResult.Method.Name.Should().Be( nameof( string.Concat ) );

                if ( methodCallResult.Arguments.Count != 2 )
                    return;

                methodCallResult.Arguments.ElementAt( 0 ).Should().BeSameAs( left );
                methodCallResult.Arguments.ElementAt( 1 ).Should().BeSameAs( right );
            } );
    }

    [Fact]
    public void AddOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstantAndEmpty()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant<string, string, string>(
            sut: new MathExpressionAddStringOperator(),
            expectedNodeType: ExpressionType.Parameter,
            rightValue: string.Empty,
            (left, _, result) => result.Should().BeSameAs( left ) );
    }

    [Fact]
    public void EqualToOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable<string, string, bool>(
            sut: new MathExpressionEqualToStringOperator(),
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
            sut: new MathExpressionEqualToStringOperator(),
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
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant<string, string, bool>(
            sut: new MathExpressionEqualToStringOperator(),
            expectedNodeType: ExpressionType.Equal,
            leftValue: Fixture.Create<string>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void EqualToOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant<string, string, bool>(
            sut: new MathExpressionEqualToStringOperator(),
            expectedNodeType: ExpressionType.Equal,
            rightValue: Fixture.Create<string>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void NotEqualToOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable<string, string, bool>(
            sut: new MathExpressionNotEqualToStringOperator(),
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
            sut: new MathExpressionNotEqualToStringOperator(),
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
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant<string, string, bool>(
            sut: new MathExpressionNotEqualToStringOperator(),
            expectedNodeType: ExpressionType.NotEqual,
            leftValue: Fixture.Create<string>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void NotEqualToOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant<string, string, bool>(
            sut: new MathExpressionNotEqualToStringOperator(),
            expectedNodeType: ExpressionType.NotEqual,
            rightValue: Fixture.Create<string>(),
            DefaultNodeAssertion );
    }

    [Fact]
    public void CompareOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable<string, string, int>(
            sut: new MathExpressionCompareStringOperator(),
            expectedNodeType: ExpressionType.Call,
            (left, right, result) =>
            {
                result.Should().BeAssignableTo<MethodCallExpression>();
                if ( result is not MethodCallExpression methodCallResult )
                    return;

                methodCallResult.Object.Should().BeNull();
                methodCallResult.Arguments.Should().HaveCount( 3 );
                methodCallResult.Method.Name.Should().Be( nameof( string.Compare ) );

                if ( methodCallResult.Arguments.Count != 3 )
                    return;

                methodCallResult.Arguments.ElementAt( 0 ).Should().BeSameAs( left );
                methodCallResult.Arguments.ElementAt( 1 ).Should().BeSameAs( right );
                var stringComparisonArgument = methodCallResult.Arguments.ElementAt( 2 );
                stringComparisonArgument.Should().BeAssignableTo<ConstantExpression>();
                if ( stringComparisonArgument is not ConstantExpression constantStringComparisonArgument )
                    return;

                constantStringComparisonArgument.Value.Should().Be( StringComparison.Ordinal );
            } );
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
            sut: new MathExpressionCompareStringOperator(),
            expectedNodeType: ExpressionType.Constant,
            leftValue: left,
            rightValue: right,
            (_, _, result) =>
            {
                result.Should().BeAssignableTo<ConstantExpression>();
                if ( result is not ConstantExpression constantResult )
                    return;

                Math.Sign( (int)constantResult.Value! ).Should().Be( expected );
            } );
    }

    [Fact]
    public void CompareOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant<string, string, int>(
            sut: new MathExpressionCompareStringOperator(),
            expectedNodeType: ExpressionType.Call,
            leftValue: Fixture.Create<string>(),
            (left, right, result) =>
            {
                result.Should().BeAssignableTo<MethodCallExpression>();
                if ( result is not MethodCallExpression methodCallResult )
                    return;

                methodCallResult.Object.Should().BeNull();
                methodCallResult.Arguments.Should().HaveCount( 3 );
                methodCallResult.Method.Name.Should().Be( nameof( string.Compare ) );

                if ( methodCallResult.Arguments.Count != 3 )
                    return;

                methodCallResult.Arguments.ElementAt( 0 ).Should().BeSameAs( left );
                methodCallResult.Arguments.ElementAt( 1 ).Should().BeSameAs( right );
                var stringComparisonArgument = methodCallResult.Arguments.ElementAt( 2 );
                stringComparisonArgument.Should().BeAssignableTo<ConstantExpression>();
                if ( stringComparisonArgument is not ConstantExpression constantStringComparisonArgument )
                    return;

                constantStringComparisonArgument.Value.Should().Be( StringComparison.Ordinal );
            } );
    }

    [Fact]
    public void CompareOperatorProcess_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant()
    {
        Process_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant<string, string, int>(
            sut: new MathExpressionCompareStringOperator(),
            expectedNodeType: ExpressionType.Call,
            rightValue: Fixture.Create<string>(),
            (left, right, result) =>
            {
                result.Should().BeAssignableTo<MethodCallExpression>();
                if ( result is not MethodCallExpression methodCallResult )
                    return;

                methodCallResult.Object.Should().BeNull();
                methodCallResult.Arguments.Should().HaveCount( 3 );
                methodCallResult.Method.Name.Should().Be( nameof( string.Compare ) );

                if ( methodCallResult.Arguments.Count != 3 )
                    return;

                methodCallResult.Arguments.ElementAt( 0 ).Should().BeSameAs( left );
                methodCallResult.Arguments.ElementAt( 1 ).Should().BeSameAs( right );
                var stringComparisonArgument = methodCallResult.Arguments.ElementAt( 2 );
                stringComparisonArgument.Should().BeAssignableTo<ConstantExpression>();
                if ( stringComparisonArgument is not ConstantExpression constantStringComparisonArgument )
                    return;

                constantStringComparisonArgument.Value.Should().Be( StringComparison.Ordinal );
            } );
    }
}
