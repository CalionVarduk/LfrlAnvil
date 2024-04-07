using System.Linq;
using LfrlAnvil.Functional;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Extensions;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.TestExtensions.FluentAssertions;
using LfrlAnvil.TestExtensions.Sql.Mocks;

namespace LfrlAnvil.Sql.Tests.ObjectsTests.BuildersTests;

public class SqlViewBuilderTests : TestsBase
{
    [Fact]
    public void ToString_ShouldReturnCorrectResult()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var sut = schema.Objects.CreateView( "bar", SqlNode.RawQuery( "SELECT * FROM foo" ) );

        var result = sut.ToString();

        result.Should().Be( "[View] foo.bar" );
    }

    [Fact]
    public void Creation_ShouldMarkViewForCreation()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );

        var actionCount = schema.Database.GetPendingActionCount();
        var sut = schema.Objects.CreateView( "V", SqlNode.RawQuery( "SELECT * FROM bar" ) );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            schema.Objects.TryGet( sut.Name ).Should().BeSameAs( sut );
            sut.Name.Should().Be( "V" );
            sut.ReferencedObjects.Should().BeEmpty();

            actions.Should().HaveCount( 1 );
            actions.ElementAtOrDefault( 0 ).Sql.Should().Be( "CREATE [View] foo.V;" );
        }
    }

    [Fact]
    public void Creation_FollowedByRemoval_ShouldDoNothing()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );

        var actionCount = schema.Database.GetPendingActionCount();
        var sut = schema.Objects.CreateView( "V", SqlNode.RawQuery( "SELECT * FROM bar" ) );
        sut.Remove();
        var actions = schema.Database.GetLastPendingActions( actionCount );

        actions.Should().BeEmpty();
    }

    [Fact]
    public void SetName_ShouldDoNothing_WhenNewNameEqualsOldName()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
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
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
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
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
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
                .Be(
                    @"ALTER [View] foo.bar
  ALTER [View] foo.bar ([1] : 'Name' (System.String) FROM V);" );
        }
    }

    [Theory]
    [InlineData( "" )]
    [InlineData( " " )]
    [InlineData( "'" )]
    [InlineData( "f\'oo" )]
    public void SetName_ShouldThrowSqlObjectBuilderException_WhenNameIsInvalid(string name)
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var sut = schema.Objects.CreateView( "V", SqlNode.RawQuery( "SELECT * FROM bar" ) );

        var action = Lambda.Of( () => sut.SetName( name ) );

        action.Should()
            .ThrowExactly<SqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == SqlDialectMock.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void SetName_ShouldThrowSqlObjectBuilderException_WhenViewIsRemoved()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var sut = schema.Objects.CreateView( "V", SqlNode.RawQuery( "SELECT * FROM bar" ) );
        sut.Remove();

        var action = Lambda.Of( () => sut.SetName( "bar" ) );

        action.Should()
            .ThrowExactly<SqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == SqlDialectMock.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void SetName_ShouldThrowSqlObjectBuilderException_WhenNewNameAlreadyExistsInSchemaObjects()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var other = schema.Objects.CreateTable( "T" );
        other.Constraints.SetPrimaryKey( other.Columns.Create( "C" ).Asc() );
        var sut = schema.Objects.CreateView( "V", SqlNode.RawQuery( "SELECT * FROM bar" ) );

        var action = Lambda.Of( () => sut.SetName( "T" ) );

        action.Should()
            .ThrowExactly<SqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == SqlDialectMock.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void Remove_ShouldRemoveViewAndClearReferencedObjects()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var column = table.Columns.Create( "C" );
        var sut = schema.Objects.CreateView( "V", table.Node.ToDataSource().Select( s => new[] { s.From["C"].AsSelf() } ) );

        var actionCount = schema.Database.GetPendingActionCount();
        sut.Remove();
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            schema.Objects.TryGet( sut.Name ).Should().BeNull();
            schema.Objects.Count.Should().Be( 1 );
            sut.IsRemoved.Should().BeTrue();
            sut.ReferencedObjects.Should().BeEmpty();

            table.ReferencingObjects.Should().BeEmpty();
            column.ReferencingObjects.Should().BeEmpty();

            actions.Should().HaveCount( 1 );
            actions.ElementAtOrDefault( 0 ).Sql.Should().Be( "REMOVE [View] foo.V;" );
        }
    }

    [Fact]
    public void Remove_ShouldDoNothing_WhenViewIsRemoved()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
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
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var sut = schema.Objects.CreateView( "V", SqlNode.RawQuery( "SELECT * FROM bar" ) );
        schema.Objects.CreateView( "W", sut.Node.ToDataSource().Select( s => new[] { s.GetAll() } ) );

        var action = Lambda.Of( () => sut.Remove() );

        action.Should()
            .ThrowExactly<SqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == SqlDialectMock.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void QuickRemove_ShouldClearReferencingObjectsAndReferencedObjects()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var column = table.Columns.Create( "C" );
        var sut = schema.Objects.CreateView( "V", table.Node.ToDataSource().Select( s => new[] { s.From["C"].AsSelf() } ) );
        var other = schema.Objects.CreateView( "W", sut.Node.ToDataSource().Select( s => new[] { s.GetAll() } ) );

        var actionCount = schema.Database.GetPendingActionCount();
        SqlDatabaseBuilderMock.QuickRemove( sut );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            schema.Objects.TryGet( sut.Name ).Should().BeSameAs( sut );
            sut.IsRemoved.Should().BeTrue();
            sut.ReferencingObjects.Should().BeEmpty();
            sut.ReferencedObjects.Should().BeEmpty();

            column.ReferencingObjects.Should()
                .BeSequentiallyEqualTo( SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( sut ), column ) );

            table.ReferencingObjects.Should()
                .BeSequentiallyEqualTo( SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( sut ), table ) );

            other.ReferencedObjects.Should().BeSequentiallyEqualTo( sut );

            actions.Should().BeEmpty();
        }
    }

    [Fact]
    public void QuickRemove_ShouldDoNothing_WhenViewIsRemoved()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var sut = schema.Objects.CreateView( "V", SqlNode.RawQuery( "SELECT * FROM bar" ) );

        schema.Database.Changes.CompletePendingChanges();
        sut.Remove();

        var actionCount = schema.Database.GetPendingActionCount();
        SqlDatabaseBuilderMock.QuickRemove( sut );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        actions.Should().BeEmpty();
    }

    [Theory]
    [InlineData( true )]
    [InlineData( false )]
    public void ToCreateNode_ShouldReturnCorrectNode(bool replaceIfExists)
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var sut = schema.Objects.CreateView( "V", SqlNode.RawQuery( "SELECT * FROM bar" ) );

        var result = sut.ToCreateNode( replaceIfExists );

        using ( new AssertionScope() )
        {
            result.Info.Should().Be( sut.Info );
            result.Source.Should().BeSameAs( sut.Source );
            result.ReplaceIfExists.Should().Be( replaceIfExists );
        }
    }

    [Fact]
    public void ISqlViewBuilder_SetName_ShouldBeEquivalentToSetName()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var sut = schema.Objects.CreateView( "V", SqlNode.RawQuery( "SELECT * FROM bar" ) );
        var recordSet = sut.Node;
        _ = sut.Info;

        var actionCount = schema.Database.GetPendingActionCount();
        var result = (( ISqlViewBuilder )sut).SetName( "bar" );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.Name.Should().Be( "bar" );
            sut.Info.Should().Be( SqlRecordSetInfo.Create( "foo", "bar" ) );
            recordSet.Info.Should().Be( sut.Info );
            schema.Objects.TryGet( "bar" ).Should().BeSameAs( sut );
            schema.Objects.TryGet( "V" ).Should().BeNull();

            actions.Should().HaveCount( 1 );
            actions.ElementAtOrDefault( 0 )
                .Sql.Should()
                .Be(
                    @"ALTER [View] foo.bar
  ALTER [View] foo.bar ([1] : 'Name' (System.String) FROM V);" );
        }
    }

    [Fact]
    public void ISqlObjectBuilder_SetName_ShouldBeEquivalentToSetName()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var sut = schema.Objects.CreateView( "V", SqlNode.RawQuery( "SELECT * FROM bar" ) );
        var recordSet = sut.Node;
        _ = sut.Info;

        var actionCount = schema.Database.GetPendingActionCount();
        var result = (( ISqlObjectBuilder )sut).SetName( "bar" );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.Name.Should().Be( "bar" );
            sut.Info.Should().Be( SqlRecordSetInfo.Create( "foo", "bar" ) );
            recordSet.Info.Should().Be( sut.Info );
            schema.Objects.TryGet( "bar" ).Should().BeSameAs( sut );
            schema.Objects.TryGet( "V" ).Should().BeNull();

            actions.Should().HaveCount( 1 );
            actions.ElementAtOrDefault( 0 )
                .Sql.Should()
                .Be(
                    @"ALTER [View] foo.bar
  ALTER [View] foo.bar ([1] : 'Name' (System.String) FROM V);" );
        }
    }
}
