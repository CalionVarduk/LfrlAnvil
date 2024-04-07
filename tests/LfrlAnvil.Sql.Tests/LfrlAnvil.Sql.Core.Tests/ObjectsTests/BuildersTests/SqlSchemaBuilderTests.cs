using System.Linq;
using LfrlAnvil.Functional;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Extensions;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.TestExtensions.FluentAssertions;
using LfrlAnvil.TestExtensions.Sql.Mocks;

namespace LfrlAnvil.Sql.Tests.ObjectsTests.BuildersTests;

public partial class SqlSchemaBuilderTests : TestsBase
{
    [Fact]
    public void ToString_ShouldReturnCorrectResult()
    {
        var db = SqlDatabaseBuilderMock.Create();
        var sut = db.Schemas.Create( "foo" );

        var result = sut.ToString();

        result.Should().Be( "[Schema] foo" );
    }

    [Fact]
    public void Creation_ShouldMarkSchemaForCreation()
    {
        var db = SqlDatabaseBuilderMock.Create();

        var actionCount = db.GetPendingActionCount();
        var sut = db.Schemas.Create( "foo" );
        var actions = db.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            db.Schemas.TryGet( sut.Name ).Should().BeSameAs( sut );
            sut.Name.Should().Be( "foo" );

            actions.Should().HaveCount( 1 );
            actions.ElementAtOrDefault( 0 ).Sql.Should().Be( "CREATE [Schema] foo;" );
        }
    }

    [Fact]
    public void Creation_FollowedByRemoval_ShouldDoNothing()
    {
        var db = SqlDatabaseBuilderMock.Create();

        var actionCount = db.GetPendingActionCount();
        var sut = db.Schemas.Create( "foo" );
        sut.Remove();
        var actions = db.GetLastPendingActions( actionCount );

        actions.Should().BeEmpty();
    }

    [Fact]
    public void SetName_ShouldDoNothing_WhenNewNameEqualsOldName()
    {
        var db = SqlDatabaseBuilderMock.Create();
        var sut = db.Schemas.Create( "foo" );

        var actionCount = db.GetPendingActionCount();
        var result = sut.SetName( sut.Name );
        var actions = db.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            actions.Should().BeEmpty();
        }
    }

    [Fact]
    public void SetName_ShouldDoNothing_WhenNameChangeIsFollowedByChangeToOriginal()
    {
        var db = SqlDatabaseBuilderMock.Create();
        var sut = db.Schemas.Create( "foo" );
        var oldName = sut.Name;

        var actionCount = db.GetPendingActionCount();
        sut.SetName( "bar" );
        var result = sut.SetName( oldName );
        var actions = db.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            actions.Should().BeEmpty();
        }
    }

    [Fact]
    public void SetName_ShouldUpdateName_WhenNewNameIsDifferentFromOldName()
    {
        var db = SqlDatabaseBuilderMock.Create();
        var sut = db.Schemas.Create( "foo" );

        var actionCount = db.GetPendingActionCount();
        var result = sut.SetName( "bar" );
        var actions = db.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.Name.Should().Be( "bar" );
            db.Schemas.TryGet( "bar" ).Should().BeSameAs( sut );
            db.Schemas.TryGet( "foo" ).Should().BeNull();

            actions.Should().HaveCount( 1 );
            actions.ElementAtOrDefault( 0 )
                .Sql.Should()
                .Be(
                    @"ALTER [Schema] bar
  ALTER [Schema] bar ([1] : 'Name' (System.String) FROM foo);" );
        }
    }

    [Fact]
    public void SetName_ShouldUpdateName_WhenNewNameIsDifferentFromOldNameAndSchemaContainsTablesAndViews()
    {
        var db = SqlDatabaseBuilderMock.Create();
        var sut = db.Schemas.Create( "foo" );

        var table = sut.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C" ).Asc() );
        var view = sut.Objects.CreateView( "V", SqlNode.RawQuery( "SELECT * FROM bar" ) );
        var tableRecordSet = table.Node;
        var viewRecordSet = view.Node;
        _ = table.Info;
        _ = view.Info;

        var actionCount = db.GetPendingActionCount();
        var result = sut.SetName( "bar" );
        var actions = db.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.Name.Should().Be( "bar" );
            db.Schemas.TryGet( "bar" ).Should().BeSameAs( sut );
            db.Schemas.TryGet( "foo" ).Should().BeNull();

            table.Info.Should().Be( SqlRecordSetInfo.Create( "bar", "T" ) );
            view.Info.Should().Be( SqlRecordSetInfo.Create( "bar", "V" ) );
            tableRecordSet.Info.Should().Be( table.Info );
            viewRecordSet.Info.Should().Be( view.Info );

            actions.Should().HaveCount( 1 );
            actions.ElementAtOrDefault( 0 )
                .Sql.Should()
                .Be(
                    @"ALTER [Schema] bar
  ALTER [Schema] bar ([1] : 'Name' (System.String) FROM foo);" );
        }
    }

    [Theory]
    [InlineData( "" )]
    [InlineData( " " )]
    [InlineData( "'" )]
    [InlineData( "f\'oo" )]
    public void SetName_ShouldThrowSqlObjectBuilderException_WhenNameIsInvalid(string name)
    {
        var db = SqlDatabaseBuilderMock.Create();
        var sut = db.Schemas.Create( "foo" );

        var action = Lambda.Of( () => sut.SetName( name ) );

        action.Should()
            .ThrowExactly<SqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == SqlDialectMock.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void SetName_ShouldThrowSqlObjectBuilderException_WhenSchemaIsRemoved()
    {
        var db = SqlDatabaseBuilderMock.Create();
        var sut = db.Schemas.Create( "foo" );
        sut.Remove();

        var action = Lambda.Of( () => sut.SetName( "bar" ) );

        action.Should()
            .ThrowExactly<SqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == SqlDialectMock.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void SetName_ShouldThrowSqlObjectBuilderException_WhenNewNameAlreadyExistsInSchemas()
    {
        var db = SqlDatabaseBuilderMock.Create();
        var sut = db.Schemas.Create( "foo" );
        var other = db.Schemas.Create( "bar" );

        var action = Lambda.Of( () => sut.SetName( other.Name ) );

        action.Should()
            .ThrowExactly<SqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == SqlDialectMock.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void Remove_ShouldRemoveSchemaAndQuickRemoveObjects()
    {
        var db = SqlDatabaseBuilderMock.Create();
        var sut = db.Schemas.Create( "foo" );
        var otherTable = sut.Objects.CreateTable( "U" );
        var otherPk = otherTable.Constraints.SetPrimaryKey( otherTable.Columns.Create( "D1" ).Asc() );
        var table = sut.Objects.CreateTable( "T" );
        var c1 = table.Columns.Create( "C1" );
        var c2 = table.Columns.Create( "C2" );
        var pk = table.Constraints.SetPrimaryKey( c1.Asc() );
        var ix = table.Constraints.CreateIndex( c2.Asc() );
        var selfFk = table.Constraints.CreateForeignKey( ix, pk.Index );
        var externalFk = table.Constraints.CreateForeignKey( pk.Index, otherPk.Index );
        var chk = table.Constraints.CreateCheck( c1.Node > SqlNode.Literal( 0 ) );
        var view = sut.Objects.CreateView( "V", table.Node.ToDataSource().Select( d => new[] { d.GetAll() } ) );

        var actionCount = db.GetPendingActionCount();
        sut.Remove();
        var actions = db.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            db.Schemas.TryGet( sut.Name ).Should().BeNull();

            sut.IsRemoved.Should().BeTrue();
            sut.ReferencingObjects.Should().BeEmpty();
            sut.Objects.Should().BeEmpty();
            c1.IsRemoved.Should().BeTrue();
            c1.ReferencingObjects.Should().BeEmpty();
            c2.IsRemoved.Should().BeTrue();
            c2.ReferencingObjects.Should().BeEmpty();
            pk.IsRemoved.Should().BeTrue();
            pk.ReferencingObjects.Should().BeEmpty();
            pk.Index.IsRemoved.Should().BeTrue();
            pk.Index.ReferencingObjects.Should().BeEmpty();
            pk.Index.Columns.Expressions.Should().BeEmpty();
            pk.Index.PrimaryKey.Should().BeNull();
            ix.IsRemoved.Should().BeTrue();
            ix.ReferencingObjects.Should().BeEmpty();
            ix.Columns.Expressions.Should().BeEmpty();
            selfFk.IsRemoved.Should().BeTrue();
            selfFk.ReferencingObjects.Should().BeEmpty();
            externalFk.IsRemoved.Should().BeTrue();
            externalFk.ReferencingObjects.Should().BeEmpty();
            chk.IsRemoved.Should().BeTrue();
            chk.ReferencingObjects.Should().BeEmpty();
            chk.ReferencedColumns.Should().BeEmpty();
            view.IsRemoved.Should().BeTrue();
            view.ReferencingObjects.Should().BeEmpty();
            view.ReferencedObjects.Should().BeEmpty();

            otherPk.Index.ReferencingObjects.Should().BeEmpty();
            otherTable.ReferencingObjects.Should().BeEmpty();
            table.ReferencingObjects.Should().BeEmpty();

            actions.Should().HaveCount( 1 );
            actions.ElementAtOrDefault( 0 ).Sql.Should().Be( "REMOVE [Schema] foo;" );
        }
    }

    [Fact]
    public void Remove_ShouldDoNothing_WhenSchemaIsRemoved()
    {
        var db = SqlDatabaseBuilderMock.Create();
        var sut = db.Schemas.Create( "foo" );

        db.Changes.CompletePendingChanges();
        sut.Remove();

        var actionCount = db.GetPendingActionCount();
        sut.Remove();
        var actions = db.GetLastPendingActions( actionCount );

        actions.Should().BeEmpty();
    }

    [Fact]
    public void Remove_ShouldThrowSqlObjectBuilderException_WhenSchemaIsDefault()
    {
        var db = SqlDatabaseBuilderMock.Create();
        var sut = db.Schemas.Default;

        var action = Lambda.Of( () => sut.Remove() );

        action.Should()
            .ThrowExactly<SqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == SqlDialectMock.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void Remove_ShouldThrowSqlObjectBuilderException_WhenSchemaIsReferencedByForeignKeyFromAnotherSchema()
    {
        var db = SqlDatabaseBuilderMock.Create();
        var sut = db.Schemas.Create( "foo" );
        var table = sut.Objects.CreateTable( "T1" );
        var pk = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );

        var otherTable = db.Schemas.Default.Objects.CreateTable( "T2" );
        var otherPk = otherTable.Constraints.SetPrimaryKey( otherTable.Columns.Create( "C2" ).Asc() );
        otherTable.Constraints.CreateForeignKey( otherPk.Index, pk.Index );

        var action = Lambda.Of( () => sut.Remove() );

        action.Should()
            .ThrowExactly<SqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == SqlDialectMock.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void Remove_ShouldThrowSqlObjectBuilderException_WhenSchemaIsReferencedByViewFromAnotherSchema()
    {
        var db = SqlDatabaseBuilderMock.Create();
        var sut = db.Schemas.Create( "foo" );
        var table = sut.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C" ).Asc() );

        db.Schemas.Default.Objects.CreateView( "V", table.Node.ToDataSource().Select( s => new[] { s.GetAll() } ) );

        var action = Lambda.Of( () => sut.Remove() );

        action.Should()
            .ThrowExactly<SqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == SqlDialectMock.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void QuickRemove_ShouldThrowNotSupportedException()
    {
        var db = SqlDatabaseBuilderMock.Create();
        var sut = db.Schemas.Create( "foo" );

        var action = Lambda.Of( () => SqlDatabaseBuilderMock.QuickRemove( sut ) );

        action.Should().ThrowExactly<NotSupportedException>();
    }

    [Fact]
    public void ISqlSchemaBuilder_SetName_ShouldBeEquivalentToSetName()
    {
        var db = SqlDatabaseBuilderMock.Create();
        var sut = db.Schemas.Create( "foo" );

        var actionCount = db.GetPendingActionCount();
        var result = (( ISqlSchemaBuilder )sut).SetName( "bar" );
        var actions = db.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.Name.Should().Be( "bar" );
            db.Schemas.TryGet( "bar" ).Should().BeSameAs( sut );
            db.Schemas.TryGet( "foo" ).Should().BeNull();

            actions.Should().HaveCount( 1 );
            actions.ElementAtOrDefault( 0 )
                .Sql.Should()
                .Be(
                    @"ALTER [Schema] bar
  ALTER [Schema] bar ([1] : 'Name' (System.String) FROM foo);" );
        }
    }

    [Fact]
    public void ISqlObjectBuilder_SetName_ShouldBeEquivalentToSetName()
    {
        var db = SqlDatabaseBuilderMock.Create();
        var sut = db.Schemas.Create( "foo" );

        var actionCount = db.GetPendingActionCount();
        var result = (( ISqlObjectBuilder )sut).SetName( "bar" );
        var actions = db.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.Name.Should().Be( "bar" );
            db.Schemas.TryGet( "bar" ).Should().BeSameAs( sut );
            db.Schemas.TryGet( "foo" ).Should().BeNull();

            actions.Should().HaveCount( 1 );
            actions.ElementAtOrDefault( 0 )
                .Sql.Should()
                .Be(
                    @"ALTER [Schema] bar
  ALTER [Schema] bar ([1] : 'Name' (System.String) FROM foo);" );
        }
    }
}
