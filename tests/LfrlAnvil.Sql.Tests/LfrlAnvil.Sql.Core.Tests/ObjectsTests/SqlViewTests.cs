﻿using System.Collections.Generic;
using System.Linq;
using LfrlAnvil.Functional;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.Sql.Tests.Helpers;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.Sql.Tests.ObjectsTests;

public class SqlViewTests : TestsBase
{
    [Fact]
    public void Properties_ShouldBeCorrectlyCopiedFromBuilder()
    {
        var schemaBuilder = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var viewBuilder = schemaBuilder.Objects.CreateView(
            "V",
            SqlNode.RawRecordSet( "bar" )
                .ToDataSource()
                .Select( s => new[] { s.From["a"].AsSelf(), s.From["b"].As( "x" ), s.From["c"].AsSelf() } ) );

        var db = SqlDatabaseMock.Create( schemaBuilder.Database );
        var schema = db.Schemas.Get( "foo" );

        ISqlView sut = schema.Objects.GetView( "V" );
        var a = sut.DataFields.Get( "a" );
        var x = sut.DataFields.Get( "x" );
        var c = sut.DataFields.Get( "c" );

        using ( new AssertionScope() )
        {
            sut.Database.Should().BeSameAs( db );
            sut.Schema.Should().BeSameAs( schema );
            sut.Type.Should().Be( SqlObjectType.View );
            sut.Name.Should().Be( "V" );
            sut.Info.Should().Be( viewBuilder.Info );
            sut.Node.View.Should().BeSameAs( sut );
            sut.Node.Info.Should().Be( sut.Info );
            sut.Node.Alias.Should().BeNull();
            sut.Node.Identifier.Should().Be( sut.Info.Identifier );
            sut.Node.IsOptional.Should().BeFalse();
            sut.ToString().Should().Be( "[View] foo.V" );

            sut.DataFields.Count.Should().Be( 3 );
            sut.DataFields.View.Should().BeSameAs( sut );
            sut.DataFields.Should().BeSequentiallyEqualTo( a, x, c );
        }
    }

    [Theory]
    [InlineData( "F1", true )]
    [InlineData( "F2", true )]
    [InlineData( "F3", false )]
    public void DataFields_Contains_ShouldReturnTrue_WhenFieldExists(string name, bool expected)
    {
        var schemaBuilder = SqlDatabaseBuilderMock.Create().Schemas.Default;
        schemaBuilder.Objects.CreateView(
            "V",
            SqlNode.RawRecordSet( "foo" ).ToDataSource().Select( s => new[] { s.From["F1"].AsSelf(), s.From["F2"].AsSelf() } ) );

        var db = SqlDatabaseMock.Create( schemaBuilder.Database );
        ISqlViewDataFieldCollection sut = db.Schemas.Default.Objects.GetView( "V" ).DataFields;

        var result = sut.Contains( name );

        result.Should().Be( expected );
    }

    [Fact]
    public void DataFields_Get_ShouldReturnCorrectField()
    {
        var schemaBuilder = SqlDatabaseBuilderMock.Create().Schemas.Default;
        schemaBuilder.Objects.CreateView(
            "V",
            SqlNode.RawRecordSet( "foo" ).ToDataSource().Select( s => new[] { s.From["F1"].AsSelf(), s.From["F2"].AsSelf() } ) );

        var db = SqlDatabaseMock.Create( schemaBuilder.Database );
        ISqlViewDataFieldCollection sut = db.Schemas.Default.Objects.GetView( "V" ).DataFields;

        var result = sut.Get( "F2" );

        result.Should().BeSameAs( sut.First( f => f.Name == "F2" ) );
    }

    [Fact]
    public void DataFields_Get_ShouldThrowKeyNotFoundException_WhenFieldDoesNotExist()
    {
        var schemaBuilder = SqlDatabaseBuilderMock.Create().Schemas.Default;
        schemaBuilder.Objects.CreateView(
            "V",
            SqlNode.RawRecordSet( "foo" ).ToDataSource().Select( s => new[] { s.From["F1"].AsSelf() } ) );

        var db = SqlDatabaseMock.Create( schemaBuilder.Database );
        ISqlViewDataFieldCollection sut = db.Schemas.Default.Objects.GetView( "V" ).DataFields;

        var action = Lambda.Of( () => sut.Get( "F2" ) );

        action.Should().ThrowExactly<KeyNotFoundException>();
    }

    [Fact]
    public void DataFields_TryGet_ShouldReturnCorrectField()
    {
        var schemaBuilder = SqlDatabaseBuilderMock.Create().Schemas.Default;
        schemaBuilder.Objects.CreateView(
            "V",
            SqlNode.RawRecordSet( "foo" ).ToDataSource().Select( s => new[] { s.From["F1"].AsSelf(), s.From["F2"].AsSelf() } ) );

        var db = SqlDatabaseMock.Create( schemaBuilder.Database );
        ISqlViewDataFieldCollection sut = db.Schemas.Default.Objects.GetView( "V" ).DataFields;

        var result = sut.TryGet( "F2" );

        result.Should().BeSameAs( sut.First( f => f.Name == "F2" ) );
    }

    [Fact]
    public void DataFields_TryGet_ShouldReturnNull_WhenFieldDoesNotExist()
    {
        var schemaBuilder = SqlDatabaseBuilderMock.Create().Schemas.Default;
        schemaBuilder.Objects.CreateView(
            "V",
            SqlNode.RawRecordSet( "foo" ).ToDataSource().Select( s => new[] { s.From["F1"].AsSelf() } ) );

        var db = SqlDatabaseMock.Create( schemaBuilder.Database );
        ISqlViewDataFieldCollection sut = db.Schemas.Default.Objects.GetView( "V" ).DataFields;

        var result = sut.TryGet( "F2" );

        result.Should().BeNull();
    }
}
