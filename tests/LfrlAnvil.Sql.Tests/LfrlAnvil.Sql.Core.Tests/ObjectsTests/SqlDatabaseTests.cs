using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using LfrlAnvil.Functional;
using LfrlAnvil.Sql.Extensions;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.Sql.Tests.Helpers;
using LfrlAnvil.Sql.Versioning;
using LfrlAnvil.TestExtensions.FluentAssertions;
using LfrlAnvil.TestExtensions.Sql.Mocks;

namespace LfrlAnvil.Sql.Tests.ObjectsTests;

public class SqlDatabaseTests : TestsBase
{
    [Fact]
    public void Properties_ShouldBeCorrectlyCopiedFromBuilder()
    {
        var dbBuilder = SqlDatabaseBuilderMockFactory.Create();
        ISqlDatabase sut = SqlDatabaseMock.Create( dbBuilder );

        using ( new AssertionScope() )
        {
            sut.Dialect.Should().BeSameAs( dbBuilder.Dialect );
            sut.Version.Should().Be( new Version( "0.0.0" ) );
            sut.ServerVersion.Should().BeSameAs( dbBuilder.ServerVersion );
            sut.DataTypes.Should().BeSameAs( dbBuilder.DataTypes );
            sut.TypeDefinitions.Should().BeSameAs( dbBuilder.TypeDefinitions );
            sut.NodeInterpreters.Should().BeSameAs( dbBuilder.NodeInterpreters );
            sut.QueryReaders.Should().BeSameAs( dbBuilder.QueryReaders );
            sut.ParameterBinders.Should().BeSameAs( dbBuilder.ParameterBinders );
            sut.Schemas.Database.Should().BeSameAs( sut );
            sut.Schemas.Count.Should().Be( 1 );
            sut.Schemas.Default.Name.Should().Be( "common" );
            sut.Schemas.Should().BeSequentiallyEqualTo( sut.Schemas.Default );
        }
    }

    [Fact]
    public void Dispose_ShouldNotThrow()
    {
        var dbBuilder = SqlDatabaseBuilderMockFactory.Create();
        ISqlDatabase sut = SqlDatabaseMock.Create( dbBuilder );

        var action = Lambda.Of( () => sut.Dispose() );

        action.Should().NotThrow();
    }

    [Fact]
    public void Connect_ShouldCallConnectImplementation()
    {
        var dbBuilder = SqlDatabaseBuilderMockFactory.Create();
        ISqlDatabase sut = SqlDatabaseMock.Create( dbBuilder );

        var result = sut.Connect();

        result.State.Should().Be( ConnectionState.Open );
    }

    [Fact]
    public async Task ConnectAsync_ShouldCallConnectImplementation()
    {
        var dbBuilder = SqlDatabaseBuilderMockFactory.Create();
        ISqlDatabase sut = SqlDatabaseMock.Create( dbBuilder );

        var result = await sut.ConnectAsync();

        result.State.Should().Be( ConnectionState.Open );
    }

    [Fact]
    public void GetRegisteredVersions_ShouldReturnEmptyArray_WhenVersionRecordsQueryReturnsEmptyResult()
    {
        var dbBuilder = SqlDatabaseBuilderMockFactory.Create();
        var versionRecordsProvider = Lambda.Of( Enumerable.Empty<SqlDatabaseVersionRecord> );
        ISqlDatabase sut = new SqlDatabaseMock( dbBuilder, versionRecordsProvider: versionRecordsProvider );

        var result = sut.GetRegisteredVersions();

        result.Should().BeEmpty();
    }

    [Fact]
    public void GetRegisteredVersions_ShouldReturnCorrectResult_WhenVersionRecordsQueryReturnsNonEmptyResult()
    {
        var expected = new[]
        {
            new SqlDatabaseVersionRecord( 1, new Version( "0.1" ), "1st version", DateTime.UnixEpoch, TimeSpan.FromSeconds( 1 ) ),
            new SqlDatabaseVersionRecord( 1, new Version( "0.2" ), "2nd version", DateTime.UtcNow, TimeSpan.FromSeconds( 2 ) )
        };

        var dbBuilder = SqlDatabaseBuilderMockFactory.Create();
        var versionRecordsProvider = Lambda.Of( () => expected );
        ISqlDatabase sut = new SqlDatabaseMock( dbBuilder, versionRecordsProvider: versionRecordsProvider );

        var result = sut.GetRegisteredVersions();

        result.Should().BeSequentiallyEqualTo( expected );
    }

    [Theory]
    [InlineData( "common", true )]
    [InlineData( "foo", true )]
    [InlineData( "bar", false )]
    public void Schemas_Contains_ShouldReturnTrue_WhenSchemaExists(string name, bool expected)
    {
        var dbBuilder = SqlDatabaseBuilderMockFactory.Create();
        dbBuilder.Schemas.Create( "foo" );
        var db = SqlDatabaseMock.Create( dbBuilder );
        ISqlSchemaCollection sut = db.Schemas;

        var result = sut.Contains( name );

        result.Should().Be( expected );
    }

    [Fact]
    public void Schemas_Get_ShouldReturnExistingSchema()
    {
        var dbBuilder = SqlDatabaseBuilderMockFactory.Create();
        dbBuilder.Schemas.Default.SetName( "foo" );
        var db = SqlDatabaseMock.Create( dbBuilder );
        ISqlSchemaCollection sut = db.Schemas;

        var result = sut.Get( "foo" );

        result.Should().BeSameAs( sut.Default );
    }

    [Fact]
    public void Schemas_Get_ShouldThrowKeyNotFoundException_WhenSchemaDoesNotExist()
    {
        var dbBuilder = SqlDatabaseBuilderMockFactory.Create();
        dbBuilder.Schemas.Default.SetName( "foo" );
        var db = SqlDatabaseMock.Create( dbBuilder );
        ISqlSchemaCollection sut = db.Schemas;

        var action = Lambda.Of( () => sut.Get( "bar" ) );

        action.Should().ThrowExactly<KeyNotFoundException>();
    }

    [Fact]
    public void Schemas_TryGet_ShouldReturnExistingSchema()
    {
        var dbBuilder = SqlDatabaseBuilderMockFactory.Create();
        dbBuilder.Schemas.Default.SetName( "foo" );
        var db = SqlDatabaseMock.Create( dbBuilder );
        ISqlSchemaCollection sut = db.Schemas;

        var result = sut.TryGet( "foo" );

        result.Should().BeSameAs( sut.Default );
    }

    [Fact]
    public void Schemas_TryGet_ShouldReturnNull_WhenSchemaDoesNotExist()
    {
        var dbBuilder = SqlDatabaseBuilderMockFactory.Create();
        dbBuilder.Schemas.Default.SetName( "foo" );
        var db = SqlDatabaseMock.Create( dbBuilder );
        ISqlSchemaCollection sut = db.Schemas;

        var result = sut.TryGet( "bar" );

        result.Should().BeNull();
    }

    [Fact]
    public void Schemas_GetEnumerator_ShouldReturnCorrectResult()
    {
        var dbBuilder = SqlDatabaseBuilderMockFactory.Create();
        dbBuilder.Schemas.Create( "foo" );
        var sut = SqlDatabaseMock.Create( dbBuilder ).Schemas;
        var schema = sut.Get( "foo" );

        var result = new List<SqlSchemaMock>();
        foreach ( var s in sut )
            result.Add( s );

        using ( new AssertionScope() )
        {
            result.Should().HaveCount( 2 );
            result.Should().BeEquivalentTo( sut.Default, schema );
        }
    }

    [Fact]
    public void Creation_ShouldHandleCorrectUnknownObject()
    {
        var dbBuilder = SqlDatabaseBuilderMockFactory.Create();
        var table = dbBuilder.Schemas.Default.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C" ).Asc() );
        table.Constraints.CreateUnknown( "UNK", useDefaultImplementation: false, deferCreation: false );
        ISqlDatabase sut = SqlDatabaseMock.Create( dbBuilder );

        var obj = sut.Schemas.Default.Objects.GetTable( "T" ).Constraints.Get( "UNK" );

        using ( new AssertionScope() )
        {
            obj.Name.Should().Be( "UNK" );
            obj.Type.Should().Be( SqlObjectType.Unknown );
            sut.Schemas.Default.Objects.TryGet( "UNK" ).Should().BeSameAs( obj );
        }
    }

    [Fact]
    public void Creation_ShouldHandleCorrectUnknownObject_WithDeferredCreation()
    {
        var dbBuilder = SqlDatabaseBuilderMockFactory.Create();
        var table = dbBuilder.Schemas.Default.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C" ).Asc() );
        table.Constraints.CreateUnknown( "UNK", useDefaultImplementation: false, deferCreation: true );
        ISqlDatabase sut = SqlDatabaseMock.Create( dbBuilder );

        var obj = sut.Schemas.Default.Objects.GetTable( "T" ).Constraints.Get( "UNK" );

        using ( new AssertionScope() )
        {
            obj.Name.Should().Be( "UNK" );
            obj.Type.Should().Be( SqlObjectType.Unknown );
            sut.Schemas.Default.Objects.TryGet( "UNK" ).Should().BeSameAs( obj );
        }
    }

    [Fact]
    public void Creation_ShouldThrowNotSupportedException_WhenIncorrectUnknownObjectExists()
    {
        var dbBuilder = SqlDatabaseBuilderMockFactory.Create();
        var table = dbBuilder.Schemas.Default.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C" ).Asc() );
        table.Constraints.CreateUnknown( "UNK", useDefaultImplementation: true, deferCreation: false );

        var action = Lambda.Of( () => SqlDatabaseMock.Create( dbBuilder ) );

        action.Should().ThrowExactly<NotSupportedException>();
    }

    [Fact]
    public void Creation_ShouldThrowNotSupportedException_WhenIncorrectUnknownObjectExists_WithDeferredCreation()
    {
        var dbBuilder = SqlDatabaseBuilderMockFactory.Create();
        var table = dbBuilder.Schemas.Default.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C" ).Asc() );
        table.Constraints.CreateUnknown( "UNK", useDefaultImplementation: true, deferCreation: true );

        var action = Lambda.Of( () => SqlDatabaseMock.Create( dbBuilder ) );

        action.Should().ThrowExactly<NotSupportedException>();
    }
}
