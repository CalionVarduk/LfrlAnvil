using System.Linq;
using LfrlAnvil.Functional;
using LfrlAnvil.MySql.Exceptions;
using LfrlAnvil.MySql.Extensions;
using LfrlAnvil.MySql.Objects.Builders;
using LfrlAnvil.MySql.Tests.Helpers;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.MySql.Tests.ObjectsTests.BuildersTests;

public class MySqlCheckBuilderTests : TestsBase
{
    [Fact]
    public void ToString_ShouldReturnCorrectResult()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var column = table.Columns.Create( "C" );
        var sut = table.Checks.Create( column.Node > SqlNode.Literal( 0 ) );

        var result = sut.ToString();

        result.Should().Be( "[Check] foo.CHK_T_0" );
    }

    [Fact]
    public void Create_ShouldMarkTableForAlteration()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var column = table.Columns.Create( "C" );
        table.SetPrimaryKey( column.Asc() );

        var startStatementCount = schema.Database.GetPendingStatements().Length;

        var sut = table.Checks.Create( column.Node > SqlNode.Literal( 0 ) );
        var statements = schema.Database.GetPendingStatements().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            table.Checks.Get( sut.Name ).Should().BeSameAs( sut );
            sut.Name.Should().Be( "CHK_T_0" );
            sut.FullName.Should().Be( "foo.CHK_T_0" );
            sut.ReferencedColumns.Should().BeSequentiallyEqualTo( column );
            column.ReferencingChecks.Should().BeSequentiallyEqualTo( sut );

            statements.Should().HaveCount( 1 );
            statements.ElementAtOrDefault( 0 )
                .Sql.Should()
                .SatisfySql(
                    @"ALTER TABLE `foo`.`T`
                      ADD CONSTRAINT `CHK_T_0` CHECK (`C` > 0);" );
        }
    }

    [Fact]
    public void Create_FollowedByRemove_ShouldDoNothing()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.SetPrimaryKey( table.Columns.Create( "C" ).Asc() );

        var startStatementCount = schema.Database.GetPendingStatements().Length;

        var sut = table.Checks.Create( table.RecordSet["C"] > SqlNode.Literal( 0 ) );
        sut.Remove();
        var statements = schema.Database.GetPendingStatements().Slice( startStatementCount ).ToArray();

        statements.Should().BeEmpty();
    }

    [Fact]
    public void SetName_ShouldDoNothing_WhenNewNameEqualsOldName()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.SetPrimaryKey( table.Columns.Create( "C" ).Asc() );
        var sut = table.Checks.Create( table.RecordSet["C"] > SqlNode.Literal( 0 ) );

        var startStatementCount = schema.Database.GetPendingStatements().Length;

        var result = ((ISqlCheckBuilder)sut).SetName( sut.Name );
        var statements = schema.Database.GetPendingStatements().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            statements.Should().BeEmpty();
        }
    }

    [Fact]
    public void SetName_ShouldDoNothing_WhenNameChangeIsFollowedByChangeToOriginal()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.SetPrimaryKey( table.Columns.Create( "C" ).Asc() );
        var sut = table.Checks.Create( table.RecordSet["C"] > SqlNode.Literal( 0 ) );
        var oldName = sut.Name;

        var startStatementCount = schema.Database.GetPendingStatements().Length;

        sut.SetName( "bar" );
        var result = ((ISqlCheckBuilder)sut).SetName( oldName );

        var statements = schema.Database.GetPendingStatements().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            statements.Should().BeEmpty();
        }
    }

    [Fact]
    public void SetName_ShouldUpdateName_WhenNewNameIsDifferentFromOldName()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.SetPrimaryKey( table.Columns.Create( "C" ).Asc() );
        var sut = table.Checks.Create( table.RecordSet["C"] > SqlNode.Literal( 0 ) );
        var oldName = sut.Name;

        var startStatementCount = schema.Database.GetPendingStatements().Length;

        var result = ((ISqlCheckBuilder)sut).SetName( "bar" );
        var statements = schema.Database.GetPendingStatements().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.Name.Should().Be( "bar" );
            sut.FullName.Should().Be( "foo.bar" );
            table.Checks.Get( "bar" ).Should().BeSameAs( sut );
            table.Checks.Contains( oldName ).Should().BeFalse();

            statements.Should().HaveCount( 1 );
            statements.ElementAtOrDefault( 0 )
                .Sql.Should()
                .SatisfySql(
                    @"ALTER TABLE `foo`.`T`
                      DROP CHECK `CHK_T_0`,
                      ADD CONSTRAINT `bar` CHECK (`C` > 0);" );
        }
    }

    [Theory]
    [InlineData( "" )]
    [InlineData( " " )]
    [InlineData( "`" )]
    [InlineData( "'" )]
    [InlineData( "f`oo" )]
    public void SetName_ShouldThrowMySqlObjectBuilderException_WhenNameIsInvalid(string name)
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.SetPrimaryKey( table.Columns.Create( "C" ).Asc() );
        var sut = table.Checks.Create( table.RecordSet["C"] > SqlNode.Literal( 0 ) );

        var action = Lambda.Of( () => ((ISqlCheckBuilder)sut).SetName( name ) );

        action.Should()
            .ThrowExactly<MySqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void SetName_ShouldThrowMySqlObjectBuilderException_WhenCheckIsRemoved()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.SetPrimaryKey( table.Columns.Create( "C" ).Asc() );
        var sut = table.Checks.Create( table.RecordSet["C"] > SqlNode.Literal( 0 ) );
        sut.Remove();

        var action = Lambda.Of( () => ((ISqlCheckBuilder)sut).SetName( "bar" ) );

        action.Should()
            .ThrowExactly<MySqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void SetName_ShouldThrowMySqlObjectBuilderException_WhenNewNameAlreadyExistsInTableChecks()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.SetPrimaryKey( table.Columns.Create( "C" ).Asc() );
        table.Checks.Create( table.RecordSet["C"] != null );
        var sut = table.Checks.Create( table.RecordSet["C"] > SqlNode.Literal( 0 ) );

        var action = Lambda.Of( () => ((ISqlCheckBuilder)sut).SetName( "CHK_T_0" ) );

        action.Should()
            .ThrowExactly<MySqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void SetName_ShouldThrowMySqlObjectBuilderException_WhenNewNameAlreadyExistsInSchemaObjects()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.SetPrimaryKey( table.Columns.Create( "C" ).Asc() );
        table.Checks.Create( table.RecordSet["C"] != null );
        var sut = table.Checks.Create( table.RecordSet["C"] > SqlNode.Literal( 0 ) );

        var action = Lambda.Of( () => ((ISqlCheckBuilder)sut).SetName( "PK_T" ) );

        action.Should()
            .ThrowExactly<MySqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void Remove_ShouldRemoveCheckAndClearAssignedColumns()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var column = table.Columns.Create( "C" );
        table.SetPrimaryKey( column.Asc() );
        var sut = table.Checks.Create( column.Node > SqlNode.Literal( 0 ) );

        var startStatementCount = schema.Database.GetPendingStatements().Length;

        sut.SetName( "bar" ).Remove();
        var statements = schema.Database.GetPendingStatements().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            table.Checks.Contains( sut.Name ).Should().BeFalse();
            sut.IsRemoved.Should().BeTrue();
            sut.ReferencedColumns.Should().BeEmpty();
            column.ReferencingChecks.Should().BeEmpty();

            statements.Should().HaveCount( 1 );
            statements.ElementAtOrDefault( 0 )
                .Sql.Should()
                .SatisfySql(
                    @"ALTER TABLE `foo`.`T`
                      DROP CHECK `CHK_T_0`;" );
        }
    }

    [Fact]
    public void Remove_ShouldDoNothing_WhenCheckIsRemoved()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.SetPrimaryKey( table.Columns.Create( "C" ).Asc() );
        var sut = table.Checks.Create( table.RecordSet["C"] > SqlNode.Literal( 0 ) );

        _ = schema.Database.GetPendingStatements();
        sut.Remove();
        var startStatementCount = schema.Database.GetPendingStatements().Length;

        sut.Remove();
        var statements = schema.Database.GetPendingStatements().Slice( startStatementCount ).ToArray();

        statements.Should().BeEmpty();
    }

    [Fact]
    public void ForMySql_ShouldInvokeAction_WhenCheckIsMySql()
    {
        var action = Substitute.For<Action<MySqlCheckBuilder>>();
        var table = MySqlDatabaseBuilderMock.Create().Schemas.Default.Objects.CreateTable( "T" );
        var column = table.Columns.Create( "C" );
        var sut = table.Checks.Create( column.Node > SqlNode.Literal( 0 ) );

        var result = sut.ForMySql( action );

        result.Should().BeSameAs( sut );
        action.Verify().CallAt( 0 ).Exists().And.Arguments.Should().BeSequentiallyEqualTo( sut );
    }

    [Fact]
    public void ForMySql_ShouldNotInvokeAction_WhenCheckIsNotMySql()
    {
        var action = Substitute.For<Action<MySqlCheckBuilder>>();
        var sut = Substitute.For<ISqlCheckBuilder>();

        var result = sut.ForMySql( action );

        result.Should().BeSameAs( sut );
        action.Verify().CallCount.Should().Be( 0 );
    }
}
