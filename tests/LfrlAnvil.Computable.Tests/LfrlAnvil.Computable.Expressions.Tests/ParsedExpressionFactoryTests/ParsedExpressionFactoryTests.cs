using System.Linq;
using FluentAssertions.Execution;
using LfrlAnvil.Computable.Expressions.Constructs;
using LfrlAnvil.Computable.Expressions.Errors;
using LfrlAnvil.Computable.Expressions.Exceptions;
using LfrlAnvil.Computable.Expressions.Internal;
using LfrlAnvil.Functional;
using LfrlAnvil.TestExtensions.Attributes;
using LfrlAnvil.TestExtensions.FluentAssertions;
using LfrlAnvil.TestExtensions.NSubstitute;

namespace LfrlAnvil.Computable.Expressions.Tests.ParsedExpressionFactoryTests;

[TestClass( typeof( ParsedExpressionFactoryTestsData ) )]
public partial class ParsedExpressionFactoryTests : TestsBase
{
    [Fact]
    public void Create_ShouldProvideCorrectParamsToNumberParserProvider()
    {
        var provider = Substitute.For<Func<ParsedExpressionNumberParserParams, IParsedExpressionNumberParser>>();
        provider.WithAnyArgs(
            c => ParsedExpressionNumberParser.CreateDefaultDecimal( c.ArgAt<ParsedExpressionNumberParserParams>( 0 ).Configuration ) );

        var builder = new ParsedExpressionFactoryBuilder()
            .AddPrefixTypeConverter( "[string]", new MockPrefixTypeConverter() )
            .SetNumberParserProvider( provider )
            .SetPrefixUnaryConstructPrecedence( "[string]", 1 );

        var sut = builder.Build();

        var _ = sut.Create<decimal, string>( "[string] 0" );

        using ( new AssertionScope() )
        {
            var @params = (ParsedExpressionNumberParserParams)provider.Verify().CallAt( 0 ).Exists().And.Arguments[0]!;
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
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new ParsedExpressionAddOperator() )
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

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenUnexpectedExceptionIsThrown()
    {
        var input = "a";
        var exception = new Exception();
        var builder = new ParsedExpressionFactoryBuilder().SetNumberParserProvider( _ => throw exception );
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<decimal, decimal>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.Error ) );
    }

    [Theory]
    [InlineData( "'foobar'" )]
    [InlineData( "( 'foobar' )" )]
    [InlineData( "( ( ( 'foobar' ) ) )" )]
    public void DelegateInvoke_ShouldReturnCorrectResult_WhenExpressionConsistsOfOnlyStringConstant(string input)
    {
        var builder = new ParsedExpressionFactoryBuilder();
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
        var builder = new ParsedExpressionFactoryBuilder();
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
    public void DelegateInvoke_ShouldReturnCorrectResult_WhenExpressionConsistsOfOnlyBooleanConstant(string input, bool expected)
    {
        var builder = new ParsedExpressionFactoryBuilder();
        var sut = builder.Build();

        var expression = sut.Create<bool, bool>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke();

        result.Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( ParsedExpressionFactoryTestsData.GetArgumentOnlyData ) )]
    public void DelegateInvoke_ShouldReturnCorrectResult_WhenExpressionConsistsOfOnlyArgumentSymbol(string input, decimal value)
    {
        var builder = new ParsedExpressionFactoryBuilder();
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

    [Theory]
    [InlineData( "Zero" )]
    [InlineData( "( Zero )" )]
    [InlineData( "( ( ( Zero ) ) )" )]
    public void DelegateInvoke_ShouldReturnCorrectResult_WhenExpressionConsistsOfOnlyCustomConstant(string input)
    {
        var builder = new ParsedExpressionFactoryBuilder().AddConstant( "Zero", new ZeroConstant() );
        var sut = builder.Build();

        var expression = sut.Create<string, string>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke();

        result.Should().Be( "ZERO" );
    }

    [Theory]
    [InlineData( "foo()" )]
    [InlineData( "( foo() )" )]
    [InlineData( "( ( ( foo() ) ) )" )]
    public void DelegateInvoke_ShouldReturnCorrectResult_WhenExpressionConsistsOfOnlyParameterlessFunction(string input)
    {
        var builder = new ParsedExpressionFactoryBuilder().AddFunction( "foo", new MockParameterlessFunction() );
        var sut = builder.Build();

        var expression = sut.Create<string, string>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke();

        result.Should().Be( "Func()" );
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenStringConstantIsTheLastTokenAndDoesNotHaveClosingDelimiter()
    {
        var input = "'foobar";
        var builder = new ParsedExpressionFactoryBuilder();
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.StringConstantParsingFailure ) );
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenNumberParserFailedToParseNumberConstant()
    {
        var input = "12.34";
        var builder = new ParsedExpressionFactoryBuilder().SetNumberParserProvider( _ => new FailingNumberParser() );
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<decimal, decimal>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.NumberConstantParsingFailure ) );
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenArgumentHasInvalidName()
    {
        var input = "/";
        var builder = new ParsedExpressionFactoryBuilder();
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<decimal, decimal>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.InvalidArgumentName ) );
    }

    [Theory]
    [MethodData( nameof( ParsedExpressionFactoryTestsData.GetOperandFollowedByOperandData ) )]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenOperandIsFollowedByOperand(string input)
    {
        var builder = new ParsedExpressionFactoryBuilder()
            .AddConstant( "Zero", new ZeroConstant() )
            .AddFunction( "foo", new MockParameterlessFunction() );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.UnexpectedOperand ) );
    }

    [Theory]
    [MethodData( nameof( ParsedExpressionFactoryTestsData.GetOperandFollowedByFunctionData ) )]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenOperandIsFollowedByFunction(string input)
    {
        var builder = new ParsedExpressionFactoryBuilder()
            .AddConstant( "Zero", new ZeroConstant() )
            .AddFunction( "foo", new MockParameterlessFunction() );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.UnexpectedFunctionCall ) );
    }

    [Theory]
    [MethodData( nameof( ParsedExpressionFactoryTestsData.GetOperandFollowedByPrefixUnaryOperatorData ) )]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenOperandIsFollowedByPrefixUnaryOperator(string input)
    {
        var builder = new ParsedExpressionFactoryBuilder()
            .AddConstant( "Zero", new ZeroConstant() )
            .AddFunction( "foo", new MockParameterlessFunction() )
            .AddPrefixUnaryOperator( "-", new MockPrefixUnaryOperator() )
            .SetPrefixUnaryConstructPrecedence( "-", 1 );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ExpectedPostfixUnaryOrBinaryConstruct ) );
    }

    [Theory]
    [MethodData( nameof( ParsedExpressionFactoryTestsData.GetOperandFollowedByPrefixTypeConverterData ) )]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenOperandIsFollowedByPrefixTypeConverter(string input)
    {
        var builder = new ParsedExpressionFactoryBuilder()
            .AddConstant( "Zero", new ZeroConstant() )
            .AddFunction( "foo", new MockParameterlessFunction() )
            .AddPrefixTypeConverter( "[string]", new MockPrefixTypeConverter() )
            .SetPrefixUnaryConstructPrecedence( "[string]", 1 );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ExpectedPostfixUnaryOrBinaryConstruct ) );
    }

    [Theory]
    [MethodData( nameof( ParsedExpressionFactoryTestsData.GetOperandFollowedByOpenParenthesisData ) )]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenOperandIsFollowedByOpenedParenthesis(string input)
    {
        var builder = new ParsedExpressionFactoryBuilder()
            .AddConstant( "Zero", new ZeroConstant() )
            .AddFunction( "foo", new MockParameterlessFunction() );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<decimal, decimal>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.UnexpectedOpenedParenthesis ) );
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenPostfixUnaryOperatorIsFollowedByOpenedParenthesis()
    {
        var input = "a ^ (";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddPostfixUnaryOperator( "^", new MockPostfixUnaryOperator() )
            .SetPostfixUnaryConstructPrecedence( "^", 1 );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.UnexpectedOpenedParenthesis ) );
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenPostfixTypeConverterIsFollowedByOpenedParenthesis()
    {
        var input = "a ToString (";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddPostfixTypeConverter( "ToString", new MockPostfixTypeConverter() )
            .SetPostfixUnaryConstructPrecedence( "ToString", 1 );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<decimal, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.UnexpectedOpenedParenthesis ) );
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenClosedParenthesisIsFollowedByOpenedParenthesis()
    {
        var input = "( a ) (";
        var builder = new ParsedExpressionFactoryBuilder();
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<decimal, decimal>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.UnexpectedOpenedParenthesis ) );
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenOpenedParenthesisIsFollowedByClosedParenthesis()
    {
        var input = "( )";
        var builder = new ParsedExpressionFactoryBuilder();
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<decimal, decimal>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.UnexpectedClosedParenthesis ) );
    }

    [Fact]
    public void
        Create_ShouldThrowParsedExpressionCreationException_WhenAmbiguousBinaryOperatorCannotBeProcessedDuringOpenedParenthesisHandling()
    {
        var input = "a + ( b )";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new ThrowingBinaryOperator() )
            .AddPostfixUnaryOperator( "+", new MockPostfixUnaryOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 )
            .SetPostfixUnaryConstructPrecedence( "+", 1 );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) );
    }

    [Fact]
    public void
        Create_ShouldThrowParsedExpressionCreationException_WhenAmbiguousPostfixUnaryOperatorCannotBeProcessedDuringOpenedParenthesisHandling()
    {
        var input = "a + + ( b )";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new MockBinaryOperator() )
            .AddPostfixUnaryOperator( "+", new ThrowingUnaryOperator() )
            .AddPrefixUnaryOperator( "+", new MockPrefixUnaryOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 )
            .SetPostfixUnaryConstructPrecedence( "+", 1 )
            .SetPrefixUnaryConstructPrecedence( "+", 1 );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) );
    }

    [Theory]
    [MethodData( nameof( ParsedExpressionFactoryTestsData.GetClosedParenthesisDoesNotHaveCorrespondingOpenParenthesisData ) )]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenClosedParenthesisDoesNotHaveCorrespondingOpenedParenthesis(
        string input)
    {
        var builder = new ParsedExpressionFactoryBuilder()
            .AddConstant( "Zero", new ZeroConstant() )
            .AddFunction( "foo", new MockParameterlessFunction() )
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
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.UnexpectedClosedParenthesis ) );
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenPrefixUnaryOperatorIsFollowedByClosedParenthesis()
    {
        var input = "( - )";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddPrefixUnaryOperator( "-", new MockPrefixUnaryOperator() )
            .SetPrefixUnaryConstructPrecedence( "-", 1 );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.UnexpectedClosedParenthesis ) );
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenPrefixTypeConverterIsFollowedByClosedParenthesis()
    {
        var input = "( [string] )";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddPrefixTypeConverter( "[string]", new MockPrefixTypeConverter() )
            .SetPrefixUnaryConstructPrecedence( "[string]", 1 );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.UnexpectedClosedParenthesis ) );
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenBinaryOperatorIsFollowedByClosedParenthesis()
    {
        var input = "( a + )";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new MockBinaryOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.UnexpectedClosedParenthesis ) );
    }

    [Fact]
    public void
        Create_ShouldThrowParsedExpressionCreationException_WhenAmbiguousPostfixUnaryOperatorCannotBeProcessedDuringClosedParenthesisHandling()
    {
        var input = "( a + )";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new MockBinaryOperator() )
            .AddPostfixUnaryOperator( "+", new ThrowingUnaryOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 )
            .SetPostfixUnaryConstructPrecedence( "+", 1 );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) );
    }

    [Fact]
    public void
        Create_ShouldThrowParsedExpressionCreationException_WhenPostfixUnaryConstructAmbiguityCannotBeResolvedDueToBinaryOperatorAmbiguity()
    {
        var input = "( a + + )";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new MockBinaryOperator() )
            .AddPostfixUnaryOperator( "+", new MockPostfixUnaryOperator() )
            .AddPrefixUnaryOperator( "+", new MockPrefixUnaryOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 )
            .SetPostfixUnaryConstructPrecedence( "+", 1 )
            .SetPrefixUnaryConstructPrecedence( "+", 1 );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch(
                e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.AmbiguousPostfixUnaryConstructResolutionFailure ) );
    }

    [Fact]
    public void
        Create_ShouldThrowParsedExpressionCreationException_WhenPrefixUnaryConstructCannotBeProcessedDuringClosedParenthesisHandling()
    {
        var input = "( - a )";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddPrefixUnaryOperator( "-", new ThrowingUnaryOperator() )
            .SetPrefixUnaryConstructPrecedence( "-", 1 );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) );
    }

    [Fact]
    public void
        Create_ShouldThrowParsedExpressionCreationException_WhenPrefixTypeConverterCannotBeProcessedDuringClosedParenthesisHandling()
    {
        var input = "( [string] a )";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddPrefixTypeConverter( "[string]", new ThrowingTypeConverter() )
            .SetPrefixUnaryConstructPrecedence( "[string]", 1 );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<decimal, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) );
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenBinaryOperatorCannotBeProcessedDuringClosedParenthesisHandling()
    {
        var input = "( a + b )";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new ThrowingBinaryOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) );
    }

    [Fact]
    public void
        Create_ShouldThrowParsedExpressionCreationException_WhenAmbiguousPostfixUnaryOperatorCannotBeProcessedDuringPrefixUnaryOperatorHandling()
    {
        var input = "a + * * b";
        var builder = new ParsedExpressionFactoryBuilder()
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
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) );
    }

    [Fact]
    public void
        Create_ShouldThrowParsedExpressionCreationException_WhenAmbiguousBinaryOperatorCannotBeProcessedDuringPrefixUnaryOperatorHandling()
    {
        var input = "a + b + * c";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new ThrowingBinaryOperator() )
            .AddPostfixUnaryOperator( "+", new MockPostfixUnaryOperator() )
            .AddPrefixUnaryOperator( "*", new MockPrefixUnaryOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 )
            .SetPostfixUnaryConstructPrecedence( "+", 1 )
            .SetPrefixUnaryConstructPrecedence( "*", 1 );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) );
    }

    [Fact]
    public void
        Create_ShouldThrowParsedExpressionCreationException_WhenAmbiguousPostfixUnaryOperatorCannotBeProcessedDuringPrefixTypeConverterHandling()
    {
        var input = "a + * [string] b";
        var builder = new ParsedExpressionFactoryBuilder()
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
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) );
    }

    [Fact]
    public void
        Create_ShouldThrowParsedExpressionCreationException_WhenAmbiguousBinaryOperatorCannotBeProcessedDuringPrefixTypeConverterHandling()
    {
        var input = "a + b + [string] c";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new ThrowingBinaryOperator() )
            .AddPostfixUnaryOperator( "+", new MockPostfixUnaryOperator() )
            .AddPrefixTypeConverter( "[string]", new MockPrefixTypeConverter() )
            .SetBinaryOperatorPrecedence( "+", 1 )
            .SetPostfixUnaryConstructPrecedence( "+", 1 )
            .SetPrefixUnaryConstructPrecedence( "[string]", 1 );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<decimal, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) );
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenAmbiguousPostfixUnaryOperatorIsFollowedByPostfixUnaryOperator()
    {
        var input = "a + * b";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new MockBinaryOperator() )
            .AddPostfixUnaryOperator( "+", new MockPostfixUnaryOperator() )
            .AddPostfixUnaryOperator( "*", new MockPrefixUnaryOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 )
            .SetPostfixUnaryConstructPrecedence( "+", 1 )
            .SetPostfixUnaryConstructPrecedence( "*", 1 );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<decimal, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ExpectedBinaryOrPrefixUnaryConstruct ) );
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenAmbiguousPostfixUnaryOperatorIsFollowedByPostfixTypeConverter()
    {
        var input = "a + ToString b";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new MockBinaryOperator() )
            .AddPostfixUnaryOperator( "+", new MockPostfixUnaryOperator() )
            .AddPostfixTypeConverter( "ToString", new MockPostfixTypeConverter() )
            .SetBinaryOperatorPrecedence( "+", 1 )
            .SetPostfixUnaryConstructPrecedence( "+", 1 )
            .SetPostfixUnaryConstructPrecedence( "ToString", 1 );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<decimal, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ExpectedBinaryOrPrefixUnaryConstruct ) );
    }

    [Theory]
    [MethodData( nameof( ParsedExpressionFactoryTestsData.GetPrefixUnaryConstructIsFollowedByAnotherConstructData ) )]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenPrefixUnaryConstructIsFollowedByAnotherConstruct(string input)
    {
        var builder = new ParsedExpressionFactoryBuilder()
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
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.UnexpectedConstruct ) );
    }

    [Fact]
    public void
        Create_ShouldThrowParsedExpressionCreationException_WhenAmbiguousPostfixUnaryOperatorCannotBeProcessedDuringBinaryOperatorHandling()
    {
        var input = "a + * b";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new MockBinaryOperator() )
            .AddBinaryOperator( "*", new MockBinaryOperator() )
            .AddPostfixUnaryOperator( "+", new ThrowingUnaryOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 )
            .SetBinaryOperatorPrecedence( "*", 1 )
            .SetPostfixUnaryConstructPrecedence( "+", 1 );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) );
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenPrefixUnaryOperatorThrowsExceptionDuringBinaryOperatorHandling()
    {
        var input = "- a + b";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddPrefixUnaryOperator( "-", new ThrowingUnaryOperator() )
            .AddBinaryOperator( "+", new MockBinaryOperator() )
            .SetPrefixUnaryConstructPrecedence( "-", 1 )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) );
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenPrefixTypeConverterThrowsExceptionDuringBinaryOperatorHandling()
    {
        var input = "[string] a + b";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddPrefixTypeConverter( "[string]", new ThrowingTypeConverter() )
            .AddBinaryOperator( "+", new MockBinaryOperator() )
            .SetPrefixUnaryConstructPrecedence( "[string]", 1 )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<decimal, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) );
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenBinaryOperatorThrowsException()
    {
        var input = "a + b";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new ThrowingBinaryOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<decimal, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) );
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenPostfixUnaryOperatorThrowsException()
    {
        var input = "a ^";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddPostfixUnaryOperator( "^", new ThrowingUnaryOperator() )
            .SetPostfixUnaryConstructPrecedence( "^", 1 );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) );
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenPostfixTypeConverterThrowsException()
    {
        var input = "a ToString";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddPostfixTypeConverter( "ToString", new ThrowingTypeConverter() )
            .SetPostfixUnaryConstructPrecedence( "ToString", 1 );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<decimal, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) );
    }

    [Theory]
    [InlineData( 1, 1 )]
    [InlineData( 1, 2 )]
    [InlineData( 2, 1 )]
    public void
        Create_ShouldThrowParsedExpressionCreationException_WhenPostfixUnaryOperatorThrowsExceptionDuringPrefixUnaryOperatorResolution(
            int prefixPrecedence,
            int postfixPrecedence)
    {
        var input = "- a ^";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddPostfixUnaryOperator( "^", new ThrowingUnaryOperator() )
            .AddPrefixUnaryOperator( "-", new MockPrefixUnaryOperator() )
            .SetPostfixUnaryConstructPrecedence( "^", postfixPrecedence )
            .SetPrefixUnaryConstructPrecedence( "-", prefixPrecedence );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<decimal, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) );
    }

    [Theory]
    [InlineData( 1, 1 )]
    [InlineData( 1, 2 )]
    [InlineData( 2, 1 )]
    public void
        Create_ShouldThrowParsedExpressionCreationException_WhenPostfixUnaryOperatorThrowsExceptionDuringPrefixTypeConverterResolution(
            int prefixPrecedence,
            int postfixPrecedence)
    {
        var input = "[string] a ^";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddPostfixUnaryOperator( "^", new ThrowingUnaryOperator() )
            .AddPrefixTypeConverter( "[string]", new MockPrefixTypeConverter() )
            .SetPostfixUnaryConstructPrecedence( "^", postfixPrecedence )
            .SetPrefixUnaryConstructPrecedence( "[string]", prefixPrecedence );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<decimal, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) );
    }

    [Fact]
    public void
        Create_ShouldThrowParsedExpressionCreationException_WhenPostfixTypeConverterThrowsExceptionDuringPrefixUnaryOperatorResolution()
    {
        var input = "- a ToString";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddPostfixTypeConverter( "ToString", new ThrowingTypeConverter() )
            .AddPrefixUnaryOperator( "-", new MockPrefixUnaryOperator() )
            .SetPostfixUnaryConstructPrecedence( "ToString", 1 )
            .SetPrefixUnaryConstructPrecedence( "-", 2 );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<decimal, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) );
    }

    [Theory]
    [InlineData( 1, 1 )]
    [InlineData( 1, 2 )]
    [InlineData( 2, 1 )]
    public void
        Create_ShouldThrowParsedExpressionCreationException_WhenPrefixUnaryOperatorThrowsExceptionDuringItsResolutionWithPostfixUnaryOperator(
            int prefixPrecedence,
            int postfixPrecedence)
    {
        var input = "- a ^";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddPostfixUnaryOperator( "^", new MockPostfixUnaryOperator() )
            .AddPrefixUnaryOperator( "-", new ThrowingUnaryOperator() )
            .SetPostfixUnaryConstructPrecedence( "^", postfixPrecedence )
            .SetPrefixUnaryConstructPrecedence( "-", prefixPrecedence );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<decimal, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) );
    }

    [Theory]
    [InlineData( 1, 1 )]
    [InlineData( 1, 2 )]
    public void
        Create_ShouldThrowParsedExpressionCreationException_WhenPrefixTypeConverterThrowsExceptionDuringItsResolutionWithPostfixUnaryOperator(
            int prefixPrecedence,
            int postfixPrecedence)
    {
        var input = "[string] a ^";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddPostfixUnaryOperator( "^", new MockPostfixUnaryOperator() )
            .AddPrefixTypeConverter( "[string]", new ThrowingTypeConverter() )
            .SetPostfixUnaryConstructPrecedence( "^", postfixPrecedence )
            .SetPrefixUnaryConstructPrecedence( "[string]", prefixPrecedence );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<decimal, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) );
    }

    [Theory]
    [InlineData( 1, 1 )]
    [InlineData( 1, 2 )]
    public void
        Create_ShouldThrowParsedExpressionCreationException_WhenPrefixUnaryOperatorThrowsExceptionDuringItsResolutionWithPostfixTypeConverter(
            int prefixPrecedence,
            int postfixPrecedence)
    {
        var input = "- a ToString";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddPostfixTypeConverter( "ToString", new MockPostfixTypeConverter() )
            .AddPrefixUnaryOperator( "-", new ThrowingUnaryOperator() )
            .SetPostfixUnaryConstructPrecedence( "ToString", postfixPrecedence )
            .SetPrefixUnaryConstructPrecedence( "-", prefixPrecedence );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<decimal, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) );
    }

    [Fact]
    public void
        Create_ShouldThrowParsedExpressionCreationException_WhenBinaryOperatorCollectionIsEmptyAfterNonAmbiguousPostfixUnaryOperatorHandling()
    {
        var input = "a ^ + b";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddPostfixUnaryOperator( "^", new MockPostfixUnaryOperator() )
            .AddPrefixUnaryOperator( "+", new MockPrefixUnaryOperator() )
            .SetPostfixUnaryConstructPrecedence( "^", 1 )
            .SetPrefixUnaryConstructPrecedence( "+", 1 );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ExpectedBinaryOperator ) );
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenBinaryOperatorCollectionIsEmptyAfterPostfixTypeConverterHandling()
    {
        var input = "a ToString + b";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddPostfixTypeConverter( "ToString", new MockPostfixTypeConverter() )
            .AddPrefixUnaryOperator( "+", new MockPrefixUnaryOperator() )
            .SetPostfixUnaryConstructPrecedence( "ToString", 1 )
            .SetPrefixUnaryConstructPrecedence( "+", 1 );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ExpectedBinaryOperator ) );
    }

    [Theory]
    [InlineData( 1, 1 )]
    [InlineData( 1, 2 )]
    public void
        Create_ShouldThrowParsedExpressionCreationException_WhenBinaryOperatorCannotBeProcessedDuringBinaryOperatorWithGreaterOrEqualPrecedenceHandling(
            int firstOperatorPrecedence,
            int secondOperatorPrecedence)
    {
        var input = "a * b + c + d";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new ThrowingBinaryOperator() )
            .AddBinaryOperator( "*", new MockBinaryOperator() )
            .SetBinaryOperatorPrecedence( "+", secondOperatorPrecedence )
            .SetBinaryOperatorPrecedence( "*", firstOperatorPrecedence );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) );
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenPrefixUnaryOperatorCollectionIsEmptyAtTheStart()
    {
        var input = "+ a + b";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new MockBinaryOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ExpectedPrefixUnaryConstruct ) );
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenPrefixUnaryOperatorCollectionIsEmptyAfterBinaryOperatorHandling()
    {
        var input = "a * + b";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "*", new MockBinaryOperator() )
            .AddBinaryOperator( "+", new MockBinaryOperator() )
            .SetBinaryOperatorPrecedence( "*", 1 )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ExpectedPrefixUnaryConstruct ) );
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenPrefixTypeConverterCollectionIsEmptyAtTheStart()
    {
        var input = "[string] a + b";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddPostfixTypeConverter( "[string]", new MockPostfixTypeConverter() )
            .AddBinaryOperator( "+", new MockBinaryOperator() )
            .SetPostfixUnaryConstructPrecedence( "[string]", 1 )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<decimal, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ExpectedPrefixUnaryConstruct ) );
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenPrefixTypeConverterCollectionIsEmptyAfterBinaryOperatorHandling()
    {
        var input = "a + [string] b";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddPostfixTypeConverter( "[string]", new MockPostfixTypeConverter() )
            .AddBinaryOperator( "+", new MockBinaryOperator() )
            .SetPostfixUnaryConstructPrecedence( "[string]", 1 )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<decimal, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ExpectedPrefixUnaryConstruct ) );
    }

    [Theory]
    [MethodData( nameof( ParsedExpressionFactoryTestsData.GetMockedSimpleExpressionData ) )]
    public void DelegateInvoke_ShouldReturnCorrectResult_ForMockedSimpleExpression(
        string input,
        string[] argumentValues,
        string expected)
    {
        var builder = new ParsedExpressionFactoryBuilder()
            .AddConstant( "Zero", new ZeroConstant() )
            .AddFunction( "foo", new MockParameterlessFunction() )
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
    [MethodData( nameof( ParsedExpressionFactoryTestsData.GetMockedExpressionWithDifferencesInPrecedenceData ) )]
    public void DelegateInvoke_ShouldReturnCorrectResult_ForMockedExpressionWithDifferencesInPrecedence(string input, string expected)
    {
        var builder = new ParsedExpressionFactoryBuilder()
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
    [MethodData( nameof( ParsedExpressionFactoryTestsData.GetMockedExpressionWithOperatorAmbiguityData ) )]
    public void DelegateInvoke_ShouldReturnCorrectResult_ForMockedExpressionWithOperatorAmbiguity(
        string input,
        string[] argumentValues,
        string expected)
    {
        var builder = new ParsedExpressionFactoryBuilder()
            .AddConstant( "Zero", new ZeroConstant() )
            .AddFunction( "foo", new MockParameterlessFunction() )
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
    [MethodData( nameof( ParsedExpressionFactoryTestsData.GetCorrectBinaryOperatorSpecializationsData ) )]
    public void DelegateInvoke_ShouldUseCorrectBinaryOperatorSpecializations(string input, string expected)
    {
        var builder = new ParsedExpressionFactoryBuilder()
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
    [MethodData( nameof( ParsedExpressionFactoryTestsData.GetCorrectPrefixUnaryOperatorSpecializationsData ) )]
    public void DelegateInvoke_ShouldUseCorrectPrefixUnaryOperatorSpecializations(string input, string expected)
    {
        var builder = new ParsedExpressionFactoryBuilder()
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
    [MethodData( nameof( ParsedExpressionFactoryTestsData.GetCorrectPrefixTypeConverterSpecializationsData ) )]
    public void DelegateInvoke_ShouldUseCorrectPrefixTypeConverterSpecializations(string input, string expected)
    {
        var builder = new ParsedExpressionFactoryBuilder()
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
    [MethodData( nameof( ParsedExpressionFactoryTestsData.GetCorrectPostfixUnaryOperatorSpecializationsData ) )]
    public void DelegateInvoke_ShouldUseCorrectPostfixUnaryOperatorSpecializations(string input, string expected)
    {
        var builder = new ParsedExpressionFactoryBuilder()
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
    [MethodData( nameof( ParsedExpressionFactoryTestsData.GetCorrectPostfixTypeConverterSpecializationsData ) )]
    public void DelegateInvoke_ShouldUseCorrectPostfixTypeConverterSpecializations(string input, string expected)
    {
        var builder = new ParsedExpressionFactoryBuilder()
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
    public void Create_ShouldThrowParsedExpressionCreationException_WhenBinaryOperatorForArgumentTypesDoesNotExist()
    {
        var input = "12.34 + 'foo'";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new MockBinaryOperator<string, string>() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.BinaryOperatorCouldNotBeResolved ) );
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenPrefixUnaryOperatorForArgumentTypeDoesNotExist()
    {
        var input = "- 12.34";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddPrefixUnaryOperator( "-", new MockPrefixUnaryOperator<string>() )
            .SetPrefixUnaryConstructPrecedence( "-", 1 );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.PrefixUnaryOperatorCouldNotBeResolved ) );
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenPrefixTypeConverterForArgumentTypeDoesNotExist()
    {
        var input = "[string] 12.34";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddPrefixTypeConverter( "[string]", new MockPrefixTypeConverter<bool>() )
            .SetPrefixUnaryConstructPrecedence( "[string]", 1 );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.PrefixTypeConverterCouldNotBeResolved ) );
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenPostfixUnaryOperatorForArgumentTypeDoesNotExist()
    {
        var input = "12.34 ^";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddPostfixUnaryOperator( "^", new MockPostfixUnaryOperator<string>() )
            .SetPostfixUnaryConstructPrecedence( "^", 1 );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.PostfixUnaryOperatorCouldNotBeResolved ) );
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenPostfixTypeConverterForArgumentTypeDoesNotExist()
    {
        var input = "12.34 ToString";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddPostfixTypeConverter( "ToString", new MockPostfixTypeConverter<bool>() )
            .SetPostfixUnaryConstructPrecedence( "ToString", 1 );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.PostfixTypeConverterCouldNotBeResolved ) );
    }

    [Theory]
    [MethodData( nameof( ParsedExpressionFactoryTestsData.GetTypeDeclarationIsUsedOutsideOfDelegateParametersDefinitionData ) )]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenTypeDeclarationIsUsedOutsideOfDelegateParametersDefinition(
        string input)
    {
        var builder = new ParsedExpressionFactoryBuilder()
            .AddTypeDeclaration<int>( "int" )
            .AddConstant( "Zero", new ZeroConstant() )
            .AddFunction( "foo", new MockParameterlessFunction() )
            .AddBinaryOperator( "+", new MockBinaryOperator() )
            .AddPrefixUnaryOperator( "-", new MockPrefixUnaryOperator() )
            .AddPostfixUnaryOperator( "^", new MockPostfixUnaryOperator() )
            .AddPrefixTypeConverter( "[string]", new MockPrefixTypeConverter() )
            .AddPostfixTypeConverter( "ToString", new MockPostfixTypeConverter() )
            .SetBinaryOperatorPrecedence( "+", 1 )
            .SetPrefixUnaryConstructPrecedence( "-", 1 )
            .SetPrefixUnaryConstructPrecedence( "[string]", 1 )
            .SetPostfixUnaryConstructPrecedence( "^", 1 )
            .SetPostfixUnaryConstructPrecedence( "ToString", 1 );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.UnexpectedTypeDeclaration ) );
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenInputIsEmpty()
    {
        var input = string.Empty;
        var builder = new ParsedExpressionFactoryBuilder();
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch(
                e => MatchExpectations(
                    e,
                    input,
                    ParsedExpressionBuilderErrorType.ExpressionMustContainAtLeastOneOperand,
                    ParsedExpressionBuilderErrorType.ExpressionContainsInvalidOperandToOperatorRatio ) );
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenOperandCountIsDifferentFromOperatorCountPlusOne()
    {
        var input = "a +";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new MockBinaryOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch(
                e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ExpressionContainsInvalidOperandToOperatorRatio ) );
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenSomeOpenedParenthesisAreUnclosed()
    {
        var input = "( ( a";
        var builder = new ParsedExpressionFactoryBuilder();
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ExpressionContainsUnclosedParentheses ) );
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenAmbiguousPostfixUnaryOperatorCannotBeProcessedAtTheEndOfInput()
    {
        var input = "a +";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new MockBinaryOperator() )
            .AddPostfixUnaryOperator( "+", new ThrowingUnaryOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 )
            .SetPostfixUnaryConstructPrecedence( "+", 1 );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) );
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenPrefixUnaryOperatorCannotBeProcessedAtTheEndOfInput()
    {
        var input = "- a";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddPrefixUnaryOperator( "-", new ThrowingUnaryOperator() )
            .SetPrefixUnaryConstructPrecedence( "-", 1 );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) );
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenPrefixTypeConverterCannotBeProcessedAtTheEndOfInput()
    {
        var input = "[string] a";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddPrefixTypeConverter( "[string]", new ThrowingTypeConverter() )
            .SetPrefixUnaryConstructPrecedence( "[string]", 1 );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<decimal, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) );
    }

    [Theory]
    [InlineData( "," )]
    [InlineData( "a , b" )]
    [InlineData( "( a , b )" )]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenElementSeparatorIsFoundWhenFunctionParametersAreNotBeingParsed(
        string input)
    {
        var builder = new ParsedExpressionFactoryBuilder();
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.UnexpectedElementSeparator ) );
    }

    [Theory]
    [InlineData( "foo)" )]
    [InlineData( "foo( a , )" )]
    [InlineData( "foo( a , - )" )]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenClosedParenthesisIsTooSoonDuringFunctionParametersParsing(
        string input)
    {
        var builder = new ParsedExpressionFactoryBuilder()
            .AddFunction( "foo", new MockParameterlessFunction() )
            .AddPrefixUnaryOperator( "-", new MockPrefixUnaryOperator() )
            .SetPrefixUnaryConstructPrecedence( "-", 1 );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.NestedExpressionFailure ) );
    }

    [Theory]
    [InlineData( "foo 'foobar'" )]
    [InlineData( "foo 12.34" )]
    [InlineData( "foo false" )]
    [InlineData( "foo a" )]
    [InlineData( "foo Zero" )]
    [InlineData( "foo foo" )]
    [InlineData( "foo int" )]
    [InlineData( "foo -" )]
    [InlineData( "foo ^" )]
    [InlineData( "foo +" )]
    [InlineData( "foo [string]" )]
    [InlineData( "foo ToString" )]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenFunctionIsNotFollowedByOpenedParenthesis(string input)
    {
        var builder = new ParsedExpressionFactoryBuilder()
            .AddFunction( "foo", new MockParameterlessFunction() )
            .AddConstant( "Zero", new ZeroConstant() )
            .AddTypeDeclaration<int>( "int" )
            .AddPrefixUnaryOperator( "-", new MockPrefixUnaryOperator() )
            .AddPrefixTypeConverter( "[string]", new MockPrefixTypeConverter() )
            .AddPostfixUnaryOperator( "^", new MockPostfixUnaryOperator() )
            .AddPostfixTypeConverter( "ToString", new MockPostfixTypeConverter() )
            .AddBinaryOperator( "+", new MockBinaryOperator() )
            .SetPrefixUnaryConstructPrecedence( "-", 1 )
            .SetPrefixUnaryConstructPrecedence( "[string]", 1 )
            .SetPostfixUnaryConstructPrecedence( "^", 1 )
            .SetPostfixUnaryConstructPrecedence( "ToString", 1 )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.NestedExpressionFailure ) );
    }

    [Theory]
    [InlineData( "foo" )]
    [InlineData( "foo(" )]
    [InlineData( "foo( a" )]
    [InlineData( "foo( a ," )]
    [InlineData( "foo( a , b" )]
    [InlineData( "foo( a , b , ( c )" )]
    [InlineData( "foo( a , b , foo(" )]
    [InlineData( "a + foo" )]
    [InlineData( "a + foo(" )]
    [InlineData( "a + foo( a" )]
    [InlineData( "a + foo( a ," )]
    [InlineData( "a + foo( a , b" )]
    [InlineData( "a + foo( a , b , ( c )" )]
    [InlineData( "a + foo( a , b , foo(" )]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenFunctionParametersDoNotEndWithClosedParenthesis(string input)
    {
        var builder = new ParsedExpressionFactoryBuilder()
            .AddFunction( "foo", new MockFunctionWithThreeParameters() )
            .AddBinaryOperator( "+", new MockBinaryOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch(
                e => e.Errors.Any(
                    er => er.Type is ParsedExpressionBuilderErrorType.NestedExpressionFailure
                        or ParsedExpressionBuilderErrorType.MissingSubExpressionClosingSymbol ) );
    }

    [Theory]
    [InlineData( "foo( a )" )]
    [InlineData( "foo( a , b )" )]
    [InlineData( "foo( a , b , c , d )" )]
    [InlineData( "foo( 1 , 2 , 3 )" )]
    [InlineData( "foo( 1 , a , b )" )]
    [InlineData( "foo( a , 1 , b )" )]
    [InlineData( "foo( a , b , 1 )" )]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenFunctionSignatureDoesNotExist(string input)
    {
        var builder = new ParsedExpressionFactoryBuilder()
            .AddFunction( "foo", new MockParameterlessFunction() )
            .AddFunction( "foo", new MockFunctionWithThreeParameters() );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.FunctionCouldNotBeResolved ) );
    }

    [Theory]
    [InlineData( "foo( , b , c )" )]
    [InlineData( "foo( a , , c )" )]
    [InlineData( "foo( a + , b , c )" )]
    [InlineData( "foo( a , b + , c )" )]
    [InlineData( "foo( a , b , c + )" )]
    [InlineData( "foo( ( a , b , c )" )]
    [InlineData( "foo( a , ( b , c )" )]
    [InlineData( "foo( - , b , c )" )]
    [InlineData( "foo( a , - , c )" )]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenAnyFunctionParameterCouldNotBeParsed(string input)
    {
        var builder = new ParsedExpressionFactoryBuilder()
            .AddFunction( "foo", new MockFunctionWithThreeParameters() )
            .AddBinaryOperator( "+", new MockBinaryOperator() )
            .AddPrefixUnaryOperator( "-", new MockPrefixUnaryOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 )
            .SetPrefixUnaryConstructPrecedence( "-", 1 );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.NestedExpressionFailure ) );
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WithCorrectNesting_WhenAnySubExpressionErrorOccurs()
    {
        var input = "foo( bar( 1 + ) )";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddFunction( "foo", new MockParameterlessFunction() )
            .AddFunction( "bar", new MockParameterlessFunction() );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        using ( new AssertionScope() )
        {
            var exception = action.Should().ThrowExactly<ParsedExpressionCreationException>().And;
            exception.Errors.Select( e => e.Type )
                .Should()
                .BeSequentiallyEqualTo( ParsedExpressionBuilderErrorType.NestedExpressionFailure );

            if ( exception.Errors.Count == 0 || exception.Errors.First() is not ParsedExpressionBuilderAggregateError fooAggregateError )
                return;

            fooAggregateError.Token.ToString().Should().Be( "foo" );
            fooAggregateError.Inner.Select( e => e.Type )
                .Should()
                .BeSequentiallyEqualTo( ParsedExpressionBuilderErrorType.NestedExpressionFailure );

            if ( fooAggregateError.Inner.First() is not ParsedExpressionBuilderAggregateError barAggregateError )
                return;

            barAggregateError.Token.ToString().Should().Be( "bar" );
            barAggregateError.Inner.Select( e => e.Token.ToString() ).Distinct().Should().BeSequentiallyEqualTo( "+" );
        }
    }

    [Fact]
    public void Create_ShouldReturnExpressionWithCorrectArgumentIndexes_WhenArgumentsAreUsedAsFunctionParameters()
    {
        var (aValue, bValue, cValue, dValue) = Fixture.CreateDistinctCollection<decimal>( count: 4 );
        var expected = aValue + bValue + cValue + bValue + dValue + dValue + cValue;
        var input = "a + b + foo( c , b , d ) + d + c";

        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new ParsedExpressionAddOperator() )
            .AddFunction( "foo", new ParsedExpressionFunction<decimal, decimal, decimal, decimal>( (a, b, c) => a + b + c ) )
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
    [InlineData( "foo( 'a', 'b', 'c' )", "Func(a,b,c)" )]
    [InlineData( "foo( ( 'a' ), ( 'b' ), ( 'c' ) )", "Func(a,b,c)" )]
    [InlineData( "foo( 'a', 'b', bar() )", "Func(a,b,Func())" )]
    [InlineData( "foo( 'a', foo( 'b', 'c', 'd' ), 'e' )", "Func(a,Func(b,c,d),e)" )]
    [InlineData( "foo( bar(), foo( ( bar() ) , foo( 'a' , 'b' , 'c' ) , 'd' ) , 'e' )", "Func(Func(),Func(Func(),Func(a,b,c),d),e)" )]
    public void DelegateInvoke_ShouldReturnCorrectResult_WhenExpressionContainsFunction(string input, string expected)
    {
        var builder = new ParsedExpressionFactoryBuilder()
            .AddFunction( "foo", new MockFunctionWithThreeParameters() )
            .AddFunction( "bar", new MockParameterlessFunction() );

        var sut = builder.Build();

        var expression = sut.Create<string, string>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke();

        result.Should().Be( expected );
    }

    [Theory]
    [InlineData( "foo( 'a' + , 'b' , 'c' )", "Func(( a|PostOp ),b,c)" )]
    [InlineData( "foo( 'a' , 'b' + , 'c' )", "Func(a,( b|PostOp ),c)" )]
    [InlineData( "foo( 'a' , 'b' , 'c' + )", "Func(a,b,( c|PostOp ))" )]
    public void DelegateInvoke_ShouldReturnCorrectResult_WhenPostfixUnaryOperatorAmbiguityIsResolvedInFunctionParameters(
        string input,
        string expected)
    {
        var builder = new ParsedExpressionFactoryBuilder()
            .AddFunction( "foo", new MockFunctionWithThreeParameters() )
            .AddBinaryOperator( "+", new MockBinaryOperator() )
            .AddPostfixUnaryOperator( "+", new MockPostfixUnaryOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 )
            .SetPostfixUnaryConstructPrecedence( "+", 1 );

        var sut = builder.Build();

        var expression = sut.Create<string, string>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke();

        result.Should().Be( expected );
    }

    [Fact]
    public void Create_ShouldAddAutomaticResultConversion_WhenActualResultTypeIsDifferentFromExpected_BasedOnPrefixTypeConverter()
    {
        var input = "12.34";
        var expected = "( PreDecimalCast|12.34 )";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new MockBinaryOperator() )
            .AddPrefixTypeConverter( "[int]", new ParsedExpressionTypeConverter<int>() )
            .AddPostfixTypeConverter( "ToInt", new ParsedExpressionTypeConverter<int>() )
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
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new MockBinaryOperator() )
            .AddPrefixTypeConverter( "[int]", new ParsedExpressionTypeConverter<int>() )
            .AddPostfixTypeConverter( "ToInt", new ParsedExpressionTypeConverter<int>() )
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
    public void Create_ShouldThrowParsedExpressionCreationException_WhenAutomaticResultConversionFailsDueToTypeConverterThrowingException()
    {
        var input = "12.34";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddPrefixTypeConverter( "[string]", new ThrowingTypeConverter() )
            .SetPrefixUnaryConstructPrecedence( "[string]", 1 );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.OutputTypeConverterHasThrownException ) );
    }

    [Fact]
    public void
        Create_ShouldAddAutomaticResultConversion_WhenActualResultTypeIsDifferentFromExpectedAndTypeConverterDoesNotExistButResultIsAssignableToExpectedType()
    {
        var value = "foo";
        var input = $"'{value}'";
        var builder = new ParsedExpressionFactoryBuilder();
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
        var builder = new ParsedExpressionFactoryBuilder();
        var sut = builder.Build();

        var expression = sut.Create<decimal, double>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke();

        result.Should().Be( expected );
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenAutomaticResultConversionFailsDueToMissingConversionOperator()
    {
        var input = "12.34";
        var builder = new ParsedExpressionFactoryBuilder();
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.OutputTypeConverterHasThrownException ) );
    }

    [Fact]
    public void
        Create_ShouldAddAutomaticResultConversion_WhenActualResultTypeIsDifferentFromExpectedAndAutomaticResultConversionIsDisabledButResultIsAssignableToResultType()
    {
        var value = "foo";
        var input = $"'{value}'";
        var configuration = Substitute.For<IParsedExpressionFactoryConfiguration>();
        configuration.DecimalPoint.Returns( '.' );
        configuration.IntegerDigitSeparator.Returns( '_' );
        configuration.StringDelimiter.Returns( '\'' );
        configuration.ScientificNotationExponents.Returns( "eE" );
        configuration.AllowScientificNotation.Returns( true );
        configuration.AllowNonIntegerNumbers.Returns( true );
        configuration.ConvertResultToOutputTypeAutomatically.Returns( false );
        var builder = new ParsedExpressionFactoryBuilder().SetConfiguration( configuration );
        var sut = builder.Build();

        var expression = sut.Create<string, object>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke();

        result.Should().Be( value );
    }

    [Fact]
    public void
        Create_ShouldThrowParsedExpressionCreationException_WhenAutomaticResultConversionIsDisabledAndResultIsNotAssignableToResultType()
    {
        var input = "12.34";
        var configuration = Substitute.For<IParsedExpressionFactoryConfiguration>();
        configuration.DecimalPoint.Returns( '.' );
        configuration.IntegerDigitSeparator.Returns( '_' );
        configuration.StringDelimiter.Returns( '\'' );
        configuration.ScientificNotationExponents.Returns( "eE" );
        configuration.AllowScientificNotation.Returns( true );
        configuration.AllowNonIntegerNumbers.Returns( true );
        configuration.ConvertResultToOutputTypeAutomatically.Returns( false );
        var builder = new ParsedExpressionFactoryBuilder().SetConfiguration( configuration );
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<decimal, int>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch(
                e => MatchExpectations(
                    e,
                    input,
                    ParsedExpressionBuilderErrorType.ExpressionResultTypeIsNotCompatibleWithExpectedOutputType ) );
    }

    [Fact]
    public void IMathExpressionFactoryCreate_ShouldBeEquivalentToCreate()
    {
        var input = "a+b+c";
        var values = new[] { 1m, 2m, 3m };

        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new ParsedExpressionAddOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var factory = builder.Build();
        IParsedExpressionFactory sut = factory;

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
        var builder = new ParsedExpressionFactoryBuilder();
        IParsedExpressionFactory sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<decimal, decimal>( input ) );

        action.Should().ThrowExactly<ParsedExpressionCreationException>();
    }
}
