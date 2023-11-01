using System.Collections.Generic;
using LfrlAnvil.Functional;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.Sqlite.Exceptions;
using LfrlAnvil.Sqlite.Extensions;
using LfrlAnvil.Sqlite.Tests.Helpers;

namespace LfrlAnvil.Sqlite.Tests.ObjectsTests;

public class SqliteSchemaTests : TestsBase
{
    [Fact]
    public void Properties_ShouldBeCorrectlyCopiedFromBuilder()
    {
        var schemaBuilder = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.SetPrimaryKey( tableBuilder.Columns.Create( "C1" ).Asc() );

        var db = new SqliteDatabaseMock( schemaBuilder.Database );

        ISqlSchema sut = db.Schemas.Get( "foo" );
        var table = sut.Objects.GetTable( "T" );

        using ( new AssertionScope() )
        {
            sut.Database.Should().BeSameAs( db );
            sut.Name.Should().Be( "foo" );
            sut.FullName.Should().Be( "foo" );
            sut.Type.Should().Be( SqlObjectType.Schema );

            sut.Objects.Schema.Should().BeSameAs( sut );
            sut.Objects.Count.Should().Be( 3 );
            sut.Objects.Should().BeEquivalentTo( table, table.PrimaryKey, table.PrimaryKey.Index );
        }
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
        var schemaBuilder = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        var indexBuilder1 = tableBuilder.Indexes.Create( tableBuilder.Columns.Create( "C2" ).Asc() );
        var indexBuilder2 = tableBuilder.SetPrimaryKey( tableBuilder.Columns.Create( "C1" ).Asc() ).Index;
        tableBuilder.ForeignKeys.Create( indexBuilder1, indexBuilder2 );

        var db = new SqliteDatabaseMock( schemaBuilder.Database );
        ISqlObjectCollection sut = db.Schemas.Get( "foo" ).Objects;

        var result = sut.Contains( name );

        result.Should().Be( expected );
    }

    [Fact]
    public void Objects_Get_ShouldReturnCorrectObject()
    {
        var schemaBuilder = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.SetPrimaryKey( tableBuilder.Columns.Create( "C1" ).Asc() );

        var db = new SqliteDatabaseMock( schemaBuilder.Database );
        ISqlObjectCollection sut = db.Schemas.Get( "foo" ).Objects;

        var result = sut.Get( "T" );

        using ( new AssertionScope() )
        {
            result.Type.Should().Be( SqlObjectType.Table );
            result.Name.Should().Be( "T" );
        }
    }

    [Fact]
    public void Objects_Get_ShouldThrowKeyNotFoundException_WhenObjectDoesNotExist()
    {
        var schemaBuilder = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.SetPrimaryKey( tableBuilder.Columns.Create( "C1" ).Asc() );

        var db = new SqliteDatabaseMock( schemaBuilder.Database );
        ISqlObjectCollection sut = db.Schemas.Get( "foo" ).Objects;

        var action = Lambda.Of( () => sut.Get( "U" ) );

        action.Should().ThrowExactly<KeyNotFoundException>();
    }

    [Fact]
    public void Objects_TryGet_ShouldReturnCorrectObject()
    {
        var schemaBuilder = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.SetPrimaryKey( tableBuilder.Columns.Create( "C1" ).Asc() );

        var db = new SqliteDatabaseMock( schemaBuilder.Database );
        ISqlObjectCollection sut = db.Schemas.Get( "foo" ).Objects;

        var result = sut.TryGet( "T", out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            (outResult?.Type).Should().Be( SqlObjectType.Table );
            (outResult?.Name).Should().Be( "T" );
        }
    }

    [Fact]
    public void Objects_TryGet_ShouldReturnFalse_WhenObjectDoesNotExist()
    {
        var schemaBuilder = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.SetPrimaryKey( tableBuilder.Columns.Create( "C1" ).Asc() );

        var db = new SqliteDatabaseMock( schemaBuilder.Database );
        ISqlObjectCollection sut = db.Schemas.Get( "foo" ).Objects;

        var result = sut.TryGet( "U", out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().BeFalse();
            outResult.Should().BeNull();
        }
    }

    [Fact]
    public void Objects_GetTable_ShouldReturnCorrectTable()
    {
        var schemaBuilder = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.SetPrimaryKey( tableBuilder.Columns.Create( "C1" ).Asc() );

        var db = new SqliteDatabaseMock( schemaBuilder.Database );
        ISqlObjectCollection sut = db.Schemas.Get( "foo" ).Objects;

        var result = sut.GetTable( "T" );

        using ( new AssertionScope() )
        {
            result.Type.Should().Be( SqlObjectType.Table );
            result.Name.Should().Be( "T" );
        }
    }

    [Fact]
    public void Objects_GetTable_ShouldThrowKeyNotFoundException_WhenObjectDoesNotExist()
    {
        var schemaBuilder = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.SetPrimaryKey( tableBuilder.Columns.Create( "C1" ).Asc() );

        var db = new SqliteDatabaseMock( schemaBuilder.Database );
        ISqlObjectCollection sut = db.Schemas.Get( "foo" ).Objects;

        var action = Lambda.Of( () => sut.GetTable( "U" ) );

        action.Should().ThrowExactly<KeyNotFoundException>();
    }

    [Fact]
    public void Objects_GetTable_ShouldThrowSqliteObjectCastException_WhenObjectExistsButIsNotTable()
    {
        var schemaBuilder = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.SetPrimaryKey( tableBuilder.Columns.Create( "C1" ).Asc() );

        var db = new SqliteDatabaseMock( schemaBuilder.Database );
        ISqlObjectCollection sut = db.Schemas.Get( "foo" ).Objects;

        var action = Lambda.Of( () => sut.GetTable( "PK_T" ) );

        action.Should().ThrowExactly<SqliteObjectCastException>();
    }

    [Fact]
    public void Objects_TryGetTable_ShouldReturnCorrectTable()
    {
        var schemaBuilder = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.SetPrimaryKey( tableBuilder.Columns.Create( "C1" ).Asc() );

        var db = new SqliteDatabaseMock( schemaBuilder.Database );
        var sut = db.Schemas.Get( "foo" ).Objects;

        var result = sut.TryGetTable( "T", out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            (outResult?.Type).Should().Be( SqlObjectType.Table );
            (outResult?.Name).Should().Be( "T" );
        }
    }

    [Fact]
    public void Objects_TryGetTable_ShouldReturnFalse_WhenObjectDoesNotExist()
    {
        var schemaBuilder = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.SetPrimaryKey( tableBuilder.Columns.Create( "C1" ).Asc() );

        var db = new SqliteDatabaseMock( schemaBuilder.Database );
        var sut = db.Schemas.Get( "foo" ).Objects;

        var result = sut.TryGetTable( "U", out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().BeFalse();
            outResult.Should().BeNull();
        }
    }

    [Fact]
    public void Objects_TryGetTable_ShouldReturnFalse_WhenObjectExistsButIsNotTable()
    {
        var schemaBuilder = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.SetPrimaryKey( tableBuilder.Columns.Create( "C1" ).Asc() );

        var db = new SqliteDatabaseMock( schemaBuilder.Database );
        var sut = db.Schemas.Get( "foo" ).Objects;

        var result = sut.TryGetTable( "PK_T", out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().BeFalse();
            outResult.Should().BeNull();
        }
    }

    [Fact]
    public void ISqlObjectCollection_TryGetTable_ShouldBeEquivalentToTryGetTable()
    {
        var schemaBuilder = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        var indexBuilder1 = tableBuilder.Indexes.Create( tableBuilder.Columns.Create( "C2" ).Asc() );
        var indexBuilder2 = tableBuilder.SetPrimaryKey( tableBuilder.Columns.Create( "C1" ).Asc() ).Index;
        tableBuilder.ForeignKeys.Create( indexBuilder1, indexBuilder2 );

        var db = new SqliteDatabaseMock( schemaBuilder.Database );
        var sut = db.Schemas.Get( "foo" ).Objects;
        var expected = sut.TryGetTable( "T", out var outExpected );

        var result = ((ISqlObjectCollection)sut).TryGetTable( "T", out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().Be( expected );
            outResult.Should().BeSameAs( outExpected );
        }
    }

    [Fact]
    public void Objects_GetPrimaryKey_ShouldReturnCorrectPrimaryKey()
    {
        var schemaBuilder = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.SetPrimaryKey( tableBuilder.Columns.Create( "C1" ).Asc() );

        var db = new SqliteDatabaseMock( schemaBuilder.Database );
        ISqlObjectCollection sut = db.Schemas.Get( "foo" ).Objects;

        var result = sut.GetPrimaryKey( "PK_T" );

        using ( new AssertionScope() )
        {
            result.Type.Should().Be( SqlObjectType.PrimaryKey );
            result.Name.Should().Be( "PK_T" );
        }
    }

    [Fact]
    public void Objects_GetPrimaryKey_ShouldThrowKeyNotFoundException_WhenObjectDoesNotExist()
    {
        var schemaBuilder = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.SetPrimaryKey( tableBuilder.Columns.Create( "C1" ).Asc() );

        var db = new SqliteDatabaseMock( schemaBuilder.Database );
        ISqlObjectCollection sut = db.Schemas.Get( "foo" ).Objects;

        var action = Lambda.Of( () => sut.GetPrimaryKey( "U" ) );

        action.Should().ThrowExactly<KeyNotFoundException>();
    }

    [Fact]
    public void Objects_GetPrimaryKey_ShouldThrowSqliteObjectCastException_WhenObjectExistsButIsNotPrimaryKey()
    {
        var schemaBuilder = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.SetPrimaryKey( tableBuilder.Columns.Create( "C1" ).Asc() );

        var db = new SqliteDatabaseMock( schemaBuilder.Database );
        ISqlObjectCollection sut = db.Schemas.Get( "foo" ).Objects;

        var action = Lambda.Of( () => sut.GetPrimaryKey( "T" ) );

        action.Should().ThrowExactly<SqliteObjectCastException>();
    }

    [Fact]
    public void Objects_TryGetPrimaryKey_ShouldReturnCorrectPrimaryKey()
    {
        var schemaBuilder = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.SetPrimaryKey( tableBuilder.Columns.Create( "C1" ).Asc() );

        var db = new SqliteDatabaseMock( schemaBuilder.Database );
        var sut = db.Schemas.Get( "foo" ).Objects;

        var result = sut.TryGetPrimaryKey( "PK_T", out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            (outResult?.Type).Should().Be( SqlObjectType.PrimaryKey );
            (outResult?.Name).Should().Be( "PK_T" );
        }
    }

    [Fact]
    public void Objects_TryGetPrimaryKey_ShouldReturnFalse_WhenObjectDoesNotExist()
    {
        var schemaBuilder = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.SetPrimaryKey( tableBuilder.Columns.Create( "C1" ).Asc() );

        var db = new SqliteDatabaseMock( schemaBuilder.Database );
        var sut = db.Schemas.Get( "foo" ).Objects;

        var result = sut.TryGetPrimaryKey( "U", out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().BeFalse();
            outResult.Should().BeNull();
        }
    }

    [Fact]
    public void Objects_TryGetPrimaryKey_ShouldReturnFalse_WhenObjectExistsButIsNotPrimaryKey()
    {
        var schemaBuilder = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.SetPrimaryKey( tableBuilder.Columns.Create( "C1" ).Asc() );

        var db = new SqliteDatabaseMock( schemaBuilder.Database );
        var sut = db.Schemas.Get( "foo" ).Objects;

        var result = sut.TryGetPrimaryKey( "T", out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().BeFalse();
            outResult.Should().BeNull();
        }
    }

    [Fact]
    public void ISqlObjectCollection_TryGetPrimaryKey_ShouldBeEquivalentToTryGetPrimaryKey()
    {
        var schemaBuilder = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        var indexBuilder1 = tableBuilder.Indexes.Create( tableBuilder.Columns.Create( "C2" ).Asc() );
        var indexBuilder2 = tableBuilder.SetPrimaryKey( tableBuilder.Columns.Create( "C1" ).Asc() ).Index;
        tableBuilder.ForeignKeys.Create( indexBuilder1, indexBuilder2 );

        var db = new SqliteDatabaseMock( schemaBuilder.Database );
        var sut = db.Schemas.Get( "foo" ).Objects;
        var expected = sut.TryGetPrimaryKey( "PK_T", out var outExpected );

        var result = ((ISqlObjectCollection)sut).TryGetPrimaryKey( "PK_T", out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().Be( expected );
            outResult.Should().BeSameAs( outExpected );
        }
    }

    [Fact]
    public void Objects_GetIndex_ShouldReturnCorrectIndex()
    {
        var schemaBuilder = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.SetPrimaryKey( tableBuilder.Columns.Create( "C1" ).Asc() );

        var db = new SqliteDatabaseMock( schemaBuilder.Database );
        ISqlObjectCollection sut = db.Schemas.Get( "foo" ).Objects;

        var result = sut.GetIndex( "UIX_T_C1A" );

        using ( new AssertionScope() )
        {
            result.Type.Should().Be( SqlObjectType.Index );
            result.Name.Should().Be( "UIX_T_C1A" );
        }
    }

    [Fact]
    public void Objects_GetIndex_ShouldThrowKeyNotFoundException_WhenObjectDoesNotExist()
    {
        var schemaBuilder = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.SetPrimaryKey( tableBuilder.Columns.Create( "C1" ).Asc() );

        var db = new SqliteDatabaseMock( schemaBuilder.Database );
        ISqlObjectCollection sut = db.Schemas.Get( "foo" ).Objects;

        var action = Lambda.Of( () => sut.GetIndex( "U" ) );

        action.Should().ThrowExactly<KeyNotFoundException>();
    }

    [Fact]
    public void Objects_GetIndex_ShouldThrowSqliteObjectCastException_WhenObjectExistsButIsNotIndex()
    {
        var schemaBuilder = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.SetPrimaryKey( tableBuilder.Columns.Create( "C1" ).Asc() );

        var db = new SqliteDatabaseMock( schemaBuilder.Database );
        ISqlObjectCollection sut = db.Schemas.Get( "foo" ).Objects;

        var action = Lambda.Of( () => sut.GetIndex( "T" ) );

        action.Should().ThrowExactly<SqliteObjectCastException>();
    }

    [Fact]
    public void Objects_TryGetIndex_ShouldReturnCorrectIndex()
    {
        var schemaBuilder = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.SetPrimaryKey( tableBuilder.Columns.Create( "C1" ).Asc() );

        var db = new SqliteDatabaseMock( schemaBuilder.Database );
        var sut = db.Schemas.Get( "foo" ).Objects;

        var result = sut.TryGetIndex( "UIX_T_C1A", out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            (outResult?.Type).Should().Be( SqlObjectType.Index );
            (outResult?.Name).Should().Be( "UIX_T_C1A" );
        }
    }

    [Fact]
    public void Objects_TryGetIndex_ShouldReturnFalse_WhenObjectDoesNotExist()
    {
        var schemaBuilder = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.SetPrimaryKey( tableBuilder.Columns.Create( "C1" ).Asc() );

        var db = new SqliteDatabaseMock( schemaBuilder.Database );
        var sut = db.Schemas.Get( "foo" ).Objects;

        var result = sut.TryGetIndex( "U", out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().BeFalse();
            outResult.Should().BeNull();
        }
    }

    [Fact]
    public void Objects_TryGetIndex_ShouldReturnFalse_WhenObjectExistsButIsNotIndex()
    {
        var schemaBuilder = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.SetPrimaryKey( tableBuilder.Columns.Create( "C1" ).Asc() );

        var db = new SqliteDatabaseMock( schemaBuilder.Database );
        var sut = db.Schemas.Get( "foo" ).Objects;

        var result = sut.TryGetIndex( "T", out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().BeFalse();
            outResult.Should().BeNull();
        }
    }

    [Fact]
    public void ISqlObjectCollection_TryGetIndex_ShouldBeEquivalentToTryGetIndex()
    {
        var schemaBuilder = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        var indexBuilder1 = tableBuilder.Indexes.Create( tableBuilder.Columns.Create( "C2" ).Asc() );
        var indexBuilder2 = tableBuilder.SetPrimaryKey( tableBuilder.Columns.Create( "C1" ).Asc() ).Index;
        tableBuilder.ForeignKeys.Create( indexBuilder1, indexBuilder2 );

        var db = new SqliteDatabaseMock( schemaBuilder.Database );
        var sut = db.Schemas.Get( "foo" ).Objects;
        var expected = sut.TryGetIndex( "UIX_T_C1A", out var outExpected );

        var result = ((ISqlObjectCollection)sut).TryGetIndex( "UIX_T_C1A", out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().Be( expected );
            outResult.Should().BeSameAs( outExpected );
        }
    }

    [Fact]
    public void Objects_GetForeignKey_ShouldReturnCorrectForeignKey()
    {
        var schemaBuilder = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        var indexBuilder1 = tableBuilder.Indexes.Create( tableBuilder.Columns.Create( "C2" ).Asc() );
        var indexBuilder2 = tableBuilder.SetPrimaryKey( tableBuilder.Columns.Create( "C1" ).Asc() ).Index;
        tableBuilder.ForeignKeys.Create( indexBuilder1, indexBuilder2 );

        var db = new SqliteDatabaseMock( schemaBuilder.Database );
        ISqlObjectCollection sut = db.Schemas.Get( "foo" ).Objects;

        var result = sut.GetForeignKey( "FK_T_C2_REF_T" );

        using ( new AssertionScope() )
        {
            result.Type.Should().Be( SqlObjectType.ForeignKey );
            result.Name.Should().Be( "FK_T_C2_REF_T" );
        }
    }

    [Fact]
    public void Objects_GetForeignKey_ShouldThrowKeyNotFoundException_WhenObjectDoesNotExist()
    {
        var schemaBuilder = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        var indexBuilder1 = tableBuilder.Indexes.Create( tableBuilder.Columns.Create( "C2" ).Asc() );
        var indexBuilder2 = tableBuilder.SetPrimaryKey( tableBuilder.Columns.Create( "C1" ).Asc() ).Index;
        tableBuilder.ForeignKeys.Create( indexBuilder1, indexBuilder2 );

        var db = new SqliteDatabaseMock( schemaBuilder.Database );
        ISqlObjectCollection sut = db.Schemas.Get( "foo" ).Objects;

        var action = Lambda.Of( () => sut.GetForeignKey( "U" ) );

        action.Should().ThrowExactly<KeyNotFoundException>();
    }

    [Fact]
    public void Objects_GetForeignKey_ShouldThrowSqliteObjectCastException_WhenObjectExistsButIsNotForeignKey()
    {
        var schemaBuilder = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        var indexBuilder1 = tableBuilder.Indexes.Create( tableBuilder.Columns.Create( "C2" ).Asc() );
        var indexBuilder2 = tableBuilder.SetPrimaryKey( tableBuilder.Columns.Create( "C1" ).Asc() ).Index;
        tableBuilder.ForeignKeys.Create( indexBuilder1, indexBuilder2 );

        var db = new SqliteDatabaseMock( schemaBuilder.Database );
        ISqlObjectCollection sut = db.Schemas.Get( "foo" ).Objects;

        var action = Lambda.Of( () => sut.GetForeignKey( "T" ) );

        action.Should().ThrowExactly<SqliteObjectCastException>();
    }

    [Fact]
    public void Objects_TryGetForeignKey_ShouldReturnCorrectForeignKey()
    {
        var schemaBuilder = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        var indexBuilder1 = tableBuilder.Indexes.Create( tableBuilder.Columns.Create( "C2" ).Asc() );
        var indexBuilder2 = tableBuilder.SetPrimaryKey( tableBuilder.Columns.Create( "C1" ).Asc() ).Index;
        tableBuilder.ForeignKeys.Create( indexBuilder1, indexBuilder2 );

        var db = new SqliteDatabaseMock( schemaBuilder.Database );
        var sut = db.Schemas.Get( "foo" ).Objects;

        var result = sut.TryGetForeignKey( "FK_T_C2_REF_T", out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            (outResult?.Type).Should().Be( SqlObjectType.ForeignKey );
            (outResult?.Name).Should().Be( "FK_T_C2_REF_T" );
        }
    }

    [Fact]
    public void Objects_TryGetForeignKey_ShouldReturnFalse_WhenObjectDoesNotExist()
    {
        var schemaBuilder = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        var indexBuilder1 = tableBuilder.Indexes.Create( tableBuilder.Columns.Create( "C2" ).Asc() );
        var indexBuilder2 = tableBuilder.SetPrimaryKey( tableBuilder.Columns.Create( "C1" ).Asc() ).Index;
        tableBuilder.ForeignKeys.Create( indexBuilder1, indexBuilder2 );

        var db = new SqliteDatabaseMock( schemaBuilder.Database );
        var sut = db.Schemas.Get( "foo" ).Objects;

        var result = sut.TryGetForeignKey( "U", out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().BeFalse();
            outResult.Should().BeNull();
        }
    }

    [Fact]
    public void Objects_TryGetForeignKey_ShouldReturnFalse_WhenObjectExistsButIsNotForeignKey()
    {
        var schemaBuilder = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        var indexBuilder1 = tableBuilder.Indexes.Create( tableBuilder.Columns.Create( "C2" ).Asc() );
        var indexBuilder2 = tableBuilder.SetPrimaryKey( tableBuilder.Columns.Create( "C1" ).Asc() ).Index;
        tableBuilder.ForeignKeys.Create( indexBuilder1, indexBuilder2 );

        var db = new SqliteDatabaseMock( schemaBuilder.Database );
        var sut = db.Schemas.Get( "foo" ).Objects;

        var result = sut.TryGetForeignKey( "T", out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().BeFalse();
            outResult.Should().BeNull();
        }
    }

    [Fact]
    public void ISqlObjectCollection_TryGetForeignKey_ShouldBeEquivalentToTryGetForeignKey()
    {
        var schemaBuilder = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        var indexBuilder1 = tableBuilder.Indexes.Create( tableBuilder.Columns.Create( "C2" ).Asc() );
        var indexBuilder2 = tableBuilder.SetPrimaryKey( tableBuilder.Columns.Create( "C1" ).Asc() ).Index;
        tableBuilder.ForeignKeys.Create( indexBuilder1, indexBuilder2 );

        var db = new SqliteDatabaseMock( schemaBuilder.Database );
        var sut = db.Schemas.Get( "foo" ).Objects;
        var expected = sut.TryGetForeignKey( "FK_T_C2_REF_T", out var outExpected );

        var result = ((ISqlObjectCollection)sut).TryGetForeignKey( "FK_T_C2_REF_T", out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().Be( expected );
            outResult.Should().BeSameAs( outExpected );
        }
    }

    [Fact]
    public void Objects_GetView_ShouldReturnCorrectView()
    {
        var schemaBuilder = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        schemaBuilder.Objects.CreateView( "V", SqlNode.RawQuery( "SELECT * FROM foo" ) );

        var db = new SqliteDatabaseMock( schemaBuilder.Database );
        ISqlObjectCollection sut = db.Schemas.Get( "foo" ).Objects;

        var result = sut.GetView( "V" );

        using ( new AssertionScope() )
        {
            result.Type.Should().Be( SqlObjectType.View );
            result.Name.Should().Be( "V" );
        }
    }

    [Fact]
    public void Objects_GetView_ShouldThrowKeyNotFoundException_WhenObjectDoesNotExist()
    {
        var schemaBuilder = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.SetPrimaryKey( tableBuilder.Columns.Create( "C" ).Asc() );

        var db = new SqliteDatabaseMock( schemaBuilder.Database );
        ISqlObjectCollection sut = db.Schemas.Get( "foo" ).Objects;

        var action = Lambda.Of( () => sut.GetView( "V" ) );

        action.Should().ThrowExactly<KeyNotFoundException>();
    }

    [Fact]
    public void Objects_GetView_ShouldThrowSqliteObjectCastException_WhenObjectExistsButIsNotView()
    {
        var schemaBuilder = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.SetPrimaryKey( tableBuilder.Columns.Create( "C" ).Asc() );

        var db = new SqliteDatabaseMock( schemaBuilder.Database );
        ISqlObjectCollection sut = db.Schemas.Get( "foo" ).Objects;

        var action = Lambda.Of( () => sut.GetView( "T" ) );

        action.Should().ThrowExactly<SqliteObjectCastException>();
    }

    [Fact]
    public void Objects_TryGetView_ShouldReturnCorrectView()
    {
        var schemaBuilder = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        schemaBuilder.Objects.CreateView( "V", SqlNode.RawQuery( "SELECT * FROM foo" ) );

        var db = new SqliteDatabaseMock( schemaBuilder.Database );
        var sut = db.Schemas.Get( "foo" ).Objects;

        var result = sut.TryGetView( "V", out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            (outResult?.Type).Should().Be( SqlObjectType.View );
            (outResult?.Name).Should().Be( "V" );
        }
    }

    [Fact]
    public void Objects_TryGetView_ShouldReturnFalse_WhenObjectDoesNotExist()
    {
        var schemaBuilder = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        var indexBuilder1 = tableBuilder.Indexes.Create( tableBuilder.Columns.Create( "C2" ).Asc() );
        var indexBuilder2 = tableBuilder.SetPrimaryKey( tableBuilder.Columns.Create( "C1" ).Asc() ).Index;
        tableBuilder.ForeignKeys.Create( indexBuilder1, indexBuilder2 );

        var db = new SqliteDatabaseMock( schemaBuilder.Database );
        var sut = db.Schemas.Get( "foo" ).Objects;

        var result = sut.TryGetView( "U", out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().BeFalse();
            outResult.Should().BeNull();
        }
    }

    [Fact]
    public void Objects_TryGetView_ShouldReturnFalse_WhenObjectExistsButIsNotView()
    {
        var schemaBuilder = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.SetPrimaryKey( tableBuilder.Columns.Create( "C" ).Asc() );

        var db = new SqliteDatabaseMock( schemaBuilder.Database );
        var sut = db.Schemas.Get( "foo" ).Objects;

        var result = sut.TryGetView( "T", out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().BeFalse();
            outResult.Should().BeNull();
        }
    }

    [Fact]
    public void ISqlObjectCollection_TryGetView_ShouldBeEquivalentToTryGetView()
    {
        var schemaBuilder = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        schemaBuilder.Objects.CreateView( "V", SqlNode.RawQuery( "SELECT * FROM foo" ) );

        var db = new SqliteDatabaseMock( schemaBuilder.Database );
        var sut = db.Schemas.Get( "foo" ).Objects;
        var expected = sut.TryGetView( "V", out var outExpected );

        var result = ((ISqlObjectCollection)sut).TryGetView( "V", out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().Be( expected );
            outResult.Should().BeSameAs( outExpected );
        }
    }

    [Fact]
    public void Objects_GetCheck_ShouldReturnCorrectCheck()
    {
        var schemaBuilder = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.SetPrimaryKey( tableBuilder.Columns.Create( "C" ).Asc() );
        tableBuilder.Checks.Create( tableBuilder.RecordSet["C"] > SqlNode.Literal( 0 ) );

        var db = new SqliteDatabaseMock( schemaBuilder.Database );
        ISqlObjectCollection sut = db.Schemas.Get( "foo" ).Objects;

        var result = sut.GetCheck( "CHK_T_0" );

        using ( new AssertionScope() )
        {
            result.Type.Should().Be( SqlObjectType.Check );
            result.Name.Should().Be( "CHK_T_0" );
        }
    }

    [Fact]
    public void Objects_GetCheck_ShouldThrowKeyNotFoundException_WhenObjectDoesNotExist()
    {
        var schemaBuilder = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.SetPrimaryKey( tableBuilder.Columns.Create( "C" ).Asc() );
        tableBuilder.Checks.Create( tableBuilder.RecordSet["C"] > SqlNode.Literal( 0 ) );

        var db = new SqliteDatabaseMock( schemaBuilder.Database );
        ISqlObjectCollection sut = db.Schemas.Get( "foo" ).Objects;

        var action = Lambda.Of( () => sut.GetCheck( "U" ) );

        action.Should().ThrowExactly<KeyNotFoundException>();
    }

    [Fact]
    public void Objects_GetCheck_ShouldThrowSqliteObjectCastException_WhenObjectExistsButIsNotCheck()
    {
        var schemaBuilder = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.SetPrimaryKey( tableBuilder.Columns.Create( "C" ).Asc() );
        tableBuilder.Checks.Create( tableBuilder.RecordSet["C"] > SqlNode.Literal( 0 ) );

        var db = new SqliteDatabaseMock( schemaBuilder.Database );
        ISqlObjectCollection sut = db.Schemas.Get( "foo" ).Objects;

        var action = Lambda.Of( () => sut.GetCheck( "T" ) );

        action.Should().ThrowExactly<SqliteObjectCastException>();
    }

    [Fact]
    public void Objects_TryGetCheck_ShouldReturnCorrectCheck()
    {
        var schemaBuilder = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.SetPrimaryKey( tableBuilder.Columns.Create( "C" ).Asc() );
        tableBuilder.Checks.Create( tableBuilder.RecordSet["C"] > SqlNode.Literal( 0 ) );

        var db = new SqliteDatabaseMock( schemaBuilder.Database );
        var sut = db.Schemas.Get( "foo" ).Objects;

        var result = sut.TryGetCheck( "CHK_T_0", out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            (outResult?.Type).Should().Be( SqlObjectType.Check );
            (outResult?.Name).Should().Be( "CHK_T_0" );
        }
    }

    [Fact]
    public void Objects_TryGetCheck_ShouldReturnFalse_WhenObjectDoesNotExist()
    {
        var schemaBuilder = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.SetPrimaryKey( tableBuilder.Columns.Create( "C" ).Asc() );
        tableBuilder.Checks.Create( tableBuilder.RecordSet["C"] > SqlNode.Literal( 0 ) );

        var db = new SqliteDatabaseMock( schemaBuilder.Database );
        var sut = db.Schemas.Get( "foo" ).Objects;

        var result = sut.TryGetCheck( "U", out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().BeFalse();
            outResult.Should().BeNull();
        }
    }

    [Fact]
    public void Objects_TryGetCheck_ShouldReturnFalse_WhenObjectExistsButIsNotCheck()
    {
        var schemaBuilder = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.SetPrimaryKey( tableBuilder.Columns.Create( "C" ).Asc() );
        tableBuilder.Checks.Create( tableBuilder.RecordSet["C"] > SqlNode.Literal( 0 ) );

        var db = new SqliteDatabaseMock( schemaBuilder.Database );
        var sut = db.Schemas.Get( "foo" ).Objects;

        var result = sut.TryGetCheck( "T", out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().BeFalse();
            outResult.Should().BeNull();
        }
    }

    [Fact]
    public void ISqlObjectCollection_TryGetCheck_ShouldBeEquivalentToTryGetCheck()
    {
        var schemaBuilder = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.SetPrimaryKey( tableBuilder.Columns.Create( "C" ).Asc() );
        tableBuilder.Checks.Create( tableBuilder.RecordSet["C"] > SqlNode.Literal( 0 ) );

        var db = new SqliteDatabaseMock( schemaBuilder.Database );
        var sut = db.Schemas.Get( "foo" ).Objects;
        var expected = sut.TryGetCheck( "CHK_T_0", out var outExpected );

        var result = ((ISqlObjectCollection)sut).TryGetCheck( "CHK_T_0", out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().Be( expected );
            outResult.Should().BeSameAs( outExpected );
        }
    }
}
