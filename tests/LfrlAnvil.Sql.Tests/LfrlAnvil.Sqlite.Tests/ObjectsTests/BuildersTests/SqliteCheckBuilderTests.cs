using System.Linq;
using System.Text.RegularExpressions;
using LfrlAnvil.Functional;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.Sqlite.Extensions;
using LfrlAnvil.Sqlite.Objects.Builders;
using LfrlAnvil.Sqlite.Tests.Helpers;

namespace LfrlAnvil.Sqlite.Tests.ObjectsTests.BuildersTests;

public class SqliteCheckBuilderTests : TestsBase
{
    [Fact]
    public void ToString_ShouldReturnCorrectResult()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var column = table.Columns.Create( "C" );
        var sut = table.Constraints.CreateCheck( column.Node > SqlNode.Literal( 0 ) );

        var result = sut.ToString();

        result.TestMatch( new Regex( "\\[Check\\] foo_CHK_T_[0-9a-fA-F]{32}" ) ).Go();
    }

    [Fact]
    public void Creation_ShouldMarkTableForReconstruction()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var column = table.Columns.Create( "C" );
        var pk = table.Constraints.SetPrimaryKey( column.Asc() );

        var actionCount = schema.Database.GetPendingActionCount();
        var sut = table.Constraints.CreateCheck( column.Node > SqlNode.Literal( 0 ) );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        Assertion.All(
                table.Constraints.GetCheck( sut.Name ).TestRefEquals( sut ),
                schema.Objects.TryGet( sut.Name ).TestRefEquals( sut ),
                sut.Name.TestMatch( new Regex( "CHK_T_[0-9a-fA-F]{32}" ) ),
                sut.ReferencedColumns.TestSequence( [ column ] ),
                column.ReferencingObjects.Count.TestEquals( 2 ),
                column.ReferencingObjects.TestSetEqual(
                [
                    SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( pk.Index ), column ),
                    SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( sut ), column )
                ] ),
                actions.Select( a => a.Sql )
                    .TestSequence(
                    [
                        (sql, _) => sql.TestSatisfySql(
                            """
                            CREATE TABLE "__foo_T__{GUID}__" (
                              "C" ANY NOT NULL,
                              CONSTRAINT "foo_PK_T" PRIMARY KEY ("C" ASC),
                              CONSTRAINT "foo_CHK_T_{GUID}" CHECK ("C" > 0)
                            ) WITHOUT ROWID;
                            """,
                            """
                            INSERT INTO "__foo_T__{GUID}__" ("C")
                            SELECT
                              "foo_T"."C"
                            FROM "foo_T";
                            """,
                            "DROP TABLE \"foo_T\";",
                            "ALTER TABLE \"__foo_T__{GUID}__\" RENAME TO \"foo_T\";" )
                    ] ) )
            .Go();
    }

    [Fact]
    public void Creation_FollowedByRemove_ShouldDoNothing()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C" ).Asc() );

        var actionCount = schema.Database.GetPendingActionCount();
        var sut = table.Constraints.CreateCheck( SqlNode.True() );
        sut.Remove();
        var actions = schema.Database.GetLastPendingActions( actionCount );

        actions.TestEmpty().Go();
    }

    [Fact]
    public void SetName_ShouldDoNothing_WhenNewNameEqualsOldName()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C" ).Asc() );
        var sut = table.Constraints.CreateCheck( SqlNode.True() );

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.SetName( sut.Name );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        Assertion.All(
                result.TestRefEquals( sut ),
                actions.TestEmpty() )
            .Go();
    }

    [Fact]
    public void SetName_ShouldDoNothing_WhenNameChangeIsFollowedByChangeToOriginal()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C" ).Asc() );
        var sut = table.Constraints.CreateCheck( SqlNode.True() );
        var oldName = sut.Name;

        var actionCount = schema.Database.GetPendingActionCount();
        sut.SetName( "bar" );
        var result = sut.SetName( oldName );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        Assertion.All(
                result.TestRefEquals( sut ),
                actions.TestEmpty() )
            .Go();
    }

    [Fact]
    public void SetName_ShouldUpdateName_WhenNewNameIsDifferentFromOldName()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C" ).Asc() );
        var sut = table.Constraints.CreateCheck( table.Node["C"] > SqlNode.Literal( 0 ) );
        var oldName = sut.Name;

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.SetName( "bar" );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        Assertion.All(
                result.TestRefEquals( sut ),
                sut.Name.TestEquals( "bar" ),
                table.Constraints.TryGet( "bar" ).TestRefEquals( sut ),
                table.Constraints.TryGet( oldName ).TestNull(),
                schema.Objects.TryGet( "bar" ).TestRefEquals( sut ),
                schema.Objects.TryGet( oldName ).TestNull(),
                actions.Select( a => a.Sql )
                    .TestSequence(
                    [
                        (sql, _) => sql.TestSatisfySql(
                            """
                            CREATE TABLE "__foo_T__{GUID}__" (
                              "C" ANY NOT NULL,
                              CONSTRAINT "foo_PK_T" PRIMARY KEY ("C" ASC),
                              CONSTRAINT "foo_bar" CHECK ("C" > 0)
                            ) WITHOUT ROWID;
                            """,
                            """
                            INSERT INTO "__foo_T__{GUID}__" ("C")
                            SELECT
                              "foo_T"."C"
                            FROM "foo_T";
                            """,
                            "DROP TABLE \"foo_T\";",
                            "ALTER TABLE \"__foo_T__{GUID}__\" RENAME TO \"foo_T\";" )
                    ] ) )
            .Go();
    }

    [Theory]
    [InlineData( "" )]
    [InlineData( " " )]
    [InlineData( "\"" )]
    [InlineData( "'" )]
    [InlineData( "f\"oo" )]
    public void SetName_ShouldThrowSqlObjectBuilderException_WhenNameIsInvalid(string name)
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C" ).Asc() );
        var sut = table.Constraints.CreateCheck( SqlNode.True() );

        var action = Lambda.Of( () => sut.SetName( name ) );

        action.Test(
                exc => exc.TestType()
                    .Exact<SqlObjectBuilderException>(
                        e => Assertion.All( e.Dialect.TestEquals( SqliteDialect.Instance ), e.Errors.Count.TestEquals( 1 ) ) ) )
            .Go();
    }

    [Fact]
    public void SetName_ShouldThrowSqlObjectBuilderException_WhenCheckIsRemoved()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C" ).Asc() );
        var sut = table.Constraints.CreateCheck( SqlNode.True() );
        sut.Remove();

        var action = Lambda.Of( () => sut.SetName( "bar" ) );

        action.Test(
                exc => exc.TestType()
                    .Exact<SqlObjectBuilderException>(
                        e => Assertion.All( e.Dialect.TestEquals( SqliteDialect.Instance ), e.Errors.Count.TestEquals( 1 ) ) ) )
            .Go();
    }

    [Fact]
    public void SetName_ShouldThrowSqlObjectBuilderException_WhenNewNameAlreadyExistsInSchemaObjects()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C" ).Asc() );
        var other = table.Constraints.CreateCheck( SqlNode.True() );
        var sut = table.Constraints.CreateCheck( SqlNode.True() );

        var action = Lambda.Of( () => sut.SetName( other.Name ) );

        action.Test(
                exc => exc.TestType()
                    .Exact<SqlObjectBuilderException>(
                        e => Assertion.All( e.Dialect.TestEquals( SqliteDialect.Instance ), e.Errors.Count.TestEquals( 1 ) ) ) )
            .Go();
    }

    [Fact]
    public void SetDefaultName_ShouldUpdateName_WhenNewNameIsDifferentFromOldName()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Constraints.CreateCheck( SqlNode.True() ).SetName( "bar" );
        var oldName = sut.Name;

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.SetDefaultName();
        var actions = schema.Database.GetLastPendingActions( actionCount );

        Assertion.All(
                result.TestRefEquals( sut ),
                sut.Name.TestMatch( new Regex( "CHK_T_[0-9a-fA-F]{32}" ) ),
                table.Constraints.TryGet( result.Name ).TestRefEquals( sut ),
                table.Constraints.TryGet( oldName ).TestNull(),
                schema.Objects.TryGet( result.Name ).TestRefEquals( sut ),
                schema.Objects.TryGet( oldName ).TestNull(),
                actions.Select( a => a.Sql )
                    .TestSequence(
                    [
                        (sql, _) => sql.TestSatisfySql(
                            """
                            CREATE TABLE "__foo_T__{GUID}__" (
                              "C1" ANY NOT NULL,
                              CONSTRAINT "foo_PK_T" PRIMARY KEY ("C1" ASC),
                              CONSTRAINT "foo_CHK_T_{GUID}" CHECK (TRUE)
                            ) WITHOUT ROWID;
                            """,
                            """
                            INSERT INTO "__foo_T__{GUID}__" ("C1")
                            SELECT
                              "foo_T"."C1"
                            FROM "foo_T";
                            """,
                            "DROP TABLE \"foo_T\";",
                            "ALTER TABLE \"__foo_T__{GUID}__\" RENAME TO \"foo_T\";" )
                    ] ) )
            .Go();
    }

    [Fact]
    public void SetDefaultName_ShouldThrowSqlObjectBuilderException_WhenCheckIsRemoved()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Constraints.CreateCheck( SqlNode.True() ).SetName( "bar" );
        sut.Remove();

        var action = Lambda.Of( () => sut.SetDefaultName() );

        action.Test(
                exc => exc.TestType()
                    .Exact<SqlObjectBuilderException>(
                        e => Assertion.All( e.Dialect.TestEquals( SqliteDialect.Instance ), e.Errors.Count.TestEquals( 1 ) ) ) )
            .Go();
    }

    [Fact]
    public void Remove_ShouldRemoveCheckAndClearReferencedColumns()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var column = table.Columns.Create( "C" );
        var pk = table.Constraints.SetPrimaryKey( column.Asc() );
        var sut = table.Constraints.CreateCheck( column.Node > SqlNode.Literal( 0 ) );

        var actionCount = schema.Database.GetPendingActionCount();
        sut.Remove();
        var actions = schema.Database.GetLastPendingActions( actionCount );

        Assertion.All(
                table.Constraints.TryGet( sut.Name ).TestNull(),
                schema.Objects.TryGet( sut.Name ).TestNull(),
                sut.IsRemoved.TestTrue(),
                sut.ReferencedColumns.TestEmpty(),
                column.ReferencingObjects.TestSequence(
                    [ SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( pk.Index ), column ) ] ),
                actions.Select( a => a.Sql )
                    .TestSequence(
                    [
                        (sql, _) => sql.TestSatisfySql(
                            """
                            CREATE TABLE "__foo_T__{GUID}__" (
                              "C" ANY NOT NULL,
                              CONSTRAINT "foo_PK_T" PRIMARY KEY ("C" ASC)
                            ) WITHOUT ROWID;
                            """,
                            """
                            INSERT INTO "__foo_T__{GUID}__" ("C")
                            SELECT
                              "foo_T"."C"
                            FROM "foo_T";
                            """,
                            "DROP TABLE \"foo_T\";",
                            "ALTER TABLE \"__foo_T__{GUID}__\" RENAME TO \"foo_T\";" )
                    ] ) )
            .Go();
    }

    [Fact]
    public void Remove_ShouldDoNothing_WhenCheckIsRemoved()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C" ).Asc() );
        var sut = table.Constraints.CreateCheck( SqlNode.True() );

        schema.Database.Changes.CompletePendingChanges();
        sut.Remove();

        var actionCount = schema.Database.GetPendingActionCount();
        sut.Remove();
        var actions = schema.Database.GetLastPendingActions( actionCount );

        actions.TestEmpty().Go();
    }

    [Fact]
    public void ForSqlite_ShouldInvokeAction_WhenCheckIsSqlite()
    {
        var action = Substitute.For<Action<SqliteCheckBuilder>>();
        var table = SqliteDatabaseBuilderMock.Create().Schemas.Default.Objects.CreateTable( "T" );
        var column = table.Columns.Create( "C" );
        var sut = table.Constraints.CreateCheck( column.Node > SqlNode.Literal( 0 ) );

        var result = sut.ForSqlite( action );

        Assertion.All(
                result.TestRefEquals( sut ),
                action.CallAt( 0 ).Arguments.TestSequence( [ sut ] ) )
            .Go();
    }

    [Fact]
    public void ForSqlite_ShouldNotInvokeAction_WhenCheckIsNotSqlite()
    {
        var action = Substitute.For<Action<SqliteCheckBuilder>>();
        var sut = Substitute.For<ISqlCheckBuilder>();

        var result = sut.ForSqlite( action );

        Assertion.All(
                result.TestRefEquals( sut ),
                action.CallCount().TestEquals( 0 ) )
            .Go();
    }
}
