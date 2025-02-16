using System.Linq;
using LfrlAnvil.Functional;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Extensions;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.TestExtensions.Sql.Mocks;

namespace LfrlAnvil.Sql.Tests.ObjectsTests.BuildersTests;

public class SqlForeignKeyBuilderTests : TestsBase
{
    [Fact]
    public void ToString_ShouldReturnCorrectResult()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var ix1 = table.Constraints.CreateIndex( table.Columns.Create( "C1" ).Asc() );
        var ix2 = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() ).MarkAsUnique();
        var sut = table.Constraints.CreateForeignKey( ix1, ix2 ).SetName( "bar" );

        var result = sut.ToString();

        result.TestEquals( "[ForeignKey] foo.bar" ).Go();
    }

    [Fact]
    public void Creation_ShouldMarkTableForAlteration_WhenForeignKeyReferencesTheSameTable()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var ix1 = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
        var ix2 = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );

        var actionCount = schema.Database.GetPendingActionCount();
        var sut = table.Constraints.CreateForeignKey( ix2, ix1 );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        Assertion.All(
                table.Constraints.TryGet( sut.Name ).TestRefEquals( sut ),
                schema.Objects.TryGet( sut.Name ).TestRefEquals( sut ),
                sut.Name.TestEquals( "FK_T_C2_REF_T" ),
                sut.OriginIndex.TestRefEquals( ix2 ),
                sut.ReferencedIndex.TestRefEquals( ix1 ),
                ix1.ReferencingObjects.TestSequence(
                    [ SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( sut ), ix1 ) ] ),
                ix2.ReferencingObjects.TestSequence(
                    [ SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( sut ), ix2 ) ] ),
                table.ReferencingObjects.TestEmpty(),
                schema.ReferencingObjects.TestEmpty(),
                actions.Select( a => a.Sql )
                    .TestSequence(
                    [
                        """
                        ALTER [Table] foo.T
                          CREATE [ForeignKey] foo.FK_T_C2_REF_T;
                        """
                    ] ) )
            .Go();
    }

    [Fact]
    public void Creation_ShouldMarkTableForAlteration_WhenForeignKeyReferencesDifferentTableFromTheSameSchema()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var ix1 = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
        var t2 = schema.Objects.CreateTable( "T2" );
        var ix2 = t2.Constraints.SetPrimaryKey( t2.Columns.Create( "C2" ).Asc() ).Index;

        var actionCount = schema.Database.GetPendingActionCount();
        var sut = t2.Constraints.CreateForeignKey( ix2, ix1 );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        Assertion.All(
                t2.Constraints.TryGet( sut.Name ).TestRefEquals( sut ),
                schema.Objects.TryGet( sut.Name ).TestRefEquals( sut ),
                sut.Name.TestEquals( "FK_T2_C2_REF_T" ),
                sut.OriginIndex.TestRefEquals( ix2 ),
                sut.ReferencedIndex.TestRefEquals( ix1 ),
                ix1.ReferencingObjects.TestSequence(
                    [ SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( sut ), ix1 ) ] ),
                ix2.ReferencingObjects.TestSequence(
                    [ SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( sut ), ix2 ) ] ),
                table.ReferencingObjects.TestSequence(
                    [ SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( sut ), ix1 ) ] ),
                schema.ReferencingObjects.TestEmpty(),
                actions.Select( a => a.Sql )
                    .TestSequence(
                    [
                        """
                        ALTER [Table] foo.T2
                          CREATE [ForeignKey] foo.FK_T2_C2_REF_T;
                        """
                    ] ) )
            .Go();
    }

    [Fact]
    public void Creation_ShouldMarkTableForAlteration_WhenForeignKeyReferencesDifferentSchema()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var ix1 = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
        var s2 = schema.Database.Schemas.Create( "bar" );
        var t2 = s2.Objects.CreateTable( "T2" );
        var ix2 = t2.Constraints.SetPrimaryKey( t2.Columns.Create( "C2" ).Asc() ).Index;

        var actionCount = schema.Database.GetPendingActionCount();
        var sut = t2.Constraints.CreateForeignKey( ix2, ix1 );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        Assertion.All(
                t2.Constraints.TryGet( sut.Name ).TestRefEquals( sut ),
                s2.Objects.TryGet( sut.Name ).TestRefEquals( sut ),
                sut.Name.TestEquals( "FK_T2_C2_REF_foo_T" ),
                sut.OriginIndex.TestRefEquals( ix2 ),
                sut.ReferencedIndex.TestRefEquals( ix1 ),
                ix1.ReferencingObjects.TestSequence(
                    [ SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( sut ), ix1 ) ] ),
                ix2.ReferencingObjects.TestSequence(
                    [ SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( sut ), ix2 ) ] ),
                table.ReferencingObjects.TestSequence(
                    [ SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( sut ), ix1 ) ] ),
                schema.ReferencingObjects.TestSequence(
                    [ SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( sut ), ix1 ) ] ),
                actions.Select( a => a.Sql )
                    .TestSequence(
                    [
                        """
                        ALTER [Table] bar.T2
                          CREATE [ForeignKey] bar.FK_T2_C2_REF_foo_T;
                        """
                    ] ) )
            .Go();
    }

    [Fact]
    public void Creation_FollowedByRemoval_ShouldDoNothing()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var ix1 = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );
        var ix2 = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;

        var actionCount = schema.Database.GetPendingActionCount();
        var sut = table.Constraints.CreateForeignKey( ix1, ix2 );
        sut.Remove();
        var actions = schema.Database.GetLastPendingActions( actionCount );

        actions.TestEmpty().Go();
    }

    [Fact]
    public void SetName_ShouldDoNothing_WhenNewNameEqualsOldName()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var ix1 = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );
        var ix2 = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
        var sut = table.Constraints.CreateForeignKey( ix1, ix2 );

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
        var ix1 = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );
        var ix2 = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
        var sut = table.Constraints.CreateForeignKey( ix1, ix2 );
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
        var ix1 = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );
        var ix2 = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
        var sut = table.Constraints.CreateForeignKey( ix1, ix2 );
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
                          ALTER [ForeignKey] foo.bar ([1] : 'Name' (System.String) FROM FK_T_C2_REF_T);
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
        var ix1 = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );
        var ix2 = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
        var sut = table.Constraints.CreateForeignKey( ix1, ix2 );

        var action = Lambda.Of( () => sut.SetName( name ) );

        action.Test(
                exc => exc.TestType()
                    .Exact<SqlObjectBuilderException>(
                        e => Assertion.All( e.Dialect.TestEquals( SqlDialectMock.Instance ), e.Errors.Count.TestEquals( 1 ) ) ) )
            .Go();
    }

    [Fact]
    public void SetName_ShouldThrowSqlObjectBuilderException_WhenForeignKeyIsRemoved()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var ix1 = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );
        var ix2 = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
        var sut = table.Constraints.CreateForeignKey( ix1, ix2 );
        sut.Remove();

        var action = Lambda.Of( () => sut.SetName( "bar" ) );

        action.Test(
                exc => exc.TestType()
                    .Exact<SqlObjectBuilderException>(
                        e => Assertion.All( e.Dialect.TestEquals( SqlDialectMock.Instance ), e.Errors.Count.TestEquals( 1 ) ) ) )
            .Go();
    }

    [Fact]
    public void SetName_ShouldThrowSqlObjectBuilderException_WhenNewNameAlreadyExistsInSchemaObjects()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var ix1 = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );
        var ix2 = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
        var sut = table.Constraints.CreateForeignKey( ix1, ix2 );

        var action = Lambda.Of( () => sut.SetName( "T" ) );

        action.Test(
                exc => exc.TestType()
                    .Exact<SqlObjectBuilderException>(
                        e => Assertion.All( e.Dialect.TestEquals( SqlDialectMock.Instance ), e.Errors.Count.TestEquals( 1 ) ) ) )
            .Go();
    }

    [Fact]
    public void SetDefaultName_ShouldDoNothing_WhenNewNameEqualsOldName()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var ix1 = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );
        var ix2 = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
        var sut = table.Constraints.CreateForeignKey( ix1, ix2 );

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
        var ix1 = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );
        var ix2 = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
        var sut = table.Constraints.CreateForeignKey( ix1, ix2 );

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
        var ix1 = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );
        var ix2 = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
        var sut = table.Constraints.CreateForeignKey( ix1, ix2 ).SetName( "bar" );

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.SetDefaultName();
        var actions = schema.Database.GetLastPendingActions( actionCount );

        Assertion.All(
                result.TestRefEquals( sut ),
                sut.Name.TestEquals( "FK_T_C2_REF_T" ),
                table.Constraints.TryGet( "FK_T_C2_REF_T" ).TestRefEquals( sut ),
                table.Constraints.TryGet( "bar" ).TestNull(),
                schema.Objects.TryGet( "FK_T_C2_REF_T" ).TestRefEquals( sut ),
                schema.Objects.TryGet( "bar" ).TestNull(),
                actions.Select( a => a.Sql )
                    .TestSequence(
                    [
                        """
                        ALTER [Table] foo.T
                          ALTER [ForeignKey] foo.FK_T_C2_REF_T ([1] : 'Name' (System.String) FROM bar);
                        """
                    ] ) )
            .Go();
    }

    [Fact]
    public void SetDefaultName_ShouldThrowSqlObjectBuilderException_WhenForeignKeyIsRemoved()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var ix1 = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );
        var ix2 = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
        var sut = table.Constraints.CreateForeignKey( ix1, ix2 ).SetName( "bar" );
        sut.Remove();

        var action = Lambda.Of( () => sut.SetDefaultName() );

        action.Test(
                exc => exc.TestType()
                    .Exact<SqlObjectBuilderException>(
                        e => Assertion.All( e.Dialect.TestEquals( SqlDialectMock.Instance ), e.Errors.Count.TestEquals( 1 ) ) ) )
            .Go();
    }

    [Fact]
    public void SetDefaultName_ShouldThrowSqlObjectBuilderException_WhenNewNameAlreadyExistsInSchemaObjects()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var ix1 = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );
        var ix2 = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
        var sut = table.Constraints.CreateForeignKey( ix1, ix2 ).SetName( "bar" );
        ix1.SetName( "FK_T_C2_REF_T" );

        var action = Lambda.Of( () => sut.SetDefaultName() );

        action.Test(
                exc => exc.TestType()
                    .Exact<SqlObjectBuilderException>(
                        e => Assertion.All( e.Dialect.TestEquals( SqlDialectMock.Instance ), e.Errors.Count.TestEquals( 1 ) ) ) )
            .Go();
    }

    [Theory]
    [InlineData( ReferenceBehavior.Values.Cascade )]
    [InlineData( ReferenceBehavior.Values.SetNull )]
    [InlineData( ReferenceBehavior.Values.NoAction )]
    public void SetOnDeleteBehavior_ShouldUpdateBehavior_WhenNewValueIsDifferentFromOldValue(ReferenceBehavior.Values value)
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var ix1 = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).MarkAsNullable().Asc() );
        var ix2 = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
        var sut = table.Constraints.CreateForeignKey( ix1, ix2 );
        var behavior = ReferenceBehavior.GetBehavior( value );

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.SetOnDeleteBehavior( behavior );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        Assertion.All(
                result.TestRefEquals( sut ),
                result.OnDeleteBehavior.TestEquals( behavior ),
                actions.Select( a => a.Sql )
                    .TestSequence(
                    [
                        """
                        ALTER [Table] foo.T
                          ALTER [ForeignKey] foo.FK_T_C2_REF_T ([10] : 'OnDeleteBehavior' (LfrlAnvil.Sql.ReferenceBehavior) FROM 'RESTRICT' (Restrict));
                        """
                    ] ) )
            .Go();
    }

    [Fact]
    public void SetOnDeleteBehavior_ShouldDoNothing_WhenNewValueEqualsOldValue()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var ix1 = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );
        var ix2 = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
        var sut = table.Constraints.CreateForeignKey( ix1, ix2 );

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.SetOnDeleteBehavior( ReferenceBehavior.Restrict );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        Assertion.All(
                result.TestRefEquals( sut ),
                actions.TestEmpty() )
            .Go();
    }

    [Fact]
    public void SetOnDeleteBehavior_ShouldDoNothing_WhenValueChangeIsFollowedByChangeToOriginal()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var ix1 = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );
        var ix2 = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
        var sut = table.Constraints.CreateForeignKey( ix1, ix2 );

        var actionCount = schema.Database.GetPendingActionCount();
        sut.SetOnDeleteBehavior( ReferenceBehavior.Cascade );
        var result = sut.SetOnDeleteBehavior( ReferenceBehavior.Restrict );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        Assertion.All(
                result.TestRefEquals( sut ),
                actions.TestEmpty() )
            .Go();
    }

    [Fact]
    public void SetOnDeleteBehavior_ShouldThrowSqlObjectBuilderException_WhenBehaviorIsSetNullAndNotAllOriginColumnsAreNullable()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var ix1 = table.Constraints.CreateIndex( table.Columns.Create( "C3" ).MarkAsNullable().Asc(), table.Columns.Create( "C4" ).Asc() );
        var ix2 = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc(), table.Columns.Create( "C2" ).Asc() ).Index;
        var sut = table.Constraints.CreateForeignKey( ix1, ix2 );

        var action = Lambda.Of( () => sut.SetOnDeleteBehavior( ReferenceBehavior.SetNull ) );

        action.Test(
                exc => exc.TestType()
                    .Exact<SqlObjectBuilderException>(
                        e => Assertion.All( e.Dialect.TestEquals( SqlDialectMock.Instance ), e.Errors.Count.TestEquals( 1 ) ) ) )
            .Go();
    }

    [Fact]
    public void SetOnDeleteBehavior_ShouldThrowSqlObjectBuilderException_WhenForeignKeyIsRemoved()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var ix1 = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );
        var ix2 = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
        var sut = table.Constraints.CreateForeignKey( ix1, ix2 );
        sut.Remove();

        var action = Lambda.Of( () => sut.SetOnDeleteBehavior( ReferenceBehavior.Cascade ) );

        action.Test(
                exc => exc.TestType()
                    .Exact<SqlObjectBuilderException>(
                        e => Assertion.All( e.Dialect.TestEquals( SqlDialectMock.Instance ), e.Errors.Count.TestEquals( 1 ) ) ) )
            .Go();
    }

    [Theory]
    [InlineData( ReferenceBehavior.Values.Cascade )]
    [InlineData( ReferenceBehavior.Values.SetNull )]
    [InlineData( ReferenceBehavior.Values.NoAction )]
    public void SetOnUpdateBehavior_ShouldUpdateBehavior_WhenNewValueIsDifferentFromOldValue(ReferenceBehavior.Values value)
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var ix1 = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).MarkAsNullable().Asc() );
        var ix2 = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
        var sut = table.Constraints.CreateForeignKey( ix1, ix2 );
        var behavior = ReferenceBehavior.GetBehavior( value );

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.SetOnUpdateBehavior( behavior );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        Assertion.All(
                result.TestRefEquals( sut ),
                result.OnUpdateBehavior.TestEquals( behavior ),
                actions.Select( a => a.Sql )
                    .TestSequence(
                    [
                        """
                        ALTER [Table] foo.T
                          ALTER [ForeignKey] foo.FK_T_C2_REF_T ([11] : 'OnUpdateBehavior' (LfrlAnvil.Sql.ReferenceBehavior) FROM 'RESTRICT' (Restrict));
                        """
                    ] ) )
            .Go();
    }

    [Fact]
    public void SetOnUpdateBehavior_ShouldDoNothing_WhenNewValueEqualsOldValue()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var ix1 = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );
        var ix2 = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
        var sut = table.Constraints.CreateForeignKey( ix1, ix2 );

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.SetOnUpdateBehavior( ReferenceBehavior.Restrict );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        Assertion.All(
                result.TestRefEquals( sut ),
                actions.TestEmpty() )
            .Go();
    }

    [Fact]
    public void SetOnUpdateBehavior_ShouldDoNothing_WhenValueChangeIsFollowedByChangeToOriginal()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var ix1 = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );
        var ix2 = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
        var sut = table.Constraints.CreateForeignKey( ix1, ix2 );

        var actionCount = schema.Database.GetPendingActionCount();
        sut.SetOnUpdateBehavior( ReferenceBehavior.Cascade );
        var result = sut.SetOnUpdateBehavior( ReferenceBehavior.Restrict );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        Assertion.All(
                result.TestRefEquals( sut ),
                actions.TestEmpty() )
            .Go();
    }

    [Fact]
    public void SetOnUpdateBehavior_ShouldThrowSqlObjectBuilderException_WhenBehaviorIsSetNullAndNotAllOriginColumnsAreNullable()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var ix1 = table.Constraints.CreateIndex( table.Columns.Create( "C3" ).MarkAsNullable().Asc(), table.Columns.Create( "C4" ).Asc() );
        var ix2 = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc(), table.Columns.Create( "C2" ).Asc() ).Index;
        var sut = table.Constraints.CreateForeignKey( ix1, ix2 );

        var action = Lambda.Of( () => sut.SetOnUpdateBehavior( ReferenceBehavior.SetNull ) );

        action.Test(
                exc => exc.TestType()
                    .Exact<SqlObjectBuilderException>(
                        e => Assertion.All( e.Dialect.TestEquals( SqlDialectMock.Instance ), e.Errors.Count.TestEquals( 1 ) ) ) )
            .Go();
    }

    [Fact]
    public void SetOnUpdateBehavior_ShouldThrowSqlObjectBuilderException_WhenForeignKeyIsRemoved()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var ix1 = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );
        var ix2 = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
        var sut = table.Constraints.CreateForeignKey( ix1, ix2 );
        sut.Remove();

        var action = Lambda.Of( () => sut.SetOnUpdateBehavior( ReferenceBehavior.Cascade ) );

        action.Test(
                exc => exc.TestType()
                    .Exact<SqlObjectBuilderException>(
                        e => Assertion.All( e.Dialect.TestEquals( SqlDialectMock.Instance ), e.Errors.Count.TestEquals( 1 ) ) ) )
            .Go();
    }

    [Fact]
    public void Remove_ShouldRemoveForeignKey_WhenForeignKeyReferencesTheSameTable()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var ix1 = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
        var ix2 = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );
        var sut = table.Constraints.CreateForeignKey( ix2, ix1 );

        var actionCount = schema.Database.GetPendingActionCount();
        sut.Remove();
        var actions = schema.Database.GetLastPendingActions( actionCount );

        Assertion.All(
                table.Constraints.TryGet( sut.Name ).TestNull(),
                schema.Objects.TryGet( sut.Name ).TestNull(),
                sut.IsRemoved.TestTrue(),
                ix1.ReferencingObjects.TestEmpty(),
                ix2.ReferencingObjects.TestEmpty(),
                actions.Select( a => a.Sql )
                    .TestSequence(
                    [
                        """
                        ALTER [Table] foo.T
                          REMOVE [ForeignKey] foo.FK_T_C2_REF_T;
                        """
                    ] ) )
            .Go();
    }

    [Fact]
    public void Remove_ShouldRemoveForeignKey_WhenForeignKeyReferencesDifferentTableFromTheSameSchema()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var ix1 = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
        var t2 = schema.Objects.CreateTable( "T2" );
        var ix2 = t2.Constraints.SetPrimaryKey( t2.Columns.Create( "C2" ).Asc() ).Index;
        var sut = t2.Constraints.CreateForeignKey( ix2, ix1 );

        var actionCount = schema.Database.GetPendingActionCount();
        sut.Remove();
        var actions = schema.Database.GetLastPendingActions( actionCount );

        Assertion.All(
                t2.Constraints.TryGet( sut.Name ).TestNull(),
                schema.Objects.TryGet( sut.Name ).TestNull(),
                sut.IsRemoved.TestTrue(),
                ix1.ReferencingObjects.TestEmpty(),
                ix2.ReferencingObjects.TestEmpty(),
                table.ReferencingObjects.TestEmpty(),
                actions.Select( a => a.Sql )
                    .TestSequence(
                    [
                        """
                        ALTER [Table] foo.T2
                          REMOVE [ForeignKey] foo.FK_T2_C2_REF_T;
                        """
                    ] ) )
            .Go();
    }

    [Fact]
    public void Remove_ShouldRemoveForeignKey_WhenForeignKeyReferencesDifferentSchema()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var ix1 = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
        var s2 = schema.Database.Schemas.Create( "bar" );
        var t2 = s2.Objects.CreateTable( "T2" );
        var ix2 = t2.Constraints.SetPrimaryKey( t2.Columns.Create( "C2" ).Asc() ).Index;
        var sut = t2.Constraints.CreateForeignKey( ix2, ix1 );

        var actionCount = schema.Database.GetPendingActionCount();
        sut.Remove();
        var actions = schema.Database.GetLastPendingActions( actionCount );

        Assertion.All(
                t2.Constraints.TryGet( sut.Name ).TestNull(),
                s2.Objects.TryGet( sut.Name ).TestNull(),
                sut.IsRemoved.TestTrue(),
                ix1.ReferencingObjects.TestEmpty(),
                ix2.ReferencingObjects.TestEmpty(),
                table.ReferencingObjects.TestEmpty(),
                schema.ReferencingObjects.TestEmpty(),
                actions.Select( a => a.Sql )
                    .TestSequence(
                    [
                        """
                        ALTER [Table] bar.T2
                          REMOVE [ForeignKey] bar.FK_T2_C2_REF_foo_T;
                        """
                    ] ) )
            .Go();
    }

    [Fact]
    public void Remove_ShouldDoNothing_WhenForeignKeyIsRemoved()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var ix1 = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );
        var ix2 = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
        var sut = table.Constraints.CreateForeignKey( ix1, ix2 );

        schema.Database.Changes.CompletePendingChanges();
        sut.Remove();

        var actionCount = schema.Database.GetPendingActionCount();
        sut.Remove();
        var actions = schema.Database.GetLastPendingActions( actionCount );

        actions.TestEmpty().Go();
    }

    [Fact]
    public void Remove_ShouldThrowSqlObjectBuilderException_WhenForeignKeyIsReferenced()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var ix1 = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );
        var ix2 = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
        var sut = table.Constraints.CreateForeignKey( ix1, ix2 );
        SqlDatabaseBuilderMock.AddReference( sut, SqlObjectBuilderReferenceSource.Create( table ) );

        schema.Database.Changes.CompletePendingChanges();
        var action = Lambda.Of( () => sut.Remove() );

        action.Test(
                exc => exc.TestType()
                    .Exact<SqlObjectBuilderException>(
                        e => Assertion.All( e.Dialect.TestEquals( SqlDialectMock.Instance ), e.Errors.Count.TestEquals( 1 ) ) ) )
            .Go();
    }

    [Fact]
    public void QuickRemove_ShouldClearReferencingObjects_WhenForeignKeyReferencesTheSameTable()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var ix1 = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );
        var ix2 = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
        var sut = table.Constraints.CreateForeignKey( ix1, ix2 );
        SqlDatabaseBuilderMock.AddReference( sut, SqlObjectBuilderReferenceSource.Create( table ) );

        var actionCount = schema.Database.GetPendingActionCount();
        SqlDatabaseBuilderMock.QuickRemove( sut );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        Assertion.All(
                table.Constraints.TryGet( sut.Name ).TestRefEquals( sut ),
                schema.Objects.TryGet( sut.Name ).TestRefEquals( sut ),
                sut.IsRemoved.TestTrue(),
                sut.ReferencingObjects.TestEmpty(),
                ix2.ReferencingObjects.TestSequence(
                    [ SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( sut ), ix2 ) ] ),
                ix1.ReferencingObjects.TestSequence(
                    [ SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( sut ), ix1 ) ] ),
                actions.TestEmpty() )
            .Go();
    }

    [Fact]
    public void QuickRemove_ShouldClearReferencingObjectsAndReferencedIndex()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var ix1 = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
        var s2 = schema.Database.Schemas.Create( "bar" );
        var t2 = s2.Objects.CreateTable( "T2" );
        var ix2 = t2.Constraints.SetPrimaryKey( t2.Columns.Create( "C2" ).Asc() ).Index;
        var sut = t2.Constraints.CreateForeignKey( ix2, ix1 );
        SqlDatabaseBuilderMock.AddReference( sut, SqlObjectBuilderReferenceSource.Create( table ) );

        var actionCount = schema.Database.GetPendingActionCount();
        SqlDatabaseBuilderMock.QuickRemove( sut );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        Assertion.All(
                t2.Constraints.TryGet( sut.Name ).TestRefEquals( sut ),
                s2.Objects.TryGet( sut.Name ).TestRefEquals( sut ),
                sut.IsRemoved.TestTrue(),
                sut.ReferencingObjects.TestEmpty(),
                ix2.ReferencingObjects.TestSequence(
                    [ SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( sut ), ix2 ) ] ),
                ix1.ReferencingObjects.TestEmpty(),
                table.ReferencingObjects.TestEmpty(),
                schema.ReferencingObjects.TestEmpty(),
                actions.TestEmpty() )
            .Go();
    }

    [Fact]
    public void QuickRemove_ShouldDoNothing_WhenForeignKeyIsRemoved()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var ix1 = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );
        var ix2 = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
        var sut = table.Constraints.CreateForeignKey( ix1, ix2 );

        schema.Database.Changes.CompletePendingChanges();
        sut.Remove();

        var actionCount = schema.Database.GetPendingActionCount();
        SqlDatabaseBuilderMock.QuickRemove( sut );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        actions.TestEmpty().Go();
    }

    [Fact]
    public void ToDefinitionNode_ShouldReturnCorrectNode_WhenForeignKeyIsSelfReference()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var c1 = table.Columns.Create( "C1" );
        var c2 = table.Columns.Create( "C2" );
        var ix1 = table.Constraints.CreateIndex( c2.Asc() );
        var ix2 = table.Constraints.SetPrimaryKey( c1.Asc() ).Index;
        var sut = table.Constraints.CreateForeignKey( ix1, ix2 ).SetOnUpdateBehavior( ReferenceBehavior.Cascade );

        var result = sut.ToDefinitionNode( table.Node );

        Assertion.All(
                result.Name.TestEquals( SqlSchemaObjectName.Create( "foo", "FK_T_C2_REF_T" ) ),
                result.Columns.Count.TestEquals( 1 ),
                (result.Columns.ElementAtOrDefault( 0 )?.Name).TestEquals( c2.Name ),
                (result.Columns.ElementAtOrDefault( 0 )?.RecordSet).TestRefEquals( table.Node ),
                result.ReferencedTable.TestRefEquals( table.Node ),
                result.ReferencedColumns.Count.TestEquals( 1 ),
                (result.ReferencedColumns.ElementAtOrDefault( 0 )?.Name).TestEquals( c1.Name ),
                (result.ReferencedColumns.ElementAtOrDefault( 0 )?.RecordSet).TestRefEquals( table.Node ),
                result.OnDeleteBehavior.TestRefEquals( sut.OnDeleteBehavior ),
                result.OnUpdateBehavior.TestRefEquals( sut.OnUpdateBehavior ) )
            .Go();
    }

    [Fact]
    public void ToDefinitionNode_ShouldReturnCorrectNode_WhenForeignKeyIsNotSelfReference()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var c1 = table.Columns.Create( "C1" );
        var ix1 = table.Constraints.SetPrimaryKey( c1.Asc() ).Index;
        var t2 = schema.Objects.CreateTable( "T2" );
        var c2 = t2.Columns.Create( "C2" );
        var ix2 = t2.Constraints.SetPrimaryKey( c2.Asc() ).Index;
        var sut = t2.Constraints.CreateForeignKey( ix2, ix1 ).SetOnUpdateBehavior( ReferenceBehavior.Cascade );

        var result = sut.ToDefinitionNode( t2.Node );

        Assertion.All(
                result.Name.TestEquals( SqlSchemaObjectName.Create( "foo", "FK_T2_C2_REF_T" ) ),
                result.Columns.Count.TestEquals( 1 ),
                (result.Columns.ElementAtOrDefault( 0 )?.Name).TestEquals( c2.Name ),
                (result.Columns.ElementAtOrDefault( 0 )?.RecordSet).TestRefEquals( t2.Node ),
                result.ReferencedTable.TestRefEquals( table.Node ),
                result.ReferencedColumns.Count.TestEquals( 1 ),
                (result.ReferencedColumns.ElementAtOrDefault( 0 )?.Name).TestEquals( c1.Name ),
                (result.ReferencedColumns.ElementAtOrDefault( 0 )?.RecordSet).TestRefEquals( table.Node ),
                result.OnDeleteBehavior.TestRefEquals( sut.OnDeleteBehavior ),
                result.OnUpdateBehavior.TestRefEquals( sut.OnUpdateBehavior ) )
            .Go();
    }

    [Fact]
    public void ISqlForeignKeyBuilder_SetName_ShouldBeEquivalentToSetName()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var ix1 = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );
        var ix2 = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
        var sut = table.Constraints.CreateForeignKey( ix1, ix2 );
        var oldName = sut.Name;

        var actionCount = schema.Database.GetPendingActionCount();
        var result = (( ISqlForeignKeyBuilder )sut).SetName( "bar" );
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
                          ALTER [ForeignKey] foo.bar ([1] : 'Name' (System.String) FROM FK_T_C2_REF_T);
                        """
                    ] ) )
            .Go();
    }

    [Fact]
    public void ISqlConstraintBuilder_SetName_ShouldBeEquivalentToSetName()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var ix1 = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );
        var ix2 = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
        var sut = table.Constraints.CreateForeignKey( ix1, ix2 );
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
                          ALTER [ForeignKey] foo.bar ([1] : 'Name' (System.String) FROM FK_T_C2_REF_T);
                        """
                    ] ) )
            .Go();
    }

    [Fact]
    public void ISqlObjectBuilder_SetName_ShouldBeEquivalentToSetName()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var ix1 = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );
        var ix2 = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
        var sut = table.Constraints.CreateForeignKey( ix1, ix2 );
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
                          ALTER [ForeignKey] foo.bar ([1] : 'Name' (System.String) FROM FK_T_C2_REF_T);
                        """
                    ] ) )
            .Go();
    }

    [Fact]
    public void ISqlForeignKeyBuilder_SetDefaultName_ShouldBeEquivalentToSetDefaultName()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var ix1 = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );
        var ix2 = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
        var sut = table.Constraints.CreateForeignKey( ix1, ix2 ).SetName( "bar" );

        var actionCount = schema.Database.GetPendingActionCount();
        var result = (( ISqlForeignKeyBuilder )sut).SetDefaultName();
        var actions = schema.Database.GetLastPendingActions( actionCount );

        Assertion.All(
                result.TestRefEquals( sut ),
                sut.Name.TestEquals( "FK_T_C2_REF_T" ),
                table.Constraints.TryGet( "FK_T_C2_REF_T" ).TestRefEquals( sut ),
                table.Constraints.TryGet( "bar" ).TestNull(),
                schema.Objects.TryGet( "FK_T_C2_REF_T" ).TestRefEquals( sut ),
                schema.Objects.TryGet( "bar" ).TestNull(),
                actions.Select( a => a.Sql )
                    .TestSequence(
                    [
                        """
                        ALTER [Table] foo.T
                          ALTER [ForeignKey] foo.FK_T_C2_REF_T ([1] : 'Name' (System.String) FROM bar);
                        """
                    ] ) )
            .Go();
    }

    [Fact]
    public void ISqlConstraintBuilder_SetDefaultName_ShouldBeEquivalentToSetDefaultName()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var ix1 = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );
        var ix2 = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
        var sut = table.Constraints.CreateForeignKey( ix1, ix2 ).SetName( "bar" );

        var actionCount = schema.Database.GetPendingActionCount();
        var result = (( ISqlConstraintBuilder )sut).SetDefaultName();
        var actions = schema.Database.GetLastPendingActions( actionCount );

        Assertion.All(
                result.TestRefEquals( sut ),
                sut.Name.TestEquals( "FK_T_C2_REF_T" ),
                table.Constraints.TryGet( "FK_T_C2_REF_T" ).TestRefEquals( sut ),
                table.Constraints.TryGet( "bar" ).TestNull(),
                schema.Objects.TryGet( "FK_T_C2_REF_T" ).TestRefEquals( sut ),
                schema.Objects.TryGet( "bar" ).TestNull(),
                actions.Select( a => a.Sql )
                    .TestSequence(
                    [
                        """
                        ALTER [Table] foo.T
                          ALTER [ForeignKey] foo.FK_T_C2_REF_T ([1] : 'Name' (System.String) FROM bar);
                        """
                    ] ) )
            .Go();
    }

    [Fact]
    public void ISqlForeignKeyBuilder_SetOnDeleteBehavior_ShouldBeEquivalentToSetOnDeleteBehavior()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var ix1 = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );
        var ix2 = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
        var sut = table.Constraints.CreateForeignKey( ix1, ix2 );

        var actionCount = schema.Database.GetPendingActionCount();
        var result = (( ISqlForeignKeyBuilder )sut).SetOnDeleteBehavior( ReferenceBehavior.Cascade );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        Assertion.All(
                result.TestRefEquals( sut ),
                result.OnDeleteBehavior.TestEquals( ReferenceBehavior.Cascade ),
                actions.Select( a => a.Sql )
                    .TestSequence(
                    [
                        """
                        ALTER [Table] foo.T
                          ALTER [ForeignKey] foo.FK_T_C2_REF_T ([10] : 'OnDeleteBehavior' (LfrlAnvil.Sql.ReferenceBehavior) FROM 'RESTRICT' (Restrict));
                        """
                    ] ) )
            .Go();
    }

    [Fact]
    public void ISqlForeignKeyBuilder_SetOnUpdateBehavior_ShouldBeEquivalentToSetOnUpdateBehavior()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var ix1 = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );
        var ix2 = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
        var sut = table.Constraints.CreateForeignKey( ix1, ix2 );

        var actionCount = schema.Database.GetPendingActionCount();
        var result = (( ISqlForeignKeyBuilder )sut).SetOnUpdateBehavior( ReferenceBehavior.Cascade );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        Assertion.All(
                result.TestRefEquals( sut ),
                result.OnUpdateBehavior.TestEquals( ReferenceBehavior.Cascade ),
                actions.Select( a => a.Sql )
                    .TestSequence(
                    [
                        """
                        ALTER [Table] foo.T
                          ALTER [ForeignKey] foo.FK_T_C2_REF_T ([11] : 'OnUpdateBehavior' (LfrlAnvil.Sql.ReferenceBehavior) FROM 'RESTRICT' (Restrict));
                        """
                    ] ) )
            .Go();
    }
}
