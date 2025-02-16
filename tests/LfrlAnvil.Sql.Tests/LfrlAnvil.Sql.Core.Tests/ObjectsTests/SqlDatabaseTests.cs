using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using LfrlAnvil.Functional;
using LfrlAnvil.Sql.Extensions;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.Sql.Versioning;
using LfrlAnvil.TestExtensions.Sql.Mocks;
using LfrlAnvil.TestExtensions.Sql.Mocks.System;

namespace LfrlAnvil.Sql.Tests.ObjectsTests;

public class SqlDatabaseTests : TestsBase
{
    [Fact]
    public void Properties_ShouldBeCorrectlyCopiedFromBuilder()
    {
        var dbBuilder = SqlDatabaseBuilderMock.Create();
        ISqlDatabase sut = SqlDatabaseMock.Create( dbBuilder );

        Assertion.All(
                sut.Dialect.TestRefEquals( dbBuilder.Dialect ),
                sut.Version.TestEquals( new Version( "0.0.0" ) ),
                sut.ServerVersion.TestRefEquals( dbBuilder.ServerVersion ),
                sut.DataTypes.TestRefEquals( dbBuilder.DataTypes ),
                sut.TypeDefinitions.TestRefEquals( dbBuilder.TypeDefinitions ),
                sut.NodeInterpreters.TestRefEquals( dbBuilder.NodeInterpreters ),
                sut.QueryReaders.TestRefEquals( dbBuilder.QueryReaders ),
                sut.ParameterBinders.TestRefEquals( dbBuilder.ParameterBinders ),
                sut.Schemas.Database.TestRefEquals( sut ),
                sut.Schemas.Count.TestEquals( 1 ),
                sut.Schemas.Default.Name.TestEquals( "common" ),
                sut.Schemas.TestSequence( [ sut.Schemas.Default ] ),
                (( ISqlDatabaseConnector<DbConnectionMock> )sut.Connector).Database.TestRefEquals( sut ),
                (( ISqlDatabaseConnector<DbConnection> )sut.Connector).Database.TestRefEquals( sut ),
                sut.Connector.Database.TestRefEquals( sut ) )
            .Go();
    }

    [Fact]
    public void Dispose_ShouldNotThrow()
    {
        var dbBuilder = SqlDatabaseBuilderMock.Create();
        ISqlDatabase sut = SqlDatabaseMock.Create( dbBuilder );

        var action = Lambda.Of( () => sut.Dispose() );

        action.Test( exc => exc.TestNull() ).Go();
    }

    [Fact]
    public void Connector_Connect_ShouldCallConnectImplementation()
    {
        var dbBuilder = SqlDatabaseBuilderMock.Create();
        ISqlDatabase sut = SqlDatabaseMock.Create( dbBuilder );

        var result = sut.Connector.Connect();

        result.State.TestEquals( ConnectionState.Open ).Go();
    }

    [Fact]
    public void Connector_Connect_WithOptions_ShouldCallConnectImplementation()
    {
        var dbBuilder = SqlDatabaseBuilderMock.Create();
        ISqlDatabase sut = SqlDatabaseMock.Create( dbBuilder );

        var result = sut.Connector.Connect( "test=true" );

        Assertion.All(
                result.State.TestEquals( ConnectionState.Open ),
                result.ConnectionString.TestEquals( "test=true" ) )
            .Go();
    }

    [Fact]
    public async Task Connector_ConnectAsync_ShouldCallConnectImplementation()
    {
        var dbBuilder = SqlDatabaseBuilderMock.Create();
        ISqlDatabase sut = SqlDatabaseMock.Create( dbBuilder );

        var result = await sut.Connector.ConnectAsync();

        result.State.TestEquals( ConnectionState.Open ).Go();
    }

    [Fact]
    public async Task Connector_ConnectAsync_WithOptions_ShouldCallConnectImplementation()
    {
        var dbBuilder = SqlDatabaseBuilderMock.Create();
        ISqlDatabase sut = SqlDatabaseMock.Create( dbBuilder );

        var result = await sut.Connector.ConnectAsync( "test=true" );

        Assertion.All(
                result.State.TestEquals( ConnectionState.Open ),
                result.ConnectionString.TestEquals( "test=true" ) )
            .Go();
    }

    [Fact]
    public void GetRegisteredVersions_ShouldReturnEmptyArray_WhenVersionRecordsQueryReturnsEmptyResult()
    {
        var dbBuilder = SqlDatabaseBuilderMock.Create();
        var versionRecordsProvider = Lambda.Of( Enumerable.Empty<SqlDatabaseVersionRecord> );
        ISqlDatabase sut = SqlDatabaseMock.Create( dbBuilder, versionRecordsProvider: versionRecordsProvider );

        var result = sut.GetRegisteredVersions();

        result.TestEmpty().Go();
    }

    [Fact]
    public void GetRegisteredVersions_ShouldReturnCorrectResult_WhenVersionRecordsQueryReturnsNonEmptyResult()
    {
        var expected = new[]
        {
            new SqlDatabaseVersionRecord( 1, new Version( "0.1" ), "1st version", DateTime.UnixEpoch, TimeSpan.FromSeconds( 1 ) ),
            new SqlDatabaseVersionRecord( 1, new Version( "0.2" ), "2nd version", DateTime.UtcNow, TimeSpan.FromSeconds( 2 ) )
        };

        var dbBuilder = SqlDatabaseBuilderMock.Create();
        var versionRecordsProvider = Lambda.Of( () => expected );
        ISqlDatabase sut = SqlDatabaseMock.Create( dbBuilder, versionRecordsProvider: versionRecordsProvider );

        var result = sut.GetRegisteredVersions();

        result.TestSequence( expected ).Go();
    }

    [Theory]
    [InlineData( "common", true )]
    [InlineData( "foo", true )]
    [InlineData( "bar", false )]
    public void Schemas_Contains_ShouldReturnTrue_WhenSchemaExists(string name, bool expected)
    {
        var dbBuilder = SqlDatabaseBuilderMock.Create();
        dbBuilder.Schemas.Create( "foo" );
        var db = SqlDatabaseMock.Create( dbBuilder );
        ISqlSchemaCollection sut = db.Schemas;

        var result = sut.Contains( name );

        result.TestEquals( expected ).Go();
    }

    [Fact]
    public void Schemas_Get_ShouldReturnExistingSchema()
    {
        var dbBuilder = SqlDatabaseBuilderMock.Create();
        dbBuilder.Schemas.Default.SetName( "foo" );
        var db = SqlDatabaseMock.Create( dbBuilder );
        ISqlSchemaCollection sut = db.Schemas;

        var result = sut.Get( "foo" );

        result.TestRefEquals( sut.Default ).Go();
    }

    [Fact]
    public void Schemas_Get_ShouldThrowKeyNotFoundException_WhenSchemaDoesNotExist()
    {
        var dbBuilder = SqlDatabaseBuilderMock.Create();
        dbBuilder.Schemas.Default.SetName( "foo" );
        var db = SqlDatabaseMock.Create( dbBuilder );
        ISqlSchemaCollection sut = db.Schemas;

        var action = Lambda.Of( () => sut.Get( "bar" ) );

        action.Test( exc => exc.TestType().Exact<KeyNotFoundException>() ).Go();
    }

    [Fact]
    public void Schemas_TryGet_ShouldReturnExistingSchema()
    {
        var dbBuilder = SqlDatabaseBuilderMock.Create();
        dbBuilder.Schemas.Default.SetName( "foo" );
        var db = SqlDatabaseMock.Create( dbBuilder );
        ISqlSchemaCollection sut = db.Schemas;

        var result = sut.TryGet( "foo" );

        result.TestRefEquals( sut.Default ).Go();
    }

    [Fact]
    public void Schemas_TryGet_ShouldReturnNull_WhenSchemaDoesNotExist()
    {
        var dbBuilder = SqlDatabaseBuilderMock.Create();
        dbBuilder.Schemas.Default.SetName( "foo" );
        var db = SqlDatabaseMock.Create( dbBuilder );
        ISqlSchemaCollection sut = db.Schemas;

        var result = sut.TryGet( "bar" );

        result.TestNull().Go();
    }

    [Fact]
    public void Schemas_GetEnumerator_ShouldReturnCorrectResult()
    {
        var dbBuilder = SqlDatabaseBuilderMock.Create();
        dbBuilder.Schemas.Create( "foo" );
        var sut = SqlDatabaseMock.Create( dbBuilder ).Schemas;
        var schema = sut.Get( "foo" );

        var result = new List<SqlSchemaMock>();
        foreach ( var s in sut ) result.Add( s );

        Assertion.All(
                result.Count.TestEquals( 2 ),
                result.TestSetEqual( [ sut.Default, schema ] ) )
            .Go();
    }

    [Fact]
    public void Creation_ShouldHandleCorrectUnknownObject()
    {
        var dbBuilder = SqlDatabaseBuilderMock.Create();
        var table = dbBuilder.Schemas.Default.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C" ).Asc() );
        table.Constraints.CreateUnknown( "UNK", useDefaultImplementation: false, deferCreation: false );
        ISqlDatabase sut = SqlDatabaseMock.Create( dbBuilder );

        var obj = sut.Schemas.Default.Objects.GetTable( "T" ).Constraints.Get( "UNK" );

        Assertion.All(
                obj.Name.TestEquals( "UNK" ),
                obj.Type.TestEquals( SqlObjectType.Unknown ),
                sut.Schemas.Default.Objects.TryGet( "UNK" ).TestRefEquals( obj ) )
            .Go();
    }

    [Fact]
    public void Creation_ShouldHandleCorrectUnknownObject_WithDeferredCreation()
    {
        var dbBuilder = SqlDatabaseBuilderMock.Create();
        var table = dbBuilder.Schemas.Default.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C" ).Asc() );
        table.Constraints.CreateUnknown( "UNK", useDefaultImplementation: false, deferCreation: true );
        ISqlDatabase sut = SqlDatabaseMock.Create( dbBuilder );

        var obj = sut.Schemas.Default.Objects.GetTable( "T" ).Constraints.Get( "UNK" );

        Assertion.All(
                obj.Name.TestEquals( "UNK" ),
                obj.Type.TestEquals( SqlObjectType.Unknown ),
                sut.Schemas.Default.Objects.TryGet( "UNK" ).TestRefEquals( obj ) )
            .Go();
    }

    [Fact]
    public void Creation_ShouldThrowNotSupportedException_WhenIncorrectUnknownObjectExists()
    {
        var dbBuilder = SqlDatabaseBuilderMock.Create();
        var table = dbBuilder.Schemas.Default.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C" ).Asc() );
        table.Constraints.CreateUnknown( "UNK", useDefaultImplementation: true, deferCreation: false );

        var action = Lambda.Of( () => SqlDatabaseMock.Create( dbBuilder ) );

        action.Test( exc => exc.TestType().Exact<NotSupportedException>() ).Go();
    }

    [Fact]
    public void Creation_ShouldThrowNotSupportedException_WhenIncorrectUnknownObjectExists_WithDeferredCreation()
    {
        var dbBuilder = SqlDatabaseBuilderMock.Create();
        var table = dbBuilder.Schemas.Default.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C" ).Asc() );
        table.Constraints.CreateUnknown( "UNK", useDefaultImplementation: true, deferCreation: true );

        var action = Lambda.Of( () => SqlDatabaseMock.Create( dbBuilder ) );

        action.Test( exc => exc.TestType().Exact<NotSupportedException>() ).Go();
    }
}
