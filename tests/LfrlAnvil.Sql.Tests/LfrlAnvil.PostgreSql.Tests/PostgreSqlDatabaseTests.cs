using System.Collections.Generic;
using System.Data.Common;
using LfrlAnvil.Functional;
using LfrlAnvil.PostgreSql.Objects;
using LfrlAnvil.PostgreSql.Tests.Helpers;
using LfrlAnvil.Sql;
using Npgsql;

namespace LfrlAnvil.PostgreSql.Tests;

public class PostgreSqlDatabaseTests : TestsBase
{
    [Fact]
    public void Properties_ShouldBeCorrectlyCopiedFromBuilder()
    {
        var dbBuilder = PostgreSqlDatabaseBuilderMock.Create();
        var sut = PostgreSqlDatabaseMock.Create( dbBuilder );

        Assertion.All(
                sut.Dialect.TestRefEquals( dbBuilder.Dialect ),
                sut.Version.TestEquals( new Version( "0.0" ) ),
                sut.ServerVersion.TestRefEquals( dbBuilder.ServerVersion ),
                sut.DataTypes.TestRefEquals( dbBuilder.DataTypes ),
                sut.TypeDefinitions.TestRefEquals( dbBuilder.TypeDefinitions ),
                sut.NodeInterpreters.TestRefEquals( dbBuilder.NodeInterpreters ),
                sut.QueryReaders.TestRefEquals( dbBuilder.QueryReaders ),
                sut.ParameterBinders.TestRefEquals( dbBuilder.ParameterBinders ),
                sut.ParameterBinders.SupportsPositionalParameters.TestTrue(),
                sut.Schemas.Database.TestRefEquals( sut ),
                sut.Schemas.Count.TestEquals( 1 ),
                sut.Schemas.Default.Name.TestEquals( "public" ),
                sut.Schemas.TestSequence( [ sut.Schemas.Default ] ),
                (( ISqlDatabaseConnector<NpgsqlConnection> )sut.Connector).Database.TestRefEquals( sut ),
                (( ISqlDatabaseConnector<DbConnection> )sut.Connector).Database.TestRefEquals( sut ),
                (( ISqlDatabaseConnector )sut.Connector).Database.TestRefEquals( sut ) )
            .Go();
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

        result.TestEquals( expected ).Go();
    }

    [Fact]
    public void Schemas_Get_ShouldReturnExistingSchema()
    {
        var dbBuilder = PostgreSqlDatabaseBuilderMock.Create();
        dbBuilder.Schemas.Default.SetName( "foo" );
        var db = PostgreSqlDatabaseMock.Create( dbBuilder );
        var sut = db.Schemas;

        var result = sut.Get( "foo" );

        result.TestRefEquals( sut.Default ).Go();
    }

    [Fact]
    public void Schemas_Get_ShouldThrowKeyNotFoundException_WhenSchemaDoesNotExist()
    {
        var dbBuilder = PostgreSqlDatabaseBuilderMock.Create();
        dbBuilder.Schemas.Default.SetName( "foo" );
        var db = PostgreSqlDatabaseMock.Create( dbBuilder );
        var sut = db.Schemas;

        var action = Lambda.Of( () => sut.Get( "bar" ) );

        action.Test( exc => exc.TestType().Exact<KeyNotFoundException>() ).Go();
    }

    [Fact]
    public void Schemas_TryGet_ShouldReturnExistingSchema()
    {
        var dbBuilder = PostgreSqlDatabaseBuilderMock.Create();
        dbBuilder.Schemas.Default.SetName( "foo" );
        var db = PostgreSqlDatabaseMock.Create( dbBuilder );
        var sut = db.Schemas;

        var result = sut.TryGet( "foo" );

        result.TestRefEquals( sut.Default ).Go();
    }

    [Fact]
    public void Schemas_TryGet_ShouldReturnNull_WhenSchemaDoesNotExist()
    {
        var dbBuilder = PostgreSqlDatabaseBuilderMock.Create();
        dbBuilder.Schemas.Default.SetName( "foo" );
        var db = PostgreSqlDatabaseMock.Create( dbBuilder );
        var sut = db.Schemas;

        var result = sut.TryGet( "bar" );

        result.TestNull().Go();
    }

    [Fact]
    public void Schemas_GetEnumerator_ShouldReturnCorrectResult()
    {
        var dbBuilder = PostgreSqlDatabaseBuilderMock.Create();
        dbBuilder.Schemas.Create( "foo" );
        var sut = PostgreSqlDatabaseMock.Create( dbBuilder ).Schemas;
        var schema = sut.Get( "foo" );

        var result = new List<PostgreSqlSchema>();
        foreach ( var s in sut ) result.Add( s );

        Assertion.All(
                result.Count.TestEquals( 2 ),
                result.TestSetEqual( [ sut.Default, schema ] ) )
            .Go();
    }

    [Fact]
    public void Dispose_ShouldDoNothing()
    {
        var dbBuilder = PostgreSqlDatabaseBuilderMock.Create();
        var sut = PostgreSqlDatabaseMock.Create( dbBuilder );

        var action = Lambda.Of( () => sut.Dispose() );

        action.Test( exc => exc.TestNull() ).Go();
    }
}
