using System.Collections.Generic;
using System.Linq;
using LfrlAnvil.Functional;

namespace LfrlAnvil.Computable.Expressions.Tests.ParsedExpressionUnboundArgumentsTests;

public class ParsedExpressionUnboundArgumentsTests : TestsBase
{
    [Fact]
    public void Empty_ShouldReturnCorrectResult()
    {
        var sut = ParsedExpressionUnboundArguments.Empty;
        sut.TestEmpty().Go();
    }

    [Fact]
    public void Ctor_ShouldReturnCorrectResult()
    {
        var sut = new ParsedExpressionUnboundArguments(
            new[]
            {
                KeyValuePair.Create( ( StringSegment )"a", 0 ),
                KeyValuePair.Create( ( StringSegment )"b", 1 ),
                KeyValuePair.Create( ( StringSegment )"c", 2 )
            } );

        Assertion.All(
                sut.Count.TestEquals( 3 ),
                sut.Select( kv => KeyValuePair.Create( kv.Key.ToString(), kv.Value ) )
                    .TestSetEqual( [ KeyValuePair.Create( "a", 0 ), KeyValuePair.Create( "b", 1 ), KeyValuePair.Create( "c", 2 ) ] ) )
            .Go();
    }

    [Theory]
    [InlineData( "a" )]
    [InlineData( "b" )]
    [InlineData( "c" )]
    public void Contains_ShouldReturnTrue_WhenNameExists(string name)
    {
        var sut = new ParsedExpressionUnboundArguments(
            new[]
            {
                KeyValuePair.Create( ( StringSegment )"a", 0 ),
                KeyValuePair.Create( ( StringSegment )"b", 1 ),
                KeyValuePair.Create( ( StringSegment )"c", 2 )
            } );

        var result = sut.Contains( name );

        result.TestTrue().Go();
    }

    [Fact]
    public void Contains_ShouldReturnFalse_WhenNameDoesNotExist()
    {
        var sut = new ParsedExpressionUnboundArguments( new[] { KeyValuePair.Create( ( StringSegment )"a", 0 ) } );
        var result = sut.Contains( "b" );
        result.TestFalse().Go();
    }

    [Theory]
    [InlineData( "a", 0 )]
    [InlineData( "b", 1 )]
    [InlineData( "c", 2 )]
    public void GetIndex_ShouldReturnCorrectResult_WhenNameExists(string name, int expected)
    {
        var sut = new ParsedExpressionUnboundArguments(
            new[]
            {
                KeyValuePair.Create( ( StringSegment )"a", 0 ),
                KeyValuePair.Create( ( StringSegment )"b", 1 ),
                KeyValuePair.Create( ( StringSegment )"c", 2 )
            } );

        var result = sut.GetIndex( name );

        result.TestEquals( expected ).Go();
    }

    [Fact]
    public void GetIndex_ShouldReturnMinusOne_WhenNameDoesNotExist()
    {
        var sut = new ParsedExpressionUnboundArguments( new[] { KeyValuePair.Create( ( StringSegment )"a", 0 ) } );
        var result = sut.GetIndex( "b" );
        result.TestEquals( -1 ).Go();
    }

    [Theory]
    [InlineData( 0, "a" )]
    [InlineData( 1, "b" )]
    [InlineData( 2, "c" )]
    public void GetName_ShouldReturnCorrectResult_WhenIndexExists(int index, string expected)
    {
        var sut = new ParsedExpressionUnboundArguments(
            new[]
            {
                KeyValuePair.Create( ( StringSegment )"a", 0 ),
                KeyValuePair.Create( ( StringSegment )"b", 1 ),
                KeyValuePair.Create( ( StringSegment )"c", 2 )
            } );

        var result = sut.GetName( index );

        result.ToString().TestEquals( expected ).Go();
    }

    [Theory]
    [InlineData( -1 )]
    [InlineData( 1 )]
    public void GetName_ShouldThrowIndexOutOfRangeException_WhenIndexDoesNotExist(int index)
    {
        var sut = new ParsedExpressionUnboundArguments( new[] { KeyValuePair.Create( ( StringSegment )"a", 0 ) } );
        var action = Lambda.Of( () => sut.GetName( index ) );
        action.Test( exc => exc.TestType().Exact<IndexOutOfRangeException>() ).Go();
    }
}
