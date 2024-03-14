using System.Linq;
using LfrlAnvil.Functional;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Extensions;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.TestExtensions.FluentAssertions;
using LfrlAnvil.TestExtensions.Sql.Mocks;

namespace LfrlAnvil.Sql.Tests.ObjectsTests.BuildersTests;

public partial class SqlTableBuilderTests : TestsBase
{
    [Fact]
    public void ToString_ShouldReturnCorrectResult()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var sut = schema.Objects.CreateTable( "bar" );

        var result = sut.ToString();

        result.Should().Be( "[Table] foo.bar" );
    }

    [Fact]
    public void Creation_ShouldMarkTableForCreation()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );

        var actionCount = schema.Database.GetPendingActionCount();
        var sut = schema.Objects.CreateTable( "T" );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            schema.Objects.TryGet( sut.Name ).Should().BeSameAs( sut );
            sut.Name.Should().Be( "T" );

            actions.Should().HaveCount( 1 );
            actions.ElementAtOrDefault( 0 ).Sql.Should().Be( "CREATE [Table] foo.T;" );
        }
    }

    [Fact]
    public void Creation_FollowedByRemoval_ShouldDoNothing()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );

        var actionCount = schema.Database.GetPendingActionCount();
        var sut = schema.Objects.CreateTable( "T" );
        sut.Remove();
        var actions = schema.Database.GetLastPendingActions( actionCount );

        actions.Should().BeEmpty();
    }

    [Fact]
    public void SetName_ShouldDoNothing_WhenNewNameEqualsOldName()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var sut = schema.Objects.CreateTable( "T" );

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
        var sut = schema.Objects.CreateTable( "T" );
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
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var sut = schema.Objects.CreateTable( "T" );
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
                    @"ALTER [Table] foo.bar
  ALTER [Table] foo.bar ([1] : 'Name' (System.String) FROM T);" );
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
        var sut = schema.Objects.CreateTable( "T" );

        var action = Lambda.Of( () => sut.SetName( name ) );

        action.Should()
            .ThrowExactly<SqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == SqlDialectMock.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void SetName_ShouldThrowSqlObjectBuilderException_WhenTableIsRemoved()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var sut = schema.Objects.CreateTable( "T" );
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
        var sut = schema.Objects.CreateTable( "T" );
        sut.Constraints.SetPrimaryKey( sut.Columns.Create( "C" ).Asc() );

        var action = Lambda.Of( () => sut.SetName( "PK_T" ) );

        action.Should()
            .ThrowExactly<SqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == SqlDialectMock.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void Remove_ShouldRemoveTableAndQuickRemoveColumnsAndConstraints()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var otherTable = schema.Objects.CreateTable( "U" );
        var otherPk = otherTable.Constraints.SetPrimaryKey( otherTable.Columns.Create( "D1" ).Asc() );
        var sut = schema.Objects.CreateTable( "T" );
        var c1 = sut.Columns.Create( "C1" );
        var c2 = sut.Columns.Create( "C2" );
        var pk = sut.Constraints.SetPrimaryKey( c1.Asc() );
        var ix = sut.Constraints.CreateIndex( c2.Asc() );
        var selfFk = sut.Constraints.CreateForeignKey( ix, pk.Index );
        var externalFk = sut.Constraints.CreateForeignKey( pk.Index, otherPk.Index );
        var chk = sut.Constraints.CreateCheck( c1.Node > SqlNode.Literal( 0 ) );

        var actionCount = schema.Database.GetPendingActionCount();
        sut.Remove();
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            schema.Objects.TryGet( sut.Name ).Should().BeNull();
            schema.Objects.TryGet( pk.Name ).Should().BeNull();
            schema.Objects.TryGet( pk.Index.Name ).Should().BeNull();
            schema.Objects.TryGet( ix.Name ).Should().BeNull();
            schema.Objects.TryGet( selfFk.Name ).Should().BeNull();
            schema.Objects.TryGet( externalFk.Name ).Should().BeNull();
            schema.Objects.TryGet( chk.Name ).Should().BeNull();

            sut.IsRemoved.Should().BeTrue();
            sut.ReferencingObjects.Should().BeEmpty();
            sut.Columns.Should().BeEmpty();
            sut.Constraints.Should().BeEmpty();
            sut.Constraints.TryGetPrimaryKey().Should().BeNull();
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

            otherPk.Index.ReferencingObjects.Should().BeEmpty();
            otherTable.ReferencingObjects.Should().BeEmpty();

            actions.Should().HaveCount( 1 );
            actions.ElementAtOrDefault( 0 ).Sql.Should().Be( "REMOVE [Table] foo.T;" );
        }
    }

    [Fact]
    public void Remove_ShouldDoNothing_WhenTableHasAlreadyBeenRemoved()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var sut = schema.Objects.CreateTable( "T" );

        schema.Database.Changes.CompletePendingChanges();
        sut.Remove();

        var actionCount = schema.Database.GetPendingActionCount();
        sut.Remove();
        var actions = schema.Database.GetLastPendingActions( actionCount );

        actions.Should().BeEmpty();
    }

    [Fact]
    public void Remove_ShouldThrowSqlObjectBuilderException_WhenTableIsReferencedByAnyExternalForeignKey()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var sut = schema.Objects.CreateTable( "T" );
        var pk = sut.Constraints.SetPrimaryKey( sut.Columns.Create( "C" ).Asc() );

        var otherTable = schema.Objects.CreateTable( "U" );
        var otherPk = otherTable.Constraints.SetPrimaryKey( otherTable.Columns.Create( "D" ).Asc() );
        otherTable.Constraints.CreateForeignKey( otherPk.Index, pk.Index );

        var action = Lambda.Of( () => sut.Remove() );

        action.Should()
            .ThrowExactly<SqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == SqlDialectMock.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void Remove_ShouldThrowSqlObjectBuilderException_WhenTableIsReferencedByAnyView()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var sut = schema.Objects.CreateTable( "T" );
        sut.Constraints.SetPrimaryKey( sut.Columns.Create( "C" ).Asc() );
        schema.Objects.CreateView( "V", sut.Node.ToDataSource().Select( s => new[] { s.GetAll() } ) );

        var action = Lambda.Of( () => sut.Remove() );

        action.Should()
            .ThrowExactly<SqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == SqlDialectMock.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void QuickRemove_ShouldClearReferencingObjectsAndQuickRemoveColumnsAndClearConstraints()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var otherTable = schema.Objects.CreateTable( "U" );
        var otherPk = otherTable.Constraints.SetPrimaryKey( otherTable.Columns.Create( "D1" ).Asc() );
        var sut = schema.Objects.CreateTable( "T" );
        var c1 = sut.Columns.Create( "C1" );
        var c2 = sut.Columns.Create( "C2" );
        var pk = sut.Constraints.SetPrimaryKey( c1.Asc() );
        var ix = sut.Constraints.CreateIndex( c2.Asc() );
        var selfFk = sut.Constraints.CreateForeignKey( ix, pk.Index );
        var externalFk = sut.Constraints.CreateForeignKey( pk.Index, otherPk.Index );
        var chk = sut.Constraints.CreateCheck( c1.Node > SqlNode.Literal( 0 ) );
        var view = schema.Objects.CreateView( "V", sut.Node.ToDataSource().Select( s => new[] { s.GetAll() } ) );

        var actionCount = schema.Database.GetPendingActionCount();
        SqlDatabaseBuilderMock.QuickRemove( sut );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            schema.Objects.TryGet( sut.Name ).Should().BeSameAs( sut );
            schema.Objects.TryGet( pk.Name ).Should().BeSameAs( pk );
            schema.Objects.TryGet( pk.Index.Name ).Should().BeSameAs( pk.Index );
            schema.Objects.TryGet( ix.Name ).Should().BeSameAs( ix );
            schema.Objects.TryGet( selfFk.Name ).Should().BeSameAs( selfFk );
            schema.Objects.TryGet( externalFk.Name ).Should().BeSameAs( externalFk );
            schema.Objects.TryGet( chk.Name ).Should().BeSameAs( chk );

            sut.IsRemoved.Should().BeTrue();
            sut.ReferencingObjects.Should().BeEmpty();
            sut.Columns.Should().BeEmpty();
            sut.Constraints.Should().BeEmpty();
            sut.Constraints.TryGetPrimaryKey().Should().BeNull();
            c1.IsRemoved.Should().BeTrue();
            c1.ReferencingObjects.Should().BeEmpty();
            c2.IsRemoved.Should().BeTrue();
            c2.ReferencingObjects.Should().BeEmpty();
            pk.IsRemoved.Should().BeFalse();
            pk.Index.IsRemoved.Should().BeFalse();
            ix.IsRemoved.Should().BeFalse();
            selfFk.IsRemoved.Should().BeFalse();
            externalFk.IsRemoved.Should().BeFalse();
            chk.IsRemoved.Should().BeFalse();

            otherPk.Index.ReferencingObjects.Should()
                .BeSequentiallyEqualTo(
                    SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( externalFk ), otherPk.Index ) );

            otherTable.ReferencingObjects.Should()
                .BeSequentiallyEqualTo(
                    SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( externalFk ), otherPk.Index ) );

            view.ReferencedObjects.Should().BeSequentiallyEqualTo( sut );

            actions.Should().BeEmpty();
        }
    }

    [Fact]
    public void QuickRemove_ShouldDoNothing_WhenTableIsRemoved()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var sut = schema.Objects.CreateTable( "T" );

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
    public void ToCreateNode_ShouldReturnCorrectNode(bool ifNotExists)
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var sut = schema.Objects.CreateTable( "T" );
        var c1 = sut.Columns.Create( "C1" );
        var c2 = sut.Columns.Create( "C2" );
        var pk = sut.Constraints.SetPrimaryKey( c1.Asc() );
        var ix = sut.Constraints.CreateIndex( c2.Asc() );
        var fk = sut.Constraints.CreateForeignKey( ix, pk.Index );
        var chk = sut.Constraints.CreateCheck( c1.Node > SqlNode.Literal( 0 ) );

        var result = sut.ToCreateNode( ifNotExists: ifNotExists );

        using ( new AssertionScope() )
        {
            result.Info.Should().Be( sut.Info );
            result.RecordSet.Info.Should().Be( result.Info );
            result.RecordSet.CreationNode.Should().BeSameAs( result );
            result.RecordSet.GetKnownFields().Should().HaveCount( 2 );
            result.Columns.ToArray().Should().HaveCount( 2 );
            (result.Columns.ToArray().ElementAtOrDefault( 0 )?.Name).Should().Be( c1.Name );
            (result.Columns.ToArray().ElementAtOrDefault( 1 )?.Name).Should().Be( c2.Name );
            result.ForeignKeys.ToArray().Should().HaveCount( 1 );
            (result.ForeignKeys.ToArray().ElementAtOrDefault( 0 )?.Name).Should().Be( SqlSchemaObjectName.Create( "foo", fk.Name ) );
            result.Checks.ToArray().Should().HaveCount( 1 );
            (result.Checks.ToArray().ElementAtOrDefault( 0 )?.Name).Should().Be( SqlSchemaObjectName.Create( "foo", chk.Name ) );
            (result.PrimaryKey?.Name).Should().Be( SqlSchemaObjectName.Create( "foo", pk.Name ) );
            result.IfNotExists.Should().Be( ifNotExists );
        }
    }

    [Fact]
    public void ToCreateNode_ShouldReturnCorrectNode_WithEmptyTable()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var sut = schema.Objects.CreateTable( "T" );

        var result = sut.ToCreateNode();

        using ( new AssertionScope() )
        {
            result.Info.Should().Be( sut.Info );
            result.RecordSet.Info.Should().Be( result.Info );
            result.RecordSet.CreationNode.Should().BeSameAs( result );
            result.RecordSet.GetKnownFields().Should().BeEmpty();
            result.Columns.ToArray().Should().BeEmpty();
            result.ForeignKeys.ToArray().Should().BeEmpty();
            result.Checks.ToArray().Should().BeEmpty();
            result.PrimaryKey.Should().BeNull();
            result.IfNotExists.Should().BeFalse();
        }
    }

    [Fact]
    public void ToCreateNode_ShouldReturnCorrectNode_WithCustomInfo()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var sut = schema.Objects.CreateTable( "T" );

        var result = sut.ToCreateNode( customInfo: SqlRecordSetInfo.Create( "bar", "qux" ) );

        using ( new AssertionScope() )
        {
            result.Info.Should().Be( SqlRecordSetInfo.Create( "bar", "qux" ) );
            result.RecordSet.Info.Should().Be( result.Info );
        }
    }

    [Fact]
    public void ToCreateNode_ShouldReturnCorrectNode_WithIgnoredForeignKeys()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var sut = schema.Objects.CreateTable( "T" );
        var c1 = sut.Columns.Create( "C1" );
        var c2 = sut.Columns.Create( "C2" );
        var pk = sut.Constraints.SetPrimaryKey( c1.Asc() );
        var ix = sut.Constraints.CreateIndex( c2.Asc() );
        sut.Constraints.CreateForeignKey( ix, pk.Index );
        var chk = sut.Constraints.CreateCheck( c1.Node > SqlNode.Literal( 0 ) );

        var result = sut.ToCreateNode( includeForeignKeys: false );

        using ( new AssertionScope() )
        {
            result.Info.Should().Be( sut.Info );
            result.RecordSet.Info.Should().Be( result.Info );
            result.RecordSet.CreationNode.Should().BeSameAs( result );
            result.RecordSet.GetKnownFields().Should().HaveCount( 2 );
            result.Columns.Should().HaveCount( 2 );
            (result.Columns.ElementAtOrDefault( 0 )?.Name).Should().Be( c1.Name );
            (result.Columns.ElementAtOrDefault( 1 )?.Name).Should().Be( c2.Name );
            result.ForeignKeys.Should().BeEmpty();
            result.Checks.Should().HaveCount( 1 );
            (result.Checks.ElementAtOrDefault( 0 )?.Name).Should().Be( SqlSchemaObjectName.Create( "foo", chk.Name ) );
            (result.PrimaryKey?.Name).Should().Be( SqlSchemaObjectName.Create( "foo", pk.Name ) );
        }
    }

    [Fact]
    public void ToCreateNode_ShouldReturnCorrectNode_WithSortedGeneratedColumns_WithoutAny()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var sut = schema.Objects.CreateTable( "T" );
        var c1 = sut.Columns.Create( "C1" );
        var c2 = sut.Columns.Create( "C2" );

        var result = sut.ToCreateNode( sortGeneratedColumns: true );

        using ( new AssertionScope() )
        {
            result.Info.Should().Be( sut.Info );
            result.Columns.ToArray().Should().HaveCount( 2 );
            (result.Columns.ToArray().ElementAtOrDefault( 0 )?.Name).Should().Be( c1.Name );
            (result.Columns.ToArray().ElementAtOrDefault( 1 )?.Name).Should().Be( c2.Name );
        }
    }

    [Fact]
    public void ToCreateNode_ShouldReturnCorrectNode_WithSortedGeneratedColumns_WithOne()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var sut = schema.Objects.CreateTable( "T" );
        var c1 = sut.Columns.Create( "C1" );
        var c2 = sut.Columns.Create( "C2" );
        var c3 = sut.Columns.Create( "C3" );
        c2.SetComputation( SqlColumnComputation.Virtual( c1.Node + SqlNode.Literal( 1 ) ) );

        var result = sut.ToCreateNode( sortGeneratedColumns: true );

        using ( new AssertionScope() )
        {
            result.Info.Should().Be( sut.Info );
            result.Columns.ToArray().Should().HaveCount( 3 );
            (result.Columns.ToArray().ElementAtOrDefault( 0 )?.Name).Should().Be( c1.Name );
            (result.Columns.ToArray().ElementAtOrDefault( 1 )?.Name).Should().Be( c3.Name );
            (result.Columns.ToArray().ElementAtOrDefault( 2 )?.Name).Should().Be( c2.Name );
        }
    }

    [Fact]
    public void ToCreateNode_ShouldReturnCorrectNode_WithSortedGeneratedColumns_WithMoreThanOne()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var sut = schema.Objects.CreateTable( "T" );
        var c1 = sut.Columns.Create( "C1" );
        var c2 = sut.Columns.Create( "C2" );
        var c3 = sut.Columns.Create( "C3" );
        var c4 = sut.Columns.Create( "C4" );
        c4.SetComputation( SqlColumnComputation.Virtual( c1.Node + SqlNode.Literal( 1 ) ) );
        c2.SetComputation( SqlColumnComputation.Stored( c4.Node * SqlNode.Literal( 2 ) ) );
        c3.SetComputation( SqlColumnComputation.Stored( c4.Node + c2.Node ) );

        var result = sut.ToCreateNode( sortGeneratedColumns: true );

        using ( new AssertionScope() )
        {
            result.Info.Should().Be( sut.Info );
            result.Columns.ToArray().Should().HaveCount( 4 );
            (result.Columns.ToArray().ElementAtOrDefault( 0 )?.Name).Should().Be( c1.Name );
            (result.Columns.ToArray().ElementAtOrDefault( 1 )?.Name).Should().Be( c4.Name );
            (result.Columns.ToArray().ElementAtOrDefault( 2 )?.Name).Should().Be( c2.Name );
            (result.Columns.ToArray().ElementAtOrDefault( 3 )?.Name).Should().Be( c3.Name );
        }
    }

    [Fact]
    public void ISqlTableBuilder_SetName_ShouldBeEquivalentToSetName()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var sut = schema.Objects.CreateTable( "T" );
        var recordSet = sut.Node;
        _ = sut.Info;

        var actionCount = schema.Database.GetPendingActionCount();
        var result = ((ISqlTableBuilder)sut).SetName( "bar" );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.Name.Should().Be( "bar" );
            sut.Info.Should().Be( SqlRecordSetInfo.Create( "foo", "bar" ) );
            recordSet.Info.Should().Be( sut.Info );
            schema.Objects.TryGet( "bar" ).Should().BeSameAs( sut );
            schema.Objects.TryGet( "T" ).Should().BeNull();

            actions.Should().HaveCount( 1 );
            actions.ElementAtOrDefault( 0 )
                .Sql.Should()
                .Be(
                    @"ALTER [Table] foo.bar
  ALTER [Table] foo.bar ([1] : 'Name' (System.String) FROM T);" );
        }
    }

    [Fact]
    public void ISqlObjectBuilder_SetName_ShouldBeEquivalentToSetName()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var sut = schema.Objects.CreateTable( "T" );
        var recordSet = sut.Node;
        _ = sut.Info;

        var actionCount = schema.Database.GetPendingActionCount();
        var result = ((ISqlObjectBuilder)sut).SetName( "bar" );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.Name.Should().Be( "bar" );
            sut.Info.Should().Be( SqlRecordSetInfo.Create( "foo", "bar" ) );
            recordSet.Info.Should().Be( sut.Info );
            schema.Objects.TryGet( "bar" ).Should().BeSameAs( sut );
            schema.Objects.TryGet( "T" ).Should().BeNull();

            actions.Should().HaveCount( 1 );
            actions.ElementAtOrDefault( 0 )
                .Sql.Should()
                .Be(
                    @"ALTER [Table] foo.bar
  ALTER [Table] foo.bar ([1] : 'Name' (System.String) FROM T);" );
        }
    }
}
