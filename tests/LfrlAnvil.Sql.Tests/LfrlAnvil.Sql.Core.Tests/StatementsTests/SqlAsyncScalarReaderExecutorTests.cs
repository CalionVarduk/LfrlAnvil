﻿using System.Data;
using System.Threading;
using System.Threading.Tasks;
using LfrlAnvil.Sql.Statements;
using LfrlAnvil.TestExtensions.FluentAssertions;
using LfrlAnvil.TestExtensions.NSubstitute;
using LfrlAnvil.TestExtensions.Sql.Mocks.System;

namespace LfrlAnvil.Sql.Tests.StatementsTests;

public class SqlAsyncScalarReaderExecutorTests : TestsBase
{
    [Fact]
    public void Bind_Extension_ForTypeErased_ShouldCreateCorrectExecutor()
    {
        var sql = "SELECT * FROM foo";
        var @delegate = Substitute.For<Func<IDataReader, CancellationToken, ValueTask<SqlScalarResult>>>();
        var reader = new SqlAsyncScalarReader( new SqlDialect( "foo" ), @delegate );
        var sut = reader.Bind( sql );

        using ( new AssertionScope() )
        {
            sut.Sql.Should().BeSameAs( sql );
            sut.Reader.Should().BeEquivalentTo( reader );
        }
    }

    [Fact]
    public async Task ExecuteAsync_ForTypeErased_ShouldSetCommandTextAndInvokeDelegate()
    {
        var expected = new SqlScalarResult( "bar" );

        var sql = "SELECT * FROM foo";
        var command = new DbCommandMock();
        var @delegate = Substitute.For<Func<IDataReader, CancellationToken, ValueTask<SqlScalarResult>>>();
        @delegate.WithAnyArgs( _ => ValueTask.FromResult( expected ) );
        var reader = new SqlAsyncScalarReader( new SqlDialect( "foo" ), @delegate );
        var sut = reader.Bind( sql );

        var result = await sut.ExecuteAsync( command );

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
        var @delegate = Substitute.For<Func<IDataReader, CancellationToken, ValueTask<SqlScalarResult<int>>>>();
        var reader = new SqlAsyncScalarReader<int>( new SqlDialect( "foo" ), @delegate );
        var sut = reader.Bind( sql );

        using ( new AssertionScope() )
        {
            sut.Sql.Should().BeSameAs( sql );
            sut.Reader.Should().BeEquivalentTo( reader );
        }
    }

    [Fact]
    public async Task ExecuteAsync_ForGeneric_ShouldSetCommandTextAndInvokeDelegate()
    {
        var expected = new SqlScalarResult<int>( 42 );

        var sql = "SELECT * FROM foo";
        var command = new DbCommandMock();
        var @delegate = Substitute.For<Func<IDataReader, CancellationToken, ValueTask<SqlScalarResult<int>>>>();
        @delegate.WithAnyArgs( _ => ValueTask.FromResult( expected ) );
        var reader = new SqlAsyncScalarReader<int>( new SqlDialect( "foo" ), @delegate );
        var sut = reader.Bind( sql );

        var result = await sut.ExecuteAsync( command );

        using ( new AssertionScope() )
        {
            @delegate.Verify().CallCount.Should().Be( 1 );
            command.CommandText.Should().BeSameAs( sql );
            result.Should().BeEquivalentTo( expected );
        }
    }
}
