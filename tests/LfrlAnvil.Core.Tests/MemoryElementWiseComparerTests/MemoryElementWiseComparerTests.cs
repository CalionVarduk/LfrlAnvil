namespace LfrlAnvil.Tests.MemoryElementWiseComparerTests;

public class MemoryElementWiseComparerTests : TestsBase
{
    [Theory]
    [InlineData( "", "", true )]
    [InlineData( "bar", "bar", true )]
    [InlineData( "bar", "baz", false )]
    [InlineData( "foo", "bar", false )]
    [InlineData( "", "foo", false )]
    [InlineData( "foo", "", false )]
    [InlineData( "foo", "foob", false )]
    [InlineData( "bar", "b", false )]
    [InlineData( "b", "bar", false )]
    public void Equals_ShouldReturnCorrectResult(string a, string b, bool expected)
    {
        var sut = new MemoryElementWiseComparer<char>();
        var result = sut.Equals( a.AsMemory(), b.AsMemory() );
        result.Should().Be( expected );
    }

    [Fact]
    public void GetHashCode_ShouldReturnDefaultValue_WhenMemoryIsEmpty()
    {
        var sut = new MemoryElementWiseComparer<char>();
        var result = sut.GetHashCode( string.Empty.AsMemory() );
        result.Should().Be( Hash.Default.Value );
    }

    [Fact]
    public void GetHashCode_ShouldIncludeAllElements_WhenMemoryContainsElements()
    {
        var sut = new MemoryElementWiseComparer<char>();
        var expected = Hash.Default.Add( 'b' ).Add( 'a' ).Add( 'r' ).Value;
        var result = sut.GetHashCode( "bar".AsMemory() );
        result.Should().Be( expected );
    }
}
