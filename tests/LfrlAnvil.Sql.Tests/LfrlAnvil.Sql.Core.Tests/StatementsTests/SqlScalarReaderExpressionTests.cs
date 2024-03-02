using System.Data;
using LfrlAnvil.Functional;
using LfrlAnvil.Sql.Statements;
using LfrlAnvil.TestExtensions.Sql.Mocks.System;

namespace LfrlAnvil.Sql.Tests.StatementsTests;

public class SqlScalarReaderExpressionTests : TestsBase
{
    [Fact]
    public void Compile_ShouldCreateCorrectQueryReader()
    {
        var expected = new SqlScalarResult<string>( "foo" );

        var reader = new DbDataReaderMock();
        var dialect = new SqlDialect( "foo" );
        var resultType = typeof( string );

        var expression = Lambda.ExpressionOf( (IDataReader r) => expected );
        var @base = new SqlScalarReaderExpression( dialect, resultType, expression );
        var sut = new SqlScalarReaderExpression<string>( @base );

        var queryReader = sut.Compile();
        var result = queryReader.Read( reader );

        using ( new AssertionScope() )
        {
            sut.Dialect.Should().BeSameAs( dialect );
            sut.Expression.Should().BeSameAs( expression );
            queryReader.Dialect.Should().BeSameAs( dialect );
            result.Should().BeEquivalentTo( expected );
        }
    }
}
