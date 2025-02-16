using System.Linq;
using LfrlAnvil.Functional;
using LfrlAnvil.MySql.Extensions;
using LfrlAnvil.MySql.Objects.Builders;
using LfrlAnvil.MySql.Tests.Helpers;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.MySql.Tests.ObjectsTests.BuildersTests;

public class MySqlForeignKeyBuilderTests : TestsBase
{
    [Fact]
    public void ToString_ShouldReturnCorrectResult()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
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
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
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
                        (sql, _) => sql.TestSatisfySql(
                            """
                            ALTER TABLE `foo`.`T`
                                ADD CONSTRAINT `FK_T_C2_REF_T` FOREIGN KEY (`C2`) REFERENCES `foo`.`T` (`C1`) ON DELETE RESTRICT ON UPDATE RESTRICT;
                            """ )
                    ] ) )
            .Go();
    }

    [Fact]
    public void Creation_ShouldMarkTableForAlteration_WhenForeignKeyReferencesDifferentTableFromTheSameSchema()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
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
                        (sql, _) => sql.TestSatisfySql(
                            """
                            ALTER TABLE `foo`.`T2`
                                ADD CONSTRAINT `FK_T2_C2_REF_T` FOREIGN KEY (`C2`) REFERENCES `foo`.`T` (`C1`) ON DELETE RESTRICT ON UPDATE RESTRICT;
                            """ )
                    ] ) )
            .Go();
    }

    [Fact]
    public void Creation_ShouldMarkTableForAlteration_WhenForeignKeyReferencesDifferentSchema()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
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
                        (sql, _) => sql.TestSatisfySql(
                            """
                            ALTER TABLE `bar`.`T2`
                                ADD CONSTRAINT `FK_T2_C2_REF_foo_T` FOREIGN KEY (`C2`) REFERENCES `foo`.`T` (`C1`) ON DELETE RESTRICT ON UPDATE RESTRICT;
                            """ )
                    ] ) )
            .Go();
    }

    [Fact]
    public void Creation_FollowedByRemoval_ShouldDoNothing()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
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
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
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
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
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
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
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
                        (sql, _) => sql.TestSatisfySql(
                            """
                            ALTER TABLE `foo`.`T`
                                DROP FOREIGN KEY `FK_T_C2_REF_T`;
                            """,
                            """
                            ALTER TABLE `foo`.`T`
                                ADD CONSTRAINT `bar` FOREIGN KEY (`C2`) REFERENCES `foo`.`T` (`C1`) ON DELETE RESTRICT ON UPDATE RESTRICT;
                            """ )
                    ] ) )
            .Go();
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
        var table = schema.Objects.CreateTable( "T" );
        var ix1 = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );
        var ix2 = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
        var sut = table.Constraints.CreateForeignKey( ix1, ix2 );

        var action = Lambda.Of( () => sut.SetName( name ) );

        action.Test(
                exc => exc.TestType()
                    .Exact<SqlObjectBuilderException>(
                        e => Assertion.All( e.Dialect.TestEquals( MySqlDialect.Instance ), e.Errors.Count.TestEquals( 1 ) ) ) )
            .Go();
    }

    [Fact]
    public void SetName_ShouldThrowSqlObjectBuilderException_WhenForeignKeyIsRemoved()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var ix1 = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );
        var ix2 = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
        var sut = table.Constraints.CreateForeignKey( ix1, ix2 );
        sut.Remove();

        var action = Lambda.Of( () => sut.SetName( "bar" ) );

        action.Test(
                exc => exc.TestType()
                    .Exact<SqlObjectBuilderException>(
                        e => Assertion.All( e.Dialect.TestEquals( MySqlDialect.Instance ), e.Errors.Count.TestEquals( 1 ) ) ) )
            .Go();
    }

    [Fact]
    public void SetName_ShouldThrowSqlObjectBuilderException_WhenNewNameAlreadyExistsInSchemaObjects()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var ix1 = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );
        var ix2 = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
        var sut = table.Constraints.CreateForeignKey( ix1, ix2 );

        var action = Lambda.Of( () => sut.SetName( "T" ) );

        action.Test(
                exc => exc.TestType()
                    .Exact<SqlObjectBuilderException>(
                        e => Assertion.All( e.Dialect.TestEquals( MySqlDialect.Instance ), e.Errors.Count.TestEquals( 1 ) ) ) )
            .Go();
    }

    [Fact]
    public void SetDefaultName_ShouldDoNothing_WhenNewNameEqualsOldName()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
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
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
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
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
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
                        (sql, _) => sql.TestSatisfySql(
                            """
                            ALTER TABLE `foo`.`T`
                                DROP FOREIGN KEY `bar`;
                            """,
                            """
                            ALTER TABLE `foo`.`T`
                                ADD CONSTRAINT `FK_T_C2_REF_T` FOREIGN KEY (`C2`) REFERENCES `foo`.`T` (`C1`) ON DELETE RESTRICT ON UPDATE RESTRICT;
                            """ )
                    ] ) )
            .Go();
    }

    [Fact]
    public void SetDefaultName_ShouldThrowSqlObjectBuilderException_WhenForeignKeyIsRemoved()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var ix1 = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );
        var ix2 = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
        var sut = table.Constraints.CreateForeignKey( ix1, ix2 ).SetName( "bar" );
        sut.Remove();

        var action = Lambda.Of( () => sut.SetDefaultName() );

        action.Test(
                exc => exc.TestType()
                    .Exact<SqlObjectBuilderException>(
                        e => Assertion.All( e.Dialect.TestEquals( MySqlDialect.Instance ), e.Errors.Count.TestEquals( 1 ) ) ) )
            .Go();
    }

    [Fact]
    public void SetDefaultName_ShouldThrowSqlObjectBuilderException_WhenNewNameAlreadyExistsInSchema()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var ix1 = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );
        var ix2 = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
        var sut = table.Constraints.CreateForeignKey( ix1, ix2 ).SetName( "bar" );
        ix1.SetName( "FK_T_C2_REF_T" );

        var action = Lambda.Of( () => sut.SetDefaultName() );

        action.Test(
                exc => exc.TestType()
                    .Exact<SqlObjectBuilderException>(
                        e => Assertion.All( e.Dialect.TestEquals( MySqlDialect.Instance ), e.Errors.Count.TestEquals( 1 ) ) ) )
            .Go();
    }

    [Theory]
    [InlineData( ReferenceBehavior.Values.Cascade )]
    [InlineData( ReferenceBehavior.Values.SetNull )]
    [InlineData( ReferenceBehavior.Values.NoAction )]
    public void SetOnDeleteBehavior_ShouldUpdateBehavior_WhenNewValueIsDifferentFromOldValue(ReferenceBehavior.Values value)
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
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
                        (sql, _) => sql.TestSatisfySql(
                            """
                            ALTER TABLE `foo`.`T`
                                DROP FOREIGN KEY `FK_T_C2_REF_T`;
                            """,
                            $"""
                             ALTER TABLE `foo`.`T`
                                 ADD CONSTRAINT `FK_T_C2_REF_T` FOREIGN KEY (`C2`) REFERENCES `foo`.`T` (`C1`) ON DELETE {behavior.Name} ON UPDATE RESTRICT;
                             """ )
                    ] ) )
            .Go();
    }

    [Fact]
    public void SetOnDeleteBehavior_ShouldDoNothing_WhenNewValueEqualsOldValue()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
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
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
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
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var ix1 = table.Constraints.CreateIndex( table.Columns.Create( "C3" ).MarkAsNullable().Asc(), table.Columns.Create( "C4" ).Asc() );
        var ix2 = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc(), table.Columns.Create( "C2" ).Asc() ).Index;
        var sut = table.Constraints.CreateForeignKey( ix1, ix2 );

        var action = Lambda.Of( () => sut.SetOnDeleteBehavior( ReferenceBehavior.SetNull ) );

        action.Test(
                exc => exc.TestType()
                    .Exact<SqlObjectBuilderException>(
                        e => Assertion.All( e.Dialect.TestEquals( MySqlDialect.Instance ), e.Errors.Count.TestEquals( 1 ) ) ) )
            .Go();
    }

    [Fact]
    public void SetOnDeleteBehavior_ShouldThrowSqlObjectBuilderException_WhenForeignKeyIsRemoved()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var ix1 = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );
        var ix2 = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
        var sut = table.Constraints.CreateForeignKey( ix1, ix2 );
        sut.Remove();

        var action = Lambda.Of( () => sut.SetOnDeleteBehavior( ReferenceBehavior.Cascade ) );

        action.Test(
                exc => exc.TestType()
                    .Exact<SqlObjectBuilderException>(
                        e => Assertion.All( e.Dialect.TestEquals( MySqlDialect.Instance ), e.Errors.Count.TestEquals( 1 ) ) ) )
            .Go();
    }

    [Theory]
    [InlineData( ReferenceBehavior.Values.Cascade )]
    [InlineData( ReferenceBehavior.Values.SetNull )]
    [InlineData( ReferenceBehavior.Values.NoAction )]
    public void SetOnUpdateBehavior_ShouldUpdateBehavior_WhenNewValueIsDifferentFromOldValue(ReferenceBehavior.Values value)
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
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
                        (sql, _) => sql.TestSatisfySql(
                            """
                            ALTER TABLE `foo`.`T`
                                DROP FOREIGN KEY `FK_T_C2_REF_T`;
                            """,
                            $"""
                             ALTER TABLE `foo`.`T`
                                 ADD CONSTRAINT `FK_T_C2_REF_T` FOREIGN KEY (`C2`) REFERENCES `foo`.`T` (`C1`) ON DELETE RESTRICT ON UPDATE {behavior.Name};
                             """ )
                    ] ) )
            .Go();
    }

    [Fact]
    public void SetOnUpdateBehavior_ShouldDoNothing_WhenNewValueEqualsOldValue()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
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
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
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
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var ix1 = table.Constraints.CreateIndex( table.Columns.Create( "C3" ).MarkAsNullable().Asc(), table.Columns.Create( "C4" ).Asc() );
        var ix2 = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc(), table.Columns.Create( "C2" ).Asc() ).Index;
        var sut = table.Constraints.CreateForeignKey( ix1, ix2 );

        var action = Lambda.Of( () => sut.SetOnUpdateBehavior( ReferenceBehavior.SetNull ) );

        action.Test(
                exc => exc.TestType()
                    .Exact<SqlObjectBuilderException>(
                        e => Assertion.All( e.Dialect.TestEquals( MySqlDialect.Instance ), e.Errors.Count.TestEquals( 1 ) ) ) )
            .Go();
    }

    [Fact]
    public void SetOnUpdateBehavior_ShouldThrowSqlObjectBuilderException_WhenForeignKeyIsRemoved()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var ix1 = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );
        var ix2 = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
        var sut = table.Constraints.CreateForeignKey( ix1, ix2 );
        sut.Remove();

        var action = Lambda.Of( () => sut.SetOnUpdateBehavior( ReferenceBehavior.Cascade ) );

        action.Test(
                exc => exc.TestType()
                    .Exact<SqlObjectBuilderException>(
                        e => Assertion.All( e.Dialect.TestEquals( MySqlDialect.Instance ), e.Errors.Count.TestEquals( 1 ) ) ) )
            .Go();
    }

    [Fact]
    public void Remove_ShouldRemoveForeignKey_WhenForeignKeyReferencesTheSameTable()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var ix1 = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );
        var ix2 = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
        var sut = table.Constraints.CreateForeignKey( ix1, ix2 );

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
                        (sql, _) => sql.TestSatisfySql(
                            """
                            ALTER TABLE `foo`.`T`
                                DROP FOREIGN KEY `FK_T_C2_REF_T`;
                            """ )
                    ] ) )
            .Go();
    }

    [Fact]
    public void Remove_ShouldRemoveForeignKey_WhenForeignKeyReferencesDifferentTableFromTheSameSchema()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
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
                        (sql, _) => sql.TestSatisfySql(
                            """
                            ALTER TABLE `foo`.`T2`
                                DROP FOREIGN KEY `FK_T2_C2_REF_T`;
                            """ )
                    ] ) )
            .Go();
    }

    [Fact]
    public void Remove_ShouldRemoveForeignKey_WhenForeignKeyReferencesDifferentSchema()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
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
                        (sql, _) => sql.TestSatisfySql(
                            """
                            ALTER TABLE `bar`.`T2`
                                DROP FOREIGN KEY `FK_T2_C2_REF_foo_T`;
                            """ )
                    ] ) )
            .Go();
    }

    [Fact]
    public void Remove_ShouldDoNothing_WhenForeignKeyIsRemoved()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
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
    public void ForMySql_ShouldInvokeAction_WhenForeignKeyIsMySql()
    {
        var action = Substitute.For<Action<MySqlForeignKeyBuilder>>();
        var table = MySqlDatabaseBuilderMock.Create().Schemas.Default.Objects.CreateTable( "T" );
        var ix1 = table.Constraints.CreateIndex( table.Columns.Create( "C1" ).Asc() );
        var ix2 = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() ).MarkAsUnique();
        var sut = table.Constraints.CreateForeignKey( ix1, ix2 );

        var result = sut.ForMySql( action );

        Assertion.All(
                result.TestRefEquals( sut ),
                action.CallAt( 0 ).Arguments.TestSequence( [ sut ] ) )
            .Go();
    }

    [Fact]
    public void ForMySql_ShouldNotInvokeAction_WhenForeignKeyIsNotMySql()
    {
        var action = Substitute.For<Action<MySqlForeignKeyBuilder>>();
        var sut = Substitute.For<ISqlForeignKeyBuilder>();

        var result = sut.ForMySql( action );

        Assertion.All(
                result.TestRefEquals( sut ),
                action.CallCount().TestEquals( 0 ) )
            .Go();
    }
}
