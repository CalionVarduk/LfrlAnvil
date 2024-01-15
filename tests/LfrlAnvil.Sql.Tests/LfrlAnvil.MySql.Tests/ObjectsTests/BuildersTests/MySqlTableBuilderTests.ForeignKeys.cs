using System.Collections.Generic;
using LfrlAnvil.Functional;
using LfrlAnvil.MySql.Exceptions;
using LfrlAnvil.MySql.Extensions;
using LfrlAnvil.MySql.Objects.Builders;
using LfrlAnvil.MySql.Tests.Helpers;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.MySql.Tests.ObjectsTests.BuildersTests;

public partial class MySqlTableBuilderTests
{
    public class ForeignKeys : TestsBase
    {
        [Fact]
        public void Create_ShouldCreateNewForeignKey_WhenIndexesBelongToTheSameTable()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.ForeignKeys;
            var ix1 = table.Indexes.Create( table.Columns.Create( "C1" ).Asc() );
            var ix2 = table.Indexes.Create( table.Columns.Create( "C2" ).Asc() ).MarkAsUnique();

            var result = ((ISqlForeignKeyBuilderCollection)sut).Create( ix1, ix2 );

            using ( new AssertionScope() )
            {
                result.OriginIndex.Should().BeSameAs( ix1 );
                result.ReferencedIndex.Should().BeSameAs( ix2 );
                result.Name.Should().Be( "FK_T_C1_REF_T" );
                result.FullName.Should().Be( "foo.FK_T_C1_REF_T" );
                result.Database.Should().BeSameAs( table.Database );
                result.Type.Should().Be( SqlObjectType.ForeignKey );
                result.OnUpdateBehavior.Should().Be( ReferenceBehavior.Restrict );
                result.OnDeleteBehavior.Should().Be( ReferenceBehavior.Restrict );
                sut.Count.Should().Be( 1 );
                sut.Should().BeEquivalentTo( result );
                schema.Objects.Contains( result.Name ).Should().BeTrue();

                ((ISqlIndexBuilder)ix1).OriginatingForeignKeys.Should().BeSequentiallyEqualTo( result );
                ix1.ReferencingForeignKeys.Should().BeEmpty();

                ix2.OriginatingForeignKeys.Should().BeEmpty();
                ((ISqlIndexBuilder)ix2).ReferencingForeignKeys.Should().BeSequentiallyEqualTo( result );
            }
        }

        [Fact]
        public void Create_ShouldCreateNewForeignKey_WhenIndexesDoNotBelongToTheSameTable()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var t1 = schema.Objects.CreateTable( "T1" );
            var ix2 = t1.SetPrimaryKey( t1.Columns.Create( "C1" ).Asc() ).Index;
            var t2 = schema.Objects.CreateTable( "T2" );
            var sut = t2.ForeignKeys;
            var ix1 = t2.Indexes.Create( t2.Columns.Create( "C2" ).Asc() );

            var result = ((ISqlForeignKeyBuilderCollection)sut).Create( ix1, ix2 );

            using ( new AssertionScope() )
            {
                result.OriginIndex.Should().BeSameAs( ix1 );
                result.ReferencedIndex.Should().BeSameAs( ix2 );
                result.Name.Should().Be( "FK_T2_C2_REF_T1" );
                result.FullName.Should().Be( "foo.FK_T2_C2_REF_T1" );
                result.Database.Should().BeSameAs( t1.Database );
                result.Type.Should().Be( SqlObjectType.ForeignKey );
                result.OnUpdateBehavior.Should().Be( ReferenceBehavior.Restrict );
                result.OnDeleteBehavior.Should().Be( ReferenceBehavior.Restrict );
                sut.Count.Should().Be( 1 );
                sut.Should().BeEquivalentTo( result );
                schema.Objects.Contains( result.Name ).Should().BeTrue();

                ((ISqlIndexBuilder)ix1).OriginatingForeignKeys.Should().BeSequentiallyEqualTo( result );
                ix1.ReferencingForeignKeys.Should().BeEmpty();

                ix2.OriginatingForeignKeys.Should().BeEmpty();
                ((ISqlIndexBuilder)ix2).ReferencingForeignKeys.Should().BeSequentiallyEqualTo( result );
            }
        }

        [Fact]
        public void Create_ShouldCreateNewForeignKey_WhenIndexesDoNotBelongToTheSameSchema()
        {
            var db = MySqlDatabaseBuilderMock.Create();
            var schema1 = db.Schemas.Create( "foo" );
            var schema2 = db.Schemas.Create( "bar" );
            var t1 = schema1.Objects.CreateTable( "T1" );
            var ix2 = t1.SetPrimaryKey( t1.Columns.Create( "C1" ).Asc() ).Index;
            var t2 = schema2.Objects.CreateTable( "T2" );
            var sut = t2.ForeignKeys;
            var ix1 = t2.Indexes.Create( t2.Columns.Create( "C2" ).Asc() );

            var result = ((ISqlForeignKeyBuilderCollection)sut).Create( ix1, ix2 );

            using ( new AssertionScope() )
            {
                result.OriginIndex.Should().BeSameAs( ix1 );
                result.ReferencedIndex.Should().BeSameAs( ix2 );
                result.Name.Should().Be( "FK_T2_C2_REF_foo_T1" );
                result.FullName.Should().Be( "bar.FK_T2_C2_REF_foo_T1" );
                result.Database.Should().BeSameAs( t1.Database );
                result.Type.Should().Be( SqlObjectType.ForeignKey );
                result.OnUpdateBehavior.Should().Be( ReferenceBehavior.Restrict );
                result.OnDeleteBehavior.Should().Be( ReferenceBehavior.Restrict );
                sut.Count.Should().Be( 1 );
                sut.Should().BeEquivalentTo( result );
                schema2.Objects.Contains( result.Name ).Should().BeTrue();

                ((ISqlIndexBuilder)ix1).OriginatingForeignKeys.Should().BeSequentiallyEqualTo( result );
                ix1.ReferencingForeignKeys.Should().BeEmpty();

                ix2.OriginatingForeignKeys.Should().BeEmpty();
                ((ISqlIndexBuilder)ix2).ReferencingForeignKeys.Should().BeSequentiallyEqualTo( result );
            }
        }

        [Fact]
        public void Create_ShouldThrowMySqlObjectBuilderException_WhenTableIsRemoved()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.ForeignKeys;
            var ix1 = table.Indexes.Create( table.Columns.Create( "C1" ).Asc() );
            var ix2 = table.Indexes.Create( table.Columns.Create( "C2" ).Asc() ).MarkAsUnique();
            table.Remove();

            var action = Lambda.Of( () => sut.Create( ix1, ix2 ) );

            action.Should()
                .ThrowExactly<MySqlObjectBuilderException>()
                .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
        }

        [Fact]
        public void Create_ShouldThrowMySqlObjectBuilderException_WhenForeignKeyAlreadyExists()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.ForeignKeys;
            var ix1 = table.Indexes.Create( table.Columns.Create( "C1" ).Asc() );
            var ix2 = table.Indexes.Create( table.Columns.Create( "C2" ).Asc() ).MarkAsUnique();
            sut.Create( ix1, ix2 );

            var action = Lambda.Of( () => sut.Create( ix1, ix2 ) );

            action.Should()
                .ThrowExactly<MySqlObjectBuilderException>()
                .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
        }

        [Fact]
        public void Create_ShouldThrowMySqlObjectBuilderException_WhenSchemaObjectWithDefaultForeignKeyNameAlreadyExists()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.ForeignKeys;
            var ix1 = table.Indexes.Create( table.Columns.Create( "C1" ).Asc() ).SetName( "FK_T_C1_REF_T" );
            var ix2 = table.Indexes.Create( table.Columns.Create( "C2" ).Asc() ).MarkAsUnique();

            var action = Lambda.Of( () => sut.Create( ix1, ix2 ) );

            action.Should()
                .ThrowExactly<MySqlObjectBuilderException>()
                .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
        }

        [Fact]
        public void Create_ShouldThrowMySqlObjectBuilderException_WhenIndexAndReferencedIndexAreTheSame()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.ForeignKeys;
            var ix1 = table.Indexes.Create( table.Columns.Create( "C1" ).Asc() ).MarkAsUnique();

            var action = Lambda.Of( () => sut.Create( ix1, ix1 ) );

            action.Should()
                .ThrowExactly<MySqlObjectBuilderException>()
                .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
        }

        [Fact]
        public void Create_ShouldThrowMySqlObjectBuilderException_WhenReferencedIndexIsNotUnique()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.ForeignKeys;
            var ix1 = table.Indexes.Create( table.Columns.Create( "C1" ).Asc() );
            var ix2 = table.Indexes.Create( table.Columns.Create( "C2" ).Asc() );

            var action = Lambda.Of( () => sut.Create( ix1, ix2 ) );

            action.Should()
                .ThrowExactly<MySqlObjectBuilderException>()
                .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
        }

        [Fact]
        public void Create_ShouldThrowSqliteObjectBuilderException_WhenReferencedIndexHasFilter()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.ForeignKeys;
            var ix1 = table.Indexes.Create( table.Columns.Create( "C1" ).Asc() );
            var ix2 = table.Indexes.Create( table.Columns.Create( "C2" ).Asc() ).MarkAsUnique().SetFilter( SqlNode.True() );

            var action = Lambda.Of( () => sut.Create( ix1, ix2 ) );

            action.Should()
                .ThrowExactly<MySqlObjectBuilderException>()
                .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
        }

        [Fact]
        public void Create_ShouldThrowMySqlObjectBuilderException_WhenIndexBelongsToAnotherTable()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var t1 = schema.Objects.CreateTable( "T1" );
            var sut = t1.ForeignKeys;
            var ix1 = t1.SetPrimaryKey( t1.Columns.Create( "C1" ).Asc() ).Index;
            var t2 = schema.Objects.CreateTable( "T2" );
            var ix2 = t2.Indexes.Create( t2.Columns.Create( "C2" ).Asc() );

            var action = Lambda.Of( () => sut.Create( ix2, ix1 ) );

            action.Should()
                .ThrowExactly<MySqlObjectBuilderException>()
                .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
        }

        [Fact]
        public void Create_ShouldThrowMySqlObjectBuilderException_WhenReferencedIndexBelongsToAnotherDatabase()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var otherSchema = MySqlDatabaseBuilderMock.Create().Schemas.Default;
            var t1 = schema.Objects.CreateTable( "T1" );
            var sut = t1.ForeignKeys;
            var ix1 = t1.SetPrimaryKey( t1.Columns.Create( "C1" ).Asc() ).Index;
            var t2 = otherSchema.Objects.CreateTable( "T2" );
            var ix2 = t2.Indexes.Create( t2.Columns.Create( "C2" ).Asc() ).MarkAsUnique();

            var action = Lambda.Of( () => sut.Create( ix1, ix2 ) );

            action.Should()
                .ThrowExactly<MySqlObjectBuilderException>()
                .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
        }

        [Fact]
        public void Create_ShouldThrowMySqlObjectBuilderException_WhenIndexIsRemoved()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.ForeignKeys;
            var ix1 = table.Indexes.Create( table.Columns.Create( "C1" ).Asc() );
            var ix2 = table.Indexes.Create( table.Columns.Create( "C2" ).Asc() ).MarkAsUnique();
            ix1.Remove();

            var action = Lambda.Of( () => sut.Create( ix1, ix2 ) );

            action.Should()
                .ThrowExactly<MySqlObjectBuilderException>()
                .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 2 );
        }

        [Fact]
        public void Create_ShouldThrowMySqlObjectBuilderException_WhenReferencedIndexIsRemoved()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.ForeignKeys;
            var ix1 = table.Indexes.Create( table.Columns.Create( "C1" ).Asc() );
            var ix2 = table.Indexes.Create( table.Columns.Create( "C2" ).Asc() ).MarkAsUnique();
            ix2.Remove();

            var action = Lambda.Of( () => sut.Create( ix1, ix2 ) );

            action.Should()
                .ThrowExactly<MySqlObjectBuilderException>()
                .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 2 );
        }

        [Fact]
        public void Create_ShouldThrowMySqlObjectBuilderException_WhenReferencedIndexContainsNullableColumn()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
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
                .ThrowExactly<MySqlObjectBuilderException>()
                .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
        }

        [Fact]
        public void Create_ShouldThrowMySqlObjectBuilderException_WhenIndexAndReferencedIndexHaveDifferentAmountOfColumns()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.ForeignKeys;
            var ix1 = table.Indexes.Create( table.Columns.Create( "C1" ).Asc(), table.Columns.Create( "C2" ).Asc() );
            var ix2 = table.Indexes.Create( table.Columns.Create( "C3" ).Asc() ).MarkAsUnique();

            var action = Lambda.Of( () => sut.Create( ix1, ix2 ) );

            action.Should()
                .ThrowExactly<MySqlObjectBuilderException>()
                .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
        }

        [Fact]
        public void Create_ShouldThrowMySqlObjectBuilderException_WhenAtLeastOneIndexAndReferencedIndexColumnPairHasIncompatibleTypes()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
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
                .ThrowExactly<MySqlObjectBuilderException>()
                .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
        }

        [Fact]
        public void Create_ShouldThrowSqlObjectCastException_WhenIndexIsOfInvalidType()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.ForeignKeys;
            var ix1 = Substitute.For<ISqlIndexBuilder>();
            var ix2 = table.Indexes.Create( table.Columns.Create( "C2" ).Asc() ).MarkAsUnique();

            var action = Lambda.Of( () => ((ISqlForeignKeyBuilderCollection)sut).Create( ix1, ix2 ) );

            action.Should()
                .ThrowExactly<SqlObjectCastException>()
                .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Expected == typeof( MySqlIndexBuilder ) );
        }

        [Fact]
        public void Create_ShouldThrowSqlObjectCastException_WhenReferencedIndexIsOfInvalidType()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.ForeignKeys;
            var ix1 = table.Indexes.Create( table.Columns.Create( "C1" ).Asc() );
            var ix2 = Substitute.For<ISqlIndexBuilder>();

            var action = Lambda.Of( () => ((ISqlForeignKeyBuilderCollection)sut).Create( ix1, ix2 ) );

            action.Should()
                .ThrowExactly<SqlObjectCastException>()
                .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Expected == typeof( MySqlIndexBuilder ) );
        }

        [Fact]
        public void GetOrCreate_ShouldCreateNewForeignKey_WhenForeignKeyDoesNotExist()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.ForeignKeys;
            var ix1 = table.Indexes.Create( table.Columns.Create( "C1" ).Asc() );
            var ix2 = table.Indexes.Create( table.Columns.Create( "C2" ).Asc() ).MarkAsUnique();

            var result = ((ISqlForeignKeyBuilderCollection)sut).GetOrCreate( ix1, ix2 );

            using ( new AssertionScope() )
            {
                result.OriginIndex.Should().BeSameAs( ix1 );
                result.ReferencedIndex.Should().BeSameAs( ix2 );
                result.Name.Should().Be( "FK_T_C1_REF_T" );
                result.FullName.Should().Be( "foo.FK_T_C1_REF_T" );
                result.Database.Should().BeSameAs( table.Database );
                result.Type.Should().Be( SqlObjectType.ForeignKey );
                result.OnUpdateBehavior.Should().Be( ReferenceBehavior.Restrict );
                result.OnDeleteBehavior.Should().Be( ReferenceBehavior.Restrict );
                sut.Count.Should().Be( 1 );
                sut.Should().BeEquivalentTo( result );
                schema.Objects.Contains( result.Name ).Should().BeTrue();

                ((ISqlIndexBuilder)ix1).OriginatingForeignKeys.Should().BeSequentiallyEqualTo( result );
                ix1.ReferencingForeignKeys.Should().BeEmpty();

                ix2.OriginatingForeignKeys.Should().BeEmpty();
                ((ISqlIndexBuilder)ix2).ReferencingForeignKeys.Should().BeSequentiallyEqualTo( result );
            }
        }

        [Fact]
        public void GetOrCreate_ShouldReturnExistingForeignKey_WhenForeignKeyAlreadyExists()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
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
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
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
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
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
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
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
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
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
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
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
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
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
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
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
                ix1.OriginatingForeignKeys.Should().BeEmpty();
                ix2.ReferencingForeignKeys.Should().BeEmpty();
                schema.Objects.Contains( fk.Name ).Should().BeFalse();
            }
        }

        [Fact]
        public void Remove_ShouldReturnFalse_WhenForeignKeyDoesNotExist()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
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
