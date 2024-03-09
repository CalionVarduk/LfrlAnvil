using System.Linq;
using LfrlAnvil.Functional;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.Sqlite.Extensions;
using LfrlAnvil.Sqlite.Objects.Builders;
using LfrlAnvil.Sqlite.Tests.Helpers;
using LfrlAnvil.TestExtensions.FluentAssertions;
using LfrlAnvil.TestExtensions.Sql;
using LfrlAnvil.TestExtensions.Sql.FluentAssertions;

namespace LfrlAnvil.Sqlite.Tests.ObjectsTests.BuildersTests;

public partial class SqliteSchemaBuilderTests : TestsBase
{
    [Fact]
    public void ToString_ShouldReturnCorrectResult()
    {
        var db = SqliteDatabaseBuilderMock.Create();
        var sut = db.Schemas.Create( "foo" );

        var result = sut.ToString();

        result.Should().Be( "[Schema] foo" );
    }

    [Fact]
    public void Creation_ShouldNotAddAnyStatements()
    {
        var db = SqliteDatabaseBuilderMock.Create();

        var actionCount = db.GetPendingActionCount();
        var sut = db.Schemas.Create( "foo" );
        var actions = db.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            db.Schemas.TryGet( sut.Name ).Should().BeSameAs( sut );
            sut.Name.Should().Be( "foo" );
            actions.Should().BeEmpty();
        }
    }

    [Fact]
    public void SetName_ShouldDoNothing_WhenNewNameEqualsOldName()
    {
        var db = SqliteDatabaseBuilderMock.Create();
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
    public void SetName_ShouldUpdateName_WhenNewNameIsDifferentFromOldName()
    {
        var db = SqliteDatabaseBuilderMock.Create();
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
            db.Changes.ModifiedTables.Should().BeEmpty();
            actions.Should().BeEmpty();
        }
    }

    [Fact]
    public void SetName_ShouldUpdateName_WhenNewNameIsDifferentFromOldNameAndNewNameIsEmpty()
    {
        var db = SqliteDatabaseBuilderMock.Create();
        var sut = db.Schemas.Create( "foo" );
        db.Schemas.Default.SetName( "bar" );

        var actionCount = db.GetPendingActionCount();
        var result = sut.SetName( string.Empty );
        var actions = db.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.Name.Should().BeEmpty();
            db.Schemas.TryGet( string.Empty ).Should().BeSameAs( sut );
            db.Schemas.TryGet( "foo" ).Should().BeNull();
            db.Changes.ModifiedTables.Should().BeEmpty();
            actions.Should().BeEmpty();
        }
    }

    [Fact]
    public void SetName_ShouldUpdateName_WhenNewNameIsDifferentFromOldNameAndSchemaHasObjects()
    {
        var db = SqliteDatabaseBuilderMock.Create();
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
        db.Changes.ClearModifiedTables();
        var result = sut.SetName( "bar" );
        var actions = db.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.Name.Should().Be( "bar" );
            db.Schemas.TryGet( "bar" ).Should().BeSameAs( sut );
            db.Schemas.TryGet( "foo" ).Should().BeNull();
            db.Changes.ModifiedTables.Should().BeEquivalentTo( t1, t2 );

            t1.Info.Should().Be( SqlRecordSetInfo.Create( "bar", "T1" ) );
            recordSet1.Info.Should().Be( t1.Info );
            t2.Info.Should().Be( SqlRecordSetInfo.Create( "bar", "T2" ) );
            recordSet2.Info.Should().Be( t2.Info );
            v1.Info.Should().Be( SqlRecordSetInfo.Create( "bar", "V1" ) );
            recordSet3.Info.Should().Be( v1.Info );
            v2.Info.Should().Be( SqlRecordSetInfo.Create( "bar", "V2" ) );
            recordSet4.Info.Should().Be( v2.Info );

            actions.Should().HaveCount( 6 );

            actions.ElementAtOrDefault( 0 ).Sql.Should().SatisfySql( "ALTER TABLE \"foo_T1\" RENAME TO \"bar_T1\";" );
            actions.ElementAtOrDefault( 1 ).Sql.Should().SatisfySql( "ALTER TABLE \"foo_T2\" RENAME TO \"bar_T2\";" );

            actions.ElementAtOrDefault( 2 )
                .Sql.Should()
                .SatisfySql(
                    "DROP VIEW \"foo_V1\";",
                    @"CREATE VIEW ""bar_V1"" AS
                    SELECT * FROM bar;" );

            actions.ElementAtOrDefault( 3 )
                .Sql.Should()
                .SatisfySql(
                    "DROP VIEW \"foo_V2\";",
                    @"CREATE VIEW ""bar_V2"" AS
                    SELECT * FROM qux;" );

            actions.ElementAtOrDefault( 4 )
                .Sql.Should()
                .SatisfySql(
                    "DROP INDEX \"foo_IX_T1_C2A\";",
                    @"CREATE TABLE ""__bar_T1__{GUID}__"" (
                      ""C1"" ANY NOT NULL,
                      ""C2"" ANY,
                      CONSTRAINT ""bar_PK_T1"" PRIMARY KEY (""C1"" ASC)
                    ) WITHOUT ROWID;",
                    @"INSERT INTO ""__bar_T1__{GUID}__"" (""C1"", ""C2"")
                    SELECT
                      ""bar_T1"".""C1"",
                      ""bar_T1"".""C2""
                    FROM ""bar_T1"";",
                    "DROP TABLE \"bar_T1\";",
                    "ALTER TABLE \"__bar_T1__{GUID}__\" RENAME TO \"bar_T1\";",
                    "CREATE INDEX \"bar_IX_T1_C2A\" ON \"bar_T1\" (\"C2\" ASC);" );

            actions.ElementAtOrDefault( 5 )
                .Sql.Should()
                .SatisfySql(
                    @"CREATE TABLE ""__bar_T2__{GUID}__"" (
                      ""C3"" ANY NOT NULL,
                      CONSTRAINT ""bar_PK_T2"" PRIMARY KEY (""C3"" ASC),
                      CONSTRAINT ""bar_CHK_T2_{GUID}"" CHECK (TRUE)
                    ) WITHOUT ROWID;",
                    @"INSERT INTO ""__bar_T2__{GUID}__"" (""C3"")
                    SELECT
                      ""bar_T2"".""C3""
                    FROM ""bar_T2"";",
                    "DROP TABLE \"bar_T2\";",
                    "ALTER TABLE \"__bar_T2__{GUID}__\" RENAME TO \"bar_T2\";" );
        }
    }

    [Fact]
    public void SetName_ShouldUpdateName_WhenNewNameIsDifferentFromOldNameAndSchemaHasSelfReferencingTable()
    {
        var db = SqliteDatabaseBuilderMock.Create();
        var sut = db.Schemas.Create( "foo" );

        var table = sut.Objects.CreateTable( "T" );
        var c1 = table.Columns.Create( "C1" );
        var c2 = table.Columns.Create( "C2" ).MarkAsNullable();
        var pk = table.Constraints.SetPrimaryKey( c1.Asc() );
        table.Constraints.CreateForeignKey( table.Constraints.CreateIndex( c2.Asc() ), pk.Index );
        var recordSet = table.Node;
        _ = table.Info;

        var actionCount = db.GetPendingActionCount();
        db.Changes.ClearModifiedTables();
        var result = sut.SetName( "bar" );
        var actions = db.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.Name.Should().Be( "bar" );
            db.Schemas.TryGet( "bar" ).Should().BeSameAs( sut );
            db.Schemas.TryGet( "foo" ).Should().BeNull();
            db.Changes.ModifiedTables.Should().BeEquivalentTo( table );

            table.Info.Should().Be( SqlRecordSetInfo.Create( "bar", "T" ) );
            recordSet.Info.Should().Be( table.Info );

            actions.Should().HaveCount( 2 );

            actions.ElementAtOrDefault( 0 ).Sql.Should().SatisfySql( "ALTER TABLE \"foo_T\" RENAME TO \"bar_T\";" );

            actions.ElementAtOrDefault( 1 )
                .Sql.Should()
                .SatisfySql(
                    "DROP INDEX \"foo_IX_T_C2A\";",
                    @"CREATE TABLE ""__bar_T__{GUID}__"" (
                      ""C1"" ANY NOT NULL,
                      ""C2"" ANY,
                      CONSTRAINT ""bar_PK_T"" PRIMARY KEY (""C1"" ASC),
                      CONSTRAINT ""bar_FK_T_C2_REF_T"" FOREIGN KEY (""C2"") REFERENCES ""bar_T"" (""C1"") ON DELETE RESTRICT ON UPDATE RESTRICT
                    ) WITHOUT ROWID;",
                    @"INSERT INTO ""__bar_T__{GUID}__"" (""C1"", ""C2"")
                    SELECT
                      ""bar_T"".""C1"",
                      ""bar_T"".""C2""
                    FROM ""bar_T"";",
                    "DROP TABLE \"bar_T\";",
                    "ALTER TABLE \"__bar_T__{GUID}__\" RENAME TO \"bar_T\";",
                    "CREATE INDEX \"bar_IX_T_C2A\" ON \"bar_T\" (\"C2\" ASC);" );
        }
    }

    [Fact]
    public void SetName_ShouldUpdateName_WhenNewNameIsDifferentFromOldNameAndTableIsReferencedByAnotherTable()
    {
        var db = SqliteDatabaseBuilderMock.Create();
        var sut = db.Schemas.Create( "foo" );

        var t1 = sut.Objects.CreateTable( "T1" );
        var c1 = t1.Columns.Create( "C1" );
        var pk1 = t1.Constraints.SetPrimaryKey( c1.Asc() );
        var recordSet1 = t1.Node;
        _ = t1.Info;

        var t2 = sut.Objects.CreateTable( "T2" );
        var c3 = t2.Columns.Create( "C2" );
        var pk2 = t2.Constraints.SetPrimaryKey( c3.Asc() );
        t2.Constraints.CreateForeignKey( pk2.Index, pk1.Index );
        var recordSet2 = t2.Node;
        _ = t2.Info;

        var actionCount = db.GetPendingActionCount();
        db.Changes.ClearModifiedTables();
        var result = sut.SetName( "bar" );
        var actions = db.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.Name.Should().Be( "bar" );
            db.Schemas.TryGet( "bar" ).Should().BeSameAs( sut );
            db.Schemas.TryGet( "foo" ).Should().BeNull();
            db.Changes.ModifiedTables.Should().BeEquivalentTo( t1, t2 );

            t1.Info.Should().Be( SqlRecordSetInfo.Create( "bar", "T1" ) );
            recordSet1.Info.Should().Be( t1.Info );
            t2.Info.Should().Be( SqlRecordSetInfo.Create( "bar", "T2" ) );
            recordSet2.Info.Should().Be( t2.Info );

            actions.Should().HaveCount( 4 );

            actions.ElementAtOrDefault( 0 ).Sql.Should().SatisfySql( "ALTER TABLE \"foo_T1\" RENAME TO \"bar_T1\";" );
            actions.ElementAtOrDefault( 1 ).Sql.Should().SatisfySql( "ALTER TABLE \"foo_T2\" RENAME TO \"bar_T2\";" );

            actions.ElementAtOrDefault( 2 )
                .Sql.Should()
                .SatisfySql(
                    @"CREATE TABLE ""__bar_T1__{GUID}__"" (
                      ""C1"" ANY NOT NULL,
                      CONSTRAINT ""bar_PK_T1"" PRIMARY KEY (""C1"" ASC)
                    ) WITHOUT ROWID;",
                    @"INSERT INTO ""__bar_T1__{GUID}__"" (""C1"")
                    SELECT
                      ""bar_T1"".""C1""
                    FROM ""bar_T1"";",
                    "DROP TABLE \"bar_T1\";",
                    "ALTER TABLE \"__bar_T1__{GUID}__\" RENAME TO \"bar_T1\";" );

            actions.ElementAtOrDefault( 3 )
                .Sql.Should()
                .SatisfySql(
                    @"CREATE TABLE ""__bar_T2__{GUID}__"" (
                      ""C2"" ANY NOT NULL,
                      CONSTRAINT ""bar_PK_T2"" PRIMARY KEY (""C2"" ASC),
                      CONSTRAINT ""bar_FK_T2_C2_REF_T1"" FOREIGN KEY (""C2"") REFERENCES ""bar_T1"" (""C1"") ON DELETE RESTRICT ON UPDATE RESTRICT
                    ) WITHOUT ROWID;",
                    @"INSERT INTO ""__bar_T2__{GUID}__"" (""C2"")
                    SELECT
                      ""bar_T2"".""C2""
                    FROM ""bar_T2"";",
                    "DROP TABLE \"bar_T2\";",
                    "ALTER TABLE \"__bar_T2__{GUID}__\" RENAME TO \"bar_T2\";" );
        }
    }

    [Fact]
    public void SetName_ShouldUpdateName_WhenNewNameIsDifferentFromOldNameAndTableIsReferencedByTableFromAnotherSchema()
    {
        var db = SqliteDatabaseBuilderMock.Create();
        var sut = db.Schemas.Create( "foo" );
        var other = db.Schemas.Default;

        var table = sut.Objects.CreateTable( "T" );
        var c1 = table.Columns.Create( "C" );
        var pk1 = table.Constraints.SetPrimaryKey( c1.Asc() );
        var recordSet1 = table.Node;
        _ = table.Info;

        var t2 = other.Objects.CreateTable( "T" );
        var c3 = t2.Columns.Create( "C" );
        var pk2 = t2.Constraints.SetPrimaryKey( c3.Asc() );
        t2.Constraints.CreateForeignKey( pk2.Index, pk1.Index );

        var actionCount = db.GetPendingActionCount();
        db.Changes.ClearModifiedTables();
        var result = sut.SetName( "bar" );
        var actions = db.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.Name.Should().Be( "bar" );
            db.Schemas.TryGet( "bar" ).Should().BeSameAs( sut );
            db.Schemas.TryGet( "foo" ).Should().BeNull();
            db.Changes.ModifiedTables.Should().BeEquivalentTo( table, t2 );

            table.Info.Should().Be( SqlRecordSetInfo.Create( "bar", "T" ) );
            recordSet1.Info.Should().Be( table.Info );

            actions.Should().HaveCount( 3 );

            actions.ElementAtOrDefault( 0 ).Sql.Should().SatisfySql( "ALTER TABLE \"foo_T\" RENAME TO \"bar_T\";" );

            actions.ElementAtOrDefault( 1 )
                .Sql.Should()
                .SatisfySql(
                    @"CREATE TABLE ""__bar_T__{GUID}__"" (
                      ""C"" ANY NOT NULL,
                      CONSTRAINT ""bar_PK_T"" PRIMARY KEY (""C"" ASC)
                    ) WITHOUT ROWID;",
                    @"INSERT INTO ""__bar_T__{GUID}__"" (""C"")
                    SELECT
                      ""bar_T"".""C""
                    FROM ""bar_T"";",
                    "DROP TABLE \"bar_T\";",
                    "ALTER TABLE \"__bar_T__{GUID}__\" RENAME TO \"bar_T\";" );

            actions.ElementAtOrDefault( 2 )
                .Sql.Should()
                .SatisfySql(
                    @"CREATE TABLE ""__T__{GUID}__"" (
                      ""C"" ANY NOT NULL,
                      CONSTRAINT ""PK_T"" PRIMARY KEY (""C"" ASC),
                      CONSTRAINT ""FK_T_C_REF_foo_T"" FOREIGN KEY (""C"") REFERENCES ""bar_T"" (""C"") ON DELETE RESTRICT ON UPDATE RESTRICT
                    ) WITHOUT ROWID;",
                    @"INSERT INTO ""__T__{GUID}__"" (""C"")
                    SELECT
                      ""T"".""C""
                    FROM ""T"";",
                    "DROP TABLE \"T\";",
                    "ALTER TABLE \"__T__{GUID}__\" RENAME TO \"T\";" );
        }
    }

    [Fact]
    public void SetName_ShouldUpdateName_WhenNewNameIsDifferentFromOldNameAndSchemaContainsReferencingView()
    {
        var db = SqliteDatabaseBuilderMock.Create();
        var sut = db.Schemas.Create( "foo" );

        var table = sut.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C" ).Asc() );
        var recordSet1 = table.Node;
        _ = table.Info;

        var v1 = sut.Objects.CreateView( "V1", table.Node.ToDataSource().Select( d => new[] { d.GetAll() } ) );
        var recordSet2 = v1.Node;
        _ = v1.Info;

        var v2 = sut.Objects.CreateView( "V2", v1.Node.ToDataSource().Select( d => new[] { d.GetAll() } ) );
        var recordSet3 = v2.Node;
        _ = v2.Info;

        var actionCount = db.GetPendingActionCount();
        db.Changes.ClearModifiedTables();
        var result = sut.SetName( "bar" );
        var actions = db.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.Name.Should().Be( "bar" );
            db.Schemas.TryGet( "bar" ).Should().BeSameAs( sut );
            db.Schemas.TryGet( "foo" ).Should().BeNull();
            db.Changes.ModifiedTables.Should().BeEquivalentTo( table );

            table.Info.Should().Be( SqlRecordSetInfo.Create( "bar", "T" ) );
            recordSet1.Info.Should().Be( table.Info );
            v1.Info.Should().Be( SqlRecordSetInfo.Create( "bar", "V1" ) );
            recordSet2.Info.Should().Be( v1.Info );
            v2.Info.Should().Be( SqlRecordSetInfo.Create( "bar", "V2" ) );
            recordSet3.Info.Should().Be( v2.Info );

            actions.Should().HaveCount( 4 );

            actions.ElementAtOrDefault( 0 ).Sql.Should().SatisfySql( "ALTER TABLE \"foo_T\" RENAME TO \"bar_T\";" );

            actions.ElementAtOrDefault( 1 )
                .Sql.Should()
                .SatisfySql(
                    "DROP VIEW \"foo_V1\";",
                    @"CREATE VIEW ""bar_V1"" AS
                    SELECT
                      *
                    FROM ""bar_T"";" );

            actions.ElementAtOrDefault( 2 )
                .Sql.Should()
                .SatisfySql(
                    "DROP VIEW \"foo_V2\";",
                    @"CREATE VIEW ""bar_V2"" AS
                    SELECT
                      *
                    FROM ""bar_V1"";" );

            actions.ElementAtOrDefault( 3 )
                .Sql.Should()
                .SatisfySql(
                    @"CREATE TABLE ""__bar_T__{GUID}__"" (
                      ""C"" ANY NOT NULL,
                      CONSTRAINT ""bar_PK_T"" PRIMARY KEY (""C"" ASC)
                    ) WITHOUT ROWID;",
                    @"INSERT INTO ""__bar_T__{GUID}__"" (""C"")
                    SELECT
                      ""bar_T"".""C""
                    FROM ""bar_T"";",
                    "DROP TABLE \"bar_T\";",
                    "ALTER TABLE \"__bar_T__{GUID}__\" RENAME TO \"bar_T\";" );
        }
    }

    [Fact]
    public void SetName_ShouldUpdateName_WhenNewNameIsDifferentFromOldNameAndAnotherSchemaContainsReferencingView()
    {
        var db = SqliteDatabaseBuilderMock.Create();
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
        db.Changes.ClearModifiedTables();
        var result = sut.SetName( "bar" );
        var actions = db.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.Name.Should().Be( "bar" );
            db.Schemas.TryGet( "bar" ).Should().BeSameAs( sut );
            db.Schemas.TryGet( "foo" ).Should().BeNull();
            db.Changes.ModifiedTables.Should().BeEquivalentTo( table );

            table.Info.Should().Be( SqlRecordSetInfo.Create( "bar", "T" ) );
            recordSet1.Info.Should().Be( table.Info );
            view.Info.Should().Be( SqlRecordSetInfo.Create( "bar", "V" ) );
            recordSet2.Info.Should().Be( view.Info );

            actions.Should().HaveCount( 4 );

            actions.ElementAtOrDefault( 0 ).Sql.Should().SatisfySql( "ALTER TABLE \"foo_T\" RENAME TO \"bar_T\";" );

            actions.ElementAtOrDefault( 1 )
                .Sql.Should()
                .SatisfySql(
                    "DROP VIEW \"foo_V\";",
                    @"CREATE VIEW ""bar_V"" AS
                    SELECT * FROM qux;" );

            actions.ElementAtOrDefault( 2 )
                .Sql.Should()
                .SatisfySql(
                    @"CREATE TABLE ""__bar_T__{GUID}__"" (
                      ""C"" ANY NOT NULL,
                      CONSTRAINT ""bar_PK_T"" PRIMARY KEY (""C"" ASC)
                    ) WITHOUT ROWID;",
                    @"INSERT INTO ""__bar_T__{GUID}__"" (""C"")
                    SELECT
                      ""bar_T"".""C""
                    FROM ""bar_T"";",
                    "DROP TABLE \"bar_T\";",
                    "ALTER TABLE \"__bar_T__{GUID}__\" RENAME TO \"bar_T\";" );

            actions.ElementAtOrDefault( 3 )
                .Sql.Should()
                .SatisfySql(
                    "DROP VIEW \"V\";",
                    @"CREATE VIEW ""V"" AS
                    SELECT
                      *
                    FROM ""bar_V""
                    INNER JOIN ""bar_T"" ON TRUE;" );
        }
    }

    [Theory]
    [InlineData( " " )]
    [InlineData( "\"" )]
    [InlineData( "'" )]
    [InlineData( "f\"oo" )]
    public void SetName_ShouldThrowSqlObjectBuilderException_WhenNameIsInvalid(string name)
    {
        var db = SqliteDatabaseBuilderMock.Create();
        var sut = db.Schemas.Create( "foo" );

        var action = Lambda.Of( () => sut.SetName( name ) );

        action.Should()
            .ThrowExactly<SqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == SqliteDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void SetName_ShouldThrowSqlObjectBuilderException_WhenSchemaIsRemoved()
    {
        var db = SqliteDatabaseBuilderMock.Create();
        var sut = db.Schemas.Create( "foo" );
        sut.Remove();

        var action = Lambda.Of( () => sut.SetName( "bar" ) );

        action.Should()
            .ThrowExactly<SqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == SqliteDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void SetName_ShouldThrowSqlObjectBuilderException_WhenNewNameAlreadyExistsInSchemas()
    {
        var db = SqliteDatabaseBuilderMock.Create();
        var sut = db.Schemas.Create( "foo" );
        var other = db.Schemas.Create( "bar" );

        var action = Lambda.Of( () => sut.SetName( other.Name ) );

        action.Should()
            .ThrowExactly<SqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == SqliteDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void Remove_ShouldRemoveSchema_WhenSchemaDoesNotHaveAnyObjects()
    {
        var db = SqliteDatabaseBuilderMock.Create();
        var sut = db.Schemas.Create( "foo" );

        var actionCount = db.GetPendingActionCount();
        sut.Remove();
        var actions = db.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            db.Schemas.TryGet( sut.Name ).Should().BeNull();
            sut.IsRemoved.Should().BeTrue();
            actions.Should().BeEmpty();
        }
    }

    [Fact]
    public void Remove_ShouldRemoveSchemaAndAllOfItsObjects_WhenSchemaHasTablesAndViewsWithoutReferencesFromOtherSchemas()
    {
        var db = SqliteDatabaseBuilderMock.Create();
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

        using ( new AssertionScope() )
        {
            db.Schemas.TryGet( sut.Name ).Should().BeNull();
            db.Changes.ModifiedTables.Should().BeEmpty();
            sut.IsRemoved.Should().BeTrue();
            t1.IsRemoved.Should().BeTrue();
            t2.IsRemoved.Should().BeTrue();
            t3.IsRemoved.Should().BeTrue();
            t4.IsRemoved.Should().BeTrue();
            c1.IsRemoved.Should().BeTrue();
            c2.IsRemoved.Should().BeTrue();
            c3.IsRemoved.Should().BeTrue();
            c4.IsRemoved.Should().BeTrue();
            c5.IsRemoved.Should().BeTrue();
            c6.IsRemoved.Should().BeTrue();
            c7.IsRemoved.Should().BeTrue();
            pk1.IsRemoved.Should().BeTrue();
            pk2.IsRemoved.Should().BeTrue();
            pk3.IsRemoved.Should().BeTrue();
            pk4.IsRemoved.Should().BeTrue();
            pk1.Index.IsRemoved.Should().BeTrue();
            pk2.Index.IsRemoved.Should().BeTrue();
            pk3.Index.IsRemoved.Should().BeTrue();
            pk4.Index.IsRemoved.Should().BeTrue();
            ix1.IsRemoved.Should().BeTrue();
            ix2.IsRemoved.Should().BeTrue();
            ix3.IsRemoved.Should().BeTrue();
            fk1.IsRemoved.Should().BeTrue();
            fk2.IsRemoved.Should().BeTrue();
            fk3.IsRemoved.Should().BeTrue();
            fk4.IsRemoved.Should().BeTrue();
            fk5.IsRemoved.Should().BeTrue();
            v1.IsRemoved.Should().BeTrue();
            v2.IsRemoved.Should().BeTrue();
            chk1.IsRemoved.Should().BeTrue();
            chk2.IsRemoved.Should().BeTrue();
            sut.Objects.Should().BeEmpty();

            actions.Should().HaveCount( 6 );
            actions.ElementAtOrDefault( 0 ).Sql.Should().SatisfySql( "DROP TABLE \"foo_T1\";" );
            actions.ElementAtOrDefault( 1 ).Sql.Should().SatisfySql( "DROP TABLE \"foo_T2\";" );
            actions.ElementAtOrDefault( 2 ).Sql.Should().SatisfySql( "DROP TABLE \"foo_T3\";" );
            actions.ElementAtOrDefault( 3 ).Sql.Should().SatisfySql( "DROP TABLE \"foo_T4\";" );
            actions.ElementAtOrDefault( 4 ).Sql.Should().SatisfySql( "DROP VIEW \"foo_V1\";" );
            actions.ElementAtOrDefault( 5 ).Sql.Should().SatisfySql( "DROP VIEW \"foo_V2\";" );
        }
    }

    [Fact]
    public void Remove_ShouldNotAddAnyStatements_WhenSchemaIsRemoved()
    {
        var db = SqliteDatabaseBuilderMock.Create();
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
        var db = SqliteDatabaseBuilderMock.Create();
        var sut = db.Schemas.Default;

        var action = Lambda.Of( () => sut.Remove() );

        action.Should()
            .ThrowExactly<SqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == SqliteDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void Remove_ShouldThrowSqlObjectBuilderException_WhenSchemaIsReferencedByForeignKeyFromAnotherSchema()
    {
        var db = SqliteDatabaseBuilderMock.Create();
        var sut = db.Schemas.Create( "foo" );
        var table = sut.Objects.CreateTable( "T1" );
        var pk = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );

        var otherTable = db.Schemas.Default.Objects.CreateTable( "T2" );
        var otherPk = otherTable.Constraints.SetPrimaryKey( otherTable.Columns.Create( "C2" ).Asc() );
        otherTable.Constraints.CreateForeignKey( otherPk.Index, pk.Index );

        var action = Lambda.Of( () => sut.Remove() );

        action.Should()
            .ThrowExactly<SqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == SqliteDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void Remove_ShouldThrowSqlObjectBuilderException_WhenSchemaIsReferencedByViewFromAnotherSchema()
    {
        var db = SqliteDatabaseBuilderMock.Create();
        var sut = db.Schemas.Create( "foo" );
        var table = sut.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C" ).Asc() );

        db.Schemas.Default.Objects.CreateView( "V", table.Node.ToDataSource().Select( s => new[] { s.GetAll() } ) );

        var action = Lambda.Of( () => sut.Remove() );

        action.Should()
            .ThrowExactly<SqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == SqliteDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void ForSqlite_ShouldInvokeAction_WhenSchemaIsSqlite()
    {
        var action = Substitute.For<Action<SqliteSchemaBuilder>>();
        var sut = SqliteDatabaseBuilderMock.Create().Schemas.Default;

        var result = sut.ForSqlite( action );

        result.Should().BeSameAs( sut );
        action.Verify().CallAt( 0 ).Exists().And.Arguments.Should().BeSequentiallyEqualTo( sut );
    }

    [Fact]
    public void ForSqlite_ShouldNotInvokeAction_WhenSchemaIsNotSqlite()
    {
        var action = Substitute.For<Action<SqliteSchemaBuilder>>();
        var sut = Substitute.For<ISqlSchemaBuilder>();

        var result = sut.ForSqlite( action );

        result.Should().BeSameAs( sut );
        action.Verify().CallCount.Should().Be( 0 );
    }
}
