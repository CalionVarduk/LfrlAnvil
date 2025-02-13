using System.Linq.Expressions;
using System.Linq;
using LfrlAnvil.Computable.Expressions.Constructs.Variadic;
using LfrlAnvil.Computable.Expressions.Exceptions;
using LfrlAnvil.Functional;

namespace LfrlAnvil.Computable.Expressions.Tests.ConstructsTests.VariadicTests;

public class ThrowTests : TestsBase
{
    [Fact]
    public void Process_ShouldReturnThrowExpression_WhenParametersAreEmpty()
    {
        var sut = new ParsedExpressionThrow();
        var result = sut.Process( Array.Empty<Expression>() );

        Assertion.All(
                result.NodeType.TestEquals( ExpressionType.Throw ),
                result.TestType()
                    .AssignableTo<UnaryExpression>(
                        @throw => Assertion.All(
                            "@throw",
                            @throw.Type.TestEquals( typeof( void ) ),
                            @throw.Operand.NodeType.TestEquals( ExpressionType.New ),
                            @throw.Operand.TestType()
                                .AssignableTo<NewExpression>(
                                    exception => Assertion.All(
                                        "exception",
                                        exception.Type.TestEquals( typeof( ParsedExpressionInvocationException ) ),
                                        exception.Arguments.Count.TestEquals( 2 ),
                                        (exception.Arguments.FirstOrDefault()?.NodeType).TestEquals( ExpressionType.Constant ),
                                        (exception.Arguments.ElementAtOrDefault( 1 )?.NodeType).TestEquals( ExpressionType.Constant ),
                                        exception.Arguments.FirstOrDefault()
                                            .TestType()
                                            .AssignableTo<ConstantExpression>(
                                                firstConstant =>
                                                    firstConstant.Value.TestEquals( Resources.InvocationHasThrownAnException ) ),
                                        exception.Arguments.ElementAtOrDefault( 1 )
                                            .TestType()
                                            .AssignableTo<ConstantExpression>(
                                                secondConstant => secondConstant.Value.TestEquals( Array.Empty<object?>() ) ) ) ) ) ) )
            .Go();
    }

    [Fact]
    public void Process_ShouldReturnThrowExpression_WhenParametersContainsSingleElementOfExceptionType()
    {
        var expression = Expression.Constant( new InvalidOperationException() );
        var sut = new ParsedExpressionThrow();

        var result = sut.Process( new[] { expression } );

        Assertion.All(
                result.NodeType.TestEquals( ExpressionType.Throw ),
                result.TestType()
                    .AssignableTo<UnaryExpression>(
                        @throw => Assertion.All(
                            "@throw",
                            @throw.Type.TestEquals( typeof( void ) ),
                            @throw.Operand.TestRefEquals( expression ) ) ) )
            .Go();
    }

    [Fact]
    public void Process_ShouldThrowArgumentException_WhenParametersContainsSingleElementNotOfStringOrExceptionType()
    {
        var parameters = new Expression[] { Expression.Constant( 0 ) };
        var sut = new ParsedExpressionThrow();

        var action = Lambda.Of( () => sut.Process( parameters ) );

        action.Test( exc => exc.TestType().Exact<ArgumentException>() ).Go();
    }

    [Fact]
    public void Process_ShouldThrowArgumentException_WhenParametersContainsManyElementsAndFirstElementIsNotOfStringType()
    {
        var parameters = new Expression[] { Expression.Constant( 0 ), Expression.Constant( 1 ) };
        var sut = new ParsedExpressionThrow();

        var action = Lambda.Of( () => sut.Process( parameters ) );

        action.Test( exc => exc.TestType().Exact<ArgumentException>() ).Go();
    }

    [Fact]
    public void Process_ShouldReturnThrowExpression_WhenParametersContainsSingleElementOfStringType()
    {
        var expression = Expression.Constant( "foo" );
        var sut = new ParsedExpressionThrow();

        var result = sut.Process( new[] { expression } );

        Assertion.All(
                result.NodeType.TestEquals( ExpressionType.Throw ),
                result.TestType()
                    .AssignableTo<UnaryExpression>(
                        @throw => Assertion.All(
                            "@throw",
                            @throw.Type.TestEquals( typeof( void ) ),
                            @throw.Operand.NodeType.TestEquals( ExpressionType.New ),
                            @throw.Operand.TestType()
                                .AssignableTo<NewExpression>(
                                    exception => Assertion.All(
                                        "exception",
                                        exception.Type.TestEquals( typeof( ParsedExpressionInvocationException ) ),
                                        exception.Arguments.Count.TestEquals( 2 ),
                                        exception.Arguments.FirstOrDefault().TestRefEquals( expression ),
                                        (exception.Arguments.ElementAtOrDefault( 1 )?.NodeType).TestEquals( ExpressionType.Constant ),
                                        exception.Arguments.ElementAtOrDefault( 1 )
                                            .TestType()
                                            .AssignableTo<ConstantExpression>(
                                                secondConstant => secondConstant.Value.TestEquals( Array.Empty<object?>() ) ) ) ) ) ) )
            .Go();
    }

    [Fact]
    public void
        Process_ShouldReturnThrowExpression_WhenParametersContainsManyElementsAndFirstElementIsOfStringTypeAndOtherElementsAreConstant()
    {
        var parameters = new Expression[] { Expression.Constant( "foo" ), Expression.Constant( 0 ), Expression.Constant( 1m ) };
        var sut = new ParsedExpressionThrow();

        var result = sut.Process( parameters );

        Assertion.All(
                result.NodeType.TestEquals( ExpressionType.Throw ),
                result.TestType()
                    .AssignableTo<UnaryExpression>(
                        @throw => Assertion.All(
                            "@throw",
                            @throw.Type.TestEquals( typeof( void ) ),
                            @throw.Operand.NodeType.TestEquals( ExpressionType.New ),
                            @throw.Operand.TestType()
                                .AssignableTo<NewExpression>(
                                    exception => Assertion.All(
                                        "exception",
                                        exception.Type.TestEquals( typeof( ParsedExpressionInvocationException ) ),
                                        exception.Arguments.Count.TestEquals( 2 ),
                                        exception.Arguments.FirstOrDefault().TestRefEquals( parameters[0] ),
                                        (exception.Arguments.ElementAtOrDefault( 1 )?.NodeType).TestEquals( ExpressionType.Constant ),
                                        exception.Arguments.ElementAtOrDefault( 1 )
                                            .TestType()
                                            .AssignableTo<ConstantExpression>(
                                                secondConstant =>
                                                    secondConstant.Value.TestType()
                                                        .Exact<object[]>( value => value.TestSequence( [ 0, 1m ] ) ) ) ) ) ) ) )
            .Go();
    }

    [Fact]
    public void
        Process_ShouldReturnThrowExpression_WhenParametersContainsManyElementsAndFirstElementIsOfStringTypeAndAtLeastOneOfOtherElementsIsNotConstant()
    {
        var parameters = new Expression[]
        {
            Expression.Constant( "foo" ),
            Expression.Constant( 0 ),
            Expression.Constant( 1, typeof( object ) ),
            Expression.Parameter( typeof( decimal ) )
        };

        var sut = new ParsedExpressionThrow();

        var result = sut.Process( parameters );

        Assertion.All(
                result.NodeType.TestEquals( ExpressionType.Throw ),
                result.TestType()
                    .AssignableTo<UnaryExpression>(
                        @throw => Assertion.All(
                            "@throw",
                            @throw.Type.TestEquals( typeof( void ) ),
                            @throw.Operand.NodeType.TestEquals( ExpressionType.New ),
                            @throw.Operand.TestType()
                                .AssignableTo<NewExpression>(
                                    exception => Assertion.All(
                                        "exception",
                                        exception.Type.TestEquals( typeof( ParsedExpressionInvocationException ) ),
                                        exception.Arguments.Count.TestEquals( 2 ),
                                        exception.Arguments.FirstOrDefault().TestRefEquals( parameters[0] ),
                                        (exception.Arguments.ElementAtOrDefault( 1 )?.NodeType).TestEquals( ExpressionType.NewArrayInit ),
                                        exception.Arguments.ElementAtOrDefault( 1 )
                                            .TestType()
                                            .AssignableTo<NewArrayExpression>(
                                                argsExpression => Assertion.All(
                                                    "argsExpression",
                                                    argsExpression.Type.TestEquals( typeof( object?[] ) ),
                                                    argsExpression.Expressions.Count.TestEquals( 3 ),
                                                    (argsExpression.Expressions.FirstOrDefault()?.NodeType).TestEquals(
                                                        ExpressionType.Convert ),
                                                    argsExpression.Expressions.ElementAtOrDefault( 1 ).TestRefEquals( parameters[2] ),
                                                    (argsExpression.Expressions.ElementAtOrDefault( 2 )?.NodeType).TestEquals(
                                                        ExpressionType.Convert ),
                                                    argsExpression.Expressions.FirstOrDefault()
                                                        .TestType()
                                                        .AssignableTo<UnaryExpression>(
                                                            firstConvertArg => firstConvertArg.Operand.TestRefEquals( parameters[1] ) ),
                                                    argsExpression.Expressions.ElementAtOrDefault( 2 )
                                                        .TestType()
                                                        .AssignableTo<UnaryExpression>(
                                                            thirdFormatArg =>
                                                                thirdFormatArg.Operand.TestRefEquals( parameters[3] ) ) ) ) ) ) ) ) )
            .Go();
    }

    [Fact]
    public void Exception_ShouldHaveCorrectProperties()
    {
        var sut = new ParsedExpressionInvocationException( "foo {0} bar {1} qux", 1, 2 );

        Assertion.All(
                sut.Format.TestEquals( "foo {0} bar {1} qux" ),
                sut.Args.TestSequence( [ 1, 2 ] ),
                sut.Message.TestEquals( "foo 1 bar 2 qux" ) )
            .Go();
    }
}
