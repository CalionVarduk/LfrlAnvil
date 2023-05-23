using System.Collections.Generic;
using LfrlAnvil.Functional;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Extensions;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.Sqlite.Exceptions;
using LfrlAnvil.Sqlite.Extensions;
using LfrlAnvil.Sqlite.Objects.Builders;
using LfrlAnvil.Sqlite.Tests.Helpers;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.Sqlite.Tests.ObjectsTests;

public class SqliteTableTests : TestsBase
{
    [Fact]
    public void Properties_ShouldBeCorrectlyCopiedFromBuilder()
    {
        var schemaBuilder = new SqliteDatabaseBuilder().Schemas.Create( "foo" );
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.SetPrimaryKey( tableBuilder.Columns.Create( "C1" ).Asc() );

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
            sut.FullName.Should().Be( "foo_T" );
            sut.ToString().Should().Be( "[Table] foo_T" );

            sut.Columns.Count.Should().Be( 1 );
            sut.Columns.Table.Should().BeSameAs( sut );
            sut.Columns.Should().BeSequentiallyEqualTo( c1 );

            sut.Indexes.Count.Should().Be( 1 );
            sut.Indexes.Table.Should().BeSameAs( sut );
            sut.Indexes.Should().BeSequentiallyEqualTo( sut.PrimaryKey.Index );

            sut.ForeignKeys.Count.Should().Be( 0 );
            sut.ForeignKeys.Table.Should().BeSameAs( sut );
            sut.ForeignKeys.Should().BeEmpty();
        }
    }

    [Fact]
    public void Creation_ShouldThrowSqliteObjectBuilderException_WhenBuilderPrimaryKeyIsNull()
    {
        var schemaBuilder = new SqliteDatabaseBuilder().Schemas.Create( "foo" );
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
        var schemaBuilder = new SqliteDatabaseBuilder().Schemas.Default;
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.Columns.Create( "C1" );
        tableBuilder.SetPrimaryKey( tableBuilder.Columns.Create( "C2" ).Asc() );

        var db = new SqliteDatabaseMock( schemaBuilder.Database );
        ISqlColumnCollection sut = db.Schemas.Default.Objects.GetTable( "T" ).Columns;

        var result = sut.Contains( name );

        result.Should().Be( expected );
    }

    [Fact]
    public void Columns_Get_ShouldReturnCorrectColumn()
    {
        var schemaBuilder = new SqliteDatabaseBuilder().Schemas.Default;
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.Columns.Create( "C1" );
        tableBuilder.SetPrimaryKey( tableBuilder.Columns.Create( "C2" ).Asc() );

        var db = new SqliteDatabaseMock( schemaBuilder.Database );
        ISqlColumnCollection sut = db.Schemas.Default.Objects.GetTable( "T" ).Columns;

        var result = sut.Get( "C2" );

        result.Should().BeSameAs( sut.Table.PrimaryKey.Index.Columns.Span[0].Column );
    }

    [Fact]
    public void Columns_Get_ShouldThrowKeyNotFoundException_WhenColumnDoesNotExist()
    {
        var schemaBuilder = new SqliteDatabaseBuilder().Schemas.Default;
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.SetPrimaryKey( tableBuilder.Columns.Create( "C1" ).Asc() );

        var db = new SqliteDatabaseMock( schemaBuilder.Database );
        ISqlColumnCollection sut = db.Schemas.Default.Objects.GetTable( "T" ).Columns;

        var action = Lambda.Of( () => sut.Get( "C2" ) );

        action.Should().ThrowExactly<KeyNotFoundException>();
    }

    [Fact]
    public void Columns_TryGet_ShouldReturnCorrectColumn()
    {
        var schemaBuilder = new SqliteDatabaseBuilder().Schemas.Default;
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.Columns.Create( "C1" );
        tableBuilder.SetPrimaryKey( tableBuilder.Columns.Create( "C2" ).Asc() );

        var db = new SqliteDatabaseMock( schemaBuilder.Database );
        ISqlColumnCollection sut = db.Schemas.Default.Objects.GetTable( "T" ).Columns;

        var result = sut.TryGet( "C2", out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            outResult.Should().BeSameAs( sut.Table.PrimaryKey.Index.Columns.Span[0].Column );
        }
    }

    [Fact]
    public void Columns_TryGet_ShouldReturnFalse_WhenColumnDoesNotExist()
    {
        var schemaBuilder = new SqliteDatabaseBuilder().Schemas.Default;
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.SetPrimaryKey( tableBuilder.Columns.Create( "C1" ).Asc() );

        var db = new SqliteDatabaseMock( schemaBuilder.Database );
        ISqlColumnCollection sut = db.Schemas.Default.Objects.GetTable( "T" ).Columns;

        var result = sut.TryGet( "C2", out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().BeFalse();
            outResult.Should().BeNull();
        }
    }

    [Fact]
    public void Indexes_Contains_ShouldReturnTrue_WhenIndexExists()
    {
        var schemaBuilder = new SqliteDatabaseBuilder().Schemas.Default;
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.Indexes.Create( tableBuilder.Columns.Create( "C1" ).Asc(), tableBuilder.Columns.Create( "C2" ).Desc() );
        tableBuilder.SetPrimaryKey( tableBuilder.Columns.Create( "C3" ).Asc() );

        var db = new SqliteDatabaseMock( schemaBuilder.Database );
        var table = db.Schemas.Default.Objects.GetTable( "T" );
        ISqlColumn column1 = table.Columns.Get( "C1" );
        ISqlColumn column2 = table.Columns.Get( "C2" );
        ISqlIndexCollection sut = table.Indexes;

        var result = sut.Contains( column1.Asc(), column2.Desc() );

        result.Should().BeTrue();
    }

    [Fact]
    public void Indexes_Contains_ShouldReturnFalse_WhenIndexDoesNotExist()
    {
        var schemaBuilder = new SqliteDatabaseBuilder().Schemas.Default;
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.Indexes.Create( tableBuilder.Columns.Create( "C1" ).Asc(), tableBuilder.Columns.Create( "C2" ).Desc() );
        tableBuilder.SetPrimaryKey( tableBuilder.Columns.Create( "C3" ).Asc() );

        var db = new SqliteDatabaseMock( schemaBuilder.Database );
        var table = db.Schemas.Default.Objects.GetTable( "T" );
        ISqlColumn column1 = table.Columns.Get( "C1" );
        ISqlColumn column2 = table.Columns.Get( "C2" );
        ISqlIndexCollection sut = table.Indexes;

        var result = sut.Contains( column1.Asc(), column2.Asc() );

        result.Should().BeFalse();
    }

    [Fact]
    public void Indexes_Get_ShouldReturnCorrectIndex()
    {
        var schemaBuilder = new SqliteDatabaseBuilder().Schemas.Default;
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.Indexes.Create( tableBuilder.Columns.Create( "C1" ).Asc(), tableBuilder.Columns.Create( "C2" ).Desc() );
        tableBuilder.SetPrimaryKey( tableBuilder.Columns.Create( "C3" ).Asc() );

        var db = new SqliteDatabaseMock( schemaBuilder.Database );
        var table = db.Schemas.Default.Objects.GetTable( "T" );
        ISqlColumn column1 = table.Columns.Get( "C1" );
        ISqlColumn column2 = table.Columns.Get( "C2" );
        ISqlIndexCollection sut = table.Indexes;

        var result = sut.Get( column1.Asc(), column2.Desc() );

        result.Columns.ToArray().Should().BeSequentiallyEqualTo( column1.Asc(), column2.Desc() );
    }

    [Fact]
    public void Indexes_Get_ShouldThrowKeyNotFoundException_WhenIndexDoesNotExist()
    {
        var schemaBuilder = new SqliteDatabaseBuilder().Schemas.Default;
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.Indexes.Create( tableBuilder.Columns.Create( "C1" ).Asc(), tableBuilder.Columns.Create( "C2" ).Desc() );
        tableBuilder.SetPrimaryKey( tableBuilder.Columns.Create( "C3" ).Asc() );

        var db = new SqliteDatabaseMock( schemaBuilder.Database );
        var table = db.Schemas.Default.Objects.GetTable( "T" );
        ISqlColumn column1 = table.Columns.Get( "C1" );
        ISqlColumn column2 = table.Columns.Get( "C2" );
        ISqlIndexCollection sut = table.Indexes;

        var action = Lambda.Of( () => sut.Get( column1.Asc(), column2.Asc() ) );

        action.Should().ThrowExactly<KeyNotFoundException>();
    }

    [Fact]
    public void Indexes_TryGet_ShouldReturnCorrectIndex()
    {
        var schemaBuilder = new SqliteDatabaseBuilder().Schemas.Default;
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.Indexes.Create( tableBuilder.Columns.Create( "C1" ).Asc(), tableBuilder.Columns.Create( "C2" ).Desc() );
        tableBuilder.SetPrimaryKey( tableBuilder.Columns.Create( "C3" ).Asc() );

        var db = new SqliteDatabaseMock( schemaBuilder.Database );
        var table = db.Schemas.Default.Objects.GetTable( "T" );
        ISqlColumn column1 = table.Columns.Get( "C1" );
        ISqlColumn column2 = table.Columns.Get( "C2" );
        ISqlIndexCollection sut = table.Indexes;

        var result = sut.TryGet( new[] { column1.Asc(), column2.Desc() }, out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            (outResult?.Columns.ToArray()).Should().BeSequentiallyEqualTo( column1.Asc(), column2.Desc() );
        }
    }

    [Fact]
    public void Indexes_TryGet_ShouldReturnFalse_WhenIndexDoesNotExist()
    {
        var schemaBuilder = new SqliteDatabaseBuilder().Schemas.Default;
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.Indexes.Create( tableBuilder.Columns.Create( "C1" ).Asc(), tableBuilder.Columns.Create( "C2" ).Desc() );
        tableBuilder.SetPrimaryKey( tableBuilder.Columns.Create( "C3" ).Asc() );

        var db = new SqliteDatabaseMock( schemaBuilder.Database );
        var table = db.Schemas.Default.Objects.GetTable( "T" );
        ISqlColumn column1 = table.Columns.Get( "C1" );
        ISqlColumn column2 = table.Columns.Get( "C2" );
        ISqlIndexCollection sut = table.Indexes;

        var result = sut.TryGet( new[] { column1.Asc(), column2.Asc() }, out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().BeFalse();
            outResult.Should().BeNull();
        }
    }

    [Fact]
    public void ForeignKeys_Contains_ShouldReturnTrue_WhenForeignKeyExists()
    {
        var schemaBuilder = new SqliteDatabaseBuilder().Schemas.Default;
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        var indexBuilder1 = tableBuilder.Indexes.Create( tableBuilder.Columns.Create( "C1" ).Asc() );
        var indexBuilder2 = tableBuilder.SetPrimaryKey( tableBuilder.Columns.Create( "C2" ).Asc() ).Index;
        tableBuilder.ForeignKeys.Create( indexBuilder1, indexBuilder2 );

        var db = new SqliteDatabaseMock( schemaBuilder.Database );
        var table = db.Schemas.Default.Objects.GetTable( "T" );
        var index1 = table.Indexes.Get( table.Columns.Get( "C1" ).Asc() );
        var index2 = table.Indexes.Get( table.Columns.Get( "C2" ).Asc() );
        ISqlForeignKeyCollection sut = table.ForeignKeys;

        var result = sut.Contains( index1, index2 );

        result.Should().BeTrue();
    }

    [Fact]
    public void ForeignKeys_Contains_ShouldReturnFalse_WhenForeignKeyDoesNotExist()
    {
        var schemaBuilder = new SqliteDatabaseBuilder().Schemas.Default;
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        var indexBuilder1 = tableBuilder.Indexes.Create( tableBuilder.Columns.Create( "C1" ).Asc() );
        var indexBuilder2 = tableBuilder.SetPrimaryKey( tableBuilder.Columns.Create( "C2" ).Asc() ).Index;
        tableBuilder.ForeignKeys.Create( indexBuilder1, indexBuilder2 );

        var db = new SqliteDatabaseMock( schemaBuilder.Database );
        var table = db.Schemas.Default.Objects.GetTable( "T" );
        var index1 = table.Indexes.Get( table.Columns.Get( "C1" ).Asc() );
        var index2 = table.Indexes.Get( table.Columns.Get( "C2" ).Asc() );
        ISqlForeignKeyCollection sut = table.ForeignKeys;

        var result = sut.Contains( index2, index1 );

        result.Should().BeFalse();
    }

    [Fact]
    public void ForeignKeys_Get_ShouldReturnCorrectForeignKey()
    {
        var schemaBuilder = new SqliteDatabaseBuilder().Schemas.Default;
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        var indexBuilder1 = tableBuilder.Indexes.Create( tableBuilder.Columns.Create( "C1" ).Asc() );
        var indexBuilder2 = tableBuilder.SetPrimaryKey( tableBuilder.Columns.Create( "C2" ).Asc() ).Index;
        tableBuilder.ForeignKeys.Create( indexBuilder1, indexBuilder2 );

        var db = new SqliteDatabaseMock( schemaBuilder.Database );
        var table = db.Schemas.Default.Objects.GetTable( "T" );
        var index1 = table.Indexes.Get( table.Columns.Get( "C1" ).Asc() );
        var index2 = table.Indexes.Get( table.Columns.Get( "C2" ).Asc() );
        ISqlForeignKeyCollection sut = table.ForeignKeys;

        var result = sut.Get( index1, index2 );

        using ( new AssertionScope() )
        {
            result.Index.Should().BeSameAs( index1 );
            result.ReferencedIndex.Should().BeSameAs( index2 );
        }
    }

    [Fact]
    public void ForeignKeys_Get_ShouldThrowKeyNotFoundException_WhenForeignKeyDoesNotExist()
    {
        var schemaBuilder = new SqliteDatabaseBuilder().Schemas.Default;
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        var indexBuilder1 = tableBuilder.Indexes.Create( tableBuilder.Columns.Create( "C1" ).Asc() );
        var indexBuilder2 = tableBuilder.SetPrimaryKey( tableBuilder.Columns.Create( "C2" ).Asc() ).Index;
        tableBuilder.ForeignKeys.Create( indexBuilder1, indexBuilder2 );

        var db = new SqliteDatabaseMock( schemaBuilder.Database );
        var table = db.Schemas.Default.Objects.GetTable( "T" );
        var index1 = table.Indexes.Get( table.Columns.Get( "C1" ).Asc() );
        var index2 = table.Indexes.Get( table.Columns.Get( "C2" ).Asc() );
        ISqlForeignKeyCollection sut = table.ForeignKeys;

        var action = Lambda.Of( () => sut.Get( index2, index1 ) );

        action.Should().ThrowExactly<KeyNotFoundException>();
    }

    [Fact]
    public void ForeignKeys_TryGet_ShouldReturnCorrectForeignKey()
    {
        var schemaBuilder = new SqliteDatabaseBuilder().Schemas.Default;
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        var indexBuilder1 = tableBuilder.Indexes.Create( tableBuilder.Columns.Create( "C1" ).Asc() );
        var indexBuilder2 = tableBuilder.SetPrimaryKey( tableBuilder.Columns.Create( "C2" ).Asc() ).Index;
        tableBuilder.ForeignKeys.Create( indexBuilder1, indexBuilder2 );

        var db = new SqliteDatabaseMock( schemaBuilder.Database );
        var table = db.Schemas.Default.Objects.GetTable( "T" );
        var index1 = table.Indexes.Get( table.Columns.Get( "C1" ).Asc() );
        var index2 = table.Indexes.Get( table.Columns.Get( "C2" ).Asc() );
        ISqlForeignKeyCollection sut = table.ForeignKeys;

        var result = sut.TryGet( index1, index2, out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            (outResult?.Index).Should().BeSameAs( index1 );
            (outResult?.ReferencedIndex).Should().BeSameAs( index2 );
        }
    }

    [Fact]
    public void ForeignKeys_TryGet_ShouldReturnFalse_WhenForeignKeyDoesNotExist()
    {
        var schemaBuilder = new SqliteDatabaseBuilder().Schemas.Default;
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        var indexBuilder1 = tableBuilder.Indexes.Create( tableBuilder.Columns.Create( "C1" ).Asc() );
        var indexBuilder2 = tableBuilder.SetPrimaryKey( tableBuilder.Columns.Create( "C2" ).Asc() ).Index;
        tableBuilder.ForeignKeys.Create( indexBuilder1, indexBuilder2 );

        var db = new SqliteDatabaseMock( schemaBuilder.Database );
        var table = db.Schemas.Default.Objects.GetTable( "T" );
        var index1 = table.Indexes.Get( table.Columns.Get( "C1" ).Asc() );
        var index2 = table.Indexes.Get( table.Columns.Get( "C2" ).Asc() );
        ISqlForeignKeyCollection sut = table.ForeignKeys;

        var result = sut.TryGet( index2, index1, out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().BeFalse();
            outResult.Should().BeNull();
        }
    }
}
