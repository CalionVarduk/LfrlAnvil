using System.Collections.Generic;
using LfrlAnvil.Functional;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Builders;
using LfrlAnvil.Sql.Extensions;
using LfrlAnvil.Sqlite.Builders;
using LfrlAnvil.Sqlite.Exceptions;
using LfrlAnvil.Sqlite.Extensions;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.Sqlite.Tests.BuildersTests;

public partial class SqliteTableBuilderTests
{
    public class Indexes : TestsBase
    {
        [Fact]
        public void Create_ShouldCreateNewIndex()
        {
            var schema = new SqliteDatabaseBuilder().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Indexes;
            var c1 = table.Columns.Create( "C1" ).Asc();
            var c2 = table.Columns.Create( "C2" ).Desc();

            var result = ((ISqlIndexBuilderCollection)sut).Create( c1, c2 );

            using ( new AssertionScope() )
            {
                result.Columns.Should().BeSequentiallyEqualTo( c1, c2 );
                result.Name.Should().Be( "IX_T_C1A_C2D" );
                result.FullName.Should().Be( "foo_IX_T_C1A_C2D" );
                result.Table.Should().BeSameAs( table );
                result.Database.Should().BeSameAs( table.Database );
                result.Type.Should().Be( SqlObjectType.Index );
                result.ForeignKeys.Should().BeEmpty();
                result.ReferencingForeignKeys.Should().BeEmpty();
                result.PrimaryKey.Should().BeNull();
                result.IsUnique.Should().BeFalse();
                sut.Count.Should().Be( 1 );
                sut.Should().BeEquivalentTo( result );
                schema.Objects.Contains( result.Name ).Should().BeTrue();

                ((ISqlColumnBuilder)c1.Column).Indexes.Should().BeSequentiallyEqualTo( result );
                ((ISqlColumnBuilder)c2.Column).Indexes.Should().BeSequentiallyEqualTo( result );
            }
        }

        [Fact]
        public void Create_ShouldThrowSqliteObjectBuilderException_WhenTableIsRemoved()
        {
            var schema = new SqliteDatabaseBuilder().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Indexes;
            var column = table.Columns.Create( "C" ).Asc();
            table.Remove();

            var action = Lambda.Of( () => sut.Create( column ) );

            action.Should()
                .ThrowExactly<SqliteObjectBuilderException>()
                .AndMatch( e => e.Dialect == SqliteDialect.Instance && e.Errors.Count == 1 );
        }

        [Fact]
        public void Create_ShouldThrowSqliteObjectBuilderException_WhenIndexAlreadyExists()
        {
            var schema = new SqliteDatabaseBuilder().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Indexes;
            var c1 = table.Columns.Create( "C1" ).Asc();
            var c2 = table.Columns.Create( "C2" ).Desc();
            sut.Create( c1, c2 );

            var action = Lambda.Of( () => sut.Create( c1, c2 ) );

            action.Should()
                .ThrowExactly<SqliteObjectBuilderException>()
                .AndMatch( e => e.Dialect == SqliteDialect.Instance && e.Errors.Count == 1 );
        }

        [Fact]
        public void Create_ShouldThrowSqliteObjectBuilderException_WhenColumnsAreEmpty()
        {
            var schema = new SqliteDatabaseBuilder().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Indexes;

            var action = Lambda.Of( () => sut.Create() );

            action.Should()
                .ThrowExactly<SqliteObjectBuilderException>()
                .AndMatch( e => e.Dialect == SqliteDialect.Instance && e.Errors.Count == 1 );
        }

        [Fact]
        public void Create_ShouldThrowSqliteObjectBuilderException_WhenAtLeastOneColumnBelongsToAnotherTable()
        {
            var schema = new SqliteDatabaseBuilder().Schemas.Create( "foo" );
            var t1 = schema.Objects.CreateTable( "T1" );
            var c1 = t1.Columns.Create( "C1" );
            t1.SetPrimaryKey( c1.Asc() );

            var t2 = schema.Objects.CreateTable( "T2" );
            var sut = t2.Indexes;

            var action = Lambda.Of( () => sut.Create( c1.Asc() ) );

            action.Should()
                .ThrowExactly<SqliteObjectBuilderException>()
                .AndMatch( e => e.Dialect == SqliteDialect.Instance && e.Errors.Count == 1 );
        }

        [Fact]
        public void Create_ShouldThrowSqliteObjectBuilderException_WhenAtLeastOneColumnIsRemoved()
        {
            var schema = new SqliteDatabaseBuilder().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var column = table.Columns.Create( "C" );
            var sut = table.Indexes;
            column.Remove();

            var action = Lambda.Of( () => sut.Create( column.Asc() ) );

            action.Should()
                .ThrowExactly<SqliteObjectBuilderException>()
                .AndMatch( e => e.Dialect == SqliteDialect.Instance && e.Errors.Count == 1 );
        }

        [Fact]
        public void Create_ShouldThrowSqliteObjectBuilderException_WhenAtLeastOneColumnIsDuplicated()
        {
            var schema = new SqliteDatabaseBuilder().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var column = table.Columns.Create( "C" );
            var sut = table.Indexes;

            var action = Lambda.Of( () => sut.Create( column.Asc(), column.Desc() ) );

            action.Should()
                .ThrowExactly<SqliteObjectBuilderException>()
                .AndMatch( e => e.Dialect == SqliteDialect.Instance && e.Errors.Count == 1 );
        }

        [Fact]
        public void Create_ShouldThrowSqliteObjectCastException_WhenAtLeastOneColumnIsOfInvalidType()
        {
            var schema = new SqliteDatabaseBuilder().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var column = Substitute.For<ISqlColumnBuilder>();
            var sut = table.Indexes;

            var action = Lambda.Of( () => sut.Create( column.Asc() ) );

            action.Should()
                .ThrowExactly<SqliteObjectCastException>()
                .AndMatch( e => e.Dialect == SqliteDialect.Instance && e.Expected == typeof( SqliteIndexColumnBuilder ) );
        }

        [Fact]
        public void GetOrCreate_ShouldCreateNewIndex_WhenIndexDoesNotExist()
        {
            var schema = new SqliteDatabaseBuilder().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Indexes;
            var c1 = table.Columns.Create( "C1" ).Asc();
            var c2 = table.Columns.Create( "C2" ).Desc();

            var result = ((ISqlIndexBuilderCollection)sut).GetOrCreate( c1, c2 );

            using ( new AssertionScope() )
            {
                result.Columns.Should().BeSequentiallyEqualTo( c1, c2 );
                result.Name.Should().Be( "IX_T_C1A_C2D" );
                result.FullName.Should().Be( "foo_IX_T_C1A_C2D" );
                result.Table.Should().BeSameAs( table );
                result.Database.Should().BeSameAs( table.Database );
                result.Type.Should().Be( SqlObjectType.Index );
                result.ForeignKeys.Should().BeEmpty();
                result.ReferencingForeignKeys.Should().BeEmpty();
                result.PrimaryKey.Should().BeNull();
                result.IsUnique.Should().BeFalse();
                sut.Count.Should().Be( 1 );
                sut.Should().BeEquivalentTo( result );
                schema.Objects.Contains( result.Name ).Should().BeTrue();

                ((ISqlColumnBuilder)c1.Column).Indexes.Should().BeSequentiallyEqualTo( result );
                ((ISqlColumnBuilder)c2.Column).Indexes.Should().BeSequentiallyEqualTo( result );
            }
        }

        [Fact]
        public void GetOrCreate_ShouldReturnExistingIndex_WhenIndexExists()
        {
            var schema = new SqliteDatabaseBuilder().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Indexes;
            var c1 = table.Columns.Create( "C1" ).Asc();
            var c2 = table.Columns.Create( "C2" ).Desc();
            var expected = sut.Create( c1, c2 );

            var result = sut.GetOrCreate( c1, c2 );

            using ( new AssertionScope() )
            {
                result.Should().BeSameAs( expected );
                sut.Count.Should().Be( 1 );
            }
        }

        [Fact]
        public void Contains_ShouldReturnTrue_WhenIndexExists()
        {
            var schema = new SqliteDatabaseBuilder().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Indexes;
            var c1 = table.Columns.Create( "C1" ).Asc();
            var c2 = table.Columns.Create( "C2" ).Desc();
            sut.Create( c1, c2 );

            var result = sut.Contains( c1, c2 );

            result.Should().BeTrue();
        }

        [Fact]
        public void Contains_ShouldReturnFalse_WhenIndexDoesNotExist()
        {
            var schema = new SqliteDatabaseBuilder().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Indexes;
            var c1 = table.Columns.Create( "C1" ).Asc();
            var c2 = table.Columns.Create( "C2" ).Desc();
            sut.Create( c1, c2 );

            var result = sut.Contains( c1, c2.Column.Asc() );

            result.Should().BeFalse();
        }

        [Fact]
        public void Get_ShouldReturnExistingIndex()
        {
            var schema = new SqliteDatabaseBuilder().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Indexes;
            var c1 = table.Columns.Create( "C1" ).Asc();
            var c2 = table.Columns.Create( "C2" ).Desc();
            var expected = sut.Create( c1, c2 );

            var result = ((ISqlIndexBuilderCollection)sut).Get( c1, c2 );

            result.Should().BeSameAs( expected );
        }

        [Fact]
        public void Get_ShouldThrowKeyNotFoundException_WhenIndexDoesNotExist()
        {
            var schema = new SqliteDatabaseBuilder().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Indexes;
            var c1 = table.Columns.Create( "C1" ).Asc();
            var c2 = table.Columns.Create( "C2" ).Desc();
            sut.Create( c1, c2 );

            var action = Lambda.Of( () => sut.Get( c1, c2.Column.Asc() ) );

            action.Should().ThrowExactly<KeyNotFoundException>();
        }

        [Fact]
        public void TryGet_ShouldReturnExistingIndex()
        {
            var schema = new SqliteDatabaseBuilder().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Indexes;
            var c1 = table.Columns.Create( "C1" ).Asc();
            var c2 = table.Columns.Create( "C2" ).Desc();
            var expected = sut.Create( c1, c2 );

            var result = ((ISqlIndexBuilderCollection)sut).TryGet( new[] { c1, c2 }, out var outResult );

            using ( new AssertionScope() )
            {
                result.Should().BeTrue();
                outResult.Should().BeSameAs( expected );
            }
        }

        [Fact]
        public void TryGet_ShouldReturnFalse_WhenIndexDoesNotExist()
        {
            var schema = new SqliteDatabaseBuilder().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Indexes;
            var c1 = table.Columns.Create( "C1" ).Asc();
            var c2 = table.Columns.Create( "C2" ).Desc();
            sut.Create( c1, c2 );

            var result = ((ISqlIndexBuilderCollection)sut).TryGet( new[] { c1, c2.Column.Asc() }, out var outResult );

            using ( new AssertionScope() )
            {
                result.Should().BeFalse();
                outResult.Should().BeNull();
            }
        }

        [Fact]
        public void Remove_ShouldRemoveExistingIndex()
        {
            var schema = new SqliteDatabaseBuilder().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Indexes;
            var c1 = table.Columns.Create( "C1" ).Asc();
            var c2 = table.Columns.Create( "C2" ).Desc();
            var index = sut.Create( c1, c2 );

            var result = sut.Remove( c1, c2 );

            using ( new AssertionScope() )
            {
                result.Should().BeTrue();
                index.IsRemoved.Should().BeTrue();
                c1.Column.Indexes.Should().BeEmpty();
                c2.Column.Indexes.Should().BeEmpty();
                sut.Count.Should().Be( 0 );
                schema.Objects.Contains( index.Name ).Should().BeFalse();
            }
        }

        [Fact]
        public void Remove_ShouldReturnFalse_WhenIndexDoesNotExist()
        {
            var schema = new SqliteDatabaseBuilder().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Indexes;
            var c1 = table.Columns.Create( "C1" ).Asc();
            var c2 = table.Columns.Create( "C2" ).Desc();
            sut.Create( c1, c2 );

            var result = sut.Remove( c1, c2.Column.Asc() );

            using ( new AssertionScope() )
            {
                result.Should().BeFalse();
                sut.Count.Should().Be( 1 );
            }
        }

        [Fact]
        public void Remove_ShouldReturnFalse_WhenIndexContainsReferencingForeignKeys()
        {
            var schema = new SqliteDatabaseBuilder().Schemas.Create( "foo" );
            var t1 = schema.Objects.CreateTable( "T1" );
            var c1 = t1.Columns.Create( "C1" ).Asc();
            var sut = t1.Indexes;
            var ix1 = t1.SetPrimaryKey( c1 ).Index;
            var t2 = schema.Objects.CreateTable( "T2" );
            t2.ForeignKeys.Create( t2.SetPrimaryKey( t2.Columns.Create( "C2" ).Desc() ).Index, ix1 );

            var result = sut.Remove( c1 );

            using ( new AssertionScope() )
            {
                result.Should().BeFalse();
                sut.Count.Should().Be( 1 );
            }
        }
    }
}
