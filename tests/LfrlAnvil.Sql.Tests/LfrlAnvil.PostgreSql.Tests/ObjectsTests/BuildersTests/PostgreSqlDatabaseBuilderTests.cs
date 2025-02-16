using LfrlAnvil.PostgreSql.Extensions;
using LfrlAnvil.PostgreSql.Internal;
using LfrlAnvil.PostgreSql.Objects.Builders;
using LfrlAnvil.PostgreSql.Tests.Helpers;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Expressions.Visitors;
using LfrlAnvil.Sql.Internal;
using LfrlAnvil.Sql.Objects.Builders;
using Npgsql;

namespace LfrlAnvil.PostgreSql.Tests.ObjectsTests.BuildersTests;

public partial class PostgreSqlDatabaseBuilderTests : TestsBase
{
    [Fact]
    public void Ctor_ShouldCreateBuilderWithDefaultSchema()
    {
        var sut = PostgreSqlDatabaseBuilderMock.Create();

        Assertion.All(
                sut.Schemas.Count.TestEquals( 1 ),
                sut.Schemas.Database.TestRefEquals( sut ),
                sut.Schemas.TestSequence( [ sut.Schemas.Default ] ),
                sut.Schemas.Default.Database.TestRefEquals( sut ),
                sut.Schemas.Default.Name.TestEquals( "public" ),
                sut.Schemas.Default.Objects.TestEmpty(),
                sut.Schemas.Default.Objects.Schema.TestRefEquals( sut.Schemas.Default ),
                sut.Dialect.TestRefEquals( PostgreSqlDialect.Instance ),
                sut.ServerVersion.TestEquals( "0.0.0" ),
                sut.Changes.Database.TestRefEquals( sut ),
                sut.Changes.Mode.TestEquals( SqlDatabaseCreateMode.DryRun ),
                sut.Changes.IsAttached.TestTrue(),
                sut.Changes.ActiveObject.TestNull(),
                sut.Changes.ActiveObjectExistenceState.TestEquals( default ),
                sut.Changes.IsActive.TestTrue(),
                sut.Changes.GetPendingActions().ToArray().TestEmpty() )
            .Go();
    }

    [Fact]
    public void Ctor_ShouldCreateBuilderWithDefaultSchemaDifferentThenPublic()
    {
        var sut = PostgreSqlDatabaseBuilderMock.Create( defaultSchemaName: "common" );

        Assertion.All(
                sut.Schemas.Count.TestEquals( 2 ),
                sut.Schemas.Database.TestRefEquals( sut ),
                sut.Schemas.TestSequence( [ sut.Schemas.Default, sut.Schemas.TryGet( "public" )! ] ),
                sut.Schemas.Default.Database.TestRefEquals( sut ),
                sut.Schemas.Default.Name.TestEquals( "common" ),
                sut.Schemas.Default.Objects.TestEmpty(),
                sut.Schemas.Default.Objects.Schema.TestRefEquals( sut.Schemas.Default ),
                sut.Dialect.TestRefEquals( PostgreSqlDialect.Instance ),
                sut.ServerVersion.TestEquals( "0.0.0" ),
                sut.Changes.Database.TestRefEquals( sut ),
                sut.Changes.Mode.TestEquals( SqlDatabaseCreateMode.DryRun ),
                sut.Changes.IsAttached.TestTrue(),
                sut.Changes.ActiveObject.TestNull(),
                sut.Changes.ActiveObjectExistenceState.TestEquals( default ),
                sut.Changes.IsActive.TestTrue(),
                sut.Changes.GetPendingActions().ToArray().TestEmpty() )
            .Go();
    }

    [Fact]
    public void AddConnectionChangeCallback_ShouldNotThrow()
    {
        var sut = PostgreSqlDatabaseBuilderMock.Create();
        var result = sut.AddConnectionChangeCallback( _ => { } );
        result.TestRefEquals( sut ).Go();
    }

    [Theory]
    [InlineData( null, null, null, "CREATE DATABASE \"foo\"" )]
    [InlineData( "enc", "loc", 10, "CREATE DATABASE \"foo\" ENCODING = 'enc' LOCALE = 'loc' CONNECTION LIMIT = 10" )]
    public void Helpers_AppendCreateDatabase_ShouldReturnCorrectResult(
        string? encodingName,
        string? localeName,
        int? concurrentConnectionsLimit,
        string expected)
    {
        var interpreter = new PostgreSqlNodeInterpreter( PostgreSqlNodeInterpreterOptions.Default, SqlNodeInterpreterContext.Create() );
        PostgreSqlHelpers.AppendCreateDatabase( interpreter, "foo", encodingName, localeName, concurrentConnectionsLimit );
        var result = interpreter.Context.Sql.ToString();
        result.TestEquals( expected ).Go();
    }

    [Fact]
    public void Helpers_ExtractConnectionStringEntries_ShouldReturnCorrectResult()
    {
        var connectionString
            = new NpgsqlConnectionStringBuilder( "Host=localhost;Port=5431;Database=tests;UserID=admin;Password=password;Pooling=False" );

        var result = PostgreSqlHelpers.ExtractConnectionStringEntries( connectionString );

        result.TestSequence(
            [
                new SqlConnectionStringEntry( "Host", "localhost", false ), new SqlConnectionStringEntry( "Port", "5431", false ),
                new SqlConnectionStringEntry( "Database", "tests", false ), new SqlConnectionStringEntry( "Username", "admin", true ),
                new SqlConnectionStringEntry( "Password", "password", true ), new SqlConnectionStringEntry( "Pooling", "False", true )
            ] )
            .Go();
    }

    [Fact]
    public void Helpers_ExtendConnectionString_ShouldReturnCorrectResult()
    {
        var connectionString
            = new NpgsqlConnectionStringBuilder( "Host=localhost;Port=5431;Database=tests;UserID=admin;Password=password;Pooling=False" );

        var entries = PostgreSqlHelpers.ExtractConnectionStringEntries( connectionString );
        var result = PostgreSqlHelpers.ExtendConnectionString(
            entries,
            "Port=5432;Database=tests2;UserID=tester;Password=pwd;Pooling=true" );

        result.TestEquals( "Port=5431;Database=tests;Username=tester;Password=pwd;Pooling=True;Host=localhost" ).Go();
    }

    [Fact]
    public void ForPostgreSql_ShouldInvokeAction_WhenDatabaseIsPostgreSql()
    {
        var action = Substitute.For<Action<PostgreSqlDatabaseBuilder>>();
        var sut = PostgreSqlDatabaseBuilderMock.Create();

        var result = sut.ForPostgreSql( action );

        Assertion.All(
                result.TestRefEquals( sut ),
                action.CallAt( 0 ).Arguments.TestSequence( [ sut ] ) )
            .Go();
    }

    [Fact]
    public void ForPostgreSql_ShouldNotInvokeAction_WhenDatabaseIsNotPostgreSql()
    {
        var action = Substitute.For<Action<PostgreSqlDatabaseBuilder>>();
        var sut = Substitute.For<ISqlDatabaseBuilder>();

        var result = sut.ForPostgreSql( action );

        Assertion.All(
                result.TestRefEquals( sut ),
                action.CallCount().TestEquals( 0 ) )
            .Go();
    }
}
