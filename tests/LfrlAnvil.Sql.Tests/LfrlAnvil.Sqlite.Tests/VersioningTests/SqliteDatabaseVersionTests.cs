using LfrlAnvil.Functional;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.Sql.Versioning;
using LfrlAnvil.Sqlite.Objects.Builders;
using LfrlAnvil.Sqlite.Tests.Helpers;
using LfrlAnvil.Sqlite.Versioning;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.Sqlite.Tests.VersioningTests;

public class SqliteDatabaseVersionTests : TestsBase
{
    [Fact]
    public void Create_ShouldReturnCorrectResult()
    {
        var version = Version.Parse( "1.2.3.4" );
        var description = Fixture.Create<string>();
        var apply = Substitute.For<Action<SqliteDatabaseBuilder>>();
        var builder = SqliteDatabaseBuilderMock.Create();
        var sut = SqliteDatabaseVersion.Create( version, description, apply );

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
        var apply = Substitute.For<Action<SqliteDatabaseBuilder>>();
        var builder = SqliteDatabaseBuilderMock.Create();
        var sut = SqliteDatabaseVersion.Create( version, apply );

        sut.Apply( builder );

        using ( new AssertionScope() )
        {
            sut.Value.Should().BeSameAs( version );
            sut.Description.Should().BeEmpty();
            apply.Verify().CallAt( 0 ).Exists().And.Arguments.Should().BeSequentiallyEqualTo( builder );
        }
    }

    [Fact]
    public void Apply_ShouldThrowSqlObjectCastException_WhenDatabaseBuilderIsOfInvalidType()
    {
        var version = Version.Parse( "1.2.3.4" );
        var apply = Substitute.For<Action<SqliteDatabaseBuilder>>();
        var builder = Substitute.For<ISqlDatabaseBuilder>();
        var sut = SqliteDatabaseVersion.Create( version, apply );

        var action = Lambda.Of( () => (( ISqlDatabaseVersion )sut).Apply( builder ) );

        action.Should().ThrowExactly<SqlObjectCastException>();
    }
}
