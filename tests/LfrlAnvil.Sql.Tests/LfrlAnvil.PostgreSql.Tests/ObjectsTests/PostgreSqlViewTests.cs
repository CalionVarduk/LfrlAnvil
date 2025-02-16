using System.Collections.Generic;
using System.Linq;
using LfrlAnvil.Functional;
using LfrlAnvil.PostgreSql.Objects;
using LfrlAnvil.PostgreSql.Tests.Helpers;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Expressions;

namespace LfrlAnvil.PostgreSql.Tests.ObjectsTests;

public class PostgreSqlViewTests : TestsBase
{
    [Fact]
    public void Properties_ShouldBeCorrectlyCopiedFromBuilder()
    {
        var schemaBuilder = PostgreSqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var viewBuilder = schemaBuilder.Objects.CreateView(
            "V",
            SqlNode.RawRecordSet( "bar" )
                .ToDataSource()
                .Select( s => new[] { s.From["a"].AsSelf(), s.From["b"].As( "x" ), s.From["c"].AsSelf() } ) );

        var db = PostgreSqlDatabaseMock.Create( schemaBuilder.Database );
        var schema = db.Schemas.Get( "foo" );

        var sut = schema.Objects.GetView( "V" );
        var a = sut.DataFields.Get( "a" );
        var x = sut.DataFields.Get( "x" );
        var c = sut.DataFields.Get( "c" );

        Assertion.All(
                sut.Database.TestRefEquals( db ),
                sut.Schema.TestRefEquals( schema ),
                sut.Type.TestEquals( SqlObjectType.View ),
                sut.Name.TestEquals( "V" ),
                sut.Info.TestEquals( viewBuilder.Info ),
                sut.Node.View.TestRefEquals( sut ),
                sut.Node.Info.TestEquals( sut.Info ),
                sut.Node.Alias.TestNull(),
                sut.Node.Identifier.TestEquals( sut.Info.Identifier ),
                sut.Node.IsOptional.TestFalse(),
                sut.ToString().TestEquals( "[View] foo.V" ),
                sut.DataFields.Count.TestEquals( 3 ),
                sut.DataFields.View.TestRefEquals( sut ),
                sut.DataFields.TestSequence( [ a, x, c ] ) )
            .Go();
    }

    [Theory]
    [InlineData( "F1", true )]
    [InlineData( "F2", true )]
    [InlineData( "F3", false )]
    public void DataFields_Contains_ShouldReturnTrue_WhenFieldExists(string name, bool expected)
    {
        var schemaBuilder = PostgreSqlDatabaseBuilderMock.Create().Schemas.Default;
        schemaBuilder.Objects.CreateView(
            "V",
            SqlNode.RawRecordSet( "foo" ).ToDataSource().Select( s => new[] { s.From["F1"].AsSelf(), s.From["F2"].AsSelf() } ) );

        var db = PostgreSqlDatabaseMock.Create( schemaBuilder.Database );
        var sut = db.Schemas.Default.Objects.GetView( "V" ).DataFields;

        var result = sut.Contains( name );

        result.TestEquals( expected ).Go();
    }

    [Fact]
    public void DataFields_Get_ShouldReturnCorrectField()
    {
        var schemaBuilder = PostgreSqlDatabaseBuilderMock.Create().Schemas.Default;
        schemaBuilder.Objects.CreateView(
            "V",
            SqlNode.RawRecordSet( "foo" ).ToDataSource().Select( s => new[] { s.From["F1"].AsSelf(), s.From["F2"].AsSelf() } ) );

        var db = PostgreSqlDatabaseMock.Create( schemaBuilder.Database );
        var sut = db.Schemas.Default.Objects.GetView( "V" ).DataFields;

        var result = sut.Get( "F2" );

        result.TestRefEquals( sut.First( f => f.Name == "F2" ) ).Go();
    }

    [Fact]
    public void DataFields_Get_ShouldThrowKeyNotFoundException_WhenFieldDoesNotExist()
    {
        var schemaBuilder = PostgreSqlDatabaseBuilderMock.Create().Schemas.Default;
        schemaBuilder.Objects.CreateView(
            "V",
            SqlNode.RawRecordSet( "foo" ).ToDataSource().Select( s => new[] { s.From["F1"].AsSelf() } ) );

        var db = PostgreSqlDatabaseMock.Create( schemaBuilder.Database );
        var sut = db.Schemas.Default.Objects.GetView( "V" ).DataFields;

        var action = Lambda.Of( () => sut.Get( "F2" ) );

        action.Test( exc => exc.TestType().Exact<KeyNotFoundException>() ).Go();
    }

    [Fact]
    public void DataFields_TryGet_ShouldReturnCorrectField()
    {
        var schemaBuilder = PostgreSqlDatabaseBuilderMock.Create().Schemas.Default;
        schemaBuilder.Objects.CreateView(
            "V",
            SqlNode.RawRecordSet( "foo" ).ToDataSource().Select( s => new[] { s.From["F1"].AsSelf(), s.From["F2"].AsSelf() } ) );

        var db = PostgreSqlDatabaseMock.Create( schemaBuilder.Database );
        var sut = db.Schemas.Default.Objects.GetView( "V" ).DataFields;

        var result = sut.TryGet( "F2" );

        result.TestRefEquals( sut.First( f => f.Name == "F2" ) ).Go();
    }

    [Fact]
    public void DataFields_TryGet_ShouldReturnNull_WhenFieldDoesNotExist()
    {
        var schemaBuilder = PostgreSqlDatabaseBuilderMock.Create().Schemas.Default;
        schemaBuilder.Objects.CreateView(
            "V",
            SqlNode.RawRecordSet( "foo" ).ToDataSource().Select( s => new[] { s.From["F1"].AsSelf() } ) );

        var db = PostgreSqlDatabaseMock.Create( schemaBuilder.Database );
        var sut = db.Schemas.Default.Objects.GetView( "V" ).DataFields;

        var result = sut.TryGet( "F2" );

        result.TestNull().Go();
    }

    [Fact]
    public void DataFields_GetEnumerator_ShouldReturnCorrectResult()
    {
        var schemaBuilder = PostgreSqlDatabaseBuilderMock.Create().Schemas.Default;
        schemaBuilder.Objects.CreateView(
            "V",
            SqlNode.RawRecordSet( "foo" ).ToDataSource().Select( s => new[] { s.From["F1"].AsSelf(), s.From["F2"].As( "C2" ) } ) );

        var db = PostgreSqlDatabaseMock.Create( schemaBuilder.Database );
        var sut = db.Schemas.Default.Objects.GetView( "V" ).DataFields;

        var result = new List<PostgreSqlViewDataField>();
        foreach ( var e in sut )
            result.Add( e );

        Assertion.All(
                result.Count.TestEquals( 2 ),
                result.TestSetEqual( [ sut.Get( "F1" ), sut.Get( "C2" ) ] ) )
            .Go();
    }
}
