using System.Linq;
using LfrlAnvil.Functional;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.Sqlite.Exceptions;
using LfrlAnvil.Sqlite.Extensions;
using LfrlAnvil.Sqlite.Objects.Builders;
using LfrlAnvil.Sqlite.Tests.Helpers;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.Sqlite.Tests.ObjectsTests.BuildersTests;

public class SqliteCheckBuilderTests : TestsBase
{
    [Fact]
    public void ToString_ShouldReturnCorrectResult()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var column = table.Columns.Create( "C" );
        var sut = table.Checks.Create( column.Node > SqlNode.Literal( 0 ) );

        var result = sut.ToString();

        result.Should().Be( "[Check] foo_CHK_T_0" );
    }

    [Fact]
    public void Create_ShouldMarkTableForReconstruction()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
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
            sut.FullName.Should().Be( "foo_CHK_T_0" );
            sut.ReferencedColumns.Should().BeSequentiallyEqualTo( column );
            column.ReferencingChecks.Should().BeSequentiallyEqualTo( sut );

            statements.Should().HaveCount( 1 );
            statements.ElementAtOrDefault( 0 )
                .Should()
                .SatisfySql(
                    @"CREATE TABLE ""__foo_T__{GUID}__"" (
                      ""C"" ANY NOT NULL,
                      CONSTRAINT ""foo_PK_T"" PRIMARY KEY (""C"" ASC),
                      CONSTRAINT ""foo_CHK_T_0"" CHECK (""C"" > 0)
                    ) WITHOUT ROWID;",
                    @"INSERT INTO ""__foo_T__{GUID}__"" (""C"")
                    SELECT
                      ""foo_T"".""C""
                    FROM ""foo_T"";",
                    "DROP TABLE \"foo_T\";",
                    "ALTER TABLE \"__foo_T__{GUID}__\" RENAME TO \"foo_T\";" );
        }
    }

    [Fact]
    public void Create_FollowedByRemove_ShouldDoNothing()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
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
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
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
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
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
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
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
            sut.FullName.Should().Be( "foo_bar" );
            table.Checks.Get( "bar" ).Should().BeSameAs( sut );
            table.Checks.Contains( oldName ).Should().BeFalse();

            statements.Should().HaveCount( 1 );
            statements.ElementAtOrDefault( 0 )
                .Should()
                .SatisfySql(
                    @"CREATE TABLE ""__foo_T__{GUID}__"" (
                      ""C"" ANY NOT NULL,
                      CONSTRAINT ""foo_PK_T"" PRIMARY KEY (""C"" ASC),
                      CONSTRAINT ""foo_bar"" CHECK (""C"" > 0)
                    ) WITHOUT ROWID;",
                    @"INSERT INTO ""__foo_T__{GUID}__"" (""C"")
                    SELECT
                      ""foo_T"".""C""
                    FROM ""foo_T"";",
                    "DROP TABLE \"foo_T\";",
                    "ALTER TABLE \"__foo_T__{GUID}__\" RENAME TO \"foo_T\";" );
        }
    }

    [Theory]
    [InlineData( "" )]
    [InlineData( " " )]
    [InlineData( "\"" )]
    [InlineData( "'" )]
    [InlineData( "f\"oo" )]
    public void SetName_ShouldThrowSqliteObjectBuilderException_WhenNameIsInvalid(string name)
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.SetPrimaryKey( table.Columns.Create( "C" ).Asc() );
        var sut = table.Checks.Create( table.RecordSet["C"] > SqlNode.Literal( 0 ) );

        var action = Lambda.Of( () => ((ISqlCheckBuilder)sut).SetName( name ) );

        action.Should()
            .ThrowExactly<SqliteObjectBuilderException>()
            .AndMatch( e => e.Dialect == SqliteDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void SetName_ShouldThrowSqliteObjectBuilderException_WhenCheckIsRemoved()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.SetPrimaryKey( table.Columns.Create( "C" ).Asc() );
        var sut = table.Checks.Create( table.RecordSet["C"] > SqlNode.Literal( 0 ) );
        sut.Remove();

        var action = Lambda.Of( () => ((ISqlCheckBuilder)sut).SetName( "bar" ) );

        action.Should()
            .ThrowExactly<SqliteObjectBuilderException>()
            .AndMatch( e => e.Dialect == SqliteDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void SetName_ShouldThrowSqliteObjectBuilderException_WhenNewNameAlreadyExistsInTableChecks()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.SetPrimaryKey( table.Columns.Create( "C" ).Asc() );
        table.Checks.Create( table.RecordSet["C"] != null );
        var sut = table.Checks.Create( table.RecordSet["C"] > SqlNode.Literal( 0 ) );

        var action = Lambda.Of( () => ((ISqlCheckBuilder)sut).SetName( "CHK_T_0" ) );

        action.Should()
            .ThrowExactly<SqliteObjectBuilderException>()
            .AndMatch( e => e.Dialect == SqliteDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void SetName_ShouldThrowSqliteObjectBuilderException_WhenNewNameAlreadyExistsInSchemaObjects()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.SetPrimaryKey( table.Columns.Create( "C" ).Asc() );
        table.Checks.Create( table.RecordSet["C"] != null );
        var sut = table.Checks.Create( table.RecordSet["C"] > SqlNode.Literal( 0 ) );

        var action = Lambda.Of( () => ((ISqlCheckBuilder)sut).SetName( "PK_T" ) );

        action.Should()
            .ThrowExactly<SqliteObjectBuilderException>()
            .AndMatch( e => e.Dialect == SqliteDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void Remove_ShouldRemoveCheckAndClearAssignedColumns()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
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
                .Should()
                .SatisfySql(
                    @"CREATE TABLE ""__foo_T__{GUID}__"" (
                      ""C"" ANY NOT NULL,
                      CONSTRAINT ""foo_PK_T"" PRIMARY KEY (""C"" ASC)
                    ) WITHOUT ROWID;",
                    @"INSERT INTO ""__foo_T__{GUID}__"" (""C"")
                    SELECT
                      ""foo_T"".""C""
                    FROM ""foo_T"";",
                    "DROP TABLE \"foo_T\";",
                    "ALTER TABLE \"__foo_T__{GUID}__\" RENAME TO \"foo_T\";" );
        }
    }

    [Fact]
    public void Remove_ShouldDoNothing_WhenCheckIsRemoved()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
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
    public void ForSqlite_ShouldInvokeAction_WhenCheckIsSqlite()
    {
        var action = Substitute.For<Action<SqliteCheckBuilder>>();
        var table = SqliteDatabaseBuilderMock.Create().Schemas.Default.Objects.CreateTable( "T" );
        var column = table.Columns.Create( "C" );
        var sut = table.Checks.Create( column.Node > SqlNode.Literal( 0 ) );

        var result = sut.ForSqlite( action );

        result.Should().BeSameAs( sut );
        action.Verify().CallAt( 0 ).Exists().And.Arguments.Should().BeSequentiallyEqualTo( sut );
    }

    [Fact]
    public void ForSqlite_ShouldNotInvokeAction_WhenCheckIsNotSqlite()
    {
        var action = Substitute.For<Action<SqliteCheckBuilder>>();
        var sut = Substitute.For<ISqlCheckBuilder>();

        var result = sut.ForSqlite( action );

        result.Should().BeSameAs( sut );
        action.Verify().CallCount.Should().Be( 0 );
    }
}
