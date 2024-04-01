using System.Linq;
using LfrlAnvil.Functional;
using LfrlAnvil.PostgreSql.Extensions;
using LfrlAnvil.PostgreSql.Objects.Builders;
using LfrlAnvil.PostgreSql.Tests.Helpers;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.TestExtensions.FluentAssertions;
using LfrlAnvil.TestExtensions.Sql;
using LfrlAnvil.TestExtensions.Sql.FluentAssertions;

namespace LfrlAnvil.PostgreSql.Tests.ObjectsTests.BuildersTests;

public class PostgreSqlCheckBuilderTests : TestsBase
{
    [Fact]
    public void ToString_ShouldReturnCorrectResult()
    {
        var schema = PostgreSqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var column = table.Columns.Create( "C" );
        var sut = table.Constraints.CreateCheck( column.Node > SqlNode.Literal( 0 ) );

        var result = sut.ToString();

        result.Should().MatchRegex( "\\[Check\\] foo.CHK_T_[0-9a-fA-F]{32}" );
    }

    [Fact]
    public void Creation_ShouldMarkTableForAlteration()
    {
        var schema = PostgreSqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var column = table.Columns.Create( "C" );
        var pk = table.Constraints.SetPrimaryKey( column.Asc() );

        var actionCount = schema.Database.GetPendingActionCount();
        var sut = table.Constraints.CreateCheck( column.Node > SqlNode.Literal( 0 ) );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            table.Constraints.GetCheck( sut.Name ).Should().BeSameAs( sut );
            schema.Objects.TryGet( sut.Name ).Should().BeSameAs( sut );
            sut.Name.Should().MatchRegex( "CHK_T_[0-9a-fA-F]{32}" );
            sut.ReferencedColumns.Should().BeSequentiallyEqualTo( column );

            column.ReferencingObjects.Should().HaveCount( 2 );
            column.ReferencingObjects.Should()
                .BeEquivalentTo(
                    SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( pk.Index ), column ),
                    SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( sut ), column ) );

            actions.Should().HaveCount( 1 );
            actions.ElementAtOrDefault( 0 )
                .Sql.Should()
                .SatisfySql(
                    @"ALTER TABLE ""foo"".""T""
                      ADD CONSTRAINT ""CHK_T_{GUID}"" CHECK (""C"" > 0);" );
        }
    }

    [Fact]
    public void Creation_FollowedByRemove_ShouldDoNothing()
    {
        var schema = PostgreSqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C" ).Asc() );

        var actionCount = schema.Database.GetPendingActionCount();
        var sut = table.Constraints.CreateCheck( SqlNode.True() );
        sut.Remove();
        var actions = schema.Database.GetLastPendingActions( actionCount );

        actions.Should().BeEmpty();
    }

    [Fact]
    public void SetName_ShouldDoNothing_WhenNewNameEqualsOldName()
    {
        var schema = PostgreSqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C" ).Asc() );
        var sut = table.Constraints.CreateCheck( SqlNode.True() );

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.SetName( sut.Name );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            actions.Should().BeEmpty();
        }
    }

    [Fact]
    public void SetName_ShouldDoNothing_WhenNameChangeIsFollowedByChangeToOriginal()
    {
        var schema = PostgreSqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C" ).Asc() );
        var sut = table.Constraints.CreateCheck( SqlNode.True() );
        var oldName = sut.Name;

        var actionCount = schema.Database.GetPendingActionCount();
        sut.SetName( "bar" );
        var result = sut.SetName( oldName );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            actions.Should().BeEmpty();
        }
    }

    [Fact]
    public void SetName_ShouldUpdateName_WhenNewNameIsDifferentFromOldName()
    {
        var schema = PostgreSqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C" ).Asc() );
        var sut = table.Constraints.CreateCheck( table.Node["C"] > SqlNode.Literal( 0 ) );
        var oldName = sut.Name;

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.SetName( "bar" );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.Name.Should().Be( "bar" );
            table.Constraints.TryGet( "bar" ).Should().BeSameAs( sut );
            table.Constraints.TryGet( oldName ).Should().BeNull();
            schema.Objects.TryGet( "bar" ).Should().BeSameAs( sut );
            schema.Objects.TryGet( oldName ).Should().BeNull();

            actions.Should().HaveCount( 1 );
            actions.ElementAtOrDefault( 0 )
                .Sql.Should()
                .SatisfySql(
                    @"ALTER TABLE ""foo"".""T""
                      RENAME CONSTRAINT ""CHK_T_{GUID}"" TO ""bar"";" );
        }
    }

    [Theory]
    [InlineData( "" )]
    [InlineData( " " )]
    [InlineData( "\"" )]
    [InlineData( "'" )]
    [InlineData( "f\"oo" )]
    public void SetName_ShouldThrowSqlObjectBuilderException_WhenNameIsInvalid(string name)
    {
        var schema = PostgreSqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C" ).Asc() );
        var sut = table.Constraints.CreateCheck( SqlNode.True() );

        var action = Lambda.Of( () => sut.SetName( name ) );

        action.Should()
            .ThrowExactly<SqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == PostgreSqlDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void SetName_ShouldThrowSqlObjectBuilderException_WhenCheckIsRemoved()
    {
        var schema = PostgreSqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C" ).Asc() );
        var sut = table.Constraints.CreateCheck( SqlNode.True() );
        sut.Remove();

        var action = Lambda.Of( () => sut.SetName( "bar" ) );

        action.Should()
            .ThrowExactly<SqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == PostgreSqlDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void SetName_ShouldThrowSqlObjectBuilderException_WhenNewNameAlreadyExistsInSchemaObjects()
    {
        var schema = PostgreSqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C" ).Asc() );
        var other = table.Constraints.CreateCheck( SqlNode.True() );
        var sut = table.Constraints.CreateCheck( SqlNode.True() );

        var action = Lambda.Of( () => sut.SetName( other.Name ) );

        action.Should()
            .ThrowExactly<SqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == PostgreSqlDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void SetDefaultName_ShouldUpdateName_WhenNewNameIsDifferentFromOldName()
    {
        var schema = PostgreSqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Constraints.CreateCheck( SqlNode.True() ).SetName( "bar" );
        var oldName = sut.Name;

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.SetDefaultName();
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.Name.Should().MatchRegex( "CHK_T_[0-9a-fA-F]{32}" );
            table.Constraints.TryGet( result.Name ).Should().BeSameAs( sut );
            table.Constraints.TryGet( oldName ).Should().BeNull();
            schema.Objects.TryGet( result.Name ).Should().BeSameAs( sut );
            schema.Objects.TryGet( oldName ).Should().BeNull();

            actions.Should().HaveCount( 1 );
            actions.ElementAtOrDefault( 0 )
                .Sql.Should()
                .SatisfySql(
                    @"ALTER TABLE ""foo"".""T""
                      RENAME CONSTRAINT ""bar"" TO ""CHK_T_{GUID}"";" );
        }
    }

    [Fact]
    public void SetDefaultName_ShouldThrowSqlObjectBuilderException_WhenCheckIsRemoved()
    {
        var schema = PostgreSqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Constraints.CreateCheck( SqlNode.True() ).SetName( "bar" );
        sut.Remove();

        var action = Lambda.Of( () => sut.SetDefaultName() );

        action.Should()
            .ThrowExactly<SqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == PostgreSqlDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void Remove_ShouldRemoveCheckAndClearReferencedColumns()
    {
        var schema = PostgreSqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var column = table.Columns.Create( "C" );
        var pk = table.Constraints.SetPrimaryKey( column.Asc() );
        var sut = table.Constraints.CreateCheck( column.Node > SqlNode.Literal( 0 ) );

        var actionCount = schema.Database.GetPendingActionCount();
        sut.SetName( "bar" ).Remove();
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            table.Constraints.TryGet( sut.Name ).Should().BeNull();
            schema.Objects.TryGet( sut.Name ).Should().BeNull();
            sut.IsRemoved.Should().BeTrue();
            sut.ReferencedColumns.Should().BeEmpty();

            column.ReferencingObjects.Should()
                .BeSequentiallyEqualTo(
                    SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( pk.Index ), column ) );

            actions.Should().HaveCount( 1 );
            actions.ElementAtOrDefault( 0 )
                .Sql.Should()
                .SatisfySql(
                    @"ALTER TABLE ""foo"".""T""
                      DROP CONSTRAINT ""CHK_T_{GUID}"";" );
        }
    }

    [Fact]
    public void Remove_ShouldDoNothing_WhenCheckIsRemoved()
    {
        var schema = PostgreSqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C" ).Asc() );
        var sut = table.Constraints.CreateCheck( SqlNode.True() );

        schema.Database.Changes.CompletePendingChanges();
        sut.Remove();

        var actionCount = schema.Database.GetPendingActionCount();
        sut.Remove();
        var actions = schema.Database.GetLastPendingActions( actionCount );

        actions.Should().BeEmpty();
    }

    [Fact]
    public void ForPostgreSql_ShouldInvokeAction_WhenCheckIsPostgreSql()
    {
        var action = Substitute.For<Action<PostgreSqlCheckBuilder>>();
        var table = PostgreSqlDatabaseBuilderMock.Create().Schemas.Default.Objects.CreateTable( "T" );
        var column = table.Columns.Create( "C" );
        var sut = table.Constraints.CreateCheck( column.Node > SqlNode.Literal( 0 ) );

        var result = sut.ForPostgreSql( action );

        result.Should().BeSameAs( sut );
        action.Verify().CallAt( 0 ).Exists().And.Arguments.Should().BeSequentiallyEqualTo( sut );
    }

    [Fact]
    public void ForPostgreSql_ShouldNotInvokeAction_WhenCheckIsNotPostgreSql()
    {
        var action = Substitute.For<Action<PostgreSqlCheckBuilder>>();
        var sut = Substitute.For<ISqlCheckBuilder>();

        var result = sut.ForPostgreSql( action );

        result.Should().BeSameAs( sut );
        action.Verify().CallCount.Should().Be( 0 );
    }
}
