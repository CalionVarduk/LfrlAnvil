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

public partial class SqliteTableBuilderTests : TestsBase
{
    [Fact]
    public void ToString_ShouldReturnCorrectResult()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var sut = schema.Objects.CreateTable( "bar" );

        var result = sut.ToString();

        result.Should().Be( "[Table] foo_bar" );
    }

    [Fact]
    public void Creation_ShouldPrepareCorrectStatement()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var sut = schema.Objects.CreateTable( "T" );
        var ix1 = sut.Constraints.CreateIndex( sut.Columns.Create( "C1" ).Asc() );
        var ix2 = sut.Constraints.SetPrimaryKey( sut.Columns.Create( "C2" ).Asc() ).Index;
        sut.Constraints.CreateIndex( sut.Columns.Create( "C3" ).Asc(), sut.Columns.Create( "C4" ).Desc() );
        sut.Constraints.CreateForeignKey( ix1, ix2 );
        sut.Constraints.CreateCheck( sut.Node["C1"] > SqlNode.Literal( 0 ) );

        var actions = schema.Database.GetLastPendingActions( 0 );

        using ( new AssertionScope() )
        {
            schema.Objects.TryGet( sut.Name ).Should().BeSameAs( sut );
            schema.Database.Changes.ModifiedTables.Should().BeEquivalentTo( sut );
            sut.Name.Should().Be( "T" );

            actions.Should().HaveCount( 1 );
            actions.ElementAtOrDefault( 0 )
                .Sql.Should()
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
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );

        var actionCount = schema.Database.GetPendingActionCount();
        var sut = schema.Objects.CreateTable( "T" );
        sut.Remove();
        var actions = schema.Database.GetLastPendingActions( actionCount );

        actions.Should().BeEmpty();
    }

    [Fact]
    public void Creation_ShouldThrowSqlObjectBuilderException_WhenTableDoesNotHavePrimaryKeyDuringScriptResolution()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var sut = schema.Objects.CreateTable( "T" );
        sut.Columns.Create( "C" );

        var action = Lambda.Of( () => schema.Database.Changes.CompletePendingChanges() );

        action.Should()
            .ThrowExactly<SqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == SqliteDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void SetName_ShouldDoNothing_WhenNewNameEqualsOldName()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var sut = schema.Objects.CreateTable( "T" );
        sut.Constraints.SetPrimaryKey( sut.Columns.Create( "C" ).Asc() );

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.SetName( sut.Name );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            actions.Should().BeEmpty();
        }
    }

    [Fact]
    public void SetName_ShouldDoNothing_WhenNameChangeIsFollowedByChangeToOriginal()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var sut = schema.Objects.CreateTable( "T" );
        sut.Constraints.SetPrimaryKey( sut.Columns.Create( "C" ).Asc() );
        var oldName = sut.Name;

        var actionCount = schema.Database.GetPendingActionCount();
        sut.SetName( "bar" );
        var result = sut.SetName( oldName );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            actions.Should().BeEmpty();
        }
    }

    [Fact]
    public void SetName_ShouldUpdateName_WhenNewNameIsDifferentFromOldName()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var sut = schema.Objects.CreateTable( "T" );
        sut.Constraints.SetPrimaryKey( sut.Columns.Create( "C" ).Asc() );
        var oldName = sut.Name;
        var recordSet = sut.Node;
        _ = sut.Info;

        var actionCount = schema.Database.GetPendingActionCount();
        schema.Database.Changes.ClearModifiedTables();
        var result = sut.SetName( "bar" );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.Name.Should().Be( "bar" );
            sut.Info.Should().Be( SqlRecordSetInfo.Create( "foo", "bar" ) );
            recordSet.Info.Should().Be( sut.Info );
            schema.Objects.TryGet( "bar" ).Should().BeSameAs( sut );
            schema.Objects.TryGet( oldName ).Should().BeNull();
            schema.Database.Changes.ModifiedTables.Should().BeEmpty();

            actions.Should().HaveCount( 1 );
            actions.ElementAtOrDefault( 0 ).Sql.Should().SatisfySql( "ALTER TABLE \"foo_T\" RENAME TO \"foo_bar\";" );
        }
    }

    [Fact]
    public void SetName_ShouldUpdateName_WhenNameChangesAndTableHasSelfReference()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );

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
        schema.Database.Changes.ClearModifiedTables();
        var result = sut.SetName( "bar" );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.Name.Should().Be( "bar" );
            schema.Objects.TryGet( "bar" ).Should().BeSameAs( sut );
            schema.Objects.TryGet( "T" ).Should().BeNull();
            schema.Database.Changes.ModifiedTables.Should().BeEquivalentTo( sut );
            fk1.IsRemoved.Should().BeFalse();
            fk2.IsRemoved.Should().BeFalse();

            actions.Should().HaveCount( 2 );
            actions.ElementAtOrDefault( 0 ).Sql.Should().SatisfySql( "ALTER TABLE \"foo_T\" RENAME TO \"foo_bar\";" );
            actions.ElementAtOrDefault( 1 )
                .Sql.Should()
                .SatisfySql(
                    "DROP INDEX \"foo_IX_T_C2A\";",
                    @"CREATE TABLE ""__foo_bar__{GUID}__"" (
                      ""C1"" ANY NOT NULL,
                      ""C2"" ANY NOT NULL,
                      CONSTRAINT ""foo_PK_T"" PRIMARY KEY (""C1"" ASC),
                      CONSTRAINT ""foo_FK_T_C1_REF_U"" FOREIGN KEY (""C1"") REFERENCES ""U"" (""C"") ON DELETE RESTRICT ON UPDATE RESTRICT,
                      CONSTRAINT ""foo_FK_T_C2_REF_T"" FOREIGN KEY (""C2"") REFERENCES ""foo_bar"" (""C1"") ON DELETE RESTRICT ON UPDATE RESTRICT,
                      CONSTRAINT ""foo_CHK_T_{GUID}"" CHECK (""C1"" > 0)
                    ) WITHOUT ROWID;",
                    @"INSERT INTO ""__foo_bar__{GUID}__"" (""C1"", ""C2"")
                    SELECT
                      ""foo_bar"".""C1"",
                      ""foo_bar"".""C2""
                    FROM ""foo_bar"";",
                    "DROP TABLE \"foo_bar\";",
                    "ALTER TABLE \"__foo_bar__{GUID}__\" RENAME TO \"foo_bar\";",
                    "CREATE INDEX \"foo_IX_T_C2A\" ON \"foo_bar\" (\"C2\" ASC);" );
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

        var actionCount = schema.Database.GetPendingActionCount();
        schema.Database.Changes.ClearModifiedTables();
        var result = sut.SetName( "U" );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.Name.Should().Be( "U" );
            schema.Objects.TryGet( "U" ).Should().BeSameAs( sut );
            schema.Objects.TryGet( "T" ).Should().BeNull();
            schema.Database.Changes.ModifiedTables.Should().BeEquivalentTo( t2, t3 );
            fk1.IsRemoved.Should().BeFalse();
            fk2.IsRemoved.Should().BeFalse();
            fk3.IsRemoved.Should().BeFalse();

            actions.Should().HaveCount( 3 );

            actions.ElementAtOrDefault( 0 ).Sql.Should().SatisfySql( "ALTER TABLE \"foo_T1\" RENAME TO \"foo_U\";" );

            actions.ElementAtOrDefault( 1 )
                .Sql.Should()
                .SatisfySql(
                    "DROP INDEX \"foo_IX_T3_C4A\";",
                    @"CREATE TABLE ""__foo_T3__{GUID}__"" (
                      ""C3"" ANY NOT NULL,
                      ""C4"" ANY NOT NULL,
                      CONSTRAINT ""foo_PK_T3"" PRIMARY KEY (""C3"" ASC),
                      CONSTRAINT ""foo_FK_T3_C3_REF_T1"" FOREIGN KEY (""C3"") REFERENCES ""foo_U"" (""C1"") ON DELETE RESTRICT ON UPDATE RESTRICT,
                      CONSTRAINT ""foo_FK_T3_C4_REF_T1"" FOREIGN KEY (""C4"") REFERENCES ""foo_U"" (""C1"") ON DELETE RESTRICT ON UPDATE RESTRICT
                    ) WITHOUT ROWID;",
                    @"INSERT INTO ""__foo_T3__{GUID}__"" (""C3"", ""C4"")
                    SELECT
                      ""foo_T3"".""C3"",
                      ""foo_T3"".""C4""
                    FROM ""foo_T3"";",
                    "DROP TABLE \"foo_T3\";",
                    "ALTER TABLE \"__foo_T3__{GUID}__\" RENAME TO \"foo_T3\";",
                    "CREATE INDEX \"foo_IX_T3_C4A\" ON \"foo_T3\" (\"C4\" ASC);" );

            actions.ElementAtOrDefault( 2 )
                .Sql.Should()
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

        var actionCount = schema.Database.GetPendingActionCount();
        schema.Database.Changes.ClearModifiedTables();
        sut.Columns.Create( "C3" ).SetType<int>();
        var result = sut.SetName( "U" );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.Name.Should().Be( "U" );
            schema.Objects.TryGet( "U" ).Should().BeSameAs( sut );
            schema.Objects.TryGet( "T1" ).Should().BeNull();
            schema.Database.Changes.ModifiedTables.Should().BeEquivalentTo( sut, t2 );
            fk1.IsRemoved.Should().BeFalse();

            actions.Should().HaveCount( 3 );

            actions.ElementAtOrDefault( 0 ).Sql.Should().SatisfySql( "ALTER TABLE \"foo_T1\" RENAME TO \"foo_U\";" );

            actions.ElementAtOrDefault( 1 )
                .Sql.Should()
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

            actions.ElementAtOrDefault( 2 )
                .Sql.Should()
                .SatisfySql(
                    @"CREATE TABLE ""__foo_U__{GUID}__"" (
                      ""C1"" ANY NOT NULL,
                      ""C3"" INTEGER NOT NULL DEFAULT (0),
                      CONSTRAINT ""foo_PK_T1"" PRIMARY KEY (""C1"" ASC)
                    ) WITHOUT ROWID;",
                    @"INSERT INTO ""__foo_U__{GUID}__"" (""C1"", ""C3"")
                    SELECT
                      ""foo_U"".""C1"",
                      0 AS ""C3""
                    FROM ""foo_U"";",
                    "DROP TABLE \"foo_U\";",
                    "ALTER TABLE \"__foo_U__{GUID}__\" RENAME TO \"foo_U\";" );
        }
    }

    [Fact]
    public void SetName_ShouldUpdateName_WhenNameChangesAndTableHasViewReferences()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var sut = schema.Objects.CreateTable( "T" );
        sut.Constraints.SetPrimaryKey( sut.Columns.Create( "C" ).Asc() );

        var v1 = schema.Objects.CreateView( "V1", sut.Node.ToDataSource().Select( s => new[] { s.From["C"].AsSelf() } ) );
        var v2 = schema.Objects.CreateView( "V2", v1.Node.Join( sut.Node.InnerOn( SqlNode.True() ) ).Select( s => new[] { s.GetAll() } ) );
        var v3 = schema.Objects.CreateView( "V3", v1.ToRecordSet().ToDataSource().Select( s => new[] { s.GetAll() } ) );

        var actionCount = schema.Database.GetPendingActionCount();
        schema.Database.Changes.ClearModifiedTables();
        var result = sut.SetName( "U" );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.Name.Should().Be( "U" );
            schema.Objects.TryGet( "U" ).Should().BeSameAs( sut );
            schema.Objects.TryGet( "T" ).Should().BeNull();
            schema.Database.Changes.ModifiedTables.Should().BeEmpty();
            v1.IsRemoved.Should().BeFalse();
            v2.IsRemoved.Should().BeFalse();
            v3.IsRemoved.Should().BeFalse();

            actions.Should().HaveCount( 3 );

            actions.ElementAtOrDefault( 0 ).Sql.Should().SatisfySql( "ALTER TABLE \"foo_T\" RENAME TO \"foo_U\";" );

            actions.ElementAtOrDefault( 1 )
                .Sql.Should()
                .SatisfySql(
                    "DROP VIEW \"foo_V1\";",
                    @"CREATE VIEW ""foo_V1"" AS
                    SELECT
                      ""foo_U"".""C""
                    FROM ""foo_U"";" );

            actions.ElementAtOrDefault( 2 )
                .Sql.Should()
                .SatisfySql(
                    "DROP VIEW \"foo_V2\";",
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

        var v1 = schema.Objects.CreateView( "V1", sut.Node.ToDataSource().Select( s => new[] { s.From["C"].AsSelf() } ) );
        var v2 = schema.Objects.CreateView( "V2", v1.Node.Join( sut.Node.InnerOn( SqlNode.True() ) ).Select( s => new[] { s.GetAll() } ) );
        var v3 = schema.Objects.CreateView( "V3", v1.ToRecordSet().ToDataSource().Select( s => new[] { s.GetAll() } ) );

        var actionCount = schema.Database.GetPendingActionCount();
        schema.Database.Changes.ClearModifiedTables();
        sut.Columns.Create( "D" ).SetType<int>();
        var result = sut.SetName( "U" );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.Name.Should().Be( "U" );
            schema.Objects.TryGet( "U" ).Should().BeSameAs( sut );
            schema.Objects.TryGet( "T" ).Should().BeNull();
            schema.Database.Changes.ModifiedTables.Should().BeEquivalentTo( sut );
            v1.IsRemoved.Should().BeFalse();
            v2.IsRemoved.Should().BeFalse();
            v3.IsRemoved.Should().BeFalse();

            actions.Should().HaveCount( 4 );

            actions.ElementAtOrDefault( 0 ).Sql.Should().SatisfySql( "ALTER TABLE \"foo_T\" RENAME TO \"foo_U\";" );

            actions.ElementAtOrDefault( 1 )
                .Sql.Should()
                .SatisfySql(
                    "DROP VIEW \"foo_V1\";",
                    @"CREATE VIEW ""foo_V1"" AS
                    SELECT
                      ""foo_U"".""C""
                    FROM ""foo_U"";" );

            actions.ElementAtOrDefault( 2 )
                .Sql.Should()
                .SatisfySql(
                    "DROP VIEW \"foo_V2\";",
                    @"CREATE VIEW ""foo_V2"" AS
                    SELECT
                      *
                    FROM ""foo_V1""
                    INNER JOIN ""foo_U"" ON TRUE;" );

            actions.ElementAtOrDefault( 3 )
                .Sql.Should()
                .SatisfySql(
                    @"CREATE TABLE ""__foo_U__{GUID}__"" (
                      ""C"" ANY NOT NULL,
                      ""D"" INTEGER NOT NULL DEFAULT (0),
                      CONSTRAINT ""foo_PK_T"" PRIMARY KEY (""C"" ASC)
                    ) WITHOUT ROWID;",
                    @"INSERT INTO ""__foo_U__{GUID}__"" (""C"", ""D"")
                    SELECT
                      ""foo_U"".""C"",
                      0 AS ""D""
                    FROM ""foo_U"";",
                    "DROP TABLE \"foo_U\";",
                    "ALTER TABLE \"__foo_U__{GUID}__\" RENAME TO \"foo_U\";" );
        }
    }

    [Theory]
    [InlineData( "" )]
    [InlineData( " " )]
    [InlineData( "\"" )]
    [InlineData( "'" )]
    [InlineData( "f\"oo" )]
    public void SetName_ShouldThrowSqlObjectBuilderException_WhenNameIsInvalid(string name)
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var sut = schema.Objects.CreateTable( "T" );

        var action = Lambda.Of( () => sut.SetName( name ) );

        action.Should()
            .ThrowExactly<SqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == SqliteDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void SetName_ShouldThrowSqlObjectBuilderException_WhenTableIsRemoved()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var sut = schema.Objects.CreateTable( "T" );
        sut.Remove();

        var action = Lambda.Of( () => sut.SetName( "bar" ) );

        action.Should()
            .ThrowExactly<SqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == SqliteDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void SetName_ShouldThrowSqlObjectBuilderException_WhenNewNameAlreadyExistsInSchemaObjects()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var sut = schema.Objects.CreateTable( "T" );
        sut.Constraints.SetPrimaryKey( sut.Columns.Create( "C" ).Asc() );

        var action = Lambda.Of( () => sut.SetName( "PK_T" ) );

        action.Should()
            .ThrowExactly<SqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == SqliteDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void Remove_ShouldRemoveTableAndQuickRemoveColumnsAndConstraints()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
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

        using ( new AssertionScope() )
        {
            schema.Objects.TryGet( sut.Name ).Should().BeNull();
            schema.Objects.TryGet( pk.Name ).Should().BeNull();
            schema.Objects.TryGet( pk.Index.Name ).Should().BeNull();
            schema.Objects.TryGet( ix.Name ).Should().BeNull();
            schema.Objects.TryGet( selfFk.Name ).Should().BeNull();
            schema.Objects.TryGet( externalFk.Name ).Should().BeNull();
            schema.Objects.TryGet( chk.Name ).Should().BeNull();
            schema.Database.Changes.ModifiedTables.Should().BeEquivalentTo( otherTable );

            sut.IsRemoved.Should().BeTrue();
            sut.ReferencingObjects.Should().BeEmpty();
            sut.Columns.Should().BeEmpty();
            sut.Constraints.Should().BeEmpty();
            sut.Constraints.TryGetPrimaryKey().Should().BeNull();
            c1.IsRemoved.Should().BeTrue();
            c1.ReferencingObjects.Should().BeEmpty();
            c2.IsRemoved.Should().BeTrue();
            c2.ReferencingObjects.Should().BeEmpty();
            pk.IsRemoved.Should().BeTrue();
            pk.ReferencingObjects.Should().BeEmpty();
            pk.Index.IsRemoved.Should().BeTrue();
            pk.Index.ReferencingObjects.Should().BeEmpty();
            pk.Index.Columns.Should().BeEmpty();
            pk.Index.PrimaryKey.Should().BeNull();
            ix.IsRemoved.Should().BeTrue();
            ix.ReferencingObjects.Should().BeEmpty();
            ix.Columns.Should().BeEmpty();
            selfFk.IsRemoved.Should().BeTrue();
            selfFk.ReferencingObjects.Should().BeEmpty();
            externalFk.IsRemoved.Should().BeTrue();
            externalFk.ReferencingObjects.Should().BeEmpty();
            chk.IsRemoved.Should().BeTrue();
            chk.ReferencingObjects.Should().BeEmpty();
            chk.ReferencedColumns.Should().BeEmpty();

            otherPk.Index.ReferencingObjects.Should().BeEmpty();
            otherTable.ReferencingObjects.Should().BeEmpty();

            actions.Should().HaveCount( 1 );
            actions.ElementAtOrDefault( 0 ).Sql.Should().SatisfySql( "DROP TABLE \"foo_T\";" );
        }
    }

    [Fact]
    public void Remove_ShouldRemoveTable_ByOldName()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var sut = schema.Objects.CreateTable( "T" );
        var c1 = sut.Columns.Create( "C1" );
        var pk = sut.Constraints.SetPrimaryKey( c1.Asc() );

        var actionCount = schema.Database.GetPendingActionCount();
        sut.SetName( "bar" ).Remove();
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            schema.Objects.TryGet( sut.Name ).Should().BeNull();
            schema.Objects.TryGet( pk.Name ).Should().BeNull();
            schema.Objects.TryGet( pk.Index.Name ).Should().BeNull();
            schema.Database.Changes.ModifiedTables.Should().BeEmpty();

            sut.IsRemoved.Should().BeTrue();
            sut.ReferencingObjects.Should().BeEmpty();
            sut.Columns.Should().BeEmpty();
            sut.Constraints.Should().BeEmpty();
            sut.Constraints.TryGetPrimaryKey().Should().BeNull();
            c1.IsRemoved.Should().BeTrue();
            c1.ReferencingObjects.Should().BeEmpty();
            pk.IsRemoved.Should().BeTrue();
            pk.ReferencingObjects.Should().BeEmpty();
            pk.Index.IsRemoved.Should().BeTrue();
            pk.Index.ReferencingObjects.Should().BeEmpty();
            pk.Index.Columns.Should().BeEmpty();
            pk.Index.PrimaryKey.Should().BeNull();

            actions.Should().HaveCount( 1 );
            actions.ElementAtOrDefault( 0 ).Sql.Should().SatisfySql( "DROP TABLE \"foo_T\";" );
        }
    }

    [Fact]
    public void Remove_ShouldDoNothing_WhenTableHasAlreadyBeenRemoved()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var sut = schema.Objects.CreateTable( "T" );
        sut.Constraints.SetPrimaryKey( sut.Columns.Create( "C" ).Asc() );

        schema.Database.Changes.CompletePendingChanges();
        sut.Remove();

        var actionCount = schema.Database.GetPendingActionCount();
        sut.Remove();
        var actions = schema.Database.GetLastPendingActions( actionCount );

        actions.Should().BeEmpty();
    }

    [Fact]
    public void Remove_ShouldThrowSqlObjectBuilderException_WhenTableIsReferencedByAnyExternalForeignKey()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var sut = schema.Objects.CreateTable( "T" );
        var pk = sut.Constraints.SetPrimaryKey( sut.Columns.Create( "C" ).Asc() );

        var otherTable = schema.Objects.CreateTable( "U" );
        var otherPk = otherTable.Constraints.SetPrimaryKey( otherTable.Columns.Create( "D" ).Asc() );
        otherTable.Constraints.CreateForeignKey( otherPk.Index, pk.Index );

        var action = Lambda.Of( () => sut.Remove() );

        action.Should()
            .ThrowExactly<SqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == SqliteDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void Remove_ShouldThrowSqlObjectBuilderException_WhenTableIsReferencedByAnyView()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var sut = schema.Objects.CreateTable( "T" );
        sut.Constraints.SetPrimaryKey( sut.Columns.Create( "C" ).Asc() );
        schema.Objects.CreateView( "V", sut.Node.ToDataSource().Select( s => new[] { s.GetAll() } ) );

        var action = Lambda.Of( () => sut.Remove() );

        action.Should()
            .ThrowExactly<SqlObjectBuilderException>()
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

        var actionCount = schema.Database.GetPendingActionCount();
        a.SetName( "C" );
        b.SetName( "A" );
        a.SetName( "B" );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            actions.Should().HaveCount( 1 );
            actions.ElementAtOrDefault( 0 )
                .Sql.Should()
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

        var actionCount = schema.Database.GetPendingActionCount();
        a.SetName( "E" );
        b.SetName( "A" );
        c.SetName( "B" );
        d.SetName( "C" );
        a.SetName( "D" );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            actions.Should().HaveCount( 1 );
            actions.ElementAtOrDefault( 0 )
                .Sql.Should()
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

        var actionCount = schema.Database.GetPendingActionCount();
        a.SetName( "X" );
        b.SetName( "Y" );
        c.SetName( "D" );
        b.SetName( "C" );
        a.SetName( "B" );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            actions.Should().HaveCount( 1 );
            actions.ElementAtOrDefault( 0 )
                .Sql.Should()
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

        var actionCount = schema.Database.GetPendingActionCount();
        sut.SetName( "U" );
        c3.SetName( "X" );
        c4.Remove();
        ix.Remove();
        sut.Constraints.CreateIndex( c2.Asc(), c3.Desc() );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            actions.Should().HaveCount( 2 );
            actions.ElementAtOrDefault( 0 ).Sql.Should().SatisfySql( "ALTER TABLE \"foo_T\" RENAME TO \"foo_U\";" );
            actions.ElementAtOrDefault( 1 )
                .Sql.Should()
                .SatisfySql(
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
