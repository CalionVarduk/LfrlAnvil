using System.Collections.Generic;
using LfrlAnvil.Functional;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Extensions;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.TestExtensions.Sql.Mocks;

namespace LfrlAnvil.Sql.Tests.ObjectsTests.BuildersTests;

public partial class SqlSchemaBuilderTests
{
    public class Objects : TestsBase
    {
        [Fact]
        public void CreateTable_ShouldCreateNewTable()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var sut = schema.Objects;

            var result = sut.CreateTable( "T" );

            Assertion.All(
                    result.Schema.TestRefEquals( sut.Schema ),
                    result.Database.TestRefEquals( schema.Database ),
                    result.Type.TestEquals( SqlObjectType.Table ),
                    result.Name.TestEquals( "T" ),
                    result.Info.TestEquals( SqlRecordSetInfo.Create( "foo", "T" ) ),
                    result.Columns.TestEmpty(),
                    result.Columns.Table.TestRefEquals( result ),
                    result.Columns.DefaultTypeDefinition.TestRefEquals( schema.Database.TypeDefinitions.GetByType<object>() ),
                    result.Columns.Identity.TestNull(),
                    result.Constraints.TestEmpty(),
                    result.Constraints.Table.TestRefEquals( result ),
                    result.Constraints.TryGetPrimaryKey().TestNull(),
                    result.ReferencingObjects.TestEmpty(),
                    result.Node.Table.TestRefEquals( result ),
                    result.Node.Info.TestEquals( result.Info ),
                    result.Node.Alias.TestNull(),
                    result.Node.Identifier.TestEquals( result.Info.Identifier ),
                    result.Node.IsOptional.TestFalse(),
                    (( ISqlTableBuilder )result).Schema.TestRefEquals( result.Schema ),
                    (( ISqlTableBuilder )result).Columns.TestRefEquals( result.Columns ),
                    (( ISqlTableBuilder )result).Constraints.TestRefEquals( result.Constraints ),
                    (( ISqlObjectBuilder )result).Database.TestRefEquals( result.Database ),
                    (( ISqlObjectBuilder )result).ReferencingObjects.TestSequence(
                        result.ReferencingObjects.UnsafeReinterpretAs<ISqlObjectBuilder>() ),
                    (( ISqlColumnBuilderCollection )result.Columns).Table.TestRefEquals( result.Columns.Table ),
                    (( ISqlColumnBuilderCollection )result.Columns).DefaultTypeDefinition.TestRefEquals(
                        result.Columns.DefaultTypeDefinition ),
                    (( ISqlColumnBuilderCollection )result.Columns).Identity.TestRefEquals( result.Columns.Identity ),
                    (( ISqlConstraintBuilderCollection )result.Constraints).Table.TestRefEquals( result.Constraints.Table ),
                    sut.Count.TestEquals( 1 ),
                    sut.TestSetEqual( [ result ] ) )
                .Go();
        }

        [Fact]
        public void CreateTable_ShouldThrowSqlObjectBuilderException_WhenSchemaIsRemoved()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var sut = schema.Objects;
            schema.Remove();

            var action = Lambda.Of( () => sut.CreateTable( "T" ) );

            action.Test( exc => exc.TestType()
                    .Exact<SqlObjectBuilderException>( e => Assertion.All(
                        e.Dialect.TestEquals( SqlDialectMock.Instance ),
                        e.Errors.Count.TestEquals( 1 ) ) ) )
                .Go();
        }

        [Fact]
        public void CreateTable_ShouldThrowSqlObjectBuilderException_WhenObjectNameAlreadyExists()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var sut = schema.Objects;
            var table = sut.CreateTable( "T" );
            table.Constraints.SetPrimaryKey( table.Columns.Create( "C" ).Asc() );

            var action = Lambda.Of( () => sut.CreateTable( "PK_T" ) );

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
        public void CreateTable_ShouldThrowSqlObjectBuilderException_WhenNameIsInvalid(string name)
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var sut = schema.Objects;

            var action = Lambda.Of( () => sut.CreateTable( name ) );

            action.Test( exc => exc.TestType()
                    .Exact<SqlObjectBuilderException>( e => Assertion.All(
                        e.Dialect.TestEquals( SqlDialectMock.Instance ),
                        e.Errors.Count.TestEquals( 1 ) ) ) )
                .Go();
        }

        [Fact]
        public void GetOrCreateTable_ShouldCreateNewTable_WhenTableDoesNotExist()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var sut = schema.Objects;

            var result = sut.GetOrCreateTable( "T" );

            Assertion.All(
                    result.Schema.TestRefEquals( sut.Schema ),
                    result.Database.TestRefEquals( schema.Database ),
                    result.Type.TestEquals( SqlObjectType.Table ),
                    result.Name.TestEquals( "T" ),
                    result.Info.TestEquals( SqlRecordSetInfo.Create( "foo", "T" ) ),
                    result.Columns.TestEmpty(),
                    result.Columns.Table.TestRefEquals( result ),
                    result.Columns.DefaultTypeDefinition.TestRefEquals( schema.Database.TypeDefinitions.GetByType<object>() ),
                    result.Columns.Identity.TestNull(),
                    result.Constraints.TestEmpty(),
                    result.Constraints.Table.TestRefEquals( result ),
                    result.Constraints.TryGetPrimaryKey().TestNull(),
                    result.ReferencingObjects.TestEmpty(),
                    result.Node.Table.TestRefEquals( result ),
                    result.Node.Info.TestEquals( result.Info ),
                    result.Node.Alias.TestNull(),
                    result.Node.Identifier.TestEquals( result.Info.Identifier ),
                    result.Node.IsOptional.TestFalse(),
                    (( ISqlTableBuilder )result).Schema.TestRefEquals( result.Schema ),
                    (( ISqlTableBuilder )result).Columns.TestRefEquals( result.Columns ),
                    (( ISqlTableBuilder )result).Constraints.TestRefEquals( result.Constraints ),
                    (( ISqlObjectBuilder )result).Database.TestRefEquals( result.Database ),
                    (( ISqlObjectBuilder )result).ReferencingObjects.TestSequence(
                        result.ReferencingObjects.UnsafeReinterpretAs<ISqlObjectBuilder>() ),
                    (( ISqlColumnBuilderCollection )result.Columns).Table.TestRefEquals( result.Columns.Table ),
                    (( ISqlColumnBuilderCollection )result.Columns).DefaultTypeDefinition.TestRefEquals(
                        result.Columns.DefaultTypeDefinition ),
                    (( ISqlColumnBuilderCollection )result.Columns).Identity.TestRefEquals( result.Columns.Identity ),
                    (( ISqlConstraintBuilderCollection )result.Constraints).Table.TestRefEquals( result.Constraints.Table ),
                    sut.Count.TestEquals( 1 ),
                    sut.TestSetEqual( [ result ] ) )
                .Go();
        }

        [Fact]
        public void GetOrCreateTable_ShouldReturnExistingTable_WhenTableWithNameAlreadyExists()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var sut = schema.Objects;
            var expected = sut.CreateTable( "T" );

            var result = sut.GetOrCreateTable( "T" );

            Assertion.All(
                    result.TestRefEquals( expected ),
                    sut.Count.TestEquals( 1 ) )
                .Go();
        }

        [Fact]
        public void GetOrCreateTable_ShouldThrowSqlObjectBuilderException_WhenSchemaIsRemoved()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var sut = schema.Objects;
            schema.Remove();

            var action = Lambda.Of( () => sut.GetOrCreateTable( "T" ) );

            action.Test( exc => exc.TestType()
                    .Exact<SqlObjectBuilderException>( e => Assertion.All(
                        e.Dialect.TestEquals( SqlDialectMock.Instance ),
                        e.Errors.Count.TestEquals( 1 ) ) ) )
                .Go();
        }

        [Fact]
        public void GetOrCreateTable_ShouldThrowSqlObjectCastException_WhenNonTableObjectWithNameAlreadyExists()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var sut = schema.Objects;
            var t = sut.CreateTable( "T" );
            var c = t.Columns.Create( "C" );
            t.Constraints.SetPrimaryKey( c.Asc() ).SetName( "bar" );

            var action = Lambda.Of( () => sut.GetOrCreateTable( "bar" ) );

            action.Test( exc => exc.TestType()
                    .Exact<SqlObjectCastException>( e => Assertion.All(
                        e.Dialect.TestEquals( SqlDialectMock.Instance ),
                        e.Expected.TestEquals( typeof( SqlTableBuilder ) ),
                        e.Actual.TestEquals( typeof( SqlPrimaryKeyBuilderMock ) ) ) ) )
                .Go();
        }

        [Theory]
        [InlineData( "" )]
        [InlineData( " " )]
        [InlineData( "'" )]
        [InlineData( "f\'oo" )]
        public void GetOrCreateTable_ShouldThrowSqlObjectBuilderException_WhenNameIsInvalid(string name)
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var sut = schema.Objects;

            var action = Lambda.Of( () => sut.GetOrCreateTable( name ) );

            action.Test( exc => exc.TestType()
                    .Exact<SqlObjectBuilderException>( e => Assertion.All(
                        e.Dialect.TestEquals( SqlDialectMock.Instance ),
                        e.Errors.Count.TestEquals( 1 ) ) ) )
                .Go();
        }

        [Fact]
        public void CreateView_ShouldCreateNewView_WithRawSource()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var sut = schema.Objects;
            var source = SqlNode.RawQuery( "SELECT * FROM bar" );

            var result = sut.CreateView( "V", source );

            Assertion.All(
                    result.Schema.TestRefEquals( sut.Schema ),
                    result.Database.TestRefEquals( schema.Database ),
                    result.Type.TestEquals( SqlObjectType.View ),
                    result.Name.TestEquals( "V" ),
                    result.Info.TestEquals( SqlRecordSetInfo.Create( "foo", "V" ) ),
                    result.Source.TestRefEquals( source ),
                    result.ReferencedObjects.TestEmpty(),
                    result.ReferencingObjects.TestEmpty(),
                    result.Node.View.TestRefEquals( result ),
                    result.Node.Info.TestEquals( result.Info ),
                    result.Node.Alias.TestNull(),
                    result.Node.Identifier.TestEquals( result.Info.Identifier ),
                    result.Node.IsOptional.TestFalse(),
                    (( ISqlViewBuilder )result).Schema.TestRefEquals( result.Schema ),
                    (( ISqlViewBuilder )result).ReferencedObjects.TestSequence( result.ReferencedObjects ),
                    (( ISqlObjectBuilder )result).Database.TestRefEquals( result.Database ),
                    (( ISqlObjectBuilder )result).ReferencingObjects.TestSequence(
                        result.ReferencingObjects.UnsafeReinterpretAs<ISqlObjectBuilder>() ),
                    sut.Count.TestEquals( 1 ),
                    sut.TestSetEqual( [ result ] ) )
                .Go();
        }

        [Fact]
        public void CreateView_ShouldCreateNewViewWithColumnReference()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var sut = schema.Objects;

            var table = sut.CreateTable( "T" );
            var column = table.Columns.Create( "C" );
            var pk = table.Constraints.SetPrimaryKey( column.Asc() );

            var source = table.Node.ToDataSource().Select( s => new[] { s.From["C"].AsSelf() } );
            var result = sut.CreateView( "V", source );

            Assertion.All(
                    result.Schema.TestRefEquals( sut.Schema ),
                    result.Database.TestRefEquals( schema.Database ),
                    result.Type.TestEquals( SqlObjectType.View ),
                    result.Name.TestEquals( "V" ),
                    result.Info.TestEquals( SqlRecordSetInfo.Create( "foo", "V" ) ),
                    result.Source.TestRefEquals( source ),
                    result.ReferencedObjects.Count.TestEquals( 2 ),
                    result.ReferencedObjects.TestSetEqual( [ table, column ] ),
                    result.ReferencingObjects.TestEmpty(),
                    result.Node.View.TestRefEquals( result ),
                    result.Node.Info.TestEquals( result.Info ),
                    result.Node.Alias.TestNull(),
                    result.Node.Identifier.TestEquals( result.Info.Identifier ),
                    result.Node.IsOptional.TestFalse(),
                    (( ISqlViewBuilder )result).Schema.TestRefEquals( result.Schema ),
                    (( ISqlViewBuilder )result).ReferencedObjects.TestSequence( result.ReferencedObjects ),
                    (( ISqlObjectBuilder )result).Database.TestRefEquals( result.Database ),
                    (( ISqlObjectBuilder )result).ReferencingObjects.TestSequence(
                        result.ReferencingObjects.UnsafeReinterpretAs<ISqlObjectBuilder>() ),
                    sut.Count.TestEquals( 4 ),
                    sut.TestSetEqual( [ table, pk.Index, pk, result ] ),
                    table.ReferencingObjects.TestSequence(
                        [ SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( result ), table ) ] ),
                    column.ReferencingObjects.Count.TestEquals( 2 ),
                    column.ReferencingObjects.TestSetEqual(
                    [
                        SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( pk.Index ), column ),
                        SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( result ), column )
                    ] ),
                    schema.ReferencingObjects.TestEmpty() )
                .Go();
        }

        [Fact]
        public void CreateView_ShouldCreateNewViewWithAnotherViewReference()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var sut = schema.Objects;
            var other = sut.CreateView( "W", SqlNode.RawQuery( "SELECT * FROM bar" ) );

            var source = other.Node.ToDataSource().Select( s => new[] { s.GetAll() } );
            var result = sut.CreateView( "V", source );

            Assertion.All(
                    result.Schema.TestRefEquals( sut.Schema ),
                    result.Database.TestRefEquals( schema.Database ),
                    result.Type.TestEquals( SqlObjectType.View ),
                    result.Name.TestEquals( "V" ),
                    result.Info.TestEquals( SqlRecordSetInfo.Create( "foo", "V" ) ),
                    result.Source.TestRefEquals( source ),
                    result.ReferencedObjects.Count.TestEquals( 1 ),
                    result.ReferencedObjects.TestSetEqual( [ other ] ),
                    result.ReferencingObjects.TestEmpty(),
                    result.Node.View.TestRefEquals( result ),
                    result.Node.Info.TestEquals( result.Info ),
                    result.Node.Alias.TestNull(),
                    result.Node.Identifier.TestEquals( result.Info.Identifier ),
                    result.Node.IsOptional.TestFalse(),
                    (( ISqlViewBuilder )result).Schema.TestRefEquals( result.Schema ),
                    (( ISqlViewBuilder )result).ReferencedObjects.TestSequence( result.ReferencedObjects ),
                    (( ISqlObjectBuilder )result).Database.TestRefEquals( result.Database ),
                    (( ISqlObjectBuilder )result).ReferencingObjects.TestSequence(
                        result.ReferencingObjects.UnsafeReinterpretAs<ISqlObjectBuilder>() ),
                    sut.Count.TestEquals( 2 ),
                    sut.TestSetEqual( [ other, result ] ),
                    other.ReferencingObjects.TestSequence(
                        [ SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( result ), other ) ] ),
                    schema.ReferencingObjects.TestEmpty() )
                .Go();
        }

        [Fact]
        public void CreateView_ShouldCreateNewViewWithColumnReferenceFromAnotherSchema()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var sut = schema.Objects;

            var table = schema.Database.Schemas.Default.Objects.CreateTable( "T" );
            var column = table.Columns.Create( "C" );
            var pk = table.Constraints.SetPrimaryKey( column.Asc() );

            var source = table.Node.ToDataSource().Select( s => new[] { s.From["C"].AsSelf() } );
            var result = sut.CreateView( "V", source );

            Assertion.All(
                    result.Schema.TestRefEquals( sut.Schema ),
                    result.Database.TestRefEquals( schema.Database ),
                    result.Type.TestEquals( SqlObjectType.View ),
                    result.Name.TestEquals( "V" ),
                    result.Info.TestEquals( SqlRecordSetInfo.Create( "foo", "V" ) ),
                    result.Source.TestRefEquals( source ),
                    result.ReferencedObjects.Count.TestEquals( 3 ),
                    result.ReferencedObjects.TestSetEqual( [ schema.Database.Schemas.Default, table, column ] ),
                    result.ReferencingObjects.TestEmpty(),
                    result.Node.View.TestRefEquals( result ),
                    result.Node.Info.TestEquals( result.Info ),
                    result.Node.Alias.TestNull(),
                    result.Node.Identifier.TestEquals( result.Info.Identifier ),
                    result.Node.IsOptional.TestFalse(),
                    (( ISqlViewBuilder )result).Schema.TestRefEquals( result.Schema ),
                    (( ISqlViewBuilder )result).ReferencedObjects.TestSequence( result.ReferencedObjects ),
                    (( ISqlObjectBuilder )result).Database.TestRefEquals( result.Database ),
                    (( ISqlObjectBuilder )result).ReferencingObjects.TestSequence(
                        result.ReferencingObjects.UnsafeReinterpretAs<ISqlObjectBuilder>() ),
                    sut.Count.TestEquals( 1 ),
                    sut.TestSetEqual( [ result ] ),
                    table.ReferencingObjects.TestSequence(
                        [ SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( result ), table ) ] ),
                    column.ReferencingObjects.Count.TestEquals( 2 ),
                    column.ReferencingObjects.TestSetEqual(
                    [
                        SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( pk.Index ), column ),
                        SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( result ), column )
                    ] ),
                    schema.Database.Schemas.Default.ReferencingObjects.TestSequence(
                    [
                        SqlObjectBuilderReference.Create(
                            SqlObjectBuilderReferenceSource.Create( result ),
                            schema.Database.Schemas.Default )
                    ] ) )
                .Go();
        }

        [Fact]
        public void CreateView_ShouldCreateNewViewWithAnotherViewReferenceFromAnotherSchema()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var sut = schema.Objects;
            var other = schema.Database.Schemas.Default.Objects.CreateView( "W", SqlNode.RawQuery( "SELECT * FROM bar" ) );

            var source = other.Node.ToDataSource().Select( s => new[] { s.GetAll() } );
            var result = sut.CreateView( "V", source );

            Assertion.All(
                    result.Schema.TestRefEquals( sut.Schema ),
                    result.Database.TestRefEquals( schema.Database ),
                    result.Type.TestEquals( SqlObjectType.View ),
                    result.Name.TestEquals( "V" ),
                    result.Info.TestEquals( SqlRecordSetInfo.Create( "foo", "V" ) ),
                    result.Source.TestRefEquals( source ),
                    result.ReferencedObjects.Count.TestEquals( 2 ),
                    result.ReferencedObjects.TestSetEqual( [ schema.Database.Schemas.Default, other ] ),
                    result.ReferencingObjects.TestEmpty(),
                    result.Node.View.TestRefEquals( result ),
                    result.Node.Info.TestEquals( result.Info ),
                    result.Node.Alias.TestNull(),
                    result.Node.Identifier.TestEquals( result.Info.Identifier ),
                    result.Node.IsOptional.TestFalse(),
                    (( ISqlViewBuilder )result).Schema.TestRefEquals( result.Schema ),
                    (( ISqlViewBuilder )result).ReferencedObjects.TestSequence( result.ReferencedObjects ),
                    (( ISqlObjectBuilder )result).Database.TestRefEquals( result.Database ),
                    (( ISqlObjectBuilder )result).ReferencingObjects.TestSequence(
                        result.ReferencingObjects.UnsafeReinterpretAs<ISqlObjectBuilder>() ),
                    sut.Count.TestEquals( 1 ),
                    sut.TestSetEqual( [ result ] ),
                    other.ReferencingObjects.TestSequence(
                        [ SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( result ), other ) ] ),
                    schema.Database.Schemas.Default.ReferencingObjects.TestSequence(
                    [
                        SqlObjectBuilderReference.Create(
                            SqlObjectBuilderReferenceSource.Create( result ),
                            schema.Database.Schemas.Default )
                    ] ) )
                .Go();
        }

        [Fact]
        public void CreateView_ShouldThrowSqlObjectBuilderException_WhenSourceIsNotValid()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var sut = schema.Objects;

            var source = SqlNode.RawQuery( "SELECT * FROM foo WHERE a > @a", SqlNode.Parameter<int>( "a" ) );
            var action = Lambda.Of( () => sut.CreateView( "V", source ) );

            action.Test( exc => exc.TestType()
                    .Exact<SqlObjectBuilderException>( e => Assertion.All(
                        e.Dialect.TestEquals( SqlDialectMock.Instance ),
                        e.Errors.Count.TestEquals( 1 ) ) ) )
                .Go();
        }

        [Fact]
        public void CreateView_ShouldThrowSqlObjectBuilderException_WhenSchemaIsRemoved()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var sut = schema.Objects;
            schema.Remove();

            var action = Lambda.Of( () => sut.CreateView( "V", SqlNode.RawQuery( "SELECT * FROM bar" ) ) );

            action.Test( exc => exc.TestType()
                    .Exact<SqlObjectBuilderException>( e => Assertion.All(
                        e.Dialect.TestEquals( SqlDialectMock.Instance ),
                        e.Errors.Count.TestEquals( 1 ) ) ) )
                .Go();
        }

        [Fact]
        public void CreateView_ShouldThrowSqlObjectBuilderException_WhenObjectNameAlreadyExists()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var sut = schema.Objects;
            sut.CreateTable( "T" );

            var action = Lambda.Of( () => sut.CreateView( "T", SqlNode.RawQuery( "SELECT * FROM bar" ) ) );

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
        public void CreateView_ShouldThrowSqlObjectBuilderException_WhenNameIsInvalid(string name)
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var sut = schema.Objects;

            var action = Lambda.Of( () => sut.CreateView( name, SqlNode.RawQuery( "SELECT * FROM foo" ) ) );

            action.Test( exc => exc.TestType()
                    .Exact<SqlObjectBuilderException>( e => Assertion.All(
                        e.Dialect.TestEquals( SqlDialectMock.Instance ),
                        e.Errors.Count.TestEquals( 1 ) ) ) )
                .Go();
        }

        [Theory]
        [InlineData( "foo", false )]
        [InlineData( "T", true )]
        [InlineData( "PK", true )]
        [InlineData( "IX", true )]
        [InlineData( "V", true )]
        [InlineData( "CHK", true )]
        public void Contains_ShouldReturnTrue_WhenObjectExists(string name, bool expected)
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var sut = schema.Objects;
            var t = sut.CreateTable( "T" );
            var c = t.Columns.Create( "C" );
            t.Constraints.SetPrimaryKey( c.Asc() ).SetName( "PK" ).Index.SetName( "IX" );
            t.Constraints.CreateCheck( t.Node["C"] != null ).SetName( "CHK" );
            sut.CreateView( "V", SqlNode.RawQuery( "SELECT * FROM bar" ) );

            var result = sut.Contains( name );

            result.TestEquals( expected ).Go();
        }

        [Fact]
        public void Get_ShouldReturnExistingObject()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var sut = schema.Objects;
            var expected = sut.CreateTable( "T" );

            var result = (( ISqlObjectBuilderCollection )sut).Get( "T" );

            result.TestRefEquals( expected ).Go();
        }

        [Fact]
        public void Get_ShouldThrowKeyNotFoundException_WhenObjectDoesNotExist()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var sut = schema.Objects;

            var action = Lambda.Of( () => (( ISqlObjectBuilderCollection )sut).Get( "T" ) );

            action.Test( exc => exc.TestType().Exact<KeyNotFoundException>() ).Go();
        }

        [Fact]
        public void TryGet_ShouldReturnExistingObject()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var sut = schema.Objects;
            var expected = sut.CreateTable( "T" );

            var result = (( ISqlObjectBuilderCollection )sut).TryGet( "T" );

            result.TestRefEquals( expected ).Go();
        }

        [Fact]
        public void TryGetObject_ShouldReturnNull_WhenObjectDoesNotExist()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var sut = schema.Objects;

            var result = (( ISqlObjectBuilderCollection )sut).TryGet( "T" );

            result.TestNull().Go();
        }

        [Fact]
        public void GetTable_ShouldReturnExistingTable()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var sut = schema.Objects;
            var expected = sut.CreateTable( "T" );

            var result = (( ISqlObjectBuilderCollection )sut).GetTable( "T" );

            result.TestRefEquals( expected ).Go();
        }

        [Fact]
        public void GetTable_ShouldThrowKeyNotFoundException_WhenObjectDoesNotExist()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var sut = schema.Objects;

            var action = Lambda.Of( () => (( ISqlObjectBuilderCollection )sut).GetTable( "T" ) );

            action.Test( exc => exc.TestType().Exact<KeyNotFoundException>() ).Go();
        }

        [Fact]
        public void GetTable_ShouldThrowSqlObjectCastException_WhenObjectExistsButNotAsTable()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var sut = schema.Objects;
            var t = sut.CreateTable( "T" );
            var c = t.Columns.Create( "C" );
            t.Constraints.SetPrimaryKey( c.Asc() ).SetName( "bar" );

            var action = Lambda.Of( () => (( ISqlObjectBuilderCollection )sut).GetTable( "bar" ) );

            action.Test( exc => exc.TestType()
                    .Exact<SqlObjectCastException>( e => Assertion.All(
                        e.Dialect.TestEquals( SqlDialectMock.Instance ),
                        e.Expected.TestEquals( typeof( SqlTableBuilder ) ),
                        e.Actual.TestEquals( typeof( SqlPrimaryKeyBuilderMock ) ) ) ) )
                .Go();
        }

        [Fact]
        public void TryGetTable_ShouldReturnExistingTable()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var sut = schema.Objects;
            var expected = sut.CreateTable( "T" );

            var result = (( ISqlObjectBuilderCollection )sut).TryGetTable( "T" );

            result.TestRefEquals( expected ).Go();
        }

        [Fact]
        public void TryGetTable_ShouldReturnNull_WhenObjectDoesNotExist()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var sut = schema.Objects;

            var result = (( ISqlObjectBuilderCollection )sut).TryGetTable( "T" );

            result.TestNull().Go();
        }

        [Fact]
        public void TryGetTable_ShouldReturnNull_WhenObjectExistsButNotAsTable()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var sut = schema.Objects;
            var t = sut.CreateTable( "T" );
            var c = t.Columns.Create( "C" );
            t.Constraints.SetPrimaryKey( c.Asc() ).SetName( "bar" );

            var result = (( ISqlObjectBuilderCollection )sut).TryGetTable( "bar" );

            result.TestNull().Go();
        }

        [Fact]
        public void GetIndex_ShouldReturnExistingIndex()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var sut = schema.Objects;
            var t = sut.CreateTable( "T" );
            var c = t.Columns.Create( "C" );
            var expected = t.Constraints.CreateIndex( c.Asc() ).SetName( "bar" );

            var result = (( ISqlObjectBuilderCollection )sut).GetIndex( "bar" );

            result.TestRefEquals( expected ).Go();
        }

        [Fact]
        public void GetIndex_ShouldThrowKeyNotFoundException_WhenObjectDoesNotExist()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var sut = schema.Objects;

            var action = Lambda.Of( () => (( ISqlObjectBuilderCollection )sut).GetIndex( "bar" ) );

            action.Test( exc => exc.TestType().Exact<KeyNotFoundException>() ).Go();
        }

        [Fact]
        public void GetIndex_ShouldThrowSqlObjectCastException_WhenObjectExistsButNotAsIndex()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var sut = schema.Objects;
            sut.CreateTable( "bar" );

            var action = Lambda.Of( () => (( ISqlObjectBuilderCollection )sut).GetIndex( "bar" ) );

            action.Test( exc => exc.TestType()
                    .Exact<SqlObjectCastException>( e => Assertion.All(
                        e.Dialect.TestEquals( SqlDialectMock.Instance ),
                        e.Expected.TestEquals( typeof( SqlIndexBuilder ) ),
                        e.Actual.TestEquals( typeof( SqlTableBuilderMock ) ) ) ) )
                .Go();
        }

        [Fact]
        public void TryGetIndex_ShouldReturnExistingIndex()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var sut = schema.Objects;
            var t = sut.CreateTable( "T" );
            var c = t.Columns.Create( "C" );
            var expected = t.Constraints.CreateIndex( c.Asc() ).SetName( "bar" );

            var result = (( ISqlObjectBuilderCollection )sut).TryGetIndex( "bar" );

            result.TestRefEquals( expected ).Go();
        }

        [Fact]
        public void TryGetIndex_ShouldReturnFNull_WhenObjectDoesNotExist()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var sut = schema.Objects;

            var result = (( ISqlObjectBuilderCollection )sut).TryGetIndex( "bar" );

            result.TestNull().Go();
        }

        [Fact]
        public void TryGetIndex_ShouldReturnNull_WhenObjectExistsButNotAsIndex()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var sut = schema.Objects;
            sut.CreateTable( "bar" );

            var result = (( ISqlObjectBuilderCollection )sut).TryGetIndex( "bar" );

            result.TestNull().Go();
        }

        [Fact]
        public void GetPrimaryKey_ShouldReturnExistingPrimaryKey()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var sut = schema.Objects;
            var t = sut.CreateTable( "T" );
            var c = t.Columns.Create( "C" );
            var expected = t.Constraints.SetPrimaryKey( c.Asc() ).SetName( "bar" );

            var result = (( ISqlObjectBuilderCollection )sut).GetPrimaryKey( "bar" );

            result.TestRefEquals( expected ).Go();
        }

        [Fact]
        public void GetPrimaryKey_ShouldThrowKeyNotFoundException_WhenObjectDoesNotExist()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var sut = schema.Objects;

            var action = Lambda.Of( () => (( ISqlObjectBuilderCollection )sut).GetPrimaryKey( "bar" ) );

            action.Test( exc => exc.TestType().Exact<KeyNotFoundException>() ).Go();
        }

        [Fact]
        public void GetPrimaryKey_ShouldThrowSqlObjectCastException_WhenObjectExistsButNotAsPrimaryKey()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var sut = schema.Objects;
            sut.CreateTable( "bar" );

            var action = Lambda.Of( () => (( ISqlObjectBuilderCollection )sut).GetPrimaryKey( "bar" ) );

            action.Test( exc => exc.TestType()
                    .Exact<SqlObjectCastException>( e => Assertion.All(
                        e.Dialect.TestEquals( SqlDialectMock.Instance ),
                        e.Expected.TestEquals( typeof( SqlPrimaryKeyBuilder ) ),
                        e.Actual.TestEquals( typeof( SqlTableBuilderMock ) ) ) ) )
                .Go();
        }

        [Fact]
        public void TryGetPrimaryKey_ShouldReturnExistingPrimaryKey()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var sut = schema.Objects;
            var t = sut.CreateTable( "T" );
            var c = t.Columns.Create( "C" );
            var expected = t.Constraints.SetPrimaryKey( c.Asc() ).SetName( "bar" );

            var result = (( ISqlObjectBuilderCollection )sut).TryGetPrimaryKey( "bar" );

            result.TestRefEquals( expected ).Go();
        }

        [Fact]
        public void TryGetPrimaryKey_ShouldReturnNull_WhenObjectDoesNotExist()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var sut = schema.Objects;

            var result = (( ISqlObjectBuilderCollection )sut).TryGetPrimaryKey( "bar" );

            result.TestNull().Go();
        }

        [Fact]
        public void TryGetPrimaryKey_ShouldReturnNull_WhenObjectExistsButNotAsPrimaryKey()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var sut = schema.Objects;
            sut.CreateTable( "bar" );

            var result = (( ISqlObjectBuilderCollection )sut).TryGetPrimaryKey( "bar" );

            result.TestNull().Go();
        }

        [Fact]
        public void GetForeignKey_ShouldReturnExistingForeignKey()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var sut = schema.Objects;
            var t = sut.CreateTable( "T" );
            var c = t.Columns.Create( "C" );
            var d = t.Columns.Create( "D" ).MarkAsNullable();
            var pk = t.Constraints.SetPrimaryKey( c.Asc() );
            var ix = t.Constraints.CreateIndex( d.Asc() );
            var expected = t.Constraints.CreateForeignKey( ix, pk.Index ).SetName( "bar" );

            var result = (( ISqlObjectBuilderCollection )sut).GetForeignKey( "bar" );

            result.TestRefEquals( expected ).Go();
        }

        [Fact]
        public void GetForeignKey_ShouldThrowKeyNotFoundException_WhenObjectDoesNotExist()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var sut = schema.Objects;

            var action = Lambda.Of( () => (( ISqlObjectBuilderCollection )sut).GetForeignKey( "bar" ) );

            action.Test( exc => exc.TestType().Exact<KeyNotFoundException>() ).Go();
        }

        [Fact]
        public void GetForeignKey_ShouldThrowSqlObjectCastException_WhenObjectExistsButNotAsForeignKey()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var sut = schema.Objects;
            sut.CreateTable( "bar" );

            var action = Lambda.Of( () => (( ISqlObjectBuilderCollection )sut).GetForeignKey( "bar" ) );

            action.Test( exc => exc.TestType()
                    .Exact<SqlObjectCastException>( e => Assertion.All(
                        e.Dialect.TestEquals( SqlDialectMock.Instance ),
                        e.Expected.TestEquals( typeof( SqlForeignKeyBuilder ) ),
                        e.Actual.TestEquals( typeof( SqlTableBuilderMock ) ) ) ) )
                .Go();
        }

        [Fact]
        public void TryGetForeignKey_ShouldReturnExistingForeignKey()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var sut = schema.Objects;
            var t = sut.CreateTable( "T" );
            var c = t.Columns.Create( "C" );
            var d = t.Columns.Create( "D" ).MarkAsNullable();
            var pk = t.Constraints.SetPrimaryKey( c.Asc() );
            var ix = t.Constraints.CreateIndex( d.Asc() );
            var expected = t.Constraints.CreateForeignKey( ix, pk.Index ).SetName( "bar" );

            var result = (( ISqlObjectBuilderCollection )sut).TryGetForeignKey( "bar" );

            result.TestRefEquals( expected ).Go();
        }

        [Fact]
        public void TryGetForeignKey_ShouldReturnNull_WhenObjectDoesNotExist()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var sut = schema.Objects;

            var result = (( ISqlObjectBuilderCollection )sut).TryGetForeignKey( "bar" );

            result.TestNull().Go();
        }

        [Fact]
        public void TryGetForeignKey_ShouldReturnNull_WhenObjectExistsButNotAsForeignKey()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var sut = schema.Objects;
            sut.CreateTable( "bar" );

            var result = (( ISqlObjectBuilderCollection )sut).TryGetForeignKey( "bar" );

            result.TestNull().Go();
        }

        [Fact]
        public void GetView_ShouldReturnExistingView()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var sut = schema.Objects;
            var expected = sut.CreateView( "bar", SqlNode.RawQuery( "SELECT * FROM qux" ) );

            var result = (( ISqlObjectBuilderCollection )sut).GetView( "bar" );

            result.TestRefEquals( expected ).Go();
        }

        [Fact]
        public void GetView_ShouldThrowKeyNotFoundException_WhenObjectDoesNotExist()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var sut = schema.Objects;

            var action = Lambda.Of( () => (( ISqlObjectBuilderCollection )sut).GetView( "bar" ) );

            action.Test( exc => exc.TestType().Exact<KeyNotFoundException>() ).Go();
        }

        [Fact]
        public void GetView_ShouldThrowSqlObjectCastException_WhenObjectExistsButNotAsView()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var sut = schema.Objects;
            sut.CreateTable( "bar" );

            var action = Lambda.Of( () => (( ISqlObjectBuilderCollection )sut).GetView( "bar" ) );

            action.Test( exc => exc.TestType()
                    .Exact<SqlObjectCastException>( e => Assertion.All(
                        e.Dialect.TestEquals( SqlDialectMock.Instance ),
                        e.Expected.TestEquals( typeof( SqlViewBuilder ) ),
                        e.Actual.TestEquals( typeof( SqlTableBuilderMock ) ) ) ) )
                .Go();
        }

        [Fact]
        public void TryGetView_ShouldReturnExistingView()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var sut = schema.Objects;
            var expected = sut.CreateView( "bar", SqlNode.RawQuery( "SELECT * FROM qux" ) );

            var result = (( ISqlObjectBuilderCollection )sut).TryGetView( "bar" );

            result.TestRefEquals( expected ).Go();
        }

        [Fact]
        public void TryGetView_ShouldReturnNull_WhenObjectDoesNotExist()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var sut = schema.Objects;

            var result = (( ISqlObjectBuilderCollection )sut).TryGetView( "bar" );

            result.TestNull().Go();
        }

        [Fact]
        public void TryGetView_ShouldReturnNull_WhenObjectExistsButNotAsView()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var sut = schema.Objects;
            sut.CreateTable( "bar" );

            var result = (( ISqlObjectBuilderCollection )sut).TryGetView( "bar" );

            result.TestNull().Go();
        }

        [Fact]
        public void GetCheck_ShouldReturnExistingCheck()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var sut = schema.Objects;
            var t = sut.CreateTable( "T" );
            var c = t.Columns.Create( "C" );
            var expected = t.Constraints.CreateCheck( c.Node != null ).SetName( "bar" );

            var result = (( ISqlObjectBuilderCollection )sut).GetCheck( "bar" );

            result.TestRefEquals( expected ).Go();
        }

        [Fact]
        public void GetCheck_ShouldThrowKeyNotFoundException_WhenObjectDoesNotExist()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var sut = schema.Objects;

            var action = Lambda.Of( () => (( ISqlObjectBuilderCollection )sut).GetCheck( "bar" ) );

            action.Test( exc => exc.TestType().Exact<KeyNotFoundException>() ).Go();
        }

        [Fact]
        public void GetCheck_ShouldThrowSqlObjectCastException_WhenObjectExistsButNotAsCheck()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var sut = schema.Objects;
            sut.CreateTable( "bar" );

            var action = Lambda.Of( () => (( ISqlObjectBuilderCollection )sut).GetCheck( "bar" ) );

            action.Test( exc => exc.TestType()
                    .Exact<SqlObjectCastException>( e => Assertion.All(
                        e.Dialect.TestEquals( SqlDialectMock.Instance ),
                        e.Expected.TestEquals( typeof( SqlCheckBuilder ) ),
                        e.Actual.TestEquals( typeof( SqlTableBuilderMock ) ) ) ) )
                .Go();
        }

        [Fact]
        public void TryGetCheck_ShouldReturnExistingCheck()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var sut = schema.Objects;
            var t = sut.CreateTable( "T" );
            var c = t.Columns.Create( "C" );
            var expected = t.Constraints.CreateCheck( c.Node != null ).SetName( "bar" );

            var result = (( ISqlObjectBuilderCollection )sut).TryGetCheck( "bar" );

            result.TestRefEquals( expected ).Go();
        }

        [Fact]
        public void TryGetCheck_ShouldReturnNull_WhenObjectDoesNotExist()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var sut = schema.Objects;

            var result = (( ISqlObjectBuilderCollection )sut).TryGetCheck( "bar" );

            result.TestNull().Go();
        }

        [Fact]
        public void TryGetCheck_ShouldReturnNull_WhenObjectExistsButNotAsCheck()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var sut = schema.Objects;
            sut.CreateTable( "bar" );

            var result = (( ISqlObjectBuilderCollection )sut).TryGetCheck( "bar" );

            result.TestNull().Go();
        }

        [Fact]
        public void Remove_ShouldRemoveExistingTable()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var sut = schema.Objects;
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

            var result = sut.Remove( table.Name );

            Assertion.All(
                    result.TestTrue(),
                    sut.TryGet( table.Name ).TestNull(),
                    sut.TryGet( pk.Name ).TestNull(),
                    sut.TryGet( pk.Index.Name ).TestNull(),
                    sut.TryGet( ix.Name ).TestNull(),
                    sut.TryGet( selfFk.Name ).TestNull(),
                    sut.TryGet( externalFk.Name ).TestNull(),
                    sut.TryGet( chk.Name ).TestNull(),
                    sut.Count.TestEquals( 3 ),
                    table.IsRemoved.TestTrue(),
                    table.ReferencingObjects.TestEmpty(),
                    table.Columns.TestEmpty(),
                    table.Constraints.TestEmpty(),
                    table.Constraints.TryGetPrimaryKey().TestNull(),
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
                    otherPk.Index.ReferencingObjects.TestEmpty(),
                    otherTable.ReferencingObjects.TestEmpty() )
                .Go();
        }

        [Fact]
        public void Remove_ShouldReturnFalse_WhenTableToRemoveIsReferencedByAnyExternalForeignKey()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var sut = schema.Objects;
            var table = sut.CreateTable( "T" );
            var pk = table.Constraints.SetPrimaryKey( table.Columns.Create( "C" ).Asc() );

            var otherTable = sut.CreateTable( "U" );
            var otherPk = otherTable.Constraints.SetPrimaryKey( otherTable.Columns.Create( "D" ).Asc() );
            otherTable.Constraints.CreateForeignKey( otherPk.Index, pk.Index );

            var result = sut.Remove( table.Name );

            Assertion.All(
                    result.TestFalse(),
                    table.IsRemoved.TestFalse(),
                    sut.Count.TestEquals( 7 ),
                    sut.TryGet( table.Name ).TestRefEquals( table ) )
                .Go();
        }

        [Fact]
        public void Remove_ShouldReturnFalse_WhenTableToRemoveIsReferencedByAnyView()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var sut = schema.Objects;
            var table = sut.CreateTable( "T" );
            table.Constraints.SetPrimaryKey( table.Columns.Create( "C" ).Asc() );
            sut.CreateView( "V", table.Node.ToDataSource().Select( s => new[] { s.GetAll() } ) );

            var result = sut.Remove( table.Name );

            Assertion.All(
                    result.TestFalse(),
                    table.IsRemoved.TestFalse(),
                    sut.Count.TestEquals( 4 ),
                    sut.TryGet( table.Name ).TestRefEquals( table ) )
                .Go();
        }

        [Fact]
        public void Remove_ShouldRemoveExistingView()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var sut = schema.Objects;
            var view = sut.CreateView( "V", SqlNode.RawQuery( "SELECT * FROM bar" ) );

            var result = sut.Remove( view.Name );

            Assertion.All(
                    result.TestTrue(),
                    sut.Count.TestEquals( 0 ),
                    sut.TryGet( view.Name ).TestNull(),
                    view.IsRemoved.TestTrue() )
                .Go();
        }

        [Fact]
        public void Remove_ShouldReturnFalse_WhenViewToRemoveIsReferencedByAnotherView()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var sut = schema.Objects;
            var view = sut.CreateView( "V", SqlNode.RawQuery( "SELECT * FROM bar" ) );
            sut.CreateView( "W", view.ToRecordSet().ToDataSource().Select( s => new[] { s.GetAll() } ) );

            var result = sut.Remove( view.Name );

            Assertion.All(
                    result.TestFalse(),
                    view.IsRemoved.TestFalse(),
                    sut.Count.TestEquals( 2 ),
                    sut.TryGet( view.Name ).TestRefEquals( view ) )
                .Go();
        }

        [Fact]
        public void Remove_ShouldRemoveExistingIndex()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var sut = schema.Objects;
            var table = sut.CreateTable( "T" );
            var c1 = table.Columns.Create( "C1" );
            var c2 = table.Columns.Create( "C2" );
            var index = table.Constraints.CreateIndex( c1.Asc(), c2.Desc() );

            var result = sut.Remove( index.Name );

            Assertion.All(
                    result.TestTrue(),
                    sut.TryGet( index.Name ).TestNull(),
                    index.IsRemoved.TestTrue(),
                    table.Constraints.TryGet( index.Name ).TestNull(),
                    c1.ReferencingObjects.TestEmpty(),
                    c2.ReferencingObjects.TestEmpty() )
                .Go();
        }

        [Fact]
        public void Remove_ShouldReturnFalse_WhenIndexHasOriginatingForeignKey()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var sut = schema.Objects;
            var table = schema.Objects.CreateTable( "T" );
            var pk = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
            var index = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );
            table.Constraints.CreateForeignKey( index, pk.Index );

            var result = sut.Remove( index.Name );

            Assertion.All(
                    result.TestFalse(),
                    index.IsRemoved.TestFalse(),
                    sut.Count.TestEquals( 5 ) )
                .Go();
        }

        [Fact]
        public void Remove_ShouldReturnFalse_WhenIndexHasReferencingForeignKey()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var sut = schema.Objects;
            var table = schema.Objects.CreateTable( "T" );
            var pk = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
            var index = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );
            table.Constraints.CreateForeignKey( index, pk.Index );

            var result = sut.Remove( pk.Index.Name );

            Assertion.All(
                    result.TestFalse(),
                    pk.Index.IsRemoved.TestFalse(),
                    sut.Count.TestEquals( 5 ) )
                .Go();
        }

        [Fact]
        public void Remove_ShouldRemoveExistingPrimaryKey()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var sut = schema.Objects;
            var table = schema.Objects.CreateTable( "T" );
            var column = table.Columns.Create( "C" );
            var pk = table.Constraints.SetPrimaryKey( column.Asc() );

            var result = sut.Remove( pk.Name );

            Assertion.All(
                    result.TestTrue(),
                    sut.TryGet( pk.Name ).TestNull(),
                    sut.TryGet( pk.Index.Name ).TestNull(),
                    pk.IsRemoved.TestTrue(),
                    pk.Index.IsRemoved.TestTrue(),
                    table.Constraints.TryGet( pk.Name ).TestNull(),
                    table.Constraints.TryGet( pk.Index.Name ).TestNull(),
                    table.Constraints.TryGetPrimaryKey().TestNull(),
                    column.ReferencingObjects.TestEmpty() )
                .Go();
        }

        [Fact]
        public void Remove_ShouldReturnFalse_WhenPrimaryKeyUnderlyingIndexHasOriginatingForeignKey()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var sut = schema.Objects;
            var table = schema.Objects.CreateTable( "T" );
            var pk = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
            var index = table.Constraints.CreateUniqueIndex( table.Columns.Create( "C2" ).Asc() );
            table.Constraints.CreateForeignKey( pk.Index, index );

            var result = sut.Remove( pk.Name );

            Assertion.All(
                    result.TestFalse(),
                    index.IsRemoved.TestFalse(),
                    sut.Count.TestEquals( 5 ) )
                .Go();
        }

        [Fact]
        public void Remove_ShouldReturnFalse_WhenPrimaryKeyUnderlyingIndexHasReferencingForeignKey()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var sut = schema.Objects;
            var table = schema.Objects.CreateTable( "T" );
            var pk = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
            var index = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );
            table.Constraints.CreateForeignKey( index, pk.Index );

            var result = sut.Remove( pk.Name );

            Assertion.All(
                    result.TestFalse(),
                    pk.Index.IsRemoved.TestFalse(),
                    sut.Count.TestEquals( 5 ) )
                .Go();
        }

        [Fact]
        public void Remove_ShouldRemoveExistingForeignKey()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var sut = schema.Objects;
            var table = schema.Objects.CreateTable( "T" );
            var ix1 = table.Constraints.CreateIndex( table.Columns.Create( "C1" ).Asc() );
            var ix2 = table.Constraints.CreateUniqueIndex( table.Columns.Create( "C2" ).Asc() );
            var fk = table.Constraints.CreateForeignKey( ix1, ix2 );

            var result = sut.Remove( fk.Name );

            Assertion.All(
                    result.TestTrue(),
                    sut.Count.TestEquals( 3 ),
                    sut.TryGet( fk.Name ).TestNull(),
                    fk.IsRemoved.TestTrue(),
                    table.Constraints.TryGet( fk.Name ).TestNull(),
                    ix1.ReferencingObjects.TestEmpty(),
                    ix2.ReferencingObjects.TestEmpty() )
                .Go();
        }

        [Fact]
        public void Remove_ShouldRemoveExistingCheck()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var sut = schema.Objects;
            var table = schema.Objects.CreateTable( "T" );
            var c = table.Columns.Create( "C" );
            var check = table.Constraints.CreateCheck( c.Node > SqlNode.Literal( 0 ) );

            var result = sut.Remove( check.Name );

            Assertion.All(
                    result.TestTrue(),
                    sut.Count.TestEquals( 1 ),
                    sut.TryGet( check.Name ).TestNull(),
                    check.IsRemoved.TestTrue(),
                    table.Constraints.TryGet( check.Name ).TestNull(),
                    c.ReferencingObjects.TestEmpty() )
                .Go();
        }

        [Fact]
        public void Remove_ShouldReturnFalse_WhenObjectDoesNotExist()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var sut = schema.Objects;

            var result = sut.Remove( "PK" );

            result.TestFalse().Go();
        }

        [Fact]
        public void ISqlObjectBuilderCollection_CreateTable_ShouldBeEquivalentToCreateTable()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var sut = schema.Objects;

            var result = (( ISqlObjectBuilderCollection )sut).CreateTable( "T" );

            Assertion.All(
                    result.Schema.TestRefEquals( sut.Schema ),
                    result.Database.TestRefEquals( schema.Database ),
                    result.Type.TestEquals( SqlObjectType.Table ),
                    result.Name.TestEquals( "T" ),
                    result.Info.TestEquals( SqlRecordSetInfo.Create( "foo", "T" ) ),
                    result.Columns.TestEmpty(),
                    result.Columns.Table.TestRefEquals( result ),
                    result.Columns.DefaultTypeDefinition.TestRefEquals( schema.Database.TypeDefinitions.GetByType<object>() ),
                    result.Constraints.TestEmpty(),
                    result.Constraints.Table.TestRefEquals( result ),
                    result.Constraints.TryGetPrimaryKey().TestNull(),
                    result.ReferencingObjects.TestEmpty(),
                    result.Node.Table.TestRefEquals( result ),
                    result.Node.Info.TestEquals( result.Info ),
                    result.Node.Alias.TestNull(),
                    result.Node.Identifier.TestEquals( result.Info.Identifier ),
                    result.Node.IsOptional.TestFalse(),
                    sut.Count.TestEquals( 1 ),
                    sut.TestSetEqual( [ result ] ) )
                .Go();
        }

        [Fact]
        public void ISqlObjectBuilderCollection_GetOrCreateTable_ShouldBeEquivalentToGetOrCreateTable()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var sut = schema.Objects;
            var expected = sut.CreateTable( "T" );

            var result = (( ISqlObjectBuilderCollection )sut).GetOrCreateTable( expected.Name );

            Assertion.All(
                    result.TestRefEquals( expected ),
                    sut.Count.TestEquals( 1 ) )
                .Go();
        }

        [Fact]
        public void ISqlObjectBuilderCollection_CreateView_ShouldBeEquivalentToCreateView()
        {
            var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var sut = schema.Objects;
            var source = SqlNode.RawQuery( "SELECT * FROM bar" );

            var result = (( ISqlObjectBuilderCollection )sut).CreateView( "V", source );

            Assertion.All(
                    result.Schema.TestRefEquals( sut.Schema ),
                    result.Database.TestRefEquals( schema.Database ),
                    result.Type.TestEquals( SqlObjectType.View ),
                    result.Name.TestEquals( "V" ),
                    result.Info.TestEquals( SqlRecordSetInfo.Create( "foo", "V" ) ),
                    result.Source.TestRefEquals( source ),
                    result.ReferencedObjects.TestEmpty(),
                    result.ReferencingObjects.TestEmpty(),
                    result.Node.View.TestRefEquals( result ),
                    result.Node.Info.TestEquals( result.Info ),
                    result.Node.Alias.TestNull(),
                    result.Node.Identifier.TestEquals( result.Info.Identifier ),
                    result.Node.IsOptional.TestFalse(),
                    sut.Count.TestEquals( 1 ),
                    sut.TestSetEqual( [ result ] ) )
                .Go();
        }
    }
}
