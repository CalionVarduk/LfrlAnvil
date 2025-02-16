using System.Collections.Generic;
using System.Text.RegularExpressions;
using LfrlAnvil.Functional;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Extensions;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.TestExtensions.Sql.Mocks;

namespace LfrlAnvil.Sql.Tests.ObjectsTests.BuildersTests;

public partial class SqlTableBuilderTests
{
    public class Constraints : TestsBase
    {
        [Theory]
        [InlineData( false, "IX_T_C1A_C2D" )]
        [InlineData( true, "UIX_T_C1A_C2D" )]
        public void CreateIndex_ShouldCreateNewIndex(bool isUnique, string expectedName)
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Constraints;
            var c1 = table.Columns.Create( "C1" );
            var c2 = table.Columns.Create( "C2" );
            var ixc1 = c1.Asc();
            var ixc2 = c2.Desc();

            var result = sut.CreateIndex( new[] { ixc1, ixc2 }, isUnique );

            Assertion.All(
                    result.Table.TestRefEquals( table ),
                    result.Database.TestRefEquals( table.Database ),
                    result.Type.TestEquals( SqlObjectType.Index ),
                    result.Name.TestEquals( expectedName ),
                    result.Columns.Expressions.TestSequence( [ ixc1, ixc2 ] ),
                    result.ReferencedColumns.TestSequence( [ c1, c2 ] ),
                    result.ReferencedFilterColumns.TestEmpty(),
                    result.PrimaryKey.TestNull(),
                    result.IsUnique.TestEquals( isUnique ),
                    result.IsVirtual.TestFalse(),
                    result.Filter.TestNull(),
                    result.ReferencingObjects.TestEmpty(),
                    (( ISqlIndexBuilder )result).PrimaryKey.TestRefEquals( result.PrimaryKey ),
                    (( ISqlIndexBuilder )result).Columns.Expressions.TestSequence( result.Columns.Expressions ),
                    (( ISqlIndexBuilder )result).ReferencedColumns.TestSequence(
                        result.ReferencedColumns.UnsafeReinterpretAs<ISqlColumnBuilder>() ),
                    (( ISqlIndexBuilder )result).ReferencedFilterColumns.TestSequence(
                        result.ReferencedFilterColumns.UnsafeReinterpretAs<ISqlColumnBuilder>() ),
                    (( ISqlConstraintBuilder )result).Table.TestRefEquals( result.Table ),
                    (( ISqlObjectBuilder )result).Database.TestRefEquals( result.Database ),
                    (( ISqlObjectBuilder )result).ReferencingObjects.TestSequence(
                        result.ReferencingObjects.UnsafeReinterpretAs<ISqlObjectBuilder>() ),
                    sut.Count.TestEquals( 1 ),
                    sut.TestSequence( [ result ] ),
                    schema.Objects.TryGet( result.Name ).TestRefEquals( result ),
                    c1.ReferencingObjects.TestSequence(
                        [ SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( result ), c1 ) ] ),
                    c2.ReferencingObjects.TestSequence(
                        [ SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( result ), c2 ) ] ) )
                .Go();
        }

        [Theory]
        [InlineData( false )]
        [InlineData( true )]
        public void CreateIndex_WithExplicitName_ShouldCreateNewIndex(bool isUnique)
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Constraints;
            var c1 = table.Columns.Create( "C1" );
            var c2 = table.Columns.Create( "C2" );
            var ixc1 = c1.Asc();
            var ixc2 = c2.Desc();

            var result = sut.CreateIndex( "IX_T", new[] { ixc1, ixc2 }, isUnique );

            Assertion.All(
                    result.Table.TestRefEquals( table ),
                    result.Database.TestRefEquals( table.Database ),
                    result.Type.TestEquals( SqlObjectType.Index ),
                    result.Name.TestEquals( "IX_T" ),
                    result.Columns.Expressions.TestSequence( [ ixc1, ixc2 ] ),
                    result.ReferencedColumns.TestSequence( [ c1, c2 ] ),
                    result.ReferencedFilterColumns.TestEmpty(),
                    result.PrimaryKey.TestNull(),
                    result.IsUnique.TestEquals( isUnique ),
                    result.IsVirtual.TestFalse(),
                    result.Filter.TestNull(),
                    result.ReferencingObjects.TestEmpty(),
                    (( ISqlIndexBuilder )result).PrimaryKey.TestRefEquals( result.PrimaryKey ),
                    (( ISqlIndexBuilder )result).Columns.Expressions.TestSequence( result.Columns.Expressions ),
                    (( ISqlIndexBuilder )result).ReferencedColumns.TestSequence(
                        result.ReferencedColumns.UnsafeReinterpretAs<ISqlColumnBuilder>() ),
                    (( ISqlIndexBuilder )result).ReferencedFilterColumns.TestSequence(
                        result.ReferencedFilterColumns.UnsafeReinterpretAs<ISqlColumnBuilder>() ),
                    (( ISqlConstraintBuilder )result).Table.TestRefEquals( result.Table ),
                    (( ISqlObjectBuilder )result).Database.TestRefEquals( result.Database ),
                    (( ISqlObjectBuilder )result).ReferencingObjects.TestSequence(
                        result.ReferencingObjects.UnsafeReinterpretAs<ISqlObjectBuilder>() ),
                    sut.Count.TestEquals( 1 ),
                    sut.TestSequence( [ result ] ),
                    schema.Objects.TryGet( result.Name ).TestRefEquals( result ),
                    c1.ReferencingObjects.TestSequence(
                        [ SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( result ), c1 ) ] ),
                    c2.ReferencingObjects.TestSequence(
                        [ SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( result ), c2 ) ] ) )
                .Go();
        }

        [Fact]
        public void CreateIndex_ShouldCreateNewIndex_WithExpressions()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Constraints;
            var c1 = table.Columns.Create( "C1" );
            var c2 = table.Columns.Create( "C2" );
            var ixc1 = c1.Asc();
            var ixc2 = (c2.Node + SqlNode.Literal( 1 )).Desc();
            var ixc3 = (c1.Node + c2.Node).Asc();

            var result = sut.CreateIndex( ixc1, ixc2, ixc3 );

            Assertion.All(
                    result.Table.TestRefEquals( table ),
                    result.Database.TestRefEquals( table.Database ),
                    result.Type.TestEquals( SqlObjectType.Index ),
                    result.Name.TestEquals( "IX_T_C1A_E1D_E2A" ),
                    result.Columns.Expressions.TestSequence( [ ixc1, ixc2, ixc3 ] ),
                    result.ReferencedColumns.TestSequence( [ c1, c2 ] ),
                    result.ReferencedFilterColumns.TestEmpty(),
                    result.PrimaryKey.TestNull(),
                    result.IsUnique.TestFalse(),
                    result.IsVirtual.TestFalse(),
                    result.Filter.TestNull(),
                    result.ReferencingObjects.TestEmpty(),
                    (( ISqlIndexBuilder )result).PrimaryKey.TestRefEquals( result.PrimaryKey ),
                    (( ISqlIndexBuilder )result).Columns.Expressions.TestSequence( result.Columns.Expressions ),
                    (( ISqlIndexBuilder )result).ReferencedColumns.TestSequence(
                        result.ReferencedColumns.UnsafeReinterpretAs<ISqlColumnBuilder>() ),
                    (( ISqlIndexBuilder )result).ReferencedFilterColumns.TestSequence(
                        result.ReferencedFilterColumns.UnsafeReinterpretAs<ISqlColumnBuilder>() ),
                    (( ISqlConstraintBuilder )result).Table.TestRefEquals( result.Table ),
                    (( ISqlObjectBuilder )result).Database.TestRefEquals( result.Database ),
                    (( ISqlObjectBuilder )result).ReferencingObjects.TestSequence(
                        result.ReferencingObjects.UnsafeReinterpretAs<ISqlObjectBuilder>() ),
                    sut.Count.TestEquals( 1 ),
                    sut.TestSequence( [ result ] ),
                    schema.Objects.TryGet( result.Name ).TestRefEquals( result ),
                    c1.ReferencingObjects.TestSequence(
                        [ SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( result ), c1 ) ] ),
                    c2.ReferencingObjects.TestSequence(
                        [ SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( result ), c2 ) ] ) )
                .Go();
        }

        [Fact]
        public void CreateIndex_ShouldThrowSqlObjectBuilderException_WhenTableIsRemoved()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Constraints;
            var column = table.Columns.Create( "C" ).Asc();
            table.Remove();

            var action = Lambda.Of( () => sut.CreateIndex( column ) );

            action.Test(
                    exc => exc.TestType()
                        .Exact<SqlObjectBuilderException>(
                            e => Assertion.All( e.Dialect.TestEquals( SqlDialectMock.Instance ), e.Errors.Count.TestEquals( 1 ) ) ) )
                .Go();
        }

        [Fact]
        public void CreateIndex_ShouldThrowSqlObjectBuilderException_WhenSchemaObjectNameAlreadyExists()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Constraints;
            var column = table.Columns.Create( "C" ).Asc();

            var action = Lambda.Of( () => sut.CreateIndex( "T", column ) );

            action.Test(
                    exc => exc.TestType()
                        .Exact<SqlObjectBuilderException>(
                            e => Assertion.All( e.Dialect.TestEquals( SqlDialectMock.Instance ), e.Errors.Count.TestEquals( 1 ) ) ) )
                .Go();
        }

        [Fact]
        public void CreateIndex_ShouldThrowSqlObjectBuilderException_WhenColumnsAreEmpty()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Constraints;

            var action = Lambda.Of( () => sut.CreateIndex() );

            action.Test(
                    exc => exc.TestType()
                        .Exact<SqlObjectBuilderException>(
                            e => Assertion.All( e.Dialect.TestEquals( SqlDialectMock.Instance ), e.Errors.Count.TestEquals( 1 ) ) ) )
                .Go();
        }

        [Fact]
        public void CreateIndex_ShouldThrowSqlObjectBuilderException_WhenAtLeastOneColumnBelongsToAnotherTable()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var t1 = schema.Objects.CreateTable( "T1" );
            var c1 = t1.Columns.Create( "C1" );
            t1.Constraints.SetPrimaryKey( c1.Asc() );

            var t2 = schema.Objects.CreateTable( "T2" );
            var sut = t2.Constraints;

            var action = Lambda.Of( () => sut.CreateIndex( c1.Asc() ) );

            action.Test(
                    exc => exc.TestType()
                        .Exact<SqlObjectBuilderException>(
                            e => Assertion.All( e.Dialect.TestEquals( SqlDialectMock.Instance ), e.Errors.Count.TestEquals( 1 ) ) ) )
                .Go();
        }

        [Fact]
        public void CreateIndex_ShouldThrowSqlObjectBuilderException_WhenAtLeastOneColumnIsRemoved()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var column = table.Columns.Create( "C" );
            var sut = table.Constraints;
            var ixColumn = column.Asc();
            column.Remove();

            var action = Lambda.Of( () => sut.CreateIndex( ixColumn ) );

            action.Test(
                    exc => exc.TestType()
                        .Exact<SqlObjectBuilderException>(
                            e => Assertion.All( e.Dialect.TestEquals( SqlDialectMock.Instance ), e.Errors.Count.TestEquals( 1 ) ) ) )
                .Go();
        }

        [Fact]
        public void CreateIndex_ShouldThrowSqlObjectBuilderException_WhenAtLeastOneColumnIsDuplicated()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var column = table.Columns.Create( "C" );
            var sut = table.Constraints;

            var action = Lambda.Of( () => sut.CreateIndex( column.Asc(), column.Desc() ) );

            action.Test(
                    exc => exc.TestType()
                        .Exact<SqlObjectBuilderException>(
                            e => Assertion.All( e.Dialect.TestEquals( SqlDialectMock.Instance ), e.Errors.Count.TestEquals( 1 ) ) ) )
                .Go();
        }

        [Fact]
        public void CreateIndex_ShouldThrowSqlObjectBuilderException_WhenUniqueIndexContainsExpressions()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var c1 = table.Columns.Create( "C1" );
            var c2 = table.Columns.Create( "C2" );
            var sut = table.Constraints;

            var action = Lambda.Of( () => sut.CreateUniqueIndex( c1.Asc(), (c2.Node + SqlNode.Literal( 1 )).Desc() ) );

            action.Test(
                    exc => exc.TestType()
                        .Exact<SqlObjectBuilderException>(
                            e => Assertion.All( e.Dialect.TestEquals( SqlDialectMock.Instance ), e.Errors.Count.TestEquals( 1 ) ) ) )
                .Go();
        }

        [Theory]
        [InlineData( "" )]
        [InlineData( " " )]
        [InlineData( "'" )]
        [InlineData( "f\'oo" )]
        public void CreateIndex_ShouldThrowSqlObjectBuilderException_WhenNameIsInvalid(string name)
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Constraints;
            var c = table.Columns.Create( "C" ).Asc();

            var action = Lambda.Of( () => sut.CreateIndex( name, c ) );

            action.Test(
                    exc => exc.TestType()
                        .Exact<SqlObjectBuilderException>(
                            e => Assertion.All( e.Dialect.TestEquals( SqlDialectMock.Instance ), e.Errors.Count.TestEquals( 1 ) ) ) )
                .Go();
        }

        [Fact]
        public void SetPrimaryKey_ShouldCreatePrimaryKey_WhenTableDoesNotHaveOne()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Constraints;
            var column = table.Columns.Create( "C" );
            var ixColumn = column.Asc();

            var result = sut.SetPrimaryKey( ixColumn );

            Assertion.All(
                    result.Table.TestRefEquals( table ),
                    result.Database.TestRefEquals( table.Database ),
                    result.Type.TestEquals( SqlObjectType.PrimaryKey ),
                    result.Name.TestEquals( "PK_T" ),
                    result.Index.Table.TestRefEquals( table ),
                    result.Index.Database.TestRefEquals( schema.Database ),
                    result.Index.Type.TestEquals( SqlObjectType.Index ),
                    result.Index.Name.TestEquals( "UIX_T_CA" ),
                    result.Index.Columns.Expressions.TestSequence( [ ixColumn ] ),
                    result.Index.ReferencedColumns.TestSequence( [ column ] ),
                    result.Index.ReferencedFilterColumns.TestEmpty(),
                    result.Index.PrimaryKey.TestRefEquals( result ),
                    result.Index.IsUnique.TestTrue(),
                    result.Index.IsVirtual.TestTrue(),
                    result.Index.Filter.TestNull(),
                    result.ReferencingObjects.TestEmpty(),
                    (( ISqlPrimaryKeyBuilder )result).Index.TestRefEquals( result.Index ),
                    (( ISqlConstraintBuilder )result).Table.TestRefEquals( result.Table ),
                    (( ISqlObjectBuilder )result).Database.TestRefEquals( result.Database ),
                    (( ISqlObjectBuilder )result).ReferencingObjects.TestSequence(
                        result.ReferencingObjects.UnsafeReinterpretAs<ISqlObjectBuilder>() ),
                    sut.Count.TestEquals( 2 ),
                    sut.TestSetEqual( [ result, result.Index ] ),
                    sut.TryGetPrimaryKey().TestRefEquals( result ),
                    schema.Objects.TryGet( result.Name ).TestRefEquals( result ),
                    schema.Objects.TryGet( result.Index.Name ).TestRefEquals( result.Index ),
                    column.ReferencingObjects.TestSequence(
                        [ SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( result.Index ), column ) ] ) )
                .Go();
        }

        [Fact]
        public void SetPrimaryKey_ShouldDoNothing_WhenPrimaryKeyIndexAndPrimaryKeyNameDoNotChange()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Constraints;
            var c1 = table.Columns.Create( "C1" );
            var c2 = table.Columns.Create( "C2" );
            var c3 = table.Columns.Create( "C3" );
            var oldPk = table.Constraints.SetPrimaryKey( c1.Asc(), c2.Asc(), c3.Asc() );

            var result = sut.SetPrimaryKey( oldPk.Name, oldPk.Index );

            Assertion.All(
                    result.TestRefEquals( oldPk ),
                    result.TestRefEquals( table.Constraints.TryGetPrimaryKey() ) )
                .Go();
        }

        [Fact]
        public void SetPrimaryKey_ShouldUpdatePrimaryKey_WhenPrimaryKeyNameChanges()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Constraints;
            var c1 = table.Columns.Create( "C1" );
            var c2 = table.Columns.Create( "C2" );
            var oldPk = sut.SetPrimaryKey( c1.Asc(), c2.Asc() );

            var result = sut.SetPrimaryKey( "PK_NEW", oldPk.Index );

            Assertion.All(
                    result.TestRefEquals( oldPk ),
                    result.TestRefEquals( table.Constraints.TryGetPrimaryKey() ),
                    result.Name.TestEquals( "PK_NEW" ) )
                .Go();
        }

        [Fact]
        public void SetPrimaryKey_ShouldCreatePrimaryKeyAndRemoveOldOne_WhenPrimaryKeyIndexChanges()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Constraints;
            var c1 = table.Columns.Create( "C1" );
            var c2 = table.Columns.Create( "C2" );
            var oldPk = sut.SetPrimaryKey( c1.Asc() );
            var ixColumn2 = c2.Asc();

            var result = sut.SetPrimaryKey( oldPk.Name, ixColumn2 );

            Assertion.All(
                    result.Table.TestRefEquals( table ),
                    result.Database.TestRefEquals( table.Database ),
                    result.Type.TestEquals( SqlObjectType.PrimaryKey ),
                    result.Name.TestEquals( "PK_T" ),
                    result.Index.Table.TestRefEquals( table ),
                    result.Index.Database.TestRefEquals( schema.Database ),
                    result.Index.Type.TestEquals( SqlObjectType.Index ),
                    result.Index.Name.TestEquals( "UIX_T_C2A" ),
                    result.Index.Columns.Expressions.TestSequence( [ ixColumn2 ] ),
                    result.Index.ReferencedColumns.TestSequence( [ c2 ] ),
                    result.Index.ReferencedFilterColumns.TestEmpty(),
                    result.Index.PrimaryKey.TestRefEquals( result ),
                    result.Index.IsUnique.TestTrue(),
                    result.Index.IsVirtual.TestTrue(),
                    result.Index.Filter.TestNull(),
                    result.ReferencingObjects.TestEmpty(),
                    (( ISqlPrimaryKeyBuilder )result).Index.TestRefEquals( result.Index ),
                    (( ISqlConstraintBuilder )result).Table.TestRefEquals( result.Table ),
                    (( ISqlObjectBuilder )result).Database.TestRefEquals( result.Database ),
                    (( ISqlObjectBuilder )result).ReferencingObjects.TestSequence(
                        result.ReferencingObjects.UnsafeReinterpretAs<ISqlObjectBuilder>() ),
                    sut.Count.TestEquals( 2 ),
                    sut.TestSetEqual( [ result, result.Index ] ),
                    sut.TryGetPrimaryKey().TestRefEquals( result ),
                    schema.Objects.TryGet( result.Name ).TestRefEquals( result ),
                    schema.Objects.TryGet( result.Index.Name ).TestRefEquals( result.Index ),
                    schema.Objects.TryGet( oldPk.Index.Name ).TestNull(),
                    oldPk.IsRemoved.TestTrue(),
                    oldPk.Index.IsRemoved.TestTrue() )
                .Go();
        }

        [Fact]
        public void SetPrimaryKey_ShouldCreatePrimaryKeyAndRemoveOldOne_WhenPrimaryKeyIndexAndPrimaryKeyNameChanges()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Constraints;
            var c1 = table.Columns.Create( "C1" );
            var c2 = table.Columns.Create( "C2" );
            var oldPk = sut.SetPrimaryKey( c1.Asc() );
            var ixColumn2 = c2.Asc();

            var result = sut.SetPrimaryKey( "PK_NEW", ixColumn2 );

            Assertion.All(
                    result.Table.TestRefEquals( table ),
                    result.Database.TestRefEquals( table.Database ),
                    result.Type.TestEquals( SqlObjectType.PrimaryKey ),
                    result.Name.TestEquals( "PK_NEW" ),
                    result.Index.Table.TestRefEquals( table ),
                    result.Index.Database.TestRefEquals( schema.Database ),
                    result.Index.Type.TestEquals( SqlObjectType.Index ),
                    result.Index.Name.TestEquals( "UIX_T_C2A" ),
                    result.Index.Columns.Expressions.TestSequence( [ ixColumn2 ] ),
                    result.Index.ReferencedColumns.TestSequence( [ c2 ] ),
                    result.Index.ReferencedFilterColumns.TestEmpty(),
                    result.Index.PrimaryKey.TestRefEquals( result ),
                    result.Index.IsUnique.TestTrue(),
                    result.Index.IsVirtual.TestTrue(),
                    result.Index.Filter.TestNull(),
                    result.ReferencingObjects.TestEmpty(),
                    (( ISqlPrimaryKeyBuilder )result).Index.TestRefEquals( result.Index ),
                    (( ISqlConstraintBuilder )result).Table.TestRefEquals( result.Table ),
                    (( ISqlObjectBuilder )result).Database.TestRefEquals( result.Database ),
                    (( ISqlObjectBuilder )result).ReferencingObjects.TestSequence(
                        result.ReferencingObjects.UnsafeReinterpretAs<ISqlObjectBuilder>() ),
                    sut.Count.TestEquals( 2 ),
                    sut.TestSetEqual( [ result, result.Index ] ),
                    sut.TryGetPrimaryKey().TestRefEquals( result ),
                    schema.Objects.TryGet( result.Name ).TestRefEquals( result ),
                    schema.Objects.TryGet( result.Index.Name ).TestRefEquals( result.Index ),
                    schema.Objects.TryGet( oldPk.Name ).TestNull(),
                    schema.Objects.TryGet( oldPk.Index.Name ).TestNull(),
                    oldPk.IsRemoved.TestTrue(),
                    oldPk.Index.IsRemoved.TestTrue() )
                .Go();
        }

        [Fact]
        public void
            SetPrimaryKey_ShouldCreatePrimaryKeyAndRemoveOldOne_WhenPrimaryKeyIndexAndPrimaryKeyNameChangesToOldPrimaryKeyIndexName()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Constraints;
            var c1 = table.Columns.Create( "C1" );
            var c2 = table.Columns.Create( "C2" );
            var oldPk = sut.SetPrimaryKey( c1.Asc() );
            var ixColumn2 = c2.Asc();

            var result = sut.SetPrimaryKey( oldPk.Index.Name, ixColumn2 );

            Assertion.All(
                    result.Table.TestRefEquals( table ),
                    result.Database.TestRefEquals( table.Database ),
                    result.Type.TestEquals( SqlObjectType.PrimaryKey ),
                    result.Name.TestEquals( "UIX_T_C1A" ),
                    result.Index.Table.TestRefEquals( table ),
                    result.Index.Database.TestRefEquals( schema.Database ),
                    result.Index.Type.TestEquals( SqlObjectType.Index ),
                    result.Index.Name.TestEquals( "UIX_T_C2A" ),
                    result.Index.Columns.Expressions.TestSequence( [ ixColumn2 ] ),
                    result.Index.ReferencedColumns.TestSequence( [ c2 ] ),
                    result.Index.ReferencedFilterColumns.TestEmpty(),
                    result.Index.PrimaryKey.TestRefEquals( result ),
                    result.Index.IsUnique.TestTrue(),
                    result.Index.IsVirtual.TestTrue(),
                    result.Index.Filter.TestNull(),
                    result.ReferencingObjects.TestEmpty(),
                    (( ISqlPrimaryKeyBuilder )result).Index.TestRefEquals( result.Index ),
                    (( ISqlConstraintBuilder )result).Table.TestRefEquals( result.Table ),
                    (( ISqlObjectBuilder )result).Database.TestRefEquals( result.Database ),
                    (( ISqlObjectBuilder )result).ReferencingObjects.TestSequence(
                        result.ReferencingObjects.UnsafeReinterpretAs<ISqlObjectBuilder>() ),
                    sut.Count.TestEquals( 2 ),
                    sut.TestSetEqual( [ result, result.Index ] ),
                    sut.TryGetPrimaryKey().TestRefEquals( result ),
                    schema.Objects.TryGet( result.Name ).TestRefEquals( result ),
                    schema.Objects.TryGet( result.Index.Name ).TestRefEquals( result.Index ),
                    schema.Objects.TryGet( oldPk.Name ).TestNull(),
                    oldPk.IsRemoved.TestTrue(),
                    oldPk.Index.IsRemoved.TestTrue() )
                .Go();
        }

        [Fact]
        public void SetPrimaryKey_ShouldThrowSqlObjectBuilderException_WhenCurrentPrimaryKeyIndexCannotBeRemoved()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Constraints;
            var c1 = table.Columns.Create( "C1" );
            var c2 = table.Columns.Create( "C2" );
            var oldPk = sut.SetPrimaryKey( c1.Asc() );
            sut.CreateForeignKey( sut.CreateIndex( table.Columns.Create( "C3" ).Asc() ), oldPk.Index );

            var action = Lambda.Of( () => sut.SetPrimaryKey( c2.Asc() ) );

            action.Test(
                    exc => exc.TestType()
                        .Exact<SqlObjectBuilderException>(
                            e => Assertion.All( e.Dialect.TestEquals( SqlDialectMock.Instance ), e.Errors.Count.TestEquals( 1 ) ) ) )
                .Go();
        }

        [Fact]
        public void SetPrimaryKey_ShouldThrowSqlObjectBuilderException_WhenPrimaryKeyNameExistsDuringCreationInSchemaObjects()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Constraints;
            var c1 = table.Columns.Create( "C1" );
            var index = sut.CreateUniqueIndex( c1.Asc() );

            var action = Lambda.Of( () => sut.SetPrimaryKey( index.Name, index ) );

            action.Test(
                    exc => exc.TestType()
                        .Exact<SqlObjectBuilderException>(
                            e => Assertion.All( e.Dialect.TestEquals( SqlDialectMock.Instance ), e.Errors.Count.TestEquals( 1 ) ) ) )
                .Go();
        }

        [Fact]
        public void SetPrimaryKey_ShouldThrowSqlObjectBuilderException_WhenPrimaryKeyNameExistsDuringUpdateInSchemaObjects()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Constraints;
            var c1 = table.Columns.Create( "C1" );
            var c2 = table.Columns.Create( "C2" );
            var index = sut.CreateUniqueIndex( c1.Asc() );
            sut.SetPrimaryKey( c2.Asc() );

            var action = Lambda.Of( () => sut.SetPrimaryKey( index.Name, index ) );

            action.Test(
                    exc => exc.TestType()
                        .Exact<SqlObjectBuilderException>(
                            e => Assertion.All( e.Dialect.TestEquals( SqlDialectMock.Instance ), e.Errors.Count.TestEquals( 1 ) ) ) )
                .Go();
        }

        [Fact]
        public void SetPrimaryKey_ShouldThrowSqlObjectBuilderException_WhenIndexIsRemoved()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Constraints;
            var column = table.Columns.Create( "C" );
            var index = sut.CreateUniqueIndex( "UIX", column.Asc() );
            index.Remove();

            var action = Lambda.Of( () => sut.SetPrimaryKey( index ) );

            action.Test(
                    exc => exc.TestType()
                        .Exact<SqlObjectBuilderException>(
                            e => Assertion.All( e.Dialect.TestEquals( SqlDialectMock.Instance ), e.Errors.Count.TestEquals( 1 ) ) ) )
                .Go();
        }

        [Fact]
        public void SetPrimaryKey_ShouldThrowSqlObjectBuilderException_WhenIndexIsNotUnique()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Constraints;
            var column = table.Columns.Create( "C" );
            var index = sut.CreateIndex( column.Asc() );

            var action = Lambda.Of( () => sut.SetPrimaryKey( index ) );

            action.Test(
                    exc => exc.TestType()
                        .Exact<SqlObjectBuilderException>(
                            e => Assertion.All( e.Dialect.TestEquals( SqlDialectMock.Instance ), e.Errors.Count.TestEquals( 1 ) ) ) )
                .Go();
        }

        [Fact]
        public void SetPrimaryKey_ShouldThrowSqlObjectBuilderException_WhenIndexContainsExpressions()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Constraints;
            var column = table.Columns.Create( "C" );
            var index = sut.CreateIndex( (column.Node + SqlNode.Literal( 1 )).Asc() );

            var action = Lambda.Of( () => sut.SetPrimaryKey( index ) );

            action.Test(
                    exc => exc.TestType()
                        .Exact<SqlObjectBuilderException>(
                            e => Assertion.All( e.Dialect.TestEquals( SqlDialectMock.Instance ), e.Errors.Count.TestEquals( 2 ) ) ) )
                .Go();
        }

        [Fact]
        public void SetPrimaryKey_ShouldThrowSqlObjectBuilderException_WhenIndexIsPartial()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Constraints;
            var column = table.Columns.Create( "C" );
            var index = sut.CreateUniqueIndex( column.Asc() ).SetFilter( SqlNode.True() );

            var action = Lambda.Of( () => sut.SetPrimaryKey( index ) );

            action.Test(
                    exc => exc.TestType()
                        .Exact<SqlObjectBuilderException>(
                            e => Assertion.All( e.Dialect.TestEquals( SqlDialectMock.Instance ), e.Errors.Count.TestEquals( 1 ) ) ) )
                .Go();
        }

        [Fact]
        public void SetPrimaryKey_ShouldThrowSqlObjectBuilderException_WhenIndexBelongsToAnotherTable()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Default;
            var otherTable = schema.Objects.CreateTable( "U" );
            var index = otherTable.Constraints.SetPrimaryKey( otherTable.Columns.Create( "C" ).Asc() ).Index;
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Constraints;

            var action = Lambda.Of( () => sut.SetPrimaryKey( index ) );

            action.Test(
                    exc => exc.TestType()
                        .Exact<SqlObjectBuilderException>(
                            e => Assertion.All( e.Dialect.TestEquals( SqlDialectMock.Instance ), e.Errors.Count.TestEquals( 1 ) ) ) )
                .Go();
        }

        [Fact]
        public void SetPrimaryKey_ShouldThrowSqlObjectBuilderException_WhenAtLeastOneColumnIsNullable()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Constraints;
            var c1 = table.Columns.Create( "C1" );
            var c2 = table.Columns.Create( "C2" ).MarkAsNullable();
            var index = sut.CreateUniqueIndex( c1.Asc(), c2.Asc() );

            var action = Lambda.Of( () => sut.SetPrimaryKey( index ) );

            action.Test(
                    exc => exc.TestType()
                        .Exact<SqlObjectBuilderException>(
                            e => Assertion.All( e.Dialect.TestEquals( SqlDialectMock.Instance ), e.Errors.Count.TestEquals( 1 ) ) ) )
                .Go();
        }

        [Fact]
        public void SetPrimaryKey_ShouldThrowSqlObjectBuilderException_WhenAtLeastOneColumnIsGenerated()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Constraints;
            var c1 = table.Columns.Create( "C1" );
            var c2 = table.Columns.Create( "C2" ).SetComputation( SqlColumnComputation.Virtual( SqlNode.Literal( 1 ) ) );
            var index = sut.CreateUniqueIndex( c1.Asc(), c2.Asc() );

            var action = Lambda.Of( () => sut.SetPrimaryKey( index ) );

            action.Test(
                    exc => exc.TestType()
                        .Exact<SqlObjectBuilderException>(
                            e => Assertion.All( e.Dialect.TestEquals( SqlDialectMock.Instance ), e.Errors.Count.TestEquals( 1 ) ) ) )
                .Go();
        }

        [Fact]
        public void SetPrimaryKey_ShouldThrowSqlObjectBuilderException_WhenTableHasBeenRemoved()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Constraints;
            var column = table.Columns.Create( "C" );
            var index = sut.CreateUniqueIndex( column.Asc() );
            table.Remove();

            var action = Lambda.Of( () => sut.SetPrimaryKey( index ) );

            action.Test(
                    exc => exc.TestType()
                        .Exact<SqlObjectBuilderException>(
                            e => Assertion.All( e.Dialect.TestEquals( SqlDialectMock.Instance ), e.Errors.Count.TestEquals( 1 ) ) ) )
                .Go();
        }

        [Theory]
        [InlineData( "" )]
        [InlineData( " " )]
        [InlineData( "'" )]
        [InlineData( "f\'oo" )]
        public void SetPrimaryKey_ShouldThrowSqlObjectBuilderException_WhenNameIsInvalid(string name)
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Constraints;
            var index = sut.CreateUniqueIndex( table.Columns.Create( "C" ).Asc() );

            var action = Lambda.Of( () => sut.SetPrimaryKey( name, index ) );

            action.Test(
                    exc => exc.TestType()
                        .Exact<SqlObjectBuilderException>(
                            e => Assertion.All( e.Dialect.TestEquals( SqlDialectMock.Instance ), e.Errors.Count.TestEquals( 1 ) ) ) )
                .Go();
        }

        [Fact]
        public void CreateForeignKey_ShouldCreateNewForeignKey_WhenIndexesBelongToTheSameTable()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Constraints;
            var ix1 = sut.CreateIndex( "IX_T_C1", table.Columns.Create( "C1" ).Asc() );
            var ix2 = sut.CreateUniqueIndex( "UIX_T_C2", table.Columns.Create( "C2" ).Asc() );

            var result = sut.CreateForeignKey( ix1, ix2 );

            Assertion.All(
                    result.Table.TestRefEquals( table ),
                    result.Database.TestRefEquals( table.Database ),
                    result.Type.TestEquals( SqlObjectType.ForeignKey ),
                    result.Name.TestEquals( "FK_T_C1_REF_T" ),
                    result.OriginIndex.TestRefEquals( ix1 ),
                    result.ReferencedIndex.TestRefEquals( ix2 ),
                    result.OnUpdateBehavior.TestEquals( ReferenceBehavior.Restrict ),
                    result.OnDeleteBehavior.TestEquals( ReferenceBehavior.Restrict ),
                    result.ReferencingObjects.TestEmpty(),
                    (( ISqlForeignKeyBuilder )result).OriginIndex.TestRefEquals( result.OriginIndex ),
                    (( ISqlForeignKeyBuilder )result).ReferencedIndex.TestRefEquals( result.ReferencedIndex ),
                    (( ISqlConstraintBuilder )result).Table.TestRefEquals( result.Table ),
                    (( ISqlObjectBuilder )result).Database.TestRefEquals( result.Database ),
                    (( ISqlObjectBuilder )result).ReferencingObjects.TestSequence(
                        result.ReferencingObjects.UnsafeReinterpretAs<ISqlObjectBuilder>() ),
                    sut.Count.TestEquals( 3 ),
                    sut.TestSetEqual( [ ix1, ix2, result ] ),
                    schema.Objects.TryGet( result.Name ).TestRefEquals( result ),
                    ix1.ReferencingObjects.TestSequence(
                        [ SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( result ), ix1 ) ] ),
                    ix2.ReferencingObjects.TestSequence(
                        [ SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( result ), ix2 ) ] ),
                    table.ReferencingObjects.TestEmpty(),
                    schema.ReferencingObjects.TestEmpty() )
                .Go();
        }

        [Fact]
        public void CreateForeignKey_ShouldCreateNewForeignKey_WhenIndexesDoNotBelongToTheSameTable()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var t1 = schema.Objects.CreateTable( "T1" );
            var ix2 = t1.Constraints.SetPrimaryKey( t1.Columns.Create( "C1" ).Asc() ).Index;
            var t2 = schema.Objects.CreateTable( "T2" );
            var sut = t2.Constraints;
            var ix1 = sut.CreateIndex( t2.Columns.Create( "C2" ).Asc() );

            var result = sut.CreateForeignKey( ix1, ix2 );

            Assertion.All(
                    result.Table.TestRefEquals( t2 ),
                    result.Database.TestRefEquals( t2.Database ),
                    result.Type.TestEquals( SqlObjectType.ForeignKey ),
                    result.Name.TestEquals( "FK_T2_C2_REF_T1" ),
                    result.OriginIndex.TestRefEquals( ix1 ),
                    result.ReferencedIndex.TestRefEquals( ix2 ),
                    result.OnUpdateBehavior.TestEquals( ReferenceBehavior.Restrict ),
                    result.OnDeleteBehavior.TestEquals( ReferenceBehavior.Restrict ),
                    result.ReferencingObjects.TestEmpty(),
                    (( ISqlForeignKeyBuilder )result).OriginIndex.TestRefEquals( result.OriginIndex ),
                    (( ISqlForeignKeyBuilder )result).ReferencedIndex.TestRefEquals( result.ReferencedIndex ),
                    (( ISqlConstraintBuilder )result).Table.TestRefEquals( result.Table ),
                    (( ISqlObjectBuilder )result).Database.TestRefEquals( result.Database ),
                    (( ISqlObjectBuilder )result).ReferencingObjects.TestSequence(
                        result.ReferencingObjects.UnsafeReinterpretAs<ISqlObjectBuilder>() ),
                    sut.Count.TestEquals( 2 ),
                    sut.TestSetEqual( [ ix1, result ] ),
                    schema.Objects.TryGet( result.Name ).TestRefEquals( result ),
                    ix1.ReferencingObjects.TestSequence(
                        [ SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( result ), ix1 ) ] ),
                    ix2.ReferencingObjects.TestSequence(
                        [ SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( result ), ix2 ) ] ),
                    t1.ReferencingObjects.TestSequence(
                        [ SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( result ), ix2 ) ] ),
                    schema.ReferencingObjects.TestEmpty() )
                .Go();
        }

        [Fact]
        public void CreateForeignKey_ShouldCreateNewForeignKey_WhenIndexesDoNotBelongToTheSameSchema()
        {
            var db = SqlDatabaseBuilderMock.Create();
            var schema1 = db.Schemas.Create( "foo" );
            var schema2 = db.Schemas.Create( "bar" );
            var t1 = schema1.Objects.CreateTable( "T1" );
            var ix2 = t1.Constraints.SetPrimaryKey( t1.Columns.Create( "C1" ).Asc() ).Index;
            var t2 = schema2.Objects.CreateTable( "T2" );
            var sut = t2.Constraints;
            var ix1 = sut.CreateIndex( t2.Columns.Create( "C2" ).Asc() );

            var result = sut.CreateForeignKey( ix1, ix2 );

            Assertion.All(
                    result.Table.TestRefEquals( t2 ),
                    result.Database.TestRefEquals( t2.Database ),
                    result.Type.TestEquals( SqlObjectType.ForeignKey ),
                    result.Name.TestEquals( "FK_T2_C2_REF_foo_T1" ),
                    result.OriginIndex.TestRefEquals( ix1 ),
                    result.ReferencedIndex.TestRefEquals( ix2 ),
                    result.OnUpdateBehavior.TestEquals( ReferenceBehavior.Restrict ),
                    result.OnDeleteBehavior.TestEquals( ReferenceBehavior.Restrict ),
                    result.ReferencingObjects.TestEmpty(),
                    (( ISqlForeignKeyBuilder )result).OriginIndex.TestRefEquals( result.OriginIndex ),
                    (( ISqlForeignKeyBuilder )result).ReferencedIndex.TestRefEquals( result.ReferencedIndex ),
                    (( ISqlConstraintBuilder )result).Table.TestRefEquals( result.Table ),
                    (( ISqlObjectBuilder )result).Database.TestRefEquals( result.Database ),
                    (( ISqlObjectBuilder )result).ReferencingObjects.TestSequence(
                        result.ReferencingObjects.UnsafeReinterpretAs<ISqlObjectBuilder>() ),
                    sut.Count.TestEquals( 2 ),
                    sut.TestSetEqual( [ ix1, result ] ),
                    schema2.Objects.TryGet( result.Name ).TestRefEquals( result ),
                    ix1.ReferencingObjects.TestSequence(
                        [ SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( result ), ix1 ) ] ),
                    ix2.ReferencingObjects.TestSequence(
                        [ SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( result ), ix2 ) ] ),
                    t1.ReferencingObjects.TestSequence(
                        [ SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( result ), ix2 ) ] ),
                    schema1.ReferencingObjects.TestSequence(
                        [ SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( result ), ix2 ) ] ) )
                .Go();
        }

        [Fact]
        public void CreateForeignKey_WithExplicitName_ShouldCreateNewForeignKey()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Constraints;
            var ix1 = sut.CreateIndex( table.Columns.Create( "C1" ).Asc() );
            var ix2 = sut.CreateUniqueIndex( "UIX_T", table.Columns.Create( "C2" ).Asc() );

            var result = sut.CreateForeignKey( "FK_T", ix1, ix2 );

            Assertion.All(
                    result.Table.TestRefEquals( table ),
                    result.Database.TestRefEquals( table.Database ),
                    result.Type.TestEquals( SqlObjectType.ForeignKey ),
                    result.Name.TestEquals( "FK_T" ),
                    result.OriginIndex.TestRefEquals( ix1 ),
                    result.ReferencedIndex.TestRefEquals( ix2 ),
                    result.OnUpdateBehavior.TestEquals( ReferenceBehavior.Restrict ),
                    result.OnDeleteBehavior.TestEquals( ReferenceBehavior.Restrict ),
                    result.ReferencingObjects.TestEmpty(),
                    (( ISqlForeignKeyBuilder )result).OriginIndex.TestRefEquals( result.OriginIndex ),
                    (( ISqlForeignKeyBuilder )result).ReferencedIndex.TestRefEquals( result.ReferencedIndex ),
                    (( ISqlConstraintBuilder )result).Table.TestRefEquals( result.Table ),
                    (( ISqlObjectBuilder )result).Database.TestRefEquals( result.Database ),
                    (( ISqlObjectBuilder )result).ReferencingObjects.TestSequence(
                        result.ReferencingObjects.UnsafeReinterpretAs<ISqlObjectBuilder>() ),
                    sut.Count.TestEquals( 3 ),
                    sut.TestSetEqual( [ ix1, ix2, result ] ),
                    schema.Objects.TryGet( result.Name ).TestRefEquals( result ),
                    ix1.ReferencingObjects.TestSequence(
                        [ SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( result ), ix1 ) ] ),
                    ix2.ReferencingObjects.TestSequence(
                        [ SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( result ), ix2 ) ] ),
                    table.ReferencingObjects.TestEmpty(),
                    schema.ReferencingObjects.TestEmpty() )
                .Go();
        }

        [Fact]
        public void CreateForeignKey_ShouldThrowSqlObjectBuilderException_WhenTableIsRemoved()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Constraints;
            var ix1 = sut.CreateIndex( table.Columns.Create( "C1" ).Asc() );
            var ix2 = sut.CreateUniqueIndex( table.Columns.Create( "C2" ).Asc() );
            table.Remove();

            var action = Lambda.Of( () => sut.CreateForeignKey( ix1, ix2 ) );

            action.Test(
                    exc => exc.TestType()
                        .Exact<SqlObjectBuilderException>(
                            e => Assertion.All( e.Dialect.TestEquals( SqlDialectMock.Instance ), e.Errors.Count.TestEquals( 1 ) ) ) )
                .Go();
        }

        [Fact]
        public void CreateForeignKey_ShouldThrowSqlObjectBuilderException_WhenSchemaObjectNameAlreadyExists()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Constraints;
            var ix1 = sut.CreateIndex( table.Columns.Create( "C1" ).Asc() );
            var ix2 = sut.CreateUniqueIndex( table.Columns.Create( "C2" ).Asc() );

            var action = Lambda.Of( () => sut.CreateForeignKey( "T", ix1, ix2 ) );

            action.Test(
                    exc => exc.TestType()
                        .Exact<SqlObjectBuilderException>(
                            e => Assertion.All( e.Dialect.TestEquals( SqlDialectMock.Instance ), e.Errors.Count.TestEquals( 1 ) ) ) )
                .Go();
        }

        [Fact]
        public void CreateForeignKey_ShouldThrowSqlObjectBuilderException_WhenOriginIndexAndReferencedIndexAreTheSame()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Constraints;
            var ix1 = sut.CreateUniqueIndex( table.Columns.Create( "C1" ).Asc() );

            var action = Lambda.Of( () => sut.CreateForeignKey( ix1, ix1 ) );

            action.Test(
                    exc => exc.TestType()
                        .Exact<SqlObjectBuilderException>(
                            e => Assertion.All( e.Dialect.TestEquals( SqlDialectMock.Instance ), e.Errors.Count.TestEquals( 1 ) ) ) )
                .Go();
        }

        [Fact]
        public void CreateForeignKey_ShouldThrowSqlObjectBuilderException_WhenReferencedIndexIsNotUnique()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Constraints;
            var ix1 = sut.CreateIndex( table.Columns.Create( "C1" ).Asc() );
            var ix2 = sut.CreateIndex( table.Columns.Create( "C2" ).Asc() );

            var action = Lambda.Of( () => sut.CreateForeignKey( ix1, ix2 ) );

            action.Test(
                    exc => exc.TestType()
                        .Exact<SqlObjectBuilderException>(
                            e => Assertion.All( e.Dialect.TestEquals( SqlDialectMock.Instance ), e.Errors.Count.TestEquals( 1 ) ) ) )
                .Go();
        }

        [Fact]
        public void CreateForeignKey_ShouldThrowSqlObjectBuilderException_WhenOriginIndexContainsExpressions()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Constraints;
            var ix1 = sut.CreateIndex( (table.Columns.Create( "C1" ).Node + SqlNode.Literal( 1 )).Asc() );
            var ix2 = sut.CreateUniqueIndex( table.Columns.Create( "C2" ).Asc() );

            var action = Lambda.Of( () => sut.CreateForeignKey( ix1, ix2 ) );

            action.Test(
                    exc => exc.TestType()
                        .Exact<SqlObjectBuilderException>(
                            e => Assertion.All( e.Dialect.TestEquals( SqlDialectMock.Instance ), e.Errors.Count.TestEquals( 1 ) ) ) )
                .Go();
        }

        [Fact]
        public void CreateForeignKey_ShouldThrowSqlObjectBuilderException_WhenReferencedIndexContainsExpressions()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Constraints;
            var ix1 = sut.CreateIndex( table.Columns.Create( "C1" ).Asc() );
            var ix2 = sut.CreateIndex( (table.Columns.Create( "C2" ).Node + SqlNode.Literal( 1 )).Asc() );

            var action = Lambda.Of( () => sut.CreateForeignKey( ix1, ix2 ) );

            action.Test(
                    exc => exc.TestType()
                        .Exact<SqlObjectBuilderException>(
                            e => Assertion.All( e.Dialect.TestEquals( SqlDialectMock.Instance ), e.Errors.Count.TestEquals( 2 ) ) ) )
                .Go();
        }

        [Fact]
        public void CreateForeignKey_ShouldThrowSqlObjectBuilderException_WhenReferencedIndexHasFilter()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Constraints;
            var ix1 = sut.CreateIndex( table.Columns.Create( "C1" ).Asc() );
            var ix2 = sut.CreateUniqueIndex( table.Columns.Create( "C2" ).Asc() ).SetFilter( SqlNode.True() );

            var action = Lambda.Of( () => sut.CreateForeignKey( ix1, ix2 ) );

            action.Test(
                    exc => exc.TestType()
                        .Exact<SqlObjectBuilderException>(
                            e => Assertion.All( e.Dialect.TestEquals( SqlDialectMock.Instance ), e.Errors.Count.TestEquals( 1 ) ) ) )
                .Go();
        }

        [Fact]
        public void CreateForeignKey_ShouldThrowSqlObjectBuilderException_WhenOriginIndexBelongsToAnotherTable()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var t1 = schema.Objects.CreateTable( "T1" );
            var sut = t1.Constraints;
            var ix1 = t1.Constraints.SetPrimaryKey( t1.Columns.Create( "C1" ).Asc() ).Index;
            var t2 = schema.Objects.CreateTable( "T2" );
            var ix2 = t2.Constraints.CreateIndex( t2.Columns.Create( "C2" ).Asc() );

            var action = Lambda.Of( () => sut.CreateForeignKey( ix2, ix1 ) );

            action.Test(
                    exc => exc.TestType()
                        .Exact<SqlObjectBuilderException>(
                            e => Assertion.All( e.Dialect.TestEquals( SqlDialectMock.Instance ), e.Errors.Count.TestEquals( 1 ) ) ) )
                .Go();
        }

        [Fact]
        public void CreateForeignKey_ShouldThrowSqlObjectBuilderException_WhenReferencedIndexBelongsToAnotherDatabase()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var otherSchema = SqlDatabaseBuilderMock.Create().Schemas.Default;
            var t1 = schema.Objects.CreateTable( "T1" );
            var sut = t1.Constraints;
            var ix1 = t1.Constraints.SetPrimaryKey( t1.Columns.Create( "C1" ).Asc() ).Index;
            var t2 = otherSchema.Objects.CreateTable( "T2" );
            var ix2 = t2.Constraints.CreateUniqueIndex( t2.Columns.Create( "C2" ).Asc() );

            var action = Lambda.Of( () => sut.CreateForeignKey( ix1, ix2 ) );

            action.Test(
                    exc => exc.TestType()
                        .Exact<SqlObjectBuilderException>(
                            e => Assertion.All( e.Dialect.TestEquals( SqlDialectMock.Instance ), e.Errors.Count.TestEquals( 1 ) ) ) )
                .Go();
        }

        [Fact]
        public void CreateForeignKey_ShouldThrowSqlObjectBuilderException_WhenOriginIndexIsRemoved()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Constraints;
            var ix1 = sut.CreateIndex( table.Columns.Create( "C1" ).Asc() );
            var ix2 = sut.CreateUniqueIndex( table.Columns.Create( "C2" ).Asc() );
            ix1.Remove();

            var action = Lambda.Of( () => sut.CreateForeignKey( ix1, ix2 ) );

            action.Test(
                    exc => exc.TestType()
                        .Exact<SqlObjectBuilderException>(
                            e => Assertion.All( e.Dialect.TestEquals( SqlDialectMock.Instance ), e.Errors.Count.TestEquals( 2 ) ) ) )
                .Go();
        }

        [Fact]
        public void CreateForeignKey_ShouldThrowSqlObjectBuilderException_WhenReferencedIndexIsRemoved()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Constraints;
            var ix1 = sut.CreateIndex( table.Columns.Create( "C1" ).Asc() );
            var ix2 = sut.CreateUniqueIndex( table.Columns.Create( "C2" ).Asc() );
            ix2.Remove();

            var action = Lambda.Of( () => sut.CreateForeignKey( ix1, ix2 ) );

            action.Test(
                    exc => exc.TestType()
                        .Exact<SqlObjectBuilderException>(
                            e => Assertion.All( e.Dialect.TestEquals( SqlDialectMock.Instance ), e.Errors.Count.TestEquals( 2 ) ) ) )
                .Go();
        }

        [Fact]
        public void CreateForeignKey_ShouldThrowSqlObjectBuilderException_WhenReferencedIndexContainsNullableColumn()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Constraints;
            var ix1 = sut.CreateIndex( table.Columns.Create( "C1" ).Asc(), table.Columns.Create( "C2" ).Asc() );
            var ix2 = sut.CreateUniqueIndex( table.Columns.Create( "C3" ).Asc(), table.Columns.Create( "C4" ).MarkAsNullable().Asc() );

            var action = Lambda.Of( () => sut.CreateForeignKey( ix1, ix2 ) );

            action.Test(
                    exc => exc.TestType()
                        .Exact<SqlObjectBuilderException>(
                            e => Assertion.All( e.Dialect.TestEquals( SqlDialectMock.Instance ), e.Errors.Count.TestEquals( 1 ) ) ) )
                .Go();
        }

        [Fact]
        public void CreateForeignKey_ShouldThrowSqlObjectBuilderException_WhenReferencedIndexContainsGeneratedColumn()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Constraints;
            var ix1 = sut.CreateIndex( table.Columns.Create( "C1" ).Asc(), table.Columns.Create( "C2" ).Asc() );
            var ix2 = sut.CreateUniqueIndex(
                table.Columns.Create( "C3" ).Asc(),
                table.Columns.Create( "C4" ).SetComputation( SqlColumnComputation.Virtual( SqlNode.Literal( 1 ) ) ).Asc() );

            var action = Lambda.Of( () => sut.CreateForeignKey( ix1, ix2 ) );

            action.Test(
                    exc => exc.TestType()
                        .Exact<SqlObjectBuilderException>(
                            e => Assertion.All( e.Dialect.TestEquals( SqlDialectMock.Instance ), e.Errors.Count.TestEquals( 1 ) ) ) )
                .Go();
        }

        [Fact]
        public void CreateForeignKey_ShouldThrowSqlObjectBuilderException_WhenOriginIndexAndReferencedIndexHaveDifferentAmountOfColumns()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Constraints;
            var ix1 = sut.CreateIndex( table.Columns.Create( "C1" ).Asc(), table.Columns.Create( "C2" ).Asc() );
            var ix2 = sut.CreateUniqueIndex( table.Columns.Create( "C3" ).Asc() );

            var action = Lambda.Of( () => sut.CreateForeignKey( ix1, ix2 ) );

            action.Test(
                    exc => exc.TestType()
                        .Exact<SqlObjectBuilderException>(
                            e => Assertion.All( e.Dialect.TestEquals( SqlDialectMock.Instance ), e.Errors.Count.TestEquals( 1 ) ) ) )
                .Go();
        }

        [Fact]
        public void
            CreateForeignKey_ShouldThrowSqlObjectBuilderException_WhenAtLeastOneOriginIndexAndReferencedIndexColumnPairHasIncompatibleRuntimeTypes()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Constraints;
            var ix1 = sut.CreateIndex(
                table.Columns.Create( "C1" ).SetType<int>().Asc(),
                table.Columns.Create( "C2" ).SetType<string>().Asc() );

            var ix2 = sut.CreateUniqueIndex(
                table.Columns.Create( "C3" ).SetType<int>().Asc(),
                table.Columns.Create( "C4" ).SetType<double>().Asc() );

            var action = Lambda.Of( () => sut.CreateForeignKey( ix1, ix2 ) );

            action.Test(
                    exc => exc.TestType()
                        .Exact<SqlObjectBuilderException>(
                            e => Assertion.All( e.Dialect.TestEquals( SqlDialectMock.Instance ), e.Errors.Count.TestEquals( 1 ) ) ) )
                .Go();
        }

        [Fact]
        public void CreateForeignKey_ShouldThrowSqlObjectCastException_WhenOriginIndexIsOfInvalidType()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            ISqlConstraintBuilderCollection sut = table.Constraints;
            var ix1 = Substitute.For<ISqlIndexBuilder>();
            var ix2 = sut.CreateUniqueIndex( table.Columns.Create( "C2" ).Asc() );

            var action = Lambda.Of( () => sut.CreateForeignKey( ix1, ix2 ) );

            action.Test(
                    exc => exc.TestType()
                        .Exact<SqlObjectCastException>(
                            e => Assertion.All(
                                e.Dialect.TestEquals( SqlDialectMock.Instance ),
                                e.Expected.TestEquals( typeof( SqlIndexBuilder ) ) ) ) )
                .Go();
        }

        [Fact]
        public void CreateForeignKey_ShouldThrowSqlObjectCastException_WhenReferencedIndexIsOfInvalidType()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            ISqlConstraintBuilderCollection sut = table.Constraints;
            var ix1 = sut.CreateIndex( table.Columns.Create( "C1" ).Asc() );
            var ix2 = Substitute.For<ISqlIndexBuilder>();

            var action = Lambda.Of( () => sut.CreateForeignKey( ix1, ix2 ) );

            action.Test(
                    exc => exc.TestType()
                        .Exact<SqlObjectCastException>(
                            e => Assertion.All(
                                e.Dialect.TestEquals( SqlDialectMock.Instance ),
                                e.Expected.TestEquals( typeof( SqlIndexBuilder ) ) ) ) )
                .Go();
        }

        [Theory]
        [InlineData( "" )]
        [InlineData( " " )]
        [InlineData( "'" )]
        [InlineData( "f\'oo" )]
        public void CreateForeignKey_ShouldThrowSqlObjectBuilderException_WhenNameIsInvalid(string name)
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            ISqlConstraintBuilderCollection sut = table.Constraints;
            var ix1 = sut.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
            var ix2 = sut.CreateIndex( table.Columns.Create( "C2" ).Asc() );

            var action = Lambda.Of( () => sut.CreateForeignKey( name, ix2, ix1 ) );

            action.Test(
                    exc => exc.TestType()
                        .Exact<SqlObjectBuilderException>(
                            e => Assertion.All( e.Dialect.TestEquals( SqlDialectMock.Instance ), e.Errors.Count.TestEquals( 1 ) ) ) )
                .Go();
        }

        [Fact]
        public void CreateCheck_ShouldCreateNewCheck()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Constraints;
            var c = table.Columns.Create( "C" );
            var condition = c.Node > SqlNode.Literal( 0 );

            var result = sut.CreateCheck( condition );

            Assertion.All(
                    result.Table.TestRefEquals( table ),
                    result.Database.TestRefEquals( table.Database ),
                    result.Type.TestEquals( SqlObjectType.Check ),
                    result.Name.TestMatch( new Regex( "CHK_T_[0-9a-fA-F]{32}" ) ),
                    result.Condition.TestRefEquals( condition ),
                    result.ReferencedColumns.TestSequence( [ c ] ),
                    (( ISqlCheckBuilder )result).ReferencedColumns.TestSequence(
                        result.ReferencedColumns.UnsafeReinterpretAs<ISqlColumnBuilder>() ),
                    (( ISqlConstraintBuilder )result).Table.TestRefEquals( result.Table ),
                    (( ISqlObjectBuilder )result).Database.TestRefEquals( result.Database ),
                    (( ISqlObjectBuilder )result).ReferencingObjects.TestSequence(
                        result.ReferencingObjects.UnsafeReinterpretAs<ISqlObjectBuilder>() ),
                    sut.Count.TestEquals( 1 ),
                    sut.TestSetEqual( [ result ] ),
                    schema.Objects.TryGet( result.Name ).TestRefEquals( result ),
                    c.ReferencingObjects.TestSequence(
                        [ SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( result ), c ) ] ) )
                .Go();
        }

        [Fact]
        public void CreateCheck_WithExplicitName_ShouldCreateNewCheck()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Constraints;
            var c = table.Columns.Create( "C" );
            var condition = c.Node > SqlNode.Literal( 0 );

            var result = sut.CreateCheck( "CHK", condition );

            Assertion.All(
                    result.Table.TestRefEquals( table ),
                    result.Database.TestRefEquals( table.Database ),
                    result.Type.TestEquals( SqlObjectType.Check ),
                    result.Name.TestMatch( new Regex( "CHK" ) ),
                    result.Condition.TestRefEquals( condition ),
                    result.ReferencedColumns.TestSequence( [ c ] ),
                    (( ISqlCheckBuilder )result).ReferencedColumns.TestSequence(
                        result.ReferencedColumns.UnsafeReinterpretAs<ISqlColumnBuilder>() ),
                    (( ISqlConstraintBuilder )result).Table.TestRefEquals( result.Table ),
                    (( ISqlObjectBuilder )result).Database.TestRefEquals( result.Database ),
                    (( ISqlObjectBuilder )result).ReferencingObjects.TestSequence(
                        result.ReferencingObjects.UnsafeReinterpretAs<ISqlObjectBuilder>() ),
                    sut.Count.TestEquals( 1 ),
                    sut.TestSetEqual( [ result ] ),
                    schema.Objects.TryGet( result.Name ).TestRefEquals( result ),
                    c.ReferencingObjects.TestSequence(
                        [ SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( result ), c ) ] ) )
                .Go();
        }

        [Fact]
        public void CreateCheck_ShouldThrowSqlObjectBuilderException_WhenTableIsRemoved()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Constraints;
            table.Remove();

            var action = Lambda.Of( () => sut.CreateCheck( SqlNode.True() ) );

            action.Test(
                    exc => exc.TestType()
                        .Exact<SqlObjectBuilderException>(
                            e => Assertion.All( e.Dialect.TestEquals( SqlDialectMock.Instance ), e.Errors.Count.TestEquals( 1 ) ) ) )
                .Go();
        }

        [Fact]
        public void CreateCheck_ShouldThrowSqlObjectBuilderException_WhenSchemaObjectNameAlreadyExists()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Constraints;

            var action = Lambda.Of( () => sut.CreateCheck( "T", SqlNode.True() ) );

            action.Test(
                    exc => exc.TestType()
                        .Exact<SqlObjectBuilderException>(
                            e => Assertion.All( e.Dialect.TestEquals( SqlDialectMock.Instance ), e.Errors.Count.TestEquals( 1 ) ) ) )
                .Go();
        }

        [Fact]
        public void CreateCheck_ShouldThrowSqlObjectBuilderException_WhenConditionIsInvalid()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Constraints;

            var action = Lambda.Of( () => sut.CreateCheck( SqlNode.WindowFunctions.RowNumber() == SqlNode.Literal( 0 ) ) );

            action.Test(
                    exc => exc.TestType()
                        .Exact<SqlObjectBuilderException>(
                            e => Assertion.All( e.Dialect.TestEquals( SqlDialectMock.Instance ), e.Errors.Count.TestEquals( 1 ) ) ) )
                .Go();
        }

        [Theory]
        [InlineData( "" )]
        [InlineData( " " )]
        [InlineData( "'" )]
        [InlineData( "f\'oo" )]
        public void CreateCheck_ShouldThrowSqlObjectBuilderException_WhenNameIsInvalid(string name)
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Constraints;

            var action = Lambda.Of( () => sut.CreateCheck( name, SqlNode.True() ) );

            action.Test(
                    exc => exc.TestType()
                        .Exact<SqlObjectBuilderException>(
                            e => Assertion.All( e.Dialect.TestEquals( SqlDialectMock.Instance ), e.Errors.Count.TestEquals( 1 ) ) ) )
                .Go();
        }

        [Theory]
        [InlineData( "T", false )]
        [InlineData( "PK_T", true )]
        [InlineData( "UIX_T_CA", true )]
        [InlineData( "CHK", true )]
        public void Contains_ShouldReturnTrue_WhenConstraintExists(string name, bool expected)
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Constraints;
            sut.SetPrimaryKey( table.Columns.Create( "C" ).Asc() );
            sut.CreateCheck( "CHK", SqlNode.True() );

            var result = sut.Contains( name );

            result.TestEquals( expected ).Go();
        }

        [Fact]
        public void Get_ShouldReturnExistingConstraint()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            ISqlConstraintBuilderCollection sut = table.Constraints;
            var expected = sut.SetPrimaryKey( table.Columns.Create( "C" ).Asc() );

            var result = sut.Get( expected.Name );

            result.TestRefEquals( expected ).Go();
        }

        [Fact]
        public void Get_ShouldThrowKeyNotFoundException_WhenConstraintDoesNotExist()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            ISqlConstraintBuilderCollection sut = table.Constraints;
            sut.SetPrimaryKey( table.Columns.Create( "C" ).Asc() );

            var action = Lambda.Of( () => sut.Get( "T" ) );

            action.Test( exc => exc.TestType().Exact<KeyNotFoundException>() ).Go();
        }

        [Fact]
        public void TryGet_ShouldReturnExistingConstraint()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            ISqlConstraintBuilderCollection sut = table.Constraints;
            var expected = sut.SetPrimaryKey( table.Columns.Create( "C" ).Asc() );

            var result = sut.TryGet( expected.Name );

            result.TestRefEquals( expected ).Go();
        }

        [Fact]
        public void TryGet_ShouldReturnNull_WhenConstraintDoesNotExist()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            ISqlConstraintBuilderCollection sut = table.Constraints;
            sut.SetPrimaryKey( table.Columns.Create( "C" ).Asc() );

            var result = sut.TryGet( "T" );

            result.TestNull().Go();
        }

        [Fact]
        public void GetIndex_ShouldReturnExistingIndex()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            ISqlConstraintBuilderCollection sut = table.Constraints;
            var index = sut.SetPrimaryKey( table.Columns.Create( "C" ).Asc() ).Index;

            var result = sut.GetIndex( index.Name );

            result.TestRefEquals( index ).Go();
        }

        [Fact]
        public void GetIndex_ShouldThrowKeyNotFoundException_WhenConstraintDoesNotExist()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            ISqlConstraintBuilderCollection sut = table.Constraints;

            var action = Lambda.Of( () => sut.GetIndex( "T" ) );

            action.Test( exc => exc.TestType().Exact<KeyNotFoundException>() ).Go();
        }

        [Fact]
        public void GetIndex_ShouldThrowSqlObjectCastException_WhenConstraintExistsButNotAsIndex()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            ISqlConstraintBuilderCollection sut = table.Constraints;
            sut.CreateCheck( "CHK", SqlNode.True() );

            var action = Lambda.Of( () => sut.GetIndex( "CHK" ) );

            action.Test(
                    exc => exc.TestType()
                        .Exact<SqlObjectCastException>(
                            e => Assertion.All(
                                e.Dialect.TestEquals( SqlDialectMock.Instance ),
                                e.Expected.TestEquals( typeof( SqlIndexBuilder ) ),
                                e.Actual.TestEquals( typeof( SqlCheckBuilderMock ) ) ) ) )
                .Go();
        }

        [Fact]
        public void TryGetIndex_ShouldReturnExistingIndex()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            ISqlConstraintBuilderCollection sut = table.Constraints;
            var index = sut.SetPrimaryKey( table.Columns.Create( "C" ).Asc() ).Index;

            var result = sut.TryGetIndex( index.Name );

            result.TestRefEquals( index ).Go();
        }

        [Fact]
        public void TryGetIndex_ShouldReturnNull_WhenConstraintDoesNotExist()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            ISqlConstraintBuilderCollection sut = table.Constraints;

            var result = sut.TryGetIndex( "T" );

            result.TestNull().Go();
        }

        [Fact]
        public void TryGetIndex_ShouldReturnNull_WhenConstraintExistsButNotAsIndex()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            ISqlConstraintBuilderCollection sut = table.Constraints;
            sut.CreateCheck( "CHK", SqlNode.True() );

            var result = sut.TryGetIndex( "CHK" );

            result.TestNull().Go();
        }

        [Fact]
        public void GetForeignKey_ShouldReturnExistingForeignKey()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            ISqlConstraintBuilderCollection sut = table.Constraints;
            var ix1 = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
            var ix2 = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );
            var foreignKey = sut.CreateForeignKey( ix2, ix1 );

            var result = sut.GetForeignKey( foreignKey.Name );

            result.TestRefEquals( foreignKey ).Go();
        }

        [Fact]
        public void GetForeignKey_ShouldThrowKeyNotFoundException_WhenConstraintDoesNotExist()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            ISqlConstraintBuilderCollection sut = table.Constraints;

            var action = Lambda.Of( () => sut.GetForeignKey( "T" ) );

            action.Test( exc => exc.TestType().Exact<KeyNotFoundException>() ).Go();
        }

        [Fact]
        public void GetForeignKey_ShouldThrowSqlObjectCastException_WhenConstraintExistsButNotAsForeignKey()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            ISqlConstraintBuilderCollection sut = table.Constraints;
            var index = sut.SetPrimaryKey( table.Columns.Create( "C" ).Asc() ).Index;

            var action = Lambda.Of( () => sut.GetForeignKey( index.Name ) );

            action.Test(
                    exc => exc.TestType()
                        .Exact<SqlObjectCastException>(
                            e => Assertion.All(
                                e.Dialect.TestEquals( SqlDialectMock.Instance ),
                                e.Expected.TestEquals( typeof( SqlForeignKeyBuilder ) ),
                                e.Actual.TestEquals( typeof( SqlIndexBuilderMock ) ) ) ) )
                .Go();
        }

        [Fact]
        public void TryGetForeignKey_ShouldReturnExistingForeignKey()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            ISqlConstraintBuilderCollection sut = table.Constraints;
            var ix1 = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
            var ix2 = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );
            var foreignKey = sut.CreateForeignKey( ix2, ix1 );

            var result = sut.TryGetForeignKey( foreignKey.Name );

            result.TestRefEquals( foreignKey ).Go();
        }

        [Fact]
        public void TryGetForeignKey_ShouldReturnNull_WhenConstraintDoesNotExist()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            ISqlConstraintBuilderCollection sut = table.Constraints;

            var result = sut.TryGetForeignKey( "T" );

            result.TestNull().Go();
        }

        [Fact]
        public void TryGetForeignKey_ShouldReturnNull_WhenConstraintExistsButNotAsForeignKey()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            ISqlConstraintBuilderCollection sut = table.Constraints;
            var index = sut.SetPrimaryKey( table.Columns.Create( "C" ).Asc() ).Index;

            var result = sut.TryGetForeignKey( index.Name );

            result.TestNull().Go();
        }

        [Fact]
        public void GetCheck_ShouldReturnExistingCheck()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            ISqlConstraintBuilderCollection sut = table.Constraints;
            var check = sut.CreateCheck( SqlNode.True() );

            var result = sut.GetCheck( check.Name );

            result.TestRefEquals( check ).Go();
        }

        [Fact]
        public void GetCheck_ShouldThrowKeyNotFoundException_WhenConstraintDoesNotExist()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            ISqlConstraintBuilderCollection sut = table.Constraints;

            var action = Lambda.Of( () => sut.GetCheck( "T" ) );

            action.Test( exc => exc.TestType().Exact<KeyNotFoundException>() ).Go();
        }

        [Fact]
        public void GetCheck_ShouldThrowSqlObjectCastException_WhenConstraintExistsButNotAsCheck()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            ISqlConstraintBuilderCollection sut = table.Constraints;
            var index = sut.SetPrimaryKey( table.Columns.Create( "C" ).Asc() ).Index;

            var action = Lambda.Of( () => sut.GetCheck( index.Name ) );

            action.Test(
                    exc => exc.TestType()
                        .Exact<SqlObjectCastException>(
                            e => Assertion.All(
                                e.Dialect.TestEquals( SqlDialectMock.Instance ),
                                e.Expected.TestEquals( typeof( SqlCheckBuilder ) ),
                                e.Actual.TestEquals( typeof( SqlIndexBuilderMock ) ) ) ) )
                .Go();
        }

        [Fact]
        public void TryGetCheck_ShouldReturnExistingCheck()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            ISqlConstraintBuilderCollection sut = table.Constraints;
            var check = sut.CreateCheck( SqlNode.True() );

            var result = sut.TryGetCheck( check.Name );

            result.TestRefEquals( check ).Go();
        }

        [Fact]
        public void TryGetCheck_ShouldReturnNull_WhenConstraintDoesNotExist()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            ISqlConstraintBuilderCollection sut = table.Constraints;

            var result = sut.TryGetCheck( "T" );

            result.TestNull().Go();
        }

        [Fact]
        public void TryGetCheck_ShouldReturnNull_WhenConstraintExistsButNotAsCheck()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            ISqlConstraintBuilderCollection sut = table.Constraints;
            var index = sut.SetPrimaryKey( table.Columns.Create( "C" ).Asc() ).Index;

            var result = sut.TryGetCheck( index.Name );

            result.TestNull().Go();
        }

        [Fact]
        public void GetPrimaryKey_ShouldReturnPrimaryKey_WhenPrimaryKeyExists()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            ISqlConstraintBuilderCollection sut = table.Constraints;
            var expected = sut.SetPrimaryKey( table.Columns.Create( "C" ).Asc() );

            var result = sut.GetPrimaryKey();

            result.TestRefEquals( expected ).Go();
        }

        [Fact]
        public void GetPrimaryKey_ShouldThrowSqlObjectBuilderException_WhenPrimaryKeyDoesNotExist()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            ISqlConstraintBuilderCollection sut = table.Constraints;

            var action = Lambda.Of( () => sut.GetPrimaryKey() );

            action.Test(
                    exc => exc.TestType()
                        .Exact<SqlObjectBuilderException>(
                            e => Assertion.All( e.Dialect.TestEquals( SqlDialectMock.Instance ), e.Errors.Count.TestEquals( 1 ) ) ) )
                .Go();
        }

        [Fact]
        public void TryGetPrimaryKey_ShouldReturnPrimaryKey_WhenPrimaryKeyExists()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            ISqlConstraintBuilderCollection sut = table.Constraints;
            var expected = sut.SetPrimaryKey( table.Columns.Create( "C" ).Asc() );

            var result = sut.TryGetPrimaryKey();

            result.TestRefEquals( expected ).Go();
        }

        [Fact]
        public void TryGetPrimaryKey_ShouldReturnNull_WhenPrimaryKeyDoesNotExist()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            ISqlConstraintBuilderCollection sut = table.Constraints;

            var result = sut.TryGetPrimaryKey();

            result.TestNull().Go();
        }

        [Fact]
        public void Remove_ShouldRemoveExistingIndex()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Constraints;
            var c1 = table.Columns.Create( "C1" );
            var c2 = table.Columns.Create( "C2" );
            var ixc1 = c1.Asc();
            var ixc2 = c2.Desc();
            var index = sut.CreateIndex( ixc1, ixc2 );

            var result = sut.Remove( index.Name );

            Assertion.All(
                    result.TestTrue(),
                    sut.TryGet( index.Name ).TestNull(),
                    index.IsRemoved.TestTrue(),
                    schema.Objects.TryGet( index.Name ).TestNull(),
                    c1.ReferencingObjects.TestEmpty(),
                    c2.ReferencingObjects.TestEmpty() )
                .Go();
        }

        [Fact]
        public void Remove_ShouldReturnFalse_WhenIndexHasOriginatingForeignKey()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Constraints;
            var pk = sut.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
            var index = sut.CreateIndex( table.Columns.Create( "C2" ).Asc() );
            table.Constraints.CreateForeignKey( index, pk.Index );

            var result = sut.Remove( index.Name );

            Assertion.All(
                    result.TestFalse(),
                    index.IsRemoved.TestFalse(),
                    sut.Count.TestEquals( 4 ) )
                .Go();
        }

        [Fact]
        public void Remove_ShouldReturnFalse_WhenIndexHasReferencingForeignKey()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Constraints;
            var pk = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
            var index = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );
            table.Constraints.CreateForeignKey( index, pk.Index );

            var result = sut.Remove( pk.Index.Name );

            Assertion.All(
                    result.TestFalse(),
                    pk.Index.IsRemoved.TestFalse(),
                    sut.Count.TestEquals( 4 ) )
                .Go();
        }

        [Fact]
        public void Remove_ShouldRemoveExistingPrimaryKey()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Constraints;
            var column = table.Columns.Create( "C" );
            var pk = table.Constraints.SetPrimaryKey( column.Asc() );

            var result = sut.Remove( pk.Name );

            Assertion.All(
                    result.TestTrue(),
                    sut.TryGet( pk.Name ).TestNull(),
                    sut.TryGet( pk.Index.Name ).TestNull(),
                    sut.TryGetPrimaryKey().TestNull(),
                    pk.IsRemoved.TestTrue(),
                    pk.Index.IsRemoved.TestTrue(),
                    schema.Objects.TryGet( pk.Name ).TestNull(),
                    schema.Objects.TryGet( pk.Index.Name ).TestNull(),
                    column.ReferencingObjects.TestEmpty() )
                .Go();
        }

        [Fact]
        public void Remove_ShouldReturnFalse_WhenPrimaryKeyUnderlyingIndexHasOriginatingForeignKey()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Constraints;
            var pk = sut.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
            var index = sut.CreateUniqueIndex( table.Columns.Create( "C2" ).Asc() );
            table.Constraints.CreateForeignKey( pk.Index, index );

            var result = sut.Remove( pk.Name );

            Assertion.All(
                    result.TestFalse(),
                    index.IsRemoved.TestFalse(),
                    sut.Count.TestEquals( 4 ),
                    sut.TryGetPrimaryKey().TestRefEquals( pk ) )
                .Go();
        }

        [Fact]
        public void Remove_ShouldReturnFalse_WhenPrimaryKeyUnderlyingIndexHasReferencingForeignKey()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Constraints;
            var pk = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
            var index = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );
            table.Constraints.CreateForeignKey( index, pk.Index );

            var result = sut.Remove( pk.Name );

            Assertion.All(
                    result.TestFalse(),
                    pk.Index.IsRemoved.TestFalse(),
                    sut.Count.TestEquals( 4 ),
                    sut.TryGetPrimaryKey().TestRefEquals( pk ) )
                .Go();
        }

        [Fact]
        public void Remove_ShouldRemoveExistingForeignKey()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Constraints;
            var ix1 = sut.CreateIndex( table.Columns.Create( "C1" ).Asc() );
            var ix2 = sut.CreateUniqueIndex( table.Columns.Create( "C2" ).Asc() );
            var fk = sut.CreateForeignKey( ix1, ix2 );

            var result = sut.Remove( fk.Name );

            Assertion.All(
                    result.TestTrue(),
                    sut.Count.TestEquals( 2 ),
                    sut.TryGet( fk.Name ).TestNull(),
                    fk.IsRemoved.TestTrue(),
                    schema.Objects.TryGet( fk.Name ).TestNull(),
                    ix1.ReferencingObjects.TestEmpty(),
                    ix2.ReferencingObjects.TestEmpty() )
                .Go();
        }

        [Fact]
        public void Remove_ShouldRemoveExistingCheck()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Constraints;
            var c = table.Columns.Create( "C" );
            var check = sut.CreateCheck( c.Node > SqlNode.Literal( 0 ) );

            var result = sut.Remove( check.Name );

            Assertion.All(
                    result.TestTrue(),
                    sut.Count.TestEquals( 0 ),
                    sut.TryGet( check.Name ).TestNull(),
                    check.IsRemoved.TestTrue(),
                    schema.Objects.TryGet( check.Name ).TestNull(),
                    c.ReferencingObjects.TestEmpty() )
                .Go();
        }

        [Fact]
        public void Remove_ShouldReturnFalse_WhenConstraintDoesNotExist()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Constraints;

            var result = sut.Remove( "PK" );

            result.TestFalse().Go();
        }

        [Fact]
        public void ISqlConstraintBuilderCollection_CreateIndex_ShouldBeEquivalentToCreateIndex()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Constraints;
            var c1 = table.Columns.Create( "C1" );
            var c2 = table.Columns.Create( "C2" );
            var ixc1 = c1.Asc();
            var ixc2 = c2.Desc();

            var result = (( ISqlConstraintBuilderCollection )sut).CreateIndex( ixc1, ixc2 );

            Assertion.All(
                    result.Table.TestRefEquals( table ),
                    result.Database.TestRefEquals( table.Database ),
                    result.Type.TestEquals( SqlObjectType.Index ),
                    result.Name.TestEquals( "IX_T_C1A_C2D" ),
                    result.Columns.Expressions.TestSequence( [ ixc1, ixc2 ] ),
                    result.ReferencedColumns.TestSequence( [ c1, c2 ] ),
                    result.ReferencedFilterColumns.TestEmpty(),
                    result.PrimaryKey.TestNull(),
                    result.IsUnique.TestFalse(),
                    result.Filter.TestNull(),
                    result.ReferencingObjects.TestEmpty(),
                    sut.Count.TestEquals( 1 ),
                    sut.TestSequence( [ result ] ),
                    schema.Objects.TryGet( result.Name ).TestRefEquals( result ),
                    c1.ReferencingObjects.TestSequence(
                        [ SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( ( SqlObjectBuilder )result ), c1 ) ] ),
                    c2.ReferencingObjects.TestSequence(
                        [ SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( ( SqlObjectBuilder )result ), c2 ) ] ) )
                .Go();
        }

        [Fact]
        public void ISqlConstraintBuilderCollection_CreateIndex_WithExplicitName_ShouldBeEquivalentToCreateIndex()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Constraints;
            var c1 = table.Columns.Create( "C1" );
            var c2 = table.Columns.Create( "C2" );
            var ixc1 = c1.Asc();
            var ixc2 = c2.Desc();

            var result = (( ISqlConstraintBuilderCollection )sut).CreateIndex( "IX_T", ixc1, ixc2 );

            Assertion.All(
                    result.Table.TestRefEquals( table ),
                    result.Database.TestRefEquals( table.Database ),
                    result.Type.TestEquals( SqlObjectType.Index ),
                    result.Name.TestEquals( "IX_T" ),
                    result.Columns.Expressions.TestSequence( [ ixc1, ixc2 ] ),
                    result.ReferencedColumns.TestSequence( [ c1, c2 ] ),
                    result.ReferencedFilterColumns.TestEmpty(),
                    result.PrimaryKey.TestNull(),
                    result.IsUnique.TestFalse(),
                    result.Filter.TestNull(),
                    result.ReferencingObjects.TestEmpty(),
                    sut.Count.TestEquals( 1 ),
                    sut.TestSequence( [ result ] ),
                    schema.Objects.TryGet( result.Name ).TestRefEquals( result ),
                    c1.ReferencingObjects.TestSequence(
                        [ SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( ( SqlObjectBuilder )result ), c1 ) ] ),
                    c2.ReferencingObjects.TestSequence(
                        [ SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( ( SqlObjectBuilder )result ), c2 ) ] ) )
                .Go();
        }

        [Fact]
        public void ISqlConstraintBuilderCollection_CreateUniqueIndex_WithExplicitName_ShouldBeEquivalentToCreateUniqueIndex()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Constraints;
            var c1 = table.Columns.Create( "C1" );
            var c2 = table.Columns.Create( "C2" );
            var ixc1 = c1.Asc();
            var ixc2 = c2.Desc();

            var result = (( ISqlConstraintBuilderCollection )sut).CreateUniqueIndex( "IX_T", ixc1, ixc2 );

            Assertion.All(
                    result.Table.TestRefEquals( table ),
                    result.Database.TestRefEquals( table.Database ),
                    result.Type.TestEquals( SqlObjectType.Index ),
                    result.Name.TestEquals( "IX_T" ),
                    result.Columns.Expressions.TestSequence( [ ixc1, ixc2 ] ),
                    result.ReferencedColumns.TestSequence( [ c1, c2 ] ),
                    result.ReferencedFilterColumns.TestEmpty(),
                    result.PrimaryKey.TestNull(),
                    result.IsUnique.TestTrue(),
                    result.Filter.TestNull(),
                    result.ReferencingObjects.TestEmpty(),
                    sut.Count.TestEquals( 1 ),
                    sut.TestSequence( [ result ] ),
                    schema.Objects.TryGet( result.Name ).TestRefEquals( result ),
                    c1.ReferencingObjects.TestSequence(
                        [ SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( ( SqlObjectBuilder )result ), c1 ) ] ),
                    c2.ReferencingObjects.TestSequence(
                        [ SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( ( SqlObjectBuilder )result ), c2 ) ] ) )
                .Go();
        }

        [Fact]
        public void ISqlConstraintBuilderCollection_SetPrimaryKey_ShouldBeEquivalentToSetPrimaryKey()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Constraints;
            var column = table.Columns.Create( "C" );
            var ixColumn = column.Asc();

            var result = (( ISqlConstraintBuilderCollection )sut).SetPrimaryKey( ixColumn );

            Assertion.All(
                    result.Table.TestRefEquals( table ),
                    result.Database.TestRefEquals( table.Database ),
                    result.Type.TestEquals( SqlObjectType.PrimaryKey ),
                    result.Name.TestEquals( "PK_T" ),
                    result.Index.Table.TestRefEquals( table ),
                    result.Index.Database.TestRefEquals( schema.Database ),
                    result.Index.Type.TestEquals( SqlObjectType.Index ),
                    result.Index.Name.TestEquals( "UIX_T_CA" ),
                    result.Index.Columns.Expressions.TestSequence( [ ixColumn ] ),
                    result.Index.ReferencedColumns.TestSequence( [ column ] ),
                    result.Index.ReferencedFilterColumns.TestEmpty(),
                    result.Index.PrimaryKey.TestRefEquals( result ),
                    result.Index.IsUnique.TestTrue(),
                    result.Index.Filter.TestNull(),
                    result.ReferencingObjects.TestEmpty(),
                    sut.Count.TestEquals( 2 ),
                    sut.TestSetEqual( [ result, result.Index ] ),
                    sut.TryGetPrimaryKey().TestRefEquals( result ),
                    schema.Objects.TryGet( result.Name ).TestRefEquals( result ),
                    schema.Objects.TryGet( result.Index.Name ).TestRefEquals( result.Index ),
                    column.ReferencingObjects.TestSequence(
                    [
                        SqlObjectBuilderReference.Create(
                            SqlObjectBuilderReferenceSource.Create( ( SqlObjectBuilder )result.Index ),
                            column )
                    ] ) )
                .Go();
        }

        [Fact]
        public void ISqlConstraintBuilderCollection_SetPrimaryKey_WithExplicitName_ShouldBeEquivalentToSetPrimaryKey()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Constraints;
            var column = table.Columns.Create( "C" );
            var ixColumn = column.Asc();

            var result = (( ISqlConstraintBuilderCollection )sut).SetPrimaryKey( "PK", ixColumn );

            Assertion.All(
                    result.Table.TestRefEquals( table ),
                    result.Database.TestRefEquals( table.Database ),
                    result.Type.TestEquals( SqlObjectType.PrimaryKey ),
                    result.Name.TestEquals( "PK" ),
                    result.Index.Table.TestRefEquals( table ),
                    result.Index.Database.TestRefEquals( schema.Database ),
                    result.Index.Type.TestEquals( SqlObjectType.Index ),
                    result.Index.Name.TestEquals( "UIX_T_CA" ),
                    result.Index.Columns.Expressions.TestSequence( [ ixColumn ] ),
                    result.Index.ReferencedColumns.TestSequence( [ column ] ),
                    result.Index.ReferencedFilterColumns.TestEmpty(),
                    result.Index.PrimaryKey.TestRefEquals( result ),
                    result.Index.IsUnique.TestTrue(),
                    result.Index.Filter.TestNull(),
                    result.ReferencingObjects.TestEmpty(),
                    sut.Count.TestEquals( 2 ),
                    sut.TestSetEqual( [ result, result.Index ] ),
                    sut.TryGetPrimaryKey().TestRefEquals( result ),
                    schema.Objects.TryGet( result.Name ).TestRefEquals( result ),
                    schema.Objects.TryGet( result.Index.Name ).TestRefEquals( result.Index ),
                    column.ReferencingObjects.TestSequence(
                    [
                        SqlObjectBuilderReference.Create(
                            SqlObjectBuilderReferenceSource.Create( ( SqlObjectBuilder )result.Index ),
                            column )
                    ] ) )
                .Go();
        }

        [Fact]
        public void ISqlConstraintBuilderCollection_CreateForeignKey_ShouldBeEquivalentToCreateForeignKey()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Constraints;
            var ix1 = sut.CreateIndex( "IX_T_C1", table.Columns.Create( "C1" ).Asc() );
            var ix2 = sut.CreateUniqueIndex( "UIX_T_C2", table.Columns.Create( "C2" ).Asc() );

            var result = (( ISqlConstraintBuilderCollection )sut).CreateForeignKey( ix1, ix2 );

            Assertion.All(
                    result.Table.TestRefEquals( table ),
                    result.Database.TestRefEquals( table.Database ),
                    result.Type.TestEquals( SqlObjectType.ForeignKey ),
                    result.Name.TestEquals( "FK_T_C1_REF_T" ),
                    result.OriginIndex.TestRefEquals( ix1 ),
                    result.ReferencedIndex.TestRefEquals( ix2 ),
                    result.OnUpdateBehavior.TestEquals( ReferenceBehavior.Restrict ),
                    result.OnDeleteBehavior.TestEquals( ReferenceBehavior.Restrict ),
                    result.ReferencingObjects.TestEmpty(),
                    sut.Count.TestEquals( 3 ),
                    sut.TestSetEqual( [ ix1, ix2, result ] ),
                    schema.Objects.TryGet( result.Name ).TestRefEquals( result ),
                    ix1.ReferencingObjects.TestSequence(
                        [ SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( ( SqlObjectBuilder )result ), ix1 ) ] ),
                    ix2.ReferencingObjects.TestSequence(
                        [ SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( ( SqlObjectBuilder )result ), ix2 ) ] ),
                    table.ReferencingObjects.TestEmpty(),
                    schema.ReferencingObjects.TestEmpty() )
                .Go();
        }

        [Fact]
        public void ISqlConstraintBuilderCollection_CreateForeignKey_WithExplicitName_ShouldBeEquivalentToCreateForeignKey()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Constraints;
            var ix1 = sut.CreateIndex( "IX_T_C1", table.Columns.Create( "C1" ).Asc() );
            var ix2 = sut.CreateUniqueIndex( "UIX_T_C2", table.Columns.Create( "C2" ).Asc() );

            var result = (( ISqlConstraintBuilderCollection )sut).CreateForeignKey( "FK_T", ix1, ix2 );

            Assertion.All(
                    result.Table.TestRefEquals( table ),
                    result.Database.TestRefEquals( table.Database ),
                    result.Type.TestEquals( SqlObjectType.ForeignKey ),
                    result.Name.TestEquals( "FK_T" ),
                    result.OriginIndex.TestRefEquals( ix1 ),
                    result.ReferencedIndex.TestRefEquals( ix2 ),
                    result.OnUpdateBehavior.TestEquals( ReferenceBehavior.Restrict ),
                    result.OnDeleteBehavior.TestEquals( ReferenceBehavior.Restrict ),
                    result.ReferencingObjects.TestEmpty(),
                    sut.Count.TestEquals( 3 ),
                    sut.TestSetEqual( [ ix1, ix2, result ] ),
                    schema.Objects.TryGet( result.Name ).TestRefEquals( result ),
                    ix1.ReferencingObjects.TestSequence(
                        [ SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( ( SqlObjectBuilder )result ), ix1 ) ] ),
                    ix2.ReferencingObjects.TestSequence(
                        [ SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( ( SqlObjectBuilder )result ), ix2 ) ] ),
                    table.ReferencingObjects.TestEmpty(),
                    schema.ReferencingObjects.TestEmpty() )
                .Go();
        }

        [Fact]
        public void ISqlConstraintBuilderCollection_CreateCheck_ShouldBeEquivalentToCreateCheck()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Constraints;
            var c = table.Columns.Create( "C" );
            var condition = c.Node > SqlNode.Literal( 0 );

            var result = (( ISqlConstraintBuilderCollection )sut).CreateCheck( condition );

            Assertion.All(
                    result.Table.TestRefEquals( table ),
                    result.Database.TestRefEquals( table.Database ),
                    result.Type.TestEquals( SqlObjectType.Check ),
                    result.Name.TestMatch( new Regex( "CHK_T_[0-9a-fA-F]{32}" ) ),
                    result.Condition.TestRefEquals( condition ),
                    result.ReferencedColumns.TestSequence( [ c ] ),
                    sut.Count.TestEquals( 1 ),
                    sut.TestSetEqual( [ result ] ),
                    schema.Objects.TryGet( result.Name ).TestRefEquals( result ),
                    c.ReferencingObjects.TestSequence(
                        [ SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( ( SqlObjectBuilder )result ), c ) ] ) )
                .Go();
        }

        [Fact]
        public void ISqlConstraintBuilderCollection_CreateCheck_WithExplicitName_ShouldBeEquivalentToCreateCheck()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Constraints;
            var c = table.Columns.Create( "C" );
            var condition = c.Node > SqlNode.Literal( 0 );

            var result = (( ISqlConstraintBuilderCollection )sut).CreateCheck( "CHK", condition );

            Assertion.All(
                    result.Table.TestRefEquals( table ),
                    result.Database.TestRefEquals( table.Database ),
                    result.Type.TestEquals( SqlObjectType.Check ),
                    result.Name.TestEquals( "CHK" ),
                    result.Condition.TestRefEquals( condition ),
                    result.ReferencedColumns.TestSequence( [ c ] ),
                    sut.Count.TestEquals( 1 ),
                    sut.TestSetEqual( [ result ] ),
                    schema.Objects.TryGet( result.Name ).TestRefEquals( result ),
                    c.ReferencingObjects.TestSequence(
                        [ SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( ( SqlObjectBuilder )result ), c ) ] ) )
                .Go();
        }
    }
}
