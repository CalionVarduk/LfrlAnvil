using System.Collections.Generic;
using System.Data.Common;
using LfrlAnvil.Functional;
using LfrlAnvil.MySql.Objects;
using LfrlAnvil.MySql.Tests.Helpers;
using LfrlAnvil.Sql;
using LfrlAnvil.TestExtensions.FluentAssertions;
using MySqlConnector;

namespace LfrlAnvil.MySql.Tests;

public class MySqlDatabaseTests : TestsBase
{
    [Fact]
    public void Properties_ShouldBeCorrectlyCopiedFromBuilder()
    {
        var dbBuilder = MySqlDatabaseBuilderMock.Create();
        var sut = MySqlDatabaseMock.Create( dbBuilder );

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
            sut.Schemas.Database.Should().BeSameAs( sut );
            sut.Schemas.Count.Should().Be( 1 );
            sut.Schemas.Default.Name.Should().Be( "common" );
            sut.Schemas.Should().BeSequentiallyEqualTo( sut.Schemas.Default );
            ((ISqlDatabaseConnector<MySqlConnection>)sut.Connector).Database.Should().BeSameAs( sut );
            ((ISqlDatabaseConnector<DbConnection>)sut.Connector).Database.Should().BeSameAs( sut );
            ((ISqlDatabaseConnector)sut.Connector).Database.Should().BeSameAs( sut );
        }
    }

    [Theory]
    [InlineData( "common", true )]
    [InlineData( "foo", true )]
    [InlineData( "bar", false )]
    public void Schemas_Contains_ShouldReturnTrue_WhenSchemaExists(string name, bool expected)
    {
        var dbBuilder = MySqlDatabaseBuilderMock.Create();
        dbBuilder.Schemas.Create( "foo" );
        var db = MySqlDatabaseMock.Create( dbBuilder );
        var sut = db.Schemas;

        var result = sut.Contains( name );

        result.Should().Be( expected );
    }

    [Fact]
    public void Schemas_Get_ShouldReturnExistingSchema()
    {
        var dbBuilder = MySqlDatabaseBuilderMock.Create();
        dbBuilder.Schemas.Default.SetName( "foo" );
        var db = MySqlDatabaseMock.Create( dbBuilder );
        var sut = db.Schemas;

        var result = sut.Get( "foo" );

        result.Should().BeSameAs( sut.Default );
    }

    [Fact]
    public void Schemas_Get_ShouldThrowKeyNotFoundException_WhenSchemaDoesNotExist()
    {
        var dbBuilder = MySqlDatabaseBuilderMock.Create();
        dbBuilder.Schemas.Default.SetName( "foo" );
        var db = MySqlDatabaseMock.Create( dbBuilder );
        var sut = db.Schemas;

        var action = Lambda.Of( () => sut.Get( "bar" ) );

        action.Should().ThrowExactly<KeyNotFoundException>();
    }

    [Fact]
    public void Schemas_TryGet_ShouldReturnExistingSchema()
    {
        var dbBuilder = MySqlDatabaseBuilderMock.Create();
        dbBuilder.Schemas.Default.SetName( "foo" );
        var db = MySqlDatabaseMock.Create( dbBuilder );
        var sut = db.Schemas;

        var result = sut.TryGet( "foo" );

        result.Should().BeSameAs( sut.Default );
    }

    [Fact]
    public void Schemas_TryGet_ShouldReturnNull_WhenSchemaDoesNotExist()
    {
        var dbBuilder = MySqlDatabaseBuilderMock.Create();
        dbBuilder.Schemas.Default.SetName( "foo" );
        var db = MySqlDatabaseMock.Create( dbBuilder );
        var sut = db.Schemas;

        var result = sut.TryGet( "bar" );

        result.Should().BeNull();
    }

    [Fact]
    public void Schemas_GetEnumerator_ShouldReturnCorrectResult()
    {
        var dbBuilder = MySqlDatabaseBuilderMock.Create();
        dbBuilder.Schemas.Create( "foo" );
        var sut = MySqlDatabaseMock.Create( dbBuilder ).Schemas;
        var schema = sut.Get( "foo" );

        var result = new List<MySqlSchema>();
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
        var dbBuilder = MySqlDatabaseBuilderMock.Create();
        var sut = MySqlDatabaseMock.Create( dbBuilder );

        var action = Lambda.Of( () => sut.Dispose() );

        action.Should().NotThrow();
    }
}
