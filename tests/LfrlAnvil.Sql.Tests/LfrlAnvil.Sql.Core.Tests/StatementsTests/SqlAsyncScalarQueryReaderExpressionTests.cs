using System.Threading.Tasks;
using LfrlAnvil.Functional;
using LfrlAnvil.Sql.Statements;
using LfrlAnvil.Sql.Statements.Compilers;
using LfrlAnvil.TestExtensions.Sql.Mocks.System;

namespace LfrlAnvil.Sql.Tests.StatementsTests;

public class SqlAsyncScalarQueryReaderExpressionTests : TestsBase
{
    [Fact]
    public async Task Compile_ShouldCreateCorrectAsyncScalarQueryReader()
    {
        var reader = new DbDataReaderMock(
            new ResultSet( new[] { "a", "b" }, new[] { new object[] { "foo", 3 }, new object[] { "lorem", 5 } } ) );

        var dialect = new SqlDialect( "foo" );
        var resultType = typeof( string );

        var readResultExpression =
            Lambda.ExpressionOf( (DbDataReaderMock r) => new SqlScalarQueryResult<string>( (string)r.GetValue( 0 ) ) );

        var expression = SqlAsyncScalarQueryLambdaExpression<DbDataReaderMock, string>.Create( readResultExpression );
        var @base = new SqlAsyncScalarQueryReaderExpression( dialect, resultType, expression );
        var sut = new SqlAsyncScalarQueryReaderExpression<string>( @base );

        var scalarReader = sut.Compile();
        var result = await scalarReader.ReadAsync( reader );

        using ( new AssertionScope() )
        {
            sut.Dialect.Should().BeSameAs( dialect );
            sut.Expression.Should().BeSameAs( expression );
            scalarReader.Dialect.Should().BeSameAs( dialect );
            result.HasValue.Should().BeTrue();
            result.Value.Should().Be( "foo" );
        }
    }
}
