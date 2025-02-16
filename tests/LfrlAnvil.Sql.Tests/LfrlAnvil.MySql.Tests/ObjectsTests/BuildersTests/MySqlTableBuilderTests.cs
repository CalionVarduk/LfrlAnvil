using System.Linq;
using LfrlAnvil.Functional;
using LfrlAnvil.MySql.Extensions;
using LfrlAnvil.MySql.Objects.Builders;
using LfrlAnvil.MySql.Tests.Helpers;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.TestExtensions.Sql;
using LfrlAnvil.TestExtensions.Sql.Assertions;

namespace LfrlAnvil.MySql.Tests.ObjectsTests.BuildersTests;

public partial class MySqlTableBuilderTests : TestsBase
{
    [Fact]
    public void ToString_ShouldReturnCorrectResult()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var sut = schema.Objects.CreateTable( "bar" );

        var result = sut.ToString();

        result.TestEquals( "[Table] foo.bar" ).Go();
    }

    [Fact]
    public void Creation_ShouldPrepareCorrectStatement()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var sut = schema.Objects.CreateTable( "T" );
        var c5 = sut.Columns.Create( "C5" );
        var ix1 = sut.Constraints.CreateIndex( sut.Columns.Create( "C1" ).SetType<int>().Asc() );
        var ix2 = sut.Constraints.SetPrimaryKey( sut.Columns.Create( "C2" ).SetType<int>().Asc() ).Index;
        var c6 = sut.Columns.Create( "C6" ).MarkAsNullable();
        sut.Constraints.CreateIndex( sut.Columns.Create( "C3" ).SetType<long>().Asc(), sut.Columns.Create( "C4" ).SetType<long>().Desc() );
        sut.Constraints.CreateForeignKey( ix1, ix2 );
        sut.Constraints.CreateCheck( sut.Node["C1"] > SqlNode.Literal( 0 ) );
        c5.SetComputation( SqlColumnComputation.Virtual( sut.Columns.Get( "C1" ).Node + SqlNode.Literal( 1 ) ) );
        c6.SetComputation( SqlColumnComputation.Stored( sut.Columns.Get( "C2" ).Node * sut.Columns.Get( "C5" ).Node ) );

        var actions = schema.Database.GetLastPendingActions( 1 );

        Assertion.All(
                schema.Objects.TryGet( sut.Name ).TestRefEquals( sut ),
                sut.Name.TestEquals( "T" ),
                actions.Select( a => a.Sql )
                    .TestSequence(
                    [
                        (sql, _) => sql.SatisfySql(
                            """
                            CREATE TABLE `foo`.`T` (
                              `C1` INT NOT NULL,
                              `C2` INT NOT NULL,
                              `C3` BIGINT NOT NULL,
                              `C4` BIGINT NOT NULL,
                              `C5` LONGBLOB GENERATED ALWAYS AS (`C1` + 1) VIRTUAL NOT NULL,
                              `C6` LONGBLOB GENERATED ALWAYS AS (`C2` * `C5`) STORED,
                              CONSTRAINT `PK_T` PRIMARY KEY (`C2` ASC),
                              CONSTRAINT `CHK_T_{GUID}` CHECK (`C1` > 0)
                            );
                            """,
                            "CREATE INDEX `IX_T_C1A` ON `foo`.`T` (`C1` ASC);",
                            "CREATE INDEX `IX_T_C3A_C4D` ON `foo`.`T` (`C3` ASC, `C4` DESC);",
                            """
                            ALTER TABLE `foo`.`T`
                                ADD CONSTRAINT `FK_T_C1_REF_T` FOREIGN KEY (`C1`) REFERENCES `foo`.`T` (`C2`) ON DELETE RESTRICT ON UPDATE RESTRICT;
                            """ )
                    ] ) )
            .Go();
    }

    [Fact]
    public void Creation_FollowedByRemoval_ShouldDoNothing()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );

        var actionCount = schema.Database.GetPendingActionCount();
        var sut = schema.Objects.CreateTable( "T" );
        sut.Remove();
        var actions = schema.Database.GetLastPendingActions( actionCount );

        actions.TestEmpty().Go();
    }

    [Fact]
    public void Creation_ShouldThrowSqlObjectBuilderException_WhenTableDoesNotHavePrimaryKeyDuringScriptResolution()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var sut = schema.Objects.CreateTable( "T" );
        sut.Columns.Create( "C" );

        var action = Lambda.Of( () => schema.Database.Changes.CompletePendingChanges() );

        action.Test(
                exc => exc.TestType()
                    .Exact<SqlObjectBuilderException>(
                        e => Assertion.All( e.Dialect.TestEquals( MySqlDialect.Instance ), e.Errors.Count.TestEquals( 1 ) ) ) )
            .Go();
    }

    [Fact]
    public void SetName_ShouldDoNothing_WhenNewNameEqualsOldName()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var sut = schema.Objects.CreateTable( "T" );
        sut.Constraints.SetPrimaryKey( sut.Columns.Create( "C" ).Asc() );

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
        var sut = schema.Objects.CreateTable( "T" );
        sut.Constraints.SetPrimaryKey( sut.Columns.Create( "C" ).Asc() );
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
        var sut = schema.Objects.CreateTable( "T" );
        sut.Constraints.SetPrimaryKey( sut.Columns.Create( "C" ).Asc() );
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
                    .TestSequence( [ (sql, _) => sql.SatisfySql( "ALTER TABLE `foo`.`T` RENAME TO `foo`.`bar`;" ) ] ) )
            .Go();
    }

    [Fact]
    public void SetName_ShouldUpdateName_WhenNameChangesAndTableHasSelfReference()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );

        var other = schema.Database.Schemas.Default.Objects.CreateTable( "U" );
        var otherPk = other.Constraints.SetPrimaryKey( other.Columns.Create( "C" ).Asc() );

        var sut = schema.Objects.CreateTable( "T" );
        var c1 = sut.Columns.Create( "C1" );
        var c2 = sut.Columns.Create( "C2" );
        var pk = sut.Constraints.SetPrimaryKey( c1.Asc() );
        var fk1 = sut.Constraints.CreateForeignKey( pk.Index, otherPk.Index );
        var fk2 = sut.Constraints.CreateForeignKey( sut.Constraints.CreateIndex( c2.Asc() ), pk.Index );
        sut.Constraints.CreateCheck( c1.Node > SqlNode.Literal( 0 ) );

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.SetName( "bar" );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        Assertion.All(
                result.TestRefEquals( sut ),
                sut.Name.TestEquals( "bar" ),
                schema.Objects.TryGet( "bar" ).TestRefEquals( sut ),
                schema.Objects.TryGet( "T" ).TestNull(),
                fk1.IsRemoved.TestFalse(),
                fk2.IsRemoved.TestFalse(),
                actions.Select( a => a.Sql )
                    .TestSequence( [ (sql, _) => sql.SatisfySql( "ALTER TABLE `foo`.`T` RENAME TO `foo`.`bar`;" ) ] ) )
            .Go();
    }

    [Fact]
    public void SetName_ShouldUpdateName_WhenNameChangesAndTableHasExternalForeignKeyReferences()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var sut = schema.Objects.CreateTable( "T1" );
        var c1 = sut.Columns.Create( "C1" );
        var pk1 = sut.Constraints.SetPrimaryKey( c1.Asc() );

        var t2 = schema.Objects.CreateTable( "T2" );
        var c2 = t2.Columns.Create( "C2" );
        var pk2 = t2.Constraints.SetPrimaryKey( c2.Asc() );

        var t3 = schema.Objects.CreateTable( "T3" );
        var c3 = t3.Columns.Create( "C3" );
        var c4 = t3.Columns.Create( "C4" );
        var pk3 = t3.Constraints.SetPrimaryKey( c3.Asc() );

        var fk1 = t3.Constraints.CreateForeignKey( pk3.Index, pk1.Index );
        var fk2 = t2.Constraints.CreateForeignKey( pk2.Index, pk1.Index );
        var fk3 = t3.Constraints.CreateForeignKey( t3.Constraints.CreateIndex( c4.Asc() ), pk1.Index );

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.SetName( "U" );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        Assertion.All(
                result.TestRefEquals( sut ),
                sut.Name.TestEquals( "U" ),
                schema.Objects.TryGet( "U" ).TestRefEquals( sut ),
                schema.Objects.TryGet( "T" ).TestNull(),
                fk1.IsRemoved.TestFalse(),
                fk2.IsRemoved.TestFalse(),
                fk3.IsRemoved.TestFalse(),
                actions.Select( a => a.Sql )
                    .TestSequence( [ (sql, _) => sql.SatisfySql( "ALTER TABLE `foo`.`T1` RENAME TO `foo`.`U`;" ) ] ) )
            .Go();
    }

    [Fact]
    public void SetName_ShouldUpdateName_WhenNameChangesAndTableHasExternalForeignKeyReferencesAndChangedTableHasOtherPendingChanges()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var sut = schema.Objects.CreateTable( "T1" );
        var c1 = sut.Columns.Create( "C1" );
        var pk1 = sut.Constraints.SetPrimaryKey( c1.Asc() );

        var t2 = schema.Objects.CreateTable( "T2" );
        var c2 = t2.Columns.Create( "C2" );
        var pk2 = t2.Constraints.SetPrimaryKey( c2.Asc() );
        var fk1 = t2.Constraints.CreateForeignKey( pk2.Index, pk1.Index );

        var actionCount = schema.Database.GetPendingActionCount();
        sut.Columns.Create( "C3" ).SetType<int>();
        var result = sut.SetName( "U" );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        Assertion.All(
                result.TestRefEquals( sut ),
                sut.Name.TestEquals( "U" ),
                schema.Objects.TryGet( "U" ).TestRefEquals( sut ),
                schema.Objects.TryGet( "T1" ).TestNull(),
                fk1.IsRemoved.TestFalse(),
                actions.Select( a => a.Sql )
                    .TestSequence(
                    [
                        (sql, _) => sql.SatisfySql( "ALTER TABLE `foo`.`T1` RENAME TO `foo`.`U`;" ),
                        (sql, _) => sql.SatisfySql(
                            """
                            ALTER TABLE `foo`.`U`
                                ADD COLUMN `C3` INT NOT NULL DEFAULT 0;
                            """ )
                    ] ) )
            .Go();
    }

    [Fact]
    public void SetName_ShouldUpdateName_WhenNameChangesAndTableHasViewReferences()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var sut = schema.Objects.CreateTable( "T" );
        sut.Constraints.SetPrimaryKey( sut.Columns.Create( "C" ).Asc() );

        var v1 = schema.Objects.CreateView( "V1", sut.Node.ToDataSource().Select( s => new[] { s.From["C"].AsSelf() } ) );
        var v2 = schema.Objects.CreateView( "V2", v1.Node.Join( sut.Node.InnerOn( SqlNode.True() ) ).Select( s => new[] { s.GetAll() } ) );
        var v3 = schema.Objects.CreateView( "V3", v1.ToRecordSet().ToDataSource().Select( s => new[] { s.GetAll() } ) );

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.SetName( "U" );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        Assertion.All(
                result.TestRefEquals( sut ),
                sut.Name.TestEquals( "U" ),
                schema.Objects.TryGet( "U" ).TestRefEquals( sut ),
                schema.Objects.TryGet( "T" ).TestNull(),
                v1.IsRemoved.TestFalse(),
                v2.IsRemoved.TestFalse(),
                v3.IsRemoved.TestFalse(),
                actions.Select( a => a.Sql )
                    .TestSequence(
                    [
                        (sql, _) => sql.SatisfySql( "ALTER TABLE `foo`.`T` RENAME TO `foo`.`U`;" ),
                        (sql, _) => sql.SatisfySql(
                            "DROP VIEW `foo`.`V1`;",
                            """
                            CREATE VIEW `foo`.`V1` AS
                                SELECT
                                  `foo`.`U`.`C`
                                FROM `foo`.`U`;
                            """ ),
                        (sql, _) => sql.SatisfySql(
                            "DROP VIEW `foo`.`V2`;",
                            """
                            CREATE VIEW `foo`.`V2` AS
                                SELECT
                                  *
                                FROM `foo`.`V1`
                                INNER JOIN `foo`.`U` ON TRUE;
                            """ )
                    ] ) )
            .Go();
    }

    [Fact]
    public void SetName_ShouldUpdateName_WhenNameChangesAndTableHasViewReferencesAndChangedTableHasOtherPendingChanges()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var sut = schema.Objects.CreateTable( "T" );
        sut.Constraints.SetPrimaryKey( sut.Columns.Create( "C" ).Asc() );

        var v1 = schema.Objects.CreateView( "V1", sut.Node.ToDataSource().Select( s => new[] { s.From["C"].AsSelf() } ) );
        var v2 = schema.Objects.CreateView( "V2", v1.Node.Join( sut.Node.InnerOn( SqlNode.True() ) ).Select( s => new[] { s.GetAll() } ) );
        var v3 = schema.Objects.CreateView( "V3", v1.ToRecordSet().ToDataSource().Select( s => new[] { s.GetAll() } ) );

        var actionCount = schema.Database.GetPendingActionCount();
        sut.Columns.Create( "D" ).SetType<int>();
        var result = sut.SetName( "U" );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        Assertion.All(
                result.TestRefEquals( sut ),
                sut.Name.TestEquals( "U" ),
                schema.Objects.TryGet( "U" ).TestRefEquals( sut ),
                schema.Objects.TryGet( "T" ).TestNull(),
                v1.IsRemoved.TestFalse(),
                v2.IsRemoved.TestFalse(),
                v3.IsRemoved.TestFalse(),
                actions.Select( a => a.Sql )
                    .TestSequence(
                    [
                        (sql, _) => sql.SatisfySql( "ALTER TABLE `foo`.`T` RENAME TO `foo`.`U`;" ),
                        (sql, _) => sql.SatisfySql(
                            "DROP VIEW `foo`.`V1`;",
                            """
                            CREATE VIEW `foo`.`V1` AS
                                SELECT
                                  `foo`.`U`.`C`
                                FROM `foo`.`U`;
                            """ ),
                        (sql, _) => sql.SatisfySql(
                            "DROP VIEW `foo`.`V2`;",
                            """
                            CREATE VIEW `foo`.`V2` AS
                                SELECT
                                  *
                                FROM `foo`.`V1`
                                INNER JOIN `foo`.`U` ON TRUE;
                            """ ),
                        (sql, _) => sql.SatisfySql(
                            """
                            ALTER TABLE `foo`.`U`
                                ADD COLUMN `D` INT NOT NULL DEFAULT 0;
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
        var sut = schema.Objects.CreateTable( "T" );

        var action = Lambda.Of( () => sut.SetName( name ) );

        action.Test(
                exc => exc.TestType()
                    .Exact<SqlObjectBuilderException>(
                        e => Assertion.All( e.Dialect.TestEquals( MySqlDialect.Instance ), e.Errors.Count.TestEquals( 1 ) ) ) )
            .Go();
    }

    [Fact]
    public void SetName_ShouldThrowSqlObjectBuilderException_WhenTableIsRemoved()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var sut = schema.Objects.CreateTable( "T" );
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
        var sut = schema.Objects.CreateTable( "T" );
        sut.Constraints.SetPrimaryKey( sut.Columns.Create( "C" ).Asc() );

        var action = Lambda.Of( () => sut.SetName( "PK_T" ) );

        action.Test(
                exc => exc.TestType()
                    .Exact<SqlObjectBuilderException>(
                        e => Assertion.All( e.Dialect.TestEquals( MySqlDialect.Instance ), e.Errors.Count.TestEquals( 1 ) ) ) )
            .Go();
    }

    [Fact]
    public void Remove_ShouldRemoveTableAndQuickRemoveColumnsAndConstraints()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
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
                actions.Select( a => a.Sql ).TestSequence( [ (sql, _) => sql.SatisfySql( "DROP TABLE `foo`.`T`;" ) ] ) )
            .Go();
    }

    [Fact]
    public void Remove_ShouldRemoveTable_ByOldName()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var sut = schema.Objects.CreateTable( "T" );
        var c1 = sut.Columns.Create( "C1" );
        var pk = sut.Constraints.SetPrimaryKey( c1.Asc() );

        var actionCount = schema.Database.GetPendingActionCount();
        sut.SetName( "bar" ).Remove();
        var actions = schema.Database.GetLastPendingActions( actionCount );

        Assertion.All(
                schema.Objects.TryGet( sut.Name ).TestNull(),
                schema.Objects.TryGet( pk.Name ).TestNull(),
                schema.Objects.TryGet( pk.Index.Name ).TestNull(),
                sut.IsRemoved.TestTrue(),
                sut.ReferencingObjects.TestEmpty(),
                sut.Columns.TestEmpty(),
                sut.Constraints.TestEmpty(),
                sut.Constraints.TryGetPrimaryKey().TestNull(),
                c1.IsRemoved.TestTrue(),
                c1.ReferencingObjects.TestEmpty(),
                pk.IsRemoved.TestTrue(),
                pk.ReferencingObjects.TestEmpty(),
                pk.Index.IsRemoved.TestTrue(),
                pk.Index.ReferencingObjects.TestEmpty(),
                pk.Index.Columns.Expressions.TestEmpty(),
                pk.Index.PrimaryKey.TestNull(),
                actions.Select( a => a.Sql ).TestSequence( [ (sql, _) => sql.SatisfySql( "DROP TABLE `foo`.`T`;" ) ] ) )
            .Go();
    }

    [Fact]
    public void Remove_ShouldDoNothing_WhenTableHasAlreadyBeenRemoved()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var sut = schema.Objects.CreateTable( "T" );
        sut.Constraints.SetPrimaryKey( sut.Columns.Create( "C" ).Asc() );

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
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var sut = schema.Objects.CreateTable( "T" );
        var pk = sut.Constraints.SetPrimaryKey( sut.Columns.Create( "C" ).Asc() );

        var otherTable = schema.Objects.CreateTable( "U" );
        var otherPk = otherTable.Constraints.SetPrimaryKey( otherTable.Columns.Create( "D" ).Asc() );
        otherTable.Constraints.CreateForeignKey( otherPk.Index, pk.Index );

        var action = Lambda.Of( () => sut.Remove() );

        action.Test(
                exc => exc.TestType()
                    .Exact<SqlObjectBuilderException>(
                        e => Assertion.All( e.Dialect.TestEquals( MySqlDialect.Instance ), e.Errors.Count.TestEquals( 1 ) ) ) )
            .Go();
    }

    [Fact]
    public void Remove_ShouldThrowSqlObjectBuilderException_WhenTableIsReferencedByAnyView()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var sut = schema.Objects.CreateTable( "T" );
        sut.Constraints.SetPrimaryKey( sut.Columns.Create( "C" ).Asc() );
        schema.Objects.CreateView( "V", sut.Node.ToDataSource().Select( s => new[] { s.GetAll() } ) );

        var action = Lambda.Of( () => sut.Remove() );

        action.Test(
                exc => exc.TestType()
                    .Exact<SqlObjectBuilderException>(
                        e => Assertion.All( e.Dialect.TestEquals( MySqlDialect.Instance ), e.Errors.Count.TestEquals( 1 ) ) ) )
            .Go();
    }

    [Fact]
    public void ColumnNameSwap_ShouldGenerateCorrectScript()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var sut = schema.Objects.CreateTable( "T" );
        sut.Constraints.SetPrimaryKey( sut.Columns.Create( "P" ).Asc() );
        var a = sut.Columns.Create( "A" );
        var b = sut.Columns.Create( "B" );

        var actionCount = schema.Database.GetPendingActionCount();
        a.SetName( "C" );
        b.SetName( "A" );
        a.SetName( "B" );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        actions.Select( ac => ac.Sql )
            .TestSequence(
            [
                (sql, _) => sql.SatisfySql(
                    """
                    ALTER TABLE `foo`.`T`
                        CHANGE COLUMN `A` `B` LONGBLOB NOT NULL,
                        CHANGE COLUMN `B` `A` LONGBLOB NOT NULL;
                    """ )
            ] )
            .Go();
    }

    [Fact]
    public void ColumnChainNameSwap_ShouldGenerateCorrectScript()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var sut = schema.Objects.CreateTable( "T" );
        sut.Constraints.SetPrimaryKey( sut.Columns.Create( "P" ).Asc() );
        var a = sut.Columns.Create( "A" );
        var b = sut.Columns.Create( "B" );
        var c = sut.Columns.Create( "C" );
        var d = sut.Columns.Create( "D" );

        var actionCount = schema.Database.GetPendingActionCount();
        a.SetName( "E" );
        b.SetName( "A" );
        c.SetName( "B" );
        d.SetName( "C" );
        a.SetName( "D" );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        actions.Select( ac => ac.Sql )
            .TestSequence(
            [
                (sql, _) => sql.SatisfySql(
                    """
                    ALTER TABLE `foo`.`T`
                        CHANGE COLUMN `A` `D` LONGBLOB NOT NULL,
                        CHANGE COLUMN `B` `A` LONGBLOB NOT NULL,
                        CHANGE COLUMN `C` `B` LONGBLOB NOT NULL,
                        CHANGE COLUMN `D` `C` LONGBLOB NOT NULL;
                    """ )
            ] )
            .Go();
    }

    [Fact]
    public void MultipleColumnNameChange_ShouldGenerateCorrectScript_WhenThereAreTemporaryNameConflicts()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var sut = schema.Objects.CreateTable( "T" );
        sut.Constraints.SetPrimaryKey( sut.Columns.Create( "P" ).Asc() );
        var a = sut.Columns.Create( "A" );
        var b = sut.Columns.Create( "B" );
        var c = sut.Columns.Create( "C" );

        var actionCount = schema.Database.GetPendingActionCount();
        a.SetName( "X" );
        b.SetName( "Y" );
        c.SetName( "D" );
        b.SetName( "C" );
        a.SetName( "B" );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        actions.Select( ac => ac.Sql )
            .TestSequence(
            [
                (sql, _) => sql.SatisfySql(
                    """
                    ALTER TABLE `foo`.`T`
                        CHANGE COLUMN `A` `B` LONGBLOB NOT NULL,
                        CHANGE COLUMN `B` `C` LONGBLOB NOT NULL,
                        CHANGE COLUMN `C` `D` LONGBLOB NOT NULL;
                    """ )
            ] )
            .Go();
    }

    [Fact]
    public void ConstraintChainNameSwap_ShouldGenerateCorrectScript()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var sut = schema.Objects.CreateTable( "T" );
        var a = sut.Constraints.SetPrimaryKey( "A", sut.Columns.Create( "P" ).Asc() );
        var b = sut.Constraints.CreateCheck( "B", SqlNode.True() );
        var c = sut.Constraints.CreateIndex( "C", sut.Columns.Create( "I" ).Asc() );
        var d = sut.Constraints.CreateCheck( "D", SqlNode.True() );

        var actionCount = schema.Database.GetPendingActionCount();
        a.SetName( "E" );
        b.SetName( "A" );
        c.SetName( "B" );
        d.SetName( "C" );
        a.SetName( "D" );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        actions.Select( ac => ac.Sql )
            .TestSequence(
            [
                (sql, _) => sql.SatisfySql(
                    """
                    ALTER TABLE `foo`.`T`
                        DROP PRIMARY KEY,
                        DROP CHECK `B`,
                        DROP CHECK `D`,
                        RENAME INDEX `C` TO `B`,
                        ADD CONSTRAINT `D` PRIMARY KEY (`P`(500) ASC),
                        ADD CONSTRAINT `A` CHECK (TRUE),
                        ADD CONSTRAINT `C` CHECK (TRUE);
                    """ )
            ] )
            .Go();
    }

    [Fact]
    public void MultipleConstraintNameChange_ShouldGenerateCorrectScript_WhenThereAreTemporaryNameConflicts()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var sut = schema.Objects.CreateTable( "T" );
        var a = sut.Constraints.SetPrimaryKey( "A", sut.Columns.Create( "P" ).Asc() );
        var b = sut.Constraints.CreateCheck( "B", SqlNode.True() );
        var c = sut.Constraints.CreateIndex( "C", sut.Columns.Create( "I" ).Asc() );

        var actionCount = schema.Database.GetPendingActionCount();
        a.SetName( "X" );
        b.SetName( "Y" );
        c.SetName( "D" );
        b.SetName( "C" );
        a.SetName( "B" );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        actions.Select( ac => ac.Sql )
            .TestSequence(
            [
                (sql, _) => sql.SatisfySql(
                    """
                    ALTER TABLE `foo`.`T`
                        DROP PRIMARY KEY,
                        DROP CHECK `B`,
                        RENAME INDEX `C` TO `D`,
                        ADD CONSTRAINT `B` PRIMARY KEY (`P`(500) ASC),
                        ADD CONSTRAINT `C` CHECK (TRUE);
                    """ )
            ] )
            .Go();
    }

    [Fact]
    public void MultipleTableChanges_ShouldGenerateCorrectScript()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var sut = schema.Objects.CreateTable( "T" );
        sut.Constraints.SetPrimaryKey( sut.Columns.Create( "C1" ).Asc() );
        var c2 = sut.Columns.Create( "C2" ).SetType<int>();
        var c3 = sut.Columns.Create( "C3" ).SetType<int>();
        var c4 = sut.Columns.Create( "C4" ).SetType<int>();
        var c5 = sut.Columns.Create( "C5" ).SetType<int>().SetDefaultValue( 123 );
        var c6 = sut.Columns.Create( "C6" ).SetType<int>().SetComputation( SqlColumnComputation.Stored( SqlNode.Literal( 42 ) ) );
        var ix1 = sut.Constraints.CreateIndex( c2.Asc() );
        var ix2 = sut.Constraints.CreateIndex( sut.Columns.Create( "C7" ).Asc() );
        var chk1 = sut.Constraints.CreateCheck( "CHK_1", SqlNode.True() );
        var chk2 = sut.Constraints.CreateCheck( "CHK_2", SqlNode.True() );
        var fk1 = sut.Constraints.CreateForeignKey( ix2, sut.Constraints.CreateUniqueIndex( sut.Columns.Create( "C8" ).Asc() ) );
        var fk2 = sut.Constraints.CreateForeignKey( "FK", ix2, sut.Constraints.CreateUniqueIndex( sut.Columns.Create( "C9" ).Asc() ) );

        var actionCount = schema.Database.GetPendingActionCount();
        sut.SetName( "U" );
        c3.SetName( "X" ).MarkAsNullable().SetType<long>();
        c4.Remove();
        c5.SetComputation( SqlColumnComputation.Stored( SqlNode.Literal( 1 ) ) );
        c6.SetComputation( null ).SetName( "Y" );
        ix1.Remove();
        ix2.SetName( "IX_2" );
        fk1.Remove();
        fk2.SetName( "FK_2" );
        chk1.Remove();
        chk2.SetName( "CHK_1" );
        sut.Constraints.CreateIndex( c2.Asc(), c3.Desc() );
        sut.Constraints.CreateCheck( "CHK_3", SqlNode.True() );
        sut.Constraints.SetPrimaryKey( sut.Columns.Create( "C10" ).Asc() );
        sut.Constraints.CreateForeignKey( ix2, sut.Constraints.CreateUniqueIndex( sut.Columns.Create( "C11" ).Asc() ) );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        actions.Select( a => a.Sql )
            .TestSequence(
            [
                (sql, _) => sql.SatisfySql( "ALTER TABLE `foo`.`T` RENAME TO `foo`.`U`;" ),
                (sql, _) => sql.SatisfySql(
                    """
                    ALTER TABLE `foo`.`U`
                        DROP FOREIGN KEY `FK_T_C7_REF_T`,
                        DROP FOREIGN KEY `FK`;
                    """,
                    "DROP INDEX `IX_T_C2A` ON `foo`.`U`;",
                    """
                    ALTER TABLE `foo`.`U`
                        DROP PRIMARY KEY,
                        DROP CHECK `CHK_1`,
                        DROP CHECK `CHK_2`,
                        RENAME INDEX `IX_T_C7A` TO `IX_2`,
                        DROP COLUMN `C4`,
                        CHANGE COLUMN `C3` `X` BIGINT,
                        CHANGE COLUMN `C5` `C5` INT GENERATED ALWAYS AS (1) STORED NOT NULL,
                        CHANGE COLUMN `C6` `Y` INT NOT NULL,
                        ADD COLUMN `C10` LONGBLOB NOT NULL DEFAULT (X''),
                        ADD COLUMN `C11` LONGBLOB NOT NULL DEFAULT (X''),
                        ADD CONSTRAINT `PK_U` PRIMARY KEY (`C10`(500) ASC),
                        ADD CONSTRAINT `CHK_1` CHECK (TRUE),
                        ADD CONSTRAINT `CHK_3` CHECK (TRUE);
                    """,
                    "CREATE INDEX `IX_U_C2A_XD` ON `foo`.`U` (`C2` ASC, `X` DESC);",
                    "CREATE UNIQUE INDEX `UIX_U_C11A` ON `foo`.`U` (`C11`(500) ASC);",
                    """
                    ALTER TABLE `foo`.`U`
                        ADD CONSTRAINT `FK_2` FOREIGN KEY (`C7`) REFERENCES `foo`.`U` (`C9`) ON DELETE RESTRICT ON UPDATE RESTRICT,
                        ADD CONSTRAINT `FK_U_C7_REF_U` FOREIGN KEY (`C7`) REFERENCES `foo`.`U` (`C11`) ON DELETE RESTRICT ON UPDATE RESTRICT;
                    """ )
            ] )
            .Go();
    }

    [Fact]
    public void ForMySql_ShouldInvokeAction_WhenTableIsMySql()
    {
        var action = Substitute.For<Action<MySqlTableBuilder>>();
        var sut = MySqlDatabaseBuilderMock.Create().Schemas.Default.Objects.CreateTable( "T" );

        var result = sut.ForMySql( action );

        Assertion.All(
                result.TestRefEquals( sut ),
                action.CallAt( 0 ).Arguments.TestSequence( [ sut ] ) )
            .Go();
    }

    [Fact]
    public void ForMySql_ShouldNotInvokeAction_WhenTableIsNotMySql()
    {
        var action = Substitute.For<Action<MySqlTableBuilder>>();
        var sut = Substitute.For<ISqlTableBuilder>();

        var result = sut.ForMySql( action );

        Assertion.All(
                result.TestRefEquals( sut ),
                action.CallCount().TestEquals( 0 ) )
            .Go();
    }
}
