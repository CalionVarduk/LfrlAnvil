using LfrlAnvil.Functional;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.Sql.Expressions.Traits;
using LfrlAnvil.Sql.Expressions.Visitors;
using LfrlAnvil.TestExtensions.FluentAssertions;
using LfrlAnvil.TestExtensions.Sql.FluentAssertions;
using LfrlAnvil.TestExtensions.Sql.Mocks;

namespace LfrlAnvil.Sqlite.Tests;

public partial class SqliteNodeInterpreterTests
{
    public class Update : TestsBase
    {
        [Fact]
        public void Visit_ShouldInterpretUpdateSingleDataSource_WithoutTraits()
        {
            var sut = CreateInterpreter();
            var dataSource = SqlNode.RawRecordSet( "foo" ).ToDataSource();

            sut.Visit( dataSource.ToUpdate( s => new[] { s["foo"]["a"].Assign( SqlNode.Literal( "bar" ) ) } ) );

            sut.Context.Sql.ToString()
                .Should()
                .Be(
                    @"UPDATE foo SET
  ""a"" = 'bar'" );
        }

        [Fact]
        public void Visit_ShouldInterpretUpdateSingleDataSource_WithWhere()
        {
            var sut = CreateInterpreter();
            var dataSource = SqlNode.RawRecordSet( "foo" )
                .ToDataSource()
                .AndWhere( s => s["foo"]["a"] < SqlNode.Literal( 10 ) );

            sut.Visit( dataSource.ToUpdate( s => new[] { s["foo"]["a"].Assign( SqlNode.Literal( "bar" ) ) } ) );

            sut.Context.Sql.ToString()
                .Should()
                .Be(
                    @"UPDATE foo SET
  ""a"" = 'bar'
WHERE foo.""a"" < 10" );
        }

        [Fact]
        public void Visit_ShouldInterpretUpdateSimpleDataSource_WithWhereAndAlias()
        {
            var sut = CreateInterpreter();
            var dataSource = SqlTableMock.Create<int>( "foo", new[] { "a" } )
                .ToRecordSet( "bar" )
                .ToDataSource()
                .AndWhere( s => s["bar"]["a"] < SqlNode.Literal( 10 ) );

            sut.Visit( dataSource.ToUpdate( s => new[] { s["bar"]["a"].Assign( SqlNode.Literal( 10 ) ) } ) );

            sut.Context.Sql.ToString()
                .Should()
                .Be(
                    @"UPDATE ""common_foo"" AS ""bar"" SET
  ""a"" = 10
WHERE ""bar"".""a"" < 10" );
        }

        [Fact]
        public void Visit_ShouldInterpretUpdateSingleDataSource_WithCteAndWhereAndOrderByAndLimit()
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

            sut.Visit( dataSource.ToUpdate( s => new[] { s["f"]["a"].Assign( SqlNode.Literal( 10 ) ) } ) );

            sut.Context.Sql.ToString()
                .Should()
                .Be(
                    @"WITH ""cba"" AS (
  SELECT * FROM abc
)
UPDATE ""common_foo"" AS ""f"" SET
  ""a"" = 10
WHERE ""f"".""a"" IN (
  SELECT cba.c FROM cba
)
ORDER BY ""f"".""a"" ASC
LIMIT 5 OFFSET 10" );
        }

        [Fact]
        public void Visit_ShouldInterpretUpdateSingleDataSource_WithUpdateLimitDisabled_WithCteAndWhereAndOrderByAndLimit()
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

            sut.Visit( dataSource.ToUpdate( s => new[] { s["f"]["a"].Assign( SqlNode.Literal( 10 ) ) } ) );

            sut.Context.Sql.ToString()
                .Should()
                .Be(
                    @"WITH ""cba"" AS (
  SELECT * FROM abc
)
UPDATE ""common_foo"" SET
  ""a"" = 10
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
        public void Visit_ShouldInterpretUpdateSingleDataSource_WithAllTraits()
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

            sut.Visit( dataSource.ToUpdate( s => new[] { s["f"]["a"].Assign( SqlNode.Literal( 10 ) ) } ) );

            sut.Context.Sql.ToString()
                .Should()
                .Be(
                    @"WITH ""cba"" AS (
  SELECT * FROM abc
)
UPDATE ""common_foo"" SET
  ""a"" = 10
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
        public void Visit_ShouldInterpretUpdateSimpleDataSource_WithSubQueryInAssignment()
        {
            var sut = CreateInterpreter();
            var dataSource = SqlNode.RawRecordSet( "foo" ).ToDataSource();

            sut.Visit(
                dataSource.ToUpdate(
                    s => new[]
                    {
                        s["foo"]["b"]
                            .Assign(
                                SqlNode.RawRecordSet( "bar" )
                                    .ToDataSource()
                                    .AndWhere( b => b.From["x"] == s["foo"]["a"] )
                                    .Select( b => new[] { b.From["y"].AsSelf() } ) )
                    } ) );

            sut.Context.Sql.ToString()
                .Should()
                .Be(
                    @"UPDATE foo SET
  ""b"" = (
    SELECT
      bar.""y""
    FROM bar
    WHERE bar.""x"" = foo.""a""
  )" );
        }

        [Fact]
        public void Visit_ShouldInterpretUpdateSimpleDataSource_WithDataSourceFieldsAsAssignedValues()
        {
            var sut = CreateInterpreter();
            var dataSource = SqlNode.RawRecordSet( "foo" ).ToDataSource();

            sut.Visit(
                dataSource.ToUpdate(
                    s => new[]
                    {
                        s["foo"]["a"].Assign( s["foo"]["a"] + SqlNode.Literal( 1 ) ),
                        s["foo"]["b"].Assign( s["foo"]["c"] * s["foo"]["d"] )
                    } ) );

            sut.Context.Sql.ToString()
                .Should()
                .Be(
                    @"UPDATE foo SET
  ""a"" = (foo.""a"" + 1),
  ""b"" = (foo.""c"" * foo.""d"")" );
        }

        [Fact]
        public void Visit_ShouldInterpretUpdateMultiDataSource_WithoutTraits()
        {
            var sut = CreateInterpreter();
            var foo = SqlTableMock.Create<int>( "foo", new[] { "a" } ).ToRecordSet( "f" );
            var other = SqlNode.RawRecordSet( "bar" );
            var dataSource = foo.Join( other.InnerOn( foo["a"] == other["a"] ) );

            sut.Visit( dataSource.ToUpdate( s => new[] { s["f"]["a"].Assign( SqlNode.Literal( 10 ) ) } ) );

            sut.Context.Sql.ToString()
                .Should()
                .Be(
                    @"UPDATE ""common_foo"" AS ""f"" SET
  ""a"" = 10
FROM bar
WHERE ""f"".""a"" = bar.""a""" );
        }

        [Fact]
        public void Visit_ShouldInterpretUpdateMultiDataSource_WithUpdateFromDisabled_WithoutTraits()
        {
            var sut = CreateInterpreter( SqliteNodeInterpreterOptions.Default.EnableUpdateFrom( false ) );
            var foo = SqlTableMock.Create<int>( "foo", new[] { "a" } ).ToRecordSet( "f" );
            var other = SqlNode.RawRecordSet( "bar" );
            var dataSource = foo.Join( other.InnerOn( foo["a"] == other["a"] ) );

            sut.Visit( dataSource.ToUpdate( s => new[] { s["f"]["a"].Assign( SqlNode.Literal( 10 ) ) } ) );

            sut.Context.Sql.ToString()
                .Should()
                .Be(
                    @"UPDATE ""common_foo"" SET
  ""a"" = 10
WHERE ""common_foo"".""a"" IN (
  SELECT
    ""f"".""a""
  FROM ""common_foo"" AS ""f""
  INNER JOIN bar ON ""f"".""a"" = bar.""a""
)" );
        }

        [Fact]
        public void Visit_ShouldInterpretUpdateMultiDataSource_WithMoreThanTwoRecordSets()
        {
            var sut = CreateInterpreter();
            var foo = SqlTableMock.Create<int>( "foo", new[] { "a" } ).ToRecordSet( "f" );
            var bar = SqlNode.RawRecordSet( "bar" );
            var qux = SqlNode.RawRecordSet( "qux" );
            var dataSource = foo.Join( bar.InnerOn( foo["a"] == bar["a"] ), qux.InnerOn( bar["b"] == qux["b"] ) );

            sut.Visit( dataSource.ToUpdate( s => new[] { s["f"]["a"].Assign( SqlNode.Literal( 10 ) ) } ) );

            sut.Context.Sql.ToString()
                .Should()
                .Be(
                    @"UPDATE ""common_foo"" SET
  ""a"" = 10
WHERE ""common_foo"".""a"" IN (
  SELECT
    ""f"".""a""
  FROM ""common_foo"" AS ""f""
  INNER JOIN bar ON ""f"".""a"" = bar.""a""
  INNER JOIN qux ON bar.""b"" = qux.""b""
)" );
        }

        [Fact]
        public void Visit_ShouldInterpretUpdateMultiDataSource_WithCrossJoin()
        {
            var sut = CreateInterpreter();
            var foo = SqlTableMock.Create<int>( "foo", new[] { "a" } ).ToRecordSet( "f" );
            var other = SqlNode.RawRecordSet( "bar" );
            var dataSource = foo.Join( other.Cross() );

            sut.Visit( dataSource.ToUpdate( s => new[] { s["f"]["a"].Assign( SqlNode.Literal( 10 ) ) } ) );

            sut.Context.Sql.ToString()
                .Should()
                .Be(
                    @"UPDATE ""common_foo"" AS ""f"" SET
  ""a"" = 10
FROM bar" );
        }

        [Fact]
        public void Visit_ShouldInterpretUpdateMultiDataSource_WithLeftJoin()
        {
            var sut = CreateInterpreter();
            var foo = SqlTableMock.Create<int>( "foo", new[] { "a" } ).ToRecordSet( "f" );
            var other = SqlNode.RawRecordSet( "bar" );
            var dataSource = foo.Join( other.LeftOn( foo["a"] == other["a"] ) );

            sut.Visit( dataSource.ToUpdate( s => new[] { s["f"]["a"].Assign( SqlNode.Literal( 10 ) ) } ) );

            sut.Context.Sql.ToString()
                .Should()
                .Be(
                    @"UPDATE ""common_foo"" SET
  ""a"" = 10
WHERE ""common_foo"".""a"" IN (
  SELECT
    ""f"".""a""
  FROM ""common_foo"" AS ""f""
  LEFT JOIN bar ON ""f"".""a"" = bar.""a""
)" );
        }

        [Fact]
        public void Visit_ShouldInterpretUpdateMultiDataSource_WithRightJoin()
        {
            var sut = CreateInterpreter();
            var foo = SqlTableMock.Create<int>( "foo", new[] { "a" } ).ToRecordSet( "f" );
            var other = SqlNode.RawRecordSet( "bar" );
            var dataSource = foo.Join( other.RightOn( foo["a"] == other["a"] ) );

            sut.Visit( dataSource.ToUpdate( s => new[] { s["f"]["a"].Assign( SqlNode.Literal( 10 ) ) } ) );

            sut.Context.Sql.ToString()
                .Should()
                .Be(
                    @"UPDATE ""common_foo"" SET
  ""a"" = 10
WHERE ""common_foo"".""a"" IN (
  SELECT
    ""f"".""a""
  FROM ""common_foo"" AS ""f""
  RIGHT JOIN bar ON ""f"".""a"" = bar.""a""
)" );
        }

        [Fact]
        public void Visit_ShouldInterpretUpdateMultiDataSource_WithFullJoin()
        {
            var sut = CreateInterpreter();
            var foo = SqlTableMock.Create<int>( "foo", new[] { "a" } ).ToRecordSet( "f" );
            var other = SqlNode.RawRecordSet( "bar" );
            var dataSource = foo.Join( other.FullOn( foo["a"] == other["a"] ) );

            sut.Visit( dataSource.ToUpdate( s => new[] { s["f"]["a"].Assign( SqlNode.Literal( 10 ) ) } ) );

            sut.Context.Sql.ToString()
                .Should()
                .Be(
                    @"UPDATE ""common_foo"" SET
  ""a"" = 10
WHERE ""common_foo"".""a"" IN (
  SELECT
    ""f"".""a""
  FROM ""common_foo"" AS ""f""
  FULL JOIN bar ON ""f"".""a"" = bar.""a""
)" );
        }

        [Fact]
        public void Visit_ShouldInterpretUpdateMultiDataSource_WithCteAndWhere()
        {
            var sut = CreateInterpreter();
            var foo = SqlTableMock.Create<int>( "foo", new[] { "a" } ).ToRecordSet( "f" );
            var other = SqlNode.RawRecordSet( "bar", "b" );

            var dataSource = foo
                .Join( other.InnerOn( foo["a"] == other["a"] ) )
                .With( SqlNode.RawQuery( "SELECT * FROM abc" ).ToCte( "cba" ) )
                .AndWhere( s => s["f"]["a"] < SqlNode.Literal( 10 ) );

            sut.Visit( dataSource.ToUpdate( s => new[] { s["f"]["a"].Assign( SqlNode.Literal( 10 ) ) } ) );

            sut.Context.Sql.ToString()
                .Should()
                .Be(
                    @"WITH ""cba"" AS (
  SELECT * FROM abc
)
UPDATE ""common_foo"" AS ""f"" SET
  ""a"" = 10
FROM bar AS ""b""
WHERE (""f"".""a"" = ""b"".""a"") AND (""f"".""a"" < 10)" );
        }

        [Fact]
        public void Visit_ShouldInterpretUpdateMultiDataSource_WithUpdateFromDisabled_WithCteAndWhere()
        {
            var sut = CreateInterpreter( SqliteNodeInterpreterOptions.Default.EnableUpdateFrom( false ) );
            var foo = SqlTableMock.Create<int>( "foo", new[] { "a" } ).ToRecordSet( "f" );
            var other = SqlNode.RawRecordSet( "bar", "b" );

            var dataSource = foo
                .Join( other.InnerOn( foo["a"] == other["a"] ) )
                .With( SqlNode.RawQuery( "SELECT * FROM abc" ).ToCte( "cba" ) )
                .AndWhere( s => s["f"]["a"] < SqlNode.Literal( 10 ) );

            sut.Visit( dataSource.ToUpdate( s => new[] { s["f"]["a"].Assign( SqlNode.Literal( 10 ) ) } ) );

            sut.Context.Sql.ToString()
                .Should()
                .Be(
                    @"WITH ""cba"" AS (
  SELECT * FROM abc
)
UPDATE ""common_foo"" SET
  ""a"" = 10
WHERE ""common_foo"".""a"" IN (
  SELECT
    ""f"".""a""
  FROM ""common_foo"" AS ""f""
  INNER JOIN bar AS ""b"" ON ""f"".""a"" = ""b"".""a""
  WHERE ""f"".""a"" < 10
)" );
        }

        [Fact]
        public void Visit_ShouldInterpretUpdateMultiDataSource_WithCteAndWhereAndOrderByAndLimit()
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
                .Limit( SqlNode.Literal( 5 ) )
                .Offset( SqlNode.Literal( 10 ) );

            sut.Visit( dataSource.ToUpdate( s => new[] { s["f"]["a"].Assign( SqlNode.Literal( 10 ) ) } ) );

            sut.Context.Sql.ToString()
                .Should()
                .Be(
                    @"WITH ""cba"" AS (
  SELECT * FROM abc
)
UPDATE ""common_foo"" AS ""f"" SET
  ""a"" = 10
FROM bar
WHERE (""f"".""a"" = bar.""a"") AND (""f"".""a"" IN (
    SELECT cba.c FROM cba
  ))
ORDER BY ""f"".""a"" ASC
LIMIT 5 OFFSET 10" );
        }

        [Fact]
        public void Visit_ShouldInterpretUpdateMultiDataSource_WithUpdateLimitDisabled_WithCteAndWhereAndOrderByAndLimit()
        {
            var sut = CreateInterpreter( SqliteNodeInterpreterOptions.Default.EnableUpdateOrDeleteLimit( false ) );
            var table = SqlTableMock.Create<int>( "foo", new[] { "a", "b" }, new[] { "a" } );
            var foo = table.ToRecordSet( "f" );
            var other = SqlNode.RawRecordSet( "bar" );

            var dataSource = foo
                .Join( other.InnerOn( foo["a"] == other["a"] ) )
                .With( SqlNode.RawQuery( "SELECT * FROM abc" ).ToCte( "cba" ) )
                .AndWhere( s => s["f"]["a"].InQuery( SqlNode.RawQuery( "SELECT cba.c FROM cba" ) ) )
                .OrderBy( s => new[] { s["f"]["a"].Asc() } )
                .Limit( SqlNode.Literal( 5 ) )
                .Offset( SqlNode.Literal( 10 ) );

            sut.Visit( dataSource.ToUpdate( s => new[] { s["f"]["a"].Assign( SqlNode.Literal( 10 ) ) } ) );

            sut.Context.Sql.ToString()
                .Should()
                .Be(
                    @"WITH ""cba"" AS (
  SELECT * FROM abc
)
UPDATE ""common_foo"" SET
  ""a"" = 10
WHERE ""common_foo"".""a"" IN (
  SELECT
    ""f"".""a""
  FROM ""common_foo"" AS ""f""
  INNER JOIN bar ON ""f"".""a"" = bar.""a""
  WHERE ""f"".""a"" IN (
    SELECT cba.c FROM cba
  )
  ORDER BY ""f"".""a"" ASC
  LIMIT 5 OFFSET 10
)" );
        }

        [Fact]
        public void Visit_ShouldInterpretUpdateMultiDataSource_WithUpdateFromDisabled_WithCteAndWhereAndOrderByAndLimit()
        {
            var sut = CreateInterpreter( SqliteNodeInterpreterOptions.Default.EnableUpdateFrom( false ) );
            var table = SqlTableMock.Create<int>( "foo", new[] { "a", "b" }, new[] { "a" } );
            var foo = table.ToRecordSet( "f" );
            var other = SqlNode.RawRecordSet( "bar" );

            var dataSource = foo
                .Join( other.InnerOn( foo["a"] == other["a"] ) )
                .With( SqlNode.RawQuery( "SELECT * FROM abc" ).ToCte( "cba" ) )
                .AndWhere( s => s["f"]["a"].InQuery( SqlNode.RawQuery( "SELECT cba.c FROM cba" ) ) )
                .OrderBy( s => new[] { s["f"]["a"].Asc() } )
                .Limit( SqlNode.Literal( 5 ) )
                .Offset( SqlNode.Literal( 10 ) );

            sut.Visit( dataSource.ToUpdate( s => new[] { s["f"]["a"].Assign( SqlNode.Literal( 10 ) ) } ) );

            sut.Context.Sql.ToString()
                .Should()
                .Be(
                    @"WITH ""cba"" AS (
  SELECT * FROM abc
)
UPDATE ""common_foo"" SET
  ""a"" = 10
WHERE ""common_foo"".""a"" IN (
  SELECT
    ""f"".""a""
  FROM ""common_foo"" AS ""f""
  INNER JOIN bar ON ""f"".""a"" = bar.""a""
  WHERE ""f"".""a"" IN (
    SELECT cba.c FROM cba
  )
  ORDER BY ""f"".""a"" ASC
  LIMIT 5 OFFSET 10
)" );
        }

        [Fact]
        public void Visit_ShouldInterpretUpdateMultiDataSource_WithAllTraits()
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

            sut.Visit( dataSource.ToUpdate( s => new[] { s["f"]["a"].Assign( SqlNode.Literal( 10 ) ) } ) );

            sut.Context.Sql.ToString()
                .Should()
                .Be(
                    @"WITH ""cba"" AS (
  SELECT * FROM abc
)
UPDATE ""common_foo"" SET
  ""a"" = 10
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
        public void Visit_ShouldInterpretUpdateMultiDataSource_WithSubQueryInAssignment()
        {
            var sut = CreateInterpreter();
            var table = SqlTableMock.Create<int>( "foo", new[] { "a", "b" }, new[] { "a" } );
            var foo = table.ToRecordSet( "f" );
            var other = SqlNode.RawRecordSet( "bar" );
            var dataSource = foo.Join( other.InnerOn( foo["a"] == other["a"] ) );

            sut.Visit(
                dataSource.ToUpdate(
                    s => new[]
                    {
                        s["f"]["b"]
                            .Assign(
                                SqlNode.RawRecordSet( "qux" )
                                    .ToDataSource()
                                    .AndWhere( b => b.From["x"] == s["f"]["a"] )
                                    .Select( b => new[] { (s["bar"]["y"] + b.From["y"]).As( "y" ) } ) )
                    } ) );

            sut.Context.Sql.ToString()
                .Should()
                .Be(
                    @"UPDATE ""common_foo"" AS ""f"" SET
  ""b"" = (
    SELECT
      (bar.""y"" + qux.""y"") AS ""y""
    FROM qux
    WHERE qux.""x"" = ""f"".""a""
  )
FROM bar
WHERE ""f"".""a"" = bar.""a""" );
        }

        [Fact]
        public void Visit_ShouldInterpretUpdateMultiDataSource_WithUpdateFromDisabled_WithSubQueryInAssignment()
        {
            var sut = CreateInterpreter( SqliteNodeInterpreterOptions.Default.EnableUpdateFrom( false ) );
            var table = SqlTableMock.Create<int>( "foo", new[] { "a", "b" }, new[] { "a" } );
            var foo = table.ToRecordSet( "f" );
            var other = SqlNode.RawRecordSet( "bar" );
            var dataSource = foo.Join( other.InnerOn( foo["a"] == other["a"] ) );

            sut.Visit(
                dataSource.ToUpdate(
                    s => new[]
                    {
                        s["f"]["b"]
                            .Assign(
                                SqlNode.RawRecordSet( "qux" )
                                    .ToDataSource()
                                    .AndWhere( b => b.From["x"] == s["f"]["a"] )
                                    .Select( b => new[] { (s["bar"]["y"] + b.From["y"]).As( "y" ) } ) )
                    } ) );

            sut.Context.Sql.ToString()
                .Should()
                .SatisfySql(
                    @"WITH ""_{GUID}"" AS (
  SELECT
    ""f"".""a"" AS ""ID_a_0"",
    (
      SELECT
        (bar.""y"" + qux.""y"") AS ""y""
      FROM qux
      WHERE qux.""x"" = ""f"".""a""
    ) AS ""VAL_b_0""
  FROM ""common_foo"" AS ""f""
  INNER JOIN bar ON ""f"".""a"" = bar.""a""
)
UPDATE ""common_foo"" SET
  ""b"" = (
    SELECT
      ""_{GUID}"".""VAL_b_0""
    FROM ""_{GUID}""
    WHERE ""common_foo"".""a"" = ""_{GUID}"".""ID_a_0""
    LIMIT 1
  )
WHERE ""common_foo"".""a"" IN (
  SELECT
    ""_{GUID}"".""ID_a_0""
  FROM ""_{GUID}""
);" );
        }

        [Fact]
        public void Visit_ShouldInterpretUpdateMultiDataSource_WithDataSourceFieldsAsAssignedValues()
        {
            var sut = CreateInterpreter();
            var table = SqlTableMock.Create<int>( "foo", new[] { "a", "b", "c", "d" }, new[] { "a" } );
            var foo = table.ToRecordSet( "f" );
            var other = SqlNode.RawRecordSet( "bar" );
            var dataSource = foo.Join( other.InnerOn( foo["a"] == other["a"] ) );

            sut.Visit(
                dataSource.ToUpdate(
                    s => new[]
                    {
                        s["f"]["a"].Assign( s["f"]["a"] + s["bar"]["a"] + SqlNode.Literal( 1 ) ),
                        s["f"]["b"].Assign( s["f"]["c"] * s["f"]["d"] )
                    } ) );

            sut.Context.Sql.ToString()
                .Should()
                .Be(
                    @"UPDATE ""common_foo"" AS ""f"" SET
  ""a"" = ((""f"".""a"" + bar.""a"") + 1),
  ""b"" = (""f"".""c"" * ""f"".""d"")
FROM bar
WHERE ""f"".""a"" = bar.""a""" );
        }

        [Fact]
        public void Visit_ShouldInterpretUpdateMultiDataSource_WithUpdateFromDisabled_WithDataSourceFieldsAsAssignedValues()
        {
            var sut = CreateInterpreter( SqliteNodeInterpreterOptions.Default.EnableUpdateFrom( false ) );
            var table = SqlTableMock.Create<int>( "foo", new[] { "a", "b", "c", "d" }, new[] { "a" } );
            var foo = table.ToRecordSet( "f" );
            var other = SqlNode.RawRecordSet( "bar" );
            var dataSource = foo.Join( other.InnerOn( foo["a"] == other["a"] ) );

            sut.Visit(
                dataSource.ToUpdate(
                    s => new[]
                    {
                        s["f"]["a"].Assign( s["f"]["a"] + s["bar"]["a"] + SqlNode.Literal( 1 ) ),
                        s["f"]["b"].Assign( s["f"]["c"] * s["f"]["d"] )
                    } ) );

            sut.Context.Sql.ToString()
                .Should()
                .SatisfySql(
                    @"WITH ""_{GUID}"" AS (
  SELECT
    ""f"".""a"" AS ""ID_a_0"",
    ((""f"".""a"" + bar.""a"") + 1) AS ""VAL_a_0""
  FROM ""common_foo"" AS ""f""
  INNER JOIN bar ON ""f"".""a"" = bar.""a""
)
UPDATE ""common_foo"" SET
  ""a"" = (
    SELECT
      ""_{GUID}"".""VAL_a_0""
    FROM ""_{GUID}""
    WHERE ""common_foo"".""a"" = ""_{GUID}"".""ID_a_0""
    LIMIT 1
  ),
  ""b"" = (""common_foo"".""c"" * ""common_foo"".""d"")
WHERE ""common_foo"".""a"" IN (
  SELECT
    ""_{GUID}"".""ID_a_0""
  FROM ""_{GUID}""
);" );
        }

        [Fact]
        public void Visit_ShouldInterpretUpdateComplexDataSource_WhenTargetIsTableWithSingleColumnPrimaryKey()
        {
            var sut = CreateInterpreter();
            var table = SqlTableMock.Create<int>( "foo", new[] { "a", "b" }, new[] { "a" } );
            var foo = table.ToRecordSet( "f" );
            var other = SqlNode.RawRecordSet( "bar" );

            var dataSource = foo
                .Join( other.InnerOn( foo["a"] == other["a"] ) )
                .GroupBy( s => new[] { s["f"]["b"] } );

            sut.Visit( dataSource.ToUpdate( s => new[] { s["f"]["a"].Assign( SqlNode.Literal( 10 ) ) } ) );

            sut.Context.Sql.ToString()
                .Should()
                .Be(
                    @"UPDATE ""common_foo"" SET
  ""a"" = 10
WHERE ""common_foo"".""a"" IN (
  SELECT
    ""f"".""a""
  FROM ""common_foo"" AS ""f""
  INNER JOIN bar ON ""f"".""a"" = bar.""a""
  GROUP BY ""f"".""b""
)" );
        }

        [Fact]
        public void Visit_ShouldInterpretUpdateComplexDataSource_WhenTargetIsTableWithMultiColumnPrimaryKey()
        {
            var sut = CreateInterpreter();
            var table = SqlTableMock.Create<int>( "foo", new[] { "a", "b" } );
            var foo = table.ToRecordSet( "f" );
            var other = SqlNode.RawRecordSet( "bar" );

            var dataSource = foo
                .Join( other.InnerOn( foo["a"] == other["a"] ) )
                .GroupBy( s => new[] { s["f"]["b"] } );

            sut.Visit( dataSource.ToUpdate( s => new[] { s["f"]["a"].Assign( SqlNode.Literal( 10 ) ) } ) );

            sut.Context.Sql.ToString()
                .Should()
                .Be(
                    @"UPDATE ""common_foo"" SET
  ""a"" = 10
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
        public void Visit_ShouldInterpretUpdateComplexDataSource_WhenTargetIsTableBuilderWithSingleColumnPrimaryKey()
        {
            var sut = CreateInterpreter();
            var table = SqlTableBuilderMock.Create<int>( "foo", new[] { "a", "b" }, new[] { "a" } );
            var foo = table.ToRecordSet( "f" );
            var other = SqlNode.RawRecordSet( "bar" );

            var dataSource = foo
                .Join( other.InnerOn( foo["a"] == other["a"] ) )
                .GroupBy( s => new[] { s["f"]["b"] } );

            sut.Visit( dataSource.ToUpdate( s => new[] { s["f"]["a"].Assign( SqlNode.Literal( 10 ) ) } ) );

            sut.Context.Sql.ToString()
                .Should()
                .Be(
                    @"UPDATE ""common_foo"" SET
  ""a"" = 10
WHERE ""common_foo"".""a"" IN (
  SELECT
    ""f"".""a""
  FROM ""common_foo"" AS ""f""
  INNER JOIN bar ON ""f"".""a"" = bar.""a""
  GROUP BY ""f"".""b""
)" );
        }

        [Fact]
        public void Visit_ShouldInterpretUpdateComplexDataSource_WhenTargetIsTableBuilderWithMultiColumnPrimaryKey()
        {
            var sut = CreateInterpreter();
            var table = SqlTableBuilderMock.Create<int>( "foo", new[] { "a", "b" } );
            var foo = table.ToRecordSet( "f" );
            var other = SqlNode.RawRecordSet( "bar" );

            var dataSource = foo
                .Join( other.InnerOn( foo["a"] == other["a"] ) )
                .GroupBy( s => new[] { s["f"]["b"] } );

            sut.Visit( dataSource.ToUpdate( s => new[] { s["f"]["a"].Assign( SqlNode.Literal( 10 ) ) } ) );

            sut.Context.Sql.ToString()
                .Should()
                .Be(
                    @"UPDATE ""common_foo"" SET
  ""a"" = 10
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
        public void Visit_ShouldInterpretUpdateComplexDataSource_WhenTargetIsTableBuilderWithoutPrimaryKey()
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

            sut.Visit( dataSource.ToUpdate( s => new[] { s["f"]["a"].Assign( SqlNode.Literal( 10 ) ) } ) );

            sut.Context.Sql.ToString()
                .Should()
                .Be(
                    @"UPDATE ""common_foo"" SET
  ""a"" = 10
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
        public void Visit_ShouldInterpretUpdateComplexDataSource_WhenTargetIsNewTableWithSingleColumnPrimaryKey(
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

            sut.Visit( dataSource.ToUpdate( s => new[] { s["f"]["a"].Assign( SqlNode.Literal( 10 ) ) } ) );

            sut.Context.Sql.ToString()
                .Should()
                .Be(
                    $@"UPDATE {expectedName} SET
  ""a"" = 10
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
        public void Visit_ShouldInterpretUpdateComplexDataSource_WhenTargetIsNewTableWithMultiColumnPrimaryKey(
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

            sut.Visit( dataSource.ToUpdate( s => new[] { s["f"]["a"].Assign( SqlNode.Literal( 10 ) ) } ) );

            sut.Context.Sql.ToString()
                .Should()
                .Be(
                    $@"UPDATE {expectedName} SET
  ""a"" = 10
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
        public void Visit_ShouldInterpretUpdateComplexDataSource_WhenTargetIsNewTableWithoutPrimaryKey(
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

            sut.Visit( dataSource.ToUpdate( s => new[] { s["f"]["a"].Assign( SqlNode.Literal( 10 ) ) } ) );

            sut.Context.Sql.ToString()
                .Should()
                .Be(
                    $@"UPDATE {expectedName} SET
  ""a"" = 10
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
        public void
            Visit_ShouldInterpretUpdateComplexDataSource_WhenTargetHasSingleColumnPrimaryKey_WithDataSourceFieldsAsAssignedValues()
        {
            var sut = CreateInterpreter();
            var table = SqlTableBuilderMock.Create<int>( "foo", new[] { "a", "b", "c", "d" }, new[] { "a" } );
            var foo = table.ToRecordSet( "f" );
            var other = SqlNode.RawRecordSet( "bar" );

            var dataSource = foo
                .Join( other.InnerOn( foo["a"] == other["a"] ) )
                .GroupBy( s => new[] { s["f"]["b"] } );

            sut.Visit(
                dataSource.ToUpdate(
                    s => new[]
                    {
                        s["f"]["a"]
                            .Assign(
                                table.ToRecordSet( "x" )
                                    .ToDataSource()
                                    .AndWhere( t => t["x"]["a"] > dataSource["f"]["a"] )
                                    .Select( t => new[] { t["x"]["b"].AsSelf() } ) ),
                        s["f"]["b"].Assign( s["f"]["c"] * s["f"]["d"] )
                    } ) );

            sut.Context.Sql.ToString()
                .Should()
                .Be(
                    @"UPDATE ""common_foo"" SET
  ""a"" = (
    SELECT
      ""x"".""b""
    FROM ""common_foo"" AS ""x""
    WHERE ""x"".""a"" > ""common_foo"".""a""
  ),
  ""b"" = (""common_foo"".""c"" * ""common_foo"".""d"")
WHERE ""common_foo"".""a"" IN (
  SELECT
    ""f"".""a""
  FROM ""common_foo"" AS ""f""
  INNER JOIN bar ON ""f"".""a"" = bar.""a""
  GROUP BY ""f"".""b""
)" );
        }

        [Fact]
        public void
            Visit_ShouldInterpretUpdateComplexDataSource_WhenTargetHasMultiColumnPrimaryKey_WithDataSourceFieldsAsAssignedValues()
        {
            var sut = CreateInterpreter();
            var table = SqlTableBuilderMock.Create<int>( "foo", new[] { "a", "b", "c", "d" }, new[] { "a", "b" } );
            var foo = table.ToRecordSet( "f" );
            var other = SqlNode.RawRecordSet( "bar" );

            var dataSource = foo
                .Join( other.InnerOn( foo["a"] == other["a"] ) )
                .GroupBy( s => new[] { s["f"]["b"] } );

            sut.Visit(
                dataSource.ToUpdate(
                    s => new[]
                    {
                        s["f"]["a"]
                            .Assign(
                                table.ToRecordSet( "x" )
                                    .ToDataSource()
                                    .AndWhere( t => t["x"]["a"] > dataSource["f"]["a"] )
                                    .Select( t => new[] { t["x"]["b"].AsSelf() } ) ),
                        s["f"]["b"].Assign( s["f"]["c"] * s["f"]["d"] )
                    } ) );

            sut.Context.Sql.ToString()
                .Should()
                .Be(
                    @"UPDATE ""common_foo"" SET
  ""a"" = (
    SELECT
      ""x"".""b""
    FROM ""common_foo"" AS ""x""
    WHERE ""x"".""a"" > ""common_foo"".""a""
  ),
  ""b"" = (""common_foo"".""c"" * ""common_foo"".""d"")
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
        public void Visit_ShouldInterpretUpdateComplexDataSource_WithComplexAssignmentAndSingleColumnPrimaryKey()
        {
            var sut = CreateInterpreter();
            var table = SqlTableMock.Create<int>( "foo", new[] { "a", "b", "c" }, new[] { "a" } );
            var foo = table.ToRecordSet( "f" );
            var dataSource = foo
                .Join( SqlJoinDefinition.Inner( SqlNode.RawRecordSet( "bar" ), x => x.Inner["a"] == foo["a"] ) )
                .GroupBy( s => new[] { s["f"]["b"] } );

            sut.Visit(
                dataSource.ToUpdate(
                    s => new[]
                    {
                        s["f"]["b"].Assign( s["f"]["b"] + s["bar"]["b"] ), s["f"]["c"].Assign( s["f"]["c"] + SqlNode.Literal( 1 ) )
                    } ) );

            sut.Context.Sql.ToString()
                .Should()
                .SatisfySql(
                    @"WITH ""_{GUID}"" AS (
  SELECT
    ""f"".""a"" AS ""ID_a_0"",
    (""f"".""b"" + bar.""b"") AS ""VAL_b_0""
  FROM ""common_foo"" AS ""f""
  INNER JOIN bar ON bar.""a"" = ""f"".""a""
  GROUP BY ""f"".""b""
)
UPDATE ""common_foo"" SET
  ""b"" = ""_{GUID}"".""VAL_b_0"",
  ""c"" = (""common_foo"".""c"" + 1)
FROM ""_{GUID}""
WHERE ""common_foo"".""a"" = ""_{GUID}"".""ID_a_0"";" );
        }

        [Fact]
        public void Visit_ShouldInterpretUpdateComplexDataSource_WithUpdateFromDisabled_WithComplexAssignmentAndSingleColumnPrimaryKey()
        {
            var sut = CreateInterpreter( SqliteNodeInterpreterOptions.Default.EnableUpdateFrom( false ) );
            var table = SqlTableMock.Create<int>( "foo", new[] { "a", "b", "c" }, new[] { "a" } );
            var foo = table.ToRecordSet( "f" );
            var dataSource = foo
                .Join( SqlJoinDefinition.Inner( SqlNode.RawRecordSet( "bar" ), x => x.Inner["a"] == foo["a"] ) )
                .GroupBy( s => new[] { s["f"]["b"] } );

            sut.Visit(
                dataSource.ToUpdate(
                    s => new[]
                    {
                        s["f"]["b"].Assign( s["f"]["b"] + s["bar"]["b"] ), s["f"]["c"].Assign( s["f"]["c"] + SqlNode.Literal( 1 ) )
                    } ) );

            sut.Context.Sql.ToString()
                .Should()
                .SatisfySql(
                    @"WITH ""_{GUID}"" AS (
  SELECT
    ""f"".""a"" AS ""ID_a_0"",
    (""f"".""b"" + bar.""b"") AS ""VAL_b_0""
  FROM ""common_foo"" AS ""f""
  INNER JOIN bar ON bar.""a"" = ""f"".""a""
  GROUP BY ""f"".""b""
)
UPDATE ""common_foo"" SET
  ""b"" = (
    SELECT
      ""_{GUID}"".""VAL_b_0""
    FROM ""_{GUID}""
    WHERE ""common_foo"".""a"" = ""_{GUID}"".""ID_a_0""
    LIMIT 1
  ),
  ""c"" = (""common_foo"".""c"" + 1)
WHERE ""common_foo"".""a"" IN (
  SELECT
    ""_{GUID}"".""ID_a_0""
    FROM ""_{GUID}""
);" );
        }

        [Fact]
        public void Visit_ShouldInterpretUpdateComplexDataSource_WithComplexAssignmentAndMultipleColumnPrimaryKey()
        {
            var sut = CreateInterpreter();
            var table = SqlTableMock.Create<int>( "foo", new[] { "a", "b", "c", "d" }, new[] { "a", "b" } );
            var foo = table.ToRecordSet( "f" );
            var dataSource = foo
                .Join( SqlJoinDefinition.Inner( SqlNode.RawRecordSet( "bar" ), x => x.Inner["a"] == foo["a"] ) )
                .GroupBy( s => new[] { s["f"]["b"] } );

            sut.Visit(
                dataSource.ToUpdate(
                    s => new[]
                    {
                        s["f"]["c"].Assign( s["f"]["c"] + s["bar"]["c"] ), s["f"]["d"].Assign( s["f"]["d"] + SqlNode.Literal( 1 ) )
                    } ) );

            sut.Context.Sql.ToString()
                .Should()
                .SatisfySql(
                    @"WITH ""_{GUID}"" AS (
  SELECT
    ""f"".""a"" AS ""ID_a_0"",
    ""f"".""b"" AS ""ID_b_1"",
    (""f"".""c"" + bar.""c"") AS ""VAL_c_0""
  FROM ""common_foo"" AS ""f""
  INNER JOIN bar ON bar.""a"" = ""f"".""a""
  GROUP BY ""f"".""b""
)
UPDATE ""common_foo"" SET
  ""c"" = ""_{GUID}"".""VAL_c_0"",
  ""d"" = (""common_foo"".""d"" + 1)
FROM ""_{GUID}""
WHERE (""common_foo"".""a"" = ""_{GUID}"".""ID_a_0"") AND (""common_foo"".""b"" = ""_{GUID}"".""ID_b_1"");" );
        }

        [Fact]
        public void Visit_ShouldInterpretUpdateComplexDataSource_WithUpdateFromDisabled_WithComplexAssignmentAndMultipleColumnPrimaryKey()
        {
            var sut = CreateInterpreter( SqliteNodeInterpreterOptions.Default.EnableUpdateFrom( false ) );
            var table = SqlTableMock.Create<int>( "foo", new[] { "a", "b", "c", "d" }, new[] { "a", "b" } );
            var foo = table.ToRecordSet( "f" );
            var dataSource = foo
                .Join( SqlJoinDefinition.Inner( SqlNode.RawRecordSet( "bar" ), x => x.Inner["a"] == foo["a"] ) )
                .GroupBy( s => new[] { s["f"]["b"] } );

            sut.Visit(
                dataSource.ToUpdate(
                    s => new[]
                    {
                        s["f"]["c"].Assign( s["f"]["c"] + s["bar"]["c"] ), s["f"]["d"].Assign( s["f"]["d"] + SqlNode.Literal( 1 ) )
                    } ) );

            sut.Context.Sql.ToString()
                .Should()
                .SatisfySql(
                    @"WITH ""_{GUID}"" AS (
  SELECT
    ""f"".""a"" AS ""ID_a_0"",
    ""f"".""b"" AS ""ID_b_1"",
    (""f"".""c"" + bar.""c"") AS ""VAL_c_0""
  FROM ""common_foo"" AS ""f""
  INNER JOIN bar ON bar.""a"" = ""f"".""a""
  GROUP BY ""f"".""b""
)
UPDATE ""common_foo"" SET
  ""c"" = (
    SELECT
      ""_{GUID}"".""VAL_c_0""
    FROM ""_{GUID}""
    WHERE (""common_foo"".""a"" = ""_{GUID}"".""ID_a_0"") AND (""common_foo"".""b"" = ""_{GUID}"".""ID_b_1"")
    LIMIT 1
  ),
  ""d"" = (""common_foo"".""d"" + 1)
WHERE EXISTS (
  SELECT
    *
  FROM ""_{GUID}""
  WHERE (""common_foo"".""a"" = ""_{GUID}"".""ID_a_0"") AND (""common_foo"".""b"" = ""_{GUID}"".""ID_b_1"")
);" );
        }

        [Fact]
        public void Visit_ShouldInterpretUpdateComplexDataSource_WithCteAndComplexAssignment()
        {
            var sut = CreateInterpreter();
            var table = SqlTableMock.Create<int>( "foo", new[] { "a", "b" }, new[] { "a" } );
            var view = SqlViewMock.Create(
                    "v",
                    SqlNode.RawRecordSet( "bar" ).ToDataSource().Select( s => new[] { s.From["a"].AsSelf(), s.From["b"].AsSelf() } ) )
                .Node;

            var foo = table.ToRecordSet( "f" );

            var dataSource = foo
                .Join( SqlJoinDefinition.Inner( view, x => x.Inner["a"] == foo["a"] ) )
                .With( SqlNode.RawQuery( "SELECT * FROM lorem" ).ToCte( "ipsum" ) )
                .GroupBy( s => new[] { s["f"]["b"] } );

            sut.Visit( dataSource.ToUpdate( s => new[] { s["f"]["b"].Assign( s["f"]["b"] + s["common.v"]["b"] ) } ) );

            sut.Context.Sql.ToString()
                .Should()
                .SatisfySql(
                    @"WITH ""ipsum"" AS (
  SELECT * FROM lorem
),
""_{GUID}"" AS (
  SELECT
    ""f"".""a"" AS ""ID_a_0"",
    (""f"".""b"" + ""common_v"".""b"") AS ""VAL_b_0""
  FROM ""common_foo"" AS ""f""
  INNER JOIN ""common_v"" ON ""common_v"".""a"" = ""f"".""a""
  GROUP BY ""f"".""b""
)
UPDATE ""common_foo"" SET
  ""b"" = ""_{GUID}"".""VAL_b_0""
FROM ""_{GUID}""
WHERE ""common_foo"".""a"" = ""_{GUID}"".""ID_a_0"";" );
        }

        [Fact]
        public void Visit_ShouldInterpretUpdateComplexDataSource_WithUpdateFromDisabled_WithCteAndComplexAssignment()
        {
            var sut = CreateInterpreter( SqliteNodeInterpreterOptions.Default.EnableUpdateFrom( false ) );
            var table = SqlTableMock.Create<int>( "foo", new[] { "a", "b" }, new[] { "a" } );
            var view = SqlViewMock.Create(
                    "v",
                    SqlNode.RawRecordSet( "bar" ).ToDataSource().Select( s => new[] { s.From["a"].AsSelf(), s.From["b"].AsSelf() } ) )
                .Node;

            var foo = table.ToRecordSet( "f" );

            var dataSource = foo
                .Join( SqlJoinDefinition.Inner( view, x => x.Inner["a"] == foo["a"] ) )
                .With( SqlNode.RawQuery( "SELECT * FROM lorem" ).ToCte( "ipsum" ) )
                .GroupBy( s => new[] { s["f"]["b"] } );

            sut.Visit( dataSource.ToUpdate( s => new[] { s["f"]["b"].Assign( s["f"]["b"] + s["common.v"]["b"] ) } ) );

            sut.Context.Sql.ToString()
                .Should()
                .SatisfySql(
                    @"WITH ""ipsum"" AS (
  SELECT * FROM lorem
),
""_{GUID}"" AS (
  SELECT
    ""f"".""a"" AS ""ID_a_0"",
    (""f"".""b"" + ""common_v"".""b"") AS ""VAL_b_0""
  FROM ""common_foo"" AS ""f""
  INNER JOIN ""common_v"" ON ""common_v"".""a"" = ""f"".""a""
  GROUP BY ""f"".""b""
)
UPDATE ""common_foo"" SET
  ""b"" = (
    SELECT
      ""_{GUID}"".""VAL_b_0""
    FROM ""_{GUID}""
    WHERE ""common_foo"".""a"" = ""_{GUID}"".""ID_a_0""
    LIMIT 1
  )
WHERE ""common_foo"".""a"" IN (
  SELECT
    ""_{GUID}"".""ID_a_0""
  FROM ""_{GUID}""
);" );
        }

        [Fact]
        public void Visit_ShouldInterpretUpdateWithComplexAssignments_WithMultipleAssignments()
        {
            var sut = CreateInterpreter();
            var table = SqlTableMock.Create<int>( "foo", new[] { "a", "b", "c", "d" }, new[] { "a" } );
            var subQuery = SqlNode.RawRecordSet( "U" )
                .ToDataSource()
                .Select( x => new[] { x.From["x"].AsSelf(), x.From["y"].AsSelf() } )
                .AsSet( "lorem" );

            var foo = table.ToRecordSet( "f" );
            var dataSource = foo.Join( SqlJoinDefinition.Inner( subQuery, x => x.Inner["x"] == foo["b"] ) );

            sut.Visit(
                dataSource.ToUpdate(
                    s => new[]
                    {
                        s["f"]["b"].Assign( s["lorem"]["x"] + SqlNode.Literal( 1 ) ),
                        s["f"]["d"].Assign( s["f"]["d"] + SqlNode.Literal( 1 ) ),
                        s["f"]["c"].Assign( s["f"]["c"] * s["lorem"]["y"] )
                    } ) );

            sut.Context.Sql.ToString()
                .Should()
                .SatisfySql(
                    @"UPDATE ""common_foo"" AS ""f"" SET
  ""b"" = (""lorem"".""x"" + 1),
  ""d"" = (""f"".""d"" + 1),
  ""c"" = (""f"".""c"" * ""lorem"".""y"")
FROM (
  SELECT
    U.""x"",
    U.""y""
  FROM U
) AS ""lorem""
WHERE ""lorem"".""x"" = ""f"".""b"";" );
        }

        [Fact]
        public void Visit_ShouldInterpretUpdateWithComplexAssignments_WithUpdateFromDisabled_WithMultipleAssignments()
        {
            var sut = CreateInterpreter( SqliteNodeInterpreterOptions.Default.EnableUpdateFrom( false ) );
            var table = SqlTableMock.Create<int>( "foo", new[] { "a", "b", "c", "d" }, new[] { "a" } );
            var subQuery = SqlNode.RawRecordSet( "U" )
                .ToDataSource()
                .Select( x => new[] { x.From["x"].AsSelf(), x.From["y"].AsSelf() } )
                .AsSet( "lorem" );

            var foo = table.ToRecordSet( "f" );
            var dataSource = foo.Join( SqlJoinDefinition.Inner( subQuery, x => x.Inner["x"] == foo["b"] ) );

            sut.Visit(
                dataSource.ToUpdate(
                    s => new[]
                    {
                        s["f"]["b"].Assign( s["lorem"]["x"] + SqlNode.Literal( 1 ) ),
                        s["f"]["d"].Assign( s["f"]["d"] + SqlNode.Literal( 1 ) ),
                        s["f"]["c"].Assign( s["f"]["c"] * s["lorem"]["y"] )
                    } ) );

            sut.Context.Sql.ToString()
                .Should()
                .SatisfySql(
                    @"WITH ""_{GUID}"" AS (
  SELECT
    ""f"".""a"" AS ""ID_a_0"",
    (""lorem"".""x"" + 1) AS ""VAL_b_0"",
    (""f"".""c"" * ""lorem"".""y"") AS ""VAL_c_2""
  FROM ""common_foo"" AS ""f""
  INNER JOIN (
    SELECT
      U.""x"",
      U.""y""
    FROM U
  ) AS ""lorem"" ON ""lorem"".""x"" = ""f"".""b""
)
UPDATE ""common_foo"" SET
  ""b"" = (
    SELECT
      ""_{GUID}"".""VAL_b_0""
    FROM ""_{GUID}""
    WHERE ""common_foo"".""a"" = ""_{GUID}"".""ID_a_0""
    LIMIT 1
  ),
  ""d"" = (""common_foo"".""d"" + 1),
  ""c"" = (
    SELECT
      ""_{GUID}"".""VAL_c_2""
    FROM ""_{GUID}""
    WHERE ""common_foo"".""a"" = ""_{GUID}"".""ID_a_0""
    LIMIT 1
  )
WHERE ""common_foo"".""a"" IN (
  SELECT
    ""_{GUID}"".""ID_a_0""
  FROM ""_{GUID}""
);" );
        }

        [Fact]
        public void Visit_ShouldThrowSqlNodeVisitorException_WhenUpdateIsComplexAndDataSourceFromIsNotTable()
        {
            var sut = CreateInterpreter();
            var node = SqlNode.RawRecordSet( "foo" ).ToDataSource().GroupBy( s => new[] { s["foo"]["x"] } ).ToUpdate();
            var action = Lambda.Of( () => sut.Visit( node ) );

            action.Should()
                .ThrowExactly<SqlNodeVisitorException>()
                .AndMatch( e => ReferenceEquals( e.Node, node ) && ReferenceEquals( e.Visitor, sut ) );
        }

        [Fact]
        public void Visit_ShouldThrowSqlNodeVisitorException_WhenUpdateIsComplexAndDataSourceFromIsTableWithoutAlias()
        {
            var sut = CreateInterpreter();
            var foo = SqlTableMock.Create<int>( "foo", new[] { "a" } ).ToRecordSet();
            var node = foo.Join( SqlNode.RawRecordSet( "bar" ).Cross() ).GroupBy( s => new[] { s["bar"]["x"] } ).ToUpdate();
            var action = Lambda.Of( () => sut.Visit( node ) );

            action.Should()
                .ThrowExactly<SqlNodeVisitorException>()
                .AndMatch( e => ReferenceEquals( e.Node, node ) && ReferenceEquals( e.Visitor, sut ) );
        }

        [Fact]
        public void Visit_ShouldThrowSqlNodeVisitorException_WhenUpdateIsComplexAndDataSourceFromIsTableBuilderWithoutColumns()
        {
            var sut = CreateInterpreter();
            var foo = SqlTableBuilderMock.CreateEmpty( "foo" ).ToRecordSet( "f" );
            var node = foo.Join( SqlNode.RawRecordSet( "bar" ).Cross() ).GroupBy( s => new[] { s["bar"]["x"] } ).ToUpdate();
            var action = Lambda.Of( () => sut.Visit( node ) );

            action.Should()
                .ThrowExactly<SqlNodeVisitorException>()
                .AndMatch( e => ReferenceEquals( e.Node, node ) && ReferenceEquals( e.Visitor, sut ) );
        }

        [Fact]
        public void
            Visit_ShouldThrowSqlNodeVisitorException_WhenUpdateIsComplexAndDataSourceFromIsNewTableWithoutPrimaryKeyAndColumns()
        {
            var sut = CreateInterpreter();
            var foo = SqlNode.CreateTable( SqlRecordSetInfo.Create( "foo" ), Array.Empty<SqlColumnDefinitionNode>() ).AsSet( "f" );
            var node = foo.Join( SqlNode.RawRecordSet( "bar" ).Cross() ).GroupBy( s => new[] { s["bar"]["x"] } ).ToUpdate();
            var action = Lambda.Of( () => sut.Visit( node ) );

            action.Should()
                .ThrowExactly<SqlNodeVisitorException>()
                .AndMatch( e => ReferenceEquals( e.Node, node ) && ReferenceEquals( e.Visitor, sut ) );
        }

        [Fact]
        public void
            Visit_ShouldThrowSqlNodeVisitorException_WhenUpdateIsComplexAndDataSourceFromIsNewTableWithPrimaryKeyWithoutColumns()
        {
            var sut = CreateInterpreter();
            var foo = SqlNode.CreateTable(
                    SqlRecordSetInfo.Create( "foo" ),
                    new[] { SqlNode.Column<int>( "a" ) },
                    constraintsProvider: _ =>
                        SqlCreateTableConstraints.Empty.WithPrimaryKey(
                            SqlNode.PrimaryKey( SqlSchemaObjectName.Create( "PK" ), ReadOnlyArray<SqlOrderByNode>.Empty ) ) )
                .AsSet( "f" );

            var node = foo.Join( SqlNode.RawRecordSet( "bar" ).Cross() ).GroupBy( s => new[] { s["bar"]["x"] } ).ToUpdate();
            var action = Lambda.Of( () => sut.Visit( node ) );

            action.Should()
                .ThrowExactly<SqlNodeVisitorException>()
                .AndMatch( e => ReferenceEquals( e.Node, node ) && ReferenceEquals( e.Visitor, sut ) );
        }

        [Fact]
        public void
            Visit_ShouldThrowSqlNodeVisitorException_WhenUpdateIsComplexAndDataSourceFromIsNewTableWithPrimaryKeyContainingNonDataFieldColumn()
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

            var node = foo.Join( SqlNode.RawRecordSet( "bar" ).Cross() ).GroupBy( s => new[] { s["bar"]["x"] } ).ToUpdate();
            var action = Lambda.Of( () => sut.Visit( node ) );

            action.Should()
                .ThrowExactly<SqlNodeVisitorException>()
                .AndMatch( e => ReferenceEquals( e.Node, node ) && ReferenceEquals( e.Visitor, sut ) );
        }
    }
}
