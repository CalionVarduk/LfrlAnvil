using LfrlAnvil.Sql.Statements;

namespace LfrlAnvil.Sql.Tests.StatementsTests;

public class SqlNamedParameterCollectionTests : TestsBase
{
    [Fact]
    public void TryAdd_ShouldAddNewParameter()
    {
        var sut = new SqlNamedParameterCollection();

        var result = sut.TryAdd( "foo", 1 );

        Assertion.All(
                result.TestTrue(),
                sut.Count.TestEquals( 1 ),
                sut.TestSetEqual( [ SqlParameter.Named( "foo", 1 ) ] ),
                sut.Contains( "foo" ).TestTrue(),
                sut.TryGet( "foo" ).TestEquals( SqlParameter.Named( "foo", 1 ) ) )
            .Go();
    }

    [Fact]
    public void TryAdd_ShouldReturnFalse_WhenParameterAlreadyExists()
    {
        var sut = new SqlNamedParameterCollection();
        sut.TryAdd( "foo", 1 );

        var result = sut.TryAdd( "foo", 2 );

        Assertion.All(
                result.TestFalse(),
                sut.Count.TestEquals( 1 ),
                sut.TestSetEqual( [ SqlParameter.Named( "foo", 1 ) ] ),
                sut.Contains( "foo" ).TestTrue(),
                sut.TryGet( "foo" ).TestEquals( SqlParameter.Named( "foo", 1 ) ) )
            .Go();
    }

    [Fact]
    public void AddOrUpdate_ShouldAddNewParameter()
    {
        var sut = new SqlNamedParameterCollection();

        sut.AddOrUpdate( "foo", 1 );

        Assertion.All(
                sut.Count.TestEquals( 1 ),
                sut.TestSetEqual( [ SqlParameter.Named( "foo", 1 ) ] ),
                sut.Contains( "foo" ).TestTrue(),
                sut.TryGet( "foo" ).TestEquals( SqlParameter.Named( "foo", 1 ) ) )
            .Go();
    }

    [Fact]
    public void AddOrUpdate_ShouldUpdateExistingParameter()
    {
        var sut = new SqlNamedParameterCollection();
        sut.TryAdd( "foo", 1 );

        sut.AddOrUpdate( "foo", 2 );

        Assertion.All(
                sut.Count.TestEquals( 1 ),
                sut.TestSetEqual( [ SqlParameter.Named( "foo", 2 ) ] ),
                sut.Contains( "foo" ).TestTrue(),
                sut.TryGet( "foo" ).TestEquals( SqlParameter.Named( "foo", 2 ) ) )
            .Go();
    }

    [Fact]
    public void Clear_ShouldRemoveAllParameters()
    {
        var sut = new SqlNamedParameterCollection();
        sut.AddOrUpdate( "foo", 1 );
        sut.AddOrUpdate( "bar", 2 );

        sut.Clear();

        Assertion.All(
                sut.Count.TestEquals( 0 ),
                sut.TestEmpty() )
            .Go();
    }
}
