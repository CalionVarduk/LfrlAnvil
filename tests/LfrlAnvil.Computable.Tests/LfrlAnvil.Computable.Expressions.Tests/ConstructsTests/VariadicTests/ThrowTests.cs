using System.Linq.Expressions;
using LfrlAnvil.Computable.Expressions.Constructs.Variadic;
using LfrlAnvil.Computable.Expressions.Exceptions;
using LfrlAnvil.Functional;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.Computable.Expressions.Tests.ConstructsTests.VariadicTests;

public class ThrowTests : TestsBase
{
    [Fact]
    public void Process_ShouldReturnThrowExpression_WhenParametersAreEmpty()
    {
        var sut = new ParsedExpressionThrow();
        var result = sut.Process( Array.Empty<Expression>() );

        using ( new AssertionScope() )
        {
            result.NodeType.Should().Be( ExpressionType.Throw );
            if ( result is not UnaryExpression @throw )
                return;

            @throw.Type.Should().Be( typeof( void ) );
            @throw.Operand.NodeType.Should().Be( ExpressionType.New );
            if ( @throw.Operand is not NewExpression exception )
                return;

            exception.Type.Should().Be( typeof( ParsedExpressionInvocationException ) );
            exception.Arguments.Should().HaveCount( 2 );
            if ( exception.Arguments.Count != 2 )
                return;

            var firstArg = exception.Arguments[0];
            var secondArg = exception.Arguments[1];

            firstArg.NodeType.Should().Be( ExpressionType.Constant );
            secondArg.NodeType.Should().Be( ExpressionType.Constant );
            if ( firstArg is not ConstantExpression firstConstant || secondArg is not ConstantExpression secondConstant )
                return;

            firstConstant.Value.Should().Be( Resources.InvocationHasThrownAnException );
            secondConstant.Value.Should().BeEquivalentTo( Array.Empty<object?>() );
        }
    }

    [Fact]
    public void Process_ShouldReturnThrowExpression_WhenParametersContainsSingleElementOfExceptionType()
    {
        var expression = Expression.Constant( new InvalidOperationException() );
        var sut = new ParsedExpressionThrow();

        var result = sut.Process( new[] { expression } );

        using ( new AssertionScope() )
        {
            result.NodeType.Should().Be( ExpressionType.Throw );
            if ( result is not UnaryExpression @throw )
                return;

            @throw.Type.Should().Be( typeof( void ) );
            @throw.Operand.Should().BeSameAs( expression );
        }
    }

    [Fact]
    public void Process_ShouldThrowArgumentException_WhenParametersContainsSingleElementNotOfStringOrExceptionType()
    {
        var parameters = new Expression[] { Expression.Constant( 0 ) };
        var sut = new ParsedExpressionThrow();

        var action = Lambda.Of( () => sut.Process( parameters ) );

        action.Should().ThrowExactly<ArgumentException>();
    }

    [Fact]
    public void Process_ShouldThrowArgumentException_WhenParametersContainsManyElementsAndFirstElementIsNotOfStringType()
    {
        var parameters = new Expression[] { Expression.Constant( 0 ), Expression.Constant( 1 ) };
        var sut = new ParsedExpressionThrow();

        var action = Lambda.Of( () => sut.Process( parameters ) );

        action.Should().ThrowExactly<ArgumentException>();
    }

    [Fact]
    public void Process_ShouldReturnThrowExpression_WhenParametersContainsSingleElementOfStringType()
    {
        var expression = Expression.Constant( "foo" );
        var sut = new ParsedExpressionThrow();

        var result = sut.Process( new[] { expression } );

        using ( new AssertionScope() )
        {
            result.NodeType.Should().Be( ExpressionType.Throw );
            if ( result is not UnaryExpression @throw )
                return;

            @throw.Type.Should().Be( typeof( void ) );
            @throw.Operand.NodeType.Should().Be( ExpressionType.New );
            if ( @throw.Operand is not NewExpression exception )
                return;

            exception.Type.Should().Be( typeof( ParsedExpressionInvocationException ) );
            exception.Arguments.Should().HaveCount( 2 );
            if ( exception.Arguments.Count != 2 )
                return;

            var firstArg = exception.Arguments[0];
            var secondArg = exception.Arguments[1];

            firstArg.Should().BeSameAs( expression );
            secondArg.NodeType.Should().Be( ExpressionType.Constant );
            if ( secondArg is not ConstantExpression secondConstant )
                return;

            secondConstant.Value.Should().BeEquivalentTo( Array.Empty<object?>() );
        }
    }

    [Fact]
    public void
        Process_ShouldReturnThrowExpression_WhenParametersContainsManyElementsAndFirstElementIsOfStringTypeAndOtherElementsAreConstant()
    {
        var parameters = new Expression[] { Expression.Constant( "foo" ), Expression.Constant( 0 ), Expression.Constant( 1m ) };
        var sut = new ParsedExpressionThrow();

        var result = sut.Process( parameters );

        using ( new AssertionScope() )
        {
            result.NodeType.Should().Be( ExpressionType.Throw );
            if ( result is not UnaryExpression @throw )
                return;

            @throw.Type.Should().Be( typeof( void ) );
            @throw.Operand.NodeType.Should().Be( ExpressionType.New );
            if ( @throw.Operand is not NewExpression exception )
                return;

            exception.Type.Should().Be( typeof( ParsedExpressionInvocationException ) );
            exception.Arguments.Should().HaveCount( 2 );
            if ( exception.Arguments.Count != 2 )
                return;

            var firstArg = exception.Arguments[0];
            var secondArg = exception.Arguments[1];

            firstArg.Should().BeSameAs( parameters[0] );
            secondArg.NodeType.Should().Be( ExpressionType.Constant );
            if ( secondArg is not ConstantExpression secondConstant )
                return;

            secondConstant.Value.Should().BeEquivalentTo( new object?[] { 0, 1m } );
        }
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

        using ( new AssertionScope() )
        {
            result.NodeType.Should().Be( ExpressionType.Throw );
            if ( result is not UnaryExpression @throw )
                return;

            @throw.Type.Should().Be( typeof( void ) );
            @throw.Operand.NodeType.Should().Be( ExpressionType.New );
            if ( @throw.Operand is not NewExpression exception )
                return;

            exception.Type.Should().Be( typeof( ParsedExpressionInvocationException ) );
            exception.Arguments.Should().HaveCount( 2 );
            if ( exception.Arguments.Count != 2 )
                return;

            var firstArg = exception.Arguments[0];
            var secondArg = exception.Arguments[1];

            firstArg.Should().BeSameAs( parameters[0] );
            secondArg.NodeType.Should().Be( ExpressionType.NewArrayInit );
            if ( secondArg is not NewArrayExpression argsExpression )
                return;

            argsExpression.Type.Should().Be( typeof( object?[] ) );
            argsExpression.Expressions.Should().HaveCount( 3 );
            if ( argsExpression.Expressions.Count != 3 )
                return;

            var firstFormatArg = argsExpression.Expressions[0];
            var secondFormatArg = argsExpression.Expressions[1];
            var thirdFormatArg = argsExpression.Expressions[2];

            firstFormatArg.NodeType.Should().Be( ExpressionType.Convert );
            secondFormatArg.Should().BeSameAs( parameters[2] );
            thirdFormatArg.NodeType.Should().Be( ExpressionType.Convert );

            if ( firstFormatArg is not UnaryExpression firstConvertArg || thirdFormatArg is not UnaryExpression thirdConvertArg )
                return;

            firstConvertArg.Operand.Should().BeSameAs( parameters[1] );
            thirdConvertArg.Operand.Should().BeSameAs( parameters[3] );
        }
    }

    [Fact]
    public void Exception_ShouldHaveCorrectProperties()
    {
        var sut = new ParsedExpressionInvocationException( "foo {0} bar {1} qux", 1, 2 );

        using ( new AssertionScope() )
        {
            sut.Format.Should().Be( "foo {0} bar {1} qux" );
            sut.Args.Should().BeSequentiallyEqualTo( 1, 2 );
            sut.Message.Should().Be( "foo 1 bar 2 qux" );
        }
    }
}
