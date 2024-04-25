using LfrlAnvil.Sql.Statements;

namespace LfrlAnvil.Sql.Tests.StatementsTests;

public class SqlNamedParameterCollectionTests : TestsBase
{
    [Fact]
    public void TryAdd_ShouldAddNewParameter()
    {
        var sut = new SqlNamedParameterCollection();

        var result = sut.TryAdd( "foo", 1 );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            sut.Count.Should().Be( 1 );
            sut.Should().BeEquivalentTo( SqlParameter.Named( "foo", 1 ) );
            sut.Contains( "foo" ).Should().BeTrue();
            sut.TryGet( "foo" ).Should().BeEquivalentTo( SqlParameter.Named( "foo", 1 ) );
        }
    }

    [Fact]
    public void TryAdd_ShouldReturnFalse_WhenParameterAlreadyExists()
    {
        var sut = new SqlNamedParameterCollection();
        sut.TryAdd( "foo", 1 );

        var result = sut.TryAdd( "foo", 2 );

        using ( new AssertionScope() )
        {
            result.Should().BeFalse();
            sut.Count.Should().Be( 1 );
            sut.Should().BeEquivalentTo( SqlParameter.Named( "foo", 1 ) );
            sut.Contains( "foo" ).Should().BeTrue();
            sut.TryGet( "foo" ).Should().BeEquivalentTo( SqlParameter.Named( "foo", 1 ) );
        }
    }

    [Fact]
    public void AddOrUpdate_ShouldAddNewParameter()
    {
        var sut = new SqlNamedParameterCollection();

        sut.AddOrUpdate( "foo", 1 );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 1 );
            sut.Should().BeEquivalentTo( SqlParameter.Named( "foo", 1 ) );
            sut.Contains( "foo" ).Should().BeTrue();
            sut.TryGet( "foo" ).Should().BeEquivalentTo( SqlParameter.Named( "foo", 1 ) );
        }
    }

    [Fact]
    public void AddOrUpdate_ShouldUpdateExistingParameter()
    {
        var sut = new SqlNamedParameterCollection();
        sut.TryAdd( "foo", 1 );

        sut.AddOrUpdate( "foo", 2 );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 1 );
            sut.Should().BeEquivalentTo( SqlParameter.Named( "foo", 2 ) );
            sut.Contains( "foo" ).Should().BeTrue();
            sut.TryGet( "foo" ).Should().BeEquivalentTo( SqlParameter.Named( "foo", 2 ) );
        }
    }

    [Fact]
    public void Clear_ShouldRemoveAllParameters()
    {
        var sut = new SqlNamedParameterCollection();
        sut.AddOrUpdate( "foo", 1 );
        sut.AddOrUpdate( "bar", 2 );

        sut.Clear();

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 0 );
            sut.Should().BeEmpty();
        }
    }
}
