using System.Collections.Generic;
using System.Linq;

namespace LfrlAnvil.Computable.Expressions.Tests.ParsedExpressionBoundArgumentsTests;

public class ParsedExpressionBoundArgumentsTests : TestsBase
{
    [Fact]
    public void Empty_ShouldReturnCorrectResult()
    {
        var sut = ParsedExpressionBoundArguments<int>.Empty;
        sut.TestEmpty().Go();
    }

    [Fact]
    public void Ctor_ShouldReturnCorrectResult()
    {
        var sut = new ParsedExpressionBoundArguments<int>(
            new[]
            {
                KeyValuePair.Create( ( StringSegment )"a", 10 ),
                KeyValuePair.Create( ( StringSegment )"b", 20 ),
                KeyValuePair.Create( ( StringSegment )"c", 30 )
            } );

        Assertion.All(
                sut.Count.TestEquals( 3 ),
                sut.Select( kv => KeyValuePair.Create( kv.Key.ToString(), kv.Value ) )
                    .TestSetEqual( [ KeyValuePair.Create( "a", 10 ), KeyValuePair.Create( "b", 20 ), KeyValuePair.Create( "c", 30 ) ] ) )
            .Go();
    }

    [Theory]
    [InlineData( "a" )]
    [InlineData( "b" )]
    [InlineData( "c" )]
    public void Contains_ShouldReturnTrue_WhenNameExists(string name)
    {
        var sut = new ParsedExpressionBoundArguments<int>(
            new[]
            {
                KeyValuePair.Create( ( StringSegment )"a", 10 ),
                KeyValuePair.Create( ( StringSegment )"b", 20 ),
                KeyValuePair.Create( ( StringSegment )"c", 30 )
            } );

        var result = sut.Contains( name );

        result.TestTrue().Go();
    }

    [Fact]
    public void Contains_ShouldReturnFalse_WhenNameDoesNotExist()
    {
        var sut = new ParsedExpressionBoundArguments<int>( new[] { KeyValuePair.Create( ( StringSegment )"a", 10 ) } );
        var result = sut.Contains( "b" );
        result.TestFalse().Go();
    }

    [Theory]
    [InlineData( "a", 10 )]
    [InlineData( "b", 20 )]
    [InlineData( "c", 30 )]
    public void TryGetValue_ShouldReturnCorrectResult_WhenNameExists(string name, int expected)
    {
        var sut = new ParsedExpressionBoundArguments<int>(
            new[]
            {
                KeyValuePair.Create( ( StringSegment )"a", 10 ),
                KeyValuePair.Create( ( StringSegment )"b", 20 ),
                KeyValuePair.Create( ( StringSegment )"c", 30 )
            } );

        var result = sut.TryGetValue( name, out var outResult );

        Assertion.All(
                result.TestTrue(),
                outResult.TestEquals( expected ) )
            .Go();
    }

    [Fact]
    public void TryGetValue_ShouldReturnFalse_WhenNameDoesNotExist()
    {
        var sut = new ParsedExpressionBoundArguments<int>( new[] { KeyValuePair.Create( ( StringSegment )"a", 10 ) } );

        var result = sut.TryGetValue( "b", out var outResult );

        Assertion.All(
                result.TestFalse(),
                outResult.TestEquals( default ) )
            .Go();
    }
}
