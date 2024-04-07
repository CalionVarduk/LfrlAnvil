using System.Linq;
using LfrlAnvil.Functional;
using LfrlAnvil.MySql.Extensions;
using LfrlAnvil.MySql.Objects.Builders;
using LfrlAnvil.MySql.Tests.Helpers;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.TestExtensions.FluentAssertions;
using LfrlAnvil.TestExtensions.Sql;
using LfrlAnvil.TestExtensions.Sql.FluentAssertions;

namespace LfrlAnvil.MySql.Tests.ObjectsTests.BuildersTests;

public class MySqlViewBuilderTests : TestsBase
{
    [Fact]
    public void ToString_ShouldReturnCorrectResult()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var sut = schema.Objects.CreateView( "bar", SqlNode.RawQuery( "SELECT * FROM foo" ) );

        var result = sut.ToString();

        result.Should().Be( "[View] foo.bar" );
    }

    [Fact]
    public void Creation_ShouldPrepareCorrectStatement()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );

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
                    @"CREATE VIEW `foo`.`V` AS
                    SELECT * FROM bar;" );
        }
    }

    [Fact]
    public void Creation_FollowedByRemoval_ShouldDoNothing()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );

        var actionCount = schema.Database.GetPendingActionCount();
        var sut = schema.Objects.CreateView( "V", SqlNode.RawQuery( "SELECT * FROM bar" ) );
        sut.Remove();
        var actions = schema.Database.GetLastPendingActions( actionCount );

        actions.Should().BeEmpty();
    }

    [Fact]
    public void SetName_ShouldDoNothing_WhenNewNameEqualsOldName()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
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
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
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
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
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
                .SatisfySql(
                    "DROP VIEW `foo`.`V`;",
                    @"CREATE VIEW `foo`.`bar` AS
                    SELECT * FROM bar;" );
        }
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

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.Name.Should().Be( "bar" );
            sut.Info.Should().Be( SqlRecordSetInfo.Create( "foo", "bar" ) );
            recordSet.Info.Should().Be( sut.Info );
            schema.Objects.TryGet( "bar" ).Should().BeSameAs( sut );
            schema.Objects.TryGet( oldName ).Should().BeNull();

            actions.Should().HaveCount( 3 );
            actions.ElementAtOrDefault( 0 )
                .Sql.Should()
                .SatisfySql(
                    "DROP VIEW `foo`.`V`;",
                    @"CREATE VIEW `foo`.`bar` AS
                    SELECT * FROM bar;" );

            actions.ElementAtOrDefault( 1 )
                .Sql.Should()
                .SatisfySql(
                    "DROP VIEW `foo`.`W1`;",
                    @"CREATE VIEW `foo`.`W1` AS
                    SELECT
                      *
                    FROM `foo`.`bar`;" );

            actions.ElementAtOrDefault( 2 )
                .Sql.Should()
                .SatisfySql(
                    "DROP VIEW `foo`.`W3`;",
                    @"CREATE VIEW `foo`.`W3` AS
                    SELECT
                      *
                    FROM `foo`.`bar`
                    INNER JOIN `foo`.`W1` ON TRUE;" );
        }
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

        action.Should()
            .ThrowExactly<SqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void SetName_ShouldThrowSqlObjectBuilderException_WhenViewIsRemoved()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var sut = schema.Objects.CreateView( "V", SqlNode.RawQuery( "SELECT * FROM bar" ) );
        sut.Remove();

        var action = Lambda.Of( () => sut.SetName( "bar" ) );

        action.Should()
            .ThrowExactly<SqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void SetName_ShouldThrowSqlObjectBuilderException_WhenNewNameAlreadyExistsInSchemaObjects()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var other = schema.Objects.CreateTable( "T" );
        other.Constraints.SetPrimaryKey( other.Columns.Create( "C" ).Asc() );
        var sut = schema.Objects.CreateView( "V", SqlNode.RawQuery( "SELECT * FROM bar" ) );

        var action = Lambda.Of( () => sut.SetName( "T" ) );

        action.Should()
            .ThrowExactly<SqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
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

        using ( new AssertionScope() )
        {
            schema.Objects.TryGet( sut.Name ).Should().BeNull();
            schema.Objects.Count.Should().Be( 3 );
            sut.IsRemoved.Should().BeTrue();
            sut.ReferencedObjects.Should().BeEmpty();

            table.ReferencingObjects.Should().BeEmpty();
            column.ReferencingObjects.Should()
                .BeSequentiallyEqualTo( SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( pk.Index ), column ) );

            actions.Should().HaveCount( 1 );
            actions.ElementAtOrDefault( 0 ).Sql.Should().SatisfySql( "DROP VIEW `foo`.`V`;" );
        }
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

        using ( new AssertionScope() )
        {
            schema.Objects.TryGet( sut.Name ).Should().BeNull();
            schema.Objects.Count.Should().Be( 3 );
            sut.IsRemoved.Should().BeTrue();
            sut.ReferencedObjects.Should().BeEmpty();

            table.ReferencingObjects.Should().BeEmpty();
            column.ReferencingObjects.Should()
                .BeSequentiallyEqualTo( SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( pk.Index ), column ) );

            actions.Should().HaveCount( 1 );
            actions.ElementAtOrDefault( 0 ).Sql.Should().SatisfySql( "DROP VIEW `foo`.`V`;" );
        }
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

        actions.Should().BeEmpty();
    }

    [Fact]
    public void Remove_ShouldThrowSqlObjectBuilderException_WhenViewIsReferencedByAnyView()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var sut = schema.Objects.CreateView( "V", SqlNode.RawQuery( "SELECT * FROM bar" ) );
        schema.Objects.CreateView( "W", sut.Node.ToDataSource().Select( s => new[] { s.GetAll() } ) );

        var action = Lambda.Of( () => sut.Remove() );

        action.Should()
            .ThrowExactly<SqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void ForMySql_ShouldInvokeAction_WhenViewIsMySql()
    {
        var action = Substitute.For<Action<MySqlViewBuilder>>();
        var sut = MySqlDatabaseBuilderMock.Create().Schemas.Default.Objects.CreateView( "V", SqlNode.RawQuery( "SELECT * FROM foo" ) );

        var result = sut.ForMySql( action );

        result.Should().BeSameAs( sut );
        action.Verify().CallAt( 0 ).Exists().And.Arguments.Should().BeSequentiallyEqualTo( sut );
    }

    [Fact]
    public void ForMySql_ShouldNotInvokeAction_WhenViewIsNotMySql()
    {
        var action = Substitute.For<Action<MySqlViewBuilder>>();
        var sut = Substitute.For<ISqlViewBuilder>();

        var result = sut.ForMySql( action );

        result.Should().BeSameAs( sut );
        action.Verify().CallCount.Should().Be( 0 );
    }
}
