using System.Linq;
using System.Linq.Expressions;
using FluentAssertions.Execution;
using LfrlAnvil.Computable.Expressions.Constructs.Variadic;
using LfrlAnvil.Functional;

namespace LfrlAnvil.Computable.Expressions.Tests.ConstructsTests.VariadicTests;

public class IfTests : TestsBase
{
    [Theory]
    [InlineData( 0 )]
    [InlineData( 1 )]
    [InlineData( 2 )]
    [InlineData( 4 )]
    [InlineData( 5 )]
    public void Process_ShouldThrowArgumentException_WhenParameterCountIsNotEqualToThree(int count)
    {
        var parameters = Enumerable.Range( 0, count ).Select( _ => Expression.Constant( true ) ).ToArray();
        var sut = new ParsedExpressionIf();

        var action = Lambda.Of( () => sut.Process( parameters ) );

        action.Should().ThrowExactly<ArgumentException>();
    }

    [Fact]
    public void Process_ShouldThrowArgumentException_WhenFirstParameterIsConstantAndNotOfBooleanType()
    {
        var parameters = new Expression[]
        {
            Expression.Constant( "foo" ),
            Expression.Constant( 1 ),
            Expression.Constant( 2 )
        };

        var sut = new ParsedExpressionIf();

        var action = Lambda.Of( () => sut.Process( parameters ) );

        action.Should().ThrowExactly<ArgumentException>();
    }

    [Fact]
    public void Process_ShouldThrowArgumentException_WhenFirstParameterIsNotConstantAndNotOfBooleanType()
    {
        var parameters = new Expression[]
        {
            Expression.Parameter( typeof( string ) ),
            Expression.Constant( 1 ),
            Expression.Constant( 2 )
        };

        var sut = new ParsedExpressionIf();

        var action = Lambda.Of( () => sut.Process( parameters ) );

        action.Should().ThrowExactly<ArgumentException>();
    }

    [Fact]
    public void Process_ShouldReturnSecondExpression_WhenFirstParameterIsConstantAndEqualToTrue()
    {
        var parameters = new Expression[]
        {
            Expression.Constant( true ),
            Expression.Constant( 1 ),
            Expression.Constant( "foo" )
        };

        var sut = new ParsedExpressionIf();

        var result = sut.Process( parameters );

        result.Should().BeSameAs( parameters[1] );
    }

    [Fact]
    public void Process_ShouldReturnThirdExpression_WhenFirstParameterIsConstantAndEqualToFalse()
    {
        var parameters = new Expression[]
        {
            Expression.Constant( false ),
            Expression.Constant( 1 ),
            Expression.Constant( "foo" )
        };

        var sut = new ParsedExpressionIf();

        var result = sut.Process( parameters );

        result.Should().BeSameAs( parameters[2] );
    }

    [Fact]
    public void Process_ShouldReturnConditionalExpression_WhenFirstParameterIsNotConstant()
    {
        var parameters = new Expression[]
        {
            Expression.Parameter( typeof( bool ) ),
            Expression.Constant( 1 ),
            Expression.Constant( 2 )
        };

        var sut = new ParsedExpressionIf();

        var result = sut.Process( parameters );

        using ( new AssertionScope() )
        {
            result.NodeType.Should().Be( ExpressionType.Conditional );
            if ( result is not ConditionalExpression conditional )
                return;

            conditional.Test.Should().BeSameAs( parameters[0] );
            conditional.IfTrue.Should().BeSameAs( parameters[1] );
            conditional.IfFalse.Should().BeSameAs( parameters[2] );
        }
    }

    [Fact]
    public void Process_ShouldThrowArgumentException_WhenFirstParameterIsNotConstantAndSecondAndThirdParametersHaveDifferentType()
    {
        var parameters = new Expression[]
        {
            Expression.Parameter( typeof( bool ) ),
            Expression.Constant( 1 ),
            Expression.Constant( "foo" )
        };

        var sut = new ParsedExpressionIf();

        var action = Lambda.Of( () => sut.Process( parameters ) );

        action.Should().ThrowExactly<ArgumentException>();
    }
}
