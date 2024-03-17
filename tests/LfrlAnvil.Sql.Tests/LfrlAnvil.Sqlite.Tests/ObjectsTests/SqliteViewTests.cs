using System.Collections.Generic;
using System.Linq;
using LfrlAnvil.Functional;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sqlite.Objects;
using LfrlAnvil.Sqlite.Tests.Helpers;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.Sqlite.Tests.ObjectsTests;

public class SqliteViewTests : TestsBase
{
    [Fact]
    public void Properties_ShouldBeCorrectlyCopiedFromBuilder()
    {
        var schemaBuilder = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var viewBuilder = schemaBuilder.Objects.CreateView(
            "V",
            SqlNode.RawRecordSet( "bar" )
                .ToDataSource()
                .Select( s => new[] { s.From["a"].AsSelf(), s.From["b"].As( "x" ), s.From["c"].AsSelf() } ) );

        var db = SqliteDatabaseMock.Create( schemaBuilder.Database );
        var schema = db.Schemas.Get( "foo" );

        var sut = schema.Objects.GetView( "V" );
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
            sut.ToString().Should().Be( "[View] foo_V" );

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
        var schemaBuilder = SqliteDatabaseBuilderMock.Create().Schemas.Default;
        schemaBuilder.Objects.CreateView(
            "V",
            SqlNode.RawRecordSet( "foo" ).ToDataSource().Select( s => new[] { s.From["F1"].AsSelf(), s.From["F2"].AsSelf() } ) );

        var db = SqliteDatabaseMock.Create( schemaBuilder.Database );
        var sut = db.Schemas.Default.Objects.GetView( "V" ).DataFields;

        var result = sut.Contains( name );

        result.Should().Be( expected );
    }

    [Fact]
    public void DataFields_Get_ShouldReturnCorrectField()
    {
        var schemaBuilder = SqliteDatabaseBuilderMock.Create().Schemas.Default;
        schemaBuilder.Objects.CreateView(
            "V",
            SqlNode.RawRecordSet( "foo" ).ToDataSource().Select( s => new[] { s.From["F1"].AsSelf(), s.From["F2"].AsSelf() } ) );

        var db = SqliteDatabaseMock.Create( schemaBuilder.Database );
        var sut = db.Schemas.Default.Objects.GetView( "V" ).DataFields;

        var result = sut.Get( "F2" );

        result.Should().BeSameAs( sut.First( f => f.Name == "F2" ) );
    }

    [Fact]
    public void DataFields_Get_ShouldThrowKeyNotFoundException_WhenFieldDoesNotExist()
    {
        var schemaBuilder = SqliteDatabaseBuilderMock.Create().Schemas.Default;
        schemaBuilder.Objects.CreateView(
            "V",
            SqlNode.RawRecordSet( "foo" ).ToDataSource().Select( s => new[] { s.From["F1"].AsSelf() } ) );

        var db = SqliteDatabaseMock.Create( schemaBuilder.Database );
        var sut = db.Schemas.Default.Objects.GetView( "V" ).DataFields;

        var action = Lambda.Of( () => sut.Get( "F2" ) );

        action.Should().ThrowExactly<KeyNotFoundException>();
    }

    [Fact]
    public void DataFields_TryGet_ShouldReturnCorrectField()
    {
        var schemaBuilder = SqliteDatabaseBuilderMock.Create().Schemas.Default;
        schemaBuilder.Objects.CreateView(
            "V",
            SqlNode.RawRecordSet( "foo" ).ToDataSource().Select( s => new[] { s.From["F1"].AsSelf(), s.From["F2"].AsSelf() } ) );

        var db = SqliteDatabaseMock.Create( schemaBuilder.Database );
        var sut = db.Schemas.Default.Objects.GetView( "V" ).DataFields;

        var result = sut.TryGet( "F2" );

        result.Should().BeSameAs( sut.First( f => f.Name == "F2" ) );
    }

    [Fact]
    public void DataFields_TryGet_ShouldReturnNull_WhenFieldDoesNotExist()
    {
        var schemaBuilder = SqliteDatabaseBuilderMock.Create().Schemas.Default;
        schemaBuilder.Objects.CreateView(
            "V",
            SqlNode.RawRecordSet( "foo" ).ToDataSource().Select( s => new[] { s.From["F1"].AsSelf() } ) );

        var db = SqliteDatabaseMock.Create( schemaBuilder.Database );
        var sut = db.Schemas.Default.Objects.GetView( "V" ).DataFields;

        var result = sut.TryGet( "F2" );

        result.Should().BeNull();
    }

    [Fact]
    public void DataFields_GetEnumerator_ShouldReturnCorrectResult()
    {
        var schemaBuilder = SqliteDatabaseBuilderMock.Create().Schemas.Default;
        schemaBuilder.Objects.CreateView(
            "V",
            SqlNode.RawRecordSet( "foo" ).ToDataSource().Select( s => new[] { s.From["F1"].AsSelf(), s.From["F2"].As( "C2" ) } ) );

        var db = SqliteDatabaseMock.Create( schemaBuilder.Database );
        var sut = db.Schemas.Default.Objects.GetView( "V" ).DataFields;

        var result = new List<SqliteViewDataField>();
        foreach ( var e in sut )
            result.Add( e );

        using ( new AssertionScope() )
        {
            result.Should().HaveCount( 2 );
            result.Should().BeEquivalentTo( sut.Get( "F1" ), sut.Get( "C2" ) );
        }
    }
}
