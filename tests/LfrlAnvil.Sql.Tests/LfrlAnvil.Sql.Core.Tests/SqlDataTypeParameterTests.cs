namespace LfrlAnvil.Sql.Tests;

public class SqlDataTypeParameterTests : TestsBase
{
    [Fact]
    public void Default_ShouldReturnEmptyParameter()
    {
        var sut = default( SqlDataTypeParameter );
        using ( new AssertionScope() )
        {
            sut.Name.Should().BeEmpty();
            sut.Bounds.Min.Should().Be( 0 );
            sut.Bounds.Max.Should().Be( 0 );
        }
    }

    [Theory]
    [InlineData( "foo", 0, 0 )]
    [InlineData( "foo", -1, 1 )]
    [InlineData( "bar", 0, 60 )]
    public void Ctor_ShouldCreateCorrectParameter(string name, int min, int max)
    {
        var sut = new SqlDataTypeParameter( name, Bounds.Create( min, max ) );
        using ( new AssertionScope() )
        {
            sut.Name.Should().Be( name );
            sut.Bounds.Min.Should().Be( min );
            sut.Bounds.Max.Should().Be( max );
        }
    }

    [Fact]
    public void ToString_ShouldReturnCorrectResult()
    {
        var sut = new SqlDataTypeParameter( "foo", Bounds.Create( 0, 60 ) );
        var result = sut.ToString();
        result.Should().Be( "'foo' [0, 60]" );
    }
}
