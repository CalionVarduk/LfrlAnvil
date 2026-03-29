using System.Collections.Generic;
using System.Data;
using LfrlAnvil.Sql.Statements;
using LfrlAnvil.TestExtensions.NSubstitute;
using LfrlAnvil.TestExtensions.Sql.Mocks.System;

namespace LfrlAnvil.Sql.Tests.StatementsTests;

public class SqlQueryReaderExecutorTests : TestsBase
{
    [Fact]
    public void BindStatement_Extension_ForTypeErased_ShouldCreateCorrectExecutor()
    {
        var sql = "SELECT * FROM foo";
        var @delegate = Substitute.For<Func<IDataReader, SqlQueryReaderOptions, SqlQueryResult>>();
        var reader = new SqlQueryReader( new SqlDialect( "foo" ), @delegate );
        var sut = reader.BindStatement( sql );

        Assertion.All(
                sut.Sql.TestRefEquals( sql ),
                sut.Reader.TestEquals( reader ) )
            .Go();
    }

    [Fact]
    public void Execute_ForTypeErased_ShouldSetCommandTextAndInvokeDelegate()
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

        var sql = "SELECT * FROM foo";
        var command = new DbCommandMock();
        var @delegate = Substitute.For<Func<IDataReader, SqlQueryReaderOptions, SqlQueryResult>>();
        @delegate.WithAnyArgs( _ => expected );
        var reader = new SqlQueryReader( new SqlDialect( "foo" ), @delegate );
        var sut = reader.BindStatement( sql );

        var result = sut.Execute( command );

        Assertion.All(
                @delegate.CallCount().TestEquals( 1 ),
                command.CommandText.TestRefEquals( sql ),
                result.TestEquals( expected ) )
            .Go();
    }

    [Fact]
    public void BindStatement_Extension_ForGeneric_ShouldCreateCorrectExecutor()
    {
        var sql = "SELECT * FROM foo";
        var @delegate = Substitute.For<Func<IDataReader, SqlQueryReaderOptions, SqlQueryResult<object[]>>>();
        var reader = new SqlQueryReader<object[]>( new SqlDialect( "foo" ), @delegate );
        var sut = reader.BindStatement( sql );

        Assertion.All(
                sut.Sql.TestRefEquals( sql ),
                sut.Reader.TestEquals( reader ) )
            .Go();
    }

    [Fact]
    public void Execute_ForGeneric_ShouldSetCommandTextAndInvokeDelegate()
    {
        var expected = new SqlQueryResult<object[]>(
            new[] { new SqlResultSetField( 0, "a" ), new SqlResultSetField( 1, "b" ) },
            new List<object[]>
            {
                new object[] { "foo", 3 },
                new object[] { "lorem", 5 }
            } );

        var sql = "SELECT * FROM foo";
        var command = new DbCommandMock();
        var @delegate = Substitute.For<Func<IDataReader, SqlQueryReaderOptions, SqlQueryResult<object[]>>>();
        @delegate.WithAnyArgs( _ => expected );
        var reader = new SqlQueryReader<object[]>( new SqlDialect( "foo" ), @delegate );
        var sut = reader.BindStatement( sql );

        var result = sut.Execute( command );

        Assertion.All(
                @delegate.CallCount().TestEquals( 1 ),
                command.CommandText.TestRefEquals( sql ),
                result.TestEquals( expected ) )
            .Go();
    }
}
