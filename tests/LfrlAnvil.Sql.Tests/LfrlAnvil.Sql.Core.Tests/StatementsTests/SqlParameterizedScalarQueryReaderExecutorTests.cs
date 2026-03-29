using System.Collections.Generic;
using System.Data;
using LfrlAnvil.Sql.Statements;
using LfrlAnvil.TestExtensions.NSubstitute;
using LfrlAnvil.TestExtensions.Sql.Mocks.System;

namespace LfrlAnvil.Sql.Tests.StatementsTests;

public class SqlParameterizedScalarQueryReaderExecutorTests : TestsBase
{
    [Fact]
    public void Execute_TypeErased_ShouldBindParametersAndExecuteReader()
    {
        var command = new DbCommandMock();
        var dialect = new SqlDialect( "foo" );
        var binderDelegate = Substitute.For<Action<IDbCommand, IEnumerable<SqlParameter>>>();
        var parameters = new[] { SqlParameter.Named( "a", 0 ), SqlParameter.Named( "b", 1 ) };

        var expected = new SqlScalarQueryResult( "foo" );
        var sql = "SELECT * FROM foo";
        var readerDelegate = Substitute.For<Func<IDataReader, SqlScalarQueryResult>>();
        readerDelegate.WithAnyArgs( _ => expected );
        var reader = new SqlScalarQueryReader( new SqlDialect( "foo" ), readerDelegate );
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

        var expected = new SqlScalarQueryResult<object[]>( [ 1, "foo" ] );

        var sql = "SELECT * FROM foo";
        var readerDelegate = Substitute.For<Func<IDataReader, SqlScalarQueryResult<object[]>>>();
        readerDelegate.WithAnyArgs( _ => expected );
        var reader = new SqlScalarQueryReader<object[]>( new SqlDialect( "foo" ), readerDelegate );
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
