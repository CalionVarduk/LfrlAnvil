using LfrlAnvil.PostgreSql.Extensions;
using LfrlAnvil.PostgreSql.Internal;
using LfrlAnvil.PostgreSql.Objects.Builders;
using LfrlAnvil.PostgreSql.Tests.Helpers;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Internal;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.TestExtensions.FluentAssertions;
using Npgsql;

namespace LfrlAnvil.PostgreSql.Tests.ObjectsTests.BuildersTests;

public partial class PostgreSqlDatabaseBuilderTests : TestsBase
{
    [Fact]
    public void Ctor_ShouldCreateBuilderWithDefaultSchema()
    {
        var sut = PostgreSqlDatabaseBuilderMock.Create();

        using ( new AssertionScope() )
        {
            sut.Schemas.Count.Should().Be( 1 );
            sut.Schemas.Database.Should().BeSameAs( sut );
            sut.Schemas.Should().BeSequentiallyEqualTo( sut.Schemas.Default );

            sut.Schemas.Default.Database.Should().BeSameAs( sut );
            sut.Schemas.Default.Name.Should().Be( "public" );
            sut.Schemas.Default.Objects.Should().BeEmpty();
            sut.Schemas.Default.Objects.Schema.Should().BeSameAs( sut.Schemas.Default );

            sut.Dialect.Should().BeSameAs( PostgreSqlDialect.Instance );
            sut.ServerVersion.Should().Be( "0.0.0" );

            sut.Changes.Database.Should().BeSameAs( sut );
            sut.Changes.Mode.Should().Be( SqlDatabaseCreateMode.DryRun );
            sut.Changes.IsAttached.Should().BeTrue();
            sut.Changes.ActiveObject.Should().BeNull();
            sut.Changes.ActiveObjectExistenceState.Should().Be( default( SqlObjectExistenceState ) );
            sut.Changes.IsActive.Should().BeTrue();
            sut.Changes.GetPendingActions().ToArray().Should().BeEmpty();
        }
    }

    [Fact]
    public void AddConnectionChangeCallback_ShouldNotThrow()
    {
        var sut = PostgreSqlDatabaseBuilderMock.Create();
        var result = sut.AddConnectionChangeCallback( _ => { } );
        result.Should().BeSameAs( sut );
    }

    [Fact]
    public void Helpers_ExtractConnectionStringEntries_ShouldReturnCorrectResult()
    {
        var connectionString = new NpgsqlConnectionStringBuilder(
            "Host=localhost;Port=5431;Database=tests;UserID=admin;Password=password;Pooling=False" );

        var result = PostgreSqlHelpers.ExtractConnectionStringEntries( connectionString );

        result.Should()
            .BeSequentiallyEqualTo(
                new SqlConnectionStringEntry( "Host", "localhost", false ),
                new SqlConnectionStringEntry( "Port", "5431", false ),
                new SqlConnectionStringEntry( "Database", "tests", false ),
                new SqlConnectionStringEntry( "Username", "admin", true ),
                new SqlConnectionStringEntry( "Password", "password", true ),
                new SqlConnectionStringEntry( "Pooling", "False", true ) );
    }

    [Fact]
    public void Helpers_ExtendConnectionString_ShouldReturnCorrectResult()
    {
        var connectionString = new NpgsqlConnectionStringBuilder(
            "Host=localhost;Port=5431;Database=tests;UserID=admin;Password=password;Pooling=False" );

        var entries = PostgreSqlHelpers.ExtractConnectionStringEntries( connectionString );
        var result = PostgreSqlHelpers.ExtendConnectionString(
            entries,
            "Port=5432;Database=tests2;UserID=tester;Password=pwd;Pooling=true" );

        result.Should().Be( "Port=5431;Database=tests;Username=tester;Password=pwd;Pooling=True;Host=localhost" );
    }

    [Fact]
    public void ForPostgreSql_ShouldInvokeAction_WhenDatabaseIsPostgreSql()
    {
        var action = Substitute.For<Action<PostgreSqlDatabaseBuilder>>();
        var sut = PostgreSqlDatabaseBuilderMock.Create();

        var result = sut.ForPostgreSql( action );

        result.Should().BeSameAs( sut );
        action.Verify().CallAt( 0 ).Exists().And.Arguments.Should().BeSequentiallyEqualTo( sut );
    }

    [Fact]
    public void ForPostgreSql_ShouldNotInvokeAction_WhenDatabaseIsNotPostgreSql()
    {
        var action = Substitute.For<Action<PostgreSqlDatabaseBuilder>>();
        var sut = Substitute.For<ISqlDatabaseBuilder>();

        var result = sut.ForPostgreSql( action );

        result.Should().BeSameAs( sut );
        action.Verify().CallCount.Should().Be( 0 );
    }
}
