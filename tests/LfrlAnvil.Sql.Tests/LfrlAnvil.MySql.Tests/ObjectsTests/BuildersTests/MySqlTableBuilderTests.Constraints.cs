using System.Collections.Generic;
using LfrlAnvil.Functional;
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
    public class Constraints : TestsBase
    {
        [Theory]
        [InlineData( false, "IX_T_C1A_C2D" )]
        [InlineData( true, "UIX_T_C1A_C2D" )]
        public void CreateIndex_ShouldCreateNewIndex(bool isUnique, string expectedName)
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Constraints;
            var c1 = table.Columns.Create( "C1" );
            var c2 = table.Columns.Create( "C2" );
            var ixc1 = c1.Asc();
            var ixc2 = c2.Desc();

            var result = sut.CreateIndex( new[] { ixc1, ixc2 }, isUnique );

            using ( new AssertionScope() )
            {
                result.Table.Should().BeSameAs( table );
                result.Database.Should().BeSameAs( table.Database );
                result.Type.Should().Be( SqlObjectType.Index );
                result.Name.Should().Be( expectedName );
                result.Columns.Expressions.Should().BeSequentiallyEqualTo( ixc1, ixc2 );
                result.ReferencedColumns.Should().BeSequentiallyEqualTo( c1, c2 );
                result.ReferencedFilterColumns.Should().BeEmpty();
                result.PrimaryKey.Should().BeNull();
                result.IsUnique.Should().Be( isUnique );
                result.IsVirtual.Should().BeFalse();
                result.Filter.Should().BeNull();
                result.ReferencingObjects.Should().BeEmpty();

                sut.Count.Should().Be( 1 );
                sut.Should().BeSequentiallyEqualTo( result );
                schema.Objects.TryGet( result.Name ).Should().BeSameAs( result );

                c1.ReferencingObjects.Should()
                    .BeSequentiallyEqualTo(
                        SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( result ), c1 ) );

                c2.ReferencingObjects.Should()
                    .BeSequentiallyEqualTo(
                        SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( result ), c2 ) );
            }
        }

        [Theory]
        [InlineData( false )]
        [InlineData( true )]
        public void CreateIndex_WithExplicitName_ShouldCreateNewIndex(bool isUnique)
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Constraints;
            var c1 = table.Columns.Create( "C1" );
            var c2 = table.Columns.Create( "C2" );
            var ixc1 = c1.Asc();
            var ixc2 = c2.Desc();

            var result = sut.CreateIndex( "IX_T", new[] { ixc1, ixc2 }, isUnique );

            using ( new AssertionScope() )
            {
                result.Table.Should().BeSameAs( table );
                result.Database.Should().BeSameAs( table.Database );
                result.Type.Should().Be( SqlObjectType.Index );
                result.Name.Should().Be( "IX_T" );
                result.Columns.Expressions.Should().BeSequentiallyEqualTo( ixc1, ixc2 );
                result.ReferencedColumns.Should().BeSequentiallyEqualTo( c1, c2 );
                result.ReferencedFilterColumns.Should().BeEmpty();
                result.PrimaryKey.Should().BeNull();
                result.IsUnique.Should().Be( isUnique );
                result.IsVirtual.Should().BeFalse();
                result.Filter.Should().BeNull();
                result.ReferencingObjects.Should().BeEmpty();

                sut.Count.Should().Be( 1 );
                sut.Should().BeSequentiallyEqualTo( result );
                schema.Objects.TryGet( result.Name ).Should().BeSameAs( result );

                c1.ReferencingObjects.Should()
                    .BeSequentiallyEqualTo(
                        SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( result ), c1 ) );

                c2.ReferencingObjects.Should()
                    .BeSequentiallyEqualTo(
                        SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( result ), c2 ) );
            }
        }

        [Fact]
        public void CreateIndex_ShouldCreateNewIndex_WithExpressions()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Constraints;
            var c1 = table.Columns.Create( "C1" );
            var c2 = table.Columns.Create( "C2" );
            var ixc1 = c1.Asc();
            var ixc2 = (c2.Node + SqlNode.Literal( 1 )).Desc();
            var ixc3 = (c1.Node + c2.Node).Asc();

            var result = sut.CreateIndex( ixc1, ixc2, ixc3 );

            using ( new AssertionScope() )
            {
                result.Table.Should().BeSameAs( table );
                result.Database.Should().BeSameAs( table.Database );
                result.Type.Should().Be( SqlObjectType.Index );
                result.Name.Should().Be( "IX_T_C1A_E1D_E2A" );
                result.Columns.Expressions.Should().BeSequentiallyEqualTo( ixc1, ixc2, ixc3 );
                result.ReferencedColumns.Should().BeSequentiallyEqualTo( c1, c2 );
                result.ReferencedFilterColumns.Should().BeEmpty();
                result.PrimaryKey.Should().BeNull();
                result.IsUnique.Should().BeFalse();
                result.IsVirtual.Should().BeFalse();
                result.Filter.Should().BeNull();
                result.ReferencingObjects.Should().BeEmpty();

                sut.Count.Should().Be( 1 );
                sut.Should().BeSequentiallyEqualTo( result );
                schema.Objects.TryGet( result.Name ).Should().BeSameAs( result );

                c1.ReferencingObjects.Should()
                    .BeSequentiallyEqualTo(
                        SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( result ), c1 ) );

                c2.ReferencingObjects.Should()
                    .BeSequentiallyEqualTo(
                        SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( result ), c2 ) );
            }
        }

        [Fact]
        public void CreateIndex_ShouldThrowSqlObjectBuilderException_WhenTableIsRemoved()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Constraints;
            var column = table.Columns.Create( "C" ).Asc();
            table.Remove();

            var action = Lambda.Of( () => sut.CreateIndex( column ) );

            action.Should()
                .ThrowExactly<SqlObjectBuilderException>()
                .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
        }

        [Fact]
        public void CreateIndex_ShouldThrowSqlObjectBuilderException_WhenSchemaObjectNameAlreadyExists()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Constraints;
            var column = table.Columns.Create( "C" ).Asc();

            var action = Lambda.Of( () => sut.CreateIndex( "T", column ) );

            action.Should()
                .ThrowExactly<SqlObjectBuilderException>()
                .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
        }

        [Fact]
        public void CreateIndex_ShouldThrowSqlObjectBuilderException_WhenColumnsAreEmpty()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Constraints;

            var action = Lambda.Of( () => sut.CreateIndex() );

            action.Should()
                .ThrowExactly<SqlObjectBuilderException>()
                .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
        }

        [Fact]
        public void CreateIndex_ShouldThrowSqlObjectBuilderException_WhenAtLeastOneColumnBelongsToAnotherTable()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var t1 = schema.Objects.CreateTable( "T1" );
            var c1 = t1.Columns.Create( "C1" );
            t1.Constraints.SetPrimaryKey( c1.Asc() );

            var t2 = schema.Objects.CreateTable( "T2" );
            var sut = t2.Constraints;

            var action = Lambda.Of( () => sut.CreateIndex( c1.Asc() ) );

            action.Should()
                .ThrowExactly<SqlObjectBuilderException>()
                .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
        }

        [Fact]
        public void CreateIndex_ShouldThrowSqlObjectBuilderException_WhenAtLeastOneColumnIsRemoved()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var column = table.Columns.Create( "C" );
            var sut = table.Constraints;
            var ixColumn = column.Asc();
            column.Remove();

            var action = Lambda.Of( () => sut.CreateIndex( ixColumn ) );

            action.Should()
                .ThrowExactly<SqlObjectBuilderException>()
                .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
        }

        [Fact]
        public void CreateIndex_ShouldThrowSqlObjectBuilderException_WhenAtLeastOneColumnIsDuplicated()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var column = table.Columns.Create( "C" );
            var sut = table.Constraints;

            var action = Lambda.Of( () => sut.CreateIndex( column.Asc(), column.Desc() ) );

            action.Should()
                .ThrowExactly<SqlObjectBuilderException>()
                .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
        }

        [Fact]
        public void CreateIndex_ShouldThrowSqlObjectBuilderException_WhenUniqueIndexContainsExpressions()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var c1 = table.Columns.Create( "C1" );
            var c2 = table.Columns.Create( "C2" );
            var sut = table.Constraints;

            var action = Lambda.Of( () => sut.CreateUniqueIndex( c1.Asc(), (c2.Node + SqlNode.Literal( 1 )).Desc() ) );

            action.Should()
                .ThrowExactly<SqlObjectBuilderException>()
                .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
        }

        [Theory]
        [InlineData( "" )]
        [InlineData( " " )]
        [InlineData( "`" )]
        [InlineData( "'" )]
        [InlineData( "f`oo" )]
        public void CreateIndex_ShouldThrowSqlObjectBuilderException_WhenNameIsInvalid(string name)
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Constraints;
            var c = table.Columns.Create( "C" ).Asc();

            var action = Lambda.Of( () => sut.CreateIndex( name, c ) );

            action.Should()
                .ThrowExactly<SqlObjectBuilderException>()
                .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
        }

        [Fact]
        public void SetPrimaryKey_ShouldCreatePrimaryKey_WhenTableDoesNotHaveOne()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Constraints;
            var column = table.Columns.Create( "C" );
            var ixColumn = column.Asc();

            var result = sut.SetPrimaryKey( ixColumn );

            using ( new AssertionScope() )
            {
                result.Table.Should().BeSameAs( table );
                result.Database.Should().BeSameAs( table.Database );
                result.Type.Should().Be( SqlObjectType.PrimaryKey );
                result.Name.Should().Be( "PK_T" );
                result.Index.Table.Should().BeSameAs( table );
                result.Index.Database.Should().BeSameAs( schema.Database );
                result.Index.Type.Should().Be( SqlObjectType.Index );
                result.Index.Name.Should().Be( "UIX_T_CA" );
                result.Index.Columns.Expressions.Should().BeSequentiallyEqualTo( ixColumn );
                result.Index.ReferencedColumns.Should().BeSequentiallyEqualTo( column );
                result.Index.ReferencedFilterColumns.Should().BeEmpty();
                result.Index.PrimaryKey.Should().BeSameAs( result );
                result.Index.IsUnique.Should().BeTrue();
                result.Index.IsVirtual.Should().BeTrue();
                result.Index.Filter.Should().BeNull();
                result.ReferencingObjects.Should().BeEmpty();

                sut.Count.Should().Be( 2 );
                sut.Should().BeEquivalentTo( result, result.Index );
                sut.TryGetPrimaryKey().Should().BeSameAs( result );
                schema.Objects.TryGet( result.Name ).Should().BeSameAs( result );
                schema.Objects.TryGet( result.Index.Name ).Should().BeSameAs( result.Index );

                column.ReferencingObjects.Should()
                    .BeSequentiallyEqualTo(
                        SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( result.Index ), column ) );
            }
        }

        [Fact]
        public void SetPrimaryKey_ShouldDoNothing_WhenPrimaryKeyIndexAndPrimaryKeyNameDoNotChange()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Constraints;
            var c1 = table.Columns.Create( "C1" );
            var c2 = table.Columns.Create( "C2" );
            var c3 = table.Columns.Create( "C3" );
            var oldPk = table.Constraints.SetPrimaryKey( c1.Asc(), c2.Asc(), c3.Asc() );

            var result = sut.SetPrimaryKey( oldPk.Name, oldPk.Index );

            using ( new AssertionScope() )
            {
                result.Should().BeSameAs( oldPk );
                result.Should().BeSameAs( table.Constraints.TryGetPrimaryKey() );
            }
        }

        [Fact]
        public void SetPrimaryKey_ShouldUpdatePrimaryKey_WhenPrimaryKeyNameChanges()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Constraints;
            var c1 = table.Columns.Create( "C1" );
            var c2 = table.Columns.Create( "C2" );
            var oldPk = sut.SetPrimaryKey( c1.Asc(), c2.Asc() );

            var result = sut.SetPrimaryKey( "PK_NEW", oldPk.Index );

            using ( new AssertionScope() )
            {
                result.Should().BeSameAs( oldPk );
                result.Should().BeSameAs( table.Constraints.TryGetPrimaryKey() );
                result.Name.Should().Be( "PK_NEW" );
            }
        }

        [Fact]
        public void SetPrimaryKey_ShouldCreatePrimaryKeyAndRemoveOldOne_WhenPrimaryKeyIndexChanges()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Constraints;
            var c1 = table.Columns.Create( "C1" );
            var c2 = table.Columns.Create( "C2" );
            var oldPk = sut.SetPrimaryKey( c1.Asc() );
            var ixColumn2 = c2.Asc();

            var result = sut.SetPrimaryKey( oldPk.Name, ixColumn2 );

            using ( new AssertionScope() )
            {
                result.Table.Should().BeSameAs( table );
                result.Database.Should().BeSameAs( table.Database );
                result.Type.Should().Be( SqlObjectType.PrimaryKey );
                result.Name.Should().Be( "PK_T" );
                result.Index.Table.Should().BeSameAs( table );
                result.Index.Database.Should().BeSameAs( schema.Database );
                result.Index.Type.Should().Be( SqlObjectType.Index );
                result.Index.Name.Should().Be( "UIX_T_C2A" );
                result.Index.Columns.Expressions.Should().BeSequentiallyEqualTo( ixColumn2 );
                result.Index.ReferencedColumns.Should().BeSequentiallyEqualTo( c2 );
                result.Index.ReferencedFilterColumns.Should().BeEmpty();
                result.Index.PrimaryKey.Should().BeSameAs( result );
                result.Index.IsUnique.Should().BeTrue();
                result.Index.IsVirtual.Should().BeTrue();
                result.Index.Filter.Should().BeNull();
                result.ReferencingObjects.Should().BeEmpty();

                sut.Count.Should().Be( 2 );
                sut.Should().BeEquivalentTo( result, result.Index );
                sut.TryGetPrimaryKey().Should().BeSameAs( result );
                schema.Objects.TryGet( result.Name ).Should().BeSameAs( result );
                schema.Objects.TryGet( result.Index.Name ).Should().BeSameAs( result.Index );
                schema.Objects.TryGet( oldPk.Index.Name ).Should().BeNull();

                oldPk.IsRemoved.Should().BeTrue();
                oldPk.Index.IsRemoved.Should().BeTrue();
            }
        }

        [Fact]
        public void SetPrimaryKey_ShouldCreatePrimaryKeyAndRemoveOldOne_WhenPrimaryKeyIndexAndPrimaryKeyNameChanges()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Constraints;
            var c1 = table.Columns.Create( "C1" );
            var c2 = table.Columns.Create( "C2" );
            var oldPk = sut.SetPrimaryKey( c1.Asc() );
            var ixColumn2 = c2.Asc();

            var result = sut.SetPrimaryKey( "PK_NEW", ixColumn2 );

            using ( new AssertionScope() )
            {
                result.Table.Should().BeSameAs( table );
                result.Database.Should().BeSameAs( table.Database );
                result.Type.Should().Be( SqlObjectType.PrimaryKey );
                result.Name.Should().Be( "PK_NEW" );
                result.Index.Table.Should().BeSameAs( table );
                result.Index.Database.Should().BeSameAs( schema.Database );
                result.Index.Type.Should().Be( SqlObjectType.Index );
                result.Index.Name.Should().Be( "UIX_T_C2A" );
                result.Index.Columns.Expressions.Should().BeSequentiallyEqualTo( ixColumn2 );
                result.Index.ReferencedColumns.Should().BeSequentiallyEqualTo( c2 );
                result.Index.ReferencedFilterColumns.Should().BeEmpty();
                result.Index.PrimaryKey.Should().BeSameAs( result );
                result.Index.IsUnique.Should().BeTrue();
                result.Index.IsVirtual.Should().BeTrue();
                result.Index.Filter.Should().BeNull();
                result.ReferencingObjects.Should().BeEmpty();

                sut.Count.Should().Be( 2 );
                sut.Should().BeEquivalentTo( result, result.Index );
                sut.TryGetPrimaryKey().Should().BeSameAs( result );
                schema.Objects.TryGet( result.Name ).Should().BeSameAs( result );
                schema.Objects.TryGet( result.Index.Name ).Should().BeSameAs( result.Index );
                schema.Objects.TryGet( oldPk.Name ).Should().BeNull();
                schema.Objects.TryGet( oldPk.Index.Name ).Should().BeNull();

                oldPk.IsRemoved.Should().BeTrue();
                oldPk.Index.IsRemoved.Should().BeTrue();
            }
        }

        [Fact]
        public void
            SetPrimaryKey_ShouldCreatePrimaryKeyAndRemoveOldOne_WhenPrimaryKeyIndexAndPrimaryKeyNameChangesToOldPrimaryKeyIndexName()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Constraints;
            var c1 = table.Columns.Create( "C1" );
            var c2 = table.Columns.Create( "C2" );
            var oldPk = sut.SetPrimaryKey( c1.Asc() );
            var ixColumn2 = c2.Asc();

            var result = sut.SetPrimaryKey( oldPk.Index.Name, ixColumn2 );

            using ( new AssertionScope() )
            {
                result.Table.Should().BeSameAs( table );
                result.Database.Should().BeSameAs( table.Database );
                result.Type.Should().Be( SqlObjectType.PrimaryKey );
                result.Name.Should().Be( "UIX_T_C1A" );
                result.Index.Table.Should().BeSameAs( table );
                result.Index.Database.Should().BeSameAs( schema.Database );
                result.Index.Type.Should().Be( SqlObjectType.Index );
                result.Index.Name.Should().Be( "UIX_T_C2A" );
                result.Index.Columns.Expressions.Should().BeSequentiallyEqualTo( ixColumn2 );
                result.Index.ReferencedColumns.Should().BeSequentiallyEqualTo( c2 );
                result.Index.ReferencedFilterColumns.Should().BeEmpty();
                result.Index.PrimaryKey.Should().BeSameAs( result );
                result.Index.IsUnique.Should().BeTrue();
                result.Index.IsVirtual.Should().BeTrue();
                result.Index.Filter.Should().BeNull();
                result.ReferencingObjects.Should().BeEmpty();

                sut.Count.Should().Be( 2 );
                sut.Should().BeEquivalentTo( result, result.Index );
                sut.TryGetPrimaryKey().Should().BeSameAs( result );
                schema.Objects.TryGet( result.Name ).Should().BeSameAs( result );
                schema.Objects.TryGet( result.Index.Name ).Should().BeSameAs( result.Index );
                schema.Objects.TryGet( oldPk.Name ).Should().BeNull();

                oldPk.IsRemoved.Should().BeTrue();
                oldPk.Index.IsRemoved.Should().BeTrue();
            }
        }

        [Fact]
        public void SetPrimaryKey_ShouldThrowSqlObjectBuilderException_WhenCurrentPrimaryKeyIndexCannotBeRemoved()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Constraints;
            var c1 = table.Columns.Create( "C1" );
            var c2 = table.Columns.Create( "C2" );
            var oldPk = sut.SetPrimaryKey( c1.Asc() );
            sut.CreateForeignKey( sut.CreateIndex( table.Columns.Create( "C3" ).Asc() ), oldPk.Index );

            var action = Lambda.Of( () => sut.SetPrimaryKey( c2.Asc() ) );

            action.Should()
                .ThrowExactly<SqlObjectBuilderException>()
                .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
        }

        [Fact]
        public void SetPrimaryKey_ShouldThrowSqlObjectBuilderException_WhenPrimaryKeyNameExistsDuringCreationInSchemaObjects()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Constraints;
            var c1 = table.Columns.Create( "C1" );
            var index = sut.CreateUniqueIndex( c1.Asc() );

            var action = Lambda.Of( () => sut.SetPrimaryKey( index.Name, index ) );

            action.Should()
                .ThrowExactly<SqlObjectBuilderException>()
                .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
        }

        [Fact]
        public void SetPrimaryKey_ShouldThrowSqlObjectBuilderException_WhenPrimaryKeyNameExistsDuringUpdateInSchemaObjects()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Constraints;
            var c1 = table.Columns.Create( "C1" );
            var c2 = table.Columns.Create( "C2" );
            var index = sut.CreateUniqueIndex( c1.Asc() );
            sut.SetPrimaryKey( c2.Asc() );

            var action = Lambda.Of( () => sut.SetPrimaryKey( index.Name, index ) );

            action.Should()
                .ThrowExactly<SqlObjectBuilderException>()
                .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
        }

        [Fact]
        public void SetPrimaryKey_ShouldThrowSqlObjectBuilderException_WhenIndexIsRemoved()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Default;
            var table = schema.Objects.CreateTable( Fixture.Create<string>() );
            var column = table.Columns.Create( "C" );
            var index = table.Constraints.CreateUniqueIndex( "UIX", column.Asc() );
            index.Remove();

            var action = Lambda.Of( () => table.Constraints.SetPrimaryKey( index ) );

            action.Should()
                .ThrowExactly<SqlObjectBuilderException>()
                .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
        }

        [Fact]
        public void SetPrimaryKey_ShouldThrowSqlObjectBuilderException_WhenIndexIsNotUnique()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Default;
            var table = schema.Objects.CreateTable( Fixture.Create<string>() );
            var column = table.Columns.Create( "C" );
            var index = table.Constraints.CreateIndex( column.Asc() );

            var action = Lambda.Of( () => table.Constraints.SetPrimaryKey( index ) );

            action.Should()
                .ThrowExactly<SqlObjectBuilderException>()
                .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
        }

        [Fact]
        public void SetPrimaryKey_ShouldThrowSqlObjectBuilderException_WhenIndexContainsExpressions()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Constraints;
            var column = table.Columns.Create( "C" );
            var index = sut.CreateIndex( (column.Node + SqlNode.Literal( 1 )).Asc() );

            var action = Lambda.Of( () => sut.SetPrimaryKey( index ) );

            action.Should()
                .ThrowExactly<SqlObjectBuilderException>()
                .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 2 );
        }

        [Fact]
        public void SetPrimaryKey_ShouldThrowSqlObjectBuilderException_WhenIndexIsPartial()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Default;
            var table = schema.Objects.CreateTable( Fixture.Create<string>() );
            var column = table.Columns.Create( "C" );
            var index = table.Constraints.CreateUniqueIndex( column.Asc() ).SetFilter( SqlNode.True() );

            var action = Lambda.Of( () => table.Constraints.SetPrimaryKey( index ) );

            action.Should()
                .ThrowExactly<SqlObjectBuilderException>()
                .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
        }

        [Fact]
        public void SetPrimaryKey_ShouldThrowSqlObjectBuilderException_WhenIndexBelongsToAnotherTable()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Default;
            var otherTable = schema.Objects.CreateTable( "T" );
            var index = otherTable.Constraints.SetPrimaryKey( otherTable.Columns.Create( "C" ).Asc() ).Index;
            var table = schema.Objects.CreateTable( Fixture.Create<string>() );

            var action = Lambda.Of( () => table.Constraints.SetPrimaryKey( index ) );

            action.Should()
                .ThrowExactly<SqlObjectBuilderException>()
                .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
        }

        [Fact]
        public void SetPrimaryKey_ShouldThrowSqlObjectBuilderException_WhenAtLeastOneColumnIsNullable()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Default;
            var table = schema.Objects.CreateTable( Fixture.Create<string>() );
            var c1 = table.Columns.Create( "C1" );
            var c2 = table.Columns.Create( "C2" ).MarkAsNullable();
            var index = table.Constraints.CreateUniqueIndex( c1.Asc(), c2.Asc() );

            var action = Lambda.Of( () => table.Constraints.SetPrimaryKey( index ) );

            action.Should()
                .ThrowExactly<SqlObjectBuilderException>()
                .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
        }

        [Fact]
        public void SetPrimaryKey_ShouldThrowSqlObjectBuilderException_WhenAtLeastOneColumnIsGenerated()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Constraints;
            var c1 = table.Columns.Create( "C1" );
            var c2 = table.Columns.Create( "C2" ).SetComputation( SqlColumnComputation.Virtual( SqlNode.Literal( 1 ) ) );
            var index = sut.CreateUniqueIndex( c1.Asc(), c2.Asc() );

            var action = Lambda.Of( () => sut.SetPrimaryKey( index ) );

            action.Should()
                .ThrowExactly<SqlObjectBuilderException>()
                .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
        }

        [Fact]
        public void SetPrimaryKey_ShouldThrowSqlObjectBuilderException_WhenTableHasBeenRemoved()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Default;
            var table = schema.Objects.CreateTable( Fixture.Create<string>() );
            var column = table.Columns.Create( "C" );
            var index = table.Constraints.CreateUniqueIndex( column.Asc() );
            schema.Objects.Remove( table.Name );

            var action = Lambda.Of( () => table.Constraints.SetPrimaryKey( index ) );

            action.Should()
                .ThrowExactly<SqlObjectBuilderException>()
                .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
        }

        [Theory]
        [InlineData( "" )]
        [InlineData( " " )]
        [InlineData( "`" )]
        [InlineData( "'" )]
        [InlineData( "f`oo" )]
        public void SetPrimaryKey_ShouldThrowSqlObjectBuilderException_WhenNameIsInvalid(string name)
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var index = table.Constraints.CreateUniqueIndex( table.Columns.Create( "C" ).Asc() );

            var action = Lambda.Of( () => table.Constraints.SetPrimaryKey( name, index ) );

            action.Should()
                .ThrowExactly<SqlObjectBuilderException>()
                .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
        }

        [Fact]
        public void CreateForeignKey_ShouldCreateNewForeignKey_WhenIndexesBelongToTheSameTable()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Constraints;
            var ix1 = sut.CreateIndex( "IX_T_C1", table.Columns.Create( "C1" ).Asc() );
            var ix2 = sut.CreateUniqueIndex( "UIX_T_C2", table.Columns.Create( "C2" ).Asc() );

            var result = sut.CreateForeignKey( ix1, ix2 );

            using ( new AssertionScope() )
            {
                result.Table.Should().BeSameAs( table );
                result.Database.Should().BeSameAs( table.Database );
                result.Type.Should().Be( SqlObjectType.ForeignKey );
                result.Name.Should().Be( "FK_T_C1_REF_T" );
                result.OriginIndex.Should().BeSameAs( ix1 );
                result.ReferencedIndex.Should().BeSameAs( ix2 );
                result.OnUpdateBehavior.Should().Be( ReferenceBehavior.Restrict );
                result.OnDeleteBehavior.Should().Be( ReferenceBehavior.Restrict );
                result.ReferencingObjects.Should().BeEmpty();

                sut.Count.Should().Be( 3 );
                sut.Should().BeEquivalentTo( ix1, ix2, result );
                schema.Objects.TryGet( result.Name ).Should().BeSameAs( result );

                ix1.ReferencingObjects.Should()
                    .BeSequentiallyEqualTo(
                        SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( result ), ix1 ) );

                ix2.ReferencingObjects.Should()
                    .BeSequentiallyEqualTo(
                        SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( result ), ix2 ) );

                table.ReferencingObjects.Should().BeEmpty();
                schema.ReferencingObjects.Should().BeEmpty();
            }
        }

        [Fact]
        public void CreateForeignKey_ShouldCreateNewForeignKey_WhenIndexesDoNotBelongToTheSameTable()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var t1 = schema.Objects.CreateTable( "T1" );
            var ix2 = t1.Constraints.SetPrimaryKey( t1.Columns.Create( "C1" ).Asc() ).Index;
            var t2 = schema.Objects.CreateTable( "T2" );
            var sut = t2.Constraints;
            var ix1 = t2.Constraints.CreateIndex( t2.Columns.Create( "C2" ).Asc() );

            var result = sut.CreateForeignKey( ix1, ix2 );

            using ( new AssertionScope() )
            {
                result.Table.Should().BeSameAs( t2 );
                result.Database.Should().BeSameAs( t2.Database );
                result.Type.Should().Be( SqlObjectType.ForeignKey );
                result.Name.Should().Be( "FK_T2_C2_REF_T1" );
                result.OriginIndex.Should().BeSameAs( ix1 );
                result.ReferencedIndex.Should().BeSameAs( ix2 );
                result.OnUpdateBehavior.Should().Be( ReferenceBehavior.Restrict );
                result.OnDeleteBehavior.Should().Be( ReferenceBehavior.Restrict );
                result.ReferencingObjects.Should().BeEmpty();

                sut.Count.Should().Be( 2 );
                sut.Should().BeEquivalentTo( ix1, result );
                schema.Objects.TryGet( result.Name ).Should().BeSameAs( result );

                ix1.ReferencingObjects.Should()
                    .BeSequentiallyEqualTo(
                        SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( result ), ix1 ) );

                ix2.ReferencingObjects.Should()
                    .BeSequentiallyEqualTo(
                        SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( result ), ix2 ) );

                t1.ReferencingObjects.Should()
                    .BeSequentiallyEqualTo(
                        SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( result ), ix2 ) );

                schema.ReferencingObjects.Should().BeEmpty();
            }
        }

        [Fact]
        public void CreateForeignKey_ShouldCreateNewForeignKey_WhenIndexesDoNotBelongToTheSameSchema()
        {
            var db = MySqlDatabaseBuilderMock.Create();
            var schema1 = db.Schemas.Create( "foo" );
            var schema2 = db.Schemas.Create( "bar" );
            var t1 = schema1.Objects.CreateTable( "T1" );
            var ix2 = t1.Constraints.SetPrimaryKey( t1.Columns.Create( "C1" ).Asc() ).Index;
            var t2 = schema2.Objects.CreateTable( "T2" );
            var sut = t2.Constraints;
            var ix1 = t2.Constraints.CreateIndex( t2.Columns.Create( "C2" ).Asc() );

            var result = sut.CreateForeignKey( ix1, ix2 );

            using ( new AssertionScope() )
            {
                result.Table.Should().BeSameAs( t2 );
                result.Database.Should().BeSameAs( t2.Database );
                result.Type.Should().Be( SqlObjectType.ForeignKey );
                result.Name.Should().Be( "FK_T2_C2_REF_foo_T1" );
                result.OriginIndex.Should().BeSameAs( ix1 );
                result.ReferencedIndex.Should().BeSameAs( ix2 );
                result.OnUpdateBehavior.Should().Be( ReferenceBehavior.Restrict );
                result.OnDeleteBehavior.Should().Be( ReferenceBehavior.Restrict );
                result.ReferencingObjects.Should().BeEmpty();

                sut.Count.Should().Be( 2 );
                sut.Should().BeEquivalentTo( ix1, result );
                schema2.Objects.TryGet( result.Name ).Should().BeSameAs( result );

                ix1.ReferencingObjects.Should()
                    .BeSequentiallyEqualTo(
                        SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( result ), ix1 ) );

                ix2.ReferencingObjects.Should()
                    .BeSequentiallyEqualTo(
                        SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( result ), ix2 ) );

                t1.ReferencingObjects.Should()
                    .BeSequentiallyEqualTo(
                        SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( result ), ix2 ) );

                schema1.ReferencingObjects.Should()
                    .BeSequentiallyEqualTo(
                        SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( result ), ix2 ) );
            }
        }

        [Fact]
        public void CreateForeignKey_WithExplicitName_ShouldCreateNewForeignKey()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Constraints;
            var ix1 = sut.CreateIndex( table.Columns.Create( "C1" ).Asc() );
            var ix2 = sut.CreateUniqueIndex( "UIX_T", table.Columns.Create( "C2" ).Asc() );

            var result = sut.CreateForeignKey( "FK_T", ix1, ix2 );

            using ( new AssertionScope() )
            {
                result.Table.Should().BeSameAs( table );
                result.Database.Should().BeSameAs( table.Database );
                result.Type.Should().Be( SqlObjectType.ForeignKey );
                result.Name.Should().Be( "FK_T" );
                result.OriginIndex.Should().BeSameAs( ix1 );
                result.ReferencedIndex.Should().BeSameAs( ix2 );
                result.OnUpdateBehavior.Should().Be( ReferenceBehavior.Restrict );
                result.OnDeleteBehavior.Should().Be( ReferenceBehavior.Restrict );
                result.ReferencingObjects.Should().BeEmpty();

                sut.Count.Should().Be( 3 );
                sut.Should().BeEquivalentTo( ix1, ix2, result );
                schema.Objects.TryGet( result.Name ).Should().BeSameAs( result );

                ix1.ReferencingObjects.Should()
                    .BeSequentiallyEqualTo(
                        SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( result ), ix1 ) );

                ix2.ReferencingObjects.Should()
                    .BeSequentiallyEqualTo(
                        SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( result ), ix2 ) );

                table.ReferencingObjects.Should().BeEmpty();
                schema.ReferencingObjects.Should().BeEmpty();
            }
        }

        [Fact]
        public void CreateForeignKey_ShouldThrowSqlObjectBuilderException_WhenTableIsRemoved()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Constraints;
            var ix1 = sut.CreateIndex( table.Columns.Create( "C1" ).Asc() );
            var ix2 = sut.CreateUniqueIndex( table.Columns.Create( "C2" ).Asc() );
            table.Remove();

            var action = Lambda.Of( () => sut.CreateForeignKey( ix1, ix2 ) );

            action.Should()
                .ThrowExactly<SqlObjectBuilderException>()
                .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
        }

        [Fact]
        public void CreateForeignKey_ShouldThrowSqlObjectBuilderException_WhenSchemaObjectNameAlreadyExists()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Constraints;
            var ix1 = sut.CreateIndex( table.Columns.Create( "C1" ).Asc() );
            var ix2 = sut.CreateUniqueIndex( table.Columns.Create( "C2" ).Asc() );

            var action = Lambda.Of( () => sut.CreateForeignKey( "T", ix1, ix2 ) );

            action.Should()
                .ThrowExactly<SqlObjectBuilderException>()
                .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
        }

        [Fact]
        public void CreateForeignKey_ShouldThrowSqlObjectBuilderException_WhenOriginIndexAndReferencedIndexAreTheSame()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Constraints;
            var ix1 = sut.CreateUniqueIndex( table.Columns.Create( "C1" ).Asc() );

            var action = Lambda.Of( () => sut.CreateForeignKey( ix1, ix1 ) );

            action.Should()
                .ThrowExactly<SqlObjectBuilderException>()
                .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
        }

        [Fact]
        public void CreateForeignKey_ShouldThrowSqlObjectBuilderException_WhenReferencedIndexIsNotUnique()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Constraints;
            var ix1 = sut.CreateIndex( table.Columns.Create( "C1" ).Asc() );
            var ix2 = sut.CreateIndex( table.Columns.Create( "C2" ).Asc() );

            var action = Lambda.Of( () => sut.CreateForeignKey( ix1, ix2 ) );

            action.Should()
                .ThrowExactly<SqlObjectBuilderException>()
                .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
        }

        [Fact]
        public void CreateForeignKey_ShouldThrowSqlObjectBuilderException_WhenOriginIndexContainsExpressions()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Constraints;
            var ix1 = sut.CreateIndex( (table.Columns.Create( "C1" ).Node + SqlNode.Literal( 1 )).Asc() );
            var ix2 = sut.CreateUniqueIndex( table.Columns.Create( "C2" ).Asc() );

            var action = Lambda.Of( () => sut.CreateForeignKey( ix1, ix2 ) );

            action.Should()
                .ThrowExactly<SqlObjectBuilderException>()
                .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
        }

        [Fact]
        public void CreateForeignKey_ShouldThrowSqlObjectBuilderException_WhenReferencedIndexContainsExpressions()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Constraints;
            var ix1 = sut.CreateIndex( table.Columns.Create( "C1" ).Asc() );
            var ix2 = sut.CreateIndex( (table.Columns.Create( "C2" ).Node + SqlNode.Literal( 1 )).Asc() );

            var action = Lambda.Of( () => sut.CreateForeignKey( ix1, ix2 ) );

            action.Should()
                .ThrowExactly<SqlObjectBuilderException>()
                .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 2 );
        }

        [Fact]
        public void CreateForeignKey_ShouldThrowSqlObjectBuilderException_WhenReferencedIndexHasFilter()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Constraints;
            var ix1 = sut.CreateIndex( table.Columns.Create( "C1" ).Asc() );
            var ix2 = sut.CreateUniqueIndex( table.Columns.Create( "C2" ).Asc() ).SetFilter( SqlNode.True() );

            var action = Lambda.Of( () => sut.CreateForeignKey( ix1, ix2 ) );

            action.Should()
                .ThrowExactly<SqlObjectBuilderException>()
                .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
        }

        [Fact]
        public void CreateForeignKey_ShouldThrowSqlObjectBuilderException_WhenOriginIndexBelongsToAnotherTable()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var t1 = schema.Objects.CreateTable( "T1" );
            var sut = t1.Constraints;
            var ix1 = t1.Constraints.SetPrimaryKey( t1.Columns.Create( "C1" ).Asc() ).Index;
            var t2 = schema.Objects.CreateTable( "T2" );
            var ix2 = t2.Constraints.CreateIndex( t2.Columns.Create( "C2" ).Asc() );

            var action = Lambda.Of( () => sut.CreateForeignKey( ix2, ix1 ) );

            action.Should()
                .ThrowExactly<SqlObjectBuilderException>()
                .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
        }

        [Fact]
        public void CreateForeignKey_ShouldThrowSqlObjectBuilderException_WhenReferencedIndexBelongsToAnotherDatabase()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var otherSchema = MySqlDatabaseBuilderMock.Create().Schemas.Default;
            var t1 = schema.Objects.CreateTable( "T1" );
            var sut = t1.Constraints;
            var ix1 = t1.Constraints.SetPrimaryKey( t1.Columns.Create( "C1" ).Asc() ).Index;
            var t2 = otherSchema.Objects.CreateTable( "T2" );
            var ix2 = t2.Constraints.CreateUniqueIndex( t2.Columns.Create( "C2" ).Asc() );

            var action = Lambda.Of( () => sut.CreateForeignKey( ix1, ix2 ) );

            action.Should()
                .ThrowExactly<SqlObjectBuilderException>()
                .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
        }

        [Fact]
        public void CreateForeignKey_ShouldThrowSqlObjectBuilderException_WhenOriginIndexIsRemoved()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Constraints;
            var ix1 = sut.CreateIndex( table.Columns.Create( "C1" ).Asc() );
            var ix2 = sut.CreateUniqueIndex( table.Columns.Create( "C2" ).Asc() );
            ix1.Remove();

            var action = Lambda.Of( () => sut.CreateForeignKey( ix1, ix2 ) );

            action.Should()
                .ThrowExactly<SqlObjectBuilderException>()
                .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 2 );
        }

        [Fact]
        public void CreateForeignKey_ShouldThrowSqlObjectBuilderException_WhenReferencedIndexIsRemoved()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Constraints;
            var ix1 = sut.CreateIndex( table.Columns.Create( "C1" ).Asc() );
            var ix2 = sut.CreateUniqueIndex( table.Columns.Create( "C2" ).Asc() );
            ix2.Remove();

            var action = Lambda.Of( () => sut.CreateForeignKey( ix1, ix2 ) );

            action.Should()
                .ThrowExactly<SqlObjectBuilderException>()
                .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 2 );
        }

        [Fact]
        public void CreateForeignKey_ShouldThrowSqlObjectBuilderException_WhenReferencedIndexContainsNullableColumn()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Constraints;
            var ix1 = sut.CreateIndex( table.Columns.Create( "C1" ).Asc(), table.Columns.Create( "C2" ).Asc() );
            var ix2 = sut.CreateUniqueIndex( table.Columns.Create( "C3" ).Asc(), table.Columns.Create( "C4" ).MarkAsNullable().Asc() );

            var action = Lambda.Of( () => sut.CreateForeignKey( ix1, ix2 ) );

            action.Should()
                .ThrowExactly<SqlObjectBuilderException>()
                .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
        }

        [Fact]
        public void CreateForeignKey_ShouldThrowSqlObjectBuilderException_WhenReferencedIndexContainsGeneratedColumn()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Constraints;
            var ix1 = sut.CreateIndex( table.Columns.Create( "C1" ).Asc(), table.Columns.Create( "C2" ).Asc() );
            var ix2 = sut.CreateUniqueIndex(
                table.Columns.Create( "C3" ).Asc(),
                table.Columns.Create( "C4" ).SetComputation( SqlColumnComputation.Virtual( SqlNode.Literal( 1 ) ) ).Asc() );

            var action = Lambda.Of( () => sut.CreateForeignKey( ix1, ix2 ) );

            action.Should()
                .ThrowExactly<SqlObjectBuilderException>()
                .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
        }

        [Fact]
        public void CreateForeignKey_ShouldThrowSqlObjectBuilderException_WhenOriginIndexAndReferencedIndexHaveDifferentAmountOfColumns()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Constraints;
            var ix1 = sut.CreateIndex( table.Columns.Create( "C1" ).Asc(), table.Columns.Create( "C2" ).Asc() );
            var ix2 = sut.CreateUniqueIndex( table.Columns.Create( "C3" ).Asc() );

            var action = Lambda.Of( () => sut.CreateForeignKey( ix1, ix2 ) );

            action.Should()
                .ThrowExactly<SqlObjectBuilderException>()
                .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
        }

        [Fact]
        public void
            CreateForeignKey_ShouldThrowSqlObjectBuilderException_WhenAtLeastOneOriginIndexAndReferencedIndexColumnPairHasIncompatibleRuntimeTypes()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Constraints;
            var ix1 = sut.CreateIndex(
                table.Columns.Create( "C1" ).SetType<int>().Asc(),
                table.Columns.Create( "C2" ).SetType<string>().Asc() );

            var ix2 = sut.CreateUniqueIndex(
                table.Columns.Create( "C3" ).SetType<int>().Asc(),
                table.Columns.Create( "C4" ).SetType<double>().Asc() );

            var action = Lambda.Of( () => sut.CreateForeignKey( ix1, ix2 ) );

            action.Should()
                .ThrowExactly<SqlObjectBuilderException>()
                .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
        }

        [Fact]
        public void CreateForeignKey_ShouldThrowSqlObjectCastException_WhenOriginIndexIsOfInvalidType()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Constraints;
            var ix1 = Substitute.For<ISqlIndexBuilder>();
            var ix2 = sut.CreateUniqueIndex( table.Columns.Create( "C2" ).Asc() );

            var action = Lambda.Of( () => ((ISqlConstraintBuilderCollection)sut).CreateForeignKey( ix1, ix2 ) );

            action.Should()
                .ThrowExactly<SqlObjectCastException>()
                .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Expected == typeof( SqlIndexBuilder ) );
        }

        [Fact]
        public void CreateForeignKey_ShouldThrowSqlObjectCastException_WhenReferencedIndexIsOfInvalidType()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Constraints;
            var ix1 = sut.CreateIndex( table.Columns.Create( "C1" ).Asc() );
            var ix2 = Substitute.For<ISqlIndexBuilder>();

            var action = Lambda.Of( () => ((ISqlConstraintBuilderCollection)sut).CreateForeignKey( ix1, ix2 ) );

            action.Should()
                .ThrowExactly<SqlObjectCastException>()
                .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Expected == typeof( SqlIndexBuilder ) );
        }

        [Theory]
        [InlineData( "" )]
        [InlineData( " " )]
        [InlineData( "`" )]
        [InlineData( "'" )]
        [InlineData( "f`oo" )]
        public void CreateForeignKey_ShouldThrowSqlObjectBuilderException_WhenNameIsInvalid(string name)
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Constraints;
            var ix1 = sut.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
            var ix2 = sut.CreateIndex( table.Columns.Create( "C2" ).Asc() );

            var action = Lambda.Of( () => sut.CreateForeignKey( name, ix2, ix1 ) );

            action.Should()
                .ThrowExactly<SqlObjectBuilderException>()
                .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
        }

        [Fact]
        public void CreateCheck_ShouldCreateNewCheck()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Constraints;
            var c = table.Columns.Create( "C" );
            var condition = c.Node > SqlNode.Literal( 0 );

            var result = sut.CreateCheck( condition );

            using ( new AssertionScope() )
            {
                result.Table.Should().BeSameAs( table );
                result.Database.Should().BeSameAs( table.Database );
                result.Type.Should().Be( SqlObjectType.Check );
                result.Name.Should().MatchRegex( "CHK_T_[0-9a-fA-F]{32}" );
                result.Condition.Should().BeSameAs( condition );
                result.ReferencedColumns.Should().BeSequentiallyEqualTo( c );

                sut.Count.Should().Be( 1 );
                sut.Should().BeEquivalentTo( result );
                schema.Objects.TryGet( result.Name ).Should().BeSameAs( result );

                c.ReferencingObjects.Should()
                    .BeSequentiallyEqualTo(
                        SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( result ), c ) );
            }
        }

        [Fact]
        public void CreateCheck_WithExplicitName_ShouldCreateNewCheck()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Constraints;
            var c = table.Columns.Create( "C" );
            var condition = c.Node > SqlNode.Literal( 0 );

            var result = sut.CreateCheck( "CHK", condition );

            using ( new AssertionScope() )
            {
                result.Table.Should().BeSameAs( table );
                result.Database.Should().BeSameAs( table.Database );
                result.Type.Should().Be( SqlObjectType.Check );
                result.Name.Should().MatchRegex( "CHK" );
                result.Condition.Should().BeSameAs( condition );
                result.ReferencedColumns.Should().BeSequentiallyEqualTo( c );

                sut.Count.Should().Be( 1 );
                sut.Should().BeEquivalentTo( result );
                schema.Objects.TryGet( result.Name ).Should().BeSameAs( result );

                c.ReferencingObjects.Should()
                    .BeSequentiallyEqualTo(
                        SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( result ), c ) );
            }
        }

        [Fact]
        public void CreateCheck_ShouldThrowSqlObjectBuilderException_WhenTableIsRemoved()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Constraints;
            table.Remove();

            var action = Lambda.Of( () => sut.CreateCheck( SqlNode.True() ) );

            action.Should()
                .ThrowExactly<SqlObjectBuilderException>()
                .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
        }

        [Fact]
        public void CreateCheck_ShouldThrowSqlObjectBuilderException_WhenSchemaObjectNameAlreadyExists()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Constraints;

            var action = Lambda.Of( () => sut.CreateCheck( "T", SqlNode.True() ) );

            action.Should()
                .ThrowExactly<SqlObjectBuilderException>()
                .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
        }

        [Fact]
        public void CreateCheck_ShouldThrowSqlObjectBuilderException_WhenConditionIsInvalid()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Constraints;

            var action = Lambda.Of( () => sut.CreateCheck( SqlNode.Functions.RecordsAffected() == SqlNode.Literal( 0 ) ) );

            action.Should()
                .ThrowExactly<SqlObjectBuilderException>()
                .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
        }

        [Theory]
        [InlineData( "" )]
        [InlineData( " " )]
        [InlineData( "`" )]
        [InlineData( "'" )]
        [InlineData( "f`oo" )]
        public void CreateCheck_ShouldThrowSqlObjectBuilderException_WhenNameIsInvalid(string name)
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Constraints;

            var action = Lambda.Of( () => sut.CreateCheck( name, SqlNode.True() ) );

            action.Should()
                .ThrowExactly<SqlObjectBuilderException>()
                .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
        }

        [Theory]
        [InlineData( "T", false )]
        [InlineData( "PK_T", true )]
        [InlineData( "UIX_T_CA", true )]
        [InlineData( "CHK", true )]
        public void Contains_ShouldReturnTrue_WhenConstraintExists(string name, bool expected)
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Constraints;
            sut.SetPrimaryKey( table.Columns.Create( "C" ).Asc() );
            sut.CreateCheck( "CHK", SqlNode.True() );

            var result = sut.Contains( name );

            result.Should().Be( expected );
        }

        [Fact]
        public void Get_ShouldReturnExistingConstraint()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Constraints;
            var expected = sut.SetPrimaryKey( table.Columns.Create( "C" ).Asc() );

            var result = sut.Get( expected.Name );

            result.Should().BeSameAs( expected );
        }

        [Fact]
        public void Get_ShouldThrowKeyNotFoundException_WhenConstraintDoesNotExist()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Constraints;
            sut.SetPrimaryKey( table.Columns.Create( "C" ).Asc() );

            var action = Lambda.Of( () => sut.Get( "T" ) );

            action.Should().ThrowExactly<KeyNotFoundException>();
        }

        [Fact]
        public void TryGet_ShouldReturnExistingConstraint()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Constraints;
            var expected = sut.SetPrimaryKey( table.Columns.Create( "C" ).Asc() );

            var result = sut.TryGet( expected.Name );

            result.Should().BeSameAs( expected );
        }

        [Fact]
        public void TryGet_ShouldReturnNull_WhenConstraintDoesNotExist()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Constraints;
            sut.SetPrimaryKey( table.Columns.Create( "C" ).Asc() );

            var result = sut.TryGet( "T" );

            result.Should().BeNull();
        }

        [Fact]
        public void GetIndex_ShouldReturnExistingIndex()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Constraints;
            var index = sut.SetPrimaryKey( table.Columns.Create( "C" ).Asc() ).Index;

            var result = sut.GetIndex( index.Name );

            result.Should().BeSameAs( index );
        }

        [Fact]
        public void GetIndex_ShouldThrowKeyNotFoundException_WhenConstraintDoesNotExist()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Constraints;

            var action = Lambda.Of( () => sut.GetIndex( "T" ) );

            action.Should().ThrowExactly<KeyNotFoundException>();
        }

        [Fact]
        public void GetIndex_ShouldThrowSqlObjectCastException_WhenConstraintExistsButNotAsIndex()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Constraints;
            sut.CreateCheck( "CHK", SqlNode.True() );

            var action = Lambda.Of( () => sut.GetIndex( "CHK" ) );

            action.Should()
                .ThrowExactly<SqlObjectCastException>()
                .AndMatch(
                    e => e.Dialect == MySqlDialect.Instance &&
                        e.Expected == typeof( SqlIndexBuilder ) &&
                        e.Actual == typeof( MySqlCheckBuilder ) );
        }

        [Fact]
        public void TryGetIndex_ShouldReturnExistingIndex()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Constraints;
            var index = sut.SetPrimaryKey( table.Columns.Create( "C" ).Asc() ).Index;

            var result = sut.TryGetIndex( index.Name );

            result.Should().BeSameAs( index );
        }

        [Fact]
        public void TryGetIndex_ShouldReturnNull_WhenConstraintDoesNotExist()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Constraints;

            var result = sut.TryGetIndex( "T" );

            result.Should().BeNull();
        }

        [Fact]
        public void TryGetIndex_ShouldReturnNull_WhenConstraintExistsButNotAsIndex()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Constraints;
            sut.CreateCheck( "CHK", SqlNode.True() );

            var result = sut.TryGetIndex( "CHK" );

            result.Should().BeNull();
        }

        [Fact]
        public void GetForeignKey_ShouldReturnExistingForeignKey()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Constraints;
            var ix1 = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
            var ix2 = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );
            var foreignKey = sut.CreateForeignKey( ix2, ix1 );

            var result = sut.GetForeignKey( foreignKey.Name );

            result.Should().BeSameAs( foreignKey );
        }

        [Fact]
        public void GetForeignKey_ShouldThrowKeyNotFoundException_WhenConstraintDoesNotExist()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Constraints;

            var action = Lambda.Of( () => sut.GetForeignKey( "T" ) );

            action.Should().ThrowExactly<KeyNotFoundException>();
        }

        [Fact]
        public void GetForeignKey_ShouldThrowSqlObjectCastException_WhenConstraintExistsButNotAsForeignKey()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Constraints;
            var index = sut.SetPrimaryKey( table.Columns.Create( "C" ).Asc() ).Index;

            var action = Lambda.Of( () => sut.GetForeignKey( index.Name ) );

            action.Should()
                .ThrowExactly<SqlObjectCastException>()
                .AndMatch(
                    e => e.Dialect == MySqlDialect.Instance &&
                        e.Expected == typeof( SqlForeignKeyBuilder ) &&
                        e.Actual == typeof( MySqlIndexBuilder ) );
        }

        [Fact]
        public void TryGetForeignKey_ShouldReturnExistingForeignKey()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Constraints;
            var ix1 = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
            var ix2 = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );
            var foreignKey = sut.CreateForeignKey( ix2, ix1 );

            var result = sut.TryGetForeignKey( foreignKey.Name );

            result.Should().BeSameAs( foreignKey );
        }

        [Fact]
        public void TryGetForeignKey_ShouldReturnNull_WhenConstraintDoesNotExist()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Constraints;

            var result = sut.TryGetForeignKey( "T" );

            result.Should().BeNull();
        }

        [Fact]
        public void TryGetForeignKey_ShouldReturnNull_WhenConstraintExistsButNotAsForeignKey()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Constraints;
            var index = sut.SetPrimaryKey( table.Columns.Create( "C" ).Asc() ).Index;

            var result = sut.TryGetForeignKey( index.Name );

            result.Should().BeNull();
        }

        [Fact]
        public void GetCheck_ShouldReturnExistingCheck()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Constraints;
            var check = sut.CreateCheck( SqlNode.True() );

            var result = sut.GetCheck( check.Name );

            result.Should().BeSameAs( check );
        }

        [Fact]
        public void GetCheck_ShouldThrowKeyNotFoundException_WhenConstraintDoesNotExist()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Constraints;

            var action = Lambda.Of( () => sut.GetCheck( "T" ) );

            action.Should().ThrowExactly<KeyNotFoundException>();
        }

        [Fact]
        public void GetCheck_ShouldThrowSqlObjectCastException_WhenConstraintExistsButNotAsCheck()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Constraints;
            var index = sut.SetPrimaryKey( table.Columns.Create( "C" ).Asc() ).Index;

            var action = Lambda.Of( () => sut.GetCheck( index.Name ) );

            action.Should()
                .ThrowExactly<SqlObjectCastException>()
                .AndMatch(
                    e => e.Dialect == MySqlDialect.Instance &&
                        e.Expected == typeof( SqlCheckBuilder ) &&
                        e.Actual == typeof( MySqlIndexBuilder ) );
        }

        [Fact]
        public void TryGetCheck_ShouldReturnExistingCheck()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Constraints;
            var check = sut.CreateCheck( SqlNode.True() );

            var result = sut.TryGetCheck( check.Name );

            result.Should().BeSameAs( check );
        }

        [Fact]
        public void TryGetCheck_ShouldReturnNull_WhenConstraintDoesNotExist()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Constraints;

            var result = sut.TryGetCheck( "T" );

            result.Should().BeNull();
        }

        [Fact]
        public void TryGetCheck_ShouldReturnNull_WhenConstraintExistsButNotAsCheck()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Constraints;
            var index = sut.SetPrimaryKey( table.Columns.Create( "C" ).Asc() ).Index;

            var result = sut.TryGetCheck( index.Name );

            result.Should().BeNull();
        }

        [Fact]
        public void GetPrimaryKey_ShouldReturnPrimaryKey_WhenPrimaryKeyExists()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Constraints;
            var expected = sut.SetPrimaryKey( table.Columns.Create( "C" ).Asc() );

            var result = sut.GetPrimaryKey();

            result.Should().BeSameAs( expected );
        }

        [Fact]
        public void GetPrimaryKey_ShouldThrowMySqlObjectCastException_WhenPrimaryKeyDoesNotExist()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Constraints;

            var action = Lambda.Of( () => sut.GetPrimaryKey() );

            action.Should().ThrowExactly<SqlObjectBuilderException>();
        }

        [Fact]
        public void TryGetPrimaryKey_ShouldReturnPrimaryKey_WhenPrimaryKeyExists()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Constraints;
            var expected = sut.SetPrimaryKey( table.Columns.Create( "C" ).Asc() );

            var result = sut.TryGetPrimaryKey();

            result.Should().BeSameAs( expected );
        }

        [Fact]
        public void TryGetPrimaryKey_ShouldReturnNull_WhenPrimaryKeyDoesNotExist()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Constraints;

            var result = sut.TryGetPrimaryKey();

            result.Should().BeNull();
        }

        [Fact]
        public void Remove_ShouldRemoveExistingIndex()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Constraints;
            var c1 = table.Columns.Create( "C1" );
            var c2 = table.Columns.Create( "C2" );
            var ixc1 = c1.Asc();
            var ixc2 = c2.Desc();
            var index = sut.CreateIndex( ixc1, ixc2 );

            var result = sut.Remove( index.Name );

            using ( new AssertionScope() )
            {
                result.Should().BeTrue();
                sut.TryGet( index.Name ).Should().BeNull();
                index.IsRemoved.Should().BeTrue();
                schema.Objects.TryGet( index.Name ).Should().BeNull();
                c1.ReferencingObjects.Should().BeEmpty();
                c2.ReferencingObjects.Should().BeEmpty();
            }
        }

        [Fact]
        public void Remove_ShouldReturnFalse_WhenIndexHasOriginatingForeignKey()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Constraints;
            var pk = sut.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
            var index = sut.CreateIndex( table.Columns.Create( "C2" ).Asc() );
            table.Constraints.CreateForeignKey( index, pk.Index );

            var result = sut.Remove( index.Name );

            using ( new AssertionScope() )
            {
                result.Should().BeFalse();
                index.IsRemoved.Should().BeFalse();
                sut.Count.Should().Be( 4 );
            }
        }

        [Fact]
        public void Remove_ShouldReturnFalse_WhenIndexHasReferencingForeignKey()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Constraints;
            var pk = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
            var index = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );
            table.Constraints.CreateForeignKey( index, pk.Index );

            var result = sut.Remove( pk.Index.Name );

            using ( new AssertionScope() )
            {
                result.Should().BeFalse();
                pk.Index.IsRemoved.Should().BeFalse();
                sut.Count.Should().Be( 4 );
            }
        }

        [Fact]
        public void Remove_ShouldRemoveExistingPrimaryKey()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Constraints;
            var column = table.Columns.Create( "C" );
            var pk = table.Constraints.SetPrimaryKey( column.Asc() );

            var result = sut.Remove( pk.Name );

            using ( new AssertionScope() )
            {
                result.Should().BeTrue();
                sut.TryGet( pk.Name ).Should().BeNull();
                sut.TryGet( pk.Index.Name ).Should().BeNull();
                sut.TryGetPrimaryKey().Should().BeNull();
                pk.IsRemoved.Should().BeTrue();
                pk.Index.IsRemoved.Should().BeTrue();
                schema.Objects.TryGet( pk.Name ).Should().BeNull();
                schema.Objects.TryGet( pk.Index.Name ).Should().BeNull();
                column.ReferencingObjects.Should().BeEmpty();
            }
        }

        [Fact]
        public void Remove_ShouldReturnFalse_WhenPrimaryKeyUnderlyingIndexHasOriginatingForeignKey()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Constraints;
            var pk = sut.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
            var index = sut.CreateUniqueIndex( table.Columns.Create( "C2" ).Asc() );
            table.Constraints.CreateForeignKey( pk.Index, index );

            var result = sut.Remove( pk.Name );

            using ( new AssertionScope() )
            {
                result.Should().BeFalse();
                index.IsRemoved.Should().BeFalse();
                sut.Count.Should().Be( 4 );
                sut.TryGetPrimaryKey().Should().BeSameAs( pk );
            }
        }

        [Fact]
        public void Remove_ShouldReturnFalse_WhenPrimaryKeyUnderlyingIndexHasReferencingForeignKey()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Constraints;
            var pk = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
            var index = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );
            table.Constraints.CreateForeignKey( index, pk.Index );

            var result = sut.Remove( pk.Name );

            using ( new AssertionScope() )
            {
                result.Should().BeFalse();
                pk.Index.IsRemoved.Should().BeFalse();
                sut.Count.Should().Be( 4 );
                sut.TryGetPrimaryKey().Should().BeSameAs( pk );
            }
        }

        [Fact]
        public void Remove_ShouldRemoveExistingForeignKey()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Constraints;
            var ix1 = sut.CreateIndex( table.Columns.Create( "C1" ).Asc() );
            var ix2 = sut.CreateUniqueIndex( table.Columns.Create( "C2" ).Asc() );
            var fk = sut.CreateForeignKey( ix1, ix2 );

            var result = sut.Remove( fk.Name );

            using ( new AssertionScope() )
            {
                result.Should().BeTrue();
                sut.Count.Should().Be( 2 );
                sut.TryGet( fk.Name ).Should().BeNull();
                fk.IsRemoved.Should().BeTrue();
                schema.Objects.TryGet( fk.Name ).Should().BeNull();
                ix1.ReferencingObjects.Should().BeEmpty();
                ix2.ReferencingObjects.Should().BeEmpty();
            }
        }

        [Fact]
        public void Remove_ShouldRemoveExistingCheck()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Constraints;
            var c = table.Columns.Create( "C" );
            var check = sut.CreateCheck( c.Node > SqlNode.Literal( 0 ) );

            var result = sut.Remove( check.Name );

            using ( new AssertionScope() )
            {
                result.Should().BeTrue();
                sut.Count.Should().Be( 0 );
                sut.TryGet( check.Name ).Should().BeNull();
                check.IsRemoved.Should().BeTrue();
                schema.Objects.TryGet( check.Name ).Should().BeNull();
                c.ReferencingObjects.Should().BeEmpty();
            }
        }

        [Fact]
        public void Remove_ShouldReturnFalse_WhenConstraintDoesNotExist()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Constraints;

            var result = sut.Remove( "PK" );

            result.Should().BeFalse();
        }
    }
}
