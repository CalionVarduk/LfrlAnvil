using System.Linq;
using LfrlAnvil.Functional;
using LfrlAnvil.MySql.Extensions;
using LfrlAnvil.MySql.Objects.Builders;
using LfrlAnvil.MySql.Tests.Helpers;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.MySql.Tests.ObjectsTests.BuildersTests;

public class MySqlViewBuilderTests : TestsBase
{
    [Fact]
    public void ToString_ShouldReturnCorrectResult()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var sut = schema.Objects.CreateView( "bar", SqlNode.RawQuery( "SELECT * FROM foo" ) );

        var result = sut.ToString();

        result.TestEquals( "[View] foo.bar" ).Go();
    }

    [Fact]
    public void Creation_ShouldPrepareCorrectStatement()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );

        var actionCount = schema.Database.GetPendingActionCount();
        var sut = schema.Objects.CreateView( "V", SqlNode.RawQuery( "SELECT * FROM bar" ) );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        Assertion.All(
                schema.Objects.TryGet( sut.Name ).TestRefEquals( sut ),
                sut.Name.TestEquals( "V" ),
                sut.ReferencedObjects.TestEmpty(),
                actions.Select( a => a.Sql )
                    .TestSequence(
                    [
                        (sql, _) => sql.TestSatisfySql(
                            """
                            CREATE VIEW `foo`.`V` AS
                                SELECT * FROM bar;
                            """ )
                    ] ) )
            .Go();
    }

    [Fact]
    public void Creation_FollowedByRemoval_ShouldDoNothing()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );

        var actionCount = schema.Database.GetPendingActionCount();
        var sut = schema.Objects.CreateView( "V", SqlNode.RawQuery( "SELECT * FROM bar" ) );
        sut.Remove();
        var actions = schema.Database.GetLastPendingActions( actionCount );

        actions.TestEmpty().Go();
    }

    [Fact]
    public void SetName_ShouldDoNothing_WhenNewNameEqualsOldName()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var sut = schema.Objects.CreateView( "V", SqlNode.RawQuery( "SELECT * FROM bar" ) );

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
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var sut = schema.Objects.CreateView( "V", SqlNode.RawQuery( "SELECT * FROM bar" ) );
        var oldName = sut.Name;

        var actionCount = schema.Database.GetPendingActionCount();
        sut.SetName( "W" );
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
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var sut = schema.Objects.CreateView( "V", SqlNode.RawQuery( "SELECT * FROM bar" ) );
        var oldName = sut.Name;
        var recordSet = sut.Node;
        _ = sut.Info;

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.SetName( "bar" );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        Assertion.All(
                result.TestRefEquals( sut ),
                sut.Name.TestEquals( "bar" ),
                sut.Info.TestEquals( SqlRecordSetInfo.Create( "foo", "bar" ) ),
                recordSet.Info.TestEquals( sut.Info ),
                schema.Objects.TryGet( "bar" ).TestRefEquals( sut ),
                schema.Objects.TryGet( oldName ).TestNull(),
                actions.Select( a => a.Sql )
                    .TestSequence(
                    [
                        (sql, _) => sql.TestSatisfySql(
                            "DROP VIEW `foo`.`V`;",
                            """
                            CREATE VIEW `foo`.`bar` AS
                                SELECT * FROM bar;
                            """ )
                    ] ) )
            .Go();
    }

    [Fact]
    public void SetName_ShouldUpdateName_WhenNewNameIsDifferentFromOldName_WithReferencingViews()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var sut = schema.Objects.CreateView( "V", SqlNode.RawQuery( "SELECT * FROM bar" ) );
        var w1 = schema.Objects.CreateView( "W1", sut.Node.ToDataSource().Select( s => new[] { s.GetAll() } ) );
        schema.Objects.CreateView( "W2", w1.Node.ToDataSource().Select( s => new[] { s.GetAll() } ) );
        schema.Objects.CreateView( "W3", sut.Node.Join( w1.ToRecordSet().InnerOn( SqlNode.True() ) ).Select( s => new[] { s.GetAll() } ) );
        var oldName = sut.Name;
        var recordSet = sut.Node;

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.SetName( "bar" );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        Assertion.All(
                result.TestRefEquals( sut ),
                sut.Name.TestEquals( "bar" ),
                sut.Info.TestEquals( SqlRecordSetInfo.Create( "foo", "bar" ) ),
                recordSet.Info.TestEquals( sut.Info ),
                schema.Objects.TryGet( "bar" ).TestRefEquals( sut ),
                schema.Objects.TryGet( oldName ).TestNull(),
                actions.Select( a => a.Sql )
                    .TestSequence(
                    [
                        (sql, _) => sql.TestSatisfySql(
                            "DROP VIEW `foo`.`V`;",
                            """
                            CREATE VIEW `foo`.`bar` AS
                                SELECT * FROM bar;
                            """ ),
                        (sql, _) => sql.TestSatisfySql(
                            "DROP VIEW `foo`.`W1`;",
                            """
                            CREATE VIEW `foo`.`W1` AS
                                SELECT
                                  *
                                FROM `foo`.`bar`;
                            """ ),
                        (sql, _) => sql.TestSatisfySql(
                            "DROP VIEW `foo`.`W3`;",
                            """
                            CREATE VIEW `foo`.`W3` AS
                                SELECT
                                  *
                                FROM `foo`.`bar`
                                INNER JOIN `foo`.`W1` ON TRUE;
                            """ )
                    ] ) )
            .Go();
    }

    [Theory]
    [InlineData( "" )]
    [InlineData( " " )]
    [InlineData( "`" )]
    [InlineData( "'" )]
    [InlineData( "f`oo" )]
    public void SetName_ShouldThrowSqlObjectBuilderException_WhenNameIsInvalid(string name)
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var sut = schema.Objects.CreateView( "V", SqlNode.RawQuery( "SELECT * FROM bar" ) );

        var action = Lambda.Of( () => sut.SetName( name ) );

        action.Test(
                exc => exc.TestType()
                    .Exact<SqlObjectBuilderException>(
                        e => Assertion.All( e.Dialect.TestEquals( MySqlDialect.Instance ), e.Errors.Count.TestEquals( 1 ) ) ) )
            .Go();
    }

    [Fact]
    public void SetName_ShouldThrowSqlObjectBuilderException_WhenViewIsRemoved()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var sut = schema.Objects.CreateView( "V", SqlNode.RawQuery( "SELECT * FROM bar" ) );
        sut.Remove();

        var action = Lambda.Of( () => sut.SetName( "bar" ) );

        action.Test(
                exc => exc.TestType()
                    .Exact<SqlObjectBuilderException>(
                        e => Assertion.All( e.Dialect.TestEquals( MySqlDialect.Instance ), e.Errors.Count.TestEquals( 1 ) ) ) )
            .Go();
    }

    [Fact]
    public void SetName_ShouldThrowSqlObjectBuilderException_WhenNewNameAlreadyExistsInSchemaObjects()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var other = schema.Objects.CreateTable( "T" );
        other.Constraints.SetPrimaryKey( other.Columns.Create( "C" ).Asc() );
        var sut = schema.Objects.CreateView( "V", SqlNode.RawQuery( "SELECT * FROM bar" ) );

        var action = Lambda.Of( () => sut.SetName( "T" ) );

        action.Test(
                exc => exc.TestType()
                    .Exact<SqlObjectBuilderException>(
                        e => Assertion.All( e.Dialect.TestEquals( MySqlDialect.Instance ), e.Errors.Count.TestEquals( 1 ) ) ) )
            .Go();
    }

    [Fact]
    public void Remove_ShouldRemoveViewAndClearReferencedObjects()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var column = table.Columns.Create( "C" );
        var pk = table.Constraints.SetPrimaryKey( column.Asc() );
        var sut = schema.Objects.CreateView( "V", table.Node.ToDataSource().Select( s => new[] { s.From["C"].AsSelf() } ) );

        var actionCount = schema.Database.GetPendingActionCount();
        sut.Remove();
        var actions = schema.Database.GetLastPendingActions( actionCount );

        Assertion.All(
                schema.Objects.TryGet( sut.Name ).TestNull(),
                schema.Objects.Count.TestEquals( 3 ),
                sut.IsRemoved.TestTrue(),
                sut.ReferencedObjects.TestEmpty(),
                table.ReferencingObjects.TestEmpty(),
                column.ReferencingObjects.TestSequence(
                    [ SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( pk.Index ), column ) ] ),
                actions.Select( a => a.Sql ).TestSequence( [ (sql, _) => sql.TestSatisfySql( "DROP VIEW `foo`.`V`;" ) ] ) )
            .Go();
    }

    [Fact]
    public void Remove_ShouldRemoveViewAndClearReferencedObjects_ByOldName()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var column = table.Columns.Create( "C" );
        var pk = table.Constraints.SetPrimaryKey( column.Asc() );
        var sut = schema.Objects.CreateView( "V", table.Node.ToDataSource().Select( s => new[] { s.From["C"].AsSelf() } ) );

        var actionCount = schema.Database.GetPendingActionCount();
        sut.SetName( "X" ).Remove();
        var actions = schema.Database.GetLastPendingActions( actionCount );

        Assertion.All(
                schema.Objects.TryGet( sut.Name ).TestNull(),
                schema.Objects.Count.TestEquals( 3 ),
                sut.IsRemoved.TestTrue(),
                sut.ReferencedObjects.TestEmpty(),
                table.ReferencingObjects.TestEmpty(),
                column.ReferencingObjects.TestSequence(
                    [ SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( pk.Index ), column ) ] ),
                actions.Select( a => a.Sql ).TestSequence( [ (sql, _) => sql.TestSatisfySql( "DROP VIEW `foo`.`V`;" ) ] ) )
            .Go();
    }

    [Fact]
    public void Remove_ShouldDoNothing_WhenViewIsRemoved()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var sut = schema.Objects.CreateView( "V", SqlNode.RawQuery( "SELECT * FROM bar" ) );

        schema.Database.Changes.CompletePendingChanges();
        sut.Remove();

        var actionCount = schema.Database.GetPendingActionCount();
        sut.Remove();
        var actions = schema.Database.GetLastPendingActions( actionCount );

        actions.TestEmpty().Go();
    }

    [Fact]
    public void Remove_ShouldThrowSqlObjectBuilderException_WhenViewIsReferencedByAnyView()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var sut = schema.Objects.CreateView( "V", SqlNode.RawQuery( "SELECT * FROM bar" ) );
        schema.Objects.CreateView( "W", sut.Node.ToDataSource().Select( s => new[] { s.GetAll() } ) );

        var action = Lambda.Of( () => sut.Remove() );

        action.Test(
                exc => exc.TestType()
                    .Exact<SqlObjectBuilderException>(
                        e => Assertion.All( e.Dialect.TestEquals( MySqlDialect.Instance ), e.Errors.Count.TestEquals( 1 ) ) ) )
            .Go();
    }

    [Fact]
    public void ForMySql_ShouldInvokeAction_WhenViewIsMySql()
    {
        var action = Substitute.For<Action<MySqlViewBuilder>>();
        var sut = MySqlDatabaseBuilderMock.Create().Schemas.Default.Objects.CreateView( "V", SqlNode.RawQuery( "SELECT * FROM foo" ) );

        var result = sut.ForMySql( action );

        Assertion.All(
                result.TestRefEquals( sut ),
                action.CallAt( 0 ).Arguments.TestSequence( [ sut ] ) )
            .Go();
    }

    [Fact]
    public void ForMySql_ShouldNotInvokeAction_WhenViewIsNotMySql()
    {
        var action = Substitute.For<Action<MySqlViewBuilder>>();
        var sut = Substitute.For<ISqlViewBuilder>();

        var result = sut.ForMySql( action );

        Assertion.All(
                result.TestRefEquals( sut ),
                action.CallCount().TestEquals( 0 ) )
            .Go();
    }
}
