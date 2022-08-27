using System.Collections.Generic;
using System.Linq;
using FluentAssertions.Execution;
using LfrlAnvil.Computable.Expressions.Constructs;
using LfrlAnvil.Computable.Expressions.Constructs.Boolean;
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

    [Theory]
    [InlineData( "int[]" )]
    [InlineData( "( int[] )" )]
    [InlineData( "( ( ( int[] ) ) )" )]
    public void DelegateInvoke_ShouldReturnCorrectResult_WhenExpressionConsistsOfOnlyEmptyInlineArray(string input)
    {
        var builder = new ParsedExpressionFactoryBuilder().AddTypeDeclaration<int>( "int" );
        var sut = builder.Build();

        var expression = sut.Create<string, int[]>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke();

        result.Should().BeEmpty();
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
            .AddFunction( "foo", new MockParameterlessFunction() )
            .AddTypeDeclaration<int>( "int" );

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
            .AddFunction( "foo", new MockParameterlessFunction() )
            .AddTypeDeclaration<int>( "int" );

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
            .AddTypeDeclaration<int>( "int" )
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
            .AddTypeDeclaration<int>( "int" )
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
            .AddFunction( "foo", new MockParameterlessFunction() )
            .AddTypeDeclaration<int>( "int" );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<decimal, decimal>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.UnexpectedOpenedParenthesis ) );
    }

    [Theory]
    [MethodData( nameof( ParsedExpressionFactoryTestsData.GetOperandFollowedByTypeDeclarationData ) )]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenOperandIsFollowedByTypeDeclaration(string input)
    {
        var builder = new ParsedExpressionFactoryBuilder()
            .AddConstant( "Zero", new ZeroConstant() )
            .AddFunction( "foo", new MockParameterlessFunction() )
            .AddTypeDeclaration<int>( "int" );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.UnexpectedTypeDeclaration ) );
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
    public void Create_ShouldThrowParsedExpressionCreationException_WhenOpenedSquareBracketIsFirst()
    {
        var input = "[";
        var builder = new ParsedExpressionFactoryBuilder();
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.UnexpectedOpenedSquareBracket ) );
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenPrefixUnaryOperatorIsFollowedByOpenedSquareBracket()
    {
        var input = "- [";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddPrefixUnaryOperator( "-", new MockPrefixUnaryOperator() )
            .SetPrefixUnaryConstructPrecedence( "-", 1 );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.UnexpectedOpenedSquareBracket ) );
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenPrefixTypeConverterIsFollowedByOpenedSquareBracket()
    {
        var input = "[string] [";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddPrefixTypeConverter( "[string]", new MockPrefixTypeConverter() )
            .SetPrefixUnaryConstructPrecedence( "[string]", 1 );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.UnexpectedOpenedSquareBracket ) );
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenBinaryOperatorIsFollowedByOpenedSquareBracket()
    {
        var input = "a + [";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new MockBinaryOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.UnexpectedOpenedSquareBracket ) );
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenPostfixUnaryOperatorIsFollowedByOpenedSquareBracket()
    {
        var input = "a ^ [";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddPostfixUnaryOperator( "^", new MockPostfixUnaryOperator() )
            .SetPostfixUnaryConstructPrecedence( "^", 1 );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.UnexpectedOpenedSquareBracket ) );
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenPostfixTypeConverterIsFollowedByOpenedSquareBracket()
    {
        var input = "a ToString [";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddPostfixTypeConverter( "ToString", new MockPostfixTypeConverter() )
            .SetPostfixUnaryConstructPrecedence( "ToString", 1 );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<decimal, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.UnexpectedOpenedSquareBracket ) );
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenClosedSquareBracketIsFirst()
    {
        var input = "]";
        var builder = new ParsedExpressionFactoryBuilder();
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.UnexpectedClosedSquareBracket ) );
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenPrefixUnaryOperatorIsFollowedByClosedSquareBracket()
    {
        var input = "- ]";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddPrefixUnaryOperator( "-", new MockPrefixUnaryOperator() )
            .SetPrefixUnaryConstructPrecedence( "-", 1 );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.UnexpectedClosedSquareBracket ) );
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenPrefixTypeConverterIsFollowedByClosedSquareBracket()
    {
        var input = "[string] ]";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddPrefixTypeConverter( "[string]", new MockPrefixTypeConverter() )
            .SetPrefixUnaryConstructPrecedence( "[string]", 1 );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.UnexpectedClosedSquareBracket ) );
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenBinaryOperatorIsFollowedByClosedSquareBracket()
    {
        var input = "a + ]";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new MockBinaryOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.UnexpectedClosedSquareBracket ) );
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenPostfixUnaryOperatorIsFollowedByClosedSquareBracket()
    {
        var input = "a ^ ]";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddPostfixUnaryOperator( "^", new MockPostfixUnaryOperator() )
            .SetPostfixUnaryConstructPrecedence( "^", 1 );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.UnexpectedClosedSquareBracket ) );
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenPostfixTypeConverterIsFollowedByClosedSquareBracket()
    {
        var input = "a ToString ]";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddPostfixTypeConverter( "ToString", new MockPostfixTypeConverter() )
            .SetPostfixUnaryConstructPrecedence( "ToString", 1 );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<decimal, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.UnexpectedClosedSquareBracket ) );
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
            .AddTypeDeclaration<int>( "int" )
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
    [InlineData( "foo ." )]
    [InlineData( "foo [" )]
    [InlineData( "foo ]" )]
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
    public void Create_ShouldThrowParsedExpressionCreationException_WithCorrectNesting_WhenAnyFunctionSubExpressionErrorOccurs()
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
    [MethodData( nameof( ParsedExpressionFactoryTestsData.GetExpressionContainsFunctionData ) )]
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
    [MethodData( nameof( ParsedExpressionFactoryTestsData.GetPostfixUnaryOperatorAmbiguityIsResolvedInFunctionParametersData ) )]
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

    [Theory]
    [MethodData( nameof( ParsedExpressionFactoryTestsData.GetExpressionContainsVariadicFunctionData ) )]
    public void DelegateInvoke_ShouldReturnCorrectResult_WhenExpressionContainsVariadicFunction(string input, string expected)
    {
        var builder = new ParsedExpressionFactoryBuilder()
            .AddVariadicFunction( "foo", new MockVariadicFunction() );

        var sut = builder.Build();

        var expression = sut.Create<string, string>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke();

        result.Should().Be( expected );
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenVariadicFunctionCannotBeProcessed()
    {
        var input = "foo()";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddVariadicFunction( "foo", new ThrowingVariadicFunction() );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) );
    }

    [Theory]
    [InlineData( "int]" )]
    [InlineData( "int[ a , ]" )]
    [InlineData( "int[ a , - ]" )]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenClosedSquareBracketIsTooSoonDuringInlineArrayElementsParsing(
        string input)
    {
        var builder = new ParsedExpressionFactoryBuilder()
            .AddTypeDeclaration<int>( "int" )
            .AddPrefixUnaryOperator( "-", new MockPrefixUnaryOperator() )
            .SetPrefixUnaryConstructPrecedence( "-", 1 );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<int, int[]>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.NestedExpressionFailure ) );
    }

    [Theory]
    [InlineData( "int 'foobar'" )]
    [InlineData( "int 12.34" )]
    [InlineData( "int false" )]
    [InlineData( "int a" )]
    [InlineData( "int Zero" )]
    [InlineData( "int foo" )]
    [InlineData( "int int" )]
    [InlineData( "int -" )]
    [InlineData( "int ^" )]
    [InlineData( "int +" )]
    [InlineData( "int [string]" )]
    [InlineData( "int ToString" )]
    [InlineData( "int ." )]
    [InlineData( "int (" )]
    [InlineData( "int )" )]
    public void
        Create_ShouldThrowParsedExpressionCreationException_WhenTypeDeclarationIsNotFollowedByOpenedSquareBracketDuringInlineArrayParsing(
            string input)
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
    [InlineData( "int" )]
    [InlineData( "int[" )]
    [InlineData( "int[ a" )]
    [InlineData( "int[ a ," )]
    [InlineData( "int[ a , b" )]
    [InlineData( "int[ a , b , ( c )" )]
    [InlineData( "int[ a , b , int[" )]
    [InlineData( "a + int" )]
    [InlineData( "a + int[" )]
    [InlineData( "a + int[ a" )]
    [InlineData( "a + int[ a ," )]
    [InlineData( "a + int[ a , b" )]
    [InlineData( "a + int[ a , b , ( c )" )]
    [InlineData( "a + int[ a , b , int[" )]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenInlineArrayElementsDoNotEndWithClosedSquareBracket(string input)
    {
        var builder = new ParsedExpressionFactoryBuilder()
            .AddTypeDeclaration<int>( "int" )
            .AddBinaryOperator( "+", new MockBinaryOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<int, int[]>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch(
                e => e.Errors.Any(
                    er => er.Type is ParsedExpressionBuilderErrorType.NestedExpressionFailure
                        or ParsedExpressionBuilderErrorType.MissingSubExpressionClosingSymbol ) );
    }

    [Theory]
    [InlineData( "int[ , b , c ]" )]
    [InlineData( "int[ a , , c ]" )]
    [InlineData( "int[ a + , b , c ]" )]
    [InlineData( "int[ a , b + , c ]" )]
    [InlineData( "int[ a , b , c + ]" )]
    [InlineData( "int[ ( a , b , c ]" )]
    [InlineData( "int[ a , ( b , c ]" )]
    [InlineData( "int[ - , b , c ]" )]
    [InlineData( "int[ a , - , c ]" )]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenAnyInlineArrayElementCouldNotBeParsed(string input)
    {
        var builder = new ParsedExpressionFactoryBuilder()
            .AddTypeDeclaration<int>( "int" )
            .AddBinaryOperator( "+", new MockBinaryOperator() )
            .AddPrefixUnaryOperator( "-", new MockPrefixUnaryOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 )
            .SetPrefixUnaryConstructPrecedence( "-", 1 );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<int, int[]>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.NestedExpressionFailure ) );
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WithCorrectNesting_WhenAnyInlineArraySubExpressionErrorOccurs()
    {
        var input = "int[] [ int[ 1 + ] ]";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddTypeDeclaration<int>( "int" )
            .AddTypeDeclaration<int[]>( "int[]" );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<int, int[][]>( input ) );

        using ( new AssertionScope() )
        {
            var exception = action.Should().ThrowExactly<ParsedExpressionCreationException>().And;
            exception.Errors.Select( e => e.Type )
                .Should()
                .BeSequentiallyEqualTo( ParsedExpressionBuilderErrorType.NestedExpressionFailure );

            if ( exception.Errors.Count == 0 || exception.Errors.First() is not ParsedExpressionBuilderAggregateError fooAggregateError )
                return;

            fooAggregateError.Token.ToString().Should().Be( "int[]" );
            fooAggregateError.Inner.Select( e => e.Type )
                .Should()
                .BeSequentiallyEqualTo( ParsedExpressionBuilderErrorType.NestedExpressionFailure );

            if ( fooAggregateError.Inner.First() is not ParsedExpressionBuilderAggregateError barAggregateError )
                return;

            barAggregateError.Token.ToString().Should().Be( "int" );
            barAggregateError.Inner.Select( e => e.Token.ToString() ).Distinct().Should().BeSequentiallyEqualTo( "+" );
        }
    }

    [Theory]
    [MethodData( nameof( ParsedExpressionFactoryTestsData.GetExpressionContainsInlineArrayData ) )]
    public void DelegateInvoke_ShouldReturnCorrectResult_WhenExpressionContainsInlineArray(string input, string[] values, string[] expected)
    {
        var builder = new ParsedExpressionFactoryBuilder()
            .AddTypeDeclaration<string>( "string" );

        var sut = builder.Build();

        var expression = sut.Create<string, string[]>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke( values );

        result.Should().BeSequentiallyEqualTo( expected );
    }

    [Theory]
    [MethodData( nameof( ParsedExpressionFactoryTestsData.GetPostfixUnaryOperatorAmbiguityIsResolvedInInlineArrayElementsData ) )]
    public void DelegateInvoke_ShouldReturnCorrectResult_WhenPostfixUnaryOperatorAmbiguityIsResolvedInInlineArrayElements(
        string input,
        string[] expected)
    {
        var builder = new ParsedExpressionFactoryBuilder()
            .AddTypeDeclaration<string>( "string" )
            .AddBinaryOperator( "+", new MockBinaryOperator() )
            .AddPostfixUnaryOperator( "+", new MockPostfixUnaryOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 )
            .SetPostfixUnaryConstructPrecedence( "+", 1 );

        var sut = builder.Build();

        var expression = sut.Create<string, string[]>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke();

        result.Should().BeSequentiallyEqualTo( expected );
    }

    [Fact]
    public void DelegateInvoke_ShouldReturnCorrectResult_WhenExpressionContainsNestedInlineArray()
    {
        var input = "string[] [ string[ 'a' ] , string [] , string[ 'b' , 'c' ] ]";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddTypeDeclaration<string>( "string" )
            .AddTypeDeclaration<string[]>( "string[]" );

        var sut = builder.Build();

        var expression = sut.Create<string, string[][]>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke();

        using ( new AssertionScope() )
        {
            result.Should().HaveCount( 3 );
            if ( result.Length != 3 )
                return;

            result[0].Should().BeSequentiallyEqualTo( "a" );
            result[1].Should().BeEmpty();
            result[2].Should().BeSequentiallyEqualTo( "b", "c" );
        }
    }

    [Fact]
    public void DelegateInvoke_ShouldReturnCorrectResult_WhenExpressionContainsConstantInlineArrayWithElementsAssignableToArrayElementType()
    {
        var input = "object[ 'a' , 1 , true ]";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddTypeDeclaration<object>( "object" );

        var sut = builder.Build();

        var expression = sut.Create<object, object[]>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke();

        result.Should().BeSequentiallyEqualTo( "a", 1m, true );
    }

    [Fact]
    public void DelegateInvoke_ShouldReturnCorrectResult_WhenExpressionContainsVariableInlineArrayWithElementsAssignableToArrayElementType()
    {
        var input = "object[ a , b , c ]";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddTypeDeclaration<object>( "object" );

        var sut = builder.Build();

        var expression = sut.Create<object, object[]>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke( "a", 1m, true );

        result.Should().BeSequentiallyEqualTo( "a", 1m, true );
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenExpressionContainsConstantInlineArrayWithInvalidElementTypes()
    {
        var input = "string[ 'a' , 'b' , 1 ]";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddTypeDeclaration<string>( "string" );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string[]>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) );
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenExpressionContainsVariableInlineArrayWithInvalidElementTypes()
    {
        var input = "string[ 'a' , 'b' , p0 ]";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddTypeDeclaration<string>( "string" );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<decimal, string[]>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) );
    }

    [Theory]
    [InlineData( "." )]
    [InlineData( "( ." )]
    [InlineData( "- ." )]
    [InlineData( "[string] ." )]
    [InlineData( "a + ." )]
    [InlineData( "a ^ ." )]
    [InlineData( "a ToString ." )]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenUnexpectedMemberAccessIsEncountered(string input)
    {
        var builder = new ParsedExpressionFactoryBuilder()
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

        var action = Lambda.Of( () => sut.Create<decimal, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.UnexpectedMemberAccess ) );
    }

    [Theory]
    [InlineData( "a . ." )]
    [InlineData( "a . +" )]
    [InlineData( "a . -" )]
    [InlineData( "a . [string]" )]
    [InlineData( "a . ^" )]
    [InlineData( "a . ToString" )]
    [InlineData( "a . (" )]
    [InlineData( "a . )" )]
    [InlineData( "a . [" )]
    [InlineData( "a . ]" )]
    [InlineData( "a . 'foo'" )]
    [InlineData( "a . 12.34" )]
    [InlineData( "a . false" )]
    [InlineData( "a . Zero" )]
    [InlineData( "a . foo" )]
    [InlineData( "a . int" )]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenMemberAccessIsNotFollowedByPotentialMemberName(string input)
    {
        var builder = new ParsedExpressionFactoryBuilder()
            .AddConstant( "Zero", new ZeroConstant() )
            .AddFunction( "foo", new MockParameterlessFunction() )
            .AddTypeDeclaration<int>( "int" )
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

        var action = Lambda.Of( () => sut.Create<decimal, string>( input ) );

        action.Should().ThrowExactly<ParsedExpressionCreationException>();
    }

    [Fact]
    public void DelegateInvoke_ShouldReturnCorrectResult_ForPublicFieldMemberAccess()
    {
        var value = new TestParameter( "privateField", "privateProperty", "publicField", "publicProperty", next: null );
        var input = "a.PublicField";
        var builder = new ParsedExpressionFactoryBuilder();
        var sut = builder.Build();

        var expression = sut.Create<TestParameter, string>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke( value );

        result.Should().Be( "publicField" );
    }

    [Fact]
    public void DelegateInvoke_ShouldReturnCorrectResult_ForPublicPropertyMemberAccess()
    {
        var value = new TestParameter( "privateField", "privateProperty", "publicField", "publicProperty", next: null );
        var input = "a.PublicProperty";
        var builder = new ParsedExpressionFactoryBuilder();
        var sut = builder.Build();

        var expression = sut.Create<TestParameter, string>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke( value );

        result.Should().Be( "publicProperty" );
    }

    [Fact]
    public void DelegateInvoke_ShouldReturnCorrectResult_ForPublicParameterlessMethodCall()
    {
        var value = new TestParameter( "privateField", "privateProperty", "publicField", "publicProperty", next: null );
        var input = "a.PublicMethodZero()";
        var builder = new ParsedExpressionFactoryBuilder();
        var sut = builder.Build();

        var expression = sut.Create<TestParameter, string>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke( value );

        result.Should().Be( value.PublicMethodZero() );
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenMethodExistsButWithDifferentAmountOfParameters()
    {
        var input = "a.PublicMethodOne( 'foo' , 'bar' )";
        var builder = new ParsedExpressionFactoryBuilder();
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<TestParameter, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) );
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenMethodExistsButWithDifferentParameterTypes()
    {
        var input = "a.PublicMethodOne( true )";
        var builder = new ParsedExpressionFactoryBuilder();
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<TestParameter, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) );
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_ForPublicParameterlessGenericMethodCall()
    {
        var input = "a.PublicGenericMethodZero()";
        var builder = new ParsedExpressionFactoryBuilder();
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<TestParameter, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) );
    }

    [Theory]
    [InlineData( "'foo'", "foo" )]
    [InlineData( "1", "1" )]
    public void DelegateInvoke_ShouldReturnCorrectResult_ForPublicMethodCallWithOverloads(string parameter, string expected)
    {
        var value = new TestParameter( "privateField", "privateProperty", "publicField", "publicProperty", next: null );
        var input = $"a.PublicMethodOne( {parameter} )";
        var builder = new ParsedExpressionFactoryBuilder()
            .SetNumberParserProvider( p => ParsedExpressionNumberParser.CreateDefaultInt32( p.Configuration ) );

        var sut = builder.Build();

        var expression = sut.Create<TestParameter, string>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke( value );

        result.Should().Be( expected );
    }

    [Fact]
    public void DelegateInvoke_ShouldReturnCorrectResult_ForPublicFullyGenericMethodWithParametersOfTheSameType()
    {
        var value = new TestParameter( "privateField", "privateProperty", "publicField", "publicProperty", next: null );
        var input = "a.PublicGenericMethodThreeSameType( 'foo' , 'bar' , 'qux' )";
        var builder = new ParsedExpressionFactoryBuilder();
        var sut = builder.Build();

        var expression = sut.Create<TestParameter, string>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke( value );

        result.Should().Be( value.PublicGenericMethodThreeSameType( "foo", "bar", "qux" ) );
    }

    [Fact]
    public void
        Create_ShouldThrowParsedExpressionCreationException_WhenPublicFullyGenericMethodWithParametersOfTheSameTypeReceivesInvalidParameters()
    {
        var input = "a.PublicGenericMethodThreeSameType( 'foo' , 1 , true )";
        var builder = new ParsedExpressionFactoryBuilder();
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<TestParameter, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) );
    }

    [Fact]
    public void DelegateInvoke_ShouldReturnCorrectResult_ForPublicFullyGenericMethodWithParametersOfDifferentTypes()
    {
        var value = new TestParameter( "privateField", "privateProperty", "publicField", "publicProperty", next: null );
        var input = "a.PublicGenericMethodThreeDiffTypes( 'foo' , 1 , true )";
        var builder = new ParsedExpressionFactoryBuilder();
        var sut = builder.Build();

        var expression = sut.Create<TestParameter, string>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke( value );

        result.Should().Be( value.PublicGenericMethodThreeDiffTypes( "foo", 1m, true ) );
    }

    [Theory]
    [InlineData( "'foo'", "foo" )]
    [InlineData( "1", "1" )]
    [InlineData( "true", "Boolean" )]
    public void DelegateInvoke_ShouldReturnCorrectResult_ForPublicMethodCallWithSingleGenericOverload(string parameter, string expected)
    {
        var value = new TestParameter( "privateField", "privateProperty", "publicField", "publicProperty", next: null );
        var input = $"a.PublicAmbiguousMethodOne( {parameter} )";
        var builder = new ParsedExpressionFactoryBuilder()
            .SetNumberParserProvider( p => ParsedExpressionNumberParser.CreateDefaultInt32( p.Configuration ) );

        var sut = builder.Build();

        var expression = sut.Create<TestParameter, string>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke( value );

        result.Should().Be( expected );
    }

    [Theory]
    [InlineData( "1", "'foo'", "Decimal foo" )]
    [InlineData( "'foo'", "1", "foo Decimal" )]
    [InlineData( "1", "true", "Decimal Boolean" )]
    public void DelegateInvoke_ShouldReturnCorrectResult_ForPublicMethodCallWithManyGenericOverloads(
        string parameter1,
        string parameter2,
        string expected)
    {
        var value = new TestParameter( "privateField", "privateProperty", "publicField", "publicProperty", next: null );
        var input = $"a.PublicAmbiguousMethodTwo( {parameter1} , {parameter2} )";
        var builder = new ParsedExpressionFactoryBuilder();
        var sut = builder.Build();

        var expression = sut.Create<TestParameter, string>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke( value );

        result.Should().Be( expected );
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenPublicGenericMethodOverloadsAreAmbiguous()
    {
        var input = "a.PublicAmbiguousMethodTwo( 1, 2 )";
        var builder = new ParsedExpressionFactoryBuilder();
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<TestParameter, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) );
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenPublicGenericAndNonGenericMethodOverloadsAreAmbiguous()
    {
        var input = "a.PublicAmbiguousMethodTwo( 'foo' , 'bar' )";
        var builder = new ParsedExpressionFactoryBuilder();
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<TestParameter, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) );
    }

    [Fact]
    public void DelegateInvoke_ShouldReturnCorrectResult_WhenMemberNameEqualsOneOfArgumentNames()
    {
        var value = new TestParameter( "privateField", "privateProperty", "publicField", "publicProperty", next: null );
        var input = "PublicField.PublicField";
        var builder = new ParsedExpressionFactoryBuilder();
        var sut = builder.Build();

        var expression = sut.Create<TestParameter, string>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke( value );

        result.Should().Be( "publicField" );
    }

    [Fact]
    public void
        Create_ShouldThrowParsedExpressionCreationException_WhenPublicGenericMethodHasGenericArgumentsThatCannotBeInferredFromParameters()
    {
        var input = "a.PublicGenericMethodOne( 'foo' )";
        var builder = new ParsedExpressionFactoryBuilder();
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<TestParameter, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) );
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenPublicGenericMethodCannotBeResolvedDueToConstraints()
    {
        var input = "a.PublicConstrainedMethod( true )";
        var builder = new ParsedExpressionFactoryBuilder();
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<TestParameter, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) );
    }

    [Fact]
    public void DelegateInvoke_ShouldReturnCorrectResult_WhenPublicGenericMethodCanBeResolvedDespiteConstraints()
    {
        var value = new TestParameter( "privateField", "privateProperty", "publicField", "publicProperty", next: null );
        var input = "a.PublicConstrainedMethod( 'foo' )";
        var builder = new ParsedExpressionFactoryBuilder();
        var sut = builder.Build();

        var expression = sut.Create<TestParameter, string>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke( value );

        result.Should().Be( value.PublicConstrainedMethod( "foo" ) );
    }

    [Fact]
    public void DelegateInvoke_ShouldReturnCorrectResult_WhenMethodTargetAndParametersAreConstant()
    {
        var input = "const.PublicMethodOne( 'foo' )";
        var value = new TestParameter( "privateField", "privateProperty", "publicField", "publicProperty", next: null );
        var builder = new ParsedExpressionFactoryBuilder()
            .AddConstant( "const", new ParsedExpressionConstant<TestParameter>( value ) );

        var sut = builder.Build();

        var expression = sut.Create<string, string>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke();

        result.Should().Be( value.PublicMethodOne( "foo" ) );
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenMethodTargetAndParametersAreConstantButItThrowsAnException()
    {
        var input = "const.ThrowingMethod( 'foo' )";
        var value = new TestParameter( "privateField", "privateProperty", "publicField", "publicProperty", next: null );
        var builder = new ParsedExpressionFactoryBuilder()
            .AddConstant( "const", new ParsedExpressionConstant<TestParameter>( value ) );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) );
    }

    [Fact]
    public void DelegateInvoke_ShouldReturnCorrectResult_WhenMemberAccessIsChained()
    {
        var value = new TestParameter(
            "privateField",
            "privateProperty",
            "publicField",
            "publicProperty",
            next:
            new TestParameter( "privateField_next", "privateProperty_next", "publicField_next", "publicProperty_next", next: null ) );

        var input = "a.Next.PublicProperty.Length";
        var builder = new ParsedExpressionFactoryBuilder();
        var sut = builder.Build();

        var expression = sut.Create<TestParameter, int>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke( value );

        result.Should().Be( "publicProperty_next".Length );
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenMemberDoesNotExist()
    {
        var input = "a.foobar";
        var builder = new ParsedExpressionFactoryBuilder();
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<TestParameter, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) );
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenMemberIsStatic()
    {
        var input = "a.Empty";
        var builder = new ParsedExpressionFactoryBuilder();
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) );
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenMemberIsPropertyWithSetterOnly()
    {
        var input = "a.SetOnly";
        var builder = new ParsedExpressionFactoryBuilder();
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<TestParameter, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) );
    }

    [Theory]
    [InlineData( "a.publicproperty" )]
    [InlineData( "a.Publicproperty" )]
    [InlineData( "a.publicProperty" )]
    [InlineData( "a.PUBLICPROPERTY" )]
    public void DelegateInvoke_ShouldReturnCorrectResult_WhenMemberIsFoundWithIgnoredCase(string input)
    {
        var value = new TestParameter( "privateField", "privateProperty", "publicField", "publicProperty", next: null );
        var configuration = Substitute.For<IParsedExpressionFactoryConfiguration>();
        configuration.DecimalPoint.Returns( '.' );
        configuration.IntegerDigitSeparator.Returns( '_' );
        configuration.StringDelimiter.Returns( '\'' );
        configuration.ScientificNotationExponents.Returns( "eE" );
        configuration.AllowScientificNotation.Returns( true );
        configuration.AllowNonIntegerNumbers.Returns( true );
        configuration.ConvertResultToOutputTypeAutomatically.Returns( false );
        configuration.AllowNonPublicMemberAccess.Returns( false );
        configuration.IgnoreMemberNameCase.Returns( true );
        var builder = new ParsedExpressionFactoryBuilder().SetConfiguration( configuration );
        var sut = builder.Build();

        var expression = sut.Create<TestParameter, string>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke( value );

        result.Should().Be( "publicProperty" );
    }

    [Theory]
    [InlineData( "a.value" )]
    [InlineData( "a.Value" )]
    [InlineData( "a.VALUE" )]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenFieldOrPropertyNameIsAmbiguous(string input)
    {
        var configuration = Substitute.For<IParsedExpressionFactoryConfiguration>();
        configuration.DecimalPoint.Returns( '.' );
        configuration.IntegerDigitSeparator.Returns( '_' );
        configuration.StringDelimiter.Returns( '\'' );
        configuration.ScientificNotationExponents.Returns( "eE" );
        configuration.AllowScientificNotation.Returns( true );
        configuration.AllowNonIntegerNumbers.Returns( true );
        configuration.ConvertResultToOutputTypeAutomatically.Returns( false );
        configuration.AllowNonPublicMemberAccess.Returns( false );
        configuration.IgnoreMemberNameCase.Returns( true );
        var builder = new ParsedExpressionFactoryBuilder().SetConfiguration( configuration );
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<TestParameter, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) );
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenFieldMemberExistsButIsPrivateAndAllowNonPublicMemberAccessIsFalse()
    {
        var input = "a._privateField";
        var configuration = Substitute.For<IParsedExpressionFactoryConfiguration>();
        configuration.DecimalPoint.Returns( '.' );
        configuration.IntegerDigitSeparator.Returns( '_' );
        configuration.StringDelimiter.Returns( '\'' );
        configuration.ScientificNotationExponents.Returns( "eE" );
        configuration.AllowScientificNotation.Returns( true );
        configuration.AllowNonIntegerNumbers.Returns( true );
        configuration.ConvertResultToOutputTypeAutomatically.Returns( false );
        configuration.AllowNonPublicMemberAccess.Returns( false );
        configuration.IgnoreMemberNameCase.Returns( false );
        var builder = new ParsedExpressionFactoryBuilder().SetConfiguration( configuration );
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<TestParameter, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) );
    }

    [Fact]
    public void DelegateInvoke_ShouldReturnCorrectResult_WhenFieldMemberExistsButIsPrivateAndAllowNonPublicMemberAccessIsTrue()
    {
        var value = new TestParameter( "privateField", "privateProperty", "publicField", "publicProperty", next: null );
        var input = "a._privateField";
        var configuration = Substitute.For<IParsedExpressionFactoryConfiguration>();
        configuration.DecimalPoint.Returns( '.' );
        configuration.IntegerDigitSeparator.Returns( '_' );
        configuration.StringDelimiter.Returns( '\'' );
        configuration.ScientificNotationExponents.Returns( "eE" );
        configuration.AllowScientificNotation.Returns( true );
        configuration.AllowNonIntegerNumbers.Returns( true );
        configuration.ConvertResultToOutputTypeAutomatically.Returns( false );
        configuration.AllowNonPublicMemberAccess.Returns( true );
        configuration.IgnoreMemberNameCase.Returns( false );
        var builder = new ParsedExpressionFactoryBuilder().SetConfiguration( configuration );
        var sut = builder.Build();

        var expression = sut.Create<TestParameter, string>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke( value );

        result.Should().Be( "privateField" );
    }

    [Fact]
    public void
        Create_ShouldThrowParsedExpressionCreationException_WhenPropertyMemberExistsButIsPrivateAndAllowNonPublicMemberAccessIsFalse()
    {
        var input = "a.PrivateProperty";
        var configuration = Substitute.For<IParsedExpressionFactoryConfiguration>();
        configuration.DecimalPoint.Returns( '.' );
        configuration.IntegerDigitSeparator.Returns( '_' );
        configuration.StringDelimiter.Returns( '\'' );
        configuration.ScientificNotationExponents.Returns( "eE" );
        configuration.AllowScientificNotation.Returns( true );
        configuration.AllowNonIntegerNumbers.Returns( true );
        configuration.ConvertResultToOutputTypeAutomatically.Returns( false );
        configuration.AllowNonPublicMemberAccess.Returns( false );
        configuration.IgnoreMemberNameCase.Returns( false );
        var builder = new ParsedExpressionFactoryBuilder().SetConfiguration( configuration );
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<TestParameter, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) );
    }

    [Fact]
    public void DelegateInvoke_ShouldReturnCorrectResult_WhenPropertyMemberExistsButIsPrivateAndAllowNonPublicMemberAccessIsTrue()
    {
        var value = new TestParameter( "privateField", "privateProperty", "publicField", "publicProperty", next: null );
        var input = "a.PrivateProperty";
        var configuration = Substitute.For<IParsedExpressionFactoryConfiguration>();
        configuration.DecimalPoint.Returns( '.' );
        configuration.IntegerDigitSeparator.Returns( '_' );
        configuration.StringDelimiter.Returns( '\'' );
        configuration.ScientificNotationExponents.Returns( "eE" );
        configuration.AllowScientificNotation.Returns( true );
        configuration.AllowNonIntegerNumbers.Returns( true );
        configuration.ConvertResultToOutputTypeAutomatically.Returns( false );
        configuration.AllowNonPublicMemberAccess.Returns( true );
        configuration.IgnoreMemberNameCase.Returns( false );
        var builder = new ParsedExpressionFactoryBuilder().SetConfiguration( configuration );
        var sut = builder.Build();

        var expression = sut.Create<TestParameter, string>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke( value );

        result.Should().Be( "privateProperty" );
    }

    [Fact]
    public void
        Create_ShouldThrowParsedExpressionCreationException_WhenPublicPropertyMemberExistsButItsGetterIsPrivateAndAllowNonPublicMemberAccessIsFalse()
    {
        var input = "a.PrivateGetterProperty";
        var configuration = Substitute.For<IParsedExpressionFactoryConfiguration>();
        configuration.DecimalPoint.Returns( '.' );
        configuration.IntegerDigitSeparator.Returns( '_' );
        configuration.StringDelimiter.Returns( '\'' );
        configuration.ScientificNotationExponents.Returns( "eE" );
        configuration.AllowScientificNotation.Returns( true );
        configuration.AllowNonIntegerNumbers.Returns( true );
        configuration.ConvertResultToOutputTypeAutomatically.Returns( false );
        configuration.AllowNonPublicMemberAccess.Returns( false );
        configuration.IgnoreMemberNameCase.Returns( false );
        var builder = new ParsedExpressionFactoryBuilder().SetConfiguration( configuration );
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<TestParameter, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) );
    }

    [Fact]
    public void
        DelegateInvoke_ShouldReturnCorrectResult_WhenPublicPropertyMemberExistsButItsGetterIsPrivateAndAllowNonPublicMemberAccessIsTrue()
    {
        var value = new TestParameter( "privateField", "privateProperty", "publicField", "publicProperty", next: null );
        var input = "a.PrivateGetterProperty";
        var configuration = Substitute.For<IParsedExpressionFactoryConfiguration>();
        configuration.DecimalPoint.Returns( '.' );
        configuration.IntegerDigitSeparator.Returns( '_' );
        configuration.StringDelimiter.Returns( '\'' );
        configuration.ScientificNotationExponents.Returns( "eE" );
        configuration.AllowScientificNotation.Returns( true );
        configuration.AllowNonIntegerNumbers.Returns( true );
        configuration.ConvertResultToOutputTypeAutomatically.Returns( false );
        configuration.AllowNonPublicMemberAccess.Returns( true );
        configuration.IgnoreMemberNameCase.Returns( false );
        var builder = new ParsedExpressionFactoryBuilder().SetConfiguration( configuration );
        var sut = builder.Build();

        var expression = sut.Create<TestParameter, string>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke( value );

        result.Should().Be( "privateProperty" );
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenMethodExistsButIsPrivateAndAllowNonPublicMemberAccessIsFalse()
    {
        var input = "a.PrivateMethod( 'foo' )";
        var configuration = Substitute.For<IParsedExpressionFactoryConfiguration>();
        configuration.DecimalPoint.Returns( '.' );
        configuration.IntegerDigitSeparator.Returns( '_' );
        configuration.StringDelimiter.Returns( '\'' );
        configuration.ScientificNotationExponents.Returns( "eE" );
        configuration.AllowScientificNotation.Returns( true );
        configuration.AllowNonIntegerNumbers.Returns( true );
        configuration.ConvertResultToOutputTypeAutomatically.Returns( false );
        configuration.AllowNonPublicMemberAccess.Returns( false );
        configuration.IgnoreMemberNameCase.Returns( false );
        var builder = new ParsedExpressionFactoryBuilder().SetConfiguration( configuration );
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<TestParameter, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) );
    }

    [Fact]
    public void DelegateInvoke_ShouldReturnCorrectResult_WhenMethodExistsButIsPrivateAndAllowNonPublicMemberAccessIsTrue()
    {
        var value = new TestParameter( "privateField", "privateProperty", "publicField", "publicProperty", next: null );
        var input = "a.PrivateMethod( 'foo' )";
        var configuration = Substitute.For<IParsedExpressionFactoryConfiguration>();
        configuration.DecimalPoint.Returns( '.' );
        configuration.IntegerDigitSeparator.Returns( '_' );
        configuration.StringDelimiter.Returns( '\'' );
        configuration.ScientificNotationExponents.Returns( "eE" );
        configuration.AllowScientificNotation.Returns( true );
        configuration.AllowNonIntegerNumbers.Returns( true );
        configuration.ConvertResultToOutputTypeAutomatically.Returns( false );
        configuration.AllowNonPublicMemberAccess.Returns( true );
        configuration.IgnoreMemberNameCase.Returns( false );
        var builder = new ParsedExpressionFactoryBuilder().SetConfiguration( configuration );
        var sut = builder.Build();

        var expression = sut.Create<TestParameter, string>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke( value );

        result.Should().Be( "foo" );
    }

    [Fact]
    public void DelegateInvoke_ShouldReturnCorrectResult_ForMemberAccessOnSubExpressionInsideParentheses()
    {
        var input = "(a + b).Length";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new MockBinaryOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var sut = builder.Build();

        var expression = sut.Create<string, int>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke( "foobar", "qux" );

        result.Should().Be( "( foobar|BiOp|qux )".Length );
    }

    [Fact]
    public void DelegateInvoke_ShouldReturnCorrectResult_ForMemberAccessWithPrefixUnaryOperator()
    {
        var input = "- a.Length";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddPrefixUnaryOperator( "-", new MockPrefixUnaryOperator() )
            .SetPrefixUnaryConstructPrecedence( "-", 1 );

        var sut = builder.Build();

        var expression = sut.Create<string, string>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke( "foobar" );

        result.Should().Be( "( PreOp|6 )" );
    }

    [Fact]
    public void DelegateInvoke_ShouldReturnCorrectResult_ForMemberAccessWithPostfixUnaryOperator()
    {
        var input = "a.Length ^";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddPostfixUnaryOperator( "^", new MockPostfixUnaryOperator() )
            .SetPostfixUnaryConstructPrecedence( "^", 1 );

        var sut = builder.Build();

        var expression = sut.Create<string, string>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke( "foobar" );

        result.Should().Be( "( 6|PostOp )" );
    }

    [Theory]
    [InlineData( 1, 2, "( ( PreOp|6 )|PostOp )" )]
    [InlineData( 2, 1, "( PreOp|( 6|PostOp ) )" )]
    public void DelegateInvoke_ShouldReturnCorrectResult_ForMemberAccessWithPrefixAndPostfixUnaryOperator(
        int prefixPrecedence,
        int postfixPrecedence,
        string expected)
    {
        var input = "- a.Length ^";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddTypeDeclaration<string>( "string" )
            .AddPrefixUnaryOperator( "-", new MockPrefixUnaryOperator() )
            .AddPostfixUnaryOperator( "^", new MockPostfixUnaryOperator() )
            .SetPrefixUnaryConstructPrecedence( "-", prefixPrecedence )
            .SetPostfixUnaryConstructPrecedence( "^", postfixPrecedence );

        var sut = builder.Build();

        var expression = sut.Create<string, string>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke( "foobar" );

        result.Should().Be( expected );
    }

    [Fact]
    public void DelegateInvoke_ShouldReturnCorrectResult_ForFieldMemberAccessOnConstantValue()
    {
        var input = "const.PublicField";
        var constant = new TestParameter( "privateField", "privateProperty", "publicField", "publicProperty", next: null );
        var builder = new ParsedExpressionFactoryBuilder()
            .AddConstant( "const", new ParsedExpressionConstant<TestParameter>( constant ) );

        var sut = builder.Build();

        var expression = sut.Create<TestParameter, string>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke();

        result.Should().Be( "publicField" );
    }

    [Fact]
    public void DelegateInvoke_ShouldReturnCorrectResult_ForPropertyMemberAccessOnConstantValue()
    {
        var input = "const.PublicProperty";
        var constant = new TestParameter( "privateField", "privateProperty", "publicField", "publicProperty", next: null );
        var builder = new ParsedExpressionFactoryBuilder()
            .AddConstant( "const", new ParsedExpressionConstant<TestParameter>( constant ) );

        var sut = builder.Build();

        var expression = sut.Create<TestParameter, string>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke();

        result.Should().Be( "publicProperty" );
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_ForFieldMemberAccessOnNullConstant()
    {
        var input = "const.PublicField";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddConstant( "const", new ParsedExpressionConstant<TestParameter?>( null ) );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<TestParameter, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) );
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_ForPropertyMemberAccessOnNullConstant()
    {
        var input = "const.PublicProperty";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddConstant( "const", new ParsedExpressionConstant<TestParameter?>( null ) );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<TestParameter, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) );
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_ForDirectIndexerMemberAccess()
    {
        var input = "a.Item";
        var builder = new ParsedExpressionFactoryBuilder();
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<TestParameter, int>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) );
    }

    [Theory]
    [InlineData( "a[ i , ]" )]
    [InlineData( "a[ i , - ]" )]
    [InlineData( "string[ 'a' ][ i , ]" )]
    [InlineData( "string[ 'a' ][ i , - ]" )]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenClosedSquareBracketIsTooSoonDuringIndexerParametersParsing(
        string input)
    {
        var builder = new ParsedExpressionFactoryBuilder()
            .AddTypeDeclaration<string>( "string" )
            .AddPrefixUnaryOperator( "-", new MockPrefixUnaryOperator() )
            .SetPrefixUnaryConstructPrecedence( "-", 1 );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<int, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.NestedExpressionFailure ) );
    }

    [Theory]
    [InlineData( "a[" )]
    [InlineData( "a[ i" )]
    [InlineData( "a[ i ," )]
    [InlineData( "a[ i , j" )]
    [InlineData( "a[ i , j , ( k )" )]
    [InlineData( "string[ 'a' ][" )]
    [InlineData( "string[ 'a' ][ i" )]
    [InlineData( "string[ 'a' ][ i ," )]
    [InlineData( "string[ 'a' ][ i , j" )]
    [InlineData( "string[ 'a' ][ i , j , ( k )" )]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenIndexerParametersDoNotEndWithClosedSquareBracket(string input)
    {
        var builder = new ParsedExpressionFactoryBuilder()
            .AddTypeDeclaration<string>( "string" );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<int, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch(
                e => e.Errors.Any(
                    er => er.Type is ParsedExpressionBuilderErrorType.NestedExpressionFailure
                        or ParsedExpressionBuilderErrorType.MissingSubExpressionClosingSymbol ) );
    }

    [Theory]
    [InlineData( "x[ , b , c ]" )]
    [InlineData( "x[ a , , c ]" )]
    [InlineData( "x[ a + , b , c ]" )]
    [InlineData( "x[ a , b + , c ]" )]
    [InlineData( "x[ a , b , c + ]" )]
    [InlineData( "x[ ( a , b , c ]" )]
    [InlineData( "x[ a , ( b , c ]" )]
    [InlineData( "x[ - , b , c ]" )]
    [InlineData( "x[ a , - , c ]" )]
    [InlineData( "string[ 'a' ][ , b , c ]" )]
    [InlineData( "string[ 'a' ][ a , , c ]" )]
    [InlineData( "string[ 'a' ][ a + , b , c ]" )]
    [InlineData( "string[ 'a' ][ a , b + , c ]" )]
    [InlineData( "string[ 'a' ][ a , b , c + ]" )]
    [InlineData( "string[ 'a' ][ ( a , b , c ]" )]
    [InlineData( "string[ 'a' ][ a , ( b , c ]" )]
    [InlineData( "string[ 'a' ][ - , b , c ]" )]
    [InlineData( "string[ 'a' ][ a , - , c ]" )]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenAnyIndexerParameterCouldNotBeParsed(string input)
    {
        var builder = new ParsedExpressionFactoryBuilder()
            .AddTypeDeclaration<string>( "string" )
            .AddBinaryOperator( "+", new MockBinaryOperator() )
            .AddPrefixUnaryOperator( "-", new MockPrefixUnaryOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 )
            .SetPrefixUnaryConstructPrecedence( "-", 1 );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<int, int[]>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.NestedExpressionFailure ) );
    }

    [Theory]
    [MethodData( nameof( ParsedExpressionFactoryTestsData.GetExpressionContainsArrayIndexerData ) )]
    public void DelegateInvoke_ShouldReturnCorrectResult_WhenExpressionContainsArrayIndexer(string input, int index, string expected)
    {
        var builder = new ParsedExpressionFactoryBuilder()
            .AddTypeDeclaration<string>( "string" );

        var sut = builder.Build();

        var expression = sut.Create<int, string>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke( index );

        result.Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( ParsedExpressionFactoryTestsData.GetExpressionContainsObjectIndexerData ) )]
    public void DelegateInvoke_ShouldReturnCorrectResult_WhenExpressionContainsObjectIndexer(string input, int expected)
    {
        var builder = new ParsedExpressionFactoryBuilder();
        var sut = builder.Build();

        var map = new Dictionary<string, int>
        {
            { "a", 0 },
            { "b", 1 },
            { "c", 2 }
        };

        var expression = sut.Create<Dictionary<string, int>, int>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke( map );

        result.Should().Be( expected );
    }

    [Fact]
    public void DelegateInvoke_ShouldReturnCorrectResult_WhenExpressionContainsStringIndexer()
    {
        var input = "'foo'[ i ]";
        var builder = new ParsedExpressionFactoryBuilder();
        var sut = builder.Build();

        var expression = sut.Create<int, char>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke( 0 );

        result.Should().Be( 'f' );
    }

    [Fact]
    public void DelegateInvoke_ShouldReturnCorrectResult_WhenExpressionContainsMultidimensionalArrayIndexer()
    {
        var input = "arr[ [int] 1 , [int] 0 ]";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddPrefixTypeConverter( "[int]", new ParsedExpressionTypeConverter<int>() )
            .SetPrefixUnaryConstructPrecedence( "[int]", 1 );

        var sut = builder.Build();

        var arr = new[,] { { 0, 1 }, { 2, 3 } };
        var expression = sut.Create<int[,], int>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke( arr );

        result.Should().Be( 2 );
    }

    [Fact]
    public void DelegateInvoke_ShouldReturnCorrectResult_WhenPostfixUnaryOperatorAmbiguityIsResolvedInIndexerParameters()
    {
        var input = "map[ 'b' + ]";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new MockBinaryOperator() )
            .AddPostfixUnaryOperator( "+", new MockPostfixUnaryOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 )
            .SetPostfixUnaryConstructPrecedence( "+", 1 );

        var sut = builder.Build();

        var map = new Dictionary<string, int>
        {
            { "( a|PostOp )", 0 },
            { "( b|PostOp )", 1 },
            { "( c|PostOp )", 2 }
        };

        var expression = sut.Create<Dictionary<string, int>, int>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke( map );

        result.Should().Be( 1 );
    }

    [Fact]
    public void DelegateInvoke_ShouldReturnCorrectResult_WhenIndexerIsChained()
    {
        var input = "string[ 'a' ][ i ][ i ]";
        var builder = new ParsedExpressionFactoryBuilder().AddTypeDeclaration<string>( "string" );
        var sut = builder.Build();

        var expression = sut.Create<int, char>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke( 0 );

        result.Should().Be( 'a' );
    }

    [Fact]
    public void DelegateInvoke_ShouldReturnCorrectResult_WhenMemberAccessIsAfterIndexer()
    {
        var input = "string[ 'a' ][ i ].Length";
        var builder = new ParsedExpressionFactoryBuilder().AddTypeDeclaration<string>( "string" );
        var sut = builder.Build();

        var expression = sut.Create<int, int>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke( 0 );

        result.Should().Be( 1 );
    }

    [Fact]
    public void DelegateInvoke_ShouldReturnCorrectResult_WhenIndexerIsAfterMemberAccess()
    {
        var value = new TestParameter( "privateField", "privateProperty", "publicField", "publicProperty", next: null );
        var input = "a.PublicProperty[ [int] 1 ]";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddPrefixTypeConverter( "[int]", new ParsedExpressionTypeConverter<int>() )
            .SetPrefixUnaryConstructPrecedence( "[int]", 1 );

        var sut = builder.Build();

        var expression = sut.Create<TestParameter, char>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke( value );

        result.Should().Be( 'u' );
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenArrayIndexerDoesNotExist()
    {
        var input = "string[ 'a' ][ 'a' ]";
        var builder = new ParsedExpressionFactoryBuilder().AddTypeDeclaration<string>( "string" );
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) );
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenObjectIndexerDoesNotExist()
    {
        var input = "a[ 0 ]";
        var builder = new ParsedExpressionFactoryBuilder();
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<TestParameter, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) );
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenIndexerExistsButNotWithProvidedParameters()
    {
        var input = "'a'[ 'b' ]";
        var builder = new ParsedExpressionFactoryBuilder();
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) );
    }

    [Fact]
    public void DelegateInvoke_ShouldReturnCorrectResult_ForIndexerOnSubExpressionInsideParentheses()
    {
        var input = "(a + b)[ [int] 2 ]";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new MockBinaryOperator() )
            .AddPrefixTypeConverter( "[int]", new ParsedExpressionTypeConverter<int>() )
            .SetBinaryOperatorPrecedence( "+", 1 )
            .SetPrefixUnaryConstructPrecedence( "[int]", 1 );

        var sut = builder.Build();

        var expression = sut.Create<string, char>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke( "foobar", "qux" );

        result.Should().Be( 'f' );
    }

    [Fact]
    public void DelegateInvoke_ShouldReturnCorrectResult_ForIndexerWithPrefixUnaryOperator()
    {
        var input = "- string[ 'a' ][ i ]";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddTypeDeclaration<string>( "string" )
            .AddPrefixUnaryOperator( "-", new MockPrefixUnaryOperator() )
            .SetPrefixUnaryConstructPrecedence( "-", 1 );

        var sut = builder.Build();

        var expression = sut.Create<int, string>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke( 0 );

        result.Should().Be( "( PreOp|a )" );
    }

    [Fact]
    public void DelegateInvoke_ShouldReturnCorrectResult_ForIndexerWithPostfixUnaryOperator()
    {
        var input = "string[ 'a' ][ i ] ^";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddTypeDeclaration<string>( "string" )
            .AddPostfixUnaryOperator( "^", new MockPostfixUnaryOperator() )
            .SetPostfixUnaryConstructPrecedence( "^", 1 );

        var sut = builder.Build();

        var expression = sut.Create<int, string>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke( 0 );

        result.Should().Be( "( a|PostOp )" );
    }

    [Theory]
    [InlineData( 1, 2, "( ( PreOp|a )|PostOp )" )]
    [InlineData( 2, 1, "( PreOp|( a|PostOp ) )" )]
    public void DelegateInvoke_ShouldReturnCorrectResult_ForIndexerWithPrefixAndPostfixUnaryOperator(
        int prefixPrecedence,
        int postfixPrecedence,
        string expected)
    {
        var input = "- string[ 'a' ][ i ] ^";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddTypeDeclaration<string>( "string" )
            .AddPrefixUnaryOperator( "-", new MockPrefixUnaryOperator() )
            .AddPostfixUnaryOperator( "^", new MockPostfixUnaryOperator() )
            .SetPrefixUnaryConstructPrecedence( "-", prefixPrecedence )
            .SetPostfixUnaryConstructPrecedence( "^", postfixPrecedence );

        var sut = builder.Build();

        var expression = sut.Create<int, string>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke( 0 );

        result.Should().Be( expected );
    }

    [Fact]
    public void DelegateInvoke_ShouldReturnCorrectResult_WhenArrayIndexerTargetAndParametersAreConstant()
    {
        var input = "string[ 'a' ][ 0 ]";
        var builder = new ParsedExpressionFactoryBuilder()
            .SetNumberParserProvider( p => ParsedExpressionNumberParser.CreateDefaultInt32( p.Configuration ) )
            .AddTypeDeclaration<string>( "string" );

        var sut = builder.Build();

        var expression = sut.Create<int, string>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke();

        result.Should().Be( "a" );
    }

    [Fact]
    public void DelegateInvoke_ShouldReturnCorrectResult_WhenObjectIndexerTargetAndParametersAreConstant()
    {
        var input = "'a'[ 0 ]";
        var builder = new ParsedExpressionFactoryBuilder()
            .SetNumberParserProvider( p => ParsedExpressionNumberParser.CreateDefaultInt32( p.Configuration ) );

        var sut = builder.Build();

        var expression = sut.Create<int, char>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke();

        result.Should().Be( 'a' );
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenIndexerTargetAndParametersAreConstantButItThrowsAnException()
    {
        var input = "'a'[ 1 ]";
        var builder = new ParsedExpressionFactoryBuilder()
            .SetNumberParserProvider( p => ParsedExpressionNumberParser.CreateDefaultInt32( p.Configuration ) );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) );
    }

    [Fact]
    public void DelegateInvoke_ShouldReturnCorrectResult_WhenMemberAccessVariadicIsCalledDirectly()
    {
        var input = "MEMBER_ACCESS( 'foo' , 'Length' )";
        var builder = new ParsedExpressionFactoryBuilder();
        var sut = builder.Build();

        var expression = sut.Create<string, int>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke();

        result.Should().Be( "foo".Length );
    }

    [Theory]
    [InlineData( 0 )]
    [InlineData( 1 )]
    [InlineData( 3 )]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenMemberAccessVariadicReceivesInvalidAmountOfParameters(int count)
    {
        var input = $"MEMBER_ACCESS( {string.Join( " , ", Fixture.CreateMany<string>( count ).Select( p => $"'{p}'" ) )} )";
        var builder = new ParsedExpressionFactoryBuilder();

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) );
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenMemberAccessVariadicReceivesNonConstantSecondParameter()
    {
        var input = "MEMBER_ACCESS( 'foo' , a )";
        var builder = new ParsedExpressionFactoryBuilder();
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) );
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenMemberAccessVariadicReceivesSecondParameterNotOfStringType()
    {
        var input = "MEMBER_ACCESS( 'foo' , 1 )";
        var builder = new ParsedExpressionFactoryBuilder();
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) );
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenMemberAccessVariadicReceivesNullSecondParameterOfStringType()
    {
        var input = "MEMBER_ACCESS( 'foo' , null )";
        var builder = new ParsedExpressionFactoryBuilder().AddConstant( "null", new ParsedExpressionConstant<string?>( null ) );
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) );
    }

    [Fact]
    public void DelegateInvoke_ShouldReturnCorrectResult_WhenIndexerCallVariadicIsCalledDirectly()
    {
        var input = "INDEXER_CALL( 'foo' , 1 )";
        var builder = new ParsedExpressionFactoryBuilder().SetNumberParserProvider(
            p => ParsedExpressionNumberParser.CreateDefaultInt32( p.Configuration ) );

        var sut = builder.Build();

        var expression = sut.Create<string, char>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke();

        result.Should().Be( 'o' );
    }

    [Theory]
    [InlineData( 0 )]
    [InlineData( 1 )]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenIndexerCallVariadicReceivesInvalidAmountOfParameters(int count)
    {
        var input = $"INDEXER_CALL( {string.Join( " , ", Fixture.CreateMany<string>( count ).Select( p => $"'{p}'" ) )} )";
        var builder = new ParsedExpressionFactoryBuilder();

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) );
    }

    [Fact]
    public void DelegateInvoke_ShouldReturnCorrectResult_WhenMethodCallVariadicIsCalledDirectly()
    {
        var input = "METHOD_CALL( a , 'PublicMethodOne' , 'foo' )";
        var value = new TestParameter( "privateField", "privateProperty", "publicField", "publicProperty", next: null );
        var builder = new ParsedExpressionFactoryBuilder();
        var sut = builder.Build();

        var expression = sut.Create<TestParameter, string>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke( value );

        result.Should().Be( value.PublicMethodOne( "foo" ) );
    }

    [Theory]
    [InlineData( 0 )]
    [InlineData( 1 )]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenMethodCallVariadicReceivesInvalidAmountOfParameters(int count)
    {
        var input = $"METHOD_CALL( {string.Join( " , ", Fixture.CreateMany<string>( count ).Select( p => $"'{p}'" ) )} )";
        var builder = new ParsedExpressionFactoryBuilder();

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) );
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenMethodCallVariadicReceivesNonConstantSecondParameter()
    {
        var input = "METHOD_CALL( 'foo' , a )";
        var builder = new ParsedExpressionFactoryBuilder();
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) );
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenMethodCallVariadicReceivesSecondParameterNotOfStringType()
    {
        var input = "METHOD_CALL( 'foo' , 1 )";
        var builder = new ParsedExpressionFactoryBuilder();
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) );
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenMethodCallVariadicReceivesNullSecondParameterOfStringType()
    {
        var input = "METHOD_CALL( 'foo' , null )";
        var builder = new ParsedExpressionFactoryBuilder().AddConstant( "null", new ParsedExpressionConstant<string?>( null ) );
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) );
    }

    [Fact]
    public void DelegateInvoke_ShouldReturnCorrectResult_WhenMakeArrayVariadicIsCalledDirectly()
    {
        var input = "MAKE_ARRAY( STRING , 'foo' , 'bar' )";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddConstant( "STRING", new ParsedExpressionConstant<Type>( typeof( string ) ) );

        var sut = builder.Build();

        var expression = sut.Create<string, string[]>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke();

        result.Should().BeSequentiallyEqualTo( "foo", "bar" );
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenMakeArrayVariadicReceivesZeroParameters()
    {
        var input = "MAKE_ARRAY()";
        var builder = new ParsedExpressionFactoryBuilder();

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string[]>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) );
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenMakeArrayVariadicReceivesNonConstantFirstParameter()
    {
        var input = "MAKE_ARRAY( a )";
        var builder = new ParsedExpressionFactoryBuilder();
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<Type, string[]>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) );
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenMakeArrayVariadicReceivesFirstParameterNotOfTypeType()
    {
        var input = "MAKE_ARRAY( 1 )";
        var builder = new ParsedExpressionFactoryBuilder();
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string[]>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) );
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenMakeArrayVariadicReceivesNullFirstParameterOfTypeType()
    {
        var input = "MAKE_ARRAY( null )";
        var builder = new ParsedExpressionFactoryBuilder().AddConstant( "null", new ParsedExpressionConstant<Type?>( null ) );
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string[]>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) );
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
    public void Create_ShouldRemoveUnusedArguments_WhenAllArgumentsAreUnused()
    {
        var input = "a || b || c || true";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "||", new ParsedExpressionOrOperator() )
            .SetBinaryOperatorPrecedence( "||", 1 );

        var sut = builder.Build();

        var result = sut.Create<bool, bool>( input );

        result.GetArgumentCount().Should().Be( 0 );
    }

    [Fact]
    public void Create_ShouldRemoveUnusedArguments_WhenOnlyArgumentsAtEndAreUnused()
    {
        var input = "a && ( b || c || true )";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "||", new ParsedExpressionOrOperator() )
            .AddBinaryOperator( "&&", new ParsedExpressionAndOperator() )
            .SetBinaryOperatorPrecedence( "||", 1 )
            .SetBinaryOperatorPrecedence( "&&", 1 );

        var sut = builder.Build();

        var result = sut.Create<bool, bool>( input );

        using ( new AssertionScope() )
        {
            result.GetArgumentCount().Should().Be( 1 );
            result.GetArgumentNames().Select( n => n.ToString() ).Should().BeEquivalentTo( "a" );
            result.GetUnboundArgumentIndex( "a" ).Should().Be( 0 );
        }
    }

    [Fact]
    public void Create_ShouldRemoveUnusedArguments_WhenRemainingArgumentsNeedToBeReorganized()
    {
        var input = "a && ( b || c || true ) && d && ( e || true )";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "||", new ParsedExpressionOrOperator() )
            .AddBinaryOperator( "&&", new ParsedExpressionAndOperator() )
            .SetBinaryOperatorPrecedence( "||", 1 )
            .SetBinaryOperatorPrecedence( "&&", 1 );

        var sut = builder.Build();

        var result = sut.Create<bool, bool>( input );

        using ( new AssertionScope() )
        {
            result.GetArgumentCount().Should().Be( 2 );
            result.GetArgumentNames().Select( n => n.ToString() ).Should().BeEquivalentTo( "a", "d" );
            result.GetUnboundArgumentIndex( "a" ).Should().Be( 0 );
            result.GetUnboundArgumentIndex( "d" ).Should().Be( 1 );
        }
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
