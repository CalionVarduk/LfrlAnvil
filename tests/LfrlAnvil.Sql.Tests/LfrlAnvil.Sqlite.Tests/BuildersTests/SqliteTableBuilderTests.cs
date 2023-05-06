using System.Linq;
using LfrlAnvil.Functional;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Builders;
using LfrlAnvil.Sql.Extensions;
using LfrlAnvil.Sqlite.Builders;
using LfrlAnvil.Sqlite.Exceptions;
using LfrlAnvil.Sqlite.Extensions;
using LfrlAnvil.Sqlite.Tests.Helpers;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.Sqlite.Tests.BuildersTests;

public partial class SqliteTableBuilderTests : TestsBase
{
    [Fact]
    public void Create_ShouldPrepareCorrectStatement()
    {
        var db = new SqliteDatabaseBuilder();
        var sut = db.Schemas.Create( "foo" ).Objects.CreateTable( "T" );
        var ix1 = sut.Indexes.Create( sut.Columns.Create( "C1" ).Asc() );
        var ix2 = sut.SetPrimaryKey( sut.Columns.Create( "C2" ).Asc() ).Index;
        sut.Indexes.Create( sut.Columns.Create( "C3" ).Asc(), sut.Columns.Create( "C4" ).Desc() );
        sut.ForeignKeys.Create( ix1, ix2 );

        var statements = db.GetPendingStatements().ToArray();

        using ( new AssertionScope() )
        {
            statements.Should().HaveCount( 1 );
            statements.ElementAtOrDefault( 0 )
                .Should()
                .SatisfySql(
                    @"CREATE TABLE ""foo_T"" (
                      ""C1"" ANY NOT NULL,
                      ""C2"" ANY NOT NULL,
                      ""C3"" ANY NOT NULL,
                      ""C4"" ANY NOT NULL,
                      CONSTRAINT ""foo_PK_T"" PRIMARY KEY (""C2"" ASC),
                      CONSTRAINT ""foo_FK_T_C1_REF_foo_T"" FOREIGN KEY (""C1"") REFERENCES ""foo_T"" (""C2"") ON DELETE RESTRICT ON UPDATE RESTRICT
                    ) WITHOUT ROWID;",
                    "CREATE INDEX \"foo_IX_T_C1A\" ON \"foo_T\" (\"C1\" ASC);",
                    "CREATE INDEX \"foo_IX_T_C3A_C4D\" ON \"foo_T\" (\"C3\" ASC, \"C4\" DESC);" );
        }
    }

    [Fact]
    public void Create_FollowedByRemove_ShouldDoNothing()
    {
        var db = new SqliteDatabaseBuilder();
        var sut = db.Schemas.Create( "foo" ).Objects.CreateTable( "T" );
        sut.SetPrimaryKey( sut.Columns.Create( "C" ).Asc() );
        sut.Remove();

        var statements = db.GetPendingStatements().ToArray();

        statements.Should().BeEmpty();
    }

    [Fact]
    public void Create_ShouldThrowSqliteObjectBuilderException_WhenTableDoesNotHavePrimaryKeyDuringScriptResolution()
    {
        var db = new SqliteDatabaseBuilder();
        var sut = db.Schemas.Create( "foo" ).Objects.CreateTable( "T" );
        sut.Columns.Create( "C" );

        var action = Lambda.Of(
            () =>
            {
                var _ = db.GetPendingStatements();
            } );

        action.Should()
            .ThrowExactly<SqliteObjectBuilderException>()
            .AndMatch( e => e.Dialect == SqliteDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void ToString_ShouldReturnCorrectResult()
    {
        var db = new SqliteDatabaseBuilder();
        var sut = db.Schemas.Create( "foo" ).Objects.CreateTable( "bar" );

        var result = sut.ToString();

        result.Should().Be( "[Table] foo_bar" );
    }

    [Fact]
    public void SetName_ShouldDoNothing_WhenNewNameEqualsOldName()
    {
        var name = Fixture.Create<string>();
        var schema = new SqliteDatabaseBuilder().Schemas.Default;
        var sut = schema.Objects.CreateTable( name );

        var result = ((ISqlTableBuilder)sut).SetName( name );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.Name.Should().Be( name );
            sut.FullName.Should().Be( name );
            schema.Objects.Contains( name ).Should().BeTrue();
        }
    }

    [Fact]
    public void SetName_ShouldDoNothing_WhenNameChangesToNewNameAndThenChangesToOldName()
    {
        var (oldName, newName) = Fixture.CreateDistinctCollection<string>( count: 2 );
        var schema = new SqliteDatabaseBuilder().Schemas.Default;
        var sut = schema.Objects.CreateTable( oldName );
        sut.SetPrimaryKey( sut.Columns.Create( "C" ).Asc() );

        var startStatementCount = schema.Database.GetPendingStatements().Length;

        var result = ((ISqlTableBuilder)sut).SetName( newName ).SetName( oldName );
        var statements = schema.Database.GetPendingStatements().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.Name.Should().Be( oldName );
            sut.FullName.Should().Be( oldName );
            schema.Objects.Contains( oldName ).Should().BeTrue();
            schema.Objects.Contains( newName ).Should().BeFalse();
            statements.Should().BeEmpty();
        }
    }

    [Fact]
    public void SetName_ShouldUpdateName_WhenNameChangesAndTableDoesNotHaveAnyExternalReferences()
    {
        var (oldName, newName) = ("foo", "bar");
        var schema = new SqliteDatabaseBuilder().Schemas.Create( "s" );
        var sut = schema.Objects.CreateTable( oldName );
        sut.SetPrimaryKey( sut.Columns.Create( "C" ).Asc() );

        var startStatementCount = schema.Database.GetPendingStatements().Length;

        var result = ((ISqlTableBuilder)sut).SetName( newName );
        var statements = schema.Database.GetPendingStatements().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.Name.Should().Be( newName );
            sut.FullName.Should().Be( "s_bar" );
            schema.Objects.Contains( newName ).Should().BeTrue();
            schema.Objects.Contains( oldName ).Should().BeFalse();
            statements.Should().HaveCount( 1 );
            statements.ElementAtOrDefault( 0 ).Should().SatisfySql( "ALTER TABLE \"s_foo\" RENAME TO \"s_bar\";" );
        }
    }

    [Fact]
    public void SetName_ShouldUpdateName_WhenNameChangesAndTableHasSelfReference()
    {
        var (oldName, newName) = ("foo", "bar");
        var schema = new SqliteDatabaseBuilder().Schemas.Create( "s" );
        var sut = schema.Objects.CreateTable( oldName );
        var c1 = sut.Columns.Create( "C1" );
        var c2 = sut.Columns.Create( "C2" );
        var pk = sut.SetPrimaryKey( c1.Asc() );
        var fk = sut.ForeignKeys.Create( sut.Indexes.Create( c2.Asc() ), pk.Index );

        var startStatementCount = schema.Database.GetPendingStatements().Length;

        var result = ((ISqlTableBuilder)sut).SetName( newName );
        var statements = schema.Database.GetPendingStatements().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.Name.Should().Be( newName );
            sut.FullName.Should().Be( "s_bar" );
            schema.Objects.Contains( newName ).Should().BeTrue();
            schema.Objects.Contains( oldName ).Should().BeFalse();
            fk.IsRemoved.Should().BeFalse();
            statements.Should().HaveCount( 1 );
            statements.ElementAtOrDefault( 0 )
                .Should()
                .SatisfySql(
                    "ALTER TABLE \"s_foo\" RENAME TO \"s_bar\";",
                    "DROP INDEX \"s_IX_foo_C2A\";",
                    @"CREATE TABLE ""__s_bar__{GUID}__"" (
                      ""C1"" ANY NOT NULL,
                      ""C2"" ANY NOT NULL,
                      CONSTRAINT ""s_PK_foo"" PRIMARY KEY (""C1"" ASC),
                      CONSTRAINT ""s_FK_foo_C2_REF_s_foo"" FOREIGN KEY (""C2"") REFERENCES ""s_bar"" (""C1"") ON DELETE RESTRICT ON UPDATE RESTRICT
                    ) WITHOUT ROWID;",
                    @"INSERT INTO ""__s_bar__{GUID}__"" (""C1"", ""C2"")
                    SELECT ""C1"", ""C2""
                    FROM ""s_bar"";",
                    "DROP TABLE \"s_bar\";",
                    "ALTER TABLE \"__s_bar__{GUID}__\" RENAME TO \"s_bar\";",
                    "CREATE INDEX \"s_IX_foo_C2A\" ON \"s_bar\" (\"C2\" ASC);" );
        }
    }

    [Fact]
    public void SetName_ShouldUpdateName_WhenNameChangesAndTableHasExternalReferences()
    {
        var schema = new SqliteDatabaseBuilder().Schemas.Create( "foo" );
        var sut = schema.Objects.CreateTable( "T1" );
        var c1 = sut.Columns.Create( "C1" );
        var pk1 = sut.SetPrimaryKey( c1.Asc() );

        var t2 = schema.Objects.CreateTable( "T2" );
        var c2 = t2.Columns.Create( "C2" );
        var pk2 = t2.SetPrimaryKey( c2.Asc() );

        var t3 = schema.Objects.CreateTable( "T3" );
        var c3 = t3.Columns.Create( "C3" );
        var c4 = t3.Columns.Create( "C4" );
        var pk3 = t3.SetPrimaryKey( c3.Asc() );

        var fk1 = t3.ForeignKeys.Create( pk3.Index, pk1.Index );
        var fk2 = t2.ForeignKeys.Create( pk2.Index, pk1.Index );
        var fk3 = t3.ForeignKeys.Create( t3.Indexes.Create( c4.Asc() ), pk1.Index );

        var startStatementCount = schema.Database.GetPendingStatements().Length;

        var result = ((ISqlTableBuilder)sut).SetName( "U" );
        var statements = schema.Database.GetPendingStatements().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.Name.Should().Be( "U" );
            sut.FullName.Should().Be( "foo_U" );
            c1.FullName.Should().Be( "foo_U.C1" );
            schema.Objects.Contains( "U" ).Should().BeTrue();
            schema.Objects.Contains( "T1" ).Should().BeFalse();
            fk1.IsRemoved.Should().BeFalse();
            fk2.IsRemoved.Should().BeFalse();
            fk3.IsRemoved.Should().BeFalse();

            statements.Should().HaveCount( 5 );

            statements.ElementAtOrDefault( 0 )
                .Should()
                .SatisfySql(
                    @"CREATE TABLE ""__foo_T2__{GUID}__"" (
                      ""C2"" ANY NOT NULL,
                      CONSTRAINT ""foo_PK_T2"" PRIMARY KEY (""C2"" ASC)
                    ) WITHOUT ROWID;",
                    @"INSERT INTO ""__foo_T2__{GUID}__"" (""C2"")
                    SELECT ""C2""
                    FROM ""foo_T2"";",
                    "DROP TABLE \"foo_T2\";",
                    "ALTER TABLE \"__foo_T2__{GUID}__\" RENAME TO \"foo_T2\";" );

            statements.ElementAtOrDefault( 1 )
                .Should()
                .SatisfySql(
                    "DROP INDEX \"foo_IX_T3_C4A\";",
                    @"CREATE TABLE ""__foo_T3__{GUID}__"" (
                      ""C3"" ANY NOT NULL,
                      ""C4"" ANY NOT NULL,
                      CONSTRAINT ""foo_PK_T3"" PRIMARY KEY (""C3"" ASC)
                    ) WITHOUT ROWID;",
                    @"INSERT INTO ""__foo_T3__{GUID}__"" (""C3"", ""C4"")
                    SELECT ""C3"", ""C4""
                    FROM ""foo_T3"";",
                    "DROP TABLE \"foo_T3\";",
                    "ALTER TABLE \"__foo_T3__{GUID}__\" RENAME TO \"foo_T3\";",
                    "CREATE INDEX \"foo_IX_T3_C4A\" ON \"foo_T3\" (\"C4\" ASC);" );

            statements.ElementAtOrDefault( 2 ).Should().SatisfySql( "ALTER TABLE \"foo_T1\" RENAME TO \"foo_U\";" );

            statements.ElementAtOrDefault( 3 )
                .Should()
                .SatisfySql(
                    @"CREATE TABLE ""__foo_T2__{GUID}__"" (
                      ""C2"" ANY NOT NULL,
                      CONSTRAINT ""foo_PK_T2"" PRIMARY KEY (""C2"" ASC),
                      CONSTRAINT ""foo_FK_T2_C2_REF_foo_T1"" FOREIGN KEY (""C2"") REFERENCES ""foo_U"" (""C1"") ON DELETE RESTRICT ON UPDATE RESTRICT
                    ) WITHOUT ROWID;",
                    @"INSERT INTO ""__foo_T2__{GUID}__"" (""C2"")
                    SELECT ""C2""
                    FROM ""foo_T2"";",
                    "DROP TABLE \"foo_T2\";",
                    "ALTER TABLE \"__foo_T2__{GUID}__\" RENAME TO \"foo_T2\";" );

            statements.ElementAtOrDefault( 4 )
                .Should()
                .SatisfySql(
                    "DROP INDEX \"foo_IX_T3_C4A\";",
                    @"CREATE TABLE ""__foo_T3__{GUID}__"" (
                      ""C3"" ANY NOT NULL,
                      ""C4"" ANY NOT NULL,
                      CONSTRAINT ""foo_PK_T3"" PRIMARY KEY (""C3"" ASC),
                      CONSTRAINT ""foo_FK_T3_C4_REF_foo_T1"" FOREIGN KEY (""C4"") REFERENCES ""foo_U"" (""C1"") ON DELETE RESTRICT ON UPDATE RESTRICT,
                      CONSTRAINT ""foo_FK_T3_C3_REF_foo_T1"" FOREIGN KEY (""C3"") REFERENCES ""foo_U"" (""C1"") ON DELETE RESTRICT ON UPDATE RESTRICT
                    ) WITHOUT ROWID;",
                    @"INSERT INTO ""__foo_T3__{GUID}__"" (""C3"", ""C4"")
                    SELECT ""C3"", ""C4""
                    FROM ""foo_T3"";",
                    "DROP TABLE \"foo_T3\";",
                    "ALTER TABLE \"__foo_T3__{GUID}__\" RENAME TO \"foo_T3\";",
                    "CREATE INDEX \"foo_IX_T3_C4A\" ON \"foo_T3\" (\"C4\" ASC);" );
        }
    }

    [Fact]
    public void SetName_ShouldUpdateName_WhenNameChangesAndTableHasExternalReferencesWithOneOfReferencingTablesUnderChange()
    {
        var schema = new SqliteDatabaseBuilder().Schemas.Create( "foo" );
        var sut = schema.Objects.CreateTable( "T1" );
        var c1 = sut.Columns.Create( "C1" );
        var pk1 = sut.SetPrimaryKey( c1.Asc() );

        var t2 = schema.Objects.CreateTable( "T2" );
        var c2 = t2.Columns.Create( "C2" );
        var pk2 = t2.SetPrimaryKey( c2.Asc() );

        var t3 = schema.Objects.CreateTable( "T3" );
        var c3 = t3.Columns.Create( "C3" );
        var c4 = t3.Columns.Create( "C4" );
        var pk3 = t3.SetPrimaryKey( c3.Asc() );

        var t4 = schema.Objects.CreateTable( "T4" );
        var c5 = t4.Columns.Create( "C5" );
        var pk4 = t4.SetPrimaryKey( c5.Asc() );

        var fk1 = t3.ForeignKeys.Create( pk3.Index, pk1.Index );
        var fk2 = t4.ForeignKeys.Create( pk4.Index, pk1.Index );
        var fk3 = t2.ForeignKeys.Create( pk2.Index, pk1.Index );
        var fk4 = t3.ForeignKeys.Create( t3.Indexes.Create( c4.Asc() ), pk1.Index );

        var startStatementCount = schema.Database.GetPendingStatements().Length;

        t3.Columns.Create( "C6" ).SetType<int>();
        var result = ((ISqlTableBuilder)sut).SetName( "U" );
        var statements = schema.Database.GetPendingStatements().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.Name.Should().Be( "U" );
            sut.FullName.Should().Be( "foo_U" );
            schema.Objects.Contains( "U" ).Should().BeTrue();
            schema.Objects.Contains( "T1" ).Should().BeFalse();
            fk1.IsRemoved.Should().BeFalse();
            fk2.IsRemoved.Should().BeFalse();
            fk3.IsRemoved.Should().BeFalse();
            fk4.IsRemoved.Should().BeFalse();

            statements.Should().HaveCount( 7 );

            statements.ElementAtOrDefault( 0 )
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
                    SELECT ""C3"", ""C4"", 0
                    FROM ""foo_T3"";",
                    "DROP TABLE \"foo_T3\";",
                    "ALTER TABLE \"__foo_T3__{GUID}__\" RENAME TO \"foo_T3\";",
                    "CREATE INDEX \"foo_IX_T3_C4A\" ON \"foo_T3\" (\"C4\" ASC);" );

            statements.ElementAtOrDefault( 1 )
                .Should()
                .SatisfySql(
                    @"CREATE TABLE ""__foo_T2__{GUID}__"" (
                      ""C2"" ANY NOT NULL,
                      CONSTRAINT ""foo_PK_T2"" PRIMARY KEY (""C2"" ASC)
                    ) WITHOUT ROWID;",
                    @"INSERT INTO ""__foo_T2__{GUID}__"" (""C2"")
                    SELECT ""C2""
                    FROM ""foo_T2"";",
                    "DROP TABLE \"foo_T2\";",
                    "ALTER TABLE \"__foo_T2__{GUID}__\" RENAME TO \"foo_T2\";" );

            statements.ElementAtOrDefault( 2 )
                .Should()
                .SatisfySql(
                    @"CREATE TABLE ""__foo_T4__{GUID}__"" (
                      ""C5"" ANY NOT NULL,
                      CONSTRAINT ""foo_PK_T4"" PRIMARY KEY (""C5"" ASC)
                    ) WITHOUT ROWID;",
                    @"INSERT INTO ""__foo_T4__{GUID}__"" (""C5"")
                    SELECT ""C5""
                    FROM ""foo_T4"";",
                    "DROP TABLE \"foo_T4\";",
                    "ALTER TABLE \"__foo_T4__{GUID}__\" RENAME TO \"foo_T4\";" );

            statements.ElementAtOrDefault( 3 ).Should().SatisfySql( "ALTER TABLE \"foo_T1\" RENAME TO \"foo_U\";" );

            statements.ElementAtOrDefault( 4 )
                .Should()
                .SatisfySql(
                    "DROP INDEX \"foo_IX_T3_C4A\";",
                    @"CREATE TABLE ""__foo_T3__{GUID}__"" (
                      ""C3"" ANY NOT NULL,
                      ""C4"" ANY NOT NULL,
                      ""C6"" INTEGER NOT NULL DEFAULT (0),
                      CONSTRAINT ""foo_PK_T3"" PRIMARY KEY (""C3"" ASC),
                      CONSTRAINT ""foo_FK_T3_C4_REF_foo_T1"" FOREIGN KEY (""C4"") REFERENCES ""foo_U"" (""C1"") ON DELETE RESTRICT ON UPDATE RESTRICT,
                      CONSTRAINT ""foo_FK_T3_C3_REF_foo_T1"" FOREIGN KEY (""C3"") REFERENCES ""foo_U"" (""C1"") ON DELETE RESTRICT ON UPDATE RESTRICT
                    ) WITHOUT ROWID;",
                    @"INSERT INTO ""__foo_T3__{GUID}__"" (""C3"", ""C4"", ""C6"")
                    SELECT ""C3"", ""C4"", ""C6""
                    FROM ""foo_T3"";",
                    "DROP TABLE \"foo_T3\";",
                    "ALTER TABLE \"__foo_T3__{GUID}__\" RENAME TO \"foo_T3\";",
                    "CREATE INDEX \"foo_IX_T3_C4A\" ON \"foo_T3\" (\"C4\" ASC);" );

            statements.ElementAtOrDefault( 5 )
                .Should()
                .SatisfySql(
                    @"CREATE TABLE ""__foo_T2__{GUID}__"" (
                      ""C2"" ANY NOT NULL,
                      CONSTRAINT ""foo_PK_T2"" PRIMARY KEY (""C2"" ASC),
                      CONSTRAINT ""foo_FK_T2_C2_REF_foo_T1"" FOREIGN KEY (""C2"") REFERENCES ""foo_U"" (""C1"") ON DELETE RESTRICT ON UPDATE RESTRICT
                    ) WITHOUT ROWID;",
                    @"INSERT INTO ""__foo_T2__{GUID}__"" (""C2"")
                    SELECT ""C2""
                    FROM ""foo_T2"";",
                    "DROP TABLE \"foo_T2\";",
                    "ALTER TABLE \"__foo_T2__{GUID}__\" RENAME TO \"foo_T2\";" );

            statements.ElementAtOrDefault( 6 )
                .Should()
                .SatisfySql(
                    @"CREATE TABLE ""__foo_T4__{GUID}__"" (
                      ""C5"" ANY NOT NULL,
                      CONSTRAINT ""foo_PK_T4"" PRIMARY KEY (""C5"" ASC),
                      CONSTRAINT ""foo_FK_T4_C5_REF_foo_T1"" FOREIGN KEY (""C5"") REFERENCES ""foo_U"" (""C1"") ON DELETE RESTRICT ON UPDATE RESTRICT
                    ) WITHOUT ROWID;",
                    @"INSERT INTO ""__foo_T4__{GUID}__"" (""C5"")
                    SELECT ""C5""
                    FROM ""foo_T4"";",
                    "DROP TABLE \"foo_T4\";",
                    "ALTER TABLE \"__foo_T4__{GUID}__\" RENAME TO \"foo_T4\";" );
        }
    }

    [Fact]
    public void SetName_ShouldUpdateName_WhenNameChangesAndTableHasExternalReferencesAndChangedTableHasOtherPendingChanges()
    {
        var schema = new SqliteDatabaseBuilder().Schemas.Create( "foo" );
        var sut = schema.Objects.CreateTable( "T1" );
        var c1 = sut.Columns.Create( "C1" );
        var pk1 = sut.SetPrimaryKey( c1.Asc() );

        var t2 = schema.Objects.CreateTable( "T2" );
        var c2 = t2.Columns.Create( "C2" );
        var pk2 = t2.SetPrimaryKey( c2.Asc() );
        var fk1 = t2.ForeignKeys.Create( pk2.Index, pk1.Index );

        var startStatementCount = schema.Database.GetPendingStatements().Length;

        sut.Columns.Create( "C3" ).SetType<int>();
        var result = ((ISqlTableBuilder)sut).SetName( "U" );
        var statements = schema.Database.GetPendingStatements().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.Name.Should().Be( "U" );
            sut.FullName.Should().Be( "foo_U" );
            schema.Objects.Contains( "U" ).Should().BeTrue();
            schema.Objects.Contains( "T1" ).Should().BeFalse();
            fk1.IsRemoved.Should().BeFalse();

            statements.Should().HaveCount( 4 );

            statements.ElementAtOrDefault( 0 )
                .Should()
                .SatisfySql(
                    @"CREATE TABLE ""__foo_T1__{GUID}__"" (
                      ""C1"" ANY NOT NULL,
                      ""C3"" INTEGER NOT NULL DEFAULT (0),
                      CONSTRAINT ""foo_PK_T1"" PRIMARY KEY (""C1"" ASC)
                    ) WITHOUT ROWID;",
                    @"INSERT INTO ""__foo_T1__{GUID}__"" (""C1"", ""C3"")
                    SELECT ""C1"", 0
                    FROM ""foo_T1"";",
                    "DROP TABLE \"foo_T1\";",
                    "ALTER TABLE \"__foo_T1__{GUID}__\" RENAME TO \"foo_T1\";" );

            statements.ElementAtOrDefault( 1 )
                .Should()
                .SatisfySql(
                    @"CREATE TABLE ""__foo_T2__{GUID}__"" (
                      ""C2"" ANY NOT NULL,
                      CONSTRAINT ""foo_PK_T2"" PRIMARY KEY (""C2"" ASC)
                    ) WITHOUT ROWID;",
                    @"INSERT INTO ""__foo_T2__{GUID}__"" (""C2"")
                    SELECT ""C2""
                    FROM ""foo_T2"";",
                    "DROP TABLE \"foo_T2\";",
                    "ALTER TABLE \"__foo_T2__{GUID}__\" RENAME TO \"foo_T2\";" );

            statements.ElementAtOrDefault( 2 ).Should().SatisfySql( "ALTER TABLE \"foo_T1\" RENAME TO \"foo_U\";" );

            statements.ElementAtOrDefault( 3 )
                .Should()
                .SatisfySql(
                    @"CREATE TABLE ""__foo_T2__{GUID}__"" (
                      ""C2"" ANY NOT NULL,
                      CONSTRAINT ""foo_PK_T2"" PRIMARY KEY (""C2"" ASC),
                      CONSTRAINT ""foo_FK_T2_C2_REF_foo_T1"" FOREIGN KEY (""C2"") REFERENCES ""foo_U"" (""C1"") ON DELETE RESTRICT ON UPDATE RESTRICT
                    ) WITHOUT ROWID;",
                    @"INSERT INTO ""__foo_T2__{GUID}__"" (""C2"")
                    SELECT ""C2""
                    FROM ""foo_T2"";",
                    "DROP TABLE \"foo_T2\";",
                    "ALTER TABLE \"__foo_T2__{GUID}__\" RENAME TO \"foo_T2\";" );
        }
    }

    [Fact]
    public void SetName_ShouldUpdateName_WhenNameChangesAndTableHasExternalReferencesAndUnrelatedTableHasPendingChanges()
    {
        var schema = new SqliteDatabaseBuilder().Schemas.Create( "foo" );
        var sut = schema.Objects.CreateTable( "T1" );
        var c1 = sut.Columns.Create( "C1" );
        var pk1 = sut.SetPrimaryKey( c1.Asc() );

        var t2 = schema.Objects.CreateTable( "T2" );
        var c2 = t2.Columns.Create( "C2" );
        var pk2 = t2.SetPrimaryKey( c2.Asc() );
        var fk1 = t2.ForeignKeys.Create( pk2.Index, pk1.Index );

        var t3 = schema.Objects.CreateTable( "T3" );
        t3.SetPrimaryKey( t3.Columns.Create( "C3" ).Asc() );

        var startStatementCount = schema.Database.GetPendingStatements().Length;

        t3.Columns.Create( "C4" ).SetType<int>();
        var result = ((ISqlTableBuilder)sut).SetName( "U" );
        var statements = schema.Database.GetPendingStatements().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.Name.Should().Be( "U" );
            sut.FullName.Should().Be( "foo_U" );
            schema.Objects.Contains( "U" ).Should().BeTrue();
            schema.Objects.Contains( "T1" ).Should().BeFalse();
            fk1.IsRemoved.Should().BeFalse();

            statements.Should().HaveCount( 4 );

            statements.ElementAtOrDefault( 0 )
                .Should()
                .SatisfySql(
                    @"CREATE TABLE ""__foo_T3__{GUID}__"" (
                      ""C3"" ANY NOT NULL,
                      ""C4"" INTEGER NOT NULL DEFAULT (0),
                      CONSTRAINT ""foo_PK_T3"" PRIMARY KEY (""C3"" ASC)
                    ) WITHOUT ROWID;",
                    @"INSERT INTO ""__foo_T3__{GUID}__"" (""C3"", ""C4"")
                    SELECT ""C3"", 0
                    FROM ""foo_T3"";",
                    "DROP TABLE \"foo_T3\";",
                    "ALTER TABLE \"__foo_T3__{GUID}__\" RENAME TO \"foo_T3\";" );

            statements.ElementAtOrDefault( 1 )
                .Should()
                .SatisfySql(
                    @"CREATE TABLE ""__foo_T2__{GUID}__"" (
                      ""C2"" ANY NOT NULL,
                      CONSTRAINT ""foo_PK_T2"" PRIMARY KEY (""C2"" ASC)
                    ) WITHOUT ROWID;",
                    @"INSERT INTO ""__foo_T2__{GUID}__"" (""C2"")
                    SELECT ""C2""
                    FROM ""foo_T2"";",
                    "DROP TABLE \"foo_T2\";",
                    "ALTER TABLE \"__foo_T2__{GUID}__\" RENAME TO \"foo_T2\";" );

            statements.ElementAtOrDefault( 2 ).Should().SatisfySql( "ALTER TABLE \"foo_T1\" RENAME TO \"foo_U\";" );

            statements.ElementAtOrDefault( 3 )
                .Should()
                .SatisfySql(
                    @"CREATE TABLE ""__foo_T2__{GUID}__"" (
                      ""C2"" ANY NOT NULL,
                      CONSTRAINT ""foo_PK_T2"" PRIMARY KEY (""C2"" ASC),
                      CONSTRAINT ""foo_FK_T2_C2_REF_foo_T1"" FOREIGN KEY (""C2"") REFERENCES ""foo_U"" (""C1"") ON DELETE RESTRICT ON UPDATE RESTRICT
                    ) WITHOUT ROWID;",
                    @"INSERT INTO ""__foo_T2__{GUID}__"" (""C2"")
                    SELECT ""C2""
                    FROM ""foo_T2"";",
                    "DROP TABLE \"foo_T2\";",
                    "ALTER TABLE \"__foo_T2__{GUID}__\" RENAME TO \"foo_T2\";" );
        }
    }

    [Theory]
    [InlineData( " " )]
    [InlineData( "\"" )]
    [InlineData( "f\"oo" )]
    public void SetName_ShouldThrowSqliteObjectBuilderException_WhenNameIsInvalid(string name)
    {
        var db = new SqliteDatabaseBuilder();
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
        var schema = new SqliteDatabaseBuilder().Schemas.Default;
        var other = schema.Objects.CreateTable( name2 );
        other.SetPrimaryKey( other.Columns.Create( "C" ).Asc() );
        var sut = schema.Objects.CreateTable( name1 );

        var action = Lambda.Of( () => sut.SetName( name2 ) );

        action.Should()
            .ThrowExactly<SqliteObjectBuilderException>()
            .AndMatch( e => e.Dialect == SqliteDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void SetName_ShouldThrowSqliteObjectBuilderException_WhenTableHasBeenRemoved()
    {
        var schema = new SqliteDatabaseBuilder().Schemas.Default;
        var sut = schema.Objects.CreateTable( Fixture.Create<string>() );
        schema.Objects.Remove( sut.Name );

        var action = Lambda.Of( () => sut.SetName( Fixture.Create<string>() ) );

        action.Should()
            .ThrowExactly<SqliteObjectBuilderException>()
            .AndMatch( e => e.Dialect == SqliteDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void SetPrimaryKey_ShouldUpdatePrimaryKey_WhenTableDoesNotHaveOne()
    {
        var schema = new SqliteDatabaseBuilder().Schemas.Create( "foo" );
        var sut = schema.Objects.CreateTable( "T" );
        var column = sut.Columns.Create( "C" );

        var result = ((ISqlTableBuilder)sut).SetPrimaryKey( column.Asc() );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut.PrimaryKey );
            result.Name.Should().Be( "PK_T" );
            result.FullName.Should().Be( "foo_PK_T" );
            result.Type.Should().Be( SqlObjectType.PrimaryKey );
            result.Database.Should().BeSameAs( schema.Database );
            result.Index.Table.Should().BeSameAs( sut );
            result.Index.IsUnique.Should().BeTrue();
            result.Index.Name.Should().Be( "UIX_T_CA" );
            result.Index.FullName.Should().Be( "foo_UIX_T_CA" );
            result.Index.ForeignKeys.Should().BeEmpty();
            result.Index.ReferencingForeignKeys.Should().BeEmpty();
            result.Index.Columns.Should().BeSequentiallyEqualTo( column.Asc() );
            result.Index.PrimaryKey.Should().BeSameAs( result );
            result.Index.Type.Should().Be( SqlObjectType.Index );
            result.Index.Database.Should().BeSameAs( schema.Database );
        }
    }

    [Fact]
    public void SetPrimaryKey_ShouldDoNothing_WhenNewPrimaryKeyIsEquivalentToTheOldOne()
    {
        var schema = new SqliteDatabaseBuilder().Schemas.Create( "foo" );
        var sut = schema.Objects.CreateTable( "T" );
        var c1 = sut.Columns.Create( "C1" );
        var c2 = sut.Columns.Create( "C2" );
        var c3 = sut.Columns.Create( "C3" );
        var oldPk = sut.SetPrimaryKey( c1.Asc(), c2.Asc(), c3.Asc() );

        var result = ((ISqlTableBuilder)sut).SetPrimaryKey( c1.Asc(), c2.Asc(), c3.Asc() );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( oldPk );
            result.Should().BeSameAs( sut.PrimaryKey );
        }
    }

    [Fact]
    public void SetPrimaryKey_ShouldUpdatePrimaryKey_WhenNewPrimaryKeyHasDifferentAmountOfColumnsFromTheCurrentOne()
    {
        var schema = new SqliteDatabaseBuilder().Schemas.Create( "foo" );
        var sut = schema.Objects.CreateTable( "T" );
        var c1 = sut.Columns.Create( "C1" );
        var c2 = sut.Columns.Create( "C2" );
        var oldPk = sut.SetPrimaryKey( c1.Asc(), c2.Asc() ).SetName( "PK_OLD" );

        var startStatementCount = schema.Database.GetPendingStatements().Length;

        var result = sut.SetPrimaryKey( c1.Asc() );
        var statements = schema.Database.GetPendingStatements().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut.PrimaryKey );
            result.Name.Should().Be( "PK_OLD" );
            result.FullName.Should().Be( "foo_PK_OLD" );
            result.Type.Should().Be( SqlObjectType.PrimaryKey );
            result.Database.Should().BeSameAs( schema.Database );
            result.Index.Table.Should().BeSameAs( sut );
            result.Index.IsUnique.Should().BeTrue();
            result.Index.Name.Should().Be( "UIX_T_C1A" );
            result.Index.FullName.Should().Be( "foo_UIX_T_C1A" );
            result.Index.ForeignKeys.Should().BeEmpty();
            result.Index.ReferencingForeignKeys.Should().BeEmpty();
            result.Index.Columns.Should().BeSequentiallyEqualTo( c1.Asc() );
            result.Index.PrimaryKey.Should().BeSameAs( result );
            result.Index.Type.Should().Be( SqlObjectType.Index );
            result.Index.Database.Should().BeSameAs( schema.Database );
            oldPk.IsRemoved.Should().BeTrue();
            oldPk.Index.IsRemoved.Should().BeTrue();
            oldPk.Index.PrimaryKey.Should().BeNull();

            statements.Should().HaveCount( 1 );

            statements.ElementAtOrDefault( 0 )
                .Should()
                .SatisfySql(
                    @"CREATE TABLE ""__foo_T__{GUID}__"" (
                      ""C1"" ANY NOT NULL,
                      ""C2"" ANY NOT NULL,
                      CONSTRAINT ""foo_PK_OLD"" PRIMARY KEY (""C1"" ASC)
                    ) WITHOUT ROWID;",
                    @"INSERT INTO ""__foo_T__{GUID}__"" (""C1"", ""C2"")
                    SELECT ""C1"", ""C2""
                    FROM ""foo_T"";",
                    "DROP TABLE \"foo_T\";",
                    "ALTER TABLE \"__foo_T__{GUID}__\" RENAME TO \"foo_T\";" );
        }
    }

    [Fact]
    public void SetPrimaryKey_ShouldUpdatePrimaryKey_WhenNewPrimaryKeyHasTheSameAmountOfColumnsButDifferentAsCurrentOne()
    {
        var schema = new SqliteDatabaseBuilder().Schemas.Create( "foo" );
        var sut = schema.Objects.CreateTable( "T" );
        var c1 = sut.Columns.Create( "C1" );
        var c2 = sut.Columns.Create( "C2" );
        var oldPk = sut.SetPrimaryKey( c1.Asc(), c2.Asc() ).SetName( "PK_OLD" );

        var startStatementCount = schema.Database.GetPendingStatements().Length;

        var result = sut.SetPrimaryKey( c1.Asc(), c2.Desc() );
        var statements = schema.Database.GetPendingStatements().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut.PrimaryKey );
            result.Name.Should().Be( "PK_OLD" );
            result.FullName.Should().Be( "foo_PK_OLD" );
            result.Type.Should().Be( SqlObjectType.PrimaryKey );
            result.Database.Should().BeSameAs( schema.Database );
            result.Index.Table.Should().BeSameAs( sut );
            result.Index.IsUnique.Should().BeTrue();
            result.Index.Name.Should().Be( "UIX_T_C1A_C2D" );
            result.Index.FullName.Should().Be( "foo_UIX_T_C1A_C2D" );
            result.Index.ForeignKeys.Should().BeEmpty();
            result.Index.ReferencingForeignKeys.Should().BeEmpty();
            result.Index.Columns.Should().BeSequentiallyEqualTo( c1.Asc(), c2.Desc() );
            result.Index.PrimaryKey.Should().BeSameAs( result );
            result.Index.Type.Should().Be( SqlObjectType.Index );
            result.Index.Database.Should().BeSameAs( schema.Database );
            oldPk.IsRemoved.Should().BeTrue();
            oldPk.Index.IsRemoved.Should().BeTrue();
            oldPk.Index.PrimaryKey.Should().BeNull();

            statements.Should().HaveCount( 1 );

            statements.ElementAtOrDefault( 0 )
                .Should()
                .SatisfySql(
                    @"CREATE TABLE ""__foo_T__{GUID}__"" (
                      ""C1"" ANY NOT NULL,
                      ""C2"" ANY NOT NULL,
                      CONSTRAINT ""foo_PK_OLD"" PRIMARY KEY (""C1"" ASC, ""C2"" DESC)
                    ) WITHOUT ROWID;",
                    @"INSERT INTO ""__foo_T__{GUID}__"" (""C1"", ""C2"")
                    SELECT ""C1"", ""C2""
                    FROM ""foo_T"";",
                    "DROP TABLE \"foo_T\";",
                    "ALTER TABLE \"__foo_T__{GUID}__\" RENAME TO \"foo_T\";" );
        }
    }

    [Fact]
    public void SetPrimaryKey_ShouldUpdatePrimaryKey_WhenNewPrimaryKeyIsDifferentFromCurrentOneButUsesExistingIndex()
    {
        var schema = new SqliteDatabaseBuilder().Schemas.Create( "foo" );
        var sut = schema.Objects.CreateTable( "T" );
        var c1 = sut.Columns.Create( "C1" );
        var c2 = sut.Columns.Create( "C2" );
        var oldPk = sut.SetPrimaryKey( c1.Asc() ).SetName( "PK_OLD" );
        var ix = sut.Indexes.Create( c1.Asc(), c2.Desc() );

        var startStatementCount = schema.Database.GetPendingStatements().Length;

        var result = sut.SetPrimaryKey( c1.Asc(), c2.Desc() );
        var statements = schema.Database.GetPendingStatements().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut.PrimaryKey );
            result.Name.Should().Be( "PK_OLD" );
            result.FullName.Should().Be( "foo_PK_OLD" );
            result.Type.Should().Be( SqlObjectType.PrimaryKey );
            result.Database.Should().BeSameAs( schema.Database );
            result.Index.Should().BeSameAs( ix );
            ix.IsUnique.Should().BeTrue();
            oldPk.IsRemoved.Should().BeTrue();
            oldPk.Index.IsRemoved.Should().BeTrue();
            oldPk.Index.PrimaryKey.Should().BeNull();

            statements.Should().HaveCount( 1 );

            statements.ElementAtOrDefault( 0 )
                .Should()
                .SatisfySql(
                    "DROP INDEX \"foo_IX_T_C1A_C2D\";",
                    @"CREATE TABLE ""__foo_T__{GUID}__"" (
                      ""C1"" ANY NOT NULL,
                      ""C2"" ANY NOT NULL,
                      CONSTRAINT ""foo_PK_OLD"" PRIMARY KEY (""C1"" ASC, ""C2"" DESC)
                    ) WITHOUT ROWID;",
                    @"INSERT INTO ""__foo_T__{GUID}__"" (""C1"", ""C2"")
                    SELECT ""C1"", ""C2""
                    FROM ""foo_T"";",
                    "DROP TABLE \"foo_T\";",
                    "ALTER TABLE \"__foo_T__{GUID}__\" RENAME TO \"foo_T\";" );
        }
    }

    [Fact]
    public void SetPrimaryKey_ShouldUpdatePrimaryKeyAndRemoveSelfReferencingForeignKeysToCurrentOne()
    {
        var schema = new SqliteDatabaseBuilder().Schemas.Create( "foo" );
        var sut = schema.Objects.CreateTable( "T" );
        var c1 = sut.Columns.Create( "C1" );
        var c2 = sut.Columns.Create( "C2" );
        var oldPk = sut.SetPrimaryKey( c1.Asc() );
        var ix = sut.Indexes.Create( c2.Asc() );
        var fk = sut.ForeignKeys.Create( ix, oldPk.Index );

        var startStatementCount = schema.Database.GetPendingStatements().Length;

        var result = sut.SetPrimaryKey( c2.Asc() );
        var statements = schema.Database.GetPendingStatements().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut.PrimaryKey );
            result.Name.Should().Be( "PK_T" );
            result.FullName.Should().Be( "foo_PK_T" );
            result.Type.Should().Be( SqlObjectType.PrimaryKey );
            result.Database.Should().BeSameAs( schema.Database );
            result.Index.Should().BeSameAs( ix );
            oldPk.IsRemoved.Should().BeTrue();
            oldPk.Index.IsRemoved.Should().BeTrue();
            oldPk.Index.PrimaryKey.Should().BeNull();
            fk.IsRemoved.Should().BeTrue();

            statements.Should().HaveCount( 1 );

            statements.ElementAtOrDefault( 0 )
                .Should()
                .SatisfySql(
                    "DROP INDEX \"foo_IX_T_C2A\";",
                    @"CREATE TABLE ""__foo_T__{GUID}__"" (
                      ""C1"" ANY NOT NULL,
                      ""C2"" ANY NOT NULL,
                      CONSTRAINT ""foo_PK_T"" PRIMARY KEY (""C2"" ASC)
                    ) WITHOUT ROWID;",
                    @"INSERT INTO ""__foo_T__{GUID}__"" (""C1"", ""C2"")
                    SELECT ""C1"", ""C2""
                    FROM ""foo_T"";",
                    "DROP TABLE \"foo_T\";",
                    "ALTER TABLE \"__foo_T__{GUID}__\" RENAME TO \"foo_T\";" );
        }
    }

    [Fact]
    public void SetPrimaryKey_ShouldThrowSqliteObjectBuilderException_WhenCurrentPrimaryKeyIndexHasExternalReferences()
    {
        var schema = new SqliteDatabaseBuilder().Schemas.Create( "foo" );
        var sut = schema.Objects.CreateTable( "T" );
        var c1 = sut.Columns.Create( "C1" );
        var c2 = sut.Columns.Create( "C2" );
        var oldPk = sut.SetPrimaryKey( c1.Asc() );

        var t2 = schema.Objects.CreateTable( "T2" );
        var pk2 = t2.SetPrimaryKey( t2.Columns.Create( "C3" ).Asc() );
        t2.ForeignKeys.Create( pk2.Index, oldPk.Index );

        var action = Lambda.Of( () => sut.SetPrimaryKey( c2.Asc() ) );

        action.Should()
            .ThrowExactly<SqliteObjectBuilderException>()
            .AndMatch( e => e.Dialect == SqliteDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void SetPrimaryKey_ShouldThrowSqliteObjectCastException_WhenAtLeastOneColumnIsOfInvalidType()
    {
        var schema = new SqliteDatabaseBuilder().Schemas.Default;
        var sut = schema.Objects.CreateTable( "foo" );
        var c1 = sut.Columns.Create( "C1" );
        var c2 = Substitute.For<ISqlIndexColumnBuilder>();

        var action = Lambda.Of( () => sut.SetPrimaryKey( c1.Asc(), c2 ) );

        action.Should()
            .ThrowExactly<SqliteObjectCastException>()
            .AndMatch( e => e.Dialect == SqliteDialect.Instance && e.Expected == typeof( SqliteIndexColumnBuilder ) );
    }

    [Fact]
    public void SetPrimaryKey_ShouldThrowSqliteObjectBuilderException_WhenDefaultPrimaryKeyNameExists()
    {
        var schema = new SqliteDatabaseBuilder().Schemas.Default;
        var sut = schema.Objects.CreateTable( "foo" );
        var c1 = sut.Columns.Create( "C1" );
        var c2 = sut.Columns.Create( "C2" );
        sut.Indexes.Create( c2.Asc() ).SetName( "PK_foo" );

        var action = Lambda.Of( () => sut.SetPrimaryKey( c1.Asc() ) );

        action.Should()
            .ThrowExactly<SqliteObjectBuilderException>()
            .AndMatch( e => e.Dialect == SqliteDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void SetPrimaryKey_ShouldThrowSqliteObjectBuilderException_WhenDefaultUnderlyingIndexNameExists()
    {
        var schema = new SqliteDatabaseBuilder().Schemas.Default;
        var sut = schema.Objects.CreateTable( "foo" );
        var c1 = sut.Columns.Create( "C1" );
        var c2 = sut.Columns.Create( "C2" );
        sut.Indexes.Create( c2.Asc() ).SetName( "UIX_foo_C1A" );

        var action = Lambda.Of( () => sut.SetPrimaryKey( c1.Asc() ) );

        action.Should()
            .ThrowExactly<SqliteObjectBuilderException>()
            .AndMatch( e => e.Dialect == SqliteDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void SetPrimaryKey_ShouldThrowSqliteObjectBuilderException_WhenAtLeastOneColumnBelongsToAnotherTable()
    {
        var schema = new SqliteDatabaseBuilder().Schemas.Default;
        var otherTable = schema.Objects.CreateTable( "foo" );
        var c2 = otherTable.Columns.Create( "C2" );
        otherTable.SetPrimaryKey( c2.Asc() );

        var sut = schema.Objects.CreateTable( Fixture.Create<string>() );
        var c1 = sut.Columns.Create( "C1" );

        var action = Lambda.Of( () => sut.SetPrimaryKey( c1.Asc(), c2.Asc() ) );

        action.Should()
            .ThrowExactly<SqliteObjectBuilderException>()
            .AndMatch( e => e.Dialect == SqliteDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void SetPrimaryKey_ShouldThrowSqliteObjectBuilderException_WhenSomeColumnsAreDuplicated()
    {
        var schema = new SqliteDatabaseBuilder().Schemas.Default;
        var sut = schema.Objects.CreateTable( Fixture.Create<string>() );
        var c1 = sut.Columns.Create( "C1" );
        var c2 = sut.Columns.Create( "C2" );

        var action = Lambda.Of( () => sut.SetPrimaryKey( c1.Asc(), c2.Asc(), c1.Desc() ) );

        action.Should()
            .ThrowExactly<SqliteObjectBuilderException>()
            .AndMatch( e => e.Dialect == SqliteDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void SetPrimaryKey_ShouldThrowSqliteObjectBuilderException_WhenAtLeastOneColumnIsRemoved()
    {
        var schema = new SqliteDatabaseBuilder().Schemas.Default;
        var sut = schema.Objects.CreateTable( Fixture.Create<string>() );
        var c1 = sut.Columns.Create( "C1" );
        var c2 = sut.Columns.Create( "C2" );
        c2.Remove();

        var action = Lambda.Of( () => sut.SetPrimaryKey( c1.Asc(), c2.Asc() ) );

        action.Should()
            .ThrowExactly<SqliteObjectBuilderException>()
            .AndMatch( e => e.Dialect == SqliteDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void SetPrimaryKey_ShouldThrowSqliteObjectBuilderException_WhenAtLeastOneColumnIsNullable()
    {
        var schema = new SqliteDatabaseBuilder().Schemas.Default;
        var sut = schema.Objects.CreateTable( Fixture.Create<string>() );
        var c1 = sut.Columns.Create( "C1" );
        var c2 = sut.Columns.Create( "C2" ).MarkAsNullable();

        var action = Lambda.Of( () => sut.SetPrimaryKey( c1.Asc(), c2.Asc() ) );

        action.Should()
            .ThrowExactly<SqliteObjectBuilderException>()
            .AndMatch( e => e.Dialect == SqliteDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void SetPrimaryKey_ShouldThrowSqliteObjectBuilderException_WhenColumnsAreEmpty()
    {
        var schema = new SqliteDatabaseBuilder().Schemas.Default;
        var sut = schema.Objects.CreateTable( Fixture.Create<string>() );

        var action = Lambda.Of( () => sut.SetPrimaryKey() );

        action.Should()
            .ThrowExactly<SqliteObjectBuilderException>()
            .AndMatch( e => e.Dialect == SqliteDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void SetPrimaryKey_ShouldThrowSqliteObjectBuilderException_WhenTableHasBeenRemoved()
    {
        var schema = new SqliteDatabaseBuilder().Schemas.Default;
        var sut = schema.Objects.CreateTable( Fixture.Create<string>() );
        var column = sut.Columns.Create( "C" );
        schema.Objects.Remove( sut.Name );

        var action = Lambda.Of( () => sut.SetPrimaryKey( column.Asc() ) );

        action.Should()
            .ThrowExactly<SqliteObjectBuilderException>()
            .AndMatch( e => e.Dialect == SqliteDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void Remove_ShouldRemoveTable_WhenTableDoesNotHaveAnyExternalReferences()
    {
        var schema = new SqliteDatabaseBuilder().Schemas.Create( "foo" );
        var sut = schema.Objects.CreateTable( "T" );
        var column = sut.Columns.Create( "C" );
        var otherColumn = sut.Columns.Create( "D" );
        var pk = sut.SetPrimaryKey( column.Asc() );
        var ix = sut.Indexes.Create( otherColumn.Asc() );
        var fk = sut.ForeignKeys.Create( ix, pk.Index );

        var startStatementCount = schema.Database.GetPendingStatements().Length;

        sut.Remove();
        var statements = schema.Database.GetPendingStatements().Slice( startStatementCount ).ToArray();

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
            statements.Should().HaveCount( 1 );
            statements.ElementAtOrDefault( 0 ).Should().SatisfySql( "DROP TABLE \"foo_T\";" );
        }
    }

    [Fact]
    public void Remove_ShouldDoNothing_WhenTableHasAlreadyBeenRemoved()
    {
        var name = Fixture.Create<string>();
        var schema = new SqliteDatabaseBuilder().Schemas.Default;
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
    public void Remove_ShouldThrowSqliteObjectBuilderException_WhenTableHasExternalReferences()
    {
        var schema = new SqliteDatabaseBuilder().Schemas.Create( "foo" );
        var sut = schema.Objects.CreateTable( "T" );
        var column = sut.Columns.Create( "C" );
        var pk = sut.SetPrimaryKey( column.Asc() );

        var otherTable = schema.Objects.CreateTable( "U" );
        var otherColumn = otherTable.Columns.Create( "D" );
        var otherPk = otherTable.SetPrimaryKey( otherColumn.Asc() );
        otherTable.ForeignKeys.Create( otherPk.Index, pk.Index );

        var action = Lambda.Of( () => sut.Remove() );

        action.Should()
            .ThrowExactly<SqliteObjectBuilderException>()
            .AndMatch( e => e.Dialect == SqliteDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void ColumnNameSwap_ShouldGenerateCorrectScript()
    {
        var schema = new SqliteDatabaseBuilder().Schemas.Create( "foo" );
        var sut = schema.Objects.CreateTable( "T" );
        sut.SetPrimaryKey( sut.Columns.Create( "P" ).Asc() );
        var a = sut.Columns.Create( "A" );
        var b = sut.Columns.Create( "B" );

        var startStatementCount = schema.Database.GetPendingStatements().Length;

        a.SetName( "C" );
        b.SetName( "A" );
        a.SetName( "B" );

        var statements = schema.Database.GetPendingStatements().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            statements.Should().HaveCount( 1 );
            statements.ElementAtOrDefault( 0 )
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
        var schema = new SqliteDatabaseBuilder().Schemas.Create( "foo" );
        var sut = schema.Objects.CreateTable( "T" );
        sut.SetPrimaryKey( sut.Columns.Create( "P" ).Asc() );
        var a = sut.Columns.Create( "A" );
        var b = sut.Columns.Create( "B" );
        var c = sut.Columns.Create( "C" );
        var d = sut.Columns.Create( "D" );

        var startStatementCount = schema.Database.GetPendingStatements().Length;

        a.SetName( "E" );
        b.SetName( "A" );
        c.SetName( "B" );
        d.SetName( "C" );
        a.SetName( "D" );

        var statements = schema.Database.GetPendingStatements().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            statements.Should().HaveCount( 1 );
            statements.ElementAtOrDefault( 0 )
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
        var schema = new SqliteDatabaseBuilder().Schemas.Create( "foo" );
        var sut = schema.Objects.CreateTable( "T" );
        sut.SetPrimaryKey( sut.Columns.Create( "P" ).Asc() );
        var a = sut.Columns.Create( "A" );
        var b = sut.Columns.Create( "B" );
        var c = sut.Columns.Create( "C" );

        var startStatementCount = schema.Database.GetPendingStatements().Length;

        a.SetName( "X" );
        b.SetName( "Y" );
        c.SetName( "D" );
        b.SetName( "C" );
        a.SetName( "B" );

        var statements = schema.Database.GetPendingStatements().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            statements.Should().HaveCount( 1 );
            statements.ElementAtOrDefault( 0 )
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
        var schema = new SqliteDatabaseBuilder().Schemas.Create( "foo" );
        var sut = schema.Objects.CreateTable( "T" );
        sut.SetPrimaryKey( sut.Columns.Create( "C1" ).Asc() );
        var c2 = sut.Columns.Create( "C2" );
        var c3 = sut.Columns.Create( "C3" );
        var c4 = sut.Columns.Create( "C4" );
        var ix = sut.Indexes.Create( c2.Asc() );

        var startStatementCount = schema.Database.GetPendingStatements().Length;

        sut.SetName( "U" );
        c3.SetName( "X" );
        c4.Remove();
        ix.Remove();
        sut.Indexes.Create( c2.Asc(), c3.Desc() );

        var statements = schema.Database.GetPendingStatements().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            statements.Should().HaveCount( 1 );
            statements.ElementAtOrDefault( 0 )
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
    public void ForSqlite_ShouldInvokeAction_WhenTableIsSqlite()
    {
        var action = Substitute.For<Action<SqliteTableBuilder>>();
        var sut = new SqliteDatabaseBuilder().Schemas.Default.Objects.CreateTable( "T" );

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
