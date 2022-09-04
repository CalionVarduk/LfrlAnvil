using System.Linq;
using FluentAssertions.Execution;

namespace LfrlAnvil.Computable.Expressions.Tests.ParsedExpressionDiscardedArgumentsTests;

public class ParsedExpressionDiscardedArgumentsTests : TestsBase
{
    [Fact]
    public void Empty_ShouldReturnCorrectResult()
    {
        var sut = ParsedExpressionDiscardedArguments.Empty;
        sut.Should().BeEmpty();
    }

    [Fact]
    public void Ctor_ShouldReturnCorrectResult()
    {
        var sut = new ParsedExpressionDiscardedArguments( new[] { "a".AsMemory(), "b".AsMemory(), "c".AsMemory() } );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 3 );
            sut.Select( n => n.ToString() ).Should().BeEquivalentTo( "a", "b", "c" );
        }
    }

    [Theory]
    [InlineData( "a" )]
    [InlineData( "b" )]
    [InlineData( "c" )]
    public void Contains_ShouldReturnTrue_WhenNameExists(string name)
    {
        var sut = new ParsedExpressionDiscardedArguments( new[] { "a".AsMemory(), "b".AsMemory(), "c".AsMemory() } );
        var result = sut.Contains( name );
        result.Should().BeTrue();
    }

    [Fact]
    public void Contains_ShouldReturnFalse_WhenNameDoesNotExist()
    {
        var sut = new ParsedExpressionDiscardedArguments( new[] { "a".AsMemory() } );
        var result = sut.Contains( "b" );
        result.Should().BeFalse();
    }
}
