using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Visitors;
using LfrlAnvil.TestExtensions.Sql.Mocks;

namespace LfrlAnvil.MySql.Tests;

public partial class MySqlNodeInterpreterTests
{
    public class Upsert : TestsBase
    {
        [Fact]
        public void Visit_ShouldInterpretUpsertWithValues_WithConflictTarget()
        {
            var sut = CreateInterpreter();
            sut.Visit(
                SqlNode.Values(
                        new[,]
                        {
                            { SqlNode.Literal( "foo" ), SqlNode.Literal( 5 ) },
                            { SqlNode.RawExpression( "bar.a" ), SqlNode.Literal( 25 ) }
                        } )
                    .ToUpsert(
                        SqlNode.RawRecordSet( "qux" ),
                        r => new[] { r["a"], r["b"] },
                        (r, i) => new[] { r["b"].Assign( r["b"] + i["b"] ), r["c"].Assign( i["b"] + SqlNode.Literal( 1 ) ) },
                        r => new[] { r["a"], r["c"] } ) );

            sut.Context.Sql.ToString()
                .Should()
                .Be(
                    @"INSERT INTO qux (`a`, `b`)
VALUES
('foo', 5),
((bar.a), 25)
AS `new`
ON DUPLICATE KEY UPDATE
  `b` = (qux.`b` + `new`.`b`),
  `c` = (`new`.`b` + 1)" );
        }

        [Fact]
        public void Visit_ShouldInterpretUpsertWithValues_WithoutConflictTarget()
        {
            var sut = CreateInterpreter();
            var table = SqlTableMock.Create<int>( "qux", new[] { "a", "b", "c" }, new[] { "a", "c" } );
            sut.Visit(
                SqlNode.Values(
                        new[,]
                        {
                            { SqlNode.Literal( "foo" ), SqlNode.Literal( 5 ) },
                            { SqlNode.RawExpression( "bar.a" ), SqlNode.Literal( 25 ) }
                        } )
                    .ToUpsert(
                        table.Node,
                        r => new[] { r["a"], r["b"] },
                        (r, i) => new[] { r["b"].Assign( r["b"] + i["b"] ), r["c"].Assign( i["b"] + SqlNode.Literal( 1 ) ) } ) );

            sut.Context.Sql.ToString()
                .Should()
                .Be(
                    @"INSERT INTO `common`.`qux` (`a`, `b`)
VALUES
('foo', 5),
((bar.a), 25)
AS `new`
ON DUPLICATE KEY UPDATE
  `b` = (`common`.`qux`.`b` + `new`.`b`),
  `c` = (`new`.`b` + 1)" );
        }

        [Fact]
        public void Visit_ShouldInterpretUpsertWithDataSourceQuery_WithConflictTarget()
        {
            var sut = CreateInterpreter();
            var foo = SqlTableMock.Create<int>( "foo", new[] { "a", "b" } ).ToRecordSet();
            var bar = SqlTableMock.Create<int>( "bar", new[] { "c", "d" } ).ToRecordSet();
            var wnd = SqlNode.WindowDefinition( "wnd", new[] { foo["a"].Asc() } );

            var query = foo
                .Join( bar.InnerOn( bar["c"] == foo["a"] ) )
                .With( SqlNode.RawQuery( "SELECT * FROM abc" ).ToCte( "cba" ) )
                .Distinct()
                .AndWhere( s => s["common.bar"]["c"].InQuery( SqlNode.RawQuery( "SELECT cba.c FROM cba" ) ) )
                .GroupBy( s => new[] { s["common.foo"]["b"] } )
                .AndHaving( s => s["common.foo"]["b"] < SqlNode.Literal( 100 ) )
                .Window( wnd )
                .Select(
                    s => new SqlSelectNode[]
                    {
                        s["common.foo"]["b"].As( "a" ),
                        SqlNode.AggregateFunctions.Count( s.GetAll().ToExpression() ).Over( wnd ).As( "b" )
                    } )
                .OrderBy( s => new[] { s.DataSource["common.foo"]["b"].Asc() } )
                .Limit( SqlNode.Literal( 50 ) )
                .Offset( SqlNode.Literal( 100 ) );

            sut.Visit(
                query.ToUpsert(
                    SqlNode.RawRecordSet( "qux" ),
                    r => new[] { r["a"], r["b"] },
                    (r, i) => new[] { r["b"].Assign( r["b"] + i["b"] ), r["c"].Assign( i["b"] + SqlNode.Literal( 1 ) ) },
                    r => new[] { r["a"], r["c"] } ) );

            sut.Context.Sql.ToString()
                .Should()
                .Be(
                    @"INSERT INTO qux (`a`, `b`)
WITH `cba` AS (
  SELECT * FROM abc
)
SELECT * FROM (
  SELECT DISTINCT
    `common`.`foo`.`b` AS `a`,
    (COUNT(*) OVER `wnd`) AS `b`
  FROM `common`.`foo`
  INNER JOIN `common`.`bar` ON `common`.`bar`.`c` = `common`.`foo`.`a`
  WHERE `common`.`bar`.`c` IN (
    SELECT cba.c FROM cba
  )
  GROUP BY `common`.`foo`.`b`
  HAVING `common`.`foo`.`b` < 100
  WINDOW `wnd` AS (ORDER BY `common`.`foo`.`a` ASC)
  ORDER BY `common`.`foo`.`b` ASC
  LIMIT 50 OFFSET 100
) AS `new`(`a`, `b`)
ON DUPLICATE KEY UPDATE
  `b` = (qux.`b` + `new`.`b`),
  `c` = (`new`.`b` + 1)" );
        }

        [Fact]
        public void Visit_ShouldInterpretUpsertWithDataSourceQuery_WithoutConflictTarget()
        {
            var sut = CreateInterpreter();
            var table = SqlTableMock.Create<int>( "qux", new[] { "a", "b", "c" }, new[] { "a", "c" } );
            var foo = SqlTableMock.Create<int>( "foo", new[] { "a", "b" } ).ToRecordSet();
            var bar = SqlTableMock.Create<int>( "bar", new[] { "c", "d" } ).ToRecordSet();
            var wnd = SqlNode.WindowDefinition( "wnd", new[] { foo["a"].Asc() } );

            var query = foo
                .Join( bar.InnerOn( bar["c"] == foo["a"] ) )
                .With( SqlNode.RawQuery( "SELECT * FROM abc" ).ToCte( "cba" ) )
                .Distinct()
                .AndWhere( s => s["common.bar"]["c"].InQuery( SqlNode.RawQuery( "SELECT cba.c FROM cba" ) ) )
                .GroupBy( s => new[] { s["common.foo"]["b"] } )
                .AndHaving( s => s["common.foo"]["b"] < SqlNode.Literal( 100 ) )
                .Window( wnd )
                .Select(
                    s => new SqlSelectNode[]
                    {
                        s["common.foo"]["b"].As( "a" ),
                        SqlNode.AggregateFunctions.Count( s.GetAll().ToExpression() ).Over( wnd ).As( "b" )
                    } )
                .OrderBy( s => new[] { s.DataSource["common.foo"]["b"].Asc() } )
                .Limit( SqlNode.Literal( 50 ) )
                .Offset( SqlNode.Literal( 100 ) );

            sut.Visit(
                query.ToUpsert(
                    table.Node,
                    r => new[] { r["a"], r["b"] },
                    (r, i) => new[] { r["b"].Assign( r["b"] + i["b"] ), r["c"].Assign( i["b"] + SqlNode.Literal( 1 ) ) } ) );

            sut.Context.Sql.ToString()
                .Should()
                .Be(
                    @"INSERT INTO `common`.`qux` (`a`, `b`)
WITH `cba` AS (
  SELECT * FROM abc
)
SELECT * FROM (
  SELECT DISTINCT
    `common`.`foo`.`b` AS `a`,
    (COUNT(*) OVER `wnd`) AS `b`
  FROM `common`.`foo`
  INNER JOIN `common`.`bar` ON `common`.`bar`.`c` = `common`.`foo`.`a`
  WHERE `common`.`bar`.`c` IN (
    SELECT cba.c FROM cba
  )
  GROUP BY `common`.`foo`.`b`
  HAVING `common`.`foo`.`b` < 100
  WINDOW `wnd` AS (ORDER BY `common`.`foo`.`a` ASC)
  ORDER BY `common`.`foo`.`b` ASC
  LIMIT 50 OFFSET 100
) AS `new`(`a`, `b`)
ON DUPLICATE KEY UPDATE
  `b` = (`common`.`qux`.`b` + `new`.`b`),
  `c` = (`new`.`b` + 1)" );
        }

        [Fact]
        public void Visit_ShouldInterpretUpsertWithCompoundQuery_WithConflictTarget()
        {
            var sut = CreateInterpreter();
            var query = SqlNode.RawQuery( "SELECT foo.a, foo.b FROM foo JOIN x ON x.a = foo.a" )
                .CompoundWith(
                    SqlNode.RawQuery( "SELECT a, b FROM bar" ).ToUnionAll(),
                    SqlNode.RawQuery( "SELECT a, b FROM qux" ).ToUnion() )
                .With( SqlNode.RawQuery( "SELECT * FROM ipsum" ).ToCte( "x" ) )
                .OrderBy( SqlNode.RawExpression( "a" ).Asc() )
                .Limit( SqlNode.Literal( 50 ) )
                .Offset( SqlNode.Literal( 75 ) );

            sut.Visit(
                query.ToUpsert(
                    SqlNode.RawRecordSet( "qux" ),
                    r => new[] { r["a"], r["b"] },
                    (r, i) => new[] { r["b"].Assign( r["b"] + i["b"] ), r["c"].Assign( i["b"] + SqlNode.Literal( 1 ) ) },
                    r => new[] { r["a"], r["c"] } ) );

            sut.Context.Sql.ToString()
                .Should()
                .Be(
                    @"INSERT INTO qux (`a`, `b`)
WITH `x` AS (
  SELECT * FROM ipsum
)
SELECT * FROM (
  SELECT foo.a, foo.b FROM foo JOIN x ON x.a = foo.a
  UNION ALL
  SELECT a, b FROM bar
  UNION
  SELECT a, b FROM qux
  ORDER BY (a) ASC
  LIMIT 50 OFFSET 75
) AS `new`(`a`, `b`)
ON DUPLICATE KEY UPDATE
  `b` = (qux.`b` + `new`.`b`),
  `c` = (`new`.`b` + 1)" );
        }

        [Fact]
        public void Visit_ShouldInterpretUpsertWithCompoundQuery_WithoutConflictTarget()
        {
            var sut = CreateInterpreter();
            var table = SqlTableMock.Create<int>( "qux", new[] { "a", "b", "c" }, new[] { "a", "c" } );
            var query = SqlNode.RawQuery( "SELECT foo.a, foo.b FROM foo JOIN x ON x.a = foo.a" )
                .CompoundWith(
                    SqlNode.RawQuery( "SELECT a, b FROM bar" ).ToUnionAll(),
                    SqlNode.RawQuery( "SELECT a, b FROM qux" ).ToUnion() )
                .With( SqlNode.RawQuery( "SELECT * FROM ipsum" ).ToCte( "x" ) )
                .OrderBy( SqlNode.RawExpression( "a" ).Asc() )
                .Limit( SqlNode.Literal( 50 ) )
                .Offset( SqlNode.Literal( 75 ) );

            sut.Visit(
                query.ToUpsert(
                    table.Node,
                    r => new[] { r["a"], r["b"] },
                    (r, i) => new[] { r["b"].Assign( r["b"] + i["b"] ), r["c"].Assign( i["b"] + SqlNode.Literal( 1 ) ) } ) );

            sut.Context.Sql.ToString()
                .Should()
                .Be(
                    @"INSERT INTO `common`.`qux` (`a`, `b`)
WITH `x` AS (
  SELECT * FROM ipsum
)
SELECT * FROM (
  SELECT foo.a, foo.b FROM foo JOIN x ON x.a = foo.a
  UNION ALL
  SELECT a, b FROM bar
  UNION
  SELECT a, b FROM qux
  ORDER BY (a) ASC
  LIMIT 50 OFFSET 75
) AS `new`(`a`, `b`)
ON DUPLICATE KEY UPDATE
  `b` = (`common`.`qux`.`b` + `new`.`b`),
  `c` = (`new`.`b` + 1)" );
        }

        [Fact]
        public void Visit_ShouldInterpretUpsertWithRawQuery_WithConflictTarget()
        {
            var sut = CreateInterpreter();
            sut.Visit(
                SqlNode.RawQuery( "SELECT * FROM bar" )
                    .ToUpsert(
                        SqlNode.RawRecordSet( "qux" ),
                        r => new[] { r["a"], r["b"] },
                        (r, i) => new[] { r["b"].Assign( r["b"] + i["b"] ), r["c"].Assign( i["b"] + SqlNode.Literal( 1 ) ) },
                        r => new[] { r["a"], r["c"] } ) );

            sut.Context.Sql.ToString()
                .Should()
                .Be(
                    @"INSERT INTO qux (`a`, `b`)
SELECT * FROM (
  SELECT * FROM bar
) AS `new`(`a`, `b`)
ON DUPLICATE KEY UPDATE
  `b` = (qux.`b` + `new`.`b`),
  `c` = (`new`.`b` + 1)" );
        }

        [Fact]
        public void Visit_ShouldInterpretUpsertWithRawQuery_WithoutConflictTarget()
        {
            var sut = CreateInterpreter();
            var table = SqlTableMock.Create<int>( "qux", new[] { "a", "b", "c" }, new[] { "a", "c" } );
            sut.Visit(
                SqlNode.RawQuery( "SELECT * FROM bar" )
                    .ToUpsert(
                        table.Node,
                        r => new[] { r["a"], r["b"] },
                        (r, i) => new[] { r["b"].Assign( r["b"] + i["b"] ), r["c"].Assign( i["b"] + SqlNode.Literal( 1 ) ) } ) );

            sut.Context.Sql.ToString()
                .Should()
                .Be(
                    @"INSERT INTO `common`.`qux` (`a`, `b`)
SELECT * FROM (
  SELECT * FROM bar
) AS `new`(`a`, `b`)
ON DUPLICATE KEY UPDATE
  `b` = (`common`.`qux`.`b` + `new`.`b`),
  `c` = (`new`.`b` + 1)" );
        }

        [Fact]
        public void Visit_ShouldInterpretUpsert_WithCustomUpsertSourceAlias()
        {
            var sut = CreateInterpreter( MySqlNodeInterpreterOptions.Default.SetUpdateSourceAlias( "upsert-source" ) );
            sut.Visit(
                SqlNode.Values(
                        new[,]
                        {
                            { SqlNode.Literal( "foo" ), SqlNode.Literal( 5 ) },
                            { SqlNode.RawExpression( "bar.a" ), SqlNode.Literal( 25 ) }
                        } )
                    .ToUpsert(
                        SqlNode.RawRecordSet( "qux" ),
                        r => new[] { r["a"], r["b"] },
                        (r, i) => new[] { r["b"].Assign( r["b"] + i["b"] ), r["c"].Assign( i["b"] + SqlNode.Literal( 1 ) ) } ) );

            sut.Context.Sql.ToString()
                .Should()
                .Be(
                    @"INSERT INTO qux (`a`, `b`)
VALUES
('foo', 5),
((bar.a), 25)
AS `upsert-source`
ON DUPLICATE KEY UPDATE
  `b` = (qux.`b` + `upsert-source`.`b`),
  `c` = (`upsert-source`.`b` + 1)" );
        }
    }
}
