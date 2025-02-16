using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using LfrlAnvil.Sql.Statements;
using LfrlAnvil.TestExtensions.NSubstitute;
using LfrlAnvil.TestExtensions.Sql.Mocks.System;

namespace LfrlAnvil.Sql.Tests.StatementsTests;

public class SqlAsyncQueryReaderTests : TestsBase
{
    [Fact]
    public async Task ReadAsync_ForTypeErased_ShouldInvokeDelegate()
    {
        var expected = new SqlQueryResult(
            new[] { new SqlResultSetField( 0, "a" ), new SqlResultSetField( 1, "b" ) },
            new List<object?>
            {
                "foo",
                3,
                "lorem",
                5
            } );

        var reader = new DbDataReaderMock();
        var options = new SqlQueryReaderOptions();
        var cancellationTokenSource = new CancellationTokenSource();
        var dialect = new SqlDialect( "foo" );
        var @delegate = Substitute.For<Func<IDataReader, SqlQueryReaderOptions, CancellationToken, ValueTask<SqlQueryResult>>>();
        @delegate.WithAnyArgs( _ => ValueTask.FromResult( expected ) );
        var sut = new SqlAsyncQueryReader( dialect, @delegate );

        var result = await sut.ReadAsync( reader, options, cancellationTokenSource.Token );

        Assertion.All(
                sut.Dialect.TestRefEquals( dialect ),
                sut.Delegate.TestRefEquals( @delegate ),
                @delegate.CallAt( 0 ).Arguments.TestSequence( [ reader, options, cancellationTokenSource.Token ] ),
                result.TestEquals( expected ) )
            .Go();
    }

    [Fact]
    public async Task ReadAsync_ForGeneric_ShouldInvokeDelegate()
    {
        var expected = new SqlQueryResult<object[]>(
            new[] { new SqlResultSetField( 0, "a" ), new SqlResultSetField( 1, "b" ) },
            new List<object[]>
            {
                new object[] { "foo", 3 },
                new object[] { "lorem", 5 }
            } );

        var reader = new DbDataReaderMock();
        var options = new SqlQueryReaderOptions();
        var cancellationTokenSource = new CancellationTokenSource();
        var dialect = new SqlDialect( "foo" );
        var @delegate = Substitute.For<Func<IDataReader, SqlQueryReaderOptions, CancellationToken, ValueTask<SqlQueryResult<object[]>>>>();

        @delegate.WithAnyArgs( _ => ValueTask.FromResult( expected ) );
        var sut = new SqlAsyncQueryReader<object[]>( dialect, @delegate );

        var result = await sut.ReadAsync( reader, options, cancellationTokenSource.Token );

        Assertion.All(
                sut.Dialect.TestRefEquals( dialect ),
                sut.Delegate.TestRefEquals( @delegate ),
                @delegate.CallAt( 0 ).Arguments.TestSequence( [ reader, options, cancellationTokenSource.Token ] ),
                result.TestEquals( expected ) )
            .Go();
    }
}
