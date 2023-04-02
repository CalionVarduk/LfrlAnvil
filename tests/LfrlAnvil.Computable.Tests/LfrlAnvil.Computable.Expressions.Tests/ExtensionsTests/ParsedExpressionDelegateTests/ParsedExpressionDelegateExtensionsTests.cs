using System.Collections.Generic;
using System.Linq;
using LfrlAnvil.Computable.Expressions.Constructs;
using LfrlAnvil.Computable.Expressions.Exceptions;
using LfrlAnvil.Computable.Expressions.Extensions;
using LfrlAnvil.Functional;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.Computable.Expressions.Tests.ExtensionsTests.ParsedExpressionDelegateTests;

public class ParsedExpressionDelegateExtensionsTests : TestsBase
{
    [Fact]
    public void MapArguments_WithStringKey_ShouldReturnCorrectlyPopulatedArray()
    {
        var (aValue, bValue, dValue) = Fixture.CreateDistinctCollection<decimal>( count: 3 );

        var input = "a + b + c + d";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new ParsedExpressionAddOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var factory = builder.Build();
        var expression = factory.Create<decimal, decimal>( input );
        var sut = expression.Compile();

        var result = sut.MapArguments(
            KeyValuePair.Create( "a", aValue ),
            KeyValuePair.Create( "b", bValue ),
            KeyValuePair.Create( "d", dValue ) );

        result.Should().BeSequentiallyEqualTo( aValue, bValue, default, dValue );
    }

    [Fact]
    public void MapArguments_WithStringSegmentKey_ShouldReturnCorrectlyPopulatedArray()
    {
        var (aValue, bValue, dValue) = Fixture.CreateDistinctCollection<decimal>( count: 3 );

        var input = "a + b + c + d";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new ParsedExpressionAddOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var factory = builder.Build();
        var expression = factory.Create<decimal, decimal>( input );
        var sut = expression.Compile();

        var result = sut.MapArguments(
            KeyValuePair.Create( (StringSegment)"a", aValue ),
            KeyValuePair.Create( (StringSegment)"b", bValue ),
            KeyValuePair.Create( (StringSegment)"d", dValue ) );

        result.Should().BeSequentiallyEqualTo( aValue, bValue, default, dValue );
    }

    [Fact]
    public void MapArguments_WithStringKeyAndCustomBuffer_ShouldCorrectlyPopulateProvidedArray()
    {
        var (aValue, bValue, dValue) = Fixture.CreateDistinctCollection<decimal>( count: 3 );

        var input = "a + b + c + d";
        var buffer = new decimal[4];
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new ParsedExpressionAddOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var factory = builder.Build();
        var expression = factory.Create<decimal, decimal>( input );
        var sut = expression.Compile();

        sut.MapArguments(
            buffer,
            KeyValuePair.Create( "a", aValue ),
            KeyValuePair.Create( "b", bValue ),
            KeyValuePair.Create( "d", dValue ) );

        buffer.Should().BeSequentiallyEqualTo( aValue, bValue, default, dValue );
    }

    [Fact]
    public void MapArguments_WithStringSegmentKeyAndCustomBuffer_ShouldCorrectlyPopulateProvidedArray()
    {
        var (aValue, bValue, dValue) = Fixture.CreateDistinctCollection<decimal>( count: 3 );

        var input = "a + b + c + d";
        var buffer = new decimal[4];
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new ParsedExpressionAddOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var factory = builder.Build();
        var expression = factory.Create<decimal, decimal>( input );
        var sut = expression.Compile();

        sut.MapArguments(
            buffer,
            KeyValuePair.Create( (StringSegment)"a", aValue ),
            KeyValuePair.Create( (StringSegment)"b", bValue ),
            KeyValuePair.Create( (StringSegment)"d", dValue ) );

        buffer.Should().BeSequentiallyEqualTo( aValue, bValue, default, dValue );
    }

    [Fact]
    public void MapArguments_WithCustomBuffer_ShouldCorrectlyPopulateProvidedArray_EvenWhenBufferIsLargerThanNeeded()
    {
        var (aValue, bValue, dValue) = Fixture.CreateDistinctCollection<decimal>( count: 3 );
        var buffer = Fixture.CreateDistinctCollection<decimal>( count: 6 ).ToArray();
        var oldThirdValue = buffer[2];
        var oldFifthValue = buffer[4];
        var oldSixthValue = buffer[5];

        var input = "a + b + c + d";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new ParsedExpressionAddOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var factory = builder.Build();
        var expression = factory.Create<decimal, decimal>( input );
        var sut = expression.Compile();

        sut.MapArguments(
            buffer,
            KeyValuePair.Create( "a", aValue ),
            KeyValuePair.Create( "b", bValue ),
            KeyValuePair.Create( "d", dValue ) );

        buffer.Should().BeSequentiallyEqualTo( aValue, bValue, oldThirdValue, dValue, oldFifthValue, oldSixthValue );
    }

    [Fact]
    public void MapArguments_WithCustomBuffer_ShouldDoNothing_WhenDelegateDoesNotHaveAnyArguments()
    {
        var buffer = Fixture.CreateDistinctCollection<decimal>( count: 3 ).ToArray();
        var oldFirstValue = buffer[0];
        var oldSecondValue = buffer[1];
        var oldThirdValue = buffer[2];

        var input = "0";
        var builder = new ParsedExpressionFactoryBuilder();
        var factory = builder.Build();
        var expression = factory.Create<decimal, decimal>( input );
        var sut = expression.Compile();

        sut.MapArguments( buffer, Array.Empty<KeyValuePair<string, decimal>>() );

        buffer.Should().BeSequentiallyEqualTo( oldFirstValue, oldSecondValue, oldThirdValue );
    }

    [Theory]
    [InlineData( 0 )]
    [InlineData( 1 )]
    [InlineData( 2 )]
    [InlineData( 3 )]
    public void
        MapArguments_WithCustomBuffer_ShouldThrowMathExpressionArgumentBufferTooSmallException_WhenBufferLengthIsLessThanExpressionArgumentCount(
            int bufferLength)
    {
        var buffer = new decimal[bufferLength];

        var input = "a + b + c + d";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new ParsedExpressionAddOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var factory = builder.Build();
        var expression = factory.Create<decimal, decimal>( input );
        var sut = expression.Compile();

        var action = Lambda.Of( () => sut.MapArguments( buffer, Array.Empty<KeyValuePair<string, decimal>>() ) );

        action.Should().ThrowExactly<ParsedExpressionArgumentBufferTooSmallException>();
    }

    [Fact]
    public void MapArguments_ShouldThrowInvalidMathExpressionArgumentsException_WhenAnyArgumentDoesNotExistInDelegate()
    {
        var input = "a + b + c + d";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new ParsedExpressionAddOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var factory = builder.Build();
        var expression = factory.Create<decimal, decimal>( input );
        var sut = expression.Compile();

        var action = Lambda.Of(
            () => sut.MapArguments( KeyValuePair.Create( "a", 0m ), KeyValuePair.Create( "e", 0m ), KeyValuePair.Create( "f", 0m ) ) );

        action.Should()
            .ThrowExactly<InvalidParsedExpressionArgumentsException>()
            .AndMatch( e => e.ArgumentNames.Select( n => n.ToString() ).SequenceEqual( new[] { "e", "f" } ) );
    }
}
