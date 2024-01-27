using System.Linq;
using LfrlAnvil.Functional;
using LfrlAnvil.MySql.Exceptions;
using LfrlAnvil.MySql.Extensions;
using LfrlAnvil.MySql.Objects.Builders;
using LfrlAnvil.MySql.Tests.Helpers;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.MySql.Tests.ObjectsTests.BuildersTests;

public class MySqlViewBuilderTests : TestsBase
{
    [Fact]
    public void Creation_ShouldPrepareCorrectStatement()
    {
        var db = MySqlDatabaseBuilderMock.Create();
        var schema = db.Schemas.Create( "foo" );
        db.ChangeTracker.ClearStatements();
        schema.Objects.CreateView( "V", SqlNode.RawQuery( "SELECT * FROM foo" ) );

        var statements = db.GetPendingStatements().ToArray();

        using ( new AssertionScope() )
        {
            statements.Should().HaveCount( 1 );
            statements.ElementAtOrDefault( 0 )
                .Sql
                .Should()
                .SatisfySql(
                    @"CREATE VIEW `foo`.`V` AS
                    SELECT * FROM foo;" );
        }
    }

    [Fact]
    public void Creation_FollowedByRemoval_ShouldDoNothing()
    {
        var db = MySqlDatabaseBuilderMock.Create();
        var schema = db.Schemas.Create( "foo" );
        db.ChangeTracker.ClearStatements();
        var sut = schema.Objects.CreateView( "V", SqlNode.RawQuery( "SELECT * FROM foo" ) );
        sut.Remove();

        var statements = db.GetPendingStatements().ToArray();

        statements.Should().BeEmpty();
    }

    [Fact]
    public void ToString_ShouldReturnCorrectResult()
    {
        var db = MySqlDatabaseBuilderMock.Create();
        var sut = db.Schemas.Create( "foo" ).Objects.CreateView( "bar", SqlNode.RawQuery( "SELECT * FROM foo" ) );

        var result = sut.ToString();

        result.Should().Be( "[View] foo.bar" );
    }

    [Fact]
    public void SetName_ShouldDoNothing_WhenNewNameEqualsOldName()
    {
        var name = Fixture.Create<string>();
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Default;
        var sut = schema.Objects.CreateView( name, SqlNode.RawQuery( "SELECT * FROM foo" ) );

        var result = ((ISqlViewBuilder)sut).SetName( name );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.Name.Should().Be( name );
            schema.Objects.Contains( name ).Should().BeTrue();
        }
    }

    [Fact]
    public void SetName_ShouldDoNothing_WhenNameChangesToNewNameAndThenChangesToOldName()
    {
        var (oldName, newName) = Fixture.CreateDistinctCollection<string>( count: 2 );
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Default;
        var sut = schema.Objects.CreateView( oldName, SqlNode.RawQuery( "SELECT * FROM foo" ) );

        var startStatementCount = schema.Database.GetPendingStatements().Length;

        var result = ((ISqlViewBuilder)sut).SetName( newName ).SetName( oldName );
        var statements = schema.Database.GetPendingStatements().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.Name.Should().Be( oldName );
            schema.Objects.Contains( oldName ).Should().BeTrue();
            schema.Objects.Contains( newName ).Should().BeFalse();
            statements.Should().BeEmpty();
        }
    }

    [Fact]
    public void SetName_ShouldUpdateName_WhenNameChangesAndViewDoesNotHaveAnyReferencingViews()
    {
        var (oldName, newName) = ("foo", "bar");
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "s" );
        var sut = schema.Objects.CreateView( oldName, SqlNode.RawQuery( "SELECT * FROM foo" ) );
        var recordSet = sut.RecordSet;

        var startStatementCount = schema.Database.GetPendingStatements().Length;

        var result = ((ISqlViewBuilder)sut).SetName( newName );
        var statements = schema.Database.GetPendingStatements().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.Name.Should().Be( newName );
            sut.Info.Should().Be( SqlRecordSetInfo.Create( "s", "bar" ) );
            recordSet.Info.Should().Be( sut.Info );
            schema.Objects.Contains( newName ).Should().BeTrue();
            schema.Objects.Contains( oldName ).Should().BeFalse();
            statements.Should().HaveCount( 1 );
            statements.ElementAtOrDefault( 0 )
                .Sql
                .Should()
                .SatisfySql(
                    "DROP VIEW `s`.`foo`;",
                    @"CREATE VIEW `s`.`bar` AS
                    SELECT * FROM foo;" );
        }
    }

    [Fact]
    public void SetName_ShouldUpdateName_WhenNameChangesAndViewHasReferencingViews()
    {
        var (oldName, newName) = ("V1", "V2");
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "s" );
        var sut = schema.Objects.CreateView( oldName, SqlNode.RawQuery( "SELECT * FROM foo" ) );
        var w1 = schema.Objects.CreateView( "W1", sut.ToRecordSet().ToDataSource().Select( s => new[] { s.GetAll() } ) );
        schema.Objects.CreateView( "W2", w1.ToRecordSet().ToDataSource().Select( s => new[] { s.GetAll() } ) );
        schema.Objects.CreateView(
            "W3",
            sut.ToRecordSet().Join( w1.ToRecordSet().InnerOn( SqlNode.True() ) ).Select( s => new[] { s.GetAll() } ) );

        var startStatementCount = schema.Database.GetPendingStatements().Length;

        var result = ((ISqlViewBuilder)sut).SetName( newName );
        var statements = schema.Database.GetPendingStatements().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.Name.Should().Be( newName );
            schema.Objects.Contains( newName ).Should().BeTrue();
            schema.Objects.Contains( oldName ).Should().BeFalse();

            statements.Should().HaveCount( 5 );
            statements.ElementAtOrDefault( 0 ).Sql.Should().SatisfySql( "DROP VIEW `s`.`W3`;" );
            statements.ElementAtOrDefault( 1 ).Sql.Should().SatisfySql( "DROP VIEW `s`.`W1`;" );
            statements.ElementAtOrDefault( 2 )
                .Sql
                .Should()
                .SatisfySql(
                    "DROP VIEW `s`.`V1`;",
                    @"CREATE VIEW `s`.`V2` AS
                    SELECT * FROM foo;" );

            statements.ElementAtOrDefault( 3 )
                .Sql
                .Should()
                .SatisfySql(
                    @"CREATE VIEW `s`.`W1` AS
                    SELECT
                      *
                    FROM `s`.`V2`;" );

            statements.ElementAtOrDefault( 4 )
                .Sql
                .Should()
                .SatisfySql(
                    @"CREATE VIEW `s`.`W3` AS
                    SELECT
                      *
                    FROM `s`.`V2`
                    INNER JOIN `s`.`W1` ON TRUE;" );
        }
    }

    [Theory]
    [InlineData( " " )]
    [InlineData( "`" )]
    [InlineData( "'" )]
    [InlineData( "f`oo" )]
    public void SetName_ShouldThrowMySqlObjectBuilderException_WhenNameIsInvalid(string name)
    {
        var db = MySqlDatabaseBuilderMock.Create();
        var sut = db.Schemas.Default.Objects.CreateView( Fixture.Create<string>(), SqlNode.RawQuery( "SELECT * FROM foo" ) );

        var action = Lambda.Of( () => sut.SetName( name ) );

        action.Should()
            .ThrowExactly<MySqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void SetName_ShouldThrowMySqlObjectBuilderException_WhenObjectWithNameAlreadyExists()
    {
        var (name1, name2) = Fixture.CreateDistinctCollection<string>( count: 2 );
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Default;
        var other = schema.Objects.CreateTable( name2 );
        other.Constraints.SetPrimaryKey( other.Columns.Create( "C" ).Asc() );
        var sut = schema.Objects.CreateView( name1, SqlNode.RawQuery( "SELECT * FROM foo" ) );

        var action = Lambda.Of( () => sut.SetName( name2 ) );

        action.Should()
            .ThrowExactly<MySqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void SetName_ShouldThrowMySqlObjectBuilderException_WhenViewHasBeenRemoved()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Default;
        var sut = schema.Objects.CreateView( Fixture.Create<string>(), SqlNode.RawQuery( "SELECT * FROM foo" ) );
        schema.Objects.Remove( sut.Name );

        var action = Lambda.Of( () => sut.SetName( Fixture.Create<string>() ) );

        action.Should()
            .ThrowExactly<MySqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void Remove_ShouldRemoveView_WhenViewDoesNotHaveAnyReferencingViews()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var otherView = schema.Objects.CreateView( "W", SqlNode.RawQuery( "SELECT * FROM foo" ) );
        var sut = schema.Objects.CreateView( "V", otherView.ToRecordSet().ToDataSource().Select( s => new[] { s.GetAll() } ) );

        var startStatementCount = schema.Database.GetPendingStatements().Length;

        sut.Remove();
        var statements = schema.Database.GetPendingStatements().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            schema.Objects.Count.Should().Be( 1 );
            otherView.IsRemoved.Should().BeFalse();
            sut.IsRemoved.Should().BeTrue();
            otherView.ReferencingViews.Should().BeEmpty();
            sut.ReferencedObjects.Should().BeEmpty();
            statements.Should().HaveCount( 1 );
            statements.ElementAtOrDefault( 0 ).Sql.Should().SatisfySql( "DROP VIEW `foo`.`V`;" );
        }
    }

    [Fact]
    public void Remove_ShouldDoNothing_WhenViewHasAlreadyBeenRemoved()
    {
        var name = Fixture.Create<string>();
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Default;
        var sut = schema.Objects.CreateView( name, SqlNode.RawQuery( "SELECT * FROM foo" ) );
        sut.Remove();

        sut.Remove();

        using ( new AssertionScope() )
        {
            schema.Objects.Contains( name ).Should().BeFalse();
            sut.IsRemoved.Should().BeTrue();
        }
    }

    [Fact]
    public void Remove_ShouldThrowMySqlObjectBuilderException_WhenViewIsReferencedByAnyView()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var sut = schema.Objects.CreateView( "V", SqlNode.RawQuery( "SELECT * FROM foo" ) );
        schema.Objects.CreateView( "W", sut.ToRecordSet().ToDataSource().Select( s => new[] { s.GetAll() } ) );

        var action = Lambda.Of( () => sut.Remove() );

        action.Should()
            .ThrowExactly<MySqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void NameChange_ThenRemoval_ShouldDropViewByUsingItsOldName()
    {
        var builder = MySqlDatabaseBuilderMock.Create();
        var sut = builder.Schemas.Create( "s" ).Objects.CreateView( "foo", SqlNode.RawQuery( "SELECT * FROM foo" ) );
        _ = builder.GetPendingStatements();

        sut.SetName( "bar" );
        sut.Remove();

        var result = builder.GetPendingStatements()[^1].Sql;

        result.Should().SatisfySql( "DROP VIEW `s`.`foo`;" );
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
