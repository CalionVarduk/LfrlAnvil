using System.Linq;
using LfrlAnvil.Functional;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Extensions;
using LfrlAnvil.Sql.Objects.Builders;
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

        result.TestEquals( "[Schema] foo" ).Go();
    }

    [Fact]
    public void Creation_ShouldMarkSchemaForCreation()
    {
        var db = SqlDatabaseBuilderMock.Create();

        var actionCount = db.GetPendingActionCount();
        var sut = db.Schemas.Create( "foo" );
        var actions = db.GetLastPendingActions( actionCount );

        Assertion.All(
                db.Schemas.TryGet( sut.Name ).TestRefEquals( sut ),
                sut.Name.TestEquals( "foo" ),
                actions.Select( a => a.Sql ).TestSequence( [ "CREATE [Schema] foo;" ] ) )
            .Go();
    }

    [Fact]
    public void Creation_FollowedByRemoval_ShouldDoNothing()
    {
        var db = SqlDatabaseBuilderMock.Create();

        var actionCount = db.GetPendingActionCount();
        var sut = db.Schemas.Create( "foo" );
        sut.Remove();
        var actions = db.GetLastPendingActions( actionCount );

        actions.TestEmpty().Go();
    }

    [Fact]
    public void SetName_ShouldDoNothing_WhenNewNameEqualsOldName()
    {
        var db = SqlDatabaseBuilderMock.Create();
        var sut = db.Schemas.Create( "foo" );

        var actionCount = db.GetPendingActionCount();
        var result = sut.SetName( sut.Name );
        var actions = db.GetLastPendingActions( actionCount );

        Assertion.All(
                result.TestRefEquals( sut ),
                actions.TestEmpty() )
            .Go();
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

        Assertion.All(
                result.TestRefEquals( sut ),
                actions.TestEmpty() )
            .Go();
    }

    [Fact]
    public void SetName_ShouldUpdateName_WhenNewNameIsDifferentFromOldName()
    {
        var db = SqlDatabaseBuilderMock.Create();
        var sut = db.Schemas.Create( "foo" );

        var actionCount = db.GetPendingActionCount();
        var result = sut.SetName( "bar" );
        var actions = db.GetLastPendingActions( actionCount );

        Assertion.All(
                result.TestRefEquals( sut ),
                sut.Name.TestEquals( "bar" ),
                db.Schemas.TryGet( "bar" ).TestRefEquals( sut ),
                db.Schemas.TryGet( "foo" ).TestNull(),
                actions.Select( a => a.Sql )
                    .TestSequence(
                    [
                        """
                        ALTER [Schema] bar
                          ALTER [Schema] bar ([1] : 'Name' (System.String) FROM foo);
                        """
                    ] ) )
            .Go();
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

        Assertion.All(
                result.TestRefEquals( sut ),
                sut.Name.TestEquals( "bar" ),
                db.Schemas.TryGet( "bar" ).TestRefEquals( sut ),
                db.Schemas.TryGet( "foo" ).TestNull(),
                table.Info.TestEquals( SqlRecordSetInfo.Create( "bar", "T" ) ),
                view.Info.TestEquals( SqlRecordSetInfo.Create( "bar", "V" ) ),
                tableRecordSet.Info.TestEquals( table.Info ),
                viewRecordSet.Info.TestEquals( view.Info ),
                actions.Select( a => a.Sql )
                    .TestSequence(
                    [
                        """
                        ALTER [Schema] bar
                          ALTER [Schema] bar ([1] : 'Name' (System.String) FROM foo);
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
        var db = SqlDatabaseBuilderMock.Create();
        var sut = db.Schemas.Create( "foo" );

        var action = Lambda.Of( () => sut.SetName( name ) );

        action.Test( exc => exc.TestType()
                .Exact<SqlObjectBuilderException>( e => Assertion.All(
                    e.Dialect.TestEquals( SqlDialectMock.Instance ),
                    e.Errors.Count.TestEquals( 1 ) ) ) )
            .Go();
    }

    [Fact]
    public void SetName_ShouldThrowSqlObjectBuilderException_WhenSchemaIsRemoved()
    {
        var db = SqlDatabaseBuilderMock.Create();
        var sut = db.Schemas.Create( "foo" );
        sut.Remove();

        var action = Lambda.Of( () => sut.SetName( "bar" ) );

        action.Test( exc => exc.TestType()
                .Exact<SqlObjectBuilderException>( e => Assertion.All(
                    e.Dialect.TestEquals( SqlDialectMock.Instance ),
                    e.Errors.Count.TestEquals( 1 ) ) ) )
            .Go();
    }

    [Fact]
    public void SetName_ShouldThrowSqlObjectBuilderException_WhenNewNameAlreadyExistsInSchemas()
    {
        var db = SqlDatabaseBuilderMock.Create();
        var sut = db.Schemas.Create( "foo" );
        var other = db.Schemas.Create( "bar" );

        var action = Lambda.Of( () => sut.SetName( other.Name ) );

        action.Test( exc => exc.TestType()
                .Exact<SqlObjectBuilderException>( e => Assertion.All(
                    e.Dialect.TestEquals( SqlDialectMock.Instance ),
                    e.Errors.Count.TestEquals( 1 ) ) ) )
            .Go();
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

        Assertion.All(
                db.Schemas.TryGet( sut.Name ).TestNull(),
                sut.IsRemoved.TestTrue(),
                sut.ReferencingObjects.TestEmpty(),
                sut.Objects.TestEmpty(),
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
                view.IsRemoved.TestTrue(),
                view.ReferencingObjects.TestEmpty(),
                view.ReferencedObjects.TestEmpty(),
                otherPk.Index.ReferencingObjects.TestEmpty(),
                otherTable.ReferencingObjects.TestEmpty(),
                table.ReferencingObjects.TestEmpty(),
                actions.Select( a => a.Sql ).TestSequence( [ "REMOVE [Schema] foo;" ] ) )
            .Go();
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

        actions.TestEmpty().Go();
    }

    [Fact]
    public void Remove_ShouldThrowSqlObjectBuilderException_WhenSchemaIsDefault()
    {
        var db = SqlDatabaseBuilderMock.Create();
        var sut = db.Schemas.Default;

        var action = Lambda.Of( () => sut.Remove() );

        action.Test( exc => exc.TestType()
                .Exact<SqlObjectBuilderException>( e => Assertion.All(
                    e.Dialect.TestEquals( SqlDialectMock.Instance ),
                    e.Errors.Count.TestEquals( 1 ) ) ) )
            .Go();
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

        action.Test( exc => exc.TestType()
                .Exact<SqlObjectBuilderException>( e => Assertion.All(
                    e.Dialect.TestEquals( SqlDialectMock.Instance ),
                    e.Errors.Count.TestEquals( 1 ) ) ) )
            .Go();
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

        action.Test( exc => exc.TestType()
                .Exact<SqlObjectBuilderException>( e => Assertion.All(
                    e.Dialect.TestEquals( SqlDialectMock.Instance ),
                    e.Errors.Count.TestEquals( 1 ) ) ) )
            .Go();
    }

    [Fact]
    public void QuickRemove_ShouldThrowNotSupportedException()
    {
        var db = SqlDatabaseBuilderMock.Create();
        var sut = db.Schemas.Create( "foo" );

        var action = Lambda.Of( () => SqlDatabaseBuilderMock.QuickRemove( sut ) );

        action.Test( exc => exc.TestType().Exact<NotSupportedException>() ).Go();
    }

    [Fact]
    public void ISqlSchemaBuilder_SetName_ShouldBeEquivalentToSetName()
    {
        var db = SqlDatabaseBuilderMock.Create();
        var sut = db.Schemas.Create( "foo" );

        var actionCount = db.GetPendingActionCount();
        var result = (( ISqlSchemaBuilder )sut).SetName( "bar" );
        var actions = db.GetLastPendingActions( actionCount );

        Assertion.All(
                result.TestRefEquals( sut ),
                sut.Name.TestEquals( "bar" ),
                db.Schemas.TryGet( "bar" ).TestRefEquals( sut ),
                db.Schemas.TryGet( "foo" ).TestNull(),
                actions.Select( a => a.Sql )
                    .TestSequence(
                    [
                        """
                        ALTER [Schema] bar
                          ALTER [Schema] bar ([1] : 'Name' (System.String) FROM foo);
                        """
                    ] ) )
            .Go();
    }

    [Fact]
    public void ISqlObjectBuilder_SetName_ShouldBeEquivalentToSetName()
    {
        var db = SqlDatabaseBuilderMock.Create();
        var sut = db.Schemas.Create( "foo" );

        var actionCount = db.GetPendingActionCount();
        var result = (( ISqlObjectBuilder )sut).SetName( "bar" );
        var actions = db.GetLastPendingActions( actionCount );

        Assertion.All(
                result.TestRefEquals( sut ),
                sut.Name.TestEquals( "bar" ),
                db.Schemas.TryGet( "bar" ).TestRefEquals( sut ),
                db.Schemas.TryGet( "foo" ).TestNull(),
                actions.Select( a => a.Sql )
                    .TestSequence(
                    [
                        """
                        ALTER [Schema] bar
                          ALTER [Schema] bar ([1] : 'Name' (System.String) FROM foo);
                        """
                    ] ) )
            .Go();
    }
}
