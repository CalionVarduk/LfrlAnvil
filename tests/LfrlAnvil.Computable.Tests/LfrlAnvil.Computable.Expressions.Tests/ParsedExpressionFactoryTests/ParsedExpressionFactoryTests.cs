using System.Linq;
using LfrlAnvil.Computable.Expressions.Constructs;
using LfrlAnvil.Computable.Expressions.Constructs.Boolean;
using LfrlAnvil.Computable.Expressions.Constructs.Int32;
using LfrlAnvil.Computable.Expressions.Errors;
using LfrlAnvil.Computable.Expressions.Exceptions;
using LfrlAnvil.Computable.Expressions.Internal;
using LfrlAnvil.Functional;
using LfrlAnvil.TestExtensions.Attributes;
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

        _ = sut.Create<decimal, string>( "[string] 0" );

        Assertion.All(
                provider.CallAt( 0 ).Exists.TestTrue(),
                provider.CallAt( 0 )
                    .Arguments.TestAll(
                        (arg, _) =>
                        {
                            var @params = ( ParsedExpressionNumberParserParams )arg!;
                            return Assertion.All(
                                "@params",
                                @params.Configuration.TestRefEquals( sut.Configuration ),
                                @params.ArgumentType.TestEquals( typeof( decimal ) ),
                                @params.ResultType.TestEquals( typeof( string ) ) );
                        } ) )
            .Go();
    }

    [Fact]
    public void Create_ShouldReturnExpressionWithCorrectArgumentIndexes()
    {
        var (aValue, bValue, cValue, dValue) = Fixture.CreateManyDistinct<decimal>( count: 4 );
        var expected = aValue + bValue + cValue + aValue + cValue + dValue + bValue;

        var input = "a + b + c + a + c + d + b";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new ParsedExpressionAddOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var sut = builder.Build();

        var expression = sut.Create<decimal, decimal>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke( aValue, bValue, cValue, dValue );

        Assertion.All(
                result.TestEquals( expected ),
                expression.UnboundArguments.GetIndex( "a" ).TestEquals( 0 ),
                expression.UnboundArguments.GetIndex( "b" ).TestEquals( 1 ),
                expression.UnboundArguments.GetIndex( "c" ).TestEquals( 2 ),
                expression.UnboundArguments.GetIndex( "d" ).TestEquals( 3 ) )
            .Go();
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenUnexpectedExceptionIsThrown()
    {
        var input = "a";
        var exception = new Exception();
        var builder = new ParsedExpressionFactoryBuilder().SetNumberParserProvider( _ => throw exception );
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<decimal, decimal>( input ) );

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.Error ) ) )
            .Go();
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenStringConstantIsTheLastTokenAndDoesNotHaveClosingDelimiter()
    {
        var input = "'foobar";
        var builder = new ParsedExpressionFactoryBuilder();
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.StringConstantParsingFailure ) ) )
            .Go();
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenNumberParserFailedToParseNumberConstant()
    {
        var input = "12.34";
        var builder = new ParsedExpressionFactoryBuilder().SetNumberParserProvider( _ => new FailingNumberParser() );
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<decimal, decimal>( input ) );

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.NumberConstantParsingFailure ) ) )
            .Go();
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenArgumentHasInvalidName()
    {
        var input = "/";
        var builder = new ParsedExpressionFactoryBuilder();
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<decimal, decimal>( input ) );

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.InvalidArgumentName ) ) )
            .Go();
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

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.UnexpectedOperand ) ) )
            .Go();
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

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.UnexpectedFunctionCall ) ) )
            .Go();
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

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ExpectedPostfixUnaryOrBinaryConstruct ) ) )
            .Go();
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

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ExpectedPostfixUnaryOrBinaryConstruct ) ) )
            .Go();
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

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.UnexpectedOpenedParenthesis ) ) )
            .Go();
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

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.UnexpectedTypeDeclaration ) ) )
            .Go();
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

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.UnexpectedOpenedParenthesis ) ) )
            .Go();
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

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.UnexpectedOpenedParenthesis ) ) )
            .Go();
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenClosedParenthesisIsFollowedByOpenedParenthesis()
    {
        var input = "( a ) (";
        var builder = new ParsedExpressionFactoryBuilder();
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<decimal, decimal>( input ) );

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.UnexpectedOpenedParenthesis ) ) )
            .Go();
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenOpenedParenthesisIsFollowedByClosedParenthesis()
    {
        var input = "( )";
        var builder = new ParsedExpressionFactoryBuilder();
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<decimal, decimal>( input ) );

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.UnexpectedClosedParenthesis ) ) )
            .Go();
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenInputContainsOnlyOpenedSquareBracket()
    {
        var input = "[";
        var builder = new ParsedExpressionFactoryBuilder();
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Test( exc => exc.TestType().Exact<ParsedExpressionCreationException>() ).Go();
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

        action.Test( exc => exc.TestType().Exact<ParsedExpressionCreationException>() ).Go();
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

        action.Test( exc => exc.TestType().Exact<ParsedExpressionCreationException>() ).Go();
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

        action.Test( exc => exc.TestType().Exact<ParsedExpressionCreationException>() ).Go();
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

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.UnexpectedOpenedSquareBracket ) ) )
            .Go();
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

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.UnexpectedOpenedSquareBracket ) ) )
            .Go();
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenClosedSquareBracketIsFirst()
    {
        var input = "]";
        var builder = new ParsedExpressionFactoryBuilder();
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.UnexpectedClosedSquareBracket ) ) )
            .Go();
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

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.UnexpectedClosedSquareBracket ) ) )
            .Go();
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

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.UnexpectedClosedSquareBracket ) ) )
            .Go();
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

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.UnexpectedClosedSquareBracket ) ) )
            .Go();
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

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.UnexpectedClosedSquareBracket ) ) )
            .Go();
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

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.UnexpectedClosedSquareBracket ) ) )
            .Go();
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

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) ) )
            .Go();
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

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) ) )
            .Go();
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

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.UnexpectedClosedParenthesis ) ) )
            .Go();
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

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.UnexpectedClosedParenthesis ) ) )
            .Go();
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

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.UnexpectedClosedParenthesis ) ) )
            .Go();
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

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.UnexpectedClosedParenthesis ) ) )
            .Go();
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

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) ) )
            .Go();
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

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations(
                            e,
                            input,
                            ParsedExpressionBuilderErrorType.AmbiguousPostfixUnaryConstructResolutionFailure ) ) )
            .Go();
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

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) ) )
            .Go();
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

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) ) )
            .Go();
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

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) ) )
            .Go();
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

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) ) )
            .Go();
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

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) ) )
            .Go();
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

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) ) )
            .Go();
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

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) ) )
            .Go();
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

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ExpectedBinaryOrPrefixUnaryConstruct ) ) )
            .Go();
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

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ExpectedBinaryOrPrefixUnaryConstruct ) ) )
            .Go();
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

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.UnexpectedConstruct ) ) )
            .Go();
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

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) ) )
            .Go();
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

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) ) )
            .Go();
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

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) ) )
            .Go();
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

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) ) )
            .Go();
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

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) ) )
            .Go();
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

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) ) )
            .Go();
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

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) ) )
            .Go();
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

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) ) )
            .Go();
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

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) ) )
            .Go();
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

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) ) )
            .Go();
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

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) ) )
            .Go();
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

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) ) )
            .Go();
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

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ExpectedBinaryOperator ) ) )
            .Go();
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

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ExpectedBinaryOperator ) ) )
            .Go();
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

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) ) )
            .Go();
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

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ExpectedPrefixUnaryConstruct ) ) )
            .Go();
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

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ExpectedPrefixUnaryConstruct ) ) )
            .Go();
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

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ExpectedPrefixUnaryConstruct ) ) )
            .Go();
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

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ExpectedPrefixUnaryConstruct ) ) )
            .Go();
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

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.BinaryOperatorCouldNotBeResolved ) ) )
            .Go();
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

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.PrefixUnaryOperatorCouldNotBeResolved ) ) )
            .Go();
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

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.PrefixTypeConverterCouldNotBeResolved ) ) )
            .Go();
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

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations(
                            e,
                            input,
                            ParsedExpressionBuilderErrorType.PostfixUnaryOperatorCouldNotBeResolved ) ) )
            .Go();
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

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations(
                            e,
                            input,
                            ParsedExpressionBuilderErrorType.PostfixTypeConverterCouldNotBeResolved ) ) )
            .Go();
    }

    [Theory]
    [InlineData( "" )]
    [InlineData( ";" )]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenInputIsEmpty(string input)
    {
        var builder = new ParsedExpressionFactoryBuilder();
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations(
                            e,
                            input,
                            ParsedExpressionBuilderErrorType.ExpressionMustContainAtLeastOneOperand,
                            ParsedExpressionBuilderErrorType.ExpressionContainsInvalidOperandToOperatorRatio ) ) )
            .Go();
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

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations(
                            e,
                            input,
                            ParsedExpressionBuilderErrorType.ExpressionContainsInvalidOperandToOperatorRatio ) ) )
            .Go();
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenSomeOpenedParenthesisAreUnclosed()
    {
        var input = "( ( a";
        var builder = new ParsedExpressionFactoryBuilder();
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ExpressionContainsUnclosedParentheses ) ) )
            .Go();
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

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) ) )
            .Go();
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

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) ) )
            .Go();
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

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) ) )
            .Go();
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

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.UnexpectedElementSeparator ) ) )
            .Go();
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

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.NestedExpressionFailure ) ) )
            .Go();
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

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.NestedExpressionFailure ) ) )
            .Go();
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

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => e.Errors.Select( er => er.Type )
                            .TestAny(
                                (t, _) => Assertion.Any(
                                    t.TestEquals( ParsedExpressionBuilderErrorType.NestedExpressionFailure ),
                                    t.TestEquals( ParsedExpressionBuilderErrorType.MissingSubExpressionClosingSymbol ) ) ) ) )
            .Go();
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

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.FunctionCouldNotBeResolved ) ) )
            .Go();
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

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.NestedExpressionFailure ) ) )
            .Go();
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

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        exception => Assertion.All(
                            "exception",
                            exception.Errors.Select( e => e.Type )
                                .TestSequence( [ ParsedExpressionBuilderErrorType.NestedExpressionFailure ] ),
                            exception.Errors.FirstOrDefault()
                                .TestType()
                                .AssignableTo<ParsedExpressionBuilderAggregateError>(
                                    fooAggregateError => Assertion.All(
                                        "fooAggregateError",
                                        fooAggregateError.Token.ToString().TestEquals( "foo" ),
                                        fooAggregateError.Inner.Select( e => e.Type )
                                            .TestSequence( [ ParsedExpressionBuilderErrorType.NestedExpressionFailure ] ),
                                        fooAggregateError.Inner.FirstOrDefault()
                                            .TestType()
                                            .AssignableTo<ParsedExpressionBuilderAggregateError>(
                                                barAggregateError => Assertion.All(
                                                    "barAggregateError",
                                                    barAggregateError.Token.ToString().TestEquals( "bar" ),
                                                    barAggregateError.Inner.Select( e => e.Token.ToString() )
                                                        .Distinct()
                                                        .TestSequence( [ "+" ] ) ) ) ) ) ) ) )
            .Go();
    }

    [Fact]
    public void Create_ShouldReturnExpressionWithCorrectArgumentIndexes_WhenArgumentsAreUsedAsFunctionParameters()
    {
        var (aValue, bValue, cValue, dValue) = Fixture.CreateManyDistinct<decimal>( count: 4 );
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

        Assertion.All(
                result.TestEquals( expected ),
                expression.UnboundArguments.GetIndex( "a" ).TestEquals( 0 ),
                expression.UnboundArguments.GetIndex( "b" ).TestEquals( 1 ),
                expression.UnboundArguments.GetIndex( "c" ).TestEquals( 2 ),
                expression.UnboundArguments.GetIndex( "d" ).TestEquals( 3 ) )
            .Go();
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenVariadicFunctionCannotBeProcessed()
    {
        var input = "foo()";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddVariadicFunction( "foo", new ThrowingVariadicFunction() );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) ) )
            .Go();
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

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.NestedExpressionFailure ) ) )
            .Go();
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
    [InlineData( "int )" )]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenTypeDeclarationIsNotFollowedByOpenedSquareBracketOrParenthesis(
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

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.NestedExpressionFailure ) ) )
            .Go();
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

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) ) )
            .Go();
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

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.UnexpectedOpenedParenthesis ) ) )
            .Go();
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

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => e.Errors.Select( er => er.Type )
                            .TestAny(
                                (t, _) => Assertion.Any(
                                    t.TestEquals( ParsedExpressionBuilderErrorType.NestedExpressionFailure ),
                                    t.TestEquals( ParsedExpressionBuilderErrorType.MissingSubExpressionClosingSymbol ) ) ) ) )
            .Go();
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

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.NestedExpressionFailure ) ) )
            .Go();
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

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        exception => Assertion.All(
                            "exception",
                            exception.Errors.Select( e => e.Type )
                                .TestSequence( [ ParsedExpressionBuilderErrorType.NestedExpressionFailure ] ),
                            exception.Errors.FirstOrDefault()
                                .TestType()
                                .AssignableTo<ParsedExpressionBuilderAggregateError>(
                                    fooAggregateError => Assertion.All(
                                        "fooAggregateError",
                                        fooAggregateError.Token.ToString().TestEquals( "int[]" ),
                                        fooAggregateError.Inner.Select( e => e.Type )
                                            .TestSequence( [ ParsedExpressionBuilderErrorType.NestedExpressionFailure ] ),
                                        fooAggregateError.Inner.FirstOrDefault()
                                            .TestType()
                                            .AssignableTo<ParsedExpressionBuilderAggregateError>(
                                                barAggregateError => Assertion.All(
                                                    "barAggregateError",
                                                    barAggregateError.Token.ToString().TestEquals( "int" ),
                                                    barAggregateError.Inner.Select( e => e.Token.ToString() )
                                                        .Distinct()
                                                        .TestSequence( [ "+" ] ) ) ) ) ) ) ) )
            .Go();
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenExpressionContainsConstantInlineArrayWithInvalidElementTypes()
    {
        var input = "string[ 'a' , 'b' , 1 ]";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddTypeDeclaration<string>( "string" );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string[]>( input ) );

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) ) )
            .Go();
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenExpressionContainsVariableInlineArrayWithInvalidElementTypes()
    {
        var input = "string[ 'a' , 'b' , p0 ]";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddTypeDeclaration<string>( "string" );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<decimal, string[]>( input ) );

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) ) )
            .Go();
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

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.NestedExpressionFailure ) ) )
            .Go();
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

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.NestedExpressionFailure ) ) )
            .Go();
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

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.NestedExpressionFailure ) ) )
            .Go();
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

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.NestedExpressionFailure ) ) )
            .Go();
    }

    [Theory]
    [InlineData( "[ string a , ] a" )]
    [InlineData( "[ string a , string ] a" )]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenInlineDelegateParameterEndIsUnexpected(string input)
    {
        var builder = new ParsedExpressionFactoryBuilder().AddTypeDeclaration<string>( "string" );
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.NestedExpressionFailure ) ) )
            .Go();
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenInlineDelegateParameterNameIsInvalid()
    {
        var input = "[string +] 'foo'";
        var builder = new ParsedExpressionFactoryBuilder().AddTypeDeclaration<string>( "string" );
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.NestedExpressionFailure ) ) )
            .Go();
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

        action.Test( exc => exc.TestType().Exact<ParsedExpressionCreationException>() ).Go();
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenInlineDelegateParameterEndWithUnexpectedInParentClosedParenthesis()
    {
        var input = "[] 'foo' )";
        var builder = new ParsedExpressionFactoryBuilder();
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.UnexpectedClosedParenthesis ) ) )
            .Go();
    }

    [Fact]
    public void
        Create_ShouldThrowParsedExpressionCreationException_WhenInlineDelegateParameterEndWithUnexpectedInParentClosedSquareBracket()
    {
        var input = "[] 'foo' ]";
        var builder = new ParsedExpressionFactoryBuilder();
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.UnexpectedClosedSquareBracket ) ) )
            .Go();
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenInlineDelegateParameterEndWithUnexpectedInParentElementSeparator()
    {
        var input = "[] 'foo' ,";
        var builder = new ParsedExpressionFactoryBuilder();
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.UnexpectedElementSeparator ) ) )
            .Go();
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

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.NestedExpressionFailure ) ) )
            .Go();
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

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.NestedExpressionFailure ) ) )
            .Go();
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

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.NestedExpressionFailure ) ) )
            .Go();
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

        action.Test( exc => exc.TestType().Exact<ParsedExpressionCreationException>() ).Go();
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenDelegateWithClosureHasMoreThanFifteenParameters()
    {
        var input =
            """
            ( [ int a , int b , int c , int d , int e , int f , int g , int h , int i , int j , int k , int l , int m , int n , int o , int p ]
            [] a + b + c + d + e + f + g + h + i + j + k + l + m + n + o + p + x ) ( 1 , 2 , 3 , 4 , 5 , 6 , 7 , 8 , 9 , 10 , 11 , 12 , 13 , 14 , 15 , 16 )
            """;

        var builder = new ParsedExpressionFactoryBuilder()
            .SetNumberParserProvider( p => ParsedExpressionNumberParser.CreateDefaultInt32( p.Configuration ) )
            .AddTypeDeclaration<int>( "int" )
            .AddBinaryOperator( "+", new ParsedExpressionAddOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<int, Func<int>>( input ) );

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.NestedExpressionFailure ) ) )
            .Go();
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

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.UnexpectedMemberAccess ) ) )
            .Go();
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

        action.Test( exc => exc.TestType().Exact<ParsedExpressionCreationException>() ).Go();
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenMethodExistsButWithDifferentAmountOfParameters()
    {
        var input = "a.PublicMethodOne( 'foo' , 'bar' )";
        var builder = new ParsedExpressionFactoryBuilder();
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<TestParameter, string>( input ) );

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) ) )
            .Go();
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenMethodExistsButWithDifferentParameterTypes()
    {
        var input = "a.PublicMethodOne( true )";
        var builder = new ParsedExpressionFactoryBuilder();
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<TestParameter, string>( input ) );

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) ) )
            .Go();
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_ForPublicParameterlessGenericMethodCall()
    {
        var input = "a.PublicGenericMethodZero()";
        var builder = new ParsedExpressionFactoryBuilder();
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<TestParameter, string>( input ) );

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) ) )
            .Go();
    }

    [Fact]
    public void
        Create_ShouldThrowParsedExpressionCreationException_WhenPublicFullyGenericMethodWithParametersOfTheSameTypeReceivesInvalidParameters()
    {
        var input = "a.PublicGenericMethodThreeSameType( 'foo' , 1 , true )";
        var builder = new ParsedExpressionFactoryBuilder();
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<TestParameter, string>( input ) );

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) ) )
            .Go();
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenPublicGenericMethodOverloadsAreAmbiguous()
    {
        var input = "a.PublicAmbiguousMethodTwo( 1, 2 )";
        var builder = new ParsedExpressionFactoryBuilder();
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<TestParameter, string>( input ) );

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) ) )
            .Go();
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenPublicGenericAndNonGenericMethodOverloadsAreAmbiguous()
    {
        var input = "a.PublicAmbiguousMethodTwo( 'foo' , 'bar' )";
        var builder = new ParsedExpressionFactoryBuilder();
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<TestParameter, string>( input ) );

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) ) )
            .Go();
    }

    [Fact]
    public void
        Create_ShouldThrowParsedExpressionCreationException_WhenPublicGenericMethodHasGenericArgumentsThatCannotBeInferredFromParameters()
    {
        var input = "a.PublicGenericMethodOne( 'foo' )";
        var builder = new ParsedExpressionFactoryBuilder();
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<TestParameter, string>( input ) );

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) ) )
            .Go();
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenPublicGenericMethodCannotBeResolvedDueToConstraints()
    {
        var input = "a.PublicConstrainedMethod( true )";
        var builder = new ParsedExpressionFactoryBuilder();
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<TestParameter, string>( input ) );

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) ) )
            .Go();
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

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) ) )
            .Go();
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenCtorExistsButWithDifferentAmountOfParameters()
    {
        var input = "test( 'foo' )";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddTypeDeclaration<TestParameter>( "test" );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<int, TestParameter>( input ) );

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) ) )
            .Go();
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenCtorExistsButWithDifferentParameterTypes()
    {
        var input = "dt( true )";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddTypeDeclaration<DateTime>( "dt" );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<int, DateTime>( input ) );

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) ) )
            .Go();
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenCtorParametersAreConstantButItThrowsAnException()
    {
        var input = "dt( -1 , -1 , -1 )";
        var builder = new ParsedExpressionFactoryBuilder()
            .SetNumberParserProvider( p => ParsedExpressionNumberParser.CreateDefaultInt32( p.Configuration ) )
            .AddTypeDeclaration<DateTime>( "dt" )
            .AddPrefixUnaryOperator( "-", new ParsedExpressionNegateInt32Operator() )
            .SetPrefixUnaryConstructPrecedence( "-", 1 );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) ) )
            .Go();
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenMemberDoesNotExist()
    {
        var input = "a.foobar";
        var builder = new ParsedExpressionFactoryBuilder();
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<TestParameter, string>( input ) );

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) ) )
            .Go();
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenMemberIsStatic()
    {
        var input = "a.Empty";
        var builder = new ParsedExpressionFactoryBuilder();
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) ) )
            .Go();
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenMemberIsPropertyWithSetterOnly()
    {
        var input = "a.SetOnly";
        var builder = new ParsedExpressionFactoryBuilder();
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<TestParameter, string>( input ) );

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) ) )
            .Go();
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

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) ) )
            .Go();
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

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) ) )
            .Go();
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

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) ) )
            .Go();
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

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) ) )
            .Go();
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

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) ) )
            .Go();
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_ForFieldMemberAccessOnNullConstant()
    {
        var input = "const.PublicField";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddConstant( "const", new ParsedExpressionConstant<TestParameter?>( null ) );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<TestParameter, string>( input ) );

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) ) )
            .Go();
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_ForPropertyMemberAccessOnNullConstant()
    {
        var input = "const.PublicProperty";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddConstant( "const", new ParsedExpressionConstant<TestParameter?>( null ) );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<TestParameter, string>( input ) );

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) ) )
            .Go();
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_ForDirectIndexerMemberAccess()
    {
        var input = "a.Item";
        var builder = new ParsedExpressionFactoryBuilder();
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<TestParameter, int>( input ) );

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) ) )
            .Go();
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

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.NestedExpressionFailure ) ) )
            .Go();
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

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => e.Errors.Select( er => er.Type )
                            .TestAny(
                                (t, _) => Assertion.Any(
                                    t.TestEquals( ParsedExpressionBuilderErrorType.NestedExpressionFailure ),
                                    t.TestEquals( ParsedExpressionBuilderErrorType.MissingSubExpressionClosingSymbol ) ) ) ) )
            .Go();
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

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.NestedExpressionFailure ) ) )
            .Go();
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenArrayIndexerDoesNotExist()
    {
        var input = "string[ 'a' ][ 'a' ]";
        var builder = new ParsedExpressionFactoryBuilder().AddTypeDeclaration<string>( "string" );
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) ) )
            .Go();
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenObjectIndexerDoesNotExist()
    {
        var input = "a[ 0 ]";
        var builder = new ParsedExpressionFactoryBuilder();
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<TestParameter, string>( input ) );

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) ) )
            .Go();
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenIndexerExistsButNotWithProvidedParameters()
    {
        var input = "'a'[ 'b' ]";
        var builder = new ParsedExpressionFactoryBuilder();
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) ) )
            .Go();
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenIndexerTargetAndParametersAreConstantButItThrowsAnException()
    {
        var input = "'a'[ 1 ]";
        var builder = new ParsedExpressionFactoryBuilder()
            .SetNumberParserProvider( p => ParsedExpressionNumberParser.CreateDefaultInt32( p.Configuration ) );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) ) )
            .Go();
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

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) ) )
            .Go();
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenMemberAccessVariadicReceivesNonConstantSecondParameter()
    {
        var input = "MEMBER_ACCESS( 'foo' , a )";
        var builder = new ParsedExpressionFactoryBuilder();
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) ) )
            .Go();
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenMemberAccessVariadicReceivesSecondParameterNotOfStringType()
    {
        var input = "MEMBER_ACCESS( 'foo' , 1 )";
        var builder = new ParsedExpressionFactoryBuilder();
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) ) )
            .Go();
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenMemberAccessVariadicReceivesNullSecondParameterOfStringType()
    {
        var input = "MEMBER_ACCESS( 'foo' , null )";
        var builder = new ParsedExpressionFactoryBuilder().AddConstant( "null", new ParsedExpressionConstant<string?>( null ) );
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) ) )
            .Go();
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

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) ) )
            .Go();
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

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) ) )
            .Go();
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenMethodCallVariadicReceivesNonConstantSecondParameter()
    {
        var input = "METHOD_CALL( 'foo' , a )";
        var builder = new ParsedExpressionFactoryBuilder();
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) ) )
            .Go();
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenMethodCallVariadicReceivesSecondParameterNotOfStringType()
    {
        var input = "METHOD_CALL( 'foo' , 1 )";
        var builder = new ParsedExpressionFactoryBuilder();
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) ) )
            .Go();
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenMethodCallVariadicReceivesNullSecondParameterOfStringType()
    {
        var input = "METHOD_CALL( 'foo' , null )";
        var builder = new ParsedExpressionFactoryBuilder().AddConstant( "null", new ParsedExpressionConstant<string?>( null ) );
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) ) )
            .Go();
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenCtorCallVariadicReceivesZeroParameters()
    {
        var input = "CTOR_CALL()";
        var builder = new ParsedExpressionFactoryBuilder();

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) ) )
            .Go();
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenCtorCallVariadicReceivesNonConstantFirstParameter()
    {
        var input = "CTOR_CALL( a )";
        var builder = new ParsedExpressionFactoryBuilder();
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<Type, string>( input ) );

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) ) )
            .Go();
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenCtorCallVariadicReceivesFirstParameterNotOfTypeType()
    {
        var input = "CTOR_CALL( 1 )";
        var builder = new ParsedExpressionFactoryBuilder();
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) ) )
            .Go();
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenCtorCallVariadicReceivesNullFirstParameterOfTypeType()
    {
        var input = "CTOR_CALL( null )";
        var builder = new ParsedExpressionFactoryBuilder().AddConstant( "null", new ParsedExpressionConstant<Type?>( null ) );
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) ) )
            .Go();
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenMakeArrayVariadicReceivesZeroParameters()
    {
        var input = "MAKE_ARRAY()";
        var builder = new ParsedExpressionFactoryBuilder();

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string[]>( input ) );

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) ) )
            .Go();
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenMakeArrayVariadicReceivesNonConstantFirstParameter()
    {
        var input = "MAKE_ARRAY( a )";
        var builder = new ParsedExpressionFactoryBuilder();
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<Type, string[]>( input ) );

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) ) )
            .Go();
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenMakeArrayVariadicReceivesFirstParameterNotOfTypeType()
    {
        var input = "MAKE_ARRAY( 1 )";
        var builder = new ParsedExpressionFactoryBuilder();
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string[]>( input ) );

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) ) )
            .Go();
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenMakeArrayVariadicReceivesNullFirstParameterOfTypeType()
    {
        var input = "MAKE_ARRAY( null )";
        var builder = new ParsedExpressionFactoryBuilder().AddConstant( "null", new ParsedExpressionConstant<Type?>( null ) );
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string[]>( input ) );

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) ) )
            .Go();
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenInvokeVariadicReceivesZeroParameters()
    {
        var input = "INVOKE()";
        var builder = new ParsedExpressionFactoryBuilder();

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) ) )
            .Go();
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenInvokeVariadicReceivesNonInvocableFirstParameter()
    {
        var input = "INVOKE( 'foo' )";
        var builder = new ParsedExpressionFactoryBuilder();

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) ) )
            .Go();
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenInvokeVariadicReceivesNullInvocableFirstParameter()
    {
        var input = "INVOKE( delegate )";
        var builder = new ParsedExpressionFactoryBuilder().AddConstant( "delegate", new ParsedExpressionConstant<Func<string>?>( null ) );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.ConstructHasThrownException ) ) )
            .Go();
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

        result.TestEquals( "( foo|BiOp|bar )" ).Go();
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenLineSeparatorIsEncounteredInNonLocalTermNestedExpression()
    {
        var input = "foo( a , b ;";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddFunction( "foo", new MockFunctionWithThreeParameters() );

        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.NestedExpressionFailure ) ) )
            .Go();
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

        action.Test( exc => exc.TestType().Exact<ParsedExpressionCreationException>() ).Go();
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

        action.Test( exc => exc.TestType().Exact<ParsedExpressionCreationException>() ).Go();
    }

    [Theory]
    [InlineData( "let a =" )]
    [InlineData( "let a = b" )]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenVariableDeclarationEndsWithoutLineSeparator(string input)
    {
        var builder = new ParsedExpressionFactoryBuilder();
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Test( exc => exc.TestType().Exact<ParsedExpressionCreationException>() ).Go();
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenVariableNameIsInvalid()
    {
        var input = "let ? = 'bar'; 'foo'";
        var builder = new ParsedExpressionFactoryBuilder();
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.NestedExpressionFailure ) ) )
            .Go();
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenVariableNameDuplicatesArgumentName()
    {
        var input = "let x = a ; let a = b ; 'foo'";
        var builder = new ParsedExpressionFactoryBuilder();
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.NestedExpressionFailure ) ) )
            .Go();
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenVariableNameDuplicatesMacroName()
    {
        var input = "macro x = a ; let x = b ; 'foo'";
        var builder = new ParsedExpressionFactoryBuilder();
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.NestedExpressionFailure ) ) )
            .Go();
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenDelegateParameterNameDuplicatesVariableName()
    {
        var input = "let x = a ; ( [ string x ] x + a )( 'foo' ) ;";
        var builder = new ParsedExpressionFactoryBuilder().AddTypeDeclaration<string>( "string" );
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.NestedExpressionFailure ) ) )
            .Go();
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenVariableDeclarationIsEmpty()
    {
        var input = "let a = ; 'foo'";
        var builder = new ParsedExpressionFactoryBuilder();
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.NestedExpressionFailure ) ) )
            .Go();
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

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.NestedExpressionFailure ) ) )
            .Go();
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

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.LocalTermError ) ) )
            .Go();
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

        action.Test( exc => exc.TestType().Exact<ParsedExpressionCreationException>() ).Go();
    }

    [Theory]
    [InlineData( "macro a =" )]
    [InlineData( "macro a = b" )]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenMacroDeclarationEndsWithoutLineSeparator(string input)
    {
        var builder = new ParsedExpressionFactoryBuilder();
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Test( exc => exc.TestType().Exact<ParsedExpressionCreationException>() ).Go();
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenMacroNameIsInvalid()
    {
        var input = "macro ? = 'bar'; 'foo'";
        var builder = new ParsedExpressionFactoryBuilder();
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.NestedExpressionFailure ) ) )
            .Go();
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenMacroNameDuplicatesArgumentName()
    {
        var input = "let x = a ; macro a = b ; 'foo'";
        var builder = new ParsedExpressionFactoryBuilder();
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.NestedExpressionFailure ) ) )
            .Go();
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenMacroNameDuplicatesVariableName()
    {
        var input = "let x = a ; macro x = b ; 'foo'";
        var builder = new ParsedExpressionFactoryBuilder();
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.NestedExpressionFailure ) ) )
            .Go();
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenMacroNameDuplicatesMacroName()
    {
        var input = "macro x = a ; macro x = b ; 'foo'";
        var builder = new ParsedExpressionFactoryBuilder();
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.NestedExpressionFailure ) ) )
            .Go();
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenMacroDeclarationIsEmpty()
    {
        var input = "macro a = ; 'foo'";
        var builder = new ParsedExpressionFactoryBuilder();
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.MacroMustContainAtLeastOneToken ) ) )
            .Go();
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

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.NestedExpressionFailure ) ) )
            .Go();
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

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.MacroResolutionFailure ) ) )
            .Go();
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

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.NestedExpressionFailure ) ) )
            .Go();
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

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.NestedExpressionFailure ) ) )
            .Go();
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

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.NestedExpressionFailure ) ) )
            .Go();
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

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.NestedExpressionFailure ) ) )
            .Go();
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

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.NestedExpressionFailure ) ) )
            .Go();
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

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.NestedExpressionFailure ) ) )
            .Go();
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

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.InvalidMacroParameterCount ) ) )
            .Go();
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

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.MacroResolutionFailure ) ) )
            .Go();
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

        result.TestEquals( expected ).Go();
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

        result.TestEquals( expected ).Go();
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

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.OutputTypeConverterHasThrownException ) ) )
            .Go();
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

        result.TestEquals( value ).Go();
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

        result.TestEquals( expected ).Go();
    }

    [Fact]
    public void Create_ShouldThrowParsedExpressionCreationException_WhenAutomaticResultConversionFailsDueToMissingConversionOperator()
    {
        var input = "12.34";
        var builder = new ParsedExpressionFactoryBuilder();
        var sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<string, string>( input ) );

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations( e, input, ParsedExpressionBuilderErrorType.OutputTypeConverterHasThrownException ) ) )
            .Go();
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

        result.TestEquals( value ).Go();
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

        action.Test(
                exc => exc.TestType()
                    .Exact<ParsedExpressionCreationException>(
                        e => MatchExpectations(
                            e,
                            input,
                            ParsedExpressionBuilderErrorType.ExpressionResultTypeIsNotCompatibleWithExpectedOutputType ) ) )
            .Go();
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

        Assertion.All(
                result.UnboundArguments.TestEmpty(),
                result.DiscardedArguments.Select( n => n.ToString() ).TestSetEqual( [ "a", "b", "c" ] ) )
            .Go();
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

        Assertion.All(
                result.UnboundArguments.Count.TestEquals( 1 ),
                result.UnboundArguments.GetIndex( "a" ).TestEquals( 0 ),
                result.DiscardedArguments.Select( n => n.ToString() ).TestSetEqual( [ "b", "c" ] ) )
            .Go();
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

        Assertion.All(
                result.UnboundArguments.Count.TestEquals( 2 ),
                result.UnboundArguments.GetIndex( "a" ).TestEquals( 0 ),
                result.UnboundArguments.GetIndex( "d" ).TestEquals( 1 ),
                result.DiscardedArguments.Select( n => n.ToString() ).TestSetEqual( [ "b", "c", "e" ] ) )
            .Go();
    }

    [Fact]
    public void Create_ShouldNotRemoveIndexersFromNonParameterExpressionArray_WhenTryingToRemoveUnusedArguments()
    {
        var (aValue, externalValue) = Fixture.CreateManyDistinct<int>( count: 2 );
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

        Assertion.All(
                expression.UnboundArguments.Count.TestEquals( 1 ),
                expression.UnboundArguments.GetIndex( "a" ).TestEquals( 0 ),
                expression.DiscardedArguments.TestEmpty(),
                result.TestEquals( expected ) )
            .Go();
    }

    [Theory]
    [InlineData( -1 )]
    [InlineData( 2 )]
    public void Create_ShouldIgnoreParameterArrayIndexesThatAreOutOfRange_WhenTryingToRemoveUnusedArguments(int index)
    {
        var (aValue, bValue) = Fixture.CreateManyDistinct<int>( count: 2 );

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

        Assertion.All(
                expression.UnboundArguments.Count.TestEquals( 2 ),
                expression.UnboundArguments.GetIndex( "a" ).TestEquals( 0 ),
                expression.UnboundArguments.GetIndex( "b" ).TestEquals( 1 ),
                expression.DiscardedArguments.TestEmpty(),
                action.Test( exc => exc.TestType().Exact<IndexOutOfRangeException>() ) )
            .Go();
    }

    [Fact]
    public void Create_ShouldIgnoreParameterArrayIndexesThatAreNotConstant_WhenTryingToRemoveUnusedArguments()
    {
        var (aValue, bValue, cValue) = Fixture.CreateManyDistinct<int>( count: 3 );
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

        Assertion.All(
                expression.UnboundArguments.Count.TestEquals( 3 ),
                expression.UnboundArguments.GetIndex( "a" ).TestEquals( 0 ),
                expression.UnboundArguments.GetIndex( "b" ).TestEquals( 1 ),
                expression.UnboundArguments.GetIndex( "c" ).TestEquals( 2 ),
                expression.DiscardedArguments.TestEmpty(),
                result.TestEquals( expected ) )
            .Go();
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

        actual.TestEquals( expected ).Go();
    }

    [Fact]
    public void IMathExpressionFactoryCreate_ShouldBeEquivalentToCreate_WhenErrorsOccur()
    {
        var input = "a+b+c";
        var builder = new ParsedExpressionFactoryBuilder();
        IParsedExpressionFactory sut = builder.Build();

        var action = Lambda.Of( () => sut.Create<decimal, decimal>( input ) );

        action.Test( exc => exc.TestType().Exact<ParsedExpressionCreationException>() ).Go();
    }
}
