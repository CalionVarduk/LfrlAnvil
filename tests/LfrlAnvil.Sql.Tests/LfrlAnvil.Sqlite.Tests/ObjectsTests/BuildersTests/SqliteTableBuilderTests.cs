using System.Linq;
using LfrlAnvil.Functional;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.Sqlite.Exceptions;
using LfrlAnvil.Sqlite.Extensions;
using LfrlAnvil.Sqlite.Objects.Builders;
using LfrlAnvil.Sqlite.Tests.Helpers;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.Sqlite.Tests.ObjectsTests.BuildersTests;

public partial class SqliteTableBuilderTests : TestsBase
{
    [Fact]
    public void Creation_ShouldPrepareCorrectStatement()
    {
        var db = SqliteDatabaseBuilderMock.Create();
        var sut = db.Schemas.Create( "foo" ).Objects.CreateTable( "T" );
        var ix1 = sut.Constraints.CreateIndex( sut.Columns.Create( "C1" ).Asc() );
        var ix2 = sut.Constraints.SetPrimaryKey( sut.Columns.Create( "C2" ).Asc() ).Index;
        sut.Constraints.CreateIndex( sut.Columns.Create( "C3" ).Asc(), sut.Columns.Create( "C4" ).Desc() );
        sut.Constraints.CreateForeignKey( ix1, ix2 );
        sut.Constraints.CreateCheck( sut.Node["C1"] > SqlNode.Literal( 0 ) );

        var statements = db.Changes.GetPendingActions().ToArray();

        using ( new AssertionScope() )
        {
            statements.Should().HaveCount( 1 );
            statements.ElementAtOrDefault( 0 )
                .Sql
                .Should()
                .SatisfySql(
                    @"CREATE TABLE ""foo_T"" (
                      ""C1"" ANY NOT NULL,
                      ""C2"" ANY NOT NULL,
                      ""C3"" ANY NOT NULL,
                      ""C4"" ANY NOT NULL,
                      CONSTRAINT ""foo_PK_T"" PRIMARY KEY (""C2"" ASC),
                      CONSTRAINT ""foo_FK_T_C1_REF_T"" FOREIGN KEY (""C1"") REFERENCES ""foo_T"" (""C2"") ON DELETE RESTRICT ON UPDATE RESTRICT,
                      CONSTRAINT ""foo_CHK_T_{GUID}"" CHECK (""C1"" > 0)
                    ) WITHOUT ROWID;",
                    "CREATE INDEX \"foo_IX_T_C1A\" ON \"foo_T\" (\"C1\" ASC);",
                    "CREATE INDEX \"foo_IX_T_C3A_C4D\" ON \"foo_T\" (\"C3\" ASC, \"C4\" DESC);" );
        }
    }

    [Fact]
    public void Creation_FollowedByRemoval_ShouldDoNothing()
    {
        var db = SqliteDatabaseBuilderMock.Create();
        var sut = db.Schemas.Create( "foo" ).Objects.CreateTable( "T" );
        sut.Constraints.SetPrimaryKey( sut.Columns.Create( "C" ).Asc() );
        sut.Remove();

        var statements = db.Changes.GetPendingActions().ToArray();

        statements.Should().BeEmpty();
    }

    [Fact]
    public void Creation_ShouldThrowSqliteObjectBuilderException_WhenTableDoesNotHavePrimaryKeyDuringScriptResolution()
    {
        var db = SqliteDatabaseBuilderMock.Create();
        var sut = db.Schemas.Create( "foo" ).Objects.CreateTable( "T" );
        sut.Columns.Create( "C" );

        var action = Lambda.Of(
            () => { _ = db.Changes.GetPendingActions(); } );

        action.Should()
            .ThrowExactly<SqliteObjectBuilderException>()
            .AndMatch( e => e.Dialect == SqliteDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void ToString_ShouldReturnCorrectResult()
    {
        var db = SqliteDatabaseBuilderMock.Create();
        var sut = db.Schemas.Create( "foo" ).Objects.CreateTable( "bar" );

        var result = sut.ToString();

        result.Should().Be( "[Table] foo_bar" );
    }

    [Fact]
    public void SetName_ShouldDoNothing_WhenNewNameEqualsOldName()
    {
        var name = Fixture.Create<string>();
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Default;
        var sut = schema.Objects.CreateTable( name );

        var result = ((ISqlTableBuilder)sut).SetName( name );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.Name.Should().Be( name );
            schema.Objects.Contains( name ).Should().BeTrue();
        }
    }

    [Fact]
    public void SetName_ShouldDoNothing_WhenNameChangesToNewNameAndThenChangesToOldName()
    {
        var (oldName, newName) = Fixture.CreateDistinctCollection<string>( count: 2 );
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Default;
        var sut = schema.Objects.CreateTable( oldName );
        sut.Constraints.SetPrimaryKey( sut.Columns.Create( "C" ).Asc() );

        var startStatementCount = schema.Database.Changes.GetPendingActions().Length;

        var result = ((ISqlTableBuilder)sut).SetName( newName ).SetName( oldName );
        var statements = schema.Database.Changes.GetPendingActions().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.Name.Should().Be( oldName );
            schema.Objects.Contains( oldName ).Should().BeTrue();
            schema.Objects.Contains( newName ).Should().BeFalse();
            statements.Should().BeEmpty();
        }
    }

    [Fact]
    public void SetName_ShouldUpdateName_WhenNameChangesAndTableDoesNotHaveAnyExternalReferences()
    {
        var (oldName, newName) = ("foo", "bar");
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "s" );
        var sut = schema.Objects.CreateTable( oldName );
        sut.Constraints.SetPrimaryKey( sut.Columns.Create( "C" ).Asc() );
        var recordSet = sut.Node;

        var startStatementCount = schema.Database.Changes.GetPendingActions().Length;

        var result = ((ISqlTableBuilder)sut).SetName( newName );
        var statements = schema.Database.Changes.GetPendingActions().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.Name.Should().Be( newName );
            sut.Info.Should().Be( SqlRecordSetInfo.Create( "s", "bar" ) );
            recordSet.Info.Should().Be( sut.Info );
            schema.Objects.Contains( newName ).Should().BeTrue();
            schema.Objects.Contains( oldName ).Should().BeFalse();
            statements.Should().HaveCount( 1 );
            statements.ElementAtOrDefault( 0 ).Sql.Should().SatisfySql( "ALTER TABLE \"s_foo\" RENAME TO \"s_bar\";" );
        }
    }

    [Fact]
    public void SetName_ShouldUpdateName_WhenNameChangesAndTableHasSelfReference()
    {
        var (oldName, newName) = ("foo", "bar");
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "s" );
        var sut = schema.Objects.CreateTable( oldName );
        var c1 = sut.Columns.Create( "C1" );
        var c2 = sut.Columns.Create( "C2" );
        var pk = sut.Constraints.SetPrimaryKey( c1.Asc() );
        var fk = sut.Constraints.CreateForeignKey( sut.Constraints.CreateIndex( c2.Asc() ), pk.Index );
        sut.Constraints.CreateCheck( c1.Node > SqlNode.Literal( 0 ) );

        var startStatementCount = schema.Database.Changes.GetPendingActions().Length;

        var result = ((ISqlTableBuilder)sut).SetName( newName );
        var statements = schema.Database.Changes.GetPendingActions().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.Name.Should().Be( newName );
            schema.Objects.Contains( newName ).Should().BeTrue();
            schema.Objects.Contains( oldName ).Should().BeFalse();
            fk.IsRemoved.Should().BeFalse();
            statements.Should().HaveCount( 1 );
            statements.ElementAtOrDefault( 0 )
                .Sql
                .Should()
                .SatisfySql(
                    "ALTER TABLE \"s_foo\" RENAME TO \"s_bar\";",
                    "DROP INDEX \"s_IX_foo_C2A\";",
                    @"CREATE TABLE ""__s_bar__{GUID}__"" (
                      ""C1"" ANY NOT NULL,
                      ""C2"" ANY NOT NULL,
                      CONSTRAINT ""s_PK_foo"" PRIMARY KEY (""C1"" ASC),
                      CONSTRAINT ""s_FK_foo_C2_REF_foo"" FOREIGN KEY (""C2"") REFERENCES ""s_bar"" (""C1"") ON DELETE RESTRICT ON UPDATE RESTRICT,
                      CONSTRAINT ""s_CHK_foo_{GUID}"" CHECK (""C1"" > 0)
                    ) WITHOUT ROWID;",
                    @"INSERT INTO ""__s_bar__{GUID}__"" (""C1"", ""C2"")
                    SELECT
                      ""s_bar"".""C1"",
                      ""s_bar"".""C2""
                    FROM ""s_bar"";",
                    "DROP TABLE \"s_bar\";",
                    "ALTER TABLE \"__s_bar__{GUID}__\" RENAME TO \"s_bar\";",
                    "CREATE INDEX \"s_IX_foo_C2A\" ON \"s_bar\" (\"C2\" ASC);" );
        }
    }

    [Fact]
    public void SetName_ShouldUpdateName_WhenNameChangesAndTableHasExternalForeignKeyReferences()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
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

        var startStatementCount = schema.Database.Changes.GetPendingActions().Length;

        var result = ((ISqlTableBuilder)sut).SetName( "U" );
        var statements = schema.Database.Changes.GetPendingActions().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.Name.Should().Be( "U" );
            schema.Objects.Contains( "U" ).Should().BeTrue();
            schema.Objects.Contains( "T1" ).Should().BeFalse();
            fk1.IsRemoved.Should().BeFalse();
            fk2.IsRemoved.Should().BeFalse();
            fk3.IsRemoved.Should().BeFalse();

            statements.Should().HaveCount( 5 );

            statements.ElementAtOrDefault( 0 )
                .Sql
                .Should()
                .SatisfySql(
                    @"CREATE TABLE ""__foo_T2__{GUID}__"" (
                      ""C2"" ANY NOT NULL,
                      CONSTRAINT ""foo_PK_T2"" PRIMARY KEY (""C2"" ASC)
                    ) WITHOUT ROWID;",
                    @"INSERT INTO ""__foo_T2__{GUID}__"" (""C2"")
                    SELECT
                      ""foo_T2"".""C2""
                    FROM ""foo_T2"";",
                    "DROP TABLE \"foo_T2\";",
                    "ALTER TABLE \"__foo_T2__{GUID}__\" RENAME TO \"foo_T2\";" );

            statements.ElementAtOrDefault( 1 )
                .Sql
                .Should()
                .SatisfySql(
                    "DROP INDEX \"foo_IX_T3_C4A\";",
                    @"CREATE TABLE ""__foo_T3__{GUID}__"" (
                      ""C3"" ANY NOT NULL,
                      ""C4"" ANY NOT NULL,
                      CONSTRAINT ""foo_PK_T3"" PRIMARY KEY (""C3"" ASC)
                    ) WITHOUT ROWID;",
                    @"INSERT INTO ""__foo_T3__{GUID}__"" (""C3"", ""C4"")
                    SELECT
                      ""foo_T3"".""C3"",
                      ""foo_T3"".""C4""
                    FROM ""foo_T3"";",
                    "DROP TABLE \"foo_T3\";",
                    "ALTER TABLE \"__foo_T3__{GUID}__\" RENAME TO \"foo_T3\";",
                    "CREATE INDEX \"foo_IX_T3_C4A\" ON \"foo_T3\" (\"C4\" ASC);" );

            statements.ElementAtOrDefault( 2 ).Sql.Should().SatisfySql( "ALTER TABLE \"foo_T1\" RENAME TO \"foo_U\";" );

            statements.ElementAtOrDefault( 3 )
                .Sql
                .Should()
                .SatisfySql(
                    @"CREATE TABLE ""__foo_T2__{GUID}__"" (
                      ""C2"" ANY NOT NULL,
                      CONSTRAINT ""foo_PK_T2"" PRIMARY KEY (""C2"" ASC),
                      CONSTRAINT ""foo_FK_T2_C2_REF_T1"" FOREIGN KEY (""C2"") REFERENCES ""foo_U"" (""C1"") ON DELETE RESTRICT ON UPDATE RESTRICT
                    ) WITHOUT ROWID;",
                    @"INSERT INTO ""__foo_T2__{GUID}__"" (""C2"")
                    SELECT
                      ""foo_T2"".""C2""
                    FROM ""foo_T2"";",
                    "DROP TABLE \"foo_T2\";",
                    "ALTER TABLE \"__foo_T2__{GUID}__\" RENAME TO \"foo_T2\";" );

            statements.ElementAtOrDefault( 4 )
                .Sql
                .Should()
                .SatisfySql(
                    "DROP INDEX \"foo_IX_T3_C4A\";",
                    @"CREATE TABLE ""__foo_T3__{GUID}__"" (
                      ""C3"" ANY NOT NULL,
                      ""C4"" ANY NOT NULL,
                      CONSTRAINT ""foo_PK_T3"" PRIMARY KEY (""C3"" ASC),
                      CONSTRAINT ""foo_FK_T3_C4_REF_T1"" FOREIGN KEY (""C4"") REFERENCES ""foo_U"" (""C1"") ON DELETE RESTRICT ON UPDATE RESTRICT,
                      CONSTRAINT ""foo_FK_T3_C3_REF_T1"" FOREIGN KEY (""C3"") REFERENCES ""foo_U"" (""C1"") ON DELETE RESTRICT ON UPDATE RESTRICT
                    ) WITHOUT ROWID;",
                    @"INSERT INTO ""__foo_T3__{GUID}__"" (""C3"", ""C4"")
                    SELECT
                      ""foo_T3"".""C3"",
                      ""foo_T3"".""C4""
                    FROM ""foo_T3"";",
                    "DROP TABLE \"foo_T3\";",
                    "ALTER TABLE \"__foo_T3__{GUID}__\" RENAME TO \"foo_T3\";",
                    "CREATE INDEX \"foo_IX_T3_C4A\" ON \"foo_T3\" (\"C4\" ASC);" );
        }
    }

    [Fact]
    public void SetName_ShouldUpdateName_WhenNameChangesAndTableHasExternalForeignKeyReferencesWithOneOfReferencingTablesUnderChange()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
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

        var t4 = schema.Objects.CreateTable( "T4" );
        var c5 = t4.Columns.Create( "C5" );
        var pk4 = t4.Constraints.SetPrimaryKey( c5.Asc() );

        var fk1 = t3.Constraints.CreateForeignKey( pk3.Index, pk1.Index );
        var fk2 = t4.Constraints.CreateForeignKey( pk4.Index, pk1.Index );
        var fk3 = t2.Constraints.CreateForeignKey( pk2.Index, pk1.Index );
        var fk4 = t3.Constraints.CreateForeignKey( t3.Constraints.CreateIndex( c4.Asc() ), pk1.Index );

        var startStatementCount = schema.Database.Changes.GetPendingActions().Length;

        t3.Columns.Create( "C6" ).SetType<int>();
        var result = ((ISqlTableBuilder)sut).SetName( "U" );
        var statements = schema.Database.Changes.GetPendingActions().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.Name.Should().Be( "U" );
            schema.Objects.Contains( "U" ).Should().BeTrue();
            schema.Objects.Contains( "T1" ).Should().BeFalse();
            fk1.IsRemoved.Should().BeFalse();
            fk2.IsRemoved.Should().BeFalse();
            fk3.IsRemoved.Should().BeFalse();
            fk4.IsRemoved.Should().BeFalse();

            statements.Should().HaveCount( 7 );

            statements.ElementAtOrDefault( 0 )
                .Sql
                .Should()
                .SatisfySql(
                    "DROP INDEX \"foo_IX_T3_C4A\";",
                    @"CREATE TABLE ""__foo_T3__{GUID}__"" (
                      ""C3"" ANY NOT NULL,
                      ""C4"" ANY NOT NULL,
                      ""C6"" INTEGER NOT NULL DEFAULT (0),
                      CONSTRAINT ""foo_PK_T3"" PRIMARY KEY (""C3"" ASC)
                    ) WITHOUT ROWID;",
                    @"INSERT INTO ""__foo_T3__{GUID}__"" (""C3"", ""C4"", ""C6"")
                    SELECT
                      ""foo_T3"".""C3"",
                      ""foo_T3"".""C4"",
                      0 AS ""C6""
                    FROM ""foo_T3"";",
                    "DROP TABLE \"foo_T3\";",
                    "ALTER TABLE \"__foo_T3__{GUID}__\" RENAME TO \"foo_T3\";",
                    "CREATE INDEX \"foo_IX_T3_C4A\" ON \"foo_T3\" (\"C4\" ASC);" );

            statements.ElementAtOrDefault( 1 )
                .Sql
                .Should()
                .SatisfySql(
                    @"CREATE TABLE ""__foo_T2__{GUID}__"" (
                      ""C2"" ANY NOT NULL,
                      CONSTRAINT ""foo_PK_T2"" PRIMARY KEY (""C2"" ASC)
                    ) WITHOUT ROWID;",
                    @"INSERT INTO ""__foo_T2__{GUID}__"" (""C2"")
                    SELECT
                      ""foo_T2"".""C2""
                    FROM ""foo_T2"";",
                    "DROP TABLE \"foo_T2\";",
                    "ALTER TABLE \"__foo_T2__{GUID}__\" RENAME TO \"foo_T2\";" );

            statements.ElementAtOrDefault( 2 )
                .Sql
                .Should()
                .SatisfySql(
                    @"CREATE TABLE ""__foo_T4__{GUID}__"" (
                      ""C5"" ANY NOT NULL,
                      CONSTRAINT ""foo_PK_T4"" PRIMARY KEY (""C5"" ASC)
                    ) WITHOUT ROWID;",
                    @"INSERT INTO ""__foo_T4__{GUID}__"" (""C5"")
                    SELECT
                      ""foo_T4"".""C5""
                    FROM ""foo_T4"";",
                    "DROP TABLE \"foo_T4\";",
                    "ALTER TABLE \"__foo_T4__{GUID}__\" RENAME TO \"foo_T4\";" );

            statements.ElementAtOrDefault( 3 ).Sql.Should().SatisfySql( "ALTER TABLE \"foo_T1\" RENAME TO \"foo_U\";" );

            statements.ElementAtOrDefault( 4 )
                .Sql
                .Should()
                .SatisfySql(
                    "DROP INDEX \"foo_IX_T3_C4A\";",
                    @"CREATE TABLE ""__foo_T3__{GUID}__"" (
                      ""C3"" ANY NOT NULL,
                      ""C4"" ANY NOT NULL,
                      ""C6"" INTEGER NOT NULL DEFAULT (0),
                      CONSTRAINT ""foo_PK_T3"" PRIMARY KEY (""C3"" ASC),
                      CONSTRAINT ""foo_FK_T3_C4_REF_T1"" FOREIGN KEY (""C4"") REFERENCES ""foo_U"" (""C1"") ON DELETE RESTRICT ON UPDATE RESTRICT,
                      CONSTRAINT ""foo_FK_T3_C3_REF_T1"" FOREIGN KEY (""C3"") REFERENCES ""foo_U"" (""C1"") ON DELETE RESTRICT ON UPDATE RESTRICT
                    ) WITHOUT ROWID;",
                    @"INSERT INTO ""__foo_T3__{GUID}__"" (""C3"", ""C4"", ""C6"")
                    SELECT
                      ""foo_T3"".""C3"",
                      ""foo_T3"".""C4"",
                      ""foo_T3"".""C6""
                    FROM ""foo_T3"";",
                    "DROP TABLE \"foo_T3\";",
                    "ALTER TABLE \"__foo_T3__{GUID}__\" RENAME TO \"foo_T3\";",
                    "CREATE INDEX \"foo_IX_T3_C4A\" ON \"foo_T3\" (\"C4\" ASC);" );

            statements.ElementAtOrDefault( 5 )
                .Sql
                .Should()
                .SatisfySql(
                    @"CREATE TABLE ""__foo_T2__{GUID}__"" (
                      ""C2"" ANY NOT NULL,
                      CONSTRAINT ""foo_PK_T2"" PRIMARY KEY (""C2"" ASC),
                      CONSTRAINT ""foo_FK_T2_C2_REF_T1"" FOREIGN KEY (""C2"") REFERENCES ""foo_U"" (""C1"") ON DELETE RESTRICT ON UPDATE RESTRICT
                    ) WITHOUT ROWID;",
                    @"INSERT INTO ""__foo_T2__{GUID}__"" (""C2"")
                    SELECT
                      ""foo_T2"".""C2""
                    FROM ""foo_T2"";",
                    "DROP TABLE \"foo_T2\";",
                    "ALTER TABLE \"__foo_T2__{GUID}__\" RENAME TO \"foo_T2\";" );

            statements.ElementAtOrDefault( 6 )
                .Sql
                .Should()
                .SatisfySql(
                    @"CREATE TABLE ""__foo_T4__{GUID}__"" (
                      ""C5"" ANY NOT NULL,
                      CONSTRAINT ""foo_PK_T4"" PRIMARY KEY (""C5"" ASC),
                      CONSTRAINT ""foo_FK_T4_C5_REF_T1"" FOREIGN KEY (""C5"") REFERENCES ""foo_U"" (""C1"") ON DELETE RESTRICT ON UPDATE RESTRICT
                    ) WITHOUT ROWID;",
                    @"INSERT INTO ""__foo_T4__{GUID}__"" (""C5"")
                    SELECT
                      ""foo_T4"".""C5""
                    FROM ""foo_T4"";",
                    "DROP TABLE \"foo_T4\";",
                    "ALTER TABLE \"__foo_T4__{GUID}__\" RENAME TO \"foo_T4\";" );
        }
    }

    [Fact]
    public void SetName_ShouldUpdateName_WhenNameChangesAndTableHasExternalForeignKeyReferencesAndChangedTableHasOtherPendingChanges()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var sut = schema.Objects.CreateTable( "T1" );
        var c1 = sut.Columns.Create( "C1" );
        var pk1 = sut.Constraints.SetPrimaryKey( c1.Asc() );

        var t2 = schema.Objects.CreateTable( "T2" );
        var c2 = t2.Columns.Create( "C2" );
        var pk2 = t2.Constraints.SetPrimaryKey( c2.Asc() );
        var fk1 = t2.Constraints.CreateForeignKey( pk2.Index, pk1.Index );

        var startStatementCount = schema.Database.Changes.GetPendingActions().Length;

        sut.Columns.Create( "C3" ).SetType<int>();
        var result = ((ISqlTableBuilder)sut).SetName( "U" );
        var statements = schema.Database.Changes.GetPendingActions().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.Name.Should().Be( "U" );
            schema.Objects.Contains( "U" ).Should().BeTrue();
            schema.Objects.Contains( "T1" ).Should().BeFalse();
            fk1.IsRemoved.Should().BeFalse();

            statements.Should().HaveCount( 4 );

            statements.ElementAtOrDefault( 0 )
                .Sql
                .Should()
                .SatisfySql(
                    @"CREATE TABLE ""__foo_T1__{GUID}__"" (
                      ""C1"" ANY NOT NULL,
                      ""C3"" INTEGER NOT NULL DEFAULT (0),
                      CONSTRAINT ""foo_PK_T1"" PRIMARY KEY (""C1"" ASC)
                    ) WITHOUT ROWID;",
                    @"INSERT INTO ""__foo_T1__{GUID}__"" (""C1"", ""C3"")
                    SELECT
                      ""foo_T1"".""C1"",
                      0 AS ""C3""
                    FROM ""foo_T1"";",
                    "DROP TABLE \"foo_T1\";",
                    "ALTER TABLE \"__foo_T1__{GUID}__\" RENAME TO \"foo_T1\";" );

            statements.ElementAtOrDefault( 1 )
                .Sql
                .Should()
                .SatisfySql(
                    @"CREATE TABLE ""__foo_T2__{GUID}__"" (
                      ""C2"" ANY NOT NULL,
                      CONSTRAINT ""foo_PK_T2"" PRIMARY KEY (""C2"" ASC)
                    ) WITHOUT ROWID;",
                    @"INSERT INTO ""__foo_T2__{GUID}__"" (""C2"")
                    SELECT
                      ""foo_T2"".""C2""
                    FROM ""foo_T2"";",
                    "DROP TABLE \"foo_T2\";",
                    "ALTER TABLE \"__foo_T2__{GUID}__\" RENAME TO \"foo_T2\";" );

            statements.ElementAtOrDefault( 2 ).Sql.Should().SatisfySql( "ALTER TABLE \"foo_T1\" RENAME TO \"foo_U\";" );

            statements.ElementAtOrDefault( 3 )
                .Sql
                .Should()
                .SatisfySql(
                    @"CREATE TABLE ""__foo_T2__{GUID}__"" (
                      ""C2"" ANY NOT NULL,
                      CONSTRAINT ""foo_PK_T2"" PRIMARY KEY (""C2"" ASC),
                      CONSTRAINT ""foo_FK_T2_C2_REF_T1"" FOREIGN KEY (""C2"") REFERENCES ""foo_U"" (""C1"") ON DELETE RESTRICT ON UPDATE RESTRICT
                    ) WITHOUT ROWID;",
                    @"INSERT INTO ""__foo_T2__{GUID}__"" (""C2"")
                    SELECT
                      ""foo_T2"".""C2""
                    FROM ""foo_T2"";",
                    "DROP TABLE \"foo_T2\";",
                    "ALTER TABLE \"__foo_T2__{GUID}__\" RENAME TO \"foo_T2\";" );
        }
    }

    [Fact]
    public void SetName_ShouldUpdateName_WhenNameChangesAndTableHasExternalForeignKeyReferencesAndUnrelatedTableHasPendingChanges()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var sut = schema.Objects.CreateTable( "T1" );
        var c1 = sut.Columns.Create( "C1" );
        var pk1 = sut.Constraints.SetPrimaryKey( c1.Asc() );

        var t2 = schema.Objects.CreateTable( "T2" );
        var c2 = t2.Columns.Create( "C2" );
        var pk2 = t2.Constraints.SetPrimaryKey( c2.Asc() );
        var fk1 = t2.Constraints.CreateForeignKey( pk2.Index, pk1.Index );

        var t3 = schema.Objects.CreateTable( "T3" );
        t3.Constraints.SetPrimaryKey( t3.Columns.Create( "C3" ).Asc() );

        var startStatementCount = schema.Database.Changes.GetPendingActions().Length;

        t3.Columns.Create( "C4" ).SetType<int>();
        var result = ((ISqlTableBuilder)sut).SetName( "U" );
        var statements = schema.Database.Changes.GetPendingActions().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.Name.Should().Be( "U" );
            schema.Objects.Contains( "U" ).Should().BeTrue();
            schema.Objects.Contains( "T1" ).Should().BeFalse();
            fk1.IsRemoved.Should().BeFalse();

            statements.Should().HaveCount( 4 );

            statements.ElementAtOrDefault( 0 )
                .Sql
                .Should()
                .SatisfySql(
                    @"CREATE TABLE ""__foo_T3__{GUID}__"" (
                      ""C3"" ANY NOT NULL,
                      ""C4"" INTEGER NOT NULL DEFAULT (0),
                      CONSTRAINT ""foo_PK_T3"" PRIMARY KEY (""C3"" ASC)
                    ) WITHOUT ROWID;",
                    @"INSERT INTO ""__foo_T3__{GUID}__"" (""C3"", ""C4"")
                    SELECT
                      ""foo_T3"".""C3"",
                      0 AS ""C4""
                    FROM ""foo_T3"";",
                    "DROP TABLE \"foo_T3\";",
                    "ALTER TABLE \"__foo_T3__{GUID}__\" RENAME TO \"foo_T3\";" );

            statements.ElementAtOrDefault( 1 )
                .Sql
                .Should()
                .SatisfySql(
                    @"CREATE TABLE ""__foo_T2__{GUID}__"" (
                      ""C2"" ANY NOT NULL,
                      CONSTRAINT ""foo_PK_T2"" PRIMARY KEY (""C2"" ASC)
                    ) WITHOUT ROWID;",
                    @"INSERT INTO ""__foo_T2__{GUID}__"" (""C2"")
                    SELECT
                      ""foo_T2"".""C2""
                    FROM ""foo_T2"";",
                    "DROP TABLE \"foo_T2\";",
                    "ALTER TABLE \"__foo_T2__{GUID}__\" RENAME TO \"foo_T2\";" );

            statements.ElementAtOrDefault( 2 ).Sql.Should().SatisfySql( "ALTER TABLE \"foo_T1\" RENAME TO \"foo_U\";" );

            statements.ElementAtOrDefault( 3 )
                .Sql
                .Should()
                .SatisfySql(
                    @"CREATE TABLE ""__foo_T2__{GUID}__"" (
                      ""C2"" ANY NOT NULL,
                      CONSTRAINT ""foo_PK_T2"" PRIMARY KEY (""C2"" ASC),
                      CONSTRAINT ""foo_FK_T2_C2_REF_T1"" FOREIGN KEY (""C2"") REFERENCES ""foo_U"" (""C1"") ON DELETE RESTRICT ON UPDATE RESTRICT
                    ) WITHOUT ROWID;",
                    @"INSERT INTO ""__foo_T2__{GUID}__"" (""C2"")
                    SELECT
                      ""foo_T2"".""C2""
                    FROM ""foo_T2"";",
                    "DROP TABLE \"foo_T2\";",
                    "ALTER TABLE \"__foo_T2__{GUID}__\" RENAME TO \"foo_T2\";" );
        }
    }

    [Fact]
    public void SetName_ShouldUpdateName_WhenNameChangesAndTableHasViewReferences()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var sut = schema.Objects.CreateTable( "T" );
        sut.Constraints.SetPrimaryKey( sut.Columns.Create( "C" ).Asc() );

        var v1 = schema.Objects.CreateView( "V1", sut.ToRecordSet().ToDataSource().Select( s => new[] { s.From["C"].AsSelf() } ) );

        var v2 = schema.Objects.CreateView(
            "V2",
            v1.ToRecordSet().Join( sut.ToRecordSet().InnerOn( SqlNode.True() ) ).Select( s => new[] { s.GetAll() } ) );

        var v3 = schema.Objects.CreateView( "V3", v1.ToRecordSet().ToDataSource().Select( s => new[] { s.GetAll() } ) );

        var startStatementCount = schema.Database.Changes.GetPendingActions().Length;

        var result = ((ISqlTableBuilder)sut).SetName( "U" );
        var statements = schema.Database.Changes.GetPendingActions().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.Name.Should().Be( "U" );
            schema.Objects.Contains( "U" ).Should().BeTrue();
            schema.Objects.Contains( "T" ).Should().BeFalse();
            v1.IsRemoved.Should().BeFalse();
            v2.IsRemoved.Should().BeFalse();
            v3.IsRemoved.Should().BeFalse();

            statements.Should().HaveCount( 5 );

            statements.ElementAtOrDefault( 0 ).Sql.Should().SatisfySql( "DROP VIEW \"foo_V2\";" );
            statements.ElementAtOrDefault( 1 ).Sql.Should().SatisfySql( "DROP VIEW \"foo_V1\";" );

            statements.ElementAtOrDefault( 2 ).Sql.Should().SatisfySql( "ALTER TABLE \"foo_T\" RENAME TO \"foo_U\";" );

            statements.ElementAtOrDefault( 3 )
                .Sql
                .Should()
                .SatisfySql(
                    @"CREATE VIEW ""foo_V1"" AS
                    SELECT
                      ""foo_U"".""C""
                    FROM ""foo_U"";" );

            statements.ElementAtOrDefault( 4 )
                .Sql
                .Should()
                .SatisfySql(
                    @"CREATE VIEW ""foo_V2"" AS
                    SELECT
                      *
                    FROM ""foo_V1""
                    INNER JOIN ""foo_U"" ON TRUE;" );
        }
    }

    [Fact]
    public void SetName_ShouldUpdateName_WhenNameChangesAndTableHasViewReferencesAndChangedTableHasOtherPendingChanges()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var sut = schema.Objects.CreateTable( "T" );
        sut.Constraints.SetPrimaryKey( sut.Columns.Create( "C" ).Asc() );

        var v1 = schema.Objects.CreateView( "V1", sut.ToRecordSet().ToDataSource().Select( s => new[] { s.From["C"].AsSelf() } ) );

        var v2 = schema.Objects.CreateView(
            "V2",
            v1.ToRecordSet().Join( sut.ToRecordSet().InnerOn( SqlNode.True() ) ).Select( s => new[] { s.GetAll() } ) );

        var v3 = schema.Objects.CreateView( "V3", v1.ToRecordSet().ToDataSource().Select( s => new[] { s.GetAll() } ) );

        var startStatementCount = schema.Database.Changes.GetPendingActions().Length;

        sut.Columns.Create( "D" ).SetType<int>();
        var result = ((ISqlTableBuilder)sut).SetName( "U" );
        var statements = schema.Database.Changes.GetPendingActions().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.Name.Should().Be( "U" );
            schema.Objects.Contains( "U" ).Should().BeTrue();
            schema.Objects.Contains( "T" ).Should().BeFalse();
            v1.IsRemoved.Should().BeFalse();
            v2.IsRemoved.Should().BeFalse();
            v3.IsRemoved.Should().BeFalse();

            statements.Should().HaveCount( 6 );

            statements.ElementAtOrDefault( 0 )
                .Sql
                .Should()
                .SatisfySql(
                    @"CREATE TABLE ""__foo_T__{GUID}__"" (
                      ""C"" ANY NOT NULL,
                      ""D"" INTEGER NOT NULL DEFAULT (0),
                      CONSTRAINT ""foo_PK_T"" PRIMARY KEY (""C"" ASC)
                    ) WITHOUT ROWID;",
                    @"INSERT INTO ""__foo_T__{GUID}__"" (""C"", ""D"")
                    SELECT
                      ""foo_T"".""C"",
                      0 AS ""D""
                    FROM ""foo_T"";",
                    "DROP TABLE \"foo_T\";",
                    "ALTER TABLE \"__foo_T__{GUID}__\" RENAME TO \"foo_T\";" );

            statements.ElementAtOrDefault( 1 ).Sql.Should().SatisfySql( "DROP VIEW \"foo_V2\";" );
            statements.ElementAtOrDefault( 2 ).Sql.Should().SatisfySql( "DROP VIEW \"foo_V1\";" );

            statements.ElementAtOrDefault( 3 ).Sql.Should().SatisfySql( "ALTER TABLE \"foo_T\" RENAME TO \"foo_U\";" );

            statements.ElementAtOrDefault( 4 )
                .Sql
                .Should()
                .SatisfySql(
                    @"CREATE VIEW ""foo_V1"" AS
                    SELECT
                      ""foo_U"".""C""
                    FROM ""foo_U"";" );

            statements.ElementAtOrDefault( 5 )
                .Sql
                .Should()
                .SatisfySql(
                    @"CREATE VIEW ""foo_V2"" AS
                    SELECT
                      *
                    FROM ""foo_V1""
                    INNER JOIN ""foo_U"" ON TRUE;" );
        }
    }

    [Theory]
    [InlineData( "" )]
    [InlineData( " " )]
    [InlineData( "\"" )]
    [InlineData( "'" )]
    [InlineData( "f\"oo" )]
    public void SetName_ShouldThrowSqliteObjectBuilderException_WhenNameIsInvalid(string name)
    {
        var db = SqliteDatabaseBuilderMock.Create();
        var sut = db.Schemas.Default.Objects.CreateTable( Fixture.Create<string>() );

        var action = Lambda.Of( () => sut.SetName( name ) );

        action.Should()
            .ThrowExactly<SqliteObjectBuilderException>()
            .AndMatch( e => e.Dialect == SqliteDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void SetName_ShouldThrowSqliteObjectBuilderException_WhenObjectWithNameAlreadyExists()
    {
        var (name1, name2) = Fixture.CreateDistinctCollection<string>( count: 2 );
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Default;
        var other = schema.Objects.CreateTable( name2 );
        other.Constraints.SetPrimaryKey( other.Columns.Create( "C" ).Asc() );
        var sut = schema.Objects.CreateTable( name1 );

        var action = Lambda.Of( () => sut.SetName( name2 ) );

        action.Should()
            .ThrowExactly<SqliteObjectBuilderException>()
            .AndMatch( e => e.Dialect == SqliteDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void SetName_ShouldThrowSqliteObjectBuilderException_WhenTableHasBeenRemoved()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Default;
        var sut = schema.Objects.CreateTable( Fixture.Create<string>() );
        schema.Objects.Remove( sut.Name );

        var action = Lambda.Of( () => sut.SetName( Fixture.Create<string>() ) );

        action.Should()
            .ThrowExactly<SqliteObjectBuilderException>()
            .AndMatch( e => e.Dialect == SqliteDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void Remove_ShouldRemoveTable_WhenTableDoesNotHaveAnyExternalReferences()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var sut = schema.Objects.CreateTable( "T" );
        var column = sut.Columns.Create( "C" );
        var otherColumn = sut.Columns.Create( "D" );
        var pk = sut.Constraints.SetPrimaryKey( column.Asc() );
        var ix = sut.Constraints.CreateIndex( otherColumn.Asc() );
        var fk = sut.Constraints.CreateForeignKey( ix, pk.Index );
        var chk = sut.Constraints.CreateCheck( column.Node > SqlNode.Literal( 0 ) );

        var startStatementCount = schema.Database.Changes.GetPendingActions().Length;

        sut.Remove();
        var statements = schema.Database.Changes.GetPendingActions().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            schema.Objects.Count.Should().Be( 0 );
            sut.IsRemoved.Should().BeTrue();
            column.IsRemoved.Should().BeTrue();
            otherColumn.IsRemoved.Should().BeTrue();
            pk.IsRemoved.Should().BeTrue();
            pk.Index.IsRemoved.Should().BeTrue();
            ix.IsRemoved.Should().BeTrue();
            fk.IsRemoved.Should().BeTrue();
            chk.IsRemoved.Should().BeTrue();
            statements.Should().HaveCount( 1 );
            statements.ElementAtOrDefault( 0 ).Sql.Should().SatisfySql( "DROP TABLE \"foo_T\";" );
        }
    }

    [Fact]
    public void Remove_ShouldDoNothing_WhenTableHasAlreadyBeenRemoved()
    {
        var name = Fixture.Create<string>();
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Default;
        var sut = schema.Objects.CreateTable( name );
        sut.Remove();

        sut.Remove();

        using ( new AssertionScope() )
        {
            schema.Objects.Contains( name ).Should().BeFalse();
            sut.IsRemoved.Should().BeTrue();
        }
    }

    [Fact]
    public void Remove_ShouldThrowSqliteObjectBuilderException_WhenTableHasExternalForeignKeyReferences()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var sut = schema.Objects.CreateTable( "T" );
        var column = sut.Columns.Create( "C" );
        var pk = sut.Constraints.SetPrimaryKey( column.Asc() );

        var otherTable = schema.Objects.CreateTable( "U" );
        var otherColumn = otherTable.Columns.Create( "D" );
        var otherPk = otherTable.Constraints.SetPrimaryKey( otherColumn.Asc() );
        otherTable.Constraints.CreateForeignKey( otherPk.Index, pk.Index );

        var action = Lambda.Of( () => sut.Remove() );

        action.Should()
            .ThrowExactly<SqliteObjectBuilderException>()
            .AndMatch( e => e.Dialect == SqliteDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void Remove_ShouldThrowSqliteObjectBuilderException_WhenTableHasViewReferences()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var sut = schema.Objects.CreateTable( "T" );
        var column = sut.Columns.Create( "C" );
        sut.Constraints.SetPrimaryKey( column.Asc() );
        schema.Objects.CreateView( "V", sut.ToRecordSet().ToDataSource().Select( s => new[] { s.GetAll() } ) );

        var action = Lambda.Of( () => sut.Remove() );

        action.Should()
            .ThrowExactly<SqliteObjectBuilderException>()
            .AndMatch( e => e.Dialect == SqliteDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void ColumnNameSwap_ShouldGenerateCorrectScript()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var sut = schema.Objects.CreateTable( "T" );
        sut.Constraints.SetPrimaryKey( sut.Columns.Create( "P" ).Asc() );
        var a = sut.Columns.Create( "A" );
        var b = sut.Columns.Create( "B" );

        var startStatementCount = schema.Database.Changes.GetPendingActions().Length;

        a.SetName( "C" );
        b.SetName( "A" );
        a.SetName( "B" );

        var statements = schema.Database.Changes.GetPendingActions().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            statements.Should().HaveCount( 1 );
            statements.ElementAtOrDefault( 0 )
                .Sql
                .Should()
                .SatisfySql(
                    "ALTER TABLE \"foo_T\" RENAME COLUMN \"A\" TO \"__A__{GUID}__\";",
                    "ALTER TABLE \"foo_T\" RENAME COLUMN \"B\" TO \"A\";",
                    "ALTER TABLE \"foo_T\" RENAME COLUMN \"__A__{GUID}__\" TO \"B\";" );
        }
    }

    [Fact]
    public void ColumnChainNameSwap_ShouldGenerateCorrectScript()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var sut = schema.Objects.CreateTable( "T" );
        sut.Constraints.SetPrimaryKey( sut.Columns.Create( "P" ).Asc() );
        var a = sut.Columns.Create( "A" );
        var b = sut.Columns.Create( "B" );
        var c = sut.Columns.Create( "C" );
        var d = sut.Columns.Create( "D" );

        var startStatementCount = schema.Database.Changes.GetPendingActions().Length;

        a.SetName( "E" );
        b.SetName( "A" );
        c.SetName( "B" );
        d.SetName( "C" );
        a.SetName( "D" );

        var statements = schema.Database.Changes.GetPendingActions().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            statements.Should().HaveCount( 1 );
            statements.ElementAtOrDefault( 0 )
                .Sql
                .Should()
                .SatisfySql(
                    "ALTER TABLE \"foo_T\" RENAME COLUMN \"A\" TO \"__A__{GUID}__\";",
                    "ALTER TABLE \"foo_T\" RENAME COLUMN \"B\" TO \"A\";",
                    "ALTER TABLE \"foo_T\" RENAME COLUMN \"C\" TO \"B\";",
                    "ALTER TABLE \"foo_T\" RENAME COLUMN \"D\" TO \"C\";",
                    "ALTER TABLE \"foo_T\" RENAME COLUMN \"__A__{GUID}__\" TO \"D\";" );
        }
    }

    [Fact]
    public void MultipleColumnNameChange_ShouldGenerateCorrectScript_WhenThereAreTemporaryNameConflicts()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var sut = schema.Objects.CreateTable( "T" );
        sut.Constraints.SetPrimaryKey( sut.Columns.Create( "P" ).Asc() );
        var a = sut.Columns.Create( "A" );
        var b = sut.Columns.Create( "B" );
        var c = sut.Columns.Create( "C" );

        var startStatementCount = schema.Database.Changes.GetPendingActions().Length;

        a.SetName( "X" );
        b.SetName( "Y" );
        c.SetName( "D" );
        b.SetName( "C" );
        a.SetName( "B" );

        var statements = schema.Database.Changes.GetPendingActions().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            statements.Should().HaveCount( 1 );
            statements.ElementAtOrDefault( 0 )
                .Sql
                .Should()
                .SatisfySql(
                    "ALTER TABLE \"foo_T\" RENAME COLUMN \"C\" TO \"D\";",
                    "ALTER TABLE \"foo_T\" RENAME COLUMN \"B\" TO \"C\";",
                    "ALTER TABLE \"foo_T\" RENAME COLUMN \"A\" TO \"B\";" );
        }
    }

    [Fact]
    public void MultipleTableChangesWithoutReconstruction_ShouldGenerateCorrectScript()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var sut = schema.Objects.CreateTable( "T" );
        sut.Constraints.SetPrimaryKey( sut.Columns.Create( "C1" ).Asc() );
        var c2 = sut.Columns.Create( "C2" );
        var c3 = sut.Columns.Create( "C3" );
        var c4 = sut.Columns.Create( "C4" );
        var ix = sut.Constraints.CreateIndex( c2.Asc() );

        var startStatementCount = schema.Database.Changes.GetPendingActions().Length;

        sut.SetName( "U" );
        c3.SetName( "X" );
        c4.Remove();
        ix.Remove();
        sut.Constraints.CreateIndex( c2.Asc(), c3.Desc() );

        var statements = schema.Database.Changes.GetPendingActions().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            statements.Should().HaveCount( 1 );
            statements.ElementAtOrDefault( 0 )
                .Sql
                .Should()
                .SatisfySql(
                    "ALTER TABLE \"foo_T\" RENAME TO \"foo_U\";",
                    "DROP INDEX \"foo_IX_T_C2A\";",
                    "ALTER TABLE \"foo_U\" DROP COLUMN \"C4\";",
                    "ALTER TABLE \"foo_U\" RENAME COLUMN \"C3\" TO \"X\";",
                    "CREATE INDEX \"foo_IX_U_C2A_XD\" ON \"foo_U\" (\"C2\" ASC, \"X\" DESC);" );
        }
    }

    [Fact]
    public void NameChange_ThenRemoval_ShouldDropTableByUsingItsOldName()
    {
        var builder = SqliteDatabaseBuilderMock.Create();
        var sut = builder.Schemas.Create( "s" ).Objects.CreateTable( "foo" );
        sut.Constraints.SetPrimaryKey( sut.Columns.Create( "a" ).Asc() );
        _ = builder.Changes.GetPendingActions();

        sut.SetName( "bar" );
        sut.Remove();

        var result = builder.Changes.GetPendingActions()[^1].Sql;

        result.Should().SatisfySql( "DROP TABLE \"s_foo\";" );
    }

    [Fact]
    public void ForSqlite_ShouldInvokeAction_WhenTableIsSqlite()
    {
        var action = Substitute.For<Action<SqliteTableBuilder>>();
        var sut = SqliteDatabaseBuilderMock.Create().Schemas.Default.Objects.CreateTable( "T" );

        var result = sut.ForSqlite( action );

        result.Should().BeSameAs( sut );
        action.Verify().CallAt( 0 ).Exists().And.Arguments.Should().BeSequentiallyEqualTo( sut );
    }

    [Fact]
    public void ForSqlite_ShouldNotInvokeAction_WhenTableIsNotSqlite()
    {
        var action = Substitute.For<Action<SqliteTableBuilder>>();
        var sut = Substitute.For<ISqlTableBuilder>();

        var result = sut.ForSqlite( action );

        result.Should().BeSameAs( sut );
        action.Verify().CallCount.Should().Be( 0 );
    }
}
