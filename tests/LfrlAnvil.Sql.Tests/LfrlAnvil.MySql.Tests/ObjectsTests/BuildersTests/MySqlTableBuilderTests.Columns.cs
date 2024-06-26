﻿using System.Collections.Generic;
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

            var result = sut.Create( "C" );

            using ( new AssertionScope() )
            {
                result.Table.Should().BeSameAs( table );
                result.Database.Should().BeSameAs( table.Database );
                result.Type.Should().Be( SqlObjectType.Column );
                result.Name.Should().Be( "C" );
                result.IsNullable.Should().BeFalse();
                result.TypeDefinition.Should().BeSameAs( sut.DefaultTypeDefinition );
                result.DefaultValue.Should().BeNull();
                result.Node.Should().BeEquivalentTo( table.Node["C"] );
                result.ReferencingObjects.Should().BeEmpty();
                result.Computation.Should().BeNull();
                result.ReferencedComputationColumns.Should().BeEmpty();

                sut.Count.Should().Be( 1 );
                sut.Should().BeSequentiallyEqualTo( result );
            }
        }

        [Fact]
        public void Create_ShouldThrowSqlObjectBuilderException_WhenColumnNameAlreadyExists()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Columns;
            sut.Create( "C" );

            var action = Lambda.Of( () => sut.Create( "C" ) );

            action.Should()
                .ThrowExactly<SqlObjectBuilderException>()
                .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
        }

        [Fact]
        public void Create_ShouldThrowSqlObjectBuilderException_WhenTableIsRemoved()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Columns;
            table.Remove();

            var action = Lambda.Of( () => sut.Create( "C" ) );

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
        public void Create_ShouldThrowSqlObjectBuilderException_WhenNameIsInvalid(string name)
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Columns;

            var action = Lambda.Of( () => sut.Create( name ) );

            action.Should()
                .ThrowExactly<SqlObjectBuilderException>()
                .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
        }

        [Fact]
        public void GetOrCreate_ShouldCreateNewColumn_WhenColumnDoesNotExist()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T1" );
            var sut = table.Columns;

            var result = sut.GetOrCreate( "C" );

            using ( new AssertionScope() )
            {
                result.Table.Should().BeSameAs( table );
                result.Database.Should().BeSameAs( table.Database );
                result.Type.Should().Be( SqlObjectType.Column );
                result.Name.Should().Be( "C" );
                result.IsNullable.Should().BeFalse();
                result.TypeDefinition.Should().BeSameAs( sut.DefaultTypeDefinition );
                result.DefaultValue.Should().BeNull();
                result.Node.Should().BeEquivalentTo( table.Node["C"] );
                result.ReferencingObjects.Should().BeEmpty();
                result.Computation.Should().BeNull();
                result.ReferencedComputationColumns.Should().BeEmpty();

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
        public void GetOrCreate_ShouldThrowSqlObjectBuilderException_WhenTableIsRemoved()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Columns;
            table.Remove();

            var action = Lambda.Of( () => sut.GetOrCreate( "C" ) );

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
        public void GetOrCreate_ShouldThrowSqlObjectBuilderException_WhenNameIsInvalid(string name)
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Columns;

            var action = Lambda.Of( () => sut.GetOrCreate( name ) );

            action.Should()
                .ThrowExactly<SqlObjectBuilderException>()
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
        public void Get_ShouldReturnExistingColumn()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Columns;
            var expected = sut.Create( "C" );

            var result = sut.Get( "C" );

            result.Should().BeSameAs( expected );
        }

        [Fact]
        public void Get_ShouldThrowKeyNotFoundException_WhenColumnDoesNotExist()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Columns;
            sut.Create( "C" );

            var action = Lambda.Of( () => sut.Get( "D" ) );

            action.Should().ThrowExactly<KeyNotFoundException>();
        }

        [Fact]
        public void TryGet_ShouldReturnExistingColumn()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Columns;
            var expected = sut.Create( "C" );

            var result = sut.TryGet( "C" );

            result.Should().BeSameAs( expected );
        }

        [Fact]
        public void TryGetColumn_ShouldReturnNull_WhenColumnDoesNotExist()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Columns;
            sut.Create( "C" );

            var result = sut.TryGet( "D" );

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
                sut.TryGet( column.Name ).Should().BeNull();
                column.IsRemoved.Should().BeTrue();
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

            result.Should().BeFalse();
        }

        [Fact]
        public void Remove_ShouldReturnFalse_WhenColumnExistsButIsReferencedByIndex()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Columns;
            table.Constraints.SetPrimaryKey( sut.Create( "C1" ).Asc() );
            var column = sut.Create( "C2" );
            table.Constraints.CreateIndex( column.Asc() );

            var result = sut.Remove( "C" );

            using ( new AssertionScope() )
            {
                result.Should().BeFalse();
                column.IsRemoved.Should().BeFalse();
                sut.Count.Should().Be( 2 );
            }
        }

        [Fact]
        public void Remove_ShouldReturnFalse_WhenColumnExistsButIsReferencedByIndexFilter()
        {
            var schema = MySqlDatabaseBuilderMock.Create( indexFilterResolution: SqlOptionalFunctionalityResolution.Include )
                .Schemas.Create( "foo" );

            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Columns;
            table.Constraints.SetPrimaryKey( sut.Create( "C1" ).Asc() );
            var column = sut.Create( "C2" );
            table.Constraints.CreateIndex( sut.Create( "C3" ).Asc() ).SetFilter( t => t["C2"] != null );

            var result = sut.Remove( "C2" );

            using ( new AssertionScope() )
            {
                result.Should().BeFalse();
                column.IsRemoved.Should().BeFalse();
                sut.Count.Should().Be( 3 );
            }
        }

        [Fact]
        public void Remove_ShouldReturnFalse_WhenColumnExistsButIsReferencedByView()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Columns;
            table.Constraints.SetPrimaryKey( sut.Create( "C1" ).Asc() );
            var column = sut.Create( "C2" );
            schema.Objects.CreateView( "V", table.Node.ToDataSource().Select( s => new[] { s.From["C2"].AsSelf() } ) );

            var result = sut.Remove( "C2" );

            using ( new AssertionScope() )
            {
                result.Should().BeFalse();
                column.IsRemoved.Should().BeFalse();
                sut.Count.Should().Be( 2 );
            }
        }

        [Fact]
        public void Remove_ShouldReturnFalse_WhenColumnExistsButIsReferencedByCheck()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Columns;
            table.Constraints.SetPrimaryKey( sut.Create( "C1" ).Asc() );
            var column = sut.Create( "C2" );
            table.Constraints.CreateCheck( table.Node["C2"] != SqlNode.Literal( 0 ) );

            var result = sut.Remove( "C2" );

            using ( new AssertionScope() )
            {
                result.Should().BeFalse();
                column.IsRemoved.Should().BeFalse();
                sut.Count.Should().Be( 2 );
            }
        }

        [Fact]
        public void SetDefaultTypeDefinition_ShouldUpdateDefaultTypeDefinition()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Columns;
            var definition = schema.Database.TypeDefinitions.GetByType<string>();

            var result = sut.SetDefaultTypeDefinition( definition );

            using ( new AssertionScope() )
            {
                result.Should().BeSameAs( sut );
                result.DefaultTypeDefinition.Should().BeSameAs( definition );
            }
        }

        [Fact]
        public void SetDefaultTypeDefinition_ShouldThrowSqlObjectBuilderException_WhenDefinitionDoesNotBelongToTheDatabase()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Columns;
            var definition = MySqlDatabaseBuilderMock.Create().TypeDefinitions.GetByType<string>();

            var action = Lambda.Of( () => (( ISqlColumnBuilderCollection )sut).SetDefaultTypeDefinition( definition ) );

            action.Should()
                .ThrowExactly<SqlObjectBuilderException>()
                .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
        }

        [Fact]
        public void SetDefaultTypeDefinition_ShouldThrowSqlObjectCastException_WhenDefinitionIsOfInvalidType()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Columns;
            var definition = Substitute.For<ISqlColumnTypeDefinition>();

            var action = Lambda.Of( () => (( ISqlColumnBuilderCollection )sut).SetDefaultTypeDefinition( definition ) );

            action.Should()
                .ThrowExactly<SqlObjectCastException>()
                .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Expected == typeof( SqlColumnTypeDefinition ) );
        }

        [Fact]
        public void GetEnumerator_ShouldReturnCorrectResult()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Columns;
            var c1 = sut.Create( "C1" );
            var c2 = sut.Create( "C2" );

            var result = new List<MySqlColumnBuilder>();
            foreach ( var c in sut )
                result.Add( c );

            using ( new AssertionScope() )
            {
                result.Should().HaveCount( 2 );
                result.Should().BeEquivalentTo( c1, c2 );
            }
        }
    }
}
