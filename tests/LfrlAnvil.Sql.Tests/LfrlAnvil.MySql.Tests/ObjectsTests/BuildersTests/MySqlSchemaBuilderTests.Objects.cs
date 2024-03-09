using System.Collections.Generic;
using LfrlAnvil.Functional;
using LfrlAnvil.MySql.Extensions;
using LfrlAnvil.MySql.Objects.Builders;
using LfrlAnvil.MySql.Tests.Helpers;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Extensions;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.MySql.Tests.ObjectsTests.BuildersTests;

public partial class MySqlSchemaBuilderTests
{
    public class Objects : TestsBase
    {
        [Fact]
        public void CreateTable_ShouldCreateNewTable()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var sut = schema.Objects;

            var result = sut.CreateTable( "T" );

            using ( new AssertionScope() )
            {
                result.Schema.Should().BeSameAs( sut.Schema );
                result.Database.Should().BeSameAs( schema.Database );
                result.Type.Should().Be( SqlObjectType.Table );
                result.Name.Should().Be( "T" );
                result.Info.Should().Be( SqlRecordSetInfo.Create( "foo", "T" ) );
                result.Columns.Should().BeEmpty();
                result.Columns.Table.Should().BeSameAs( result );
                result.Columns.DefaultTypeDefinition.Should().BeSameAs( schema.Database.TypeDefinitions.GetByType<object>() );
                result.Constraints.Should().BeEmpty();
                result.Constraints.Table.Should().BeSameAs( result );
                result.Constraints.TryGetPrimaryKey().Should().BeNull();
                result.ReferencingObjects.Should().BeEmpty();
                result.Node.Table.Should().BeSameAs( result );
                result.Node.Info.Should().Be( result.Info );
                result.Node.Alias.Should().BeNull();
                result.Node.Identifier.Should().Be( result.Info.Identifier );
                result.Node.IsOptional.Should().BeFalse();

                sut.Count.Should().Be( 1 );
                sut.Should().BeEquivalentTo( result );
            }
        }

        [Fact]
        public void CreateTable_ShouldThrowSqlObjectBuilderException_WhenSchemaIsRemoved()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var sut = schema.Objects;
            schema.Remove();

            var action = Lambda.Of( () => sut.CreateTable( "T" ) );

            action.Should()
                .ThrowExactly<SqlObjectBuilderException>()
                .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
        }

        [Fact]
        public void CreateTable_ShouldThrowSqlObjectBuilderException_WhenObjectNameAlreadyExists()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var sut = schema.Objects;
            var table = sut.CreateTable( "T" );
            table.Constraints.SetPrimaryKey( table.Columns.Create( "C" ).Asc() );

            var action = Lambda.Of( () => sut.CreateTable( "PK_T" ) );

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
        public void CreateTable_ShouldThrowSqlObjectBuilderException_WhenNameIsInvalid(string name)
        {
            var db = MySqlDatabaseBuilderMock.Create();
            var sut = db.Schemas.Create( "foo" ).Objects;

            var action = Lambda.Of( () => sut.CreateTable( name ) );

            action.Should()
                .ThrowExactly<SqlObjectBuilderException>()
                .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
        }

        [Fact]
        public void GetOrCreateTable_ShouldCreateNewTable_WhenTableDoesNotExist()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var sut = schema.Objects;

            var result = sut.GetOrCreateTable( "T" );

            using ( new AssertionScope() )
            {
                result.Schema.Should().BeSameAs( sut.Schema );
                result.Database.Should().BeSameAs( schema.Database );
                result.Type.Should().Be( SqlObjectType.Table );
                result.Name.Should().Be( "T" );
                result.Info.Should().Be( SqlRecordSetInfo.Create( "foo", "T" ) );
                result.Columns.Should().BeEmpty();
                result.Columns.Table.Should().BeSameAs( result );
                result.Columns.DefaultTypeDefinition.Should().BeSameAs( schema.Database.TypeDefinitions.GetByType<object>() );
                result.Constraints.Should().BeEmpty();
                result.Constraints.Table.Should().BeSameAs( result );
                result.Constraints.TryGetPrimaryKey().Should().BeNull();
                result.ReferencingObjects.Should().BeEmpty();
                result.Node.Table.Should().BeSameAs( result );
                result.Node.Info.Should().Be( result.Info );
                result.Node.Alias.Should().BeNull();
                result.Node.Identifier.Should().Be( result.Info.Identifier );
                result.Node.IsOptional.Should().BeFalse();

                sut.Count.Should().Be( 1 );
                sut.Should().BeEquivalentTo( result );
            }
        }

        [Fact]
        public void GetOrCreateTable_ShouldReturnExistingTable_WhenTableWithNameAlreadyExists()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var sut = schema.Objects;
            var expected = sut.CreateTable( "T" );

            var result = sut.GetOrCreateTable( "T" );

            using ( new AssertionScope() )
            {
                result.Should().BeSameAs( expected );
                sut.Count.Should().Be( 1 );
            }
        }

        [Fact]
        public void GetOrCreateTable_ShouldThrowSqlObjectBuilderException_WhenSchemaIsRemoved()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var sut = schema.Objects;
            schema.Remove();

            var action = Lambda.Of( () => sut.GetOrCreateTable( "T" ) );

            action.Should()
                .ThrowExactly<SqlObjectBuilderException>()
                .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
        }

        [Fact]
        public void GetOrCreateTable_ShouldThrowSqlObjectCastException_WhenNonTableObjectWithNameAlreadyExists()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var sut = schema.Objects;
            var t = sut.CreateTable( "T" );
            var c = t.Columns.Create( "C" );
            t.Constraints.SetPrimaryKey( c.Asc() ).SetName( "bar" );

            var action = Lambda.Of( () => sut.GetOrCreateTable( "bar" ) );

            action.Should()
                .ThrowExactly<SqlObjectCastException>()
                .AndMatch(
                    e => e.Dialect == MySqlDialect.Instance &&
                        e.Expected == typeof( SqlTableBuilder ) &&
                        e.Actual == typeof( MySqlPrimaryKeyBuilder ) );
        }

        [Theory]
        [InlineData( "" )]
        [InlineData( " " )]
        [InlineData( "`" )]
        [InlineData( "f`oo" )]
        public void GetOrCreateTable_ShouldThrowSqlObjectBuilderException_WhenNameIsInvalid(string name)
        {
            var db = MySqlDatabaseBuilderMock.Create();
            var sut = db.Schemas.Create( "foo" ).Objects;

            var action = Lambda.Of( () => sut.GetOrCreateTable( name ) );

            action.Should()
                .ThrowExactly<SqlObjectBuilderException>()
                .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
        }

        [Fact]
        public void CreateView_ShouldCreateNewView_WithRawSource()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var sut = schema.Objects;
            var source = SqlNode.RawQuery( "SELECT * FROM bar" );

            var result = sut.CreateView( "V", source );

            using ( new AssertionScope() )
            {
                result.Schema.Should().BeSameAs( sut.Schema );
                result.Database.Should().BeSameAs( schema.Database );
                result.Type.Should().Be( SqlObjectType.View );
                result.Name.Should().Be( "V" );
                result.Info.Should().Be( SqlRecordSetInfo.Create( "foo", "V" ) );
                result.Source.Should().BeSameAs( source );
                result.ReferencedObjects.Should().BeEmpty();
                result.ReferencingObjects.Should().BeEmpty();
                result.Node.View.Should().BeSameAs( result );
                result.Node.Info.Should().Be( result.Info );
                result.Node.Alias.Should().BeNull();
                result.Node.Identifier.Should().Be( result.Info.Identifier );
                result.Node.IsOptional.Should().BeFalse();

                sut.Count.Should().Be( 1 );
                sut.Should().BeEquivalentTo( result );
            }
        }

        [Fact]
        public void CreateView_ShouldCreateNewViewWithColumnReference()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var sut = schema.Objects;

            var table = sut.CreateTable( "T" );
            var column = table.Columns.Create( "C" );
            var pk = table.Constraints.SetPrimaryKey( column.Asc() );

            var source = table.Node.ToDataSource().Select( s => new[] { s.From["C"].AsSelf() } );
            var result = sut.CreateView( "V", source );

            using ( new AssertionScope() )
            {
                result.Schema.Should().BeSameAs( sut.Schema );
                result.Database.Should().BeSameAs( schema.Database );
                result.Type.Should().Be( SqlObjectType.View );
                result.Name.Should().Be( "V" );
                result.Info.Should().Be( SqlRecordSetInfo.Create( "foo", "V" ) );
                result.Source.Should().BeSameAs( source );
                result.ReferencedObjects.Should().HaveCount( 2 );
                result.ReferencedObjects.Should().BeEquivalentTo( table, column );
                result.ReferencingObjects.Should().BeEmpty();
                result.Node.View.Should().BeSameAs( result );
                result.Node.Info.Should().Be( result.Info );
                result.Node.Alias.Should().BeNull();
                result.Node.Identifier.Should().Be( result.Info.Identifier );
                result.Node.IsOptional.Should().BeFalse();

                sut.Count.Should().Be( 4 );
                sut.Should().BeEquivalentTo( table, pk.Index, pk, result );

                table.ReferencingObjects.Should()
                    .BeSequentiallyEqualTo(
                        SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( result ), table ) );

                column.ReferencingObjects.Should().HaveCount( 2 );
                column.ReferencingObjects.Should()
                    .BeEquivalentTo(
                        SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( pk.Index ), column ),
                        SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( result ), column ) );

                schema.ReferencingObjects.Should().BeEmpty();
            }
        }

        [Fact]
        public void CreateView_ShouldCreateNewViewWithAnotherViewReference()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var sut = schema.Objects;
            var other = sut.CreateView( "W", SqlNode.RawQuery( "SELECT * FROM bar" ) );

            var source = other.Node.ToDataSource().Select( s => new[] { s.GetAll() } );
            var result = sut.CreateView( "V", source );

            using ( new AssertionScope() )
            {
                result.Schema.Should().BeSameAs( sut.Schema );
                result.Database.Should().BeSameAs( schema.Database );
                result.Type.Should().Be( SqlObjectType.View );
                result.Name.Should().Be( "V" );
                result.Info.Should().Be( SqlRecordSetInfo.Create( "foo", "V" ) );
                result.Source.Should().BeSameAs( source );
                result.ReferencedObjects.Should().HaveCount( 1 );
                result.ReferencedObjects.Should().BeEquivalentTo( other );
                result.ReferencingObjects.Should().BeEmpty();
                result.Node.View.Should().BeSameAs( result );
                result.Node.Info.Should().Be( result.Info );
                result.Node.Alias.Should().BeNull();
                result.Node.Identifier.Should().Be( result.Info.Identifier );
                result.Node.IsOptional.Should().BeFalse();

                sut.Count.Should().Be( 2 );
                sut.Should().BeEquivalentTo( other, result );

                other.ReferencingObjects.Should()
                    .BeSequentiallyEqualTo(
                        SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( result ), other ) );

                schema.ReferencingObjects.Should().BeEmpty();
            }
        }

        [Fact]
        public void CreateView_ShouldCreateNewViewWithColumnReferenceFromAnotherSchema()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var sut = schema.Objects;

            var table = schema.Database.Schemas.Default.Objects.CreateTable( "T" );
            var column = table.Columns.Create( "C" );
            var pk = table.Constraints.SetPrimaryKey( column.Asc() );

            var source = table.Node.ToDataSource().Select( s => new[] { s.From["C"].AsSelf() } );
            var result = sut.CreateView( "V", source );

            using ( new AssertionScope() )
            {
                result.Schema.Should().BeSameAs( sut.Schema );
                result.Database.Should().BeSameAs( schema.Database );
                result.Type.Should().Be( SqlObjectType.View );
                result.Name.Should().Be( "V" );
                result.Info.Should().Be( SqlRecordSetInfo.Create( "foo", "V" ) );
                result.Source.Should().BeSameAs( source );
                result.ReferencedObjects.Should().HaveCount( 3 );
                result.ReferencedObjects.Should().BeEquivalentTo( schema.Database.Schemas.Default, table, column );
                result.ReferencingObjects.Should().BeEmpty();
                result.Node.View.Should().BeSameAs( result );
                result.Node.Info.Should().Be( result.Info );
                result.Node.Alias.Should().BeNull();
                result.Node.Identifier.Should().Be( result.Info.Identifier );
                result.Node.IsOptional.Should().BeFalse();

                sut.Count.Should().Be( 1 );
                sut.Should().BeEquivalentTo( result );

                table.ReferencingObjects.Should()
                    .BeSequentiallyEqualTo(
                        SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( result ), table ) );

                column.ReferencingObjects.Should().HaveCount( 2 );
                column.ReferencingObjects.Should()
                    .BeEquivalentTo(
                        SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( pk.Index ), column ),
                        SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( result ), column ) );

                schema.Database.Schemas.Default.ReferencingObjects.Should()
                    .BeSequentiallyEqualTo(
                        SqlObjectBuilderReference.Create(
                            SqlObjectBuilderReferenceSource.Create( result ),
                            schema.Database.Schemas.Default ) );
            }
        }

        [Fact]
        public void CreateView_ShouldCreateNewViewWithAnotherViewReferenceFromAnotherSchema()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var sut = schema.Objects;
            var other = schema.Database.Schemas.Default.Objects.CreateView( "W", SqlNode.RawQuery( "SELECT * FROM bar" ) );

            var source = other.Node.ToDataSource().Select( s => new[] { s.GetAll() } );
            var result = sut.CreateView( "V", source );

            using ( new AssertionScope() )
            {
                result.Schema.Should().BeSameAs( sut.Schema );
                result.Database.Should().BeSameAs( schema.Database );
                result.Type.Should().Be( SqlObjectType.View );
                result.Name.Should().Be( "V" );
                result.Info.Should().Be( SqlRecordSetInfo.Create( "foo", "V" ) );
                result.Source.Should().BeSameAs( source );
                result.ReferencedObjects.Should().HaveCount( 2 );
                result.ReferencedObjects.Should().BeEquivalentTo( schema.Database.Schemas.Default, other );
                result.ReferencingObjects.Should().BeEmpty();
                result.Node.View.Should().BeSameAs( result );
                result.Node.Info.Should().Be( result.Info );
                result.Node.Alias.Should().BeNull();
                result.Node.Identifier.Should().Be( result.Info.Identifier );
                result.Node.IsOptional.Should().BeFalse();

                sut.Count.Should().Be( 1 );
                sut.Should().BeEquivalentTo( result );

                other.ReferencingObjects.Should()
                    .BeSequentiallyEqualTo(
                        SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( result ), other ) );

                schema.Database.Schemas.Default.ReferencingObjects.Should()
                    .BeSequentiallyEqualTo(
                        SqlObjectBuilderReference.Create(
                            SqlObjectBuilderReferenceSource.Create( result ),
                            schema.Database.Schemas.Default ) );
            }
        }

        [Fact]
        public void CreateView_ShouldThrowSqlObjectBuilderException_WhenSourceIsNotValid()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var sut = schema.Objects;

            var source = SqlNode.RawQuery( "SELECT * FROM foo WHERE a > @a", SqlNode.Parameter<int>( "a" ) );
            var action = Lambda.Of( () => sut.CreateView( "V", source ) );

            action.Should()
                .ThrowExactly<SqlObjectBuilderException>()
                .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
        }

        [Fact]
        public void CreateView_ShouldThrowSqlObjectBuilderException_WhenSchemaIsRemoved()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var sut = schema.Objects;
            schema.Remove();

            var action = Lambda.Of( () => sut.CreateView( "V", SqlNode.RawQuery( "SELECT * FROM bar" ) ) );

            action.Should()
                .ThrowExactly<SqlObjectBuilderException>()
                .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
        }

        [Fact]
        public void CreateView_ShouldThrowSqlObjectBuilderException_WhenObjectNameAlreadyExists()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var sut = schema.Objects;
            sut.CreateTable( "T" );

            var action = Lambda.Of( () => sut.CreateView( "T", SqlNode.RawQuery( "SELECT * FROM bar" ) ) );

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
        public void CreateView_ShouldThrowSqlObjectBuilderException_WhenNameIsInvalid(string name)
        {
            var db = MySqlDatabaseBuilderMock.Create();
            var sut = db.Schemas.Create( "foo" ).Objects;

            var action = Lambda.Of( () => sut.CreateView( name, SqlNode.RawQuery( "SELECT * FROM foo" ) ) );

            action.Should()
                .ThrowExactly<SqlObjectBuilderException>()
                .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
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
            var db = MySqlDatabaseBuilderMock.Create();
            var sut = db.Schemas.Create( "foo" ).Objects;
            var t = sut.CreateTable( "T" );
            var c = t.Columns.Create( "C" );
            t.Constraints.SetPrimaryKey( c.Asc() ).SetName( "PK" ).Index.SetName( "IX" );
            t.Constraints.CreateCheck( t.Node["C"] != null ).SetName( "CHK" );
            sut.CreateView( "V", SqlNode.RawQuery( "SELECT * FROM foo" ) );

            var result = sut.Contains( name );

            result.Should().Be( expected );
        }

        [Fact]
        public void Get_ShouldReturnExistingObject()
        {
            var name = Fixture.Create<string>();
            var db = MySqlDatabaseBuilderMock.Create();
            var sut = db.Schemas.Create( "foo" ).Objects;
            var expected = sut.CreateTable( name );

            var result = sut.Get( name );

            result.Should().BeSameAs( expected );
        }

        [Fact]
        public void Get_ShouldThrowKeyNotFoundException_WhenObjectDoesNotExist()
        {
            var name = Fixture.Create<string>();
            var db = MySqlDatabaseBuilderMock.Create();
            var sut = db.Schemas.Create( "foo" ).Objects;

            var action = Lambda.Of( () => sut.Get( name ) );

            action.Should().ThrowExactly<KeyNotFoundException>();
        }

        [Fact]
        public void TryGet_ShouldReturnExistingObject()
        {
            var name = Fixture.Create<string>();
            var db = MySqlDatabaseBuilderMock.Create();
            var sut = db.Schemas.Create( "foo" ).Objects;
            var expected = sut.CreateTable( name );

            var result = sut.TryGet( name );

            result.Should().BeSameAs( expected );
        }

        [Fact]
        public void TryGet_ShouldReturnNull_WhenObjectDoesNotExist()
        {
            var name = Fixture.Create<string>();
            var db = MySqlDatabaseBuilderMock.Create();
            var sut = db.Schemas.Create( "foo" ).Objects;

            var result = sut.TryGet( name );

            result.Should().BeNull();
        }

        [Fact]
        public void GetTable_ShouldReturnExistingTable()
        {
            var name = Fixture.Create<string>();
            var db = MySqlDatabaseBuilderMock.Create();
            var sut = db.Schemas.Create( "foo" ).Objects;
            var expected = sut.CreateTable( name );

            var result = sut.GetTable( name );

            result.Should().BeSameAs( expected );
        }

        [Fact]
        public void GetTable_ShouldThrowKeyNotFoundException_WhenObjectDoesNotExist()
        {
            var name = Fixture.Create<string>();
            var db = MySqlDatabaseBuilderMock.Create();
            var sut = db.Schemas.Create( "foo" ).Objects;

            var action = Lambda.Of( () => sut.GetTable( name ) );

            action.Should().ThrowExactly<KeyNotFoundException>();
        }

        [Fact]
        public void GetTable_ShouldThrowSqlObjectCastException_WhenObjectExistsButNotAsTable()
        {
            var name = Fixture.Create<string>();
            var db = MySqlDatabaseBuilderMock.Create();
            var sut = db.Schemas.Create( "foo" ).Objects;
            var t = sut.CreateTable( "T" );
            var c = t.Columns.Create( "C" );
            t.Constraints.SetPrimaryKey( c.Asc() ).SetName( name );

            var action = Lambda.Of( () => sut.GetTable( name ) );

            action.Should()
                .ThrowExactly<SqlObjectCastException>()
                .AndMatch(
                    e => e.Dialect == MySqlDialect.Instance &&
                        e.Expected == typeof( SqlTableBuilder ) &&
                        e.Actual == typeof( MySqlPrimaryKeyBuilder ) );
        }

        [Fact]
        public void TryGetTable_ShouldReturnExistingTable()
        {
            var name = Fixture.Create<string>();
            var db = MySqlDatabaseBuilderMock.Create();
            var sut = db.Schemas.Create( "foo" ).Objects;
            var expected = sut.CreateTable( name );

            var result = sut.TryGetTable( name );

            result.Should().BeSameAs( expected );
        }

        [Fact]
        public void TryGetTable_ShouldReturnNull_WhenObjectDoesNotExist()
        {
            var name = Fixture.Create<string>();
            var db = MySqlDatabaseBuilderMock.Create();
            var sut = db.Schemas.Create( "foo" ).Objects;

            var result = sut.TryGetTable( name );

            result.Should().BeNull();
        }

        [Fact]
        public void TryGetTable_ShouldReturnNull_WhenObjectExistsButNotAsTable()
        {
            var name = Fixture.Create<string>();
            var db = MySqlDatabaseBuilderMock.Create();
            var sut = db.Schemas.Create( "foo" ).Objects;
            var t = sut.CreateTable( "T" );
            var c = t.Columns.Create( "C" );
            t.Constraints.SetPrimaryKey( c.Asc() ).SetName( name );

            var result = sut.TryGetTable( name );

            result.Should().BeNull();
        }

        [Fact]
        public void GetIndex_ShouldReturnExistingIndex()
        {
            var name = Fixture.Create<string>();
            var db = MySqlDatabaseBuilderMock.Create();
            var sut = db.Schemas.Create( "foo" ).Objects;
            var t = sut.CreateTable( "T" );
            var c = t.Columns.Create( "C" );
            var expected = t.Constraints.CreateIndex( c.Asc() ).SetName( name );

            var result = sut.GetIndex( name );

            result.Should().BeSameAs( expected );
        }

        [Fact]
        public void GetIndex_ShouldThrowKeyNotFoundException_WhenObjectDoesNotExist()
        {
            var name = Fixture.Create<string>();
            var db = MySqlDatabaseBuilderMock.Create();
            var sut = db.Schemas.Create( "foo" ).Objects;

            var action = Lambda.Of( () => sut.GetIndex( name ) );

            action.Should().ThrowExactly<KeyNotFoundException>();
        }

        [Fact]
        public void GetIndex_ShouldThrowSqlObjectCastException_WhenObjectExistsButNotAsIndex()
        {
            var name = Fixture.Create<string>();
            var db = MySqlDatabaseBuilderMock.Create();
            var sut = db.Schemas.Create( "foo" ).Objects;
            sut.CreateTable( name );

            var action = Lambda.Of( () => sut.GetIndex( name ) );

            action.Should()
                .ThrowExactly<SqlObjectCastException>()
                .AndMatch(
                    e => e.Dialect == MySqlDialect.Instance &&
                        e.Expected == typeof( SqlIndexBuilder ) &&
                        e.Actual == typeof( MySqlTableBuilder ) );
        }

        [Fact]
        public void TryGetIndex_ShouldReturnExistingIndex()
        {
            var name = Fixture.Create<string>();
            var db = MySqlDatabaseBuilderMock.Create();
            var sut = db.Schemas.Create( "foo" ).Objects;
            var t = sut.CreateTable( "T" );
            var c = t.Columns.Create( "C" );
            var expected = t.Constraints.CreateIndex( c.Asc() ).SetName( name );

            var result = sut.TryGetIndex( name );

            result.Should().BeSameAs( expected );
        }

        [Fact]
        public void TryGetIndex_ShouldReturnFNull_WhenObjectDoesNotExist()
        {
            var name = Fixture.Create<string>();
            var db = MySqlDatabaseBuilderMock.Create();
            var sut = db.Schemas.Create( "foo" ).Objects;

            var result = sut.TryGetIndex( name );

            result.Should().BeNull();
        }

        [Fact]
        public void TryGetIndex_ShouldReturnNull_WhenObjectExistsButNotAsIndex()
        {
            var name = Fixture.Create<string>();
            var db = MySqlDatabaseBuilderMock.Create();
            var sut = db.Schemas.Create( "foo" ).Objects;
            sut.CreateTable( name );

            var result = sut.TryGetIndex( name );

            result.Should().BeNull();
        }

        [Fact]
        public void GetPrimaryKey_ShouldReturnExistingPrimaryKey()
        {
            var name = Fixture.Create<string>();
            var db = MySqlDatabaseBuilderMock.Create();
            var sut = db.Schemas.Create( "foo" ).Objects;
            var t = sut.CreateTable( "T" );
            var c = t.Columns.Create( "C" );
            var expected = t.Constraints.SetPrimaryKey( c.Asc() ).SetName( name );

            var result = sut.GetPrimaryKey( name );

            result.Should().BeSameAs( expected );
        }

        [Fact]
        public void GetPrimaryKey_ShouldThrowKeyNotFoundException_WhenObjectDoesNotExist()
        {
            var name = Fixture.Create<string>();
            var db = MySqlDatabaseBuilderMock.Create();
            var sut = db.Schemas.Create( "foo" ).Objects;

            var action = Lambda.Of( () => sut.GetPrimaryKey( name ) );

            action.Should().ThrowExactly<KeyNotFoundException>();
        }

        [Fact]
        public void GetPrimaryKey_ShouldThrowSqlObjectCastException_WhenObjectExistsButNotAsPrimaryKey()
        {
            var name = Fixture.Create<string>();
            var db = MySqlDatabaseBuilderMock.Create();
            var sut = db.Schemas.Create( "foo" ).Objects;
            sut.CreateTable( name );

            var action = Lambda.Of( () => sut.GetPrimaryKey( name ) );

            action.Should()
                .ThrowExactly<SqlObjectCastException>()
                .AndMatch(
                    e => e.Dialect == MySqlDialect.Instance &&
                        e.Expected == typeof( SqlPrimaryKeyBuilder ) &&
                        e.Actual == typeof( MySqlTableBuilder ) );
        }

        [Fact]
        public void TryGetPrimaryKey_ShouldReturnExistingPrimaryKey()
        {
            var name = Fixture.Create<string>();
            var db = MySqlDatabaseBuilderMock.Create();
            var sut = db.Schemas.Create( "foo" ).Objects;
            var t = sut.CreateTable( "T" );
            var c = t.Columns.Create( "C" );
            var expected = t.Constraints.SetPrimaryKey( c.Asc() ).SetName( name );

            var result = sut.TryGetPrimaryKey( name );

            result.Should().BeSameAs( expected );
        }

        [Fact]
        public void TryGetPrimaryKey_ShouldReturnNull_WhenObjectDoesNotExist()
        {
            var name = Fixture.Create<string>();
            var db = MySqlDatabaseBuilderMock.Create();
            var sut = db.Schemas.Create( "foo" ).Objects;

            var result = sut.TryGetPrimaryKey( name );

            result.Should().BeNull();
        }

        [Fact]
        public void TryGetPrimaryKey_ShouldReturnNull_WhenObjectExistsButNotAsPrimaryKey()
        {
            var name = Fixture.Create<string>();
            var db = MySqlDatabaseBuilderMock.Create();
            var sut = db.Schemas.Create( "foo" ).Objects;
            sut.CreateTable( name );

            var result = sut.TryGetPrimaryKey( name );

            result.Should().BeNull();
        }

        [Fact]
        public void GetForeignKey_ShouldReturnExistingForeignKey()
        {
            var name = Fixture.Create<string>();
            var db = MySqlDatabaseBuilderMock.Create();
            var sut = db.Schemas.Create( "foo" ).Objects;
            var t = sut.CreateTable( "T" );
            var c = t.Columns.Create( "C" );
            var d = t.Columns.Create( "D" ).MarkAsNullable();
            var pk = t.Constraints.SetPrimaryKey( c.Asc() );
            var ix = t.Constraints.CreateIndex( d.Asc() );
            var expected = t.Constraints.CreateForeignKey( ix, pk.Index ).SetName( name );

            var result = sut.GetForeignKey( name );

            result.Should().BeSameAs( expected );
        }

        [Fact]
        public void GetForeignKey_ShouldThrowKeyNotFoundException_WhenObjectDoesNotExist()
        {
            var name = Fixture.Create<string>();
            var db = MySqlDatabaseBuilderMock.Create();
            var sut = db.Schemas.Create( "foo" ).Objects;

            var action = Lambda.Of( () => sut.GetForeignKey( name ) );

            action.Should().ThrowExactly<KeyNotFoundException>();
        }

        [Fact]
        public void GetForeignKey_ShouldThrowSqlObjectCastException_WhenObjectExistsButNotAsForeignKey()
        {
            var name = Fixture.Create<string>();
            var db = MySqlDatabaseBuilderMock.Create();
            var sut = db.Schemas.Create( "foo" ).Objects;
            sut.CreateTable( name );

            var action = Lambda.Of( () => sut.GetForeignKey( name ) );

            action.Should()
                .ThrowExactly<SqlObjectCastException>()
                .AndMatch(
                    e => e.Dialect == MySqlDialect.Instance &&
                        e.Expected == typeof( SqlForeignKeyBuilder ) &&
                        e.Actual == typeof( MySqlTableBuilder ) );
        }

        [Fact]
        public void TryGetForeignKey_ShouldReturnExistingForeignKey()
        {
            var name = Fixture.Create<string>();
            var db = MySqlDatabaseBuilderMock.Create();
            var sut = db.Schemas.Create( "foo" ).Objects;
            var t = sut.CreateTable( "T" );
            var c = t.Columns.Create( "C" );
            var d = t.Columns.Create( "D" ).MarkAsNullable();
            var pk = t.Constraints.SetPrimaryKey( c.Asc() );
            var ix = t.Constraints.CreateIndex( d.Asc() );
            var expected = t.Constraints.CreateForeignKey( ix, pk.Index ).SetName( name );

            var result = sut.TryGetForeignKey( name );

            result.Should().BeSameAs( expected );
        }

        [Fact]
        public void TryGetForeignKey_ShouldReturnNull_WhenObjectDoesNotExist()
        {
            var name = Fixture.Create<string>();
            var db = MySqlDatabaseBuilderMock.Create();
            var sut = db.Schemas.Create( "foo" ).Objects;

            var result = sut.TryGetForeignKey( name );

            result.Should().BeNull();
        }

        [Fact]
        public void TryGetForeignKey_ShouldReturnNull_WhenObjectExistsButNotAsForeignKey()
        {
            var name = Fixture.Create<string>();
            var db = MySqlDatabaseBuilderMock.Create();
            var sut = db.Schemas.Create( "foo" ).Objects;
            sut.CreateTable( name );

            var result = sut.TryGetForeignKey( name );

            result.Should().BeNull();
        }

        [Fact]
        public void GetView_ShouldReturnExistingView()
        {
            var name = Fixture.Create<string>();
            var db = MySqlDatabaseBuilderMock.Create();
            var sut = db.Schemas.Create( "foo" ).Objects;
            var expected = sut.CreateView( name, SqlNode.RawQuery( "SELECT * FROM foo" ) );

            var result = sut.GetView( name );

            result.Should().BeSameAs( expected );
        }

        [Fact]
        public void GetView_ShouldThrowKeyNotFoundException_WhenObjectDoesNotExist()
        {
            var name = Fixture.Create<string>();
            var db = MySqlDatabaseBuilderMock.Create();
            var sut = db.Schemas.Create( "foo" ).Objects;

            var action = Lambda.Of( () => sut.GetView( name ) );

            action.Should().ThrowExactly<KeyNotFoundException>();
        }

        [Fact]
        public void GetView_ShouldThrowSqlObjectCastException_WhenObjectExistsButNotAsView()
        {
            var name = Fixture.Create<string>();
            var db = MySqlDatabaseBuilderMock.Create();
            var sut = db.Schemas.Create( "foo" ).Objects;
            sut.CreateTable( name );

            var action = Lambda.Of( () => sut.GetView( name ) );

            action.Should()
                .ThrowExactly<SqlObjectCastException>()
                .AndMatch(
                    e => e.Dialect == MySqlDialect.Instance &&
                        e.Expected == typeof( SqlViewBuilder ) &&
                        e.Actual == typeof( MySqlTableBuilder ) );
        }

        [Fact]
        public void TryGetView_ShouldReturnExistingView()
        {
            var name = Fixture.Create<string>();
            var db = MySqlDatabaseBuilderMock.Create();
            var sut = db.Schemas.Create( "foo" ).Objects;
            var expected = sut.CreateView( name, SqlNode.RawQuery( "SELECT * FROM foo" ) );

            var result = sut.TryGetView( name );

            result.Should().BeSameAs( expected );
        }

        [Fact]
        public void TryGetView_ShouldReturnNull_WhenObjectDoesNotExist()
        {
            var name = Fixture.Create<string>();
            var db = MySqlDatabaseBuilderMock.Create();
            var sut = db.Schemas.Create( "foo" ).Objects;

            var result = sut.TryGetView( name );

            result.Should().BeNull();
        }

        [Fact]
        public void TryGetView_ShouldReturnNull_WhenObjectExistsButNotAsView()
        {
            var name = Fixture.Create<string>();
            var db = MySqlDatabaseBuilderMock.Create();
            var sut = db.Schemas.Create( "foo" ).Objects;
            sut.CreateTable( name );

            var result = sut.TryGetView( name );

            result.Should().BeNull();
        }

        [Fact]
        public void GetCheck_ShouldReturnExistingCheck()
        {
            var name = Fixture.Create<string>();
            var db = MySqlDatabaseBuilderMock.Create();
            var sut = db.Schemas.Create( "foo" ).Objects;
            var t = sut.CreateTable( "T" );
            var c = t.Columns.Create( "C" );
            var expected = t.Constraints.CreateCheck( c.Node != null ).SetName( name );

            var result = sut.GetCheck( name );

            result.Should().BeSameAs( expected );
        }

        [Fact]
        public void GetCheck_ShouldThrowKeyNotFoundException_WhenObjectDoesNotExist()
        {
            var name = Fixture.Create<string>();
            var db = MySqlDatabaseBuilderMock.Create();
            var sut = db.Schemas.Create( "foo" ).Objects;

            var action = Lambda.Of( () => sut.GetCheck( name ) );

            action.Should().ThrowExactly<KeyNotFoundException>();
        }

        [Fact]
        public void GetCheck_ShouldThrowSqlObjectCastException_WhenObjectExistsButNotAsCheck()
        {
            var name = Fixture.Create<string>();
            var db = MySqlDatabaseBuilderMock.Create();
            var sut = db.Schemas.Create( "foo" ).Objects;
            sut.CreateTable( name );

            var action = Lambda.Of( () => sut.GetCheck( name ) );

            action.Should()
                .ThrowExactly<SqlObjectCastException>()
                .AndMatch(
                    e => e.Dialect == MySqlDialect.Instance &&
                        e.Expected == typeof( SqlCheckBuilder ) &&
                        e.Actual == typeof( MySqlTableBuilder ) );
        }

        [Fact]
        public void TryGetCheck_ShouldReturnExistingCheck()
        {
            var name = Fixture.Create<string>();
            var db = MySqlDatabaseBuilderMock.Create();
            var sut = db.Schemas.Create( "foo" ).Objects;
            var t = sut.CreateTable( "T" );
            var c = t.Columns.Create( "C" );
            var expected = t.Constraints.CreateCheck( c.Node != null ).SetName( name );

            var result = sut.TryGetCheck( name );

            result.Should().BeSameAs( expected );
        }

        [Fact]
        public void TryGetCheck_ShouldReturnNull_WhenObjectDoesNotExist()
        {
            var name = Fixture.Create<string>();
            var db = MySqlDatabaseBuilderMock.Create();
            var sut = db.Schemas.Create( "foo" ).Objects;

            var result = sut.TryGetCheck( name );

            result.Should().BeNull();
        }

        [Fact]
        public void TryGetCheck_ShouldReturnNull_WhenObjectExistsButNotAsCheck()
        {
            var name = Fixture.Create<string>();
            var db = MySqlDatabaseBuilderMock.Create();
            var sut = db.Schemas.Create( "foo" ).Objects;
            sut.CreateTable( name );

            var result = sut.TryGetCheck( name );

            result.Should().BeNull();
        }

        [Fact]
        public void Remove_ShouldRemoveExistingTable()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
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

            using ( new AssertionScope() )
            {
                result.Should().BeTrue();
                sut.TryGet( table.Name ).Should().BeNull();
                sut.TryGet( pk.Name ).Should().BeNull();
                sut.TryGet( pk.Index.Name ).Should().BeNull();
                sut.TryGet( ix.Name ).Should().BeNull();
                sut.TryGet( selfFk.Name ).Should().BeNull();
                sut.TryGet( externalFk.Name ).Should().BeNull();
                sut.TryGet( chk.Name ).Should().BeNull();
                sut.Count.Should().Be( 3 );

                table.IsRemoved.Should().BeTrue();
                table.ReferencingObjects.Should().BeEmpty();
                table.Columns.Should().BeEmpty();
                table.Constraints.Should().BeEmpty();
                table.Constraints.TryGetPrimaryKey().Should().BeNull();
                c1.IsRemoved.Should().BeTrue();
                c1.ReferencingObjects.Should().BeEmpty();
                c2.IsRemoved.Should().BeTrue();
                c2.ReferencingObjects.Should().BeEmpty();
                pk.IsRemoved.Should().BeTrue();
                pk.ReferencingObjects.Should().BeEmpty();
                pk.Index.IsRemoved.Should().BeTrue();
                pk.Index.ReferencingObjects.Should().BeEmpty();
                pk.Index.Columns.Should().BeEmpty();
                pk.Index.PrimaryKey.Should().BeNull();
                ix.IsRemoved.Should().BeTrue();
                ix.ReferencingObjects.Should().BeEmpty();
                ix.Columns.Should().BeEmpty();
                selfFk.IsRemoved.Should().BeTrue();
                selfFk.ReferencingObjects.Should().BeEmpty();
                externalFk.IsRemoved.Should().BeTrue();
                externalFk.ReferencingObjects.Should().BeEmpty();
                chk.IsRemoved.Should().BeTrue();
                chk.ReferencingObjects.Should().BeEmpty();
                chk.ReferencedColumns.Should().BeEmpty();

                otherPk.Index.ReferencingObjects.Should().BeEmpty();
                otherTable.ReferencingObjects.Should().BeEmpty();
            }
        }

        [Fact]
        public void Remove_ShouldReturnFalse_WhenTableToRemoveIsReferencedByAnyExternalForeignKey()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var sut = schema.Objects;
            var table = sut.CreateTable( "T" );
            var pk = table.Constraints.SetPrimaryKey( table.Columns.Create( "C" ).Asc() );

            var otherTable = sut.CreateTable( "U" );
            var otherPk = otherTable.Constraints.SetPrimaryKey( otherTable.Columns.Create( "D" ).Asc() );
            otherTable.Constraints.CreateForeignKey( otherPk.Index, pk.Index );

            var result = sut.Remove( table.Name );

            using ( new AssertionScope() )
            {
                result.Should().BeFalse();
                table.IsRemoved.Should().BeFalse();
                sut.Count.Should().Be( 7 );
                sut.TryGet( table.Name ).Should().BeSameAs( table );
            }
        }

        [Fact]
        public void Remove_ShouldReturnFalse_WhenTableToRemoveIsReferencedByAnyView()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var sut = schema.Objects;
            var table = sut.CreateTable( "T" );
            table.Constraints.SetPrimaryKey( table.Columns.Create( "C" ).Asc() );
            sut.CreateView( "V", table.Node.ToDataSource().Select( s => new[] { s.GetAll() } ) );

            var result = sut.Remove( table.Name );

            using ( new AssertionScope() )
            {
                result.Should().BeFalse();
                table.IsRemoved.Should().BeFalse();
                sut.Count.Should().Be( 4 );
                sut.TryGet( table.Name ).Should().BeSameAs( table );
            }
        }

        [Fact]
        public void Remove_ShouldRemoveExistingView()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var sut = schema.Objects;
            var view = sut.CreateView( "V", SqlNode.RawQuery( "SELECT * FROM bar" ) );

            var result = sut.Remove( view.Name );

            using ( new AssertionScope() )
            {
                result.Should().BeTrue();
                sut.Count.Should().Be( 0 );
                sut.TryGet( view.Name ).Should().BeNull();
                view.IsRemoved.Should().BeTrue();
            }
        }

        [Fact]
        public void Remove_ShouldReturnFalse_WhenViewToRemoveIsReferencedByAnotherView()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var sut = schema.Objects;
            var view = sut.CreateView( "V", SqlNode.RawQuery( "SELECT * FROM bar" ) );
            sut.CreateView( "W", view.ToRecordSet().ToDataSource().Select( s => new[] { s.GetAll() } ) );

            var result = sut.Remove( view.Name );

            using ( new AssertionScope() )
            {
                result.Should().BeFalse();
                view.IsRemoved.Should().BeFalse();
                sut.Count.Should().Be( 2 );
                sut.TryGet( view.Name ).Should().BeSameAs( view );
            }
        }

        [Fact]
        public void Remove_ShouldRemoveExistingIndex()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var sut = schema.Objects;
            var table = sut.CreateTable( "T" );
            var c1 = table.Columns.Create( "C1" ).Asc();
            var c2 = table.Columns.Create( "C2" ).Desc();
            var index = table.Constraints.CreateIndex( c1, c2 );

            var result = sut.Remove( index.Name );

            using ( new AssertionScope() )
            {
                result.Should().BeTrue();
                sut.TryGet( index.Name ).Should().BeNull();
                index.IsRemoved.Should().BeTrue();
                table.Constraints.TryGet( index.Name ).Should().BeNull();
                c1.Column.ReferencingObjects.Should().BeEmpty();
                c2.Column.ReferencingObjects.Should().BeEmpty();
            }
        }

        [Fact]
        public void Remove_ShouldReturnFalse_WhenIndexHasOriginatingForeignKey()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var sut = schema.Objects;
            var table = schema.Objects.CreateTable( "T" );
            var pk = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
            var index = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );
            table.Constraints.CreateForeignKey( index, pk.Index );

            var result = sut.Remove( index.Name );

            using ( new AssertionScope() )
            {
                result.Should().BeFalse();
                index.IsRemoved.Should().BeFalse();
                sut.Count.Should().Be( 5 );
            }
        }

        [Fact]
        public void Remove_ShouldReturnFalse_WhenIndexHasReferencingForeignKey()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var sut = schema.Objects;
            var table = schema.Objects.CreateTable( "T" );
            var pk = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
            var index = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );
            table.Constraints.CreateForeignKey( index, pk.Index );

            var result = sut.Remove( pk.Index.Name );

            using ( new AssertionScope() )
            {
                result.Should().BeFalse();
                pk.Index.IsRemoved.Should().BeFalse();
                sut.Count.Should().Be( 5 );
            }
        }

        [Fact]
        public void Remove_ShouldRemoveExistingPrimaryKey()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var sut = schema.Objects;
            var table = schema.Objects.CreateTable( "T" );
            var column = table.Columns.Create( "C" );
            var pk = table.Constraints.SetPrimaryKey( column.Asc() );

            var result = sut.Remove( pk.Name );

            using ( new AssertionScope() )
            {
                result.Should().BeTrue();
                sut.TryGet( pk.Name ).Should().BeNull();
                sut.TryGet( pk.Index.Name ).Should().BeNull();
                pk.IsRemoved.Should().BeTrue();
                pk.Index.IsRemoved.Should().BeTrue();
                table.Constraints.TryGet( pk.Name ).Should().BeNull();
                table.Constraints.TryGet( pk.Index.Name ).Should().BeNull();
                table.Constraints.TryGetPrimaryKey().Should().BeNull();
                column.ReferencingObjects.Should().BeEmpty();
            }
        }

        [Fact]
        public void Remove_ShouldReturnFalse_WhenPrimaryKeyUnderlyingIndexHasOriginatingForeignKey()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var sut = schema.Objects;
            var table = schema.Objects.CreateTable( "T" );
            var pk = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
            var index = table.Constraints.CreateUniqueIndex( table.Columns.Create( "C2" ).Asc() );
            table.Constraints.CreateForeignKey( pk.Index, index );

            var result = sut.Remove( pk.Name );

            using ( new AssertionScope() )
            {
                result.Should().BeFalse();
                index.IsRemoved.Should().BeFalse();
                sut.Count.Should().Be( 5 );
            }
        }

        [Fact]
        public void Remove_ShouldReturnFalse_WhenPrimaryKeyUnderlyingIndexHasReferencingForeignKey()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var sut = schema.Objects;
            var table = schema.Objects.CreateTable( "T" );
            var pk = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
            var index = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );
            table.Constraints.CreateForeignKey( index, pk.Index );

            var result = sut.Remove( pk.Name );

            using ( new AssertionScope() )
            {
                result.Should().BeFalse();
                pk.Index.IsRemoved.Should().BeFalse();
                sut.Count.Should().Be( 5 );
            }
        }

        [Fact]
        public void Remove_ShouldRemoveExistingForeignKey()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var sut = schema.Objects;
            var table = schema.Objects.CreateTable( "T" );
            var ix1 = table.Constraints.CreateIndex( table.Columns.Create( "C1" ).Asc() );
            var ix2 = table.Constraints.CreateUniqueIndex( table.Columns.Create( "C2" ).Asc() );
            var fk = table.Constraints.CreateForeignKey( ix1, ix2 );

            var result = sut.Remove( fk.Name );

            using ( new AssertionScope() )
            {
                result.Should().BeTrue();
                sut.Count.Should().Be( 3 );
                sut.TryGet( fk.Name ).Should().BeNull();
                fk.IsRemoved.Should().BeTrue();
                table.Constraints.TryGet( fk.Name ).Should().BeNull();
                ix1.ReferencingObjects.Should().BeEmpty();
                ix2.ReferencingObjects.Should().BeEmpty();
            }
        }

        [Fact]
        public void Remove_ShouldRemoveExistingCheck()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var sut = schema.Objects;
            var table = schema.Objects.CreateTable( "T" );
            var c = table.Columns.Create( "C" );
            var check = table.Constraints.CreateCheck( c.Node > SqlNode.Literal( 0 ) );

            var result = sut.Remove( check.Name );

            using ( new AssertionScope() )
            {
                result.Should().BeTrue();
                sut.Count.Should().Be( 1 );
                sut.TryGet( check.Name ).Should().BeNull();
                check.IsRemoved.Should().BeTrue();
                table.Constraints.TryGet( check.Name ).Should().BeNull();
                c.ReferencingObjects.Should().BeEmpty();
            }
        }

        [Fact]
        public void Remove_ShouldReturnFalse_WhenObjectDoesNotExist()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var sut = schema.Objects;

            var result = sut.Remove( "PK" );

            result.Should().BeFalse();
        }
    }
}
