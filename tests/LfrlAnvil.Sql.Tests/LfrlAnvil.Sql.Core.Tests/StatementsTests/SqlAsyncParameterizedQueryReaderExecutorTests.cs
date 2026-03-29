using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using LfrlAnvil.Sql.Statements;
using LfrlAnvil.TestExtensions.NSubstitute;
using LfrlAnvil.TestExtensions.Sql.Mocks.System;

namespace LfrlAnvil.Sql.Tests.StatementsTests;

public class SqlAsyncParameterizedQueryReaderExecutorTests : TestsBase
{
    [Fact]
    public async Task ExecuteAsync_TypeErased_ShouldBindParametersAndExecuteReader()
    {
        var command = new DbCommandMock();
        var dialect = new SqlDialect( "foo" );
        var binderDelegate = Substitute.For<Action<IDbCommand, IEnumerable<SqlParameter>>>();
        var parameters = new[] { SqlParameter.Named( "a", 0 ), SqlParameter.Named( "b", 1 ) };

        var expected = new SqlQueryResult(
            [ new SqlResultSetField( 0, "a" ), new SqlResultSetField( 1, "b" ) ],
            [ "foo", 3, "lorem", 5 ] );

        var sql = "SELECT * FROM foo";
        var readerDelegate = Substitute.For<Func<IDataReader, SqlQueryReaderOptions, CancellationToken, ValueTask<SqlQueryResult>>>();
        readerDelegate.WithAnyArgs( _ => ValueTask.FromResult( expected ) );
        var reader = new SqlAsyncQueryReader( new SqlDialect( "foo" ), readerDelegate );
        var sut = reader.BindStatement( sql ).Parameterize( new SqlParameterBinder( dialect, binderDelegate ) );

        var result = await sut.ExecuteAsync( command, parameters );

        Assertion.All(
                binderDelegate.CallAt( 0 ).Arguments.TestSequence( [ command, parameters ] ),
                readerDelegate.CallCount().TestEquals( 1 ),
                command.CommandText.TestRefEquals( sql ),
                result.TestEquals( expected ) )
            .Go();
    }

    [Fact]
    public async Task ExecuteAsync_Generic_ShouldBindParametersAndExecuteReader()
    {
        var command = new DbCommandMock();
        var dialect = new SqlDialect( "foo" );
        var binderDelegate = Substitute.For<Action<IDbCommand, string>>();

        var expected = new SqlQueryResult<object[]>( null, [ [ 1, "foo" ], [ 2, "bar" ] ] );

        var sql = "SELECT * FROM foo";
        var readerDelegate
            = Substitute.For<Func<IDataReader, SqlQueryReaderOptions, CancellationToken, ValueTask<SqlQueryResult<object[]>>>>();

        readerDelegate.WithAnyArgs( _ => ValueTask.FromResult( expected ) );
        var reader = new SqlAsyncQueryReader<object[]>( new SqlDialect( "foo" ), readerDelegate );
        var sut = reader.BindStatement( sql ).Parameterize( new SqlParameterBinder<string>( dialect, binderDelegate ) );

        var result = await sut.ExecuteAsync( command, "param" );

        Assertion.All(
                binderDelegate.CallAt( 0 ).Arguments.TestSequence( [ command, "param" ] ),
                readerDelegate.CallCount().TestEquals( 1 ),
                command.CommandText.TestRefEquals( sql ),
                result.TestEquals( expected ) )
            .Go();
    }
}

public class SqlAsyncParameterizedScalarQueryReaderExecutorTests : TestsBase
{
    [Fact]
    public async Task ExecuteAsync_TypeErased_ShouldBindParametersAndExecuteReader()
    {
        var command = new DbCommandMock();
        var dialect = new SqlDialect( "foo" );
        var binderDelegate = Substitute.For<Action<IDbCommand, IEnumerable<SqlParameter>>>();
        var parameters = new[] { SqlParameter.Named( "a", 0 ), SqlParameter.Named( "b", 1 ) };

        var expected = new SqlScalarQueryResult( "foo" );
        var sql = "SELECT * FROM foo";
        var readerDelegate = Substitute.For<Func<IDataReader, CancellationToken, ValueTask<SqlScalarQueryResult>>>();
        readerDelegate.WithAnyArgs( _ => ValueTask.FromResult( expected ) );
        var reader = new SqlAsyncScalarQueryReader( new SqlDialect( "foo" ), readerDelegate );
        var sut = reader.BindStatement( sql ).Parameterize( new SqlParameterBinder( dialect, binderDelegate ) );

        var result = await sut.ExecuteAsync( command, parameters );

        Assertion.All(
                binderDelegate.CallAt( 0 ).Arguments.TestSequence( [ command, parameters ] ),
                readerDelegate.CallCount().TestEquals( 1 ),
                command.CommandText.TestRefEquals( sql ),
                result.TestEquals( expected ) )
            .Go();
    }

    [Fact]
    public async Task ExecuteAsync_Generic_ShouldBindParametersAndExecuteReader()
    {
        var command = new DbCommandMock();
        var dialect = new SqlDialect( "foo" );
        var binderDelegate = Substitute.For<Action<IDbCommand, string>>();

        var expected = new SqlScalarQueryResult<object[]>( [ 1, "foo" ] );
        var sql = "SELECT * FROM foo";
        var readerDelegate = Substitute.For<Func<IDataReader, CancellationToken, ValueTask<SqlScalarQueryResult<object[]>>>>();
        readerDelegate.WithAnyArgs( _ => ValueTask.FromResult( expected ) );
        var reader = new SqlAsyncScalarQueryReader<object[]>( new SqlDialect( "foo" ), readerDelegate );
        var sut = reader.BindStatement( sql ).Parameterize( new SqlParameterBinder<string>( dialect, binderDelegate ) );

        var result = await sut.ExecuteAsync( command, "param" );

        Assertion.All(
                binderDelegate.CallAt( 0 ).Arguments.TestSequence( [ command, "param" ] ),
                readerDelegate.CallCount().TestEquals( 1 ),
                command.CommandText.TestRefEquals( sql ),
                result.TestEquals( expected ) )
            .Go();
    }
}
