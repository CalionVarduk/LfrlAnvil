using System.Collections.Generic;
using LfrlAnvil.Functional;
using LfrlAnvil.MySql.Exceptions;
using LfrlAnvil.MySql.Extensions;
using LfrlAnvil.MySql.Tests.Helpers;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.MySql.Tests.ObjectsTests.BuildersTests;

public partial class MySqlTableBuilderTests
{
    public class Checks : TestsBase
    {
        [Fact]
        public void Create_ShouldCreateNewCheck()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Checks;
            var c = table.Columns.Create( "C" );
            var condition = c.Node > SqlNode.Literal( 0 );

            var result = ((ISqlCheckBuilderCollection)sut).Create( condition );

            using ( new AssertionScope() )
            {
                result.Name.Should().Be( "CHK_T_0" );
                result.FullName.Should().Be( "foo.CHK_T_0" );
                result.Table.Should().BeSameAs( table );
                result.Database.Should().BeSameAs( table.Database );
                result.Type.Should().Be( SqlObjectType.Check );
                result.ReferencedColumns.Should().BeSequentiallyEqualTo( c );
                result.Condition.Should().BeSameAs( condition );
                sut.Count.Should().Be( 1 );
                sut.Should().BeEquivalentTo( result );
                schema.Objects.Contains( result.Name ).Should().BeTrue();

                ((ISqlColumnBuilder)c).ReferencingChecks.Should().BeSequentiallyEqualTo( result );
            }
        }

        [Fact]
        public void Create_ShouldThrowMySqlObjectBuilderException_WhenTableIsRemoved()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Checks;
            table.Columns.Create( "C" );
            table.Remove();

            var action = Lambda.Of( () => sut.Create( SqlNode.True() ) );

            action.Should()
                .ThrowExactly<MySqlObjectBuilderException>()
                .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
        }

        [Fact]
        public void Create_ShouldThrowMySqlObjectBuilderException_WhenCheckAlreadyExists()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Checks;
            var c = table.Columns.Create( "C" );
            sut.Create( c.Node != null ).SetName( "CHK_T_1" );

            var action = Lambda.Of( () => sut.Create( c.Node > SqlNode.Literal( 0 ) ) );

            action.Should()
                .ThrowExactly<MySqlObjectBuilderException>()
                .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
        }

        [Fact]
        public void Create_ShouldThrowMySqlObjectBuilderException_WhenCheckAlreadyExistsDueToOtherTable()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var otherTable = schema.Objects.CreateTable( "T2" );
            otherTable.SetPrimaryKey( otherTable.Columns.Create( "D" ).Asc() );
            otherTable.Checks.Create( otherTable.RecordSet["D"] != null ).SetName( "CHK_T_0" );
            var table = schema.Objects.CreateTable( "T" );
            table.SetPrimaryKey( table.Columns.Create( "C" ).Asc() );

            var action = Lambda.Of( () => table.Checks.Create( table.RecordSet["C"] != SqlNode.Literal( 0 ) ) );

            action.Should()
                .ThrowExactly<MySqlObjectBuilderException>()
                .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
        }

        [Fact]
        public void Create_ShouldThrowMySqlObjectBuilderException_WhenConditionIsInvalid()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );

            var action = Lambda.Of(
                () => ((ISqlCheckBuilderCollection)table.Checks).Create( SqlNode.Functions.RecordsAffected() == SqlNode.Literal( 0 ) ) );

            action.Should()
                .ThrowExactly<MySqlObjectBuilderException>()
                .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
        }

        [Fact]
        public void Contains_ShouldReturnTrue_WhenCheckExists()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Checks;
            var c = table.Columns.Create( "C" );
            sut.Create( c.Node > SqlNode.Literal( 0 ) );

            var result = sut.Contains( "CHK_T_0" );

            result.Should().BeTrue();
        }

        [Fact]
        public void Contains_ShouldReturnFalse_WhenCheckDoesNotExist()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Checks;
            var c = table.Columns.Create( "C" );
            sut.Create( c.Node > SqlNode.Literal( 0 ) );

            var result = sut.Contains( "CHK_T_1" );

            result.Should().BeFalse();
        }

        [Fact]
        public void Get_ShouldReturnExistingCheck()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Checks;
            var c = table.Columns.Create( "C" );
            var expected = sut.Create( c.Node > SqlNode.Literal( 0 ) );

            var result = ((ISqlCheckBuilderCollection)sut).Get( "CHK_T_0" );

            result.Should().BeSameAs( expected );
        }

        [Fact]
        public void Get_ShouldThrowKeyNotFoundException_WhenCheckDoesNotExist()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Checks;
            var c = table.Columns.Create( "C" );
            sut.Create( c.Node > SqlNode.Literal( 0 ) );

            var action = Lambda.Of( () => sut.Get( "CHK_T_1" ) );

            action.Should().ThrowExactly<KeyNotFoundException>();
        }

        [Fact]
        public void TryGet_ShouldReturnExistingCheck()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Checks;
            var c = table.Columns.Create( "C" );
            var expected = sut.Create( c.Node > SqlNode.Literal( 0 ) );

            var result = ((ISqlCheckBuilderCollection)sut).TryGet( "CHK_T_0", out var outResult );

            using ( new AssertionScope() )
            {
                result.Should().BeTrue();
                outResult.Should().BeSameAs( expected );
            }
        }

        [Fact]
        public void TryGet_ShouldReturnFalse_WhenCheckDoesNotExist()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Checks;
            var c = table.Columns.Create( "C" );
            sut.Create( c.Node > SqlNode.Literal( 0 ) );

            var result = ((ISqlCheckBuilderCollection)sut).TryGet( "CHK_T_1", out var outResult );

            using ( new AssertionScope() )
            {
                result.Should().BeFalse();
                outResult.Should().BeNull();
            }
        }

        [Fact]
        public void Remove_ShouldRemoveExistingCheck()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Checks;
            var c = table.Columns.Create( "C" );
            var check = sut.Create( c.Node > SqlNode.Literal( 0 ) );

            var result = sut.Remove( check.Name );

            using ( new AssertionScope() )
            {
                result.Should().BeTrue();
                check.IsRemoved.Should().BeTrue();
                c.ReferencingChecks.Should().BeEmpty();
                sut.Count.Should().Be( 0 );
                schema.Objects.Contains( check.Name ).Should().BeFalse();
            }
        }

        [Fact]
        public void Remove_ShouldReturnFalse_WhenCheckDoesNotExist()
        {
            var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
            var table = schema.Objects.CreateTable( "T" );
            var sut = table.Checks;
            var c = table.Columns.Create( "C" );
            sut.Create( c.Node > SqlNode.Literal( 0 ) );

            var result = sut.Remove( "CHK_T_1" );

            using ( new AssertionScope() )
            {
                result.Should().BeFalse();
                sut.Count.Should().Be( 1 );
            }
        }
    }
}
