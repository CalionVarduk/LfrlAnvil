using System.Linq;
using LfrlAnvil.Functional;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Extensions;
using LfrlAnvil.Sql.Objects.Builders;
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

        result.TestEquals( "[Table] foo.bar" ).Go();
    }

    [Fact]
    public void Creation_ShouldMarkTableForCreation()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );

        var actionCount = schema.Database.GetPendingActionCount();
        var sut = schema.Objects.CreateTable( "T" );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        Assertion.All(
                schema.Objects.TryGet( sut.Name ).TestRefEquals( sut ),
                sut.Name.TestEquals( "T" ),
                actions.Select( a => a.Sql ).TestSequence( [ "CREATE [Table] foo.T;" ] ) )
            .Go();
    }

    [Fact]
    public void Creation_FollowedByRemoval_ShouldDoNothing()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );

        var actionCount = schema.Database.GetPendingActionCount();
        var sut = schema.Objects.CreateTable( "T" );
        sut.Remove();
        var actions = schema.Database.GetLastPendingActions( actionCount );

        actions.TestEmpty().Go();
    }

    [Fact]
    public void SetName_ShouldDoNothing_WhenNewNameEqualsOldName()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var sut = schema.Objects.CreateTable( "T" );

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
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var sut = schema.Objects.CreateTable( "T" );
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
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var sut = schema.Objects.CreateTable( "T" );
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
                        """
                        ALTER [Table] foo.bar
                          ALTER [Table] foo.bar ([1] : 'Name' (System.String) FROM T);
                        """
                    ] ) )
            .Go();
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

        action.Test( exc => exc.TestType()
                .Exact<SqlObjectBuilderException>( e => Assertion.All(
                    e.Dialect.TestEquals( SqlDialectMock.Instance ),
                    e.Errors.Count.TestEquals( 1 ) ) ) )
            .Go();
    }

    [Fact]
    public void SetName_ShouldThrowSqlObjectBuilderException_WhenTableIsRemoved()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var sut = schema.Objects.CreateTable( "T" );
        sut.Remove();

        var action = Lambda.Of( () => sut.SetName( "bar" ) );

        action.Test( exc => exc.TestType()
                .Exact<SqlObjectBuilderException>( e => Assertion.All(
                    e.Dialect.TestEquals( SqlDialectMock.Instance ),
                    e.Errors.Count.TestEquals( 1 ) ) ) )
            .Go();
    }

    [Fact]
    public void SetName_ShouldThrowSqlObjectBuilderException_WhenNewNameAlreadyExistsInSchemaObjects()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var sut = schema.Objects.CreateTable( "T" );
        sut.Constraints.SetPrimaryKey( sut.Columns.Create( "C" ).Asc() );

        var action = Lambda.Of( () => sut.SetName( "PK_T" ) );

        action.Test( exc => exc.TestType()
                .Exact<SqlObjectBuilderException>( e => Assertion.All(
                    e.Dialect.TestEquals( SqlDialectMock.Instance ),
                    e.Errors.Count.TestEquals( 1 ) ) ) )
            .Go();
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

        Assertion.All(
                schema.Objects.TryGet( sut.Name ).TestNull(),
                schema.Objects.TryGet( pk.Name ).TestNull(),
                schema.Objects.TryGet( pk.Index.Name ).TestNull(),
                schema.Objects.TryGet( ix.Name ).TestNull(),
                schema.Objects.TryGet( selfFk.Name ).TestNull(),
                schema.Objects.TryGet( externalFk.Name ).TestNull(),
                schema.Objects.TryGet( chk.Name ).TestNull(),
                sut.IsRemoved.TestTrue(),
                sut.ReferencingObjects.TestEmpty(),
                sut.Columns.TestEmpty(),
                sut.Constraints.TestEmpty(),
                sut.Constraints.TryGetPrimaryKey().TestNull(),
                c1.IsRemoved.TestTrue(),
                c1.ReferencingObjects.TestEmpty(),
                c2.IsRemoved.TestTrue(),
                c2.ReferencingObjects.TestEmpty(),
                pk.IsRemoved.TestTrue(),
                pk.ReferencingObjects.TestEmpty(),
                pk.Index.IsRemoved.TestTrue(),
                pk.Index.ReferencingObjects.TestEmpty(),
                pk.Index.Columns.Expressions.TestEmpty(),
                pk.Index.PrimaryKey.TestNull(),
                ix.IsRemoved.TestTrue(),
                ix.ReferencingObjects.TestEmpty(),
                ix.Columns.Expressions.TestEmpty(),
                selfFk.IsRemoved.TestTrue(),
                selfFk.ReferencingObjects.TestEmpty(),
                externalFk.IsRemoved.TestTrue(),
                externalFk.ReferencingObjects.TestEmpty(),
                chk.IsRemoved.TestTrue(),
                chk.ReferencingObjects.TestEmpty(),
                chk.ReferencedColumns.TestEmpty(),
                otherPk.Index.ReferencingObjects.TestEmpty(),
                otherTable.ReferencingObjects.TestEmpty(),
                actions.Select( a => a.Sql ).TestSequence( [ "REMOVE [Table] foo.T;" ] ) )
            .Go();
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

        actions.TestEmpty().Go();
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

        action.Test( exc => exc.TestType()
                .Exact<SqlObjectBuilderException>( e => Assertion.All(
                    e.Dialect.TestEquals( SqlDialectMock.Instance ),
                    e.Errors.Count.TestEquals( 1 ) ) ) )
            .Go();
    }

    [Fact]
    public void Remove_ShouldThrowSqlObjectBuilderException_WhenTableIsReferencedByAnyView()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var sut = schema.Objects.CreateTable( "T" );
        sut.Constraints.SetPrimaryKey( sut.Columns.Create( "C" ).Asc() );
        schema.Objects.CreateView( "V", sut.Node.ToDataSource().Select( s => new[] { s.GetAll() } ) );

        var action = Lambda.Of( () => sut.Remove() );

        action.Test( exc => exc.TestType()
                .Exact<SqlObjectBuilderException>( e => Assertion.All(
                    e.Dialect.TestEquals( SqlDialectMock.Instance ),
                    e.Errors.Count.TestEquals( 1 ) ) ) )
            .Go();
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

        Assertion.All(
                schema.Objects.TryGet( sut.Name ).TestRefEquals( sut ),
                schema.Objects.TryGet( pk.Name ).TestRefEquals( pk ),
                schema.Objects.TryGet( pk.Index.Name ).TestRefEquals( pk.Index ),
                schema.Objects.TryGet( ix.Name ).TestRefEquals( ix ),
                schema.Objects.TryGet( selfFk.Name ).TestRefEquals( selfFk ),
                schema.Objects.TryGet( externalFk.Name ).TestRefEquals( externalFk ),
                schema.Objects.TryGet( chk.Name ).TestRefEquals( chk ),
                sut.IsRemoved.TestTrue(),
                sut.ReferencingObjects.TestEmpty(),
                sut.Columns.TestEmpty(),
                sut.Constraints.TestEmpty(),
                sut.Constraints.TryGetPrimaryKey().TestNull(),
                c1.IsRemoved.TestTrue(),
                c1.ReferencingObjects.TestEmpty(),
                c2.IsRemoved.TestTrue(),
                c2.ReferencingObjects.TestEmpty(),
                pk.IsRemoved.TestFalse(),
                pk.Index.IsRemoved.TestFalse(),
                ix.IsRemoved.TestFalse(),
                selfFk.IsRemoved.TestFalse(),
                externalFk.IsRemoved.TestFalse(),
                chk.IsRemoved.TestFalse(),
                otherPk.Index.ReferencingObjects.TestSequence(
                    [ SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( externalFk ), otherPk.Index ) ] ),
                otherTable.ReferencingObjects.TestSequence(
                    [ SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( externalFk ), otherPk.Index ) ] ),
                view.ReferencedObjects.TestSequence( [ sut ] ),
                actions.TestEmpty() )
            .Go();
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

        actions.TestEmpty().Go();
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

        Assertion.All(
                result.Info.TestEquals( sut.Info ),
                result.RecordSet.Info.TestEquals( result.Info ),
                result.RecordSet.CreationNode.TestRefEquals( result ),
                result.RecordSet.GetKnownFields().Count.TestEquals( 2 ),
                result.Columns.Count.TestEquals( 2 ),
                (result.Columns.ElementAtOrDefault( 0 )?.Name).TestEquals( c1.Name ),
                (result.Columns.ElementAtOrDefault( 1 )?.Name).TestEquals( c2.Name ),
                result.ForeignKeys.Count.TestEquals( 1 ),
                (result.ForeignKeys.ElementAtOrDefault( 0 )?.Name).TestEquals( SqlSchemaObjectName.Create( "foo", fk.Name ) ),
                result.Checks.Count.TestEquals( 1 ),
                (result.Checks.ElementAtOrDefault( 0 )?.Name).TestEquals( SqlSchemaObjectName.Create( "foo", chk.Name ) ),
                (result.PrimaryKey?.Name).TestEquals( SqlSchemaObjectName.Create( "foo", pk.Name ) ),
                result.IfNotExists.TestEquals( ifNotExists ) )
            .Go();
    }

    [Fact]
    public void ToCreateNode_ShouldReturnCorrectNode_WithEmptyTable()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var sut = schema.Objects.CreateTable( "T" );

        var result = sut.ToCreateNode();

        Assertion.All(
                result.Info.TestEquals( sut.Info ),
                result.RecordSet.Info.TestEquals( result.Info ),
                result.RecordSet.CreationNode.TestRefEquals( result ),
                result.RecordSet.GetKnownFields().TestEmpty(),
                result.Columns.TestEmpty(),
                result.ForeignKeys.TestEmpty(),
                result.Checks.TestEmpty(),
                result.PrimaryKey.TestNull(),
                result.IfNotExists.TestFalse() )
            .Go();
    }

    [Fact]
    public void ToCreateNode_ShouldReturnCorrectNode_WithCustomInfo()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var sut = schema.Objects.CreateTable( "T" );

        var result = sut.ToCreateNode( customInfo: SqlRecordSetInfo.Create( "bar", "qux" ) );

        Assertion.All(
                result.Info.TestEquals( SqlRecordSetInfo.Create( "bar", "qux" ) ),
                result.RecordSet.Info.TestEquals( result.Info ) )
            .Go();
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

        Assertion.All(
                result.Info.TestEquals( sut.Info ),
                result.RecordSet.Info.TestEquals( result.Info ),
                result.RecordSet.CreationNode.TestRefEquals( result ),
                result.RecordSet.GetKnownFields().Count.TestEquals( 2 ),
                result.Columns.Count.TestEquals( 2 ),
                (result.Columns.ElementAtOrDefault( 0 )?.Name).TestEquals( c1.Name ),
                (result.Columns.ElementAtOrDefault( 1 )?.Name).TestEquals( c2.Name ),
                result.ForeignKeys.TestEmpty(),
                result.Checks.Count.TestEquals( 1 ),
                (result.Checks.ElementAtOrDefault( 0 )?.Name).TestEquals( SqlSchemaObjectName.Create( "foo", chk.Name ) ),
                (result.PrimaryKey?.Name).TestEquals( SqlSchemaObjectName.Create( "foo", pk.Name ) ) )
            .Go();
    }

    [Fact]
    public void ToCreateNode_ShouldReturnCorrectNode_WithSortedGeneratedColumns_WithoutAny()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var sut = schema.Objects.CreateTable( "T" );
        var c1 = sut.Columns.Create( "C1" );
        var c2 = sut.Columns.Create( "C2" );

        var result = sut.ToCreateNode( sortComputedColumns: true );

        Assertion.All(
                result.Info.TestEquals( sut.Info ),
                result.Columns.Count.TestEquals( 2 ),
                (result.Columns.ElementAtOrDefault( 0 )?.Name).TestEquals( c1.Name ),
                (result.Columns.ElementAtOrDefault( 1 )?.Name).TestEquals( c2.Name ) )
            .Go();
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

        var result = sut.ToCreateNode( sortComputedColumns: true );

        Assertion.All(
                result.Info.TestEquals( sut.Info ),
                result.Columns.Count.TestEquals( 3 ),
                (result.Columns.ElementAtOrDefault( 0 )?.Name).TestEquals( c1.Name ),
                (result.Columns.ElementAtOrDefault( 1 )?.Name).TestEquals( c3.Name ),
                (result.Columns.ElementAtOrDefault( 2 )?.Name).TestEquals( c2.Name ) )
            .Go();
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

        var result = sut.ToCreateNode( sortComputedColumns: true );

        Assertion.All(
                result.Info.TestEquals( sut.Info ),
                result.Columns.Count.TestEquals( 4 ),
                (result.Columns.ElementAtOrDefault( 0 )?.Name).TestEquals( c1.Name ),
                (result.Columns.ElementAtOrDefault( 1 )?.Name).TestEquals( c4.Name ),
                (result.Columns.ElementAtOrDefault( 2 )?.Name).TestEquals( c2.Name ),
                (result.Columns.ElementAtOrDefault( 3 )?.Name).TestEquals( c3.Name ) )
            .Go();
    }

    [Fact]
    public void ISqlTableBuilder_SetName_ShouldBeEquivalentToSetName()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var sut = schema.Objects.CreateTable( "T" );
        var recordSet = sut.Node;
        _ = sut.Info;

        var actionCount = schema.Database.GetPendingActionCount();
        var result = (( ISqlTableBuilder )sut).SetName( "bar" );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        Assertion.All(
                result.TestRefEquals( sut ),
                sut.Name.TestEquals( "bar" ),
                sut.Info.TestEquals( SqlRecordSetInfo.Create( "foo", "bar" ) ),
                recordSet.Info.TestEquals( sut.Info ),
                schema.Objects.TryGet( "bar" ).TestRefEquals( sut ),
                schema.Objects.TryGet( "T" ).TestNull(),
                actions.Select( a => a.Sql )
                    .TestSequence(
                    [
                        """
                        ALTER [Table] foo.bar
                          ALTER [Table] foo.bar ([1] : 'Name' (System.String) FROM T);
                        """
                    ] ) )
            .Go();
    }

    [Fact]
    public void ISqlObjectBuilder_SetName_ShouldBeEquivalentToSetName()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var sut = schema.Objects.CreateTable( "T" );
        var recordSet = sut.Node;
        _ = sut.Info;

        var actionCount = schema.Database.GetPendingActionCount();
        var result = (( ISqlObjectBuilder )sut).SetName( "bar" );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        Assertion.All(
                result.TestRefEquals( sut ),
                sut.Name.TestEquals( "bar" ),
                sut.Info.TestEquals( SqlRecordSetInfo.Create( "foo", "bar" ) ),
                recordSet.Info.TestEquals( sut.Info ),
                schema.Objects.TryGet( "bar" ).TestRefEquals( sut ),
                schema.Objects.TryGet( "T" ).TestNull(),
                actions.Select( a => a.Sql )
                    .TestSequence(
                    [
                        """
                        ALTER [Table] foo.bar
                          ALTER [Table] foo.bar ([1] : 'Name' (System.String) FROM T);
                        """
                    ] ) )
            .Go();
    }
}
