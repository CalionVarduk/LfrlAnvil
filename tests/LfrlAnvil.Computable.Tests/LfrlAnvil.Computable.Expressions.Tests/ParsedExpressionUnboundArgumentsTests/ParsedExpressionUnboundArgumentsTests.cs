using System.Collections.Generic;
using System.Linq;
using LfrlAnvil.Extensions;
using LfrlAnvil.Functional;

namespace LfrlAnvil.Computable.Expressions.Tests.ParsedExpressionUnboundArgumentsTests;

public class ParsedExpressionUnboundArgumentsTests : TestsBase
{
    [Fact]
    public void Empty_ShouldReturnCorrectResult()
    {
        var sut = ParsedExpressionUnboundArguments.Empty;
        sut.Should().BeEmpty();
    }

    [Fact]
    public void Ctor_ShouldReturnCorrectResult()
    {
        var sut = new ParsedExpressionUnboundArguments(
            new[]
            {
                KeyValuePair.Create( "a".AsSlice(), 0 ),
                KeyValuePair.Create( "b".AsSlice(), 1 ),
                KeyValuePair.Create( "c".AsSlice(), 2 )
            } );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 3 );
            sut.Select( kv => KeyValuePair.Create( kv.Key.ToString(), kv.Value ) )
                .Should()
                .BeEquivalentTo( KeyValuePair.Create( "a", 0 ), KeyValuePair.Create( "b", 1 ), KeyValuePair.Create( "c", 2 ) );
        }
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
                KeyValuePair.Create( "a".AsSlice(), 0 ),
                KeyValuePair.Create( "b".AsSlice(), 1 ),
                KeyValuePair.Create( "c".AsSlice(), 2 )
            } );

        var result = sut.Contains( name );

        result.Should().BeTrue();
    }

    [Fact]
    public void Contains_ShouldReturnFalse_WhenNameDoesNotExist()
    {
        var sut = new ParsedExpressionUnboundArguments( new[] { KeyValuePair.Create( "a".AsSlice(), 0 ) } );
        var result = sut.Contains( "b" );
        result.Should().BeFalse();
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
                KeyValuePair.Create( "a".AsSlice(), 0 ),
                KeyValuePair.Create( "b".AsSlice(), 1 ),
                KeyValuePair.Create( "c".AsSlice(), 2 )
            } );

        var result = sut.GetIndex( name );

        result.Should().Be( expected );
    }

    [Fact]
    public void GetIndex_ShouldReturnMinusOne_WhenNameDoesNotExist()
    {
        var sut = new ParsedExpressionUnboundArguments( new[] { KeyValuePair.Create( "a".AsSlice(), 0 ) } );
        var result = sut.GetIndex( "b" );
        result.Should().Be( -1 );
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
                KeyValuePair.Create( "a".AsSlice(), 0 ),
                KeyValuePair.Create( "b".AsSlice(), 1 ),
                KeyValuePair.Create( "c".AsSlice(), 2 )
            } );

        var result = sut.GetName( index );

        result.ToString().Should().Be( expected );
    }

    [Theory]
    [InlineData( -1 )]
    [InlineData( 1 )]
    public void GetName_ShouldThrowIndexOutOfRangeException_WhenIndexDoesNotExist(int index)
    {
        var sut = new ParsedExpressionUnboundArguments( new[] { KeyValuePair.Create( "a".AsSlice(), 0 ) } );
        var action = Lambda.Of( () => sut.GetName( index ) );
        action.Should().ThrowExactly<IndexOutOfRangeException>();
    }
}
