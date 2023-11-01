using System.Linq;
using LfrlAnvil.Functional;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Extensions;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.Sqlite.Exceptions;
using LfrlAnvil.Sqlite.Extensions;
using LfrlAnvil.Sqlite.Objects.Builders;
using LfrlAnvil.Sqlite.Tests.Helpers;
using LfrlAnvil.TestExtensions.FluentAssertions;

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
    public void Asc_ShouldReturnCorrectResult()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var sut = table.Columns.Create( "C" );

        var result = ((ISqlColumnBuilder)sut).Asc();

        using ( new AssertionScope() )
        {
            result.Column.Should().BeSameAs( sut );
            result.Ordering.Should().BeSameAs( OrderBy.Asc );
            result.ToString().Should().Be( "foo_T.C ASC" );
        }
    }

    [Fact]
    public void Desc_ShouldReturnCorrectResult()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var sut = table.Columns.Create( "C" );

        var result = ((ISqlColumnBuilder)sut).Desc();

        using ( new AssertionScope() )
        {
            result.Column.Should().BeSameAs( sut );
            result.Ordering.Should().BeSameAs( OrderBy.Desc );
            result.ToString().Should().Be( "foo_T.C DESC" );
        }
    }

    [Fact]
    public void Create_ShouldMarkTableForReconstructionAndAutomaticallySetDefaultValue()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );

        var startStatementCount = schema.Database.GetPendingStatements().Length;

        var sut = table.Columns.Create( "C2" );
        var statements = schema.Database.GetPendingStatements().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            table.Columns.Get( sut.Name ).Should().BeSameAs( sut );
            sut.Name.Should().Be( "C2" );
            sut.FullName.Should().Be( "foo_T.C2" );
            sut.DefaultValue.Should().BeEquivalentTo( SqlNode.Literal<object>( Array.Empty<byte>() ) );

            statements.Should().HaveCount( 1 );
            statements.ElementAtOrDefault( 0 )
                .Should()
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
    public void Create_ShouldMarkTableForReconstruction_WithoutDefaultValueWhenColumnIsNullable()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );

        var startStatementCount = schema.Database.GetPendingStatements().Length;

        var sut = table.Columns.Create( "C2" ).MarkAsNullable();
        var statements = schema.Database.GetPendingStatements().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            table.Columns.Get( sut.Name ).Should().BeSameAs( sut );
            sut.Name.Should().Be( "C2" );
            sut.FullName.Should().Be( "foo_T.C2" );
            sut.DefaultValue.Should().BeNull();

            statements.Should().HaveCount( 1 );
            statements.ElementAtOrDefault( 0 )
                .Should()
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
    public void Create_ShouldMarkTableForReconstruction_WhenDefaultValueIsDefinedExplicitly()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );

        var startStatementCount = schema.Database.GetPendingStatements().Length;

        var sut = table.Columns.Create( "C2" ).SetDefaultValue( new byte[] { 1, 2, 3 } );
        var statements = schema.Database.GetPendingStatements().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            table.Columns.Get( sut.Name ).Should().BeSameAs( sut );
            sut.Name.Should().Be( "C2" );
            sut.FullName.Should().Be( "foo_T.C2" );

            statements.Should().HaveCount( 1 );
            statements.ElementAtOrDefault( 0 )
                .Should()
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
    public void Create_WithReusedRemovedColumnName_ShouldTreatTheColumnAsModified()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var removed = table.Columns.Create( "C2" );

        var startStatementCount = schema.Database.GetPendingStatements().Length;

        removed.SetName( "C3" ).Remove();
        var sut = table.Columns.Create( "C2" ).SetType<string>();
        var statements = schema.Database.GetPendingStatements().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            table.Columns.Get( sut.Name ).Should().BeSameAs( sut );
            sut.Name.Should().Be( "C2" );
            sut.FullName.Should().Be( "foo_T.C2" );
            sut.DefaultValue.Should().BeNull();
            removed.IsRemoved.Should().BeTrue();

            statements.Should().HaveCount( 1 );
            statements.ElementAtOrDefault( 0 )
                .Should()
                .SatisfySql(
                    @"CREATE TABLE ""__foo_T__{GUID}__"" (
                      ""C1"" ANY NOT NULL,
                      ""C2"" TEXT NOT NULL,
                      CONSTRAINT ""foo_PK_T"" PRIMARY KEY (""C1"" ASC)
                    ) WITHOUT ROWID;",
                    @"INSERT INTO ""__foo_T__{GUID}__"" (""C1"", ""C2"")
                    SELECT
                      ""foo_T"".""C1"",
                      CAST(""foo_T"".""C2"" AS TEXT) AS ""C2""
                    FROM ""foo_T"";",
                    "DROP TABLE \"foo_T\";",
                    "ALTER TABLE \"__foo_T__{GUID}__\" RENAME TO \"foo_T\";" );
        }
    }

    [Fact]
    public void Create_FollowedByRemove_ShouldDoNothing()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );

        var startStatementCount = schema.Database.GetPendingStatements().Length;

        var sut = table.Columns.Create( "C2" );
        sut.Remove();
        var statements = schema.Database.GetPendingStatements().Slice( startStatementCount ).ToArray();

        statements.Should().BeEmpty();
    }

    [Fact]
    public void SetName_ShouldDoNothing_WhenNewNameEqualsOldName()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" );

        var startStatementCount = schema.Database.GetPendingStatements().Length;

        var result = ((ISqlColumnBuilder)sut).SetName( sut.Name );
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
        var sut = table.Columns.Create( "C2" );
        var oldName = sut.Name;

        var startStatementCount = schema.Database.GetPendingStatements().Length;

        sut.SetName( "bar" );
        var result = ((ISqlColumnBuilder)sut).SetName( oldName );

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
        table.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" );
        var oldName = sut.Name;
        var node = sut.Node;

        var startStatementCount = schema.Database.GetPendingStatements().Length;

        var result = ((ISqlColumnBuilder)sut).SetName( "bar" );
        var statements = schema.Database.GetPendingStatements().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.Name.Should().Be( "bar" );
            sut.FullName.Should().Be( "foo_T.bar" );
            table.Columns.Get( "bar" ).Should().BeSameAs( sut );
            table.Columns.Contains( oldName ).Should().BeFalse();
            node.Name.Should().Be( "bar" );

            statements.Should().HaveCount( 1 );
            statements.ElementAtOrDefault( 0 ).Should().SatisfySql( "ALTER TABLE \"foo_T\" RENAME COLUMN \"C2\" TO \"bar\";" );
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
        var sut = table.Columns.Create( "C2" );

        var action = Lambda.Of( () => ((ISqlColumnBuilder)sut).SetName( name ) );

        action.Should()
            .ThrowExactly<SqliteObjectBuilderException>()
            .AndMatch( e => e.Dialect == SqliteDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void SetName_ShouldThrowSqliteObjectBuilderException_WhenColumnIsRemoved()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" );
        sut.Remove();

        var action = Lambda.Of( () => ((ISqlColumnBuilder)sut).SetName( "bar" ) );

        action.Should()
            .ThrowExactly<SqliteObjectBuilderException>()
            .AndMatch( e => e.Dialect == SqliteDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void SetName_ShouldThrowSqliteObjectBuilderException_WhenNewNameAlreadyExistsInTableColumns()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" );

        var action = Lambda.Of( () => ((ISqlColumnBuilder)sut).SetName( "C1" ) );

        action.Should()
            .ThrowExactly<SqliteObjectBuilderException>()
            .AndMatch( e => e.Dialect == SqliteDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void SetName_ShouldThrowSqliteObjectBuilderException_WhenColumnIsUsedInIndex()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" );
        table.Indexes.Create( sut.Asc() );

        var action = Lambda.Of( () => ((ISqlColumnBuilder)sut).SetName( "C3" ) );

        action.Should()
            .ThrowExactly<SqliteObjectBuilderException>()
            .AndMatch( e => e.Dialect == SqliteDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void SetName_ShouldThrowSqliteObjectBuilderException_WhenColumnIsUsedInView()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" );
        schema.Objects.CreateView( "V", table.ToRecordSet().ToDataSource().Select( s => new[] { s.From["C2"].AsSelf() } ) );

        var action = Lambda.Of( () => ((ISqlColumnBuilder)sut).SetName( "C3" ) );

        action.Should()
            .ThrowExactly<SqliteObjectBuilderException>()
            .AndMatch( e => e.Dialect == SqliteDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void SetType_ShouldDoNothing_WhenNewTypeEqualsOldType()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" );

        var startStatementCount = schema.Database.GetPendingStatements().Length;

        var result = ((ISqlColumnBuilder)sut).SetType( SqliteDataType.Any );
        var statements = schema.Database.GetPendingStatements().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            statements.Should().BeEmpty();
        }
    }

    [Fact]
    public void SetType_ShouldDoNothing_WhenTypeChangeIsFollowedByChangeToOriginal()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" );

        var startStatementCount = schema.Database.GetPendingStatements().Length;

        sut.SetType<int>();
        var result = ((ISqlColumnBuilder)sut).SetType( SqliteDataType.Any );

        var statements = schema.Database.GetPendingStatements().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            statements.Should().BeEmpty();
        }
    }

    [Fact]
    public void SetType_ShouldUpdateTypeAndSetDefaultValueToNull_WhenNewTypeIsDifferentFromOldType()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" ).SetDefaultValue( 123 );

        var startStatementCount = schema.Database.GetPendingStatements().Length;

        var result = ((ISqlColumnBuilder)sut).SetType<int>();
        var statements = schema.Database.GetPendingStatements().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.TypeDefinition.Should().BeSameAs( schema.Database.TypeDefinitions.GetByType<int>() );
            sut.DefaultValue.Should().BeNull();

            statements.Should().HaveCount( 1 );
            statements.ElementAtOrDefault( 0 )
                .Should()
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
        table.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" ).SetType<bool>();

        var startStatementCount = schema.Database.GetPendingStatements().Length;

        var result = ((ISqlColumnBuilder)sut).SetType<int>();
        var statements = schema.Database.GetPendingStatements().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.TypeDefinition.Should().BeSameAs( schema.Database.TypeDefinitions.GetByType<int>() );
            statements.Should().BeEmpty();
        }
    }

    [Fact]
    public void SetType_ShouldThrowSqliteObjectBuilderException_WhenColumnIsRemoved()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" );
        sut.Remove();

        var action = Lambda.Of( () => ((ISqlColumnBuilder)sut).SetType<int>() );

        action.Should()
            .ThrowExactly<SqliteObjectBuilderException>()
            .AndMatch( e => e.Dialect == SqliteDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void SetType_ShouldThrowSqliteObjectBuilderException_WhenColumnIsUsedInIndex()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" );
        table.Indexes.Create( sut.Asc() );

        var action = Lambda.Of( () => ((ISqlColumnBuilder)sut).SetType<int>() );

        action.Should()
            .ThrowExactly<SqliteObjectBuilderException>()
            .AndMatch( e => e.Dialect == SqliteDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void SetType_ShouldThrowSqliteObjectBuilderException_WhenColumnIsUsedInView()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" );
        schema.Objects.CreateView( "V", table.ToRecordSet().ToDataSource().Select( s => new[] { s.From["C2"].AsSelf() } ) );

        var action = Lambda.Of( () => ((ISqlColumnBuilder)sut).SetType<int>() );

        action.Should()
            .ThrowExactly<SqliteObjectBuilderException>()
            .AndMatch( e => e.Dialect == SqliteDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void SetType_ShouldThrowSqliteObjectBuilderException_WhenTypeDefinitionDoesNotBelongToDatabase()
    {
        var definition = SqliteDatabaseBuilderMock.Create().TypeDefinitions.GetByType<int>();
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" );

        var action = Lambda.Of( () => ((ISqlColumnBuilder)sut).SetType( definition ) );

        action.Should()
            .ThrowExactly<SqliteObjectBuilderException>()
            .AndMatch( e => e.Dialect == SqliteDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void SetType_ShouldThrowSqliteObjectCastException_WhenTypeDefinitionIsOfInvalidType()
    {
        var definition = Substitute.For<ISqlColumnTypeDefinition>();
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" );

        var action = Lambda.Of( () => ((ISqlColumnBuilder)sut).SetType( definition ) );

        action.Should()
            .ThrowExactly<SqliteObjectCastException>()
            .AndMatch( e => e.Dialect == SqliteDialect.Instance && e.Expected == typeof( SqliteColumnTypeDefinition ) );
    }

    [Theory]
    [InlineData( true )]
    [InlineData( false )]
    public void MarkAsNullable_ShouldDoNothing_WhenNewValueEqualsOldValue(bool value)
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" ).MarkAsNullable( value );

        var startStatementCount = schema.Database.GetPendingStatements().Length;

        var result = ((ISqlColumnBuilder)sut).MarkAsNullable( value );
        var statements = schema.Database.GetPendingStatements().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            statements.Should().BeEmpty();
        }
    }

    [Theory]
    [InlineData( true )]
    [InlineData( false )]
    public void MarkAsNullable_ShouldDoNothing_WhenValueChangeIsFollowedByChangeToOriginal(bool value)
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" ).MarkAsNullable( value );

        var startStatementCount = schema.Database.GetPendingStatements().Length;

        sut.MarkAsNullable( ! value );
        var result = ((ISqlColumnBuilder)sut).MarkAsNullable( value );

        var statements = schema.Database.GetPendingStatements().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            statements.Should().BeEmpty();
        }
    }

    [Fact]
    public void MarkAsNullable_ShouldUpdateIsNullableToTrue_WhenOldValueIsFalse()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" );

        var startStatementCount = schema.Database.GetPendingStatements().Length;

        var result = ((ISqlColumnBuilder)sut).MarkAsNullable();
        var statements = schema.Database.GetPendingStatements().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.IsNullable.Should().BeTrue();

            statements.Should().HaveCount( 1 );
            statements.ElementAtOrDefault( 0 )
                .Should()
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
        table.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" ).MarkAsNullable();

        var startStatementCount = schema.Database.GetPendingStatements().Length;

        var result = ((ISqlColumnBuilder)sut).MarkAsNullable( false );
        var statements = schema.Database.GetPendingStatements().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            result.IsNullable.Should().BeFalse();

            statements.Should().HaveCount( 1 );
            statements.ElementAtOrDefault( 0 )
                .Should()
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
        table.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" ).MarkAsNullable().SetDefaultValue( new byte[] { 1, 2, 3 } );

        var startStatementCount = schema.Database.GetPendingStatements().Length;

        var result = ((ISqlColumnBuilder)sut).MarkAsNullable( false );
        var statements = schema.Database.GetPendingStatements().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            result.IsNullable.Should().BeFalse();

            statements.Should().HaveCount( 1 );
            statements.ElementAtOrDefault( 0 )
                .Should()
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
        table.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" ).MarkAsNullable();

        var startStatementCount = schema.Database.GetPendingStatements().Length;

        var result = ((ISqlColumnBuilder)sut.SetType( SqliteDataType.Integer )).MarkAsNullable( false );
        var statements = schema.Database.GetPendingStatements().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            result.IsNullable.Should().BeFalse();

            statements.Should().HaveCount( 1 );
            statements.ElementAtOrDefault( 0 )
                .Should()
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
    public void MarkAsNullable_ShouldThrowSqliteObjectBuilderException_WhenColumnIsRemoved(bool value)
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" ).MarkAsNullable( ! value );
        sut.Remove();

        var action = Lambda.Of( () => ((ISqlColumnBuilder)sut).MarkAsNullable( value ) );

        action.Should()
            .ThrowExactly<SqliteObjectBuilderException>()
            .AndMatch( e => e.Dialect == SqliteDialect.Instance && e.Errors.Count == 1 );
    }

    [Theory]
    [InlineData( true )]
    [InlineData( false )]
    public void MarkAsNullable_ShouldThrowSqliteObjectBuilderException_WhenColumnIsUsedInIndex(bool value)
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" ).MarkAsNullable( ! value );
        table.Indexes.Create( sut.Asc() );

        var action = Lambda.Of( () => ((ISqlColumnBuilder)sut).MarkAsNullable( value ) );

        action.Should()
            .ThrowExactly<SqliteObjectBuilderException>()
            .AndMatch( e => e.Dialect == SqliteDialect.Instance && e.Errors.Count == 1 );
    }

    [Theory]
    [InlineData( true )]
    [InlineData( false )]
    public void MarkAsNullable_ShouldThrowSqliteObjectBuilderException_WhenColumnIsUsedInView(bool value)
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" ).MarkAsNullable( ! value );
        schema.Objects.CreateView( "V", table.ToRecordSet().ToDataSource().Select( s => new[] { s.From["C2"].AsSelf() } ) );

        var action = Lambda.Of( () => ((ISqlColumnBuilder)sut).MarkAsNullable( value ) );

        action.Should()
            .ThrowExactly<SqliteObjectBuilderException>()
            .AndMatch( e => e.Dialect == SqliteDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void SetDefaultValue_ShouldDoNothing_WhenNewValueEqualsOldValue()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" ).SetDefaultValue( 123 );

        var startStatementCount = schema.Database.GetPendingStatements().Length;

        var result = ((ISqlColumnBuilder)sut).SetDefaultValue( sut.DefaultValue );
        var statements = schema.Database.GetPendingStatements().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            statements.Should().BeEmpty();
        }
    }

    [Fact]
    public void SetDefaultValue_ShouldDoNothing_WhenValueChangeIsFollowedByChangeToOriginal()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" ).SetDefaultValue( 123 );
        var originalDefaultValue = sut.DefaultValue;

        var startStatementCount = schema.Database.GetPendingStatements().Length;

        sut.SetDefaultValue( (int?)42 );
        var result = ((ISqlColumnBuilder)sut).SetDefaultValue( originalDefaultValue );

        var statements = schema.Database.GetPendingStatements().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            statements.Should().BeEmpty();
        }
    }

    [Fact]
    public void SetDefaultValue_ShouldUpdateDefaultValue_WhenNewValueIsDifferentFromOldValue()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" ).SetDefaultValue( 123 );

        var startStatementCount = schema.Database.GetPendingStatements().Length;

        var result = ((ISqlColumnBuilder)sut).SetDefaultValue( 42 );
        var statements = schema.Database.GetPendingStatements().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.DefaultValue.Should().BeEquivalentTo( SqlNode.Literal( 42 ) );

            statements.Should().HaveCount( 1 );
            statements.ElementAtOrDefault( 0 )
                .Should()
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
        table.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" ).SetDefaultValue( 123 );

        var startStatementCount = schema.Database.GetPendingStatements().Length;

        var result = ((ISqlColumnBuilder)sut).SetDefaultValue( null );
        var statements = schema.Database.GetPendingStatements().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.DefaultValue.Should().BeNull();

            statements.Should().HaveCount( 1 );
            statements.ElementAtOrDefault( 0 )
                .Should()
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
        table.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" );

        var startStatementCount = schema.Database.GetPendingStatements().Length;

        var defaultValue = SqlNode.Literal( 10 ) + SqlNode.Literal( 50 ) + SqlNode.Literal( 100 ).Max( SqlNode.Literal( 80 ) );
        var result = ((ISqlColumnBuilder)sut).SetDefaultValue( defaultValue );

        var statements = schema.Database.GetPendingStatements().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.DefaultValue.Should().BeSameAs( defaultValue );

            statements.Should().HaveCount( 1 );
            statements.ElementAtOrDefault( 0 )
                .Should()
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
        table.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" );
        table.Indexes.Create( sut.Asc() );

        var startStatementCount = schema.Database.GetPendingStatements().Length;

        var result = ((ISqlColumnBuilder)sut).SetDefaultValue( 123 );
        var statements = schema.Database.GetPendingStatements().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.DefaultValue.Should().BeEquivalentTo( SqlNode.Literal( 123 ) );

            statements.Should().HaveCount( 1 );
            statements.ElementAtOrDefault( 0 )
                .Should()
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
        table.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" );
        schema.Objects.CreateView( "V", table.ToRecordSet().ToDataSource().Select( s => new[] { s.From["C2"].AsSelf() } ) );

        var startStatementCount = schema.Database.GetPendingStatements().Length;

        var result = ((ISqlColumnBuilder)sut).SetDefaultValue( (int?)123 );
        var statements = schema.Database.GetPendingStatements().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.DefaultValue.Should().BeEquivalentTo( SqlNode.Literal( 123 ) );

            statements.Should().HaveCount( 1 );
            statements.ElementAtOrDefault( 0 )
                .Should()
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
    public void SetDefaultValue_ShouldThrowSqliteObjectBuilderException_WhenColumnIsRemoved()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" );
        sut.Remove();

        var action = Lambda.Of( () => ((ISqlColumnBuilder)sut).SetDefaultValue( 42 ) );

        action.Should()
            .ThrowExactly<SqliteObjectBuilderException>()
            .AndMatch( e => e.Dialect == SqliteDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void SetDefaultValue_ShouldThrowSqliteObjectBuilderException_WhenExpressionIsInvalid()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" );

        var action = Lambda.Of( () => ((ISqlColumnBuilder)sut).SetDefaultValue( table.ToRecordSet().GetField( "C1" ) ) );

        action.Should()
            .ThrowExactly<SqliteObjectBuilderException>()
            .AndMatch( e => e.Dialect == SqliteDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void Remove_ShouldRemoveColumn()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" );

        var startStatementCount = schema.Database.GetPendingStatements().Length;

        sut.SetName( "bar" ).Remove();
        var statements = schema.Database.GetPendingStatements().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            table.Columns.Contains( sut.Name ).Should().BeFalse();
            sut.IsRemoved.Should().BeTrue();
            sut.ReferencingIndexes.Should().BeEmpty();

            statements.Should().HaveCount( 1 );
            statements.ElementAtOrDefault( 0 ).Should().SatisfySql( "ALTER TABLE \"foo_T\" DROP COLUMN \"C2\";" );
        }
    }

    [Fact]
    public void Remove_ShouldDoNothing_WhenColumnIsRemoved()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" );

        _ = schema.Database.GetPendingStatements();
        sut.Remove();
        var startStatementCount = schema.Database.GetPendingStatements().Length;

        sut.Remove();
        var statements = schema.Database.GetPendingStatements().Slice( startStatementCount ).ToArray();

        statements.Should().BeEmpty();
    }

    [Fact]
    public void Remove_ShouldThrowSqliteObjectBuilderException_WhenColumnIsReferencedByIndexes()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var t1 = schema.Objects.CreateTable( "T1" );
        t1.SetPrimaryKey( t1.Columns.Create( "C1" ).Asc() );
        var sut = t1.Columns.Create( "C2" );
        t1.Indexes.Create( sut.Asc() );

        var action = Lambda.Of( () => sut.Remove() );

        action.Should().ThrowExactly<SqliteObjectBuilderException>();
    }

    [Fact]
    public void Remove_ShouldThrowSqliteObjectBuilderException_WhenColumnIsReferencedByIndexFilters()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var t1 = schema.Objects.CreateTable( "T1" );
        t1.SetPrimaryKey( t1.Columns.Create( "C1" ).Asc() );
        var sut = t1.Columns.Create( "C2" );
        t1.Indexes.Create( t1.Columns.Create( "C3" ).Asc() ).SetFilter( t => t["C2"] != null );

        var action = Lambda.Of( () => sut.Remove() );

        action.Should().ThrowExactly<SqliteObjectBuilderException>();
    }

    [Fact]
    public void Remove_ShouldThrowSqliteObjectBuilderException_WhenColumnIsReferencedByViews()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var t1 = schema.Objects.CreateTable( "T1" );
        t1.SetPrimaryKey( t1.Columns.Create( "C1" ).Asc() );
        var sut = t1.Columns.Create( "C2" );
        schema.Objects.CreateView( "V", t1.ToRecordSet().ToDataSource().Select( s => new[] { s.From["C2"].AsSelf() } ) );

        var action = Lambda.Of( () => sut.Remove() );

        action.Should().ThrowExactly<SqliteObjectBuilderException>();
    }

    [Fact]
    public void Remove_ShouldThrowSqliteObjectBuilderException_WhenColumnIsReferencedByChecks()
    {
        var schema = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var t1 = schema.Objects.CreateTable( "T1" );
        t1.SetPrimaryKey( t1.Columns.Create( "C1" ).Asc() );
        var sut = t1.Columns.Create( "C2" );
        t1.Checks.Create( t1.RecordSet["C2"] != SqlNode.Literal( 0 ) );

        var action = Lambda.Of( () => sut.Remove() );

        action.Should().ThrowExactly<SqliteObjectBuilderException>();
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
