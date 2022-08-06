using System;
using System.Linq;
using FluentAssertions;
using FluentAssertions.Execution;
using LfrlAnvil.Functional;
using LfrlAnvil.Mathematical.Expressions.Constructs;
using LfrlAnvil.Mathematical.Expressions.Errors;
using LfrlAnvil.Mathematical.Expressions.Exceptions;
using LfrlAnvil.Mathematical.Expressions.Internal;
using LfrlAnvil.TestExtensions;
using LfrlAnvil.TestExtensions.Attributes;
using LfrlAnvil.TestExtensions.FluentAssertions;
using LfrlAnvil.TestExtensions.NSubstitute;
using NSubstitute;
using Xunit;

namespace LfrlAnvil.Mathematical.Expressions.Tests.MathExpressionFactoryTests;

[TestClass( typeof( MathExpressionFactoryTestsData ) )]
public partial class MathExpressionFactoryTests : TestsBase
{
    [Fact]
    public void Create_ShouldProvideCorrectParamsToNumberParserProvider()
    {
        var provider = Substitute.For<Func<MathExpressionNumberParserParams, IMathExpressionNumberParser>>();
        provider.WithAnyArgs(
            c => MathExpressionNumberParser.CreateDefaultDecimal( c.ArgAt<MathExpressionNumberParserParams>( 0 ).Configuration ) );

        var builder = new MathExpressionFactoryBuilder()
            .AddPrefixTypeConverter( "[string]", new MockPrefixTypeConverter() )
            .SetNumberParserProvider( provider )
            .SetPrefixUnaryConstructPrecedence( "[string]", 1 );

        var sut = builder.Build();

        var _ = sut.Create<decimal, string>( "[string] 0" );

        using ( new AssertionScope() )
        {
            var @params = (MathExpressionNumberParserParams)provider.Verify().CallAt( 0 ).Exists().And.Arguments[0]!;
            @params.Configuration.Should().BeSameAs( sut.Configuration );
            @params.ArgumentType.Should().Be( typeof( decimal ) );
            @params.ResultType.Should().Be( typeof( string ) );
        }
    }

    [Fact]
    public void Create_ShouldReturnExpressionWithCorrectArgumentIndexes()
    {
        var (aValue, bValue, cValue, dValue) = Fixture.CreateDistinctCollection<decimal>( count: 4 );
        var expected = aValue + bValue + cValue + aValue + cValue + dValue + bValue;

        var input = "a + b + c + a + c + d + b";
        var builder = new MathExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new MathExpressionAddOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var sut = builder.Build();

        var expression = sut.Create<decimal, decimal>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke( aValue, bValue, cValue, dValue );

        using ( new AssertionScope() )
        {
            result.Should().Be( expected );
            expression.GetUnboundArgumentIndex( "a" ).Should().Be( 0 );
            expression.GetUnboundArgumentIndex( "b" ).Should().Be( 1 );
            expression.GetUnboundArgumentIndex( "c" ).Should().Be( 2 );
            expression.GetUnboundArgumentIndex( "d" ).Should().Be( 3 );
        }
    }

    [Theory]
    [InlineData( "'foobar'" )]
    [InlineData( "( 'foobar' )" )]
    [InlineData( "( ( ( 'foobar' ) ) )" )]
    public void DelegateInvoke_ShouldReturnCorrectResult_WhenExpressionConsistsOfOnlyStringConstant(string input)
    {
        var builder = new MathExpressionFactoryBuilder();
        var sut = builder.Build();

        var expression = sut.Create<string, string>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke();

        result.Should().Be( "foobar" );
    }

    [Theory]
    [InlineData( "12.34" )]
    [InlineData( "( 12.34 )" )]
    [InlineData( "( ( ( 12.34 ) ) )" )]
    public void DelegateInvoke_ShouldReturnCorrectResult_WhenExpressionConsistsOfOnlyNumberConstant(string input)
    {
        var builder = new MathExpressionFactoryBuilder();
        var sut = builder.Build();

        var expression = sut.Create<decimal, decimal>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke();

        result.Should().Be( 12.34m );
    }

    [Theory]
    [InlineData( "false", false )]
    [InlineData( "( false )", false )]
    [InlineData( "( ( ( false ) ) )", false )]
    [InlineData( "true", true )]
    [InlineData( "( true )", true )]
    [InlineData( "( ( ( true ) ) )", true )]
    public void DelegateInvoke_ShouldReturnCorrectResult_WhenExpressionConsistsOfOnlyBooleanFalse(string input, bool expected)
    {
        var builder = new MathExpressionFactoryBuilder();
        var sut = builder.Build();

        var expression = sut.Create<bool, bool>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke();

        result.Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( MathExpressionFactoryTestsData.GetArgumentOnlyData ) )]
    public void DelegateInvoke_ShouldReturnCorrectResult_WhenExpressionConsistsOfOnlyArgumentSymbol(string input, decimal value)
    {
        var builder = new MathExpressionFactoryBuilder();
        var sut = builder.Build();

        var expression = sut.Create<decimal, decimal>( input );

        using ( new AssertionScope() )
        {
            expression.GetArgumentNames().Select( n => n.ToString() ).Should().BeSequentiallyEqualTo( "a" );
            if ( expression.GetArgumentCount() != 1 )
                return;

            var @delegate = expression.Compile();
            var result = @delegate.Invoke( value );
            result.Should().Be( value );
        }
    }

    [Fact]
    public void Build_ShouldThrowMathExpressionCreationException_WhenStringConstantIsTheLastTokenAndDoesNotHaveClosingDelimiter()
    {
        var input = "'foobar";
        var builder = new MathExpressionFactoryBuilder();
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<MathExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, MathExpressionBuilderErrorType.StringConstantParsingFailure ) );
    }

    [Fact]
    public void Create_ShouldThrowMathExpressionCreationException_WhenNumberParserFailedToParseNumberConstant()
    {
        var input = "12.34";
        var builder = new MathExpressionFactoryBuilder().SetNumberParserProvider( _ => new FailingNumberParser() );
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<decimal, decimal>( input ) );

        action.Should()
            .ThrowExactly<MathExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, MathExpressionBuilderErrorType.NumberConstantParsingFailure ) );
    }

    [Fact]
    public void Create_ShouldThrowMathExpressionCreationException_WhenArgumentHasInvalidName()
    {
        var input = "/";
        var builder = new MathExpressionFactoryBuilder();
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<decimal, decimal>( input ) );

        action.Should()
            .ThrowExactly<MathExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, MathExpressionBuilderErrorType.InvalidArgumentName ) );
    }

    [Theory]
    [MethodData( nameof( MathExpressionFactoryTestsData.GetOperandFollowedByOperandData ) )]
    public void Create_ShouldThrowMathExpressionCreationException_WhenOperandIsFollowedByOperand(string input)
    {
        var builder = new MathExpressionFactoryBuilder();
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<MathExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, MathExpressionBuilderErrorType.UnexpectedOperand ) );
    }

    [Theory]
    [MethodData( nameof( MathExpressionFactoryTestsData.GetOperandFollowedByPrefixUnaryOperatorData ) )]
    public void Create_ShouldThrowMathExpressionCreationException_WhenOperandIsFollowedByPrefixUnaryOperator(string input)
    {
        var builder = new MathExpressionFactoryBuilder()
            .AddPrefixUnaryOperator( "-", new MockPrefixUnaryOperator() )
            .SetPrefixUnaryConstructPrecedence( "-", 1 );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<MathExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, MathExpressionBuilderErrorType.PostfixUnaryOrBinaryConstructDoesNotExist ) );
    }

    [Theory]
    [MethodData( nameof( MathExpressionFactoryTestsData.GetOperandFollowedByPrefixTypeConverterData ) )]
    public void Create_ShouldThrowMathExpressionCreationException_WhenOperandIsFollowedByPrefixTypeConverter(string input)
    {
        var builder = new MathExpressionFactoryBuilder()
            .AddPrefixTypeConverter( "[string]", new MockPrefixTypeConverter() )
            .SetPrefixUnaryConstructPrecedence( "[string]", 1 );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<MathExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, MathExpressionBuilderErrorType.PostfixUnaryOrBinaryConstructDoesNotExist ) );
    }

    [Fact]
    public void Create_ShouldThrowMathExpressionCreationException_WhenUnexpectedExceptionIsThrown()
    {
        var input = "a";
        var exception = new Exception();
        var builder = new MathExpressionFactoryBuilder().SetNumberParserProvider( _ => throw exception );
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<decimal, decimal>( input ) );

        action.Should()
            .ThrowExactly<MathExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, MathExpressionBuilderErrorType.Error ) );
    }

    [Theory]
    [MethodData( nameof( MathExpressionFactoryTestsData.GetOperandFollowedByOpenParenthesisData ) )]
    public void Create_ShouldThrowMathExpressionCreationException_WhenOperandIsFollowedByOpenedParenthesis(string input)
    {
        var builder = new MathExpressionFactoryBuilder();
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<decimal, decimal>( input ) );

        action.Should()
            .ThrowExactly<MathExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, MathExpressionBuilderErrorType.UnexpectedOpenedParenthesis ) );
    }

    [Fact]
    public void Create_ShouldThrowMathExpressionCreationException_WhenPostfixUnaryOperatorIsFollowedByOpenedParenthesis()
    {
        var input = "a ^ (";
        var builder = new MathExpressionFactoryBuilder()
            .AddPostfixUnaryOperator( "^", new MockPostfixUnaryOperator() )
            .SetPostfixUnaryConstructPrecedence( "^", 1 );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<MathExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, MathExpressionBuilderErrorType.UnexpectedOpenedParenthesis ) );
    }

    [Fact]
    public void Create_ShouldThrowMathExpressionCreationException_WhenPostfixTypeConverterIsFollowedByOpenedParenthesis()
    {
        var input = "a ToString (";
        var builder = new MathExpressionFactoryBuilder()
            .AddPostfixTypeConverter( "ToString", new MockPostfixTypeConverter() )
            .SetPostfixUnaryConstructPrecedence( "ToString", 1 );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<MathExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, MathExpressionBuilderErrorType.UnexpectedOpenedParenthesis ) );
    }

    [Fact]
    public void Create_ShouldThrowMathExpressionCreationException_WhenClosedParenthesisIsFollowedByOpenedParenthesis()
    {
        var input = "( a ) (";
        var builder = new MathExpressionFactoryBuilder();
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<decimal, decimal>( input ) );

        action.Should()
            .ThrowExactly<MathExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, MathExpressionBuilderErrorType.UnexpectedOpenedParenthesis ) );
    }

    [Fact]
    public void Create_ShouldThrowMathExpressionCreationException_WhenOpenedParenthesisIsFollowedByClosedParenthesis()
    {
        var input = "( )";
        var builder = new MathExpressionFactoryBuilder();
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<decimal, decimal>( input ) );

        action.Should()
            .ThrowExactly<MathExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, MathExpressionBuilderErrorType.UnexpectedClosedParenthesis ) );
    }

    [Fact]
    public void
        Create_ShouldThrowMathExpressionCreationException_WhenAmbiguousBinaryOperatorCannotBeProcessedDuringOpenedParenthesisHandling()
    {
        var input = "a + ( b )";
        var builder = new MathExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new ThrowingBinaryOperator() )
            .AddPostfixUnaryOperator( "+", new MockPostfixUnaryOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 )
            .SetPostfixUnaryConstructPrecedence( "+", 1 );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<MathExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, MathExpressionBuilderErrorType.ConstructHasThrownException ) );
    }

    [Fact]
    public void
        Create_ShouldThrowMathExpressionCreationException_WhenAmbiguousPostfixUnaryOperatorCannotBeProcessedDuringOpenedParenthesisHandling()
    {
        var input = "a + + ( b )";
        var builder = new MathExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new MockBinaryOperator() )
            .AddPostfixUnaryOperator( "+", new ThrowingUnaryOperator() )
            .AddPrefixUnaryOperator( "+", new MockPrefixUnaryOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 )
            .SetPostfixUnaryConstructPrecedence( "+", 1 )
            .SetPrefixUnaryConstructPrecedence( "+", 1 );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<MathExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, MathExpressionBuilderErrorType.ConstructHasThrownException ) );
    }

    [Theory]
    [MethodData( nameof( MathExpressionFactoryTestsData.GetClosedParenthesisDoesNotHaveCorrespondingOpenParenthesisData ) )]
    public void Create_ShouldThrowMathExpressionCreationException_WhenClosedParenthesisDoesNotHaveCorrespondingOpenedParenthesis(
        string input)
    {
        var builder = new MathExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new MockBinaryOperator() )
            .AddPrefixUnaryOperator( "-", new MockPrefixUnaryOperator() )
            .AddPrefixTypeConverter( "[string]", new MockPrefixTypeConverter() )
            .AddPostfixUnaryOperator( "^", new MockPostfixUnaryOperator() )
            .AddPostfixTypeConverter( "ToString", new MockPostfixTypeConverter() )
            .SetBinaryOperatorPrecedence( "+", 1 )
            .SetPrefixUnaryConstructPrecedence( "-", 1 )
            .SetPrefixUnaryConstructPrecedence( "[string]", 1 )
            .SetPostfixUnaryConstructPrecedence( "^", 1 )
            .SetPostfixUnaryConstructPrecedence( "ToString", 1 );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<MathExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, MathExpressionBuilderErrorType.UnexpectedClosedParenthesis ) );
    }

    [Fact]
    public void Create_ShouldThrowMathExpressionCreationException_WhenPrefixUnaryOperatorIsFollowedByClosedParenthesis()
    {
        var input = "( - )";
        var builder = new MathExpressionFactoryBuilder()
            .AddPrefixUnaryOperator( "-", new MockPrefixUnaryOperator() )
            .SetPrefixUnaryConstructPrecedence( "-", 1 );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<MathExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, MathExpressionBuilderErrorType.UnexpectedClosedParenthesis ) );
    }

    [Fact]
    public void Create_ShouldThrowMathExpressionCreationException_WhenPrefixTypeConverterIsFollowedByClosedParenthesis()
    {
        var input = "( [string] )";
        var builder = new MathExpressionFactoryBuilder()
            .AddPrefixTypeConverter( "[string]", new MockPrefixTypeConverter() )
            .SetPrefixUnaryConstructPrecedence( "[string]", 1 );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<MathExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, MathExpressionBuilderErrorType.UnexpectedClosedParenthesis ) );
    }

    [Fact]
    public void Create_ShouldThrowMathExpressionCreationException_WhenBinaryOperatorIsFollowedByClosedParenthesis()
    {
        var input = "( a + )";
        var builder = new MathExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new MockBinaryOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<MathExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, MathExpressionBuilderErrorType.UnexpectedClosedParenthesis ) );
    }

    [Fact]
    public void
        Create_ShouldThrowMathExpressionCreationException_WhenAmbiguousPostfixUnaryOperatorCannotBeProcessedDuringClosedParenthesisHandling()
    {
        var input = "( a + )";
        var builder = new MathExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new MockBinaryOperator() )
            .AddPostfixUnaryOperator( "+", new ThrowingUnaryOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 )
            .SetPostfixUnaryConstructPrecedence( "+", 1 );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<MathExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, MathExpressionBuilderErrorType.ConstructHasThrownException ) );
    }

    [Fact]
    public void
        Create_ShouldThrowMathExpressionCreationException_WhenPostfixUnaryConstructAmbiguityCannotBeResolvedDueToBinaryOperatorAmbiguity()
    {
        var input = "( a + + )";
        var builder = new MathExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new MockBinaryOperator() )
            .AddPostfixUnaryOperator( "+", new MockPostfixUnaryOperator() )
            .AddPrefixUnaryOperator( "+", new MockPrefixUnaryOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 )
            .SetPostfixUnaryConstructPrecedence( "+", 1 )
            .SetPrefixUnaryConstructPrecedence( "+", 1 );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<MathExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, MathExpressionBuilderErrorType.AmbiguousPostfixUnaryConstructResolutionFailure ) );
    }

    [Fact]
    public void Create_ShouldThrowMathExpressionCreationException_WhenPrefixUnaryConstructCannotBeProcessedDuringClosedParenthesisHandling()
    {
        var input = "( - a )";
        var builder = new MathExpressionFactoryBuilder()
            .AddPrefixUnaryOperator( "-", new ThrowingUnaryOperator() )
            .SetPrefixUnaryConstructPrecedence( "-", 1 );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<MathExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, MathExpressionBuilderErrorType.ConstructHasThrownException ) );
    }

    [Fact]
    public void Create_ShouldThrowMathExpressionCreationException_WhenPrefixTypeConverterCannotBeProcessedDuringClosedParenthesisHandling()
    {
        var input = "( [string] a )";
        var builder = new MathExpressionFactoryBuilder()
            .AddPrefixTypeConverter( "[string]", new ThrowingTypeConverter() )
            .SetPrefixUnaryConstructPrecedence( "[string]", 1 );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<decimal, string>( input ) );

        action.Should()
            .ThrowExactly<MathExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, MathExpressionBuilderErrorType.ConstructHasThrownException ) );
    }

    [Fact]
    public void Create_ShouldThrowMathExpressionCreationException_WhenBinaryOperatorCannotBeProcessedDuringClosedParenthesisHandling()
    {
        var input = "( a + b )";
        var builder = new MathExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new ThrowingBinaryOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<MathExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, MathExpressionBuilderErrorType.ConstructHasThrownException ) );
    }

    [Fact]
    public void
        Create_ShouldThrowMathExpressionCreationException_WhenAmbiguousPostfixUnaryOperatorCannotBeProcessedDuringPrefixUnaryOperatorHandling()
    {
        var input = "a + * * b";
        var builder = new MathExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new MockBinaryOperator() )
            .AddBinaryOperator( "*", new MockBinaryOperator() )
            .AddPostfixUnaryOperator( "+", new ThrowingUnaryOperator() )
            .AddPrefixUnaryOperator( "*", new MockPrefixUnaryOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 )
            .SetBinaryOperatorPrecedence( "*", 1 )
            .SetPostfixUnaryConstructPrecedence( "+", 1 )
            .SetPrefixUnaryConstructPrecedence( "*", 1 );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<MathExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, MathExpressionBuilderErrorType.ConstructHasThrownException ) );
    }

    [Fact]
    public void
        Create_ShouldThrowMathExpressionCreationException_WhenAmbiguousBinaryOperatorCannotBeProcessedDuringPrefixUnaryOperatorHandling()
    {
        var input = "a + b + * c";
        var builder = new MathExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new ThrowingBinaryOperator() )
            .AddPostfixUnaryOperator( "+", new MockPostfixUnaryOperator() )
            .AddPrefixUnaryOperator( "*", new MockPrefixUnaryOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 )
            .SetPostfixUnaryConstructPrecedence( "+", 1 )
            .SetPrefixUnaryConstructPrecedence( "*", 1 );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<MathExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, MathExpressionBuilderErrorType.ConstructHasThrownException ) );
    }

    [Fact]
    public void
        Create_ShouldThrowMathExpressionCreationException_WhenAmbiguousPostfixUnaryOperatorCannotBeProcessedDuringPrefixTypeConverterHandling()
    {
        var input = "a + * [string] b";
        var builder = new MathExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new MockBinaryOperator() )
            .AddBinaryOperator( "*", new MockBinaryOperator() )
            .AddPostfixUnaryOperator( "+", new ThrowingUnaryOperator() )
            .AddPrefixUnaryOperator( "*", new MockPrefixUnaryOperator() )
            .AddPrefixTypeConverter( "[string]", new MockPrefixTypeConverter() )
            .SetBinaryOperatorPrecedence( "+", 1 )
            .SetBinaryOperatorPrecedence( "*", 1 )
            .SetPostfixUnaryConstructPrecedence( "+", 1 )
            .SetPrefixUnaryConstructPrecedence( "*", 1 )
            .SetPrefixUnaryConstructPrecedence( "[string]", 1 );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<decimal, string>( input ) );

        action.Should()
            .ThrowExactly<MathExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, MathExpressionBuilderErrorType.ConstructHasThrownException ) );
    }

    [Fact]
    public void
        Create_ShouldThrowMathExpressionCreationException_WhenAmbiguousBinaryOperatorCannotBeProcessedDuringPrefixTypeConverterHandling()
    {
        var input = "a + b + [string] c";
        var builder = new MathExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new ThrowingBinaryOperator() )
            .AddPostfixUnaryOperator( "+", new MockPostfixUnaryOperator() )
            .AddPrefixTypeConverter( "[string]", new MockPrefixTypeConverter() )
            .SetBinaryOperatorPrecedence( "+", 1 )
            .SetPostfixUnaryConstructPrecedence( "+", 1 )
            .SetPrefixUnaryConstructPrecedence( "[string]", 1 );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<decimal, string>( input ) );

        action.Should()
            .ThrowExactly<MathExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, MathExpressionBuilderErrorType.ConstructHasThrownException ) );
    }

    [Fact]
    public void Create_ShouldThrowMathExpressionCreationException_WhenAmbiguousPostfixUnaryOperatorIsFollowedByPostfixUnaryOperator()
    {
        var input = "a + * b";
        var builder = new MathExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new MockBinaryOperator() )
            .AddPostfixUnaryOperator( "+", new MockPostfixUnaryOperator() )
            .AddPostfixUnaryOperator( "*", new MockPrefixUnaryOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 )
            .SetPostfixUnaryConstructPrecedence( "+", 1 )
            .SetPostfixUnaryConstructPrecedence( "*", 1 );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<decimal, string>( input ) );

        action.Should()
            .ThrowExactly<MathExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, MathExpressionBuilderErrorType.BinaryOrPrefixUnaryConstructDoesNotExist ) );
    }

    [Fact]
    public void Create_ShouldThrowMathExpressionCreationException_WhenAmbiguousPostfixUnaryOperatorIsFollowedByPostfixTypeConverter()
    {
        var input = "a + ToString b";
        var builder = new MathExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new MockBinaryOperator() )
            .AddPostfixUnaryOperator( "+", new MockPostfixUnaryOperator() )
            .AddPostfixTypeConverter( "ToString", new MockPostfixTypeConverter() )
            .SetBinaryOperatorPrecedence( "+", 1 )
            .SetPostfixUnaryConstructPrecedence( "+", 1 )
            .SetPostfixUnaryConstructPrecedence( "ToString", 1 );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<decimal, string>( input ) );

        action.Should()
            .ThrowExactly<MathExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, MathExpressionBuilderErrorType.BinaryOrPrefixUnaryConstructDoesNotExist ) );
    }

    [Theory]
    [MethodData( nameof( MathExpressionFactoryTestsData.GetPrefixUnaryConstructIsFollowedByAnotherConstructData ) )]
    public void Create_ShouldThrowMathExpressionCreationException_WhenPrefixUnaryConstructIsFollowedByAnotherConstruct(string input)
    {
        var builder = new MathExpressionFactoryBuilder()
            .AddPrefixUnaryOperator( "-", new MockPrefixUnaryOperator() )
            .AddPrefixTypeConverter( "[string]", new MockPrefixTypeConverter() )
            .AddBinaryOperator( "+", new MockBinaryOperator() )
            .AddPostfixUnaryOperator( "^", new MockPostfixUnaryOperator() )
            .AddPostfixTypeConverter( "ToString", new MockPostfixTypeConverter() )
            .SetPrefixUnaryConstructPrecedence( "-", 1 )
            .SetPrefixUnaryConstructPrecedence( "[string]", 1 )
            .SetBinaryOperatorPrecedence( "+", 1 )
            .SetPostfixUnaryConstructPrecedence( "^", 1 )
            .SetPostfixUnaryConstructPrecedence( "ToString", 1 );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<decimal, string>( input ) );

        action.Should()
            .ThrowExactly<MathExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, MathExpressionBuilderErrorType.UnexpectedConstruct ) );
    }

    [Fact]
    public void
        Create_ShouldThrowMathExpressionCreationException_WhenAmbiguousPostfixUnaryOperatorCannotBeProcessedDuringBinaryOperatorHandling()
    {
        var input = "a + * b";
        var builder = new MathExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new MockBinaryOperator() )
            .AddBinaryOperator( "*", new MockBinaryOperator() )
            .AddPostfixUnaryOperator( "+", new ThrowingUnaryOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 )
            .SetBinaryOperatorPrecedence( "*", 1 )
            .SetPostfixUnaryConstructPrecedence( "+", 1 );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<MathExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, MathExpressionBuilderErrorType.ConstructHasThrownException ) );
    }

    [Fact]
    public void Create_ShouldThrowMathExpressionCreationException_WhenPrefixUnaryOperatorThrowsExceptionDuringBinaryOperatorHandling()
    {
        var input = "- a + b";
        var builder = new MathExpressionFactoryBuilder()
            .AddPrefixUnaryOperator( "-", new ThrowingUnaryOperator() )
            .AddBinaryOperator( "+", new MockBinaryOperator() )
            .SetPrefixUnaryConstructPrecedence( "-", 1 )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<MathExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, MathExpressionBuilderErrorType.ConstructHasThrownException ) );
    }

    [Fact]
    public void Create_ShouldThrowMathExpressionCreationException_WhenPrefixTypeConverterThrowsExceptionDuringBinaryOperatorHandling()
    {
        var input = "[string] a + b";
        var builder = new MathExpressionFactoryBuilder()
            .AddPrefixTypeConverter( "[string]", new ThrowingTypeConverter() )
            .AddBinaryOperator( "+", new MockBinaryOperator() )
            .SetPrefixUnaryConstructPrecedence( "[string]", 1 )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<decimal, string>( input ) );

        action.Should()
            .ThrowExactly<MathExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, MathExpressionBuilderErrorType.ConstructHasThrownException ) );
    }

    [Fact]
    public void Create_ShouldThrowMathExpressionCreationException_WhenBinaryOperatorThrowsException()
    {
        var input = "a + b";
        var builder = new MathExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new ThrowingBinaryOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<decimal, string>( input ) );

        action.Should()
            .ThrowExactly<MathExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, MathExpressionBuilderErrorType.ConstructHasThrownException ) );
    }

    [Fact]
    public void Create_ShouldThrowMathExpressionCreationException_WhenPostfixUnaryOperatorThrowsException()
    {
        var input = "a ^";
        var builder = new MathExpressionFactoryBuilder()
            .AddPostfixUnaryOperator( "^", new ThrowingUnaryOperator() )
            .SetPostfixUnaryConstructPrecedence( "^", 1 );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<MathExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, MathExpressionBuilderErrorType.ConstructHasThrownException ) );
    }

    [Fact]
    public void Create_ShouldThrowMathExpressionCreationException_WhenPostfixTypeConverterThrowsException()
    {
        var input = "a ToString";
        var builder = new MathExpressionFactoryBuilder()
            .AddPostfixTypeConverter( "ToString", new ThrowingTypeConverter() )
            .SetPostfixUnaryConstructPrecedence( "ToString", 1 );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<decimal, string>( input ) );

        action.Should()
            .ThrowExactly<MathExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, MathExpressionBuilderErrorType.ConstructHasThrownException ) );
    }

    [Theory]
    [InlineData( 1, 1 )]
    [InlineData( 1, 2 )]
    [InlineData( 2, 1 )]
    public void
        Create_ShouldThrowMathExpressionCreationException_WhenPostfixUnaryOperatorThrowsExceptionDuringPrefixUnaryOperatorResolution(
            int prefixPrecedence,
            int postfixPrecedence)
    {
        var input = "- a ^";
        var builder = new MathExpressionFactoryBuilder()
            .AddPostfixUnaryOperator( "^", new ThrowingUnaryOperator() )
            .AddPrefixUnaryOperator( "-", new MockPrefixUnaryOperator() )
            .SetPostfixUnaryConstructPrecedence( "^", postfixPrecedence )
            .SetPrefixUnaryConstructPrecedence( "-", prefixPrecedence );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<decimal, string>( input ) );

        action.Should()
            .ThrowExactly<MathExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, MathExpressionBuilderErrorType.ConstructHasThrownException ) );
    }

    [Theory]
    [InlineData( 1, 1 )]
    [InlineData( 1, 2 )]
    [InlineData( 2, 1 )]
    public void
        Create_ShouldThrowMathExpressionCreationException_WhenPostfixUnaryOperatorThrowsExceptionDuringPrefixTypeConverterResolution(
            int prefixPrecedence,
            int postfixPrecedence)
    {
        var input = "[string] a ^";
        var builder = new MathExpressionFactoryBuilder()
            .AddPostfixUnaryOperator( "^", new ThrowingUnaryOperator() )
            .AddPrefixTypeConverter( "[string]", new MockPrefixTypeConverter() )
            .SetPostfixUnaryConstructPrecedence( "^", postfixPrecedence )
            .SetPrefixUnaryConstructPrecedence( "[string]", prefixPrecedence );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<decimal, string>( input ) );

        action.Should()
            .ThrowExactly<MathExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, MathExpressionBuilderErrorType.ConstructHasThrownException ) );
    }

    [Fact]
    public void
        Create_ShouldThrowMathExpressionCreationException_WhenPostfixTypeConverterThrowsExceptionDuringPrefixUnaryOperatorResolution()
    {
        var input = "- a ToString";
        var builder = new MathExpressionFactoryBuilder()
            .AddPostfixTypeConverter( "ToString", new ThrowingTypeConverter() )
            .AddPrefixUnaryOperator( "-", new MockPrefixUnaryOperator() )
            .SetPostfixUnaryConstructPrecedence( "ToString", 1 )
            .SetPrefixUnaryConstructPrecedence( "-", 2 );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<decimal, string>( input ) );

        action.Should()
            .ThrowExactly<MathExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, MathExpressionBuilderErrorType.ConstructHasThrownException ) );
    }

    [Theory]
    [InlineData( 1, 1 )]
    [InlineData( 1, 2 )]
    [InlineData( 2, 1 )]
    public void
        Create_ShouldThrowMathExpressionCreationException_WhenPrefixUnaryOperatorThrowsExceptionDuringItsResolutionWithPostfixUnaryOperator(
            int prefixPrecedence,
            int postfixPrecedence)
    {
        var input = "- a ^";
        var builder = new MathExpressionFactoryBuilder()
            .AddPostfixUnaryOperator( "^", new MockPostfixUnaryOperator() )
            .AddPrefixUnaryOperator( "-", new ThrowingUnaryOperator() )
            .SetPostfixUnaryConstructPrecedence( "^", postfixPrecedence )
            .SetPrefixUnaryConstructPrecedence( "-", prefixPrecedence );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<decimal, string>( input ) );

        action.Should()
            .ThrowExactly<MathExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, MathExpressionBuilderErrorType.ConstructHasThrownException ) );
    }

    [Theory]
    [InlineData( 1, 1 )]
    [InlineData( 1, 2 )]
    public void
        Create_ShouldThrowMathExpressionCreationException_WhenPrefixTypeConverterThrowsExceptionDuringItsResolutionWithPostfixUnaryOperator(
            int prefixPrecedence,
            int postfixPrecedence)
    {
        var input = "[string] a ^";
        var builder = new MathExpressionFactoryBuilder()
            .AddPostfixUnaryOperator( "^", new MockPostfixUnaryOperator() )
            .AddPrefixTypeConverter( "[string]", new ThrowingTypeConverter() )
            .SetPostfixUnaryConstructPrecedence( "^", postfixPrecedence )
            .SetPrefixUnaryConstructPrecedence( "[string]", prefixPrecedence );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<decimal, string>( input ) );

        action.Should()
            .ThrowExactly<MathExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, MathExpressionBuilderErrorType.ConstructHasThrownException ) );
    }

    [Theory]
    [InlineData( 1, 1 )]
    [InlineData( 1, 2 )]
    public void
        Create_ShouldThrowMathExpressionCreationException_WhenPrefixUnaryOperatorThrowsExceptionDuringItsResolutionWithPostfixTypeConverter(
            int prefixPrecedence,
            int postfixPrecedence)
    {
        var input = "- a ToString";
        var builder = new MathExpressionFactoryBuilder()
            .AddPostfixTypeConverter( "ToString", new MockPostfixTypeConverter() )
            .AddPrefixUnaryOperator( "-", new ThrowingUnaryOperator() )
            .SetPostfixUnaryConstructPrecedence( "ToString", postfixPrecedence )
            .SetPrefixUnaryConstructPrecedence( "-", prefixPrecedence );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<decimal, string>( input ) );

        action.Should()
            .ThrowExactly<MathExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, MathExpressionBuilderErrorType.ConstructHasThrownException ) );
    }

    [Fact]
    public void
        Create_ShouldThrowMathExpressionCreationException_WhenBinaryOperatorCollectionIsEmptyAfterNonAmbiguousPostfixUnaryOperatorHandling()
    {
        var input = "a ^ + b";
        var builder = new MathExpressionFactoryBuilder()
            .AddPostfixUnaryOperator( "^", new MockPostfixUnaryOperator() )
            .AddPrefixUnaryOperator( "+", new MockPrefixUnaryOperator() )
            .SetPostfixUnaryConstructPrecedence( "^", 1 )
            .SetPrefixUnaryConstructPrecedence( "+", 1 );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<MathExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, MathExpressionBuilderErrorType.BinaryOperatorCollectionIsEmpty ) );
    }

    [Fact]
    public void Create_ShouldThrowMathExpressionCreationException_WhenBinaryOperatorCollectionIsEmptyAfterPostfixTypeConverterHandling()
    {
        var input = "a ToString + b";
        var builder = new MathExpressionFactoryBuilder()
            .AddPostfixTypeConverter( "ToString", new MockPostfixTypeConverter() )
            .AddPrefixUnaryOperator( "+", new MockPrefixUnaryOperator() )
            .SetPostfixUnaryConstructPrecedence( "ToString", 1 )
            .SetPrefixUnaryConstructPrecedence( "+", 1 );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<MathExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, MathExpressionBuilderErrorType.BinaryOperatorCollectionIsEmpty ) );
    }

    [Theory]
    [InlineData( 1, 1 )]
    [InlineData( 1, 2 )]
    public void
        Create_ShouldThrowMathExpressionCreationException_WhenBinaryOperatorCannotBeProcessedDuringBinaryOperatorWithGreaterOrEqualPrecedenceHandling(
            int firstOperatorPrecedence,
            int secondOperatorPrecedence)
    {
        var input = "a * b + c + d";
        var builder = new MathExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new ThrowingBinaryOperator() )
            .AddBinaryOperator( "*", new MockBinaryOperator() )
            .SetBinaryOperatorPrecedence( "+", secondOperatorPrecedence )
            .SetBinaryOperatorPrecedence( "*", firstOperatorPrecedence );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<MathExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, MathExpressionBuilderErrorType.ConstructHasThrownException ) );
    }

    [Fact]
    public void Create_ShouldThrowMathExpressionCreationException_WhenPrefixUnaryOperatorCollectionIsEmptyAtTheStart()
    {
        var input = "+ a + b";
        var builder = new MathExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new MockBinaryOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<MathExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, MathExpressionBuilderErrorType.PrefixUnaryOperatorCollectionIsEmpty ) );
    }

    [Fact]
    public void Create_ShouldThrowMathExpressionCreationException_WhenPrefixUnaryOperatorCollectionIsEmptyAfterBinaryOperatorHandling()
    {
        var input = "a * + b";
        var builder = new MathExpressionFactoryBuilder()
            .AddBinaryOperator( "*", new MockBinaryOperator() )
            .AddBinaryOperator( "+", new MockBinaryOperator() )
            .SetBinaryOperatorPrecedence( "*", 1 )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<MathExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, MathExpressionBuilderErrorType.PrefixUnaryOperatorCollectionIsEmpty ) );
    }

    [Fact]
    public void Create_ShouldThrowMathExpressionCreationException_WhenPrefixTypeConverterCollectionIsEmptyAtTheStart()
    {
        var input = "[string] a + b";
        var builder = new MathExpressionFactoryBuilder()
            .AddPostfixTypeConverter( "[string]", new MockPostfixTypeConverter() )
            .AddBinaryOperator( "+", new MockBinaryOperator() )
            .SetPostfixUnaryConstructPrecedence( "[string]", 1 )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<decimal, string>( input ) );

        action.Should()
            .ThrowExactly<MathExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, MathExpressionBuilderErrorType.PrefixTypeConverterCollectionIsEmpty ) );
    }

    [Fact]
    public void Create_ShouldThrowMathExpressionCreationException_WhenPrefixTypeConverterCollectionIsEmptyAfterBinaryOperatorHandling()
    {
        var input = "a + [string] b";
        var builder = new MathExpressionFactoryBuilder()
            .AddPostfixTypeConverter( "[string]", new MockPostfixTypeConverter() )
            .AddBinaryOperator( "+", new MockBinaryOperator() )
            .SetPostfixUnaryConstructPrecedence( "[string]", 1 )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<decimal, string>( input ) );

        action.Should()
            .ThrowExactly<MathExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, MathExpressionBuilderErrorType.PrefixTypeConverterCollectionIsEmpty ) );
    }

    [Theory]
    [MethodData( nameof( MathExpressionFactoryTestsData.GetMockedSimpleExpressionData ) )]
    public void DelegateInvoke_ShouldReturnCorrectResult_ForMockedSimpleExpression(
        string input,
        string[] argumentValues,
        string expected)
    {
        var builder = new MathExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new MockBinaryOperator() )
            .AddPrefixUnaryOperator( "-", new MockPrefixUnaryOperator() )
            .AddPrefixTypeConverter( "[string]", new MockPrefixTypeConverter() )
            .AddPostfixUnaryOperator( "%", new MockPostfixUnaryOperator() )
            .AddPostfixTypeConverter( "ToString", new MockPostfixTypeConverter() )
            .SetBinaryOperatorPrecedence( "+", 1 )
            .SetPrefixUnaryConstructPrecedence( "-", 1 )
            .SetPrefixUnaryConstructPrecedence( "[string]", 1 )
            .SetPostfixUnaryConstructPrecedence( "%", 1 )
            .SetPostfixUnaryConstructPrecedence( "ToString", 1 );

        var sut = builder.Build();

        var expression = sut.Create<string, string>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke( argumentValues );

        result.Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( MathExpressionFactoryTestsData.GetMockedExpressionWithDifferencesInPrecedenceData ) )]
    public void DelegateInvoke_ShouldReturnCorrectResult_ForMockedExpressionWithDifferencesInPrecedence(string input, string expected)
    {
        var builder = new MathExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new MockBinaryOperator( "Add" ) )
            .AddBinaryOperator( "*", new MockBinaryOperator( "Mult" ) )
            .AddPrefixUnaryOperator( "-", new MockPrefixUnaryOperator( "Neg" ) )
            .AddPrefixUnaryOperator( "^", new MockPrefixUnaryOperator( "Caret" ) )
            .AddPrefixTypeConverter( "[string]", new MockPrefixTypeConverter() )
            .AddPostfixUnaryOperator( "%", new MockPostfixUnaryOperator( "Per" ) )
            .AddPostfixUnaryOperator( "!", new MockPostfixUnaryOperator( "Excl" ) )
            .AddPostfixTypeConverter( "ToString", new MockPostfixTypeConverter() )
            .SetBinaryOperatorPrecedence( "+", 2 )
            .SetBinaryOperatorPrecedence( "*", 1 )
            .SetPrefixUnaryConstructPrecedence( "-", 2 )
            .SetPrefixUnaryConstructPrecedence( "^", 1 )
            .SetPrefixUnaryConstructPrecedence( "[string]", 1 )
            .SetPostfixUnaryConstructPrecedence( "%", 2 )
            .SetPostfixUnaryConstructPrecedence( "!", 1 )
            .SetPostfixUnaryConstructPrecedence( "ToString", 1 );

        var sut = builder.Build();

        var expression = sut.Create<string, string>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke();

        result.Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( MathExpressionFactoryTestsData.GetMockedExpressionWithOperatorAmbiguityData ) )]
    public void DelegateInvoke_ShouldReturnCorrectResult_ForMockedExpressionWithOperatorAmbiguity(
        string input,
        string[] argumentValues,
        string expected)
    {
        var builder = new MathExpressionFactoryBuilder()
            .AddBinaryOperator( "/", new MockBinaryOperator( "Div" ) )
            .AddPrefixUnaryOperator( "^", new MockPrefixUnaryOperator( "Caret" ) )
            .AddPostfixUnaryOperator( "!", new MockPostfixUnaryOperator( "Excl" ) )
            .AddBinaryOperator( "+", new MockBinaryOperator( "Add" ) )
            .AddPostfixUnaryOperator( "+", new MockPostfixUnaryOperator( "Plus" ) )
            .AddBinaryOperator( "*", new MockBinaryOperator( "Mult" ) )
            .AddPrefixUnaryOperator( "*", new MockPrefixUnaryOperator( "Ref" ) )
            .AddPrefixUnaryOperator( "%", new MockPrefixUnaryOperator( "Per" ) )
            .AddPostfixUnaryOperator( "%", new MockPostfixUnaryOperator( "Per" ) )
            .AddBinaryOperator( "-", new MockBinaryOperator( "Sub" ) )
            .AddPrefixUnaryOperator( "-", new MockPrefixUnaryOperator( "Neg" ) )
            .AddPostfixUnaryOperator( "-", new MockPostfixUnaryOperator( "Neg" ) )
            .SetBinaryOperatorPrecedence( "/", 1 )
            .SetBinaryOperatorPrecedence( "+", 1 )
            .SetBinaryOperatorPrecedence( "*", 1 )
            .SetBinaryOperatorPrecedence( "-", 1 )
            .SetPrefixUnaryConstructPrecedence( "^", 1 )
            .SetPrefixUnaryConstructPrecedence( "*", 1 )
            .SetPrefixUnaryConstructPrecedence( "%", 1 )
            .SetPrefixUnaryConstructPrecedence( "-", 1 )
            .SetPostfixUnaryConstructPrecedence( "!", 1 )
            .SetPostfixUnaryConstructPrecedence( "+", 1 )
            .SetPostfixUnaryConstructPrecedence( "%", 1 )
            .SetPostfixUnaryConstructPrecedence( "-", 1 );

        var sut = builder.Build();

        var expression = sut.Create<string, string>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke( argumentValues );

        result.Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( MathExpressionFactoryTestsData.GetCorrectBinaryOperatorSpecializationsData ) )]
    public void DelegateInvoke_ShouldUseCorrectBinaryOperatorSpecializations(string input, string expected)
    {
        var builder = new MathExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new MockBinaryOperator() )
            .AddBinaryOperator( "+", new MockBinaryOperator<decimal, decimal>() )
            .AddBinaryOperator( "+", new MockBinaryOperator<decimal, string>() )
            .AddBinaryOperator( "+", new MockBinaryOperator<string, decimal>() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var sut = builder.Build();

        var expression = sut.Create<decimal, string>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke();

        result.Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( MathExpressionFactoryTestsData.GetCorrectPrefixUnaryOperatorSpecializationsData ) )]
    public void DelegateInvoke_ShouldUseCorrectPrefixUnaryOperatorSpecializations(string input, string expected)
    {
        var builder = new MathExpressionFactoryBuilder()
            .AddPrefixUnaryOperator( "-", new MockPrefixUnaryOperator() )
            .AddPrefixUnaryOperator( "-", new MockPrefixUnaryOperator<decimal>() )
            .SetPrefixUnaryConstructPrecedence( "-", 1 );

        var sut = builder.Build();

        var expression = sut.Create<decimal, string>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke();

        result.Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( MathExpressionFactoryTestsData.GetCorrectPrefixTypeConverterSpecializationsData ) )]
    public void DelegateInvoke_ShouldUseCorrectPrefixTypeConverterSpecializations(string input, string expected)
    {
        var builder = new MathExpressionFactoryBuilder()
            .AddPrefixTypeConverter( "[string]", new MockPrefixTypeConverter() )
            .AddPrefixTypeConverter( "[string]", new MockPrefixTypeConverter<decimal>() )
            .SetPrefixUnaryConstructPrecedence( "[string]", 1 );

        var sut = builder.Build();

        var expression = sut.Create<decimal, string>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke();

        result.Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( MathExpressionFactoryTestsData.GetCorrectPostfixUnaryOperatorSpecializationsData ) )]
    public void DelegateInvoke_ShouldUseCorrectPostfixUnaryOperatorSpecializations(string input, string expected)
    {
        var builder = new MathExpressionFactoryBuilder()
            .AddPostfixUnaryOperator( "^", new MockPostfixUnaryOperator() )
            .AddPostfixUnaryOperator( "^", new MockPostfixUnaryOperator<decimal>() )
            .SetPostfixUnaryConstructPrecedence( "^", 1 );

        var sut = builder.Build();

        var expression = sut.Create<decimal, string>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke();

        result.Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( MathExpressionFactoryTestsData.GetCorrectPostfixTypeConverterSpecializationsData ) )]
    public void DelegateInvoke_ShouldUseCorrectPostfixTypeConverterSpecializations(string input, string expected)
    {
        var builder = new MathExpressionFactoryBuilder()
            .AddPostfixTypeConverter( "ToString", new MockPostfixTypeConverter() )
            .AddPostfixTypeConverter( "ToString", new MockPostfixTypeConverter<decimal>() )
            .SetPostfixUnaryConstructPrecedence( "ToString", 1 );

        var sut = builder.Build();

        var expression = sut.Create<decimal, string>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke();

        result.Should().Be( expected );
    }

    [Fact]
    public void Create_ShouldThrowMathExpressionCreationException_WhenBinaryOperatorForArgumentTypesDoesNotExist()
    {
        var input = "12.34 + 'foo'";
        var builder = new MathExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new MockBinaryOperator<string, string>() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<MathExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, MathExpressionBuilderErrorType.BinaryOperatorDoesNotExist ) );
    }

    [Fact]
    public void Create_ShouldThrowMathExpressionCreationException_WhenPrefixUnaryOperatorForArgumentTypeDoesNotExist()
    {
        var input = "- 12.34";
        var builder = new MathExpressionFactoryBuilder()
            .AddPrefixUnaryOperator( "-", new MockPrefixUnaryOperator<string>() )
            .SetPrefixUnaryConstructPrecedence( "-", 1 );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<MathExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, MathExpressionBuilderErrorType.PrefixUnaryOperatorDoesNotExist ) );
    }

    [Fact]
    public void Create_ShouldThrowMathExpressionCreationException_WhenPrefixTypeConverterForArgumentTypeDoesNotExist()
    {
        var input = "[string] 12.34";
        var builder = new MathExpressionFactoryBuilder()
            .AddPrefixTypeConverter( "[string]", new MockPrefixTypeConverter<bool>() )
            .SetPrefixUnaryConstructPrecedence( "[string]", 1 );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<MathExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, MathExpressionBuilderErrorType.PrefixTypeConverterDoesNotExist ) );
    }

    [Fact]
    public void Create_ShouldThrowMathExpressionCreationException_WhenPostfixUnaryOperatorForArgumentTypeDoesNotExist()
    {
        var input = "12.34 ^";
        var builder = new MathExpressionFactoryBuilder()
            .AddPostfixUnaryOperator( "^", new MockPostfixUnaryOperator<string>() )
            .SetPostfixUnaryConstructPrecedence( "^", 1 );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<MathExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, MathExpressionBuilderErrorType.PostfixUnaryOperatorDoesNotExist ) );
    }

    [Fact]
    public void Create_ShouldThrowMathExpressionCreationException_WhenPostfixTypeConverterForArgumentTypeDoesNotExist()
    {
        var input = "12.34 ToString";
        var builder = new MathExpressionFactoryBuilder()
            .AddPostfixTypeConverter( "ToString", new MockPostfixTypeConverter<bool>() )
            .SetPostfixUnaryConstructPrecedence( "ToString", 1 );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<MathExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, MathExpressionBuilderErrorType.PostfixTypeConverterDoesNotExist ) );
    }

    [Fact]
    public void Create_ShouldThrowMathExpressionCreationException_WhenInputIsEmpty()
    {
        var input = string.Empty;
        var builder = new MathExpressionFactoryBuilder();
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<MathExpressionCreationException>()
            .AndMatch(
                e => MatchExpectations(
                    e,
                    input,
                    MathExpressionBuilderErrorType.ExpressionMustContainAtLeastOneOperand,
                    MathExpressionBuilderErrorType.ExpressionContainsInvalidOperandToOperatorRatio ) );
    }

    [Fact]
    public void Create_ShouldThrowMathExpressionCreationException_WhenOperandCountIsDifferentFromOperatorCountPlusOne()
    {
        var input = "a +";
        var builder = new MathExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new MockBinaryOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<MathExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, MathExpressionBuilderErrorType.ExpressionContainsInvalidOperandToOperatorRatio ) );
    }

    [Fact]
    public void Create_ShouldThrowMathExpressionCreationException_WhenSomeOpenedParenthesisAreUnclosed()
    {
        var input = "( ( a";
        var builder = new MathExpressionFactoryBuilder();
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<MathExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, MathExpressionBuilderErrorType.ExpressionContainsUnclosedParentheses ) );
    }

    [Fact]
    public void Create_ShouldThrowMathExpressionCreationException_WhenAmbiguousPostfixUnaryOperatorCannotBeProcessedAtTheEndOfInput()
    {
        var input = "a +";
        var builder = new MathExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new MockBinaryOperator() )
            .AddPostfixUnaryOperator( "+", new ThrowingUnaryOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 )
            .SetPostfixUnaryConstructPrecedence( "+", 1 );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<MathExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, MathExpressionBuilderErrorType.ConstructHasThrownException ) );
    }

    [Fact]
    public void Create_ShouldThrowMathExpressionCreationException_WhenPrefixUnaryOperatorCannotBeProcessedAtTheEndOfInput()
    {
        var input = "- a";
        var builder = new MathExpressionFactoryBuilder()
            .AddPrefixUnaryOperator( "-", new ThrowingUnaryOperator() )
            .SetPrefixUnaryConstructPrecedence( "-", 1 );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<MathExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, MathExpressionBuilderErrorType.ConstructHasThrownException ) );
    }

    [Fact]
    public void Create_ShouldThrowMathExpressionCreationException_WhenPrefixTypeConverterCannotBeProcessedAtTheEndOfInput()
    {
        var input = "[string] a";
        var builder = new MathExpressionFactoryBuilder()
            .AddPrefixTypeConverter( "[string]", new ThrowingTypeConverter() )
            .SetPrefixUnaryConstructPrecedence( "[string]", 1 );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<decimal, string>( input ) );

        action.Should()
            .ThrowExactly<MathExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, MathExpressionBuilderErrorType.ConstructHasThrownException ) );
    }

    [Fact]
    public void Create_ShouldAddAutomaticResultConversion_WhenActualResultTypeIsDifferentFromExpected_BasedOnPrefixTypeConverter()
    {
        var input = "12.34";
        var expected = "( PreDecimalCast|12.34 )";
        var builder = new MathExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new MockBinaryOperator() )
            .AddPrefixTypeConverter( "[int]", new MathExpressionTypeConverter<int>() )
            .AddPostfixTypeConverter( "ToInt", new MathExpressionTypeConverter<int>() )
            .AddPrefixTypeConverter( "[str]", new MockPrefixTypeConverter<int>() )
            .AddPrefixTypeConverter( "[string]", new MockPrefixTypeConverter<decimal>() )
            .SetBinaryOperatorPrecedence( "+", 1 )
            .SetPrefixUnaryConstructPrecedence( "[int]", 1 )
            .SetPostfixUnaryConstructPrecedence( "ToInt", 1 )
            .SetPrefixUnaryConstructPrecedence( "[str]", 1 )
            .SetPrefixUnaryConstructPrecedence( "[string]", 1 );

        var sut = builder.Build();

        var expression = sut.Create<string, string>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke();

        result.Should().Be( expected );
    }

    [Fact]
    public void Create_ShouldAddAutomaticResultConversion_WhenActualResultTypeIsDifferentFromExpected_BasedOnPostfixTypeConverter()
    {
        var input = "12.34";
        var expected = "( 12.34|PostDecimalCast )";
        var builder = new MathExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new MockBinaryOperator() )
            .AddPrefixTypeConverter( "[int]", new MathExpressionTypeConverter<int>() )
            .AddPostfixTypeConverter( "ToInt", new MathExpressionTypeConverter<int>() )
            .AddPostfixTypeConverter( "ToStr", new MockPostfixTypeConverter<int>() )
            .AddPostfixTypeConverter( "ToString", new MockPostfixTypeConverter<decimal>() )
            .SetBinaryOperatorPrecedence( "+", 1 )
            .SetPrefixUnaryConstructPrecedence( "[int]", 1 )
            .SetPostfixUnaryConstructPrecedence( "ToInt", 1 )
            .SetPostfixUnaryConstructPrecedence( "ToStr", 1 )
            .SetPostfixUnaryConstructPrecedence( "ToString", 1 );

        var sut = builder.Build();

        var expression = sut.Create<string, string>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke();

        result.Should().Be( expected );
    }

    [Fact]
    public void Create_ShouldThrowMathExpressionCreationException_WhenAutomaticResultConversionFailsDueToTypeConverterThrowingException()
    {
        var input = "12.34";
        var builder = new MathExpressionFactoryBuilder()
            .AddPrefixTypeConverter( "[string]", new ThrowingTypeConverter() )
            .SetPrefixUnaryConstructPrecedence( "[string]", 1 );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<MathExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, MathExpressionBuilderErrorType.OutputTypeConverterHasThrownException ) );
    }

    [Fact]
    public void
        Create_ShouldAddAutomaticResultConversion_WhenActualResultTypeIsDifferentFromExpectedAndTypeConverterDoesNotExistButResultIsAssignableToExpectedType()
    {
        var value = "foo";
        var input = $"'{value}'";
        var builder = new MathExpressionFactoryBuilder();
        var sut = builder.Build();

        var expression = sut.Create<string, object>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke();

        result.Should().Be( value );
    }

    [Fact]
    public void
        Create_ShouldAddAutomaticResultConversion_WhenActualResultTypeIsDifferentFromExpectedAndTypeConverterDoesNotExistButConversionOperatorExists()
    {
        var input = "12.34";
        var expected = 12.34;
        var builder = new MathExpressionFactoryBuilder();
        var sut = builder.Build();

        var expression = sut.Create<decimal, double>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke();

        result.Should().Be( expected );
    }

    [Fact]
    public void Create_ShouldThrowMathExpressionCreationException_WhenAutomaticResultConversionFailsDueToMissingConversionOperator()
    {
        var input = "12.34";
        var builder = new MathExpressionFactoryBuilder();
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<MathExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, MathExpressionBuilderErrorType.OutputTypeConverterHasThrownException ) );
    }

    [Fact]
    public void
        Create_ShouldAddAutomaticResultConversion_WhenActualResultTypeIsDifferentFromExpectedAndAutomaticResultConversionIsDisabledButResultIsAssignableToResultType()
    {
        var value = "foo";
        var input = $"'{value}'";
        var configuration = Substitute.For<IMathExpressionFactoryConfiguration>();
        configuration.DecimalPoint.Returns( '.' );
        configuration.IntegerDigitSeparator.Returns( '_' );
        configuration.StringDelimiter.Returns( '\'' );
        configuration.ScientificNotationExponents.Returns( "eE" );
        configuration.AllowScientificNotation.Returns( true );
        configuration.AllowNonIntegerNumbers.Returns( true );
        configuration.ConvertResultToOutputTypeAutomatically.Returns( false );
        var builder = new MathExpressionFactoryBuilder().SetConfiguration( configuration );
        var sut = builder.Build();

        var expression = sut.Create<string, object>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke();

        result.Should().Be( value );
    }

    [Fact]
    public void
        Create_ShouldThrowMathExpressionCreationException_WhenAutomaticResultConversionIsDisabledAndResultIsNotAssignableToResultType()
    {
        var input = "12.34";
        var configuration = Substitute.For<IMathExpressionFactoryConfiguration>();
        configuration.DecimalPoint.Returns( '.' );
        configuration.IntegerDigitSeparator.Returns( '_' );
        configuration.StringDelimiter.Returns( '\'' );
        configuration.ScientificNotationExponents.Returns( "eE" );
        configuration.AllowScientificNotation.Returns( true );
        configuration.AllowNonIntegerNumbers.Returns( true );
        configuration.ConvertResultToOutputTypeAutomatically.Returns( false );
        var builder = new MathExpressionFactoryBuilder().SetConfiguration( configuration );
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<decimal, int>( input ) );

        action.Should()
            .ThrowExactly<MathExpressionCreationException>()
            .AndMatch(
                e => MatchExpectations(
                    e,
                    input,
                    MathExpressionBuilderErrorType.ExpressionResultTypeIsNotCompatibleWithExpectedOutputType ) );
    }

    [Fact]
    public void IMathExpressionFactoryCreate_ShouldBeEquivalentToCreate()
    {
        var input = "a+b+c";
        var values = new[] { 1m, 2m, 3m };

        var builder = new MathExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new MathExpressionAddOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var factory = builder.Build();
        IMathExpressionFactory sut = factory;

        var factoryDelegate = factory.Create<decimal, decimal>( input ).Compile();
        var @delegate = sut.Create<decimal, decimal>( input ).Compile();
        var expected = factoryDelegate.Invoke( values );
        var actual = @delegate.Invoke( values );

        actual.Should().Be( expected );
    }

    [Fact]
    public void IMathExpressionFactoryCreate_ShouldBeEquivalentToCreate_WhenErrorsOccur()
    {
        var input = "a+b+c";
        var builder = new MathExpressionFactoryBuilder();
        IMathExpressionFactory sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<decimal, decimal>( input ) );

        action.Should().ThrowExactly<MathExpressionCreationException>();
    }
}
