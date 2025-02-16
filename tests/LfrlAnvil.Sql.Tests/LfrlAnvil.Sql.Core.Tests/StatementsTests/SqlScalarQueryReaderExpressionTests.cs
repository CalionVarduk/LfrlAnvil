using System.Data;
using LfrlAnvil.Functional;
using LfrlAnvil.Sql.Statements;
using LfrlAnvil.TestExtensions.Sql.Mocks.System;

namespace LfrlAnvil.Sql.Tests.StatementsTests;

public class SqlScalarQueryReaderExpressionTests : TestsBase
{
    [Fact]
    public void Compile_ShouldCreateCorrectScalarQueryReader()
    {
        var expected = new SqlScalarQueryResult<string>( "foo" );

        var reader = new DbDataReaderMock();
        var dialect = new SqlDialect( "foo" );
        var resultType = typeof( string );

        var expression = Lambda.ExpressionOf( (IDataReader r) => expected );
        var @base = new SqlScalarQueryReaderExpression( dialect, resultType, expression );
        var sut = new SqlScalarQueryReaderExpression<string>( @base );

        var queryReader = sut.Compile();
        var result = queryReader.Read( reader );

        Assertion.All(
                sut.Dialect.TestRefEquals( dialect ),
                sut.Expression.TestRefEquals( expression ),
                queryReader.Dialect.TestRefEquals( dialect ),
                result.TestEquals( expected ) )
            .Go();
    }
}
