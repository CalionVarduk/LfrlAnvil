using System.Collections.Generic;
using LfrlAnvil.Functional;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Extensions;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.TestExtensions.Sql.Mocks;

namespace LfrlAnvil.Sql.Tests.ObjectsTests.BuildersTests;

public partial class SqlTableBuilderTests
{
    public class Columns : TestsBase
    {
        [Fact]
        public void Create_ShouldCreateNewColumn()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Columns;

            var result = sut.Create( "C" );

            Assertion.All(
                    result.Table.TestRefEquals( table ),
                    result.Database.TestRefEquals( table.Database ),
                    result.Type.TestEquals( SqlObjectType.Column ),
                    result.Name.TestEquals( "C" ),
                    result.IsNullable.TestFalse(),
                    result.TypeDefinition.TestRefEquals( sut.DefaultTypeDefinition ),
                    result.DefaultValue.TestNull(),
                    result.Node.Name.TestEquals( "C" ),
                    result.Node.Value.TestRefEquals( result ),
                    result.Node.RecordSet.TestRefEquals( table.Node ),
                    result.ReferencingObjects.TestEmpty(),
                    result.Computation.TestNull(),
                    result.ReferencedComputationColumns.TestEmpty(),
                    (( ISqlColumnBuilder )result).Table.TestRefEquals( result.Table ),
                    (( ISqlColumnBuilder )result).TypeDefinition.TestRefEquals( result.TypeDefinition ),
                    (( ISqlColumnBuilder )result).ReferencedComputationColumns.TestSequence( result.ReferencedComputationColumns ),
                    (( ISqlObjectBuilder )result).Database.TestRefEquals( result.Database ),
                    (( ISqlObjectBuilder )result).ReferencingObjects.TestSequence(
                        result.ReferencingObjects.UnsafeReinterpretAs<ISqlObjectBuilder>() ),
                    sut.Count.TestEquals( 1 ),
                    sut.TestSequence( [ result ] ) )
                .Go();
        }

        [Fact]
        public void Create_ShouldThrowSqlObjectBuilderException_WhenColumnNameAlreadyExists()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Columns;
            sut.Create( "C" );

            var action = Lambda.Of( () => sut.Create( "C" ) );

            action.Test(
                    exc => exc.TestType()
                        .Exact<SqlObjectBuilderException>(
                            e => Assertion.All( e.Dialect.TestEquals( SqlDialectMock.Instance ), e.Errors.Count.TestEquals( 1 ) ) ) )
                .Go();
        }

        [Fact]
        public void Create_ShouldThrowSqlObjectBuilderException_WhenTableIsRemoved()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Columns;
            table.Remove();

            var action = Lambda.Of( () => sut.Create( "C" ) );

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
        public void Create_ShouldThrowSqlObjectBuilderException_WhenNameIsInvalid(string name)
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Columns;

            var action = Lambda.Of( () => sut.Create( name ) );

            action.Test(
                    exc => exc.TestType()
                        .Exact<SqlObjectBuilderException>(
                            e => Assertion.All( e.Dialect.TestEquals( SqlDialectMock.Instance ), e.Errors.Count.TestEquals( 1 ) ) ) )
                .Go();
        }

        [Fact]
        public void GetOrCreate_ShouldCreateNewColumn_WhenColumnDoesNotExist()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Columns;

            var result = sut.GetOrCreate( "C" );

            Assertion.All(
                    result.Table.TestRefEquals( table ),
                    result.Database.TestRefEquals( table.Database ),
                    result.Type.TestEquals( SqlObjectType.Column ),
                    result.Name.TestEquals( "C" ),
                    result.IsNullable.TestFalse(),
                    result.TypeDefinition.TestRefEquals( sut.DefaultTypeDefinition ),
                    result.DefaultValue.TestNull(),
                    result.Node.Name.TestEquals( "C" ),
                    result.Node.Value.TestRefEquals( result ),
                    result.Node.RecordSet.TestRefEquals( table.Node ),
                    result.ReferencingObjects.TestEmpty(),
                    result.Computation.TestNull(),
                    result.ReferencedComputationColumns.TestEmpty(),
                    (( ISqlColumnBuilder )result).Table.TestRefEquals( result.Table ),
                    (( ISqlColumnBuilder )result).TypeDefinition.TestRefEquals( result.TypeDefinition ),
                    (( ISqlColumnBuilder )result).ReferencedComputationColumns.TestSequence( result.ReferencedComputationColumns ),
                    (( ISqlObjectBuilder )result).Database.TestRefEquals( result.Database ),
                    (( ISqlObjectBuilder )result).ReferencingObjects.TestSequence(
                        result.ReferencingObjects.UnsafeReinterpretAs<ISqlObjectBuilder>() ),
                    sut.Count.TestEquals( 1 ),
                    sut.TestSequence( [ result ] ) )
                .Go();
        }

        [Fact]
        public void GetOrCreate_ShouldReturnExistingColumn_WhenColumnNameAlreadyExists()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Columns;
            var expected = sut.Create( "C" );

            var result = sut.GetOrCreate( "C" );

            Assertion.All(
                    result.TestRefEquals( expected ),
                    sut.Count.TestEquals( 1 ) )
                .Go();
        }

        [Fact]
        public void GetOrCreate_ShouldThrowSqlObjectBuilderException_WhenTableIsRemoved()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Columns;
            table.Remove();

            var action = Lambda.Of( () => sut.GetOrCreate( "C" ) );

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
        public void GetOrCreate_ShouldThrowSqlObjectBuilderException_WhenNameIsInvalid(string name)
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Columns;

            var action = Lambda.Of( () => sut.GetOrCreate( name ) );

            action.Test(
                    exc => exc.TestType()
                        .Exact<SqlObjectBuilderException>(
                            e => Assertion.All( e.Dialect.TestEquals( SqlDialectMock.Instance ), e.Errors.Count.TestEquals( 1 ) ) ) )
                .Go();
        }

        [Theory]
        [InlineData( "C", true )]
        [InlineData( "D", false )]
        public void Contains_ShouldReturnTrue_WhenColumnExists(string name, bool expected)
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Columns;
            sut.Create( "C" );

            var result = sut.Contains( name );

            result.TestEquals( expected ).Go();
        }

        [Fact]
        public void Get_ShouldReturnExistingColumn()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Columns;
            var expected = sut.Create( "C" );

            var result = (( ISqlColumnBuilderCollection )sut).Get( "C" );

            result.TestRefEquals( expected ).Go();
        }

        [Fact]
        public void Get_ShouldThrowKeyNotFoundException_WhenColumnDoesNotExist()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Columns;
            sut.Create( "C" );

            var action = Lambda.Of( () => (( ISqlColumnBuilderCollection )sut).Get( "D" ) );

            action.Test( exc => exc.TestType().Exact<KeyNotFoundException>() ).Go();
        }

        [Fact]
        public void TryGet_ShouldReturnExistingColumn()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Columns;
            var expected = sut.Create( "C" );

            var result = (( ISqlColumnBuilderCollection )sut).TryGet( "C" );

            result.TestRefEquals( expected ).Go();
        }

        [Fact]
        public void TryGet_ShouldReturnNull_WhenColumnDoesNotExist()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Columns;
            sut.Create( "C" );

            var result = (( ISqlColumnBuilderCollection )sut).TryGet( "D" );

            result.TestNull().Go();
        }

        [Fact]
        public void Remove_ShouldRemoveExistingColumn()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Columns;
            var column = sut.Create( "C" );

            var result = sut.Remove( "C" );

            Assertion.All(
                    result.TestTrue(),
                    sut.TryGet( column.Name ).TestNull(),
                    column.IsRemoved.TestTrue() )
                .Go();
        }

        [Fact]
        public void Remove_ShouldReturnFalse_WhenColumnDoesNotExist()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Columns;

            var result = sut.Remove( "C" );

            result.TestFalse().Go();
        }

        [Fact]
        public void Remove_ShouldReturnFalse_WhenColumnExistsButIsReferencedByIndex()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Columns;
            table.Constraints.SetPrimaryKey( sut.Create( "C1" ).Asc() );
            var column = sut.Create( "C2" );
            table.Constraints.CreateIndex( column.Asc() );

            var result = sut.Remove( "C2" );

            Assertion.All(
                    result.TestFalse(),
                    column.IsRemoved.TestFalse(),
                    sut.Count.TestEquals( 2 ) )
                .Go();
        }

        [Fact]
        public void Remove_ShouldReturnFalse_WhenColumnExistsButIsReferencedByIndexFilter()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Columns;
            table.Constraints.SetPrimaryKey( sut.Create( "C1" ).Asc() );
            var column = sut.Create( "C2" );
            table.Constraints.CreateIndex( sut.Create( "C3" ).Asc() ).SetFilter( t => t["C2"] != null );

            var result = sut.Remove( "C2" );

            Assertion.All(
                    result.TestFalse(),
                    column.IsRemoved.TestFalse(),
                    sut.Count.TestEquals( 3 ) )
                .Go();
        }

        [Fact]
        public void Remove_ShouldReturnFalse_WhenColumnExistsButIsReferencedByView()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Columns;
            table.Constraints.SetPrimaryKey( sut.Create( "C1" ).Asc() );
            var column = sut.Create( "C2" );
            schema.Objects.CreateView( "V", table.Node.ToDataSource().Select( s => new[] { s.From["C2"].AsSelf() } ) );

            var result = sut.Remove( "C2" );

            Assertion.All(
                    result.TestFalse(),
                    column.IsRemoved.TestFalse(),
                    sut.Count.TestEquals( 2 ) )
                .Go();
        }

        [Fact]
        public void Remove_ShouldReturnFalse_WhenColumnExistsButIsReferencedByCheck()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Columns;
            table.Constraints.SetPrimaryKey( sut.Create( "C1" ).Asc() );
            var column = sut.Create( "C2" );
            table.Constraints.CreateCheck( table.Node["C2"] != SqlNode.Literal( 0 ) );

            var result = sut.Remove( "C2" );

            Assertion.All(
                    result.TestFalse(),
                    column.IsRemoved.TestFalse(),
                    sut.Count.TestEquals( 2 ) )
                .Go();
        }

        [Fact]
        public void SetDefaultTypeDefinition_ShouldUpdateDefaultTypeDefinition()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Columns;
            var definition = schema.Database.TypeDefinitions.GetByType<string>();

            var result = (( ISqlColumnBuilderCollection )sut).SetDefaultTypeDefinition( definition );

            Assertion.All(
                    result.TestRefEquals( sut ),
                    result.DefaultTypeDefinition.TestRefEquals( definition ) )
                .Go();
        }

        [Fact]
        public void SetDefaultTypeDefinition_ShouldThrowSqlObjectBuilderException_WhenDefinitionDoesNotBelongToTheDatabase()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Columns;
            var definition = SqlDatabaseBuilderMock.Create().TypeDefinitions.GetByType<string>();

            var action = Lambda.Of( () => (( ISqlColumnBuilderCollection )sut).SetDefaultTypeDefinition( definition ) );

            action.Test(
                    exc => exc.TestType()
                        .Exact<SqlObjectBuilderException>(
                            e => Assertion.All( e.Dialect.TestEquals( SqlDialectMock.Instance ), e.Errors.Count.TestEquals( 1 ) ) ) )
                .Go();
        }

        [Fact]
        public void SetDefaultTypeDefinition_ShouldThrowSqlObjectCastException_WhenDefinitionIsOfInvalidType()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Columns;
            var definition = Substitute.For<ISqlColumnTypeDefinition>();

            var action = Lambda.Of( () => (( ISqlColumnBuilderCollection )sut).SetDefaultTypeDefinition( definition ) );

            action.Test(
                    exc => exc.TestType()
                        .Exact<SqlObjectCastException>(
                            e => Assertion.All(
                                e.Dialect.TestEquals( SqlDialectMock.Instance ),
                                e.Expected.TestEquals( typeof( SqlColumnTypeDefinition ) ) ) ) )
                .Go();
        }

        [Fact]
        public void ISqlColumnBuilderCollection_Create_ShouldBeEquivalentToCreate()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Columns;

            var result = (( ISqlColumnBuilderCollection )sut).Create( "C" );

            Assertion.All(
                    result.Table.TestRefEquals( table ),
                    result.Database.TestRefEquals( table.Database ),
                    result.Type.TestEquals( SqlObjectType.Column ),
                    result.Name.TestEquals( "C" ),
                    result.IsNullable.TestFalse(),
                    result.TypeDefinition.TestRefEquals( sut.DefaultTypeDefinition ),
                    result.DefaultValue.TestNull(),
                    result.Node.Name.TestEquals( "C" ),
                    result.Node.Value.TestRefEquals( result ),
                    result.Node.RecordSet.TestRefEquals( table.Node ),
                    result.ReferencingObjects.TestEmpty(),
                    result.Computation.TestNull(),
                    result.ReferencedComputationColumns.TestEmpty(),
                    sut.Count.TestEquals( 1 ),
                    sut.TestSequence( [ result ] ) )
                .Go();
        }

        [Fact]
        public void ISqlColumnBuilderCollection_GetOrCreate_ShouldBeEquivalentToGetOrCreate()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Columns;
            var expected = sut.Create( "C" );

            var result = (( ISqlColumnBuilderCollection )sut).GetOrCreate( "C" );

            Assertion.All(
                    result.TestRefEquals( expected ),
                    sut.Count.TestEquals( 1 ) )
                .Go();
        }
    }
}
