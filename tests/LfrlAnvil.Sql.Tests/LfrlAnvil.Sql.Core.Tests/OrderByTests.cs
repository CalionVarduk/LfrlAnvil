namespace LfrlAnvil.Sql.Tests;

public class OrderByTests : TestsBase
{
    [Fact]
    public void Asc_ShouldHaveCorrectProperties()
    {
        var sut = OrderBy.Asc;

        Assertion.All(
                sut.Name.TestEquals( "ASC" ),
                sut.Value.TestEquals( OrderBy.Values.Asc ) )
            .Go();
    }

    [Fact]
    public void Desc_ShouldHaveCorrectProperties()
    {
        var sut = OrderBy.Desc;

        Assertion.All(
                sut.Name.TestEquals( "DESC" ),
                sut.Value.TestEquals( OrderBy.Values.Desc ) )
            .Go();
    }
}
