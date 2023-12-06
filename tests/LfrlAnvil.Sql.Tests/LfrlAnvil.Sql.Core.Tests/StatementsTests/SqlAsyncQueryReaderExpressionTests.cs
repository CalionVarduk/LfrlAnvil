using System.Linq;
using System.Threading.Tasks;
using LfrlAnvil.Functional;
using LfrlAnvil.Sql.Statements;
using LfrlAnvil.Sql.Statements.Compilers;
using LfrlAnvil.Sql.Tests.Helpers.Data;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.Sql.Tests.StatementsTests;

public class SqlAsyncQueryReaderExpressionTests : TestsBase
{
    [Fact]
    public async Task Compile_ShouldCreateCorrectAsyncQueryReader()
    {
        var reader = new DbDataReader(
            new ResultSet( new[] { "a", "b" }, new[] { new object[] { "foo", 3 }, new object[] { "lorem", 5 } } ) );

        var dialect = new SqlDialect( "foo" );
        var rowType = typeof( object[] );

        var initExpression = Lambda.ExpressionOf(
            (DbDataReader r) => new SqlAsyncReaderInitResult( new[] { r.GetOrdinal( "a" ), r.GetOrdinal( "b" ) }, null ) );

        var createRowExpression = Lambda.ExpressionOf( (DbDataReader r, int[] o) => o.Select( r.GetValue ).ToArray() );
        var expression = SqlAsyncLambdaExpression<DbDataReader, object[]>.Create( initExpression, createRowExpression );
        var @base = new SqlAsyncQueryReaderExpression( dialect, rowType, expression );
        var sut = new SqlAsyncQueryReaderExpression<object[]>( @base );

        var queryReader = sut.Compile();
        var result = await queryReader.ReadAsync( reader, new SqlQueryReaderOptions() );

        using ( new AssertionScope() )
        {
            sut.Dialect.Should().BeSameAs( dialect );
            sut.Expression.Should().BeSameAs( expression );
            queryReader.Dialect.Should().BeSameAs( dialect );
            result.IsEmpty.Should().BeFalse();
            result.ResultSetFields.ToArray().Should().BeEmpty();
            result.Rows.Should().HaveCount( 2 );
            (result.Rows?.ElementAtOrDefault( 0 )).Should().BeSequentiallyEqualTo( "foo", 3 );
            (result.Rows?.ElementAtOrDefault( 1 )).Should().BeSequentiallyEqualTo( "lorem", 5 );
        }
    }
}
