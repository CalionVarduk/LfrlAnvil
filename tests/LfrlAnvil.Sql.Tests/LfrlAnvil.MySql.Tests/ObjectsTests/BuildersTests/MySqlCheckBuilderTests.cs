using System.Linq;
using System.Text.RegularExpressions;
using LfrlAnvil.Functional;
using LfrlAnvil.MySql.Extensions;
using LfrlAnvil.MySql.Objects.Builders;
using LfrlAnvil.MySql.Tests.Helpers;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.MySql.Tests.ObjectsTests.BuildersTests;

public class MySqlCheckBuilderTests : TestsBase
{
    [Fact]
    public void ToString_ShouldReturnCorrectResult()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var column = table.Columns.Create( "C" );
        var sut = table.Constraints.CreateCheck( column.Node > SqlNode.Literal( 0 ) );

        var result = sut.ToString();

        result.TestMatch( new Regex( "\\[Check\\] foo.CHK_T_[0-9a-fA-F]{32}" ) ).Go();
    }

    [Fact]
    public void Creation_ShouldMarkTableForAlteration()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
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
                            ALTER TABLE `foo`.`T`
                                ADD CONSTRAINT `CHK_T_{GUID}` CHECK (`C` > 0);
                            """ )
                    ] ) )
            .Go();
    }

    [Fact]
    public void Creation_FollowedByRemove_ShouldDoNothing()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
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
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
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
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
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
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
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
                            ALTER TABLE `foo`.`T`
                                DROP CHECK `CHK_T_{GUID}`,
                                ADD CONSTRAINT `bar` CHECK (`C` > 0);
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
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C" ).Asc() );
        var sut = table.Constraints.CreateCheck( SqlNode.True() );

        var action = Lambda.Of( () => sut.SetName( name ) );

        action.Test(
                exc => exc.TestType()
                    .Exact<SqlObjectBuilderException>(
                        e => Assertion.All( e.Dialect.TestEquals( MySqlDialect.Instance ), e.Errors.Count.TestEquals( 1 ) ) ) )
            .Go();
    }

    [Fact]
    public void SetName_ShouldThrowSqlObjectBuilderException_WhenCheckIsRemoved()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C" ).Asc() );
        var sut = table.Constraints.CreateCheck( SqlNode.True() );
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
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C" ).Asc() );
        var other = table.Constraints.CreateCheck( SqlNode.True() );
        var sut = table.Constraints.CreateCheck( SqlNode.True() );

        var action = Lambda.Of( () => sut.SetName( other.Name ) );

        action.Test(
                exc => exc.TestType()
                    .Exact<SqlObjectBuilderException>(
                        e => Assertion.All( e.Dialect.TestEquals( MySqlDialect.Instance ), e.Errors.Count.TestEquals( 1 ) ) ) )
            .Go();
    }

    [Fact]
    public void SetDefaultName_ShouldUpdateName_WhenNewNameIsDifferentFromOldName()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
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
                            ALTER TABLE `foo`.`T`
                                DROP CHECK `bar`,
                                ADD CONSTRAINT `CHK_T_{GUID}` CHECK (TRUE);
                            """ )
                    ] ) )
            .Go();
    }

    [Fact]
    public void SetDefaultName_ShouldThrowSqlObjectBuilderException_WhenCheckIsRemoved()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Constraints.CreateCheck( SqlNode.True() ).SetName( "bar" );
        sut.Remove();

        var action = Lambda.Of( () => sut.SetDefaultName() );

        action.Test(
                exc => exc.TestType()
                    .Exact<SqlObjectBuilderException>(
                        e => Assertion.All( e.Dialect.TestEquals( MySqlDialect.Instance ), e.Errors.Count.TestEquals( 1 ) ) ) )
            .Go();
    }

    [Fact]
    public void Remove_ShouldRemoveCheckAndClearReferencedColumns()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var column = table.Columns.Create( "C" );
        var pk = table.Constraints.SetPrimaryKey( column.Asc() );
        var sut = table.Constraints.CreateCheck( column.Node > SqlNode.Literal( 0 ) );

        var actionCount = schema.Database.GetPendingActionCount();
        sut.SetName( "bar" ).Remove();
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
                            ALTER TABLE `foo`.`T`
                                DROP CHECK `CHK_T_{GUID}`;
                            """ )
                    ] ) )
            .Go();
    }

    [Fact]
    public void Remove_ShouldDoNothing_WhenCheckIsRemoved()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
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
    public void ForMySql_ShouldInvokeAction_WhenCheckIsMySql()
    {
        var action = Substitute.For<Action<MySqlCheckBuilder>>();
        var table = MySqlDatabaseBuilderMock.Create().Schemas.Default.Objects.CreateTable( "T" );
        var column = table.Columns.Create( "C" );
        var sut = table.Constraints.CreateCheck( column.Node > SqlNode.Literal( 0 ) );

        var result = sut.ForMySql( action );

        Assertion.All(
                result.TestRefEquals( sut ),
                action.CallAt( 0 ).Arguments.TestSequence( [ sut ] ) )
            .Go();
    }

    [Fact]
    public void ForMySql_ShouldNotInvokeAction_WhenCheckIsNotMySql()
    {
        var action = Substitute.For<Action<MySqlCheckBuilder>>();
        var sut = Substitute.For<ISqlCheckBuilder>();

        var result = sut.ForMySql( action );

        Assertion.All(
                result.TestRefEquals( sut ),
                action.CallCount().TestEquals( 0 ) )
            .Go();
    }
}
