using System.Data;
using System.Threading;
using System.Threading.Tasks;
using LfrlAnvil.Sql.Statements;
using LfrlAnvil.TestExtensions.FluentAssertions;
using LfrlAnvil.TestExtensions.NSubstitute;
using LfrlAnvil.TestExtensions.Sql.Mocks.System;

namespace LfrlAnvil.Sql.Tests.StatementsTests;

public class SqlAsyncScalarReaderTests : TestsBase
{
    [Fact]
    public async Task ReadAsync_ForTypeErased_ShouldInvokeDelegate()
    {
        var expected = new SqlScalarResult( "foo" );

        var reader = new DbDataReaderMock();
        var cancellationTokenSource = new CancellationTokenSource();
        var dialect = new SqlDialect( "foo" );
        var @delegate = Substitute.For<Func<IDataReader, CancellationToken, ValueTask<SqlScalarResult>>>();
        @delegate.WithAnyArgs( _ => ValueTask.FromResult( expected ) );
        var sut = new SqlAsyncScalarReader( dialect, @delegate );

        var result = await sut.ReadAsync( reader, cancellationTokenSource.Token );

        using ( new AssertionScope() )
        {
            sut.Dialect.Should().BeSameAs( dialect );
            sut.Delegate.Should().BeSameAs( @delegate );
            @delegate.Verify()
                .CallAt( 0 )
                .Exists()
                .And.Arguments.Should()
                .BeSequentiallyEqualTo( reader, cancellationTokenSource.Token );

            result.Should().BeEquivalentTo( expected );
        }
    }

    [Fact]
    public async Task ReadAsync_ForGeneric_ShouldInvokeDelegate()
    {
        var expected = new SqlScalarResult<int>( 42 );

        var reader = new DbDataReaderMock();
        var cancellationTokenSource = new CancellationTokenSource();
        var dialect = new SqlDialect( "foo" );
        var @delegate = Substitute.For<Func<IDataReader, CancellationToken, ValueTask<SqlScalarResult<int>>>>();
        @delegate.WithAnyArgs( _ => ValueTask.FromResult( expected ) );
        var sut = new SqlAsyncScalarReader<int>( dialect, @delegate );

        var result = await sut.ReadAsync( reader, cancellationTokenSource.Token );

        using ( new AssertionScope() )
        {
            sut.Dialect.Should().BeSameAs( dialect );
            sut.Delegate.Should().BeSameAs( @delegate );
            @delegate.Verify()
                .CallAt( 0 )
                .Exists()
                .And.Arguments.Should()
                .BeSequentiallyEqualTo( reader, cancellationTokenSource.Token );

            result.Should().BeEquivalentTo( expected );
        }
    }
}
