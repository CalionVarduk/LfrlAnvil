using System.Collections.Generic;
using System.Data;
using LfrlAnvil.Sql.Statements;
using LfrlAnvil.TestExtensions.FluentAssertions;
using LfrlAnvil.TestExtensions.NSubstitute;
using LfrlAnvil.TestExtensions.Sql.Mocks.System;

namespace LfrlAnvil.Sql.Tests.StatementsTests;

public class SqlQueryReaderExecutorTests : TestsBase
{
    [Fact]
    public void Bind_Extension_ForTypeErased_ShouldCreateCorrectExecutor()
    {
        var sql = "SELECT * FROM foo";
        var @delegate = Substitute.For<Func<IDataReader, SqlQueryReaderOptions, SqlQueryReaderResult>>();
        var reader = new SqlQueryReader( new SqlDialect( "foo" ), @delegate );
        var sut = reader.Bind( sql );

        using ( new AssertionScope() )
        {
            sut.Sql.Should().BeSameAs( sql );
            sut.Reader.Should().BeEquivalentTo( reader );
        }
    }

    [Fact]
    public void Execute_ForTypeErased_ShouldSetCommandTextAndInvokeDelegate()
    {
        var expected = new SqlQueryReaderResult(
            new[] { new SqlResultSetField( 0, "a" ), new SqlResultSetField( 1, "b" ) },
            new List<object?> { "foo", 3, "lorem", 5 } );

        var sql = "SELECT * FROM foo";
        var command = new DbCommandMock();
        var @delegate = Substitute.For<Func<IDataReader, SqlQueryReaderOptions, SqlQueryReaderResult>>();
        @delegate.WithAnyArgs( _ => expected );
        var reader = new SqlQueryReader( new SqlDialect( "foo" ), @delegate );
        var sut = reader.Bind( sql );

        var result = sut.Execute( command );

        using ( new AssertionScope() )
        {
            @delegate.Verify().CallCount.Should().Be( 1 );
            command.CommandText.Should().BeSameAs( sql );
            result.Should().BeEquivalentTo( expected );
        }
    }

    [Fact]
    public void Bind_Extension_ForGeneric_ShouldCreateCorrectExecutor()
    {
        var sql = "SELECT * FROM foo";
        var @delegate = Substitute.For<Func<IDataReader, SqlQueryReaderOptions, SqlQueryReaderResult<object[]>>>();
        var reader = new SqlQueryReader<object[]>( new SqlDialect( "foo" ), @delegate );
        var sut = reader.Bind( sql );

        using ( new AssertionScope() )
        {
            sut.Sql.Should().BeSameAs( sql );
            sut.Reader.Should().BeEquivalentTo( reader );
        }
    }

    [Fact]
    public void Execute_ForGeneric_ShouldSetCommandTextAndInvokeDelegate()
    {
        var expected = new SqlQueryReaderResult<object[]>(
            new[] { new SqlResultSetField( 0, "a" ), new SqlResultSetField( 1, "b" ) },
            new List<object[]>
            {
                new object[] { "foo", 3 },
                new object[] { "lorem", 5 }
            } );

        var sql = "SELECT * FROM foo";
        var command = new DbCommandMock();
        var @delegate = Substitute.For<Func<IDataReader, SqlQueryReaderOptions, SqlQueryReaderResult<object[]>>>();
        @delegate.WithAnyArgs( _ => expected );
        var reader = new SqlQueryReader<object[]>( new SqlDialect( "foo" ), @delegate );
        var sut = reader.Bind( sql );

        var result = sut.Execute( command );

        using ( new AssertionScope() )
        {
            @delegate.Verify().CallCount.Should().Be( 1 );
            command.CommandText.Should().BeSameAs( sql );
            result.Should().BeEquivalentTo( expected );
        }
    }
}
