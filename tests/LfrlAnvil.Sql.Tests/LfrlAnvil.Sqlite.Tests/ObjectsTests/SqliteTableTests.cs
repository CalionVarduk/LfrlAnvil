using System.Collections.Generic;
using LfrlAnvil.Functional;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.Sqlite.Exceptions;
using LfrlAnvil.Sqlite.Extensions;
using LfrlAnvil.Sqlite.Tests.Helpers;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.Sqlite.Tests.ObjectsTests;

public class SqliteTableTests : TestsBase
{
    [Fact]
    public void Properties_ShouldBeCorrectlyCopiedFromBuilder()
    {
        var schemaBuilder = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.Constraints.SetPrimaryKey( tableBuilder.Columns.Create( "C1" ).Asc() );

        var db = new SqliteDatabaseMock( schemaBuilder.Database );
        var schema = db.Schemas.Get( "foo" );

        ISqlTable sut = schema.Objects.GetTable( "T" );
        var c1 = sut.Columns.Get( "C1" );

        using ( new AssertionScope() )
        {
            sut.Database.Should().BeSameAs( db );
            sut.Schema.Should().BeSameAs( schema );
            sut.Type.Should().Be( SqlObjectType.Table );
            sut.Name.Should().Be( "T" );
            sut.Info.Should().Be( tableBuilder.Info );
            sut.Node.Table.Should().BeSameAs( sut );
            sut.Node.Info.Should().Be( sut.Info );
            sut.Node.Alias.Should().BeNull();
            sut.Node.Identifier.Should().Be( sut.Info.Identifier );
            sut.Node.IsOptional.Should().BeFalse();
            sut.ToString().Should().Be( "[Table] foo_T" );

            sut.Columns.Count.Should().Be( 1 );
            sut.Columns.Table.Should().BeSameAs( sut );
            sut.Columns.Should().BeSequentiallyEqualTo( c1 );

            sut.Constraints.Count.Should().Be( 2 );
            sut.Constraints.Table.Should().BeSameAs( sut );
            sut.Constraints.PrimaryKey.Index.Table.Should().BeSameAs( sut );
            sut.Constraints.Should().BeEquivalentTo( sut.Constraints.PrimaryKey, sut.Constraints.PrimaryKey.Index );
        }
    }

    [Fact]
    public void Creation_ShouldThrowSqliteObjectBuilderException_WhenBuilderPrimaryKeyIsNull()
    {
        var schemaBuilder = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        schemaBuilder.Objects.CreateTable( "T" );

        var action = Lambda.Of( () => new SqliteDatabaseMock( schemaBuilder.Database ) );

        action.Should().ThrowExactly<SqliteObjectBuilderException>();
    }

    [Theory]
    [InlineData( "C1", true )]
    [InlineData( "C2", true )]
    [InlineData( "C3", false )]
    public void Columns_Contains_ShouldReturnTrue_WhenColumnExists(string name, bool expected)
    {
        var schemaBuilder = SqliteDatabaseBuilderMock.Create().Schemas.Default;
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.Columns.Create( "C1" );
        tableBuilder.Constraints.SetPrimaryKey( tableBuilder.Columns.Create( "C2" ).Asc() );

        var db = new SqliteDatabaseMock( schemaBuilder.Database );
        ISqlColumnCollection sut = db.Schemas.Default.Objects.GetTable( "T" ).Columns;

        var result = sut.Contains( name );

        result.Should().Be( expected );
    }

    [Fact]
    public void Columns_GetColumn_ShouldReturnCorrectColumn()
    {
        var schemaBuilder = SqliteDatabaseBuilderMock.Create().Schemas.Default;
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.Columns.Create( "C1" );
        tableBuilder.Constraints.SetPrimaryKey( tableBuilder.Columns.Create( "C2" ).Asc() );

        var db = new SqliteDatabaseMock( schemaBuilder.Database );
        ISqlColumnCollection sut = db.Schemas.Default.Objects.GetTable( "T" ).Columns;

        var result = sut.Get( "C2" );

        result.Should().BeSameAs( sut.Table.Constraints.PrimaryKey.Index.Columns[0].Column );
    }

    [Fact]
    public void Columns_GetColumn_ShouldThrowKeyNotFoundException_WhenColumnDoesNotExist()
    {
        var schemaBuilder = SqliteDatabaseBuilderMock.Create().Schemas.Default;
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.Constraints.SetPrimaryKey( tableBuilder.Columns.Create( "C1" ).Asc() );

        var db = new SqliteDatabaseMock( schemaBuilder.Database );
        ISqlColumnCollection sut = db.Schemas.Default.Objects.GetTable( "T" ).Columns;

        var action = Lambda.Of( () => sut.Get( "C2" ) );

        action.Should().ThrowExactly<KeyNotFoundException>();
    }

    [Fact]
    public void Columns_TryGetColumn_ShouldReturnCorrectColumn()
    {
        var schemaBuilder = SqliteDatabaseBuilderMock.Create().Schemas.Default;
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.Columns.Create( "C1" );
        tableBuilder.Constraints.SetPrimaryKey( tableBuilder.Columns.Create( "C2" ).Asc() );

        var db = new SqliteDatabaseMock( schemaBuilder.Database );
        ISqlColumnCollection sut = db.Schemas.Default.Objects.GetTable( "T" ).Columns;

        var result = sut.TryGet( "C2" );

        result.Should().BeSameAs( sut.Table.Constraints.PrimaryKey.Index.Columns[0].Column );
    }

    [Fact]
    public void Columns_TryGetColumn_ShouldReturnNull_WhenColumnDoesNotExist()
    {
        var schemaBuilder = SqliteDatabaseBuilderMock.Create().Schemas.Default;
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.Constraints.SetPrimaryKey( tableBuilder.Columns.Create( "C1" ).Asc() );

        var db = new SqliteDatabaseMock( schemaBuilder.Database );
        ISqlColumnCollection sut = db.Schemas.Default.Objects.GetTable( "T" ).Columns;

        var result = sut.TryGet( "C2" );

        result.Should().BeNull();
    }

    [Theory]
    [InlineData( "PK_T", true )]
    [InlineData( "UIX_T_C3A", true )]
    [InlineData( "C1", false )]
    public void Constraints_Contains_ShouldReturnTrue_WhenConstraintExists(string name, bool expected)
    {
        var schemaBuilder = SqliteDatabaseBuilderMock.Create().Schemas.Default;
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.Constraints.CreateIndex( tableBuilder.Columns.Create( "C1" ).Asc(), tableBuilder.Columns.Create( "C2" ).Desc() );
        tableBuilder.Constraints.SetPrimaryKey( tableBuilder.Columns.Create( "C3" ).Asc() );

        var db = new SqliteDatabaseMock( schemaBuilder.Database );
        var table = db.Schemas.Default.Objects.GetTable( "T" );
        ISqlConstraintCollection sut = table.Constraints;

        var result = sut.Contains( name );

        result.Should().Be( expected );
    }

    [Fact]
    public void Constraints_GetConstraint_ShouldReturnCorrectConstraint()
    {
        var schemaBuilder = SqliteDatabaseBuilderMock.Create().Schemas.Default;
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.Constraints.CreateIndex( tableBuilder.Columns.Create( "C1" ).Asc(), tableBuilder.Columns.Create( "C2" ).Desc() );
        tableBuilder.Constraints.SetPrimaryKey( tableBuilder.Columns.Create( "C3" ).Asc() );

        var db = new SqliteDatabaseMock( schemaBuilder.Database );
        var table = db.Schemas.Default.Objects.GetTable( "T" );
        ISqlConstraintCollection sut = table.Constraints;

        var result = sut.Get( "PK_T" );

        using ( new AssertionScope() )
        {
            result.Type.Should().Be( SqlObjectType.PrimaryKey );
            result.Name.Should().Be( "PK_T" );
        }
    }

    [Fact]
    public void Constraints_GetConstraint_ShouldThrowKeyNotFoundException_WhenConstraintDoesNotExist()
    {
        var schemaBuilder = SqliteDatabaseBuilderMock.Create().Schemas.Default;
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.Constraints.CreateIndex( tableBuilder.Columns.Create( "C1" ).Asc(), tableBuilder.Columns.Create( "C2" ).Desc() );
        tableBuilder.Constraints.SetPrimaryKey( tableBuilder.Columns.Create( "C3" ).Asc() );

        var db = new SqliteDatabaseMock( schemaBuilder.Database );
        var table = db.Schemas.Default.Objects.GetTable( "T" );
        ISqlConstraintCollection sut = table.Constraints;

        var action = Lambda.Of( () => sut.Get( "foo" ) );

        action.Should().ThrowExactly<KeyNotFoundException>();
    }

    [Fact]
    public void Constraints_TryGetConstraint_ShouldReturnCorrectConstraint()
    {
        var schemaBuilder = SqliteDatabaseBuilderMock.Create().Schemas.Default;
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.Constraints.CreateIndex( tableBuilder.Columns.Create( "C1" ).Asc(), tableBuilder.Columns.Create( "C2" ).Desc() );
        tableBuilder.Constraints.SetPrimaryKey( tableBuilder.Columns.Create( "C3" ).Asc() );

        var db = new SqliteDatabaseMock( schemaBuilder.Database );
        var table = db.Schemas.Default.Objects.GetTable( "T" );
        ISqlConstraintCollection sut = table.Constraints;

        var result = sut.TryGet( "PK_T" );

        using ( new AssertionScope() )
        {
            result.Should().NotBeNull();
            (result?.Type).Should().Be( SqlObjectType.PrimaryKey );
            (result?.Name).Should().Be( "PK_T" );
        }
    }

    [Fact]
    public void Constraints_TryGetConstraint_ShouldReturnNull_WhenConstraintDoesNotExist()
    {
        var schemaBuilder = SqliteDatabaseBuilderMock.Create().Schemas.Default;
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.Constraints.CreateIndex( tableBuilder.Columns.Create( "C1" ).Asc(), tableBuilder.Columns.Create( "C2" ).Desc() );
        tableBuilder.Constraints.SetPrimaryKey( tableBuilder.Columns.Create( "C3" ).Asc() );

        var db = new SqliteDatabaseMock( schemaBuilder.Database );
        var table = db.Schemas.Default.Objects.GetTable( "T" );
        ISqlConstraintCollection sut = table.Constraints;

        var result = sut.TryGet( "foo" );

        result.Should().BeNull();
    }

    [Fact]
    public void Constraints_GetIndex_ShouldReturnCorrectIndex()
    {
        var schemaBuilder = SqliteDatabaseBuilderMock.Create().Schemas.Default;
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        var indexBuilder = tableBuilder.Constraints.CreateIndex(
            tableBuilder.Columns.Create( "C1" ).Asc(),
            tableBuilder.Columns.Create( "C2" ).Desc() );

        tableBuilder.Constraints.SetPrimaryKey( tableBuilder.Columns.Create( "C3" ).Asc() );

        var db = new SqliteDatabaseMock( schemaBuilder.Database );
        var table = db.Schemas.Default.Objects.GetTable( "T" );
        ISqlConstraintCollection sut = table.Constraints;

        var result = sut.GetIndex( indexBuilder.Name );

        using ( new AssertionScope() )
        {
            result.Type.Should().Be( SqlObjectType.Index );
            result.Name.Should().Be( indexBuilder.Name );
        }
    }

    [Fact]
    public void Constraints_GetIndex_ShouldThrowKeyNotFoundException_WhenConstraintDoesNotExist()
    {
        var schemaBuilder = SqliteDatabaseBuilderMock.Create().Schemas.Default;
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.Constraints.CreateIndex( tableBuilder.Columns.Create( "C1" ).Asc(), tableBuilder.Columns.Create( "C2" ).Desc() );
        tableBuilder.Constraints.SetPrimaryKey( tableBuilder.Columns.Create( "C3" ).Asc() );

        var db = new SqliteDatabaseMock( schemaBuilder.Database );
        var table = db.Schemas.Default.Objects.GetTable( "T" );
        ISqlConstraintCollection sut = table.Constraints;

        var action = Lambda.Of( () => sut.GetIndex( "foo" ) );

        action.Should().ThrowExactly<KeyNotFoundException>();
    }

    [Fact]
    public void Constraints_GetIndex_ShouldThrowSqlObjectCastException_WhenConstraintExistsButIsNotIndex()
    {
        var schemaBuilder = SqliteDatabaseBuilderMock.Create().Schemas.Default;
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.Constraints.CreateIndex( tableBuilder.Columns.Create( "C1" ).Asc(), tableBuilder.Columns.Create( "C2" ).Desc() );
        tableBuilder.Constraints.SetPrimaryKey( tableBuilder.Columns.Create( "C3" ).Asc() );

        var db = new SqliteDatabaseMock( schemaBuilder.Database );
        var table = db.Schemas.Default.Objects.GetTable( "T" );
        ISqlConstraintCollection sut = table.Constraints;

        var action = Lambda.Of( () => sut.GetIndex( "PK_T" ) );

        action.Should().ThrowExactly<SqlObjectCastException>();
    }

    [Fact]
    public void Constraints_TryGetIndex_ShouldReturnCorrectIndex()
    {
        var schemaBuilder = SqliteDatabaseBuilderMock.Create().Schemas.Default;
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        var indexBuilder = tableBuilder.Constraints.CreateIndex(
            tableBuilder.Columns.Create( "C1" ).Asc(),
            tableBuilder.Columns.Create( "C2" ).Desc() );

        tableBuilder.Constraints.SetPrimaryKey( tableBuilder.Columns.Create( "C3" ).Asc() );

        var db = new SqliteDatabaseMock( schemaBuilder.Database );
        var table = db.Schemas.Default.Objects.GetTable( "T" );
        ISqlConstraintCollection sut = table.Constraints;

        var result = sut.TryGetIndex( indexBuilder.Name );

        using ( new AssertionScope() )
        {
            result.Should().NotBeNull();
            (result?.Type).Should().Be( SqlObjectType.Index );
            (result?.Name).Should().Be( indexBuilder.Name );
        }
    }

    [Fact]
    public void Constraints_TryGetIndex_ShouldReturnNull_WhenConstraintDoesNotExist()
    {
        var schemaBuilder = SqliteDatabaseBuilderMock.Create().Schemas.Default;
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.Constraints.CreateIndex( tableBuilder.Columns.Create( "C1" ).Asc(), tableBuilder.Columns.Create( "C2" ).Desc() );
        tableBuilder.Constraints.SetPrimaryKey( tableBuilder.Columns.Create( "C3" ).Asc() );

        var db = new SqliteDatabaseMock( schemaBuilder.Database );
        var table = db.Schemas.Default.Objects.GetTable( "T" );
        ISqlConstraintCollection sut = table.Constraints;

        var result = sut.TryGetIndex( "foo" );

        result.Should().BeNull();
    }

    [Fact]
    public void Constraints_TryGetIndex_ShouldReturnNull_WhenConstraintExistsButIsNotIndex()
    {
        var schemaBuilder = SqliteDatabaseBuilderMock.Create().Schemas.Default;
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.Constraints.CreateIndex( tableBuilder.Columns.Create( "C1" ).Asc(), tableBuilder.Columns.Create( "C2" ).Desc() );
        tableBuilder.Constraints.SetPrimaryKey( tableBuilder.Columns.Create( "C3" ).Asc() );

        var db = new SqliteDatabaseMock( schemaBuilder.Database );
        var table = db.Schemas.Default.Objects.GetTable( "T" );
        ISqlConstraintCollection sut = table.Constraints;

        var result = sut.TryGetIndex( "PK_T" );

        result.Should().BeNull();
    }

    [Fact]
    public void Constraints_GetForeignKey_ShouldReturnCorrectForeignKey()
    {
        var schemaBuilder = SqliteDatabaseBuilderMock.Create().Schemas.Default;
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        var indexBuilder1 = tableBuilder.Constraints.CreateIndex( tableBuilder.Columns.Create( "C1" ).Asc() );
        var indexBuilder2 = tableBuilder.Constraints.SetPrimaryKey( tableBuilder.Columns.Create( "C3" ).Asc() ).Index;
        var fk = tableBuilder.Constraints.CreateForeignKey( indexBuilder1, indexBuilder2 );

        var db = new SqliteDatabaseMock( schemaBuilder.Database );
        var table = db.Schemas.Default.Objects.GetTable( "T" );
        ISqlConstraintCollection sut = table.Constraints;

        var result = sut.GetForeignKey( fk.Name );

        using ( new AssertionScope() )
        {
            result.Type.Should().Be( SqlObjectType.ForeignKey );
            result.Name.Should().Be( fk.Name );
        }
    }

    [Fact]
    public void Constraints_GetForeignKey_ShouldThrowKeyNotFoundException_WhenConstraintDoesNotExist()
    {
        var schemaBuilder = SqliteDatabaseBuilderMock.Create().Schemas.Default;
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.Constraints.CreateIndex( tableBuilder.Columns.Create( "C1" ).Asc(), tableBuilder.Columns.Create( "C2" ).Desc() );
        tableBuilder.Constraints.SetPrimaryKey( tableBuilder.Columns.Create( "C3" ).Asc() );

        var db = new SqliteDatabaseMock( schemaBuilder.Database );
        var table = db.Schemas.Default.Objects.GetTable( "T" );
        ISqlConstraintCollection sut = table.Constraints;

        var action = Lambda.Of( () => sut.GetForeignKey( "foo" ) );

        action.Should().ThrowExactly<KeyNotFoundException>();
    }

    [Fact]
    public void Constraints_GetForeignKey_ShouldThrowSqlObjectCastException_WhenConstraintExistsButIsNotForeignKey()
    {
        var schemaBuilder = SqliteDatabaseBuilderMock.Create().Schemas.Default;
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.Constraints.CreateIndex( tableBuilder.Columns.Create( "C1" ).Asc(), tableBuilder.Columns.Create( "C2" ).Desc() );
        tableBuilder.Constraints.SetPrimaryKey( tableBuilder.Columns.Create( "C3" ).Asc() );

        var db = new SqliteDatabaseMock( schemaBuilder.Database );
        var table = db.Schemas.Default.Objects.GetTable( "T" );
        ISqlConstraintCollection sut = table.Constraints;

        var action = Lambda.Of( () => sut.GetForeignKey( "PK_T" ) );

        action.Should().ThrowExactly<SqlObjectCastException>();
    }

    [Fact]
    public void Constraints_TryGetForeignKey_ShouldReturnCorrectForeignKey()
    {
        var schemaBuilder = SqliteDatabaseBuilderMock.Create().Schemas.Default;
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        var indexBuilder1 = tableBuilder.Constraints.CreateIndex( tableBuilder.Columns.Create( "C1" ).Asc() );
        var indexBuilder2 = tableBuilder.Constraints.SetPrimaryKey( tableBuilder.Columns.Create( "C3" ).Asc() ).Index;
        var fk = tableBuilder.Constraints.CreateForeignKey( indexBuilder1, indexBuilder2 );

        var db = new SqliteDatabaseMock( schemaBuilder.Database );
        var table = db.Schemas.Default.Objects.GetTable( "T" );
        ISqlConstraintCollection sut = table.Constraints;

        var result = sut.TryGetForeignKey( fk.Name );

        using ( new AssertionScope() )
        {
            result.Should().NotBeNull();
            (result?.Type).Should().Be( SqlObjectType.ForeignKey );
            (result?.Name).Should().Be( fk.Name );
        }
    }

    [Fact]
    public void Constraints_TryGetForeignKey_ShouldReturnNull_WhenConstraintDoesNotExist()
    {
        var schemaBuilder = SqliteDatabaseBuilderMock.Create().Schemas.Default;
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.Constraints.CreateIndex( tableBuilder.Columns.Create( "C1" ).Asc(), tableBuilder.Columns.Create( "C2" ).Desc() );
        tableBuilder.Constraints.SetPrimaryKey( tableBuilder.Columns.Create( "C3" ).Asc() );

        var db = new SqliteDatabaseMock( schemaBuilder.Database );
        var table = db.Schemas.Default.Objects.GetTable( "T" );
        ISqlConstraintCollection sut = table.Constraints;

        var result = sut.TryGetForeignKey( "foo" );

        result.Should().BeNull();
    }

    [Fact]
    public void Constraints_TryGetForeignKey_ShouldReturnNull_WhenConstraintExistsButIsNotForeignKey()
    {
        var schemaBuilder = SqliteDatabaseBuilderMock.Create().Schemas.Default;
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.Constraints.CreateIndex( tableBuilder.Columns.Create( "C1" ).Asc(), tableBuilder.Columns.Create( "C2" ).Desc() );
        tableBuilder.Constraints.SetPrimaryKey( tableBuilder.Columns.Create( "C3" ).Asc() );

        var db = new SqliteDatabaseMock( schemaBuilder.Database );
        var table = db.Schemas.Default.Objects.GetTable( "T" );
        ISqlConstraintCollection sut = table.Constraints;

        var result = sut.TryGetForeignKey( "PK_T" );

        result.Should().BeNull();
    }

    [Fact]
    public void Constraints_GetCheck_ShouldReturnCorrectCheck()
    {
        var schemaBuilder = SqliteDatabaseBuilderMock.Create().Schemas.Default;
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.Constraints.SetPrimaryKey( tableBuilder.Columns.Create( "C1" ).Asc() );
        var chk = tableBuilder.Constraints.CreateCheck( SqlNode.True() );

        var db = new SqliteDatabaseMock( schemaBuilder.Database );
        var table = db.Schemas.Default.Objects.GetTable( "T" );
        ISqlConstraintCollection sut = table.Constraints;

        var result = sut.GetCheck( chk.Name );

        using ( new AssertionScope() )
        {
            result.Type.Should().Be( SqlObjectType.Check );
            result.Name.Should().Be( chk.Name );
        }
    }

    [Fact]
    public void Constraints_GetCheck_ShouldThrowKeyNotFoundException_WhenConstraintDoesNotExist()
    {
        var schemaBuilder = SqliteDatabaseBuilderMock.Create().Schemas.Default;
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.Constraints.CreateIndex( tableBuilder.Columns.Create( "C1" ).Asc(), tableBuilder.Columns.Create( "C2" ).Desc() );
        tableBuilder.Constraints.SetPrimaryKey( tableBuilder.Columns.Create( "C3" ).Asc() );

        var db = new SqliteDatabaseMock( schemaBuilder.Database );
        var table = db.Schemas.Default.Objects.GetTable( "T" );
        ISqlConstraintCollection sut = table.Constraints;

        var action = Lambda.Of( () => sut.GetCheck( "foo" ) );

        action.Should().ThrowExactly<KeyNotFoundException>();
    }

    [Fact]
    public void Constraints_GetCheck_ShouldThrowSqlObjectCastException_WhenConstraintExistsButIsNotCheck()
    {
        var schemaBuilder = SqliteDatabaseBuilderMock.Create().Schemas.Default;
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.Constraints.CreateIndex( tableBuilder.Columns.Create( "C1" ).Asc(), tableBuilder.Columns.Create( "C2" ).Desc() );
        tableBuilder.Constraints.SetPrimaryKey( tableBuilder.Columns.Create( "C3" ).Asc() );

        var db = new SqliteDatabaseMock( schemaBuilder.Database );
        var table = db.Schemas.Default.Objects.GetTable( "T" );
        ISqlConstraintCollection sut = table.Constraints;

        var action = Lambda.Of( () => sut.GetCheck( "PK_T" ) );

        action.Should().ThrowExactly<SqlObjectCastException>();
    }

    [Fact]
    public void Constraints_TryGetCheck_ShouldReturnCorrectCheck()
    {
        var schemaBuilder = SqliteDatabaseBuilderMock.Create().Schemas.Default;
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.Constraints.SetPrimaryKey( tableBuilder.Columns.Create( "C1" ).Asc() );
        var chk = tableBuilder.Constraints.CreateCheck( SqlNode.True() );

        var db = new SqliteDatabaseMock( schemaBuilder.Database );
        var table = db.Schemas.Default.Objects.GetTable( "T" );
        ISqlConstraintCollection sut = table.Constraints;

        var result = sut.TryGetCheck( chk.Name );

        using ( new AssertionScope() )
        {
            result.Should().NotBeNull();
            (result?.Type).Should().Be( SqlObjectType.Check );
            (result?.Name).Should().Be( chk.Name );
        }
    }

    [Fact]
    public void Constraints_TryGetCheck_ShouldReturnNull_WhenConstraintDoesNotExist()
    {
        var schemaBuilder = SqliteDatabaseBuilderMock.Create().Schemas.Default;
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.Constraints.CreateIndex( tableBuilder.Columns.Create( "C1" ).Asc(), tableBuilder.Columns.Create( "C2" ).Desc() );
        tableBuilder.Constraints.SetPrimaryKey( tableBuilder.Columns.Create( "C3" ).Asc() );

        var db = new SqliteDatabaseMock( schemaBuilder.Database );
        var table = db.Schemas.Default.Objects.GetTable( "T" );
        ISqlConstraintCollection sut = table.Constraints;

        var result = sut.TryGetCheck( "foo" );

        result.Should().BeNull();
    }

    [Fact]
    public void Constraints_TryGetCheck_ShouldReturnNull_WhenConstraintExistsButIsNotCheck()
    {
        var schemaBuilder = SqliteDatabaseBuilderMock.Create().Schemas.Default;
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.Constraints.CreateIndex( tableBuilder.Columns.Create( "C1" ).Asc(), tableBuilder.Columns.Create( "C2" ).Desc() );
        tableBuilder.Constraints.SetPrimaryKey( tableBuilder.Columns.Create( "C3" ).Asc() );

        var db = new SqliteDatabaseMock( schemaBuilder.Database );
        var table = db.Schemas.Default.Objects.GetTable( "T" );
        ISqlConstraintCollection sut = table.Constraints;

        var result = sut.TryGetCheck( "PK_T" );

        result.Should().BeNull();
    }
}
