using System.Data;
using System.Threading;
using System.Threading.Tasks;
using LfrlAnvil.Sql.Statements;
using LfrlAnvil.TestExtensions.NSubstitute;
using LfrlAnvil.TestExtensions.Sql.Mocks.System;

namespace LfrlAnvil.Sql.Tests.StatementsTests;

public class SqlAsyncScalarQueryReaderExecutorTests : TestsBase
{
    [Fact]
    public void Bind_Extension_ForTypeErased_ShouldCreateCorrectExecutor()
    {
        var sql = "SELECT * FROM foo";
        var @delegate = Substitute.For<Func<IDataReader, CancellationToken, ValueTask<SqlScalarQueryResult>>>();
        var reader = new SqlAsyncScalarQueryReader( new SqlDialect( "foo" ), @delegate );
        var sut = reader.Bind( sql );

        Assertion.All(
                sut.Sql.TestRefEquals( sql ),
                sut.Reader.TestEquals( reader ) )
            .Go();
    }

    [Fact]
    public async Task ExecuteAsync_ForTypeErased_ShouldSetCommandTextAndInvokeDelegate()
    {
        var expected = new SqlScalarQueryResult( "bar" );

        var sql = "SELECT * FROM foo";
        var command = new DbCommandMock();
        var @delegate = Substitute.For<Func<IDataReader, CancellationToken, ValueTask<SqlScalarQueryResult>>>();
        @delegate.WithAnyArgs( _ => ValueTask.FromResult( expected ) );
        var reader = new SqlAsyncScalarQueryReader( new SqlDialect( "foo" ), @delegate );
        var sut = reader.Bind( sql );

        var result = await sut.ExecuteAsync( command );

        Assertion.All(
                @delegate.CallCount().TestEquals( 1 ),
                command.CommandText.TestRefEquals( sql ),
                result.TestEquals( expected ) )
            .Go();
    }

    [Fact]
    public void Bind_Extension_ForGeneric_ShouldCreateCorrectExecutor()
    {
        var sql = "SELECT * FROM foo";
        var @delegate = Substitute.For<Func<IDataReader, CancellationToken, ValueTask<SqlScalarQueryResult<int>>>>();
        var reader = new SqlAsyncScalarQueryReader<int>( new SqlDialect( "foo" ), @delegate );
        var sut = reader.Bind( sql );

        Assertion.All(
                sut.Sql.TestRefEquals( sql ),
                sut.Reader.TestEquals( reader ) )
            .Go();
    }

    [Fact]
    public async Task ExecuteAsync_ForGeneric_ShouldSetCommandTextAndInvokeDelegate()
    {
        var expected = new SqlScalarQueryResult<int>( 42 );

        var sql = "SELECT * FROM foo";
        var command = new DbCommandMock();
        var @delegate = Substitute.For<Func<IDataReader, CancellationToken, ValueTask<SqlScalarQueryResult<int>>>>();
        @delegate.WithAnyArgs( _ => ValueTask.FromResult( expected ) );
        var reader = new SqlAsyncScalarQueryReader<int>( new SqlDialect( "foo" ), @delegate );
        var sut = reader.Bind( sql );

        var result = await sut.ExecuteAsync( command );

        Assertion.All(
                @delegate.CallCount().TestEquals( 1 ),
                command.CommandText.TestRefEquals( sql ),
                result.TestEquals( expected ) )
            .Go();
    }
}
