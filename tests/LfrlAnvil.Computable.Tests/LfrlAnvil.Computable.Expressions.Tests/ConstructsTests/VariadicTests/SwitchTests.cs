using System.Linq;
using System.Linq.Expressions;
using FluentAssertions.Execution;
using LfrlAnvil.Computable.Expressions.Constructs.Variadic;
using LfrlAnvil.Functional;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.Computable.Expressions.Tests.ConstructsTests.VariadicTests;

public class SwitchTests : TestsBase
{
    [Theory]
    [InlineData( 0 )]
    [InlineData( 1 )]
    public void Process_ShouldThrowArgumentException_WhenParameterCountIsLessThanTwo(int count)
    {
        var parameters = Enumerable.Range( 0, count ).Select( _ => Expression.Constant( true ) ).ToArray();
        var sut = new ParsedExpressionSwitch();

        var action = Lambda.Of( () => sut.Process( parameters ) );

        action.Should().ThrowExactly<ArgumentException>();
    }

    [Fact]
    public void Process_ShouldThrowArgumentException_WhenAnyParameterExpectedToBeSwitchCaseIsSomethingElse()
    {
        var parameters = new Expression[]
        {
            Expression.Parameter( typeof( int ) ),
            Expression.Constant( Expression.SwitchCase( Expression.Constant( "foo" ), Expression.Constant( 0 ) ) ),
            Expression.Constant( Expression.SwitchCase( Expression.Constant( "bar" ), Expression.Constant( 1 ) ) ),
            Expression.Constant( "qux" ),
            Expression.Constant( "foobar" )
        };

        var sut = new ParsedExpressionSwitch();

        var action = Lambda.Of( () => sut.Process( parameters ) );

        action.Should().ThrowExactly<ArgumentException>();
    }

    [Fact]
    public void Process_ShouldThrowArgumentException_WhenValueIsNotConstantAndAnyCaseValueHasDifferentType()
    {
        var parameters = new Expression[]
        {
            Expression.Parameter( typeof( int ) ),
            Expression.Constant( Expression.SwitchCase( Expression.Constant( "foo" ), Expression.Constant( 0 ) ) ),
            Expression.Constant( Expression.SwitchCase( Expression.Constant( "bar" ), Expression.Constant( 1 ) ) ),
            Expression.Constant( Expression.SwitchCase( Expression.Constant( "qux" ), Expression.Constant( 2m ) ) ),
            Expression.Constant( "foobar" )
        };

        var sut = new ParsedExpressionSwitch();

        var action = Lambda.Of( () => sut.Process( parameters ) );

        action.Should().ThrowExactly<ArgumentException>();
    }

    [Fact]
    public void Process_ShouldThrowArgumentException_WhenValueIsNotConstantAndNotAllCaseBodiesHaveTheSameType()
    {
        var parameters = new Expression[]
        {
            Expression.Parameter( typeof( int ) ),
            Expression.Constant( Expression.SwitchCase( Expression.Constant( "foo" ), Expression.Constant( 0 ) ) ),
            Expression.Constant( Expression.SwitchCase( Expression.Constant( "bar" ), Expression.Constant( 1 ) ) ),
            Expression.Constant( Expression.SwitchCase( Expression.Constant( 0 ), Expression.Constant( 2 ) ) ),
            Expression.Constant( "foobar" )
        };

        var sut = new ParsedExpressionSwitch();

        var action = Lambda.Of( () => sut.Process( parameters ) );

        action.Should().ThrowExactly<ArgumentException>();
    }

    [Fact]
    public void Process_ShouldReturnSwitchExpression_WhenValueIsNotConstant()
    {
        var parameters = new Expression[]
        {
            Expression.Parameter( typeof( int ) ),
            Expression.Constant( Expression.SwitchCase( Expression.Constant( "foo" ), Expression.Constant( 0 ) ) ),
            Expression.Constant( Expression.SwitchCase( Expression.Constant( "bar" ), Expression.Constant( 1 ) ) ),
            Expression.Constant( Expression.SwitchCase( Expression.Constant( "qux" ), Expression.Constant( 2 ) ) ),
            Expression.Constant( "foobar" )
        };

        var sut = new ParsedExpressionSwitch();

        var result = sut.Process( parameters );

        using ( new AssertionScope() )
        {
            result.NodeType.Should().Be( ExpressionType.Switch );
            if ( result is not SwitchExpression @switch )
                return;

            @switch.SwitchValue.Should().BeSameAs( parameters[0] );
            @switch.DefaultBody.Should().BeSameAs( parameters[^1] );
            @switch.Cases.Should()
                .BeSequentiallyEqualTo(
                    (SwitchCase)((ConstantExpression)parameters[1]).Value!,
                    (SwitchCase)((ConstantExpression)parameters[2]).Value!,
                    (SwitchCase)((ConstantExpression)parameters[3]).Value! );
        }
    }

    [Fact]
    public void Process_ShouldReturnDefaultBody_WhenValueIsNotConstantAndThereAreNoSwitchCases()
    {
        var parameters = new Expression[]
        {
            Expression.Parameter( typeof( int ) ),
            Expression.Constant( "foobar" )
        };

        var sut = new ParsedExpressionSwitch();

        var result = sut.Process( parameters );

        result.Should().BeSameAs( parameters[^1] );
    }

    [Fact]
    public void Process_ShouldReturnDefaultBody_WhenValueIsConstantAndAllCaseValuesAreConstantAndNoCaseValueEqualsValue()
    {
        var parameters = new Expression[]
        {
            Expression.Constant( 3 ),
            Expression.Constant( Expression.SwitchCase( Expression.Constant( "foo" ), Expression.Constant( 0 ) ) ),
            Expression.Constant( Expression.SwitchCase( Expression.Constant( "bar" ), Expression.Constant( 1 ) ) ),
            Expression.Constant( Expression.SwitchCase( Expression.Constant( "qux" ), Expression.Constant( 2 ) ) ),
            Expression.Constant( "foobar" )
        };

        var sut = new ParsedExpressionSwitch();

        var result = sut.Process( parameters );

        result.Should().BeSameAs( parameters[^1] );
    }

    [Fact]
    public void Process_ShouldReturnCorrectCaseBody_WhenValueIsConstantAndEqualsToOneOfConstantCaseValues()
    {
        var parameters = new Expression[]
        {
            Expression.Constant( 2 ),
            Expression.Constant( Expression.SwitchCase( Expression.Constant( "foo" ), Expression.Constant( 0 ) ) ),
            Expression.Constant( Expression.SwitchCase( Expression.Constant( "bar" ), Expression.Constant( 1 ) ) ),
            Expression.Constant( Expression.SwitchCase( Expression.Constant( "qux" ), Expression.Constant( 2 ) ) ),
            Expression.Constant( "foobar" )
        };

        var sut = new ParsedExpressionSwitch();

        var result = sut.Process( parameters );

        result.Should().BeSameAs( ((SwitchCase)((ConstantExpression)parameters[3]).Value!).Body );
    }

    [Fact]
    public void Process_ShouldReturnCorrectCaseBody_WhenValueIsConstantNullAndEqualsToOneOfConstantCaseValues()
    {
        var parameters = new Expression[]
        {
            Expression.Constant( null, typeof( string ) ),
            Expression.Constant( Expression.SwitchCase( Expression.Constant( "foo" ), Expression.Constant( "a" ) ) ),
            Expression.Constant( Expression.SwitchCase( Expression.Constant( "bar" ), Expression.Constant( "b" ) ) ),
            Expression.Constant( Expression.SwitchCase( Expression.Constant( "qux" ), Expression.Constant( null, typeof( string ) ) ) ),
            Expression.Constant( "foobar" )
        };

        var sut = new ParsedExpressionSwitch();

        var result = sut.Process( parameters );

        result.Should().BeSameAs( ((SwitchCase)((ConstantExpression)parameters[3]).Value!).Body );
    }

    [Fact]
    public void Process_ShouldReturnSwitchExpression_WhenValueIsConstantAndAllCaseValuesAreVariable()
    {
        var parameters = new Expression[]
        {
            Expression.Constant( 0 ),
            Expression.Constant( Expression.SwitchCase( Expression.Constant( "foo" ), Expression.Parameter( typeof( int ) ) ) ),
            Expression.Constant( Expression.SwitchCase( Expression.Constant( "bar" ), Expression.Parameter( typeof( int ) ) ) ),
            Expression.Constant( Expression.SwitchCase( Expression.Constant( "qux" ), Expression.Parameter( typeof( int ) ) ) ),
            Expression.Constant( "foobar" )
        };

        var sut = new ParsedExpressionSwitch();

        var result = sut.Process( parameters );

        using ( new AssertionScope() )
        {
            result.NodeType.Should().Be( ExpressionType.Switch );
            if ( result is not SwitchExpression @switch )
                return;

            @switch.SwitchValue.Should().BeSameAs( parameters[0] );
            @switch.DefaultBody.Should().BeSameAs( parameters[^1] );
            @switch.Cases.Should()
                .BeSequentiallyEqualTo(
                    (SwitchCase)((ConstantExpression)parameters[1]).Value!,
                    (SwitchCase)((ConstantExpression)parameters[2]).Value!,
                    (SwitchCase)((ConstantExpression)parameters[3]).Value! );
        }
    }

    [Fact]
    public void Process_ShouldReturnSwitchExpression_WhenValueIsConstantAndIsNotEqualToAnyConstantCaseValueAndSomeCaseValuesAreVariable()
    {
        var parameters = new Expression[]
        {
            Expression.Constant( 2 ),
            Expression.Constant( Expression.SwitchCase( Expression.Constant( "foo" ), Expression.Constant( 0 ) ) ),
            Expression.Constant( Expression.SwitchCase( Expression.Constant( "bar" ), Expression.Constant( 1 ) ) ),
            Expression.Constant( Expression.SwitchCase( Expression.Constant( "qux" ), Expression.Parameter( typeof( int ) ) ) ),
            Expression.Constant( "foobar" )
        };

        var sut = new ParsedExpressionSwitch();

        var result = sut.Process( parameters );

        using ( new AssertionScope() )
        {
            result.NodeType.Should().Be( ExpressionType.Switch );
            if ( result is not SwitchExpression @switch )
                return;

            @switch.SwitchValue.Should().BeSameAs( parameters[0] );
            @switch.DefaultBody.Should().BeSameAs( parameters[^1] );
            @switch.Cases.Should().BeSequentiallyEqualTo( (SwitchCase)((ConstantExpression)parameters[3]).Value! );
        }
    }

    [Fact]
    public void
        Process_ShouldReturnSwitchExpression_WhenValueIsConstantAndIsNotEqualToAnyConstantCaseValueAndSomeCaseValuesAreVariable_WhenCaseContainsBothConstantAndVariableValues()
    {
        var caseBody = Expression.Constant( "qux" );
        var caseParameter = Expression.Parameter( typeof( int ) );

        var parameters = new Expression[]
        {
            Expression.Constant( 2 ),
            Expression.Constant( Expression.SwitchCase( Expression.Constant( "foo" ), Expression.Constant( 0 ) ) ),
            Expression.Constant( Expression.SwitchCase( Expression.Constant( "bar" ), Expression.Constant( 1 ) ) ),
            Expression.Constant( Expression.SwitchCase( caseBody, Expression.Constant( 3 ), caseParameter ) ),
            Expression.Constant( "foobar" )
        };

        var sut = new ParsedExpressionSwitch();

        var result = sut.Process( parameters );

        using ( new AssertionScope() )
        {
            result.NodeType.Should().Be( ExpressionType.Switch );
            if ( result is not SwitchExpression @switch )
                return;

            @switch.SwitchValue.Should().BeSameAs( parameters[0] );
            @switch.DefaultBody.Should().BeSameAs( parameters[^1] );
            var switchCase = @switch.Cases.Should().HaveCount( 1 ).And.Subject.First();
            switchCase.Body.Should().BeSameAs( caseBody );
            switchCase.TestValues.Should().BeSequentiallyEqualTo( caseParameter );
        }
    }
}
