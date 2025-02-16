using System.Data;
using LfrlAnvil.Sql.Statements;
using LfrlAnvil.TestExtensions.NSubstitute;
using LfrlAnvil.TestExtensions.Sql.Mocks.System;

namespace LfrlAnvil.Sql.Tests.StatementsTests;

public class SqlScalarQueryReaderExecutorTests : TestsBase
{
    [Fact]
    public void Bind_Extension_ForTypeErased_ShouldCreateCorrectExecutor()
    {
        var sql = "SELECT * FROM foo";
        var @delegate = Substitute.For<Func<IDataReader, SqlScalarQueryResult>>();
        var reader = new SqlScalarQueryReader( new SqlDialect( "foo" ), @delegate );
        var sut = reader.Bind( sql );

        Assertion.All(
                sut.Sql.TestRefEquals( sql ),
                sut.Reader.TestEquals( reader ) )
            .Go();
    }

    [Fact]
    public void Execute_ForTypeErased_ShouldSetCommandTextAndInvokeDelegate()
    {
        var expected = new SqlScalarQueryResult( "foo" );

        var sql = "SELECT * FROM foo";
        var command = new DbCommandMock();
        var @delegate = Substitute.For<Func<IDataReader, SqlScalarQueryResult>>();
        @delegate.WithAnyArgs( _ => expected );
        var reader = new SqlScalarQueryReader( new SqlDialect( "foo" ), @delegate );
        var sut = reader.Bind( sql );

        var result = sut.Execute( command );

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
        var @delegate = Substitute.For<Func<IDataReader, SqlScalarQueryResult<int>>>();
        var reader = new SqlScalarQueryReader<int>( new SqlDialect( "foo" ), @delegate );
        var sut = reader.Bind( sql );

        Assertion.All(
                sut.Sql.TestRefEquals( sql ),
                sut.Reader.TestEquals( reader ) )
            .Go();
    }

    [Fact]
    public void Execute_ForGeneric_ShouldSetCommandTextAndInvokeDelegate()
    {
        var expected = new SqlScalarQueryResult<int>( 42 );

        var sql = "SELECT * FROM foo";
        var command = new DbCommandMock();
        var @delegate = Substitute.For<Func<IDataReader, SqlScalarQueryResult<int>>>();
        @delegate.WithAnyArgs( _ => expected );
        var reader = new SqlScalarQueryReader<int>( new SqlDialect( "foo" ), @delegate );
        var sut = reader.Bind( sql );

        var result = sut.Execute( command );

        Assertion.All(
                @delegate.CallCount().TestEquals( 1 ),
                command.CommandText.TestRefEquals( sql ),
                result.TestEquals( expected ) )
            .Go();
    }
}
