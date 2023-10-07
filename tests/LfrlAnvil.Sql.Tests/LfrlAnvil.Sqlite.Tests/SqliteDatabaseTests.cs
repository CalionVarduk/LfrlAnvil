﻿using System.Collections.Generic;
using System.Data;
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
            sut.NodeInterpreterFactory.Should().BeSameAs( dbBuilder.NodeInterpreterFactory );
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
    public void Schemas_Get_ShouldReturnExistingSchema()
    {
        var dbBuilder = SqliteDatabaseBuilderMock.Create();
        dbBuilder.Schemas.Default.SetName( "foo" );
        var db = new SqliteDatabaseMock( dbBuilder );
        ISqlSchemaCollection sut = db.Schemas;

        var result = sut.Get( "foo" );

        result.Should().BeSameAs( sut.Default );
    }

    [Fact]
    public void Schemas_Get_ShouldThrowKeyNotFoundException_WhenSchemaDoesNotExist()
    {
        var dbBuilder = SqliteDatabaseBuilderMock.Create();
        dbBuilder.Schemas.Default.SetName( "foo" );
        var db = new SqliteDatabaseMock( dbBuilder );
        ISqlSchemaCollection sut = db.Schemas;

        var action = Lambda.Of( () => sut.Get( "bar" ) );

        action.Should().ThrowExactly<KeyNotFoundException>();
    }

    [Fact]
    public void Schemas_TryGet_ShouldReturnExistingSchema()
    {
        var dbBuilder = SqliteDatabaseBuilderMock.Create();
        dbBuilder.Schemas.Default.SetName( "foo" );
        var db = new SqliteDatabaseMock( dbBuilder );
        ISqlSchemaCollection sut = db.Schemas;

        var result = sut.TryGet( "foo", out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            outResult.Should().BeSameAs( sut.Default );
        }
    }

    [Fact]
    public void Schemas_TryGet_ShouldReturnFalse_WhenSchemaDoesNotExist()
    {
        var dbBuilder = SqliteDatabaseBuilderMock.Create();
        dbBuilder.Schemas.Default.SetName( "foo" );
        var db = new SqliteDatabaseMock( dbBuilder );
        ISqlSchemaCollection sut = db.Schemas;

        var result = sut.TryGet( "bar", out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().BeFalse();
            outResult.Should().BeNull();
        }
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
