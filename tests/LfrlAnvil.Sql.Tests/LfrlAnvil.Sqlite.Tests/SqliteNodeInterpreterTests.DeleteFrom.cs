using LfrlAnvil.Functional;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Traits;
using LfrlAnvil.Sql.Expressions.Visitors;
using LfrlAnvil.TestExtensions.FluentAssertions;
using LfrlAnvil.TestExtensions.Sql.Mocks;

namespace LfrlAnvil.Sqlite.Tests;

public partial class SqliteNodeInterpreterTests
{
    public class DeleteFrom : TestsBase
    {
        [Fact]
        public void Visit_ShouldInterpretDeleteFromSingleDataSource_WithoutTraits()
        {
            var sut = CreateInterpreter();
            var dataSource = SqlNode.RawRecordSet( "foo" ).ToDataSource();
            sut.Visit( dataSource.ToDeleteFrom() );
            sut.Context.Sql.ToString().Should().Be( "DELETE FROM foo" );
        }

        [Fact]
        public void Visit_ShouldInterpretDeleteFromSingleDataSource_WithWhere()
        {
            var sut = CreateInterpreter();
            var dataSource = SqlNode.RawRecordSet( "foo" )
                .ToDataSource()
                .AndWhere( s => s["foo"]["a"] < SqlNode.Literal( 10 ) );

            sut.Visit( dataSource.ToDeleteFrom() );

            sut.Context.Sql.ToString()
                .Should()
                .Be(
                    @"DELETE FROM foo
WHERE foo.""a"" < 10" );
        }

        [Fact]
        public void Visit_ShouldInterpretDeleteFromSimpleDataSource_WithWhereAndAlias()
        {
            var sut = CreateInterpreter();
            var dataSource = SqlTableMock.Create<int>( "foo", new[] { "a" } )
                .ToRecordSet( "bar" )
                .ToDataSource()
                .AndWhere( s => s["bar"]["a"] < SqlNode.Literal( 10 ) );

            sut.Visit( dataSource.ToDeleteFrom() );

            sut.Context.Sql.ToString()
                .Should()
                .Be(
                    @"DELETE FROM ""common_foo"" AS ""bar""
WHERE ""bar"".""a"" < 10" );
        }

        [Fact]
        public void Visit_ShouldInterpretDeleteFromSingleDataSource_WithCteAndWhereAndOrderByAndLimit()
        {
            var sut = CreateInterpreter();
            var dataSource = SqlTableMock.Create<int>( "foo", new[] { "a" } )
                .ToRecordSet( "f" )
                .ToDataSource()
                .With( SqlNode.RawQuery( "SELECT * FROM abc" ).ToCte( "cba" ) )
                .AndWhere( s => s["f"]["a"].InQuery( SqlNode.RawQuery( "SELECT cba.c FROM cba" ) ) )
                .OrderBy( s => new[] { s["f"]["a"].Asc() } )
                .Limit( SqlNode.Literal( 5 ) )
                .Offset( SqlNode.Literal( 10 ) );

            sut.Visit( dataSource.ToDeleteFrom() );

            sut.Context.Sql.ToString()
                .Should()
                .Be(
                    @"WITH ""cba"" AS (
  SELECT * FROM abc
)
DELETE FROM ""common_foo"" AS ""f""
WHERE ""f"".""a"" IN (
  SELECT cba.c FROM cba
)
ORDER BY ""f"".""a"" ASC
LIMIT 5 OFFSET 10" );
        }

        [Fact]
        public void Visit_ShouldInterpretDeleteFromSingleDataSource_WithDeleteLimitDisabled_WithCteAndWhereAndOrderByAndLimit()
        {
            var sut = CreateInterpreter( SqliteNodeInterpreterOptions.Default.EnableUpdateOrDeleteLimit( false ) );
            var dataSource = SqlTableMock.Create<int>( "foo", new[] { "a" } )
                .ToRecordSet( "f" )
                .ToDataSource()
                .With( SqlNode.RawQuery( "SELECT * FROM abc" ).ToCte( "cba" ) )
                .AndWhere( s => s["f"]["a"].InQuery( SqlNode.RawQuery( "SELECT cba.c FROM cba" ) ) )
                .OrderBy( s => new[] { s["f"]["a"].Asc() } )
                .Limit( SqlNode.Literal( 5 ) )
                .Offset( SqlNode.Literal( 10 ) );

            sut.Visit( dataSource.ToDeleteFrom() );

            sut.Context.Sql.ToString()
                .Should()
                .Be(
                    @"WITH ""cba"" AS (
  SELECT * FROM abc
)
DELETE FROM ""common_foo""
WHERE ""common_foo"".""a"" IN (
  SELECT
    ""f"".""a""
  FROM ""common_foo"" AS ""f""
  WHERE ""f"".""a"" IN (
    SELECT cba.c FROM cba
  )
  ORDER BY ""f"".""a"" ASC
  LIMIT 5 OFFSET 10
)" );
        }

        [Fact]
        public void Visit_ShouldInterpretDeleteFromSingleDataSource_WithAllTraits()
        {
            var sut = CreateInterpreter();
            var table = SqlTableMock.Create<int>( "foo", new[] { "a", "b" }, new[] { "a" } );
            var foo = table.ToRecordSet( "f" );

            var dataSource = foo
                .ToDataSource()
                .Distinct()
                .With( SqlNode.RawQuery( "SELECT * FROM abc" ).ToCte( "cba" ) )
                .AndWhere( s => s["f"]["a"].InQuery( SqlNode.RawQuery( "SELECT cba.c FROM cba" ) ) )
                .GroupBy( s => new[] { s["f"]["b"] } )
                .AndHaving( s => s["f"]["b"] > SqlNode.Literal( 20 ) )
                .Window( SqlNode.WindowDefinition( "wnd", Array.Empty<SqlOrderByNode>() ) )
                .OrderBy( s => new[] { s["f"]["a"].Asc() } )
                .Limit( SqlNode.Literal( 5 ) )
                .Offset( SqlNode.Literal( 10 ) );

            sut.Visit( dataSource.ToDeleteFrom() );

            sut.Context.Sql.ToString()
                .Should()
                .Be(
                    @"WITH ""cba"" AS (
  SELECT * FROM abc
)
DELETE FROM ""common_foo""
WHERE ""common_foo"".""a"" IN (
  SELECT DISTINCT
    ""f"".""a""
  FROM ""common_foo"" AS ""f""
  WHERE ""f"".""a"" IN (
    SELECT cba.c FROM cba
  )
  GROUP BY ""f"".""b""
  HAVING ""f"".""b"" > 20
  WINDOW ""wnd"" AS ()
  ORDER BY ""f"".""a"" ASC
  LIMIT 5 OFFSET 10
)" );
        }

        [Fact]
        public void Visit_ShouldInterpretDeleteFromMultiDataSource_WithoutTraits()
        {
            var sut = CreateInterpreter();
            var foo = SqlTableMock.Create<int>( "foo", new[] { "a" } ).ToRecordSet( "f" );
            var other = SqlNode.RawRecordSet( "bar" );
            var dataSource = foo.Join( other.InnerOn( foo["a"] == other["a"] ) );

            sut.Visit( dataSource.ToDeleteFrom() );

            sut.Context.Sql.ToString()
                .Should()
                .Be(
                    @"DELETE FROM ""common_foo""
WHERE ""common_foo"".""a"" IN (
  SELECT
    ""f"".""a""
  FROM ""common_foo"" AS ""f""
  INNER JOIN bar ON ""f"".""a"" = bar.""a""
)" );
        }

        [Fact]
        public void Visit_ShouldInterpretDeleteFromMultiDataSource_WithCteAndWhere()
        {
            var sut = CreateInterpreter();
            var foo = SqlTableMock.Create<int>( "foo", new[] { "a" } ).ToRecordSet( "f" );
            var other = SqlNode.RawRecordSet( "bar", "b" );

            var dataSource = foo
                .Join( other.InnerOn( foo["a"] == other["a"] ) )
                .With( SqlNode.RawQuery( "SELECT * FROM abc" ).ToCte( "cba" ) )
                .AndWhere( s => s["f"]["a"] < SqlNode.Literal( 10 ) );

            sut.Visit( dataSource.ToDeleteFrom() );

            sut.Context.Sql.ToString()
                .Should()
                .Be(
                    @"WITH ""cba"" AS (
  SELECT * FROM abc
)
DELETE FROM ""common_foo""
WHERE ""common_foo"".""a"" IN (
  SELECT
    ""f"".""a""
  FROM ""common_foo"" AS ""f""
  INNER JOIN bar AS ""b"" ON ""f"".""a"" = ""b"".""a""
  WHERE ""f"".""a"" < 10
)" );
        }

        [Fact]
        public void Visit_ShouldInterpretDeleteFromMultiDataSource_WithCteAndWhereAndOrderByAndLimit()
        {
            var sut = CreateInterpreter();
            var table = SqlTableMock.Create<int>( "foo", new[] { "a", "b" }, new[] { "a" } );
            var foo = table.ToRecordSet( "f" );
            var other = SqlNode.RawRecordSet( "bar" );

            var dataSource = foo
                .Join( other.InnerOn( foo["a"] == other["a"] ) )
                .With( SqlNode.RawQuery( "SELECT * FROM abc" ).ToCte( "cba" ) )
                .AndWhere( s => s["f"]["a"].InQuery( SqlNode.RawQuery( "SELECT cba.c FROM cba" ) ) )
                .OrderBy( s => new[] { s["f"]["a"].Asc() } )
                .Limit( SqlNode.Literal( 5 ) );

            sut.Visit( dataSource.ToDeleteFrom() );

            sut.Context.Sql.ToString()
                .Should()
                .Be(
                    @"WITH ""cba"" AS (
  SELECT * FROM abc
)
DELETE FROM ""common_foo""
WHERE ""common_foo"".""a"" IN (
  SELECT
    ""f"".""a""
  FROM ""common_foo"" AS ""f""
  INNER JOIN bar ON ""f"".""a"" = bar.""a""
  WHERE ""f"".""a"" IN (
    SELECT cba.c FROM cba
  )
  ORDER BY ""f"".""a"" ASC
  LIMIT 5
)" );
        }

        [Fact]
        public void Visit_ShouldInterpretDeleteFromMultiDataSource_WithAllTraits()
        {
            var sut = CreateInterpreter();
            var table = SqlTableMock.Create<int>( "foo", new[] { "a", "b" }, new[] { "a" } );
            var foo = table.ToRecordSet( "f" );
            var other = SqlNode.RawRecordSet( "bar" );

            var dataSource = foo
                .Join( other.InnerOn( foo["a"] == other["a"] ) )
                .Distinct()
                .With( SqlNode.RawQuery( "SELECT * FROM abc" ).ToCte( "cba" ) )
                .AndWhere( s => s["f"]["a"].InQuery( SqlNode.RawQuery( "SELECT cba.c FROM cba" ) ) )
                .GroupBy( s => new[] { s["f"]["b"] } )
                .AndHaving( s => s["f"]["b"] > SqlNode.Literal( 20 ) )
                .Window( SqlNode.WindowDefinition( "wnd", Array.Empty<SqlOrderByNode>() ) )
                .OrderBy( s => new[] { s["f"]["a"].Asc() } )
                .Limit( SqlNode.Literal( 5 ) )
                .Offset( SqlNode.Literal( 10 ) );

            sut.Visit( dataSource.ToDeleteFrom() );

            sut.Context.Sql.ToString()
                .Should()
                .Be(
                    @"WITH ""cba"" AS (
  SELECT * FROM abc
)
DELETE FROM ""common_foo""
WHERE ""common_foo"".""a"" IN (
  SELECT DISTINCT
    ""f"".""a""
  FROM ""common_foo"" AS ""f""
  INNER JOIN bar ON ""f"".""a"" = bar.""a""
  WHERE ""f"".""a"" IN (
    SELECT cba.c FROM cba
  )
  GROUP BY ""f"".""b""
  HAVING ""f"".""b"" > 20
  WINDOW ""wnd"" AS ()
  ORDER BY ""f"".""a"" ASC
  LIMIT 5 OFFSET 10
)" );
        }

        [Fact]
        public void Visit_ShouldInterpretDeleteFromComplexDataSource_WhenTargetIsTableWithSingleColumnPrimaryKey()
        {
            var sut = CreateInterpreter();
            var table = SqlTableMock.Create<int>( "foo", new[] { "a", "b" }, new[] { "a" } );
            var foo = table.ToRecordSet( "f" );
            var other = SqlNode.RawRecordSet( "bar" );

            var dataSource = foo
                .Join( other.InnerOn( foo["a"] == other["a"] ) )
                .GroupBy( s => new[] { s["f"]["b"] } );

            sut.Visit( dataSource.ToDeleteFrom() );

            sut.Context.Sql.ToString()
                .Should()
                .Be(
                    @"DELETE FROM ""common_foo""
WHERE ""common_foo"".""a"" IN (
  SELECT
    ""f"".""a""
  FROM ""common_foo"" AS ""f""
  INNER JOIN bar ON ""f"".""a"" = bar.""a""
  GROUP BY ""f"".""b""
)" );
        }

        [Fact]
        public void Visit_ShouldInterpretDeleteFromComplexDataSource_WhenTargetIsTableWithMultiColumnPrimaryKey()
        {
            var sut = CreateInterpreter();
            var table = SqlTableMock.Create<int>( "foo", new[] { "a", "b" } );
            var foo = table.ToRecordSet( "f" );
            var other = SqlNode.RawRecordSet( "bar" );

            var dataSource = foo
                .Join( other.InnerOn( foo["a"] == other["a"] ) )
                .GroupBy( s => new[] { s["f"]["b"] } );

            sut.Visit( dataSource.ToDeleteFrom() );

            sut.Context.Sql.ToString()
                .Should()
                .Be(
                    @"DELETE FROM ""common_foo""
WHERE EXISTS (
  SELECT
    *
  FROM ""common_foo"" AS ""f""
  INNER JOIN bar ON ""f"".""a"" = bar.""a""
  WHERE (""common_foo"".""a"" = ""f"".""a"") AND (""common_foo"".""b"" = ""f"".""b"")
  GROUP BY ""f"".""b""
)" );
        }

        [Fact]
        public void Visit_ShouldInterpretDeleteFromComplexDataSource_WhenTargetIsTableBuilderWithSingleColumnPrimaryKey()
        {
            var sut = CreateInterpreter();
            var table = SqlTableBuilderMock.Create<int>( "foo", new[] { "a", "b" }, new[] { "a" } );
            var foo = table.ToRecordSet( "f" );
            var other = SqlNode.RawRecordSet( "bar" );

            var dataSource = foo
                .Join( other.InnerOn( foo["a"] == other["a"] ) )
                .GroupBy( s => new[] { s["f"]["b"] } );

            sut.Visit( dataSource.ToDeleteFrom() );

            sut.Context.Sql.ToString()
                .Should()
                .Be(
                    @"DELETE FROM ""common_foo""
WHERE ""common_foo"".""a"" IN (
  SELECT
    ""f"".""a""
  FROM ""common_foo"" AS ""f""
  INNER JOIN bar ON ""f"".""a"" = bar.""a""
  GROUP BY ""f"".""b""
)" );
        }

        [Fact]
        public void Visit_ShouldInterpretDeleteFromComplexDataSource_WhenTargetIsTableBuilderWithMultiColumnPrimaryKey()
        {
            var sut = CreateInterpreter();
            var table = SqlTableBuilderMock.Create<int>( "foo", new[] { "a", "b" } );
            var foo = table.ToRecordSet( "f" );
            var other = SqlNode.RawRecordSet( "bar" );

            var dataSource = foo
                .Join( other.InnerOn( foo["a"] == other["a"] ) )
                .GroupBy( s => new[] { s["f"]["b"] } );

            sut.Visit( dataSource.ToDeleteFrom() );

            sut.Context.Sql.ToString()
                .Should()
                .Be(
                    @"DELETE FROM ""common_foo""
WHERE EXISTS (
  SELECT
    *
  FROM ""common_foo"" AS ""f""
  INNER JOIN bar ON ""f"".""a"" = bar.""a""
  WHERE (""common_foo"".""a"" = ""f"".""a"") AND (""common_foo"".""b"" = ""f"".""b"")
  GROUP BY ""f"".""b""
)" );
        }

        [Fact]
        public void Visit_ShouldInterpretDeleteFromComplexDataSource_WhenTargetIsTableBuilderWithoutPrimaryKey()
        {
            var sut = CreateInterpreter();
            var table = SqlTableBuilderMock.CreateEmpty( "foo" );
            table.Columns.Create( "a" );
            table.Columns.Create( "b" );
            var foo = table.ToRecordSet( "f" );
            var other = SqlNode.RawRecordSet( "bar" );

            var dataSource = foo
                .Join( other.InnerOn( foo["a"] == other["a"] ) )
                .AndWhere( s => s["f"]["a"] > SqlNode.Literal( 10 ) )
                .GroupBy( s => new[] { s["f"]["b"] } );

            sut.Visit( dataSource.ToDeleteFrom() );

            sut.Context.Sql.ToString()
                .Should()
                .Be(
                    @"DELETE FROM ""common_foo""
WHERE EXISTS (
  SELECT
    *
  FROM ""common_foo"" AS ""f""
  INNER JOIN bar ON ""f"".""a"" = bar.""a""
  WHERE (""f"".""a"" > 10) AND ((""common_foo"".""a"" = ""f"".""a"") AND (""common_foo"".""b"" = ""f"".""b""))
  GROUP BY ""f"".""b""
)" );
        }

        [Theory]
        [InlineData( false, "\"foo_bar\"" )]
        [InlineData( true, "temp.\"foo\"" )]
        public void Visit_ShouldInterpretDeleteFromComplexDataSource_WhenTargetIsNewTableWithSingleColumnPrimaryKey(
            bool isTemporary,
            string expectedName)
        {
            var sut = CreateInterpreter();
            var info = isTemporary ? SqlRecordSetInfo.CreateTemporary( "foo" ) : SqlRecordSetInfo.Create( "foo", "bar" );
            var table = SqlNode.CreateTable(
                info,
                new[] { SqlNode.Column<int>( "a" ), SqlNode.Column<int>( "b" ) },
                constraintsProvider: t =>
                    SqlCreateTableConstraints.Empty.WithPrimaryKey(
                        SqlNode.PrimaryKey( SqlSchemaObjectName.Create( "PK" ), new[] { t["a"].Asc() } ) ) );

            var foo = table.RecordSet.As( "f" );
            var other = SqlNode.RawRecordSet( "bar" );

            var dataSource = foo
                .Join( other.InnerOn( foo["a"] == other["a"] ) )
                .GroupBy( s => new[] { s["f"]["b"] } );

            sut.Visit( dataSource.ToDeleteFrom() );

            sut.Context.Sql.ToString()
                .Should()
                .Be(
                    $@"DELETE FROM {expectedName}
WHERE {expectedName}.""a"" IN (
  SELECT
    ""f"".""a""
  FROM {expectedName} AS ""f""
  INNER JOIN bar ON ""f"".""a"" = bar.""a""
  GROUP BY ""f"".""b""
)" );
        }

        [Theory]
        [InlineData( false, "\"foo_bar\"" )]
        [InlineData( true, "temp.\"foo\"" )]
        public void Visit_ShouldInterpretDeleteFromComplexDataSource_WhenTargetIsNewTableWithMultiColumnPrimaryKey(
            bool isTemporary,
            string expectedName)
        {
            var sut = CreateInterpreter();
            var info = isTemporary ? SqlRecordSetInfo.CreateTemporary( "foo" ) : SqlRecordSetInfo.Create( "foo", "bar" );
            var table = SqlNode.CreateTable(
                info,
                new[] { SqlNode.Column<int>( "a" ), SqlNode.Column<int>( "b" ) },
                constraintsProvider: t =>
                    SqlCreateTableConstraints.Empty.WithPrimaryKey(
                        SqlNode.PrimaryKey( SqlSchemaObjectName.Create( "PK" ), new[] { t["a"].Asc(), t["b"].Asc() } ) ) );

            var foo = table.RecordSet.As( "f" );
            var other = SqlNode.RawRecordSet( "bar" );

            var dataSource = foo
                .Join( other.InnerOn( foo["a"] == other["a"] ) )
                .GroupBy( s => new[] { s["f"]["b"] } );

            sut.Visit( dataSource.ToDeleteFrom() );

            sut.Context.Sql.ToString()
                .Should()
                .Be(
                    $@"DELETE FROM {expectedName}
WHERE EXISTS (
  SELECT
    *
  FROM {expectedName} AS ""f""
  INNER JOIN bar ON ""f"".""a"" = bar.""a""
  WHERE ({expectedName}.""a"" = ""f"".""a"") AND ({expectedName}.""b"" = ""f"".""b"")
  GROUP BY ""f"".""b""
)" );
        }

        [Theory]
        [InlineData( false, "\"foo_bar\"" )]
        [InlineData( true, "temp.\"foo\"" )]
        public void Visit_ShouldInterpretDeleteFromComplexDataSource_WhenTargetIsNewTableWithoutPrimaryKey(
            bool isTemporary,
            string expectedName)
        {
            var sut = CreateInterpreter();
            var info = isTemporary ? SqlRecordSetInfo.CreateTemporary( "foo" ) : SqlRecordSetInfo.Create( "foo", "bar" );
            var table = SqlNode.CreateTable( info, new[] { SqlNode.Column<int>( "a" ), SqlNode.Column<int>( "b" ) } );
            var foo = table.RecordSet.As( "f" );
            var other = SqlNode.RawRecordSet( "bar" );

            var dataSource = foo
                .Join( other.InnerOn( foo["a"] == other["a"] ) )
                .GroupBy( s => new[] { s["f"]["b"] } );

            sut.Visit( dataSource.ToDeleteFrom() );

            sut.Context.Sql.ToString()
                .Should()
                .Be(
                    $@"DELETE FROM {expectedName}
WHERE EXISTS (
  SELECT
    *
  FROM {expectedName} AS ""f""
  INNER JOIN bar ON ""f"".""a"" = bar.""a""
  WHERE ({expectedName}.""a"" = ""f"".""a"") AND ({expectedName}.""b"" = ""f"".""b"")
  GROUP BY ""f"".""b""
)" );
        }

        [Fact]
        public void Visit_ShouldThrowSqlNodeVisitorException_WhenDeleteFromIsComplexAndDataSourceFromIsNotTable()
        {
            var sut = CreateInterpreter();
            var foo = SqlNode.RawRecordSet( "foo" );
            var node = foo.ToDataSource().GroupBy( foo.GetUnsafeField( "a" ) ).ToDeleteFrom();
            var action = Lambda.Of( () => sut.Visit( node ) );

            action.Should()
                .ThrowExactly<SqlNodeVisitorException>()
                .AndMatch( e => ReferenceEquals( e.Node, node ) && ReferenceEquals( e.Visitor, sut ) );
        }

        [Fact]
        public void Visit_ShouldThrowSqlNodeVisitorException_WhenDeleteFromIsComplexAndDataSourceFromIsTableWithoutAlias()
        {
            var sut = CreateInterpreter();
            var foo = SqlTableMock.Create<int>( "foo", new[] { "a" } ).ToRecordSet();
            var node = foo.ToDataSource().GroupBy( foo.GetUnsafeField( "a" ) ).ToDeleteFrom();
            var action = Lambda.Of( () => sut.Visit( node ) );

            action.Should()
                .ThrowExactly<SqlNodeVisitorException>()
                .AndMatch( e => ReferenceEquals( e.Node, node ) && ReferenceEquals( e.Visitor, sut ) );
        }

        [Fact]
        public void Visit_ShouldThrowSqlNodeVisitorException_WhenDeleteFromIsComplexAndDataSourceFromIsTableBuilderWithoutColumns()
        {
            var sut = CreateInterpreter();
            var foo = SqlTableBuilderMock.CreateEmpty( "foo" ).ToRecordSet( "f" );
            var node = foo.ToDataSource().GroupBy( foo.GetUnsafeField( "a" ) ).ToDeleteFrom();
            var action = Lambda.Of( () => sut.Visit( node ) );

            action.Should()
                .ThrowExactly<SqlNodeVisitorException>()
                .AndMatch( e => ReferenceEquals( e.Node, node ) && ReferenceEquals( e.Visitor, sut ) );
        }

        [Fact]
        public void
            Visit_ShouldThrowSqlNodeVisitorException_WhenDeleteFromIsComplexAndDataSourceFromIsNewTableWithoutPrimaryKeyAndColumns()
        {
            var sut = CreateInterpreter();
            var foo = SqlNode.CreateTable( SqlRecordSetInfo.Create( "foo" ), Array.Empty<SqlColumnDefinitionNode>() ).AsSet( "f" );
            var node = foo.ToDataSource().GroupBy( foo.GetUnsafeField( "a" ) ).ToDeleteFrom();
            var action = Lambda.Of( () => sut.Visit( node ) );

            action.Should()
                .ThrowExactly<SqlNodeVisitorException>()
                .AndMatch( e => ReferenceEquals( e.Node, node ) && ReferenceEquals( e.Visitor, sut ) );
        }

        [Fact]
        public void
            Visit_ShouldThrowSqlNodeVisitorException_WhenDeleteFromIsComplexAndDataSourceFromIsNewTableWithPrimaryKeyWithoutColumns()
        {
            var sut = CreateInterpreter();
            var foo = SqlNode.CreateTable(
                    SqlRecordSetInfo.Create( "foo" ),
                    new[] { SqlNode.Column<int>( "a" ) },
                    constraintsProvider: _ =>
                        SqlCreateTableConstraints.Empty.WithPrimaryKey(
                            SqlNode.PrimaryKey( SqlSchemaObjectName.Create( "PK" ), ReadOnlyArray<SqlOrderByNode>.Empty ) ) )
                .AsSet( "f" );

            var node = foo.ToDataSource().GroupBy( foo.GetUnsafeField( "a" ) ).ToDeleteFrom();
            var action = Lambda.Of( () => sut.Visit( node ) );

            action.Should()
                .ThrowExactly<SqlNodeVisitorException>()
                .AndMatch( e => ReferenceEquals( e.Node, node ) && ReferenceEquals( e.Visitor, sut ) );
        }

        [Fact]
        public void
            Visit_ShouldThrowSqlNodeVisitorException_WhenDeleteFromIsComplexAndDataSourceFromIsNewTableWithPrimaryKeyContainingNonDataFieldColumn()
        {
            var sut = CreateInterpreter();
            var foo = SqlNode.CreateTable(
                    SqlRecordSetInfo.Create( "foo" ),
                    new[] { SqlNode.Column<int>( "a" ) },
                    constraintsProvider: t =>
                        SqlCreateTableConstraints.Empty.WithPrimaryKey(
                            SqlNode.PrimaryKey(
                                SqlSchemaObjectName.Create( "PK" ),
                                new[] { (t["a"] + SqlNode.Literal( 1 )).Asc() } ) ) )
                .AsSet( "f" );

            var node = foo.ToDataSource().GroupBy( foo.GetUnsafeField( "a" ) ).ToDeleteFrom();
            var action = Lambda.Of( () => sut.Visit( node ) );

            action.Should()
                .ThrowExactly<SqlNodeVisitorException>()
                .AndMatch( e => ReferenceEquals( e.Node, node ) && ReferenceEquals( e.Visitor, sut ) );
        }
    }
}
