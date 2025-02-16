using System.Collections.Generic;
using LfrlAnvil.Functional;
using LfrlAnvil.PostgreSql.Extensions;
using LfrlAnvil.PostgreSql.Objects;
using LfrlAnvil.PostgreSql.Tests.Helpers;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Expressions;

namespace LfrlAnvil.PostgreSql.Tests.ObjectsTests;

public class PostgreSqlTableTests : TestsBase
{
    [Fact]
    public void Properties_ShouldBeCorrectlyCopiedFromBuilder()
    {
        var schemaBuilder = PostgreSqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.Constraints.SetPrimaryKey( tableBuilder.Columns.Create( "C1" ).Asc() );

        var db = PostgreSqlDatabaseMock.Create( schemaBuilder.Database );
        var schema = db.Schemas.Get( "foo" );

        var sut = schema.Objects.GetTable( "T" );
        var c1 = sut.Columns.Get( "C1" );

        Assertion.All(
                sut.Database.TestRefEquals( db ),
                sut.Schema.TestRefEquals( schema ),
                sut.Type.TestEquals( SqlObjectType.Table ),
                sut.Name.TestEquals( "T" ),
                sut.Info.TestEquals( tableBuilder.Info ),
                sut.Node.Table.TestRefEquals( sut ),
                sut.Node.Info.TestEquals( sut.Info ),
                sut.Node.Alias.TestNull(),
                sut.Node.Identifier.TestEquals( sut.Info.Identifier ),
                sut.Node.IsOptional.TestFalse(),
                sut.ToString().TestEquals( "[Table] foo.T" ),
                sut.Columns.Count.TestEquals( 1 ),
                sut.Columns.Table.TestRefEquals( sut ),
                sut.Columns.TestSequence( [ c1 ] ),
                sut.Constraints.Count.TestEquals( 2 ),
                sut.Constraints.Table.TestRefEquals( sut ),
                sut.Constraints.PrimaryKey.Index.Table.TestRefEquals( sut ),
                sut.Constraints.TestSetEqual( [ sut.Constraints.PrimaryKey, sut.Constraints.PrimaryKey.Index ] ) )
            .Go();
    }

    [Fact]
    public void Creation_ShouldThrowSqlObjectBuilderException_WhenBuilderPrimaryKeyIsNull()
    {
        var schemaBuilder = PostgreSqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        schemaBuilder.Objects.CreateTable( "T" );

        var action = Lambda.Of( () => PostgreSqlDatabaseMock.Create( schemaBuilder.Database ) );

        action.Test( exc => exc.TestType().Exact<SqlObjectBuilderException>() ).Go();
    }

    [Theory]
    [InlineData( "C1", true )]
    [InlineData( "C2", true )]
    [InlineData( "C3", false )]
    public void Columns_Contains_ShouldReturnTrue_WhenColumnExists(string name, bool expected)
    {
        var schemaBuilder = PostgreSqlDatabaseBuilderMock.Create().Schemas.Default;
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.Columns.Create( "C1" );
        tableBuilder.Constraints.SetPrimaryKey( tableBuilder.Columns.Create( "C2" ).Asc() );

        var db = PostgreSqlDatabaseMock.Create( schemaBuilder.Database );
        var sut = db.Schemas.Default.Objects.GetTable( "T" ).Columns;

        var result = sut.Contains( name );

        result.TestEquals( expected ).Go();
    }

    [Fact]
    public void Columns_Get_ShouldReturnCorrectColumn()
    {
        var schemaBuilder = PostgreSqlDatabaseBuilderMock.Create().Schemas.Default;
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.Columns.Create( "C1" );
        tableBuilder.Constraints.SetPrimaryKey( tableBuilder.Columns.Create( "C2" ).Asc() );

        var db = PostgreSqlDatabaseMock.Create( schemaBuilder.Database );
        var sut = db.Schemas.Default.Objects.GetTable( "T" ).Columns;

        var result = sut.Get( "C2" );

        result.TestRefEquals( sut.Table.Constraints.PrimaryKey.Index.Columns[0].Column ).Go();
    }

    [Fact]
    public void Columns_Get_ShouldThrowKeyNotFoundException_WhenColumnDoesNotExist()
    {
        var schemaBuilder = PostgreSqlDatabaseBuilderMock.Create().Schemas.Default;
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.Constraints.SetPrimaryKey( tableBuilder.Columns.Create( "C1" ).Asc() );

        var db = PostgreSqlDatabaseMock.Create( schemaBuilder.Database );
        var sut = db.Schemas.Default.Objects.GetTable( "T" ).Columns;

        var action = Lambda.Of( () => sut.Get( "C2" ) );

        action.Test( exc => exc.TestType().Exact<KeyNotFoundException>() ).Go();
    }

    [Fact]
    public void Columns_TryGet_ShouldReturnCorrectColumn()
    {
        var schemaBuilder = PostgreSqlDatabaseBuilderMock.Create().Schemas.Default;
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.Columns.Create( "C1" );
        tableBuilder.Constraints.SetPrimaryKey( tableBuilder.Columns.Create( "C2" ).Asc() );

        var db = PostgreSqlDatabaseMock.Create( schemaBuilder.Database );
        var sut = db.Schemas.Default.Objects.GetTable( "T" ).Columns;

        var result = sut.TryGet( "C2" );

        result.TestRefEquals( sut.Table.Constraints.PrimaryKey.Index.Columns[0].Column ).Go();
    }

    [Fact]
    public void Columns_TryGet_ShouldReturnNull_WhenColumnDoesNotExist()
    {
        var schemaBuilder = PostgreSqlDatabaseBuilderMock.Create().Schemas.Default;
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.Constraints.SetPrimaryKey( tableBuilder.Columns.Create( "C1" ).Asc() );

        var db = PostgreSqlDatabaseMock.Create( schemaBuilder.Database );
        var sut = db.Schemas.Default.Objects.GetTable( "T" ).Columns;

        var result = sut.TryGet( "C2" );

        result.TestNull().Go();
    }

    [Fact]
    public void Columns_GetEnumerator_ShouldReturnCorrectResult()
    {
        var schemaBuilder = PostgreSqlDatabaseBuilderMock.Create().Schemas.Default;
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.Constraints.SetPrimaryKey( tableBuilder.Columns.Create( "C1" ).Asc() );
        tableBuilder.Columns.Create( "C2" );

        var db = PostgreSqlDatabaseMock.Create( schemaBuilder.Database );
        var sut = db.Schemas.Default.Objects.GetTable( "T" ).Columns;

        var result = new List<PostgreSqlColumn>();
        foreach ( var e in sut )
            result.Add( e );

        Assertion.All(
                result.Count.TestEquals( 2 ),
                result.TestSetEqual( [ sut.Get( "C1" ), sut.Get( "C2" ) ] ) )
            .Go();
    }

    [Theory]
    [InlineData( "PK_T", true )]
    [InlineData( "UIX_T_C3A", true )]
    [InlineData( "C1", false )]
    public void Constraints_Contains_ShouldReturnTrue_WhenConstraintExists(string name, bool expected)
    {
        var schemaBuilder = PostgreSqlDatabaseBuilderMock.Create().Schemas.Default;
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.Constraints.CreateIndex( tableBuilder.Columns.Create( "C1" ).Asc(), tableBuilder.Columns.Create( "C2" ).Desc() );
        tableBuilder.Constraints.SetPrimaryKey( tableBuilder.Columns.Create( "C3" ).Asc() );

        var db = PostgreSqlDatabaseMock.Create( schemaBuilder.Database );
        var table = db.Schemas.Default.Objects.GetTable( "T" );
        var sut = table.Constraints;

        var result = sut.Contains( name );

        result.TestEquals( expected ).Go();
    }

    [Fact]
    public void Constraints_Get_ShouldReturnCorrectConstraint()
    {
        var schemaBuilder = PostgreSqlDatabaseBuilderMock.Create().Schemas.Default;
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.Constraints.CreateIndex( tableBuilder.Columns.Create( "C1" ).Asc(), tableBuilder.Columns.Create( "C2" ).Desc() );
        tableBuilder.Constraints.SetPrimaryKey( tableBuilder.Columns.Create( "C3" ).Asc() );

        var db = PostgreSqlDatabaseMock.Create( schemaBuilder.Database );
        var table = db.Schemas.Default.Objects.GetTable( "T" );
        var sut = table.Constraints;

        var result = sut.Get( "PK_T" );

        Assertion.All(
                result.Type.TestEquals( SqlObjectType.PrimaryKey ),
                result.Name.TestEquals( "PK_T" ) )
            .Go();
    }

    [Fact]
    public void Constraints_Get_ShouldThrowKeyNotFoundException_WhenConstraintDoesNotExist()
    {
        var schemaBuilder = PostgreSqlDatabaseBuilderMock.Create().Schemas.Default;
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.Constraints.CreateIndex( tableBuilder.Columns.Create( "C1" ).Asc(), tableBuilder.Columns.Create( "C2" ).Desc() );
        tableBuilder.Constraints.SetPrimaryKey( tableBuilder.Columns.Create( "C3" ).Asc() );

        var db = PostgreSqlDatabaseMock.Create( schemaBuilder.Database );
        var table = db.Schemas.Default.Objects.GetTable( "T" );
        var sut = table.Constraints;

        var action = Lambda.Of( () => sut.Get( "foo" ) );

        action.Test( exc => exc.TestType().Exact<KeyNotFoundException>() ).Go();
    }

    [Fact]
    public void Constraints_TryGet_ShouldReturnCorrectConstraint()
    {
        var schemaBuilder = PostgreSqlDatabaseBuilderMock.Create().Schemas.Default;
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.Constraints.CreateIndex( tableBuilder.Columns.Create( "C1" ).Asc(), tableBuilder.Columns.Create( "C2" ).Desc() );
        tableBuilder.Constraints.SetPrimaryKey( tableBuilder.Columns.Create( "C3" ).Asc() );

        var db = PostgreSqlDatabaseMock.Create( schemaBuilder.Database );
        var table = db.Schemas.Default.Objects.GetTable( "T" );
        var sut = table.Constraints;

        var result = sut.TryGet( "PK_T" );

        Assertion.All(
                result.TestNotNull(),
                (result?.Type).TestEquals( SqlObjectType.PrimaryKey ),
                (result?.Name).TestEquals( "PK_T" ) )
            .Go();
    }

    [Fact]
    public void Constraints_TryGet_ShouldReturnNull_WhenConstraintDoesNotExist()
    {
        var schemaBuilder = PostgreSqlDatabaseBuilderMock.Create().Schemas.Default;
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.Constraints.CreateIndex( tableBuilder.Columns.Create( "C1" ).Asc(), tableBuilder.Columns.Create( "C2" ).Desc() );
        tableBuilder.Constraints.SetPrimaryKey( tableBuilder.Columns.Create( "C3" ).Asc() );

        var db = PostgreSqlDatabaseMock.Create( schemaBuilder.Database );
        var table = db.Schemas.Default.Objects.GetTable( "T" );
        var sut = table.Constraints;

        var result = sut.TryGet( "foo" );

        result.TestNull().Go();
    }

    [Fact]
    public void Constraints_GetIndex_ShouldReturnCorrectIndex()
    {
        var schemaBuilder = PostgreSqlDatabaseBuilderMock.Create().Schemas.Default;
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        var indexBuilder = tableBuilder.Constraints.CreateIndex(
            tableBuilder.Columns.Create( "C1" ).Asc(),
            tableBuilder.Columns.Create( "C2" ).Desc() );

        tableBuilder.Constraints.SetPrimaryKey( tableBuilder.Columns.Create( "C3" ).Asc() );

        var db = PostgreSqlDatabaseMock.Create( schemaBuilder.Database );
        var table = db.Schemas.Default.Objects.GetTable( "T" );
        var sut = table.Constraints;

        var result = sut.GetIndex( indexBuilder.Name );

        Assertion.All(
                result.Type.TestEquals( SqlObjectType.Index ),
                result.Name.TestEquals( indexBuilder.Name ) )
            .Go();
    }

    [Fact]
    public void Constraints_GetIndex_ShouldThrowKeyNotFoundException_WhenConstraintDoesNotExist()
    {
        var schemaBuilder = PostgreSqlDatabaseBuilderMock.Create().Schemas.Default;
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.Constraints.CreateIndex( tableBuilder.Columns.Create( "C1" ).Asc(), tableBuilder.Columns.Create( "C2" ).Desc() );
        tableBuilder.Constraints.SetPrimaryKey( tableBuilder.Columns.Create( "C3" ).Asc() );

        var db = PostgreSqlDatabaseMock.Create( schemaBuilder.Database );
        var table = db.Schemas.Default.Objects.GetTable( "T" );
        var sut = table.Constraints;

        var action = Lambda.Of( () => sut.GetIndex( "foo" ) );

        action.Test( exc => exc.TestType().Exact<KeyNotFoundException>() ).Go();
    }

    [Fact]
    public void Constraints_GetIndex_ShouldThrowSqlObjectCastException_WhenConstraintExistsButIsNotIndex()
    {
        var schemaBuilder = PostgreSqlDatabaseBuilderMock.Create().Schemas.Default;
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.Constraints.CreateIndex( tableBuilder.Columns.Create( "C1" ).Asc(), tableBuilder.Columns.Create( "C2" ).Desc() );
        tableBuilder.Constraints.SetPrimaryKey( tableBuilder.Columns.Create( "C3" ).Asc() );

        var db = PostgreSqlDatabaseMock.Create( schemaBuilder.Database );
        var table = db.Schemas.Default.Objects.GetTable( "T" );
        var sut = table.Constraints;

        var action = Lambda.Of( () => sut.GetIndex( "PK_T" ) );

        action.Test( exc => exc.TestType().Exact<SqlObjectCastException>() ).Go();
    }

    [Fact]
    public void Constraints_TryGetIndex_ShouldReturnCorrectIndex()
    {
        var schemaBuilder = PostgreSqlDatabaseBuilderMock.Create().Schemas.Default;
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        var indexBuilder = tableBuilder.Constraints.CreateIndex(
            tableBuilder.Columns.Create( "C1" ).Asc(),
            tableBuilder.Columns.Create( "C2" ).Desc() );

        tableBuilder.Constraints.SetPrimaryKey( tableBuilder.Columns.Create( "C3" ).Asc() );

        var db = PostgreSqlDatabaseMock.Create( schemaBuilder.Database );
        var table = db.Schemas.Default.Objects.GetTable( "T" );
        var sut = table.Constraints;

        var result = sut.TryGetIndex( indexBuilder.Name );

        Assertion.All(
                result.TestNotNull(),
                (result?.Type).TestEquals( SqlObjectType.Index ),
                (result?.Name).TestEquals( indexBuilder.Name ) )
            .Go();
    }

    [Fact]
    public void Constraints_TryGetIndex_ShouldReturnNull_WhenConstraintDoesNotExist()
    {
        var schemaBuilder = PostgreSqlDatabaseBuilderMock.Create().Schemas.Default;
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.Constraints.CreateIndex( tableBuilder.Columns.Create( "C1" ).Asc(), tableBuilder.Columns.Create( "C2" ).Desc() );
        tableBuilder.Constraints.SetPrimaryKey( tableBuilder.Columns.Create( "C3" ).Asc() );

        var db = PostgreSqlDatabaseMock.Create( schemaBuilder.Database );
        var table = db.Schemas.Default.Objects.GetTable( "T" );
        var sut = table.Constraints;

        var result = sut.TryGetIndex( "foo" );

        result.TestNull().Go();
    }

    [Fact]
    public void Constraints_TryGetIndex_ShouldReturnNull_WhenConstraintExistsButIsNotIndex()
    {
        var schemaBuilder = PostgreSqlDatabaseBuilderMock.Create().Schemas.Default;
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.Constraints.CreateIndex( tableBuilder.Columns.Create( "C1" ).Asc(), tableBuilder.Columns.Create( "C2" ).Desc() );
        tableBuilder.Constraints.SetPrimaryKey( tableBuilder.Columns.Create( "C3" ).Asc() );

        var db = PostgreSqlDatabaseMock.Create( schemaBuilder.Database );
        var table = db.Schemas.Default.Objects.GetTable( "T" );
        var sut = table.Constraints;

        var result = sut.TryGetIndex( "PK_T" );

        result.TestNull().Go();
    }

    [Fact]
    public void Constraints_GetForeignKey_ShouldReturnCorrectForeignKey()
    {
        var schemaBuilder = PostgreSqlDatabaseBuilderMock.Create().Schemas.Default;
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        var indexBuilder1 = tableBuilder.Constraints.CreateIndex( tableBuilder.Columns.Create( "C1" ).Asc() );
        var indexBuilder2 = tableBuilder.Constraints.SetPrimaryKey( tableBuilder.Columns.Create( "C3" ).Asc() ).Index;
        var fk = tableBuilder.Constraints.CreateForeignKey( indexBuilder1, indexBuilder2 );

        var db = PostgreSqlDatabaseMock.Create( schemaBuilder.Database );
        var table = db.Schemas.Default.Objects.GetTable( "T" );
        var sut = table.Constraints;

        var result = sut.GetForeignKey( fk.Name );

        Assertion.All(
                result.Type.TestEquals( SqlObjectType.ForeignKey ),
                result.Name.TestEquals( fk.Name ) )
            .Go();
    }

    [Fact]
    public void Constraints_GetForeignKey_ShouldThrowKeyNotFoundException_WhenConstraintDoesNotExist()
    {
        var schemaBuilder = PostgreSqlDatabaseBuilderMock.Create().Schemas.Default;
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.Constraints.CreateIndex( tableBuilder.Columns.Create( "C1" ).Asc(), tableBuilder.Columns.Create( "C2" ).Desc() );
        tableBuilder.Constraints.SetPrimaryKey( tableBuilder.Columns.Create( "C3" ).Asc() );

        var db = PostgreSqlDatabaseMock.Create( schemaBuilder.Database );
        var table = db.Schemas.Default.Objects.GetTable( "T" );
        var sut = table.Constraints;

        var action = Lambda.Of( () => sut.GetForeignKey( "foo" ) );

        action.Test( exc => exc.TestType().Exact<KeyNotFoundException>() ).Go();
    }

    [Fact]
    public void Constraints_GetForeignKey_ShouldThrowSqlObjectCastException_WhenConstraintExistsButIsNotForeignKey()
    {
        var schemaBuilder = PostgreSqlDatabaseBuilderMock.Create().Schemas.Default;
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.Constraints.CreateIndex( tableBuilder.Columns.Create( "C1" ).Asc(), tableBuilder.Columns.Create( "C2" ).Desc() );
        tableBuilder.Constraints.SetPrimaryKey( tableBuilder.Columns.Create( "C3" ).Asc() );

        var db = PostgreSqlDatabaseMock.Create( schemaBuilder.Database );
        var table = db.Schemas.Default.Objects.GetTable( "T" );
        var sut = table.Constraints;

        var action = Lambda.Of( () => sut.GetForeignKey( "PK_T" ) );

        action.Test( exc => exc.TestType().Exact<SqlObjectCastException>() ).Go();
    }

    [Fact]
    public void Constraints_TryGetForeignKey_ShouldReturnCorrectForeignKey()
    {
        var schemaBuilder = PostgreSqlDatabaseBuilderMock.Create().Schemas.Default;
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        var indexBuilder1 = tableBuilder.Constraints.CreateIndex( tableBuilder.Columns.Create( "C1" ).Asc() );
        var indexBuilder2 = tableBuilder.Constraints.SetPrimaryKey( tableBuilder.Columns.Create( "C3" ).Asc() ).Index;
        var fk = tableBuilder.Constraints.CreateForeignKey( indexBuilder1, indexBuilder2 );

        var db = PostgreSqlDatabaseMock.Create( schemaBuilder.Database );
        var table = db.Schemas.Default.Objects.GetTable( "T" );
        var sut = table.Constraints;

        var result = sut.TryGetForeignKey( fk.Name );

        Assertion.All(
                result.TestNotNull(),
                (result?.Type).TestEquals( SqlObjectType.ForeignKey ),
                (result?.Name).TestEquals( fk.Name ) )
            .Go();
    }

    [Fact]
    public void Constraints_TryGetForeignKey_ShouldReturnNull_WhenConstraintDoesNotExist()
    {
        var schemaBuilder = PostgreSqlDatabaseBuilderMock.Create().Schemas.Default;
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.Constraints.CreateIndex( tableBuilder.Columns.Create( "C1" ).Asc(), tableBuilder.Columns.Create( "C2" ).Desc() );
        tableBuilder.Constraints.SetPrimaryKey( tableBuilder.Columns.Create( "C3" ).Asc() );

        var db = PostgreSqlDatabaseMock.Create( schemaBuilder.Database );
        var table = db.Schemas.Default.Objects.GetTable( "T" );
        var sut = table.Constraints;

        var result = sut.TryGetForeignKey( "foo" );

        result.TestNull().Go();
    }

    [Fact]
    public void Constraints_TryGetForeignKey_ShouldReturnNull_WhenConstraintExistsButIsNotForeignKey()
    {
        var schemaBuilder = PostgreSqlDatabaseBuilderMock.Create().Schemas.Default;
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.Constraints.CreateIndex( tableBuilder.Columns.Create( "C1" ).Asc(), tableBuilder.Columns.Create( "C2" ).Desc() );
        tableBuilder.Constraints.SetPrimaryKey( tableBuilder.Columns.Create( "C3" ).Asc() );

        var db = PostgreSqlDatabaseMock.Create( schemaBuilder.Database );
        var table = db.Schemas.Default.Objects.GetTable( "T" );
        var sut = table.Constraints;

        var result = sut.TryGetForeignKey( "PK_T" );

        result.TestNull().Go();
    }

    [Fact]
    public void Constraints_GetCheck_ShouldReturnCorrectCheck()
    {
        var schemaBuilder = PostgreSqlDatabaseBuilderMock.Create().Schemas.Default;
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.Constraints.SetPrimaryKey( tableBuilder.Columns.Create( "C1" ).Asc() );
        var chk = tableBuilder.Constraints.CreateCheck( SqlNode.True() );

        var db = PostgreSqlDatabaseMock.Create( schemaBuilder.Database );
        var table = db.Schemas.Default.Objects.GetTable( "T" );
        var sut = table.Constraints;

        var result = sut.GetCheck( chk.Name );

        Assertion.All(
                result.Type.TestEquals( SqlObjectType.Check ),
                result.Name.TestEquals( chk.Name ) )
            .Go();
    }

    [Fact]
    public void Constraints_GetCheck_ShouldThrowKeyNotFoundException_WhenConstraintDoesNotExist()
    {
        var schemaBuilder = PostgreSqlDatabaseBuilderMock.Create().Schemas.Default;
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.Constraints.CreateIndex( tableBuilder.Columns.Create( "C1" ).Asc(), tableBuilder.Columns.Create( "C2" ).Desc() );
        tableBuilder.Constraints.SetPrimaryKey( tableBuilder.Columns.Create( "C3" ).Asc() );

        var db = PostgreSqlDatabaseMock.Create( schemaBuilder.Database );
        var table = db.Schemas.Default.Objects.GetTable( "T" );
        var sut = table.Constraints;

        var action = Lambda.Of( () => sut.GetCheck( "foo" ) );

        action.Test( exc => exc.TestType().Exact<KeyNotFoundException>() ).Go();
    }

    [Fact]
    public void Constraints_GetCheck_ShouldThrowSqlObjectCastException_WhenConstraintExistsButIsNotCheck()
    {
        var schemaBuilder = PostgreSqlDatabaseBuilderMock.Create().Schemas.Default;
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.Constraints.CreateIndex( tableBuilder.Columns.Create( "C1" ).Asc(), tableBuilder.Columns.Create( "C2" ).Desc() );
        tableBuilder.Constraints.SetPrimaryKey( tableBuilder.Columns.Create( "C3" ).Asc() );

        var db = PostgreSqlDatabaseMock.Create( schemaBuilder.Database );
        var table = db.Schemas.Default.Objects.GetTable( "T" );
        var sut = table.Constraints;

        var action = Lambda.Of( () => sut.GetCheck( "PK_T" ) );

        action.Test( exc => exc.TestType().Exact<SqlObjectCastException>() ).Go();
    }

    [Fact]
    public void Constraints_TryGetCheck_ShouldReturnCorrectCheck()
    {
        var schemaBuilder = PostgreSqlDatabaseBuilderMock.Create().Schemas.Default;
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.Constraints.SetPrimaryKey( tableBuilder.Columns.Create( "C1" ).Asc() );
        var chk = tableBuilder.Constraints.CreateCheck( SqlNode.True() );

        var db = PostgreSqlDatabaseMock.Create( schemaBuilder.Database );
        var table = db.Schemas.Default.Objects.GetTable( "T" );
        var sut = table.Constraints;

        var result = sut.TryGetCheck( chk.Name );

        Assertion.All(
                result.TestNotNull(),
                (result?.Type).TestEquals( SqlObjectType.Check ),
                (result?.Name).TestEquals( chk.Name ) )
            .Go();
    }

    [Fact]
    public void Constraints_TryGetCheck_ShouldReturnNull_WhenConstraintDoesNotExist()
    {
        var schemaBuilder = PostgreSqlDatabaseBuilderMock.Create().Schemas.Default;
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.Constraints.CreateIndex( tableBuilder.Columns.Create( "C1" ).Asc(), tableBuilder.Columns.Create( "C2" ).Desc() );
        tableBuilder.Constraints.SetPrimaryKey( tableBuilder.Columns.Create( "C3" ).Asc() );

        var db = PostgreSqlDatabaseMock.Create( schemaBuilder.Database );
        var table = db.Schemas.Default.Objects.GetTable( "T" );
        var sut = table.Constraints;

        var result = sut.TryGetCheck( "foo" );

        result.TestNull().Go();
    }

    [Fact]
    public void Constraints_TryGetCheck_ShouldReturnNull_WhenConstraintExistsButIsNotCheck()
    {
        var schemaBuilder = PostgreSqlDatabaseBuilderMock.Create().Schemas.Default;
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.Constraints.CreateIndex( tableBuilder.Columns.Create( "C1" ).Asc(), tableBuilder.Columns.Create( "C2" ).Desc() );
        tableBuilder.Constraints.SetPrimaryKey( tableBuilder.Columns.Create( "C3" ).Asc() );

        var db = PostgreSqlDatabaseMock.Create( schemaBuilder.Database );
        var table = db.Schemas.Default.Objects.GetTable( "T" );
        var sut = table.Constraints;

        var result = sut.TryGetCheck( "PK_T" );

        result.TestNull().Go();
    }
}
