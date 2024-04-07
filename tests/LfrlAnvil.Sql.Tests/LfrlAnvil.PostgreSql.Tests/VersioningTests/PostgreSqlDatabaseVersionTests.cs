using LfrlAnvil.Functional;
using LfrlAnvil.PostgreSql.Objects.Builders;
using LfrlAnvil.PostgreSql.Tests.Helpers;
using LfrlAnvil.PostgreSql.Versioning;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.Sql.Versioning;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.PostgreSql.Tests.VersioningTests;

public class PostgreSqlDatabaseVersionTests : TestsBase
{
    [Fact]
    public void Create_ShouldReturnCorrectResult()
    {
        var version = Version.Parse( "1.2.3.4" );
        var description = Fixture.Create<string>();
        var apply = Substitute.For<Action<PostgreSqlDatabaseBuilder>>();
        var builder = PostgreSqlDatabaseBuilderMock.Create();
        var sut = PostgreSqlDatabaseVersion.Create( version, description, apply );

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
        var apply = Substitute.For<Action<PostgreSqlDatabaseBuilder>>();
        var builder = PostgreSqlDatabaseBuilderMock.Create();
        var sut = PostgreSqlDatabaseVersion.Create( version, apply );

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
        var apply = Substitute.For<Action<PostgreSqlDatabaseBuilder>>();
        var builder = Substitute.For<ISqlDatabaseBuilder>();
        var sut = PostgreSqlDatabaseVersion.Create( version, apply );

        var action = Lambda.Of( () => (( ISqlDatabaseVersion )sut).Apply( builder ) );

        action.Should().ThrowExactly<SqlObjectCastException>();
    }
}
