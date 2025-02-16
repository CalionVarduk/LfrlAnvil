using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using LfrlAnvil.Functional;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Versioning;
using LfrlAnvil.Sqlite.Objects;
using LfrlAnvil.Sqlite.Tests.Helpers;

namespace LfrlAnvil.Sqlite.Tests;

public class SqliteDatabaseTests : TestsBase
{
    [Theory]
    [InlineData( true )]
    [InlineData( false )]
    public void Properties_ShouldBeCorrectlyCopiedFromBuilder(bool arePositionalParametersEnabled)
    {
        var dbBuilder = SqliteDatabaseBuilderMock.Create( arePositionalParametersEnabled );
        var sut = SqliteDatabaseMock.Create( dbBuilder );

        Assertion.All(
                sut.Dialect.TestRefEquals( dbBuilder.Dialect ),
                sut.Version.TestEquals( new Version( "0.0" ) ),
                sut.ServerVersion.TestRefEquals( dbBuilder.ServerVersion ),
                sut.DataTypes.TestRefEquals( dbBuilder.DataTypes ),
                sut.TypeDefinitions.TestRefEquals( dbBuilder.TypeDefinitions ),
                sut.NodeInterpreters.TestRefEquals( dbBuilder.NodeInterpreters ),
                sut.QueryReaders.TestRefEquals( dbBuilder.QueryReaders ),
                sut.ParameterBinders.TestRefEquals( dbBuilder.ParameterBinders ),
                sut.ParameterBinders.SupportsPositionalParameters.TestEquals( arePositionalParametersEnabled ),
                sut.Schemas.Database.TestRefEquals( sut ),
                sut.Schemas.Count.TestEquals( 1 ),
                sut.Schemas.Default.Name.TestEmpty(),
                sut.Schemas.TestSequence( [ sut.Schemas.Default ] ),
                sut.Connector.Database.TestRefEquals( sut ),
                (( ISqlDatabaseConnector<DbConnection> )sut.Connector).Database.TestRefEquals( sut ),
                (( ISqlDatabaseConnector )sut.Connector).Database.TestRefEquals( sut ) )
            .Go();
    }

    [Theory]
    [InlineData( "", true )]
    [InlineData( "foo", true )]
    [InlineData( "bar", false )]
    public void Schemas_Contains_ShouldReturnTrue_WhenSchemaExists(string name, bool expected)
    {
        var dbBuilder = SqliteDatabaseBuilderMock.Create();
        dbBuilder.Schemas.Create( "foo" );
        var db = SqliteDatabaseMock.Create( dbBuilder );
        var sut = db.Schemas;

        var result = sut.Contains( name );

        result.TestEquals( expected ).Go();
    }

    [Fact]
    public void Schemas_Get_ShouldReturnExistingSchema()
    {
        var dbBuilder = SqliteDatabaseBuilderMock.Create();
        dbBuilder.Schemas.Default.SetName( "foo" );
        var db = SqliteDatabaseMock.Create( dbBuilder );
        var sut = db.Schemas;

        var result = sut.Get( "foo" );

        result.TestRefEquals( sut.Default ).Go();
    }

    [Fact]
    public void Schemas_Get_ShouldThrowKeyNotFoundException_WhenSchemaDoesNotExist()
    {
        var dbBuilder = SqliteDatabaseBuilderMock.Create();
        dbBuilder.Schemas.Default.SetName( "foo" );
        var db = SqliteDatabaseMock.Create( dbBuilder );
        var sut = db.Schemas;

        var action = Lambda.Of( () => sut.Get( "bar" ) );

        action.Test( exc => exc.TestType().Exact<KeyNotFoundException>() ).Go();
    }

    [Fact]
    public void Schemas_TryGet_ShouldReturnExistingSchema()
    {
        var dbBuilder = SqliteDatabaseBuilderMock.Create();
        dbBuilder.Schemas.Default.SetName( "foo" );
        var db = SqliteDatabaseMock.Create( dbBuilder );
        var sut = db.Schemas;

        var result = sut.TryGet( "foo" );

        result.TestRefEquals( sut.Default ).Go();
    }

    [Fact]
    public void Schemas_TryGet_ShouldReturnNull_WhenSchemaDoesNotExist()
    {
        var dbBuilder = SqliteDatabaseBuilderMock.Create();
        dbBuilder.Schemas.Default.SetName( "foo" );
        var db = SqliteDatabaseMock.Create( dbBuilder );
        var sut = db.Schemas;

        var result = sut.TryGet( "bar" );

        result.TestNull().Go();
    }

    [Fact]
    public void Schemas_GetEnumerator_ShouldReturnCorrectResult()
    {
        var dbBuilder = SqliteDatabaseBuilderMock.Create();
        dbBuilder.Schemas.Create( "foo" );
        var sut = SqliteDatabaseMock.Create( dbBuilder ).Schemas;
        var schema = sut.Get( "foo" );

        var result = new List<SqliteSchema>();
        foreach ( var s in sut )
            result.Add( s );

        Assertion.All(
                result.Count.TestEquals( 2 ),
                result.TestSetEqual( [ sut.Default, schema ] ) )
            .Go();
    }

    [Fact]
    public void Connector_Connect_ForInMemoryDatabase_ShouldAlwaysReturnTheSameObject()
    {
        var factory = new SqliteDatabaseFactory();
        ISqlDatabase sut = factory.Create( "DataSource=:memory:", new SqlDatabaseVersionHistory() ).Database;

        var first = sut.Connector.Connect();
        var second = sut.Connector.Connect();

        first.TestRefEquals( second ).Go();
    }

    [Fact]
    public async Task Connector_ConnectAsync_ForInMemoryDatabase_ShouldAlwaysReturnTheSameObject()
    {
        var factory = new SqliteDatabaseFactory();
        ISqlDatabase sut = factory.Create( "DataSource=:memory:", new SqlDatabaseVersionHistory() ).Database;

        var first = await sut.Connector.ConnectAsync();
        var second = await (( ISqlDatabaseConnector<DbConnection> )sut.Connector).ConnectAsync();

        first.TestRefEquals( second ).Go();
    }

    [Fact]
    public void Dispose_ForInMemoryDatabase_ShouldCloseConnection()
    {
        var factory = new SqliteDatabaseFactory();
        var sut = factory.Create( "DataSource=:memory:", new SqlDatabaseVersionHistory() ).Database;
        var connection = sut.Connector.Connect();

        sut.Dispose();

        connection.State.TestEquals( ConnectionState.Closed ).Go();
    }

    [Fact]
    public void Connection_Open_ForInMemoryDatabase_ShouldThrowInvalidOperationException_WhenConnectionHasBeenClosed()
    {
        var factory = new SqliteDatabaseFactory();
        var sut = factory.Create( "DataSource=:memory:", new SqlDatabaseVersionHistory() ).Database;
        var connection = sut.Connector.Connect();
        sut.Dispose();

        var action = Lambda.Of( () => connection.Open() );

        action.Test( exc => exc.TestType().Exact<InvalidOperationException>() ).Go();
    }

    [Fact]
    public void Connection_ConnectionString_Setter_ForInMemoryDatabase_ShouldThrowInvalidOperationException()
    {
        var factory = new SqliteDatabaseFactory();
        var sut = factory.Create( "DataSource=:memory:", new SqlDatabaseVersionHistory() ).Database;
        var connection = sut.Connector.Connect();

        var action = Lambda.Of( () => connection.ConnectionString = connection.ConnectionString );

        action.Test( exc => exc.TestType().Exact<InvalidOperationException>() ).Go();
    }
}
