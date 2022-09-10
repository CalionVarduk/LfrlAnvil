using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using FluentAssertions.Execution;
using LfrlAnvil.Computable.Expressions.Constructs;
using LfrlAnvil.Computable.Expressions.Constructs.Boolean;
using LfrlAnvil.Computable.Expressions.Constructs.Int32;
using LfrlAnvil.Computable.Expressions.Constructs.String;
using LfrlAnvil.Computable.Expressions.Constructs.Variadic;
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
            expression.UnboundArguments.GetIndex( "a" ).Should().Be( 0 );
            expression.UnboundArguments.GetIndex( "b" ).Should().Be( 1 );
            expression.UnboundArguments.GetIndex( "c" ).Should().Be( 2 );
            expression.UnboundArguments.GetIndex( "d" ).Should().Be( 3 );
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
            expression.UnboundArguments.Select( kv => kv.Key.ToString() ).Should().BeSequentiallyEqualTo( "a" );
            expression.BoundArguments.Should().BeEmpty();
            expression.DiscardedArguments.Should().BeEmpty();
            if ( expression.UnboundArguments.Count != 1 )
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
    public void Create_ShouldThrowParsedExpressionCreationException_WhenInputContainsOnlyOpenedSquareBracket()
    {
        var input = "[";
        var builder = new ParsedExpressionFactoryBuilder();
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should().ThrowExactly<ParsedExpressionCreationException>();
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenPrefixUnaryOperatorIsFollowedByUnclosedOpenedSquareBracket()
    {
        var input = "- [";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddPrefixUnaryOperator( "-", new MockPrefixUnaryOperator() )
            .SetPrefixUnaryConstructPrecedence( "-", 1 );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should().ThrowExactly<ParsedExpressionCreationException>();
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenPrefixTypeConverterIsFollowedByUnclosedSquareBracket()
    {
        var input = "[string] [";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddPrefixTypeConverter( "[string]", new MockPrefixTypeConverter() )
            .SetPrefixUnaryConstructPrecedence( "[string]", 1 );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should().ThrowExactly<ParsedExpressionCreationException>();
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenBinaryOperatorIsFollowedByUnclosedOpenedSquareBracket()
    {
        var input = "a + [";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new MockBinaryOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should().ThrowExactly<ParsedExpressionCreationException>();
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

    [Theory]
    [InlineData( "" )]
    [InlineData( ";" )]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenInputIsEmpty(string input)
    {
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

    [Theory]
    [InlineData( "a +" )]
    [InlineData( "a + ;" )]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenAmbiguousPostfixUnaryOperatorCannotBeProcessedAtTheEndOfInput(
        string input)
    {
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

    [Theory]
    [InlineData( "- a" )]
    [InlineData( "- a ;" )]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenPrefixUnaryOperatorCannotBeProcessedAtTheEndOfInput(string input)
    {
        var builder = new ParsedExpressionFactoryBuilder()
            .AddPrefixUnaryOperator( "-", new ThrowingUnaryOperator() )
            .SetPrefixUnaryConstructPrecedence( "-", 1 );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) );
    }

    [Theory]
    [InlineData( "[string] a" )]
    [InlineData( "[string] a ;" )]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenPrefixTypeConverterCannotBeProcessedAtTheEndOfInput(string input)
    {
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
    public void Create_ShouldThrowParsedExpressionCreationException_WhenElementSeparatorIsFoundWhenNestedElementsAreNotBeingParsed(
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
            expression.UnboundArguments.GetIndex( "a" ).Should().Be( 0 );
            expression.UnboundArguments.GetIndex( "b" ).Should().Be( 1 );
            expression.UnboundArguments.GetIndex( "c" ).Should().Be( 2 );
            expression.UnboundArguments.GetIndex( "d" ).Should().Be( 3 );
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

    [Fact]
    public void DelegateInvoke_ShouldReturnCorrectResult_WhenConstantDelegateIsInvoked()
    {
        var input = "const( 'foo' , 'bar' )";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddConstant( "const", new ParsedExpressionConstant<Func<string, string, string>>( (a, b) => a + b ) );

        var sut = builder.Build();

        var expression = sut.Create<string, string>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke();

        result.Should().Be( "foobar" );
    }

    [Fact]
    public void DelegateInvoke_ShouldReturnCorrectResult_WhenArgumentDelegateIsInvoked()
    {
        var input = "delegate( 'foo' , 'bar' )";
        var builder = new ParsedExpressionFactoryBuilder();
        var sut = builder.Build();

        var expression = sut.Create<Func<string, string, string>, string>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke( (a, b) => a + b );

        result.Should().Be( "foobar" );
    }

    [Fact]
    public void DelegateInvoke_ShouldReturnCorrectResult_WhenDelegateInParenthesesIsInvoked()
    {
        var input = "( delegate ) ( 'foo' , 'bar' )";
        var builder = new ParsedExpressionFactoryBuilder();
        var sut = builder.Build();

        var expression = sut.Create<Func<string, string, string>, string>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke( (a, b) => a + b );

        result.Should().Be( "foobar" );
    }

    [Fact]
    public void DelegateInvoke_ShouldReturnCorrectResult_WhenDelegateFromIndexerIsInvoked()
    {
        var input = "delegates [ 0 ] ( 'foo' , 'bar' )";
        var builder = new ParsedExpressionFactoryBuilder()
            .SetNumberParserProvider( p => ParsedExpressionNumberParser.CreateDefaultInt32( p.Configuration ) )
            .AddConstant(
                "delegates",
                new ParsedExpressionConstant<Func<string, string, string>[]>( new Func<string, string, string>[] { (a, b) => a + b } ) );

        var sut = builder.Build();

        var expression = sut.Create<string, string>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke();

        result.Should().Be( "foobar" );
    }

    [Theory]
    [InlineData( 1, 2, "( ( PreOp|foobar )|PostOp )" )]
    [InlineData( 2, 1, "( PreOp|( foobar|PostOp ) )" )]
    public void DelegateInvoke_ShouldReturnCorrectResult_ForDelegateInvocationWithPrefixAndPostfixUnaryOperator(
        int prefixPrecedence,
        int postfixPrecedence,
        string expected)
    {
        var input = "- delegate( 'foo' , 'bar' ) ^";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddPrefixUnaryOperator( "-", new MockPrefixUnaryOperator() )
            .AddPostfixUnaryOperator( "^", new MockPostfixUnaryOperator() )
            .SetPrefixUnaryConstructPrecedence( "-", prefixPrecedence )
            .SetPostfixUnaryConstructPrecedence( "^", postfixPrecedence );

        var sut = builder.Build();

        var expression = sut.Create<Func<string, string, string>, string>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke( (a, b) => a + b );

        result.Should().Be( expected );
    }

    [Theory]
    [InlineData( 0 )]
    [InlineData( 1 )]
    [InlineData( 3 )]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenDelegateReceivesInvalidAmountOfParameters(int count)
    {
        var input = $"delegate( {string.Join( " , ", Fixture.CreateMany<string>( count ).Select( v => $"'{v}'" ) )} )";
        var builder = new ParsedExpressionFactoryBuilder();
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<Func<string, string, string>, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) );
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenDelegateInvocationIsPrecededByPostfixUnaryOperator()
    {
        var input = "delegate ^ ( 'foo' , 'bar' )";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddPostfixUnaryOperator( "^", new MockNoOpUnaryOperator() )
            .SetPostfixUnaryConstructPrecedence( "^", 1 );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<Func<string, string, string>, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.UnexpectedOpenedParenthesis ) );
    }

    [Fact]
    public void DelegateInvoke_ShouldReturnCorrectResult_WhenDelegateInvocationIsPrecededByPostfixUnaryOperatorWrappedInParentheses()
    {
        var input = "( delegate ^ ) ( 'foo' , 'bar' )";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddPostfixUnaryOperator( "^", new MockNoOpUnaryOperator() )
            .SetPostfixUnaryConstructPrecedence( "^", 1 );

        var sut = builder.Build();

        var expression = sut.Create<Func<string, string, string>, string>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke( (a, b) => a + b );

        result.Should().Be( "foobar" );
    }

    [Fact]
    public void DelegateInvoke_ShouldReturnCorrectResult_WhenDelegateInvocationIsChained()
    {
        var input = "delegate( 'foo' ) ( 'bar' ) ( 'qux' )";
        var builder = new ParsedExpressionFactoryBuilder();
        var sut = builder.Build();

        var expression = sut.Create<Func<string, Func<string, Func<string, string>>>, string>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke( a => b => c => a + b + c );

        result.Should().Be( "foobarqux" );
    }

    [Fact]
    public void DelegateInvoke_ShouldReturnCorrectResult_WhenInvokedDelegateAndItsParametersAreConstantAndConstantFoldingIsEnabled()
    {
        var input = "delegate( 1 , 2 , 3 ) * a";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddConstant( "delegate", new ParsedExpressionConstant<Func<int, int, int, int>>( (a, b, c) => a + b - c ) )
            .SetNumberParserProvider( p => ParsedExpressionNumberParser.CreateDefaultInt32( p.Configuration ) )
            .SetInvokeProvider( _ => new ParsedExpressionInvoke( foldConstantsWhenPossible: true ) )
            .AddBinaryOperator( "*", new ParsedExpressionMultiplyInt32Operator() )
            .SetBinaryOperatorPrecedence( "*", 1 );

        var sut = builder.Build();

        var expression = sut.Create<int, int>( input );
        var @delegate = expression.Compile();

        using ( new AssertionScope() )
        {
            expression.UnboundArguments.Should().BeEmpty();
            var result = @delegate.Invoke();
            result.Should().Be( 0 );
        }
    }

    [Fact]
    public void DelegateInvoke_ShouldReturnCorrectResult_WhenInvokedDelegateAndItsParametersAreConstantAndConstantFoldingIsDisabled()
    {
        var input = "delegate( 1 , 2 , 3 ) * a";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddConstant( "delegate", new ParsedExpressionConstant<Func<int, int, int, int>>( (a, b, c) => a + b - c ) )
            .SetNumberParserProvider( p => ParsedExpressionNumberParser.CreateDefaultInt32( p.Configuration ) )
            .SetInvokeProvider( _ => new ParsedExpressionInvoke( foldConstantsWhenPossible: false ) )
            .AddBinaryOperator( "*", new ParsedExpressionMultiplyInt32Operator() )
            .SetBinaryOperatorPrecedence( "*", 1 );

        var sut = builder.Build();

        var expression = sut.Create<int, int>( input );
        var @delegate = expression.Compile();

        using ( new AssertionScope() )
        {
            expression.UnboundArguments.Contains( "a" ).Should().BeTrue();
            var result = @delegate.Invoke( 100 );
            result.Should().Be( 0 );
        }
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

    [Fact]
    public void DelegateInvoke_ShouldReturnCorrectResult_WhenExpressionContainsParameterlessInlineDelegate()
    {
        var input = "[] 'foo' + 'bar'";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new MockBinaryOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var sut = builder.Build();

        var expression = sut.Create<string, Func<string>>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke();
        var delegateResult = result();

        delegateResult.Should().Be( "( foo|BiOp|bar )" );
    }

    [Fact]
    public void DelegateInvoke_ShouldReturnCorrectResult_WhenExpressionContainsInlineDelegateWithParameters()
    {
        var input = "[ int a , int b , int c ] a + b + c";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddTypeDeclaration<int>( "int" )
            .AddBinaryOperator( "+", new MockBinaryOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var sut = builder.Build();

        var expression = sut.Create<string, Func<int, int, int, string>>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke();
        var delegateResult = result( 1, 2, 3 );

        delegateResult.Should().Be( "( ( 1|BiOp|2 )|BiOp|3 )" );
    }

    [Fact]
    public void DelegateInvoke_ShouldReturnCorrectResult_WhenExpressionContainsInlineDelegateInvocation()
    {
        var input = "( [string a] a + 'bar' ) ( 'foo' )";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddTypeDeclaration<string>( "string" )
            .AddBinaryOperator( "+", new MockBinaryOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var sut = builder.Build();

        var expression = sut.Create<string, string>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke();

        result.Should().Be( "( foo|BiOp|bar )" );
    }

    [Fact]
    public void DelegateInvoke_ShouldReturnCorrectResult_WhenExpressionContainsInlineDelegateFromArrayInvocation()
    {
        var input = "func[ [string a] a + 'bar' ] [ 0 ] ( 'foo' )";
        var builder = new ParsedExpressionFactoryBuilder()
            .SetNumberParserProvider( p => ParsedExpressionNumberParser.CreateDefaultInt32( p.Configuration ) )
            .AddTypeDeclaration<Func<string, string>>( "func" )
            .AddTypeDeclaration<string>( "string" )
            .AddBinaryOperator( "+", new MockBinaryOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var sut = builder.Build();

        var expression = sut.Create<string, string>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke();

        result.Should().Be( "( foo|BiOp|bar )" );
    }

    [Fact]
    public void DelegateInvoke_ShouldReturnCorrectResult_WhenExpressionContainsInlineDelegatePassedAsFunctionArgument()
    {
        var input = "func( [string a] a + 'bar' , 'qux' )";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddTypeDeclaration<string>( "string" )
            .AddFunction( "func", ParsedExpressionFunction.Create( (Func<string, string> f, string s) => f( "foo" ) + s ) )
            .AddBinaryOperator( "+", new MockBinaryOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var sut = builder.Build();

        var expression = sut.Create<string, string>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke();

        result.Should().Be( "( foo|BiOp|bar )qux" );
    }

    [Fact]
    public void DelegateInvoke_ShouldReturnCorrectResult_WhenExpressionContainsChainedInlineDelegates()
    {
        var input = "[ string a ] [ string b ] [ string c ] c";
        var builder = new ParsedExpressionFactoryBuilder().AddTypeDeclaration<string>( "string" );
        var sut = builder.Build();

        var expression = sut.Create<string, Func<string, Func<string, Func<string, string>>>>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke();
        var delegateResult = result( "foo" )( "bar" )( "qux" );

        delegateResult.Should().Be( "qux" );
    }

    [Fact]
    public void DelegateInvoke_ShouldReturnCorrectResult_WhenExpressionContainsComplexInlineDelegateChaining()
    {
        var input = "[ string a ] a + ( [ string b ] [ string c ] c ) ( 'qux' ) ( 'bar' ) + ( [ string d ] d ) ( 'foobar' )";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddTypeDeclaration<string>( "string" )
            .AddBinaryOperator( "+", new MockBinaryOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var sut = builder.Build();

        var expression = sut.Create<string, Func<string, string>>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke();
        var delegateResult = result( "foo" );

        delegateResult.Should().Be( "( ( foo|BiOp|bar )|BiOp|foobar )" );
    }

    [Fact]
    public void DelegateInvoke_ShouldReturnCorrectResult_WhenExpressionContainsNestedInlineDelegatesWithSameParameterNames()
    {
        var input = "[] ( [ string a ] a ) ( 'foo' ) + ( [ string a ] a ) ( 'bar' )";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddTypeDeclaration<string>( "string" )
            .AddBinaryOperator( "+", new MockBinaryOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var sut = builder.Build();

        var expression = sut.Create<string, Func<string>>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke();
        var delegateResult = result();

        delegateResult.Should().Be( "( foo|BiOp|bar )" );
    }

    [Fact]
    public void DelegateInvoke_ShouldReturnCorrectResult_WhenExpressionContainsDelegateWithManyParameters()
    {
        var input = @"( [ string a , string b , string c , string d , string e , string f , string g , string h , string i , string j ]
a + b + c + d + e + f + g + h + i + j ) ( 'a' , 'b' , 'c' , 'd' , 'e' , 'f' , 'g' , 'h' , 'i' , 'j' )";

        var builder = new ParsedExpressionFactoryBuilder()
            .AddTypeDeclaration<string>( "string" )
            .AddBinaryOperator( "+", new ParsedExpressionAddStringOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var sut = builder.Build();

        var expression = sut.Create<string, string>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke();

        result.Should().Be( "abcdefghij" );
    }

    [Theory]
    [InlineData( "[string a , string a] a + a" )]
    [InlineData( ("[string a] [string a] a + a") )]
    [InlineData( "a + ( ( [string a] a + a ) ( a ) )" )]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenInlineDelegateParameterIsDuplicated(string input)
    {
        var builder = new ParsedExpressionFactoryBuilder()
            .AddTypeDeclaration<string>( "string" )
            .AddBinaryOperator( "+", new MockBinaryOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.NestedExpressionFailure ) );
    }

    [Theory]
    [InlineData( "[ a ] a" )]
    [InlineData( "[ string a b ] a + b" )]
    [InlineData( ("[ string a , b ] a + b") )]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenInlineDelegateParameterNameIsUnexpected(string input)
    {
        var builder = new ParsedExpressionFactoryBuilder()
            .AddTypeDeclaration<string>( "string" )
            .AddBinaryOperator( "+", new MockBinaryOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.NestedExpressionFailure ) );
    }

    [Theory]
    [InlineData( "[ string string ] 'foo'" )]
    [InlineData( "[ string a string ] a" )]
    [InlineData( "[ string a , string string ] a" )]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenInlineDelegateParameterTypeIsUnexpected(string input)
    {
        var builder = new ParsedExpressionFactoryBuilder().AddTypeDeclaration<string>( "string" );
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.NestedExpressionFailure ) );
    }

    [Theory]
    [InlineData( "[ string , ] 'foo'" )]
    [InlineData( "[ , ] 'foo'" )]
    [InlineData( "[ string a , , ] a" )]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenInlineDelegateParameterSeparatorIsUnexpected(string input)
    {
        var builder = new ParsedExpressionFactoryBuilder().AddTypeDeclaration<string>( "string" );
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.NestedExpressionFailure ) );
    }

    [Theory]
    [InlineData( "[ string a , ] a" )]
    [InlineData( "[ string a , string ] a" )]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenInlineDelegateParameterEndIsUnexpected(string input)
    {
        var builder = new ParsedExpressionFactoryBuilder().AddTypeDeclaration<string>( "string" );
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.NestedExpressionFailure ) );
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenInlineDelegateParameterNameIsInvalid()
    {
        var input = "[string +] 'foo'";
        var builder = new ParsedExpressionFactoryBuilder().AddTypeDeclaration<string>( "string" );
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.NestedExpressionFailure ) );
    }

    [Theory]
    [InlineData( "[ string" )]
    [InlineData( "[ string a" )]
    [InlineData( "[ string a ," )]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenInlineDelegateParametersAreUnfinished(string input)
    {
        var builder = new ParsedExpressionFactoryBuilder().AddTypeDeclaration<string>( "string" );
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should().ThrowExactly<ParsedExpressionCreationException>();
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenInlineDelegateParameterEndWithUnexpectedInParentClosedParenthesis()
    {
        var input = "[] 'foo' )";
        var builder = new ParsedExpressionFactoryBuilder();
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.UnexpectedClosedParenthesis ) );
    }

    [Fact]
    public void
        Create_ShouldThrowParsedExpressionCreationException_WhenInlineDelegateParameterEndWithUnexpectedInParentClosedSquareBracket()
    {
        var input = "[] 'foo' ]";
        var builder = new ParsedExpressionFactoryBuilder();
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.UnexpectedClosedSquareBracket ) );
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenInlineDelegateParameterEndWithUnexpectedInParentElementSeparator()
    {
        var input = "[] 'foo' ,";
        var builder = new ParsedExpressionFactoryBuilder();
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.UnexpectedElementSeparator ) );
    }

    [Fact]
    public void DelegateInvoke_ShouldReturnCorrectResult_WhenInlineDelegateParameterEndWithLineSeparator()
    {
        var input = "[] 'foo' ;";
        var builder = new ParsedExpressionFactoryBuilder();
        var sut = builder.Build();

        var expression = sut.Create<string, Func<string>>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke()();

        result.Should().Be( "foo" );
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenInlineDelegateParameterEndsPrematurelyDueToClosedParenthesis()
    {
        var input = "( [] 'foo' + )";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new MockBinaryOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.NestedExpressionFailure ) );
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenInlineDelegateParameterEndsPrematurelyDueToClosedSquareBracket()
    {
        var input = "string[ [] 'foo' + ]";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddTypeDeclaration<string>( "string" )
            .AddBinaryOperator( "+", new MockBinaryOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.NestedExpressionFailure ) );
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenInlineDelegateParameterEndsPrematurelyDueToElementSeparator()
    {
        var input = "string[ [] 'foo' + , 'bar' ]";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddTypeDeclaration<string>( "string" )
            .AddBinaryOperator( "+", new MockBinaryOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.NestedExpressionFailure ) );
    }

    [Theory]
    [InlineData( "[] 'foo' +" )]
    [InlineData( "[] 'foo' + ;" )]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenInlineDelegateParameterEndsPrematurely(string input)
    {
        var builder = new ParsedExpressionFactoryBuilder()
            .AddTypeDeclaration<string>( "string" )
            .AddBinaryOperator( "+", new MockBinaryOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should().ThrowExactly<ParsedExpressionCreationException>();
    }

    [Fact]
    public void DelegateInvoke_ShouldReturnCorrectResult_WhenDelegateCapturesAnArgument()
    {
        var input = "[ string a ] a + b";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddTypeDeclaration<string>( "string" )
            .AddBinaryOperator( "+", new MockBinaryOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var sut = builder.Build();

        var expression = sut.Create<string, Func<string, string>>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke( "bar" );
        var delegateResult = result( "foo" );

        delegateResult.Should().Be( "( foo|BiOp|bar )" );
    }

    [Fact]
    public void DelegateInvoke_ShouldReturnCorrectResult_WhenDelegateCapturesAnArgumentAndIsInvoked()
    {
        var input = "( [ string a ] a + b ) ( 'foo' )";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddTypeDeclaration<string>( "string" )
            .AddBinaryOperator( "+", new MockBinaryOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var sut = builder.Build();

        var expression = sut.Create<string, string>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke( "bar" );

        result.Should().Be( "( foo|BiOp|bar )" );
    }

    [Fact]
    public void DelegateInvoke_ShouldReturnCorrectResult_WhenMultipleDelegatesCaptureArguments()
    {
        var input = "( [ string a ] b + a ) ( 'bar' ) + ( [ string a ] a + c ) ( 'foo' )";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddTypeDeclaration<string>( "string" )
            .AddBinaryOperator( "+", new MockBinaryOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var sut = builder.Build();

        var expression = sut.Create<string, string>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke( "foo", "qux" );

        result.Should().Be( "( ( foo|BiOp|bar )|BiOp|( foo|BiOp|qux ) )" );
    }

    [Theory]
    [InlineData( "[] [] [ string a ] a + b" )]
    [InlineData( "[] [] [ string a ] a + b ;" )]
    public void DelegateInvoke_ShouldReturnCorrectResult_WhenNestedDelegateCapturesAnArgument(string input)
    {
        var builder = new ParsedExpressionFactoryBuilder()
            .AddTypeDeclaration<string>( "string" )
            .AddBinaryOperator( "+", new MockBinaryOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var sut = builder.Build();

        var expression = sut.Create<string, Func<Func<Func<string, string>>>>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke( "bar" );
        var delegateResult = result()()( "foo" );

        delegateResult.Should().Be( "( foo|BiOp|bar )" );
    }

    [Fact]
    public void DelegateInvoke_ShouldReturnCorrectResult_WhenNestedDelegateCapturesParentParameter()
    {
        var input = "[ string a ] [ string b ] [ string c ] a + b + c";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddTypeDeclaration<string>( "string" )
            .AddBinaryOperator( "+", new MockBinaryOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var sut = builder.Build();

        var expression = sut.Create<string, Func<string, Func<string, Func<string, string>>>>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke();
        var delegateResult = result( "foo" )( "bar" )( "qux" );

        delegateResult.Should().Be( "( ( foo|BiOp|bar )|BiOp|qux )" );
    }

    [Fact]
    public void DelegateInvoke_ShouldReturnCorrectResult_WhenNestedDelegateCapturesParentParameterAndParentIsInvoked()
    {
        var input = "( [ string a ] [ string b ] [ string c ] a + b + c ) ( 'foo' ) ( 'bar' ) ( 'qux' )";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddTypeDeclaration<string>( "string" )
            .AddBinaryOperator( "+", new MockBinaryOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var sut = builder.Build();

        var expression = sut.Create<string, string>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke();

        result.Should().Be( "( ( foo|BiOp|bar )|BiOp|qux )" );
    }

    [Fact]
    public void DelegateInvoke_ShouldReturnCorrectResult_WhenNestedDelegateCapturesParentParameterAndIsInvoked()
    {
        var input = "[ string a ] [ string b ] ( [ string c ] a + b + c ) ( 'qux' )";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddTypeDeclaration<string>( "string" )
            .AddBinaryOperator( "+", new MockBinaryOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var sut = builder.Build();

        var expression = sut.Create<string, Func<string, Func<string, string>>>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke();
        var delegateResult = result( "foo" )( "bar" );

        delegateResult.Should().Be( "( ( foo|BiOp|bar )|BiOp|qux )" );
    }

    [Fact]
    public void DelegateInvoke_ShouldReturnCorrectResult_WhenNestedDelegateCapturesParentParameterAndArgument()
    {
        var input = "[ string a ] [ string b ] [ string c ] a + b + c + d";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddTypeDeclaration<string>( "string" )
            .AddBinaryOperator( "+", new MockBinaryOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var sut = builder.Build();

        var expression = sut.Create<string, Func<string, Func<string, Func<string, string>>>>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke( "baz" );
        var delegateResult = result( "foo" )( "bar" )( "qux" );

        delegateResult.Should().Be( "( ( ( foo|BiOp|bar )|BiOp|qux )|BiOp|baz )" );
    }

    [Fact]
    public void DelegateInvoke_ShouldReturnCorrectResult_WhenNestedDelegateCapturesParentParameterAndArgumentAndParentIsInvoked()
    {
        var input = "( [ string a ] [ string b ] [ string c ] a + b + c + d ) ( 'foo' ) ( 'bar' ) ( 'qux' )";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddTypeDeclaration<string>( "string" )
            .AddBinaryOperator( "+", new MockBinaryOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var sut = builder.Build();

        var expression = sut.Create<string, string>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke( "baz" );

        result.Should().Be( "( ( ( foo|BiOp|bar )|BiOp|qux )|BiOp|baz )" );
    }

    [Fact]
    public void DelegateInvoke_ShouldReturnCorrectResult_WhenNestedDelegateCapturesParentParameterAndArgumentAndIsInvoked()
    {
        var input = "[ string a ] [ string b ] ( [ string c ] a + b + c + d ) ( 'qux' )";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddTypeDeclaration<string>( "string" )
            .AddBinaryOperator( "+", new MockBinaryOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var sut = builder.Build();

        var expression = sut.Create<string, Func<string, Func<string, string>>>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke( "baz" );
        var delegateResult = result( "foo" )( "bar" );

        delegateResult.Should().Be( "( ( ( foo|BiOp|bar )|BiOp|qux )|BiOp|baz )" );
    }

    [Fact]
    public void DelegateInvoke_ShouldReturnCorrectResult_WhenDelegateContainsMultipleNestedDelegatesWithClosure()
    {
        var input = "[ string p1 ] p1 + ( [ string p2 ] p1 + p2 + a ) ( 'bar' ) + ( [ string p3 ] p1 + p3 + a ) ( 'baz' )";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddTypeDeclaration<string>( "string" )
            .AddBinaryOperator( "+", new MockBinaryOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var sut = builder.Build();

        var expression = sut.Create<string, Func<string, string>>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke( "qux" );
        var delegateResult = result( "foo" );

        delegateResult.Should().Be( "( ( foo|BiOp|( ( foo|BiOp|bar )|BiOp|qux ) )|BiOp|( ( foo|BiOp|baz )|BiOp|qux ) )" );
    }

    [Fact]
    public void DelegateInvoke_ShouldReturnCorrectResult_WhenDelegateWithClosureContainsNestedStaticDelegate()
    {
        var input = "[ string a ] a + b + ( [] 'qux' ) ( ) + ( [] a ) ( )";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddTypeDeclaration<string>( "string" )
            .AddBinaryOperator( "+", new MockBinaryOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var sut = builder.Build();

        var expression = sut.Create<string, Func<string, string>>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke( "bar" );
        var delegateResult = result( "foo" );

        delegateResult.Should().Be( "( ( ( foo|BiOp|bar )|BiOp|qux )|BiOp|foo )" );
    }

    [Fact]
    public void DelegateInvoke_ShouldReturnCorrectResult_WhenStaticDelegateHasBeenOptimizedAway()
    {
        var input = "0 * ( [ int a ] a * a ) ( 10 ) + b";
        var builder = new ParsedExpressionFactoryBuilder()
            .SetNumberParserProvider( p => ParsedExpressionNumberParser.CreateDefaultInt32( p.Configuration ) )
            .AddTypeDeclaration<int>( "int" )
            .AddBinaryOperator( "*", new ParsedExpressionMultiplyInt32Operator() )
            .AddBinaryOperator( "+", new ParsedExpressionAddOperator() )
            .SetBinaryOperatorPrecedence( "*", 1 )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var sut = builder.Build();

        var expression = sut.Create<int, int>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke( 5 );

        result.Should().Be( 5 );
    }

    [Fact]
    public void DelegateInvoke_ShouldReturnCorrectResult_WhenDelegateWithCapturedArgumentHasBeenOptimizedAway()
    {
        var input = "0 * ( [ int a ] a * b ) ( 10 ) + c";
        var builder = new ParsedExpressionFactoryBuilder()
            .SetNumberParserProvider( p => ParsedExpressionNumberParser.CreateDefaultInt32( p.Configuration ) )
            .AddTypeDeclaration<int>( "int" )
            .AddBinaryOperator( "*", new ParsedExpressionMultiplyInt32Operator() )
            .AddBinaryOperator( "+", new ParsedExpressionAddOperator() )
            .SetBinaryOperatorPrecedence( "*", 1 )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var sut = builder.Build();

        var expression = sut.Create<int, int>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke( 5 );

        result.Should().Be( 5 );
    }

    [Fact]
    public void DelegateInvoke_ShouldReturnCorrectResult_WhenNestedStaticDelegateHasBeenOptimizedAway()
    {
        var input = "[ int p1 ] 0 * ( [ int p2 ] p2 * p2 ) ( 10 ) + p1 + a";
        var builder = new ParsedExpressionFactoryBuilder()
            .SetNumberParserProvider( p => ParsedExpressionNumberParser.CreateDefaultInt32( p.Configuration ) )
            .AddTypeDeclaration<int>( "int" )
            .AddBinaryOperator( "*", new ParsedExpressionMultiplyInt32Operator() )
            .AddBinaryOperator( "+", new ParsedExpressionAddOperator() )
            .SetBinaryOperatorPrecedence( "*", 1 )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var sut = builder.Build();

        var expression = sut.Create<int, Func<int, int>>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke( 5 );
        var delegateResult = result( 100 );

        delegateResult.Should().Be( 105 );
    }

    [Fact]
    public void DelegateInvoke_ShouldReturnCorrectResult_WhenNestedDelegateWithClosureHasBeenOptimizedAway()
    {
        var input = "[ int p1 ] 0 * ( [ int p2 ] p1 + p2 + a ) ( 10 ) + p1 + b";
        var builder = new ParsedExpressionFactoryBuilder()
            .SetNumberParserProvider( p => ParsedExpressionNumberParser.CreateDefaultInt32( p.Configuration ) )
            .AddTypeDeclaration<int>( "int" )
            .AddBinaryOperator( "*", new ParsedExpressionMultiplyInt32Operator() )
            .AddBinaryOperator( "+", new ParsedExpressionAddOperator() )
            .SetBinaryOperatorPrecedence( "*", 1 )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var sut = builder.Build();

        var expression = sut.Create<int, Func<int, int>>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke( 5 );
        var delegateResult = result( 100 );

        delegateResult.Should().Be( 105 );
    }

    [Fact]
    public void DelegateInvoke_ShouldReturnCorrectResult_WhenDelegateWithClosureWithNestedDelegatesHasBeenOptimizedAway()
    {
        var input = "0 * ( [ int p1 ] p1 + a + ( [ int p2 ] p2 + b ) ( 10 ) + ( [ int p2 ] p2 + c ) ( 20 ) ) ( 30 ) + d";
        var builder = new ParsedExpressionFactoryBuilder()
            .SetNumberParserProvider( p => ParsedExpressionNumberParser.CreateDefaultInt32( p.Configuration ) )
            .AddTypeDeclaration<int>( "int" )
            .AddBinaryOperator( "*", new ParsedExpressionMultiplyInt32Operator() )
            .AddBinaryOperator( "+", new ParsedExpressionAddOperator() )
            .SetBinaryOperatorPrecedence( "*", 1 )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var sut = builder.Build();

        var expression = sut.Create<int, int>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke( 5 );

        result.Should().Be( 5 );
    }

    [Fact]
    public void DelegateInvoke_ShouldReturnCorrectResult_WhenDelegatesCaptureArgumentsWhenOtherArgumentsHaveBeenRemoved()
    {
        var input = "a * 0 + b * 0 + ( [ int p1 ] p1 * c + ( [ int p2 ] p2 * d ) ( 20 ) ) ( 10 )";
        var builder = new ParsedExpressionFactoryBuilder()
            .SetNumberParserProvider( p => ParsedExpressionNumberParser.CreateDefaultInt32( p.Configuration ) )
            .AddTypeDeclaration<int>( "int" )
            .AddBinaryOperator( "*", new ParsedExpressionMultiplyInt32Operator() )
            .AddBinaryOperator( "+", new ParsedExpressionAddOperator() )
            .SetBinaryOperatorPrecedence( "*", 1 )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var sut = builder.Build();

        var expression = sut.Create<int, int>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke( 2, 3 );

        result.Should().Be( 80 );
    }

    [Fact]
    public void DelegateInvoke_ShouldReturnCorrectResult_WhenIndirectNestedDelegateCapturesParameters()
    {
        var input = "[ string a ] a + foo( ( [] a + b )() )";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddTypeDeclaration<string>( "string" )
            .AddFunction( "foo", ParsedExpressionFunction.Create( (string a) => $"foo({a})" ) )
            .AddBinaryOperator( "+", new MockBinaryOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var sut = builder.Build();

        var expression = sut.Create<string, Func<string, string>>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke( "bar" );
        var delegateResult = result( "foo" );

        delegateResult.Should().Be( "( foo|BiOp|foo(( foo|BiOp|bar )) )" );
    }

    [Theory]
    [MethodData( nameof( ParsedExpressionFactoryTestsData.GetDelegateNestedInStaticDelegateCapturesManyParametersData ) )]
    public void DelegateInvoke_ShouldReturnCorrectResult_WhenDelegateNestedInStaticDelegateCapturesManyParameters(
        string input,
        int expected)
    {
        var builder = new ParsedExpressionFactoryBuilder()
            .SetNumberParserProvider( p => ParsedExpressionNumberParser.CreateDefaultInt32( p.Configuration ) )
            .AddTypeDeclaration<int>( "int" )
            .AddBinaryOperator( "+", new ParsedExpressionAddOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var sut = builder.Build();

        var expression = sut.Create<int, Func<int>>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke();
        var delegateResult = result();

        delegateResult.Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( ParsedExpressionFactoryTestsData.GetDelegateNestedInNonStaticDelegateCapturesManyParametersData ) )]
    public void DelegateInvoke_ShouldReturnCorrectResult_WhenDelegateNestedInNonStaticDelegateCapturesManyParameters(
        string input,
        int expected)
    {
        var builder = new ParsedExpressionFactoryBuilder()
            .SetNumberParserProvider( p => ParsedExpressionNumberParser.CreateDefaultInt32( p.Configuration ) )
            .AddTypeDeclaration<int>( "int" )
            .AddBinaryOperator( "+", new ParsedExpressionAddOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var sut = builder.Build();

        var expression = sut.Create<int, Func<int>>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke( 10 );
        var delegateResult = result();

        delegateResult.Should().Be( expected );
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenDelegateWithClosureHasMoreThanFifteenParameters()
    {
        var input =
            @"( [ int a , int b , int c , int d , int e , int f , int g , int h , int i , int j , int k , int l , int m , int n , int o , int p ]
[] a + b + c + d + e + f + g + h + i + j + k + l + m + n + o + p + x ) ( 1 , 2 , 3 , 4 , 5 , 6 , 7 , 8 , 9 , 10 , 11 , 12 , 13 , 14 , 15 , 16 )";

        var builder = new ParsedExpressionFactoryBuilder()
            .SetNumberParserProvider( p => ParsedExpressionNumberParser.CreateDefaultInt32( p.Configuration ) )
            .AddTypeDeclaration<int>( "int" )
            .AddBinaryOperator( "+", new ParsedExpressionAddOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<int, Func<int>>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.NestedExpressionFailure ) );
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
    [InlineData( "a ." )]
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
    [InlineData( "a . ;" )]
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
    public void DelegateInvoke_ShouldReturnCorrectResult_WhenParameterlessMethodTargetIsConstant()
    {
        var input = "const.PublicMethodZero()";
        var value = new TestParameter( "privateField", "privateProperty", "publicField", "publicProperty", next: null );
        var builder = new ParsedExpressionFactoryBuilder()
            .AddConstant( "const", new ParsedExpressionConstant<TestParameter>( value ) );

        var sut = builder.Build();

        var expression = sut.Create<string, string>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke();

        result.Should().Be( value.PublicMethodZero() );
    }

    [Fact]
    public void DelegateInvoke_ShouldReturnCorrectResult_WhenMethodTargetAndParametersAreConstantWithEnabledConstantsFolding()
    {
        var input = "const.IntTest( 1 , 2 , 3 ) * a";
        var value = new TestParameter( "privateField", "privateProperty", "publicField", "publicProperty", next: null );
        var builder = new ParsedExpressionFactoryBuilder()
            .AddTypeDeclaration<string>( "string" )
            .AddConstant( "const", new ParsedExpressionConstant<TestParameter>( value ) )
            .SetMethodCallProvider( c => new ParsedExpressionMethodCall( c, foldConstantsWhenPossible: true ) )
            .SetNumberParserProvider( p => ParsedExpressionNumberParser.CreateDefaultInt32( p.Configuration ) )
            .AddBinaryOperator( "*", new ParsedExpressionMultiplyInt32Operator() )
            .SetBinaryOperatorPrecedence( "*", 1 );

        var sut = builder.Build();

        var expression = sut.Create<int, int>( input );
        var @delegate = expression.Compile();

        using ( new AssertionScope() )
        {
            expression.UnboundArguments.Should().BeEmpty();
            var result = @delegate.Invoke();
            result.Should().Be( 0 );
        }
    }

    [Fact]
    public void DelegateInvoke_ShouldReturnCorrectResult_WhenMethodTargetAndParametersAreConstantWithDisabledConstantsFolding()
    {
        var input = "const.IntTest( 1 , 2 , 3 ) * a";
        var value = new TestParameter( "privateField", "privateProperty", "publicField", "publicProperty", next: null );
        var builder = new ParsedExpressionFactoryBuilder()
            .AddTypeDeclaration<string>( "string" )
            .AddConstant( "const", new ParsedExpressionConstant<TestParameter>( value ) )
            .SetMethodCallProvider( c => new ParsedExpressionMethodCall( c, foldConstantsWhenPossible: false ) )
            .SetNumberParserProvider( p => ParsedExpressionNumberParser.CreateDefaultInt32( p.Configuration ) )
            .AddBinaryOperator( "*", new ParsedExpressionMultiplyInt32Operator() )
            .SetBinaryOperatorPrecedence( "*", 1 );

        var sut = builder.Build();

        var expression = sut.Create<int, int>( input );
        var @delegate = expression.Compile();

        using ( new AssertionScope() )
        {
            expression.UnboundArguments.Contains( "a" ).Should().BeTrue();
            var result = @delegate.Invoke( 100 );
            result.Should().Be( 0 );
        }
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
    public void DelegateInvoke_ShouldReturnCorrectResult_ForFieldMemberAccessOnConstantValueWithEnabledConstantsFolding()
    {
        var input = "const.PublicField.Length * a";
        var constant = new TestParameter( "privateField", "privateProperty", string.Empty, "publicProperty", next: null );
        var builder = new ParsedExpressionFactoryBuilder()
            .AddConstant( "const", new ParsedExpressionConstant<TestParameter>( constant ) )
            .SetMemberAccessProvider( c => new ParsedExpressionMemberAccess( c, foldConstantsWhenPossible: true ) )
            .SetNumberParserProvider( p => ParsedExpressionNumberParser.CreateDefaultInt32( p.Configuration ) )
            .AddBinaryOperator( "*", new ParsedExpressionMultiplyInt32Operator() )
            .SetBinaryOperatorPrecedence( "*", 1 );

        var sut = builder.Build();

        var expression = sut.Create<int, int>( input );
        var @delegate = expression.Compile();

        using ( new AssertionScope() )
        {
            expression.UnboundArguments.Should().BeEmpty();
            var result = @delegate.Invoke();
            result.Should().Be( 0 );
        }
    }

    [Fact]
    public void DelegateInvoke_ShouldReturnCorrectResult_ForFieldMemberAccessOnConstantValueWithDisabledConstantsFolding()
    {
        var input = "const.PublicField.Length * a";
        var constant = new TestParameter( "privateField", "privateProperty", string.Empty, "publicProperty", next: null );
        var builder = new ParsedExpressionFactoryBuilder()
            .AddConstant( "const", new ParsedExpressionConstant<TestParameter>( constant ) )
            .SetMemberAccessProvider( c => new ParsedExpressionMemberAccess( c, foldConstantsWhenPossible: false ) )
            .SetNumberParserProvider( p => ParsedExpressionNumberParser.CreateDefaultInt32( p.Configuration ) )
            .AddBinaryOperator( "*", new ParsedExpressionMultiplyInt32Operator() )
            .SetBinaryOperatorPrecedence( "*", 1 );

        var sut = builder.Build();

        var expression = sut.Create<int, int>( input );
        var @delegate = expression.Compile();

        using ( new AssertionScope() )
        {
            expression.UnboundArguments.Contains( "a" ).Should().BeTrue();
            var result = @delegate.Invoke( 100 );
            result.Should().Be( 0 );
        }
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
    public void DelegateInvoke_ShouldReturnCorrectResult_ForPropertyMemberAccessOnConstantValueWithEnabledConstantsFolding()
    {
        var input = "const.PublicProperty.Length * a";
        var constant = new TestParameter( "privateField", "privateProperty", "publicField", string.Empty, next: null );
        var builder = new ParsedExpressionFactoryBuilder()
            .AddConstant( "const", new ParsedExpressionConstant<TestParameter>( constant ) )
            .SetMemberAccessProvider( c => new ParsedExpressionMemberAccess( c, foldConstantsWhenPossible: true ) )
            .SetNumberParserProvider( p => ParsedExpressionNumberParser.CreateDefaultInt32( p.Configuration ) )
            .AddBinaryOperator( "*", new ParsedExpressionMultiplyInt32Operator() )
            .SetBinaryOperatorPrecedence( "*", 1 );

        var sut = builder.Build();

        var expression = sut.Create<int, int>( input );
        var @delegate = expression.Compile();

        using ( new AssertionScope() )
        {
            expression.UnboundArguments.Should().BeEmpty();
            var result = @delegate.Invoke();
            result.Should().Be( 0 );
        }
    }

    [Fact]
    public void DelegateInvoke_ShouldReturnCorrectResult_ForPropertyMemberAccessOnConstantValueWithDisabledConstantsFolding()
    {
        var input = "const.PublicProperty.Length * a";
        var constant = new TestParameter( "privateField", "privateProperty", "publicField", string.Empty, next: null );
        var builder = new ParsedExpressionFactoryBuilder()
            .AddConstant( "const", new ParsedExpressionConstant<TestParameter>( constant ) )
            .SetMemberAccessProvider( c => new ParsedExpressionMemberAccess( c, foldConstantsWhenPossible: false ) )
            .SetNumberParserProvider( p => ParsedExpressionNumberParser.CreateDefaultInt32( p.Configuration ) )
            .AddBinaryOperator( "*", new ParsedExpressionMultiplyInt32Operator() )
            .SetBinaryOperatorPrecedence( "*", 1 );

        var sut = builder.Build();

        var expression = sut.Create<int, int>( input );
        var @delegate = expression.Compile();

        using ( new AssertionScope() )
        {
            expression.UnboundArguments.Contains( "a" ).Should().BeTrue();
            var result = @delegate.Invoke( 100 );
            result.Should().Be( 0 );
        }
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
    public void DelegateInvoke_ShouldReturnCorrectResult_WhenArrayIndexerTargetAndParametersAreConstantWithEnabledConstantsFolding()
    {
        var input = "string[ '' ][ 0 ].Length * a";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddTypeDeclaration<string>( "string" )
            .SetIndexerCallProvider( c => new ParsedExpressionIndexerCall( c, foldConstantsWhenPossible: true ) )
            .SetNumberParserProvider( p => ParsedExpressionNumberParser.CreateDefaultInt32( p.Configuration ) )
            .AddBinaryOperator( "*", new ParsedExpressionMultiplyInt32Operator() )
            .SetBinaryOperatorPrecedence( "*", 1 );

        var sut = builder.Build();

        var expression = sut.Create<int, int>( input );
        var @delegate = expression.Compile();

        using ( new AssertionScope() )
        {
            expression.UnboundArguments.Should().BeEmpty();
            var result = @delegate.Invoke();
            result.Should().Be( 0 );
        }
    }

    [Fact]
    public void DelegateInvoke_ShouldReturnCorrectResult_WhenArrayIndexerTargetAndParametersAreConstantWithDisabledConstantsFolding()
    {
        var input = "string[ '' ][ 0 ].Length * a";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddTypeDeclaration<string>( "string" )
            .SetIndexerCallProvider( c => new ParsedExpressionIndexerCall( c, foldConstantsWhenPossible: false ) )
            .SetNumberParserProvider( p => ParsedExpressionNumberParser.CreateDefaultInt32( p.Configuration ) )
            .AddBinaryOperator( "*", new ParsedExpressionMultiplyInt32Operator() )
            .SetBinaryOperatorPrecedence( "*", 1 );

        var sut = builder.Build();

        var expression = sut.Create<int, int>( input );
        var @delegate = expression.Compile();

        using ( new AssertionScope() )
        {
            expression.UnboundArguments.Contains( "a" ).Should().BeTrue();
            var result = @delegate.Invoke( 100 );
            result.Should().Be( 0 );
        }
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
    public void DelegateInvoke_ShouldReturnCorrectResult_WhenObjectIndexerTargetAndParametersAreConstantWithEnabledConstantsFolding()
    {
        var input = "const[ 0 ] * a";
        var constant = new TestParameter( "privateField", "privateProperty", "publicField", string.Empty, next: null );
        var builder = new ParsedExpressionFactoryBuilder()
            .AddConstant( "const", new ParsedExpressionConstant<TestParameter>( constant ) )
            .SetIndexerCallProvider( c => new ParsedExpressionIndexerCall( c, foldConstantsWhenPossible: true ) )
            .SetNumberParserProvider( p => ParsedExpressionNumberParser.CreateDefaultInt32( p.Configuration ) )
            .AddBinaryOperator( "*", new ParsedExpressionMultiplyInt32Operator() )
            .SetBinaryOperatorPrecedence( "*", 1 );

        var sut = builder.Build();

        var expression = sut.Create<int, int>( input );
        var @delegate = expression.Compile();

        using ( new AssertionScope() )
        {
            expression.UnboundArguments.Should().BeEmpty();
            var result = @delegate.Invoke();
            result.Should().Be( 0 );
        }
    }

    [Fact]
    public void DelegateInvoke_ShouldReturnCorrectResult_WhenObjectIndexerTargetAndParametersAreConstantWithDisabledConstantsFolding()
    {
        var input = "const[ 0 ] * a";
        var constant = new TestParameter( "privateField", "privateProperty", "publicField", string.Empty, next: null );
        var builder = new ParsedExpressionFactoryBuilder()
            .AddConstant( "const", new ParsedExpressionConstant<TestParameter>( constant ) )
            .SetIndexerCallProvider( c => new ParsedExpressionIndexerCall( c, foldConstantsWhenPossible: false ) )
            .SetNumberParserProvider( p => ParsedExpressionNumberParser.CreateDefaultInt32( p.Configuration ) )
            .AddBinaryOperator( "*", new ParsedExpressionMultiplyInt32Operator() )
            .SetBinaryOperatorPrecedence( "*", 1 );

        var sut = builder.Build();

        var expression = sut.Create<int, int>( input );
        var @delegate = expression.Compile();

        using ( new AssertionScope() )
        {
            expression.UnboundArguments.Contains( "a" ).Should().BeTrue();
            var result = @delegate.Invoke( 100 );
            result.Should().Be( 0 );
        }
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
    public void DelegateInvoke_ShouldReturnCorrectResult_WhenInvokeVariadicIsCalledDirectly()
    {
        var input = "INVOKE( delegate , 'foo' , 'bar' )";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddConstant( "delegate", new ParsedExpressionConstant<Func<string, string, string>>( (a, b) => a + b ) );

        var sut = builder.Build();

        var expression = sut.Create<string, string>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke();

        result.Should().Be( "foobar" );
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenInvokeVariadicReceivesZeroParameters()
    {
        var input = "INVOKE()";
        var builder = new ParsedExpressionFactoryBuilder();

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) );
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenInvokeVariadicReceivesNonInvocableFirstParameter()
    {
        var input = "INVOKE( 'foo' )";
        var builder = new ParsedExpressionFactoryBuilder();

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) );
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenInvokeVariadicReceivesNullInvocableFirstParameter()
    {
        var input = "INVOKE( delegate )";
        var builder = new ParsedExpressionFactoryBuilder().AddConstant( "delegate", new ParsedExpressionConstant<Func<string>?>( null ) );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) );
    }

    [Fact]
    public void Create_ShouldReturnCorrectResult_WhenAssignmentIsNotExpectedButHasConstructs()
    {
        var input = "a = b";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "=", new MockBinaryOperator() )
            .SetBinaryOperatorPrecedence( "=", 1 );

        var sut = builder.Build();

        var expression = sut.Create<string, string>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke( "foo", "bar" );

        result.Should().Be( "( foo|BiOp|bar )" );
    }

    [Fact]
    public void DelegateInvoke_ShouldReturnCorrectResult_WhenPostfixUnaryOperatorAmbiguityIsResolvedByLineSeparator()
    {
        var input = "b + ;";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new MockBinaryOperator() )
            .AddPostfixUnaryOperator( "+", new MockPostfixUnaryOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 )
            .SetPostfixUnaryConstructPrecedence( "+", 1 );

        var sut = builder.Build();

        var expression = sut.Create<string, string>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke( "foo" );

        result.Should().Be( "( foo|PostOp )" );
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenLineSeparatorIsEncounteredInNonLocalTermNestedExpression()
    {
        var input = "foo( a , b ;";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddFunction( "foo", new MockFunctionWithThreeParameters() );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.NestedExpressionFailure ) );
    }

    [Theory]
    [InlineData( "a ; ;" )]
    [InlineData( "a ; b" )]
    [InlineData( "a ; -" )]
    [InlineData( "a ; +" )]
    [InlineData( "a ; ^" )]
    [InlineData( "a ; [string]" )]
    [InlineData( "a ; ToString" )]
    [InlineData( "a ; int" )]
    [InlineData( "a ; (" )]
    [InlineData( "a ; )" )]
    [InlineData( "a ; [" )]
    [InlineData( "a ; ]" )]
    [InlineData( "a ; ." )]
    [InlineData( "a ; ," )]
    [InlineData( "a ; =" )]
    [InlineData( "a ; let" )]
    [InlineData( "a ; macro" )]
    [InlineData( "a ; const" )]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenAnySymbolIsEncounteredAfterLastLineSeparator(string input)
    {
        var builder = new ParsedExpressionFactoryBuilder()
            .AddPrefixUnaryOperator( "-", new MockPrefixUnaryOperator() )
            .AddPostfixUnaryOperator( "^", new MockPostfixUnaryOperator() )
            .AddBinaryOperator( "+", new MockBinaryOperator() )
            .AddPrefixTypeConverter( "[string]", new MockPrefixTypeConverter() )
            .AddPostfixTypeConverter( "ToString", new MockPostfixTypeConverter() )
            .AddConstant( "const", new ZeroConstant() )
            .AddTypeDeclaration<int>( "int" )
            .SetBinaryOperatorPrecedence( "+", 1 )
            .SetPrefixUnaryConstructPrecedence( "-", 1 )
            .SetPostfixUnaryConstructPrecedence( "^", 1 )
            .SetPrefixUnaryConstructPrecedence( "[string]", 1 )
            .SetPostfixUnaryConstructPrecedence( "ToString", 1 );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should().ThrowExactly<ParsedExpressionCreationException>();
    }

    [Theory]
    [InlineData( "let ;" )]
    [InlineData( "let -" )]
    [InlineData( "let +" )]
    [InlineData( "let ^" )]
    [InlineData( "let [string]" )]
    [InlineData( "let ToString" )]
    [InlineData( "let int" )]
    [InlineData( "let (" )]
    [InlineData( "let )" )]
    [InlineData( "let [" )]
    [InlineData( "let ]" )]
    [InlineData( "let ." )]
    [InlineData( "let ," )]
    [InlineData( "let =" )]
    [InlineData( "let let" )]
    [InlineData( "let macro" )]
    [InlineData( "let const" )]
    [InlineData( "let a ;" )]
    [InlineData( "let a -" )]
    [InlineData( "let a +" )]
    [InlineData( "let a ^" )]
    [InlineData( "let a [string]" )]
    [InlineData( "let a ToString" )]
    [InlineData( "let a int" )]
    [InlineData( "let a (" )]
    [InlineData( "let a )" )]
    [InlineData( "let a [" )]
    [InlineData( "let a ]" )]
    [InlineData( "let a ." )]
    [InlineData( "let a ," )]
    [InlineData( "let a let" )]
    [InlineData( "let a macro" )]
    [InlineData( "let a const" )]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenVariableDeclarationIsNotFollowedByItsNameAndAssignment(string input)
    {
        var builder = new ParsedExpressionFactoryBuilder()
            .AddPrefixUnaryOperator( "-", new MockPrefixUnaryOperator() )
            .AddPostfixUnaryOperator( "^", new MockPostfixUnaryOperator() )
            .AddBinaryOperator( "+", new MockBinaryOperator() )
            .AddPrefixTypeConverter( "[string]", new MockPrefixTypeConverter() )
            .AddPostfixTypeConverter( "ToString", new MockPostfixTypeConverter() )
            .AddConstant( "const", new ZeroConstant() )
            .AddTypeDeclaration<int>( "int" )
            .SetBinaryOperatorPrecedence( "+", 1 )
            .SetPrefixUnaryConstructPrecedence( "-", 1 )
            .SetPostfixUnaryConstructPrecedence( "^", 1 )
            .SetPrefixUnaryConstructPrecedence( "[string]", 1 )
            .SetPostfixUnaryConstructPrecedence( "ToString", 1 );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should().ThrowExactly<ParsedExpressionCreationException>();
    }

    [Theory]
    [InlineData( "let a =" )]
    [InlineData( "let a = b" )]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenVariableDeclarationEndsWithoutLineSeparator(string input)
    {
        var builder = new ParsedExpressionFactoryBuilder();
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should().ThrowExactly<ParsedExpressionCreationException>();
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenVariableNameIsInvalid()
    {
        var input = "let ? = 'bar'; 'foo'";
        var builder = new ParsedExpressionFactoryBuilder();
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.NestedExpressionFailure ) );
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenVariableNameDuplicatesArgumentName()
    {
        var input = "let x = a ; let a = b ; 'foo'";
        var builder = new ParsedExpressionFactoryBuilder();
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.NestedExpressionFailure ) );
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenVariableNameDuplicatesMacroName()
    {
        var input = "macro x = a ; let x = b ; 'foo'";
        var builder = new ParsedExpressionFactoryBuilder();
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.NestedExpressionFailure ) );
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenDelegateParameterNameDuplicatesVariableName()
    {
        var input = "let x = a ; ( [ string x ] x + a )( 'foo' ) ;";
        var builder = new ParsedExpressionFactoryBuilder().AddTypeDeclaration<string>( "string" );
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.NestedExpressionFailure ) );
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenVariableDeclarationIsEmpty()
    {
        var input = "let a = ; 'foo'";
        var builder = new ParsedExpressionFactoryBuilder();
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.NestedExpressionFailure ) );
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenVariableIsYetUndeclaredAndUsesItself()
    {
        var input = "let a = a + 1 ; 'foo'";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new MockBinaryOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.NestedExpressionFailure ) );
    }

    [Fact]
    public void
        Create_ShouldThrowParsedExpressionCreationException_WhenVariableIsReassignedAndNewExpressionIsOfTypeNotAssignableToVariableType()
    {
        var input = "let a = b + 'foo' ; let a = 1 ; 'foo'";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new MockBinaryOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.LocalTermError ) );
    }

    [Fact]
    public void DelegateInvoke_ShouldReturnCorrectResult_WhenExpressionContainsSingleUsedVariable()
    {
        var input = "let v = a + 'bar' ; v + 'qux' ;";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new MockBinaryOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var sut = builder.Build();

        var expression = sut.Create<string, string>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke( "foo" );

        result.Should().Be( "( ( foo|BiOp|bar )|BiOp|qux )" );
    }

    [Fact]
    public void DelegateInvoke_ShouldReturnCorrectResult_WhenExpressionContainsVariableWithAmbiguousPostfixUnaryOperatorAtTheEnd()
    {
        var input = "let v = a + ; v + 'qux' ;";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new MockBinaryOperator() )
            .AddPostfixUnaryOperator( "+", new MockPostfixUnaryOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 )
            .SetPostfixUnaryConstructPrecedence( "+", 1 );

        var sut = builder.Build();

        var expression = sut.Create<string, string>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke( "foo" );

        result.Should().Be( "( ( foo|PostOp )|BiOp|qux )" );
    }

    [Fact]
    public void DelegateInvoke_ShouldReturnCorrectResult_WhenExpressionContainsMultipleVariables()
    {
        var input = "let x = a + 'bar'; let y = a + 'qux' ; x + y ;";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new MockBinaryOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var sut = builder.Build();

        var expression = sut.Create<string, string>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke( "foo" );

        result.Should().Be( "( ( foo|BiOp|bar )|BiOp|( foo|BiOp|qux ) )" );
    }

    [Fact]
    public void DelegateInvoke_ShouldReturnCorrectResult_WhenVariableIsReassignedWithCorrectType()
    {
        var input = "let x = a + 'bar'; let x = x + b ; x ;";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new MockBinaryOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var sut = builder.Build();

        var expression = sut.Create<string, string>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke( "foo", "qux" );

        result.Should().Be( "( ( foo|BiOp|bar )|BiOp|qux )" );
    }

    [Fact]
    public void DelegateInvoke_ShouldReturnCorrectResult_WhenSingleVariableIsUsedByMultipleOtherVariables()
    {
        var input = "let x = a + 'bar' ; let y = x + 'qux' ; let z = x + 'foo' ; x + y + z ;";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new MockBinaryOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var sut = builder.Build();

        var expression = sut.Create<string, string>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke( "foo" );

        result.Should().Be( "( ( ( foo|BiOp|bar )|BiOp|( ( foo|BiOp|bar )|BiOp|qux ) )|BiOp|( ( foo|BiOp|bar )|BiOp|foo ) )" );
    }

    [Fact]
    public void DelegateInvoke_ShouldReturnCorrectResult_WhenExpressionContainsUnusedVariable()
    {
        var input = "let x = a ; a + 'bar' ;";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new MockBinaryOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var sut = builder.Build();

        var expression = sut.Create<string, string>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke( "foo" );

        using ( new AssertionScope() )
        {
            result.Should().Be( "( foo|BiOp|bar )" );
            expression.Body.NodeType.Should().NotBe( ExpressionType.Block );
        }
    }

    [Fact]
    public void DelegateInvoke_ShouldReturnCorrectResult_WhenExpressionContainsVariableThatResolvesToConstantValue()
    {
        var input = "let x = 'foo' ; x + 'bar' ;";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new MockBinaryOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var sut = builder.Build();

        var expression = sut.Create<string, string>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke();

        using ( new AssertionScope() )
        {
            result.Should().Be( "( foo|BiOp|bar )" );
            expression.Body.NodeType.Should().NotBe( ExpressionType.Block );
        }
    }

    [Theory]
    [InlineData( "let x = a ; let y = x + 'bar' ; let x = a + 'qux' ; y + x ;", "( ( foo|BiOp|bar )|BiOp|( foo|BiOp|qux ) )" )]
    [InlineData( "let x = a ; let y = x ; let x = a + 'qux' ; y + x ;", "( foo|BiOp|( foo|BiOp|qux ) )" )]
    public void DelegateInvoke_ShouldReturnCorrectResult_WhenExpressionContainsVariableThatIsReassignedAfterBeingUsed(
        string input,
        string expected)
    {
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new MockBinaryOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var sut = builder.Build();

        var expression = sut.Create<string, string>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke( "foo" );

        result.Should().Be( expected );
    }

    [Fact]
    public void DelegateInvoke_ShouldReturnCorrectResult_WhenExpressionContainsVariableThatIsReassignedWithoutPreviousAssignmentBeingUsed()
    {
        var input = "let x = a ; let x = 'foo' + a ; x ;";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new MockBinaryOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var sut = builder.Build();

        var expression = sut.Create<string, string>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke( "bar" );

        using ( new AssertionScope() )
        {
            result.Should().Be( "( foo|BiOp|bar )" );
            expression.Body.NodeType.Should().Be( ExpressionType.Block );
            if ( expression.Body is not BlockExpression block )
                return;

            block.Expressions.Should().HaveCount( 2 );
        }
    }

    [Fact]
    public void DelegateInvoke_ShouldReturnCorrectResult_WhenExpressionContainsVariableReassignmentThatDoesNotChangeAnything()
    {
        var input = "let x = a ; let x = x ; x + 'bar' ;";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new MockBinaryOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var sut = builder.Build();

        var expression = sut.Create<string, string>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke( "foo" );

        using ( new AssertionScope() )
        {
            result.Should().Be( "( foo|BiOp|bar )" );
            expression.Body.NodeType.Should().Be( ExpressionType.Block );
            if ( expression.Body is not BlockExpression block )
                return;

            block.Expressions.Should().HaveCount( 2 );
        }
    }

    [Fact]
    public void DelegateInvoke_ShouldReturnCorrectResult_WhenExpressionContainsVariableWithAssignmentConstruct()
    {
        var input = "let x = = a ; x + 'bar' ;";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new MockBinaryOperator() )
            .AddPrefixUnaryOperator( "=", new MockPrefixUnaryOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 )
            .SetPrefixUnaryConstructPrecedence( "=", 1 );

        var sut = builder.Build();

        var expression = sut.Create<string, string>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke( "foo" );

        result.Should().Be( "( ( PreOp|foo )|BiOp|bar )" );
    }

    [Fact]
    public void
        DelegateInvoke_ShouldReturnCorrectResult_WhenExpressionContainsVariableReassignmentWithDifferentTypeButAssignableToVariableType()
    {
        var input = "let x = a ; let x = 'foo' + 'bar' ; x ;";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new MockBinaryOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var sut = builder.Build();

        var expression = sut.Create<object, object>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke();

        result.Should().Be( "( foo|BiOp|bar )" );
    }

    [Fact]
    public void DelegateInvoke_ShouldReturnCorrectResult_WhenExpressionContainsVariableCapturedByDelegate()
    {
        var input = "let x = a + 'bar' ; ( [] x )() ;";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new MockBinaryOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var sut = builder.Build();

        var expression = sut.Create<string, string>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke( "foo" );

        result.Should().Be( "( foo|BiOp|bar )" );
    }

    [Fact]
    public void DelegateInvoke_ShouldReturnCorrectResult_WhenExpressionContainsMultipleVariablesWithCapturingDelegates()
    {
        var input = "let x = [ string a ] a + b ; let y = [] x( 'qux' ) + b ; ( [] x( 'foo' ) + y() )() ;";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddTypeDeclaration<string>( "string" )
            .AddBinaryOperator( "+", new MockBinaryOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var sut = builder.Build();

        var expression = sut.Create<string, string>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke( "bar" );

        result.Should().Be( "( ( foo|BiOp|bar )|BiOp|( ( qux|BiOp|bar )|BiOp|bar ) )" );
    }

    [Fact]
    public void DelegateInvoke_ShouldReturnCorrectResult_WhenExpressionContainsVariableWithCapturingDelegateOptimizedAway()
    {
        var input = "let x = a + 1 ; 0 * ([] x )() ;";
        var builder = new ParsedExpressionFactoryBuilder()
            .SetNumberParserProvider( p => ParsedExpressionNumberParser.CreateDefaultInt32( p.Configuration ) )
            .AddBinaryOperator( "+", new ParsedExpressionAddInt32Operator() )
            .AddBinaryOperator( "*", new ParsedExpressionMultiplyInt32Operator() )
            .SetBinaryOperatorPrecedence( "+", 1 )
            .SetBinaryOperatorPrecedence( "*", 1 );

        var sut = builder.Build();

        var expression = sut.Create<int, int>( input );
        var @delegate = expression.Compile();

        using ( new AssertionScope() )
        {
            expression.UnboundArguments.Should().BeEmpty();
            expression.Body.NodeType.Should().NotBe( ExpressionType.Block );
            var result = @delegate.Invoke();
            result.Should().Be( 0 );
        }
    }

    [Fact]
    public void DelegateInvoke_ShouldReturnCorrectResult_WhenExpressionContainsVariableWhichIsOptimizedAway()
    {
        var input = "let x = a + 1 ; 0 * x ;";
        var builder = new ParsedExpressionFactoryBuilder()
            .SetNumberParserProvider( p => ParsedExpressionNumberParser.CreateDefaultInt32( p.Configuration ) )
            .AddBinaryOperator( "+", new ParsedExpressionAddInt32Operator() )
            .AddBinaryOperator( "*", new ParsedExpressionMultiplyInt32Operator() )
            .SetBinaryOperatorPrecedence( "+", 1 )
            .SetBinaryOperatorPrecedence( "*", 1 );

        var sut = builder.Build();

        var expression = sut.Create<int, int>( input );
        var @delegate = expression.Compile();

        using ( new AssertionScope() )
        {
            expression.UnboundArguments.Should().BeEmpty();
            expression.Body.NodeType.Should().NotBe( ExpressionType.Block );
            var result = @delegate.Invoke();
            result.Should().Be( 0 );
        }
    }

    [Theory]
    [InlineData( "macro ;" )]
    [InlineData( "macro -" )]
    [InlineData( "macro +" )]
    [InlineData( "macro ^" )]
    [InlineData( "macro [string]" )]
    [InlineData( "macro ToString" )]
    [InlineData( "macro int" )]
    [InlineData( "macro (" )]
    [InlineData( "macro )" )]
    [InlineData( "macro [" )]
    [InlineData( "macro ]" )]
    [InlineData( "macro ." )]
    [InlineData( "macro ," )]
    [InlineData( "macro =" )]
    [InlineData( "macro let" )]
    [InlineData( "macro macro" )]
    [InlineData( "macro const" )]
    [InlineData( "macro a ;" )]
    [InlineData( "macro a -" )]
    [InlineData( "macro a +" )]
    [InlineData( "macro a ^" )]
    [InlineData( "macro a [string]" )]
    [InlineData( "macro a ToString" )]
    [InlineData( "macro a int" )]
    [InlineData( "macro a (" )]
    [InlineData( "macro a )" )]
    [InlineData( "macro a [" )]
    [InlineData( "macro a ]" )]
    [InlineData( "macro a ." )]
    [InlineData( "macro a ," )]
    [InlineData( "macro a let" )]
    [InlineData( "macro a macro" )]
    [InlineData( "macro a const" )]
    [InlineData( "macro []" )]
    [InlineData( "macro [,]" )]
    [InlineData( "macro [ a , ]" )]
    [InlineData( "macro [ a ] ;" )]
    [InlineData( "macro [ a ] -" )]
    [InlineData( "macro [ a ] +" )]
    [InlineData( "macro [ a ] ^" )]
    [InlineData( "macro [ a ] [string]" )]
    [InlineData( "macro [ a ] ToString" )]
    [InlineData( "macro [ a ] int" )]
    [InlineData( "macro [ a ] (" )]
    [InlineData( "macro [ a ] )" )]
    [InlineData( "macro [ a ] [" )]
    [InlineData( "macro [ a ] ]" )]
    [InlineData( "macro [ a ] ." )]
    [InlineData( "macro [ a ] ," )]
    [InlineData( "macro [ a ] =" )]
    [InlineData( "macro [ a ] let" )]
    [InlineData( "macro [ a ] macro" )]
    [InlineData( "macro [ a ] const" )]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenMacroDeclarationIsNotFollowedByItsNameAndAssignment(string input)
    {
        var builder = new ParsedExpressionFactoryBuilder()
            .AddPrefixUnaryOperator( "-", new MockPrefixUnaryOperator() )
            .AddPostfixUnaryOperator( "^", new MockPostfixUnaryOperator() )
            .AddBinaryOperator( "+", new MockBinaryOperator() )
            .AddPrefixTypeConverter( "[string]", new MockPrefixTypeConverter() )
            .AddPostfixTypeConverter( "ToString", new MockPostfixTypeConverter() )
            .AddConstant( "const", new ZeroConstant() )
            .AddTypeDeclaration<int>( "int" )
            .SetBinaryOperatorPrecedence( "+", 1 )
            .SetPrefixUnaryConstructPrecedence( "-", 1 )
            .SetPostfixUnaryConstructPrecedence( "^", 1 )
            .SetPrefixUnaryConstructPrecedence( "[string]", 1 )
            .SetPostfixUnaryConstructPrecedence( "ToString", 1 );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should().ThrowExactly<ParsedExpressionCreationException>();
    }

    [Theory]
    [InlineData( "macro a =" )]
    [InlineData( "macro a = b" )]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenMacroDeclarationEndsWithoutLineSeparator(string input)
    {
        var builder = new ParsedExpressionFactoryBuilder();
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should().ThrowExactly<ParsedExpressionCreationException>();
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenMacroNameIsInvalid()
    {
        var input = "macro ? = 'bar'; 'foo'";
        var builder = new ParsedExpressionFactoryBuilder();
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.NestedExpressionFailure ) );
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenMacroNameDuplicatesArgumentName()
    {
        var input = "let x = a ; macro a = b ; 'foo'";
        var builder = new ParsedExpressionFactoryBuilder();
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.NestedExpressionFailure ) );
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenMacroNameDuplicatesVariableName()
    {
        var input = "let x = a ; macro x = b ; 'foo'";
        var builder = new ParsedExpressionFactoryBuilder();
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.NestedExpressionFailure ) );
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenMacroNameDuplicatesMacroName()
    {
        var input = "macro x = a ; macro x = b ; 'foo'";
        var builder = new ParsedExpressionFactoryBuilder();
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.NestedExpressionFailure ) );
    }

    [Fact]
    public void DelegateInvoke_ShouldReturnCorrectResult_WhenDelegateParameterNameDuplicatesMacroName()
    {
        var input = "macro x = a , string b ; ( [ string x ] a + b )( 'foo' , 'bar' ) ;";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddTypeDeclaration<string>( "string" )
            .AddBinaryOperator( "+", new MockBinaryOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var sut = builder.Build();

        var expression = sut.Create<string, string>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke();

        result.Should().Be( "( foo|BiOp|bar )" );
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenMacroDeclarationIsEmpty()
    {
        var input = "macro a = ; 'foo'";
        var builder = new ParsedExpressionFactoryBuilder();
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.MacroMustContainAtLeastOneToken ) );
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenMacroIsYetUndeclaredAndUsesItself()
    {
        var input = "macro a = a + 1 ; 'foo'";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new MockBinaryOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.NestedExpressionFailure ) );
    }

    [Fact]
    public void DelegateInvoke_ShouldReturnCorrectResult_WhenExpressionContainsSingleUsedMacro()
    {
        var input = "macro m = a + 'bar' ; m + 'qux' ;";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new MockBinaryOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var sut = builder.Build();

        var expression = sut.Create<string, string>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke( "foo" );

        result.Should().Be( "( ( foo|BiOp|bar )|BiOp|qux )" );
    }

    [Fact]
    public void DelegateInvoke_ShouldReturnCorrectResult_WhenExpressionContainsMacroUsedInVariable()
    {
        var input = "macro m = 1 + true ; let v = m + a ; v ;";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new MockBinaryOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var sut = builder.Build();

        var expression = sut.Create<string, string>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke( "bar" );

        result.Should().Be( "( ( 1|BiOp|True )|BiOp|bar )" );
    }

    [Fact]
    public void DelegateInvoke_ShouldReturnCorrectResult_WhenExpressionContainsMacroUsedInDelegateBody()
    {
        var input = "macro m = a + 'bar' ; ( [] m )() ;";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new MockBinaryOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var sut = builder.Build();

        var expression = sut.Create<string, string>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke( "foo" );

        result.Should().Be( "( foo|BiOp|bar )" );
    }

    [Fact]
    public void DelegateInvoke_ShouldReturnCorrectResult_WhenExpressionContainsMacroUsedInDelegateParameters()
    {
        var input = "macro m = [ string a , string b ] ; ( m a + b )( 'foo' , 'bar' ) ;";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddTypeDeclaration<string>( "string" )
            .AddBinaryOperator( "+", new MockBinaryOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var sut = builder.Build();

        var expression = sut.Create<string, string>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke();

        result.Should().Be( "( foo|BiOp|bar )" );
    }

    [Fact]
    public void DelegateInvoke_ShouldReturnCorrectResult_WhenExpressionContainsMacroUsedInOtherMacro()
    {
        var input = "macro m = a + ; macro n = m b ; n ;";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new MockBinaryOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var sut = builder.Build();

        var expression = sut.Create<string, string>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke( "foo", "bar" );

        result.Should().Be( "( foo|BiOp|bar )" );
    }

    [Fact]
    public void DelegateInvoke_ShouldReturnCorrectResult_WhenExpressionContainsMacroUsedInVariableThatGetsReassigned()
    {
        var input = "let v = a + 'bar' ; macro m = v ; let x = m ; let v = a + 'qux' ; x + v ;";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new MockBinaryOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var sut = builder.Build();

        var expression = sut.Create<string, string>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke( "foo" );

        result.Should().Be( "( ( foo|BiOp|bar )|BiOp|( foo|BiOp|qux ) )" );
    }

    [Fact]
    public void DelegateInvoke_ShouldReturnCorrectResult_WhenExpressionContainsMacroRepresentingConstantConstruct()
    {
        var input = "macro m = const ; m ;";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddConstant( "const", new ZeroConstant() );

        var sut = builder.Build();

        var expression = sut.Create<string, string>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke();

        result.Should().Be( "ZERO" );
    }

    [Fact]
    public void DelegateInvoke_ShouldReturnCorrectResult_WhenExpressionContainsMacroRepresentingDelegate()
    {
        var input = "macro m = ( [ string a ] a + b ) ; m( 'foo' )";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddTypeDeclaration<string>( "string" )
            .AddBinaryOperator( "+", new MockBinaryOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var sut = builder.Build();

        var expression = sut.Create<string, string>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke( "bar" );

        result.Should().Be( "( foo|BiOp|bar )" );
    }

    [Fact]
    public void DelegateInvoke_ShouldReturnCorrectResult_WhenExpressionContainsMacroUsedMultipleTimes()
    {
        var input = "macro m = a + ; m m a ;";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new MockBinaryOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var sut = builder.Build();

        var expression = sut.Create<string, string>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke( "foo" );

        result.Should().Be( "( ( foo|BiOp|foo )|BiOp|foo )" );
    }

    [Fact]
    public void DelegateInvoke_ShouldReturnCorrectResult_WhenExpressionContainsMacroRepresentingIndexerParameters()
    {
        var input = "macro m = [ 0 ] ; string[ a ] m ;";
        var builder = new ParsedExpressionFactoryBuilder()
            .SetNumberParserProvider( p => ParsedExpressionNumberParser.CreateDefaultInt32( p.Configuration ) )
            .AddTypeDeclaration<string>( "string" );

        var sut = builder.Build();

        var expression = sut.Create<string, string>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke( "foo" );

        result.Should().Be( "foo" );
    }

    [Fact]
    public void DelegateInvoke_ShouldReturnCorrectResult_WhenExpressionContainsMacroRepresentingMemberAccess()
    {
        var input = "macro m = . Length ; a m ;";
        var builder = new ParsedExpressionFactoryBuilder();
        var sut = builder.Build();

        var expression = sut.Create<string, int>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke( "foo" );

        result.Should().Be( "foo".Length );
    }

    [Fact]
    public void DelegateInvoke_ShouldReturnCorrectResult_WhenExpressionContainsMacroRepresentingInlineArrayElements()
    {
        var input = "macro m = [ a , b , c ] ; string m ;";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddTypeDeclaration<string>( "string" );

        var sut = builder.Build();

        var expression = sut.Create<string, string[]>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke( "foo", "bar", "qux" );

        result.Should().BeSequentiallyEqualTo( "foo", "bar", "qux" );
    }

    [Fact]
    public void DelegateInvoke_ShouldReturnCorrectResult_WhenExpressionContainsMacroRepresentingFunctionCall()
    {
        var input = "macro m = foo( a , b , c ) ; m ;";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddFunction( "foo", new MockFunctionWithThreeParameters() );

        var sut = builder.Build();

        var expression = sut.Create<string, string>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke( "foo", "bar", "qux" );

        result.Should().Be( "Func(foo,bar,qux)" );
    }

    [Fact]
    public void DelegateInvoke_ShouldReturnCorrectResult_WhenExpressionContainsMacroRepresentingMethodCall()
    {
        var input = "macro m = Equals( 'bar' ) ; a . m ;";
        var builder = new ParsedExpressionFactoryBuilder();
        var sut = builder.Build();

        var expression = sut.Create<string, bool>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke( "foo" );

        result.Should().Be( false );
    }

    [Fact]
    public void DelegateInvoke_ShouldReturnCorrectResult_WhenExpressionContainsUnusedMacro()
    {
        var input = "macro m = + - * ? ; a + 'bar' ;";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new MockBinaryOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var sut = builder.Build();

        var expression = sut.Create<string, string>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke( "foo" );

        result.Should().Be( "( foo|BiOp|bar )" );
    }

    [Fact]
    public void DelegateInvoke_ShouldReturnCorrectResult_WhenExpressionContainsMacroWithAssignmentToken()
    {
        var input = "macro m = = 'bar' ; a m ;";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "=", new MockBinaryOperator() )
            .SetBinaryOperatorPrecedence( "=", 1 );

        var sut = builder.Build();

        var expression = sut.Create<string, string>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke( "foo" );

        result.Should().Be( "( foo|BiOp|bar )" );
    }

    [Fact]
    public void DelegateInvoke_ShouldReturnCorrectResult_WhenExpressionContainsMacroRepresentingVariableDeclaration()
    {
        var input = "macro m = let v = a + 'bar' ; m ; v ;";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new MockBinaryOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var sut = builder.Build();

        var expression = sut.Create<string, string>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke( "foo" );

        result.Should().Be( "( foo|BiOp|bar )" );
    }

    [Fact]
    public void DelegateInvoke_ShouldReturnCorrectResult_WhenExpressionContainsMacroRepresentingMacroDeclaration()
    {
        var input = "macro m = macro n = a + 'bar' ; m ; n ;";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new MockBinaryOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var sut = builder.Build();

        var expression = sut.Create<string, string>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke( "foo" );

        result.Should().Be( "( foo|BiOp|bar )" );
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenMacroResolutionCausesAnError()
    {
        var input = "macro m = + b ; a + m ;";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new MockBinaryOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.MacroResolutionFailure ) );
    }

    [Fact]
    public void DelegateInvoke_ShouldReturnCorrectResult_WhenExpressionContainsSingleParameterizedMacro()
    {
        var input = "macro [ a , b ] m = a ( b + c ) ; m( 'foo' + , 'bar' ) ;";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new MockBinaryOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var sut = builder.Build();

        var expression = sut.Create<string, string>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke( "qux" );

        result.Should().Be( "( foo|BiOp|( bar|BiOp|qux ) )" );
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenMacroParameterNameIsInvalid()
    {
        var input = "macro[ ? ] m = a + 'bar' ; m( 'foo' ) ;";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new MockBinaryOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.NestedExpressionFailure ) );
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenMacroParameterNameDuplicatesArgumentName()
    {
        var input = "let v = a + 'bar' ; macro[ a ] m = a + 'qux' ; m( 'foo' ) ;";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new MockBinaryOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.NestedExpressionFailure ) );
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenMacroParameterNameDuplicatesVariableName()
    {
        var input = "let v = a + 'bar' ; macro[ v ] m = v + 'qux' ; m( 'foo' ) ;";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new MockBinaryOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.NestedExpressionFailure ) );
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenMacroNameDuplicatesOneOfItsOwnParameterNames()
    {
        var input = "macro[ a ] a = a + 'bar' ; m( 'foo' ) ;";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new MockBinaryOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.NestedExpressionFailure ) );
    }

    [Fact]
    public void DelegateInvoke_ShouldReturnCorrectResult_WhenMacroParameterNameDuplicatesOtherMacroName()
    {
        var input = "macro n = a , b ; macro[ n ] m = a + b ; m( 'foo' , 'bar' ) ;";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new MockBinaryOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var sut = builder.Build();

        var expression = sut.Create<string, string>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke();

        result.Should().Be( "( foo|BiOp|bar )" );
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenMacroLastParameterResolutionIsEmpty()
    {
        var input = "macro[ a , b ] m = a + b ; m( 'foo' , ) ;";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new MockBinaryOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.NestedExpressionFailure ) );
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenMacroNonLastParameterResolutionIsEmpty()
    {
        var input = "macro[ a , b ] m = a + b ; m( , 'bar' ) ;";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new MockBinaryOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.NestedExpressionFailure ) );
    }

    [Theory]
    [InlineData( 0 )]
    [InlineData( 1 )]
    [InlineData( 3 )]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenMacroParameterCountIsInvalid(int count)
    {
        var parameters = Enumerable.Range( 1, count ).Select( p => $"'{p}'" );
        var input = $"macro[ a , b ] m = a + b ; m( {string.Join( " , ", parameters )} ) ;";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new MockBinaryOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.InvalidMacroParameterCount ) );
    }

    [Fact]
    public void DelegateInvoke_ShouldReturnCorrectResult_WhenMacroParameterNameAndItsResolutionAreEquivalent()
    {
        var input = "macro[ a ] m = a + 'bar' ; m( a ) ;";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new MockBinaryOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var sut = builder.Build();

        var expression = sut.Create<string, string>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke( "foo" );

        result.Should().Be( "( foo|BiOp|bar )" );
    }

    [Fact]
    public void DelegateInvoke_ShouldReturnCorrectResult_WhenMacroParameterResolutionContainsMethodCall()
    {
        var input = "macro[ a ] m = a + true ; m( x . Equals( 'foo' ) ) ;";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new MockBinaryOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var sut = builder.Build();

        var expression = sut.Create<string, string>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke( "bar" );

        result.Should().Be( "( False|BiOp|True )" );
    }

    [Fact]
    public void DelegateInvoke_ShouldReturnCorrectResult_WhenMacroParameterResolutionsContainParentheses()
    {
        var input = "macro[ a , b ] m = a + b ; m( ( x ) , ( 'foo' + 'bar' ) ) ;";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new MockBinaryOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var sut = builder.Build();

        var expression = sut.Create<string, string>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke( "qux" );

        result.Should().Be( "( qux|BiOp|( foo|BiOp|bar ) )" );
    }

    [Fact]
    public void DelegateInvoke_ShouldReturnCorrectResult_WhenParameterizedMacroIsResolvedAsItsOwnParameter()
    {
        var input = "macro[ a ] m = a + 'bar' ; m( m( m( 'foo' ) ) ) ;";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new MockBinaryOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var sut = builder.Build();

        var expression = sut.Create<string, string>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke();

        result.Should().Be( "( ( ( foo|BiOp|bar )|BiOp|bar )|BiOp|bar )" );
    }

    [Fact]
    public void DelegateInvoke_ShouldReturnCorrectResult_WhenMacroParametersContainSyntaxResemblingFunctionCallWithElementSeparators()
    {
        var input = "macro[ x , y , z ] m = x , y , z ; m( foo( a , b , c ) ) ;";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddFunction( "foo", new MockFunctionWithThreeParameters() );

        var sut = builder.Build();

        var expression = sut.Create<string, string>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke( "foo", "bar", "qux" );

        result.Should().Be( "Func(foo,bar,qux)" );
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenMacroParameterResolutionCausesAnError()
    {
        var input = "macro[ x ] m = a x ; m( + + ) 'bar' ;";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new MockBinaryOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Should()
            .ThrowExactly<ParsedExpressionCreationException>()
            .AndMatch( e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.MacroResolutionFailure ) );
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
    public void DelegateInvoke_ShouldReturnCorrectResult_WhenDiscardUnusedArgumentsIsDisabled()
    {
        var input = "0 * a";
        var configuration = Substitute.For<IParsedExpressionFactoryConfiguration>();
        configuration.DecimalPoint.Returns( '.' );
        configuration.IntegerDigitSeparator.Returns( '_' );
        configuration.StringDelimiter.Returns( '\'' );
        configuration.ScientificNotationExponents.Returns( "eE" );
        configuration.AllowScientificNotation.Returns( true );
        configuration.AllowNonIntegerNumbers.Returns( true );
        configuration.ConvertResultToOutputTypeAutomatically.Returns( true );
        configuration.PostponeStaticInlineDelegateCompilation.Returns( false );
        configuration.DiscardUnusedArguments.Returns( false );

        var builder = new ParsedExpressionFactoryBuilder()
            .SetConfiguration( configuration )
            .SetNumberParserProvider( p => ParsedExpressionNumberParser.CreateDefaultInt32( p.Configuration ) )
            .AddBinaryOperator( "*", new ParsedExpressionMultiplyInt32Operator() )
            .SetBinaryOperatorPrecedence( "*", 1 );

        var sut = builder.Build();

        var expression = sut.Create<int, int>( input );
        var @delegate = expression.Compile();

        using ( new AssertionScope() )
        {
            expression.UnboundArguments.Contains( "a" ).Should().BeTrue();
            var result = @delegate.Invoke( 100 );
            result.Should().Be( 0 );
        }
    }

    [Fact]
    public void DelegateInvoke_ShouldReturnCorrectResult_WhenDiscardUnusedArgumentsIsEnabled()
    {
        var input = "0 * a";
        var configuration = Substitute.For<IParsedExpressionFactoryConfiguration>();
        configuration.DecimalPoint.Returns( '.' );
        configuration.IntegerDigitSeparator.Returns( '_' );
        configuration.StringDelimiter.Returns( '\'' );
        configuration.ScientificNotationExponents.Returns( "eE" );
        configuration.AllowScientificNotation.Returns( true );
        configuration.AllowNonIntegerNumbers.Returns( true );
        configuration.ConvertResultToOutputTypeAutomatically.Returns( true );
        configuration.PostponeStaticInlineDelegateCompilation.Returns( false );
        configuration.DiscardUnusedArguments.Returns( true );

        var builder = new ParsedExpressionFactoryBuilder()
            .SetConfiguration( configuration )
            .SetNumberParserProvider( p => ParsedExpressionNumberParser.CreateDefaultInt32( p.Configuration ) )
            .AddBinaryOperator( "*", new ParsedExpressionMultiplyInt32Operator() )
            .SetBinaryOperatorPrecedence( "*", 1 );

        var sut = builder.Build();

        var expression = sut.Create<int, int>( input );
        var @delegate = expression.Compile();

        using ( new AssertionScope() )
        {
            expression.UnboundArguments.Should().BeEmpty();
            var result = @delegate.Invoke();
            result.Should().Be( 0 );
        }
    }

    [Fact]
    public void DelegateInvoke_ShouldReturnCorrectResult_WhenPostponeStaticInlineDelegateCompilationIsDisabled()
    {
        var input = "( [] 0 )() * a";
        var configuration = Substitute.For<IParsedExpressionFactoryConfiguration>();
        configuration.DecimalPoint.Returns( '.' );
        configuration.IntegerDigitSeparator.Returns( '_' );
        configuration.StringDelimiter.Returns( '\'' );
        configuration.ScientificNotationExponents.Returns( "eE" );
        configuration.AllowScientificNotation.Returns( true );
        configuration.AllowNonIntegerNumbers.Returns( true );
        configuration.ConvertResultToOutputTypeAutomatically.Returns( true );
        configuration.PostponeStaticInlineDelegateCompilation.Returns( false );
        configuration.DiscardUnusedArguments.Returns( true );

        var builder = new ParsedExpressionFactoryBuilder()
            .SetConfiguration( configuration )
            .SetNumberParserProvider( p => ParsedExpressionNumberParser.CreateDefaultInt32( p.Configuration ) )
            .AddBinaryOperator( "*", new ParsedExpressionMultiplyInt32Operator() )
            .SetBinaryOperatorPrecedence( "*", 1 );

        var sut = builder.Build();

        var expression = sut.Create<int, int>( input );
        var @delegate = expression.Compile();

        using ( new AssertionScope() )
        {
            expression.UnboundArguments.Should().BeEmpty();
            var result = @delegate.Invoke();
            result.Should().Be( 0 );
        }
    }

    [Fact]
    public void DelegateInvoke_ShouldReturnCorrectResult_WhenPostponeStaticInlineDelegateCompilationIsEnabled()
    {
        var input = "( [] 0 )() * a";
        var configuration = Substitute.For<IParsedExpressionFactoryConfiguration>();
        configuration.DecimalPoint.Returns( '.' );
        configuration.IntegerDigitSeparator.Returns( '_' );
        configuration.StringDelimiter.Returns( '\'' );
        configuration.ScientificNotationExponents.Returns( "eE" );
        configuration.AllowScientificNotation.Returns( true );
        configuration.AllowNonIntegerNumbers.Returns( true );
        configuration.ConvertResultToOutputTypeAutomatically.Returns( true );
        configuration.PostponeStaticInlineDelegateCompilation.Returns( true );
        configuration.DiscardUnusedArguments.Returns( true );

        var builder = new ParsedExpressionFactoryBuilder()
            .SetConfiguration( configuration )
            .SetNumberParserProvider( p => ParsedExpressionNumberParser.CreateDefaultInt32( p.Configuration ) )
            .AddBinaryOperator( "*", new ParsedExpressionMultiplyInt32Operator() )
            .SetBinaryOperatorPrecedence( "*", 1 );

        var sut = builder.Build();

        var expression = sut.Create<int, int>( input );
        var @delegate = expression.Compile();

        using ( new AssertionScope() )
        {
            expression.UnboundArguments.Contains( "a" ).Should().BeTrue();
            var result = @delegate.Invoke( 100 );
            result.Should().Be( 0 );
        }
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

        using ( new AssertionScope() )
        {
            result.UnboundArguments.Should().BeEmpty();
            result.DiscardedArguments.Select( n => n.ToString() ).Should().BeEquivalentTo( "a", "b", "c" );
        }
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
            result.UnboundArguments.Should().HaveCount( 1 );
            result.UnboundArguments.GetIndex( "a" ).Should().Be( 0 );
            result.DiscardedArguments.Select( n => n.ToString() ).Should().BeEquivalentTo( "b", "c" );
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
            result.UnboundArguments.Should().HaveCount( 2 );
            result.UnboundArguments.GetIndex( "a" ).Should().Be( 0 );
            result.UnboundArguments.GetIndex( "d" ).Should().Be( 1 );
            result.DiscardedArguments.Select( n => n.ToString() ).Should().BeEquivalentTo( "b", "c", "e" );
        }
    }

    [Fact]
    public void Create_ShouldNotRemoveIndexersFromNonParameterExpressionArray_WhenTryingToRemoveUnusedArguments()
    {
        var (aValue, externalValue) = Fixture.CreateDistinctCollection<int>( count: 2 );
        var expected = aValue + externalValue;

        var input = "a + external_at 0";
        var builder = new ParsedExpressionFactoryBuilder()
            .SetNumberParserProvider( p => ParsedExpressionNumberParser.CreateDefaultInt32( p.Configuration ) )
            .AddBinaryOperator( "+", new ParsedExpressionAddOperator() )
            .AddPrefixUnaryOperator( "external_at", new ExternalArrayIndexUnaryOperator( externalValue ) )
            .SetBinaryOperatorPrecedence( "+", 1 )
            .SetPrefixUnaryConstructPrecedence( "external_at", 1 );

        var sut = builder.Build();

        var expression = sut.Create<int, int>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke( aValue );

        using ( new AssertionScope() )
        {
            expression.UnboundArguments.Should().HaveCount( 1 );
            expression.UnboundArguments.GetIndex( "a" ).Should().Be( 0 );
            expression.DiscardedArguments.Should().BeEmpty();
            result.Should().Be( expected );
        }
    }

    [Theory]
    [InlineData( -1 )]
    [InlineData( 2 )]
    public void Create_ShouldIgnoreParameterArrayIndexesThatAreOutOfRange_WhenTryingToRemoveUnusedArguments(int index)
    {
        var (aValue, bValue) = Fixture.CreateDistinctCollection<int>( count: 2 );

        var input = "a + external_parameter_accessor b";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new ParsedExpressionAddOperator() )
            .AddPrefixUnaryOperator( "external_parameter_accessor", new ParameterAccessorWithConstantIndexUnaryOperator( index ) )
            .SetBinaryOperatorPrecedence( "+", 1 )
            .SetPrefixUnaryConstructPrecedence( "external_parameter_accessor", 1 );

        var sut = builder.Build();

        var expression = sut.Create<int, int>( input );
        var @delegate = expression.Compile();
        var action = Lambda.Of( () => @delegate.Invoke( aValue, bValue ) );

        using ( new AssertionScope() )
        {
            expression.UnboundArguments.Should().HaveCount( 2 );
            expression.UnboundArguments.GetIndex( "a" ).Should().Be( 0 );
            expression.UnboundArguments.GetIndex( "b" ).Should().Be( 1 );
            expression.DiscardedArguments.Should().BeEmpty();
            action.Should().ThrowExactly<IndexOutOfRangeException>();
        }
    }

    [Fact]
    public void Create_ShouldIgnoreParameterArrayIndexesThatAreNotConstant_WhenTryingToRemoveUnusedArguments()
    {
        var (aValue, bValue, cValue) = Fixture.CreateDistinctCollection<int>( count: 3 );
        var expected = aValue + bValue + bValue + cValue;

        var input = "a + external_parameter_accessor b + c";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new ParsedExpressionAddOperator() )
            .AddPrefixUnaryOperator( "external_parameter_accessor", new ParameterAccessorWithVariableIndexUnaryOperator( -1, 2 ) )
            .SetBinaryOperatorPrecedence( "+", 1 )
            .SetPrefixUnaryConstructPrecedence( "external_parameter_accessor", 1 );

        var sut = builder.Build();

        var expression = sut.Create<int, int>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke( aValue, bValue, cValue );

        using ( new AssertionScope() )
        {
            expression.UnboundArguments.Should().HaveCount( 3 );
            expression.UnboundArguments.GetIndex( "a" ).Should().Be( 0 );
            expression.UnboundArguments.GetIndex( "b" ).Should().Be( 1 );
            expression.UnboundArguments.GetIndex( "c" ).Should().Be( 2 );
            expression.DiscardedArguments.Should().BeEmpty();
            result.Should().Be( expected );
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
