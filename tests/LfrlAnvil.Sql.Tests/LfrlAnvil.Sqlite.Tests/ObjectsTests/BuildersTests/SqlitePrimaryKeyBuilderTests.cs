using System.Linq;
using LfrlAnvil.Functional;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.Sqlite.Exceptions;
using LfrlAnvil.Sqlite.Extensions;
using LfrlAnvil.Sqlite.Objects.Builders;
using LfrlAnvil.Sqlite.Tests.Helpers;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.Sqlite.Tests.ObjectsTests.BuildersTests;

public class SqlitePrimaryKeyBuilderTests : TestsBase
{
    [Fact]
    public void ToString_ShouldReturnCorrectResult()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var sut = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).SetName( "bar" );

        var result = sut.ToString();

        result.Should().Be( "[PrimaryKey] foo_bar" );
    }

    [Fact]
    public void Change_ShouldMarkTableForReconstruction()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var c1 = table.Columns.Create( "C1" );
        var c2 = table.Columns.Create( "C2" );
        table.Constraints.SetPrimaryKey( c1.Asc() );

        var startStatementCount = schema.Database.Changes.GetPendingActions().Length;

        table.Constraints.SetPrimaryKey( c2.Asc() );
        var statements = schema.Database.Changes.GetPendingActions().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            statements.Should().HaveCount( 1 );
            statements.ElementAtOrDefault( 0 )
                .Sql.Should()
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
                    "ALTER TABLE \"__foo_T__{GUID}__\" RENAME TO \"foo_T\";" );
        }
    }

    [Fact]
    public void SetName_ShouldDoNothing_WhenNewNameEqualsOldName()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var sut = table.Constraints.SetPrimaryKey( table.Columns.Create( "C" ).Asc() );

        var startStatementCount = schema.Database.Changes.GetPendingActions().Length;

        var result = ((ISqlPrimaryKeyBuilder)sut).SetName( sut.Name );
        var statements = schema.Database.Changes.GetPendingActions().Slice( startStatementCount ).ToArray();

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
        var sut = table.Constraints.SetPrimaryKey( table.Columns.Create( "C" ).Asc() );
        var oldName = sut.Name;

        var startStatementCount = schema.Database.Changes.GetPendingActions().Length;

        sut.SetName( "bar" );
        var result = ((ISqlPrimaryKeyBuilder)sut).SetName( oldName );

        var statements = schema.Database.Changes.GetPendingActions().Slice( startStatementCount ).ToArray();

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
        var sut = table.Constraints.SetPrimaryKey( table.Columns.Create( "C" ).Asc() );
        var oldName = sut.Name;

        var startStatementCount = schema.Database.Changes.GetPendingActions().Length;

        var result = ((ISqlPrimaryKeyBuilder)sut).SetName( "bar" );
        var statements = schema.Database.Changes.GetPendingActions().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.Name.Should().Be( "bar" );
            table.Constraints.GetConstraint( "bar" ).Should().BeSameAs( sut );
            table.Constraints.Contains( oldName ).Should().BeFalse();
            schema.Objects.GetObject( "bar" ).Should().BeSameAs( sut );
            schema.Objects.Contains( oldName ).Should().BeFalse();

            statements.Should().HaveCount( 1 );
            statements.ElementAtOrDefault( 0 )
                .Sql
                .Should()
                .SatisfySql(
                    @"CREATE TABLE ""__foo_T__{GUID}__"" (
                      ""C"" ANY NOT NULL,
                      CONSTRAINT ""foo_bar"" PRIMARY KEY (""C"" ASC)
                    ) WITHOUT ROWID;",
                    @"INSERT INTO ""__foo_T__{GUID}__"" (""C"")
                    SELECT
                      ""foo_T"".""C""
                    FROM ""foo_T"";",
                    "DROP TABLE \"foo_T\";",
                    "ALTER TABLE \"__foo_T__{GUID}__\" RENAME TO \"foo_T\";" );
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
        var sut = table.Constraints.SetPrimaryKey( table.Columns.Create( "C" ).Asc() );

        var action = Lambda.Of( () => ((ISqlPrimaryKeyBuilder)sut).SetName( name ) );

        action.Should()
            .ThrowExactly<SqliteObjectBuilderException>()
            .AndMatch( e => e.Dialect == SqliteDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void SetName_ShouldThrowSqliteObjectBuilderException_WhenPrimaryKeyIsRemoved()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var sut = table.Constraints.SetPrimaryKey( table.Columns.Create( "C" ).Asc() );
        sut.Remove();

        var action = Lambda.Of( () => ((ISqlPrimaryKeyBuilder)sut).SetName( "bar" ) );

        action.Should()
            .ThrowExactly<SqliteObjectBuilderException>()
            .AndMatch( e => e.Dialect == SqliteDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void SetName_ShouldThrowSqliteObjectBuilderException_WhenNewNameAlreadyExistsInSchema()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var sut = table.Constraints.SetPrimaryKey( table.Columns.Create( "C" ).Asc() );

        var action = Lambda.Of( () => ((ISqlPrimaryKeyBuilder)sut).SetName( "T" ) );

        action.Should()
            .ThrowExactly<SqliteObjectBuilderException>()
            .AndMatch( e => e.Dialect == SqliteDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void SetDefaultName_ShouldDoNothing_WhenNewNameEqualsOldName()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var sut = table.Constraints.SetPrimaryKey( table.Columns.Create( "C" ).Asc() );

        var startStatementCount = schema.Database.Changes.GetPendingActions().Length;

        var result = ((ISqlPrimaryKeyBuilder)sut).SetDefaultName();
        var statements = schema.Database.Changes.GetPendingActions().Slice( startStatementCount ).ToArray();

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
        var sut = table.Constraints.SetPrimaryKey( table.Columns.Create( "C" ).Asc() );

        var startStatementCount = schema.Database.Changes.GetPendingActions().Length;

        sut.SetName( "bar" );
        var result = ((ISqlPrimaryKeyBuilder)sut).SetDefaultName();
        var statements = schema.Database.Changes.GetPendingActions().Slice( startStatementCount ).ToArray();

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
        var sut = table.Constraints.SetPrimaryKey( table.Columns.Create( "C" ).Asc() ).SetName( "bar" );
        var oldName = sut.Name;

        var startStatementCount = schema.Database.Changes.GetPendingActions().Length;

        var result = ((ISqlPrimaryKeyBuilder)sut).SetDefaultName();
        var statements = schema.Database.Changes.GetPendingActions().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.Name.Should().Be( "PK_T" );
            table.Constraints.GetConstraint( "PK_T" ).Should().BeSameAs( sut );
            table.Constraints.Contains( oldName ).Should().BeFalse();
            schema.Objects.GetObject( "PK_T" ).Should().BeSameAs( sut );
            schema.Objects.Contains( oldName ).Should().BeFalse();

            statements.Should().HaveCount( 1 );
            statements.ElementAtOrDefault( 0 )
                .Sql
                .Should()
                .SatisfySql(
                    @"CREATE TABLE ""__foo_T__{GUID}__"" (
                      ""C"" ANY NOT NULL,
                      CONSTRAINT ""foo_PK_T"" PRIMARY KEY (""C"" ASC)
                    ) WITHOUT ROWID;",
                    @"INSERT INTO ""__foo_T__{GUID}__"" (""C"")
                    SELECT
                      ""foo_T"".""C""
                    FROM ""foo_T"";",
                    "DROP TABLE \"foo_T\";",
                    "ALTER TABLE \"__foo_T__{GUID}__\" RENAME TO \"foo_T\";" );
        }
    }

    [Fact]
    public void SetDefaultName_ShouldThrowSqliteObjectBuilderException_WhenNewNameAlreadyExistsInSchema()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var sut = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).SetName( "bar" );
        table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() ).SetName( "PK_T" );

        var action = Lambda.Of( () => ((ISqlPrimaryKeyBuilder)sut).SetDefaultName() );

        action.Should()
            .ThrowExactly<SqliteObjectBuilderException>()
            .AndMatch( e => e.Dialect == SqliteDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void SetDefaultName_ShouldThrowSqliteObjectBuilderException_WhenPrimaryKeyIsRemoved()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var sut = table.Constraints.SetPrimaryKey( table.Columns.Create( "C" ).Asc() ).SetName( "bar" );
        sut.Remove();

        var action = Lambda.Of( () => ((ISqlPrimaryKeyBuilder)sut).SetDefaultName() );

        action.Should()
            .ThrowExactly<SqliteObjectBuilderException>()
            .AndMatch( e => e.Dialect == SqliteDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void Remove_ShouldRemovePrimaryKeyAndUnderlyingIndex()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var sut = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );

        sut.Remove();

        using ( new AssertionScope() )
        {
            table.Constraints.TryGetPrimaryKey().Should().BeNull();
            table.Constraints.Contains( sut.Name ).Should().BeFalse();
            table.Constraints.Contains( sut.Index.Name ).Should().BeFalse();
            schema.Objects.Contains( sut.Name ).Should().BeFalse();
            schema.Objects.Contains( sut.Index.Name ).Should().BeFalse();
            sut.IsRemoved.Should().BeTrue();
            sut.Index.IsRemoved.Should().BeTrue();
        }
    }

    [Fact]
    public void Remove_ShouldRemovePrimaryKeyAndRemoveSelfReferencingForeignKeysToItsIndex()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var c1 = table.Columns.Create( "C1" );
        var c2 = table.Columns.Create( "C2" );
        var sut = table.Constraints.SetPrimaryKey( c1.Asc() );
        var ix = table.Constraints.CreateIndex( c2.Asc() );
        var fk = table.Constraints.CreateForeignKey( ix, sut.Index );

        sut.Remove();

        using ( new AssertionScope() )
        {
            table.Constraints.TryGetPrimaryKey().Should().BeNull();
            table.Constraints.Contains( sut.Name ).Should().BeFalse();
            table.Constraints.Contains( sut.Index.Name ).Should().BeFalse();
            table.Constraints.Contains( fk.Name ).Should().BeFalse();
            schema.Objects.Contains( sut.Name ).Should().BeFalse();
            schema.Objects.Contains( sut.Index.Name ).Should().BeFalse();
            schema.Objects.Contains( fk.Name ).Should().BeFalse();
            sut.IsRemoved.Should().BeTrue();
            sut.Index.IsRemoved.Should().BeTrue();
            sut.Index.PrimaryKey.Should().BeNull();
            fk.IsRemoved.Should().BeTrue();
        }
    }

    [Fact]
    public void Remove_ShouldDoNothing_WhenPrimaryKeyIsRemoved()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var c1 = table.Columns.Create( "C1" );
        var c2 = table.Columns.Create( "C2" );
        var sut = table.Constraints.SetPrimaryKey( c1.Asc() );

        _ = schema.Database.Changes.GetPendingActions();
        sut.Remove();
        table.Constraints.SetPrimaryKey( c2.Asc() );
        var startStatementCount = schema.Database.Changes.GetPendingActions().Length;

        sut.Remove();
        var statements = schema.Database.Changes.GetPendingActions().Slice( startStatementCount ).ToArray();

        statements.Should().BeEmpty();
    }

    [Fact]
    public void Remove_ShouldThrowSqliteObjectBuilderException_WhenUnderlyingIndexIsReferencedByAnotherTable()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var t1 = schema.Objects.CreateTable( "T1" );
        var sut = t1.Constraints.SetPrimaryKey( t1.Columns.Create( "C1" ).Asc() );
        var t2 = schema.Objects.CreateTable( "T2" );
        t2.Constraints.CreateForeignKey( t2.Constraints.CreateIndex( t2.Columns.Create( "C2" ).Asc() ), sut.Index );

        var action = Lambda.Of( () => sut.Remove() );

        action.Should()
            .ThrowExactly<SqliteObjectBuilderException>()
            .AndMatch( e => e.Dialect == SqliteDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void ForSqlite_ShouldInvokeAction_WhenPrimaryKeyIsSqlite()
    {
        var action = Substitute.For<Action<SqlitePrimaryKeyBuilder>>();
        var table = SqliteDatabaseBuilderMock.Create().Schemas.Default.Objects.CreateTable( "T" );
        var sut = table.Constraints.SetPrimaryKey( table.Columns.Create( "C" ).Asc() );

        var result = sut.ForSqlite( action );

        result.Should().BeSameAs( sut );
        action.Verify().CallAt( 0 ).Exists().And.Arguments.Should().BeSequentiallyEqualTo( sut );
    }

    [Fact]
    public void ForSqlite_ShouldNotInvokeAction_WhenPrimaryKeyIsNotSqlite()
    {
        var action = Substitute.For<Action<SqlitePrimaryKeyBuilder>>();
        var sut = Substitute.For<ISqlPrimaryKeyBuilder>();

        var result = sut.ForSqlite( action );

        result.Should().BeSameAs( sut );
        action.Verify().CallCount.Should().Be( 0 );
    }
}
