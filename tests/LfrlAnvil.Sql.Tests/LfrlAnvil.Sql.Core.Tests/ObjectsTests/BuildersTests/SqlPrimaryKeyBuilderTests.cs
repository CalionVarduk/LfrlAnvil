using System.Linq;
using LfrlAnvil.Functional;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Extensions;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.TestExtensions.Sql.Mocks;

namespace LfrlAnvil.Sql.Tests.ObjectsTests.BuildersTests;

public class SqlPrimaryKeyBuilderTests : TestsBase
{
    [Fact]
    public void ToString_ShouldReturnCorrectResult()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var sut = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).SetName( "bar" );

        var result = sut.ToString();

        result.TestEquals( "[PrimaryKey] foo.bar" ).Go();
    }

    [Fact]
    public void Change_ShouldMarkTableForAlteration()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var c1 = table.Columns.Create( "C1" );
        var c2 = table.Columns.Create( "C2" );
        table.Constraints.SetPrimaryKey( c1.Asc() ).SetName( "bar" );

        var actionCount = schema.Database.GetPendingActionCount();
        var sut = table.Constraints.SetPrimaryKey( c2.Asc() );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        Assertion.All(
                table.Constraints.TryGet( sut.Name ).TestRefEquals( sut ),
                schema.Objects.TryGet( sut.Name ).TestRefEquals( sut ),
                sut.Name.TestEquals( "PK_T" ),
                actions.Select( a => a.Sql )
                    .TestSequence(
                    [
                        """
                        ALTER [Table] foo.T
                          CREATE [Index] foo.UIX_T_C2A
                          CREATE [PrimaryKey] foo.PK_T
                          REMOVE [Index] foo.UIX_T_C1A
                          REMOVE [PrimaryKey] foo.bar;
                        """
                    ] ) )
            .Go();
    }

    [Fact]
    public void SetName_ShouldDoNothing_WhenNewNameEqualsOldName()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var sut = table.Constraints.SetPrimaryKey( table.Columns.Create( "C" ).Asc() );

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
        var table = schema.Objects.CreateTable( "T" );
        var sut = table.Constraints.SetPrimaryKey( table.Columns.Create( "C" ).Asc() );
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
        var table = schema.Objects.CreateTable( "T" );
        var sut = table.Constraints.SetPrimaryKey( table.Columns.Create( "C" ).Asc() );
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
                        """
                        ALTER [Table] foo.T
                          ALTER [PrimaryKey] foo.bar ([1] : 'Name' (System.String) FROM PK_T);
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
        var table = schema.Objects.CreateTable( "T" );
        var sut = table.Constraints.SetPrimaryKey( table.Columns.Create( "C" ).Asc() );

        var action = Lambda.Of( () => sut.SetName( name ) );

        action.Test( exc => exc.TestType()
                .Exact<SqlObjectBuilderException>( e => Assertion.All(
                    e.Dialect.TestEquals( SqlDialectMock.Instance ),
                    e.Errors.Count.TestEquals( 1 ) ) ) )
            .Go();
    }

    [Fact]
    public void SetName_ShouldThrowSqlObjectBuilderException_WhenPrimaryKeyIsRemoved()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var sut = table.Constraints.SetPrimaryKey( table.Columns.Create( "C" ).Asc() );
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
        var table = schema.Objects.CreateTable( "T" );
        var sut = table.Constraints.SetPrimaryKey( table.Columns.Create( "C" ).Asc() );

        var action = Lambda.Of( () => sut.SetName( "T" ) );

        action.Test( exc => exc.TestType()
                .Exact<SqlObjectBuilderException>( e => Assertion.All(
                    e.Dialect.TestEquals( SqlDialectMock.Instance ),
                    e.Errors.Count.TestEquals( 1 ) ) ) )
            .Go();
    }

    [Fact]
    public void SetDefaultName_ShouldDoNothing_WhenNewNameEqualsOldName()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var sut = table.Constraints.SetPrimaryKey( table.Columns.Create( "C" ).Asc() );

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.SetDefaultName();
        var actions = schema.Database.GetLastPendingActions( actionCount );

        Assertion.All(
                result.TestRefEquals( sut ),
                actions.TestEmpty() )
            .Go();
    }

    [Fact]
    public void SetDefaultName_ShouldDoNothing_WhenNameChangeIsFollowedByChangeToOriginal()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var sut = table.Constraints.SetPrimaryKey( table.Columns.Create( "C" ).Asc() );

        var actionCount = schema.Database.GetPendingActionCount();
        sut.SetName( "bar" );
        var result = sut.SetDefaultName();
        var actions = schema.Database.GetLastPendingActions( actionCount );

        Assertion.All(
                result.TestRefEquals( sut ),
                actions.TestEmpty() )
            .Go();
    }

    [Fact]
    public void SetDefaultName_ShouldUpdateName_WhenNewNameIsDifferentFromOldName()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var sut = table.Constraints.SetPrimaryKey( table.Columns.Create( "C" ).Asc() ).SetName( "bar" );

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.SetDefaultName();
        var actions = schema.Database.GetLastPendingActions( actionCount );

        Assertion.All(
                result.TestRefEquals( sut ),
                sut.Name.TestEquals( "PK_T" ),
                table.Constraints.TryGet( "PK_T" ).TestRefEquals( sut ),
                table.Constraints.TryGet( "bar" ).TestNull(),
                schema.Objects.TryGet( "PK_T" ).TestRefEquals( sut ),
                schema.Objects.TryGet( "bar" ).TestNull(),
                actions.Select( a => a.Sql )
                    .TestSequence(
                    [
                        """
                        ALTER [Table] foo.T
                          ALTER [PrimaryKey] foo.PK_T ([1] : 'Name' (System.String) FROM bar);
                        """
                    ] ) )
            .Go();
    }

    [Fact]
    public void SetDefaultName_ShouldThrowSqlObjectBuilderException_WhenNewNameAlreadyExistsInSchemaObjects()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var sut = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).SetName( "bar" );
        table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() ).SetName( "PK_T" );

        var action = Lambda.Of( () => sut.SetDefaultName() );

        action.Test( exc => exc.TestType()
                .Exact<SqlObjectBuilderException>( e => Assertion.All(
                    e.Dialect.TestEquals( SqlDialectMock.Instance ),
                    e.Errors.Count.TestEquals( 1 ) ) ) )
            .Go();
    }

    [Fact]
    public void SetDefaultName_ShouldThrowSqlObjectBuilderException_WhenPrimaryKeyIsRemoved()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var sut = table.Constraints.SetPrimaryKey( table.Columns.Create( "C" ).Asc() ).SetName( "bar" );
        sut.Remove();

        var action = Lambda.Of( () => sut.SetDefaultName() );

        action.Test( exc => exc.TestType()
                .Exact<SqlObjectBuilderException>( e => Assertion.All(
                    e.Dialect.TestEquals( SqlDialectMock.Instance ),
                    e.Errors.Count.TestEquals( 1 ) ) ) )
            .Go();
    }

    [Fact]
    public void Remove_ShouldRemovePrimaryKeyAndUnderlyingIndex()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var column = table.Columns.Create( "C1" );
        var sut = table.Constraints.SetPrimaryKey( column.Asc() );

        var actionCount = schema.Database.GetPendingActionCount();
        sut.Remove();
        var actions = schema.Database.GetLastPendingActions( actionCount );

        Assertion.All(
                table.Constraints.TryGetPrimaryKey().TestNull(),
                table.Constraints.TryGet( sut.Name ).TestNull(),
                table.Constraints.TryGet( sut.Index.Name ).TestNull(),
                schema.Objects.TryGet( sut.Name ).TestNull(),
                schema.Objects.TryGet( sut.Index.Name ).TestNull(),
                sut.IsRemoved.TestTrue(),
                sut.Index.IsRemoved.TestTrue(),
                sut.Index.PrimaryKey.TestNull(),
                sut.Index.Columns.Expressions.TestEmpty(),
                column.ReferencingObjects.TestEmpty(),
                actions.Select( a => a.Sql )
                    .TestSequence(
                    [
                        """
                        ALTER [Table] foo.T
                          REMOVE [Index] foo.UIX_T_C1A
                          REMOVE [PrimaryKey] foo.PK_T;
                        """
                    ] ) )
            .Go();
    }

    [Fact]
    public void Remove_ShouldDoNothing_WhenPrimaryKeyIsRemoved()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var sut = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );

        schema.Database.Changes.CompletePendingChanges();
        sut.Remove();

        var actionCount = schema.Database.GetPendingActionCount();
        sut.Remove();
        var actions = schema.Database.GetLastPendingActions( actionCount );

        actions.TestEmpty().Go();
    }

    [Fact]
    public void Remove_ShouldThrowSqlObjectBuilderException_WhenPrimaryKeyIsReferenced()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var sut = table.Constraints.SetPrimaryKey( table.Columns.Create( "C" ).Asc() );
        SqlDatabaseBuilderMock.AddReference( sut, SqlObjectBuilderReferenceSource.Create( table ) );

        schema.Database.Changes.CompletePendingChanges();
        var action = Lambda.Of( () => sut.Remove() );

        action.Test( exc => exc.TestType()
                .Exact<SqlObjectBuilderException>( e => Assertion.All(
                    e.Dialect.TestEquals( SqlDialectMock.Instance ),
                    e.Errors.Count.TestEquals( 1 ) ) ) )
            .Go();
    }

    [Fact]
    public void Remove_ShouldThrowSqlObjectBuilderException_WhenUnderlyingIndexIsReferencedByOriginatingForeignKey()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var t1 = schema.Objects.CreateTable( "T1" );
        var sut = t1.Constraints.SetPrimaryKey( t1.Columns.Create( "C1" ).Asc() );
        var t2 = schema.Objects.CreateTable( "T2" );
        var target = t2.Constraints.SetPrimaryKey( t2.Columns.Create( "C2" ).Asc() );
        t1.Constraints.CreateForeignKey( sut.Index, target.Index );

        var action = Lambda.Of( () => sut.Remove() );

        action.Test( exc => exc.TestType()
                .Exact<SqlObjectBuilderException>( e => Assertion.All(
                    e.Dialect.TestEquals( SqlDialectMock.Instance ),
                    e.Errors.Count.TestEquals( 1 ) ) ) )
            .Go();
    }

    [Fact]
    public void Remove_ShouldThrowSqlObjectBuilderException_WhenUnderlyingIndexIsReferencedByReferencingForeignKey()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var t1 = schema.Objects.CreateTable( "T1" );
        var sut = t1.Constraints.SetPrimaryKey( t1.Columns.Create( "C1" ).Asc() );
        var t2 = schema.Objects.CreateTable( "T2" );
        var target = t2.Constraints.SetPrimaryKey( t2.Columns.Create( "C2" ).Asc() );
        t2.Constraints.CreateForeignKey( target.Index, sut.Index );

        var action = Lambda.Of( () => sut.Remove() );

        action.Test( exc => exc.TestType()
                .Exact<SqlObjectBuilderException>( e => Assertion.All(
                    e.Dialect.TestEquals( SqlDialectMock.Instance ),
                    e.Errors.Count.TestEquals( 1 ) ) ) )
            .Go();
    }

    [Fact]
    public void QuickRemove_ShouldClearReferencingObjects()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var sut = table.Constraints.SetPrimaryKey( table.Columns.Create( "C" ).Asc() );
        SqlDatabaseBuilderMock.AddReference( sut, SqlObjectBuilderReferenceSource.Create( table ) );

        var actionCount = schema.Database.GetPendingActionCount();
        SqlDatabaseBuilderMock.QuickRemove( sut );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        Assertion.All(
                table.Constraints.TryGet( sut.Name ).TestRefEquals( sut ),
                schema.Objects.TryGet( sut.Name ).TestRefEquals( sut ),
                sut.IsRemoved.TestTrue(),
                sut.ReferencingObjects.TestEmpty(),
                sut.Index.IsRemoved.TestFalse(),
                actions.TestEmpty() )
            .Go();
    }

    [Fact]
    public void QuickRemove_ShouldDoNothing_WhenPrimaryKeyIsRemoved()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var sut = table.Constraints.SetPrimaryKey( table.Columns.Create( "C" ).Asc() );

        schema.Database.Changes.CompletePendingChanges();
        sut.Remove();

        var actionCount = schema.Database.GetPendingActionCount();
        SqlDatabaseBuilderMock.QuickRemove( sut );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        actions.TestEmpty().Go();
    }

    [Fact]
    public void ToDefinitionNode_ShouldReturnCorrectNode()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var sut = table.Constraints.SetPrimaryKey( table.Columns.Create( "C" ).Asc() );

        var result = sut.ToDefinitionNode();

        Assertion.All(
                result.Name.TestEquals( SqlSchemaObjectName.Create( "foo", "PK_T" ) ),
                result.Columns.TestSequence( sut.Index.Columns.Expressions ) )
            .Go();
    }

    [Fact]
    public void ISqlPrimaryKeyBuilder_SetName_ShouldBeEquivalentToSetName()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var sut = table.Constraints.SetPrimaryKey( table.Columns.Create( "C" ).Asc() );
        var oldName = sut.Name;

        var actionCount = schema.Database.GetPendingActionCount();
        var result = (( ISqlPrimaryKeyBuilder )sut).SetName( "bar" );
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
                        """
                        ALTER [Table] foo.T
                          ALTER [PrimaryKey] foo.bar ([1] : 'Name' (System.String) FROM PK_T);
                        """
                    ] ) )
            .Go();
    }

    [Fact]
    public void ISqlConstraintBuilder_SetName_ShouldBeEquivalentToSetName()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var sut = table.Constraints.SetPrimaryKey( table.Columns.Create( "C" ).Asc() );
        var oldName = sut.Name;

        var actionCount = schema.Database.GetPendingActionCount();
        var result = (( ISqlConstraintBuilder )sut).SetName( "bar" );
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
                        """
                        ALTER [Table] foo.T
                          ALTER [PrimaryKey] foo.bar ([1] : 'Name' (System.String) FROM PK_T);
                        """
                    ] ) )
            .Go();
    }

    [Fact]
    public void ISqlObjectBuilder_SetName_ShouldBeEquivalentToSetName()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var sut = table.Constraints.SetPrimaryKey( table.Columns.Create( "C" ).Asc() );
        var oldName = sut.Name;

        var actionCount = schema.Database.GetPendingActionCount();
        var result = (( ISqlObjectBuilder )sut).SetName( "bar" );
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
                        """
                        ALTER [Table] foo.T
                          ALTER [PrimaryKey] foo.bar ([1] : 'Name' (System.String) FROM PK_T);
                        """
                    ] ) )
            .Go();
    }

    [Fact]
    public void ISqlPrimaryKeyBuilder_SetDefaultName_ShouldBeEquivalentToSetDefaultName()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var sut = table.Constraints.SetPrimaryKey( table.Columns.Create( "C" ).Asc() ).SetName( "bar" );

        var actionCount = schema.Database.GetPendingActionCount();
        var result = (( ISqlPrimaryKeyBuilder )sut).SetDefaultName();
        var actions = schema.Database.GetLastPendingActions( actionCount );

        Assertion.All(
                result.TestRefEquals( sut ),
                sut.Name.TestEquals( "PK_T" ),
                table.Constraints.TryGet( "PK_T" ).TestRefEquals( sut ),
                table.Constraints.TryGet( "bar" ).TestNull(),
                schema.Objects.TryGet( "PK_T" ).TestRefEquals( sut ),
                schema.Objects.TryGet( "bar" ).TestNull(),
                actions.Select( a => a.Sql )
                    .TestSequence(
                    [
                        """
                        ALTER [Table] foo.T
                          ALTER [PrimaryKey] foo.PK_T ([1] : 'Name' (System.String) FROM bar);
                        """
                    ] ) )
            .Go();
    }

    [Fact]
    public void ISqlConstraintBuilder_SetDefaultName_ShouldBeEquivalentToSetDefaultName()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var sut = table.Constraints.SetPrimaryKey( table.Columns.Create( "C" ).Asc() ).SetName( "bar" );

        var actionCount = schema.Database.GetPendingActionCount();
        var result = (( ISqlConstraintBuilder )sut).SetDefaultName();
        var actions = schema.Database.GetLastPendingActions( actionCount );

        Assertion.All(
                result.TestRefEquals( sut ),
                sut.Name.TestEquals( "PK_T" ),
                table.Constraints.TryGet( "PK_T" ).TestRefEquals( sut ),
                table.Constraints.TryGet( "bar" ).TestNull(),
                schema.Objects.TryGet( "PK_T" ).TestRefEquals( sut ),
                schema.Objects.TryGet( "bar" ).TestNull(),
                actions.Select( a => a.Sql )
                    .TestSequence(
                    [
                        """
                        ALTER [Table] foo.T
                          ALTER [PrimaryKey] foo.PK_T ([1] : 'Name' (System.String) FROM bar);
                        """
                    ] ) )
            .Go();
    }
}
