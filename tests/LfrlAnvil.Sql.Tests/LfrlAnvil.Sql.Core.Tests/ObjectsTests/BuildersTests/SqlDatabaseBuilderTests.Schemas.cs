using System.Collections.Generic;
using LfrlAnvil.Functional;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Extensions;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.TestExtensions.Sql.Mocks;

namespace LfrlAnvil.Sql.Tests.ObjectsTests.BuildersTests;

public partial class SqlDatabaseBuilderTests
{
    public class Schemas : TestsBase
    {
        [Fact]
        public void Create_ShouldCreateNewSchema()
        {
            var sut = SqlDatabaseBuilderMock.Create().Schemas;

            var result = sut.Create( "foo" );

            Assertion.All(
                    result.Database.TestRefEquals( sut.Database ),
                    result.Type.TestEquals( SqlObjectType.Schema ),
                    result.Name.TestEquals( "foo" ),
                    result.Objects.TestEmpty(),
                    result.Objects.Schema.TestRefEquals( result ),
                    result.ReferencingObjects.TestEmpty(),
                    (( ISqlSchemaBuilder )result).Objects.TestRefEquals( result.Objects ),
                    (( ISqlObjectBuilderCollection )result.Objects).Schema.TestRefEquals( result.Objects.Schema ),
                    (( ISqlObjectBuilder )result).Database.TestRefEquals( result.Database ),
                    (( ISqlObjectBuilder )result).ReferencingObjects.TestSequence(
                        result.ReferencingObjects.UnsafeReinterpretAs<ISqlObjectBuilder>() ),
                    sut.Count.TestEquals( 2 ),
                    sut.TestSetEqual( [ sut.Database.Schemas.Default, result ] ),
                    sut.TryGet( result.Name ).TestRefEquals( result ) )
                .Go();
        }

        [Fact]
        public void Create_ShouldThrowSqlObjectBuilderException_WhenSchemaNameAlreadyExists()
        {
            var sut = SqlDatabaseBuilderMock.Create().Schemas;

            var action = Lambda.Of( () => sut.Create( sut.Database.Schemas.Default.Name ) );

            action.Test( exc => exc.TestType()
                    .Exact<SqlObjectBuilderException>( e => Assertion.All(
                        e.Dialect.TestEquals( SqlDialectMock.Instance ),
                        e.Errors.Count.TestEquals( 1 ) ) ) )
                .Go();
        }

        [Theory]
        [InlineData( "" )]
        [InlineData( " " )]
        [InlineData( "'" )]
        [InlineData( "f\'oo" )]
        public void Create_ShouldThrowSqlObjectBuilderException_WhenNameIsInvalid(string name)
        {
            var sut = SqlDatabaseBuilderMock.Create().Schemas;

            var action = Lambda.Of( () => sut.Create( name ) );

            action.Test( exc => exc.TestType()
                    .Exact<SqlObjectBuilderException>( e => Assertion.All(
                        e.Dialect.TestEquals( SqlDialectMock.Instance ),
                        e.Errors.Count.TestEquals( 1 ) ) ) )
                .Go();
        }

        [Fact]
        public void GetOrCreate_ShouldCreateNewSchema_WhenSchemaDoesNotExist()
        {
            var sut = SqlDatabaseBuilderMock.Create().Schemas;

            var result = sut.GetOrCreate( "foo" );

            Assertion.All(
                    result.Database.TestRefEquals( sut.Database ),
                    result.Type.TestEquals( SqlObjectType.Schema ),
                    result.Name.TestEquals( "foo" ),
                    result.Objects.TestEmpty(),
                    result.Objects.Schema.TestRefEquals( result ),
                    result.ReferencingObjects.TestEmpty(),
                    (( ISqlSchemaBuilder )result).Objects.TestRefEquals( result.Objects ),
                    (( ISqlObjectBuilderCollection )result.Objects).Schema.TestRefEquals( result.Objects.Schema ),
                    (( ISqlObjectBuilder )result).Database.TestRefEquals( result.Database ),
                    (( ISqlObjectBuilder )result).ReferencingObjects.TestSequence(
                        result.ReferencingObjects.UnsafeReinterpretAs<ISqlObjectBuilder>() ),
                    sut.Count.TestEquals( 2 ),
                    sut.TestSetEqual( [ sut.Database.Schemas.Default, result ] ),
                    sut.TryGet( result.Name ).TestRefEquals( result ) )
                .Go();
        }

        [Fact]
        public void GetOrCreate_ShouldReturnExistingSchema_WhenSchemaNameAlreadyExists()
        {
            var sut = SqlDatabaseBuilderMock.Create().Schemas;
            var expected = sut.Default;

            var result = sut.GetOrCreate( expected.Name );

            Assertion.All(
                    result.TestRefEquals( expected ),
                    sut.Count.TestEquals( 1 ) )
                .Go();
        }

        [Theory]
        [InlineData( "" )]
        [InlineData( " " )]
        [InlineData( "'" )]
        [InlineData( "f\'oo" )]
        public void GetOrCreate_ShouldThrowSqlObjectBuilderException_WhenNameIsInvalid(string name)
        {
            var sut = SqlDatabaseBuilderMock.Create().Schemas;

            var action = Lambda.Of( () => sut.GetOrCreate( name ) );

            action.Test( exc => exc.TestType()
                    .Exact<SqlObjectBuilderException>( e => Assertion.All(
                        e.Dialect.TestEquals( SqlDialectMock.Instance ),
                        e.Errors.Count.TestEquals( 1 ) ) ) )
                .Go();
        }

        [Theory]
        [InlineData( "common", true )]
        [InlineData( "foo", true )]
        [InlineData( "bar", false )]
        public void Contains_ShouldReturnTrue_WhenSchemaExists(string name, bool expected)
        {
            var sut = SqlDatabaseBuilderMock.Create().Schemas;
            sut.Create( "foo" );

            var result = sut.Contains( name );

            result.TestEquals( expected ).Go();
        }

        [Fact]
        public void Get_ShouldReturnExistingSchema()
        {
            var sut = SqlDatabaseBuilderMock.Create().Schemas;
            var expected = sut.Create( "foo" );

            var result = (( ISqlSchemaBuilderCollection )sut).Get( "foo" );

            result.TestRefEquals( expected ).Go();
        }

        [Fact]
        public void Get_ShouldThrowKeyNotFoundException_WhenSchemaDoesNotExist()
        {
            var sut = SqlDatabaseBuilderMock.Create().Schemas;
            var action = Lambda.Of( () => (( ISqlSchemaBuilderCollection )sut).Get( "foo" ) );
            action.Test( exc => exc.TestType().Exact<KeyNotFoundException>() ).Go();
        }

        [Fact]
        public void TryGet_ShouldReturnExistingSchema()
        {
            var sut = SqlDatabaseBuilderMock.Create().Schemas;
            var expected = sut.Create( "foo" );

            var result = (( ISqlSchemaBuilderCollection )sut).TryGet( "foo" );

            result.TestRefEquals( expected ).Go();
        }

        [Fact]
        public void TryGet_ShouldReturnNull_WhenSchemaDoesNotExist()
        {
            var sut = SqlDatabaseBuilderMock.Create().Schemas;
            var result = (( ISqlSchemaBuilderCollection )sut).TryGet( "foo" );
            result.TestNull().Go();
        }

        [Fact]
        public void Remove_ShouldRemoveExistingSchema()
        {
            var sut = SqlDatabaseBuilderMock.Create().Schemas;
            var schema = sut.Create( "foo" );
            var otherTable = schema.Objects.CreateTable( "U" );
            var otherPk = otherTable.Constraints.SetPrimaryKey( otherTable.Columns.Create( "D1" ).Asc() );
            var table = schema.Objects.CreateTable( "T" );
            var c1 = table.Columns.Create( "C1" );
            var c2 = table.Columns.Create( "C2" );
            var pk = table.Constraints.SetPrimaryKey( c1.Asc() );
            var ix = table.Constraints.CreateIndex( c2.Asc() );
            var selfFk = table.Constraints.CreateForeignKey( ix, pk.Index );
            var externalFk = table.Constraints.CreateForeignKey( pk.Index, otherPk.Index );
            var chk = table.Constraints.CreateCheck( c1.Node > SqlNode.Literal( 0 ) );
            var view = schema.Objects.CreateView( "V", table.Node.ToDataSource().Select( d => new[] { d.GetAll() } ) );

            var result = sut.Remove( schema.Name );

            Assertion.All(
                    result.TestTrue(),
                    sut.TryGet( schema.Name ).TestNull(),
                    schema.IsRemoved.TestTrue(),
                    schema.ReferencingObjects.TestEmpty(),
                    schema.Objects.TestEmpty(),
                    c1.IsRemoved.TestTrue(),
                    c1.ReferencingObjects.TestEmpty(),
                    c2.IsRemoved.TestTrue(),
                    c2.ReferencingObjects.TestEmpty(),
                    pk.IsRemoved.TestTrue(),
                    pk.ReferencingObjects.TestEmpty(),
                    pk.Index.IsRemoved.TestTrue(),
                    pk.Index.ReferencingObjects.TestEmpty(),
                    pk.Index.Columns.Expressions.TestEmpty(),
                    pk.Index.PrimaryKey.TestNull(),
                    ix.IsRemoved.TestTrue(),
                    ix.ReferencingObjects.TestEmpty(),
                    ix.Columns.Expressions.TestEmpty(),
                    selfFk.IsRemoved.TestTrue(),
                    selfFk.ReferencingObjects.TestEmpty(),
                    externalFk.IsRemoved.TestTrue(),
                    externalFk.ReferencingObjects.TestEmpty(),
                    chk.IsRemoved.TestTrue(),
                    chk.ReferencingObjects.TestEmpty(),
                    chk.ReferencedColumns.TestEmpty(),
                    view.IsRemoved.TestTrue(),
                    view.ReferencingObjects.TestEmpty(),
                    view.ReferencedObjects.TestEmpty(),
                    otherPk.Index.ReferencingObjects.TestEmpty(),
                    otherTable.ReferencingObjects.TestEmpty(),
                    table.ReferencingObjects.TestEmpty() )
                .Go();
        }

        [Fact]
        public void Remove_ShouldReturnFalse_WhenSchemaDoesNotExist()
        {
            var sut = SqlDatabaseBuilderMock.Create().Schemas;
            var result = sut.Remove( "foo" );
            result.TestFalse().Go();
        }

        [Fact]
        public void Remove_ShouldReturnFalse_WhenSchemaIsDefault()
        {
            var sut = SqlDatabaseBuilderMock.Create().Schemas;

            var result = sut.Remove( sut.Database.Schemas.Default.Name );

            Assertion.All(
                    result.TestFalse(),
                    sut.Database.Schemas.Default.IsRemoved.TestFalse(),
                    sut.Count.TestEquals( 1 ),
                    sut.TryGet( sut.Database.Schemas.Default.Name ).TestRefEquals( sut.Database.Schemas.Default ) )
                .Go();
        }

        [Fact]
        public void Remove_ShouldReturnFalse_WhenSchemaIsReferencedByForeignKeyFromAnotherSchema()
        {
            var sut = SqlDatabaseBuilderMock.Create().Schemas;
            var schema = sut.Create( "foo" );
            var table = schema.Objects.CreateTable( "T1" );
            var pk = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );

            var otherTable = sut.Default.Objects.CreateTable( "T2" );
            var otherPk = otherTable.Constraints.SetPrimaryKey( otherTable.Columns.Create( "C2" ).Asc() );
            otherTable.Constraints.CreateForeignKey( otherPk.Index, pk.Index );

            var result = sut.Remove( schema.Name );

            Assertion.All(
                    result.TestFalse(),
                    schema.IsRemoved.TestFalse(),
                    sut.Count.TestEquals( 2 ),
                    sut.TryGet( schema.Name ).TestRefEquals( schema ) )
                .Go();
        }

        [Fact]
        public void Remove_ShouldReturnFalse_WhenSchemaIsReferencedByViewFromAnotherSchema()
        {
            var sut = SqlDatabaseBuilderMock.Create().Schemas;
            var schema = sut.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            table.Constraints.SetPrimaryKey( table.Columns.Create( "C" ).Asc() );

            sut.Default.Objects.CreateView( "V", table.Node.ToDataSource().Select( s => new[] { s.GetAll() } ) );

            var result = sut.Remove( schema.Name );

            Assertion.All(
                    result.TestFalse(),
                    schema.IsRemoved.TestFalse(),
                    sut.Count.TestEquals( 2 ),
                    sut.TryGet( schema.Name ).TestRefEquals( schema ) )
                .Go();
        }

        [Fact]
        public void GetEnumerator_ShouldReturnCorrectResult()
        {
            var sut = SqlDatabaseBuilderMock.Create().Schemas;
            var schema = sut.Create( "foo" );

            var result = new List<SqlSchemaBuilderMock>();
            foreach ( var s in sut )
                result.Add( s );

            Assertion.All(
                    result.Count.TestEquals( 2 ),
                    result.TestSetEqual( [ sut.Default, schema ] ) )
                .Go();
        }

        [Fact]
        public void ISqlSchemaBuilderCollection_Create_ShouldBeEquivalentToCreate()
        {
            var sut = SqlDatabaseBuilderMock.Create().Schemas;

            var result = (( ISqlSchemaBuilderCollection )sut).Create( "foo" );

            Assertion.All(
                    result.Database.TestRefEquals( sut.Database ),
                    result.Type.TestEquals( SqlObjectType.Schema ),
                    result.Name.TestEquals( "foo" ),
                    result.Objects.TestEmpty(),
                    result.Objects.Schema.TestRefEquals( result ),
                    result.ReferencingObjects.TestEmpty(),
                    sut.Count.TestEquals( 2 ),
                    sut.TestSetEqual( [ sut.Database.Schemas.Default, result ] ),
                    sut.TryGet( result.Name ).TestRefEquals( result ) )
                .Go();
        }

        [Fact]
        public void ISqlSchemaBuilderCollection_GetOrCreate_ShouldBeEquivalentToGetOrCreate()
        {
            var sut = SqlDatabaseBuilderMock.Create().Schemas;
            var expected = sut.Default;

            var result = (( ISqlSchemaBuilderCollection )sut).GetOrCreate( expected.Name );

            Assertion.All(
                    result.TestRefEquals( expected ),
                    sut.Count.TestEquals( 1 ) )
                .Go();
        }
    }
}
