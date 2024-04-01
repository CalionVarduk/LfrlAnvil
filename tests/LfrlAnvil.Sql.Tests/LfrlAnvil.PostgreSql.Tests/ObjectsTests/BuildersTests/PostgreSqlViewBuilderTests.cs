using System.Linq;
using LfrlAnvil.Functional;
using LfrlAnvil.PostgreSql.Extensions;
using LfrlAnvil.PostgreSql.Objects.Builders;
using LfrlAnvil.PostgreSql.Tests.Helpers;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.TestExtensions.FluentAssertions;
using LfrlAnvil.TestExtensions.Sql;
using LfrlAnvil.TestExtensions.Sql.FluentAssertions;

namespace LfrlAnvil.PostgreSql.Tests.ObjectsTests.BuildersTests;

public class PostgreSqlViewBuilderTests : TestsBase
{
    [Fact]
    public void ToString_ShouldReturnCorrectResult()
    {
        var schema = PostgreSqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var sut = schema.Objects.CreateView( "bar", SqlNode.RawQuery( "SELECT * FROM foo" ) );

        var result = sut.ToString();

        result.Should().Be( "[View] foo.bar" );
    }

    [Fact]
    public void Creation_ShouldPrepareCorrectStatement()
    {
        var schema = PostgreSqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );

        var actionCount = schema.Database.GetPendingActionCount();
        var sut = schema.Objects.CreateView( "V", SqlNode.RawQuery( "SELECT * FROM bar" ) );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            schema.Objects.TryGet( sut.Name ).Should().BeSameAs( sut );
            sut.Name.Should().Be( "V" );
            sut.ReferencedObjects.Should().BeEmpty();

            actions.Should().HaveCount( 1 );
            actions.ElementAtOrDefault( 0 )
                .Sql.Should()
                .SatisfySql(
                    @"CREATE VIEW ""foo"".""V"" AS
                    SELECT * FROM bar;" );
        }
    }

    [Fact]
    public void Creation_FollowedByRemoval_ShouldDoNothing()
    {
        var schema = PostgreSqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );

        var actionCount = schema.Database.GetPendingActionCount();
        var sut = schema.Objects.CreateView( "V", SqlNode.RawQuery( "SELECT * FROM bar" ) );
        sut.Remove();
        var actions = schema.Database.GetLastPendingActions( actionCount );

        actions.Should().BeEmpty();
    }

    [Fact]
    public void SetName_ShouldDoNothing_WhenNewNameEqualsOldName()
    {
        var schema = PostgreSqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var sut = schema.Objects.CreateView( "V", SqlNode.RawQuery( "SELECT * FROM bar" ) );

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
        var sut = schema.Objects.CreateView( "V", SqlNode.RawQuery( "SELECT * FROM bar" ) );
        var oldName = sut.Name;

        var actionCount = schema.Database.GetPendingActionCount();
        sut.SetName( "W" );
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
        var sut = schema.Objects.CreateView( "V", SqlNode.RawQuery( "SELECT * FROM bar" ) );
        var oldName = sut.Name;
        var recordSet = sut.Node;
        _ = sut.Info;

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.SetName( "bar" );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.Name.Should().Be( "bar" );
            sut.Info.Should().Be( SqlRecordSetInfo.Create( "foo", "bar" ) );
            recordSet.Info.Should().Be( sut.Info );
            schema.Objects.TryGet( "bar" ).Should().BeSameAs( sut );
            schema.Objects.TryGet( oldName ).Should().BeNull();

            actions.Should().HaveCount( 1 );
            actions.ElementAtOrDefault( 0 )
                .Sql.Should()
                .SatisfySql( "ALTER VIEW \"foo\".\"V\" RENAME TO \"bar\";" );
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
        var sut = schema.Objects.CreateView( "V", SqlNode.RawQuery( "SELECT * FROM bar" ) );

        var action = Lambda.Of( () => sut.SetName( name ) );

        action.Should()
            .ThrowExactly<SqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == PostgreSqlDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void SetName_ShouldThrowSqlObjectBuilderException_WhenViewIsRemoved()
    {
        var schema = PostgreSqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var sut = schema.Objects.CreateView( "V", SqlNode.RawQuery( "SELECT * FROM bar" ) );
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
        var other = schema.Objects.CreateTable( "T" );
        other.Constraints.SetPrimaryKey( other.Columns.Create( "C" ).Asc() );
        var sut = schema.Objects.CreateView( "V", SqlNode.RawQuery( "SELECT * FROM bar" ) );

        var action = Lambda.Of( () => sut.SetName( "T" ) );

        action.Should()
            .ThrowExactly<SqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == PostgreSqlDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void Remove_ShouldRemoveViewAndClearReferencedObjects()
    {
        var schema = PostgreSqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var column = table.Columns.Create( "C" );
        var pk = table.Constraints.SetPrimaryKey( column.Asc() );
        var sut = schema.Objects.CreateView( "V", table.Node.ToDataSource().Select( s => new[] { s.From["C"].AsSelf() } ) );

        var actionCount = schema.Database.GetPendingActionCount();
        sut.Remove();
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            schema.Objects.TryGet( sut.Name ).Should().BeNull();
            schema.Objects.Count.Should().Be( 3 );
            sut.IsRemoved.Should().BeTrue();
            sut.ReferencedObjects.Should().BeEmpty();

            table.ReferencingObjects.Should().BeEmpty();
            column.ReferencingObjects.Should()
                .BeSequentiallyEqualTo(
                    SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( pk.Index ), column ) );

            actions.Should().HaveCount( 1 );
            actions.ElementAtOrDefault( 0 ).Sql.Should().SatisfySql( "DROP VIEW \"foo\".\"V\";" );
        }
    }

    [Fact]
    public void Remove_ShouldRemoveViewAndClearReferencedObjects_ByOldName()
    {
        var schema = PostgreSqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var column = table.Columns.Create( "C" );
        var pk = table.Constraints.SetPrimaryKey( column.Asc() );
        var sut = schema.Objects.CreateView( "V", table.Node.ToDataSource().Select( s => new[] { s.From["C"].AsSelf() } ) );

        var actionCount = schema.Database.GetPendingActionCount();
        sut.SetName( "X" ).Remove();
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            schema.Objects.TryGet( sut.Name ).Should().BeNull();
            schema.Objects.Count.Should().Be( 3 );
            sut.IsRemoved.Should().BeTrue();
            sut.ReferencedObjects.Should().BeEmpty();

            table.ReferencingObjects.Should().BeEmpty();
            column.ReferencingObjects.Should()
                .BeSequentiallyEqualTo(
                    SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( pk.Index ), column ) );

            actions.Should().HaveCount( 1 );
            actions.ElementAtOrDefault( 0 ).Sql.Should().SatisfySql( "DROP VIEW \"foo\".\"V\";" );
        }
    }

    [Fact]
    public void Remove_ShouldDoNothing_WhenViewIsRemoved()
    {
        var schema = PostgreSqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var sut = schema.Objects.CreateView( "V", SqlNode.RawQuery( "SELECT * FROM bar" ) );

        schema.Database.Changes.CompletePendingChanges();
        sut.Remove();

        var actionCount = schema.Database.GetPendingActionCount();
        sut.Remove();
        var actions = schema.Database.GetLastPendingActions( actionCount );

        actions.Should().BeEmpty();
    }

    [Fact]
    public void Remove_ShouldThrowSqlObjectBuilderException_WhenViewIsReferencedByAnyView()
    {
        var schema = PostgreSqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var sut = schema.Objects.CreateView( "V", SqlNode.RawQuery( "SELECT * FROM bar" ) );
        schema.Objects.CreateView( "W", sut.Node.ToDataSource().Select( s => new[] { s.GetAll() } ) );

        var action = Lambda.Of( () => sut.Remove() );

        action.Should()
            .ThrowExactly<SqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == PostgreSqlDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void ForPostgreSql_ShouldInvokeAction_WhenViewIsPostgreSql()
    {
        var action = Substitute.For<Action<PostgreSqlViewBuilder>>();
        var sut = PostgreSqlDatabaseBuilderMock.Create().Schemas.Default.Objects.CreateView( "V", SqlNode.RawQuery( "SELECT * FROM foo" ) );

        var result = sut.ForPostgreSql( action );

        result.Should().BeSameAs( sut );
        action.Verify().CallAt( 0 ).Exists().And.Arguments.Should().BeSequentiallyEqualTo( sut );
    }

    [Fact]
    public void ForPostgreSql_ShouldNotInvokeAction_WhenViewIsNotPostgreSql()
    {
        var action = Substitute.For<Action<PostgreSqlViewBuilder>>();
        var sut = Substitute.For<ISqlViewBuilder>();

        var result = sut.ForPostgreSql( action );

        result.Should().BeSameAs( sut );
        action.Verify().CallCount.Should().Be( 0 );
    }
}
