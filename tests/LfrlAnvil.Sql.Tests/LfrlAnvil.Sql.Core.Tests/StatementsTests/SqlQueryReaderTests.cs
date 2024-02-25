using System.Collections.Generic;
using System.Data;
using LfrlAnvil.Sql.Statements;
using LfrlAnvil.TestExtensions.FluentAssertions;
using LfrlAnvil.TestExtensions.NSubstitute;
using LfrlAnvil.TestExtensions.Sql.Mocks.System;

namespace LfrlAnvil.Sql.Tests.StatementsTests;

public class SqlQueryReaderTests : TestsBase
{
    [Fact]
    public void Read_ForTypeErased_ShouldInvokeDelegate()
    {
        var expected = new SqlQueryReaderResult(
            new[] { new SqlResultSetField( 0, "a" ), new SqlResultSetField( 1, "b" ) },
            new List<object?> { "foo", 3, "lorem", 5 } );

        var reader = new DbDataReaderMock();
        var options = new SqlQueryReaderOptions();
        var dialect = new SqlDialect( "foo" );
        var @delegate = Substitute.For<Func<IDataReader, SqlQueryReaderOptions, SqlQueryReaderResult>>();
        @delegate.WithAnyArgs( _ => expected );
        var sut = new SqlQueryReader( dialect, @delegate );

        var result = sut.Read( reader, options );

        using ( new AssertionScope() )
        {
            sut.Dialect.Should().BeSameAs( dialect );
            sut.Delegate.Should().BeSameAs( @delegate );
            @delegate.Verify().CallAt( 0 ).Exists().And.Arguments.Should().BeSequentiallyEqualTo( reader, options );
            result.Should().BeEquivalentTo( expected );
        }
    }

    [Fact]
    public void Read_ForGeneric_ShouldInvokeDelegate()
    {
        var expected = new SqlQueryReaderResult<object[]>(
            new[] { new SqlResultSetField( 0, "a" ), new SqlResultSetField( 1, "b" ) },
            new List<object[]>
            {
                new object[] { "foo", 3 },
                new object[] { "lorem", 5 }
            } );

        var reader = new DbDataReaderMock();
        var options = new SqlQueryReaderOptions();
        var dialect = new SqlDialect( "foo" );
        var @delegate = Substitute.For<Func<IDataReader, SqlQueryReaderOptions, SqlQueryReaderResult<object[]>>>();
        @delegate.WithAnyArgs( _ => expected );
        var sut = new SqlQueryReader<object[]>( dialect, @delegate );

        var result = sut.Read( reader, options );

        using ( new AssertionScope() )
        {
            sut.Dialect.Should().BeSameAs( dialect );
            sut.Delegate.Should().BeSameAs( @delegate );
            @delegate.Verify().CallAt( 0 ).Exists().And.Arguments.Should().BeSequentiallyEqualTo( reader, options );
            result.Should().BeEquivalentTo( expected );
        }
    }
}
