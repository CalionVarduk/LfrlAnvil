using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.Sql.Expressions.Visitors;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.TestExtensions.Sql.Mocks;

namespace LfrlAnvil.Sqlite.Tests;

public partial class SqliteNodeInterpreterTests
{
    public class Definitions : TestsBase
    {
        [Fact]
        public void Visit_ShouldInterpretColumnDefinition_WhenNullable()
        {
            var sut = CreateInterpreter();
            sut.Visit( SqlNode.Column<int>( "a", isNullable: true ) );
            sut.Context.Sql.ToString().Should().Be( "\"a\" INTEGER" );
        }

        [Fact]
        public void Visit_ShouldInterpretColumnDefinition_WhenNonNullable()
        {
            var sut = CreateInterpreter();
            sut.Visit( SqlNode.Column<string>( "a", isNullable: false ) );
            sut.Context.Sql.ToString().Should().Be( "\"a\" TEXT NOT NULL" );
        }

        [Fact]
        public void Visit_ShouldInterpretColumnDefinition_WithDefaultValue()
        {
            var sut = CreateInterpreter();
            sut.Visit( SqlNode.Column<string>( "a", defaultValue: SqlNode.Literal( "abc" ).Concat( SqlNode.Literal( "def" ) ) ) );
            sut.Context.Sql.ToString().Should().Be( "\"a\" TEXT NOT NULL DEFAULT ('abc' || 'def')" );
        }

        [Theory]
        [InlineData( SqlColumnComputationStorage.Virtual, "VIRTUAL" )]
        [InlineData( SqlColumnComputationStorage.Stored, "STORED" )]
        public void Visit_ShouldInterpretColumnDefinition_WithComputation_WhenNonNullable(
            SqlColumnComputationStorage storage,
            string expectedStorage)
        {
            var sut = CreateInterpreter();
            sut.Visit( SqlNode.Column<string>( "a", computation: new SqlColumnComputation( SqlNode.Literal( "abc" ), storage ) ) );
            sut.Context.Sql.ToString().Should().Be( $"\"a\" TEXT NOT NULL GENERATED ALWAYS AS ('abc') {expectedStorage}" );
        }

        [Theory]
        [InlineData( SqlColumnComputationStorage.Virtual, "VIRTUAL" )]
        [InlineData( SqlColumnComputationStorage.Stored, "STORED" )]
        public void Visit_ShouldInterpretColumnDefinition_WithComputation_WhenNullable(
            SqlColumnComputationStorage storage,
            string expectedStorage)
        {
            var sut = CreateInterpreter();
            sut.Visit(
                SqlNode.Column<string>(
                    "a",
                    isNullable: true,
                    computation: new SqlColumnComputation( SqlNode.Literal( "abc" ), storage ) ) );

            sut.Context.Sql.ToString().Should().Be( $"\"a\" TEXT GENERATED ALWAYS AS ('abc') {expectedStorage}" );
        }

        [Fact]
        public void Visit_ShouldInterpretColumnDefinition_WithDbType_WhenNullable()
        {
            var sut = CreateInterpreter();
            var typeDef = sut.TypeDefinitions.GetByDataType( SqliteDataType.Integer );
            sut.Visit( SqlNode.Column( "a", typeDef, isNullable: true ) );
            sut.Context.Sql.ToString().Should().Be( "\"a\" INTEGER" );
        }

        [Fact]
        public void Visit_ShouldInterpretColumnDefinition_WithDbType_WhenNonNullable()
        {
            var sut = CreateInterpreter();
            var typeDef = sut.TypeDefinitions.GetByDataType( SqliteDataType.Text );
            sut.Visit( SqlNode.Column( "a", typeDef, isNullable: false ) );
            sut.Context.Sql.ToString().Should().Be( "\"a\" TEXT NOT NULL" );
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

            sut.Context.Sql.ToString()
                .Should()
                .Be( "CONSTRAINT \"foo_PK_foobar\" PRIMARY KEY (\"foo_bar\".\"a\" ASC, \"foo_bar\".\"b\" DESC)" );
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
                    $"CONSTRAINT \"foo_FK_foobar_REF_qux\" FOREIGN KEY (\"a\", \"b\") REFERENCES \"common_qux\" (\"a\", \"b\") ON DELETE {onDeleteBehavior.Name} ON UPDATE {onUpdateBehavior.Name}" );
        }

        [Fact]
        public void Visit_ShouldInterpretCheckDefinition()
        {
            var sut = CreateInterpreter();
            var table = SqlNode.CreateTable( SqlRecordSetInfo.Create( "foo", "bar" ), new[] { SqlNode.Column<int>( "a" ) } ).RecordSet;
            var node = SqlNode.Check( SqlSchemaObjectName.Create( "foo", "CHK_foobar" ), table["a"] > SqlNode.Literal( 10 ) );
            sut.Visit( node );

            sut.Context.Sql.ToString().Should().Be( "CONSTRAINT \"foo_CHK_foobar\" CHECK (\"foo_bar\".\"a\" > 10)" );
        }

        [Theory]
        [InlineData( false, false, "\"foo_bar\"" )]
        [InlineData( true, false, "temp.\"foo\"" )]
        [InlineData( false, true, "\"foo_bar\"" )]
        [InlineData( true, true, "temp.\"foo\"" )]
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
                    $@"CREATE TABLE{(ifNotExists ? " IF NOT EXISTS" : string.Empty)} {expectedName} (
  ""x"" INTEGER NOT NULL,
  ""y"" TEXT,
  ""z"" REAL NOT NULL DEFAULT (10.5),
  CONSTRAINT ""PK_foobar"" PRIMARY KEY (""x"" ASC),
  CONSTRAINT ""FK_foobar_REF_qux"" FOREIGN KEY (""y"") REFERENCES qux (""y"") ON DELETE RESTRICT ON UPDATE RESTRICT,
  CONSTRAINT ""CHK_foobar"" CHECK (""z"" > 100.0)
) WITHOUT ROWID" );
        }

        [Theory]
        [InlineData( false, false, "\"foo_bar\"" )]
        [InlineData( true, false, "temp.\"foo\"" )]
        [InlineData( false, true, "\"foo_bar\"" )]
        [InlineData( true, true, "temp.\"foo\"" )]
        public void Visit_ShouldInterpretCreateTable_WithStrictModeEnabled(bool isTemporary, bool ifNotExists, string expectedName)
        {
            var sut = CreateInterpreter( SqliteNodeInterpreterOptions.Default.EnableStrictMode() );
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
                    $@"CREATE TABLE{(ifNotExists ? " IF NOT EXISTS" : string.Empty)} {expectedName} (
  ""x"" INTEGER NOT NULL,
  ""y"" TEXT,
  ""z"" REAL NOT NULL DEFAULT (10.5),
  CONSTRAINT ""PK_foobar"" PRIMARY KEY (""x"" ASC),
  CONSTRAINT ""FK_foobar_REF_qux"" FOREIGN KEY (""y"") REFERENCES qux (""y"") ON DELETE RESTRICT ON UPDATE RESTRICT,
  CONSTRAINT ""CHK_foobar"" CHECK (""z"" > 100.0)
) WITHOUT ROWID, STRICT" );
        }

        [Theory]
        [InlineData( false, "\"foo_bar\"" )]
        [InlineData( true, "temp.\"foo\"" )]
        public void Visit_ShouldInterpretCreateView(bool isTemporary, string expectedName)
        {
            var sut = CreateInterpreter();
            var info = isTemporary ? SqlRecordSetInfo.CreateTemporary( "foo" ) : SqlRecordSetInfo.Create( "foo", "bar" );
            var node = SqlNode.CreateView( info, replaceIfExists: false, source: SqlNode.RawQuery( "SELECT * FROM qux" ) );
            sut.Visit( node );

            sut.Context.Sql.ToString()
                .Should()
                .Be(
                    $@"CREATE VIEW {expectedName} AS
SELECT * FROM qux" );
        }

        [Theory]
        [InlineData( false, "\"foo_bar\"" )]
        [InlineData( true, "temp.\"foo\"" )]
        public void Visit_ShouldInterpretCreateView_WithReplaceIfExists(bool isTemporary, string expectedName)
        {
            var sut = CreateInterpreter();
            var info = isTemporary ? SqlRecordSetInfo.CreateTemporary( "foo" ) : SqlRecordSetInfo.Create( "foo", "bar" );
            var node = SqlNode.CreateView( info, replaceIfExists: true, source: SqlNode.RawQuery( "SELECT * FROM qux" ) );
            sut.Visit( node );

            sut.Context.Sql.ToString()
                .Should()
                .Be(
                    $@"DROP VIEW IF EXISTS {expectedName};
CREATE VIEW {expectedName} AS
SELECT * FROM qux" );
        }

        [Theory]
        [InlineData( false, "INDEX" )]
        [InlineData( true, "UNIQUE INDEX" )]
        public void Visit_ShouldInterpretCreateIndex(bool isUnique, string expectedType)
        {
            var sut = CreateInterpreter();
            var qux = SqlNode.RawRecordSet( "qux" );
            var node = SqlNode.CreateIndex(
                SqlSchemaObjectName.Create( "foo", "bar" ),
                isUnique: isUnique,
                replaceIfExists: false,
                table: qux,
                columns: new[] { qux["a"].Asc(), qux["b"].Desc() } );

            sut.Visit( node );

            sut.Context.Sql.ToString().Should().Be( $"CREATE {expectedType} \"foo_bar\" ON qux (\"a\" ASC, \"b\" DESC)" );
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

            sut.Context.Sql.ToString().Should().Be( "CREATE INDEX temp.\"bar\" ON \"qux\" (\"a\" ASC, \"b\" DESC)" );
        }

        [Theory]
        [InlineData( false, "INDEX" )]
        [InlineData( true, "UNIQUE INDEX" )]
        public void Visit_ShouldInterpretCreateIndex_WithReplaceIfExists(bool isUnique, string expectedType)
        {
            var sut = CreateInterpreter();
            var qux = SqlNode.RawRecordSet( "qux" );
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
                    $@"DROP INDEX IF EXISTS ""foo_bar"";
CREATE {expectedType} ""foo_bar"" ON qux (""a"" ASC, ""b"" DESC)" );
        }

        [Fact]
        public void Visit_ShouldInterpretCreateIndex_WithFilter()
        {
            var sut = CreateInterpreter();
            var qux = SqlNode.RawRecordSet( "qux" );
            var node = SqlNode.CreateIndex(
                SqlSchemaObjectName.Create( "foo", "bar" ),
                isUnique: false,
                replaceIfExists: false,
                table: qux,
                columns: new[] { qux["a"].Asc(), qux["b"].Desc() },
                filter: qux["a"] != null );

            sut.Visit( node );

            sut.Context.Sql.ToString()
                .Should()
                .Be( "CREATE INDEX \"foo_bar\" ON qux (\"a\" ASC, \"b\" DESC) WHERE (\"a\" IS NOT NULL)" );
        }

        [Theory]
        [InlineData( false, "ALTER TABLE \"foo_bar\" RENAME TO \"qux_lorem\"" )]
        [InlineData( true, "ALTER TABLE temp.\"foo\" RENAME TO \"qux_lorem\"" )]
        public void Visit_ShouldInterpretRenameTable(bool isTemporary, string expected)
        {
            var sut = CreateInterpreter();
            var info = isTemporary ? SqlRecordSetInfo.CreateTemporary( "foo" ) : SqlRecordSetInfo.Create( "foo", "bar" );
            sut.Visit( SqlNode.RenameTable( info, SqlSchemaObjectName.Create( "qux", "lorem" ) ) );
            sut.Context.Sql.ToString().Should().Be( expected );
        }

        [Theory]
        [InlineData( false, "ALTER TABLE \"foo_bar\" RENAME COLUMN \"qux\" TO \"lorem\"" )]
        [InlineData( true, "ALTER TABLE temp.\"foo\" RENAME COLUMN \"qux\" TO \"lorem\"" )]
        public void Visit_ShouldInterpretRenameColumn(bool isTableTemporary, string expected)
        {
            var sut = CreateInterpreter();
            var info = isTableTemporary ? SqlRecordSetInfo.CreateTemporary( "foo" ) : SqlRecordSetInfo.Create( "foo", "bar" );
            sut.Visit( SqlNode.RenameColumn( info, "qux", "lorem" ) );
            sut.Context.Sql.ToString().Should().Be( expected );
        }

        [Theory]
        [InlineData( false, "ALTER TABLE \"foo_bar\" ADD COLUMN \"qux\" INTEGER NOT NULL DEFAULT (10)" )]
        [InlineData( true, "ALTER TABLE temp.\"foo\" ADD COLUMN \"qux\" INTEGER NOT NULL DEFAULT (10)" )]
        public void Visit_ShouldInterpretAddColumn(bool isTableTemporary, string expected)
        {
            var sut = CreateInterpreter();
            var info = isTableTemporary ? SqlRecordSetInfo.CreateTemporary( "foo" ) : SqlRecordSetInfo.Create( "foo", "bar" );
            sut.Visit( SqlNode.AddColumn( info, SqlNode.Column<int>( "qux", defaultValue: SqlNode.Literal( 10 ) ) ) );
            sut.Context.Sql.ToString().Should().Be( expected );
        }

        [Theory]
        [InlineData( false, "ALTER TABLE \"foo_bar\" DROP COLUMN \"qux\"" )]
        [InlineData( true, "ALTER TABLE temp.\"foo\" DROP COLUMN \"qux\"" )]
        public void Visit_ShouldInterpretDropColumn(bool isTableTemporary, string expected)
        {
            var sut = CreateInterpreter();
            var info = isTableTemporary ? SqlRecordSetInfo.CreateTemporary( "foo" ) : SqlRecordSetInfo.Create( "foo", "bar" );
            sut.Visit( SqlNode.DropColumn( info, "qux" ) );
            sut.Context.Sql.ToString().Should().Be( expected );
        }

        [Theory]
        [InlineData( false, false, "DROP TABLE \"foo_bar\"" )]
        [InlineData( true, false, "DROP TABLE temp.\"foo\"" )]
        [InlineData( false, true, "DROP TABLE IF EXISTS \"foo_bar\"" )]
        [InlineData( true, true, "DROP TABLE IF EXISTS temp.\"foo\"" )]
        public void Visit_ShouldInterpretDropTable(bool isTemporary, bool ifExists, string expected)
        {
            var sut = CreateInterpreter();
            var info = isTemporary ? SqlRecordSetInfo.CreateTemporary( "foo" ) : SqlRecordSetInfo.Create( "foo", "bar" );
            sut.Visit( SqlNode.DropTable( info, ifExists ) );
            sut.Context.Sql.ToString().Should().Be( expected );
        }

        [Theory]
        [InlineData( false, false, "DROP VIEW \"foo_bar\"" )]
        [InlineData( true, false, "DROP VIEW temp.\"foo\"" )]
        [InlineData( false, true, "DROP VIEW IF EXISTS \"foo_bar\"" )]
        [InlineData( true, true, "DROP VIEW IF EXISTS temp.\"foo\"" )]
        public void Visit_ShouldInterpretDropView(bool isTemporary, bool ifExists, string expected)
        {
            var sut = CreateInterpreter();
            var info = isTemporary ? SqlRecordSetInfo.CreateTemporary( "foo" ) : SqlRecordSetInfo.Create( "foo", "bar" );
            sut.Visit( SqlNode.DropView( info, ifExists ) );
            sut.Context.Sql.ToString().Should().Be( expected );
        }

        [Theory]
        [InlineData( false, false, "DROP INDEX \"foo_bar\"" )]
        [InlineData( true, false, "DROP INDEX IF EXISTS \"foo_bar\"" )]
        [InlineData( false, true, "DROP INDEX temp.\"bar\"" )]
        [InlineData( true, true, "DROP INDEX IF EXISTS temp.\"bar\"" )]
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
