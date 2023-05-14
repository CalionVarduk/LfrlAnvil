using System.Collections.Generic;
using LfrlAnvil.Functional;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.Sqlite.Exceptions;
using LfrlAnvil.Sqlite.Extensions;
using LfrlAnvil.Sqlite.Objects.Builders;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.Sqlite.Tests.BuildersTests;

public partial class SqliteTableBuilderTests
{
    public class ForeignKeys : TestsBase
    {
        [Fact]
        public void Create_ShouldCreateNewForeignKey_WhenIndexesBelongToTheSameTable()
        {
            var schema = new SqliteDatabaseBuilder().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.ForeignKeys;
            var ix1 = table.Indexes.Create( table.Columns.Create( "C1" ).Asc() );
            var ix2 = table.Indexes.Create( table.Columns.Create( "C2" ).Asc() ).MarkAsUnique();

            var result = ((ISqlForeignKeyBuilderCollection)sut).Create( ix1, ix2 );

            using ( new AssertionScope() )
            {
                result.Index.Should().BeSameAs( ix1 );
                result.ReferencedIndex.Should().BeSameAs( ix2 );
                result.Name.Should().Be( "FK_T_C1_REF_foo_T" );
                result.FullName.Should().Be( "foo_FK_T_C1_REF_foo_T" );
                result.Database.Should().BeSameAs( table.Database );
                result.Type.Should().Be( SqlObjectType.ForeignKey );
                result.OnUpdateBehavior.Should().Be( ReferenceBehavior.Restrict );
                result.OnDeleteBehavior.Should().Be( ReferenceBehavior.Restrict );
                sut.Count.Should().Be( 1 );
                sut.Should().BeEquivalentTo( result );
                schema.Objects.Contains( result.Name ).Should().BeTrue();

                ((ISqlIndexBuilder)ix1).ForeignKeys.Should().BeSequentiallyEqualTo( result );
                ix1.ReferencingForeignKeys.Should().BeEmpty();

                ix2.ForeignKeys.Should().BeEmpty();
                ((ISqlIndexBuilder)ix2).ReferencingForeignKeys.Should().BeSequentiallyEqualTo( result );
            }
        }

        [Fact]
        public void Create_ShouldCreateNewForeignKey_WhenIndexesDoNotBelongToTheSameTable()
        {
            var schema = new SqliteDatabaseBuilder().Schemas.Create( "foo" );
            var t1 = schema.Objects.CreateTable( "T1" );
            var ix2 = t1.SetPrimaryKey( t1.Columns.Create( "C1" ).Asc() ).Index;
            var t2 = schema.Objects.CreateTable( "T2" );
            var sut = t2.ForeignKeys;
            var ix1 = t2.Indexes.Create( t2.Columns.Create( "C2" ).Asc() );

            var result = ((ISqlForeignKeyBuilderCollection)sut).Create( ix1, ix2 );

            using ( new AssertionScope() )
            {
                result.Index.Should().BeSameAs( ix1 );
                result.ReferencedIndex.Should().BeSameAs( ix2 );
                result.Name.Should().Be( "FK_T2_C2_REF_foo_T1" );
                result.FullName.Should().Be( "foo_FK_T2_C2_REF_foo_T1" );
                result.Database.Should().BeSameAs( t1.Database );
                result.Type.Should().Be( SqlObjectType.ForeignKey );
                result.OnUpdateBehavior.Should().Be( ReferenceBehavior.Restrict );
                result.OnDeleteBehavior.Should().Be( ReferenceBehavior.Restrict );
                sut.Count.Should().Be( 1 );
                sut.Should().BeEquivalentTo( result );
                schema.Objects.Contains( result.Name ).Should().BeTrue();

                ((ISqlIndexBuilder)ix1).ForeignKeys.Should().BeSequentiallyEqualTo( result );
                ix1.ReferencingForeignKeys.Should().BeEmpty();

                ix2.ForeignKeys.Should().BeEmpty();
                ((ISqlIndexBuilder)ix2).ReferencingForeignKeys.Should().BeSequentiallyEqualTo( result );
            }
        }

        [Fact]
        public void Create_ShouldThrowSqliteObjectBuilderException_WhenTableIsRemoved()
        {
            var schema = new SqliteDatabaseBuilder().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.ForeignKeys;
            var ix1 = table.Indexes.Create( table.Columns.Create( "C1" ).Asc() );
            var ix2 = table.Indexes.Create( table.Columns.Create( "C2" ).Asc() ).MarkAsUnique();
            table.Remove();

            var action = Lambda.Of( () => sut.Create( ix1, ix2 ) );

            action.Should()
                .ThrowExactly<SqliteObjectBuilderException>()
                .AndMatch( e => e.Dialect == SqliteDialect.Instance && e.Errors.Count == 1 );
        }

        [Fact]
        public void Create_ShouldThrowSqliteObjectBuilderException_WhenForeignKeyAlreadyExists()
        {
            var schema = new SqliteDatabaseBuilder().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.ForeignKeys;
            var ix1 = table.Indexes.Create( table.Columns.Create( "C1" ).Asc() );
            var ix2 = table.Indexes.Create( table.Columns.Create( "C2" ).Asc() ).MarkAsUnique();
            sut.Create( ix1, ix2 );

            var action = Lambda.Of( () => sut.Create( ix1, ix2 ) );

            action.Should()
                .ThrowExactly<SqliteObjectBuilderException>()
                .AndMatch( e => e.Dialect == SqliteDialect.Instance && e.Errors.Count == 1 );
        }

        [Fact]
        public void Create_ShouldThrowSqliteObjectBuilderException_WhenSchemaObjectWithDefaultForeignKeyNameAlreadyExists()
        {
            var schema = new SqliteDatabaseBuilder().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.ForeignKeys;
            var ix1 = table.Indexes.Create( table.Columns.Create( "C1" ).Asc() ).SetName( "FK_T_C1_REF_foo_T" );
            var ix2 = table.Indexes.Create( table.Columns.Create( "C2" ).Asc() ).MarkAsUnique();

            var action = Lambda.Of( () => sut.Create( ix1, ix2 ) );

            action.Should()
                .ThrowExactly<SqliteObjectBuilderException>()
                .AndMatch( e => e.Dialect == SqliteDialect.Instance && e.Errors.Count == 1 );
        }

        [Fact]
        public void Create_ShouldThrowSqliteObjectBuilderException_WhenIndexAndReferencedIndexAreTheSame()
        {
            var schema = new SqliteDatabaseBuilder().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.ForeignKeys;
            var ix1 = table.Indexes.Create( table.Columns.Create( "C1" ).Asc() ).MarkAsUnique();

            var action = Lambda.Of( () => sut.Create( ix1, ix1 ) );

            action.Should()
                .ThrowExactly<SqliteObjectBuilderException>()
                .AndMatch( e => e.Dialect == SqliteDialect.Instance && e.Errors.Count == 1 );
        }

        [Fact]
        public void Create_ShouldThrowSqliteObjectBuilderException_WhenReferencedIndexIsNotUnique()
        {
            var schema = new SqliteDatabaseBuilder().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.ForeignKeys;
            var ix1 = table.Indexes.Create( table.Columns.Create( "C1" ).Asc() );
            var ix2 = table.Indexes.Create( table.Columns.Create( "C2" ).Asc() );

            var action = Lambda.Of( () => sut.Create( ix1, ix2 ) );

            action.Should()
                .ThrowExactly<SqliteObjectBuilderException>()
                .AndMatch( e => e.Dialect == SqliteDialect.Instance && e.Errors.Count == 1 );
        }

        [Fact]
        public void Create_ShouldThrowSqliteObjectBuilderException_WhenIndexBelongsToAnotherTable()
        {
            var schema = new SqliteDatabaseBuilder().Schemas.Create( "foo" );
            var t1 = schema.Objects.CreateTable( "T1" );
            var sut = t1.ForeignKeys;
            var ix1 = t1.SetPrimaryKey( t1.Columns.Create( "C1" ).Asc() ).Index;
            var t2 = schema.Objects.CreateTable( "T2" );
            var ix2 = t2.Indexes.Create( t2.Columns.Create( "C2" ).Asc() );

            var action = Lambda.Of( () => sut.Create( ix2, ix1 ) );

            action.Should()
                .ThrowExactly<SqliteObjectBuilderException>()
                .AndMatch( e => e.Dialect == SqliteDialect.Instance && e.Errors.Count == 1 );
        }

        [Fact]
        public void Create_ShouldThrowSqliteObjectBuilderException_WhenReferencedIndexBelongsToAnotherDatabase()
        {
            var schema = new SqliteDatabaseBuilder().Schemas.Create( "foo" );
            var otherSchema = new SqliteDatabaseBuilder().Schemas.Default;
            var t1 = schema.Objects.CreateTable( "T1" );
            var sut = t1.ForeignKeys;
            var ix1 = t1.SetPrimaryKey( t1.Columns.Create( "C1" ).Asc() ).Index;
            var t2 = otherSchema.Objects.CreateTable( "T2" );
            var ix2 = t2.Indexes.Create( t2.Columns.Create( "C2" ).Asc() ).MarkAsUnique();

            var action = Lambda.Of( () => sut.Create( ix1, ix2 ) );

            action.Should()
                .ThrowExactly<SqliteObjectBuilderException>()
                .AndMatch( e => e.Dialect == SqliteDialect.Instance && e.Errors.Count == 1 );
        }

        [Fact]
        public void Create_ShouldThrowSqliteObjectBuilderException_WhenIndexIsRemoved()
        {
            var schema = new SqliteDatabaseBuilder().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.ForeignKeys;
            var ix1 = table.Indexes.Create( table.Columns.Create( "C1" ).Asc() );
            var ix2 = table.Indexes.Create( table.Columns.Create( "C2" ).Asc() ).MarkAsUnique();
            ix1.Remove();

            var action = Lambda.Of( () => sut.Create( ix1, ix2 ) );

            action.Should()
                .ThrowExactly<SqliteObjectBuilderException>()
                .AndMatch( e => e.Dialect == SqliteDialect.Instance && e.Errors.Count == 2 );
        }

        [Fact]
        public void Create_ShouldThrowSqliteObjectBuilderException_WhenReferencedIndexIsRemoved()
        {
            var schema = new SqliteDatabaseBuilder().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.ForeignKeys;
            var ix1 = table.Indexes.Create( table.Columns.Create( "C1" ).Asc() );
            var ix2 = table.Indexes.Create( table.Columns.Create( "C2" ).Asc() ).MarkAsUnique();
            ix2.Remove();

            var action = Lambda.Of( () => sut.Create( ix1, ix2 ) );

            action.Should()
                .ThrowExactly<SqliteObjectBuilderException>()
                .AndMatch( e => e.Dialect == SqliteDialect.Instance && e.Errors.Count == 2 );
        }

        [Fact]
        public void Create_ShouldThrowSqliteObjectBuilderException_WhenReferencedIndexContainsNullableColumn()
        {
            var schema = new SqliteDatabaseBuilder().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.ForeignKeys;
            var ix1 = table.Indexes.Create( table.Columns.Create( "C1" ).Asc(), table.Columns.Create( "C2" ).Asc() );
            var ix2 = table.Indexes
                .Create(
                    table.Columns.Create( "C3" ).Asc(),
                    table.Columns.Create( "C4" ).MarkAsNullable().Asc() )
                .MarkAsUnique();

            var action = Lambda.Of( () => sut.Create( ix1, ix2 ) );

            action.Should()
                .ThrowExactly<SqliteObjectBuilderException>()
                .AndMatch( e => e.Dialect == SqliteDialect.Instance && e.Errors.Count == 1 );
        }

        [Fact]
        public void Create_ShouldThrowSqliteObjectBuilderException_WhenIndexAndReferencedIndexHaveDifferentAmountOfColumns()
        {
            var schema = new SqliteDatabaseBuilder().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.ForeignKeys;
            var ix1 = table.Indexes.Create( table.Columns.Create( "C1" ).Asc(), table.Columns.Create( "C2" ).Asc() );
            var ix2 = table.Indexes.Create( table.Columns.Create( "C3" ).Asc() ).MarkAsUnique();

            var action = Lambda.Of( () => sut.Create( ix1, ix2 ) );

            action.Should()
                .ThrowExactly<SqliteObjectBuilderException>()
                .AndMatch( e => e.Dialect == SqliteDialect.Instance && e.Errors.Count == 1 );
        }

        [Fact]
        public void Create_ShouldThrowSqliteObjectBuilderException_WhenAtLeastOneIndexAndReferencedIndexColumnPairHasIncompatibleTypes()
        {
            var schema = new SqliteDatabaseBuilder().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.ForeignKeys;
            var ix1 = table.Indexes.Create(
                table.Columns.Create( "C1" ).SetType<int>().Asc(),
                table.Columns.Create( "C2" ).SetType<string>().Asc() );

            var ix2 = table.Indexes.Create(
                    table.Columns.Create( "C3" ).SetType<int>().Asc(),
                    table.Columns.Create( "C4" ).SetType<double>().Asc() )
                .MarkAsUnique();

            var action = Lambda.Of( () => sut.Create( ix1, ix2 ) );

            action.Should()
                .ThrowExactly<SqliteObjectBuilderException>()
                .AndMatch( e => e.Dialect == SqliteDialect.Instance && e.Errors.Count == 1 );
        }

        [Fact]
        public void Create_ShouldThrowSqliteObjectCastException_WhenIndexIsOfInvalidType()
        {
            var schema = new SqliteDatabaseBuilder().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.ForeignKeys;
            var ix1 = Substitute.For<ISqlIndexBuilder>();
            var ix2 = table.Indexes.Create( table.Columns.Create( "C2" ).Asc() ).MarkAsUnique();

            var action = Lambda.Of( () => ((ISqlForeignKeyBuilderCollection)sut).Create( ix1, ix2 ) );

            action.Should()
                .ThrowExactly<SqliteObjectCastException>()
                .AndMatch( e => e.Dialect == SqliteDialect.Instance && e.Expected == typeof( SqliteIndexBuilder ) );
        }

        [Fact]
        public void Create_ShouldThrowSqliteObjectCastException_WhenReferencedIndexIsOfInvalidType()
        {
            var schema = new SqliteDatabaseBuilder().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.ForeignKeys;
            var ix1 = table.Indexes.Create( table.Columns.Create( "C1" ).Asc() );
            var ix2 = Substitute.For<ISqlIndexBuilder>();

            var action = Lambda.Of( () => ((ISqlForeignKeyBuilderCollection)sut).Create( ix1, ix2 ) );

            action.Should()
                .ThrowExactly<SqliteObjectCastException>()
                .AndMatch( e => e.Dialect == SqliteDialect.Instance && e.Expected == typeof( SqliteIndexBuilder ) );
        }

        [Fact]
        public void GetOrCreate_ShouldCreateNewForeignKey_WhenForeignKeyDoesNotExist()
        {
            var schema = new SqliteDatabaseBuilder().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.ForeignKeys;
            var ix1 = table.Indexes.Create( table.Columns.Create( "C1" ).Asc() );
            var ix2 = table.Indexes.Create( table.Columns.Create( "C2" ).Asc() ).MarkAsUnique();

            var result = ((ISqlForeignKeyBuilderCollection)sut).GetOrCreate( ix1, ix2 );

            using ( new AssertionScope() )
            {
                result.Index.Should().BeSameAs( ix1 );
                result.ReferencedIndex.Should().BeSameAs( ix2 );
                result.Name.Should().Be( "FK_T_C1_REF_foo_T" );
                result.FullName.Should().Be( "foo_FK_T_C1_REF_foo_T" );
                result.Database.Should().BeSameAs( table.Database );
                result.Type.Should().Be( SqlObjectType.ForeignKey );
                result.OnUpdateBehavior.Should().Be( ReferenceBehavior.Restrict );
                result.OnDeleteBehavior.Should().Be( ReferenceBehavior.Restrict );
                sut.Count.Should().Be( 1 );
                sut.Should().BeEquivalentTo( result );
                schema.Objects.Contains( result.Name ).Should().BeTrue();

                ((ISqlIndexBuilder)ix1).ForeignKeys.Should().BeSequentiallyEqualTo( result );
                ix1.ReferencingForeignKeys.Should().BeEmpty();

                ix2.ForeignKeys.Should().BeEmpty();
                ((ISqlIndexBuilder)ix2).ReferencingForeignKeys.Should().BeSequentiallyEqualTo( result );
            }
        }

        [Fact]
        public void GetOrCreate_ShouldReturnExistingForeignKey_WhenForeignKeyAlreadyExists()
        {
            var schema = new SqliteDatabaseBuilder().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.ForeignKeys;
            var ix1 = table.Indexes.Create( table.Columns.Create( "C1" ).Asc() );
            var ix2 = table.Indexes.Create( table.Columns.Create( "C2" ).Asc() ).MarkAsUnique();
            var expected = sut.Create( ix1, ix2 );

            var result = sut.GetOrCreate( ix1, ix2 );

            using ( new AssertionScope() )
            {
                result.Should().BeSameAs( expected );
                sut.Count.Should().Be( 1 );
            }
        }

        [Fact]
        public void Contains_ShouldReturnTrue_WhenForeignKeyExists()
        {
            var schema = new SqliteDatabaseBuilder().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.ForeignKeys;
            var ix1 = table.Indexes.Create( table.Columns.Create( "C1" ).Asc() );
            var ix2 = table.Indexes.Create( table.Columns.Create( "C2" ).Asc() ).MarkAsUnique();
            sut.Create( ix1, ix2 );

            var result = sut.Contains( ix1, ix2 );

            result.Should().BeTrue();
        }

        [Fact]
        public void Contains_ShouldReturnFalse_WhenForeignKeyDoesNotExist()
        {
            var schema = new SqliteDatabaseBuilder().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.ForeignKeys;
            var ix1 = table.Indexes.Create( table.Columns.Create( "C1" ).Asc() );
            var ix2 = table.Indexes.Create( table.Columns.Create( "C2" ).Asc() ).MarkAsUnique();
            sut.Create( ix1, ix2 );

            var result = sut.Contains( ix2, ix1 );

            result.Should().BeFalse();
        }

        [Fact]
        public void Get_ShouldReturnExistingForeignKey()
        {
            var schema = new SqliteDatabaseBuilder().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.ForeignKeys;
            var ix1 = table.Indexes.Create( table.Columns.Create( "C1" ).Asc() );
            var ix2 = table.Indexes.Create( table.Columns.Create( "C2" ).Asc() ).MarkAsUnique();
            var expected = sut.Create( ix1, ix2 );

            var result = ((ISqlForeignKeyBuilderCollection)sut).Get( ix1, ix2 );

            result.Should().BeSameAs( expected );
        }

        [Fact]
        public void Get_ShouldThrowKeyNotFoundException_WhenForeignKeyDoesNotExist()
        {
            var schema = new SqliteDatabaseBuilder().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.ForeignKeys;
            var ix1 = table.Indexes.Create( table.Columns.Create( "C1" ).Asc() );
            var ix2 = table.Indexes.Create( table.Columns.Create( "C2" ).Asc() ).MarkAsUnique();
            sut.Create( ix1, ix2 );

            var action = Lambda.Of( () => ((ISqlForeignKeyBuilderCollection)sut).Get( ix2, ix1 ) );

            action.Should().ThrowExactly<KeyNotFoundException>();
        }

        [Fact]
        public void TryGet_ShouldReturnExistingForeignKey()
        {
            var schema = new SqliteDatabaseBuilder().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.ForeignKeys;
            var ix1 = table.Indexes.Create( table.Columns.Create( "C1" ).Asc() );
            var ix2 = table.Indexes.Create( table.Columns.Create( "C2" ).Asc() ).MarkAsUnique();
            var expected = sut.Create( ix1, ix2 );

            var result = ((ISqlForeignKeyBuilderCollection)sut).TryGet( ix1, ix2, out var outResult );

            using ( new AssertionScope() )
            {
                result.Should().BeTrue();
                outResult.Should().BeSameAs( expected );
            }
        }

        [Fact]
        public void TryGet_ShouldReturnFalse_WhenForeignKeyDoesNotExist()
        {
            var schema = new SqliteDatabaseBuilder().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.ForeignKeys;
            var ix1 = table.Indexes.Create( table.Columns.Create( "C1" ).Asc() );
            var ix2 = table.Indexes.Create( table.Columns.Create( "C2" ).Asc() ).MarkAsUnique();
            sut.Create( ix1, ix2 );

            var result = ((ISqlForeignKeyBuilderCollection)sut).TryGet( ix2, ix1, out var outResult );

            using ( new AssertionScope() )
            {
                result.Should().BeFalse();
                outResult.Should().BeNull();
            }
        }

        [Fact]
        public void Remove_ShouldRemoveExistingForeignKey()
        {
            var schema = new SqliteDatabaseBuilder().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.ForeignKeys;
            table.Indexes.Create( table.Columns.Create( "C1" ).Asc() );
            var ix1 = table.Indexes.Get( table.Columns.Get( "C1" ).Asc() );
            var ix2 = table.Indexes.Create( table.Columns.Create( "C2" ).Asc() ).MarkAsUnique();
            var fk = sut.Create( ix1, ix2 );

            var result = sut.Remove( ix1, ix2 );

            using ( new AssertionScope() )
            {
                result.Should().BeTrue();
                fk.IsRemoved.Should().BeTrue();
                sut.Count.Should().Be( 0 );
                ix1.ForeignKeys.Should().BeEmpty();
                ix2.ReferencingForeignKeys.Should().BeEmpty();
                schema.Objects.Contains( fk.Name ).Should().BeFalse();
            }
        }

        [Fact]
        public void Remove_ShouldReturnFalse_WhenForeignKeyDoesNotExist()
        {
            var schema = new SqliteDatabaseBuilder().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.ForeignKeys;
            var ix1 = table.Indexes.Create( table.Columns.Create( "C1" ).Asc() );
            var ix2 = table.Indexes.Create( table.Columns.Create( "C2" ).Asc() ).MarkAsUnique();
            sut.Create( ix1, ix2 );

            var result = sut.Remove( ix2, ix1 );

            using ( new AssertionScope() )
            {
                result.Should().BeFalse();
                sut.Count.Should().Be( 1 );
            }
        }
    }
}
