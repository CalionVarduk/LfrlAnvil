using System.Linq;
using LfrlAnvil.Functional;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Extensions;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.Sqlite.Extensions;
using LfrlAnvil.Sqlite.Objects.Builders;
using LfrlAnvil.Sqlite.Tests.Helpers;
using LfrlAnvil.TestExtensions.FluentAssertions;
using LfrlAnvil.TestExtensions.Sql;
using LfrlAnvil.TestExtensions.Sql.FluentAssertions;

namespace LfrlAnvil.Sqlite.Tests.ObjectsTests.BuildersTests;

public class SqliteColumnBuilderTests : TestsBase
{
    [Fact]
    public void ToString_ShouldReturnCorrectResult()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var sut = table.Columns.Create( "C" );

        var result = sut.ToString();

        result.Should().Be( "[Column] foo_T.C" );
    }

    [Fact]
    public void Creation_ShouldMarkTableForReconstructionAndAutomaticallySetDefaultValue()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );

        var actionCount = schema.Database.GetPendingActionCount();
        var sut = table.Columns.Create( "C2" );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            table.Columns.Get( sut.Name ).Should().BeSameAs( sut );
            sut.Name.Should().Be( "C2" );
            sut.DefaultValue.Should().BeSameAs( sut.TypeDefinition.DefaultValue );

            actions.Should().HaveCount( 1 );
            actions.ElementAtOrDefault( 0 )
                .Sql.Should()
                .SatisfySql(
                    @"CREATE TABLE ""__foo_T__{GUID}__"" (
                      ""C1"" ANY NOT NULL,
                      ""C2"" ANY NOT NULL DEFAULT (X''),
                      CONSTRAINT ""foo_PK_T"" PRIMARY KEY (""C1"" ASC)
                    ) WITHOUT ROWID;",
                    @"INSERT INTO ""__foo_T__{GUID}__"" (""C1"", ""C2"")
                    SELECT
                      ""foo_T"".""C1"",
                      X'' AS ""C2""
                    FROM ""foo_T"";",
                    "DROP TABLE \"foo_T\";",
                    "ALTER TABLE \"__foo_T__{GUID}__\" RENAME TO \"foo_T\";" );
        }
    }

    [Fact]
    public void Creation_ShouldMarkTableForReconstruction_WithoutDefaultValueWhenColumnIsNullable()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );

        var actionCount = schema.Database.GetPendingActionCount();
        var sut = table.Columns.Create( "C2" ).MarkAsNullable();
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            table.Columns.Get( sut.Name ).Should().BeSameAs( sut );
            sut.Name.Should().Be( "C2" );
            sut.DefaultValue.Should().BeNull();

            actions.Should().HaveCount( 1 );
            actions.ElementAtOrDefault( 0 )
                .Sql.Should()
                .SatisfySql(
                    @"CREATE TABLE ""__foo_T__{GUID}__"" (
                      ""C1"" ANY NOT NULL,
                      ""C2"" ANY,
                      CONSTRAINT ""foo_PK_T"" PRIMARY KEY (""C1"" ASC)
                    ) WITHOUT ROWID;",
                    @"INSERT INTO ""__foo_T__{GUID}__"" (""C1"", ""C2"")
                    SELECT
                      ""foo_T"".""C1"",
                      NULL AS ""C2""
                    FROM ""foo_T"";",
                    "DROP TABLE \"foo_T\";",
                    "ALTER TABLE \"__foo_T__{GUID}__\" RENAME TO \"foo_T\";" );
        }
    }

    [Fact]
    public void Creation_ShouldMarkTableForReconstruction_WithComputation()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );

        var actionCount = schema.Database.GetPendingActionCount();
        var sut = table.Columns.Create( "C2" ).SetComputation( SqlColumnComputation.Virtual( SqlNode.Literal( 1 ) ) );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            table.Columns.Get( sut.Name ).Should().BeSameAs( sut );
            sut.Name.Should().Be( "C2" );
            sut.DefaultValue.Should().BeNull();

            actions.Should().HaveCount( 1 );
            actions.ElementAtOrDefault( 0 )
                .Sql.Should()
                .SatisfySql(
                    @"CREATE TABLE ""__foo_T__{GUID}__"" (
                      ""C1"" ANY NOT NULL,
                      ""C2"" ANY NOT NULL GENERATED ALWAYS AS (1) VIRTUAL,
                      CONSTRAINT ""foo_PK_T"" PRIMARY KEY (""C1"" ASC)
                    ) WITHOUT ROWID;",
                    @"INSERT INTO ""__foo_T__{GUID}__"" (""C1"")
                    SELECT
                      ""foo_T"".""C1""
                    FROM ""foo_T"";",
                    "DROP TABLE \"foo_T\";",
                    "ALTER TABLE \"__foo_T__{GUID}__\" RENAME TO \"foo_T\";" );
        }
    }

    [Fact]
    public void Creation_ShouldMarkTableForReconstruction_WhenDefaultValueIsDefinedExplicitly()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );

        var actionCount = schema.Database.GetPendingActionCount();
        var sut = table.Columns.Create( "C2" ).SetDefaultValue( new byte[] { 1, 2, 3 } );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            table.Columns.Get( sut.Name ).Should().BeSameAs( sut );
            sut.Name.Should().Be( "C2" );

            actions.Should().HaveCount( 1 );
            actions.ElementAtOrDefault( 0 )
                .Sql.Should()
                .SatisfySql(
                    @"CREATE TABLE ""__foo_T__{GUID}__"" (
                      ""C1"" ANY NOT NULL,
                      ""C2"" ANY NOT NULL DEFAULT (X'010203'),
                      CONSTRAINT ""foo_PK_T"" PRIMARY KEY (""C1"" ASC)
                    ) WITHOUT ROWID;",
                    @"INSERT INTO ""__foo_T__{GUID}__"" (""C1"", ""C2"")
                    SELECT
                      ""foo_T"".""C1"",
                      X'010203' AS ""C2""
                    FROM ""foo_T"";",
                    "DROP TABLE \"foo_T\";",
                    "ALTER TABLE \"__foo_T__{GUID}__\" RENAME TO \"foo_T\";" );
        }
    }

    [Fact]
    public void Creation_WithReusedRemovedColumnName_ShouldTreatTheColumnAsModified()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var removed = table.Columns.Create( "C2" );

        var actionCount = schema.Database.GetPendingActionCount();
        removed.SetName( "C3" ).Remove();
        table.Columns.Create( "C3" ).MarkAsNullable();
        var sut = table.Columns.Create( "C2" ).SetType<string>();
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            table.Columns.Get( sut.Name ).Should().BeSameAs( sut );
            sut.Name.Should().Be( "C2" );
            sut.DefaultValue.Should().BeNull();
            removed.IsRemoved.Should().BeTrue();

            actions.Should().HaveCount( 1 );
            actions.ElementAtOrDefault( 0 )
                .Sql.Should()
                .SatisfySql(
                    @"CREATE TABLE ""__foo_T__{GUID}__"" (
                      ""C1"" ANY NOT NULL,
                      ""C2"" TEXT NOT NULL,
                      ""C3"" ANY,
                      CONSTRAINT ""foo_PK_T"" PRIMARY KEY (""C1"" ASC)
                    ) WITHOUT ROWID;",
                    @"INSERT INTO ""__foo_T__{GUID}__"" (""C1"", ""C2"", ""C3"")
                    SELECT
                      ""foo_T"".""C1"",
                      CAST(""foo_T"".""C2"" AS TEXT) AS ""C2"",
                      NULL AS ""C3""
                    FROM ""foo_T"";",
                    "DROP TABLE \"foo_T\";",
                    "ALTER TABLE \"__foo_T__{GUID}__\" RENAME TO \"foo_T\";" );
        }
    }

    [Fact]
    public void Creation_WithReusedRemovedColumnName_ShouldTreatTheColumnAsModified_WithOriginalIsNullableChange()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var removed = table.Columns.Create( "C2" ).MarkAsNullable();

        var actionCount = schema.Database.GetPendingActionCount();
        removed.SetName( "C3" ).MarkAsNullable( false ).Remove();
        var sut = table.Columns.Create( "C2" );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            table.Columns.Get( sut.Name ).Should().BeSameAs( sut );
            sut.Name.Should().Be( "C2" );
            sut.DefaultValue.Should().BeNull();
            removed.IsRemoved.Should().BeTrue();

            actions.Should().HaveCount( 1 );
            actions.ElementAtOrDefault( 0 )
                .Sql.Should()
                .SatisfySql(
                    @"CREATE TABLE ""__foo_T__{GUID}__"" (
                      ""C1"" ANY NOT NULL,
                      ""C2"" ANY NOT NULL,
                      CONSTRAINT ""foo_PK_T"" PRIMARY KEY (""C1"" ASC)
                    ) WITHOUT ROWID;",
                    @"INSERT INTO ""__foo_T__{GUID}__"" (""C1"", ""C2"")
                    SELECT
                      ""foo_T"".""C1"",
                      COALESCE(""foo_T"".""C2"", X'') AS ""C2""
                    FROM ""foo_T"";",
                    "DROP TABLE \"foo_T\";",
                    "ALTER TABLE \"__foo_T__{GUID}__\" RENAME TO \"foo_T\";" );
        }
    }

    [Fact]
    public void Creation_WithReusedRemovedColumnName_ShouldTreatTheColumnAsModified_WithOriginalTypeDefinitionChange()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var removed = table.Columns.Create( "C2" );

        var actionCount = schema.Database.GetPendingActionCount();
        removed.SetName( "C3" ).SetType<string>().Remove();
        var sut = table.Columns.Create( "C2" );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            table.Columns.Get( sut.Name ).Should().BeSameAs( sut );
            sut.Name.Should().Be( "C2" );
            sut.DefaultValue.Should().BeNull();
            removed.IsRemoved.Should().BeTrue();

            actions.Should().HaveCount( 1 );
            actions.ElementAtOrDefault( 0 )
                .Sql.Should()
                .SatisfySql(
                    @"CREATE TABLE ""__foo_T__{GUID}__"" (
                      ""C1"" ANY NOT NULL,
                      ""C2"" ANY NOT NULL,
                      CONSTRAINT ""foo_PK_T"" PRIMARY KEY (""C1"" ASC)
                    ) WITHOUT ROWID;",
                    @"INSERT INTO ""__foo_T__{GUID}__"" (""C1"", ""C2"")
                    SELECT
                      ""foo_T"".""C1"",
                      ""foo_T"".""C2"" AS ""C2""
                    FROM ""foo_T"";",
                    "DROP TABLE \"foo_T\";",
                    "ALTER TABLE \"__foo_T__{GUID}__\" RENAME TO \"foo_T\";" );
        }
    }

    [Fact]
    public void Creation_FollowedByRemove_ShouldDoNothing()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );

        var actionCount = schema.Database.GetPendingActionCount();
        var sut = table.Columns.Create( "C2" );
        sut.Remove();
        var actions = schema.Database.GetLastPendingActions( actionCount );

        actions.Should().BeEmpty();
    }

    [Fact]
    public void SetName_ShouldDoNothing_WhenNewNameEqualsOldName()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" );

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
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" );
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
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" );
        var node = sut.Node;

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.SetName( "bar" );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.Name.Should().Be( "bar" );
            table.Columns.TryGet( "bar" ).Should().BeSameAs( sut );
            table.Columns.TryGet( "C2" ).Should().BeNull();
            node.Name.Should().Be( "bar" );

            actions.Should().HaveCount( 1 );
            actions.ElementAtOrDefault( 0 ).Sql.Should().SatisfySql( "ALTER TABLE \"foo_T\" RENAME COLUMN \"C2\" TO \"bar\";" );
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
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" );

        var action = Lambda.Of( () => sut.SetName( name ) );

        action.Should()
            .ThrowExactly<SqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == SqliteDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void SetName_ShouldThrowSqlObjectBuilderException_WhenColumnIsRemoved()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" );
        sut.Remove();

        var action = Lambda.Of( () => sut.SetName( "bar" ) );

        action.Should()
            .ThrowExactly<SqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == SqliteDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void SetName_ShouldThrowSqlObjectBuilderException_WhenNewNameAlreadyExistsInTableColumns()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" );

        var action = Lambda.Of( () => sut.SetName( "C1" ) );

        action.Should()
            .ThrowExactly<SqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == SqliteDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void SetName_ShouldThrowSqlObjectBuilderException_WhenColumnIsUsedInIndex()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" );
        table.Constraints.CreateIndex( sut.Asc() );

        var action = Lambda.Of( () => sut.SetName( "C3" ) );

        action.Should()
            .ThrowExactly<SqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == SqliteDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void SetName_ShouldThrowSqlObjectBuilderException_WhenColumnIsUsedInView()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" );
        schema.Objects.CreateView( "V", table.ToRecordSet().ToDataSource().Select( s => new[] { s.From["C2"].AsSelf() } ) );

        var action = Lambda.Of( () => sut.SetName( "C3" ) );

        action.Should()
            .ThrowExactly<SqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == SqliteDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void SetType_ShouldDoNothing_WhenNewTypeEqualsOldType()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" );

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.SetType( SqliteDataType.Any );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            actions.Should().BeEmpty();
        }
    }

    [Fact]
    public void SetType_ShouldDoNothing_WhenTypeChangeIsFollowedByChangeToOriginal()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" );

        var actionCount = schema.Database.GetPendingActionCount();
        sut.SetType( SqliteDataType.Integer );
        var result = sut.SetType( schema.Database.TypeDefinitions.GetByType<object>() );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            actions.Should().BeEmpty();
        }
    }

    [Fact]
    public void SetType_ShouldUpdateTypeAndSetDefaultValueToNull_WhenNewTypeIsDifferentFromOldType()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" ).SetDefaultValue( 123 );

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.SetType<int>();
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.TypeDefinition.Should().BeSameAs( schema.Database.TypeDefinitions.GetByType<int>() );
            sut.DefaultValue.Should().BeNull();

            actions.Should().HaveCount( 1 );
            actions.ElementAtOrDefault( 0 )
                .Sql.Should()
                .SatisfySql(
                    @"CREATE TABLE ""__foo_T__{GUID}__"" (
                      ""C1"" ANY NOT NULL,
                      ""C2"" INTEGER NOT NULL,
                      CONSTRAINT ""foo_PK_T"" PRIMARY KEY (""C1"" ASC)
                    ) WITHOUT ROWID;",
                    @"INSERT INTO ""__foo_T__{GUID}__"" (""C1"", ""C2"")
                    SELECT
                      ""foo_T"".""C1"",
                      CAST(""foo_T"".""C2"" AS INTEGER) AS ""C2""
                    FROM ""foo_T"";",
                    "DROP TABLE \"foo_T\";",
                    "ALTER TABLE \"__foo_T__{GUID}__\" RENAME TO \"foo_T\";" );
        }
    }

    [Fact]
    public void SetType_ShouldDoNothing_WhenNewTypeIsDifferentFromOldTypeButSqliteTypeRemainsTheSameAndDefaultValueIsNull()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" ).SetType<bool>();

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.SetType<int>();
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.TypeDefinition.Should().BeSameAs( schema.Database.TypeDefinitions.GetByType<int>() );
            actions.Should().BeEmpty();
        }
    }

    [Fact]
    public void SetType_ShouldThrowSqlObjectBuilderException_WhenColumnIsRemoved()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" );
        sut.Remove();

        var action = Lambda.Of( () => sut.SetType<int>() );

        action.Should()
            .ThrowExactly<SqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == SqliteDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void SetType_ShouldThrowSqlObjectBuilderException_WhenColumnIsUsedInIndex()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" );
        table.Constraints.CreateIndex( sut.Asc() );

        var action = Lambda.Of( () => sut.SetType<int>() );

        action.Should()
            .ThrowExactly<SqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == SqliteDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void SetType_ShouldThrowSqlObjectBuilderException_WhenColumnIsUsedInView()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" );
        schema.Objects.CreateView( "V", table.ToRecordSet().ToDataSource().Select( s => new[] { s.From["C2"].AsSelf() } ) );

        var action = Lambda.Of( () => sut.SetType<int>() );

        action.Should()
            .ThrowExactly<SqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == SqliteDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void SetType_ShouldThrowSqlObjectBuilderException_WhenTypeDefinitionDoesNotBelongToDatabase()
    {
        var definition = SqliteDatabaseBuilderMock.Create().TypeDefinitions.GetByType<int>();
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" );

        var action = Lambda.Of( () => sut.SetType( definition ) );

        action.Should()
            .ThrowExactly<SqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == SqliteDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void SetType_ShouldThrowSqlObjectCastException_WhenTypeDefinitionIsOfInvalidType()
    {
        var definition = Substitute.For<ISqlColumnTypeDefinition>();
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" );

        var action = Lambda.Of( () => ((ISqlColumnBuilder)sut).SetType( definition ) );

        action.Should()
            .ThrowExactly<SqlObjectCastException>()
            .AndMatch( e => e.Dialect == SqliteDialect.Instance && e.Expected == typeof( SqlColumnTypeDefinition ) );
    }

    [Theory]
    [InlineData( true )]
    [InlineData( false )]
    public void MarkAsNullable_ShouldDoNothing_WhenNewValueEqualsOldValue(bool value)
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" ).MarkAsNullable( value );

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.MarkAsNullable( value );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            actions.Should().BeEmpty();
        }
    }

    [Theory]
    [InlineData( true )]
    [InlineData( false )]
    public void MarkAsNullable_ShouldDoNothing_WhenValueChangeIsFollowedByChangeToOriginal(bool value)
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" ).MarkAsNullable( value );

        var actionCount = schema.Database.GetPendingActionCount();
        sut.MarkAsNullable( ! value );
        var result = sut.MarkAsNullable( value );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            actions.Should().BeEmpty();
        }
    }

    [Fact]
    public void MarkAsNullable_ShouldUpdateIsNullableToTrue_WhenOldValueIsFalse()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" );

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.MarkAsNullable();
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.IsNullable.Should().BeTrue();

            actions.Should().HaveCount( 1 );
            actions.ElementAtOrDefault( 0 )
                .Sql.Should()
                .SatisfySql(
                    @"CREATE TABLE ""__foo_T__{GUID}__"" (
                      ""C1"" ANY NOT NULL,
                      ""C2"" ANY,
                      CONSTRAINT ""foo_PK_T"" PRIMARY KEY (""C1"" ASC)
                    ) WITHOUT ROWID;",
                    @"INSERT INTO ""__foo_T__{GUID}__"" (""C1"", ""C2"")
                    SELECT
                      ""foo_T"".""C1"",
                      ""foo_T"".""C2"" AS ""C2""
                    FROM ""foo_T"";",
                    "DROP TABLE \"foo_T\";",
                    "ALTER TABLE \"__foo_T__{GUID}__\" RENAME TO \"foo_T\";" );
        }
    }

    [Fact]
    public void MarkAsNullable_ShouldUpdateIsNullableToFalse_WhenOldValueIsTrueAndColumnDoesNotHaveDefaultValue()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" ).MarkAsNullable();

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.MarkAsNullable( false );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            result.IsNullable.Should().BeFalse();

            actions.Should().HaveCount( 1 );
            actions.ElementAtOrDefault( 0 )
                .Sql.Should()
                .SatisfySql(
                    @"CREATE TABLE ""__foo_T__{GUID}__"" (
                      ""C1"" ANY NOT NULL,
                      ""C2"" ANY NOT NULL,
                      CONSTRAINT ""foo_PK_T"" PRIMARY KEY (""C1"" ASC)
                    ) WITHOUT ROWID;",
                    @"INSERT INTO ""__foo_T__{GUID}__"" (""C1"", ""C2"")
                    SELECT
                      ""foo_T"".""C1"",
                      COALESCE(""foo_T"".""C2"", X'') AS ""C2""
                    FROM ""foo_T"";",
                    "DROP TABLE \"foo_T\";",
                    "ALTER TABLE \"__foo_T__{GUID}__\" RENAME TO \"foo_T\";" );
        }
    }

    [Fact]
    public void MarkAsNullable_ShouldUpdateIsNullableToFalse_WhenOldValueIsTrueAndColumnHasDefaultValue()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" ).MarkAsNullable().SetDefaultValue( new byte[] { 1, 2, 3 } );

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.MarkAsNullable( false );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            result.IsNullable.Should().BeFalse();

            actions.Should().HaveCount( 1 );
            actions.ElementAtOrDefault( 0 )
                .Sql.Should()
                .SatisfySql(
                    @"CREATE TABLE ""__foo_T__{GUID}__"" (
                      ""C1"" ANY NOT NULL,
                      ""C2"" ANY NOT NULL DEFAULT (X'010203'),
                      CONSTRAINT ""foo_PK_T"" PRIMARY KEY (""C1"" ASC)
                    ) WITHOUT ROWID;",
                    @"INSERT INTO ""__foo_T__{GUID}__"" (""C1"", ""C2"")
                    SELECT
                      ""foo_T"".""C1"",
                      COALESCE(""foo_T"".""C2"", X'010203') AS ""C2""
                    FROM ""foo_T"";",
                    "DROP TABLE \"foo_T\";",
                    "ALTER TABLE \"__foo_T__{GUID}__\" RENAME TO \"foo_T\";" );
        }
    }

    [Fact]
    public void MarkAsNullable_ShouldUpdateIsNullableToFalse_WhenOldValueIsTrueAndColumnTypeDefinitionChanged()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" ).MarkAsNullable();

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.SetType( SqliteDataType.Integer ).MarkAsNullable( false );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            result.IsNullable.Should().BeFalse();

            actions.Should().HaveCount( 1 );
            actions.ElementAtOrDefault( 0 )
                .Sql.Should()
                .SatisfySql(
                    @"CREATE TABLE ""__foo_T__{GUID}__"" (
                      ""C1"" ANY NOT NULL,
                      ""C2"" INTEGER NOT NULL,
                      CONSTRAINT ""foo_PK_T"" PRIMARY KEY (""C1"" ASC)
                    ) WITHOUT ROWID;",
                    @"INSERT INTO ""__foo_T__{GUID}__"" (""C1"", ""C2"")
                    SELECT
                      ""foo_T"".""C1"",
                      COALESCE(CAST(""foo_T"".""C2"" AS INTEGER), 0) AS ""C2""
                    FROM ""foo_T"";",
                    "DROP TABLE \"foo_T\";",
                    "ALTER TABLE \"__foo_T__{GUID}__\" RENAME TO \"foo_T\";" );
        }
    }

    [Theory]
    [InlineData( true )]
    [InlineData( false )]
    public void MarkAsNullable_ShouldThrowSqlObjectBuilderException_WhenColumnIsRemoved(bool value)
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" ).MarkAsNullable( ! value );
        sut.Remove();

        var action = Lambda.Of( () => sut.MarkAsNullable( value ) );

        action.Should()
            .ThrowExactly<SqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == SqliteDialect.Instance && e.Errors.Count == 1 );
    }

    [Theory]
    [InlineData( true )]
    [InlineData( false )]
    public void MarkAsNullable_ShouldThrowSqlObjectBuilderException_WhenColumnIsUsedInIndex(bool value)
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" ).MarkAsNullable( ! value );
        table.Constraints.CreateIndex( sut.Asc() );

        var action = Lambda.Of( () => sut.MarkAsNullable( value ) );

        action.Should()
            .ThrowExactly<SqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == SqliteDialect.Instance && e.Errors.Count == 1 );
    }

    [Theory]
    [InlineData( true )]
    [InlineData( false )]
    public void MarkAsNullable_ShouldThrowSqlObjectBuilderException_WhenColumnIsUsedInView(bool value)
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" ).MarkAsNullable( ! value );
        schema.Objects.CreateView( "V", table.ToRecordSet().ToDataSource().Select( s => new[] { s.From["C2"].AsSelf() } ) );

        var action = Lambda.Of( () => sut.MarkAsNullable( value ) );

        action.Should()
            .ThrowExactly<SqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == SqliteDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void SetDefaultValue_ShouldDoNothing_WhenNewValueEqualsOldValue()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" ).SetDefaultValue( 123 );

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.SetDefaultValue( sut.DefaultValue );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            actions.Should().BeEmpty();
        }
    }

    [Fact]
    public void SetDefaultValue_ShouldDoNothing_WhenValueChangeIsFollowedByChangeToOriginal()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" ).SetDefaultValue( 123 );
        var originalDefaultValue = sut.DefaultValue;

        var actionCount = schema.Database.GetPendingActionCount();
        sut.SetDefaultValue( (int?)42 );
        var result = sut.SetDefaultValue( originalDefaultValue );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            actions.Should().BeEmpty();
        }
    }

    [Fact]
    public void SetDefaultValue_ShouldUpdateDefaultValue_WhenNewValueIsDifferentFromOldValue()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" ).SetDefaultValue( 123 );

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.SetDefaultValue( 42 );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.DefaultValue.Should().BeEquivalentTo( SqlNode.Literal( 42 ) );

            actions.Should().HaveCount( 1 );
            actions.ElementAtOrDefault( 0 )
                .Sql.Should()
                .SatisfySql(
                    @"CREATE TABLE ""__foo_T__{GUID}__"" (
                      ""C1"" ANY NOT NULL,
                      ""C2"" ANY NOT NULL DEFAULT (42),
                      CONSTRAINT ""foo_PK_T"" PRIMARY KEY (""C1"" ASC)
                    ) WITHOUT ROWID;",
                    @"INSERT INTO ""__foo_T__{GUID}__"" (""C1"", ""C2"")
                    SELECT
                      ""foo_T"".""C1"",
                      ""foo_T"".""C2"" AS ""C2""
                    FROM ""foo_T"";",
                    "DROP TABLE \"foo_T\";",
                    "ALTER TABLE \"__foo_T__{GUID}__\" RENAME TO \"foo_T\";" );
        }
    }

    [Fact]
    public void SetDefaultValue_ShouldUpdateDefaultValue_WhenNewValueIsNull()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" ).SetDefaultValue( 123 );

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.SetDefaultValue( null );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.DefaultValue.Should().BeNull();

            actions.Should().HaveCount( 1 );
            actions.ElementAtOrDefault( 0 )
                .Sql.Should()
                .SatisfySql(
                    @"CREATE TABLE ""__foo_T__{GUID}__"" (
                      ""C1"" ANY NOT NULL,
                      ""C2"" ANY NOT NULL,
                      CONSTRAINT ""foo_PK_T"" PRIMARY KEY (""C1"" ASC)
                    ) WITHOUT ROWID;",
                    @"INSERT INTO ""__foo_T__{GUID}__"" (""C1"", ""C2"")
                    SELECT
                      ""foo_T"".""C1"",
                      ""foo_T"".""C2"" AS ""C2""
                    FROM ""foo_T"";",
                    "DROP TABLE \"foo_T\";",
                    "ALTER TABLE \"__foo_T__{GUID}__\" RENAME TO \"foo_T\";" );
        }
    }

    [Fact]
    public void SetDefaultValue_ShouldUpdateDefaultValue_WhenNewValueIsValidComplexExpression()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" );

        var actionCount = schema.Database.GetPendingActionCount();
        var defaultValue = SqlNode.Literal( 10 ) + SqlNode.Literal( 50 ) + SqlNode.Literal( 100 ).Max( SqlNode.Literal( 80 ) );
        var result = sut.SetDefaultValue( defaultValue );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.DefaultValue.Should().BeSameAs( defaultValue );

            actions.Should().HaveCount( 1 );
            actions.ElementAtOrDefault( 0 )
                .Sql.Should()
                .SatisfySql(
                    @"CREATE TABLE ""__foo_T__{GUID}__"" (
                      ""C1"" ANY NOT NULL,
                      ""C2"" ANY NOT NULL DEFAULT ((10 + 50) + MAX(100, 80)),
                      CONSTRAINT ""foo_PK_T"" PRIMARY KEY (""C1"" ASC)
                    ) WITHOUT ROWID;",
                    @"INSERT INTO ""__foo_T__{GUID}__"" (""C1"", ""C2"")
                    SELECT
                      ""foo_T"".""C1"",
                      ""foo_T"".""C2"" AS ""C2""
                    FROM ""foo_T"";",
                    "DROP TABLE \"foo_T\";",
                    "ALTER TABLE \"__foo_T__{GUID}__\" RENAME TO \"foo_T\";" );
        }
    }

    [Fact]
    public void SetDefaultValue_ShouldBePossible_WhenColumnIsUsedInIndex()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" );
        table.Constraints.CreateIndex( sut.Asc() );

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.SetDefaultValue( 123 );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.DefaultValue.Should().BeEquivalentTo( SqlNode.Literal( 123 ) );

            actions.Should().HaveCount( 1 );
            actions.ElementAtOrDefault( 0 )
                .Sql.Should()
                .SatisfySql(
                    "DROP INDEX \"foo_IX_T_C2A\";",
                    @"CREATE TABLE ""__foo_T__{GUID}__"" (
                      ""C1"" ANY NOT NULL,
                      ""C2"" ANY NOT NULL DEFAULT (123),
                      CONSTRAINT ""foo_PK_T"" PRIMARY KEY (""C1"" ASC)
                    ) WITHOUT ROWID;",
                    @"INSERT INTO ""__foo_T__{GUID}__"" (""C1"", ""C2"")
                    SELECT
                      ""foo_T"".""C1"",
                      ""foo_T"".""C2"" AS ""C2""
                    FROM ""foo_T"";",
                    "DROP TABLE \"foo_T\";",
                    "ALTER TABLE \"__foo_T__{GUID}__\" RENAME TO \"foo_T\";",
                    "CREATE INDEX \"foo_IX_T_C2A\" ON \"foo_T\" (\"C2\" ASC);" );
        }
    }

    [Fact]
    public void SetDefaultValue_ShouldBePossible_WhenColumnIsUsedInView()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" );
        schema.Objects.CreateView( "V", table.ToRecordSet().ToDataSource().Select( s => new[] { s.From["C2"].AsSelf() } ) );

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.SetDefaultValue( (int?)123 );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.DefaultValue.Should().BeEquivalentTo( SqlNode.Literal( 123 ) );

            actions.Should().HaveCount( 1 );
            actions.ElementAtOrDefault( 0 )
                .Sql.Should()
                .SatisfySql(
                    @"CREATE TABLE ""__foo_T__{GUID}__"" (
                      ""C1"" ANY NOT NULL,
                      ""C2"" ANY NOT NULL DEFAULT (123),
                      CONSTRAINT ""foo_PK_T"" PRIMARY KEY (""C1"" ASC)
                    ) WITHOUT ROWID;",
                    @"INSERT INTO ""__foo_T__{GUID}__"" (""C1"", ""C2"")
                    SELECT
                      ""foo_T"".""C1"",
                      ""foo_T"".""C2"" AS ""C2""
                    FROM ""foo_T"";",
                    "DROP TABLE \"foo_T\";",
                    "ALTER TABLE \"__foo_T__{GUID}__\" RENAME TO \"foo_T\";" );
        }
    }

    [Fact]
    public void SetDefaultValue_ShouldThrowSqlObjectBuilderException_WhenColumnIsRemoved()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" );
        sut.Remove();

        var action = Lambda.Of( () => sut.SetDefaultValue( 42 ) );

        action.Should()
            .ThrowExactly<SqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == SqliteDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void SetDefaultValue_ShouldThrowSqlObjectBuilderException_WhenColumnIsGenerated()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" ).SetComputation( SqlColumnComputation.Virtual( SqlNode.Literal( 1 ) ) );

        var action = Lambda.Of( () => sut.SetDefaultValue( 42 ) );

        action.Should()
            .ThrowExactly<SqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == SqliteDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void SetDefaultValue_ShouldThrowSqlObjectBuilderException_WhenExpressionIsInvalid()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" );

        var action = Lambda.Of( () => sut.SetDefaultValue( table.ToRecordSet().GetField( "C1" ) ) );

        action.Should()
            .ThrowExactly<SqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == SqliteDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void SetComputation_ShouldDoNothing_WhenNewNullValueEqualsOldValue()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" );

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.SetComputation( null );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            actions.Should().BeEmpty();
        }
    }

    [Fact]
    public void SetComputation_ShouldDoNothing_WhenNewNonNullValueEqualsOldValue()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" ).SetComputation( SqlColumnComputation.Virtual( SqlNode.Literal( 1 ) ) );

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.SetComputation( sut.Computation );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            actions.Should().BeEmpty();
        }
    }

    [Fact]
    public void SetComputation_ShouldDoNothing_WhenValueChangeIsFollowedByChangeToOriginal()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" ).SetComputation( SqlColumnComputation.Virtual( SqlNode.Literal( 1 ) ) );
        var originalComputation = sut.Computation;

        var actionCount = schema.Database.GetPendingActionCount();
        sut.SetComputation( SqlColumnComputation.Stored( SqlNode.Literal( 42 ) ) );
        var result = sut.SetComputation( originalComputation );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            actions.Should().BeEmpty();
        }
    }

    [Fact]
    public void SetComputation_ShouldUpdateComputation_WhenNewNullValueIsDifferentFromOldValue()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var other = table.Columns.Create( "C2" );
        var sut = table.Columns.Create( "C3" ).SetComputation( SqlColumnComputation.Stored( other.Node + SqlNode.Literal( 1 ) ) );

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.SetComputation( null );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.Computation.Should().BeNull();
            sut.ReferencedComputationColumns.Should().BeEmpty();
            other.ReferencingObjects.Should().BeEmpty();

            actions.Should().HaveCount( 1 );
            actions.ElementAtOrDefault( 0 )
                .Sql.Should()
                .SatisfySql(
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
                      ""foo_T"".""C3"" AS ""C3""
                    FROM ""foo_T"";",
                    "DROP TABLE \"foo_T\";",
                    "ALTER TABLE \"__foo_T__{GUID}__\" RENAME TO \"foo_T\";" );
        }
    }

    [Fact]
    public void SetComputation_ShouldUpdateComputation_WhenNewValueIsDifferentFromOldNullValue()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var other = table.Columns.Create( "C2" );
        var sut = table.Columns.Create( "C3" );
        var computation = SqlColumnComputation.Stored( other.Node + SqlNode.Literal( 1 ) );

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.SetComputation( computation );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.Computation.Should().Be( computation );
            sut.ReferencedComputationColumns.Should().BeSequentiallyEqualTo( other );

            other.ReferencingObjects.Should()
                .BeSequentiallyEqualTo(
                    SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( sut, property: "Computation" ), other ) );

            actions.Should().HaveCount( 1 );
            actions.ElementAtOrDefault( 0 )
                .Sql.Should()
                .SatisfySql(
                    @"CREATE TABLE ""__foo_T__{GUID}__"" (
                      ""C1"" ANY NOT NULL,
                      ""C2"" ANY NOT NULL,
                      ""C3"" ANY NOT NULL GENERATED ALWAYS AS (""C2"" + 1) STORED,
                      CONSTRAINT ""foo_PK_T"" PRIMARY KEY (""C1"" ASC)
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
    public void SetComputation_ShouldUpdateComputation_WhenNewExpressionIsDifferentFromOldExpression()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var other = table.Columns.Create( "C2" );
        var oldOther = table.Columns.Create( "C4" );
        var sut = table.Columns.Create( "C3" ).SetComputation( SqlColumnComputation.Stored( oldOther.Node + SqlNode.Literal( 1 ) ) );
        var computation = SqlColumnComputation.Stored( other.Node + SqlNode.Literal( 1 ) );

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.SetComputation( computation );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.Computation.Should().Be( computation );
            sut.ReferencedComputationColumns.Should().BeSequentiallyEqualTo( other );

            oldOther.ReferencingObjects.Should().BeEmpty();
            other.ReferencingObjects.Should()
                .BeSequentiallyEqualTo(
                    SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( sut, property: "Computation" ), other ) );

            actions.Should().HaveCount( 1 );
            actions.ElementAtOrDefault( 0 )
                .Sql.Should()
                .SatisfySql(
                    @"CREATE TABLE ""__foo_T__{GUID}__"" (
                      ""C1"" ANY NOT NULL,
                      ""C2"" ANY NOT NULL,
                      ""C4"" ANY NOT NULL,
                      ""C3"" ANY NOT NULL GENERATED ALWAYS AS (""C2"" + 1) STORED,
                      CONSTRAINT ""foo_PK_T"" PRIMARY KEY (""C1"" ASC)
                    ) WITHOUT ROWID;",
                    @"INSERT INTO ""__foo_T__{GUID}__"" (""C1"", ""C2"", ""C4"")
                    SELECT
                      ""foo_T"".""C1"",
                      ""foo_T"".""C2"",
                      ""foo_T"".""C4""
                    FROM ""foo_T"";",
                    "DROP TABLE \"foo_T\";",
                    "ALTER TABLE \"__foo_T__{GUID}__\" RENAME TO \"foo_T\";" );
        }
    }

    [Theory]
    [InlineData( SqlColumnComputationStorage.Virtual, SqlColumnComputationStorage.Stored, "STORED" )]
    [InlineData( SqlColumnComputationStorage.Stored, SqlColumnComputationStorage.Virtual, "VIRTUAL" )]
    public void SetComputation_ShouldUpdateComputation_WhenNewStorageIsDifferentFromOldStorage(
        SqlColumnComputationStorage oldStorage,
        SqlColumnComputationStorage newStorage,
        string expectedStorage)
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var other = table.Columns.Create( "C2" );
        var expression = other.Node + SqlNode.Literal( 1 );
        var sut = table.Columns.Create( "C3" ).SetComputation( new SqlColumnComputation( expression, oldStorage ) );
        table.Constraints.CreateIndex( sut.Asc() );

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.SetComputation( new SqlColumnComputation( expression, newStorage ) );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.Computation.Should().Be( new SqlColumnComputation( expression, newStorage ) );
            sut.ReferencedComputationColumns.Should().BeSequentiallyEqualTo( other );

            other.ReferencingObjects.Should()
                .BeSequentiallyEqualTo(
                    SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( sut, property: "Computation" ), other ) );

            actions.Should().HaveCount( 1 );
            actions.ElementAtOrDefault( 0 )
                .Sql.Should()
                .SatisfySql(
                    "DROP INDEX \"foo_IX_T_C3A\";",
                    $@"CREATE TABLE ""__foo_T__{{GUID}}__"" (
                      ""C1"" ANY NOT NULL,
                      ""C2"" ANY NOT NULL,
                      ""C3"" ANY NOT NULL GENERATED ALWAYS AS (""C2"" + 1) {expectedStorage},
                      CONSTRAINT ""foo_PK_T"" PRIMARY KEY (""C1"" ASC)
                    ) WITHOUT ROWID;",
                    @"INSERT INTO ""__foo_T__{GUID}__"" (""C1"", ""C2"")
                    SELECT
                      ""foo_T"".""C1"",
                      ""foo_T"".""C2""
                    FROM ""foo_T"";",
                    "DROP TABLE \"foo_T\";",
                    "ALTER TABLE \"__foo_T__{GUID}__\" RENAME TO \"foo_T\";",
                    "CREATE INDEX \"foo_IX_T_C3A\" ON \"foo_T\" (\"C3\" ASC);" );
        }
    }

    [Fact]
    public void SetComputation_ShouldSetDefaultValueToNull_WhenValueIsNotNull()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var other = table.Columns.Create( "C2" );
        var sut = table.Columns.Create( "C3" ).SetDefaultValue( 42 );
        var computation = SqlColumnComputation.Stored( other.Node + SqlNode.Literal( 1 ) );

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.SetComputation( computation );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.DefaultValue.Should().BeNull();
            sut.Computation.Should().Be( computation );
            sut.ReferencedComputationColumns.Should().BeSequentiallyEqualTo( other );

            other.ReferencingObjects.Should()
                .BeSequentiallyEqualTo(
                    SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( sut, property: "Computation" ), other ) );

            actions.Should().HaveCount( 1 );
            actions.ElementAtOrDefault( 0 )
                .Sql.Should()
                .SatisfySql(
                    @"CREATE TABLE ""__foo_T__{GUID}__"" (
                      ""C1"" ANY NOT NULL,
                      ""C2"" ANY NOT NULL,
                      ""C3"" ANY NOT NULL GENERATED ALWAYS AS (""C2"" + 1) STORED,
                      CONSTRAINT ""foo_PK_T"" PRIMARY KEY (""C1"" ASC)
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
    public void SetComputation_ShouldThrowSqlObjectBuilderException_WhenColumnIsRemoved()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" );
        sut.Remove();

        var action = Lambda.Of( () => sut.SetComputation( SqlColumnComputation.Virtual( SqlNode.Literal( 1 ) ) ) );

        action.Should()
            .ThrowExactly<SqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == SqliteDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void SetComputation_ShouldThrowSqlObjectBuilderException_WhenColumnIsReferencedAndOldValueIsNull()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" );
        table.Constraints.CreateIndex( sut.Asc() );

        var action = Lambda.Of( () => sut.SetComputation( SqlColumnComputation.Virtual( SqlNode.Literal( 1 ) ) ) );

        action.Should()
            .ThrowExactly<SqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == SqliteDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void SetComputation_ShouldThrowSqlObjectBuilderException_WhenExpressionIsInvalidAndOldValueIsNull()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" );

        var action = Lambda.Of( () => sut.SetComputation( SqlColumnComputation.Virtual( SqlNode.RawRecordSet( "bar" )["x"] ) ) );

        action.Should()
            .ThrowExactly<SqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == SqliteDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void SetComputation_ShouldThrowSqlObjectBuilderException_WhenColumnIsReferencedAndOldValueIsNotNull()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" ).SetComputation( SqlColumnComputation.Stored( SqlNode.Literal( 42 ) ) );
        table.Constraints.CreateIndex( sut.Asc() );

        var action = Lambda.Of( () => sut.SetComputation( SqlColumnComputation.Virtual( SqlNode.Literal( 1 ) ) ) );

        action.Should()
            .ThrowExactly<SqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == SqliteDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void SetComputation_ShouldThrowSqlObjectBuilderException_WhenExpressionIsInvalidAndOldValueIsNotNull()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" ).SetComputation( SqlColumnComputation.Stored( SqlNode.Literal( 42 ) ) );

        var action = Lambda.Of( () => sut.SetComputation( SqlColumnComputation.Virtual( SqlNode.RawRecordSet( "bar" )["x"] ) ) );

        action.Should()
            .ThrowExactly<SqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == SqliteDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void SetComputation_ShouldThrowSqlObjectBuilderException_WhenColumnIsReferencedAndNewValueIsNull()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" ).SetComputation( SqlColumnComputation.Virtual( SqlNode.Literal( 1 ) ) );
        table.Constraints.CreateIndex( sut.Asc() );

        var action = Lambda.Of( () => sut.SetComputation( null ) );

        action.Should()
            .ThrowExactly<SqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == SqliteDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void SetComputation_ShouldThrowSqlObjectBuilderException_WhenColumnReferencesSelf()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" );

        var action = Lambda.Of( () => sut.SetComputation( SqlColumnComputation.Virtual( sut.Node + SqlNode.Literal( 1 ) ) ) );

        action.Should()
            .ThrowExactly<SqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == SqliteDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void Remove_ShouldRemoveColumnAndClearReferencedComputationColumns()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var other = table.Columns.Create( "C1" );
        var pk = table.Constraints.SetPrimaryKey( other.Asc() );
        var sut = table.Columns.Create( "C2" ).SetComputation( SqlColumnComputation.Stored( other.Node + SqlNode.Literal( 1 ) ) );

        var actionCount = schema.Database.GetPendingActionCount();
        sut.SetName( "bar" ).Remove();
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            table.Columns.Contains( sut.Name ).Should().BeFalse();
            sut.ReferencedComputationColumns.Should().BeEmpty();
            sut.Computation.Should().BeNull();
            sut.IsRemoved.Should().BeTrue();

            other.ReferencingObjects.Should()
                .BeSequentiallyEqualTo( SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( pk.Index ), other ) );

            actions.Should().HaveCount( 1 );
            actions.ElementAtOrDefault( 0 ).Sql.Should().SatisfySql( "ALTER TABLE \"foo_T\" DROP COLUMN \"C2\";" );
        }
    }

    [Fact]
    public void Remove_ShouldDoNothing_WhenColumnIsRemoved()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" );

        schema.Database.Changes.CompletePendingChanges();
        sut.Remove();

        var actionCount = schema.Database.GetPendingActionCount();
        sut.Remove();
        var actions = schema.Database.GetLastPendingActions( actionCount );

        actions.Should().BeEmpty();
    }

    [Fact]
    public void Remove_ShouldThrowSqlObjectBuilderException_WhenColumnIsReferencedByIndex()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var t1 = schema.Objects.CreateTable( "T1" );
        t1.Constraints.SetPrimaryKey( t1.Columns.Create( "C1" ).Asc() );
        var sut = t1.Columns.Create( "C2" );
        t1.Constraints.CreateIndex( sut.Asc() );

        var action = Lambda.Of( () => sut.Remove() );

        action.Should().ThrowExactly<SqlObjectBuilderException>();
    }

    [Fact]
    public void Remove_ShouldThrowSqlObjectBuilderException_WhenColumnIsReferencedByIndexFilter()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var t1 = schema.Objects.CreateTable( "T1" );
        t1.Constraints.SetPrimaryKey( t1.Columns.Create( "C1" ).Asc() );
        var sut = t1.Columns.Create( "C2" );
        t1.Constraints.CreateIndex( t1.Columns.Create( "C3" ).Asc() ).SetFilter( t => t["C2"] != null );

        var action = Lambda.Of( () => sut.Remove() );

        action.Should().ThrowExactly<SqlObjectBuilderException>();
    }

    [Fact]
    public void Remove_ShouldThrowSqlObjectBuilderException_WhenColumnIsReferencedByView()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var t1 = schema.Objects.CreateTable( "T1" );
        t1.Constraints.SetPrimaryKey( t1.Columns.Create( "C1" ).Asc() );
        var sut = t1.Columns.Create( "C2" );
        schema.Objects.CreateView( "V", t1.ToRecordSet().ToDataSource().Select( s => new[] { s.From["C2"].AsSelf() } ) );

        var action = Lambda.Of( () => sut.Remove() );

        action.Should().ThrowExactly<SqlObjectBuilderException>();
    }

    [Fact]
    public void Remove_ShouldThrowSqlObjectBuilderException_WhenColumnIsReferencedByCheck()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var t1 = schema.Objects.CreateTable( "T1" );
        t1.Constraints.SetPrimaryKey( t1.Columns.Create( "C1" ).Asc() );
        var sut = t1.Columns.Create( "C2" );
        t1.Constraints.CreateCheck( t1.Node["C2"] != SqlNode.Literal( 0 ) );

        var action = Lambda.Of( () => sut.Remove() );

        action.Should().ThrowExactly<SqlObjectBuilderException>();
    }

    [Fact]
    public void ForSqlite_ShouldInvokeAction_WhenColumnIsSqlite()
    {
        var action = Substitute.For<Action<SqliteColumnBuilder>>();
        var sut = SqliteDatabaseBuilderMock.Create().Schemas.Default.Objects.CreateTable( "T" ).Columns.Create( "C" );

        var result = sut.ForSqlite( action );

        result.Should().BeSameAs( sut );
        action.Verify().CallAt( 0 ).Exists().And.Arguments.Should().BeSequentiallyEqualTo( sut );
    }

    [Fact]
    public void ForSqlite_ShouldNotInvokeAction_WhenColumnIsNotSqlite()
    {
        var action = Substitute.For<Action<SqliteColumnBuilder>>();
        var sut = Substitute.For<ISqlColumnBuilder>();

        var result = sut.ForSqlite( action );

        result.Should().BeSameAs( sut );
        action.Verify().CallCount.Should().Be( 0 );
    }
}
