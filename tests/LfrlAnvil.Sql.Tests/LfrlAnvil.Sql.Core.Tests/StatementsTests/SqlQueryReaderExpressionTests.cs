using System.Collections.Generic;
using System.Data;
using LfrlAnvil.Functional;
using LfrlAnvil.Sql.Statements;
using LfrlAnvil.TestExtensions.Sql.Mocks.System;

namespace LfrlAnvil.Sql.Tests.StatementsTests;

public class SqlQueryReaderExpressionTests : TestsBase
{
    [Fact]
    public void Compile_ShouldCreateCorrectQueryReader()
    {
        var expected = new SqlQueryReaderResult<object[]>(
            new[] { new SqlResultSetField( 0, "a" ), new SqlResultSetField( 1, "b" ) },
            new List<object[]>
            {
                new object[] { "foo", 3 },
                new object[] { "lorem", 5 }
            } );

        var reader = new DbDataReaderMock();
        var dialect = new SqlDialect( "foo" );
        var rowType = typeof( object[] );

        var expression = Lambda.ExpressionOf( (IDataReader r, SqlQueryReaderOptions o) => expected );
        var @base = new SqlQueryReaderExpression( dialect, rowType, expression );
        var sut = new SqlQueryReaderExpression<object[]>( @base );

        var queryReader = sut.Compile();
        var result = queryReader.Read( reader, new SqlQueryReaderOptions() );

        using ( new AssertionScope() )
        {
            sut.Dialect.Should().BeSameAs( dialect );
            sut.Expression.Should().BeSameAs( expression );
            queryReader.Dialect.Should().BeSameAs( dialect );
            result.Should().BeEquivalentTo( expected );
        }
    }
}
