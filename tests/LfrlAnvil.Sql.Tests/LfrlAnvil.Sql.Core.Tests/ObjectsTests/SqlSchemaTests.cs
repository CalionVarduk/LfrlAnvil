using System.Collections.Generic;
using LfrlAnvil.Functional;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Extensions;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.TestExtensions.Sql.Mocks;

namespace LfrlAnvil.Sql.Tests.ObjectsTests;

public class SqlSchemaTests : TestsBase
{
    [Fact]
    public void Properties_ShouldBeCorrectlyCopiedFromBuilder()
    {
        var schemaBuilder = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.Constraints.SetPrimaryKey( tableBuilder.Columns.Create( "C1" ).Asc() );

        var db = SqlDatabaseMock.Create( schemaBuilder.Database );

        ISqlSchema sut = db.Schemas.Get( "foo" );
        var table = sut.Objects.GetTable( "T" );

        Assertion.All(
                sut.Database.TestRefEquals( db ),
                sut.Name.TestEquals( "foo" ),
                sut.Type.TestEquals( SqlObjectType.Schema ),
                sut.ToString().TestEquals( "[Schema] foo" ),
                sut.Objects.Schema.TestRefEquals( sut ),
                sut.Objects.Count.TestEquals( 3 ),
                sut.Objects.TestSetEqual( [ table, table.Constraints.PrimaryKey, table.Constraints.PrimaryKey.Index ] ) )
            .Go();
    }

    [Theory]
    [InlineData( "T", true )]
    [InlineData( "PK_T", true )]
    [InlineData( "UIX_T_C1A", true )]
    [InlineData( "FK_T_C2_REF_T", true )]
    [InlineData( "foo", false )]
    [InlineData( "C1", false )]
    public void Objects_Contains_ShouldReturnTrue_WhenObjectExists(string name, bool expected)
    {
        var schemaBuilder = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        var indexBuilder1 = tableBuilder.Constraints.CreateIndex( tableBuilder.Columns.Create( "C2" ).Asc() );
        var indexBuilder2 = tableBuilder.Constraints.SetPrimaryKey( tableBuilder.Columns.Create( "C1" ).Asc() ).Index;
        tableBuilder.Constraints.CreateForeignKey( indexBuilder1, indexBuilder2 );

        var db = SqlDatabaseMock.Create( schemaBuilder.Database );
        ISqlObjectCollection sut = db.Schemas.Get( "foo" ).Objects;

        var result = sut.Contains( name );

        result.TestEquals( expected ).Go();
    }

    [Fact]
    public void Objects_Get_ShouldReturnCorrectObject()
    {
        var schemaBuilder = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.Constraints.SetPrimaryKey( tableBuilder.Columns.Create( "C1" ).Asc() );

        var db = SqlDatabaseMock.Create( schemaBuilder.Database );
        ISqlObjectCollection sut = db.Schemas.Get( "foo" ).Objects;

        var result = sut.Get( "T" );

        Assertion.All(
                result.Type.TestEquals( SqlObjectType.Table ),
                result.Name.TestEquals( "T" ) )
            .Go();
    }

    [Fact]
    public void Objects_Get_ShouldThrowKeyNotFoundException_WhenObjectDoesNotExist()
    {
        var schemaBuilder = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.Constraints.SetPrimaryKey( tableBuilder.Columns.Create( "C1" ).Asc() );

        var db = SqlDatabaseMock.Create( schemaBuilder.Database );
        ISqlObjectCollection sut = db.Schemas.Get( "foo" ).Objects;

        var action = Lambda.Of( () => sut.Get( "U" ) );

        action.Test( exc => exc.TestType().Exact<KeyNotFoundException>() ).Go();
    }

    [Fact]
    public void Objects_TryGet_ShouldReturnCorrectObject()
    {
        var schemaBuilder = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.Constraints.SetPrimaryKey( tableBuilder.Columns.Create( "C1" ).Asc() );

        var db = SqlDatabaseMock.Create( schemaBuilder.Database );
        ISqlObjectCollection sut = db.Schemas.Get( "foo" ).Objects;

        var result = sut.TryGet( "T" );

        Assertion.All(
                result.TestNotNull(),
                (result?.Type).TestEquals( SqlObjectType.Table ),
                (result?.Name).TestEquals( "T" ) )
            .Go();
    }

    [Fact]
    public void Objects_TryGet_ShouldReturnNull_WhenObjectDoesNotExist()
    {
        var schemaBuilder = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.Constraints.SetPrimaryKey( tableBuilder.Columns.Create( "C1" ).Asc() );

        var db = SqlDatabaseMock.Create( schemaBuilder.Database );
        ISqlObjectCollection sut = db.Schemas.Get( "foo" ).Objects;

        var result = sut.TryGet( "U" );

        result.TestNull().Go();
    }

    [Fact]
    public void Objects_GetTable_ShouldReturnCorrectTable()
    {
        var schemaBuilder = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.Constraints.SetPrimaryKey( tableBuilder.Columns.Create( "C1" ).Asc() );

        var db = SqlDatabaseMock.Create( schemaBuilder.Database );
        ISqlObjectCollection sut = db.Schemas.Get( "foo" ).Objects;

        var result = sut.GetTable( "T" );

        Assertion.All(
                result.Type.TestEquals( SqlObjectType.Table ),
                result.Name.TestEquals( "T" ) )
            .Go();
    }

    [Fact]
    public void Objects_GetTable_ShouldThrowKeyNotFoundException_WhenObjectDoesNotExist()
    {
        var schemaBuilder = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.Constraints.SetPrimaryKey( tableBuilder.Columns.Create( "C1" ).Asc() );

        var db = SqlDatabaseMock.Create( schemaBuilder.Database );
        ISqlObjectCollection sut = db.Schemas.Get( "foo" ).Objects;

        var action = Lambda.Of( () => sut.GetTable( "U" ) );

        action.Test( exc => exc.TestType().Exact<KeyNotFoundException>() ).Go();
    }

    [Fact]
    public void Objects_GetTable_ShouldThrowSqlObjectCastException_WhenObjectExistsButIsNotTable()
    {
        var schemaBuilder = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.Constraints.SetPrimaryKey( tableBuilder.Columns.Create( "C1" ).Asc() );

        var db = SqlDatabaseMock.Create( schemaBuilder.Database );
        ISqlObjectCollection sut = db.Schemas.Get( "foo" ).Objects;

        var action = Lambda.Of( () => sut.GetTable( "PK_T" ) );

        action.Test( exc => exc.TestType().Exact<SqlObjectCastException>() ).Go();
    }

    [Fact]
    public void Objects_TryGetTable_ShouldReturnCorrectTable()
    {
        var schemaBuilder = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.Constraints.SetPrimaryKey( tableBuilder.Columns.Create( "C1" ).Asc() );

        var db = SqlDatabaseMock.Create( schemaBuilder.Database );
        ISqlObjectCollection sut = db.Schemas.Get( "foo" ).Objects;

        var result = sut.TryGetTable( "T" );

        Assertion.All(
                result.TestNotNull(),
                (result?.Type).TestEquals( SqlObjectType.Table ),
                (result?.Name).TestEquals( "T" ) )
            .Go();
    }

    [Fact]
    public void Objects_TryGetTable_ShouldReturnNull_WhenObjectDoesNotExist()
    {
        var schemaBuilder = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.Constraints.SetPrimaryKey( tableBuilder.Columns.Create( "C1" ).Asc() );

        var db = SqlDatabaseMock.Create( schemaBuilder.Database );
        ISqlObjectCollection sut = db.Schemas.Get( "foo" ).Objects;

        var result = sut.TryGetTable( "U" );

        result.TestNull().Go();
    }

    [Fact]
    public void Objects_TryGetTable_ShouldReturnNull_WhenObjectExistsButIsNotTable()
    {
        var schemaBuilder = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.Constraints.SetPrimaryKey( tableBuilder.Columns.Create( "C1" ).Asc() );

        var db = SqlDatabaseMock.Create( schemaBuilder.Database );
        ISqlObjectCollection sut = db.Schemas.Get( "foo" ).Objects;

        var result = sut.TryGetTable( "PK_T" );

        result.TestNull().Go();
    }

    [Fact]
    public void Objects_GetPrimaryKey_ShouldReturnCorrectPrimaryKey()
    {
        var schemaBuilder = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.Constraints.SetPrimaryKey( tableBuilder.Columns.Create( "C1" ).Asc() );

        var db = SqlDatabaseMock.Create( schemaBuilder.Database );
        ISqlObjectCollection sut = db.Schemas.Get( "foo" ).Objects;

        var result = sut.GetPrimaryKey( "PK_T" );

        Assertion.All(
                result.Type.TestEquals( SqlObjectType.PrimaryKey ),
                result.Name.TestEquals( "PK_T" ) )
            .Go();
    }

    [Fact]
    public void Objects_GetPrimaryKey_ShouldThrowKeyNotFoundException_WhenObjectDoesNotExist()
    {
        var schemaBuilder = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.Constraints.SetPrimaryKey( tableBuilder.Columns.Create( "C1" ).Asc() );

        var db = SqlDatabaseMock.Create( schemaBuilder.Database );
        ISqlObjectCollection sut = db.Schemas.Get( "foo" ).Objects;

        var action = Lambda.Of( () => sut.GetPrimaryKey( "U" ) );

        action.Test( exc => exc.TestType().Exact<KeyNotFoundException>() ).Go();
    }

    [Fact]
    public void Objects_GetPrimaryKey_ShouldThrowSqlObjectCastException_WhenObjectExistsButIsNotPrimaryKey()
    {
        var schemaBuilder = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.Constraints.SetPrimaryKey( tableBuilder.Columns.Create( "C1" ).Asc() );

        var db = SqlDatabaseMock.Create( schemaBuilder.Database );
        ISqlObjectCollection sut = db.Schemas.Get( "foo" ).Objects;

        var action = Lambda.Of( () => sut.GetPrimaryKey( "T" ) );

        action.Test( exc => exc.TestType().Exact<SqlObjectCastException>() ).Go();
    }

    [Fact]
    public void Objects_TryGetPrimaryKey_ShouldReturnCorrectPrimaryKey()
    {
        var schemaBuilder = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.Constraints.SetPrimaryKey( tableBuilder.Columns.Create( "C1" ).Asc() );

        var db = SqlDatabaseMock.Create( schemaBuilder.Database );
        ISqlObjectCollection sut = db.Schemas.Get( "foo" ).Objects;

        var result = sut.TryGetPrimaryKey( "PK_T" );

        Assertion.All(
                result.TestNotNull(),
                (result?.Type).TestEquals( SqlObjectType.PrimaryKey ),
                (result?.Name).TestEquals( "PK_T" ) )
            .Go();
    }

    [Fact]
    public void Objects_TryGetPrimaryKey_ShouldReturnNull_WhenObjectDoesNotExist()
    {
        var schemaBuilder = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.Constraints.SetPrimaryKey( tableBuilder.Columns.Create( "C1" ).Asc() );

        var db = SqlDatabaseMock.Create( schemaBuilder.Database );
        ISqlObjectCollection sut = db.Schemas.Get( "foo" ).Objects;

        var result = sut.TryGetPrimaryKey( "U" );

        result.TestNull().Go();
    }

    [Fact]
    public void Objects_TryGetPrimaryKey_ShouldReturnNull_WhenObjectExistsButIsNotPrimaryKey()
    {
        var schemaBuilder = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.Constraints.SetPrimaryKey( tableBuilder.Columns.Create( "C1" ).Asc() );

        var db = SqlDatabaseMock.Create( schemaBuilder.Database );
        ISqlObjectCollection sut = db.Schemas.Get( "foo" ).Objects;

        var result = sut.TryGetPrimaryKey( "T" );

        result.TestNull().Go();
    }

    [Fact]
    public void Objects_GetIndex_ShouldReturnCorrectIndex()
    {
        var schemaBuilder = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.Constraints.SetPrimaryKey( tableBuilder.Columns.Create( "C1" ).Asc() );

        var db = SqlDatabaseMock.Create( schemaBuilder.Database );
        ISqlObjectCollection sut = db.Schemas.Get( "foo" ).Objects;

        var result = sut.GetIndex( "UIX_T_C1A" );

        Assertion.All(
                result.Type.TestEquals( SqlObjectType.Index ),
                result.Name.TestEquals( "UIX_T_C1A" ) )
            .Go();
    }

    [Fact]
    public void Objects_GetIndex_ShouldThrowKeyNotFoundException_WhenObjectDoesNotExist()
    {
        var schemaBuilder = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.Constraints.SetPrimaryKey( tableBuilder.Columns.Create( "C1" ).Asc() );

        var db = SqlDatabaseMock.Create( schemaBuilder.Database );
        ISqlObjectCollection sut = db.Schemas.Get( "foo" ).Objects;

        var action = Lambda.Of( () => sut.GetIndex( "U" ) );

        action.Test( exc => exc.TestType().Exact<KeyNotFoundException>() ).Go();
    }

    [Fact]
    public void Objects_GetIndex_ShouldThrowSqlObjectCastException_WhenObjectExistsButIsNotIndex()
    {
        var schemaBuilder = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.Constraints.SetPrimaryKey( tableBuilder.Columns.Create( "C1" ).Asc() );

        var db = SqlDatabaseMock.Create( schemaBuilder.Database );
        ISqlObjectCollection sut = db.Schemas.Get( "foo" ).Objects;

        var action = Lambda.Of( () => sut.GetIndex( "T" ) );

        action.Test( exc => exc.TestType().Exact<SqlObjectCastException>() ).Go();
    }

    [Fact]
    public void Objects_TryGetIndex_ShouldReturnCorrectIndex()
    {
        var schemaBuilder = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.Constraints.SetPrimaryKey( tableBuilder.Columns.Create( "C1" ).Asc() );

        var db = SqlDatabaseMock.Create( schemaBuilder.Database );
        ISqlObjectCollection sut = db.Schemas.Get( "foo" ).Objects;

        var result = sut.TryGetIndex( "UIX_T_C1A" );

        Assertion.All(
                result.TestNotNull(),
                (result?.Type).TestEquals( SqlObjectType.Index ),
                (result?.Name).TestEquals( "UIX_T_C1A" ) )
            .Go();
    }

    [Fact]
    public void Objects_TryGetIndex_ShouldReturnNull_WhenObjectDoesNotExist()
    {
        var schemaBuilder = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.Constraints.SetPrimaryKey( tableBuilder.Columns.Create( "C1" ).Asc() );

        var db = SqlDatabaseMock.Create( schemaBuilder.Database );
        ISqlObjectCollection sut = db.Schemas.Get( "foo" ).Objects;

        var result = sut.TryGetIndex( "U" );

        result.TestNull().Go();
    }

    [Fact]
    public void Objects_TryGetIndex_ShouldReturnNull_WhenObjectExistsButIsNotIndex()
    {
        var schemaBuilder = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.Constraints.SetPrimaryKey( tableBuilder.Columns.Create( "C1" ).Asc() );

        var db = SqlDatabaseMock.Create( schemaBuilder.Database );
        ISqlObjectCollection sut = db.Schemas.Get( "foo" ).Objects;

        var result = sut.TryGetIndex( "T" );

        result.TestNull().Go();
    }

    [Fact]
    public void Objects_GetForeignKey_ShouldReturnCorrectForeignKey()
    {
        var schemaBuilder = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        var indexBuilder1 = tableBuilder.Constraints.CreateIndex( tableBuilder.Columns.Create( "C2" ).Asc() );
        var indexBuilder2 = tableBuilder.Constraints.SetPrimaryKey( tableBuilder.Columns.Create( "C1" ).Asc() ).Index;
        tableBuilder.Constraints.CreateForeignKey( indexBuilder1, indexBuilder2 );

        var db = SqlDatabaseMock.Create( schemaBuilder.Database );
        ISqlObjectCollection sut = db.Schemas.Get( "foo" ).Objects;

        var result = sut.GetForeignKey( "FK_T_C2_REF_T" );

        Assertion.All(
                result.Type.TestEquals( SqlObjectType.ForeignKey ),
                result.Name.TestEquals( "FK_T_C2_REF_T" ) )
            .Go();
    }

    [Fact]
    public void Objects_GetForeignKey_ShouldThrowKeyNotFoundException_WhenObjectDoesNotExist()
    {
        var schemaBuilder = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        var indexBuilder1 = tableBuilder.Constraints.CreateIndex( tableBuilder.Columns.Create( "C2" ).Asc() );
        var indexBuilder2 = tableBuilder.Constraints.SetPrimaryKey( tableBuilder.Columns.Create( "C1" ).Asc() ).Index;
        tableBuilder.Constraints.CreateForeignKey( indexBuilder1, indexBuilder2 );

        var db = SqlDatabaseMock.Create( schemaBuilder.Database );
        ISqlObjectCollection sut = db.Schemas.Get( "foo" ).Objects;

        var action = Lambda.Of( () => sut.GetForeignKey( "U" ) );

        action.Test( exc => exc.TestType().Exact<KeyNotFoundException>() ).Go();
    }

    [Fact]
    public void Objects_GetForeignKey_ShouldThrowSqlObjectCastException_WhenObjectExistsButIsNotForeignKey()
    {
        var schemaBuilder = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        var indexBuilder1 = tableBuilder.Constraints.CreateIndex( tableBuilder.Columns.Create( "C2" ).Asc() );
        var indexBuilder2 = tableBuilder.Constraints.SetPrimaryKey( tableBuilder.Columns.Create( "C1" ).Asc() ).Index;
        tableBuilder.Constraints.CreateForeignKey( indexBuilder1, indexBuilder2 );

        var db = SqlDatabaseMock.Create( schemaBuilder.Database );
        ISqlObjectCollection sut = db.Schemas.Get( "foo" ).Objects;

        var action = Lambda.Of( () => sut.GetForeignKey( "T" ) );

        action.Test( exc => exc.TestType().Exact<SqlObjectCastException>() ).Go();
    }

    [Fact]
    public void Objects_TryGetForeignKey_ShouldReturnCorrectForeignKey()
    {
        var schemaBuilder = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        var indexBuilder1 = tableBuilder.Constraints.CreateIndex( tableBuilder.Columns.Create( "C2" ).Asc() );
        var indexBuilder2 = tableBuilder.Constraints.SetPrimaryKey( tableBuilder.Columns.Create( "C1" ).Asc() ).Index;
        tableBuilder.Constraints.CreateForeignKey( indexBuilder1, indexBuilder2 );

        var db = SqlDatabaseMock.Create( schemaBuilder.Database );
        ISqlObjectCollection sut = db.Schemas.Get( "foo" ).Objects;

        var result = sut.TryGetForeignKey( "FK_T_C2_REF_T" );

        Assertion.All(
                result.TestNotNull(),
                (result?.Type).TestEquals( SqlObjectType.ForeignKey ),
                (result?.Name).TestEquals( "FK_T_C2_REF_T" ) )
            .Go();
    }

    [Fact]
    public void Objects_TryGetForeignKey_ShouldReturnNull_WhenObjectDoesNotExist()
    {
        var schemaBuilder = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        var indexBuilder1 = tableBuilder.Constraints.CreateIndex( tableBuilder.Columns.Create( "C2" ).Asc() );
        var indexBuilder2 = tableBuilder.Constraints.SetPrimaryKey( tableBuilder.Columns.Create( "C1" ).Asc() ).Index;
        tableBuilder.Constraints.CreateForeignKey( indexBuilder1, indexBuilder2 );

        var db = SqlDatabaseMock.Create( schemaBuilder.Database );
        ISqlObjectCollection sut = db.Schemas.Get( "foo" ).Objects;

        var result = sut.TryGetForeignKey( "U" );

        result.TestNull().Go();
    }

    [Fact]
    public void Objects_TryGetForeignKey_ShouldReturnNull_WhenObjectExistsButIsNotForeignKey()
    {
        var schemaBuilder = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        var indexBuilder1 = tableBuilder.Constraints.CreateIndex( tableBuilder.Columns.Create( "C2" ).Asc() );
        var indexBuilder2 = tableBuilder.Constraints.SetPrimaryKey( tableBuilder.Columns.Create( "C1" ).Asc() ).Index;
        tableBuilder.Constraints.CreateForeignKey( indexBuilder1, indexBuilder2 );

        var db = SqlDatabaseMock.Create( schemaBuilder.Database );
        ISqlObjectCollection sut = db.Schemas.Get( "foo" ).Objects;

        var result = sut.TryGetForeignKey( "T" );

        result.TestNull().Go();
    }

    [Fact]
    public void Objects_GetView_ShouldReturnCorrectView()
    {
        var schemaBuilder = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        schemaBuilder.Objects.CreateView( "V", SqlNode.RawQuery( "SELECT * FROM foo" ) );

        var db = SqlDatabaseMock.Create( schemaBuilder.Database );
        ISqlObjectCollection sut = db.Schemas.Get( "foo" ).Objects;

        var result = sut.GetView( "V" );

        Assertion.All(
                result.Type.TestEquals( SqlObjectType.View ),
                result.Name.TestEquals( "V" ) )
            .Go();
    }

    [Fact]
    public void Objects_GetView_ShouldThrowKeyNotFoundException_WhenObjectDoesNotExist()
    {
        var schemaBuilder = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.Constraints.SetPrimaryKey( tableBuilder.Columns.Create( "C" ).Asc() );

        var db = SqlDatabaseMock.Create( schemaBuilder.Database );
        ISqlObjectCollection sut = db.Schemas.Get( "foo" ).Objects;

        var action = Lambda.Of( () => sut.GetView( "V" ) );

        action.Test( exc => exc.TestType().Exact<KeyNotFoundException>() ).Go();
    }

    [Fact]
    public void Objects_GetView_ShouldThrowSqlObjectCastException_WhenObjectExistsButIsNotView()
    {
        var schemaBuilder = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.Constraints.SetPrimaryKey( tableBuilder.Columns.Create( "C" ).Asc() );

        var db = SqlDatabaseMock.Create( schemaBuilder.Database );
        ISqlObjectCollection sut = db.Schemas.Get( "foo" ).Objects;

        var action = Lambda.Of( () => sut.GetView( "T" ) );

        action.Test( exc => exc.TestType().Exact<SqlObjectCastException>() ).Go();
    }

    [Fact]
    public void Objects_TryGetView_ShouldReturnCorrectView()
    {
        var schemaBuilder = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        schemaBuilder.Objects.CreateView( "V", SqlNode.RawQuery( "SELECT * FROM foo" ) );

        var db = SqlDatabaseMock.Create( schemaBuilder.Database );
        ISqlObjectCollection sut = db.Schemas.Get( "foo" ).Objects;

        var result = sut.TryGetView( "V" );

        Assertion.All(
                result.TestNotNull(),
                (result?.Type).TestEquals( SqlObjectType.View ),
                (result?.Name).TestEquals( "V" ) )
            .Go();
    }

    [Fact]
    public void Objects_TryGetView_ShouldReturnNull_WhenObjectDoesNotExist()
    {
        var schemaBuilder = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        var indexBuilder1 = tableBuilder.Constraints.CreateIndex( tableBuilder.Columns.Create( "C2" ).Asc() );
        var indexBuilder2 = tableBuilder.Constraints.SetPrimaryKey( tableBuilder.Columns.Create( "C1" ).Asc() ).Index;
        tableBuilder.Constraints.CreateForeignKey( indexBuilder1, indexBuilder2 );

        var db = SqlDatabaseMock.Create( schemaBuilder.Database );
        ISqlObjectCollection sut = db.Schemas.Get( "foo" ).Objects;

        var result = sut.TryGetView( "U" );

        result.TestNull().Go();
    }

    [Fact]
    public void Objects_TryGetView_ShouldReturnNull_WhenObjectExistsButIsNotView()
    {
        var schemaBuilder = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.Constraints.SetPrimaryKey( tableBuilder.Columns.Create( "C" ).Asc() );

        var db = SqlDatabaseMock.Create( schemaBuilder.Database );
        ISqlObjectCollection sut = db.Schemas.Get( "foo" ).Objects;

        var result = sut.TryGetView( "T" );

        result.TestNull().Go();
    }

    [Fact]
    public void Objects_GetCheck_ShouldReturnCorrectCheck()
    {
        var schemaBuilder = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.Constraints.SetPrimaryKey( tableBuilder.Columns.Create( "C" ).Asc() );
        tableBuilder.Constraints.CreateCheck( "CHK_T_0", tableBuilder.Node["C"] > SqlNode.Literal( 0 ) );

        var db = SqlDatabaseMock.Create( schemaBuilder.Database );
        ISqlObjectCollection sut = db.Schemas.Get( "foo" ).Objects;

        var result = sut.GetCheck( "CHK_T_0" );

        Assertion.All(
                result.Type.TestEquals( SqlObjectType.Check ),
                result.Name.TestEquals( "CHK_T_0" ) )
            .Go();
    }

    [Fact]
    public void Objects_GetCheck_ShouldThrowKeyNotFoundException_WhenObjectDoesNotExist()
    {
        var schemaBuilder = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.Constraints.SetPrimaryKey( tableBuilder.Columns.Create( "C" ).Asc() );
        tableBuilder.Constraints.CreateCheck( tableBuilder.Node["C"] > SqlNode.Literal( 0 ) );

        var db = SqlDatabaseMock.Create( schemaBuilder.Database );
        ISqlObjectCollection sut = db.Schemas.Get( "foo" ).Objects;

        var action = Lambda.Of( () => sut.GetCheck( "U" ) );

        action.Test( exc => exc.TestType().Exact<KeyNotFoundException>() ).Go();
    }

    [Fact]
    public void Objects_GetCheck_ShouldThrowSqlObjectCastException_WhenObjectExistsButIsNotCheck()
    {
        var schemaBuilder = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.Constraints.SetPrimaryKey( tableBuilder.Columns.Create( "C" ).Asc() );
        tableBuilder.Constraints.CreateCheck( tableBuilder.Node["C"] > SqlNode.Literal( 0 ) );

        var db = SqlDatabaseMock.Create( schemaBuilder.Database );
        ISqlObjectCollection sut = db.Schemas.Get( "foo" ).Objects;

        var action = Lambda.Of( () => sut.GetCheck( "T" ) );

        action.Test( exc => exc.TestType().Exact<SqlObjectCastException>() ).Go();
    }

    [Fact]
    public void Objects_TryGetCheck_ShouldReturnCorrectCheck()
    {
        var schemaBuilder = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.Constraints.SetPrimaryKey( tableBuilder.Columns.Create( "C" ).Asc() );
        tableBuilder.Constraints.CreateCheck( "CHK_T_0", tableBuilder.Node["C"] > SqlNode.Literal( 0 ) );

        var db = SqlDatabaseMock.Create( schemaBuilder.Database );
        ISqlObjectCollection sut = db.Schemas.Get( "foo" ).Objects;

        var result = sut.TryGetCheck( "CHK_T_0" );

        Assertion.All(
                result.TestNotNull(),
                (result?.Type).TestEquals( SqlObjectType.Check ),
                (result?.Name).TestEquals( "CHK_T_0" ) )
            .Go();
    }

    [Fact]
    public void Objects_TryGetCheck_ShouldReturnNull_WhenObjectDoesNotExist()
    {
        var schemaBuilder = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.Constraints.SetPrimaryKey( tableBuilder.Columns.Create( "C" ).Asc() );
        tableBuilder.Constraints.CreateCheck( tableBuilder.Node["C"] > SqlNode.Literal( 0 ) );

        var db = SqlDatabaseMock.Create( schemaBuilder.Database );
        ISqlObjectCollection sut = db.Schemas.Get( "foo" ).Objects;

        var result = sut.TryGetCheck( "U" );

        result.TestNull().Go();
    }

    [Fact]
    public void Objects_TryGetCheck_ShouldReturnNull_WhenObjectExistsButIsNotCheck()
    {
        var schemaBuilder = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.Constraints.SetPrimaryKey( tableBuilder.Columns.Create( "C" ).Asc() );
        tableBuilder.Constraints.CreateCheck( tableBuilder.Node["C"] > SqlNode.Literal( 0 ) );

        var db = SqlDatabaseMock.Create( schemaBuilder.Database );
        ISqlObjectCollection sut = db.Schemas.Get( "foo" ).Objects;

        var result = sut.TryGetCheck( "T" );

        result.TestNull().Go();
    }
}
