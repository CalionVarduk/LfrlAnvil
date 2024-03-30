using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.Sql.Expressions.Visitors;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.TestExtensions.Sql.Mocks;

namespace LfrlAnvil.PostgreSql.Tests;

public partial class PostgreSqlNodeInterpreterTests
{
    public class Definitions : TestsBase
    {
        [Fact]
        public void Visit_ShouldInterpretColumnDefinition_WhenNullable()
        {
            var sut = CreateInterpreter();
            sut.Visit( SqlNode.Column<int>( "a", isNullable: true ) );
            sut.Context.Sql.ToString().Should().Be( "\"a\" INT4" );
        }

        [Fact]
        public void Visit_ShouldInterpretColumnDefinition_WhenNonNullable()
        {
            var sut = CreateInterpreter();
            sut.Visit( SqlNode.Column<string>( "a", isNullable: false ) );
            sut.Context.Sql.ToString().Should().Be( "\"a\" VARCHAR NOT NULL" );
        }

        [Fact]
        public void Visit_ShouldInterpretColumnDefinition_WithDefaultLiteralValue()
        {
            var sut = CreateInterpreter();
            sut.Visit( SqlNode.Column<int>( "a", isNullable: false, defaultValue: SqlNode.Literal( 10 ) ) );
            sut.Context.Sql.ToString().Should().Be( "\"a\" INT4 NOT NULL DEFAULT (10)" );
        }

        [Fact]
        public void Visit_ShouldInterpretColumnDefinition_WithDefaultNullValue()
        {
            var sut = CreateInterpreter();
            sut.Visit( SqlNode.Column<int>( "a", isNullable: true, defaultValue: SqlNode.Null() ) );
            sut.Context.Sql.ToString().Should().Be( "\"a\" INT4 DEFAULT (NULL)" );
        }

        [Fact]
        public void Visit_ShouldInterpretColumnDefinition_WithDefaultExpressionValue()
        {
            var sut = CreateInterpreter();
            sut.Visit( SqlNode.Column<int>( "a", isNullable: false, defaultValue: SqlNode.Literal( 10 ) + SqlNode.Literal( 20 ) ) );
            sut.Context.Sql.ToString().Should().Be( "\"a\" INT4 NOT NULL DEFAULT (10 + 20)" );
        }

        [Fact]
        public void Visit_ShouldInterpretColumnDefinition_WithDbType_WhenNullable()
        {
            var sut = CreateInterpreter();
            var typeDef = sut.TypeDefinitions.GetByDataType( PostgreSqlDataType.Int4 );
            sut.Visit( SqlNode.Column( "a", typeDef, isNullable: true ) );
            sut.Context.Sql.ToString().Should().Be( "\"a\" INT4" );
        }

        [Fact]
        public void Visit_ShouldInterpretColumnDefinition_WithDbType_WhenNonNullable()
        {
            var sut = CreateInterpreter();
            var typeDef = sut.TypeDefinitions.GetByDataType( PostgreSqlDataType.VarChar );
            sut.Visit( SqlNode.Column( "a", typeDef, isNullable: false ) );
            sut.Context.Sql.ToString().Should().Be( "\"a\" VARCHAR NOT NULL" );
        }

        [Theory]
        [InlineData( SqlColumnComputationStorage.Virtual )]
        [InlineData( SqlColumnComputationStorage.Stored )]
        public void Visit_ShouldInterpretColumnDefinition_WithComputation_WhenNonNullable(SqlColumnComputationStorage storage)
        {
            var sut = CreateInterpreter();
            sut.Visit( SqlNode.Column<string>( "a", computation: new SqlColumnComputation( SqlNode.Literal( "abc" ), storage ) ) );
            sut.Context.Sql.ToString().Should().Be( "\"a\" VARCHAR NOT NULL GENERATED ALWAYS AS ('abc') STORED" );
        }

        [Theory]
        [InlineData( SqlColumnComputationStorage.Virtual )]
        [InlineData( SqlColumnComputationStorage.Stored )]
        public void Visit_ShouldInterpretColumnDefinition_WithComputation_WhenNullable(SqlColumnComputationStorage storage)
        {
            var sut = CreateInterpreter();
            sut.Visit(
                SqlNode.Column<string>(
                    "a",
                    isNullable: true,
                    computation: new SqlColumnComputation( SqlNode.Literal( "abc" ), storage ) ) );

            sut.Context.Sql.ToString().Should().Be( "\"a\" VARCHAR GENERATED ALWAYS AS ('abc') STORED" );
        }

        [Theory]
        [InlineData( SqlColumnComputationStorage.Virtual, "VIRTUAL" )]
        [InlineData( SqlColumnComputationStorage.Stored, "STORED" )]
        public void Visit_ShouldInterpretColumnDefinition_WithComputation_WhenVirtualGeneratedColumnStorageParsingIsEnabled(
            SqlColumnComputationStorage storage,
            string expectedStorage)
        {
            var sut = CreateInterpreter( PostgreSqlNodeInterpreterOptions.Default.EnableVirtualGeneratedColumnStorageParsing() );
            sut.Visit(
                SqlNode.Column<string>(
                    "a",
                    isNullable: true,
                    computation: new SqlColumnComputation( SqlNode.Literal( "abc" ), storage ) ) );

            sut.Context.Sql.ToString().Should().Be( $"\"a\" VARCHAR GENERATED ALWAYS AS ('abc') {expectedStorage}" );
        }

        [Fact]
        public void Visit_ShouldInterpretPrimaryKeyDefinition()
        {
            var sut = CreateInterpreter();
            var table = SqlNode.CreateTable(
                    SqlRecordSetInfo.Create( "foo", "bar" ),
                    new[] { SqlNode.Column<int>( "a" ), SqlNode.Column<int>( "b" ) } )
                .RecordSet;

            var node = SqlNode.PrimaryKey(
                SqlSchemaObjectName.Create( "foo", "PK_foobar" ),
                new[] { table["a"].Asc(), table["b"].Desc() } );

            sut.Visit( node );

            sut.Context.Sql.ToString().Should().Be( "CONSTRAINT \"PK_foobar\" PRIMARY KEY (\"foo\".\"bar\".\"a\", \"foo\".\"bar\".\"b\")" );
        }

        [Theory]
        [InlineData( ReferenceBehavior.Values.Cascade, ReferenceBehavior.Values.Cascade )]
        [InlineData( ReferenceBehavior.Values.Restrict, ReferenceBehavior.Values.Cascade )]
        [InlineData( ReferenceBehavior.Values.Cascade, ReferenceBehavior.Values.Restrict )]
        [InlineData( ReferenceBehavior.Values.Restrict, ReferenceBehavior.Values.Restrict )]
        [InlineData( ReferenceBehavior.Values.SetNull, ReferenceBehavior.Values.NoAction )]
        [InlineData( ReferenceBehavior.Values.NoAction, ReferenceBehavior.Values.SetNull )]
        public void Visit_ShouldInterpretForeignKeyDefinition(ReferenceBehavior.Values onDelete, ReferenceBehavior.Values onUpdate)
        {
            var sut = CreateInterpreter();
            var onDeleteBehavior = ReferenceBehavior.GetBehavior( onDelete );
            var onUpdateBehavior = ReferenceBehavior.GetBehavior( onUpdate );

            var qux = SqlTableMock.Create<int>( "qux", new[] { "a", "b" } ).ToRecordSet();
            var table = SqlNode.CreateTable(
                    SqlRecordSetInfo.Create( "foo", "bar" ),
                    new[] { SqlNode.Column<int>( "a" ), SqlNode.Column<int>( "b" ) } )
                .RecordSet;

            var node = SqlNode.ForeignKey(
                SqlSchemaObjectName.Create( "foo", "FK_foobar_REF_qux" ),
                new SqlDataFieldNode[] { table["a"], table["b"] },
                qux,
                new SqlDataFieldNode[] { qux["a"], qux["b"] },
                onDeleteBehavior,
                onUpdateBehavior );

            sut.Visit( node );

            sut.Context.Sql.ToString()
                .Should()
                .Be(
                    $"CONSTRAINT \"FK_foobar_REF_qux\" FOREIGN KEY (\"a\", \"b\") REFERENCES \"common\".\"qux\" (\"a\", \"b\") ON DELETE {onDeleteBehavior.Name} ON UPDATE {onUpdateBehavior.Name}" );
        }

        [Fact]
        public void Visit_ShouldInterpretCheckDefinition()
        {
            var sut = CreateInterpreter();
            var table = SqlNode.CreateTable( SqlRecordSetInfo.Create( "foo", "bar" ), new[] { SqlNode.Column<int>( "a" ) } ).RecordSet;
            var node = SqlNode.Check( SqlSchemaObjectName.Create( "foo", "CHK_foobar" ), table["a"] > SqlNode.Literal( 10 ) );
            sut.Visit( node );

            sut.Context.Sql.ToString().Should().Be( "CONSTRAINT \"CHK_foobar\" CHECK (\"foo\".\"bar\".\"a\" > 10)" );
        }

        [Theory]
        [InlineData( false, false, "\"foo\".\"bar\"" )]
        [InlineData( true, false, "\"foo\"" )]
        [InlineData( false, true, "\"foo\".\"bar\"" )]
        [InlineData( true, true, "\"foo\"" )]
        public void Visit_ShouldInterpretCreateTable(bool isTemporary, bool ifNotExists, string expectedName)
        {
            var sut = CreateInterpreter();
            var info = isTemporary ? SqlRecordSetInfo.CreateTemporary( "foo" ) : SqlRecordSetInfo.Create( "foo", "bar" );
            var node = SqlNode.CreateTable(
                info,
                new[]
                {
                    SqlNode.Column<int>( "x" ),
                    SqlNode.Column<string>( "y", isNullable: true ),
                    SqlNode.Column<double>( "z", defaultValue: SqlNode.Literal( 10.5 ) )
                },
                ifNotExists: ifNotExists,
                constraintsProvider: t =>
                {
                    var qux = SqlNode.RawRecordSet( "qux" );
                    return SqlCreateTableConstraints.Empty
                        .WithPrimaryKey( SqlNode.PrimaryKey( SqlSchemaObjectName.Create( "PK_foobar" ), new[] { t["x"].Asc() } ) )
                        .WithForeignKeys(
                            SqlNode.ForeignKey(
                                SqlSchemaObjectName.Create( "FK_foobar_REF_qux" ),
                                new SqlDataFieldNode[] { t["y"] },
                                qux,
                                new SqlDataFieldNode[] { qux["y"] } ) )
                        .WithChecks( SqlNode.Check( SqlSchemaObjectName.Create( "CHK_foobar" ), t["z"] > SqlNode.Literal( 100.0 ) ) );
                } );

            sut.Visit( node );

            sut.Context.Sql.ToString()
                .Should()
                .Be(
                    $@"CREATE{(isTemporary ? " TEMPORARY" : string.Empty)} TABLE{(ifNotExists ? " IF NOT EXISTS" : string.Empty)} {expectedName} (
  ""x"" INT4 NOT NULL,
  ""y"" VARCHAR,
  ""z"" FLOAT8 NOT NULL DEFAULT (10.5),
  CONSTRAINT ""PK_foobar"" PRIMARY KEY (""x""),
  CONSTRAINT ""FK_foobar_REF_qux"" FOREIGN KEY (""y"") REFERENCES qux (""y"") ON DELETE RESTRICT ON UPDATE RESTRICT,
  CONSTRAINT ""CHK_foobar"" CHECK (""z"" > 100.0)
)" );
        }

        [Theory]
        [InlineData( false, "\"foo\".\"bar\"" )]
        [InlineData( true, "\"foo\"" )]
        public void Visit_ShouldInterpretCreateView(bool isTemporary, string expectedName)
        {
            var sut = CreateInterpreter();
            var info = isTemporary ? SqlRecordSetInfo.CreateTemporary( "foo" ) : SqlRecordSetInfo.Create( "foo", "bar" );
            var node = SqlNode.CreateView( info, replaceIfExists: false, source: SqlNode.RawQuery( "SELECT * FROM qux" ) );
            sut.Visit( node );

            sut.Context.Sql.ToString()
                .Should()
                .Be(
                    $@"CREATE{(isTemporary ? " TEMPORARY" : string.Empty)} VIEW {expectedName} AS
SELECT * FROM qux" );
        }

        [Theory]
        [InlineData( false, "\"foo\".\"bar\"" )]
        [InlineData( true, "\"foo\"" )]
        public void Visit_ShouldInterpretCreateView_WithReplaceIfExists(bool isTemporary, string expectedName)
        {
            var sut = CreateInterpreter();
            var info = isTemporary ? SqlRecordSetInfo.CreateTemporary( "foo" ) : SqlRecordSetInfo.Create( "foo", "bar" );
            var node = SqlNode.CreateView( info, replaceIfExists: true, source: SqlNode.RawQuery( "SELECT * FROM qux" ) );
            sut.Visit( node );

            sut.Context.Sql.ToString()
                .Should()
                .Be(
                    $@"CREATE OR REPLACE{(isTemporary ? " TEMPORARY" : string.Empty)} VIEW {expectedName} AS
SELECT * FROM qux" );
        }

        [Theory]
        [InlineData( false, "INDEX" )]
        [InlineData( true, "UNIQUE INDEX" )]
        public void Visit_ShouldInterpretCreateIndex(bool isUnique, string expectedType)
        {
            var sut = CreateInterpreter();
            var qux = SqlTableMock.Create<int>( "qux", new[] { "a", "b" }, new[] { "a" }, schemaName: "foo" ).Node;
            var node = SqlNode.CreateIndex(
                SqlSchemaObjectName.Create( "foo", "bar" ),
                isUnique: isUnique,
                replaceIfExists: false,
                table: qux,
                columns: new[] { qux["a"].Asc(), qux["b"].Desc() } );

            sut.Visit( node );

            sut.Context.Sql.ToString().Should().Be( $"CREATE {expectedType} \"bar\" ON \"foo\".\"qux\" (\"a\" ASC, \"b\" DESC)" );
        }

        [Fact]
        public void Visit_ShouldInterpretCreateIndex_WithTemporaryTable()
        {
            var sut = CreateInterpreter();
            var qux = SqlNode.CreateTable(
                    SqlRecordSetInfo.CreateTemporary( "qux" ),
                    new[] { SqlNode.Column<int>( "a" ), SqlNode.Column<int>( "b" ) } )
                .RecordSet;

            var node = SqlNode.CreateIndex(
                SqlSchemaObjectName.Create( "bar" ),
                isUnique: false,
                replaceIfExists: false,
                table: qux,
                columns: new[] { qux["a"].Asc(), qux["b"].Desc() } );

            sut.Visit( node );

            sut.Context.Sql.ToString().Should().Be( "CREATE INDEX \"bar\" ON \"qux\" (\"a\" ASC, \"b\" DESC)" );
        }

        [Theory]
        [InlineData( false, "INDEX" )]
        [InlineData( true, "UNIQUE INDEX" )]
        public void Visit_ShouldInterpretCreateIndex_WithReplaceIfExists(bool isUnique, string expectedType)
        {
            var sut = CreateInterpreter();
            var qux = SqlTableMock.Create<int>( "qux", new[] { "a", "b" }, new[] { "a" }, schemaName: "foo" ).Node;
            var node = SqlNode.CreateIndex(
                SqlSchemaObjectName.Create( "foo", "bar" ),
                isUnique: isUnique,
                replaceIfExists: true,
                table: qux,
                columns: new[] { qux["a"].Asc(), qux["b"].Desc() } );

            sut.Visit( node );

            sut.Context.Sql.ToString()
                .Should()
                .Be(
                    $@"DROP INDEX IF EXISTS ""foo"".""bar"";
CREATE {expectedType} ""bar"" ON ""foo"".""qux"" (""a"" ASC, ""b"" DESC)" );
        }

        [Fact]
        public void Visit_ShouldInterpretCreateIndex_WithFilter()
        {
            var sut = CreateInterpreter();
            var qux = SqlNode.RawRecordSet( "qux" );
            var node = SqlNode.CreateIndex(
                SqlSchemaObjectName.Create( "bar" ),
                isUnique: false,
                replaceIfExists: false,
                table: qux,
                columns: new[] { qux["a"].Asc(), qux["b"].Desc() },
                filter: qux["a"] != null );

            sut.Visit( node );

            sut.Context.Sql.ToString().Should().Be( "CREATE INDEX \"bar\" ON qux (\"a\" ASC, \"b\" DESC) WHERE (\"a\" IS NOT NULL)" );
        }

        [Fact]
        public void Visit_ShouldInterpretCreateIndex_WithExpression()
        {
            var sut = CreateInterpreter();
            var qux = SqlNode.RawRecordSet( "qux" );
            var node = SqlNode.CreateIndex(
                SqlSchemaObjectName.Create( "foo", "bar" ),
                isUnique: false,
                replaceIfExists: false,
                table: qux,
                columns: new[] { (qux["a"] + qux["b"]).Asc() } );

            sut.Visit( node );

            sut.Context.Sql.ToString().Should().Be( "CREATE INDEX \"bar\" ON qux ((\"a\" + \"b\") ASC)" );
        }

        [Theory]
        [InlineData( false, "ALTER TABLE \"foo\".\"bar\" RENAME TO \"lorem\"" )]
        [InlineData( true, "ALTER TABLE \"foo\" RENAME TO \"lorem\"" )]
        public void Visit_ShouldInterpretRenameTable_WithoutSchemaNameChange(bool isTemporary, string expected)
        {
            var sut = CreateInterpreter();
            var info = isTemporary ? SqlRecordSetInfo.CreateTemporary( "foo" ) : SqlRecordSetInfo.Create( "foo", "bar" );
            sut.Visit( SqlNode.RenameTable( info, SqlSchemaObjectName.Create( info.Name.Schema, "lorem" ) ) );
            sut.Context.Sql.ToString().Should().Be( expected );
        }

        [Fact]
        public void Visit_ShouldInterpretRenameTable_WithSchemaNameChange()
        {
            var sut = CreateInterpreter();
            var info = SqlRecordSetInfo.Create( "foo", "bar" );
            sut.Visit( SqlNode.RenameTable( info, SqlSchemaObjectName.Create( "qux", "lorem" ) ) );
            sut.Context.Sql.ToString()
                .Should()
                .Be(
                    @"ALTER TABLE ""foo"".""bar"" SET SCHEMA ""qux"";
ALTER TABLE ""qux"".""bar"" RENAME TO ""lorem""" );
        }

        [Theory]
        [InlineData( false, "ALTER TABLE \"foo\".\"bar\" RENAME COLUMN \"qux\" TO \"lorem\"" )]
        [InlineData( true, "ALTER TABLE \"foo\" RENAME COLUMN \"qux\" TO \"lorem\"" )]
        public void Visit_ShouldInterpretRenameColumn(bool isTableTemporary, string expected)
        {
            var sut = CreateInterpreter();
            var info = isTableTemporary ? SqlRecordSetInfo.CreateTemporary( "foo" ) : SqlRecordSetInfo.Create( "foo", "bar" );
            sut.Visit( SqlNode.RenameColumn( info, "qux", "lorem" ) );
            sut.Context.Sql.ToString().Should().Be( expected );
        }

        [Theory]
        [InlineData( false, "ALTER TABLE \"foo\".\"bar\" ADD COLUMN \"qux\" INT4 NOT NULL DEFAULT (10)" )]
        [InlineData( true, "ALTER TABLE \"foo\" ADD COLUMN \"qux\" INT4 NOT NULL DEFAULT (10)" )]
        public void Visit_ShouldInterpretAddColumn(bool isTableTemporary, string expected)
        {
            var sut = CreateInterpreter();
            var info = isTableTemporary ? SqlRecordSetInfo.CreateTemporary( "foo" ) : SqlRecordSetInfo.Create( "foo", "bar" );
            sut.Visit( SqlNode.AddColumn( info, SqlNode.Column<int>( "qux", defaultValue: SqlNode.Literal( 10 ) ) ) );
            sut.Context.Sql.ToString().Should().Be( expected );
        }

        [Theory]
        [InlineData( false, "ALTER TABLE \"foo\".\"bar\" DROP COLUMN \"qux\"" )]
        [InlineData( true, "ALTER TABLE \"foo\" DROP COLUMN \"qux\"" )]
        public void Visit_ShouldInterpretDropColumn(bool isTableTemporary, string expected)
        {
            var sut = CreateInterpreter();
            var info = isTableTemporary ? SqlRecordSetInfo.CreateTemporary( "foo" ) : SqlRecordSetInfo.Create( "foo", "bar" );
            sut.Visit( SqlNode.DropColumn( info, "qux" ) );
            sut.Context.Sql.ToString().Should().Be( expected );
        }

        [Theory]
        [InlineData( false, false, "DROP TABLE \"foo\".\"bar\"" )]
        [InlineData( true, false, "DROP TABLE \"foo\"" )]
        [InlineData( false, true, "DROP TABLE IF EXISTS \"foo\".\"bar\"" )]
        [InlineData( true, true, "DROP TABLE IF EXISTS \"foo\"" )]
        public void Visit_ShouldInterpretDropTable(bool isTemporary, bool ifExists, string expected)
        {
            var sut = CreateInterpreter();
            var info = isTemporary ? SqlRecordSetInfo.CreateTemporary( "foo" ) : SqlRecordSetInfo.Create( "foo", "bar" );
            sut.Visit( SqlNode.DropTable( info, ifExists ) );
            sut.Context.Sql.ToString().Should().Be( expected );
        }

        [Theory]
        [InlineData( false, false, "DROP VIEW \"foo\".\"bar\"" )]
        [InlineData( true, false, "DROP VIEW \"foo\"" )]
        [InlineData( false, true, "DROP VIEW IF EXISTS \"foo\".\"bar\"" )]
        [InlineData( true, true, "DROP VIEW IF EXISTS \"foo\"" )]
        public void Visit_ShouldInterpretDropView(bool isTemporary, bool ifExists, string expected)
        {
            var sut = CreateInterpreter();
            var info = isTemporary ? SqlRecordSetInfo.CreateTemporary( "foo" ) : SqlRecordSetInfo.Create( "foo", "bar" );
            sut.Visit( SqlNode.DropView( info, ifExists ) );
            sut.Context.Sql.ToString().Should().Be( expected );
        }

        [Theory]
        [InlineData( false, false, "DROP INDEX \"foo\".\"bar\"" )]
        [InlineData( true, false, "DROP INDEX IF EXISTS \"foo\".\"bar\"" )]
        [InlineData( false, true, "DROP INDEX \"bar\"" )]
        [InlineData( true, true, "DROP INDEX IF EXISTS \"bar\"" )]
        public void Visit_ShouldInterpretDropIndex(bool ifExists, bool isRecordSetTemporary, string expected)
        {
            var sut = CreateInterpreter();
            var recordSet = isRecordSetTemporary ? SqlRecordSetInfo.CreateTemporary( "qux" ) : SqlRecordSetInfo.Create( "foo", "qux" );
            var name = isRecordSetTemporary ? SqlSchemaObjectName.Create( "bar" ) : SqlSchemaObjectName.Create( "foo", "bar" );

            sut.Visit( SqlNode.DropIndex( recordSet, name, ifExists ) );

            sut.Context.Sql.ToString().Should().Be( expected );
        }
    }
}
