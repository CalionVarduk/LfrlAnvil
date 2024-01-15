using System.Collections.Generic;
using LfrlAnvil.Functional;
using LfrlAnvil.MySql.Exceptions;
using LfrlAnvil.MySql.Extensions;
using LfrlAnvil.MySql.Objects.Builders;
using LfrlAnvil.MySql.Tests.Helpers;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Extensions;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.MySql.Tests.ObjectsTests.BuildersTests;

public partial class MySqlTableBuilderTests
{
    public class Indexes : TestsBase
    {
        [Fact]
        public void Create_ShouldCreateNewIndex()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Indexes;
            var c1 = table.Columns.Create( "C1" ).Asc();
            var c2 = table.Columns.Create( "C2" ).Desc();

            var result = ((ISqlIndexBuilderCollection)sut).Create( c1, c2 );

            using ( new AssertionScope() )
            {
                result.Columns.ToArray().Should().BeSequentiallyEqualTo( c1, c2 );
                result.Name.Should().Be( "IX_T_C1A_C2D" );
                result.FullName.Should().Be( "foo.IX_T_C1A_C2D" );
                result.Table.Should().BeSameAs( table );
                result.Database.Should().BeSameAs( table.Database );
                result.Type.Should().Be( SqlObjectType.Index );
                result.OriginatingForeignKeys.Should().BeEmpty();
                result.ReferencingForeignKeys.Should().BeEmpty();
                result.ReferencedFilterColumns.Should().BeEmpty();
                result.PrimaryKey.Should().BeNull();
                result.IsUnique.Should().BeFalse();
                result.Filter.Should().BeNull();
                sut.Count.Should().Be( 1 );
                sut.Should().BeEquivalentTo( result );
                schema.Objects.Contains( result.Name ).Should().BeTrue();

                ((ISqlColumnBuilder)c1.Column).ReferencingIndexes.Should().BeSequentiallyEqualTo( result );
                ((ISqlColumnBuilder)c2.Column).ReferencingIndexes.Should().BeSequentiallyEqualTo( result );
            }
        }

        [Fact]
        public void Create_ShouldThrowMySqlObjectBuilderException_WhenTableIsRemoved()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Indexes;
            var column = table.Columns.Create( "C" ).Asc();
            table.Remove();

            var action = Lambda.Of( () => sut.Create( column ) );

            action.Should()
                .ThrowExactly<MySqlObjectBuilderException>()
                .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
        }

        [Fact]
        public void Create_ShouldThrowMySqlObjectBuilderException_WhenIndexAlreadyExists()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Indexes;
            var c1 = table.Columns.Create( "C1" ).Asc();
            var c2 = table.Columns.Create( "C2" ).Desc();
            sut.Create( c1, c2 );

            var action = Lambda.Of( () => sut.Create( c1, c2 ) );

            action.Should()
                .ThrowExactly<MySqlObjectBuilderException>()
                .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
        }

        [Fact]
        public void Create_ShouldThrowMySqlObjectBuilderException_WhenColumnsAreEmpty()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Indexes;

            var action = Lambda.Of( () => sut.Create() );

            action.Should()
                .ThrowExactly<MySqlObjectBuilderException>()
                .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
        }

        [Fact]
        public void Create_ShouldThrowMySqlObjectBuilderException_WhenAtLeastOneColumnBelongsToAnotherTable()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var t1 = schema.Objects.CreateTable( "T1" );
            var c1 = t1.Columns.Create( "C1" );
            t1.SetPrimaryKey( c1.Asc() );

            var t2 = schema.Objects.CreateTable( "T2" );
            var sut = t2.Indexes;

            var action = Lambda.Of( () => sut.Create( c1.Asc() ) );

            action.Should()
                .ThrowExactly<MySqlObjectBuilderException>()
                .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
        }

        [Fact]
        public void Create_ShouldThrowMySqlObjectBuilderException_WhenAtLeastOneColumnIsRemoved()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var column = table.Columns.Create( "C" );
            var sut = table.Indexes;
            column.Remove();

            var action = Lambda.Of( () => sut.Create( column.Asc() ) );

            action.Should()
                .ThrowExactly<MySqlObjectBuilderException>()
                .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
        }

        [Fact]
        public void Create_ShouldThrowMySqlObjectBuilderException_WhenAtLeastOneColumnIsDuplicated()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var column = table.Columns.Create( "C" );
            var sut = table.Indexes;

            var action = Lambda.Of( () => sut.Create( column.Asc(), column.Desc() ) );

            action.Should()
                .ThrowExactly<MySqlObjectBuilderException>()
                .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
        }

        [Fact]
        public void Create_ShouldThrowSqlObjectCastException_WhenAtLeastOneColumnIsOfInvalidType()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var column = Substitute.For<ISqlColumnBuilder>();
            var sut = table.Indexes;

            var action = Lambda.Of( () => sut.Create( column.Asc() ) );

            action.Should()
                .ThrowExactly<SqlObjectCastException>()
                .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Expected == typeof( MySqlIndexColumnBuilder ) );
        }

        [Fact]
        public void GetOrCreate_ShouldCreateNewIndex_WhenIndexDoesNotExist()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Indexes;
            var c1 = table.Columns.Create( "C1" ).Asc();
            var c2 = table.Columns.Create( "C2" ).Desc();

            var result = ((ISqlIndexBuilderCollection)sut).GetOrCreate( c1, c2 );

            using ( new AssertionScope() )
            {
                result.Columns.ToArray().Should().BeSequentiallyEqualTo( c1, c2 );
                result.Name.Should().Be( "IX_T_C1A_C2D" );
                result.FullName.Should().Be( "foo.IX_T_C1A_C2D" );
                result.Table.Should().BeSameAs( table );
                result.Database.Should().BeSameAs( table.Database );
                result.Type.Should().Be( SqlObjectType.Index );
                result.OriginatingForeignKeys.Should().BeEmpty();
                result.ReferencingForeignKeys.Should().BeEmpty();
                result.ReferencedFilterColumns.Should().BeEmpty();
                result.PrimaryKey.Should().BeNull();
                result.IsUnique.Should().BeFalse();
                result.Filter.Should().BeNull();
                sut.Count.Should().Be( 1 );
                sut.Should().BeEquivalentTo( result );
                schema.Objects.Contains( result.Name ).Should().BeTrue();

                ((ISqlColumnBuilder)c1.Column).ReferencingIndexes.Should().BeSequentiallyEqualTo( result );
                ((ISqlColumnBuilder)c2.Column).ReferencingIndexes.Should().BeSequentiallyEqualTo( result );
            }
        }

        [Fact]
        public void GetOrCreate_ShouldReturnExistingIndex_WhenIndexExists()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
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
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
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
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
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
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
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
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
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
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
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
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
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
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
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
                c1.Column.ReferencingIndexes.Should().BeEmpty();
                c2.Column.ReferencingIndexes.Should().BeEmpty();
                sut.Count.Should().Be( 0 );
                schema.Objects.Contains( index.Name ).Should().BeFalse();
            }
        }

        [Fact]
        public void Remove_ShouldReturnFalse_WhenIndexDoesNotExist()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
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
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
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
