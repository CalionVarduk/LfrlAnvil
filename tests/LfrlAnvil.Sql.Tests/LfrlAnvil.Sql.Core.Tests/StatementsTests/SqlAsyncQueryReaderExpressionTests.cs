using System.Linq;
using System.Threading.Tasks;
using LfrlAnvil.Functional;
using LfrlAnvil.Sql.Statements;
using LfrlAnvil.Sql.Statements.Compilers;
using LfrlAnvil.TestExtensions.Sql.Mocks.System;

namespace LfrlAnvil.Sql.Tests.StatementsTests;

public class SqlAsyncQueryReaderExpressionTests : TestsBase
{
    [Fact]
    public async Task Compile_ShouldCreateCorrectAsyncQueryReader()
    {
        var reader = new DbDataReaderMock(
            new ResultSet( new[] { "a", "b" }, new[] { new object[] { "foo", 3 }, new object[] { "lorem", 5 } } ) );

        var dialect = new SqlDialect( "foo" );
        var rowType = typeof( object[] );

        var initExpression = Lambda.ExpressionOf( (DbDataReaderMock r) =>
            new SqlAsyncQueryReaderInitResult( new[] { r.GetOrdinal( "a" ), r.GetOrdinal( "b" ) }, null ) );

        var createRowExpression = Lambda.ExpressionOf( (DbDataReaderMock r, int[] o) => o.Select( r.GetValue ).ToArray() );
        var expression = SqlAsyncQueryLambdaExpression<DbDataReaderMock, object[]>.Create( initExpression, createRowExpression );
        var @base = new SqlAsyncQueryReaderExpression( dialect, rowType, expression );
        var sut = new SqlAsyncQueryReaderExpression<object[]>( @base );

        var queryReader = sut.Compile();
        var result = await queryReader.ReadAsync( reader, new SqlQueryReaderOptions() );

        Assertion.All(
                sut.Dialect.TestRefEquals( dialect ),
                sut.Expression.TestRefEquals( expression ),
                queryReader.Dialect.TestRefEquals( dialect ),
                result.IsEmpty.TestFalse(),
                result.ResultSetFields.ToArray().TestEmpty(),
                result.Rows.TestNotNull( rows => rows.TestSequence(
                [
                    (r, _) => r.TestSequence( [ "foo", 3 ] ),
                    (r, _) => r.TestSequence( [ "lorem", 5 ] )
                ] ) ) )
            .Go();
    }
}
