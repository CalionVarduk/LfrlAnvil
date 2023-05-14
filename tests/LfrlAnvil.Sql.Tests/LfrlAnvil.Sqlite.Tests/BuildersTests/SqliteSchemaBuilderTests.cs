using System.Linq;
using LfrlAnvil.Functional;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.Sqlite.Exceptions;
using LfrlAnvil.Sqlite.Extensions;
using LfrlAnvil.Sqlite.Objects.Builders;
using LfrlAnvil.Sqlite.Tests.Helpers;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.Sqlite.Tests.BuildersTests;

public partial class SqliteSchemaBuilderTests : TestsBase
{
    [Fact]
    public void ToString_ShouldReturnCorrectResult()
    {
        var db = new SqliteDatabaseBuilder();
        var sut = db.Schemas.Create( "foo" );

        var result = sut.ToString();

        result.Should().Be( "[Schema] foo" );
    }

    [Fact]
    public void SetName_ShouldDoNothing_WhenNewNameEqualsOldName()
    {
        var name = Fixture.Create<string>();
        var db = new SqliteDatabaseBuilder();
        var sut = db.Schemas.Create( name );

        var result = ((ISqlObjectBuilder)sut).SetName( name );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.Name.Should().Be( name );
            sut.FullName.Should().Be( name );
            db.Schemas.Contains( name ).Should().BeTrue();
        }
    }

    [Fact]
    public void SetName_ShouldUpdateName_WhenNameChangesAndSchemaDoesNotHaveAnyObjects()
    {
        var (oldName, newName) = Fixture.CreateDistinctCollection<string>( count: 2 );
        var db = new SqliteDatabaseBuilder();
        var sut = db.Schemas.Create( oldName );

        var result = ((ISqlSchemaBuilder)sut).SetName( newName );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.Name.Should().Be( newName );
            sut.FullName.Should().Be( newName );
            db.Schemas.Contains( newName ).Should().BeTrue();
            db.Schemas.Contains( oldName ).Should().BeFalse();
        }
    }

    [Fact]
    public void SetName_ShouldUpdateName_WhenNameChangesAndSchemaHasObjects()
    {
        var (oldName, newName) = ("foo", "bar");
        var db = new SqliteDatabaseBuilder();
        var sut = db.Schemas.Create( oldName );

        var t1 = sut.Objects.CreateTable( "T1" );
        var c1 = t1.Columns.Create( "C1" );
        var c2 = t1.Columns.Create( "C2" ).MarkAsNullable();
        var pk1 = t1.SetPrimaryKey( c1.Asc() );
        var ix1 = t1.Indexes.Create( c2.Asc() );
        var fk1 = t1.ForeignKeys.Create( ix1, pk1.Index );

        var t2 = sut.Objects.CreateTable( "T2" );
        var c3 = t2.Columns.Create( "C3" );
        var pk2 = t2.SetPrimaryKey( c3.Asc() );
        var fk2 = t2.ForeignKeys.Create( pk2.Index, pk1.Index );

        var t3 = sut.Objects.CreateTable( "T3" );
        var c4 = t3.Columns.Create( "C4" );
        var pk3 = t3.SetPrimaryKey( c4.Asc() );

        var startStatementCount = db.GetPendingStatements().Length;

        var result = ((ISqlSchemaBuilder)sut).SetName( newName );
        var statements = db.GetPendingStatements().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.Name.Should().Be( newName );
            sut.FullName.Should().Be( newName );
            db.Schemas.Contains( newName ).Should().BeTrue();
            db.Schemas.Contains( oldName ).Should().BeFalse();
            t1.FullName.Should().Be( "bar_T1" );
            t2.FullName.Should().Be( "bar_T2" );
            t3.FullName.Should().Be( "bar_T3" );
            c1.FullName.Should().Be( "bar_T1.C1" );
            c2.FullName.Should().Be( "bar_T1.C2" );
            c3.FullName.Should().Be( "bar_T2.C3" );
            c4.FullName.Should().Be( "bar_T3.C4" );
            pk1.FullName.Should().Be( "bar_PK_T1" );
            pk2.FullName.Should().Be( "bar_PK_T2" );
            pk3.FullName.Should().Be( "bar_PK_T3" );
            pk1.Index.FullName.Should().Be( "bar_UIX_T1_C1A" );
            pk2.Index.FullName.Should().Be( "bar_UIX_T2_C3A" );
            pk3.Index.FullName.Should().Be( "bar_UIX_T3_C4A" );
            ix1.FullName.Should().Be( "bar_IX_T1_C2A" );
            fk1.FullName.Should().Be( "bar_FK_T1_C2_REF_foo_T1" );
            fk2.FullName.Should().Be( "bar_FK_T2_C3_REF_foo_T1" );
            statements.Should().Contain( s => s.Contains( "DROP INDEX \"foo_IX_T1_C2A\";" ) );
            statements.Should().Contain( s => s.Contains( "CREATE INDEX \"bar_IX_T1_C2A\" ON \"bar_T1\" (\"C2\" ASC);" ) );
            statements.Should().Contain( s => s.Contains( "ALTER TABLE \"foo_T1\" RENAME TO \"bar_T1\";" ) );
            statements.Should().Contain( s => s.Contains( "ALTER TABLE \"foo_T2\" RENAME TO \"bar_T2\";" ) );
            statements.Should().Contain( s => s.Contains( "ALTER TABLE \"foo_T3\" RENAME TO \"bar_T3\";" ) );
        }
    }

    [Theory]
    [InlineData( " " )]
    [InlineData( "\"" )]
    [InlineData( "'" )]
    [InlineData( "f\"oo" )]
    public void SetName_ShouldThrowSqliteObjectBuilderException_WhenNameIsInvalid(string name)
    {
        var db = new SqliteDatabaseBuilder();
        var sut = db.Schemas.Create( Fixture.Create<string>() );

        var action = Lambda.Of( () => sut.SetName( name ) );

        action.Should()
            .ThrowExactly<SqliteObjectBuilderException>()
            .AndMatch( e => e.Dialect == SqliteDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void SetName_ShouldThrowSqliteObjectBuilderException_WhenSchemaWithNameAlreadyExists()
    {
        var (name1, name2) = Fixture.CreateDistinctCollection<string>( count: 2 );
        var db = new SqliteDatabaseBuilder();
        db.Schemas.Create( name2 );
        var sut = db.Schemas.Create( name1 );

        var action = Lambda.Of( () => sut.SetName( name2 ) );

        action.Should()
            .ThrowExactly<SqliteObjectBuilderException>()
            .AndMatch( e => e.Dialect == SqliteDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void SetName_ShouldThrowSqliteObjectBuilderException_WhenSchemaHasBeenRemoved()
    {
        var db = new SqliteDatabaseBuilder();
        var sut = db.Schemas.Create( Fixture.Create<string>() );
        db.Schemas.Remove( sut.Name );

        var action = Lambda.Of( () => sut.SetName( Fixture.Create<string>() ) );

        action.Should()
            .ThrowExactly<SqliteObjectBuilderException>()
            .AndMatch( e => e.Dialect == SqliteDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void Remove_ShouldRemoveSchema_WhenSchemaDoesNotHaveAnyObjects()
    {
        var name = Fixture.Create<string>();
        var db = new SqliteDatabaseBuilder();
        var sut = db.Schemas.Create( name );

        sut.Remove();

        using ( new AssertionScope() )
        {
            db.Schemas.Contains( name ).Should().BeFalse();
            sut.IsRemoved.Should().BeTrue();
        }
    }

    [Fact]
    public void Remove_ShouldDoNothing_WhenSchemaHasAlreadyBeenRemoved()
    {
        var name = Fixture.Create<string>();
        var db = new SqliteDatabaseBuilder();
        var sut = db.Schemas.Create( name );
        sut.Remove();

        sut.Remove();

        using ( new AssertionScope() )
        {
            db.Schemas.Contains( name ).Should().BeFalse();
            sut.IsRemoved.Should().BeTrue();
        }
    }

    [Fact]
    public void Remove_ShouldRemoveSchemaAndAllOfItsObjects_WhenSchemaHasTablesWithoutReferencesFromOtherSchemas()
    {
        var name = "foo";
        var db = new SqliteDatabaseBuilder();
        var sut = db.Schemas.Create( name );

        var t1 = sut.Objects.CreateTable( "T1" );
        var c1 = t1.Columns.Create( "C1" );
        var c2 = t1.Columns.Create( "C2" ).MarkAsNullable();
        var pk1 = t1.SetPrimaryKey( c1.Asc() );
        var ix1 = t1.Indexes.Create( c2.Asc() );
        var fk1 = t1.ForeignKeys.Create( ix1, pk1.Index );

        var t2 = sut.Objects.CreateTable( "T2" );
        var c3 = t2.Columns.Create( "C3" );
        var c4 = t2.Columns.Create( "C4" );
        var pk2 = t2.SetPrimaryKey( c3.Asc() );
        var ix2 = t2.Indexes.Create( c4.Asc() );
        var fk2 = t2.ForeignKeys.Create( ix2, pk1.Index );

        var t3 = sut.Objects.CreateTable( "T3" );
        var c5 = t3.Columns.Create( "C5" );
        var c6 = t3.Columns.Create( "C6" );
        var pk3 = t3.SetPrimaryKey( c5.Asc() );
        var fk3 = t3.ForeignKeys.Create( pk3.Index, pk2.Index );

        var t4 = sut.Objects.CreateTable( "T4" );
        var c7 = t4.Columns.Create( "C7" );
        var pk4 = t4.SetPrimaryKey( c7.Asc() );
        var fk4 = t4.ForeignKeys.Create( pk4.Index, pk3.Index );

        var ix3 = t3.Indexes.Create( c6.Asc() );
        var fk5 = t3.ForeignKeys.Create( ix3, pk4.Index );

        var startStatementCount = db.GetPendingStatements().Length;

        sut.Remove();
        var statements = db.GetPendingStatements().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            db.Schemas.Contains( name ).Should().BeFalse();
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
            sut.Objects.Should().BeEmpty();

            statements.Should().HaveCount( 5 );

            statements.ElementAtOrDefault( 0 )
                .Should()
                .SatisfySql(
                    "DROP INDEX \"foo_IX_T3_C6A\";",
                    @"CREATE TABLE ""__foo_T3__{GUID}__"" (
                      ""C5"" ANY NOT NULL,
                      ""C6"" ANY NOT NULL,
                      CONSTRAINT ""foo_PK_T3"" PRIMARY KEY (""C5"" ASC),
                      CONSTRAINT ""foo_FK_T3_C5_REF_foo_T2"" FOREIGN KEY (""C5"") REFERENCES ""foo_T2"" (""C3"") ON DELETE RESTRICT ON UPDATE RESTRICT
                    ) WITHOUT ROWID;",
                    @"INSERT INTO ""__foo_T3__{GUID}__"" (""C5"", ""C6"")
                    SELECT ""C5"", ""C6""
                    FROM ""foo_T3"";",
                    "DROP TABLE \"foo_T3\";",
                    "ALTER TABLE \"__foo_T3__{GUID}__\" RENAME TO \"foo_T3\";",
                    "CREATE INDEX \"foo_IX_T3_C6A\" ON \"foo_T3\" (\"C6\" ASC);" );

            statements.ElementAtOrDefault( 1 ).Should().SatisfySql( "DROP TABLE \"foo_T4\";" );
            statements.ElementAtOrDefault( 2 ).Should().SatisfySql( "DROP TABLE \"foo_T3\";" );
            statements.ElementAtOrDefault( 3 ).Should().SatisfySql( "DROP TABLE \"foo_T2\";" );
            statements.ElementAtOrDefault( 4 ).Should().SatisfySql( "DROP TABLE \"foo_T1\";" );
        }
    }

    [Fact]
    public void Remove_ShouldThrowSqliteObjectBuilderException_WhenAttemptingToRemoveDefaultSchema()
    {
        var db = new SqliteDatabaseBuilder();
        var sut = db.Schemas.Default;

        var action = Lambda.Of( () => sut.Remove() );

        action.Should()
            .ThrowExactly<SqliteObjectBuilderException>()
            .AndMatch( e => e.Dialect == SqliteDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void Remove_ShouldThrowSqliteObjectBuilderException_WhenAttemptingToRemoveSchemaWithExternalForeignKeys()
    {
        var db = new SqliteDatabaseBuilder();
        var sut = db.Schemas.Create( Fixture.Create<string>() );
        var table = sut.Objects.CreateTable( "T1" );
        var column = table.Columns.Create( "C1" );
        table.SetPrimaryKey( column.Asc() );

        var otherTable = db.Schemas.Default.Objects.CreateTable( "T2" );
        var otherColumn = otherTable.Columns.Create( "C2" );
        var otherColumn2 = otherTable.Columns.Create( "C3" );
        otherTable.SetPrimaryKey( otherColumn.Asc() );
        var otherIndex = otherTable.Indexes.Create( otherColumn2.Asc() );
        otherTable.ForeignKeys.Create( otherTable.PrimaryKey!.Index, table.PrimaryKey!.Index );
        otherTable.ForeignKeys.Create( otherIndex, table.PrimaryKey!.Index );

        var action = Lambda.Of( () => sut.Remove() );

        action.Should()
            .ThrowExactly<SqliteObjectBuilderException>()
            .AndMatch( e => e.Dialect == SqliteDialect.Instance && e.Errors.Count == 2 );
    }

    [Fact]
    public void ForSqlite_ShouldInvokeAction_WhenSchemaIsSqlite()
    {
        var action = Substitute.For<Action<SqliteSchemaBuilder>>();
        var sut = new SqliteDatabaseBuilder().Schemas.Default;

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
