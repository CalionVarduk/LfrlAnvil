using LfrlAnvil.Sql.Builders;
using LfrlAnvil.Sql.Versioning;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.Sql.Tests.VersioningTests;

public class SqlDatabaseVersionTests : TestsBase
{
    [Fact]
    public void Create_ShouldReturnCorrectResult()
    {
        var version = Version.Parse( "1.2.3.4" );
        var description = Fixture.Create<string>();
        var apply = Substitute.For<Action<ISqlDatabaseBuilder>>();
        var builder = Substitute.For<ISqlDatabaseBuilder>();
        var sut = SqlDatabaseVersion.Create( version, description, apply );

        sut.Apply( builder );

        using ( new AssertionScope() )
        {
            sut.Value.Should().BeSameAs( version );
            sut.Description.Should().Be( description );
            apply.Verify().CallAt( 0 ).Exists().And.Arguments.Should().BeSequentiallyEqualTo( builder );
        }
    }

    [Fact]
    public void Create_WithoutDescription_ShouldReturnCorrectResult()
    {
        var version = Version.Parse( "1.2.3.4" );
        var apply = Substitute.For<Action<ISqlDatabaseBuilder>>();
        var builder = Substitute.For<ISqlDatabaseBuilder>();
        var sut = SqlDatabaseVersion.Create( version, apply );

        sut.Apply( builder );

        using ( new AssertionScope() )
        {
            sut.Value.Should().BeSameAs( version );
            sut.Description.Should().BeEmpty();
            apply.Verify().CallAt( 0 ).Exists().And.Arguments.Should().BeSequentiallyEqualTo( builder );
        }
    }

    [Fact]
    public void ToString_ShouldReturnCorrectResult_WhenVersionHasDescription()
    {
        var version = Version.Parse( "1.2.3.4" );
        var description = "SQL database version";
        var apply = Substitute.For<Action<ISqlDatabaseBuilder>>();
        var sut = SqlDatabaseVersion.Create( version, description, apply );

        var result = sut.ToString();

        result.Should().Be( "1.2.3.4 (SQL database version)" );
    }

    [Fact]
    public void ToString_ShouldReturnCorrectResult_WhenVersionDoesNotHaveDescription()
    {
        var version = Version.Parse( "1.2.3.4" );
        var apply = Substitute.For<Action<ISqlDatabaseBuilder>>();
        var sut = SqlDatabaseVersion.Create( version, apply );

        var result = sut.ToString();

        result.Should().Be( "1.2.3.4" );
    }
}
