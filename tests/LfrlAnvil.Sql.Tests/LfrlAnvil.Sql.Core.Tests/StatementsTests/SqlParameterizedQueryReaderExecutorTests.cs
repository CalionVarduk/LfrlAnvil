using System.Collections.Generic;
using System.Data;
using LfrlAnvil.Sql.Statements;
using LfrlAnvil.TestExtensions.NSubstitute;
using LfrlAnvil.TestExtensions.Sql.Mocks.System;

namespace LfrlAnvil.Sql.Tests.StatementsTests;

public class SqlParameterizedQueryReaderExecutorTests : TestsBase
{
    [Fact]
    public void Execute_TypeErased_ShouldBindParametersAndExecuteReader()
    {
        var command = new DbCommandMock();
        var dialect = new SqlDialect( "foo" );
        var binderDelegate = Substitute.For<Action<IDbCommand, IEnumerable<SqlParameter>>>();
        var parameters = new[] { SqlParameter.Named( "a", 0 ), SqlParameter.Named( "b", 1 ) };

        var expected = new SqlQueryResult(
            [ new SqlResultSetField( 0, "a" ), new SqlResultSetField( 1, "b" ) ],
            [ "foo", 3, "lorem", 5 ] );

        var sql = "SELECT * FROM foo";
        var readerDelegate = Substitute.For<Func<IDataReader, SqlQueryReaderOptions, SqlQueryResult>>();
        readerDelegate.WithAnyArgs( _ => expected );
        var reader = new SqlQueryReader( new SqlDialect( "foo" ), readerDelegate );
        var sut = reader.BindStatement( sql ).Parameterize( new SqlParameterBinder( dialect, binderDelegate ) );

        var result = sut.Execute( command, parameters );

        Assertion.All(
                binderDelegate.CallAt( 0 ).Arguments.TestSequence( [ command, parameters ] ),
                readerDelegate.CallCount().TestEquals( 1 ),
                command.CommandText.TestRefEquals( sql ),
                result.TestEquals( expected ) )
            .Go();
    }

    [Fact]
    public void Execute_Generic_ShouldBindParametersAndExecuteReader()
    {
        var command = new DbCommandMock();
        var dialect = new SqlDialect( "foo" );
        var binderDelegate = Substitute.For<Action<IDbCommand, string>>();

        var expected = new SqlQueryResult<object[]>( null, [ [ 1, "foo" ], [ 2, "bar" ] ] );

        var sql = "SELECT * FROM foo";
        var readerDelegate = Substitute.For<Func<IDataReader, SqlQueryReaderOptions, SqlQueryResult<object[]>>>();
        readerDelegate.WithAnyArgs( _ => expected );
        var reader = new SqlQueryReader<object[]>( new SqlDialect( "foo" ), readerDelegate );
        var sut = reader.BindStatement( sql ).Parameterize( new SqlParameterBinder<string>( dialect, binderDelegate ) );

        var result = sut.Execute( command, "param" );

        Assertion.All(
                binderDelegate.CallAt( 0 ).Arguments.TestSequence( [ command, "param" ] ),
                readerDelegate.CallCount().TestEquals( 1 ),
                command.CommandText.TestRefEquals( sql ),
                result.TestEquals( expected ) )
            .Go();
    }
}
