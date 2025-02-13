using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using LfrlAnvil.Computable.Expressions.Constructs;
using LfrlAnvil.Computable.Expressions.Constructs.Int32;
using LfrlAnvil.Computable.Expressions.Constructs.Int64;
using LfrlAnvil.Computable.Expressions.Constructs.String;
using LfrlAnvil.Computable.Expressions.Constructs.Variadic;
using LfrlAnvil.Computable.Expressions.Internal;
using LfrlAnvil.TestExtensions.Attributes;

namespace LfrlAnvil.Computable.Expressions.Tests.ParsedExpressionFactoryTests;

public partial class ParsedExpressionFactoryTests
{
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

        result.TestEquals( "foobar" ).Go();
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

        result.TestEquals( 12.34m ).Go();
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

        result.TestEquals( expected ).Go();
    }

    [Theory]
    [MethodData( nameof( ParsedExpressionFactoryTestsData.GetArgumentOnlyData ) )]
    public void DelegateInvoke_ShouldReturnCorrectResult_WhenExpressionConsistsOfOnlyArgumentSymbol(string input, decimal value)
    {
        var builder = new ParsedExpressionFactoryBuilder();
        var sut = builder.Build();

        var expression = sut.Create<decimal, decimal>( input );

        Assertion.All(
                expression.UnboundArguments.Select( kv => kv.Key.ToString() ).TestSequence( [ "a" ] ),
                expression.BoundArguments.TestEmpty(),
                expression.DiscardedArguments.TestEmpty(),
                expression.UnboundArguments.Count != 1 ? Assertion.All() : expression.Compile().Invoke( value ).TestEquals( value ) )
            .Go();
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

        result.TestEquals( "ZERO" ).Go();
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

        result.TestEquals( "Func()" ).Go();
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

        result.TestEmpty().Go();
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

        result.TestEquals( expected ).Go();
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

        result.TestEquals( expected ).Go();
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

        result.TestEquals( expected ).Go();
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

        result.TestEquals( expected ).Go();
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

        result.TestEquals( expected ).Go();
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

        result.TestEquals( expected ).Go();
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

        result.TestEquals( expected ).Go();
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

        result.TestEquals( expected ).Go();
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

        result.TestEquals( expected ).Go();
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

        result.TestEquals( expected ).Go();
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

        result.TestEquals( expected ).Go();
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

        result.TestEquals( "foobar" ).Go();
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

        result.TestEquals( "foobar" ).Go();
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

        result.TestEquals( "foobar" ).Go();
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

        result.TestEquals( "foobar" ).Go();
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

        result.TestEquals( expected ).Go();
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

        result.TestEquals( "foobar" ).Go();
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

        result.TestEquals( "foobarqux" ).Go();
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

        Assertion.All(
                expression.UnboundArguments.TestEmpty(),
                @delegate.Invoke().TestEquals( 0 ) )
            .Go();
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

        Assertion.All(
                expression.UnboundArguments.Contains( "a" ).TestTrue(),
                @delegate.Invoke( 100 ).TestEquals( 0 ) )
            .Go();
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

        result.TestSequence( expected ).Go();
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

        result.TestSequence( expected ).Go();
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

        result.TestCount( count => count.TestEquals( 3 ) )
            .Then(
                values =>
                {
                    var first = values[0];
                    var second = values[1];
                    var third = values[2];
                    return Assertion.All(
                        "values",
                        first.TestSequence( [ "a" ] ),
                        second.TestEmpty(),
                        third.TestSequence( [ "b", "c" ] ) );
                } )
            .Go();
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

        result.TestSequence( [ "a", 1m, true ] ).Go();
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

        result.TestSequence( [ "a", 1m, true ] ).Go();
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

        delegateResult.TestEquals( "( foo|BiOp|bar )" ).Go();
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

        delegateResult.TestEquals( "( ( 1|BiOp|2 )|BiOp|3 )" ).Go();
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

        result.TestEquals( "( foo|BiOp|bar )" ).Go();
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

        result.TestEquals( "( foo|BiOp|bar )" ).Go();
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

        result.TestEquals( "( foo|BiOp|bar )qux" ).Go();
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

        delegateResult.TestEquals( "qux" ).Go();
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

        delegateResult.TestEquals( "( ( foo|BiOp|bar )|BiOp|foobar )" ).Go();
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

        delegateResult.TestEquals( "( foo|BiOp|bar )" ).Go();
    }

    [Fact]
    public void DelegateInvoke_ShouldReturnCorrectResult_WhenExpressionContainsDelegateWithManyParameters()
    {
        var input = """
                    ( [ string a , string b , string c , string d , string e , string f , string g , string h , string i , string j ]
                    a + b + c + d + e + f + g + h + i + j ) ( 'a' , 'b' , 'c' , 'd' , 'e' , 'f' , 'g' , 'h' , 'i' , 'j' )
                    """;

        var builder = new ParsedExpressionFactoryBuilder()
            .AddTypeDeclaration<string>( "string" )
            .AddBinaryOperator( "+", new ParsedExpressionAddStringOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var sut = builder.Build();

        var expression = sut.Create<string, string>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke();

        result.TestEquals( "abcdefghij" ).Go();
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

        result.TestEquals( "foo" ).Go();
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

        delegateResult.TestEquals( "( foo|BiOp|bar )" ).Go();
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

        result.TestEquals( "( foo|BiOp|bar )" ).Go();
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

        result.TestEquals( "( ( foo|BiOp|bar )|BiOp|( foo|BiOp|qux ) )" ).Go();
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

        delegateResult.TestEquals( "( foo|BiOp|bar )" ).Go();
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

        delegateResult.TestEquals( "( ( foo|BiOp|bar )|BiOp|qux )" ).Go();
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

        result.TestEquals( "( ( foo|BiOp|bar )|BiOp|qux )" ).Go();
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

        delegateResult.TestEquals( "( ( foo|BiOp|bar )|BiOp|qux )" ).Go();
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

        delegateResult.TestEquals( "( ( ( foo|BiOp|bar )|BiOp|qux )|BiOp|baz )" ).Go();
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

        result.TestEquals( "( ( ( foo|BiOp|bar )|BiOp|qux )|BiOp|baz )" ).Go();
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

        delegateResult.TestEquals( "( ( ( foo|BiOp|bar )|BiOp|qux )|BiOp|baz )" ).Go();
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

        delegateResult.TestEquals( "( ( foo|BiOp|( ( foo|BiOp|bar )|BiOp|qux ) )|BiOp|( ( foo|BiOp|baz )|BiOp|qux ) )" ).Go();
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

        delegateResult.TestEquals( "( ( ( foo|BiOp|bar )|BiOp|qux )|BiOp|foo )" ).Go();
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

        result.TestEquals( 5 ).Go();
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

        result.TestEquals( 5 ).Go();
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

        delegateResult.TestEquals( 105 ).Go();
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

        delegateResult.TestEquals( 105 ).Go();
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

        result.TestEquals( 5 ).Go();
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

        result.TestEquals( 80 ).Go();
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

        delegateResult.TestEquals( "( foo|BiOp|foo(( foo|BiOp|bar )) )" ).Go();
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

        delegateResult.TestEquals( expected ).Go();
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

        delegateResult.TestEquals( expected ).Go();
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

        result.TestEquals( "publicField" ).Go();
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

        result.TestEquals( "publicProperty" ).Go();
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

        result.TestEquals( value.PublicMethodZero() ).Go();
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

        result.TestEquals( expected ).Go();
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

        result.TestEquals( value.PublicGenericMethodThreeSameType( "foo", "bar", "qux" ) ).Go();
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

        result.TestEquals( value.PublicGenericMethodThreeDiffTypes( "foo", 1m, true ) ).Go();
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

        result.TestEquals( expected ).Go();
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

        result.TestEquals( expected ).Go();
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

        result.TestEquals( "publicField" ).Go();
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

        result.TestEquals( value.PublicConstrainedMethod( "foo" ) ).Go();
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

        result.TestEquals( value.PublicMethodOne( "foo" ) ).Go();
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

        result.TestEquals( value.PublicMethodZero() ).Go();
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

        Assertion.All(
                expression.UnboundArguments.TestEmpty(),
                @delegate.Invoke().TestEquals( 0 ) )
            .Go();
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

        Assertion.All(
                expression.UnboundArguments.Contains( "a" ).TestTrue(),
                @delegate.Invoke( 100 ).TestEquals( 0 ) )
            .Go();
    }

    [Fact]
    public void DelegateInvoke_ShouldReturnCorrectResult_ForParameterlessCtorCall()
    {
        var input = "list()";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddTypeDeclaration<List<int>>( "list" );

        var sut = builder.Build();

        var expression = sut.Create<int, List<int>>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke();

        result.TestEmpty().Go();
    }

    [Fact]
    public void DelegateInvoke_ShouldReturnCorrectResult_ForCtorCallWithParameters()
    {
        var input = "dt( y , m , d )";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddTypeDeclaration<DateTime>( "dt" );

        var sut = builder.Build();

        var expression = sut.Create<int, DateTime>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke( 2022, 9, 10 );

        result.TestEquals( new DateTime( 2022, 9, 10 ) ).Go();
    }

    [Fact]
    public void DelegateInvoke_ShouldReturnCorrectResult_ForCtorCallWithParametersWhenAllParametersAreConstant()
    {
        var input = "dt( 2022 , 9 , 10 )";
        var builder = new ParsedExpressionFactoryBuilder()
            .SetNumberParserProvider( p => ParsedExpressionNumberParser.CreateDefaultInt32( p.Configuration ) )
            .AddTypeDeclaration<DateTime>( "dt" );

        var sut = builder.Build();

        var expression = sut.Create<int, DateTime>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke();

        result.TestEquals( new DateTime( 2022, 9, 10 ) ).Go();
    }

    [Fact]
    public void DelegateInvoke_ShouldReturnCorrectResult_WhenCtorParametersAreConstantWithEnabledConstantsFolding()
    {
        var input = "dt( 0 ).Ticks * a";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddTypeDeclaration<DateTime>( "dt" )
            .SetCtorCallProvider( c => new ParsedExpressionConstructorCall( c, foldConstantsWhenPossible: true ) )
            .SetNumberParserProvider( p => ParsedExpressionNumberParser.CreateDefaultInt64( p.Configuration ) )
            .AddBinaryOperator( "*", new ParsedExpressionMultiplyInt64Operator() )
            .SetBinaryOperatorPrecedence( "*", 1 );

        var sut = builder.Build();

        var expression = sut.Create<long, long>( input );
        var @delegate = expression.Compile();

        Assertion.All(
                expression.UnboundArguments.TestEmpty(),
                @delegate.Invoke().TestEquals( 0 ) )
            .Go();
    }

    [Fact]
    public void DelegateInvoke_ShouldReturnCorrectResult_WhenCtorParametersAreConstantWithDisabledConstantsFolding()
    {
        var input = "dt( 0 ).Ticks * a";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddTypeDeclaration<DateTime>( "dt" )
            .SetCtorCallProvider( c => new ParsedExpressionConstructorCall( c, foldConstantsWhenPossible: false ) )
            .SetNumberParserProvider( p => ParsedExpressionNumberParser.CreateDefaultInt64( p.Configuration ) )
            .AddBinaryOperator( "*", new ParsedExpressionMultiplyInt64Operator() )
            .SetBinaryOperatorPrecedence( "*", 1 );

        var sut = builder.Build();

        var expression = sut.Create<long, long>( input );
        var @delegate = expression.Compile();

        Assertion.All(
                expression.UnboundArguments.Contains( "a" ).TestTrue(),
                @delegate.Invoke( 100 ).TestEquals( 0 ) )
            .Go();
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

        result.TestEquals( "publicProperty_next".Length ).Go();
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

        result.TestEquals( "publicProperty" ).Go();
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

        result.TestEquals( "privateField" ).Go();
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

        result.TestEquals( "privateProperty" ).Go();
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

        result.TestEquals( "privateProperty" ).Go();
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

        result.TestEquals( "foo" ).Go();
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

        result.TestEquals( "( foobar|BiOp|qux )".Length ).Go();
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

        result.TestEquals( "( PreOp|6 )" ).Go();
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

        result.TestEquals( "( 6|PostOp )" ).Go();
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

        result.TestEquals( expected ).Go();
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

        result.TestEquals( "publicField" ).Go();
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

        Assertion.All(
                expression.UnboundArguments.TestEmpty(),
                @delegate.Invoke().TestEquals( 0 ) )
            .Go();
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

        Assertion.All(
                expression.UnboundArguments.Contains( "a" ).TestTrue(),
                @delegate.Invoke( 100 ).TestEquals( 0 ) )
            .Go();
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

        result.TestEquals( "publicProperty" ).Go();
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

        Assertion.All(
                expression.UnboundArguments.TestEmpty(),
                @delegate.Invoke().TestEquals( 0 ) )
            .Go();
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

        Assertion.All(
                expression.UnboundArguments.Contains( "a" ).TestTrue(),
                @delegate.Invoke( 100 ).TestEquals( 0 ) )
            .Go();
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

        result.TestEquals( expected ).Go();
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

        result.TestEquals( expected ).Go();
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

        result.TestEquals( 'f' ).Go();
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

        result.TestEquals( 2 ).Go();
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

        result.TestEquals( 1 ).Go();
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

        result.TestEquals( 'a' ).Go();
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

        result.TestEquals( 1 ).Go();
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

        result.TestEquals( 'u' ).Go();
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

        result.TestEquals( 'f' ).Go();
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

        result.TestEquals( "( PreOp|a )" ).Go();
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

        result.TestEquals( "( a|PostOp )" ).Go();
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

        result.TestEquals( expected ).Go();
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

        result.TestEquals( "a" ).Go();
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

        Assertion.All(
                expression.UnboundArguments.TestEmpty(),
                @delegate.Invoke().TestEquals( 0 ) )
            .Go();
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

        Assertion.All(
                expression.UnboundArguments.Contains( "a" ).TestTrue(),
                @delegate.Invoke( 100 ).TestEquals( 0 ) )
            .Go();
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

        result.TestEquals( 'a' ).Go();
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

        Assertion.All(
                expression.UnboundArguments.TestEmpty(),
                @delegate.Invoke().TestEquals( 0 ) )
            .Go();
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

        Assertion.All(
                expression.UnboundArguments.Contains( "a" ).TestTrue(),
                @delegate.Invoke( 100 ).TestEquals( 0 ) )
            .Go();
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

        result.TestEquals( "foo".Length ).Go();
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

        result.TestEquals( 'o' ).Go();
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

        result.TestEquals( value.PublicMethodOne( "foo" ) ).Go();
    }

    [Fact]
    public void DelegateInvoke_ShouldReturnCorrectResult_WhenCtorCallVariadicIsCalledDirectly()
    {
        var input = "CTOR_CALL( DATETIME , 2022 , 9 , 10 )";
        var builder = new ParsedExpressionFactoryBuilder()
            .SetNumberParserProvider( p => ParsedExpressionNumberParser.CreateDefaultInt32( p.Configuration ) )
            .AddConstant( "DATETIME", new ParsedExpressionConstant<Type>( typeof( DateTime ) ) );

        var sut = builder.Build();

        var expression = sut.Create<int, DateTime>( input );
        var @delegate = expression.Compile();
        var result = @delegate.Invoke();

        result.TestEquals( new DateTime( 2022, 9, 10 ) ).Go();
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

        result.TestSequence( [ "foo", "bar" ] ).Go();
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

        result.TestEquals( "foobar" ).Go();
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

        result.TestEquals( "( foo|PostOp )" ).Go();
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

        result.TestEquals( "( ( foo|BiOp|bar )|BiOp|qux )" ).Go();
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

        result.TestEquals( "( ( foo|PostOp )|BiOp|qux )" ).Go();
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

        result.TestEquals( "( ( foo|BiOp|bar )|BiOp|( foo|BiOp|qux ) )" ).Go();
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

        result.TestEquals( "( ( foo|BiOp|bar )|BiOp|qux )" ).Go();
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

        result.TestEquals( "( ( ( foo|BiOp|bar )|BiOp|( ( foo|BiOp|bar )|BiOp|qux ) )|BiOp|( ( foo|BiOp|bar )|BiOp|foo ) )" ).Go();
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

        Assertion.All(
                result.TestEquals( "( foo|BiOp|bar )" ),
                expression.Body.NodeType.TestNotEquals( ExpressionType.Block ) )
            .Go();
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

        Assertion.All(
                result.TestEquals( "( foo|BiOp|bar )" ),
                expression.Body.NodeType.TestNotEquals( ExpressionType.Block ) )
            .Go();
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

        result.TestEquals( expected ).Go();
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

        Assertion.All(
                result.TestEquals( "( foo|BiOp|bar )" ),
                expression.Body.NodeType.TestEquals( ExpressionType.Block ),
                expression.Body.TestType().AssignableTo<BlockExpression>( block => block.Expressions.Count.TestEquals( 2 ) ) )
            .Go();
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

        Assertion.All(
                result.TestEquals( "( foo|BiOp|bar )" ),
                expression.Body.NodeType.TestEquals( ExpressionType.Block ),
                expression.Body.TestType().AssignableTo<BlockExpression>( block => block.Expressions.Count.TestEquals( 2 ) ) )
            .Go();
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

        result.TestEquals( "( ( PreOp|foo )|BiOp|bar )" ).Go();
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

        result.TestEquals( "( foo|BiOp|bar )" ).Go();
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

        result.TestEquals( "( foo|BiOp|bar )" ).Go();
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

        result.TestEquals( "( ( foo|BiOp|bar )|BiOp|( ( qux|BiOp|bar )|BiOp|bar ) )" ).Go();
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

        Assertion.All(
                expression.UnboundArguments.TestEmpty(),
                expression.Body.NodeType.TestNotEquals( ExpressionType.Block ),
                @delegate.Invoke().TestEquals( 0 ) )
            .Go();
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

        Assertion.All(
                expression.UnboundArguments.TestEmpty(),
                expression.Body.NodeType.TestNotEquals( ExpressionType.Block ),
                @delegate.Invoke().TestEquals( 0 ) )
            .Go();
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

        result.TestEquals( "( foo|BiOp|bar )" ).Go();
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

        result.TestEquals( "( ( foo|BiOp|bar )|BiOp|qux )" ).Go();
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

        result.TestEquals( "( ( 1|BiOp|True )|BiOp|bar )" ).Go();
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

        result.TestEquals( "( foo|BiOp|bar )" ).Go();
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

        result.TestEquals( "( foo|BiOp|bar )" ).Go();
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

        result.TestEquals( "( foo|BiOp|bar )" ).Go();
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

        result.TestEquals( "( ( foo|BiOp|bar )|BiOp|( foo|BiOp|qux ) )" ).Go();
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

        result.TestEquals( "ZERO" ).Go();
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

        result.TestEquals( "( foo|BiOp|bar )" ).Go();
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

        result.TestEquals( "( ( foo|BiOp|foo )|BiOp|foo )" ).Go();
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

        result.TestEquals( "foo" ).Go();
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

        result.TestEquals( "foo".Length ).Go();
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

        result.TestSequence( [ "foo", "bar", "qux" ] ).Go();
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

        result.TestEquals( "Func(foo,bar,qux)" ).Go();
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

        result.TestEquals( false ).Go();
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

        result.TestEquals( "( foo|BiOp|bar )" ).Go();
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

        result.TestEquals( "( foo|BiOp|bar )" ).Go();
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

        result.TestEquals( "( foo|BiOp|bar )" ).Go();
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

        result.TestEquals( "( foo|BiOp|bar )" ).Go();
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

        result.TestEquals( "( foo|BiOp|( bar|BiOp|qux ) )" ).Go();
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

        result.TestEquals( "( foo|BiOp|bar )" ).Go();
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

        result.TestEquals( "( foo|BiOp|bar )" ).Go();
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

        result.TestEquals( "( False|BiOp|True )" ).Go();
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

        result.TestEquals( "( qux|BiOp|( foo|BiOp|bar ) )" ).Go();
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

        result.TestEquals( "( ( ( foo|BiOp|bar )|BiOp|bar )|BiOp|bar )" ).Go();
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

        result.TestEquals( "Func(foo,bar,qux)" ).Go();
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

        Assertion.All(
                expression.UnboundArguments.Contains( "a" ).TestTrue(),
                @delegate.Invoke( 100 ).TestEquals( 0 ) )
            .Go();
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

        Assertion.All(
                expression.UnboundArguments.TestEmpty(),
                @delegate.Invoke().TestEquals( 0 ) )
            .Go();
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

        Assertion.All(
                expression.UnboundArguments.TestEmpty(),
                @delegate.Invoke().TestEquals( 0 ) )
            .Go();
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

        Assertion.All(
                expression.UnboundArguments.Contains( "a" ).TestTrue(),
                @delegate.Invoke( 100 ).TestEquals( 0 ) )
            .Go();
    }
}
