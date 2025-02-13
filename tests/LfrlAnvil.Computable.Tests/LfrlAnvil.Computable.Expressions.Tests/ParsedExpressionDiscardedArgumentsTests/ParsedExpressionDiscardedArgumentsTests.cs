using System.Linq;

namespace LfrlAnvil.Computable.Expressions.Tests.ParsedExpressionDiscardedArgumentsTests;

public class ParsedExpressionDiscardedArgumentsTests : TestsBase
{
    [Fact]
    public void Empty_ShouldReturnCorrectResult()
    {
        var sut = ParsedExpressionDiscardedArguments.Empty;
        sut.TestEmpty().Go();
    }

    [Fact]
    public void Ctor_ShouldReturnCorrectResult()
    {
        var sut = new ParsedExpressionDiscardedArguments( new StringSegment[] { "a", "b", "c" } );

        Assertion.All(
                sut.Count.TestEquals( 3 ),
                sut.Select( n => n.ToString() ).TestSetEqual( [ "a", "b", "c" ] ) )
            .Go();
    }

    [Theory]
    [InlineData( "a" )]
    [InlineData( "b" )]
    [InlineData( "c" )]
    public void Contains_ShouldReturnTrue_WhenNameExists(string name)
    {
        var sut = new ParsedExpressionDiscardedArguments( new StringSegment[] { "a", "b", "c" } );
        var result = sut.Contains( name );
        result.TestTrue().Go();
    }

    [Fact]
    public void Contains_ShouldReturnFalse_WhenNameDoesNotExist()
    {
        var sut = new ParsedExpressionDiscardedArguments( new StringSegment[] { "a" } );
        var result = sut.Contains( "b" );
        result.TestFalse().Go();
    }
}
