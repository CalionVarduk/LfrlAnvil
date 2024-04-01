using System.Collections.Generic;
using LfrlAnvil.Functional;
using LfrlAnvil.PostgreSql.Extensions;
using LfrlAnvil.PostgreSql.Objects.Builders;
using LfrlAnvil.PostgreSql.Tests.Helpers;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.PostgreSql.Tests.ObjectsTests.BuildersTests;

public partial class PostgreSqlDatabaseBuilderTests
{
    public class Schemas : TestsBase
    {
        [Fact]
        public void Create_ShouldCreateNewSchema()
        {
            var sut = PostgreSqlDatabaseBuilderMock.Create().Schemas;

            var result = sut.Create( "foo" );

            using ( new AssertionScope() )
            {
                result.Database.Should().BeSameAs( sut.Database );
                result.Type.Should().Be( SqlObjectType.Schema );
                result.Name.Should().Be( "foo" );
                result.Objects.Should().BeEmpty();
                result.Objects.Schema.Should().BeSameAs( result );
                result.ReferencingObjects.Should().BeEmpty();

                sut.Count.Should().Be( 2 );
                sut.Should().BeEquivalentTo( sut.Database.Schemas.Default, result );
                sut.TryGet( result.Name ).Should().BeSameAs( result );
            }
        }

        [Fact]
        public void Create_ShouldThrowSqlObjectBuilderException_WhenSchemaWithNameAlreadyExists()
        {
            var sut = PostgreSqlDatabaseBuilderMock.Create().Schemas;

            var action = Lambda.Of( () => sut.Create( sut.Database.Schemas.Default.Name ) );

            action.Should()
                .ThrowExactly<SqlObjectBuilderException>()
                .AndMatch( e => e.Dialect == PostgreSqlDialect.Instance && e.Errors.Count == 1 );
        }

        [Theory]
        [InlineData( "" )]
        [InlineData( " " )]
        [InlineData( "\"" )]
        [InlineData( "'" )]
        [InlineData( "f\"oo" )]
        public void Create_ShouldThrowSqlObjectBuilderException_WhenNameIsInvalid(string name)
        {
            var sut = PostgreSqlDatabaseBuilderMock.Create().Schemas;

            var action = Lambda.Of( () => sut.Create( name ) );

            action.Should()
                .ThrowExactly<SqlObjectBuilderException>()
                .AndMatch( e => e.Dialect == PostgreSqlDialect.Instance && e.Errors.Count == 1 );
        }

        [Fact]
        public void GetOrCreate_ShouldCreateNewSchema_WhenSchemaDoesNotExist()
        {
            var sut = PostgreSqlDatabaseBuilderMock.Create().Schemas;

            var result = sut.GetOrCreate( "foo" );

            using ( new AssertionScope() )
            {
                result.Database.Should().BeSameAs( sut.Database );
                result.Type.Should().Be( SqlObjectType.Schema );
                result.Name.Should().Be( "foo" );
                result.Objects.Should().BeEmpty();
                result.Objects.Schema.Should().BeSameAs( result );
                result.ReferencingObjects.Should().BeEmpty();

                sut.Count.Should().Be( 2 );
                sut.Should().BeEquivalentTo( sut.Database.Schemas.Default, result );
                sut.TryGet( result.Name ).Should().BeSameAs( result );
            }
        }

        [Fact]
        public void GetOrCreate_ShouldReturnExistingSchema_WhenSchemaWithNameAlreadyExists()
        {
            var sut = PostgreSqlDatabaseBuilderMock.Create().Schemas;
            var expected = sut.Default;

            var result = sut.GetOrCreate( expected.Name );

            using ( new AssertionScope() )
            {
                result.Should().BeSameAs( expected );
                sut.Count.Should().Be( 1 );
            }
        }

        [Theory]
        [InlineData( "" )]
        [InlineData( " " )]
        [InlineData( "\"" )]
        [InlineData( "'" )]
        [InlineData( "f\"oo" )]
        public void GetOrCreate_ShouldThrowPostgreSqlObjectBuilderException_WhenNameIsInvalid(string name)
        {
            var sut = PostgreSqlDatabaseBuilderMock.Create().Schemas;

            var action = Lambda.Of( () => sut.Create( name ) );

            action.Should()
                .ThrowExactly<SqlObjectBuilderException>()
                .AndMatch( e => e.Dialect == PostgreSqlDialect.Instance && e.Errors.Count == 1 );
        }

        [Theory]
        [InlineData( "public", true )]
        [InlineData( "foo", true )]
        [InlineData( "bar", false )]
        public void Contains_ShouldReturnTrue_WhenSchemaExists(string name, bool expected)
        {
            var sut = PostgreSqlDatabaseBuilderMock.Create();
            sut.Schemas.Create( "foo" );

            var result = sut.Schemas.Contains( name );

            result.Should().Be( expected );
        }

        [Fact]
        public void Get_ShouldReturnExistingSchema()
        {
            var sut = PostgreSqlDatabaseBuilderMock.Create().Schemas;
            var expected = sut.Create( "foo" );

            var result = sut.Get( "foo" );

            result.Should().BeSameAs( expected );
        }

        [Fact]
        public void Get_ShouldThrowKeyNotFoundException_WhenSchemaDoesNotExist()
        {
            var sut = PostgreSqlDatabaseBuilderMock.Create().Schemas;
            var action = Lambda.Of( () => sut.Get( "foo" ) );
            action.Should().ThrowExactly<KeyNotFoundException>();
        }

        [Fact]
        public void TryGet_ShouldReturnExistingSchema()
        {
            var sut = PostgreSqlDatabaseBuilderMock.Create().Schemas;
            var expected = sut.Create( "foo" );

            var result = sut.TryGet( "foo" );

            result.Should().BeSameAs( expected );
        }

        [Fact]
        public void TryGet_ShouldReturnNull_WhenSchemaDoesNotExist()
        {
            var sut = PostgreSqlDatabaseBuilderMock.Create().Schemas;
            var result = sut.TryGet( "foo" );
            result.Should().BeNull();
        }

        [Fact]
        public void Remove_ShouldRemoveExistingNonEmptySchema()
        {
            var sut = PostgreSqlDatabaseBuilderMock.Create().Schemas;
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

            using ( new AssertionScope() )
            {
                result.Should().BeTrue();
                sut.TryGet( schema.Name ).Should().BeNull();

                schema.IsRemoved.Should().BeTrue();
                schema.ReferencingObjects.Should().BeEmpty();
                schema.Objects.Should().BeEmpty();
                c1.IsRemoved.Should().BeTrue();
                c1.ReferencingObjects.Should().BeEmpty();
                c2.IsRemoved.Should().BeTrue();
                c2.ReferencingObjects.Should().BeEmpty();
                pk.IsRemoved.Should().BeTrue();
                pk.ReferencingObjects.Should().BeEmpty();
                pk.Index.IsRemoved.Should().BeTrue();
                pk.Index.ReferencingObjects.Should().BeEmpty();
                pk.Index.Columns.Expressions.Should().BeEmpty();
                pk.Index.PrimaryKey.Should().BeNull();
                ix.IsRemoved.Should().BeTrue();
                ix.ReferencingObjects.Should().BeEmpty();
                ix.Columns.Expressions.Should().BeEmpty();
                selfFk.IsRemoved.Should().BeTrue();
                selfFk.ReferencingObjects.Should().BeEmpty();
                externalFk.IsRemoved.Should().BeTrue();
                externalFk.ReferencingObjects.Should().BeEmpty();
                chk.IsRemoved.Should().BeTrue();
                chk.ReferencingObjects.Should().BeEmpty();
                chk.ReferencedColumns.Should().BeEmpty();
                view.IsRemoved.Should().BeTrue();
                view.ReferencingObjects.Should().BeEmpty();
                view.ReferencedObjects.Should().BeEmpty();

                otherPk.Index.ReferencingObjects.Should().BeEmpty();
                otherTable.ReferencingObjects.Should().BeEmpty();
                table.ReferencingObjects.Should().BeEmpty();
            }
        }

        [Fact]
        public void Remove_ShouldReturnFalse_WhenSchemaDoesNotExist()
        {
            var sut = PostgreSqlDatabaseBuilderMock.Create().Schemas;
            var result = sut.Remove( "foo" );
            result.Should().BeFalse();
        }

        [Fact]
        public void Remove_ShouldReturnFalse_WhenSchemaIsDefault()
        {
            var sut = PostgreSqlDatabaseBuilderMock.Create().Schemas;

            var result = sut.Remove( sut.Database.Schemas.Default.Name );

            using ( new AssertionScope() )
            {
                result.Should().BeFalse();
                sut.Database.Schemas.Default.IsRemoved.Should().BeFalse();
                sut.Count.Should().Be( 1 );
                sut.TryGet( sut.Database.Schemas.Default.Name ).Should().BeSameAs( sut.Database.Schemas.Default );
            }
        }

        [Fact]
        public void Remove_ShouldReturnFalse_WhenSchemaIsReferencedByForeignKeyFromAnotherSchema()
        {
            var sut = PostgreSqlDatabaseBuilderMock.Create().Schemas;
            var schema = sut.Create( "foo" );
            var table = schema.Objects.CreateTable( "T1" );
            var pk = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );

            var otherTable = sut.Default.Objects.CreateTable( "T2" );
            var otherPk = otherTable.Constraints.SetPrimaryKey( otherTable.Columns.Create( "C2" ).Asc() );
            otherTable.Constraints.CreateForeignKey( otherPk.Index, pk.Index );

            var result = sut.Remove( schema.Name );

            using ( new AssertionScope() )
            {
                result.Should().BeFalse();
                schema.IsRemoved.Should().BeFalse();
                sut.Count.Should().Be( 2 );
                sut.TryGet( schema.Name ).Should().BeSameAs( schema );
            }
        }

        [Fact]
        public void Remove_ShouldReturnFalse_WhenSchemaIsReferencedByViewFromAnotherSchema()
        {
            var sut = PostgreSqlDatabaseBuilderMock.Create().Schemas;
            var schema = sut.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            table.Constraints.SetPrimaryKey( table.Columns.Create( "C" ).Asc() );

            sut.Default.Objects.CreateView( "V", table.Node.ToDataSource().Select( s => new[] { s.GetAll() } ) );

            var result = sut.Remove( schema.Name );

            using ( new AssertionScope() )
            {
                result.Should().BeFalse();
                schema.IsRemoved.Should().BeFalse();
                sut.Count.Should().Be( 2 );
                sut.TryGet( schema.Name ).Should().BeSameAs( schema );
            }
        }

        [Fact]
        public void GetEnumerator_ShouldReturnCorrectResult()
        {
            var sut = PostgreSqlDatabaseBuilderMock.Create().Schemas;
            var schema = sut.Create( "foo" );

            var result = new List<PostgreSqlSchemaBuilder>();
            foreach ( var s in sut )
                result.Add( s );

            using ( new AssertionScope() )
            {
                result.Should().HaveCount( 2 );
                result.Should().BeEquivalentTo( sut.Default, schema );
            }
        }
    }
}
