using System.Linq.Expressions;
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
                    .AssignableTo<UnaryExpression>( @throw => Assertion.All(
                        "@throw",
                        @throw.Type.TestEquals( typeof( void ) ),
                        @throw.Operand.NodeType.TestEquals( ExpressionType.New ),
                        @throw.Operand.TestType()
                            .AssignableTo<NewExpression>( exception => Assertion.All(
                                "exception",
                                exception.Type.TestEquals( typeof( ParsedExpressionInvocationException ) ),
                                exception.Arguments.TestCount( count => count.TestEquals( 2 ) )
                                    .Then( args =>
                                    {
                                        var first = args[0];
                                        var second = args[1];
                                        return Assertion.All(
                                            "args",
                                            first.NodeType.TestEquals( ExpressionType.Constant ),
                                            first.TestType()
                                                .AssignableTo<ConstantExpression>( firstConstant =>
                                                    firstConstant.Value.TestEquals( Resources.InvocationHasThrownAnException ) ),
                                            second.NodeType.TestEquals( ExpressionType.Constant ),
                                            second.TestType()
                                                .AssignableTo<ConstantExpression>( secondConstant =>
                                                    secondConstant.Value.TestEquals( Array.Empty<object?>() ) ) );
                                    } ) ) ) ) ) )
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
                    .AssignableTo<UnaryExpression>( @throw => Assertion.All(
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
                    .AssignableTo<UnaryExpression>( @throw => Assertion.All(
                        "@throw",
                        @throw.Type.TestEquals( typeof( void ) ),
                        @throw.Operand.NodeType.TestEquals( ExpressionType.New ),
                        @throw.Operand.TestType()
                            .AssignableTo<NewExpression>( exception => Assertion.All(
                                "exception",
                                exception.Type.TestEquals( typeof( ParsedExpressionInvocationException ) ),
                                exception.Arguments.TestCount( count => count.TestEquals( 2 ) )
                                    .Then( args =>
                                    {
                                        var first = args[0];
                                        var second = args[1];
                                        return Assertion.All(
                                            "args",
                                            first.TestRefEquals( expression ),
                                            second.NodeType.TestEquals( ExpressionType.Constant ),
                                            second.TestType()
                                                .AssignableTo<ConstantExpression>( secondConstant =>
                                                    secondConstant.Value.TestEquals( Array.Empty<object?>() ) ) );
                                    } ) ) ) ) ) )
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
                    .AssignableTo<UnaryExpression>( @throw => Assertion.All(
                        "@throw",
                        @throw.Type.TestEquals( typeof( void ) ),
                        @throw.Operand.NodeType.TestEquals( ExpressionType.New ),
                        @throw.Operand.TestType()
                            .AssignableTo<NewExpression>( exception => Assertion.All(
                                "exception",
                                exception.Type.TestEquals( typeof( ParsedExpressionInvocationException ) ),
                                exception.Arguments.TestCount( count => count.TestEquals( 2 ) )
                                    .Then( args =>
                                    {
                                        var first = args[0];
                                        var second = args[1];
                                        return Assertion.All(
                                            "args",
                                            first.TestRefEquals( parameters[0] ),
                                            second.NodeType.TestEquals( ExpressionType.Constant ),
                                            second.TestType()
                                                .AssignableTo<ConstantExpression>( secondConstant =>
                                                    secondConstant.Value.TestType()
                                                        .Exact<object[]>( value => value.TestSequence( [ 0, 1m ] ) ) ) );
                                    } ) ) ) ) ) )
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
                    .AssignableTo<UnaryExpression>( @throw => Assertion.All(
                        "@throw",
                        @throw.Type.TestEquals( typeof( void ) ),
                        @throw.Operand.NodeType.TestEquals( ExpressionType.New ),
                        @throw.Operand.TestType()
                            .AssignableTo<NewExpression>( exception => Assertion.All(
                                "exception",
                                exception.Type.TestEquals( typeof( ParsedExpressionInvocationException ) ),
                                exception.Arguments.TestCount( count => count.TestEquals( 2 ) )
                                    .Then( args =>
                                    {
                                        var first = args[0];
                                        var second = args[1];
                                        return Assertion.All(
                                            "args",
                                            first.TestRefEquals( parameters[0] ),
                                            second.NodeType.TestEquals( ExpressionType.NewArrayInit ),
                                            second.TestType()
                                                .AssignableTo<NewArrayExpression>( argsExpression => Assertion.All(
                                                    "argsExpression",
                                                    argsExpression.Type.TestEquals( typeof( object?[] ) ),
                                                    argsExpression.Expressions.TestCount( count => count.TestEquals( 3 ) )
                                                        .Then( secondArgs =>
                                                        {
                                                            var nFirst = secondArgs[0];
                                                            var nSecond = secondArgs[1];
                                                            var nThird = secondArgs[2];
                                                            return Assertion.All(
                                                                "secondArgs",
                                                                nFirst.NodeType.TestEquals( ExpressionType.Convert ),
                                                                nSecond.TestRefEquals( parameters[2] ),
                                                                nThird.NodeType.TestEquals( ExpressionType.Convert ),
                                                                nFirst.TestType()
                                                                    .AssignableTo<UnaryExpression>( firstConvertArg =>
                                                                        firstConvertArg.Operand.TestRefEquals( parameters[1] ) ),
                                                                nThird.TestType()
                                                                    .AssignableTo<UnaryExpression>( thirdFormatArg =>
                                                                        thirdFormatArg.Operand.TestRefEquals( parameters[3] ) ) );
                                                        } ) ) ) );
                                    } ) ) ) ) ) )
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
