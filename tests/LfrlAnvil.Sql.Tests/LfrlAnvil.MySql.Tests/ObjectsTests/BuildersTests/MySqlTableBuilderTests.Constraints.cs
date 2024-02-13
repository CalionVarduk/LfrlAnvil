using System.Collections.Generic;
using System.Linq;
using LfrlAnvil.Functional;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Extensions;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.MySql.Exceptions;
using LfrlAnvil.MySql.Extensions;
using LfrlAnvil.MySql.Objects.Builders;
using LfrlAnvil.MySql.Tests.Helpers;
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
            ISqlConstraintBuilderCollection sut = table.Constraints;
            var c1 = table.Columns.Create( "C1" ).Asc();
            var c2 = table.Columns.Create( "C2" ).Desc();

            var result = sut.CreateIndex( new[] { c1.UnsafeReinterpretAs<ISqlColumnBuilder>(), c2 }, isUnique );

            using ( new AssertionScope() )
            {
                result.Columns.ToArray().Should().BeSequentiallyEqualTo( c1, c2 );
                result.Name.Should().Be( expectedName );
                result.Table.Should().BeSameAs( table );
                result.Database.Should().BeSameAs( table.Database );
                result.Type.Should().Be( SqlObjectType.Index );
                result.ReferencedFilterColumns.Should().BeEmpty();
                result.PrimaryKey.Should().BeNull();
                result.IsUnique.Should().Be( isUnique );
                result.Filter.Should().BeNull();
                schema.Objects.Contains( result.Name ).Should().BeTrue();
                sut.Count.Should().Be( 1 );
                sut.Should().BeEquivalentTo( result );
                sut.Contains( result.Name ).Should().BeTrue();

                c1.Column.ReferencingIndexes.Should().BeSequentiallyEqualTo( (MySqlIndexBuilder)result );
                c2.Column.ReferencingIndexes.Should().BeSequentiallyEqualTo( (MySqlIndexBuilder)result );
            }
        }

        [Theory]
        [InlineData( false )]
        [InlineData( true )]
        public void CreateIndex_WithExplicitName_ShouldCreateNewIndex(bool isUnique)
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            ISqlConstraintBuilderCollection sut = table.Constraints;
            var c1 = table.Columns.Create( "C1" ).Asc();
            var c2 = table.Columns.Create( "C2" ).Desc();

            var result = sut.CreateIndex( "IX_T", new[] { c1.UnsafeReinterpretAs<ISqlColumnBuilder>(), c2 }, isUnique );

            using ( new AssertionScope() )
            {
                result.Columns.ToArray().Should().BeSequentiallyEqualTo( c1, c2 );
                result.Name.Should().Be( "IX_T" );
                result.Table.Should().BeSameAs( table );
                result.Database.Should().BeSameAs( table.Database );
                result.Type.Should().Be( SqlObjectType.Index );
                result.ReferencedFilterColumns.Should().BeEmpty();
                result.PrimaryKey.Should().BeNull();
                result.IsUnique.Should().Be( isUnique );
                result.Filter.Should().BeNull();
                schema.Objects.Contains( result.Name ).Should().BeTrue();
                sut.Count.Should().Be( 1 );
                sut.Should().BeEquivalentTo( result );
                sut.Contains( result.Name ).Should().BeTrue();

                c1.Column.ReferencingIndexes.Should().BeSequentiallyEqualTo( (MySqlIndexBuilder)result );
                c2.Column.ReferencingIndexes.Should().BeSequentiallyEqualTo( (MySqlIndexBuilder)result );
            }
        }

        [Fact]
        public void CreateIndex_ShouldThrowMySqlObjectBuilderException_WhenTableIsRemoved()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            ISqlConstraintBuilderCollection sut = table.Constraints;
            var column = table.Columns.Create( "C" ).Asc();
            table.Remove();

            var action = Lambda.Of( () => sut.CreateIndex( column ) );

            action.Should()
                .ThrowExactly<MySqlObjectBuilderException>()
                .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
        }

        [Fact]
        public void CreateIndex_ShouldThrowMySqlObjectBuilderException_WhenConstraintAlreadyExists()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            ISqlConstraintBuilderCollection sut = table.Constraints;
            var c1 = table.Columns.Create( "C1" ).Asc();
            var c2 = table.Columns.Create( "C2" ).Desc();
            var other = sut.CreateCheck( SqlNode.True() );

            var action = Lambda.Of( () => sut.CreateIndex( other.Name, c1, c2 ) );

            action.Should()
                .ThrowExactly<MySqlObjectBuilderException>()
                .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
        }

        [Fact]
        public void CreateIndex_ShouldThrowMySqlObjectBuilderException_WhenConstraintAlreadyExistsInAnotherTable()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var otherTable = schema.Objects.CreateTable( "T2" );
            var other = otherTable.Constraints.CreateIndex( otherTable.Columns.Create( "C1" ).Asc() );
            otherTable.Constraints.SetPrimaryKey( otherTable.Columns.Create( "C2" ).Asc() );
            var table = schema.Objects.CreateTable( "T" );
            ISqlConstraintBuilderCollection sut = table.Constraints;
            var c = table.Columns.Create( "C" ).Asc();

            var action = Lambda.Of( () => sut.CreateIndex( other.Name, c ) );

            action.Should()
                .ThrowExactly<MySqlObjectBuilderException>()
                .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
        }

        [Fact]
        public void CreateIndex_ShouldThrowMySqlObjectBuilderException_WhenColumnsAreEmpty()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            ISqlConstraintBuilderCollection sut = table.Constraints;

            var action = Lambda.Of( () => sut.CreateIndex() );

            action.Should()
                .ThrowExactly<MySqlObjectBuilderException>()
                .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
        }

        [Fact]
        public void CreateIndex_ShouldThrowMySqlObjectBuilderException_WhenAtLeastOneColumnBelongsToAnotherTable()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var t1 = schema.Objects.CreateTable( "T1" );
            var c1 = t1.Columns.Create( "C1" );
            t1.Constraints.SetPrimaryKey( c1.Asc() );

            var t2 = schema.Objects.CreateTable( "T2" );
            ISqlConstraintBuilderCollection sut = t2.Constraints;

            var action = Lambda.Of( () => sut.CreateIndex( c1.Asc() ) );

            action.Should()
                .ThrowExactly<MySqlObjectBuilderException>()
                .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
        }

        [Fact]
        public void CreateIndex_ShouldThrowMySqlObjectBuilderException_WhenAtLeastOneColumnIsRemoved()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var column = table.Columns.Create( "C" );
            ISqlConstraintBuilderCollection sut = table.Constraints;
            column.Remove();

            var action = Lambda.Of( () => sut.CreateIndex( column.Asc() ) );

            action.Should()
                .ThrowExactly<MySqlObjectBuilderException>()
                .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
        }

        [Fact]
        public void CreateIndex_ShouldThrowMySqlObjectBuilderException_WhenAtLeastOneColumnIsDuplicated()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var column = table.Columns.Create( "C" );
            ISqlConstraintBuilderCollection sut = table.Constraints;

            var action = Lambda.Of( () => sut.CreateIndex( column.Asc(), column.Desc() ) );

            action.Should()
                .ThrowExactly<MySqlObjectBuilderException>()
                .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
        }

        [Fact]
        public void CreateIndex_ShouldThrowSqlObjectCastException_WhenAtLeastOneColumnIsOfInvalidType()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var column = Substitute.For<ISqlColumnBuilder>();
            ISqlConstraintBuilderCollection sut = table.Constraints;

            var action = Lambda.Of( () => sut.CreateIndex( SqlIndexColumnBuilder.CreateAsc( column ) ) );

            action.Should()
                .ThrowExactly<SqlObjectCastException>()
                .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Expected == typeof( MySqlColumnBuilder ) );
        }

        [Theory]
        [InlineData( "" )]
        [InlineData( " " )]
        [InlineData( "`" )]
        [InlineData( "'" )]
        [InlineData( "f`oo" )]
        public void CreateIndex_ShouldThrowMySqlObjectBuilderException_WhenNameIsInvalid(string name)
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            ISqlConstraintBuilderCollection sut = table.Constraints;
            var c = table.Columns.Create( "C" ).Asc();

            var action = Lambda.Of( () => sut.CreateIndex( name, c ) );

            action.Should()
                .ThrowExactly<MySqlObjectBuilderException>()
                .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
        }

        [Fact]
        public void SetPrimaryKey_ShouldCreatePrimaryKey_WhenTableDoesNotHaveOne()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            ISqlConstraintBuilderCollection sut = table.Constraints;
            var column = table.Columns.Create( "C" );

            var result = sut.SetPrimaryKey( column.Asc() );

            using ( new AssertionScope() )
            {
                result.Should().BeSameAs( table.Constraints.TryGetPrimaryKey() );
                result.Name.Should().Be( "PK_T" );
                result.Type.Should().Be( SqlObjectType.PrimaryKey );
                result.Database.Should().BeSameAs( schema.Database );
                result.Index.Table.Should().BeSameAs( table );
                result.Index.IsUnique.Should().BeTrue();
                result.Index.Name.Should().Be( "UIX_T_CA" );
                result.Index.Columns.ToArray().Should().BeSequentiallyEqualTo( column.Asc() );
                result.Index.PrimaryKey.Should().BeSameAs( result );
                result.Index.Type.Should().Be( SqlObjectType.Index );
                result.Index.Database.Should().BeSameAs( schema.Database );
                sut.Count.Should().Be( 2 );
                sut.Should().BeEquivalentTo( result, result.Index );
                sut.Contains( result.Name ).Should().BeTrue();
                sut.Contains( result.Index.Name ).Should().BeTrue();
                schema.Objects.Contains( result.Name ).Should().BeTrue();
                schema.Objects.Contains( result.Index.Name ).Should().BeTrue();
            }
        }

        [Fact]
        public void SetPrimaryKey_ShouldDoNothing_WhenPrimaryKeyIndexAndPrimaryKeyNameDoNotChange()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            ISqlConstraintBuilderCollection sut = table.Constraints;
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
            ISqlConstraintBuilderCollection sut = table.Constraints;
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
            ISqlConstraintBuilderCollection sut = table.Constraints;
            var c1 = table.Columns.Create( "C1" );
            var c2 = table.Columns.Create( "C2" );
            var oldPk = sut.SetPrimaryKey( c1.Asc() );

            var result = sut.SetPrimaryKey( oldPk.Name, c2.Asc() );

            using ( new AssertionScope() )
            {
                result.Should().BeSameAs( table.Constraints.TryGetPrimaryKey() );
                result.Name.Should().Be( "PK_T" );
                result.Type.Should().Be( SqlObjectType.PrimaryKey );
                result.Database.Should().BeSameAs( schema.Database );
                result.Index.Table.Should().BeSameAs( table );
                result.Index.IsUnique.Should().BeTrue();
                result.Index.Name.Should().Be( "UIX_T_C2A" );
                result.Index.Columns.ToArray().Should().BeSequentiallyEqualTo( c2.Asc() );
                result.Index.PrimaryKey.Should().BeSameAs( result );
                result.Index.Type.Should().Be( SqlObjectType.Index );
                result.Index.Database.Should().BeSameAs( schema.Database );
                sut.Count.Should().Be( 2 );
                sut.Should().BeEquivalentTo( result, result.Index );
                sut.Contains( result.Name ).Should().BeTrue();
                sut.Contains( result.Index.Name ).Should().BeTrue();
                schema.Objects.Contains( result.Name ).Should().BeTrue();
                schema.Objects.Contains( result.Index.Name ).Should().BeTrue();
                oldPk.IsRemoved.Should().BeTrue();
                oldPk.Index.IsRemoved.Should().BeTrue();
                oldPk.Index.PrimaryKey.Should().BeNull();
            }
        }

        [Fact]
        public void SetPrimaryKey_ShouldCreatePrimaryKeyAndRemoveOldOne_WhenPrimaryKeyIndexAndPrimaryKeyNameChanges()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            ISqlConstraintBuilderCollection sut = table.Constraints;
            var c1 = table.Columns.Create( "C1" );
            var c2 = table.Columns.Create( "C2" );
            var oldPk = sut.SetPrimaryKey( c1.Asc() );

            var result = sut.SetPrimaryKey( "PK_NEW", c2.Asc() );

            using ( new AssertionScope() )
            {
                result.Should().BeSameAs( table.Constraints.TryGetPrimaryKey() );
                result.Name.Should().Be( "PK_NEW" );
                result.Type.Should().Be( SqlObjectType.PrimaryKey );
                result.Database.Should().BeSameAs( schema.Database );
                result.Index.Table.Should().BeSameAs( table );
                result.Index.IsUnique.Should().BeTrue();
                result.Index.Name.Should().Be( "UIX_T_C2A" );
                result.Index.Columns.ToArray().Should().BeSequentiallyEqualTo( c2.Asc() );
                result.Index.PrimaryKey.Should().BeSameAs( result );
                result.Index.Type.Should().Be( SqlObjectType.Index );
                result.Index.Database.Should().BeSameAs( schema.Database );
                sut.Count.Should().Be( 2 );
                sut.Should().BeEquivalentTo( result, result.Index );
                sut.Contains( result.Name ).Should().BeTrue();
                sut.Contains( result.Index.Name ).Should().BeTrue();
                schema.Objects.Contains( result.Name ).Should().BeTrue();
                schema.Objects.Contains( result.Index.Name ).Should().BeTrue();
                oldPk.IsRemoved.Should().BeTrue();
                oldPk.Index.IsRemoved.Should().BeTrue();
                oldPk.Index.PrimaryKey.Should().BeNull();
            }
        }

        [Fact]
        public void
            SetPrimaryKey_ShouldCreatePrimaryKeyAndRemoveOldOne_WhenPrimaryKeyIndexAndPrimaryKeyNameChangesToOldPrimaryKeyIndexName()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            ISqlConstraintBuilderCollection sut = table.Constraints;
            var c1 = table.Columns.Create( "C1" );
            var c2 = table.Columns.Create( "C2" );
            var oldPk = sut.SetPrimaryKey( c1.Asc() );

            var result = sut.SetPrimaryKey( oldPk.Index.Name, c2.Asc() );

            using ( new AssertionScope() )
            {
                result.Should().BeSameAs( table.Constraints.TryGetPrimaryKey() );
                result.Name.Should().Be( "UIX_T_C1A" );
                result.Type.Should().Be( SqlObjectType.PrimaryKey );
                result.Database.Should().BeSameAs( schema.Database );
                result.Index.Table.Should().BeSameAs( table );
                result.Index.IsUnique.Should().BeTrue();
                result.Index.Name.Should().Be( "UIX_T_C2A" );
                result.Index.Columns.ToArray().Should().BeSequentiallyEqualTo( c2.Asc() );
                result.Index.PrimaryKey.Should().BeSameAs( result );
                result.Index.Type.Should().Be( SqlObjectType.Index );
                result.Index.Database.Should().BeSameAs( schema.Database );
                sut.Count.Should().Be( 2 );
                sut.Should().BeEquivalentTo( result, result.Index );
                sut.Contains( result.Name ).Should().BeTrue();
                sut.Contains( result.Index.Name ).Should().BeTrue();
                schema.Objects.Contains( result.Name ).Should().BeTrue();
                schema.Objects.Contains( result.Index.Name ).Should().BeTrue();
                oldPk.IsRemoved.Should().BeTrue();
                oldPk.Index.IsRemoved.Should().BeTrue();
                oldPk.Index.PrimaryKey.Should().BeNull();
            }
        }

        [Fact]
        public void SetPrimaryKey_ShouldThrowMySqlObjectBuilderException_WhenCurrentPrimaryKeyIndexHasExternalReferences()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var c1 = table.Columns.Create( "C1" );
            var c2 = table.Columns.Create( "C2" );
            var oldPk = table.Constraints.SetPrimaryKey( c1.Asc() );

            var t2 = schema.Objects.CreateTable( "T2" );
            var pk2 = t2.Constraints.SetPrimaryKey( t2.Columns.Create( "C3" ).Asc() );
            t2.Constraints.CreateForeignKey( pk2.Index, oldPk.Index );

            var action = Lambda.Of( () => table.Constraints.SetPrimaryKey( c2.Asc() ) );

            action.Should()
                .ThrowExactly<MySqlObjectBuilderException>()
                .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
        }

        [Fact]
        public void SetPrimaryKey_ShouldThrowMySqlObjectBuilderException_WhenPrimaryKeyNameExistsDuringCreation()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Default;
            var table = schema.Objects.CreateTable( "foo" );
            var c1 = table.Columns.Create( "C1" );
            var index = table.Constraints.CreateUniqueIndex( c1.Asc() );

            var action = Lambda.Of( () => table.Constraints.SetPrimaryKey( index.Name, index ) );

            action.Should()
                .ThrowExactly<MySqlObjectBuilderException>()
                .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
        }

        [Fact]
        public void SetPrimaryKey_ShouldThrowMySqlObjectBuilderException_WhenPrimaryKeyNameExistsDuringUpdate()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Default;
            var table = schema.Objects.CreateTable( "foo" );
            var c1 = table.Columns.Create( "C1" );
            var c2 = table.Columns.Create( "C2" );
            var index = table.Constraints.CreateUniqueIndex( c1.Asc() );
            table.Constraints.SetPrimaryKey( c2.Asc() );

            var action = Lambda.Of( () => table.Constraints.SetPrimaryKey( index.Name, index ) );

            action.Should()
                .ThrowExactly<MySqlObjectBuilderException>()
                .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
        }

        [Fact]
        public void SetPrimaryKey_ShouldThrowMySqlObjectBuilderException_WhenIndexIsRemoved()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Default;
            var table = schema.Objects.CreateTable( Fixture.Create<string>() );
            var column = table.Columns.Create( "C" );
            var index = table.Constraints.CreateUniqueIndex( "UIX", column.Asc() );
            index.Remove();

            var action = Lambda.Of( () => table.Constraints.SetPrimaryKey( index ) );

            action.Should()
                .ThrowExactly<MySqlObjectBuilderException>()
                .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
        }

        [Fact]
        public void SetPrimaryKey_ShouldThrowMySqlObjectBuilderException_WhenIndexIsNotUnique()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Default;
            var table = schema.Objects.CreateTable( Fixture.Create<string>() );
            var column = table.Columns.Create( "C" );
            var index = table.Constraints.CreateIndex( column.Asc() );

            var action = Lambda.Of( () => table.Constraints.SetPrimaryKey( index ) );

            action.Should()
                .ThrowExactly<MySqlObjectBuilderException>()
                .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
        }

        [Fact]
        public void SetPrimaryKey_ShouldThrowMySqlObjectBuilderException_WhenIndexIsPartial()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Default;
            var table = schema.Objects.CreateTable( Fixture.Create<string>() );
            var column = table.Columns.Create( "C" );
            var index = table.Constraints.CreateUniqueIndex( column.Asc() ).SetFilter( SqlNode.True() );

            var action = Lambda.Of( () => table.Constraints.SetPrimaryKey( index ) );

            action.Should()
                .ThrowExactly<MySqlObjectBuilderException>()
                .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
        }

        [Fact]
        public void SetPrimaryKey_ShouldThrowMySqlObjectBuilderException_WhenIndexBelongsToAnotherTable()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Default;
            var otherTable = schema.Objects.CreateTable( "T" );
            var index = otherTable.Constraints.SetPrimaryKey( otherTable.Columns.Create( "C" ).Asc() ).Index;
            var table = schema.Objects.CreateTable( Fixture.Create<string>() );

            var action = Lambda.Of( () => table.Constraints.SetPrimaryKey( index ) );

            action.Should()
                .ThrowExactly<MySqlObjectBuilderException>()
                .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
        }

        [Fact]
        public void SetPrimaryKey_ShouldThrowMySqlObjectBuilderException_WhenAtLeastOneColumnIsNullable()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Default;
            var table = schema.Objects.CreateTable( Fixture.Create<string>() );
            var c1 = table.Columns.Create( "C1" );
            var c2 = table.Columns.Create( "C2" ).MarkAsNullable();
            var index = table.Constraints.CreateUniqueIndex( c1.Asc(), c2.Asc() );

            var action = Lambda.Of( () => table.Constraints.SetPrimaryKey( index ) );

            action.Should()
                .ThrowExactly<MySqlObjectBuilderException>()
                .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
        }

        [Fact]
        public void SetPrimaryKey_ShouldThrowMySqlObjectBuilderException_WhenTableHasBeenRemoved()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Default;
            var table = schema.Objects.CreateTable( Fixture.Create<string>() );
            var column = table.Columns.Create( "C" );
            var index = table.Constraints.CreateUniqueIndex( column.Asc() );
            schema.Objects.Remove( table.Name );

            var action = Lambda.Of( () => table.Constraints.SetPrimaryKey( index ) );

            action.Should()
                .ThrowExactly<MySqlObjectBuilderException>()
                .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
        }

        [Theory]
        [InlineData( "" )]
        [InlineData( " " )]
        [InlineData( "`" )]
        [InlineData( "'" )]
        [InlineData( "f`oo" )]
        public void SetPrimaryKey_ShouldThrowMySqlObjectBuilderException_WhenNameIsInvalid(string name)
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var index = table.Constraints.CreateUniqueIndex( table.Columns.Create( "C" ).Asc() );

            var action = Lambda.Of( () => table.Constraints.SetPrimaryKey( name, index ) );

            action.Should()
                .ThrowExactly<MySqlObjectBuilderException>()
                .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
        }

        [Fact]
        public void CreateForeignKey_ShouldCreateNewForeignKey_WhenIndexesBelongToTheSameTable()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            ISqlConstraintBuilderCollection sut = table.Constraints;
            var ix1 = sut.CreateIndex( "IX_T_C1", table.Columns.Create( "C1" ).Asc() );
            var ix2 = sut.CreateUniqueIndex( "UIX_T_C2", table.Columns.Create( "C2" ).Asc() );

            var result = sut.CreateForeignKey( ix1, ix2 );

            using ( new AssertionScope() )
            {
                result.OriginIndex.Should().BeSameAs( ix1 );
                result.ReferencedIndex.Should().BeSameAs( ix2 );
                result.Name.Should().Be( "FK_T_C1_REF_T" );
                result.Database.Should().BeSameAs( table.Database );
                result.Type.Should().Be( SqlObjectType.ForeignKey );
                result.OnUpdateBehavior.Should().Be( ReferenceBehavior.Restrict );
                result.OnDeleteBehavior.Should().Be( ReferenceBehavior.Restrict );
                schema.Objects.Contains( result.Name ).Should().BeTrue();
                sut.Count.Should().Be( 3 );
                sut.Should().BeEquivalentTo( ix1, ix2, result );
                sut.Contains( result.Name ).Should().BeTrue();

                // ix1.OriginatingForeignKeys.Should().BeSequentiallyEqualTo( result );
                // ix1.ReferencingForeignKeys.Should().BeEmpty();
                //
                // ix2.OriginatingForeignKeys.Should().BeEmpty();
                // ix2.ReferencingForeignKeys.Should().BeSequentiallyEqualTo( result );
            }
        }

        [Fact]
        public void CreateForeignKey_ShouldCreateNewForeignKey_WhenIndexesDoNotBelongToTheSameTable()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var t1 = schema.Objects.CreateTable( "T1" );
            var ix2 = t1.Constraints.SetPrimaryKey( t1.Columns.Create( "C1" ).Asc() ).Index;
            var t2 = schema.Objects.CreateTable( "T2" );
            ISqlConstraintBuilderCollection sut = t2.Constraints;
            var ix1 = t2.Constraints.CreateIndex( t2.Columns.Create( "C2" ).Asc() );

            var result = sut.CreateForeignKey( ix1, ix2 );

            using ( new AssertionScope() )
            {
                result.OriginIndex.Should().BeSameAs( ix1 );
                result.ReferencedIndex.Should().BeSameAs( ix2 );
                result.Name.Should().Be( "FK_T2_C2_REF_T1" );
                result.Database.Should().BeSameAs( t1.Database );
                result.Type.Should().Be( SqlObjectType.ForeignKey );
                result.OnUpdateBehavior.Should().Be( ReferenceBehavior.Restrict );
                result.OnDeleteBehavior.Should().Be( ReferenceBehavior.Restrict );
                schema.Objects.Contains( result.Name ).Should().BeTrue();
                sut.Count.Should().Be( 2 );
                sut.Should().BeEquivalentTo( ix1, result );
                sut.Contains( result.Name ).Should().BeTrue();

                ix1.OriginatingForeignKeys.Should().BeSequentiallyEqualTo( (MySqlForeignKeyBuilder)result );
                ix1.ReferencingForeignKeys.Should().BeEmpty();

                ix2.OriginatingForeignKeys.Should().BeEmpty();
                ix2.ReferencingForeignKeys.Should().BeSequentiallyEqualTo( (MySqlForeignKeyBuilder)result );
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
            ISqlConstraintBuilderCollection sut = t2.Constraints;
            var ix1 = t2.Constraints.CreateIndex( t2.Columns.Create( "C2" ).Asc() );

            var result = sut.CreateForeignKey( ix1, ix2 );

            using ( new AssertionScope() )
            {
                result.OriginIndex.Should().BeSameAs( ix1 );
                result.ReferencedIndex.Should().BeSameAs( ix2 );
                result.Name.Should().Be( "FK_T2_C2_REF_foo_T1" );
                result.Database.Should().BeSameAs( t1.Database );
                result.Type.Should().Be( SqlObjectType.ForeignKey );
                result.OnUpdateBehavior.Should().Be( ReferenceBehavior.Restrict );
                result.OnDeleteBehavior.Should().Be( ReferenceBehavior.Restrict );
                schema2.Objects.Contains( result.Name ).Should().BeTrue();
                sut.Count.Should().Be( 2 );
                sut.Should().BeEquivalentTo( ix1, result );
                sut.Contains( result.Name ).Should().BeTrue();

                ix1.OriginatingForeignKeys.Should().BeSequentiallyEqualTo( (MySqlForeignKeyBuilder)result );
                ix1.ReferencingForeignKeys.Should().BeEmpty();

                ix2.OriginatingForeignKeys.Should().BeEmpty();
                ix2.ReferencingForeignKeys.Should().BeSequentiallyEqualTo( (MySqlForeignKeyBuilder)result );
            }
        }

        [Fact]
        public void CreateForeignKey_WithExplicitName_ShouldCreateNewForeignKey()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            ISqlConstraintBuilderCollection sut = table.Constraints;
            var ix1 = sut.CreateIndex( table.Columns.Create( "C1" ).Asc() );
            var ix2 = sut.CreateUniqueIndex( "UIX_T", table.Columns.Create( "C2" ).Asc() );

            var result = sut.CreateForeignKey( "FK_T", ix1, ix2 );

            using ( new AssertionScope() )
            {
                result.OriginIndex.Should().BeSameAs( ix1 );
                result.ReferencedIndex.Should().BeSameAs( ix2 );
                result.Name.Should().Be( "FK_T" );
                result.Database.Should().BeSameAs( table.Database );
                result.Type.Should().Be( SqlObjectType.ForeignKey );
                result.OnUpdateBehavior.Should().Be( ReferenceBehavior.Restrict );
                result.OnDeleteBehavior.Should().Be( ReferenceBehavior.Restrict );
                schema.Objects.Contains( result.Name ).Should().BeTrue();
                sut.Count.Should().Be( 3 );
                sut.Should().BeEquivalentTo( ix1, ix2, result );
                sut.Contains( result.Name ).Should().BeTrue();

                // ix1.OriginatingForeignKeys.Should().BeSequentiallyEqualTo( result );
                // ix1.ReferencingForeignKeys.Should().BeEmpty();
                //
                // ix2.OriginatingForeignKeys.Should().BeEmpty();
                // ix2.ReferencingForeignKeys.Should().BeSequentiallyEqualTo( result );
            }
        }

        [Fact]
        public void CreateForeignKey_ShouldThrowMySqlObjectBuilderException_WhenTableIsRemoved()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            ISqlConstraintBuilderCollection sut = table.Constraints;
            var ix1 = sut.CreateIndex( table.Columns.Create( "C1" ).Asc() );
            var ix2 = sut.CreateUniqueIndex( table.Columns.Create( "C2" ).Asc() );
            table.Remove();

            var action = Lambda.Of( () => sut.CreateForeignKey( ix1, ix2 ) );

            action.Should()
                .ThrowExactly<MySqlObjectBuilderException>()
                .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
        }

        [Fact]
        public void CreateForeignKey_ShouldThrowMySqlObjectBuilderException_WhenConstraintAlreadyExists()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            ISqlConstraintBuilderCollection sut = table.Constraints;
            var ix1 = sut.CreateIndex( table.Columns.Create( "C1" ).Asc() );
            var ix2 = sut.CreateUniqueIndex( table.Columns.Create( "C2" ).Asc() );
            var other = sut.CreateCheck( SqlNode.True() );

            var action = Lambda.Of( () => sut.CreateForeignKey( other.Name, ix1, ix2 ) );

            action.Should()
                .ThrowExactly<MySqlObjectBuilderException>()
                .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
        }

        [Fact]
        public void CreateForeignKey_ShouldThrowMySqlObjectBuilderException_WhenConstraintAlreadyExistsInAnotherTable()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            ISqlConstraintBuilderCollection sut = table.Constraints;
            var ix1 = sut.SetPrimaryKey( table.Columns.Create( "C" ).Asc() ).Index;
            var otherTable = schema.Objects.CreateTable( "T1" );
            var ix2 = otherTable.Constraints.SetPrimaryKey( otherTable.Columns.Create( "C" ).Asc() ).Index;

            var action = Lambda.Of( () => sut.CreateForeignKey( ix2.Name, ix1, ix2 ) );

            action.Should()
                .ThrowExactly<MySqlObjectBuilderException>()
                .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
        }

        [Fact]
        public void CreateForeignKey_ShouldThrowMySqlObjectBuilderException_WhenOriginIndexAndReferencedIndexAreTheSame()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            ISqlConstraintBuilderCollection sut = table.Constraints;
            var ix1 = sut.CreateUniqueIndex( table.Columns.Create( "C1" ).Asc() );

            var action = Lambda.Of( () => sut.CreateForeignKey( ix1, ix1 ) );

            action.Should()
                .ThrowExactly<MySqlObjectBuilderException>()
                .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
        }

        [Fact]
        public void CreateForeignKey_ShouldThrowMySqlObjectBuilderException_WhenReferencedIndexIsNotUnique()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            ISqlConstraintBuilderCollection sut = table.Constraints;
            var ix1 = sut.CreateIndex( table.Columns.Create( "C1" ).Asc() );
            var ix2 = sut.CreateIndex( table.Columns.Create( "C2" ).Asc() );

            var action = Lambda.Of( () => sut.CreateForeignKey( ix1, ix2 ) );

            action.Should()
                .ThrowExactly<MySqlObjectBuilderException>()
                .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
        }

        [Fact]
        public void CreateForeignKey_ShouldThrowMySqlObjectBuilderException_WhenReferencedIndexHasFilter()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            ISqlConstraintBuilderCollection sut = table.Constraints;
            var ix1 = sut.CreateIndex( table.Columns.Create( "C1" ).Asc() );
            var ix2 = sut.CreateUniqueIndex( table.Columns.Create( "C2" ).Asc() ).SetFilter( SqlNode.True() );

            var action = Lambda.Of( () => sut.CreateForeignKey( ix1, ix2 ) );

            action.Should()
                .ThrowExactly<MySqlObjectBuilderException>()
                .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
        }

        [Fact]
        public void CreateForeignKey_ShouldThrowMySqlObjectBuilderException_WhenOriginIndexBelongsToAnotherTable()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var t1 = schema.Objects.CreateTable( "T1" );
            ISqlConstraintBuilderCollection sut = t1.Constraints;
            var ix1 = t1.Constraints.SetPrimaryKey( t1.Columns.Create( "C1" ).Asc() ).Index;
            var t2 = schema.Objects.CreateTable( "T2" );
            var ix2 = t2.Constraints.CreateIndex( t2.Columns.Create( "C2" ).Asc() );

            var action = Lambda.Of( () => sut.CreateForeignKey( ix2, ix1 ) );

            action.Should()
                .ThrowExactly<MySqlObjectBuilderException>()
                .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
        }

        [Fact]
        public void CreateForeignKey_ShouldThrowMySqlObjectBuilderException_WhenReferencedIndexBelongsToAnotherDatabase()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var otherSchema = MySqlDatabaseBuilderMock.Create().Schemas.Default;
            var t1 = schema.Objects.CreateTable( "T1" );
            ISqlConstraintBuilderCollection sut = t1.Constraints;
            var ix1 = t1.Constraints.SetPrimaryKey( t1.Columns.Create( "C1" ).Asc() ).Index;
            var t2 = otherSchema.Objects.CreateTable( "T2" );
            var ix2 = t2.Constraints.CreateUniqueIndex( t2.Columns.Create( "C2" ).Asc() );

            var action = Lambda.Of( () => sut.CreateForeignKey( ix1, ix2 ) );

            action.Should()
                .ThrowExactly<MySqlObjectBuilderException>()
                .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
        }

        [Fact]
        public void CreateForeignKey_ShouldThrowMySqlObjectBuilderException_WhenOriginIndexIsRemoved()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            ISqlConstraintBuilderCollection sut = table.Constraints;
            var ix1 = sut.CreateIndex( table.Columns.Create( "C1" ).Asc() );
            var ix2 = sut.CreateUniqueIndex( table.Columns.Create( "C2" ).Asc() );
            ix1.Remove();

            var action = Lambda.Of( () => sut.CreateForeignKey( ix1, ix2 ) );

            action.Should()
                .ThrowExactly<MySqlObjectBuilderException>()
                .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 2 );
        }

        [Fact]
        public void CreateForeignKey_ShouldThrowMySqlObjectBuilderException_WhenReferencedIndexIsRemoved()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            ISqlConstraintBuilderCollection sut = table.Constraints;
            var ix1 = sut.CreateIndex( table.Columns.Create( "C1" ).Asc() );
            var ix2 = sut.CreateUniqueIndex( table.Columns.Create( "C2" ).Asc() );
            ix2.Remove();

            var action = Lambda.Of( () => sut.CreateForeignKey( ix1, ix2 ) );

            action.Should()
                .ThrowExactly<MySqlObjectBuilderException>()
                .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 2 );
        }

        [Fact]
        public void CreateForeignKey_ShouldThrowMySqlObjectBuilderException_WhenReferencedIndexContainsNullableColumn()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            ISqlConstraintBuilderCollection sut = table.Constraints;
            var ix1 = sut.CreateIndex( table.Columns.Create( "C1" ).Asc(), table.Columns.Create( "C2" ).Asc() );
            var ix2 = sut.CreateUniqueIndex( table.Columns.Create( "C3" ).Asc(), table.Columns.Create( "C4" ).MarkAsNullable().Asc() );

            var action = Lambda.Of( () => sut.CreateForeignKey( ix1, ix2 ) );

            action.Should()
                .ThrowExactly<MySqlObjectBuilderException>()
                .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
        }

        [Fact]
        public void CreateForeignKey_ShouldThrowMySqlObjectBuilderException_WhenOriginIndexAndReferencedIndexHaveDifferentAmountOfColumns()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            ISqlConstraintBuilderCollection sut = table.Constraints;
            var ix1 = sut.CreateIndex( table.Columns.Create( "C1" ).Asc(), table.Columns.Create( "C2" ).Asc() );
            var ix2 = sut.CreateUniqueIndex( table.Columns.Create( "C3" ).Asc() );

            var action = Lambda.Of( () => sut.CreateForeignKey( ix1, ix2 ) );

            action.Should()
                .ThrowExactly<MySqlObjectBuilderException>()
                .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
        }

        [Fact]
        public void
            CreateForeignKey_ShouldThrowMySqlObjectBuilderException_WhenAtLeastOneOriginIndexAndReferencedIndexColumnPairHasIncompatibleTypes()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            ISqlConstraintBuilderCollection sut = table.Constraints;
            var ix1 = sut.CreateIndex(
                table.Columns.Create( "C1" ).SetType<int>().Asc(),
                table.Columns.Create( "C2" ).SetType<string>().Asc() );

            var ix2 = sut.CreateUniqueIndex(
                table.Columns.Create( "C3" ).SetType<int>().Asc(),
                table.Columns.Create( "C4" ).SetType<double>().Asc() );

            var action = Lambda.Of( () => sut.CreateForeignKey( ix1, ix2 ) );

            action.Should()
                .ThrowExactly<MySqlObjectBuilderException>()
                .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
        }

        [Fact]
        public void CreateForeignKey_ShouldThrowSqlObjectCastException_WhenOriginIndexIsOfInvalidType()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            ISqlConstraintBuilderCollection sut = table.Constraints;
            var ix1 = Substitute.For<ISqlIndexBuilder>();
            var ix2 = sut.CreateUniqueIndex( table.Columns.Create( "C2" ).Asc() );

            var action = Lambda.Of( () => sut.CreateForeignKey( ix1, ix2 ) );

            action.Should()
                .ThrowExactly<SqlObjectCastException>()
                .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Expected == typeof( MySqlIndexBuilder ) );
        }

        [Fact]
        public void CreateForeignKey_ShouldThrowSqlObjectCastException_WhenReferencedIndexIsOfInvalidType()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            ISqlConstraintBuilderCollection sut = table.Constraints;
            var ix1 = sut.CreateIndex( table.Columns.Create( "C1" ).Asc() );
            var ix2 = Substitute.For<ISqlIndexBuilder>();

            var action = Lambda.Of( () => sut.CreateForeignKey( ix1, ix2 ) );

            action.Should()
                .ThrowExactly<SqlObjectCastException>()
                .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Expected == typeof( MySqlIndexBuilder ) );
        }

        [Theory]
        [InlineData( "" )]
        [InlineData( " " )]
        [InlineData( "`" )]
        [InlineData( "'" )]
        [InlineData( "f`oo" )]
        public void CreateForeignKey_ShouldThrowMySqlObjectBuilderException_WhenNameIsInvalid(string name)
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            ISqlConstraintBuilderCollection sut = table.Constraints;
            var ix1 = sut.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
            var ix2 = sut.CreateIndex( table.Columns.Create( "C2" ).Asc() );

            var action = Lambda.Of( () => sut.CreateForeignKey( name, ix2, ix1 ) );

            action.Should()
                .ThrowExactly<MySqlObjectBuilderException>()
                .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
        }

        [Fact]
        public void CreateCheck_ShouldCreateNewCheck()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            ISqlConstraintBuilderCollection sut = table.Constraints;
            var c = table.Columns.Create( "C" );
            var condition = c.Node > SqlNode.Literal( 0 );

            var result = sut.CreateCheck( condition );

            using ( new AssertionScope() )
            {
                result.Name.Should().MatchRegex( "CHK_T_[0-9a-fA-F]{32}" );
                result.Table.Should().BeSameAs( table );
                result.Database.Should().BeSameAs( table.Database );
                result.Type.Should().Be( SqlObjectType.Check );
                result.ReferencedColumns.Should().BeSequentiallyEqualTo( c );
                result.Condition.Should().BeSameAs( condition );
                schema.Objects.Contains( result.Name ).Should().BeTrue();
                sut.Count.Should().Be( 1 );
                sut.Should().BeEquivalentTo( result );
                sut.Contains( result.Name ).Should().BeTrue();

                c.ReferencingChecks.Should().BeSequentiallyEqualTo( (MySqlCheckBuilder)result );
            }
        }

        [Fact]
        public void CreateCheck_WithExplicitName_ShouldCreateNewCheck()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            ISqlConstraintBuilderCollection sut = table.Constraints;
            var c = table.Columns.Create( "C" );
            var condition = c.Node > SqlNode.Literal( 0 );

            var result = sut.CreateCheck( "CHK", condition );

            using ( new AssertionScope() )
            {
                result.Name.Should().Be( "CHK" );
                result.Table.Should().BeSameAs( table );
                result.Database.Should().BeSameAs( table.Database );
                result.Type.Should().Be( SqlObjectType.Check );
                result.ReferencedColumns.Should().BeSequentiallyEqualTo( c );
                result.Condition.Should().BeSameAs( condition );
                schema.Objects.Contains( result.Name ).Should().BeTrue();
                sut.Count.Should().Be( 1 );
                sut.Should().BeEquivalentTo( result );
                sut.Contains( result.Name ).Should().BeTrue();

                c.ReferencingChecks.Should().BeSequentiallyEqualTo( (MySqlCheckBuilder)result );
            }
        }

        [Fact]
        public void CreateCheck_ShouldThrowMySqlObjectBuilderException_WhenTableIsRemoved()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            ISqlConstraintBuilderCollection sut = table.Constraints;
            table.Remove();

            var action = Lambda.Of( () => sut.CreateCheck( SqlNode.True() ) );

            action.Should()
                .ThrowExactly<MySqlObjectBuilderException>()
                .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
        }

        [Fact]
        public void CreateCheck_ShouldThrowMySqlObjectBuilderException_WhenConstraintAlreadyExists()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            ISqlConstraintBuilderCollection sut = table.Constraints;
            var other = sut.CreateIndex( table.Columns.Create( "C" ).Asc() );

            var action = Lambda.Of( () => sut.CreateCheck( other.Name, SqlNode.True() ) );

            action.Should()
                .ThrowExactly<MySqlObjectBuilderException>()
                .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
        }

        [Fact]
        public void CreateCheck_ShouldThrowMySqlObjectBuilderException_WhenConstraintAlreadyExistsInAnotherTable()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var otherTable = schema.Objects.CreateTable( "T2" );
            var other = otherTable.Constraints.SetPrimaryKey( otherTable.Columns.Create( "D" ).Asc() );
            var table = schema.Objects.CreateTable( "T" );
            ISqlConstraintBuilderCollection sut = table.Constraints;

            var action = Lambda.Of( () => sut.CreateCheck( other.Name, SqlNode.True() ) );

            action.Should()
                .ThrowExactly<MySqlObjectBuilderException>()
                .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
        }

        [Fact]
        public void CreateCheck_ShouldThrowMySqlObjectBuilderException_WhenConditionIsInvalid()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            ISqlConstraintBuilderCollection sut = table.Constraints;

            var action = Lambda.Of( () => sut.CreateCheck( SqlNode.Functions.RecordsAffected() == SqlNode.Literal( 0 ) ) );

            action.Should()
                .ThrowExactly<MySqlObjectBuilderException>()
                .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
        }

        [Theory]
        [InlineData( "" )]
        [InlineData( " " )]
        [InlineData( "`" )]
        [InlineData( "'" )]
        [InlineData( "f`oo" )]
        public void CreateCheck_ShouldThrowMySqlObjectBuilderException_WhenNameIsInvalid(string name)
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            ISqlConstraintBuilderCollection sut = table.Constraints;

            var action = Lambda.Of( () => sut.CreateCheck( name, SqlNode.True() ) );

            action.Should()
                .ThrowExactly<MySqlObjectBuilderException>()
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
            ISqlConstraintBuilderCollection sut = table.Constraints;
            sut.SetPrimaryKey( table.Columns.Create( "C" ).Asc() );
            sut.CreateCheck( "CHK", SqlNode.True() );

            var result = sut.Contains( name );

            result.Should().Be( expected );
        }

        [Fact]
        public void GetConstraint_ShouldReturnExistingConstraint()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            ISqlConstraintBuilderCollection sut = table.Constraints;
            var expected = sut.SetPrimaryKey( table.Columns.Create( "C" ).Asc() );

            var result = sut.Get( expected.Name );

            result.Should().BeSameAs( expected );
        }

        [Fact]
        public void GetConstraint_ShouldThrowKeyNotFoundException_WhenConstraintDoesNotExist()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            ISqlConstraintBuilderCollection sut = table.Constraints;
            sut.SetPrimaryKey( table.Columns.Create( "C" ).Asc() );

            var action = Lambda.Of( () => sut.Get( "T" ) );

            action.Should().ThrowExactly<KeyNotFoundException>();
        }

        [Fact]
        public void TryGetConstraint_ShouldReturnExistingConstraint()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            ISqlConstraintBuilderCollection sut = table.Constraints;
            var expected = sut.SetPrimaryKey( table.Columns.Create( "C" ).Asc() );

            var result = sut.TryGet( expected.Name );

            result.Should().BeSameAs( expected );
        }

        [Fact]
        public void TryGetConstraint_ShouldReturnNull_WhenConstraintDoesNotExist()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            ISqlConstraintBuilderCollection sut = table.Constraints;
            sut.SetPrimaryKey( table.Columns.Create( "C" ).Asc() );

            var result = sut.TryGet( "T" );

            result.Should().BeNull();
        }

        [Fact]
        public void GetIndex_ShouldReturnExistingIndex()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            ISqlConstraintBuilderCollection sut = table.Constraints;
            var index = sut.SetPrimaryKey( table.Columns.Create( "C" ).Asc() ).Index;

            var result = sut.GetIndex( index.Name );

            result.Should().BeSameAs( index );
        }

        [Fact]
        public void GetIndex_ShouldThrowKeyNotFoundException_WhenConstraintDoesNotExist()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            ISqlConstraintBuilderCollection sut = table.Constraints;

            var action = Lambda.Of( () => sut.GetIndex( "T" ) );

            action.Should().ThrowExactly<KeyNotFoundException>();
        }

        [Fact]
        public void GetIndex_ShouldThrowSqlObjectCastException_WhenConstraintExistsButNotAsIndex()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            ISqlConstraintBuilderCollection sut = table.Constraints;
            sut.CreateCheck( "CHK", SqlNode.True() );

            var action = Lambda.Of( () => sut.GetIndex( "CHK" ) );

            action.Should()
                .ThrowExactly<SqlObjectCastException>()
                .AndMatch(
                    e => e.Dialect == MySqlDialect.Instance &&
                        e.Expected == typeof( MySqlIndexBuilder ) &&
                        e.Actual == typeof( MySqlCheckBuilder ) );
        }

        [Fact]
        public void TryGetIndex_ShouldReturnExistingIndex()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            ISqlConstraintBuilderCollection sut = table.Constraints;
            var index = sut.SetPrimaryKey( table.Columns.Create( "C" ).Asc() ).Index;

            var result = sut.TryGetIndex( index.Name );

            result.Should().BeSameAs( index );
        }

        [Fact]
        public void TryGetIndex_ShouldReturnNull_WhenConstraintDoesNotExist()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            ISqlConstraintBuilderCollection sut = table.Constraints;

            var result = sut.TryGetIndex( "T" );

            result.Should().BeNull();
        }

        [Fact]
        public void TryGetIndex_ShouldReturnNull_WhenConstraintExistsButNotAsIndex()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            ISqlConstraintBuilderCollection sut = table.Constraints;
            sut.CreateCheck( "CHK", SqlNode.True() );

            var result = sut.TryGetIndex( "CHK" );

            result.Should().BeNull();
        }

        [Fact]
        public void GetForeignKey_ShouldReturnExistingForeignKey()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            ISqlConstraintBuilderCollection sut = table.Constraints;
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
            ISqlConstraintBuilderCollection sut = table.Constraints;

            var action = Lambda.Of( () => sut.GetForeignKey( "T" ) );

            action.Should().ThrowExactly<KeyNotFoundException>();
        }

        [Fact]
        public void GetForeignKey_ShouldThrowSqlObjectCastException_WhenConstraintExistsButNotAsForeignKey()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            ISqlConstraintBuilderCollection sut = table.Constraints;
            var index = sut.SetPrimaryKey( table.Columns.Create( "C" ).Asc() ).Index;

            var action = Lambda.Of( () => sut.GetForeignKey( index.Name ) );

            action.Should()
                .ThrowExactly<SqlObjectCastException>()
                .AndMatch(
                    e => e.Dialect == MySqlDialect.Instance &&
                        e.Expected == typeof( MySqlForeignKeyBuilder ) &&
                        e.Actual == typeof( MySqlIndexBuilder ) );
        }

        [Fact]
        public void TryGetForeignKey_ShouldReturnExistingForeignKey()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            ISqlConstraintBuilderCollection sut = table.Constraints;
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
            ISqlConstraintBuilderCollection sut = table.Constraints;

            var result = sut.TryGetForeignKey( "T" );

            result.Should().BeNull();
        }

        [Fact]
        public void TryGetForeignKey_ShouldReturnNull_WhenConstraintExistsButNotAsForeignKey()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            ISqlConstraintBuilderCollection sut = table.Constraints;
            var index = sut.SetPrimaryKey( table.Columns.Create( "C" ).Asc() ).Index;

            var result = sut.TryGetForeignKey( index.Name );

            result.Should().BeNull();
        }

        [Fact]
        public void GetCheck_ShouldReturnExistingCheck()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            ISqlConstraintBuilderCollection sut = table.Constraints;
            var check = sut.CreateCheck( SqlNode.True() );

            var result = sut.GetCheck( check.Name );

            result.Should().BeSameAs( check );
        }

        [Fact]
        public void GetCheck_ShouldThrowKeyNotFoundException_WhenConstraintDoesNotExist()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            ISqlConstraintBuilderCollection sut = table.Constraints;

            var action = Lambda.Of( () => sut.GetCheck( "T" ) );

            action.Should().ThrowExactly<KeyNotFoundException>();
        }

        [Fact]
        public void GetCheck_ShouldThrowSqlObjectCastException_WhenConstraintExistsButNotAsCheck()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            ISqlConstraintBuilderCollection sut = table.Constraints;
            var index = sut.SetPrimaryKey( table.Columns.Create( "C" ).Asc() ).Index;

            var action = Lambda.Of( () => sut.GetCheck( index.Name ) );

            action.Should()
                .ThrowExactly<SqlObjectCastException>()
                .AndMatch(
                    e => e.Dialect == MySqlDialect.Instance &&
                        e.Expected == typeof( MySqlCheckBuilder ) &&
                        e.Actual == typeof( MySqlIndexBuilder ) );
        }

        [Fact]
        public void TryGetCheck_ShouldReturnExistingCheck()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            ISqlConstraintBuilderCollection sut = table.Constraints;
            var check = sut.CreateCheck( SqlNode.True() );

            var result = sut.TryGetCheck( check.Name );

            result.Should().BeSameAs( check );
        }

        [Fact]
        public void TryGetCheck_ShouldReturnNull_WhenConstraintDoesNotExist()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            ISqlConstraintBuilderCollection sut = table.Constraints;

            var result = sut.TryGetCheck( "T" );

            result.Should().BeNull();
        }

        [Fact]
        public void TryGetCheck_ShouldReturnNull_WhenConstraintExistsButNotAsCheck()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            ISqlConstraintBuilderCollection sut = table.Constraints;
            var index = sut.SetPrimaryKey( table.Columns.Create( "C" ).Asc() ).Index;

            var result = sut.TryGetCheck( index.Name );

            result.Should().BeNull();
        }

        [Fact]
        public void GetPrimaryKey_ShouldReturnPrimaryKey_WhenPrimaryKeyExists()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            ISqlConstraintBuilderCollection sut = table.Constraints;
            var expected = sut.SetPrimaryKey( table.Columns.Create( "C" ).Asc() );

            var result = sut.GetPrimaryKey();

            result.Should().BeSameAs( expected );
        }

        [Fact]
        public void GetPrimaryKey_ShouldThrowMySqlObjectCastException_WhenPrimaryKeyDoesNotExist()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            ISqlConstraintBuilderCollection sut = table.Constraints;

            var action = Lambda.Of( () => sut.GetPrimaryKey() );

            action.Should().ThrowExactly<MySqlObjectBuilderException>();
        }

        [Fact]
        public void TryGetPrimaryKey_ShouldReturnPrimaryKey_WhenPrimaryKeyExists()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            ISqlConstraintBuilderCollection sut = table.Constraints;
            var expected = sut.SetPrimaryKey( table.Columns.Create( "C" ).Asc() );

            var result = sut.TryGetPrimaryKey();

            result.Should().BeSameAs( expected );
        }

        [Fact]
        public void TryGetPrimaryKey_ShouldReturnNull_WhenPrimaryKeyDoesNotExist()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            ISqlConstraintBuilderCollection sut = table.Constraints;

            var result = sut.TryGetPrimaryKey();

            result.Should().BeNull();
        }

        [Fact]
        public void Remove_ShouldRemoveExistingIndex()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            ISqlConstraintBuilderCollection sut = table.Constraints;
            var c1 = table.Columns.Create( "C1" ).Asc();
            var c2 = table.Columns.Create( "C2" ).Desc();
            var index = sut.CreateIndex( c1, c2 );

            var result = sut.Remove( index.Name );

            using ( new AssertionScope() )
            {
                result.Should().BeTrue();
                index.IsRemoved.Should().BeTrue();
                c1.Column.ReferencingIndexes.Should().BeEmpty();
                c2.Column.ReferencingIndexes.Should().BeEmpty();
                schema.Objects.Contains( index.Name ).Should().BeFalse();
                sut.Count.Should().Be( 0 );
                sut.Contains( index.Name ).Should().BeFalse();
            }
        }

        [Fact]
        public void Remove_ShouldReturnFalse_WhenIndexContainsReferencingForeignKeys()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var t1 = schema.Objects.CreateTable( "T1" );
            var c1 = t1.Columns.Create( "C1" ).Asc();
            ISqlConstraintBuilderCollection sut = t1.Constraints;
            var index = t1.Constraints.SetPrimaryKey( c1 ).Index;
            var t2 = schema.Objects.CreateTable( "T2" );
            t2.Constraints.CreateForeignKey( t2.Constraints.SetPrimaryKey( t2.Columns.Create( "C2" ).Desc() ).Index, index );

            var result = sut.Remove( index.Name );

            using ( new AssertionScope() )
            {
                result.Should().BeFalse();
                sut.Count.Should().Be( 2 );
                index.IsRemoved.Should().BeFalse();
            }
        }

        [Fact]
        public void Remove_ShouldRemoveExistingPrimaryKey()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            ISqlConstraintBuilderCollection sut = table.Constraints;
            var column = table.Columns.Create( "C" );
            var pk = table.Constraints.SetPrimaryKey( column.Asc() );

            var result = sut.Remove( pk.Name );

            using ( new AssertionScope() )
            {
                result.Should().BeTrue();
                sut.Count.Should().Be( 0 );
                sut.TryGetPrimaryKey().Should().BeNull();
                sut.Contains( pk.Name ).Should().BeFalse();
                sut.Contains( pk.Index.Name ).Should().BeFalse();
                schema.Objects.Contains( pk.Name ).Should().BeFalse();
                schema.Objects.Contains( pk.Index.Name ).Should().BeFalse();
                pk.IsRemoved.Should().BeTrue();
                pk.Index.IsRemoved.Should().BeTrue();
            }
        }

        [Fact]
        public void Remove_ShouldReturnFalse_WhenPrimaryKeyToRemoveHasExternalReferences()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            ISqlConstraintBuilderCollection sut = table.Constraints;
            var column = table.Columns.Create( "C" );
            var pk = table.Constraints.SetPrimaryKey( column.Asc() );

            var otherTable = schema.Objects.CreateTable( "U" );
            var otherColumn = otherTable.Columns.Create( "D" );
            var otherPk = otherTable.Constraints.SetPrimaryKey( otherColumn.Asc() );
            otherTable.Constraints.CreateForeignKey( otherPk.Index, pk.Index );

            var result = sut.Remove( pk.Name );

            using ( new AssertionScope() )
            {
                result.Should().BeFalse();
                sut.Count.Should().Be( 2 );
                pk.IsRemoved.Should().BeFalse();
                pk.Index.IsRemoved.Should().BeFalse();
            }
        }

        [Fact]
        public void Remove_ShouldRemoveExistingForeignKey()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            ISqlConstraintBuilderCollection sut = table.Constraints;
            var ix1 = sut.CreateIndex( table.Columns.Create( "C1" ).Asc() );
            var ix2 = sut.CreateUniqueIndex( table.Columns.Create( "C2" ).Asc() );
            var fk = sut.CreateForeignKey( ix1, ix2 );

            var result = sut.Remove( fk.Name );

            using ( new AssertionScope() )
            {
                result.Should().BeTrue();
                fk.IsRemoved.Should().BeTrue();
                sut.Count.Should().Be( 2 );
                sut.Contains( fk.Name ).Should().BeFalse();
                schema.Objects.Contains( fk.Name ).Should().BeFalse();
            }
        }

        [Fact]
        public void Remove_ShouldRemoveExistingCheck()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            ISqlConstraintBuilderCollection sut = table.Constraints;
            var c = table.Columns.Create( "C" );
            var check = sut.CreateCheck( c.Node > SqlNode.Literal( 0 ) );

            var result = sut.Remove( check.Name );

            using ( new AssertionScope() )
            {
                result.Should().BeTrue();
                check.IsRemoved.Should().BeTrue();
                c.ReferencingChecks.Should().BeEmpty();
                sut.Count.Should().Be( 0 );
                sut.Contains( check.Name ).Should().BeFalse();
                schema.Objects.Contains( check.Name ).Should().BeFalse();
            }
        }

        [Fact]
        public void Remove_ShouldReturnFalse_WhenConstraintDoesNotExist()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            ISqlConstraintBuilderCollection sut = table.Constraints;

            var result = sut.Remove( Fixture.Create<string>() );

            result.Should().BeFalse();
        }
    }
}
