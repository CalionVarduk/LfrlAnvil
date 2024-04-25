using System.Collections.Generic;
using System.Data.Common;
using LfrlAnvil.Functional;
using LfrlAnvil.PostgreSql.Objects;
using LfrlAnvil.PostgreSql.Tests.Helpers;
using LfrlAnvil.Sql;
using LfrlAnvil.TestExtensions.FluentAssertions;
using Npgsql;

namespace LfrlAnvil.PostgreSql.Tests;

public class PostgreSqlDatabaseTests : TestsBase
{
    [Fact]
    public void Properties_ShouldBeCorrectlyCopiedFromBuilder()
    {
        var dbBuilder = PostgreSqlDatabaseBuilderMock.Create();
        var sut = PostgreSqlDatabaseMock.Create( dbBuilder );

        using ( new AssertionScope() )
        {
            sut.Dialect.Should().BeSameAs( dbBuilder.Dialect );
            sut.Version.Should().Be( new Version( "0.0" ) );
            sut.ServerVersion.Should().BeSameAs( dbBuilder.ServerVersion );
            sut.DataTypes.Should().BeSameAs( dbBuilder.DataTypes );
            sut.TypeDefinitions.Should().BeSameAs( dbBuilder.TypeDefinitions );
            sut.NodeInterpreters.Should().BeSameAs( dbBuilder.NodeInterpreters );
            sut.QueryReaders.Should().BeSameAs( dbBuilder.QueryReaders );
            sut.ParameterBinders.Should().BeSameAs( dbBuilder.ParameterBinders );
            sut.ParameterBinders.SupportsPositionalParameters.Should().BeTrue();
            sut.Schemas.Database.Should().BeSameAs( sut );
            sut.Schemas.Count.Should().Be( 1 );
            sut.Schemas.Default.Name.Should().Be( "public" );
            sut.Schemas.Should().BeSequentiallyEqualTo( sut.Schemas.Default );
            (( ISqlDatabaseConnector<NpgsqlConnection> )sut.Connector).Database.Should().BeSameAs( sut );
            (( ISqlDatabaseConnector<DbConnection> )sut.Connector).Database.Should().BeSameAs( sut );
            (( ISqlDatabaseConnector )sut.Connector).Database.Should().BeSameAs( sut );
        }
    }

    [Theory]
    [InlineData( "public", true )]
    [InlineData( "foo", true )]
    [InlineData( "bar", false )]
    public void Schemas_Contains_ShouldReturnTrue_WhenSchemaExists(string name, bool expected)
    {
        var dbBuilder = PostgreSqlDatabaseBuilderMock.Create();
        dbBuilder.Schemas.Create( "foo" );
        var db = PostgreSqlDatabaseMock.Create( dbBuilder );
        var sut = db.Schemas;

        var result = sut.Contains( name );

        result.Should().Be( expected );
    }

    [Fact]
    public void Schemas_Get_ShouldReturnExistingSchema()
    {
        var dbBuilder = PostgreSqlDatabaseBuilderMock.Create();
        dbBuilder.Schemas.Default.SetName( "foo" );
        var db = PostgreSqlDatabaseMock.Create( dbBuilder );
        var sut = db.Schemas;

        var result = sut.Get( "foo" );

        result.Should().BeSameAs( sut.Default );
    }

    [Fact]
    public void Schemas_Get_ShouldThrowKeyNotFoundException_WhenSchemaDoesNotExist()
    {
        var dbBuilder = PostgreSqlDatabaseBuilderMock.Create();
        dbBuilder.Schemas.Default.SetName( "foo" );
        var db = PostgreSqlDatabaseMock.Create( dbBuilder );
        var sut = db.Schemas;

        var action = Lambda.Of( () => sut.Get( "bar" ) );

        action.Should().ThrowExactly<KeyNotFoundException>();
    }

    [Fact]
    public void Schemas_TryGet_ShouldReturnExistingSchema()
    {
        var dbBuilder = PostgreSqlDatabaseBuilderMock.Create();
        dbBuilder.Schemas.Default.SetName( "foo" );
        var db = PostgreSqlDatabaseMock.Create( dbBuilder );
        var sut = db.Schemas;

        var result = sut.TryGet( "foo" );

        result.Should().BeSameAs( sut.Default );
    }

    [Fact]
    public void Schemas_TryGet_ShouldReturnNull_WhenSchemaDoesNotExist()
    {
        var dbBuilder = PostgreSqlDatabaseBuilderMock.Create();
        dbBuilder.Schemas.Default.SetName( "foo" );
        var db = PostgreSqlDatabaseMock.Create( dbBuilder );
        var sut = db.Schemas;

        var result = sut.TryGet( "bar" );

        result.Should().BeNull();
    }

    [Fact]
    public void Schemas_GetEnumerator_ShouldReturnCorrectResult()
    {
        var dbBuilder = PostgreSqlDatabaseBuilderMock.Create();
        dbBuilder.Schemas.Create( "foo" );
        var sut = PostgreSqlDatabaseMock.Create( dbBuilder ).Schemas;
        var schema = sut.Get( "foo" );

        var result = new List<PostgreSqlSchema>();
        foreach ( var s in sut )
            result.Add( s );

        using ( new AssertionScope() )
        {
            result.Should().HaveCount( 2 );
            result.Should().BeEquivalentTo( sut.Default, schema );
        }
    }

    [Fact]
    public void Dispose_ShouldDoNothing()
    {
        var dbBuilder = PostgreSqlDatabaseBuilderMock.Create();
        var sut = PostgreSqlDatabaseMock.Create( dbBuilder );

        var action = Lambda.Of( () => sut.Dispose() );

        action.Should().NotThrow();
    }
}
