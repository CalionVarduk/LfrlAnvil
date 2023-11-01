using System.Linq;
using LfrlAnvil.Functional;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.Sqlite.Exceptions;
using LfrlAnvil.Sqlite.Extensions;
using LfrlAnvil.Sqlite.Objects.Builders;
using LfrlAnvil.Sqlite.Tests.Helpers;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.Sqlite.Tests.ObjectsTests.BuildersTests;

public class SqliteForeignKeyBuilderTests : TestsBase
{
    [Fact]
    public void ToString_ShouldReturnCorrectResult()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var ix1 = table.Indexes.Create( table.Columns.Create( "C1" ).Asc() );
        var ix2 = table.Indexes.Create( table.Columns.Create( "C2" ).Asc() ).MarkAsUnique();
        var sut = table.ForeignKeys.Create( ix1, ix2 ).SetName( "bar" );

        var result = sut.ToString();

        result.Should().Be( "[ForeignKey] foo_bar" );
    }

    [Fact]
    public void Create_ShouldMarkTableForReconstruction_WhenForeignKeyReferencesAnotherTable()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );

        var t1 = schema.Objects.CreateTable( "T1" );
        var ix2 = t1.SetPrimaryKey( t1.Columns.Create( "C1" ).Asc() ).Index;

        var t2 = schema.Objects.CreateTable( "T2" );
        var ix1 = t2.SetPrimaryKey( t2.Columns.Create( "C2" ).Asc() ).Index;

        var startStatementCount = schema.Database.GetPendingStatements().Length;

        t2.ForeignKeys.Create( ix1, ix2 );
        var statements = schema.Database.GetPendingStatements().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            statements.Should().HaveCount( 1 );
            statements.ElementAtOrDefault( 0 )
                .Should()
                .SatisfySql(
                    @"CREATE TABLE ""__foo_T2__{GUID}__"" (
                      ""C2"" ANY NOT NULL,
                      CONSTRAINT ""foo_PK_T2"" PRIMARY KEY (""C2"" ASC),
                      CONSTRAINT ""foo_FK_T2_C2_REF_T1"" FOREIGN KEY (""C2"") REFERENCES ""foo_T1"" (""C1"") ON DELETE RESTRICT ON UPDATE RESTRICT
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
    public void Create_FollowedByRemove_ShouldDoNothing()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var ix1 = table.Indexes.Create( table.Columns.Create( "C2" ).Asc() );
        var ix2 = table.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;

        var startStatementCount = schema.Database.GetPendingStatements().Length;

        var sut = table.ForeignKeys.Create( ix1, ix2 );
        sut.Remove();
        var statements = schema.Database.GetPendingStatements().Slice( startStatementCount ).ToArray();

        statements.Should().BeEmpty();
    }

    [Fact]
    public void Create_ShouldMarkTableForReconstruction_WhenForeignKeyIsSelfReference()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var ix1 = table.Indexes.Create( table.Columns.Create( "C2" ).Asc() );
        var ix2 = table.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;

        var startStatementCount = schema.Database.GetPendingStatements().Length;

        table.ForeignKeys.Create( ix1, ix2 );
        var statements = schema.Database.GetPendingStatements().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            statements.Should().HaveCount( 1 );
            statements.ElementAtOrDefault( 0 )
                .Should()
                .SatisfySql(
                    "DROP INDEX \"foo_IX_T_C2A\";",
                    @"CREATE TABLE ""__foo_T__{GUID}__"" (
                      ""C2"" ANY NOT NULL,
                      ""C1"" ANY NOT NULL,
                      CONSTRAINT ""foo_PK_T"" PRIMARY KEY (""C1"" ASC),
                      CONSTRAINT ""foo_FK_T_C2_REF_T"" FOREIGN KEY (""C2"") REFERENCES ""foo_T"" (""C1"") ON DELETE RESTRICT ON UPDATE RESTRICT
                    ) WITHOUT ROWID;",
                    @"INSERT INTO ""__foo_T__{GUID}__"" (""C2"", ""C1"")
                    SELECT
                      ""foo_T"".""C2"",
                      ""foo_T"".""C1""
                    FROM ""foo_T"";",
                    "DROP TABLE \"foo_T\";",
                    "ALTER TABLE \"__foo_T__{GUID}__\" RENAME TO \"foo_T\";",
                    "CREATE INDEX \"foo_IX_T_C2A\" ON \"foo_T\" (\"C2\" ASC);" );
        }
    }

    [Fact]
    public void SetName_ShouldDoNothing_WhenNewNameEqualsOldName()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var ix1 = table.Indexes.Create( table.Columns.Create( "C2" ).Asc() );
        var ix2 = table.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
        var sut = table.ForeignKeys.Create( ix1, ix2 );

        var startStatementCount = schema.Database.GetPendingStatements().Length;

        var result = ((ISqlForeignKeyBuilder)sut).SetName( sut.Name );
        var statements = schema.Database.GetPendingStatements().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            statements.Should().BeEmpty();
        }
    }

    [Fact]
    public void SetName_ShouldDoNothing_WhenNameChangeIsFollowedByChangeToOriginal()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var ix1 = table.Indexes.Create( table.Columns.Create( "C2" ).Asc() );
        var ix2 = table.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
        var sut = table.ForeignKeys.Create( ix1, ix2 );
        var oldName = sut.Name;

        var startStatementCount = schema.Database.GetPendingStatements().Length;

        sut.SetName( "bar" );
        var result = ((ISqlForeignKeyBuilder)sut).SetName( oldName );

        var statements = schema.Database.GetPendingStatements().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            statements.Should().BeEmpty();
        }
    }

    [Fact]
    public void SetName_ShouldUpdateName_WhenNewNameIsDifferentFromOldName()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var ix1 = table.Indexes.Create( table.Columns.Create( "C2" ).Asc() );
        var ix2 = table.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
        var sut = table.ForeignKeys.Create( ix1, ix2 );
        var oldName = sut.Name;

        var startStatementCount = schema.Database.GetPendingStatements().Length;

        var result = ((ISqlForeignKeyBuilder)sut).SetName( "bar" );
        var statements = schema.Database.GetPendingStatements().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.Name.Should().Be( "bar" );
            sut.FullName.Should().Be( "foo_bar" );
            schema.Objects.Get( "bar" ).Should().BeSameAs( sut );
            schema.Objects.Contains( oldName ).Should().BeFalse();

            statements.Should().HaveCount( 1 );
            statements.ElementAtOrDefault( 0 )
                .Should()
                .SatisfySql(
                    "DROP INDEX \"foo_IX_T_C2A\";",
                    @"CREATE TABLE ""__foo_T__{GUID}__"" (
                      ""C2"" ANY NOT NULL,
                      ""C1"" ANY NOT NULL,
                      CONSTRAINT ""foo_PK_T"" PRIMARY KEY (""C1"" ASC),
                      CONSTRAINT ""foo_bar"" FOREIGN KEY (""C2"") REFERENCES ""foo_T"" (""C1"") ON DELETE RESTRICT ON UPDATE RESTRICT
                    ) WITHOUT ROWID;",
                    @"INSERT INTO ""__foo_T__{GUID}__"" (""C2"", ""C1"")
                    SELECT
                      ""foo_T"".""C2"",
                      ""foo_T"".""C1""
                    FROM ""foo_T"";",
                    "DROP TABLE \"foo_T\";",
                    "ALTER TABLE \"__foo_T__{GUID}__\" RENAME TO \"foo_T\";",
                    "CREATE INDEX \"foo_IX_T_C2A\" ON \"foo_T\" (\"C2\" ASC);" );
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
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var ix1 = table.Indexes.Create( table.Columns.Create( "C2" ).Asc() );
        var ix2 = table.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
        var sut = table.ForeignKeys.Create( ix1, ix2 );

        var action = Lambda.Of( () => ((ISqlForeignKeyBuilder)sut).SetName( name ) );

        action.Should()
            .ThrowExactly<SqliteObjectBuilderException>()
            .AndMatch( e => e.Dialect == SqliteDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void SetName_ShouldThrowSqliteObjectBuilderException_WhenForeignKeyIsRemoved()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var ix1 = table.Indexes.Create( table.Columns.Create( "C2" ).Asc() );
        var ix2 = table.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
        var sut = table.ForeignKeys.Create( ix1, ix2 );
        sut.Remove();

        var action = Lambda.Of( () => ((ISqlForeignKeyBuilder)sut).SetName( "bar" ) );

        action.Should()
            .ThrowExactly<SqliteObjectBuilderException>()
            .AndMatch( e => e.Dialect == SqliteDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void SetName_ShouldThrowSqliteObjectBuilderException_WhenNewNameAlreadyExistsInSchema()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var ix1 = table.Indexes.Create( table.Columns.Create( "C2" ).Asc() );
        var ix2 = table.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
        var sut = table.ForeignKeys.Create( ix1, ix2 );

        var action = Lambda.Of( () => ((ISqlForeignKeyBuilder)sut).SetName( "T" ) );

        action.Should()
            .ThrowExactly<SqliteObjectBuilderException>()
            .AndMatch( e => e.Dialect == SqliteDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void SetDefaultName_ShouldDoNothing_WhenNewNameEqualsOldName()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var ix1 = table.Indexes.Create( table.Columns.Create( "C2" ).Asc() );
        var ix2 = table.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
        var sut = table.ForeignKeys.Create( ix1, ix2 );

        var startStatementCount = schema.Database.GetPendingStatements().Length;

        var result = ((ISqlForeignKeyBuilder)sut).SetDefaultName();
        var statements = schema.Database.GetPendingStatements().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            statements.Should().BeEmpty();
        }
    }

    [Fact]
    public void SetDefaultName_ShouldDoNothing_WhenNameChangeIsFollowedByChangeToOriginal()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var ix1 = table.Indexes.Create( table.Columns.Create( "C2" ).Asc() );
        var ix2 = table.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
        var sut = table.ForeignKeys.Create( ix1, ix2 );

        var startStatementCount = schema.Database.GetPendingStatements().Length;

        sut.SetName( "bar" );
        var result = ((ISqlForeignKeyBuilder)sut).SetDefaultName();

        var statements = schema.Database.GetPendingStatements().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            statements.Should().BeEmpty();
        }
    }

    [Fact]
    public void SetDefaultName_ShouldUpdateName_WhenNewNameIsDifferentFromOldName()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var ix1 = table.Indexes.Create( table.Columns.Create( "C2" ).Asc() );
        var ix2 = table.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
        var sut = table.ForeignKeys.Create( ix1, ix2 ).SetName( "bar" );
        var oldName = sut.Name;

        var startStatementCount = schema.Database.GetPendingStatements().Length;

        var result = ((ISqlForeignKeyBuilder)sut).SetDefaultName();
        var statements = schema.Database.GetPendingStatements().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.Name.Should().Be( "FK_T_C2_REF_T" );
            sut.FullName.Should().Be( "foo_FK_T_C2_REF_T" );
            schema.Objects.Get( "FK_T_C2_REF_T" ).Should().BeSameAs( sut );
            schema.Objects.Contains( oldName ).Should().BeFalse();

            statements.Should().HaveCount( 1 );
            statements.ElementAtOrDefault( 0 )
                .Should()
                .SatisfySql(
                    "DROP INDEX \"foo_IX_T_C2A\";",
                    @"CREATE TABLE ""__foo_T__{GUID}__"" (
                      ""C2"" ANY NOT NULL,
                      ""C1"" ANY NOT NULL,
                      CONSTRAINT ""foo_PK_T"" PRIMARY KEY (""C1"" ASC),
                      CONSTRAINT ""foo_FK_T_C2_REF_T"" FOREIGN KEY (""C2"") REFERENCES ""foo_T"" (""C1"") ON DELETE RESTRICT ON UPDATE RESTRICT
                    ) WITHOUT ROWID;",
                    @"INSERT INTO ""__foo_T__{GUID}__"" (""C2"", ""C1"")
                    SELECT
                      ""foo_T"".""C2"",
                      ""foo_T"".""C1""
                    FROM ""foo_T"";",
                    "DROP TABLE \"foo_T\";",
                    "ALTER TABLE \"__foo_T__{GUID}__\" RENAME TO \"foo_T\";",
                    "CREATE INDEX \"foo_IX_T_C2A\" ON \"foo_T\" (\"C2\" ASC);" );
        }
    }

    [Fact]
    public void SetDefaultName_ShouldThrowSqliteObjectBuilderException_WhenForeignKeyIsRemoved()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var ix1 = table.Indexes.Create( table.Columns.Create( "C2" ).Asc() );
        var ix2 = table.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
        var sut = table.ForeignKeys.Create( ix1, ix2 ).SetName( "bar" );
        sut.Remove();

        var action = Lambda.Of( () => ((ISqlForeignKeyBuilder)sut).SetDefaultName() );

        action.Should()
            .ThrowExactly<SqliteObjectBuilderException>()
            .AndMatch( e => e.Dialect == SqliteDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void SetDefaultName_ShouldThrowSqliteObjectBuilderException_WhenNewNameAlreadyExistsInSchema()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var ix1 = table.Indexes.Create( table.Columns.Create( "C2" ).Asc() );
        var ix2 = table.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
        var sut = table.ForeignKeys.Create( ix1, ix2 ).SetName( "bar" );
        ix1.SetName( "FK_T_C2_REF_T" );

        var action = Lambda.Of( () => ((ISqlForeignKeyBuilder)sut).SetDefaultName() );

        action.Should()
            .ThrowExactly<SqliteObjectBuilderException>()
            .AndMatch( e => e.Dialect == SqliteDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void SetOnDeleteBehavior_ShouldUpdateBehavior_WhenNewValueIsDifferentFromOldValue()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var ix1 = table.Indexes.Create( table.Columns.Create( "C2" ).Asc() );
        var ix2 = table.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
        var sut = table.ForeignKeys.Create( ix1, ix2 );

        var startStatementCount = schema.Database.GetPendingStatements().Length;

        var result = ((ISqlForeignKeyBuilder)sut).SetOnDeleteBehavior( ReferenceBehavior.Cascade );
        var statements = schema.Database.GetPendingStatements().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            result.OnDeleteBehavior.Should().Be( ReferenceBehavior.Cascade );

            statements.Should().HaveCount( 1 );
            statements.ElementAtOrDefault( 0 )
                .Should()
                .SatisfySql(
                    "DROP INDEX \"foo_IX_T_C2A\";",
                    @"CREATE TABLE ""__foo_T__{GUID}__"" (
                      ""C2"" ANY NOT NULL,
                      ""C1"" ANY NOT NULL,
                      CONSTRAINT ""foo_PK_T"" PRIMARY KEY (""C1"" ASC),
                      CONSTRAINT ""foo_FK_T_C2_REF_T"" FOREIGN KEY (""C2"") REFERENCES ""foo_T"" (""C1"") ON DELETE CASCADE ON UPDATE RESTRICT
                    ) WITHOUT ROWID;",
                    @"INSERT INTO ""__foo_T__{GUID}__"" (""C2"", ""C1"")
                    SELECT
                      ""foo_T"".""C2"",
                      ""foo_T"".""C1""
                    FROM ""foo_T"";",
                    "DROP TABLE \"foo_T\";",
                    "ALTER TABLE \"__foo_T__{GUID}__\" RENAME TO \"foo_T\";",
                    "CREATE INDEX \"foo_IX_T_C2A\" ON \"foo_T\" (\"C2\" ASC);" );
        }
    }

    [Fact]
    public void SetOnDeleteBehavior_ShouldDoNothing_WhenNewValueEqualsOldValue()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var ix1 = table.Indexes.Create( table.Columns.Create( "C2" ).Asc() );
        var ix2 = table.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
        var sut = table.ForeignKeys.Create( ix1, ix2 );

        var startStatementCount = schema.Database.GetPendingStatements().Length;

        var result = ((ISqlForeignKeyBuilder)sut).SetOnDeleteBehavior( ReferenceBehavior.Restrict );
        var statements = schema.Database.GetPendingStatements().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            statements.Should().BeEmpty();
        }
    }

    [Fact]
    public void SetOnDeleteBehavior_ShouldDoNothing_WhenValueChangeIsFollowedByChangeToOriginal()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var ix1 = table.Indexes.Create( table.Columns.Create( "C2" ).Asc() );
        var ix2 = table.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
        var sut = table.ForeignKeys.Create( ix1, ix2 );

        var startStatementCount = schema.Database.GetPendingStatements().Length;

        sut.SetOnDeleteBehavior( ReferenceBehavior.Cascade );
        var result = ((ISqlForeignKeyBuilder)sut).SetOnDeleteBehavior( ReferenceBehavior.Restrict );

        var statements = schema.Database.GetPendingStatements().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            statements.Should().BeEmpty();
        }
    }

    [Fact]
    public void SetOnDeleteBehavior_ShouldThrowSqliteObjectBuilderException_WhenForeignKeyIsRemoved()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var ix1 = table.Indexes.Create( table.Columns.Create( "C2" ).Asc() );
        var ix2 = table.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
        var sut = table.ForeignKeys.Create( ix1, ix2 );
        sut.Remove();

        var action = Lambda.Of( () => ((ISqlForeignKeyBuilder)sut).SetOnDeleteBehavior( ReferenceBehavior.Cascade ) );

        action.Should()
            .ThrowExactly<SqliteObjectBuilderException>()
            .AndMatch( e => e.Dialect == SqliteDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void SetOnUpdateBehavior_ShouldUpdateBehavior_WhenNewValueIsDifferentFromOldValue()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var ix1 = table.Indexes.Create( table.Columns.Create( "C2" ).Asc() );
        var ix2 = table.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
        var sut = table.ForeignKeys.Create( ix1, ix2 );

        var startStatementCount = schema.Database.GetPendingStatements().Length;

        var result = ((ISqlForeignKeyBuilder)sut).SetOnUpdateBehavior( ReferenceBehavior.Cascade );
        var statements = schema.Database.GetPendingStatements().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            result.OnUpdateBehavior.Should().Be( ReferenceBehavior.Cascade );

            statements.Should().HaveCount( 1 );
            statements.ElementAtOrDefault( 0 )
                .Should()
                .SatisfySql(
                    "DROP INDEX \"foo_IX_T_C2A\";",
                    @"CREATE TABLE ""__foo_T__{GUID}__"" (
                      ""C2"" ANY NOT NULL,
                      ""C1"" ANY NOT NULL,
                      CONSTRAINT ""foo_PK_T"" PRIMARY KEY (""C1"" ASC),
                      CONSTRAINT ""foo_FK_T_C2_REF_T"" FOREIGN KEY (""C2"") REFERENCES ""foo_T"" (""C1"") ON DELETE RESTRICT ON UPDATE CASCADE
                    ) WITHOUT ROWID;",
                    @"INSERT INTO ""__foo_T__{GUID}__"" (""C2"", ""C1"")
                    SELECT
                      ""foo_T"".""C2"",
                      ""foo_T"".""C1""
                    FROM ""foo_T"";",
                    "DROP TABLE \"foo_T\";",
                    "ALTER TABLE \"__foo_T__{GUID}__\" RENAME TO \"foo_T\";",
                    "CREATE INDEX \"foo_IX_T_C2A\" ON \"foo_T\" (\"C2\" ASC);" );
        }
    }

    [Fact]
    public void SetOnUpdateBehavior_ShouldDoNothing_WhenNewValueEqualsOldValue()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var ix1 = table.Indexes.Create( table.Columns.Create( "C2" ).Asc() );
        var ix2 = table.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
        var sut = table.ForeignKeys.Create( ix1, ix2 );

        var startStatementCount = schema.Database.GetPendingStatements().Length;

        var result = ((ISqlForeignKeyBuilder)sut).SetOnUpdateBehavior( ReferenceBehavior.Restrict );
        var statements = schema.Database.GetPendingStatements().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            statements.Should().BeEmpty();
        }
    }

    [Fact]
    public void SetOnUpdateBehavior_ShouldDoNothing_WhenValueChangeIsFollowedByChangeToOriginal()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var ix1 = table.Indexes.Create( table.Columns.Create( "C2" ).Asc() );
        var ix2 = table.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
        var sut = table.ForeignKeys.Create( ix1, ix2 );

        var startStatementCount = schema.Database.GetPendingStatements().Length;

        sut.SetOnUpdateBehavior( ReferenceBehavior.Cascade );
        var result = ((ISqlForeignKeyBuilder)sut).SetOnUpdateBehavior( ReferenceBehavior.Restrict );

        var statements = schema.Database.GetPendingStatements().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            statements.Should().BeEmpty();
        }
    }

    [Fact]
    public void SetOnUpdateBehavior_ShouldThrowSqliteObjectBuilderException_WhenForeignKeyIsRemoved()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var ix1 = table.Indexes.Create( table.Columns.Create( "C2" ).Asc() );
        var ix2 = table.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
        var sut = table.ForeignKeys.Create( ix1, ix2 );
        sut.Remove();

        var action = Lambda.Of( () => ((ISqlForeignKeyBuilder)sut).SetOnUpdateBehavior( ReferenceBehavior.Cascade ) );

        action.Should()
            .ThrowExactly<SqliteObjectBuilderException>()
            .AndMatch( e => e.Dialect == SqliteDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void Remove_ShouldRemoveForeignKey()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var ix1 = table.Indexes.Create( table.Columns.Create( "C2" ).Asc() );
        var ix2 = table.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
        var sut = table.ForeignKeys.Create( ix1, ix2 );

        var startStatementCount = schema.Database.GetPendingStatements().Length;

        sut.Remove();
        var statements = schema.Database.GetPendingStatements().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            table.ForeignKeys.Should().BeEmpty();
            schema.Objects.Contains( sut.Name ).Should().BeFalse();
            sut.IsRemoved.Should().BeTrue();
            ix1.ForeignKeys.Should().BeEmpty();
            ix2.ReferencingForeignKeys.Should().BeEmpty();

            statements.Should().HaveCount( 1 );
            statements.ElementAtOrDefault( 0 )
                .Should()
                .SatisfySql(
                    "DROP INDEX \"foo_IX_T_C2A\";",
                    @"CREATE TABLE ""__foo_T__{GUID}__"" (
                      ""C2"" ANY NOT NULL,
                      ""C1"" ANY NOT NULL,
                      CONSTRAINT ""foo_PK_T"" PRIMARY KEY (""C1"" ASC)
                    ) WITHOUT ROWID;",
                    @"INSERT INTO ""__foo_T__{GUID}__"" (""C2"", ""C1"")
                    SELECT
                      ""foo_T"".""C2"",
                      ""foo_T"".""C1""
                    FROM ""foo_T"";",
                    "DROP TABLE \"foo_T\";",
                    "ALTER TABLE \"__foo_T__{GUID}__\" RENAME TO \"foo_T\";",
                    "CREATE INDEX \"foo_IX_T_C2A\" ON \"foo_T\" (\"C2\" ASC);" );
        }
    }

    [Fact]
    public void Remove_ShouldDoNothing_WhenForeignKeyIsRemoved()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var ix1 = table.Indexes.Create( table.Columns.Create( "C2" ).Asc() );
        var ix2 = table.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
        var sut = table.ForeignKeys.Create( ix1, ix2 );

        _ = schema.Database.GetPendingStatements();
        sut.Remove();
        var startStatementCount = schema.Database.GetPendingStatements().Length;

        sut.Remove();
        var statements = schema.Database.GetPendingStatements().Slice( startStatementCount ).ToArray();

        statements.Should().BeEmpty();
    }

    [Fact]
    public void ForSqlite_ShouldInvokeAction_WhenForeignKeyIsSqlite()
    {
        var action = Substitute.For<Action<SqliteForeignKeyBuilder>>();
        var table = SqliteDatabaseBuilderMock.Create().Schemas.Default.Objects.CreateTable( "T" );
        var ix1 = table.Indexes.Create( table.Columns.Create( "C1" ).Asc() );
        var ix2 = table.Indexes.Create( table.Columns.Create( "C2" ).Asc() ).MarkAsUnique();
        var sut = table.ForeignKeys.Create( ix1, ix2 );

        var result = sut.ForSqlite( action );

        result.Should().BeSameAs( sut );
        action.Verify().CallAt( 0 ).Exists().And.Arguments.Should().BeSequentiallyEqualTo( sut );
    }

    [Fact]
    public void ForSqlite_ShouldNotInvokeAction_WhenForeignKeyIsNotSqlite()
    {
        var action = Substitute.For<Action<SqliteForeignKeyBuilder>>();
        var sut = Substitute.For<ISqlForeignKeyBuilder>();

        var result = sut.ForSqlite( action );

        result.Should().BeSameAs( sut );
        action.Verify().CallCount.Should().Be( 0 );
    }
}
