using System.Collections.Generic;
using LfrlAnvil.Functional;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Expressions;
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
                result.FullName.Should().Be( $"foo_{name}" );
                result.PrimaryKey.Should().BeNull();
                result.Database.Should().BeSameAs( db );
                result.ReferencingViews.Should().BeEmpty();
                result.Type.Should().Be( SqlObjectType.Table );
                sut.Count.Should().Be( 1 );
                sut.Should().BeEquivalentTo( result );

                result.Columns.Should().BeEmpty();
                result.Columns.Table.Should().BeSameAs( result );
                result.Columns.DefaultTypeDefinition.Should().BeSameAs( db.TypeDefinitions.GetByType<object>() );
                result.Columns.Count.Should().Be( 0 );

                result.Indexes.Should().BeEmpty();
                result.Indexes.Table.Should().BeSameAs( result );
                result.Indexes.Count.Should().Be( 0 );

                result.ForeignKeys.Should().BeEmpty();
                result.ForeignKeys.Table.Should().BeSameAs( result );
                result.ForeignKeys.Count.Should().Be( 0 );

                result.Checks.Should().BeEmpty();
                result.Checks.Table.Should().BeSameAs( result );
                result.Checks.Count.Should().Be( 0 );

                result.Info.Should().Be( SqlRecordSetInfo.Create( "foo", name ) );
                result.RecordSet.Table.Should().BeSameAs( result );
                result.RecordSet.Info.Should().Be( result.Info );
                result.RecordSet.Alias.Should().BeNull();
                result.RecordSet.Identifier.Should().Be( result.Info.Identifier );
                result.RecordSet.IsOptional.Should().BeFalse();
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
                result.FullName.Should().Be( $"foo_{name}" );
                result.PrimaryKey.Should().BeNull();
                result.Database.Should().BeSameAs( db );
                result.ReferencingViews.Should().BeEmpty();
                result.Type.Should().Be( SqlObjectType.Table );
                sut.Count.Should().Be( 1 );
                sut.Should().BeEquivalentTo( result );

                result.Columns.Should().BeEmpty();
                result.Columns.Table.Should().BeSameAs( result );
                result.Columns.DefaultTypeDefinition.Should().BeSameAs( db.TypeDefinitions.GetByType<object>() );
                result.Columns.Count.Should().Be( 0 );

                result.Indexes.Should().BeEmpty();
                result.Indexes.Table.Should().BeSameAs( result );
                result.Indexes.Count.Should().Be( 0 );

                result.Checks.Should().BeEmpty();
                result.Checks.Table.Should().BeSameAs( result );
                result.Checks.Count.Should().Be( 0 );

                result.ForeignKeys.Should().BeEmpty();
                result.ForeignKeys.Table.Should().BeSameAs( result );
                result.ForeignKeys.Count.Should().Be( 0 );
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
        public void GetOrCreateTable_ShouldThrowSqliteObjectCastException_WhenNonTableObjectWithNameAlreadyExists()
        {
            var name = Fixture.Create<string>();
            var db = SqliteDatabaseBuilderMock.Create();
            var sut = db.Schemas.Create( "foo" ).Objects;
            var t = sut.CreateTable( "T" );
            var c = t.Columns.Create( "C" );
            t.SetPrimaryKey( c.Asc() ).SetName( name );

            var action = Lambda.Of( () => sut.GetOrCreateTable( name ) );

            action.Should()
                .ThrowExactly<SqliteObjectCastException>()
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
            var pk = table.SetPrimaryKey( column.Asc() );

            var source = table.ToRecordSet().ToDataSource().Select( s => new[] { s.From["C"].AsSelf() } );
            var result = ((ISqlObjectBuilderCollection)sut).CreateView( name, source );

            using ( new AssertionScope() )
            {
                result.Schema.Should().BeSameAs( sut.Schema );
                result.Name.Should().Be( name );
                result.FullName.Should().Be( $"foo_{name}" );
                result.Database.Should().BeSameAs( db );
                result.Source.Should().BeSameAs( source );
                result.ReferencingViews.Should().BeEmpty();
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
                result.RecordSet.View.Should().BeSameAs( result );
                result.RecordSet.Info.Should().Be( result.Info );
                result.RecordSet.Alias.Should().BeNull();
                result.RecordSet.Identifier.Should().Be( result.Info.Identifier );
                result.RecordSet.IsOptional.Should().BeFalse();
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
                result.FullName.Should().Be( $"foo_{name}" );
                result.Database.Should().BeSameAs( db );
                result.Source.Should().BeSameAs( source );
                result.ReferencingViews.Should().BeEmpty();
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
            t.SetPrimaryKey( c.Asc() ).SetName( "PK" ).Index.SetName( "IX" );
            t.Checks.Create( t.RecordSet["C"] != null ).SetName( "CHK" );
            sut.CreateView( "V", SqlNode.RawQuery( "SELECT * FROM foo" ) );

            var result = sut.Contains( name );

            result.Should().Be( expected );
        }

        [Fact]
        public void Get_ShouldReturnExistingObject()
        {
            var name = Fixture.Create<string>();
            var db = SqliteDatabaseBuilderMock.Create();
            var sut = db.Schemas.Create( "foo" ).Objects;
            var expected = sut.CreateTable( name );

            var result = ((ISqlObjectBuilderCollection)sut).Get( name );

            result.Should().BeSameAs( expected );
        }

        [Fact]
        public void Get_ShouldThrowKeyNotFoundException_WhenObjectDoesNotExist()
        {
            var name = Fixture.Create<string>();
            var db = SqliteDatabaseBuilderMock.Create();
            var sut = db.Schemas.Create( "foo" ).Objects;

            var action = Lambda.Of( () => ((ISqlObjectBuilderCollection)sut).Get( name ) );

            action.Should().ThrowExactly<KeyNotFoundException>();
        }

        [Fact]
        public void TryGet_ShouldReturnExistingObject()
        {
            var name = Fixture.Create<string>();
            var db = SqliteDatabaseBuilderMock.Create();
            var sut = db.Schemas.Create( "foo" ).Objects;
            var expected = sut.CreateTable( name );

            var result = ((ISqlObjectBuilderCollection)sut).TryGet( name, out var outResult );

            using ( new AssertionScope() )
            {
                result.Should().BeTrue();
                outResult.Should().BeSameAs( expected );
            }
        }

        [Fact]
        public void TryGet_ShouldReturnFalse_WhenObjectDoesNotExist()
        {
            var name = Fixture.Create<string>();
            var db = SqliteDatabaseBuilderMock.Create();
            var sut = db.Schemas.Create( "foo" ).Objects;

            var result = ((ISqlObjectBuilderCollection)sut).TryGet( name, out var outResult );

            using ( new AssertionScope() )
            {
                result.Should().BeFalse();
                outResult.Should().BeNull();
            }
        }

        [Fact]
        public void GetTable_ShouldReturnExistingTable()
        {
            var name = Fixture.Create<string>();
            var db = SqliteDatabaseBuilderMock.Create();
            var sut = db.Schemas.Create( "foo" ).Objects;
            var expected = sut.CreateTable( name );

            var result = sut.GetTable( name );

            result.Should().BeSameAs( expected );
        }

        [Fact]
        public void GetTable_ShouldThrowKeyNotFoundException_WhenObjectDoesNotExist()
        {
            var name = Fixture.Create<string>();
            var db = SqliteDatabaseBuilderMock.Create();
            var sut = db.Schemas.Create( "foo" ).Objects;

            var action = Lambda.Of( () => sut.GetTable( name ) );

            action.Should().ThrowExactly<KeyNotFoundException>();
        }

        [Fact]
        public void GetTable_ShouldThrowSqliteObjectCastException_WhenObjectExistsButNotAsTable()
        {
            var name = Fixture.Create<string>();
            var db = SqliteDatabaseBuilderMock.Create();
            var sut = db.Schemas.Create( "foo" ).Objects;
            var t = sut.CreateTable( "T" );
            var c = t.Columns.Create( "C" );
            t.SetPrimaryKey( c.Asc() ).SetName( name );

            var action = Lambda.Of( () => sut.GetTable( name ) );

            action.Should()
                .ThrowExactly<SqliteObjectCastException>()
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
            var sut = db.Schemas.Create( "foo" ).Objects;
            var expected = sut.CreateTable( name );

            var result = sut.TryGetTable( name, out var outResult );

            using ( new AssertionScope() )
            {
                result.Should().BeTrue();
                outResult.Should().BeSameAs( expected );
            }
        }

        [Fact]
        public void TryGetTable_ShouldReturnFalse_WhenObjectDoesNotExist()
        {
            var name = Fixture.Create<string>();
            var db = SqliteDatabaseBuilderMock.Create();
            var sut = db.Schemas.Create( "foo" ).Objects;

            var result = sut.TryGetTable( name, out var outResult );

            using ( new AssertionScope() )
            {
                result.Should().BeFalse();
                outResult.Should().BeNull();
            }
        }

        [Fact]
        public void TryGetTable_ShouldReturnFalse_WhenObjectExistsButNotAsTable()
        {
            var name = Fixture.Create<string>();
            var db = SqliteDatabaseBuilderMock.Create();
            var sut = db.Schemas.Create( "foo" ).Objects;
            var t = sut.CreateTable( "T" );
            var c = t.Columns.Create( "C" );
            t.SetPrimaryKey( c.Asc() ).SetName( name );

            var result = sut.TryGetTable( name, out var outResult );

            using ( new AssertionScope() )
            {
                result.Should().BeFalse();
                outResult.Should().BeNull();
            }
        }

        [Fact]
        public void GetIndex_ShouldReturnExistingIndex()
        {
            var name = Fixture.Create<string>();
            var db = SqliteDatabaseBuilderMock.Create();
            var sut = db.Schemas.Create( "foo" ).Objects;
            var t = sut.CreateTable( "T" );
            var c = t.Columns.Create( "C" );
            var expected = t.Indexes.Create( c.Asc() ).SetName( name );

            var result = sut.GetIndex( name );

            result.Should().BeSameAs( expected );
        }

        [Fact]
        public void GetIndex_ShouldThrowKeyNotFoundException_WhenObjectDoesNotExist()
        {
            var name = Fixture.Create<string>();
            var db = SqliteDatabaseBuilderMock.Create();
            var sut = db.Schemas.Create( "foo" ).Objects;

            var action = Lambda.Of( () => sut.GetIndex( name ) );

            action.Should().ThrowExactly<KeyNotFoundException>();
        }

        [Fact]
        public void GetIndex_ShouldThrowSqliteObjectCastException_WhenObjectExistsButNotAsIndex()
        {
            var name = Fixture.Create<string>();
            var db = SqliteDatabaseBuilderMock.Create();
            var sut = db.Schemas.Create( "foo" ).Objects;
            sut.CreateTable( name );

            var action = Lambda.Of( () => sut.GetIndex( name ) );

            action.Should()
                .ThrowExactly<SqliteObjectCastException>()
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
            var sut = db.Schemas.Create( "foo" ).Objects;
            var t = sut.CreateTable( "T" );
            var c = t.Columns.Create( "C" );
            var expected = t.Indexes.Create( c.Asc() ).SetName( name );

            var result = sut.TryGetIndex( name, out var outResult );

            using ( new AssertionScope() )
            {
                result.Should().BeTrue();
                outResult.Should().BeSameAs( expected );
            }
        }

        [Fact]
        public void TryGetIndex_ShouldReturnFalse_WhenObjectDoesNotExist()
        {
            var name = Fixture.Create<string>();
            var db = SqliteDatabaseBuilderMock.Create();
            var sut = db.Schemas.Create( "foo" ).Objects;

            var result = sut.TryGetIndex( name, out var outResult );

            using ( new AssertionScope() )
            {
                result.Should().BeFalse();
                outResult.Should().BeNull();
            }
        }

        [Fact]
        public void TryGetIndex_ShouldReturnFalse_WhenObjectExistsButNotAsIndex()
        {
            var name = Fixture.Create<string>();
            var db = SqliteDatabaseBuilderMock.Create();
            var sut = db.Schemas.Create( "foo" ).Objects;
            sut.CreateTable( name );

            var result = sut.TryGetIndex( name, out var outResult );

            using ( new AssertionScope() )
            {
                result.Should().BeFalse();
                outResult.Should().BeNull();
            }
        }

        [Fact]
        public void GetPrimaryKey_ShouldReturnExistingPrimaryKey()
        {
            var name = Fixture.Create<string>();
            var db = SqliteDatabaseBuilderMock.Create();
            var sut = db.Schemas.Create( "foo" ).Objects;
            var t = sut.CreateTable( "T" );
            var c = t.Columns.Create( "C" );
            var expected = t.SetPrimaryKey( c.Asc() ).SetName( name );

            var result = sut.GetPrimaryKey( name );

            result.Should().BeSameAs( expected );
        }

        [Fact]
        public void GetPrimaryKey_ShouldThrowKeyNotFoundException_WhenObjectDoesNotExist()
        {
            var name = Fixture.Create<string>();
            var db = SqliteDatabaseBuilderMock.Create();
            var sut = db.Schemas.Create( "foo" ).Objects;

            var action = Lambda.Of( () => sut.GetPrimaryKey( name ) );

            action.Should().ThrowExactly<KeyNotFoundException>();
        }

        [Fact]
        public void GetPrimaryKey_ShouldThrowSqliteObjectCastException_WhenObjectExistsButNotAsPrimaryKey()
        {
            var name = Fixture.Create<string>();
            var db = SqliteDatabaseBuilderMock.Create();
            var sut = db.Schemas.Create( "foo" ).Objects;
            sut.CreateTable( name );

            var action = Lambda.Of( () => sut.GetPrimaryKey( name ) );

            action.Should()
                .ThrowExactly<SqliteObjectCastException>()
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
            var sut = db.Schemas.Create( "foo" ).Objects;
            var t = sut.CreateTable( "T" );
            var c = t.Columns.Create( "C" );
            var expected = t.SetPrimaryKey( c.Asc() ).SetName( name );

            var result = sut.TryGetPrimaryKey( name, out var outResult );

            using ( new AssertionScope() )
            {
                result.Should().BeTrue();
                outResult.Should().BeSameAs( expected );
            }
        }

        [Fact]
        public void TryGetPrimaryKey_ShouldReturnFalse_WhenObjectDoesNotExist()
        {
            var name = Fixture.Create<string>();
            var db = SqliteDatabaseBuilderMock.Create();
            var sut = db.Schemas.Create( "foo" ).Objects;

            var result = sut.TryGetPrimaryKey( name, out var outResult );

            using ( new AssertionScope() )
            {
                result.Should().BeFalse();
                outResult.Should().BeNull();
            }
        }

        [Fact]
        public void TryGetPrimaryKey_ShouldReturnFalse_WhenObjectExistsButNotAsPrimaryKey()
        {
            var name = Fixture.Create<string>();
            var db = SqliteDatabaseBuilderMock.Create();
            var sut = db.Schemas.Create( "foo" ).Objects;
            sut.CreateTable( name );

            var result = sut.TryGetPrimaryKey( name, out var outResult );

            using ( new AssertionScope() )
            {
                result.Should().BeFalse();
                outResult.Should().BeNull();
            }
        }

        [Fact]
        public void GetForeignKey_ShouldReturnExistingForeignKey()
        {
            var name = Fixture.Create<string>();
            var db = SqliteDatabaseBuilderMock.Create();
            var sut = db.Schemas.Create( "foo" ).Objects;
            var t = sut.CreateTable( "T" );
            var c = t.Columns.Create( "C" );
            var d = t.Columns.Create( "D" ).MarkAsNullable();
            var pk = t.SetPrimaryKey( c.Asc() );
            var ix = t.Indexes.Create( d.Asc() );
            var expected = t.ForeignKeys.Create( ix, pk.Index ).SetName( name );

            var result = sut.GetForeignKey( name );

            result.Should().BeSameAs( expected );
        }

        [Fact]
        public void GetForeignKey_ShouldThrowKeyNotFoundException_WhenObjectDoesNotExist()
        {
            var name = Fixture.Create<string>();
            var db = SqliteDatabaseBuilderMock.Create();
            var sut = db.Schemas.Create( "foo" ).Objects;

            var action = Lambda.Of( () => sut.GetForeignKey( name ) );

            action.Should().ThrowExactly<KeyNotFoundException>();
        }

        [Fact]
        public void GetForeignKey_ShouldThrowSqliteObjectCastException_WhenObjectExistsButNotAsForeignKey()
        {
            var name = Fixture.Create<string>();
            var db = SqliteDatabaseBuilderMock.Create();
            var sut = db.Schemas.Create( "foo" ).Objects;
            sut.CreateTable( name );

            var action = Lambda.Of( () => sut.GetForeignKey( name ) );

            action.Should()
                .ThrowExactly<SqliteObjectCastException>()
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
            var sut = db.Schemas.Create( "foo" ).Objects;
            var t = sut.CreateTable( "T" );
            var c = t.Columns.Create( "C" );
            var d = t.Columns.Create( "D" ).MarkAsNullable();
            var pk = t.SetPrimaryKey( c.Asc() );
            var ix = t.Indexes.Create( d.Asc() );
            var expected = t.ForeignKeys.Create( ix, pk.Index ).SetName( name );

            var result = sut.TryGetForeignKey( name, out var outResult );

            using ( new AssertionScope() )
            {
                result.Should().BeTrue();
                outResult.Should().BeSameAs( expected );
            }
        }

        [Fact]
        public void TryGetForeignKey_ShouldReturnFalse_WhenObjectDoesNotExist()
        {
            var name = Fixture.Create<string>();
            var db = SqliteDatabaseBuilderMock.Create();
            var sut = db.Schemas.Create( "foo" ).Objects;

            var result = sut.TryGetForeignKey( name, out var outResult );

            using ( new AssertionScope() )
            {
                result.Should().BeFalse();
                outResult.Should().BeNull();
            }
        }

        [Fact]
        public void TryGetForeignKey_ShouldReturnFalse_WhenObjectExistsButNotAsForeignKey()
        {
            var name = Fixture.Create<string>();
            var db = SqliteDatabaseBuilderMock.Create();
            var sut = db.Schemas.Create( "foo" ).Objects;
            sut.CreateTable( name );

            var result = sut.TryGetForeignKey( name, out var outResult );

            using ( new AssertionScope() )
            {
                result.Should().BeFalse();
                outResult.Should().BeNull();
            }
        }

        [Fact]
        public void GetView_ShouldReturnExistingView()
        {
            var name = Fixture.Create<string>();
            var db = SqliteDatabaseBuilderMock.Create();
            var sut = db.Schemas.Create( "foo" ).Objects;
            var expected = sut.CreateView( name, SqlNode.RawQuery( "SELECT * FROM foo" ) );

            var result = sut.GetView( name );

            result.Should().BeSameAs( expected );
        }

        [Fact]
        public void GetView_ShouldThrowKeyNotFoundException_WhenObjectDoesNotExist()
        {
            var name = Fixture.Create<string>();
            var db = SqliteDatabaseBuilderMock.Create();
            var sut = db.Schemas.Create( "foo" ).Objects;

            var action = Lambda.Of( () => sut.GetView( name ) );

            action.Should().ThrowExactly<KeyNotFoundException>();
        }

        [Fact]
        public void GetView_ShouldThrowSqliteObjectCastException_WhenObjectExistsButNotAsView()
        {
            var name = Fixture.Create<string>();
            var db = SqliteDatabaseBuilderMock.Create();
            var sut = db.Schemas.Create( "foo" ).Objects;
            sut.CreateTable( name );

            var action = Lambda.Of( () => sut.GetView( name ) );

            action.Should()
                .ThrowExactly<SqliteObjectCastException>()
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
            var sut = db.Schemas.Create( "foo" ).Objects;
            var expected = sut.CreateView( name, SqlNode.RawQuery( "SELECT * FROM foo" ) );

            var result = sut.TryGetView( name, out var outResult );

            using ( new AssertionScope() )
            {
                result.Should().BeTrue();
                outResult.Should().BeSameAs( expected );
            }
        }

        [Fact]
        public void TryGetView_ShouldReturnFalse_WhenObjectDoesNotExist()
        {
            var name = Fixture.Create<string>();
            var db = SqliteDatabaseBuilderMock.Create();
            var sut = db.Schemas.Create( "foo" ).Objects;

            var result = sut.TryGetView( name, out var outResult );

            using ( new AssertionScope() )
            {
                result.Should().BeFalse();
                outResult.Should().BeNull();
            }
        }

        [Fact]
        public void TryGetView_ShouldReturnFalse_WhenObjectExistsButNotAsView()
        {
            var name = Fixture.Create<string>();
            var db = SqliteDatabaseBuilderMock.Create();
            var sut = db.Schemas.Create( "foo" ).Objects;
            sut.CreateTable( name );

            var result = sut.TryGetView( name, out var outResult );

            using ( new AssertionScope() )
            {
                result.Should().BeFalse();
                outResult.Should().BeNull();
            }
        }

        [Fact]
        public void GetCheck_ShouldReturnExistingCheck()
        {
            var name = Fixture.Create<string>();
            var db = SqliteDatabaseBuilderMock.Create();
            var sut = db.Schemas.Create( "foo" ).Objects;
            var t = sut.CreateTable( "T" );
            var c = t.Columns.Create( "C" );
            var expected = t.Checks.Create( c.Node != null ).SetName( name );

            var result = sut.GetCheck( name );

            result.Should().BeSameAs( expected );
        }

        [Fact]
        public void GetCheck_ShouldThrowKeyNotFoundException_WhenObjectDoesNotExist()
        {
            var name = Fixture.Create<string>();
            var db = SqliteDatabaseBuilderMock.Create();
            var sut = db.Schemas.Create( "foo" ).Objects;

            var action = Lambda.Of( () => sut.GetCheck( name ) );

            action.Should().ThrowExactly<KeyNotFoundException>();
        }

        [Fact]
        public void GetCheck_ShouldThrowSqliteObjectCastException_WhenObjectExistsButNotAsCheck()
        {
            var name = Fixture.Create<string>();
            var db = SqliteDatabaseBuilderMock.Create();
            var sut = db.Schemas.Create( "foo" ).Objects;
            sut.CreateTable( name );

            var action = Lambda.Of( () => sut.GetCheck( name ) );

            action.Should()
                .ThrowExactly<SqliteObjectCastException>()
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
            var sut = db.Schemas.Create( "foo" ).Objects;
            var t = sut.CreateTable( "T" );
            var c = t.Columns.Create( "C" );
            var expected = t.Checks.Create( c.Node != null ).SetName( name );

            var result = sut.TryGetCheck( name, out var outResult );

            using ( new AssertionScope() )
            {
                result.Should().BeTrue();
                outResult.Should().BeSameAs( expected );
            }
        }

        [Fact]
        public void TryGetCheck_ShouldReturnFalse_WhenObjectDoesNotExist()
        {
            var name = Fixture.Create<string>();
            var db = SqliteDatabaseBuilderMock.Create();
            var sut = db.Schemas.Create( "foo" ).Objects;

            var result = sut.TryGetCheck( name, out var outResult );

            using ( new AssertionScope() )
            {
                result.Should().BeFalse();
                outResult.Should().BeNull();
            }
        }

        [Fact]
        public void TryGetCheck_ShouldReturnFalse_WhenObjectExistsButNotAsCheck()
        {
            var name = Fixture.Create<string>();
            var db = SqliteDatabaseBuilderMock.Create();
            var sut = db.Schemas.Create( "foo" ).Objects;
            sut.CreateTable( name );

            var result = sut.TryGetCheck( name, out var outResult );

            using ( new AssertionScope() )
            {
                result.Should().BeFalse();
                outResult.Should().BeNull();
            }
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
            var pk = table.SetPrimaryKey( column.Asc() );
            var ix = table.Indexes.Create( otherColumn.Asc() );
            var fk = table.ForeignKeys.Create( ix, pk.Index );
            var chk = table.Checks.Create( column.Node != null );

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
            var pk = table.SetPrimaryKey( column.Asc() );
            var chk = table.Checks.Create( column.Node != null );

            var otherTable = sut.CreateTable( "U" );
            var otherColumn = otherTable.Columns.Create( "D" );
            var otherPk = otherTable.SetPrimaryKey( otherColumn.Asc() );
            var fk = otherTable.ForeignKeys.Create( otherPk.Index, pk.Index );

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
            var pk = table.SetPrimaryKey( column.Asc() );

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
            var pk = table.SetPrimaryKey( column.Asc() );

            var result = sut.Remove( pk.Name );

            using ( new AssertionScope() )
            {
                result.Should().BeTrue();
                sut.Count.Should().Be( 1 );
                sut.Should().BeEquivalentTo( table );
                table.PrimaryKey.Should().BeNull();
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
            var pk = table.SetPrimaryKey( column.Asc() );

            var otherTable = sut.CreateTable( "U" );
            var otherColumn = otherTable.Columns.Create( "D" );
            var otherPk = otherTable.SetPrimaryKey( otherColumn.Asc() );
            var fk = otherTable.ForeignKeys.Create( otherPk.Index, pk.Index );

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
            var pk = table.SetPrimaryKey( column.Asc() );
            var ix = table.Indexes.Create( otherColumn.Asc() );

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
            var pk = table.SetPrimaryKey( column.Asc() );
            var ix = table.Indexes.Create( otherColumn.Asc() ).MarkAsUnique();

            var otherTable = sut.CreateTable( "U" );
            var externalColumn = otherTable.Columns.Create( "D" );
            var otherPk = otherTable.SetPrimaryKey( externalColumn.Asc() );
            var fk = otherTable.ForeignKeys.Create( otherPk.Index, ix );

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
            var pk = table.SetPrimaryKey( column.Asc() );

            var otherTable = sut.CreateTable( "U" );
            var otherColumn = otherTable.Columns.Create( "D" );
            var otherPk = otherTable.SetPrimaryKey( otherColumn.Asc() );
            var fk = otherTable.ForeignKeys.Create( otherPk.Index, pk.Index );

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
            var pk = table.SetPrimaryKey( column.Asc() );
            var chk = table.Checks.Create( column.Node != null );

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

        [Fact]
        public void ISqlObjectBuilderCollection_GetTable_ShouldBeEquivalentToGetTable()
        {
            var name = Fixture.Create<string>();
            var db = SqliteDatabaseBuilderMock.Create();
            var sut = db.Schemas.Create( "foo" ).Objects;
            sut.CreateTable( name );

            var result = ((ISqlObjectBuilderCollection)sut).GetTable( name );

            result.Should().BeSameAs( sut.GetTable( name ) );
        }

        [Fact]
        public void ISqlObjectBuilderCollection_TryGetTable_ShouldBeEquivalentToTryGetTable()
        {
            var name = Fixture.Create<string>();
            var db = SqliteDatabaseBuilderMock.Create();
            var sut = db.Schemas.Create( "foo" ).Objects;
            sut.CreateTable( name );

            var result = ((ISqlObjectBuilderCollection)sut).TryGetTable( name, out var outResult );

            using ( new AssertionScope() )
            {
                result.Should().Be( sut.TryGetTable( name, out var outExpected ) );
                outResult.Should().BeSameAs( outExpected );
            }
        }

        [Fact]
        public void ISqlObjectBuilderCollection_GetIndex_ShouldBeEquivalentToGetIndex()
        {
            var name = Fixture.Create<string>();
            var db = SqliteDatabaseBuilderMock.Create();
            var sut = db.Schemas.Create( "foo" ).Objects;
            var t = sut.CreateTable( "T" );
            var c = t.Columns.Create( "C" );
            t.Indexes.Create( c.Asc() ).SetName( name );

            var result = ((ISqlObjectBuilderCollection)sut).GetIndex( name );

            result.Should().BeSameAs( sut.GetIndex( name ) );
        }

        [Fact]
        public void ISqlObjectBuilderCollection_TryGetIndex_ShouldBeEquivalentToTryGetIndex()
        {
            var name = Fixture.Create<string>();
            var db = SqliteDatabaseBuilderMock.Create();
            var sut = db.Schemas.Create( "foo" ).Objects;
            var t = sut.CreateTable( "T" );
            var c = t.Columns.Create( "C" );
            t.Indexes.Create( c.Asc() ).SetName( name );

            var result = ((ISqlObjectBuilderCollection)sut).TryGetIndex( name, out var outResult );

            using ( new AssertionScope() )
            {
                result.Should().Be( sut.TryGetIndex( name, out var outExpected ) );
                outResult.Should().BeSameAs( outExpected );
            }
        }

        [Fact]
        public void ISqlObjectBuilderCollection_GetPrimaryKey_ShouldBeEquivalentToGetPrimaryKey()
        {
            var name = Fixture.Create<string>();
            var db = SqliteDatabaseBuilderMock.Create();
            var sut = db.Schemas.Create( "foo" ).Objects;
            var t = sut.CreateTable( "T" );
            var c = t.Columns.Create( "C" );
            t.SetPrimaryKey( c.Asc() ).SetName( name );

            var result = ((ISqlObjectBuilderCollection)sut).GetPrimaryKey( name );

            result.Should().BeSameAs( sut.GetPrimaryKey( name ) );
        }

        [Fact]
        public void ISqlObjectBuilderCollection_TryGetPrimaryKey_ShouldBeEquivalentToTryGetPrimaryKey()
        {
            var name = Fixture.Create<string>();
            var db = SqliteDatabaseBuilderMock.Create();
            var sut = db.Schemas.Create( "foo" ).Objects;
            var t = sut.CreateTable( "T" );
            var c = t.Columns.Create( "C" );
            t.SetPrimaryKey( c.Asc() ).SetName( name );

            var result = ((ISqlObjectBuilderCollection)sut).TryGetPrimaryKey( name, out var outResult );

            using ( new AssertionScope() )
            {
                result.Should().Be( sut.TryGetPrimaryKey( name, out var outExpected ) );
                outResult.Should().BeSameAs( outExpected );
            }
        }

        [Fact]
        public void ISqlObjectBuilderCollection_GetForeignKey_ShouldBeEquivalentToGetForeignKey()
        {
            var name = Fixture.Create<string>();
            var db = SqliteDatabaseBuilderMock.Create();
            var sut = db.Schemas.Create( "foo" ).Objects;
            var t = sut.CreateTable( "T" );
            var c = t.Columns.Create( "C" );
            var d = t.Columns.Create( "D" );
            var pk = t.SetPrimaryKey( c.Asc() );
            t.ForeignKeys.Create( t.Indexes.Create( d.Asc() ), pk.Index ).SetName( name );

            var result = ((ISqlObjectBuilderCollection)sut).GetForeignKey( name );

            result.Should().BeSameAs( sut.GetForeignKey( name ) );
        }

        [Fact]
        public void ISqlObjectBuilderCollection_TryGetForeignKey_ShouldBeEquivalentToTryGetForeignKey()
        {
            var name = Fixture.Create<string>();
            var db = SqliteDatabaseBuilderMock.Create();
            var sut = db.Schemas.Create( "foo" ).Objects;
            var t = sut.CreateTable( "T" );
            var c = t.Columns.Create( "C" );
            var d = t.Columns.Create( "D" );
            var pk = t.SetPrimaryKey( c.Asc() );
            t.ForeignKeys.Create( t.Indexes.Create( d.Asc() ), pk.Index ).SetName( name );

            var result = ((ISqlObjectBuilderCollection)sut).TryGetForeignKey( name, out var outResult );

            using ( new AssertionScope() )
            {
                result.Should().Be( sut.TryGetForeignKey( name, out var outExpected ) );
                outResult.Should().BeSameAs( outExpected );
            }
        }

        [Fact]
        public void ISqlObjectBuilderCollection_GetView_ShouldBeEquivalentToGetView()
        {
            var name = Fixture.Create<string>();
            var db = SqliteDatabaseBuilderMock.Create();
            var sut = db.Schemas.Create( "foo" ).Objects;
            sut.CreateView( name, SqlNode.RawQuery( "SELECT * FROM foo" ) );

            var result = ((ISqlObjectBuilderCollection)sut).GetView( name );

            result.Should().BeSameAs( sut.GetView( name ) );
        }

        [Fact]
        public void ISqlObjectBuilderCollection_TryGetView_ShouldBeEquivalentToTryGetView()
        {
            var name = Fixture.Create<string>();
            var db = SqliteDatabaseBuilderMock.Create();
            var sut = db.Schemas.Create( "foo" ).Objects;
            sut.CreateView( name, SqlNode.RawQuery( "SELECT * FROM foo" ) );

            var result = ((ISqlObjectBuilderCollection)sut).TryGetView( name, out var outResult );

            using ( new AssertionScope() )
            {
                result.Should().Be( sut.TryGetView( name, out var outExpected ) );
                outResult.Should().BeSameAs( outExpected );
            }
        }

        [Fact]
        public void ISqlObjectBuilderCollection_GetCheck_ShouldBeEquivalentToGetCheck()
        {
            var name = Fixture.Create<string>();
            var db = SqliteDatabaseBuilderMock.Create();
            var sut = db.Schemas.Create( "foo" ).Objects;
            var t = sut.CreateTable( "T" );
            var c = t.Columns.Create( "C" );
            t.Checks.Create( c.Node != null ).SetName( name );

            var result = ((ISqlObjectBuilderCollection)sut).GetCheck( name );

            result.Should().BeSameAs( sut.GetCheck( name ) );
        }

        [Fact]
        public void ISqlObjectBuilderCollection_TryGetCheck_ShouldBeEquivalentToTryGetCheck()
        {
            var name = Fixture.Create<string>();
            var db = SqliteDatabaseBuilderMock.Create();
            var sut = db.Schemas.Create( "foo" ).Objects;
            var t = sut.CreateTable( "T" );
            var c = t.Columns.Create( "C" );
            t.Checks.Create( c.Node != null ).SetName( name );

            var result = ((ISqlObjectBuilderCollection)sut).TryGetCheck( name, out var outResult );

            using ( new AssertionScope() )
            {
                result.Should().Be( sut.TryGetCheck( name, out var outExpected ) );
                outResult.Should().BeSameAs( outExpected );
            }
        }
    }
}
