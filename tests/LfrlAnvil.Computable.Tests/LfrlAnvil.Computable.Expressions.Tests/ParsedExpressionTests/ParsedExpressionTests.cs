using System.Collections.Generic;
using System.Linq;
using LfrlAnvil.Computable.Expressions.Constructs;
using LfrlAnvil.Computable.Expressions.Constructs.Decimal;
using LfrlAnvil.Computable.Expressions.Exceptions;
using LfrlAnvil.Functional;

namespace LfrlAnvil.Computable.Expressions.Tests.ParsedExpressionTests;

public class ParsedExpressionTests : TestsBase
{
    [Fact]
    public void Expression_ShouldBeCreatedWithoutBoundArguments()
    {
        var input = "a + 12.34 + b";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new ParsedExpressionAddOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var factory = builder.Build();

        var sut = factory.Create<decimal, decimal>( input );

        Assertion.All(
                sut.Input.TestEquals( input ),
                sut.BoundArguments.TestEmpty(),
                sut.DiscardedArguments.TestEmpty(),
                sut.UnboundArguments.Count.TestEquals( 2 ),
                sut.UnboundArguments.GetIndex( "a" ).TestEquals( 0 ),
                sut.UnboundArguments.GetIndex( "b" ).TestEquals( 1 ) )
            .Go();
    }

    [Fact]
    public void Expression_ShouldBeCreatedWithDiscardedArguments_WhenSomeArgumentsHaveBeenOptimizedAwayByConstructs()
    {
        var input = "0 * a + b";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new ParsedExpressionAddOperator() )
            .AddBinaryOperator( "*", new ParsedExpressionMultiplyDecimalOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 )
            .SetBinaryOperatorPrecedence( "*", 1 );

        var factory = builder.Build();

        var sut = factory.Create<decimal, decimal>( input );

        Assertion.All(
                sut.Input.TestEquals( input ),
                sut.BoundArguments.TestEmpty(),
                sut.DiscardedArguments.Select( n => n.ToString() ).TestSetEqual( [ "a" ] ),
                sut.UnboundArguments.Count.TestEquals( 1 ),
                sut.UnboundArguments.GetIndex( "b" ).TestEquals( 0 ) )
            .Go();
    }

    [Fact]
    public void ToString_ShouldReturnInfoAboutGenericArgumentsAndInput()
    {
        var input = "a + 12.34 + b";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new ParsedExpressionAddOperator() )
            .AddPrefixTypeConverter( "[double]", new ParsedExpressionTypeConverter<double>() )
            .SetBinaryOperatorPrecedence( "+", 1 )
            .SetPrefixUnaryConstructPrecedence( "[double]", 1 );

        var factory = builder.Build();
        var sut = factory.Create<decimal, double>( input );

        var result = sut.ToString();

        result.TestEquals( "[System.Decimal => System.Double] a + 12.34 + b" ).Go();
    }

    [Fact]
    public void BindArguments_WithStringKey_ShouldBindExpressionArgumentsCorrectly()
    {
        var (bValue, cValue, eValue) = Fixture.CreateManyDistinct<decimal>( count: 3 );

        var input = "a + b + c + d + e";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new ParsedExpressionAddOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var factory = builder.Build();
        var sut = factory.Create<decimal, decimal>( input );

        var result = sut.BindArguments(
            KeyValuePair.Create( "b", bValue ),
            KeyValuePair.Create( "c", cValue ),
            KeyValuePair.Create( "e", eValue ) );

        Assertion.All(
                result.UnboundArguments.Count.TestEquals( 2 ),
                result.UnboundArguments.GetIndex( "a" ).TestEquals( 0 ),
                result.UnboundArguments.GetIndex( "d" ).TestEquals( 1 ),
                result.BoundArguments.Count.TestEquals( 3 ),
                result.BoundArguments.TryGetValue( "b", out var actualB ).TestTrue(),
                result.BoundArguments.TryGetValue( "c", out var actualC ).TestTrue(),
                result.BoundArguments.TryGetValue( "e", out var actualE ).TestTrue(),
                actualB.TestEquals( bValue ),
                actualC.TestEquals( cValue ),
                actualE.TestEquals( eValue ) )
            .Go();
    }

    [Fact]
    public void BindArguments_WithStringSegmentKey_ShouldBindExpressionArgumentsCorrectly()
    {
        var (bValue, cValue, eValue) = Fixture.CreateManyDistinct<decimal>( count: 3 );

        var input = "a + b + c + d + e";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new ParsedExpressionAddOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var factory = builder.Build();
        var sut = factory.Create<decimal, decimal>( input );

        var result = sut.BindArguments(
            KeyValuePair.Create( ( StringSegment )"b", bValue ),
            KeyValuePair.Create( ( StringSegment )"c", cValue ),
            KeyValuePair.Create( ( StringSegment )"e", eValue ) );

        Assertion.All(
                result.UnboundArguments.Count.TestEquals( 2 ),
                result.UnboundArguments.GetIndex( "a" ).TestEquals( 0 ),
                result.UnboundArguments.GetIndex( "d" ).TestEquals( 1 ),
                result.BoundArguments.Count.TestEquals( 3 ),
                result.BoundArguments.TryGetValue( "b", out var actualB ).TestTrue(),
                result.BoundArguments.TryGetValue( "c", out var actualC ).TestTrue(),
                result.BoundArguments.TryGetValue( "e", out var actualE ).TestTrue(),
                actualB.TestEquals( bValue ),
                actualC.TestEquals( cValue ),
                actualE.TestEquals( eValue ) )
            .Go();
    }

    [Fact]
    public void BindArguments_WithIndexKey_ShouldBindExpressionArgumentsCorrectly()
    {
        var (bValue, cValue, eValue) = Fixture.CreateManyDistinct<decimal>( count: 3 );

        var input = "a + b + c + d + e";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new ParsedExpressionAddOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var factory = builder.Build();
        var sut = factory.Create<decimal, decimal>( input );

        var result = sut.BindArguments(
            KeyValuePair.Create( 1, bValue ),
            KeyValuePair.Create( 2, cValue ),
            KeyValuePair.Create( 4, eValue ) );

        Assertion.All(
                result.UnboundArguments.Count.TestEquals( 2 ),
                result.UnboundArguments.GetIndex( "a" ).TestEquals( 0 ),
                result.UnboundArguments.GetIndex( "d" ).TestEquals( 1 ),
                result.BoundArguments.Count.TestEquals( 3 ),
                result.BoundArguments.TryGetValue( "b", out var actualB ).TestTrue(),
                result.BoundArguments.TryGetValue( "c", out var actualC ).TestTrue(),
                result.BoundArguments.TryGetValue( "e", out var actualE ).TestTrue(),
                actualB.TestEquals( bValue ),
                actualC.TestEquals( cValue ),
                actualE.TestEquals( eValue ) )
            .Go();
    }

    [Fact]
    public void BindArguments_ShouldReturnThis_WhenParameterCollectionIsMaterializedAndEmpty()
    {
        var input = "a + b + c + d + e";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new ParsedExpressionAddOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var factory = builder.Build();
        var sut = factory.Create<decimal, decimal>( input );

        var result = sut.BindArguments( Array.Empty<KeyValuePair<int, decimal>>() );

        result.TestRefEquals( sut ).Go();
    }

    [Fact]
    public void BindArguments_ShouldReturnThis_WhenParameterCollectionIsNotMaterializedAndEmpty()
    {
        var input = "a + b + c + d + e";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new ParsedExpressionAddOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var factory = builder.Build();
        var sut = factory.Create<decimal, decimal>( input );

        var result = sut.BindArguments(
            new[] { KeyValuePair.Create( 1, 0m ), KeyValuePair.Create( 2, 0m ), KeyValuePair.Create( 4, 0m ) }
                .Where( _ => false ) );

        result.TestRefEquals( sut ).Go();
    }

    [Fact]
    public void BindArguments_CalledInChain_ShouldBindExpressionArgumentsCorrectly()
    {
        var (bValue, cValue, eValue) = Fixture.CreateManyDistinct<decimal>( count: 3 );

        var input = "a + b + c + d + e";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new ParsedExpressionAddOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var factory = builder.Build();
        var sut = factory.Create<decimal, decimal>( input );

        var result = sut
            .BindArguments( KeyValuePair.Create( "b", bValue ) )
            .BindArguments( KeyValuePair.Create( "c", cValue ) )
            .BindArguments( KeyValuePair.Create( "e", eValue ) );

        Assertion.All(
                result.UnboundArguments.Count.TestEquals( 2 ),
                result.UnboundArguments.GetIndex( "a" ).TestEquals( 0 ),
                result.UnboundArguments.GetIndex( "d" ).TestEquals( 1 ),
                result.BoundArguments.Count.TestEquals( 3 ),
                result.BoundArguments.TryGetValue( "b", out var actualB ).TestTrue(),
                result.BoundArguments.TryGetValue( "c", out var actualC ).TestTrue(),
                result.BoundArguments.TryGetValue( "e", out var actualE ).TestTrue(),
                actualB.TestEquals( bValue ),
                actualC.TestEquals( cValue ),
                actualE.TestEquals( eValue ) )
            .Go();
    }

    [Fact]
    public void BindArguments_ShouldMarkArgumentsAsDiscardedCorrectlyWhenSomeOfThemGetOptimizedAwayDueToBinding()
    {
        var input = "a * b + c";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new ParsedExpressionAddOperator() )
            .AddBinaryOperator( "*", new ParsedExpressionMultiplyDecimalOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 )
            .SetBinaryOperatorPrecedence( "*", 1 );

        var factory = builder.Build();
        var sut = factory.Create<decimal, decimal>( input );

        var result = sut.BindArguments( KeyValuePair.Create( "a", 0m ) );

        Assertion.All(
                result.DiscardedArguments.Select( n => n.ToString() ).TestSetEqual( [ "b" ] ),
                result.UnboundArguments.Count.TestEquals( 1 ),
                result.UnboundArguments.GetIndex( "c" ).TestEquals( 0 ),
                result.BoundArguments.Count.TestEquals( 1 ),
                result.BoundArguments.TryGetValue( "a", out var actualA ).TestTrue(),
                actualA.TestEquals( 0m ) )
            .Go();
    }

    [Fact]
    public void BindArguments_ShouldAddDiscardedArgumentsCorrectlyWhenSomeOfThemGetOptimizedAwayDueToBindingAndDueToOriginalParsing()
    {
        var input = "(0 * a) + (b * c) + d";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new ParsedExpressionAddOperator() )
            .AddBinaryOperator( "*", new ParsedExpressionMultiplyDecimalOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 )
            .SetBinaryOperatorPrecedence( "*", 1 );

        var factory = builder.Build();
        var sut = factory.Create<decimal, decimal>( input );

        var result = sut.BindArguments( KeyValuePair.Create( "b", 0m ) );

        Assertion.All(
                result.DiscardedArguments.Select( n => n.ToString() ).TestSetEqual( [ "a", "c" ] ),
                result.UnboundArguments.Count.TestEquals( 1 ),
                result.UnboundArguments.GetIndex( "d" ).TestEquals( 0 ),
                result.BoundArguments.Count.TestEquals( 1 ),
                result.BoundArguments.TryGetValue( "b", out var actualB ).TestTrue(),
                actualB.TestEquals( 0m ) )
            .Go();
    }

    [Fact]
    public void
        BindArguments_ShouldThrowMathExpressionArgumentBindingException_WhenAnyArgumentToBindDoesNotExistInUnboundArgumentsCollection()
    {
        var (bValue, cValue, fValue) = Fixture.CreateManyDistinct<decimal>( count: 3 );

        var input = "a + b + c + d + e";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new ParsedExpressionAddOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var factory = builder.Build();
        var sut = factory.Create<decimal, decimal>( input );

        var action = Lambda.Of(
            () => sut
                .BindArguments( KeyValuePair.Create( "b", bValue ) )
                .BindArguments( KeyValuePair.Create( "c", cValue ) )
                .BindArguments( KeyValuePair.Create( "f", fValue ) ) );

        action.Test( exc => exc.TestType().Exact<ParsedExpressionArgumentBindingException>() ).Go();
    }

    [Fact]
    public void BindArguments_ShouldThrowParsedExpressionCreationException_WhenArgumentBindingCausesAnErrorDuringExpressionParsing()
    {
        var input = "a / b";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "/", new ParsedExpressionDivideDecimalOperator() )
            .SetBinaryOperatorPrecedence( "/", 1 );

        var factory = builder.Build();
        var sut = factory.Create<decimal, decimal>( input );

        var action = Lambda.Of( () => sut.BindArguments( KeyValuePair.Create( "b", 0m ) ) );

        action.Test( exc => exc.TestType().Exact<ParsedExpressionCreationException>() ).Go();
    }

    [Fact]
    public void BindArguments_ShouldCreateExpressionThatCompilesToCorrectDelegate()
    {
        var (aValue, bValue, cValue, dValue, eValue) = Fixture.CreateManyDistinct<decimal>( count: 5 );

        var input = "a + b + c + d + e";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new ParsedExpressionAddOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var factory = builder.Build();
        var sut = factory.Create<decimal, decimal>( input );

        var result = sut.BindArguments(
            KeyValuePair.Create( "b", bValue ),
            KeyValuePair.Create( "c", cValue ),
            KeyValuePair.Create( "e", eValue ) );

        var unboundDelegate = sut.Compile();
        var boundDelegate = result.Compile();

        var unboundResult = unboundDelegate.Invoke( aValue, bValue, cValue, dValue, eValue );
        var boundResult = boundDelegate.Invoke( aValue, dValue );

        unboundResult.TestEquals( boundResult ).Go();
    }

    [Fact]
    public void IMathExpressionBindArguments_WithEnumerableStringKey_ShouldBeEquivalentToBindArguments()
    {
        var (aValue, bValue) = Fixture.CreateManyDistinct<decimal>( count: 2 );
        var expected = aValue + bValue;

        var input = "a + b";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new ParsedExpressionAddOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var factory = builder.Build();
        IParsedExpression<decimal, decimal> sut = factory.Create<decimal, decimal>( input );

        var result = sut.BindArguments( new[] { KeyValuePair.Create( "a", aValue ) }.AsEnumerable() );
        var @delegate = result.Compile();
        var resultValue = @delegate.Invoke( bValue );

        resultValue.TestEquals( expected ).Go();
    }

    [Fact]
    public void IMathExpressionBindArguments_WithParamsStringKey_ShouldBeEquivalentToBindArguments()
    {
        var (aValue, bValue) = Fixture.CreateManyDistinct<decimal>( count: 2 );
        var expected = aValue + bValue;

        var input = "a + b";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new ParsedExpressionAddOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var factory = builder.Build();
        IParsedExpression<decimal, decimal> sut = factory.Create<decimal, decimal>( input );

        var result = sut.BindArguments( KeyValuePair.Create( "a", aValue ) );
        var @delegate = result.Compile();
        var resultValue = @delegate.Invoke( bValue );

        resultValue.TestEquals( expected ).Go();
    }

    [Fact]
    public void IMathExpressionBindArguments_WithEnumerableStringSegmentKey_ShouldBeEquivalentToBindArguments()
    {
        var (aValue, bValue) = Fixture.CreateManyDistinct<decimal>( count: 2 );
        var expected = aValue + bValue;

        var input = "a + b";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new ParsedExpressionAddOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var factory = builder.Build();
        IParsedExpression<decimal, decimal> sut = factory.Create<decimal, decimal>( input );

        var result = sut.BindArguments( new[] { KeyValuePair.Create( ( StringSegment )"a", aValue ) }.AsEnumerable() );
        var @delegate = result.Compile();
        var resultValue = @delegate.Invoke( bValue );

        resultValue.TestEquals( expected ).Go();
    }

    [Fact]
    public void IMathExpressionBindArguments_WithParamsStringSegmentKey_ShouldBeEquivalentToBindArguments()
    {
        var (aValue, bValue) = Fixture.CreateManyDistinct<decimal>( count: 2 );
        var expected = aValue + bValue;

        var input = "a + b";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new ParsedExpressionAddOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var factory = builder.Build();
        IParsedExpression<decimal, decimal> sut = factory.Create<decimal, decimal>( input );

        var result = sut.BindArguments( KeyValuePair.Create( ( StringSegment )"a", aValue ) );
        var @delegate = result.Compile();
        var resultValue = @delegate.Invoke( bValue );

        resultValue.TestEquals( expected ).Go();
    }

    [Fact]
    public void IMathExpressionBindArguments_WithEnumerableIndexKey_ShouldBeEquivalentToBindArguments()
    {
        var (aValue, bValue) = Fixture.CreateManyDistinct<decimal>( count: 2 );
        var expected = aValue + bValue;

        var input = "a + b";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new ParsedExpressionAddOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var factory = builder.Build();
        IParsedExpression<decimal, decimal> sut = factory.Create<decimal, decimal>( input );

        var result = sut.BindArguments( new[] { KeyValuePair.Create( 0, aValue ) }.AsEnumerable() );
        var @delegate = result.Compile();
        var resultValue = @delegate.Invoke( bValue );

        resultValue.TestEquals( expected ).Go();
    }

    [Fact]
    public void IMathExpressionBindArguments_WithParamsIndexKey_ShouldBeEquivalentToBindArguments()
    {
        var (aValue, bValue) = Fixture.CreateManyDistinct<decimal>( count: 2 );
        var expected = aValue + bValue;

        var input = "a + b";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new ParsedExpressionAddOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var factory = builder.Build();
        IParsedExpression<decimal, decimal> sut = factory.Create<decimal, decimal>( input );

        var result = sut.BindArguments( KeyValuePair.Create( 0, aValue ) );
        var @delegate = result.Compile();
        var resultValue = @delegate.Invoke( bValue );

        resultValue.TestEquals( expected ).Go();
    }
}
