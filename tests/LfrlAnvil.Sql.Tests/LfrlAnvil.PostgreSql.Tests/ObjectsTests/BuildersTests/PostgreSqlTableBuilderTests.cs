using System.Linq;
using LfrlAnvil.Functional;
using LfrlAnvil.PostgreSql.Extensions;
using LfrlAnvil.PostgreSql.Objects.Builders;
using LfrlAnvil.PostgreSql.Tests.Helpers;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.PostgreSql.Tests.ObjectsTests.BuildersTests;

public partial class PostgreSqlTableBuilderTests : TestsBase
{
    [Fact]
    public void ToString_ShouldReturnCorrectResult()
    {
        var schema = PostgreSqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var sut = schema.Objects.CreateTable( "bar" );

        var result = sut.ToString();

        result.TestEquals( "[Table] foo.bar" ).Go();
    }

    [Fact]
    public void Creation_ShouldPrepareCorrectStatement()
    {
        var schema = PostgreSqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
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
                        (sql, _) => sql.TestSatisfySql(
                            """
                            CREATE TABLE "foo"."T" (
                              "C5" BYTEA NOT NULL GENERATED ALWAYS AS ("C1" + 1) STORED,
                              "C1" INT4 NOT NULL,
                              "C2" INT4 NOT NULL,
                              "C6" BYTEA GENERATED ALWAYS AS ("C2" * "C5") STORED,
                              "C3" INT8 NOT NULL,
                              "C4" INT8 NOT NULL,
                              CONSTRAINT "PK_T" PRIMARY KEY ("C2"),
                              CONSTRAINT "FK_T_C1_REF_T" FOREIGN KEY ("C1") REFERENCES "foo"."T" ("C2") ON DELETE RESTRICT ON UPDATE RESTRICT,
                              CONSTRAINT "CHK_T_{GUID}" CHECK ("C1" > 0)
                            );
                            """,
                            "CREATE INDEX \"IX_T_C1A\" ON \"foo\".\"T\" (\"C1\" ASC);",
                            "CREATE INDEX \"IX_T_C3A_C4D\" ON \"foo\".\"T\" (\"C3\" ASC, \"C4\" DESC);" )
                    ] ) )
            .Go();
    }

    [Fact]
    public void Creation_FollowedByRemoval_ShouldDoNothing()
    {
        var schema = PostgreSqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );

        var actionCount = schema.Database.GetPendingActionCount();
        var sut = schema.Objects.CreateTable( "T" );
        sut.Remove();
        var actions = schema.Database.GetLastPendingActions( actionCount );

        actions.TestEmpty().Go();
    }

    [Fact]
    public void Creation_ShouldThrowSqlObjectBuilderException_WhenTableDoesNotHavePrimaryKeyDuringScriptResolution()
    {
        var schema = PostgreSqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var sut = schema.Objects.CreateTable( "T" );
        sut.Columns.Create( "C" );

        var action = Lambda.Of( () => schema.Database.Changes.CompletePendingChanges() );

        action.Test(
                exc => exc.TestType()
                    .Exact<SqlObjectBuilderException>(
                        e => Assertion.All( e.Dialect.TestEquals( PostgreSqlDialect.Instance ), e.Errors.Count.TestEquals( 1 ) ) ) )
            .Go();
    }

    [Fact]
    public void SetName_ShouldDoNothing_WhenNewNameEqualsOldName()
    {
        var schema = PostgreSqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
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
        var schema = PostgreSqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
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
        var schema = PostgreSqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
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
                    .TestSequence( [ (sql, _) => sql.TestSatisfySql( "ALTER TABLE \"foo\".\"T\" RENAME TO \"bar\";" ) ] ) )
            .Go();
    }

    [Fact]
    public void SetName_ShouldUpdateName_WhenNameChangesAndTableHasSelfReference()
    {
        var schema = PostgreSqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );

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
                    .TestSequence( [ (sql, _) => sql.TestSatisfySql( "ALTER TABLE \"foo\".\"T\" RENAME TO \"bar\";" ) ] ) )
            .Go();
    }

    [Fact]
    public void SetName_ShouldUpdateName_WhenNameChangesAndTableHasExternalForeignKeyReferences()
    {
        var schema = PostgreSqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
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
                    .TestSequence( [ (sql, _) => sql.TestSatisfySql( "ALTER TABLE \"foo\".\"T1\" RENAME TO \"U\";" ) ] ) )
            .Go();
    }

    [Fact]
    public void SetName_ShouldUpdateName_WhenNameChangesAndTableHasExternalForeignKeyReferencesAndChangedTableHasOtherPendingChanges()
    {
        var schema = PostgreSqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
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
                        (sql, _) => sql.TestSatisfySql( "ALTER TABLE \"foo\".\"T1\" RENAME TO \"U\";" ),
                        (sql, _) => sql.TestSatisfySql(
                            """
                            ALTER TABLE "foo"."U"
                                ADD COLUMN "C3" INT4 NOT NULL DEFAULT (0);
                            """ )
                    ] ) )
            .Go();
    }

    [Fact]
    public void SetName_ShouldUpdateName_WhenNameChangesAndTableHasViewReferences()
    {
        var schema = PostgreSqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
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
                    .TestSequence( [ (sql, _) => sql.TestSatisfySql( "ALTER TABLE \"foo\".\"T\" RENAME TO \"U\";" ) ] ) )
            .Go();
    }

    [Fact]
    public void SetName_ShouldUpdateName_WhenNameChangesAndTableHasViewReferencesAndChangedTableHasOtherPendingChanges()
    {
        var schema = PostgreSqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
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
                        (sql, _) => sql.TestSatisfySql( "ALTER TABLE \"foo\".\"T\" RENAME TO \"U\";" ),
                        (sql, _) => sql.TestSatisfySql(
                            """
                            ALTER TABLE "foo"."U"
                                ADD COLUMN "D" INT4 NOT NULL DEFAULT (0);
                            """ )
                    ] ) )
            .Go();
    }

    [Theory]
    [InlineData( "" )]
    [InlineData( " " )]
    [InlineData( "\"" )]
    [InlineData( "'" )]
    [InlineData( "f\"oo" )]
    public void SetName_ShouldThrowSqlObjectBuilderException_WhenNameIsInvalid(string name)
    {
        var schema = PostgreSqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var sut = schema.Objects.CreateTable( "T" );

        var action = Lambda.Of( () => sut.SetName( name ) );

        action.Test(
                exc => exc.TestType()
                    .Exact<SqlObjectBuilderException>(
                        e => Assertion.All( e.Dialect.TestEquals( PostgreSqlDialect.Instance ), e.Errors.Count.TestEquals( 1 ) ) ) )
            .Go();
    }

    [Fact]
    public void SetName_ShouldThrowSqlObjectBuilderException_WhenTableIsRemoved()
    {
        var schema = PostgreSqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var sut = schema.Objects.CreateTable( "T" );
        sut.Remove();

        var action = Lambda.Of( () => sut.SetName( "bar" ) );

        action.Test(
                exc => exc.TestType()
                    .Exact<SqlObjectBuilderException>(
                        e => Assertion.All( e.Dialect.TestEquals( PostgreSqlDialect.Instance ), e.Errors.Count.TestEquals( 1 ) ) ) )
            .Go();
    }

    [Fact]
    public void SetName_ShouldThrowSqlObjectBuilderException_WhenNewNameAlreadyExistsInSchemaObjects()
    {
        var schema = PostgreSqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var sut = schema.Objects.CreateTable( "T" );
        sut.Constraints.SetPrimaryKey( sut.Columns.Create( "C" ).Asc() );

        var action = Lambda.Of( () => sut.SetName( "PK_T" ) );

        action.Test(
                exc => exc.TestType()
                    .Exact<SqlObjectBuilderException>(
                        e => Assertion.All( e.Dialect.TestEquals( PostgreSqlDialect.Instance ), e.Errors.Count.TestEquals( 1 ) ) ) )
            .Go();
    }

    [Fact]
    public void Remove_ShouldRemoveTableAndQuickRemoveColumnsAndConstraints()
    {
        var schema = PostgreSqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
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
                actions.Select( a => a.Sql ).TestSequence( [ (sql, _) => sql.TestSatisfySql( "DROP TABLE \"foo\".\"T\";" ) ] ) )
            .Go();
    }

    [Fact]
    public void Remove_ShouldRemoveTable_ByOldName()
    {
        var schema = PostgreSqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
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
                actions.Select( a => a.Sql ).TestSequence( [ (sql, _) => sql.TestSatisfySql( "DROP TABLE \"foo\".\"T\";" ) ] ) )
            .Go();
    }

    [Fact]
    public void Remove_ShouldDoNothing_WhenTableHasAlreadyBeenRemoved()
    {
        var schema = PostgreSqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
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
        var schema = PostgreSqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var sut = schema.Objects.CreateTable( "T" );
        var pk = sut.Constraints.SetPrimaryKey( sut.Columns.Create( "C" ).Asc() );

        var otherTable = schema.Objects.CreateTable( "U" );
        var otherPk = otherTable.Constraints.SetPrimaryKey( otherTable.Columns.Create( "D" ).Asc() );
        otherTable.Constraints.CreateForeignKey( otherPk.Index, pk.Index );

        var action = Lambda.Of( () => sut.Remove() );

        action.Test(
                exc => exc.TestType()
                    .Exact<SqlObjectBuilderException>(
                        e => Assertion.All( e.Dialect.TestEquals( PostgreSqlDialect.Instance ), e.Errors.Count.TestEquals( 1 ) ) ) )
            .Go();
    }

    [Fact]
    public void Remove_ShouldThrowSqlObjectBuilderException_WhenTableIsReferencedByAnyView()
    {
        var schema = PostgreSqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var sut = schema.Objects.CreateTable( "T" );
        sut.Constraints.SetPrimaryKey( sut.Columns.Create( "C" ).Asc() );
        schema.Objects.CreateView( "V", sut.Node.ToDataSource().Select( s => new[] { s.GetAll() } ) );

        var action = Lambda.Of( () => sut.Remove() );

        action.Test(
                exc => exc.TestType()
                    .Exact<SqlObjectBuilderException>(
                        e => Assertion.All( e.Dialect.TestEquals( PostgreSqlDialect.Instance ), e.Errors.Count.TestEquals( 1 ) ) ) )
            .Go();
    }

    [Fact]
    public void ColumnNameSwap_ShouldGenerateCorrectScript()
    {
        var schema = PostgreSqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
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
                (sql, _) => sql.TestSatisfySql(
                    "ALTER TABLE \"foo\".\"T\" RENAME COLUMN \"A\" TO \"__A__{GUID}__\";",
                    "ALTER TABLE \"foo\".\"T\" RENAME COLUMN \"B\" TO \"A\";",
                    "ALTER TABLE \"foo\".\"T\" RENAME COLUMN \"__A__{GUID}__\" TO \"B\";" )
            ] )
            .Go();
    }

    [Fact]
    public void ColumnChainNameSwap_ShouldGenerateCorrectScript()
    {
        var schema = PostgreSqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
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
                (sql, _) => sql.TestSatisfySql(
                    "ALTER TABLE \"foo\".\"T\" RENAME COLUMN \"A\" TO \"__A__{GUID}__\";",
                    "ALTER TABLE \"foo\".\"T\" RENAME COLUMN \"B\" TO \"A\";",
                    "ALTER TABLE \"foo\".\"T\" RENAME COLUMN \"C\" TO \"B\";",
                    "ALTER TABLE \"foo\".\"T\" RENAME COLUMN \"D\" TO \"C\";",
                    "ALTER TABLE \"foo\".\"T\" RENAME COLUMN \"__A__{GUID}__\" TO \"D\";" )
            ] )
            .Go();
    }

    [Fact]
    public void MultipleColumnNameChange_ShouldGenerateCorrectScript_WhenThereAreTemporaryNameConflicts()
    {
        var schema = PostgreSqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
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
                (sql, _) => sql.TestSatisfySql(
                    "ALTER TABLE \"foo\".\"T\" RENAME COLUMN \"C\" TO \"D\";",
                    "ALTER TABLE \"foo\".\"T\" RENAME COLUMN \"B\" TO \"C\";",
                    "ALTER TABLE \"foo\".\"T\" RENAME COLUMN \"A\" TO \"B\";" )
            ] )
            .Go();
    }

    [Fact]
    public void ConstraintChainNameSwap_ShouldGenerateCorrectScript()
    {
        var schema = PostgreSqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
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
                (sql, _) => sql.TestSatisfySql(
                    """
                    ALTER TABLE "foo"."T"
                        RENAME CONSTRAINT "A" TO "__A__{GUID}__";
                    """,
                    """
                    ALTER TABLE "foo"."T"
                        RENAME CONSTRAINT "B" TO "A";
                    """,
                    "ALTER INDEX \"foo\".\"C\" RENAME TO \"B\";",
                    """
                    ALTER TABLE "foo"."T"
                        RENAME CONSTRAINT "D" TO "C";
                    """,
                    """
                    ALTER TABLE "foo"."T"
                        RENAME CONSTRAINT "__A__{GUID}__" TO "D";
                    """ )
            ] )
            .Go();
    }

    [Fact]
    public void MultipleConstraintNameChange_ShouldGenerateCorrectScript_WhenThereAreTemporaryNameConflicts()
    {
        var schema = PostgreSqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
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
                (sql, _) => sql.TestSatisfySql(
                    "ALTER INDEX \"foo\".\"C\" RENAME TO \"D\";",
                    """
                    ALTER TABLE "foo"."T"
                        RENAME CONSTRAINT "B" TO "C";
                    """,
                    """
                    ALTER TABLE "foo"."T"
                        RENAME CONSTRAINT "A" TO "B";
                    """ )
            ] )
            .Go();
    }

    [Fact]
    public void MultipleTableChanges_ShouldGenerateCorrectScript()
    {
        var schema = PostgreSqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
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
                (sql, _) => sql.TestSatisfySql( "ALTER TABLE \"foo\".\"T\" RENAME TO \"U\";" ),
                (sql, _) => sql.TestSatisfySql(
                    """
                    ALTER TABLE "foo"."U"
                        DROP CONSTRAINT "FK_T_C7_REF_T",
                        DROP CONSTRAINT "CHK_1";
                    """,
                    "DROP INDEX \"foo\".\"IX_T_C2A\";",
                    """
                    ALTER TABLE "foo"."U"
                        DROP CONSTRAINT "PK_T";
                    """,
                    """
                    ALTER TABLE "foo"."U"
                        ALTER COLUMN "C6" DROP EXPRESSION,
                        DROP COLUMN "C4",
                        DROP COLUMN "C5";
                    """,
                    "ALTER INDEX \"foo\".\"IX_T_C7A\" RENAME TO \"IX_2\";",
                    """
                    ALTER TABLE "foo"."U"
                        RENAME CONSTRAINT "FK" TO "FK_2";
                    """,
                    """
                    ALTER TABLE "foo"."U"
                        RENAME CONSTRAINT "CHK_2" TO "CHK_1";
                    """,
                    "ALTER TABLE \"foo\".\"U\" RENAME COLUMN \"C3\" TO \"X\";",
                    "ALTER TABLE \"foo\".\"U\" RENAME COLUMN \"C6\" TO \"Y\";",
                    """
                    ALTER TABLE "foo"."U"
                        ALTER COLUMN "X" DROP NOT NULL,
                        ALTER COLUMN "X" SET DATA TYPE INT8,
                        ADD COLUMN "C10" BYTEA NOT NULL DEFAULT ('\x'::BYTEA),
                        ADD COLUMN "C11" BYTEA NOT NULL DEFAULT ('\x'::BYTEA),
                        ADD COLUMN "C5" INT4 NOT NULL GENERATED ALWAYS AS (1) STORED,
                        ADD CONSTRAINT "PK_U" PRIMARY KEY ("C10");
                    """,
                    "CREATE INDEX \"IX_U_C2A_XD\" ON \"foo\".\"U\" (\"C2\" ASC, \"X\" DESC);",
                    "CREATE UNIQUE INDEX \"UIX_U_C11A\" ON \"foo\".\"U\" (\"C11\" ASC);",
                    """
                    ALTER TABLE "foo"."U"
                        ADD CONSTRAINT "CHK_3" CHECK (TRUE),
                        ADD CONSTRAINT "FK_U_C7_REF_U" FOREIGN KEY ("C7") REFERENCES "foo"."U" ("C11") ON DELETE RESTRICT ON UPDATE RESTRICT;
                    """ )
            ] )
            .Go();
    }

    [Fact]
    public void ForPostgreSql_ShouldInvokeAction_WhenTableIsPostgreSql()
    {
        var action = Substitute.For<Action<PostgreSqlTableBuilder>>();
        var sut = PostgreSqlDatabaseBuilderMock.Create().Schemas.Default.Objects.CreateTable( "T" );

        var result = sut.ForPostgreSql( action );

        Assertion.All(
                result.TestRefEquals( sut ),
                action.CallAt( 0 ).Arguments.TestSequence( [ sut ] ) )
            .Go();
    }

    [Fact]
    public void ForPostgreSql_ShouldNotInvokeAction_WhenTableIsNotPostgreSql()
    {
        var action = Substitute.For<Action<PostgreSqlTableBuilder>>();
        var sut = Substitute.For<ISqlTableBuilder>();

        var result = sut.ForPostgreSql( action );

        Assertion.All(
                result.TestRefEquals( sut ),
                action.CallCount().TestEquals( 0 ) )
            .Go();
    }
}
