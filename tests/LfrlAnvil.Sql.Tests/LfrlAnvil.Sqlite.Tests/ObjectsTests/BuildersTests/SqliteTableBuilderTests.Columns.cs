using System.Collections.Generic;
using LfrlAnvil.Functional;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.Sqlite.Exceptions;
using LfrlAnvil.Sqlite.Extensions;
using LfrlAnvil.Sqlite.Objects.Builders;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.Sqlite.Tests.ObjectsTests.BuildersTests;

public partial class SqliteTableBuilderTests
{
    public class Columns : TestsBase
    {
        [Fact]
        public void Create_ShouldCreateNewColumn()
        {
            var schema = new SqliteDatabaseBuilder().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T1" );
            var sut = table.Columns;

            var result = ((ISqlColumnBuilderCollection)sut).Create( "C" );

            using ( new AssertionScope() )
            {
                result.Table.Should().BeSameAs( table );
                result.Database.Should().BeSameAs( table.Database );
                result.Name.Should().Be( "C" );
                result.FullName.Should().Be( "foo_T1.C" );
                result.IsNullable.Should().BeFalse();
                result.TypeDefinition.Should().BeSameAs( sut.DefaultTypeDefinition );
                result.DefaultValue.Should().BeNull();
                result.Indexes.Should().BeEmpty();
                sut.Count.Should().Be( 1 );
                sut.Should().BeSequentiallyEqualTo( result );
            }
        }

        [Fact]
        public void Create_ShouldThrowSqliteObjectBuilderException_WhenColumnAlreadyExists()
        {
            var schema = new SqliteDatabaseBuilder().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Columns;
            sut.Create( "C" );

            var action = Lambda.Of( () => sut.Create( "C" ) );

            action.Should()
                .ThrowExactly<SqliteObjectBuilderException>()
                .AndMatch( e => e.Dialect == SqliteDialect.Instance && e.Errors.Count == 1 );
        }

        [Fact]
        public void Create_ShouldThrowSqliteObjectBuilderException_WhenTableIsRemoved()
        {
            var schema = new SqliteDatabaseBuilder().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Columns;
            table.Remove();

            var action = Lambda.Of( () => sut.Create( "C" ) );

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
        public void Create_ShouldThrowSqliteObjectBuilderException_WhenNameIsInvalid(string name)
        {
            var schema = new SqliteDatabaseBuilder().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Columns;

            var action = Lambda.Of( () => sut.Create( name ) );

            action.Should()
                .ThrowExactly<SqliteObjectBuilderException>()
                .AndMatch( e => e.Dialect == SqliteDialect.Instance && e.Errors.Count == 1 );
        }

        [Fact]
        public void GetOrCreate_ShouldCreateNewColumn_When_ColumnDoesNotExist()
        {
            var schema = new SqliteDatabaseBuilder().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T1" );
            var sut = table.Columns;

            var result = ((ISqlColumnBuilderCollection)sut).GetOrCreate( "C" );

            using ( new AssertionScope() )
            {
                result.Table.Should().BeSameAs( table );
                result.Database.Should().BeSameAs( table.Database );
                result.Name.Should().Be( "C" );
                result.FullName.Should().Be( "foo_T1.C" );
                result.IsNullable.Should().BeFalse();
                result.TypeDefinition.Should().BeSameAs( sut.DefaultTypeDefinition );
                result.DefaultValue.Should().BeNull();
                result.Indexes.Should().BeEmpty();
                sut.Count.Should().Be( 1 );
                sut.Should().BeSequentiallyEqualTo( result );
            }
        }

        [Fact]
        public void GetOrCreate_ShouldReturnExistingColumn_WhenColumnAlreadyExists()
        {
            var schema = new SqliteDatabaseBuilder().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Columns;
            var expected = sut.Create( "C" );

            var result = sut.GetOrCreate( "C" );

            using ( new AssertionScope() )
            {
                result.Should().BeSameAs( expected );
                sut.Count.Should().Be( 1 );
            }
        }

        [Fact]
        public void GetOrCreate_ShouldThrowSqliteObjectBuilderException_WhenTableIsRemoved()
        {
            var schema = new SqliteDatabaseBuilder().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Columns;
            table.Remove();

            var action = Lambda.Of( () => sut.GetOrCreate( "C" ) );

            action.Should()
                .ThrowExactly<SqliteObjectBuilderException>()
                .AndMatch( e => e.Dialect == SqliteDialect.Instance && e.Errors.Count == 1 );
        }

        [Theory]
        [InlineData( "" )]
        [InlineData( " " )]
        [InlineData( "\"" )]
        [InlineData( "f\"oo" )]
        public void GetOrCreate_ShouldThrowSqliteObjectBuilderException_WhenNameIsInvalid(string name)
        {
            var schema = new SqliteDatabaseBuilder().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Columns;

            var action = Lambda.Of( () => sut.GetOrCreate( name ) );

            action.Should()
                .ThrowExactly<SqliteObjectBuilderException>()
                .AndMatch( e => e.Dialect == SqliteDialect.Instance && e.Errors.Count == 1 );
        }

        [Theory]
        [InlineData( "C", true )]
        [InlineData( "D", false )]
        public void Contains_ShouldReturnTrue_WhenColumnExists(string name, bool expected)
        {
            var schema = new SqliteDatabaseBuilder().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Columns;
            sut.Create( "C" );

            var result = sut.Contains( name );

            result.Should().Be( expected );
        }

        [Fact]
        public void Get_ShouldReturnExistingColumn()
        {
            var schema = new SqliteDatabaseBuilder().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Columns;
            var expected = sut.Create( "C" );

            var result = ((ISqlColumnBuilderCollection)sut).Get( "C" );

            result.Should().BeSameAs( expected );
        }

        [Fact]
        public void Get_ShouldThrowKeyNotFoundException_WhenColumnDoesNotExist()
        {
            var schema = new SqliteDatabaseBuilder().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Columns;
            sut.Create( "C" );

            var action = Lambda.Of( () => ((ISqlColumnBuilderCollection)sut).Get( "D" ) );

            action.Should().ThrowExactly<KeyNotFoundException>();
        }

        [Fact]
        public void TryGet_ShouldReturnExistingColumn()
        {
            var schema = new SqliteDatabaseBuilder().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Columns;
            var expected = sut.Create( "C" );

            var result = ((ISqlColumnBuilderCollection)sut).TryGet( "C", out var outResult );

            using ( new AssertionScope() )
            {
                result.Should().BeTrue();
                outResult.Should().BeSameAs( expected );
            }
        }

        [Fact]
        public void TryGet_ShouldReturnFalse_WhenColumnDoesNotExist()
        {
            var schema = new SqliteDatabaseBuilder().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Columns;
            sut.Create( "C" );

            var result = ((ISqlColumnBuilderCollection)sut).TryGet( "D", out var outResult );

            using ( new AssertionScope() )
            {
                result.Should().BeFalse();
                outResult.Should().BeNull();
            }
        }

        [Fact]
        public void Remove_ShouldRemoveExistingColumn()
        {
            var schema = new SqliteDatabaseBuilder().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Columns;
            var column = sut.Create( "C" );

            var result = sut.Remove( "C" );

            using ( new AssertionScope() )
            {
                result.Should().BeTrue();
                column.IsRemoved.Should().BeTrue();
                sut.Count.Should().Be( 0 );
                sut.Contains( "C" ).Should().BeFalse();
            }
        }

        [Fact]
        public void Remove_ShouldReturnFalse_WhenColumnDoesNotExist()
        {
            var schema = new SqliteDatabaseBuilder().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Columns;
            sut.Create( "C" );

            var result = sut.Remove( "D" );

            using ( new AssertionScope() )
            {
                result.Should().BeFalse();
                sut.Count.Should().Be( 1 );
            }
        }

        [Fact]
        public void Remove_ShouldReturnFalse_WhenColumnExistsButIsUsedByAtLeastOneIndex()
        {
            var schema = new SqliteDatabaseBuilder().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Columns;
            table.Indexes.Create( sut.Create( "C" ).Asc() );

            var result = sut.Remove( "C" );

            using ( new AssertionScope() )
            {
                result.Should().BeFalse();
                sut.Count.Should().Be( 1 );
            }
        }

        [Fact]
        public void SetDefaultTypeDefinition_ShouldUpdateDefaultTypeDefinition()
        {
            var db = new SqliteDatabaseBuilder();
            var table = db.Schemas.Create( "foo" ).Objects.CreateTable( "T" );
            var sut = table.Columns;
            var definition = db.TypeDefinitions.GetByType<string>();

            var result = ((ISqlColumnBuilderCollection)sut).SetDefaultTypeDefinition( definition );

            using ( new AssertionScope() )
            {
                result.Should().BeSameAs( sut );
                result.DefaultTypeDefinition.Should().BeSameAs( definition );
            }
        }

        [Fact]
        public void SetDefaultTypeDefinition_ShouldThrowSqliteObjectBuilderException_WhenDefinitionDoesNotBelongToTheDatabase()
        {
            var db = new SqliteDatabaseBuilder();
            var table = db.Schemas.Create( "foo" ).Objects.CreateTable( "T" );
            var sut = table.Columns;
            var definition = new SqliteDatabaseBuilder().TypeDefinitions.GetByType<string>();

            var action = Lambda.Of( () => ((ISqlColumnBuilderCollection)sut).SetDefaultTypeDefinition( definition ) );

            action.Should()
                .ThrowExactly<SqliteObjectBuilderException>()
                .AndMatch( e => e.Dialect == SqliteDialect.Instance && e.Errors.Count == 1 );
        }

        [Fact]
        public void SetDefaultTypeDefinition_ShouldThrowSqliteObjectCastException_WhenDefinitionIsOfInvalidType()
        {
            var db = new SqliteDatabaseBuilder();
            var table = db.Schemas.Create( "foo" ).Objects.CreateTable( "T" );
            var sut = table.Columns;
            var definition = Substitute.For<ISqlColumnTypeDefinition>();

            var action = Lambda.Of( () => ((ISqlColumnBuilderCollection)sut).SetDefaultTypeDefinition( definition ) );

            action.Should()
                .ThrowExactly<SqliteObjectCastException>()
                .AndMatch( e => e.Dialect == SqliteDialect.Instance && e.Expected == typeof( SqliteColumnTypeDefinition ) );
        }
    }
}
