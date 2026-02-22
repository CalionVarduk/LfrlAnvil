using System.Linq;
using LfrlAnvil.Functional;
using LfrlAnvil.MySql.Extensions;
using LfrlAnvil.MySql.Objects.Builders;
using LfrlAnvil.MySql.Tests.Helpers;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.MySql.Tests.ObjectsTests.BuildersTests;

public partial class MySqlSchemaBuilderTests : TestsBase
{
    [Fact]
    public void ToString_ShouldReturnCorrectResult()
    {
        var db = MySqlDatabaseBuilderMock.Create();
        var sut = db.Schemas.Create( "foo" );

        var result = sut.ToString();

        result.TestEquals( "[Schema] foo" ).Go();
    }

    [Fact]
    public void Creation_ShouldNotAddAnyStatements_WhenSchemaNameIsCommon()
    {
        var db = MySqlDatabaseBuilderMock.Create();
        db.Schemas.Default.SetName( "foo" );

        var actionCount = db.GetPendingActionCount();
        var sut = db.Schemas.Create( db.CommonSchemaName );
        var actions = db.GetLastPendingActions( actionCount );

        Assertion.All(
                db.Schemas.TryGet( sut.Name ).TestRefEquals( sut ),
                sut.Name.TestEquals( db.CommonSchemaName ),
                actions.TestEmpty() )
            .Go();
    }

    [Fact]
    public void Creation_ShouldAddStatement_WhenSchemaNameIsNotCommon()
    {
        var db = MySqlDatabaseBuilderMock.Create();

        var actionCount = db.GetPendingActionCount();
        var sut = db.Schemas.Create( "foo" );
        var actions = db.GetLastPendingActions( actionCount );

        Assertion.All(
                db.Schemas.TryGet( sut.Name ).TestRefEquals( sut ),
                sut.Name.TestEquals( "foo" ),
                actions.Select( a => a.Sql ).TestSequence( [ (sql, _) => sql.TestSatisfySql( "CREATE SCHEMA `foo`;" ) ] ) )
            .Go();
    }

    [Theory]
    [InlineData( "charset", "coll", true, "CREATE SCHEMA `foo` CHARACTER SET = 'charset' COLLATE = 'coll' ENCRYPTION = 'Y';" )]
    [InlineData( "charset", "coll", false, "CREATE SCHEMA `foo` CHARACTER SET = 'charset' COLLATE = 'coll' ENCRYPTION = 'N';" )]
    public void Creation_ShouldAddStatement_WithCustomOptions(
        string characterSetName,
        string collationName,
        bool isEncryptionEnabled,
        string expected)
    {
        var db = MySqlDatabaseBuilderMock.Create(
            characterSetName: characterSetName,
            collationName: collationName,
            isEncryptionEnabled: isEncryptionEnabled );

        var actionCount = db.GetPendingActionCount();
        var sut = db.Schemas.Create( "foo" );
        var actions = db.GetLastPendingActions( actionCount );

        Assertion.All(
                db.Schemas.TryGet( sut.Name ).TestRefEquals( sut ),
                sut.Name.TestEquals( "foo" ),
                actions.Select( a => a.Sql ).TestSequence( [ (sql, _) => sql.TestSatisfySql( expected ) ] ) )
            .Go();
    }

    [Fact]
    public void SetName_ShouldDoNothing_WhenNewNameEqualsOldName()
    {
        var db = MySqlDatabaseBuilderMock.Create();
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
    public void SetName_ShouldUpdateName_WhenNameChangesAndSchemaDoesNotHaveAnyObjects()
    {
        var db = MySqlDatabaseBuilderMock.Create();
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
                        (sql, _) => sql.TestSatisfySql( "CREATE SCHEMA `bar`;" ),
                        (sql, _) => sql.TestSatisfySql( "DROP SCHEMA `foo`;" )
                    ] ) )
            .Go();
    }

    [Fact]
    public void SetName_ShouldUpdateName_WhenNameChangesAndSchemaIsOriginallyCommon()
    {
        var db = MySqlDatabaseBuilderMock.Create();
        var sut = db.Schemas.Default;

        var actionCount = db.GetPendingActionCount();
        var result = sut.SetName( "bar" );
        var actions = db.GetLastPendingActions( actionCount );

        Assertion.All(
                result.TestRefEquals( sut ),
                sut.Name.TestEquals( "bar" ),
                db.Schemas.TryGet( "bar" ).TestRefEquals( sut ),
                db.Schemas.TryGet( db.CommonSchemaName ).TestNull(),
                actions.Select( a => a.Sql ).TestSequence( [ (sql, _) => sql.TestSatisfySql( "CREATE SCHEMA `bar`;" ) ] ) )
            .Go();
    }

    [Fact]
    public void SetName_ShouldUpdateName_WhenNameChangesToCommon()
    {
        var db = MySqlDatabaseBuilderMock.Create();
        db.Schemas.Default.SetName( "foo" );
        var sut = db.Schemas.Create( "bar" );

        var actionCount = db.GetPendingActionCount();
        var result = sut.SetName( db.CommonSchemaName );
        var actions = db.GetLastPendingActions( actionCount );

        Assertion.All(
                result.TestRefEquals( sut ),
                sut.Name.TestEquals( db.CommonSchemaName ),
                db.Schemas.TryGet( db.CommonSchemaName ).TestRefEquals( sut ),
                db.Schemas.TryGet( "bar" ).TestNull(),
                actions.Select( a => a.Sql ).TestSequence( [ (sql, _) => sql.TestSatisfySql( "DROP SCHEMA `bar`;" ) ] ) )
            .Go();
    }

    [Fact]
    public void SetName_ShouldUpdateName_WhenNameChangesAndSchemaHasObjects()
    {
        var db = MySqlDatabaseBuilderMock.Create();
        var sut = db.Schemas.Create( "foo" );

        var t1 = sut.Objects.CreateTable( "T1" );
        var c1 = t1.Columns.Create( "C1" );
        var c2 = t1.Columns.Create( "C2" ).MarkAsNullable();
        t1.Constraints.SetPrimaryKey( c1.Asc() );
        t1.Constraints.CreateIndex( c2.Asc() );
        var recordSet1 = t1.Node;
        _ = t1.Info;

        var t2 = sut.Objects.CreateTable( "T2" );
        var c3 = t2.Columns.Create( "C3" );
        t2.Constraints.SetPrimaryKey( c3.Asc() );
        t2.Constraints.CreateCheck( SqlNode.True() );
        var recordSet2 = t2.Node;
        _ = t2.Info;

        var v1 = sut.Objects.CreateView( "V1", SqlNode.RawQuery( "SELECT * FROM bar" ) );
        var recordSet3 = v1.Node;
        _ = v1.Info;

        var v2 = sut.Objects.CreateView( "V2", SqlNode.RawQuery( "SELECT * FROM qux" ) );
        var recordSet4 = v2.Node;
        _ = v2.Info;

        var actionCount = db.GetPendingActionCount();
        var result = sut.SetName( "bar" );
        var actions = db.GetLastPendingActions( actionCount );

        Assertion.All(
                result.TestRefEquals( sut ),
                sut.Name.TestEquals( "bar" ),
                db.Schemas.TryGet( "bar" ).TestRefEquals( sut ),
                db.Schemas.TryGet( "foo" ).TestNull(),
                t1.Info.TestEquals( SqlRecordSetInfo.Create( "bar", "T1" ) ),
                recordSet1.Info.TestEquals( t1.Info ),
                t2.Info.TestEquals( SqlRecordSetInfo.Create( "bar", "T2" ) ),
                recordSet2.Info.TestEquals( t2.Info ),
                v1.Info.TestEquals( SqlRecordSetInfo.Create( "bar", "V1" ) ),
                recordSet3.Info.TestEquals( v1.Info ),
                v2.Info.TestEquals( SqlRecordSetInfo.Create( "bar", "V2" ) ),
                recordSet4.Info.TestEquals( v2.Info ),
                actions.Select( a => a.Sql )
                    .TestSequence(
                    [
                        (sql, _) => sql.TestSatisfySql( "CREATE SCHEMA `bar`;" ),
                        (sql, _) => sql.TestSatisfySql( "ALTER TABLE `foo`.`T1` RENAME TO `bar`.`T1`;" ),
                        (sql, _) => sql.TestSatisfySql( "ALTER TABLE `foo`.`T2` RENAME TO `bar`.`T2`;" ),
                        (sql, _) => sql.TestSatisfySql(
                            "DROP VIEW `foo`.`V1`;",
                            """
                            CREATE VIEW `bar`.`V1` AS
                                SELECT * FROM bar;
                            """ ),
                        (sql, _) => sql.TestSatisfySql(
                            "DROP VIEW `foo`.`V2`;",
                            """
                            CREATE VIEW `bar`.`V2` AS
                                SELECT * FROM qux;
                            """ ),
                        (sql, _) => sql.TestSatisfySql( "DROP SCHEMA `foo`;" )
                    ] ) )
            .Go();
    }

    [Fact]
    public void SetName_ShouldUpdateName_WhenNewNameIsDifferentFromOldNameAndAnotherSchemaContainsReferencingView()
    {
        var db = MySqlDatabaseBuilderMock.Create();
        var sut = db.Schemas.Create( "foo" );
        var other = db.Schemas.Default;

        var table = sut.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C" ).Asc() );
        var recordSet1 = table.Node;
        _ = table.Info;

        var view = sut.Objects.CreateView( "V", SqlNode.RawQuery( "SELECT * FROM qux" ) );
        var recordSet2 = view.Node;
        _ = view.Info;

        other.Objects.CreateView( "V", view.Node.Join( table.Node.InnerOn( SqlNode.True() ) ).Select( d => new[] { d.GetAll() } ) );

        var actionCount = db.GetPendingActionCount();
        var result = sut.SetName( "bar" );
        var actions = db.GetLastPendingActions( actionCount );

        Assertion.All(
                result.TestRefEquals( sut ),
                sut.Name.TestEquals( "bar" ),
                db.Schemas.TryGet( "bar" ).TestRefEquals( sut ),
                db.Schemas.TryGet( "foo" ).TestNull(),
                table.Info.TestEquals( SqlRecordSetInfo.Create( "bar", "T" ) ),
                recordSet1.Info.TestEquals( table.Info ),
                view.Info.TestEquals( SqlRecordSetInfo.Create( "bar", "V" ) ),
                recordSet2.Info.TestEquals( view.Info ),
                actions.Select( a => a.Sql )
                    .TestSequence(
                    [
                        (sql, _) => sql.TestSatisfySql( "CREATE SCHEMA `bar`;" ),
                        (sql, _) => sql.TestSatisfySql( "ALTER TABLE `foo`.`T` RENAME TO `bar`.`T`;" ),
                        (sql, _) => sql.TestSatisfySql(
                            "DROP VIEW `foo`.`V`;",
                            """
                            CREATE VIEW `bar`.`V` AS
                                SELECT * FROM qux;
                            """ ),
                        (sql, _) => sql.TestSatisfySql(
                            "DROP VIEW `common`.`V`;",
                            """
                            CREATE VIEW `common`.`V` AS
                                SELECT
                                  *
                                FROM `bar`.`V`
                                INNER JOIN `bar`.`T` ON TRUE;
                            """ ),
                        (sql, _) => sql.TestSatisfySql( "DROP SCHEMA `foo`;" )
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
        var db = MySqlDatabaseBuilderMock.Create();
        var sut = db.Schemas.Create( "foo" );

        var action = Lambda.Of( () => sut.SetName( name ) );

        action.Test( exc => exc.TestType()
                .Exact<SqlObjectBuilderException>( e => Assertion.All(
                    e.Dialect.TestEquals( MySqlDialect.Instance ),
                    e.Errors.Count.TestEquals( 1 ) ) ) )
            .Go();
    }

    [Fact]
    public void SetName_ShouldThrowSqlObjectBuilderException_WhenSchemaIsRemoved()
    {
        var db = MySqlDatabaseBuilderMock.Create();
        var sut = db.Schemas.Create( "foo" );
        sut.Remove();

        var action = Lambda.Of( () => sut.SetName( "bar" ) );

        action.Test( exc => exc.TestType()
                .Exact<SqlObjectBuilderException>( e => Assertion.All(
                    e.Dialect.TestEquals( MySqlDialect.Instance ),
                    e.Errors.Count.TestEquals( 1 ) ) ) )
            .Go();
    }

    [Fact]
    public void SetName_ShouldThrowSqlObjectBuilderException_WhenNewNameAlreadyExistsInSchemas()
    {
        var db = MySqlDatabaseBuilderMock.Create();
        var sut = db.Schemas.Create( "foo" );
        var other = db.Schemas.Create( "bar" );

        var action = Lambda.Of( () => sut.SetName( other.Name ) );

        action.Test( exc => exc.TestType()
                .Exact<SqlObjectBuilderException>( e => Assertion.All(
                    e.Dialect.TestEquals( MySqlDialect.Instance ),
                    e.Errors.Count.TestEquals( 1 ) ) ) )
            .Go();
    }

    [Fact]
    public void Remove_ShouldRemoveSchemaAndAddStatement_WhenSchemaIsNotCommon()
    {
        var db = MySqlDatabaseBuilderMock.Create();
        var sut = db.Schemas.Create( "foo" );

        var actionCount = db.GetPendingActionCount();
        sut.Remove();
        var actions = db.GetLastPendingActions( actionCount );

        Assertion.All(
                db.Schemas.TryGet( sut.Name ).TestNull(),
                sut.IsRemoved.TestTrue(),
                actions.Select( a => a.Sql ).TestSequence( [ (sql, _) => sql.TestSatisfySql( "DROP SCHEMA `foo`;" ) ] ) )
            .Go();
    }

    [Fact]
    public void Remove_ShouldRemoveSchemaAndAllOfItsObjects_WhenSchemaHasTablesAndViewsWithoutReferencesFromOtherSchemas()
    {
        var db = MySqlDatabaseBuilderMock.Create();
        var sut = db.Schemas.Create( "foo" );

        var t1 = sut.Objects.CreateTable( "T1" );
        var c1 = t1.Columns.Create( "C1" );
        var c2 = t1.Columns.Create( "C2" ).MarkAsNullable();
        var pk1 = t1.Constraints.SetPrimaryKey( c1.Asc() );
        var ix1 = t1.Constraints.CreateIndex( c2.Asc() );
        var fk1 = t1.Constraints.CreateForeignKey( ix1, pk1.Index );
        var chk1 = t1.Constraints.CreateCheck( c1.Node != null );

        var t2 = sut.Objects.CreateTable( "T2" );
        var c3 = t2.Columns.Create( "C3" );
        var c4 = t2.Columns.Create( "C4" );
        var pk2 = t2.Constraints.SetPrimaryKey( c3.Asc() );
        var ix2 = t2.Constraints.CreateIndex( c4.Asc() );
        var fk2 = t2.Constraints.CreateForeignKey( ix2, pk1.Index );

        var t3 = sut.Objects.CreateTable( "T3" );
        var c5 = t3.Columns.Create( "C5" );
        var c6 = t3.Columns.Create( "C6" );
        var pk3 = t3.Constraints.SetPrimaryKey( c5.Asc() );
        var fk3 = t3.Constraints.CreateForeignKey( pk3.Index, pk2.Index );
        var chk2 = t3.Constraints.CreateCheck( c5.Node > SqlNode.Literal( 0 ) );

        var t4 = sut.Objects.CreateTable( "T4" );
        var c7 = t4.Columns.Create( "C7" );
        var pk4 = t4.Constraints.SetPrimaryKey( c7.Asc() );
        var fk4 = t4.Constraints.CreateForeignKey( pk4.Index, pk3.Index );

        var ix3 = t3.Constraints.CreateIndex( c6.Asc() );
        var fk5 = t3.Constraints.CreateForeignKey( ix3, pk4.Index );

        var v1 = sut.Objects.CreateView( "V1", t2.Node.Join( t3.Node.InnerOn( SqlNode.True() ) ).Select( s => new[] { s.GetAll() } ) );
        var v2 = sut.Objects.CreateView( "V2", t1.Node.Join( v1.Node.InnerOn( SqlNode.True() ) ).Select( s => new[] { s.GetAll() } ) );

        var actionCount = db.GetPendingActionCount();
        sut.Remove();
        var actions = db.GetLastPendingActions( actionCount );

        Assertion.All(
                db.Schemas.TryGet( sut.Name ).TestNull(),
                sut.IsRemoved.TestTrue(),
                t1.IsRemoved.TestTrue(),
                t2.IsRemoved.TestTrue(),
                t3.IsRemoved.TestTrue(),
                t4.IsRemoved.TestTrue(),
                c1.IsRemoved.TestTrue(),
                c2.IsRemoved.TestTrue(),
                c3.IsRemoved.TestTrue(),
                c4.IsRemoved.TestTrue(),
                c5.IsRemoved.TestTrue(),
                c6.IsRemoved.TestTrue(),
                c7.IsRemoved.TestTrue(),
                pk1.IsRemoved.TestTrue(),
                pk2.IsRemoved.TestTrue(),
                pk3.IsRemoved.TestTrue(),
                pk4.IsRemoved.TestTrue(),
                pk1.Index.IsRemoved.TestTrue(),
                pk2.Index.IsRemoved.TestTrue(),
                pk3.Index.IsRemoved.TestTrue(),
                pk4.Index.IsRemoved.TestTrue(),
                ix1.IsRemoved.TestTrue(),
                ix2.IsRemoved.TestTrue(),
                ix3.IsRemoved.TestTrue(),
                fk1.IsRemoved.TestTrue(),
                fk2.IsRemoved.TestTrue(),
                fk3.IsRemoved.TestTrue(),
                fk4.IsRemoved.TestTrue(),
                fk5.IsRemoved.TestTrue(),
                v1.IsRemoved.TestTrue(),
                v2.IsRemoved.TestTrue(),
                chk1.IsRemoved.TestTrue(),
                chk2.IsRemoved.TestTrue(),
                sut.Objects.TestEmpty(),
                actions.Select( a => a.Sql ).TestSequence( [ (sql, _) => sql.TestSatisfySql( "DROP SCHEMA `foo`;" ) ] ) )
            .Go();
    }

    [Fact]
    public void Remove_ShouldNotAddAnyStatements_WhenSchemaIsRemoved()
    {
        var db = MySqlDatabaseBuilderMock.Create();
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
        var db = MySqlDatabaseBuilderMock.Create();
        var sut = db.Schemas.Default.SetName( "foo" );

        var action = Lambda.Of( () => sut.Remove() );

        action.Test( exc => exc.TestType()
                .Exact<SqlObjectBuilderException>( e => Assertion.All(
                    e.Dialect.TestEquals( MySqlDialect.Instance ),
                    e.Errors.Count.TestEquals( 1 ) ) ) )
            .Go();
    }

    [Fact]
    public void Remove_ShouldThrowSqlObjectBuilderException_WhenSchemaIsCommon()
    {
        var db = MySqlDatabaseBuilderMock.Create();
        db.Schemas.Default.SetName( "foo" );
        var sut = db.Schemas.Create( db.CommonSchemaName );

        var action = Lambda.Of( () => sut.Remove() );

        action.Test( exc => exc.TestType()
                .Exact<SqlObjectBuilderException>( e => Assertion.All(
                    e.Dialect.TestEquals( MySqlDialect.Instance ),
                    e.Errors.Count.TestEquals( 1 ) ) ) )
            .Go();
    }

    [Fact]
    public void Remove_ShouldThrowSqlObjectBuilderException_WhenSchemaIsReferencedByForeignKeyFromAnotherSchema()
    {
        var db = MySqlDatabaseBuilderMock.Create();
        var sut = db.Schemas.Create( "foo" );
        var table = sut.Objects.CreateTable( "T1" );
        var pk = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );

        var otherTable = db.Schemas.Default.Objects.CreateTable( "T2" );
        var otherPk = otherTable.Constraints.SetPrimaryKey( otherTable.Columns.Create( "C2" ).Asc() );
        otherTable.Constraints.CreateForeignKey( otherPk.Index, pk.Index );

        var action = Lambda.Of( () => sut.Remove() );

        action.Test( exc => exc.TestType()
                .Exact<SqlObjectBuilderException>( e => Assertion.All(
                    e.Dialect.TestEquals( MySqlDialect.Instance ),
                    e.Errors.Count.TestEquals( 1 ) ) ) )
            .Go();
    }

    [Fact]
    public void Remove_ShouldThrowSqlObjectBuilderException_WhenSchemaIsReferencedByViewFromAnotherSchema()
    {
        var db = MySqlDatabaseBuilderMock.Create();
        var sut = db.Schemas.Create( "foo" );
        var table = sut.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C" ).Asc() );

        db.Schemas.Default.Objects.CreateView( "V", table.Node.ToDataSource().Select( s => new[] { s.GetAll() } ) );

        var action = Lambda.Of( () => sut.Remove() );

        action.Test( exc => exc.TestType()
                .Exact<SqlObjectBuilderException>( e => Assertion.All(
                    e.Dialect.TestEquals( MySqlDialect.Instance ),
                    e.Errors.Count.TestEquals( 1 ) ) ) )
            .Go();
    }

    [Fact]
    public void ForMySql_ShouldInvokeAction_WhenSchemaIsMySql()
    {
        var action = Substitute.For<Action<MySqlSchemaBuilder>>();
        var sut = MySqlDatabaseBuilderMock.Create().Schemas.Default;

        var result = sut.ForMySql( action );

        Assertion.All(
                result.TestRefEquals( sut ),
                action.CallAt( 0 ).Arguments.TestSequence( [ sut ] ) )
            .Go();
    }

    [Fact]
    public void ForMySql_ShouldNotInvokeAction_WhenSchemaIsNotMySql()
    {
        var action = Substitute.For<Action<MySqlSchemaBuilder>>();
        var sut = Substitute.For<ISqlSchemaBuilder>();

        var result = sut.ForMySql( action );

        Assertion.All(
                result.TestRefEquals( sut ),
                action.CallCount().TestEquals( 0 ) )
            .Go();
    }
}
