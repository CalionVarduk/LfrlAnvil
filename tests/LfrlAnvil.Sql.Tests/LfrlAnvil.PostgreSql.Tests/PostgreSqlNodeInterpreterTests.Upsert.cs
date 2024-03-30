using LfrlAnvil.Functional;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.Sql.Expressions.Persistence;
using LfrlAnvil.Sql.Expressions.Visitors;
using LfrlAnvil.TestExtensions.Sql.Mocks;

namespace LfrlAnvil.PostgreSql.Tests;

public partial class PostgreSqlNodeInterpreterTests
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
                    @"INSERT INTO qux (""a"", ""b"")
VALUES
('foo', 5),
((bar.a), 25)
ON CONFLICT (""a"", ""c"") DO UPDATE SET
  ""b"" = (qux.""b"" + EXCLUDED.""b""),
  ""c"" = (EXCLUDED.""b"" + 1)" );
        }

        [Fact]
        public void Visit_ShouldInterpretUpsertWithValues_WithoutConflictTargetAndTableWithPrimaryKey()
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
                    @"INSERT INTO ""common"".""qux"" (""a"", ""b"")
VALUES
('foo', 5),
((bar.a), 25)
ON CONFLICT (""a"", ""c"") DO UPDATE SET
  ""b"" = (""common"".""qux"".""b"" + EXCLUDED.""b""),
  ""c"" = (EXCLUDED.""b"" + 1)" );
        }

        [Fact]
        public void Visit_ShouldInterpretUpsertWithValues_WithoutConflictTargetAndTableBuilderWithPrimaryKey()
        {
            var sut = CreateInterpreter();
            var table = SqlTableBuilderMock.Create<int>( "qux", new[] { "a", "b", "c" }, new[] { "a", "c" } );
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
                    @"INSERT INTO ""common"".""qux"" (""a"", ""b"")
VALUES
('foo', 5),
((bar.a), 25)
ON CONFLICT (""a"", ""c"") DO UPDATE SET
  ""b"" = (""common"".""qux"".""b"" + EXCLUDED.""b""),
  ""c"" = (EXCLUDED.""b"" + 1)" );
        }

        [Fact]
        public void Visit_ShouldInterpretUpsertWithValues_WithoutConflictTargetAndNewTableWithPrimaryKey()
        {
            var sut = CreateInterpreter();
            var table = SqlNode.CreateTable(
                SqlRecordSetInfo.Create( "qux" ),
                new[] { SqlNode.Column<int>( "a" ), SqlNode.Column<int>( "b" ), SqlNode.Column<int>( "c" ) },
                constraintsProvider: r => SqlCreateTableConstraints.Empty.WithPrimaryKey(
                    SqlNode.PrimaryKey( SqlSchemaObjectName.Create( "PK" ), new[] { r["a"].Asc(), r["c"].Asc() } ) ) );

            sut.Visit(
                SqlNode.Values(
                        new[,]
                        {
                            { SqlNode.Literal( "foo" ), SqlNode.Literal( 5 ) },
                            { SqlNode.RawExpression( "bar.a" ), SqlNode.Literal( 25 ) }
                        } )
                    .ToUpsert(
                        table.RecordSet,
                        r => new[] { r["a"], r["b"] },
                        (r, i) => new[] { r["b"].Assign( r["b"] + i["b"] ), r["c"].Assign( i["b"] + SqlNode.Literal( 1 ) ) } ) );

            sut.Context.Sql.ToString()
                .Should()
                .Be(
                    @"INSERT INTO ""qux"" (""a"", ""b"")
VALUES
('foo', 5),
((bar.a), 25)
ON CONFLICT (""a"", ""c"") DO UPDATE SET
  ""b"" = (""qux"".""b"" + EXCLUDED.""b""),
  ""c"" = (EXCLUDED.""b"" + 1)" );
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
                    @"WITH ""cba"" AS (
  SELECT * FROM abc
)
INSERT INTO qux (""a"", ""b"")
SELECT DISTINCT
  ""common"".""foo"".""b"" AS ""a"",
  (COUNT(*) OVER ""wnd"") AS ""b""
FROM ""common"".""foo""
INNER JOIN ""common"".""bar"" ON ""common"".""bar"".""c"" = ""common"".""foo"".""a""
WHERE ""common"".""bar"".""c"" IN (
  SELECT cba.c FROM cba
)
GROUP BY ""common"".""foo"".""b""
HAVING ""common"".""foo"".""b"" < 100
WINDOW ""wnd"" AS (ORDER BY ""common"".""foo"".""a"" ASC)
ORDER BY ""common"".""foo"".""b"" ASC
LIMIT 50
OFFSET 100
ON CONFLICT (""a"", ""c"") DO UPDATE SET
  ""b"" = (qux.""b"" + EXCLUDED.""b""),
  ""c"" = (EXCLUDED.""b"" + 1)" );
        }

        [Fact]
        public void Visit_ShouldInterpretUpsertWithDataSourceQuery_WithoutConflictTargetAndTableWithPrimaryKey()
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
                    @"WITH ""cba"" AS (
  SELECT * FROM abc
)
INSERT INTO ""common"".""qux"" (""a"", ""b"")
SELECT DISTINCT
  ""common"".""foo"".""b"" AS ""a"",
  (COUNT(*) OVER ""wnd"") AS ""b""
FROM ""common"".""foo""
INNER JOIN ""common"".""bar"" ON ""common"".""bar"".""c"" = ""common"".""foo"".""a""
WHERE ""common"".""bar"".""c"" IN (
  SELECT cba.c FROM cba
)
GROUP BY ""common"".""foo"".""b""
HAVING ""common"".""foo"".""b"" < 100
WINDOW ""wnd"" AS (ORDER BY ""common"".""foo"".""a"" ASC)
ORDER BY ""common"".""foo"".""b"" ASC
LIMIT 50
OFFSET 100
ON CONFLICT (""a"", ""c"") DO UPDATE SET
  ""b"" = (""common"".""qux"".""b"" + EXCLUDED.""b""),
  ""c"" = (EXCLUDED.""b"" + 1)" );
        }

        [Fact]
        public void Visit_ShouldInterpretUpsertWithDataSourceQuery_WithoutConflictTargetAndTableBuilderWithPrimaryKey()
        {
            var sut = CreateInterpreter();
            var table = SqlTableBuilderMock.Create<int>( "qux", new[] { "a", "b", "c" }, new[] { "a", "c" } );
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
                    @"WITH ""cba"" AS (
  SELECT * FROM abc
)
INSERT INTO ""common"".""qux"" (""a"", ""b"")
SELECT DISTINCT
  ""common"".""foo"".""b"" AS ""a"",
  (COUNT(*) OVER ""wnd"") AS ""b""
FROM ""common"".""foo""
INNER JOIN ""common"".""bar"" ON ""common"".""bar"".""c"" = ""common"".""foo"".""a""
WHERE ""common"".""bar"".""c"" IN (
  SELECT cba.c FROM cba
)
GROUP BY ""common"".""foo"".""b""
HAVING ""common"".""foo"".""b"" < 100
WINDOW ""wnd"" AS (ORDER BY ""common"".""foo"".""a"" ASC)
ORDER BY ""common"".""foo"".""b"" ASC
LIMIT 50
OFFSET 100
ON CONFLICT (""a"", ""c"") DO UPDATE SET
  ""b"" = (""common"".""qux"".""b"" + EXCLUDED.""b""),
  ""c"" = (EXCLUDED.""b"" + 1)" );
        }

        [Fact]
        public void Visit_ShouldInterpretUpsertWithDataSourceQuery_WithoutConflictTargetAndNewTableWithPrimaryKey()
        {
            var sut = CreateInterpreter();
            var table = SqlNode.CreateTable(
                SqlRecordSetInfo.Create( "qux" ),
                new[] { SqlNode.Column<int>( "a" ), SqlNode.Column<int>( "b" ), SqlNode.Column<int>( "c" ) },
                constraintsProvider: r => SqlCreateTableConstraints.Empty.WithPrimaryKey(
                    SqlNode.PrimaryKey( SqlSchemaObjectName.Create( "PK" ), new[] { r["a"].Asc(), r["c"].Asc() } ) ) );

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
                    table.RecordSet,
                    r => new[] { r["a"], r["b"] },
                    (r, i) => new[] { r["b"].Assign( r["b"] + i["b"] ), r["c"].Assign( i["b"] + SqlNode.Literal( 1 ) ) } ) );

            sut.Context.Sql.ToString()
                .Should()
                .Be(
                    @"WITH ""cba"" AS (
  SELECT * FROM abc
)
INSERT INTO ""qux"" (""a"", ""b"")
SELECT DISTINCT
  ""common"".""foo"".""b"" AS ""a"",
  (COUNT(*) OVER ""wnd"") AS ""b""
FROM ""common"".""foo""
INNER JOIN ""common"".""bar"" ON ""common"".""bar"".""c"" = ""common"".""foo"".""a""
WHERE ""common"".""bar"".""c"" IN (
  SELECT cba.c FROM cba
)
GROUP BY ""common"".""foo"".""b""
HAVING ""common"".""foo"".""b"" < 100
WINDOW ""wnd"" AS (ORDER BY ""common"".""foo"".""a"" ASC)
ORDER BY ""common"".""foo"".""b"" ASC
LIMIT 50
OFFSET 100
ON CONFLICT (""a"", ""c"") DO UPDATE SET
  ""b"" = (""qux"".""b"" + EXCLUDED.""b""),
  ""c"" = (EXCLUDED.""b"" + 1)" );
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
                    @"WITH ""x"" AS (
  SELECT * FROM ipsum
)
INSERT INTO qux (""a"", ""b"")
SELECT foo.a, foo.b FROM foo JOIN x ON x.a = foo.a
UNION ALL
SELECT a, b FROM bar
UNION
SELECT a, b FROM qux
ORDER BY (a) ASC
LIMIT 50
OFFSET 75
ON CONFLICT (""a"", ""c"") DO UPDATE SET
  ""b"" = (qux.""b"" + EXCLUDED.""b""),
  ""c"" = (EXCLUDED.""b"" + 1)" );
        }

        [Fact]
        public void Visit_ShouldInterpretUpsertWithCompoundQuery_WithoutConflictTargetAndTableWithPrimaryKey()
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
                    @"WITH ""x"" AS (
  SELECT * FROM ipsum
)
INSERT INTO ""common"".""qux"" (""a"", ""b"")
SELECT foo.a, foo.b FROM foo JOIN x ON x.a = foo.a
UNION ALL
SELECT a, b FROM bar
UNION
SELECT a, b FROM qux
ORDER BY (a) ASC
LIMIT 50
OFFSET 75
ON CONFLICT (""a"", ""c"") DO UPDATE SET
  ""b"" = (""common"".""qux"".""b"" + EXCLUDED.""b""),
  ""c"" = (EXCLUDED.""b"" + 1)" );
        }

        [Fact]
        public void Visit_ShouldInterpretUpsertWithCompoundQuery_WithoutConflictTargetAndTableBuilderWithPrimaryKey()
        {
            var sut = CreateInterpreter();
            var table = SqlTableBuilderMock.Create<int>( "qux", new[] { "a", "b", "c" }, new[] { "a", "c" } );
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
                    @"WITH ""x"" AS (
  SELECT * FROM ipsum
)
INSERT INTO ""common"".""qux"" (""a"", ""b"")
SELECT foo.a, foo.b FROM foo JOIN x ON x.a = foo.a
UNION ALL
SELECT a, b FROM bar
UNION
SELECT a, b FROM qux
ORDER BY (a) ASC
LIMIT 50
OFFSET 75
ON CONFLICT (""a"", ""c"") DO UPDATE SET
  ""b"" = (""common"".""qux"".""b"" + EXCLUDED.""b""),
  ""c"" = (EXCLUDED.""b"" + 1)" );
        }

        [Fact]
        public void Visit_ShouldInterpretUpsertWithCompoundQuery_WithoutConflictTargetAndNewTableWithPrimaryKey()
        {
            var sut = CreateInterpreter();
            var table = SqlNode.CreateTable(
                SqlRecordSetInfo.Create( "qux" ),
                new[] { SqlNode.Column<int>( "a" ), SqlNode.Column<int>( "b" ), SqlNode.Column<int>( "c" ) },
                constraintsProvider: r => SqlCreateTableConstraints.Empty.WithPrimaryKey(
                    SqlNode.PrimaryKey( SqlSchemaObjectName.Create( "PK" ), new[] { r["a"].Asc(), r["c"].Asc() } ) ) );

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
                    table.RecordSet,
                    r => new[] { r["a"], r["b"] },
                    (r, i) => new[] { r["b"].Assign( r["b"] + i["b"] ), r["c"].Assign( i["b"] + SqlNode.Literal( 1 ) ) } ) );

            sut.Context.Sql.ToString()
                .Should()
                .Be(
                    @"WITH ""x"" AS (
  SELECT * FROM ipsum
)
INSERT INTO ""qux"" (""a"", ""b"")
SELECT foo.a, foo.b FROM foo JOIN x ON x.a = foo.a
UNION ALL
SELECT a, b FROM bar
UNION
SELECT a, b FROM qux
ORDER BY (a) ASC
LIMIT 50
OFFSET 75
ON CONFLICT (""a"", ""c"") DO UPDATE SET
  ""b"" = (""qux"".""b"" + EXCLUDED.""b""),
  ""c"" = (EXCLUDED.""b"" + 1)" );
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
                    @"INSERT INTO qux (""a"", ""b"")
SELECT * FROM bar
ON CONFLICT (""a"", ""c"") DO UPDATE SET
  ""b"" = (qux.""b"" + EXCLUDED.""b""),
  ""c"" = (EXCLUDED.""b"" + 1)" );
        }

        [Fact]
        public void Visit_ShouldInterpretUpsertWithRawQuery_WithoutConflictTargetAndTableWithPrimaryKey()
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
                    @"INSERT INTO ""common"".""qux"" (""a"", ""b"")
SELECT * FROM bar
ON CONFLICT (""a"", ""c"") DO UPDATE SET
  ""b"" = (""common"".""qux"".""b"" + EXCLUDED.""b""),
  ""c"" = (EXCLUDED.""b"" + 1)" );
        }

        [Fact]
        public void Visit_ShouldInterpretUpsertWithRawQuery_WithoutConflictTargetAndTableBuilderWithPrimaryKey()
        {
            var sut = CreateInterpreter();
            var table = SqlTableBuilderMock.Create<int>( "qux", new[] { "a", "b", "c" }, new[] { "a", "c" } );
            sut.Visit(
                SqlNode.RawQuery( "SELECT * FROM bar" )
                    .ToUpsert(
                        table.Node,
                        r => new[] { r["a"], r["b"] },
                        (r, i) => new[] { r["b"].Assign( r["b"] + i["b"] ), r["c"].Assign( i["b"] + SqlNode.Literal( 1 ) ) } ) );

            sut.Context.Sql.ToString()
                .Should()
                .Be(
                    @"INSERT INTO ""common"".""qux"" (""a"", ""b"")
SELECT * FROM bar
ON CONFLICT (""a"", ""c"") DO UPDATE SET
  ""b"" = (""common"".""qux"".""b"" + EXCLUDED.""b""),
  ""c"" = (EXCLUDED.""b"" + 1)" );
        }

        [Fact]
        public void Visit_ShouldInterpretUpsertWithRawQuery_WithoutConflictTargetAndNewTableWithPrimaryKey()
        {
            var sut = CreateInterpreter();
            var table = SqlNode.CreateTable(
                SqlRecordSetInfo.Create( "qux" ),
                new[] { SqlNode.Column<int>( "a" ), SqlNode.Column<int>( "b" ), SqlNode.Column<int>( "c" ) },
                constraintsProvider: r => SqlCreateTableConstraints.Empty.WithPrimaryKey(
                    SqlNode.PrimaryKey( SqlSchemaObjectName.Create( "PK" ), new[] { r["a"].Asc(), r["c"].Asc() } ) ) );

            sut.Visit(
                SqlNode.RawQuery( "SELECT * FROM bar" )
                    .ToUpsert(
                        table.RecordSet,
                        r => new[] { r["a"], r["b"] },
                        (r, i) => new[] { r["b"].Assign( r["b"] + i["b"] ), r["c"].Assign( i["b"] + SqlNode.Literal( 1 ) ) } ) );

            sut.Context.Sql.ToString()
                .Should()
                .Be(
                    @"INSERT INTO ""qux"" (""a"", ""b"")
SELECT * FROM bar
ON CONFLICT (""a"", ""c"") DO UPDATE SET
  ""b"" = (""qux"".""b"" + EXCLUDED.""b""),
  ""c"" = (EXCLUDED.""b"" + 1)" );
        }

        [Fact]
        public void Visit_ShouldThrowSqlNodeVisitorException_WithoutConflictTargetAndTargetWithoutIdentityColumns()
        {
            var sut = CreateInterpreter();
            var node = SqlNode.Values(
                    new[,]
                    {
                        { SqlNode.Literal( "foo" ), SqlNode.Literal( 5 ) },
                        { SqlNode.RawExpression( "bar.a" ), SqlNode.Literal( 25 ) }
                    } )
                .ToUpsert(
                    SqlTableBuilderMock.CreateEmpty( "qux" ).Node,
                    Array.Empty<SqlDataFieldNode>(),
                    (_, _) => Array.Empty<SqlValueAssignmentNode>() );

            var action = Lambda.Of( () => sut.Visit( node ) );

            action.Should().ThrowExactly<SqlNodeVisitorException>();
        }

        [Fact]
        public void Visit_ShouldThrowSqlNodeVisitorException_WithoutConflictTargetAndInvalidRecordSet()
        {
            var sut = CreateInterpreter();
            var node = SqlNode.Values(
                    new[,]
                    {
                        { SqlNode.Literal( "foo" ), SqlNode.Literal( 5 ) },
                        { SqlNode.RawExpression( "bar.a" ), SqlNode.Literal( 25 ) }
                    } )
                .ToUpsert(
                    SqlNode.RawRecordSet( "qux" ),
                    r => new[] { r["a"], r["b"] },
                    (r, i) => new[] { r["b"].Assign( r["b"] + i["b"] ), r["c"].Assign( i["b"] + SqlNode.Literal( 1 ) ) } );

            var action = Lambda.Of( () => sut.Visit( node ) );

            action.Should().ThrowExactly<SqlNodeVisitorException>();
        }
    }
}
