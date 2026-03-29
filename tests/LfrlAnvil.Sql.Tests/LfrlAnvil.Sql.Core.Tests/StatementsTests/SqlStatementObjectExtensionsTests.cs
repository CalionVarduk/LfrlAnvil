using System.Data;
using System.Linq;
using System.Threading.Tasks;
using LfrlAnvil.Sql.Statements;
using LfrlAnvil.Sql.Statements.Compilers;
using LfrlAnvil.TestExtensions.Sql.Mocks;
using LfrlAnvil.TestExtensions.Sql.Mocks.System;

namespace LfrlAnvil.Sql.Tests.StatementsTests;

public class SqlStatementObjectExtensionsTests : TestsBase
{
    [Fact]
    public async Task BeginTransactionAsync_ShouldReturnTransaction()
    {
        var sut = new DbConnectionMock();
        var result = await (( IDbConnection )sut).BeginTransactionAsync( IsolationLevel.Serializable );
        result.TestRefEquals( sut.CreatedTransactions[0] ).Go();
    }

    [Fact]
    public void CreateCommand_ForInterface_ShouldReturnCommandWithTransaction()
    {
        IDbTransaction sut = new DbConnectionMock().BeginTransaction();
        var result = sut.CreateCommand();
        result.Transaction.TestRefEquals( sut ).Go();
    }

    [Fact]
    public void CreateCommand_ShouldReturnCommandWithTransaction()
    {
        var sut = new DbConnectionMock().BeginTransaction();
        var result = sut.CreateCommand();
        result.Transaction.TestRefEquals( sut ).Go();
    }

    [Fact]
    public void Query_TypeErased_WithReader_ShouldInvokeReader()
    {
        var command = new DbCommandMock(
            new ResultSet( new[] { "A", "B" }, new[] { new object[] { 1, "foo" }, new object[] { 2, "bar" } } ) );

        var factory = SqlQueryReaderFactoryMock.CreateInstance();
        var result = command.Query( factory.Create() );

        Assertion.All(
                command.Audit.LastOrDefault().TestEquals( "DbDataReader[0].Close" ),
                result.Rows.TestNotNull( rows => rows.TestSequence(
                [
                    (r, _) => r.AsSpan().TestSequence( [ 1, "foo" ] ),
                    (r, _) => r.AsSpan().TestSequence( [ 2, "bar" ] )
                ] ) ) )
            .Go();
    }

    [Fact]
    public void Query_Generic_WithReader_ShouldInvokeReader()
    {
        var command = new DbCommandMock(
            new ResultSet( new[] { "A", "B" }, new[] { new object[] { 1, "foo" }, new object[] { 2, "bar" } } ) );

        var factory = SqlQueryReaderFactoryMock.CreateInstance();
        var result = command.Query( factory.Create<Row>() );

        Assertion.All(
                command.Audit.LastOrDefault().TestEquals( "DbDataReader[0].Close" ),
                result.Rows.TestNotNull( rows => rows.TestSequence( [ new Row( 1, "foo" ), new Row( 2, "bar" ) ] ) ) )
            .Go();
    }

    [Fact]
    public void Query_TypeErased_WithExecutor_ShouldSetCommandTextAndInvokeReader()
    {
        var sql = "SELECT * FROM foo";
        var command = new DbCommandMock(
            new ResultSet( new[] { "A", "B" }, new[] { new object[] { 1, "foo" }, new object[] { 2, "bar" } } ) );

        var factory = SqlQueryReaderFactoryMock.CreateInstance();
        var result = command.Query( factory.Create().BindStatement( sql ) );

        Assertion.All(
                command.Audit.LastOrDefault().TestEquals( "DbDataReader[0].Close" ),
                command.CommandText.TestRefEquals( sql ),
                result.Rows.TestNotNull( rows => rows.TestSequence(
                [
                    (r, _) => r.AsSpan().TestSequence( [ 1, "foo" ] ),
                    (r, _) => r.AsSpan().TestSequence( [ 2, "bar" ] )
                ] ) ) )
            .Go();
    }

    [Fact]
    public void Query_Generic_WithExecutor_ShouldSetCommandTextAndInvokeReader()
    {
        var sql = "SELECT * FROM foo";
        var command = new DbCommandMock(
            new ResultSet( new[] { "A", "B" }, new[] { new object[] { 1, "foo" }, new object[] { 2, "bar" } } ) );

        var factory = SqlQueryReaderFactoryMock.CreateInstance();
        var result = command.Query( factory.Create<Row>().BindStatement( sql ) );

        Assertion.All(
                command.Audit.LastOrDefault().TestEquals( "DbDataReader[0].Close" ),
                command.CommandText.TestRefEquals( sql ),
                result.Rows.TestNotNull( rows => rows.TestSequence( [ new Row( 1, "foo" ), new Row( 2, "bar" ) ] ) ) )
            .Go();
    }

    [Fact]
    public void Query_TypeErased_WithReader_ShouldInvokeScalarQueryReader()
    {
        var command = new DbCommandMock(
            new ResultSet( new[] { "A", "B" }, new[] { new object[] { 1, "foo" }, new object[] { 2, "bar" } } ) );

        var factory = SqlQueryReaderFactoryMock.CreateInstance();
        var result = command.Query( factory.CreateScalar() );

        Assertion.All(
                command.Audit.LastOrDefault().TestEquals( "DbDataReader[0].Close" ),
                result.HasValue.TestTrue(),
                result.Value.TestEquals( 1 ) )
            .Go();
    }

    [Fact]
    public void Query_Generic_WithReader_ShouldInvokeScalarQueryReader()
    {
        var command = new DbCommandMock(
            new ResultSet( new[] { "A", "B" }, new[] { new object[] { 1, "foo" }, new object[] { 2, "bar" } } ) );

        var factory = SqlQueryReaderFactoryMock.CreateInstance();
        var result = command.Query( factory.CreateScalar<int>() );

        Assertion.All(
                command.Audit.LastOrDefault().TestEquals( "DbDataReader[0].Close" ),
                result.HasValue.TestTrue(),
                result.Value.TestEquals( 1 ) )
            .Go();
    }

    [Fact]
    public void Query_TypeErased_WithExecutor_ShouldSetCommandTextAndInvokeScalarQueryReader()
    {
        var sql = "SELECT * FROM foo";
        var command = new DbCommandMock(
            new ResultSet( new[] { "A", "B" }, new[] { new object[] { 1, "foo" }, new object[] { 2, "bar" } } ) );

        var factory = SqlQueryReaderFactoryMock.CreateInstance();
        var result = command.Query( factory.CreateScalar().BindStatement( sql ) );

        Assertion.All(
                command.Audit.LastOrDefault().TestEquals( "DbDataReader[0].Close" ),
                command.CommandText.TestRefEquals( sql ),
                result.HasValue.TestTrue(),
                result.Value.TestEquals( 1 ) )
            .Go();
    }

    [Fact]
    public void Query_Generic_WithExecutor_ShouldSetCommandTextAndInvokeScalarQueryReader()
    {
        var sql = "SELECT * FROM foo";
        var command = new DbCommandMock(
            new ResultSet( new[] { "A", "B" }, new[] { new object[] { 1, "foo" }, new object[] { 2, "bar" } } ) );

        var factory = SqlQueryReaderFactoryMock.CreateInstance();
        var result = command.Query( factory.CreateScalar<int>().BindStatement( sql ) );

        Assertion.All(
                command.Audit.LastOrDefault().TestEquals( "DbDataReader[0].Close" ),
                command.CommandText.TestRefEquals( sql ),
                result.HasValue.TestTrue(),
                result.Value.TestEquals( 1 ) )
            .Go();
    }

    [Fact]
    public async Task QueryAsync_TypeErased_WithReader_ShouldInvokeReader()
    {
        var command = new DbCommandMock(
            new ResultSet( new[] { "A", "B" }, new[] { new object[] { 1, "foo" }, new object[] { 2, "bar" } } ) );

        var factory = SqlQueryReaderFactoryMock.CreateInstance();
        var result = await command.QueryAsync( factory.CreateAsync() );

        Assertion.All(
                command.Audit.LastOrDefault().TestEquals( "DbDataReader[0].Close" ),
                result.Rows.TestNotNull( rows => rows.TestSequence(
                [
                    (r, _) => r.AsSpan().TestSequence( [ 1, "foo" ] ),
                    (r, _) => r.AsSpan().TestSequence( [ 2, "bar" ] )
                ] ) ) )
            .Go();
    }

    [Fact]
    public async Task QueryAsync_Generic_WithReader_ShouldInvokeReader()
    {
        var command = new DbCommandMock(
            new ResultSet( new[] { "A", "B" }, new[] { new object[] { 1, "foo" }, new object[] { 2, "bar" } } ) );

        var factory = SqlQueryReaderFactoryMock.CreateInstance();
        var result = await command.QueryAsync( factory.CreateAsync<Row>() );

        Assertion.All(
                command.Audit.LastOrDefault().TestEquals( "DbDataReader[0].Close" ),
                result.Rows.TestNotNull( rows => rows.TestSequence( [ new Row( 1, "foo" ), new Row( 2, "bar" ) ] ) ) )
            .Go();
    }

    [Fact]
    public async Task QueryAsync_TypeErased_WithExecutor_ShouldSetCommandTextAndInvokeReader()
    {
        var sql = "SELECT * FROM foo";
        var command = new DbCommandMock(
            new ResultSet( new[] { "A", "B" }, new[] { new object[] { 1, "foo" }, new object[] { 2, "bar" } } ) );

        var factory = SqlQueryReaderFactoryMock.CreateInstance();
        var result = await command.QueryAsync( factory.CreateAsync().BindStatement( sql ) );

        Assertion.All(
                command.Audit.LastOrDefault().TestEquals( "DbDataReader[0].Close" ),
                command.CommandText.TestRefEquals( sql ),
                result.Rows.TestNotNull( rows => rows.TestSequence(
                [
                    (r, _) => r.AsSpan().TestSequence( [ 1, "foo" ] ),
                    (r, _) => r.AsSpan().TestSequence( [ 2, "bar" ] )
                ] ) ) )
            .Go();
    }

    [Fact]
    public async Task QueryAsync_Generic_WithExecutor_ShouldSetCommandTextAndInvokeReader()
    {
        var sql = "SELECT * FROM foo";
        var command = new DbCommandMock(
            new ResultSet( new[] { "A", "B" }, new[] { new object[] { 1, "foo" }, new object[] { 2, "bar" } } ) );

        var factory = SqlQueryReaderFactoryMock.CreateInstance();
        var result = await command.QueryAsync( factory.CreateAsync<Row>().BindStatement( sql ) );

        Assertion.All(
                command.Audit.LastOrDefault().TestEquals( "DbDataReader[0].Close" ),
                command.CommandText.TestRefEquals( sql ),
                result.Rows.TestNotNull( rows => rows.TestSequence( [ new Row( 1, "foo" ), new Row( 2, "bar" ) ] ) ) )
            .Go();
    }

    [Fact]
    public async Task QueryAsync_TypeErased_WithReader_ShouldInvokeScalarQueryReader()
    {
        var command = new DbCommandMock(
            new ResultSet( new[] { "A", "B" }, new[] { new object[] { 1, "foo" }, new object[] { 2, "bar" } } ) );

        var factory = SqlQueryReaderFactoryMock.CreateInstance();
        var result = await command.QueryAsync( factory.CreateAsyncScalar() );

        Assertion.All(
                command.Audit.LastOrDefault().TestEquals( "DbDataReader[0].Close" ),
                result.HasValue.TestTrue(),
                result.Value.TestEquals( 1 ) )
            .Go();
    }

    [Fact]
    public async Task QueryAsync_Generic_WithReader_ShouldInvokeScalarQueryReader()
    {
        var command = new DbCommandMock(
            new ResultSet( new[] { "A", "B" }, new[] { new object[] { 1, "foo" }, new object[] { 2, "bar" } } ) );

        var factory = SqlQueryReaderFactoryMock.CreateInstance();
        var result = await command.QueryAsync( factory.CreateAsyncScalar<int>() );

        Assertion.All(
                command.Audit.LastOrDefault().TestEquals( "DbDataReader[0].Close" ),
                result.HasValue.TestTrue(),
                result.Value.TestEquals( 1 ) )
            .Go();
    }

    [Fact]
    public async Task QueryAsync_TypeErased_WithExecutor_ShouldSetCommandTextAndInvokeScalarQueryReader()
    {
        var sql = "SELECT * FROM foo";
        var command = new DbCommandMock(
            new ResultSet( new[] { "A", "B" }, new[] { new object[] { 1, "foo" }, new object[] { 2, "bar" } } ) );

        var factory = SqlQueryReaderFactoryMock.CreateInstance();
        var result = await command.QueryAsync( factory.CreateAsyncScalar().BindStatement( sql ) );

        Assertion.All(
                command.Audit.LastOrDefault().TestEquals( "DbDataReader[0].Close" ),
                command.CommandText.TestRefEquals( sql ),
                result.HasValue.TestTrue(),
                result.Value.TestEquals( 1 ) )
            .Go();
    }

    [Fact]
    public async Task QueryAsync_Generic_WithExecutor_ShouldSetCommandTextAndInvokeScalarQueryReader()
    {
        var sql = "SELECT * FROM foo";
        var command = new DbCommandMock(
            new ResultSet( new[] { "A", "B" }, new[] { new object[] { 1, "foo" }, new object[] { 2, "bar" } } ) );

        var factory = SqlQueryReaderFactoryMock.CreateInstance();
        var result = await command.QueryAsync( factory.CreateAsyncScalar<int>().BindStatement( sql ) );

        Assertion.All(
                command.Audit.LastOrDefault().TestEquals( "DbDataReader[0].Close" ),
                command.CommandText.TestRefEquals( sql ),
                result.HasValue.TestTrue(),
                result.Value.TestEquals( 1 ) )
            .Go();
    }

    [Fact]
    public void Execute_ShouldInvokeStatementExecution()
    {
        var expected = Fixture.Create<int>();
        var command = new DbCommandMock { NonQueryResult = expected };

        var result = command.Execute();

        result.TestEquals( expected ).Go();
    }

    [Fact]
    public async Task ExecuteAsync_ShouldInvokeStatementExecution()
    {
        var expected = Fixture.Create<int>();
        var command = new DbCommandMock { NonQueryResult = expected };

        var result = await command.ExecuteAsync();

        result.TestEquals( expected ).Go();
    }

    [Fact]
    public void SetText_ShouldSetCommandTextAndReturnCommand()
    {
        var sql = "SELECT * FROM foo";
        var command = new DbCommandMock();

        var result = command.SetText( sql );

        Assertion.All(
                result.TestRefEquals( command ),
                result.CommandText.TestRefEquals( sql ) )
            .Go();
    }

    [Theory]
    [InlineData( 0, 0 )]
    [InlineData( 1000, 1 )]
    [InlineData( 15000, 15 )]
    [InlineData( 15001, 16 )]
    [InlineData( 15999, 16 )]
    public void SetTimeout_ShouldSetCommandTimeoutAndReturnCommand(int milliseconds, int expectedSeconds)
    {
        var timeout = TimeSpan.FromMilliseconds( milliseconds );
        var command = new DbCommandMock();

        var result = command.SetTimeout( timeout );

        Assertion.All(
                result.TestRefEquals( command ),
                result.CommandTimeout.TestEquals( expectedSeconds ) )
            .Go();
    }

    [Fact]
    public void Parameterize_TypeErased_ShouldSetParametersAndReturnCommand()
    {
        var command = new DbCommandMock();
        var factory = SqlParameterBinderFactoryMock.CreateInstance();

        var result = command.Parameterize( factory.Create().Bind( new[] { SqlParameter.Named( "a", 1 ) } ) );

        Assertion.All(
                result.TestRefEquals( command ),
                result.Parameters.Count.TestEquals( 1 ),
                command.Parameters[0].Direction.TestEquals( ParameterDirection.Input ),
                command.Parameters[0].DbType.TestEquals( DbType.Int32 ),
                command.Parameters[0].IsNullable.TestFalse(),
                command.Parameters[0].ParameterName.TestEquals( "a" ),
                command.Parameters[0].Value.TestEquals( 1 ) )
            .Go();
    }

    [Fact]
    public void Parameterize_Generic_ShouldSetParametersAndReturnCommand()
    {
        var command = new DbCommandMock();
        var factory = SqlParameterBinderFactoryMock.CreateInstance();

        var result = command.Parameterize( factory.Create<Source>().Bind( new Source { A = 1 } ) );

        Assertion.All(
                result.TestRefEquals( command ),
                result.Parameters.Count.TestEquals( 1 ),
                command.Parameters[0].Direction.TestEquals( ParameterDirection.Input ),
                command.Parameters[0].DbType.TestEquals( DbType.Int32 ),
                command.Parameters[0].IsNullable.TestFalse(),
                command.Parameters[0].ParameterName.TestEquals( "A" ),
                command.Parameters[0].Value.TestEquals( 1 ) )
            .Go();
    }

    public sealed record Row(int A, string B);

    public sealed class Source
    {
        public int A { get; init; }
    }
}
