namespace LfrlAnvil.Sql.Tests;

public class SqlDataTypeParameterTests : TestsBase
{
    [Fact]
    public void Default_ShouldReturnEmptyParameter()
    {
        var sut = default( SqlDataTypeParameter );
        Assertion.All(
                sut.Name.TestEmpty(),
                sut.Bounds.Min.TestEquals( 0 ),
                sut.Bounds.Max.TestEquals( 0 ) )
            .Go();
    }

    [Theory]
    [InlineData( "foo", 0, 0 )]
    [InlineData( "foo", -1, 1 )]
    [InlineData( "bar", 0, 60 )]
    public void Ctor_ShouldCreateCorrectParameter(string name, int min, int max)
    {
        var sut = new SqlDataTypeParameter( name, Bounds.Create( min, max ) );
        Assertion.All(
                sut.Name.TestEquals( name ),
                sut.Bounds.Min.TestEquals( min ),
                sut.Bounds.Max.TestEquals( max ) )
            .Go();
    }

    [Fact]
    public void ToString_ShouldReturnCorrectResult()
    {
        var sut = new SqlDataTypeParameter( "foo", Bounds.Create( 0, 60 ) );
        var result = sut.ToString();
        result.TestEquals( "'foo' [0, 60]" ).Go();
    }
}
