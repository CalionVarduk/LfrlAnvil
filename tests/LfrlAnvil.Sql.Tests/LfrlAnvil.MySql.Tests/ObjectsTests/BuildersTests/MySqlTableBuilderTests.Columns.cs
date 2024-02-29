﻿using System.Collections.Generic;
using LfrlAnvil.Functional;
using LfrlAnvil.MySql.Exceptions;
using LfrlAnvil.MySql.Extensions;
using LfrlAnvil.MySql.Tests.Helpers;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.MySql.Tests.ObjectsTests.BuildersTests;

public partial class MySqlTableBuilderTests
{
    public class Columns : TestsBase
    {
        [Fact]
        public void Create_ShouldCreateNewColumn()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T1" );
            var sut = table.Columns;

            var result = ((ISqlColumnBuilderCollection)sut).Create( "C" );

            using ( new AssertionScope() )
            {
                result.Table.Should().BeSameAs( table );
                result.Database.Should().BeSameAs( table.Database );
                result.Name.Should().Be( "C" );
                result.IsNullable.Should().BeFalse();
                result.TypeDefinition.Should().BeSameAs( sut.DefaultTypeDefinition );
                result.DefaultValue.Should().BeNull();
                result.Node.Should().BeEquivalentTo( table.Node["C"] );
                sut.Count.Should().Be( 1 );
                sut.Should().BeSequentiallyEqualTo( result );
            }
        }

        [Fact]
        public void Create_ShouldThrowMySqlObjectBuilderException_WhenColumnAlreadyExists()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Columns;
            sut.Create( "C" );

            var action = Lambda.Of( () => sut.Create( "C" ) );

            action.Should()
                .ThrowExactly<MySqlObjectBuilderException>()
                .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
        }

        [Fact]
        public void Create_ShouldThrowMySqlObjectBuilderException_WhenTableIsRemoved()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Columns;
            table.Remove();

            var action = Lambda.Of( () => sut.Create( "C" ) );

            action.Should()
                .ThrowExactly<MySqlObjectBuilderException>()
                .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
        }

        [Theory]
        [InlineData( "" )]
        [InlineData( " " )]
        [InlineData( "`" )]
        [InlineData( "'" )]
        [InlineData( "f`oo" )]
        public void Create_ShouldThrowMySqlObjectBuilderException_WhenNameIsInvalid(string name)
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Columns;

            var action = Lambda.Of( () => sut.Create( name ) );

            action.Should()
                .ThrowExactly<MySqlObjectBuilderException>()
                .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
        }

        [Fact]
        public void GetOrCreate_ShouldCreateNewColumn_When_ColumnDoesNotExist()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T1" );
            var sut = table.Columns;

            var result = ((ISqlColumnBuilderCollection)sut).GetOrCreate( "C" );

            using ( new AssertionScope() )
            {
                result.Table.Should().BeSameAs( table );
                result.Database.Should().BeSameAs( table.Database );
                result.Name.Should().Be( "C" );
                result.IsNullable.Should().BeFalse();
                result.TypeDefinition.Should().BeSameAs( sut.DefaultTypeDefinition );
                result.DefaultValue.Should().BeNull();
                result.Node.Should().BeEquivalentTo( table.Node["C"] );
                sut.Count.Should().Be( 1 );
                sut.Should().BeSequentiallyEqualTo( result );
            }
        }

        [Fact]
        public void GetOrCreate_ShouldReturnExistingColumn_WhenColumnAlreadyExists()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
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
        public void GetOrCreate_ShouldThrowMySqlObjectBuilderException_WhenTableIsRemoved()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Columns;
            table.Remove();

            var action = Lambda.Of( () => sut.GetOrCreate( "C" ) );

            action.Should()
                .ThrowExactly<MySqlObjectBuilderException>()
                .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
        }

        [Theory]
        [InlineData( "" )]
        [InlineData( " " )]
        [InlineData( "`" )]
        [InlineData( "f`oo" )]
        public void GetOrCreate_ShouldThrowMySqlObjectBuilderException_WhenNameIsInvalid(string name)
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Columns;

            var action = Lambda.Of( () => sut.GetOrCreate( name ) );

            action.Should()
                .ThrowExactly<MySqlObjectBuilderException>()
                .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
        }

        [Theory]
        [InlineData( "C", true )]
        [InlineData( "D", false )]
        public void Contains_ShouldReturnTrue_WhenColumnExists(string name, bool expected)
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Columns;
            sut.Create( "C" );

            var result = sut.Contains( name );

            result.Should().Be( expected );
        }

        [Fact]
        public void GetColumn_ShouldReturnExistingColumn()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Columns;
            var expected = sut.Create( "C" );

            var result = ((ISqlColumnBuilderCollection)sut).Get( "C" );

            result.Should().BeSameAs( expected );
        }

        [Fact]
        public void GetColumn_ShouldThrowKeyNotFoundException_WhenColumnDoesNotExist()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Columns;
            sut.Create( "C" );

            var action = Lambda.Of( () => ((ISqlColumnBuilderCollection)sut).Get( "D" ) );

            action.Should().ThrowExactly<KeyNotFoundException>();
        }

        [Fact]
        public void TryGetColumn_ShouldReturnExistingColumn()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Columns;
            var expected = sut.Create( "C" );

            var result = ((ISqlColumnBuilderCollection)sut).TryGet( "C" );

            result.Should().BeSameAs( expected );
        }

        [Fact]
        public void TryGetColumn_ShouldReturnNull_WhenColumnDoesNotExist()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Columns;
            sut.Create( "C" );

            var result = ((ISqlColumnBuilderCollection)sut).TryGet( "D" );

            result.Should().BeNull();
        }

        [Fact]
        public void Remove_ShouldRemoveExistingColumn()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
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
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
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
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Columns;
            table.Constraints.CreateIndex( "IX_T", sut.Create( "C" ).Asc() );

            var result = sut.Remove( "C" );

            using ( new AssertionScope() )
            {
                result.Should().BeFalse();
                sut.Count.Should().Be( 1 );
            }
        }

        [Fact]
        public void Remove_ShouldReturnFalse_WhenColumnExistsButIsUsedByAtLeastOneIndexFilter()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Columns;
            sut.Create( "C1" );
            table.Constraints.CreateIndex( sut.Create( "C2" ).Asc() ).SetFilter( t => t["C1"] != null );

            var result = sut.Remove( "C1" );

            using ( new AssertionScope() )
            {
                result.Should().BeFalse();
                sut.Count.Should().Be( 2 );
            }
        }

        [Fact]
        public void Remove_ShouldReturnFalse_WhenColumnExistsButIsUsedByAtLeastOneView()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Columns;
            sut.Create( "A" );
            table.Constraints.SetPrimaryKey( sut.Create( "B" ).Asc() );
            schema.Objects.CreateView( "V", table.ToRecordSet().ToDataSource().Select( s => new[] { s.From["A"].AsSelf() } ) );

            var result = sut.Remove( "A" );

            using ( new AssertionScope() )
            {
                result.Should().BeFalse();
                sut.Count.Should().Be( 2 );
            }
        }

        [Fact]
        public void Remove_ShouldReturnFalse_WhenColumnExistsButIsUsedByAtLeastOneCheck()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Columns;
            sut.Create( "A" );
            table.Constraints.SetPrimaryKey( "PK_T", sut.Create( "B" ).Asc() );
            table.Constraints.CreateCheck( table.Node["A"] > SqlNode.Literal( 0 ) );

            var result = sut.Remove( "A" );

            using ( new AssertionScope() )
            {
                result.Should().BeFalse();
                sut.Count.Should().Be( 2 );
            }
        }

        [Fact]
        public void SetDefaultTypeDefinition_ShouldUpdateDefaultTypeDefinition()
        {
            var db = MySqlDatabaseBuilderMock.Create();
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
        public void SetDefaultTypeDefinition_ShouldThrowMySqlObjectBuilderException_WhenDefinitionDoesNotBelongToTheDatabase()
        {
            var db = MySqlDatabaseBuilderMock.Create();
            var table = db.Schemas.Create( "foo" ).Objects.CreateTable( "T" );
            var sut = table.Columns;
            var definition = MySqlDatabaseBuilderMock.Create().TypeDefinitions.GetByType<string>();

            var action = Lambda.Of( () => ((ISqlColumnBuilderCollection)sut).SetDefaultTypeDefinition( definition ) );

            action.Should()
                .ThrowExactly<MySqlObjectBuilderException>()
                .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
        }

        [Fact]
        public void SetDefaultTypeDefinition_ShouldThrowSqlObjectCastException_WhenDefinitionIsOfInvalidType()
        {
            var db = MySqlDatabaseBuilderMock.Create();
            var table = db.Schemas.Create( "foo" ).Objects.CreateTable( "T" );
            var sut = table.Columns;
            var definition = Substitute.For<ISqlColumnTypeDefinition>();

            var action = Lambda.Of( () => ((ISqlColumnBuilderCollection)sut).SetDefaultTypeDefinition( definition ) );

            action.Should()
                .ThrowExactly<SqlObjectCastException>()
                .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Expected == typeof( MySqlColumnTypeDefinition ) );
        }
    }
}