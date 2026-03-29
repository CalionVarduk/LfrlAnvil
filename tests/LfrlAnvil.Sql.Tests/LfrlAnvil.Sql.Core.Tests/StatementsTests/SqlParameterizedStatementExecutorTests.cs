using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using LfrlAnvil.Sql.Statements;
using LfrlAnvil.TestExtensions.Sql.Mocks;
using LfrlAnvil.TestExtensions.Sql.Mocks.System;

namespace LfrlAnvil.Sql.Tests.StatementsTests;

public class SqlParameterizedStatementExecutorTests : TestsBase
{
    [Fact]
    public void MultiQuery_TypeErased_ShouldBindParametersAndReturnMultiDataReader()
    {
        var command = new DbCommandMock();
        var sql = "SELECT * FROM foo";
        var @delegate = Substitute.For<Action<IDbCommand, IEnumerable<SqlParameter>>>();
        var parameters = new[] { SqlParameter.Named( "a", 0 ), SqlParameter.Named( "b", 1 ) };

        var sut = new SqlParameterBinder( SqlDialectMock.Instance, @delegate ).BindStatement( sql );

        var result = sut.MultiQuery( command, parameters );

        Assertion.All(
                @delegate.CallAt( 0 ).Arguments.TestSequence( [ command, parameters ] ),
                command.CommandText.TestRefEquals( sql ),
                result.Reader.TestType().Exact<DbDataReaderMock>( r => r.Command.TestRefEquals( command ) ) )
            .Go();
    }

    [Fact]
    public async Task MultiQueryAsync_TypeErased_ShouldBindParametersAndReturnMultiDataReader()
    {
        var command = new DbCommandMock();
        var sql = "SELECT * FROM foo";
        var @delegate = Substitute.For<Action<IDbCommand, IEnumerable<SqlParameter>>>();
        var parameters = new[] { SqlParameter.Named( "a", 0 ), SqlParameter.Named( "b", 1 ) };

        var sut = new SqlParameterBinder( SqlDialectMock.Instance, @delegate ).BindStatement( sql );

        var result = await sut.MultiQueryAsync( command, parameters );

        Assertion.All(
                @delegate.CallAt( 0 ).Arguments.TestSequence( [ command, parameters ] ),
                command.CommandText.TestRefEquals( sql ),
                result.Reader.TestType().Exact<DbDataReaderMock>( r => r.Command.TestRefEquals( command ) ) )
            .Go();
    }

    [Fact]
    public void Execute_TypeErased_ShouldBindParametersAndExecuteCommand()
    {
        var command = new DbCommandMock { NonQueryResult = 123 };
        var sql = "SELECT * FROM foo";
        var @delegate = Substitute.For<Action<IDbCommand, IEnumerable<SqlParameter>>>();
        var parameters = new[] { SqlParameter.Named( "a", 0 ), SqlParameter.Named( "b", 1 ) };

        var sut = new SqlParameterBinder( SqlDialectMock.Instance, @delegate ).BindStatement( sql );

        var result = sut.Execute( command, parameters );

        Assertion.All(
                @delegate.CallAt( 0 ).Arguments.TestSequence( [ command, parameters ] ),
                command.CommandText.TestRefEquals( sql ),
                result.TestEquals( 123 ) )
            .Go();
    }

    [Fact]
    public async Task ExecuteAsync_TypeErased_ShouldBindParametersAndExecuteCommand()
    {
        var command = new DbCommandMock { NonQueryResult = 123 };
        var sql = "SELECT * FROM foo";
        var @delegate = Substitute.For<Action<IDbCommand, IEnumerable<SqlParameter>>>();
        var parameters = new[] { SqlParameter.Named( "a", 0 ), SqlParameter.Named( "b", 1 ) };

        var sut = new SqlParameterBinder( SqlDialectMock.Instance, @delegate ).BindStatement( sql );

        var result = await sut.ExecuteAsync( command, parameters );

        Assertion.All(
                @delegate.CallAt( 0 ).Arguments.TestSequence( [ command, parameters ] ),
                command.CommandText.TestRefEquals( sql ),
                result.TestEquals( 123 ) )
            .Go();
    }

    [Fact]
    public void Execute_ForManyParameters_TypeErased_ShouldBindParametersAndExecuteCommand()
    {
        var command = new DbCommandMock { NonQueryResult = 123 };
        var sql = "SELECT * FROM foo";
        var @delegate = Substitute.For<Action<IDbCommand, IEnumerable<SqlParameter>>>();
        var parameters = new[]
        {
            new[] { SqlParameter.Named( "a", 0 ), SqlParameter.Named( "b", 1 ) },
            new[] { SqlParameter.Named( "a", 0 ), SqlParameter.Named( "b", 1 ) }
        };

        var sut = new SqlParameterBinder( SqlDialectMock.Instance, @delegate ).BindStatement( sql );

        var result = sut.Execute( command, parameters );

        Assertion.All(
                @delegate.CallAt( 0 ).Arguments.TestSequence( [ command, parameters[0] ] ),
                @delegate.CallAt( 1 ).Arguments.TestSequence( [ command, parameters[1] ] ),
                command.CommandText.TestRefEquals( sql ),
                result.TestEquals( 246 ) )
            .Go();
    }

    [Fact]
    public async Task ExecuteAsync_ForManyParameters_TypeErased_ShouldBindParametersAndExecuteCommand()
    {
        var command = new DbCommandMock { NonQueryResult = 123 };
        var sql = "SELECT * FROM foo";
        var @delegate = Substitute.For<Action<IDbCommand, IEnumerable<SqlParameter>>>();
        var parameters = new[]
        {
            new[] { SqlParameter.Named( "a", 0 ), SqlParameter.Named( "b", 1 ) },
            new[] { SqlParameter.Named( "a", 0 ), SqlParameter.Named( "b", 1 ) }
        };

        var sut = new SqlParameterBinder( SqlDialectMock.Instance, @delegate ).BindStatement( sql );

        var result = await sut.ExecuteAsync( command, parameters );

        Assertion.All(
                @delegate.CallAt( 0 ).Arguments.TestSequence( [ command, parameters[0] ] ),
                @delegate.CallAt( 1 ).Arguments.TestSequence( [ command, parameters[1] ] ),
                command.CommandText.TestRefEquals( sql ),
                result.TestEquals( 246 ) )
            .Go();
    }

    [Fact]
    public void MultiQuery_Generic_ShouldBindParametersAndReturnMultiDataReader()
    {
        var command = new DbCommandMock();
        var sql = "SELECT * FROM foo";
        var @delegate = Substitute.For<Action<IDbCommand, string>>();
        var sut = new SqlParameterBinder<string>( SqlDialectMock.Instance, @delegate ).BindStatement( sql );

        var result = sut.MultiQuery( command, "param" );

        Assertion.All(
                @delegate.CallAt( 0 ).Arguments.TestSequence( [ command, "param" ] ),
                command.CommandText.TestRefEquals( sql ),
                result.Reader.TestType().Exact<DbDataReaderMock>( r => r.Command.TestRefEquals( command ) ) )
            .Go();
    }

    [Fact]
    public async Task MultiQueryAsync_Generic_ShouldBindParametersAndReturnMultiDataReader()
    {
        var command = new DbCommandMock();
        var sql = "SELECT * FROM foo";
        var @delegate = Substitute.For<Action<IDbCommand, string>>();
        var sut = new SqlParameterBinder<string>( SqlDialectMock.Instance, @delegate ).BindStatement( sql );

        var result = await sut.MultiQueryAsync( command, "param" );

        Assertion.All(
                @delegate.CallAt( 0 ).Arguments.TestSequence( [ command, "param" ] ),
                command.CommandText.TestRefEquals( sql ),
                result.Reader.TestType().Exact<DbDataReaderMock>( r => r.Command.TestRefEquals( command ) ) )
            .Go();
    }

    [Fact]
    public void Execute_Generic_ShouldBindParametersAndExecuteCommand()
    {
        var command = new DbCommandMock { NonQueryResult = 123 };
        var sql = "SELECT * FROM foo";
        var @delegate = Substitute.For<Action<IDbCommand, string>>();
        var sut = new SqlParameterBinder<string>( SqlDialectMock.Instance, @delegate ).BindStatement( sql );

        var result = sut.Execute( command, "param" );

        Assertion.All(
                @delegate.CallAt( 0 ).Arguments.TestSequence( [ command, "param" ] ),
                command.CommandText.TestRefEquals( sql ),
                result.TestEquals( 123 ) )
            .Go();
    }

    [Fact]
    public async Task ExecuteAsync_Generic_ShouldBindParametersAndExecuteCommand()
    {
        var command = new DbCommandMock { NonQueryResult = 123 };
        var sql = "SELECT * FROM foo";
        var @delegate = Substitute.For<Action<IDbCommand, string>>();
        var sut = new SqlParameterBinder<string>( SqlDialectMock.Instance, @delegate ).BindStatement( sql );

        var result = await sut.ExecuteAsync( command, "param" );

        Assertion.All(
                @delegate.CallAt( 0 ).Arguments.TestSequence( [ command, "param" ] ),
                command.CommandText.TestRefEquals( sql ),
                result.TestEquals( 123 ) )
            .Go();
    }

    [Fact]
    public void Execute_ForManyParameters_Generic_ShouldBindParametersAndExecuteCommand()
    {
        var command = new DbCommandMock { NonQueryResult = 123 };
        var sql = "SELECT * FROM foo";
        var @delegate = Substitute.For<Action<IDbCommand, string>>();
        var parameters = new[] { "foo", "bar" };

        var sut = new SqlParameterBinder<string>( SqlDialectMock.Instance, @delegate ).BindStatement( sql );

        var result = sut.Execute( command, parameters );

        Assertion.All(
                @delegate.CallAt( 0 ).Arguments.TestSequence( [ command, parameters[0] ] ),
                @delegate.CallAt( 1 ).Arguments.TestSequence( [ command, parameters[1] ] ),
                command.CommandText.TestRefEquals( sql ),
                result.TestEquals( 246 ) )
            .Go();
    }

    [Fact]
    public async Task ExecuteAsync_ForManyParameters_Generic_ShouldBindParametersAndExecuteCommand()
    {
        var command = new DbCommandMock { NonQueryResult = 123 };
        var sql = "SELECT * FROM foo";
        var @delegate = Substitute.For<Action<IDbCommand, string>>();
        var parameters = new[] { "foo", "bar" };

        var sut = new SqlParameterBinder<string>( SqlDialectMock.Instance, @delegate ).BindStatement( sql );

        var result = await sut.ExecuteAsync( command, parameters );

        Assertion.All(
                @delegate.CallAt( 0 ).Arguments.TestSequence( [ command, parameters[0] ] ),
                @delegate.CallAt( 1 ).Arguments.TestSequence( [ command, parameters[1] ] ),
                command.CommandText.TestRefEquals( sql ),
                result.TestEquals( 246 ) )
            .Go();
    }
}
