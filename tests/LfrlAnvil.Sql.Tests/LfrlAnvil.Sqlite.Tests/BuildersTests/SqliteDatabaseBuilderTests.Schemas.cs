using System.Collections.Generic;
using LfrlAnvil.Functional;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.Sqlite.Exceptions;
using LfrlAnvil.Sqlite.Extensions;
using LfrlAnvil.Sqlite.Objects.Builders;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.Sqlite.Tests.BuildersTests;

public partial class SqliteDatabaseBuilderTests
{
    public class Schemas : TestsBase
    {
        [Fact]
        public void Create_ShouldCreateNewSchema()
        {
            var name = Fixture.Create<string>();
            var sut = new SqliteDatabaseBuilder();

            var result = ((ISqlSchemaBuilderCollection)sut.Schemas).Create( name );

            using ( new AssertionScope() )
            {
                result.Database.Should().BeSameAs( sut );
                result.Name.Should().Be( name );
                result.FullName.Should().Be( name );
                result.Objects.Should().BeEmpty();
                result.Type.Should().Be( SqlObjectType.Schema );

                sut.Schemas.Get( name ).Should().BeSameAs( result );
                sut.Schemas.Count.Should().Be( 2 );
                sut.Schemas.Should().BeEquivalentTo( sut.Schemas.Default, result );
            }
        }

        [Theory]
        [InlineData( "" )]
        [InlineData( " " )]
        [InlineData( "\"" )]
        [InlineData( "'" )]
        [InlineData( "f\"oo" )]
        public void Create_ShouldThrowSqliteObjectBuilderException_WhenNameIsInvalid(string name)
        {
            var sut = new SqliteDatabaseBuilder();

            var action = Lambda.Of( () => sut.Schemas.Create( name ) );

            action.Should()
                .ThrowExactly<SqliteObjectBuilderException>()
                .AndMatch( e => e.Dialect == SqliteDialect.Instance && e.Errors.Count == 1 );
        }

        [Fact]
        public void Create_ShouldThrowSqliteObjectBuilderException_WhenSchemaWithNameAlreadyExists()
        {
            var name = Fixture.Create<string>();
            var sut = new SqliteDatabaseBuilder();
            sut.Schemas.Create( name );

            var action = Lambda.Of( () => sut.Schemas.Create( name ) );

            action.Should()
                .ThrowExactly<SqliteObjectBuilderException>()
                .AndMatch( e => e.Dialect == SqliteDialect.Instance && e.Errors.Count == 1 );
        }

        [Fact]
        public void GetOrCreate_ShouldCreateNewSchema()
        {
            var name = Fixture.Create<string>();
            var sut = new SqliteDatabaseBuilder();

            var result = ((ISqlSchemaBuilderCollection)sut.Schemas).GetOrCreate( name );

            using ( new AssertionScope() )
            {
                result.Database.Should().BeSameAs( sut );
                result.Name.Should().Be( name );
                result.FullName.Should().Be( name );
                result.Objects.Should().BeEmpty();
                result.Type.Should().Be( SqlObjectType.Schema );

                sut.Schemas.Get( name ).Should().BeSameAs( result );
                sut.Schemas.Count.Should().Be( 2 );
                sut.Schemas.Should().BeEquivalentTo( sut.Schemas.Default, result );
            }
        }

        [Theory]
        [InlineData( " " )]
        [InlineData( "\"" )]
        [InlineData( "f\"oo" )]
        public void GetOrCreate_ShouldThrowSqliteObjectBuilderException_WhenNameIsInvalid(string name)
        {
            var sut = new SqliteDatabaseBuilder();

            var action = Lambda.Of( () => sut.Schemas.Create( name ) );

            action.Should()
                .ThrowExactly<SqliteObjectBuilderException>()
                .AndMatch( e => e.Dialect == SqliteDialect.Instance && e.Errors.Count == 1 );
        }

        [Fact]
        public void GetOrCreate_ShouldReturnDefaultSchema_WhenNameIsEmpty()
        {
            var sut = new SqliteDatabaseBuilder();
            var result = sut.Schemas.GetOrCreate( string.Empty );
            result.Should().BeSameAs( sut.Schemas.Default );
        }

        [Fact]
        public void GetOrCreate_ShouldReturnExistingSchema_WhenSchemaWithNameAlreadyExists()
        {
            var name = Fixture.Create<string>();
            var sut = new SqliteDatabaseBuilder();
            var expected = sut.Schemas.Create( name );

            var result = sut.Schemas.GetOrCreate( name );

            using ( new AssertionScope() )
            {
                result.Should().BeSameAs( expected );
                sut.Schemas.Count.Should().Be( 2 );
            }
        }

        [Theory]
        [InlineData( "", true )]
        [InlineData( "foo", true )]
        [InlineData( "bar", false )]
        public void Contains_ShouldReturnTrue_WhenSchemaExists(string name, bool expected)
        {
            var sut = new SqliteDatabaseBuilder();
            sut.Schemas.Create( "foo" );

            var result = sut.Schemas.Contains( name );

            result.Should().Be( expected );
        }

        [Fact]
        public void Get_ShouldReturnExistingSchema()
        {
            var name = Fixture.Create<string>();
            var sut = new SqliteDatabaseBuilder();
            var expected = sut.Schemas.Create( name );

            var result = ((ISqlSchemaBuilderCollection)sut.Schemas).Get( name );

            result.Should().BeSameAs( expected );
        }

        [Fact]
        public void Get_ShouldThrowKeyNotFoundException_WhenSchemaDoesNotExist()
        {
            var name = Fixture.Create<string>();
            var sut = new SqliteDatabaseBuilder();

            var action = Lambda.Of( () => ((ISqlSchemaBuilderCollection)sut.Schemas).Get( name ) );

            action.Should().ThrowExactly<KeyNotFoundException>();
        }

        [Fact]
        public void TryGet_ShouldReturnExistingSchema()
        {
            var name = Fixture.Create<string>();
            var sut = new SqliteDatabaseBuilder();
            var expected = sut.Schemas.Create( name );

            var result = ((ISqlSchemaBuilderCollection)sut.Schemas).TryGet( name, out var outResult );

            using ( new AssertionScope() )
            {
                result.Should().BeTrue();
                outResult.Should().BeSameAs( expected );
            }
        }

        [Fact]
        public void TryGet_ShouldReturnFalse_WhenSchemaDoesNotExist()
        {
            var name = Fixture.Create<string>();
            var sut = new SqliteDatabaseBuilder();

            var result = ((ISqlSchemaBuilderCollection)sut.Schemas).TryGet( name, out var outResult );

            using ( new AssertionScope() )
            {
                result.Should().BeFalse();
                outResult.Should().BeNull();
            }
        }

        [Fact]
        public void Remove_ShouldRemoveExistingEmptySchema()
        {
            var name = Fixture.Create<string>();
            var sut = new SqliteDatabaseBuilder();
            var schema = sut.Schemas.Create( name );

            var result = sut.Schemas.Remove( name );

            using ( new AssertionScope() )
            {
                result.Should().BeTrue();
                sut.Schemas.Count.Should().Be( 1 );
                sut.Schemas.Should().BeEquivalentTo( sut.Schemas.Default );
                schema.IsRemoved.Should().BeTrue();
            }
        }

        [Fact]
        public void Remove_ShouldRemoveExistingNonEmptySchema()
        {
            var name = Fixture.Create<string>();
            var sut = new SqliteDatabaseBuilder();
            var schema = sut.Schemas.Create( name );
            var table = schema.Objects.CreateTable( "T1" );
            var column = table.Columns.Create( "C1" );
            var otherColumn = table.Columns.Create( "C2" ).MarkAsNullable();
            var pk = table.SetPrimaryKey( column.Asc() );
            var fk = table.ForeignKeys.Create( table.Indexes.Create( otherColumn.Asc() ), table.PrimaryKey!.Index );

            var result = sut.Schemas.Remove( name );

            using ( new AssertionScope() )
            {
                result.Should().BeTrue();
                sut.Schemas.Count.Should().Be( 1 );
                sut.Schemas.Should().BeEquivalentTo( sut.Schemas.Default );
                schema.IsRemoved.Should().BeTrue();
                table.IsRemoved.Should().BeTrue();
                column.IsRemoved.Should().BeTrue();
                otherColumn.IsRemoved.Should().BeTrue();
                pk.IsRemoved.Should().BeTrue();
                pk.Index.IsRemoved.Should().BeTrue();
                fk.IsRemoved.Should().BeTrue();
            }
        }

        [Fact]
        public void Remove_ShouldReturnFalse_WhenSchemaDoesNotExist()
        {
            var name = Fixture.Create<string>();
            var sut = new SqliteDatabaseBuilder();

            var result = sut.Schemas.Remove( name );

            result.Should().BeFalse();
        }

        [Fact]
        public void Remove_ShouldReturnFalse_WhenTryingToRemoveDefaultSchema()
        {
            var sut = new SqliteDatabaseBuilder();
            var result = sut.Schemas.Remove( string.Empty );
            result.Should().BeFalse();
        }

        [Fact]
        public void Remove_ShouldReturnFalse_WhenTryingToRemoveSchemaWithTableReferencedByExternalSchema()
        {
            var sut = new SqliteDatabaseBuilder();
            var schema = sut.Schemas.Create( Fixture.Create<string>() );
            var table = schema.Objects.CreateTable( "T1" );
            var column = table.Columns.Create( "C1" );
            table.SetPrimaryKey( column.Asc() );

            var otherTable = sut.Schemas.Default.Objects.CreateTable( "T2" );
            var otherColumn = otherTable.Columns.Create( "C2" );
            otherTable.SetPrimaryKey( otherColumn.Asc() );
            otherTable.ForeignKeys.Create( otherTable.PrimaryKey!.Index, table.PrimaryKey!.Index );

            var result = sut.Schemas.Remove( schema.Name );

            result.Should().BeFalse();
        }
    }
}
