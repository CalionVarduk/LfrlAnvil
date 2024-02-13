using System.Collections.Generic;
using LfrlAnvil.Functional;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Extensions;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.Sqlite.Exceptions;
using LfrlAnvil.Sqlite.Extensions;
using LfrlAnvil.Sqlite.Objects.Builders;
using LfrlAnvil.Sqlite.Tests.Helpers;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.Sqlite.Tests.ObjectsTests.BuildersTests;

public partial class SqliteSchemaBuilderTests
{
    public class Objects : TestsBase
    {
        [Fact]
        public void CreateTable_ShouldCreateNewTable()
        {
            var name = Fixture.Create<string>();
            var db = SqliteDatabaseBuilderMock.Create();
            var sut = db.Schemas.Create( "foo" ).Objects;

            var result = ((ISqlObjectBuilderCollection)sut).CreateTable( name );

            using ( new AssertionScope() )
            {
                result.Schema.Should().BeSameAs( sut.Schema );
                result.Name.Should().Be( name );
                result.Constraints.TryGetPrimaryKey().Should().BeNull();
                result.Database.Should().BeSameAs( db );
                result.Type.Should().Be( SqlObjectType.Table );
                sut.Count.Should().Be( 1 );
                sut.Should().BeEquivalentTo( result );

                result.Columns.Should().BeEmpty();
                result.Columns.Table.Should().BeSameAs( result );
                result.Columns.DefaultTypeDefinition.Should().BeSameAs( db.TypeDefinitions.GetByType<object>() );
                result.Columns.Count.Should().Be( 0 );

                result.Constraints.Should().BeEmpty();
                result.Constraints.Table.Should().BeSameAs( result );
                result.Constraints.Count.Should().Be( 0 );

                result.Info.Should().Be( SqlRecordSetInfo.Create( "foo", name ) );
                result.Node.Table.Should().BeSameAs( result );
                result.Node.Info.Should().Be( result.Info );
                result.Node.Alias.Should().BeNull();
                result.Node.Identifier.Should().Be( result.Info.Identifier );
                result.Node.IsOptional.Should().BeFalse();
            }
        }

        [Theory]
        [InlineData( "" )]
        [InlineData( " " )]
        [InlineData( "\"" )]
        [InlineData( "'" )]
        [InlineData( "f\"oo" )]
        public void CreateTable_ShouldThrowSqliteObjectBuilderException_WhenNameIsInvalid(string name)
        {
            var db = SqliteDatabaseBuilderMock.Create();
            var sut = db.Schemas.Create( "foo" ).Objects;

            var action = Lambda.Of( () => sut.CreateTable( name ) );

            action.Should()
                .ThrowExactly<SqliteObjectBuilderException>()
                .AndMatch( e => e.Dialect == SqliteDialect.Instance && e.Errors.Count == 1 );
        }

        [Fact]
        public void CreateTable_ShouldThrowSqliteObjectBuilderException_WhenTableWithNameAlreadyExists()
        {
            var name = Fixture.Create<string>();
            var db = SqliteDatabaseBuilderMock.Create();
            var sut = db.Schemas.Create( "foo" ).Objects;
            sut.CreateTable( name );

            var action = Lambda.Of( () => sut.CreateTable( name ) );

            action.Should()
                .ThrowExactly<SqliteObjectBuilderException>()
                .AndMatch( e => e.Dialect == SqliteDialect.Instance && e.Errors.Count == 1 );
        }

        [Fact]
        public void CreateTable_ShouldThrowSqliteObjectBuilderException_WhenSchemaIsRemoved()
        {
            var name = Fixture.Create<string>();
            var db = SqliteDatabaseBuilderMock.Create();
            var sut = db.Schemas.Create( "foo" ).Objects;
            sut.Schema.Remove();

            var action = Lambda.Of( () => sut.CreateTable( name ) );

            action.Should()
                .ThrowExactly<SqliteObjectBuilderException>()
                .AndMatch( e => e.Dialect == SqliteDialect.Instance && e.Errors.Count == 1 );
        }

        [Fact]
        public void GetOrCreateTable_ShouldCreateNewTable()
        {
            var name = Fixture.Create<string>();
            var db = SqliteDatabaseBuilderMock.Create();
            var sut = db.Schemas.Create( "foo" ).Objects;

            var result = ((ISqlObjectBuilderCollection)sut).GetOrCreateTable( name );

            using ( new AssertionScope() )
            {
                result.Schema.Should().BeSameAs( sut.Schema );
                result.Name.Should().Be( name );
                result.Constraints.TryGetPrimaryKey().Should().BeNull();
                result.Database.Should().BeSameAs( db );
                result.Type.Should().Be( SqlObjectType.Table );
                sut.Count.Should().Be( 1 );
                sut.Should().BeEquivalentTo( result );

                result.Columns.Should().BeEmpty();
                result.Columns.Table.Should().BeSameAs( result );
                result.Columns.DefaultTypeDefinition.Should().BeSameAs( db.TypeDefinitions.GetByType<object>() );
                result.Columns.Count.Should().Be( 0 );

                result.Constraints.Should().BeEmpty();
                result.Constraints.Table.Should().BeSameAs( result );
                result.Constraints.Count.Should().Be( 0 );
            }
        }

        [Theory]
        [InlineData( "" )]
        [InlineData( " " )]
        [InlineData( "\"" )]
        [InlineData( "f\"oo" )]
        public void GetOrCreateTable_ShouldThrowSqliteObjectBuilderException_WhenNameIsInvalid(string name)
        {
            var db = SqliteDatabaseBuilderMock.Create();
            var sut = db.Schemas.Create( "foo" ).Objects;

            var action = Lambda.Of( () => sut.GetOrCreateTable( name ) );

            action.Should()
                .ThrowExactly<SqliteObjectBuilderException>()
                .AndMatch( e => e.Dialect == SqliteDialect.Instance && e.Errors.Count == 1 );
        }

        [Fact]
        public void GetOrCreateTable_ShouldReturnExistingTable_WhenTableWithNameAlreadyExists()
        {
            var name = Fixture.Create<string>();
            var db = SqliteDatabaseBuilderMock.Create();
            var sut = db.Schemas.Create( "foo" ).Objects;
            var expected = sut.CreateTable( name );

            var result = sut.GetOrCreateTable( name );

            using ( new AssertionScope() )
            {
                result.Should().BeSameAs( expected );
                sut.Count.Should().Be( 1 );
            }
        }

        [Fact]
        public void GetOrCreateTable_ShouldThrowSqlObjectCastException_WhenNonTableObjectWithNameAlreadyExists()
        {
            var name = Fixture.Create<string>();
            var db = SqliteDatabaseBuilderMock.Create();
            var sut = db.Schemas.Create( "foo" ).Objects;
            var t = sut.CreateTable( "T" );
            var c = t.Columns.Create( "C" );
            t.Constraints.SetPrimaryKey( c.Asc() ).SetName( name );

            var action = Lambda.Of( () => sut.GetOrCreateTable( name ) );

            action.Should()
                .ThrowExactly<SqlObjectCastException>()
                .AndMatch(
                    e => e.Dialect == SqliteDialect.Instance &&
                        e.Expected == typeof( SqliteTableBuilder ) &&
                        e.Actual == typeof( SqlitePrimaryKeyBuilder ) );
        }

        [Fact]
        public void GetOrCreateTable_ShouldThrowSqliteObjectBuilderException_WhenSchemaIsRemoved()
        {
            var name = Fixture.Create<string>();
            var db = SqliteDatabaseBuilderMock.Create();
            var sut = db.Schemas.Create( "foo" ).Objects;
            sut.Schema.Remove();

            var action = Lambda.Of( () => sut.GetOrCreateTable( name ) );

            action.Should()
                .ThrowExactly<SqliteObjectBuilderException>()
                .AndMatch( e => e.Dialect == SqliteDialect.Instance && e.Errors.Count == 1 );
        }

        [Fact]
        public void CreateView_ShouldCreateNewViewWithTableReference()
        {
            var name = Fixture.Create<string>();
            var db = SqliteDatabaseBuilderMock.Create();
            var sut = db.Schemas.Create( "foo" ).Objects;

            var table = sut.CreateTable( "T" );
            var column = table.Columns.Create( "C" );
            var pk = table.Constraints.SetPrimaryKey( column.Asc() );

            var source = table.ToRecordSet().ToDataSource().Select( s => new[] { s.From["C"].AsSelf() } );
            var result = ((ISqlObjectBuilderCollection)sut).CreateView( name, source );

            using ( new AssertionScope() )
            {
                result.Schema.Should().BeSameAs( sut.Schema );
                result.Name.Should().Be( name );
                result.Database.Should().BeSameAs( db );
                result.Source.Should().BeSameAs( source );
                result.ReferencedObjects.Should().HaveCount( 2 );
                result.ReferencedObjects.Should().BeEquivalentTo( table, column );
                result.Type.Should().Be( SqlObjectType.View );
                sut.Count.Should().Be( 4 );
                sut.Should().BeEquivalentTo( table, pk, pk.Index, result );

                table.ReferencingViews.Should().HaveCount( 1 );
                table.ReferencingViews.Should().BeEquivalentTo( result );

                column.ReferencingViews.Should().HaveCount( 1 );
                column.ReferencingViews.Should().BeEquivalentTo( result );

                result.Info.Should().Be( SqlRecordSetInfo.Create( "foo", name ) );
                result.Node.View.Should().BeSameAs( result );
                result.Node.Info.Should().Be( result.Info );
                result.Node.Alias.Should().BeNull();
                result.Node.Identifier.Should().Be( result.Info.Identifier );
                result.Node.IsOptional.Should().BeFalse();
            }
        }

        [Fact]
        public void CreateView_ShouldCreateNewViewWithOtherViewReference()
        {
            var name = Fixture.Create<string>();
            var db = SqliteDatabaseBuilderMock.Create();
            var sut = db.Schemas.Create( "foo" ).Objects;
            var view = sut.CreateView( "V", SqlNode.RawQuery( "SELECT * FROM foo" ) );

            var source = view.ToRecordSet().ToDataSource().Select( s => new[] { s.GetAll() } );
            var result = ((ISqlObjectBuilderCollection)sut).CreateView( name, source );

            using ( new AssertionScope() )
            {
                result.Schema.Should().BeSameAs( sut.Schema );
                result.Name.Should().Be( name );
                result.Database.Should().BeSameAs( db );
                result.Source.Should().BeSameAs( source );
                result.ReferencedObjects.Should().HaveCount( 1 );
                result.ReferencedObjects.Should().BeEquivalentTo( view );
                result.Type.Should().Be( SqlObjectType.View );
                sut.Count.Should().Be( 2 );
                sut.Should().BeEquivalentTo( view, result );

                view.ReferencingViews.Should().HaveCount( 1 );
                view.ReferencingViews.Should().BeEquivalentTo( result );
            }
        }

        [Fact]
        public void CreateView_ShouldThrowSqliteObjectBuilderException_WhenSourceIsNotValid()
        {
            var name = Fixture.Create<string>();
            var db = SqliteDatabaseBuilderMock.Create();
            var sut = db.Schemas.Create( "foo" ).Objects;

            var source = SqlNode.RawQuery( "SELECT * FROM foo WHERE a > @a", SqlNode.Parameter<int>( "a" ) );
            var action = Lambda.Of( () => ((ISqlObjectBuilderCollection)sut).CreateView( name, source ) );

            action.Should()
                .ThrowExactly<SqliteObjectBuilderException>()
                .AndMatch( e => e.Dialect == SqliteDialect.Instance && e.Errors.Count == 1 );
        }

        [Theory]
        [InlineData( "" )]
        [InlineData( " " )]
        [InlineData( "\"" )]
        [InlineData( "'" )]
        [InlineData( "f\"oo" )]
        public void CreateView_ShouldThrowSqliteObjectBuilderException_WhenNameIsInvalid(string name)
        {
            var db = SqliteDatabaseBuilderMock.Create();
            var sut = db.Schemas.Create( "foo" ).Objects;

            var action = Lambda.Of( () => sut.CreateView( name, SqlNode.RawQuery( "SELECT * FROM foo" ) ) );

            action.Should()
                .ThrowExactly<SqliteObjectBuilderException>()
                .AndMatch( e => e.Dialect == SqliteDialect.Instance && e.Errors.Count == 1 );
        }

        [Fact]
        public void CreateView_ShouldThrowSqliteObjectBuilderException_WhenViewWithNameAlreadyExists()
        {
            var name = Fixture.Create<string>();
            var db = SqliteDatabaseBuilderMock.Create();
            var sut = db.Schemas.Create( "foo" ).Objects;
            sut.CreateView( name, SqlNode.RawQuery( "SELECT * FROM foo" ) );

            var action = Lambda.Of( () => sut.CreateView( name, SqlNode.RawQuery( "SELECT * FROM foo" ) ) );

            action.Should()
                .ThrowExactly<SqliteObjectBuilderException>()
                .AndMatch( e => e.Dialect == SqliteDialect.Instance && e.Errors.Count == 1 );
        }

        [Fact]
        public void CreateView_ShouldThrowSqliteObjectBuilderException_WhenSchemaIsRemoved()
        {
            var name = Fixture.Create<string>();
            var db = SqliteDatabaseBuilderMock.Create();
            var sut = db.Schemas.Create( "foo" ).Objects;
            sut.Schema.Remove();

            var action = Lambda.Of( () => sut.CreateView( name, SqlNode.RawQuery( "SELECT * FROM foo" ) ) );

            action.Should()
                .ThrowExactly<SqliteObjectBuilderException>()
                .AndMatch( e => e.Dialect == SqliteDialect.Instance && e.Errors.Count == 1 );
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
            var db = SqliteDatabaseBuilderMock.Create();
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
        public void GetObject_ShouldReturnExistingObject()
        {
            var name = Fixture.Create<string>();
            var db = SqliteDatabaseBuilderMock.Create();
            ISqlObjectBuilderCollection sut = db.Schemas.Create( "foo" ).Objects;
            var expected = sut.CreateTable( name );

            var result = sut.Get( name );

            result.Should().BeSameAs( expected );
        }

        [Fact]
        public void GetObject_ShouldThrowKeyNotFoundException_WhenObjectDoesNotExist()
        {
            var name = Fixture.Create<string>();
            var db = SqliteDatabaseBuilderMock.Create();
            ISqlObjectBuilderCollection sut = db.Schemas.Create( "foo" ).Objects;

            var action = Lambda.Of( () => sut.Get( name ) );

            action.Should().ThrowExactly<KeyNotFoundException>();
        }

        [Fact]
        public void TryGetObject_ShouldReturnExistingObject()
        {
            var name = Fixture.Create<string>();
            var db = SqliteDatabaseBuilderMock.Create();
            ISqlObjectBuilderCollection sut = db.Schemas.Create( "foo" ).Objects;
            var expected = sut.CreateTable( name );

            var result = sut.TryGet( name );

            result.Should().BeSameAs( expected );
        }

        [Fact]
        public void TryGetObject_ShouldReturnNull_WhenObjectDoesNotExist()
        {
            var name = Fixture.Create<string>();
            var db = SqliteDatabaseBuilderMock.Create();
            ISqlObjectBuilderCollection sut = db.Schemas.Create( "foo" ).Objects;

            var result = sut.TryGet( name );

            result.Should().BeNull();
        }

        [Fact]
        public void GetTable_ShouldReturnExistingTable()
        {
            var name = Fixture.Create<string>();
            var db = SqliteDatabaseBuilderMock.Create();
            ISqlObjectBuilderCollection sut = db.Schemas.Create( "foo" ).Objects;
            var expected = sut.CreateTable( name );

            var result = sut.GetTable( name );

            result.Should().BeSameAs( expected );
        }

        [Fact]
        public void GetTable_ShouldThrowKeyNotFoundException_WhenObjectDoesNotExist()
        {
            var name = Fixture.Create<string>();
            var db = SqliteDatabaseBuilderMock.Create();
            ISqlObjectBuilderCollection sut = db.Schemas.Create( "foo" ).Objects;

            var action = Lambda.Of( () => sut.GetTable( name ) );

            action.Should().ThrowExactly<KeyNotFoundException>();
        }

        [Fact]
        public void GetTable_ShouldThrowSqlObjectCastException_WhenObjectExistsButNotAsTable()
        {
            var name = Fixture.Create<string>();
            var db = SqliteDatabaseBuilderMock.Create();
            ISqlObjectBuilderCollection sut = db.Schemas.Create( "foo" ).Objects;
            var t = sut.CreateTable( "T" );
            var c = t.Columns.Create( "C" );
            t.Constraints.SetPrimaryKey( c.Asc() ).SetName( name );

            var action = Lambda.Of( () => sut.GetTable( name ) );

            action.Should()
                .ThrowExactly<SqlObjectCastException>()
                .AndMatch(
                    e => e.Dialect == SqliteDialect.Instance &&
                        e.Expected == typeof( SqliteTableBuilder ) &&
                        e.Actual == typeof( SqlitePrimaryKeyBuilder ) );
        }

        [Fact]
        public void TryGetTable_ShouldReturnExistingTable()
        {
            var name = Fixture.Create<string>();
            var db = SqliteDatabaseBuilderMock.Create();
            ISqlObjectBuilderCollection sut = db.Schemas.Create( "foo" ).Objects;
            var expected = sut.CreateTable( name );

            var result = sut.TryGetTable( name );

            result.Should().BeSameAs( expected );
        }

        [Fact]
        public void TryGetTable_ShouldReturnNull_WhenObjectDoesNotExist()
        {
            var name = Fixture.Create<string>();
            var db = SqliteDatabaseBuilderMock.Create();
            ISqlObjectBuilderCollection sut = db.Schemas.Create( "foo" ).Objects;

            var result = sut.TryGetTable( name );

            result.Should().BeNull();
        }

        [Fact]
        public void TryGetTable_ShouldReturnNull_WhenObjectExistsButNotAsTable()
        {
            var name = Fixture.Create<string>();
            var db = SqliteDatabaseBuilderMock.Create();
            ISqlObjectBuilderCollection sut = db.Schemas.Create( "foo" ).Objects;
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
            var db = SqliteDatabaseBuilderMock.Create();
            ISqlObjectBuilderCollection sut = db.Schemas.Create( "foo" ).Objects;
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
            var db = SqliteDatabaseBuilderMock.Create();
            ISqlObjectBuilderCollection sut = db.Schemas.Create( "foo" ).Objects;

            var action = Lambda.Of( () => sut.GetIndex( name ) );

            action.Should().ThrowExactly<KeyNotFoundException>();
        }

        [Fact]
        public void GetIndex_ShouldThrowSqlObjectCastException_WhenObjectExistsButNotAsIndex()
        {
            var name = Fixture.Create<string>();
            var db = SqliteDatabaseBuilderMock.Create();
            ISqlObjectBuilderCollection sut = db.Schemas.Create( "foo" ).Objects;
            sut.CreateTable( name );

            var action = Lambda.Of( () => sut.GetIndex( name ) );

            action.Should()
                .ThrowExactly<SqlObjectCastException>()
                .AndMatch(
                    e => e.Dialect == SqliteDialect.Instance &&
                        e.Expected == typeof( SqliteIndexBuilder ) &&
                        e.Actual == typeof( SqliteTableBuilder ) );
        }

        [Fact]
        public void TryGetIndex_ShouldReturnExistingIndex()
        {
            var name = Fixture.Create<string>();
            var db = SqliteDatabaseBuilderMock.Create();
            ISqlObjectBuilderCollection sut = db.Schemas.Create( "foo" ).Objects;
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
            var db = SqliteDatabaseBuilderMock.Create();
            ISqlObjectBuilderCollection sut = db.Schemas.Create( "foo" ).Objects;

            var result = sut.TryGetIndex( name );

            result.Should().BeNull();
        }

        [Fact]
        public void TryGetIndex_ShouldReturnNull_WhenObjectExistsButNotAsIndex()
        {
            var name = Fixture.Create<string>();
            var db = SqliteDatabaseBuilderMock.Create();
            ISqlObjectBuilderCollection sut = db.Schemas.Create( "foo" ).Objects;
            sut.CreateTable( name );

            var result = sut.TryGetIndex( name );

            result.Should().BeNull();
        }

        [Fact]
        public void GetPrimaryKey_ShouldReturnExistingPrimaryKey()
        {
            var name = Fixture.Create<string>();
            var db = SqliteDatabaseBuilderMock.Create();
            ISqlObjectBuilderCollection sut = db.Schemas.Create( "foo" ).Objects;
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
            var db = SqliteDatabaseBuilderMock.Create();
            ISqlObjectBuilderCollection sut = db.Schemas.Create( "foo" ).Objects;

            var action = Lambda.Of( () => sut.GetPrimaryKey( name ) );

            action.Should().ThrowExactly<KeyNotFoundException>();
        }

        [Fact]
        public void GetPrimaryKey_ShouldThrowSqlObjectCastException_WhenObjectExistsButNotAsPrimaryKey()
        {
            var name = Fixture.Create<string>();
            var db = SqliteDatabaseBuilderMock.Create();
            ISqlObjectBuilderCollection sut = db.Schemas.Create( "foo" ).Objects;
            sut.CreateTable( name );

            var action = Lambda.Of( () => sut.GetPrimaryKey( name ) );

            action.Should()
                .ThrowExactly<SqlObjectCastException>()
                .AndMatch(
                    e => e.Dialect == SqliteDialect.Instance &&
                        e.Expected == typeof( SqlitePrimaryKeyBuilder ) &&
                        e.Actual == typeof( SqliteTableBuilder ) );
        }

        [Fact]
        public void TryGetPrimaryKey_ShouldReturnExistingPrimaryKey()
        {
            var name = Fixture.Create<string>();
            var db = SqliteDatabaseBuilderMock.Create();
            ISqlObjectBuilderCollection sut = db.Schemas.Create( "foo" ).Objects;
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
            var db = SqliteDatabaseBuilderMock.Create();
            ISqlObjectBuilderCollection sut = db.Schemas.Create( "foo" ).Objects;

            var result = sut.TryGetPrimaryKey( name );

            result.Should().BeNull();
        }

        [Fact]
        public void TryGetPrimaryKey_ShouldReturnNull_WhenObjectExistsButNotAsPrimaryKey()
        {
            var name = Fixture.Create<string>();
            var db = SqliteDatabaseBuilderMock.Create();
            ISqlObjectBuilderCollection sut = db.Schemas.Create( "foo" ).Objects;
            sut.CreateTable( name );

            var result = sut.TryGetPrimaryKey( name );

            result.Should().BeNull();
        }

        [Fact]
        public void GetForeignKey_ShouldReturnExistingForeignKey()
        {
            var name = Fixture.Create<string>();
            var db = SqliteDatabaseBuilderMock.Create();
            ISqlObjectBuilderCollection sut = db.Schemas.Create( "foo" ).Objects;
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
            var db = SqliteDatabaseBuilderMock.Create();
            ISqlObjectBuilderCollection sut = db.Schemas.Create( "foo" ).Objects;

            var action = Lambda.Of( () => sut.GetForeignKey( name ) );

            action.Should().ThrowExactly<KeyNotFoundException>();
        }

        [Fact]
        public void GetForeignKey_ShouldThrowSqlObjectCastException_WhenObjectExistsButNotAsForeignKey()
        {
            var name = Fixture.Create<string>();
            var db = SqliteDatabaseBuilderMock.Create();
            ISqlObjectBuilderCollection sut = db.Schemas.Create( "foo" ).Objects;
            sut.CreateTable( name );

            var action = Lambda.Of( () => sut.GetForeignKey( name ) );

            action.Should()
                .ThrowExactly<SqlObjectCastException>()
                .AndMatch(
                    e => e.Dialect == SqliteDialect.Instance &&
                        e.Expected == typeof( SqliteForeignKeyBuilder ) &&
                        e.Actual == typeof( SqliteTableBuilder ) );
        }

        [Fact]
        public void TryGetForeignKey_ShouldReturnExistingForeignKey()
        {
            var name = Fixture.Create<string>();
            var db = SqliteDatabaseBuilderMock.Create();
            ISqlObjectBuilderCollection sut = db.Schemas.Create( "foo" ).Objects;
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
            var db = SqliteDatabaseBuilderMock.Create();
            ISqlObjectBuilderCollection sut = db.Schemas.Create( "foo" ).Objects;

            var result = sut.TryGetForeignKey( name );

            result.Should().BeNull();
        }

        [Fact]
        public void TryGetForeignKey_ShouldReturnNull_WhenObjectExistsButNotAsForeignKey()
        {
            var name = Fixture.Create<string>();
            var db = SqliteDatabaseBuilderMock.Create();
            ISqlObjectBuilderCollection sut = db.Schemas.Create( "foo" ).Objects;
            sut.CreateTable( name );

            var result = sut.TryGetForeignKey( name );

            result.Should().BeNull();
        }

        [Fact]
        public void GetView_ShouldReturnExistingView()
        {
            var name = Fixture.Create<string>();
            var db = SqliteDatabaseBuilderMock.Create();
            ISqlObjectBuilderCollection sut = db.Schemas.Create( "foo" ).Objects;
            var expected = sut.CreateView( name, SqlNode.RawQuery( "SELECT * FROM foo" ) );

            var result = sut.GetView( name );

            result.Should().BeSameAs( expected );
        }

        [Fact]
        public void GetView_ShouldThrowKeyNotFoundException_WhenObjectDoesNotExist()
        {
            var name = Fixture.Create<string>();
            var db = SqliteDatabaseBuilderMock.Create();
            ISqlObjectBuilderCollection sut = db.Schemas.Create( "foo" ).Objects;

            var action = Lambda.Of( () => sut.GetView( name ) );

            action.Should().ThrowExactly<KeyNotFoundException>();
        }

        [Fact]
        public void GetView_ShouldThrowSqlObjectCastException_WhenObjectExistsButNotAsView()
        {
            var name = Fixture.Create<string>();
            var db = SqliteDatabaseBuilderMock.Create();
            ISqlObjectBuilderCollection sut = db.Schemas.Create( "foo" ).Objects;
            sut.CreateTable( name );

            var action = Lambda.Of( () => sut.GetView( name ) );

            action.Should()
                .ThrowExactly<SqlObjectCastException>()
                .AndMatch(
                    e => e.Dialect == SqliteDialect.Instance &&
                        e.Expected == typeof( SqliteViewBuilder ) &&
                        e.Actual == typeof( SqliteTableBuilder ) );
        }

        [Fact]
        public void TryGetView_ShouldReturnExistingView()
        {
            var name = Fixture.Create<string>();
            var db = SqliteDatabaseBuilderMock.Create();
            ISqlObjectBuilderCollection sut = db.Schemas.Create( "foo" ).Objects;
            var expected = sut.CreateView( name, SqlNode.RawQuery( "SELECT * FROM foo" ) );

            var result = sut.TryGetView( name );

            result.Should().BeSameAs( expected );
        }

        [Fact]
        public void TryGetView_ShouldReturnNull_WhenObjectDoesNotExist()
        {
            var name = Fixture.Create<string>();
            var db = SqliteDatabaseBuilderMock.Create();
            ISqlObjectBuilderCollection sut = db.Schemas.Create( "foo" ).Objects;

            var result = sut.TryGetView( name );

            result.Should().BeNull();
        }

        [Fact]
        public void TryGetView_ShouldReturnNull_WhenObjectExistsButNotAsView()
        {
            var name = Fixture.Create<string>();
            var db = SqliteDatabaseBuilderMock.Create();
            ISqlObjectBuilderCollection sut = db.Schemas.Create( "foo" ).Objects;
            sut.CreateTable( name );

            var result = sut.TryGetView( name );

            result.Should().BeNull();
        }

        [Fact]
        public void GetCheck_ShouldReturnExistingCheck()
        {
            var name = Fixture.Create<string>();
            var db = SqliteDatabaseBuilderMock.Create();
            ISqlObjectBuilderCollection sut = db.Schemas.Create( "foo" ).Objects;
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
            var db = SqliteDatabaseBuilderMock.Create();
            ISqlObjectBuilderCollection sut = db.Schemas.Create( "foo" ).Objects;

            var action = Lambda.Of( () => sut.GetCheck( name ) );

            action.Should().ThrowExactly<KeyNotFoundException>();
        }

        [Fact]
        public void GetCheck_ShouldThrowSqlObjectCastException_WhenObjectExistsButNotAsCheck()
        {
            var name = Fixture.Create<string>();
            var db = SqliteDatabaseBuilderMock.Create();
            ISqlObjectBuilderCollection sut = db.Schemas.Create( "foo" ).Objects;
            sut.CreateTable( name );

            var action = Lambda.Of( () => sut.GetCheck( name ) );

            action.Should()
                .ThrowExactly<SqlObjectCastException>()
                .AndMatch(
                    e => e.Dialect == SqliteDialect.Instance &&
                        e.Expected == typeof( SqliteCheckBuilder ) &&
                        e.Actual == typeof( SqliteTableBuilder ) );
        }

        [Fact]
        public void TryGetCheck_ShouldReturnExistingCheck()
        {
            var name = Fixture.Create<string>();
            var db = SqliteDatabaseBuilderMock.Create();
            ISqlObjectBuilderCollection sut = db.Schemas.Create( "foo" ).Objects;
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
            var db = SqliteDatabaseBuilderMock.Create();
            ISqlObjectBuilderCollection sut = db.Schemas.Create( "foo" ).Objects;

            var result = sut.TryGetCheck( name );

            result.Should().BeNull();
        }

        [Fact]
        public void TryGetCheck_ShouldReturnNull_WhenObjectExistsButNotAsCheck()
        {
            var name = Fixture.Create<string>();
            var db = SqliteDatabaseBuilderMock.Create();
            ISqlObjectBuilderCollection sut = db.Schemas.Create( "foo" ).Objects;
            sut.CreateTable( name );

            var result = sut.TryGetCheck( name );

            result.Should().BeNull();
        }

        [Fact]
        public void Remove_ShouldRemoveExistingEmptyTable()
        {
            var db = SqliteDatabaseBuilderMock.Create();
            var sut = db.Schemas.Create( "foo" ).Objects;
            var table = sut.CreateTable( "T" );

            var result = sut.Remove( table.Name );

            using ( new AssertionScope() )
            {
                result.Should().BeTrue();
                table.IsRemoved.Should().BeTrue();
            }
        }

        [Fact]
        public void Remove_ShouldRemoveExistingNonEmptyTable()
        {
            var db = SqliteDatabaseBuilderMock.Create();
            var sut = db.Schemas.Create( "foo" ).Objects;
            var table = sut.CreateTable( "T" );
            var column = table.Columns.Create( "C" );
            var otherColumn = table.Columns.Create( "D" );
            var pk = table.Constraints.SetPrimaryKey( column.Asc() );
            var ix = table.Constraints.CreateIndex( otherColumn.Asc() );
            var fk = table.Constraints.CreateForeignKey( ix, pk.Index );
            var chk = table.Constraints.CreateCheck( column.Node != null );

            var result = sut.Remove( table.Name );

            using ( new AssertionScope() )
            {
                result.Should().BeTrue();
                sut.Count.Should().Be( 0 );
                sut.Should().BeEmpty();
                table.IsRemoved.Should().BeTrue();
                column.IsRemoved.Should().BeTrue();
                otherColumn.IsRemoved.Should().BeTrue();
                pk.IsRemoved.Should().BeTrue();
                pk.Index.IsRemoved.Should().BeTrue();
                ix.IsRemoved.Should().BeTrue();
                fk.IsRemoved.Should().BeTrue();
                chk.IsRemoved.Should().BeTrue();
            }
        }

        [Fact]
        public void Remove_ShouldReturnFalse_WhenTableToRemoveHasReferencingForeignKeys()
        {
            var db = SqliteDatabaseBuilderMock.Create();
            var sut = db.Schemas.Create( "foo" ).Objects;
            var table = sut.CreateTable( "T" );
            var column = table.Columns.Create( "C" );
            var pk = table.Constraints.SetPrimaryKey( column.Asc() );
            var chk = table.Constraints.CreateCheck( column.Node != null );

            var otherTable = sut.CreateTable( "U" );
            var otherColumn = otherTable.Columns.Create( "D" );
            var otherPk = otherTable.Constraints.SetPrimaryKey( otherColumn.Asc() );
            var fk = otherTable.Constraints.CreateForeignKey( otherPk.Index, pk.Index );

            var result = sut.Remove( table.Name );

            using ( new AssertionScope() )
            {
                result.Should().BeFalse();
                sut.Count.Should().Be( 8 );
                sut.Should().BeEquivalentTo( table, pk, pk.Index, chk, otherTable, otherPk, otherPk.Index, fk );
                table.IsRemoved.Should().BeFalse();
                column.IsRemoved.Should().BeFalse();
                pk.IsRemoved.Should().BeFalse();
                pk.Index.IsRemoved.Should().BeFalse();
                chk.IsRemoved.Should().BeFalse();
            }
        }

        [Fact]
        public void Remove_ShouldReturnFalse_WhenTableToRemoveHasReferencingViews()
        {
            var db = SqliteDatabaseBuilderMock.Create();
            var sut = db.Schemas.Create( "foo" ).Objects;
            var table = sut.CreateTable( "T" );
            var column = table.Columns.Create( "C" );
            var pk = table.Constraints.SetPrimaryKey( column.Asc() );

            var view = sut.CreateView( "V", table.ToRecordSet().ToDataSource().Select( s => new[] { s.GetAll() } ) );

            var result = sut.Remove( table.Name );

            using ( new AssertionScope() )
            {
                result.Should().BeFalse();
                sut.Count.Should().Be( 4 );
                sut.Should().BeEquivalentTo( table, pk, pk.Index, view );
                table.IsRemoved.Should().BeFalse();
                column.IsRemoved.Should().BeFalse();
                pk.IsRemoved.Should().BeFalse();
                pk.Index.IsRemoved.Should().BeFalse();
            }
        }

        [Fact]
        public void Remove_ShouldRemoveExistingPrimaryKey()
        {
            var db = SqliteDatabaseBuilderMock.Create();
            var sut = db.Schemas.Create( "foo" ).Objects;
            var table = sut.CreateTable( "T" );
            var column = table.Columns.Create( "C" );
            var pk = table.Constraints.SetPrimaryKey( column.Asc() );

            var result = sut.Remove( pk.Name );

            using ( new AssertionScope() )
            {
                result.Should().BeTrue();
                sut.Count.Should().Be( 1 );
                sut.Should().BeEquivalentTo( table );
                table.Constraints.TryGetPrimaryKey().Should().BeNull();
                pk.IsRemoved.Should().BeTrue();
                pk.Index.IsRemoved.Should().BeTrue();
            }
        }

        [Fact]
        public void Remove_ShouldReturnFalse_WhenPrimaryKeyToRemoveHasExternalReferences()
        {
            var db = SqliteDatabaseBuilderMock.Create();
            var sut = db.Schemas.Create( "foo" ).Objects;
            var table = sut.CreateTable( "T" );
            var column = table.Columns.Create( "C" );
            var pk = table.Constraints.SetPrimaryKey( column.Asc() );

            var otherTable = sut.CreateTable( "U" );
            var otherColumn = otherTable.Columns.Create( "D" );
            var otherPk = otherTable.Constraints.SetPrimaryKey( otherColumn.Asc() );
            var fk = otherTable.Constraints.CreateForeignKey( otherPk.Index, pk.Index );

            var result = sut.Remove( pk.Name );

            using ( new AssertionScope() )
            {
                result.Should().BeFalse();
                sut.Count.Should().Be( 7 );
                sut.Should().BeEquivalentTo( table, pk, pk.Index, otherTable, otherPk, otherPk.Index, fk );
                pk.IsRemoved.Should().BeFalse();
                pk.Index.IsRemoved.Should().BeFalse();
            }
        }

        [Fact]
        public void Remove_ShouldRemoveExistingIndex()
        {
            var db = SqliteDatabaseBuilderMock.Create();
            var sut = db.Schemas.Create( "foo" ).Objects;
            var table = sut.CreateTable( "T" );
            var column = table.Columns.Create( "C" );
            var otherColumn = table.Columns.Create( "D" );
            var pk = table.Constraints.SetPrimaryKey( column.Asc() );
            var ix = table.Constraints.CreateIndex( otherColumn.Asc() );

            var result = sut.Remove( ix.Name );

            using ( new AssertionScope() )
            {
                result.Should().BeTrue();
                sut.Count.Should().Be( 3 );
                sut.Should().BeEquivalentTo( table, pk, pk.Index );
                ix.IsRemoved.Should().BeTrue();
            }
        }

        [Fact]
        public void Remove_ShouldReturnFalse_WhenIndexToRemoveHasExternalReferences()
        {
            var db = SqliteDatabaseBuilderMock.Create();
            var sut = db.Schemas.Create( "foo" ).Objects;
            var table = sut.CreateTable( "T" );
            var column = table.Columns.Create( "C" );
            var otherColumn = table.Columns.Create( "D" );
            var pk = table.Constraints.SetPrimaryKey( column.Asc() );
            var ix = table.Constraints.CreateIndex( otherColumn.Asc() ).MarkAsUnique();

            var otherTable = sut.CreateTable( "U" );
            var externalColumn = otherTable.Columns.Create( "D" );
            var otherPk = otherTable.Constraints.SetPrimaryKey( externalColumn.Asc() );
            var fk = otherTable.Constraints.CreateForeignKey( otherPk.Index, ix );

            var result = sut.Remove( ix.Name );

            using ( new AssertionScope() )
            {
                result.Should().BeFalse();
                sut.Count.Should().Be( 8 );
                sut.Should().BeEquivalentTo( table, pk, pk.Index, ix, otherTable, otherPk, otherPk.Index, fk );
                ix.IsRemoved.Should().BeFalse();
            }
        }

        [Fact]
        public void Remove_ShouldRemoveExistingForeignKey()
        {
            var db = SqliteDatabaseBuilderMock.Create();
            var sut = db.Schemas.Create( "foo" ).Objects;
            var table = sut.CreateTable( "T" );
            var column = table.Columns.Create( "C" );
            var pk = table.Constraints.SetPrimaryKey( column.Asc() );

            var otherTable = sut.CreateTable( "U" );
            var otherColumn = otherTable.Columns.Create( "D" );
            var otherPk = otherTable.Constraints.SetPrimaryKey( otherColumn.Asc() );
            var fk = otherTable.Constraints.CreateForeignKey( otherPk.Index, pk.Index );

            var result = sut.Remove( fk.Name );

            using ( new AssertionScope() )
            {
                result.Should().BeTrue();
                sut.Count.Should().Be( 6 );
                sut.Should().BeEquivalentTo( table, pk, pk.Index, otherTable, otherPk, otherPk.Index );
                fk.IsRemoved.Should().BeTrue();
            }
        }

        [Fact]
        public void Remove_ShouldRemoveExistingView()
        {
            var db = SqliteDatabaseBuilderMock.Create();
            var sut = db.Schemas.Create( "foo" ).Objects;
            var view = sut.CreateView( "V", SqlNode.RawQuery( "SELECT * FROM foo" ) );

            var result = sut.Remove( view.Name );

            using ( new AssertionScope() )
            {
                result.Should().BeTrue();
                sut.Count.Should().Be( 0 );
                sut.Should().BeEmpty();
                view.IsRemoved.Should().BeTrue();
            }
        }

        [Fact]
        public void Remove_ShouldReturnFalse_WhenViewToRemoveHasReferencingViews()
        {
            var db = SqliteDatabaseBuilderMock.Create();
            var sut = db.Schemas.Create( "foo" ).Objects;
            var view = sut.CreateView( "V", SqlNode.RawQuery( "SELECT * FROM foo" ) );
            var otherView = sut.CreateView( "W", view.ToRecordSet().ToDataSource().Select( s => new[] { s.GetAll() } ) );

            var result = sut.Remove( view.Name );

            using ( new AssertionScope() )
            {
                result.Should().BeFalse();
                sut.Count.Should().Be( 2 );
                sut.Should().BeEquivalentTo( view, otherView );
                view.IsRemoved.Should().BeFalse();
            }
        }

        [Fact]
        public void Remove_ShouldRemoveExistingCheck()
        {
            var db = SqliteDatabaseBuilderMock.Create();
            var sut = db.Schemas.Create( "foo" ).Objects;
            var table = sut.CreateTable( "T" );
            var column = table.Columns.Create( "C" );
            var pk = table.Constraints.SetPrimaryKey( column.Asc() );
            var chk = table.Constraints.CreateCheck( column.Node != null );

            var result = sut.Remove( chk.Name );

            using ( new AssertionScope() )
            {
                result.Should().BeTrue();
                sut.Count.Should().Be( 3 );
                sut.Should().BeEquivalentTo( table, pk, pk.Index );
                chk.IsRemoved.Should().BeTrue();
            }
        }

        [Fact]
        public void Remove_ShouldReturnFalse_WhenObjectDoesNotExist()
        {
            var db = SqliteDatabaseBuilderMock.Create();
            var sut = db.Schemas.Create( "foo" ).Objects;

            var result = sut.Remove( Fixture.Create<string>() );

            result.Should().BeFalse();
        }
    }
}
