using System.Linq;
using LfrlAnvil.Functional;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Extensions;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.Sqlite.Exceptions;
using LfrlAnvil.Sqlite.Extensions;
using LfrlAnvil.Sqlite.Objects.Builders;
using LfrlAnvil.Sqlite.Tests.Helpers;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.Sqlite.Tests.ObjectsTests.BuildersTests;

public class SqliteIndexBuilderTests : TestsBase
{
    [Fact]
    public void ToString_ShouldReturnCorrectResult()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var sut = table.Indexes.Create( table.Columns.Create( "C" ).Asc() ).SetName( "bar" );

        var result = sut.ToString();

        result.Should().Be( "[Index] foo_bar" );
    }

    [Fact]
    public void Create_ShouldNotMarkTableForReconstruction()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var c2 = table.Columns.Create( "C2" );

        var startStatementsCount = schema.Database.GetPendingStatements().Length;

        table.Indexes.Create( c2.Asc() );
        var statements = schema.Database.GetPendingStatements().Slice( startStatementsCount ).ToArray();

        using ( new AssertionScope() )
        {
            statements.Should().HaveCount( 1 );
            statements.ElementAtOrDefault( 0 ).Sql.Should().SatisfySql( "CREATE INDEX \"foo_IX_T_C2A\" ON \"foo_T\" (\"C2\" ASC);" );
        }
    }

    [Fact]
    public void Create_FollowedByRemove_ShouldDoNothing()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var c2 = table.Columns.Create( "C2" );

        var startStatementCount = schema.Database.GetPendingStatements().Length;

        var sut = table.Indexes.Create( c2.Asc() );
        sut.Remove();
        var statements = schema.Database.GetPendingStatements().Slice( startStatementCount ).ToArray();

        statements.Should().BeEmpty();
    }

    [Fact]
    public void Create_ShouldNotCreateIndex_WhenIndexIsAttachedToPrimaryKey()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var c2 = table.Columns.Create( "C2" );

        var startStatementCount = schema.Database.GetPendingStatements().Length;

        var sut = table.Indexes.Create( c2.Asc() );
        table.SetPrimaryKey( sut.Columns.ToArray() );
        var statements = schema.Database.GetPendingStatements().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            statements.Should().HaveCount( 1 );
            statements.ElementAtOrDefault( 0 )
                .Sql
                .Should()
                .SatisfySql(
                    @"CREATE TABLE ""__foo_T__{GUID}__"" (
                      ""C1"" ANY NOT NULL,
                      ""C2"" ANY NOT NULL,
                      CONSTRAINT ""foo_PK_T"" PRIMARY KEY (""C2"" ASC)
                    ) WITHOUT ROWID;",
                    @"INSERT INTO ""__foo_T__{GUID}__"" (""C1"", ""C2"")
                    SELECT
                      ""foo_T"".""C1"",
                      ""foo_T"".""C2""
                    FROM ""foo_T"";",
                    "DROP TABLE \"foo_T\";",
                    "ALTER TABLE \"__foo_T__{GUID}__\" RENAME TO \"foo_T\";"
                );
        }
    }

    [Fact]
    public void AssigningToPrimaryKey_ShouldDropIndexByItsOldName_WhenIndexNameAlsoChanges()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Indexes.Create( table.Columns.Create( "C2" ).Asc() );

        var startStatementCount = schema.Database.GetPendingStatements().Length;

        sut.SetName( "bar" );
        table.SetPrimaryKey( sut.Columns.ToArray() );
        var statements = schema.Database.GetPendingStatements().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            statements.Should().HaveCount( 1 );
            statements.ElementAtOrDefault( 0 )
                .Sql
                .Should()
                .SatisfySql(
                    "DROP INDEX \"foo_IX_T_C2A\";",
                    @"CREATE TABLE ""__foo_T__{GUID}__"" (
                      ""C1"" ANY NOT NULL,
                      ""C2"" ANY NOT NULL,
                      CONSTRAINT ""foo_PK_T"" PRIMARY KEY (""C2"" ASC)
                    ) WITHOUT ROWID;",
                    @"INSERT INTO ""__foo_T__{GUID}__"" (""C1"", ""C2"")
                    SELECT
                      ""foo_T"".""C1"",
                      ""foo_T"".""C2""
                    FROM ""foo_T"";",
                    "DROP TABLE \"foo_T\";",
                    "ALTER TABLE \"__foo_T__{GUID}__\" RENAME TO \"foo_T\";"
                );
        }
    }

    [Fact]
    public void SetName_ShouldDoNothing_WhenNewNameEqualsOldName()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Indexes.Create( table.Columns.Create( "C2" ).Asc() );

        var startStatementCount = schema.Database.GetPendingStatements().Length;

        var result = ((ISqlIndexBuilder)sut).SetName( sut.Name );
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
        table.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Indexes.Create( table.Columns.Create( "C2" ).Asc() );
        var oldName = sut.Name;

        var startStatementCount = schema.Database.GetPendingStatements().Length;

        sut.SetName( "bar" );
        var result = ((ISqlIndexBuilder)sut).SetName( oldName );

        var statements = schema.Database.GetPendingStatements().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            statements.Should().BeEmpty();
        }
    }

    [Fact]
    public void SetName_ShouldDoNothing_WhenIndexIsAssignedToPrimaryKey()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var sut = table.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
        var oldName = sut.Name;

        var startStatementCount = schema.Database.GetPendingStatements().Length;

        var result = ((ISqlIndexBuilder)sut).SetName( "bar" );
        var statements = schema.Database.GetPendingStatements().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            schema.Objects.Contains( oldName ).Should().BeFalse();
            schema.Objects.Contains( "bar" ).Should().BeTrue();
            statements.Should().BeEmpty();
        }
    }

    [Fact]
    public void SetName_ShouldUpdateName_WhenNewNameIsDifferentFromOldName()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Indexes.Create( table.Columns.Create( "C2" ).Asc() );
        var oldName = sut.Name;

        var startStatementCount = schema.Database.GetPendingStatements().Length;

        var result = ((ISqlIndexBuilder)sut).SetName( "bar" );
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
                .Sql
                .Should()
                .SatisfySql(
                    "DROP INDEX \"foo_IX_T_C2A\";",
                    "CREATE INDEX \"foo_bar\" ON \"foo_T\" (\"C2\" ASC);" );
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
        table.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Indexes.Create( table.Columns.Create( "C2" ).Asc() );

        var action = Lambda.Of( () => ((ISqlIndexBuilder)sut).SetName( name ) );

        action.Should()
            .ThrowExactly<SqliteObjectBuilderException>()
            .AndMatch( e => e.Dialect == SqliteDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void SetName_ShouldThrowSqliteObjectBuilderException_WhenIndexIsRemoved()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Indexes.Create( table.Columns.Create( "C2" ).Asc() );
        sut.Remove();

        var action = Lambda.Of( () => ((ISqlIndexBuilder)sut).SetName( "bar" ) );

        action.Should()
            .ThrowExactly<SqliteObjectBuilderException>()
            .AndMatch( e => e.Dialect == SqliteDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void SetName_ShouldThrowSqliteObjectBuilderException_WhenNewNameAlreadyExistsInSchema()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Indexes.Create( table.Columns.Create( "C2" ).Asc() );

        var action = Lambda.Of( () => ((ISqlIndexBuilder)sut).SetName( "T" ) );

        action.Should()
            .ThrowExactly<SqliteObjectBuilderException>()
            .AndMatch( e => e.Dialect == SqliteDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void SetDefaultName_ShouldDoNothing_WhenNewNameEqualsOldName()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Indexes.Create( table.Columns.Create( "C2" ).Asc() );

        var startStatementCount = schema.Database.GetPendingStatements().Length;

        var result = ((ISqlIndexBuilder)sut).SetDefaultName();
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
        table.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Indexes.Create( table.Columns.Create( "C2" ).Asc() );

        var startStatementCount = schema.Database.GetPendingStatements().Length;

        sut.SetName( "bar" );
        var result = ((ISqlIndexBuilder)sut).SetDefaultName();

        var statements = schema.Database.GetPendingStatements().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            statements.Should().BeEmpty();
        }
    }

    [Fact]
    public void SetDefaultName_ShouldDoNothing_WhenIndexIsAssignedToPrimaryKey()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var sut = table.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index.SetName( "bar" );
        var oldName = sut.Name;

        var startStatementCount = schema.Database.GetPendingStatements().Length;

        var result = ((ISqlIndexBuilder)sut).SetDefaultName();
        var statements = schema.Database.GetPendingStatements().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            schema.Objects.Contains( oldName ).Should().BeFalse();
            schema.Objects.Contains( "UIX_T_C1A" ).Should().BeTrue();
            statements.Should().BeEmpty();
        }
    }

    [Fact]
    public void SetDefaultName_ShouldUpdateName_WhenNewNameIsDifferentFromOldName()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Indexes.Create( table.Columns.Create( "C2" ).Asc() ).SetName( "bar" );
        var oldName = sut.Name;

        var startStatementCount = schema.Database.GetPendingStatements().Length;

        var result = ((ISqlIndexBuilder)sut).SetDefaultName();
        var statements = schema.Database.GetPendingStatements().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.Name.Should().Be( "IX_T_C2A" );
            sut.FullName.Should().Be( "foo_IX_T_C2A" );
            schema.Objects.Get( "IX_T_C2A" ).Should().BeSameAs( sut );
            schema.Objects.Contains( oldName ).Should().BeFalse();

            statements.Should().HaveCount( 1 );
            statements.ElementAtOrDefault( 0 )
                .Sql
                .Should()
                .SatisfySql(
                    "DROP INDEX \"foo_bar\";",
                    "CREATE INDEX \"foo_IX_T_C2A\" ON \"foo_T\" (\"C2\" ASC);" );
        }
    }

    [Fact]
    public void SetDefaultName_ShouldUpdateName_WhenNewNameIsDifferentFromOldNameAndIndexIsUnique()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Indexes.Create( table.Columns.Create( "C2" ).Asc() ).SetName( "bar" ).MarkAsUnique();
        var oldName = sut.Name;

        var startStatementCount = schema.Database.GetPendingStatements().Length;

        var result = ((ISqlIndexBuilder)sut).SetDefaultName();
        var statements = schema.Database.GetPendingStatements().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.Name.Should().Be( "UIX_T_C2A" );
            sut.FullName.Should().Be( "foo_UIX_T_C2A" );
            schema.Objects.Get( "UIX_T_C2A" ).Should().BeSameAs( sut );
            schema.Objects.Contains( oldName ).Should().BeFalse();

            statements.Should().HaveCount( 1 );
            statements.ElementAtOrDefault( 0 )
                .Sql
                .Should()
                .SatisfySql(
                    "DROP INDEX \"foo_bar\";",
                    "CREATE UNIQUE INDEX \"foo_UIX_T_C2A\" ON \"foo_T\" (\"C2\" ASC);" );
        }
    }

    [Fact]
    public void SetDefaultName_ShouldThrowSqliteObjectBuilderException_WhenIndexIsRemoved()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Indexes.Create( table.Columns.Create( "C2" ).Asc() ).SetName( "bar" );
        sut.Remove();

        var action = Lambda.Of( () => ((ISqlIndexBuilder)sut).SetDefaultName() );

        action.Should()
            .ThrowExactly<SqliteObjectBuilderException>()
            .AndMatch( e => e.Dialect == SqliteDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void SetDefaultName_ShouldThrowSqliteObjectBuilderException_WhenNewNameAlreadyExistsInSchema()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var pk = table.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Indexes.Create( table.Columns.Create( "C2" ).Asc() ).SetName( "bar" );
        pk.SetName( "IX_T_C2A" );

        var action = Lambda.Of( () => ((ISqlIndexBuilder)sut).SetDefaultName() );

        action.Should()
            .ThrowExactly<SqliteObjectBuilderException>()
            .AndMatch( e => e.Dialect == SqliteDialect.Instance && e.Errors.Count == 1 );
    }

    [Theory]
    [InlineData( true )]
    [InlineData( false )]
    public void MarkAsUnique_ShouldDoNothing_WhenUniquenessFlagDoesNotChange(bool value)
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Indexes.Create( table.Columns.Create( "C2" ).Asc() ).MarkAsUnique( value );

        var startStatementCount = schema.Database.GetPendingStatements().Length;

        var result = ((ISqlIndexBuilder)sut).MarkAsUnique( value );
        var statements = schema.Database.GetPendingStatements().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            statements.Should().HaveCount( 0 );
        }
    }

    [Fact]
    public void MarkAsUnique_ShouldDoNothing_WhenValueChangeIsFollowedByChangeToOriginal()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Indexes.Create( table.Columns.Create( "C2" ).Asc() );

        var startStatementCount = schema.Database.GetPendingStatements().Length;

        var result = ((ISqlIndexBuilder)sut).MarkAsUnique().MarkAsUnique( false );
        var statements = schema.Database.GetPendingStatements().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            statements.Should().HaveCount( 0 );
        }
    }

    [Fact]
    public void MarkAsUnique_ShouldUpdateIsUnique_WhenValueChangesToTrue()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Indexes.Create( table.Columns.Create( "C2" ).Asc() );

        var startStatementCount = schema.Database.GetPendingStatements().Length;

        var result = ((ISqlIndexBuilder)sut).MarkAsUnique();
        var statements = schema.Database.GetPendingStatements().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            statements.Should().HaveCount( 1 );
            statements.ElementAtOrDefault( 0 )
                .Sql
                .Should()
                .SatisfySql(
                    "DROP INDEX \"foo_IX_T_C2A\";",
                    "CREATE UNIQUE INDEX \"foo_IX_T_C2A\" ON \"foo_T\" (\"C2\" ASC);" );
        }
    }

    [Fact]
    public void MarkAsUnique_ShouldUpdateIsUnique_WhenValueChangesToFalse()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Indexes.Create( table.Columns.Create( "C2" ).Asc() ).MarkAsUnique();

        var startStatementCount = schema.Database.GetPendingStatements().Length;

        var result = ((ISqlIndexBuilder)sut).MarkAsUnique( false );
        var statements = schema.Database.GetPendingStatements().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            statements.Should().HaveCount( 1 );
            statements.ElementAtOrDefault( 0 )
                .Sql
                .Should()
                .SatisfySql(
                    "DROP INDEX \"foo_IX_T_C2A\";",
                    "CREATE INDEX \"foo_IX_T_C2A\" ON \"foo_T\" (\"C2\" ASC);" );
        }
    }

    [Fact]
    public void MarkAsUnique_ShouldThrowSqliteObjectBuilderException_WhenPrimaryKeyIndexUniquenessChangesToFalse()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var sut = table.SetPrimaryKey( table.Columns.Create( "C" ).Asc() ).Index;

        var action = Lambda.Of( () => ((ISqlIndexBuilder)sut).MarkAsUnique( false ) );

        action.Should().ThrowExactly<SqliteObjectBuilderException>();
    }

    [Fact]
    public void MarkAsUnique_ShouldThrowSqliteObjectBuilderException_WhenUniquenessChangesToFalseAndIndexIsReferencedByForeignKey()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Indexes.Create( table.Columns.Create( "C2" ).Asc() ).MarkAsUnique();
        table.ForeignKeys.Create( table.Indexes.Create( table.Columns.Create( "C3" ).Asc() ), sut );

        var action = Lambda.Of( () => ((ISqlIndexBuilder)sut).MarkAsUnique( false ) );

        action.Should().ThrowExactly<SqliteObjectBuilderException>();
    }

    [Fact]
    public void MarkAsUnique_ShouldThrowSqliteObjectBuilderException_WhenIndexIsRemoved()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Indexes.Create( table.Columns.Create( "C2" ).Asc() );
        sut.Remove();

        var action = Lambda.Of( () => ((ISqlIndexBuilder)sut).MarkAsUnique() );

        action.Should()
            .ThrowExactly<SqliteObjectBuilderException>()
            .AndMatch( e => e.Dialect == SqliteDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void MarkAsUnique_ShouldUpdateIsUniqueAndNameCorrectly_WhenIsUniqueAndNameChangeAtTheSameTime()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Indexes.Create( table.Columns.Create( "C2" ).Asc() );

        var startStatementCount = schema.Database.GetPendingStatements().Length;

        var result = ((ISqlIndexBuilder)sut).MarkAsUnique().SetName( "bar" );
        var statements = schema.Database.GetPendingStatements().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            statements.Should().HaveCount( 1 );
            statements.ElementAtOrDefault( 0 )
                .Sql
                .Should()
                .SatisfySql(
                    "DROP INDEX \"foo_IX_T_C2A\";",
                    "CREATE UNIQUE INDEX \"foo_bar\" ON \"foo_T\" (\"C2\" ASC);" );
        }
    }

    [Fact]
    public void SetFilter_ShouldDoNothing_WhenValueDoesNotChange()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Indexes.Create( table.Columns.Create( "C2" ).Asc() ).SetFilter( SqlNode.True() );

        var startStatementCount = schema.Database.GetPendingStatements().Length;

        var result = ((ISqlIndexBuilder)sut).SetFilter( SqlNode.True() );
        var statements = schema.Database.GetPendingStatements().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            statements.Should().HaveCount( 0 );
        }
    }

    [Fact]
    public void SetFilter_ShouldDoNothing_WhenValueChangeIsFollowedByChangeToOriginal()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Indexes.Create( table.Columns.Create( "C2" ).Asc() );

        var startStatementCount = schema.Database.GetPendingStatements().Length;

        var result = ((ISqlIndexBuilder)sut).SetFilter( SqlNode.True() ).SetFilter( null );
        var statements = schema.Database.GetPendingStatements().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            statements.Should().HaveCount( 0 );
        }
    }

    [Fact]
    public void SetFilter_ShouldUpdateFilterAndFilterColumns_WhenValueChangesToNonNull()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var column = table.Columns.Create( "C2" );
        var sut = table.Indexes.Create( column.Asc() );

        var startStatementCount = schema.Database.GetPendingStatements().Length;

        var result = sut.SetFilter( t => t["C2"] != null );
        var statements = schema.Database.GetPendingStatements().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            result.Filter.Should().BeEquivalentTo( table.ToRecordSet().GetField( "C2" ) != null );
            result.ReferencedFilterColumns.Should().BeSequentiallyEqualTo( column );
            column.ReferencingIndexFilters.Should().BeSequentiallyEqualTo( sut );
            statements.Should().HaveCount( 1 );
            statements.ElementAtOrDefault( 0 )
                .Sql
                .Should()
                .SatisfySql(
                    "DROP INDEX \"foo_IX_T_C2A\";",
                    "CREATE INDEX \"foo_IX_T_C2A\" ON \"foo_T\" (\"C2\" ASC) WHERE (\"C2\" IS NOT NULL);" );
        }
    }

    [Fact]
    public void SetFilter_ShouldUpdateFilterAndFilterColumns_WhenValueChangesToNull()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var column = table.Columns.Create( "C2" );
        var sut = table.Indexes.Create( column.Asc() ).SetFilter( t => t["C2"] != null );

        var startStatementCount = schema.Database.GetPendingStatements().Length;

        var result = ((ISqlIndexBuilder)sut).SetFilter( null );
        var statements = schema.Database.GetPendingStatements().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            result.Filter.Should().BeNull();
            result.ReferencedFilterColumns.Should().BeEmpty();
            column.ReferencingIndexFilters.Should().BeEmpty();
            statements.Should().HaveCount( 1 );
            statements.ElementAtOrDefault( 0 )
                .Sql
                .Should()
                .SatisfySql(
                    "DROP INDEX \"foo_IX_T_C2A\";",
                    "CREATE INDEX \"foo_IX_T_C2A\" ON \"foo_T\" (\"C2\" ASC);" );
        }
    }

    [Fact]
    public void SetFilter_ShouldThrowSqliteObjectBuilderException_WhenFilterIsInvalid()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var sut = table.Indexes.Create( table.Columns.Create( "C" ).Asc() );

        var action = Lambda.Of(
            () => ((ISqlIndexBuilder)sut).SetFilter( _ => SqlNode.Functions.RecordsAffected() == SqlNode.Literal( 0 ) ) );

        action.Should().ThrowExactly<SqliteObjectBuilderException>();
    }

    [Fact]
    public void SetFilter_ShouldThrowSqliteObjectBuilderException_WhenPrimaryKeyIndexFilterChangesToNonNull()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var sut = table.SetPrimaryKey( table.Columns.Create( "C" ).Asc() ).Index;

        var action = Lambda.Of( () => ((ISqlIndexBuilder)sut).SetFilter( SqlNode.True() ) );

        action.Should().ThrowExactly<SqliteObjectBuilderException>();
    }

    [Fact]
    public void SetFilter_ShouldThrowSqliteObjectBuilderException_WhenIndexIsRemoved()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Indexes.Create( table.Columns.Create( "C2" ).Asc() );
        sut.Remove();

        var action = Lambda.Of( () => ((ISqlIndexBuilder)sut).SetFilter( null ) );

        action.Should()
            .ThrowExactly<SqliteObjectBuilderException>()
            .AndMatch( e => e.Dialect == SqliteDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void SetFilter_ShouldUpdateFilterAndIsUniqueAndNameCorrectly_WhenFilterAndIsUniqueAndNameChangeAtTheSameTime()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Indexes.Create( table.Columns.Create( "C2" ).Asc() );

        var startStatementCount = schema.Database.GetPendingStatements().Length;

        var result = ((ISqlIndexBuilder)sut).MarkAsUnique().SetFilter( t => t["C2"] != null ).SetName( "bar" );
        var statements = schema.Database.GetPendingStatements().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            statements.Should().HaveCount( 1 );
            statements.ElementAtOrDefault( 0 )
                .Sql
                .Should()
                .SatisfySql(
                    "DROP INDEX \"foo_IX_T_C2A\";",
                    "CREATE UNIQUE INDEX \"foo_bar\" ON \"foo_T\" (\"C2\" ASC) WHERE (\"C2\" IS NOT NULL);" );
        }
    }

    [Fact]
    public void Remove_ShouldRemoveIndexAndSelfReferencingForeignKeys()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var pk = table.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var c2 = table.Columns.Create( "C2" );
        var sut = table.Indexes.Create( c2.Asc() ).MarkAsUnique();
        var fk1 = table.ForeignKeys.Create( table.Indexes.Create( table.Columns.Create( "C3" ).Asc() ), sut );
        var fk2 = table.ForeignKeys.Create( sut, pk.Index );

        var startStatementCount = schema.Database.GetPendingStatements().Length;

        sut.SetName( "bar" ).Remove();
        var statements = schema.Database.GetPendingStatements().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            table.Indexes.Contains( c2.Asc() ).Should().BeFalse();
            schema.Objects.Contains( sut.Name ).Should().BeFalse();
            sut.IsRemoved.Should().BeTrue();
            sut.OriginatingForeignKeys.Should().BeEmpty();
            sut.ReferencingForeignKeys.Should().BeEmpty();
            fk1.IsRemoved.Should().BeTrue();
            fk2.IsRemoved.Should().BeTrue();
            c2.ReferencingIndexes.Should().BeEmpty();

            statements.Should().HaveCount( 1 );
            statements.ElementAtOrDefault( 0 )
                .Sql
                .Should()
                .SatisfySql(
                    "DROP INDEX \"foo_IX_T_C2A\";",
                    "DROP INDEX \"foo_IX_T_C3A\";",
                    @"CREATE TABLE ""__foo_T__{GUID}__"" (
                      ""C1"" ANY NOT NULL,
                      ""C2"" ANY NOT NULL,
                      ""C3"" ANY NOT NULL,
                      CONSTRAINT ""foo_PK_T"" PRIMARY KEY (""C1"" ASC)
                    ) WITHOUT ROWID;",
                    @"INSERT INTO ""__foo_T__{GUID}__"" (""C1"", ""C2"", ""C3"")
                    SELECT
                      ""foo_T"".""C1"",
                      ""foo_T"".""C2"",
                      ""foo_T"".""C3""
                    FROM ""foo_T"";",
                    "DROP TABLE \"foo_T\";",
                    "ALTER TABLE \"__foo_T__{GUID}__\" RENAME TO \"foo_T\";",
                    "CREATE INDEX \"foo_IX_T_C3A\" ON \"foo_T\" (\"C3\" ASC);" );
        }
    }

    [Fact]
    public void Remove_ShouldRemoveIndexAndAssignedPrimaryKey()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var column = table.Columns.Create( "C" );
        var pk = table.SetPrimaryKey( column.Asc() );
        var sut = pk.Index;

        sut.Remove();

        using ( new AssertionScope() )
        {
            table.Indexes.Contains( column.Asc() ).Should().BeFalse();
            schema.Objects.Contains( sut.Name ).Should().BeFalse();
            schema.Objects.Contains( pk.Name ).Should().BeFalse();
            sut.IsRemoved.Should().BeTrue();
            sut.OriginatingForeignKeys.Should().BeEmpty();
            sut.ReferencingForeignKeys.Should().BeEmpty();
            column.ReferencingIndexes.Should().BeEmpty();
            pk.IsRemoved.Should().BeTrue();
            table.PrimaryKey.Should().BeNull();
        }
    }

    [Fact]
    public void Remove_ShouldRemoveIndexAndClearAssignedFilterColumns()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var column = table.Columns.Create( "C" );
        var sut = table.Indexes.Create( column.Asc() ).SetFilter( t => t["C"] != null );

        sut.Remove();

        using ( new AssertionScope() )
        {
            table.Indexes.Contains( column.Asc() ).Should().BeFalse();
            schema.Objects.Contains( sut.Name ).Should().BeFalse();
            sut.IsRemoved.Should().BeTrue();
            sut.OriginatingForeignKeys.Should().BeEmpty();
            sut.ReferencingForeignKeys.Should().BeEmpty();
            sut.ReferencedFilterColumns.Should().BeEmpty();
            column.ReferencingIndexes.Should().BeEmpty();
            column.ReferencingIndexFilters.Should().BeEmpty();
            table.PrimaryKey.Should().BeNull();
        }
    }

    [Fact]
    public void Remove_ShouldDoNothing_WhenIndexIsRemoved()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Indexes.Create( table.Columns.Create( "C2" ).Asc() );

        _ = schema.Database.GetPendingStatements();
        sut.Remove();
        var startStatementCount = schema.Database.GetPendingStatements().Length;

        sut.Remove();
        var statements = schema.Database.GetPendingStatements().Slice( startStatementCount ).ToArray();

        statements.Should().BeEmpty();
    }

    [Fact]
    public void Remove_ShouldThrowSqliteObjectBuilderException_WhenIndexHasExternalReferences()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var t1 = schema.Objects.CreateTable( "T1" );
        t1.SetPrimaryKey( t1.Columns.Create( "C1" ).Asc() );
        var sut = t1.Indexes.Create( t1.Columns.Create( "C2" ).Asc() ).MarkAsUnique();
        var t2 = schema.Objects.CreateTable( "T2" );
        var ix = t2.SetPrimaryKey( t2.Columns.Create( "C3" ).Asc() ).Index;
        t2.ForeignKeys.Create( ix, sut );

        var action = Lambda.Of( () => sut.Remove() );

        action.Should().ThrowExactly<SqliteObjectBuilderException>();
    }

    [Fact]
    public void ForSqlite_ShouldInvokeAction_WhenIndexIsSqlite()
    {
        var action = Substitute.For<Action<SqliteIndexBuilder>>();
        var table = SqliteDatabaseBuilderMock.Create().Schemas.Default.Objects.CreateTable( "T" );
        var sut = table.Indexes.Create( table.Columns.Create( "C1" ).Asc() );

        var result = sut.ForSqlite( action );

        result.Should().BeSameAs( sut );
        action.Verify().CallAt( 0 ).Exists().And.Arguments.Should().BeSequentiallyEqualTo( sut );
    }

    [Fact]
    public void ForSqlite_ShouldNotInvokeAction_WhenIndexIsNotSqlite()
    {
        var action = Substitute.For<Action<SqliteIndexBuilder>>();
        var sut = Substitute.For<ISqlIndexBuilder>();

        var result = sut.ForSqlite( action );

        result.Should().BeSameAs( sut );
        action.Verify().CallCount.Should().Be( 0 );
    }
}
