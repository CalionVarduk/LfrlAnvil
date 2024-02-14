using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using LfrlAnvil.Functional;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.Sql.Versioning;
using LfrlAnvil.Sqlite.Tests.Helpers;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.Sqlite.Tests;

public class SqliteDatabaseTests : TestsBase
{
    [Fact]
    public void Properties_ShouldBeCorrectlyCopiedFromBuilder()
    {
        var dbBuilder = SqliteDatabaseBuilderMock.Create();
        ISqlDatabase sut = new SqliteDatabaseMock( dbBuilder );

        using ( new AssertionScope() )
        {
            sut.DataTypes.Should().BeSameAs( dbBuilder.DataTypes );
            sut.TypeDefinitions.Should().BeSameAs( dbBuilder.TypeDefinitions );
            sut.NodeInterpreters.Should().BeSameAs( dbBuilder.NodeInterpreters );
            sut.QueryReaders.Should().BeSameAs( dbBuilder.QueryReaders );
            sut.ParameterBinders.Should().BeSameAs( dbBuilder.ParameterBinders );
            sut.Schemas.Database.Should().BeSameAs( sut );
            sut.Schemas.Count.Should().Be( 1 );
            sut.Schemas.Default.Name.Should().BeEmpty();
            sut.Schemas.Should().BeSequentiallyEqualTo( sut.Schemas.Default );
        }
    }

    [Theory]
    [InlineData( "", true )]
    [InlineData( "foo", true )]
    [InlineData( "bar", false )]
    public void Schemas_Contains_ShouldReturnTrue_WhenSchemaExists(string name, bool expected)
    {
        var dbBuilder = SqliteDatabaseBuilderMock.Create();
        dbBuilder.Schemas.Create( "foo" );
        var db = new SqliteDatabaseMock( dbBuilder );
        ISqlSchemaCollection sut = db.Schemas;

        var result = sut.Contains( name );

        result.Should().Be( expected );
    }

    [Fact]
    public void Schemas_GetSchema_ShouldReturnExistingSchema()
    {
        var dbBuilder = SqliteDatabaseBuilderMock.Create();
        dbBuilder.Schemas.Default.SetName( "foo" );
        var db = new SqliteDatabaseMock( dbBuilder );
        ISqlSchemaCollection sut = db.Schemas;

        var result = sut.Get( "foo" );

        result.Should().BeSameAs( sut.Default );
    }

    [Fact]
    public void Schemas_GetSchema_ShouldThrowKeyNotFoundException_WhenSchemaDoesNotExist()
    {
        var dbBuilder = SqliteDatabaseBuilderMock.Create();
        dbBuilder.Schemas.Default.SetName( "foo" );
        var db = new SqliteDatabaseMock( dbBuilder );
        ISqlSchemaCollection sut = db.Schemas;

        var action = Lambda.Of( () => sut.Get( "bar" ) );

        action.Should().ThrowExactly<KeyNotFoundException>();
    }

    [Fact]
    public void Schemas_TryGetSchema_ShouldReturnExistingSchema()
    {
        var dbBuilder = SqliteDatabaseBuilderMock.Create();
        dbBuilder.Schemas.Default.SetName( "foo" );
        var db = new SqliteDatabaseMock( dbBuilder );
        ISqlSchemaCollection sut = db.Schemas;

        var result = sut.TryGet( "foo" );

        result.Should().BeSameAs( sut.Default );
    }

    [Fact]
    public void Schemas_TryGetSchema_ShouldReturnNull_WhenSchemaDoesNotExist()
    {
        var dbBuilder = SqliteDatabaseBuilderMock.Create();
        dbBuilder.Schemas.Default.SetName( "foo" );
        var db = new SqliteDatabaseMock( dbBuilder );
        ISqlSchemaCollection sut = db.Schemas;

        var result = sut.TryGet( "bar" );

        result.Should().BeNull();
    }

    [Fact]
    public void Connect_ForInMemoryDatabase_ShouldAlwaysReturnTheSameObject()
    {
        var factory = new SqliteDatabaseFactory();
        ISqlDatabase sut = factory.Create( "DataSource=:memory:", new SqlDatabaseVersionHistory() ).Database;

        var first = sut.Connect();
        var second = sut.Connect();

        first.Should().BeSameAs( second );
    }

    [Fact]
    public async Task ConnectAsync_ForInMemoryDatabase_ShouldAlwaysReturnTheSameObject()
    {
        var factory = new SqliteDatabaseFactory();
        ISqlDatabase sut = factory.Create( "DataSource=:memory:", new SqlDatabaseVersionHistory() ).Database;

        var first = await sut.ConnectAsync();
        var second = await sut.ConnectAsync();

        first.Should().BeSameAs( second );
    }

    [Fact]
    public void Dispose_ForInMemoryDatabase_ShouldCloseConnection()
    {
        var factory = new SqliteDatabaseFactory();
        var sut = factory.Create( "DataSource=:memory:", new SqlDatabaseVersionHistory() ).Database;
        var connection = sut.Connect();

        sut.Dispose();

        connection.State.Should().Be( ConnectionState.Closed );
    }

    [Fact]
    public void Connection_Open_ForInMemoryDatabase_ShouldThrowInvalidOperationException_WhenConnectionHasBeenClosed()
    {
        var factory = new SqliteDatabaseFactory();
        var sut = factory.Create( "DataSource=:memory:", new SqlDatabaseVersionHistory() ).Database;
        var connection = sut.Connect();
        sut.Dispose();

        var action = Lambda.Of( () => connection.Open() );

        action.Should().ThrowExactly<InvalidOperationException>();
    }

    [Fact]
    public void Connection_ConnectionString_Setter_ForInMemoryDatabase_ShouldThrowInvalidOperationException()
    {
        var factory = new SqliteDatabaseFactory();
        var sut = factory.Create( "DataSource=:memory:", new SqlDatabaseVersionHistory() ).Database;
        var connection = sut.Connect();

        var action = Lambda.Of( () => connection.ConnectionString = connection.ConnectionString );

        action.Should().ThrowExactly<InvalidOperationException>();
    }
}
