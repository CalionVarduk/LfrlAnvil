using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using LfrlAnvil.Sql.Statements;
using LfrlAnvil.Sql.Statements.Compilers;
using LfrlAnvil.TestExtensions.FluentAssertions;
using LfrlAnvil.TestExtensions.Sql.Mocks;
using LfrlAnvil.TestExtensions.Sql.Mocks.System;

namespace LfrlAnvil.Sql.Tests.StatementsTests;

public class SqlStatementObjectExtensionsTests : TestsBase
{
    [Fact]
    public async Task BeginTransactionAsync_ShouldReturnTransaction()
    {
        var sut = new DbConnectionMock();
        var result = await ((IDbConnection)sut).BeginTransactionAsync( IsolationLevel.Serializable );
        result.Should().BeSameAs( sut.CreatedTransactions[0] );
    }

    [Fact]
    public void Query_TypeErased_WithReader_ShouldInvokeReader()
    {
        var command = new DbCommandMock(
            new ResultSet( new[] { "A", "B" }, new[] { new object[] { 1, "foo" }, new object[] { 2, "bar" } } ) );

        var factory = SqlQueryReaderFactoryMock.CreateInstance();
        var result = command.Query( factory.Create() );

        using ( new AssertionScope() )
        {
            command.Audit.LastOrDefault().Should().Be( "DbDataReader[0].Close" );
            result.Rows.Should().NotBeNull();
            (result.Rows?.Count).Should().Be( 2 );
            (result.Rows?[0].AsSpan().ToArray()).Should().BeSequentiallyEqualTo( 1, "foo" );
            (result.Rows?[1].AsSpan().ToArray()).Should().BeSequentiallyEqualTo( 2, "bar" );
        }
    }

    [Fact]
    public void Query_Generic_WithReader_ShouldInvokeReader()
    {
        var command = new DbCommandMock(
            new ResultSet( new[] { "A", "B" }, new[] { new object[] { 1, "foo" }, new object[] { 2, "bar" } } ) );

        var factory = SqlQueryReaderFactoryMock.CreateInstance();
        var result = command.Query( factory.Create<Row>() );

        using ( new AssertionScope() )
        {
            command.Audit.LastOrDefault().Should().Be( "DbDataReader[0].Close" );
            result.Rows.Should().BeSequentiallyEqualTo( new Row( 1, "foo" ), new Row( 2, "bar" ) );
        }
    }

    [Fact]
    public void Query_TypeErased_WithExecutor_ShouldSetCommandTextAndInvokeReader()
    {
        var sql = "SELECT * FROM foo";
        var command = new DbCommandMock(
            new ResultSet( new[] { "A", "B" }, new[] { new object[] { 1, "foo" }, new object[] { 2, "bar" } } ) );

        var factory = SqlQueryReaderFactoryMock.CreateInstance();
        var result = command.Query( factory.Create().Bind( sql ) );

        using ( new AssertionScope() )
        {
            command.Audit.LastOrDefault().Should().Be( "DbDataReader[0].Close" );
            command.CommandText.Should().BeSameAs( sql );
            result.Rows.Should().NotBeNull();
            (result.Rows?.Count).Should().Be( 2 );
            (result.Rows?[0].AsSpan().ToArray()).Should().BeSequentiallyEqualTo( 1, "foo" );
            (result.Rows?[1].AsSpan().ToArray()).Should().BeSequentiallyEqualTo( 2, "bar" );
        }
    }

    [Fact]
    public void Query_Generic_WithExecutor_ShouldSetCommandTextAndInvokeReader()
    {
        var sql = "SELECT * FROM foo";
        var command = new DbCommandMock(
            new ResultSet( new[] { "A", "B" }, new[] { new object[] { 1, "foo" }, new object[] { 2, "bar" } } ) );

        var factory = SqlQueryReaderFactoryMock.CreateInstance();
        var result = command.Query( factory.Create<Row>().Bind( sql ) );

        using ( new AssertionScope() )
        {
            command.Audit.LastOrDefault().Should().Be( "DbDataReader[0].Close" );
            command.CommandText.Should().BeSameAs( sql );
            result.Rows.Should().BeSequentiallyEqualTo( new Row( 1, "foo" ), new Row( 2, "bar" ) );
        }
    }

    [Fact]
    public void Query_TypeErased_WithReader_ShouldInvokeScalarQueryReader()
    {
        var command = new DbCommandMock(
            new ResultSet( new[] { "A", "B" }, new[] { new object[] { 1, "foo" }, new object[] { 2, "bar" } } ) );

        var factory = SqlQueryReaderFactoryMock.CreateInstance();
        var result = command.Query( factory.CreateScalar() );

        using ( new AssertionScope() )
        {
            command.Audit.LastOrDefault().Should().Be( "DbDataReader[0].Close" );
            result.HasValue.Should().BeTrue();
            result.Value.Should().Be( 1 );
        }
    }

    [Fact]
    public void Query_Generic_WithReader_ShouldInvokeScalarQueryReader()
    {
        var command = new DbCommandMock(
            new ResultSet( new[] { "A", "B" }, new[] { new object[] { 1, "foo" }, new object[] { 2, "bar" } } ) );

        var factory = SqlQueryReaderFactoryMock.CreateInstance();
        var result = command.Query( factory.CreateScalar<int>() );

        using ( new AssertionScope() )
        {
            command.Audit.LastOrDefault().Should().Be( "DbDataReader[0].Close" );
            result.HasValue.Should().BeTrue();
            result.Value.Should().Be( 1 );
        }
    }

    [Fact]
    public void Query_TypeErased_WithExecutor_ShouldSetCommandTextAndInvokeScalarQueryReader()
    {
        var sql = "SELECT * FROM foo";
        var command = new DbCommandMock(
            new ResultSet( new[] { "A", "B" }, new[] { new object[] { 1, "foo" }, new object[] { 2, "bar" } } ) );

        var factory = SqlQueryReaderFactoryMock.CreateInstance();
        var result = command.Query( factory.CreateScalar().Bind( sql ) );

        using ( new AssertionScope() )
        {
            command.Audit.LastOrDefault().Should().Be( "DbDataReader[0].Close" );
            command.CommandText.Should().BeSameAs( sql );
            result.HasValue.Should().BeTrue();
            result.Value.Should().Be( 1 );
        }
    }

    [Fact]
    public void Query_Generic_WithExecutor_ShouldSetCommandTextAndInvokeScalarQueryReader()
    {
        var sql = "SELECT * FROM foo";
        var command = new DbCommandMock(
            new ResultSet( new[] { "A", "B" }, new[] { new object[] { 1, "foo" }, new object[] { 2, "bar" } } ) );

        var factory = SqlQueryReaderFactoryMock.CreateInstance();
        var result = command.Query( factory.CreateScalar<int>().Bind( sql ) );

        using ( new AssertionScope() )
        {
            command.Audit.LastOrDefault().Should().Be( "DbDataReader[0].Close" );
            command.CommandText.Should().BeSameAs( sql );
            result.HasValue.Should().BeTrue();
            result.Value.Should().Be( 1 );
        }
    }

    [Fact]
    public async Task QueryAsync_TypeErased_WithReader_ShouldInvokeReader()
    {
        var command = new DbCommandMock(
            new ResultSet( new[] { "A", "B" }, new[] { new object[] { 1, "foo" }, new object[] { 2, "bar" } } ) );

        var factory = SqlQueryReaderFactoryMock.CreateInstance();
        var result = await command.QueryAsync( factory.CreateAsync() );

        using ( new AssertionScope() )
        {
            command.Audit.LastOrDefault().Should().Be( "DbDataReader[0].Close" );
            result.Rows.Should().NotBeNull();
            (result.Rows?.Count).Should().Be( 2 );
            (result.Rows?[0].AsSpan().ToArray()).Should().BeSequentiallyEqualTo( 1, "foo" );
            (result.Rows?[1].AsSpan().ToArray()).Should().BeSequentiallyEqualTo( 2, "bar" );
        }
    }

    [Fact]
    public async Task QueryAsync_Generic_WithReader_ShouldInvokeReader()
    {
        var command = new DbCommandMock(
            new ResultSet( new[] { "A", "B" }, new[] { new object[] { 1, "foo" }, new object[] { 2, "bar" } } ) );

        var factory = SqlQueryReaderFactoryMock.CreateInstance();
        var result = await command.QueryAsync( factory.CreateAsync<Row>() );

        using ( new AssertionScope() )
        {
            command.Audit.LastOrDefault().Should().Be( "DbDataReader[0].Close" );
            result.Rows.Should().BeSequentiallyEqualTo( new Row( 1, "foo" ), new Row( 2, "bar" ) );
        }
    }

    [Fact]
    public async Task QueryAsync_TypeErased_WithExecutor_ShouldSetCommandTextAndInvokeReader()
    {
        var sql = "SELECT * FROM foo";
        var command = new DbCommandMock(
            new ResultSet( new[] { "A", "B" }, new[] { new object[] { 1, "foo" }, new object[] { 2, "bar" } } ) );

        var factory = SqlQueryReaderFactoryMock.CreateInstance();
        var result = await command.QueryAsync( factory.CreateAsync().Bind( sql ) );

        using ( new AssertionScope() )
        {
            command.Audit.LastOrDefault().Should().Be( "DbDataReader[0].Close" );
            command.CommandText.Should().BeSameAs( sql );
            result.Rows.Should().NotBeNull();
            (result.Rows?.Count).Should().Be( 2 );
            (result.Rows?[0].AsSpan().ToArray()).Should().BeSequentiallyEqualTo( 1, "foo" );
            (result.Rows?[1].AsSpan().ToArray()).Should().BeSequentiallyEqualTo( 2, "bar" );
        }
    }

    [Fact]
    public async Task QueryAsync_Generic_WithExecutor_ShouldSetCommandTextAndInvokeReader()
    {
        var sql = "SELECT * FROM foo";
        var command = new DbCommandMock(
            new ResultSet( new[] { "A", "B" }, new[] { new object[] { 1, "foo" }, new object[] { 2, "bar" } } ) );

        var factory = SqlQueryReaderFactoryMock.CreateInstance();
        var result = await command.QueryAsync( factory.CreateAsync<Row>().Bind( sql ) );

        using ( new AssertionScope() )
        {
            command.Audit.LastOrDefault().Should().Be( "DbDataReader[0].Close" );
            command.CommandText.Should().BeSameAs( sql );
            result.Rows.Should().BeSequentiallyEqualTo( new Row( 1, "foo" ), new Row( 2, "bar" ) );
        }
    }

    [Fact]
    public async Task QueryAsync_TypeErased_WithReader_ShouldInvokeScalarQueryReader()
    {
        var command = new DbCommandMock(
            new ResultSet( new[] { "A", "B" }, new[] { new object[] { 1, "foo" }, new object[] { 2, "bar" } } ) );

        var factory = SqlQueryReaderFactoryMock.CreateInstance();
        var result = await command.QueryAsync( factory.CreateAsyncScalar() );

        using ( new AssertionScope() )
        {
            command.Audit.LastOrDefault().Should().Be( "DbDataReader[0].Close" );
            result.HasValue.Should().BeTrue();
            result.Value.Should().Be( 1 );
        }
    }

    [Fact]
    public async Task QueryAsync_Generic_WithReader_ShouldInvokeScalarQueryReader()
    {
        var command = new DbCommandMock(
            new ResultSet( new[] { "A", "B" }, new[] { new object[] { 1, "foo" }, new object[] { 2, "bar" } } ) );

        var factory = SqlQueryReaderFactoryMock.CreateInstance();
        var result = await command.QueryAsync( factory.CreateAsyncScalar<int>() );

        using ( new AssertionScope() )
        {
            command.Audit.LastOrDefault().Should().Be( "DbDataReader[0].Close" );
            result.HasValue.Should().BeTrue();
            result.Value.Should().Be( 1 );
        }
    }

    [Fact]
    public async Task QueryAsync_TypeErased_WithExecutor_ShouldSetCommandTextAndInvokeScalarQueryReader()
    {
        var sql = "SELECT * FROM foo";
        var command = new DbCommandMock(
            new ResultSet( new[] { "A", "B" }, new[] { new object[] { 1, "foo" }, new object[] { 2, "bar" } } ) );

        var factory = SqlQueryReaderFactoryMock.CreateInstance();
        var result = await command.QueryAsync( factory.CreateAsyncScalar().Bind( sql ) );

        using ( new AssertionScope() )
        {
            command.Audit.LastOrDefault().Should().Be( "DbDataReader[0].Close" );
            command.CommandText.Should().BeSameAs( sql );
            result.HasValue.Should().BeTrue();
            result.Value.Should().Be( 1 );
        }
    }

    [Fact]
    public async Task QueryAsync_Generic_WithExecutor_ShouldSetCommandTextAndInvokeScalarQueryReader()
    {
        var sql = "SELECT * FROM foo";
        var command = new DbCommandMock(
            new ResultSet( new[] { "A", "B" }, new[] { new object[] { 1, "foo" }, new object[] { 2, "bar" } } ) );

        var factory = SqlQueryReaderFactoryMock.CreateInstance();
        var result = await command.QueryAsync( factory.CreateAsyncScalar<int>().Bind( sql ) );

        using ( new AssertionScope() )
        {
            command.Audit.LastOrDefault().Should().Be( "DbDataReader[0].Close" );
            command.CommandText.Should().BeSameAs( sql );
            result.HasValue.Should().BeTrue();
            result.Value.Should().Be( 1 );
        }
    }

    [Fact]
    public void Execute_ShouldInvokeStatementExecution()
    {
        var expected = Fixture.Create<int>();
        var command = new DbCommandMock { NonQueryResult = expected };

        var result = command.Execute();

        result.Should().Be( expected );
    }

    [Fact]
    public async Task ExecuteAsync_ShouldInvokeStatementExecution()
    {
        var expected = Fixture.Create<int>();
        var command = new DbCommandMock { NonQueryResult = expected };

        var result = await command.ExecuteAsync();

        result.Should().Be( expected );
    }

    [Fact]
    public void SetText_ShouldSetCommandTextAndReturnCommand()
    {
        var sql = "SELECT * FROM foo";
        var command = new DbCommandMock();

        var result = command.SetText( sql );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( command );
            result.CommandText.Should().BeSameAs( sql );
        }
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

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( command );
            result.CommandTimeout.Should().Be( expectedSeconds );
        }
    }

    [Fact]
    public void Parameterize_TypeErased_ShouldSetParametersAndReturnCommand()
    {
        var command = new DbCommandMock();
        var factory = SqlParameterBinderFactoryMock.CreateInstance();

        var result = command.Parameterize( factory.Create().Bind( new[] { KeyValuePair.Create( "a", (object?)1 ) } ) );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( command );
            result.Parameters.Should().HaveCount( 1 );
            command.Parameters[0].Direction.Should().Be( ParameterDirection.Input );
            command.Parameters[0].DbType.Should().Be( DbType.Int32 );
            command.Parameters[0].IsNullable.Should().BeFalse();
            command.Parameters[0].ParameterName.Should().Be( "a" );
            command.Parameters[0].Value.Should().Be( 1 );
        }
    }

    [Fact]
    public void Parameterize_Generic_ShouldSetParametersAndReturnCommand()
    {
        var command = new DbCommandMock();
        var factory = SqlParameterBinderFactoryMock.CreateInstance();

        var result = command.Parameterize( factory.Create<Source>().Bind( new Source { A = 1 } ) );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( command );
            result.Parameters.Should().HaveCount( 1 );
            command.Parameters[0].Direction.Should().Be( ParameterDirection.Input );
            command.Parameters[0].DbType.Should().Be( DbType.Int32 );
            command.Parameters[0].IsNullable.Should().BeFalse();
            command.Parameters[0].ParameterName.Should().Be( "A" );
            command.Parameters[0].Value.Should().Be( 1 );
        }
    }

    public sealed record Row(int A, string B);

    public sealed class Source
    {
        public int A { get; init; }
    }
}
