using System.Linq;
using System.Linq.Expressions;
using LfrlAnvil.Computable.Expressions.Constructs.Variadic;
using LfrlAnvil.Computable.Expressions.Exceptions;
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
    public void Process_ShouldReturnSwitchExpressionWithImplicitDefaultBody_WhenValueIsNotConstantAndNoDefaultBodyIsProvided()
    {
        var parameters = new Expression[]
        {
            Expression.Parameter( typeof( int ) ),
            Expression.Constant( Expression.SwitchCase( Expression.Constant( "foo" ), Expression.Constant( 0 ) ) )
        };

        var sut = new ParsedExpressionSwitch();

        var result = sut.Process( parameters );

        using ( new AssertionScope() )
        {
            result.NodeType.Should().Be( ExpressionType.Switch );
            if ( result is not SwitchExpression @switch )
                return;

            @switch.SwitchValue.Should().BeSameAs( parameters[0] );
            @switch.Cases.Should().BeSequentiallyEqualTo( (SwitchCase)((ConstantExpression)parameters[1]).Value! );
            (@switch.DefaultBody?.NodeType).Should().Be( ExpressionType.Throw );
            if ( @switch.DefaultBody is not UnaryExpression defaultThrow )
                return;

            defaultThrow.Type.Should().Be( typeof( string ) );
            defaultThrow.Operand.NodeType.Should().Be( ExpressionType.New );
            if ( defaultThrow.Operand is not NewExpression exception )
                return;

            exception.Type.Should().Be( typeof( ParsedExpressionInvocationException ) );
            exception.Arguments.Should().HaveCount( 2 );
            if ( exception.Arguments.Count != 2 )
                return;

            var firstArg = exception.Arguments[0];
            var secondArg = exception.Arguments[1];

            firstArg.NodeType.Should().Be( ExpressionType.Constant );
            secondArg.NodeType.Should().Be( ExpressionType.NewArrayInit );
            if ( firstArg is not ConstantExpression constantArg || secondArg is not NewArrayExpression arrayArg )
                return;

            constantArg.Value.Should().Be( Resources.SwitchValueWasNotHandledByAnyCaseFormat );
            arrayArg.Expressions.Should().HaveCount( 1 );
            if ( arrayArg.Expressions.Count != 1 )
                return;

            arrayArg.Expressions[0].NodeType.Should().Be( ExpressionType.Convert );
            if ( arrayArg.Expressions[0] is not UnaryExpression argConvert )
                return;

            argConvert.Operand.Should().BeSameAs( parameters[0] );
        }
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

    [Fact]
    public void Process_ShouldThrowArgumentException_WhenAllCasesAreThrowExpressions()
    {
        var exception = Expression.Constant( new Exception() );
        var parameters = new Expression[]
        {
            Expression.Constant( 0 ),
            Expression.Constant( Expression.SwitchCase( Expression.Throw( exception ), Expression.Parameter( typeof( int ) ) ) ),
            Expression.Constant( Expression.SwitchCase( Expression.Throw( exception ), Expression.Parameter( typeof( int ) ) ) ),
            Expression.Constant( Expression.SwitchCase( Expression.Throw( exception ), Expression.Parameter( typeof( int ) ) ) ),
            Expression.Throw( exception )
        };

        var sut = new ParsedExpressionSwitch();

        var action = Lambda.Of( () => sut.Process( parameters ) );

        action.Should().ThrowExactly<ArgumentException>();
    }

    [Fact]
    public void Process_ShouldReturnSwitchExpression_WhenValueIsNotConstantAndSomeCasesAreThrowExpressions()
    {
        var exception = Expression.Constant( new Exception() );
        var parameters = new Expression[]
        {
            Expression.Parameter( typeof( int ) ),
            Expression.Constant( Expression.SwitchCase( Expression.Constant( "foo" ), Expression.Constant( 0 ) ) ),
            Expression.Constant( Expression.SwitchCase( Expression.Throw( exception ), Expression.Constant( 1 ) ) ),
            Expression.Constant( Expression.SwitchCase( Expression.Constant( "qux" ), Expression.Constant( 2 ) ) ),
            Expression.Throw( exception )
        };

        var sut = new ParsedExpressionSwitch();

        var result = sut.Process( parameters );

        using ( new AssertionScope() )
        {
            result.NodeType.Should().Be( ExpressionType.Switch );
            if ( result is not SwitchExpression @switch )
                return;

            @switch.SwitchValue.Should().BeSameAs( parameters[0] );
            (@switch.DefaultBody?.NodeType).Should().Be( ExpressionType.Throw );
            @switch.Cases.Should().HaveCount( 3 );

            if ( @switch.DefaultBody is not UnaryExpression defaultThrow || @switch.Cases.Count != 3 )
                return;

            defaultThrow.Type.Should().Be( typeof( string ) );
            defaultThrow.Operand.Should().BeSameAs( exception );
            @switch.Cases[0].Should().BeSameAs( (SwitchCase)((ConstantExpression)parameters[1]).Value! );

            @switch.Cases[1].Body.NodeType.Should().Be( ExpressionType.Throw );
            @switch.Cases[1]
                .TestValues.Should()
                .BeSequentiallyEqualTo( ((SwitchCase)((ConstantExpression)parameters[2]).Value!).TestValues );

            @switch.Cases[2].Should().BeSameAs( (SwitchCase)((ConstantExpression)parameters[3]).Value! );

            if ( @switch.Cases[1].Body is not UnaryExpression caseThrow )
                return;

            caseThrow.Type.Should().Be( typeof( string ) );
            caseThrow.Operand.Should().BeSameAs( exception );
        }
    }

    [Fact]
    public void Process_ShouldReturnCorrectCaseBody_WhenValueIsConstantAndEqualsToOneOfConstantCaseValuesWithThrowBody()
    {
        var exception = Expression.Constant( new Exception() );
        var parameters = new Expression[]
        {
            Expression.Constant( 2 ),
            Expression.Constant( Expression.SwitchCase( Expression.Constant( "foo" ), Expression.Constant( 0 ) ) ),
            Expression.Constant( Expression.SwitchCase( Expression.Constant( "bar" ), Expression.Constant( 1 ) ) ),
            Expression.Constant( Expression.SwitchCase( Expression.Throw( exception ), Expression.Constant( 2 ) ) ),
            Expression.Constant( "foobar" )
        };

        var sut = new ParsedExpressionSwitch();

        var result = sut.Process( parameters );

        using ( new AssertionScope() )
        {
            result.NodeType.Should().Be( ExpressionType.Throw );
            if ( result is not UnaryExpression @throw )
                return;

            @throw.Type.Should().Be( typeof( string ) );
            @throw.Operand.Should().BeSameAs( exception );
        }
    }

    [Fact]
    public void Process_ShouldReturnDefaultBody_WhenValueIsConstantAndDoesNorEqualToAnyConstantCaseValueAndDefaultBodyIsThrowExpression()
    {
        var exception = Expression.Constant( new Exception() );
        var parameters = new Expression[]
        {
            Expression.Constant( 3 ),
            Expression.Constant( Expression.SwitchCase( Expression.Constant( "foo" ), Expression.Constant( 0 ) ) ),
            Expression.Constant( Expression.SwitchCase( Expression.Constant( "bar" ), Expression.Constant( 1 ) ) ),
            Expression.Constant( Expression.SwitchCase( Expression.Constant( "qux" ), Expression.Constant( 2 ) ) ),
            Expression.Throw( exception )
        };

        var sut = new ParsedExpressionSwitch();

        var result = sut.Process( parameters );

        using ( new AssertionScope() )
        {
            result.NodeType.Should().Be( ExpressionType.Throw );
            if ( result is not UnaryExpression @throw )
                return;

            @throw.Type.Should().Be( typeof( string ) );
            @throw.Operand.Should().BeSameAs( exception );
        }
    }
}
