﻿using System.Collections.Generic;
using LfrlAnvil.Functional;
using LfrlAnvil.MySql.Exceptions;
using LfrlAnvil.MySql.Extensions;
using LfrlAnvil.MySql.Tests.Helpers;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.MySql.Tests.ObjectsTests.BuildersTests;

public partial class MySqlDatabaseBuilderTests
{
    public class Schemas : TestsBase
    {
        [Fact]
        public void Create_ShouldCreateNewSchema()
        {
            var name = Fixture.Create<string>();
            var sut = MySqlDatabaseBuilderMock.Create();

            var result = ((ISqlSchemaBuilderCollection)sut.Schemas).Create( name );

            using ( new AssertionScope() )
            {
                result.Database.Should().BeSameAs( sut );
                result.Name.Should().Be( name );
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
        [InlineData( "`" )]
        [InlineData( "'" )]
        [InlineData( "f`oo" )]
        public void Create_ShouldThrowMySqlObjectBuilderException_WhenNameIsInvalid(string name)
        {
            var sut = MySqlDatabaseBuilderMock.Create();

            var action = Lambda.Of( () => sut.Schemas.Create( name ) );

            action.Should()
                .ThrowExactly<MySqlObjectBuilderException>()
                .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
        }

        [Fact]
        public void Create_ShouldThrowMySqlObjectBuilderException_WhenSchemaWithNameAlreadyExists()
        {
            var name = Fixture.Create<string>();
            var sut = MySqlDatabaseBuilderMock.Create();
            sut.Schemas.Create( name );

            var action = Lambda.Of( () => sut.Schemas.Create( name ) );

            action.Should()
                .ThrowExactly<MySqlObjectBuilderException>()
                .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
        }

        [Fact]
        public void GetOrCreate_ShouldCreateNewSchema()
        {
            var name = Fixture.Create<string>();
            var sut = MySqlDatabaseBuilderMock.Create();

            var result = ((ISqlSchemaBuilderCollection)sut.Schemas).GetOrCreate( name );

            using ( new AssertionScope() )
            {
                result.Database.Should().BeSameAs( sut );
                result.Name.Should().Be( name );
                result.Objects.Should().BeEmpty();
                result.Type.Should().Be( SqlObjectType.Schema );

                sut.Schemas.Get( name ).Should().BeSameAs( result );
                sut.Schemas.Count.Should().Be( 2 );
                sut.Schemas.Should().BeEquivalentTo( sut.Schemas.Default, result );
            }
        }

        [Theory]
        [InlineData( " " )]
        [InlineData( "`" )]
        [InlineData( "f`oo" )]
        public void GetOrCreate_ShouldThrowMySqlObjectBuilderException_WhenNameIsInvalid(string name)
        {
            var sut = MySqlDatabaseBuilderMock.Create();

            var action = Lambda.Of( () => sut.Schemas.Create( name ) );

            action.Should()
                .ThrowExactly<MySqlObjectBuilderException>()
                .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
        }

        [Fact]
        public void GetOrCreate_ShouldThrowMySqlObjectBuilderException_WhenNameIsEmpty()
        {
            var sut = MySqlDatabaseBuilderMock.Create();
            var action = Lambda.Of( () => sut.Schemas.GetOrCreate( string.Empty ) );
            action.Should().ThrowExactly<MySqlObjectBuilderException>();
        }

        [Fact]
        public void GetOrCreate_ShouldReturnExistingSchema_WhenSchemaWithNameAlreadyExists()
        {
            var name = Fixture.Create<string>();
            var sut = MySqlDatabaseBuilderMock.Create();
            var expected = sut.Schemas.Create( name );

            var result = sut.Schemas.GetOrCreate( name );

            using ( new AssertionScope() )
            {
                result.Should().BeSameAs( expected );
                sut.Schemas.Count.Should().Be( 2 );
            }
        }

        [Theory]
        [InlineData( "common", true )]
        [InlineData( "foo", true )]
        [InlineData( "bar", false )]
        public void Contains_ShouldReturnTrue_WhenSchemaExists(string name, bool expected)
        {
            var sut = MySqlDatabaseBuilderMock.Create();
            sut.Schemas.Create( "foo" );

            var result = sut.Schemas.Contains( name );

            result.Should().Be( expected );
        }

        [Fact]
        public void GetSchema_ShouldReturnExistingSchema()
        {
            var name = Fixture.Create<string>();
            var sut = MySqlDatabaseBuilderMock.Create();
            var expected = sut.Schemas.Create( name );

            var result = ((ISqlSchemaBuilderCollection)sut.Schemas).Get( name );

            result.Should().BeSameAs( expected );
        }

        [Fact]
        public void GetSchema_ShouldThrowKeyNotFoundException_WhenSchemaDoesNotExist()
        {
            var name = Fixture.Create<string>();
            var sut = MySqlDatabaseBuilderMock.Create();

            var action = Lambda.Of( () => ((ISqlSchemaBuilderCollection)sut.Schemas).Get( name ) );

            action.Should().ThrowExactly<KeyNotFoundException>();
        }

        [Fact]
        public void TryGetSchema_ShouldReturnExistingSchema()
        {
            var name = Fixture.Create<string>();
            var sut = MySqlDatabaseBuilderMock.Create();
            var expected = sut.Schemas.Create( name );

            var result = ((ISqlSchemaBuilderCollection)sut.Schemas).TryGet( name );

            result.Should().BeSameAs( expected );
        }

        [Fact]
        public void TryGetSchema_ShouldReturnNull_WhenSchemaDoesNotExist()
        {
            var name = Fixture.Create<string>();
            var sut = MySqlDatabaseBuilderMock.Create();

            var result = ((ISqlSchemaBuilderCollection)sut.Schemas).TryGet( name );

            result.Should().BeNull();
        }

        [Fact]
        public void Remove_ShouldRemoveExistingEmptySchema()
        {
            var name = Fixture.Create<string>();
            var sut = MySqlDatabaseBuilderMock.Create();
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
            var sut = MySqlDatabaseBuilderMock.Create();
            var schema = sut.Schemas.Create( name );
            var table = schema.Objects.CreateTable( "T1" );
            var column = table.Columns.Create( "C1" );
            var otherColumn = table.Columns.Create( "C2" ).MarkAsNullable();
            var pk = table.Constraints.SetPrimaryKey( column.Asc() );
            var fk = table.Constraints.CreateForeignKey( table.Constraints.CreateIndex( otherColumn.Asc() ), pk.Index );
            var chk = table.Constraints.CreateCheck( table.Node["C1"] != SqlNode.Literal( 0 ) );
            var view = schema.Objects.CreateView( "V1", table.ToRecordSet().ToDataSource().Select( s => new[] { s.From["C1"].AsSelf() } ) );
            var otherView = schema.Objects.CreateView( "V2", view.ToRecordSet().ToDataSource().Select( s => new[] { s.GetAll() } ) );

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
                chk.IsRemoved.Should().BeTrue();
                view.IsRemoved.Should().BeTrue();
                otherView.IsRemoved.Should().BeTrue();
            }
        }

        [Fact]
        public void Remove_ShouldReturnFalse_WhenSchemaDoesNotExist()
        {
            var name = Fixture.Create<string>();
            var sut = MySqlDatabaseBuilderMock.Create();

            var result = sut.Schemas.Remove( name );

            result.Should().BeFalse();
        }

        [Fact]
        public void Remove_ShouldReturnFalse_WhenTryingToRemoveDefaultSchema()
        {
            var sut = MySqlDatabaseBuilderMock.Create();
            var result = sut.Schemas.Remove( "dbo" );
            result.Should().BeFalse();
        }

        [Fact]
        public void Remove_ShouldReturnFalse_WhenTryingToRemoveSchemaWithTableReferencedByForeignKeyFromOtherSchema()
        {
            var sut = MySqlDatabaseBuilderMock.Create();
            var schema = sut.Schemas.Create( Fixture.Create<string>() );
            var table = schema.Objects.CreateTable( "T1" );
            var column = table.Columns.Create( "C1" );
            table.Constraints.SetPrimaryKey( column.Asc() );

            var otherTable = sut.Schemas.Default.Objects.CreateTable( "T2" );
            var otherColumn = otherTable.Columns.Create( "C2" );
            otherTable.Constraints.SetPrimaryKey( otherColumn.Asc() );
            otherTable.Constraints.CreateForeignKey(
                otherTable.Constraints.GetPrimaryKey().Index,
                table.Constraints.GetPrimaryKey().Index );

            var result = sut.Schemas.Remove( schema.Name );

            result.Should().BeFalse();
        }

        [Fact]
        public void Remove_ShouldReturnFalse_WhenTryingToRemoveSchemaWithTableReferencedByViewFromOtherSchema()
        {
            var sut = MySqlDatabaseBuilderMock.Create();
            var schema = sut.Schemas.Create( Fixture.Create<string>() );
            var table = schema.Objects.CreateTable( "T" );
            var column = table.Columns.Create( "C" );
            table.Constraints.SetPrimaryKey( column.Asc() );

            sut.Schemas.Default.Objects.CreateView(
                "V",
                table.ToRecordSet().ToDataSource().Select( s => new[] { s.GetAll() } ) );

            var result = sut.Schemas.Remove( schema.Name );

            result.Should().BeFalse();
        }

        [Fact]
        public void Remove_ShouldReturnFalse_WhenTryingToRemoveSchemaWithViewReferencedByViewFromOtherSchema()
        {
            var sut = MySqlDatabaseBuilderMock.Create();
            var schema = sut.Schemas.Create( Fixture.Create<string>() );
            var view = schema.Objects.CreateView( "V", SqlNode.RawQuery( "SELECT * FROM foo" ) );

            sut.Schemas.Default.Objects.CreateView(
                "W",
                view.ToRecordSet().ToDataSource().Select( s => new[] { s.GetAll() } ) );

            var result = sut.Schemas.Remove( schema.Name );

            result.Should().BeFalse();
        }
    }
}