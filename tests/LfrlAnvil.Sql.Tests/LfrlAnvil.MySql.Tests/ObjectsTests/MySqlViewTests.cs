﻿using System.Collections.Generic;
using System.Linq;
using LfrlAnvil.Functional;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.MySql.Tests.Helpers;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.MySql.Tests.ObjectsTests;

public class MySqlViewTests : TestsBase
{
    [Fact]
    public void Properties_ShouldBeCorrectlyCopiedFromBuilder()
    {
        var schemaBuilder = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var viewBuilder = schemaBuilder.Objects.CreateView(
            "V",
            SqlNode.RawRecordSet( "bar" )
                .ToDataSource()
                .Select( s => new[] { s.From["a"].AsSelf(), s.From["b"].As( "x" ), s.From["c"].AsSelf() } ) );

        var db = MySqlDatabaseMock.Create( schemaBuilder.Database );
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
            sut.FullName.Should().Be( "foo.V" );
            sut.Info.Should().Be( viewBuilder.Info );
            sut.RecordSet.View.Should().BeSameAs( sut );
            sut.RecordSet.Info.Should().Be( sut.Info );
            sut.RecordSet.Alias.Should().BeNull();
            sut.RecordSet.Identifier.Should().Be( sut.Info.Identifier );
            sut.RecordSet.IsOptional.Should().BeFalse();
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
        var schemaBuilder = MySqlDatabaseBuilderMock.Create().Schemas.Default;
        schemaBuilder.Objects.CreateView(
            "V",
            SqlNode.RawRecordSet( "foo" ).ToDataSource().Select( s => new[] { s.From["F1"].AsSelf(), s.From["F2"].AsSelf() } ) );

        var db = MySqlDatabaseMock.Create( schemaBuilder.Database );
        ISqlViewDataFieldCollection sut = db.Schemas.Default.Objects.GetView( "V" ).DataFields;

        var result = sut.Contains( name );

        result.Should().Be( expected );
    }

    [Fact]
    public void DataFields_Get_ShouldReturnCorrectField()
    {
        var schemaBuilder = MySqlDatabaseBuilderMock.Create().Schemas.Default;
        schemaBuilder.Objects.CreateView(
            "V",
            SqlNode.RawRecordSet( "foo" ).ToDataSource().Select( s => new[] { s.From["F1"].AsSelf(), s.From["F2"].AsSelf() } ) );

        var db = MySqlDatabaseMock.Create( schemaBuilder.Database );
        ISqlViewDataFieldCollection sut = db.Schemas.Default.Objects.GetView( "V" ).DataFields;

        var result = sut.Get( "F2" );

        result.Should().BeSameAs( sut.First( f => f.Name == "F2" ) );
    }

    [Fact]
    public void DataFields_Get_ShouldThrowKeyNotFoundException_WhenFieldDoesNotExist()
    {
        var schemaBuilder = MySqlDatabaseBuilderMock.Create().Schemas.Default;
        schemaBuilder.Objects.CreateView(
            "V",
            SqlNode.RawRecordSet( "foo" ).ToDataSource().Select( s => new[] { s.From["F1"].AsSelf() } ) );

        var db = MySqlDatabaseMock.Create( schemaBuilder.Database );
        ISqlViewDataFieldCollection sut = db.Schemas.Default.Objects.GetView( "V" ).DataFields;

        var action = Lambda.Of( () => sut.Get( "F2" ) );

        action.Should().ThrowExactly<KeyNotFoundException>();
    }

    [Fact]
    public void DataFields_TryGet_ShouldReturnCorrectField()
    {
        var schemaBuilder = MySqlDatabaseBuilderMock.Create().Schemas.Default;
        schemaBuilder.Objects.CreateView(
            "V",
            SqlNode.RawRecordSet( "foo" ).ToDataSource().Select( s => new[] { s.From["F1"].AsSelf(), s.From["F2"].AsSelf() } ) );

        var db = MySqlDatabaseMock.Create( schemaBuilder.Database );
        ISqlViewDataFieldCollection sut = db.Schemas.Default.Objects.GetView( "V" ).DataFields;

        var result = sut.TryGet( "F2", out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            outResult.Should().BeSameAs( sut.First( f => f.Name == "F2" ) );
        }
    }

    [Fact]
    public void DataFields_TryGet_ShouldReturnFalse_WhenFieldDoesNotExist()
    {
        var schemaBuilder = MySqlDatabaseBuilderMock.Create().Schemas.Default;
        schemaBuilder.Objects.CreateView(
            "V",
            SqlNode.RawRecordSet( "foo" ).ToDataSource().Select( s => new[] { s.From["F1"].AsSelf() } ) );

        var db = MySqlDatabaseMock.Create( schemaBuilder.Database );
        ISqlViewDataFieldCollection sut = db.Schemas.Default.Objects.GetView( "V" ).DataFields;

        var result = sut.TryGet( "F2", out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().BeFalse();
            outResult.Should().BeNull();
        }
    }
}
