using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using LfrlAnvil.Sql.Statements;
using LfrlAnvil.Sql.Tests.Helpers.Data;
using LfrlAnvil.TestExtensions.FluentAssertions;
using LfrlAnvil.TestExtensions.NSubstitute;

namespace LfrlAnvil.Sql.Tests.StatementsTests;

public class SqlAsyncQueryReaderTests : TestsBase
{
    [Fact]
    public async Task ReadAsync_ForTypeErased_ShouldInvokeDelegate()
    {
        var expected = new SqlQueryReaderResult(
            new[] { new SqlResultSetField( 0, "a" ), new SqlResultSetField( 1, "b" ) },
            new List<object?> { "foo", 3, "lorem", 5 } );

        var reader = new DbDataReaderMock();
        var options = new SqlQueryReaderOptions();
        var cancellationTokenSource = new CancellationTokenSource();
        var dialect = new SqlDialect( "foo" );
        var @delegate = Substitute.For<Func<IDataReader, SqlQueryReaderOptions, CancellationToken, ValueTask<SqlQueryReaderResult>>>();
        @delegate.WithAnyArgs( _ => ValueTask.FromResult( expected ) );
        var sut = new SqlAsyncQueryReader( dialect, @delegate );

        var result = await sut.ReadAsync( reader, options, cancellationTokenSource.Token );

        using ( new AssertionScope() )
        {
            sut.Dialect.Should().BeSameAs( dialect );
            sut.Delegate.Should().BeSameAs( @delegate );
            @delegate.Verify()
                .CallAt( 0 )
                .Exists()
                .And.Arguments.Should()
                .BeSequentiallyEqualTo( reader, options, cancellationTokenSource.Token );

            result.Should().BeEquivalentTo( expected );
        }
    }

    [Fact]
    public async Task ReadAsync_ForGeneric_ShouldInvokeDelegate()
    {
        var expected = new SqlQueryReaderResult<object[]>(
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
        var @delegate = Substitute
            .For<Func<IDataReader, SqlQueryReaderOptions, CancellationToken, ValueTask<SqlQueryReaderResult<object[]>>>>();

        @delegate.WithAnyArgs( _ => ValueTask.FromResult( expected ) );
        var sut = new SqlAsyncQueryReader<object[]>( dialect, @delegate );

        var result = await sut.ReadAsync( reader, options, cancellationTokenSource.Token );

        using ( new AssertionScope() )
        {
            sut.Dialect.Should().BeSameAs( dialect );
            sut.Delegate.Should().BeSameAs( @delegate );
            @delegate.Verify()
                .CallAt( 0 )
                .Exists()
                .And.Arguments.Should()
                .BeSequentiallyEqualTo( reader, options, cancellationTokenSource.Token );

            result.Should().BeEquivalentTo( expected );
        }
    }
}
