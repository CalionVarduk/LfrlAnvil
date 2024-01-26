using System.Collections.Generic;
using LfrlAnvil.Functional;
using LfrlAnvil.MySql.Tests.Helpers;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.MySql.Tests;

public class MySqlDatabaseTests : TestsBase
{
    [Fact]
    public void Properties_ShouldBeCorrectlyCopiedFromBuilder()
    {
        var dbBuilder = MySqlDatabaseBuilderMock.Create();
        ISqlDatabase sut = MySqlDatabaseMock.Create( dbBuilder );

        using ( new AssertionScope() )
        {
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

    [Theory]
    [InlineData( "common", true )]
    [InlineData( "foo", true )]
    [InlineData( "bar", false )]
    public void Schemas_Contains_ShouldReturnTrue_WhenSchemaExists(string name, bool expected)
    {
        var dbBuilder = MySqlDatabaseBuilderMock.Create();
        dbBuilder.Schemas.Create( "foo" );
        var db = MySqlDatabaseMock.Create( dbBuilder );
        ISqlSchemaCollection sut = db.Schemas;

        var result = sut.Contains( name );

        result.Should().Be( expected );
    }

    [Fact]
    public void Schemas_GetSchema_ShouldReturnExistingSchema()
    {
        var dbBuilder = MySqlDatabaseBuilderMock.Create();
        dbBuilder.Schemas.Default.SetName( "foo" );
        var db = MySqlDatabaseMock.Create( dbBuilder );
        ISqlSchemaCollection sut = db.Schemas;

        var result = sut.GetSchema( "foo" );

        result.Should().BeSameAs( sut.Default );
    }

    [Fact]
    public void Schemas_GetSchema_ShouldThrowKeyNotFoundException_WhenSchemaDoesNotExist()
    {
        var dbBuilder = MySqlDatabaseBuilderMock.Create();
        dbBuilder.Schemas.Default.SetName( "foo" );
        var db = MySqlDatabaseMock.Create( dbBuilder );
        ISqlSchemaCollection sut = db.Schemas;

        var action = Lambda.Of( () => sut.GetSchema( "bar" ) );

        action.Should().ThrowExactly<KeyNotFoundException>();
    }

    [Fact]
    public void Schemas_TryGetSchema_ShouldReturnExistingSchema()
    {
        var dbBuilder = MySqlDatabaseBuilderMock.Create();
        dbBuilder.Schemas.Default.SetName( "foo" );
        var db = MySqlDatabaseMock.Create( dbBuilder );
        ISqlSchemaCollection sut = db.Schemas;

        var result = sut.TryGetSchema( "foo" );

        result.Should().BeSameAs( sut.Default );
    }

    [Fact]
    public void Schemas_TryGetSchema_ShouldReturnNull_WhenSchemaDoesNotExist()
    {
        var dbBuilder = MySqlDatabaseBuilderMock.Create();
        dbBuilder.Schemas.Default.SetName( "foo" );
        var db = MySqlDatabaseMock.Create( dbBuilder );
        ISqlSchemaCollection sut = db.Schemas;

        var result = sut.TryGetSchema( "bar" );

        result.Should().BeNull();
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
