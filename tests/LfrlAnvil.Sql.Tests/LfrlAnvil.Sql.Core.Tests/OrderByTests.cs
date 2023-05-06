namespace LfrlAnvil.Sql.Tests;

public class OrderByTests : TestsBase
{
    [Fact]
    public void Asc_ShouldHaveCorrectProperties()
    {
        var sut = OrderBy.Asc;

        using ( new AssertionScope() )
        {
            sut.Name.Should().Be( "ASC" );
            sut.Value.Should().Be( OrderBy.Values.Asc );
        }
    }

    [Fact]
    public void Desc_ShouldHaveCorrectProperties()
    {
        var sut = OrderBy.Desc;

        using ( new AssertionScope() )
        {
            sut.Name.Should().Be( "DESC" );
            sut.Value.Should().Be( OrderBy.Values.Desc );
        }
    }
}
