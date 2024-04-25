using System.Collections.Generic;
using System.Data;
using LfrlAnvil.Sql.Statements;
using LfrlAnvil.TestExtensions.FluentAssertions;
using LfrlAnvil.TestExtensions.Sql.Mocks.System;

namespace LfrlAnvil.Sql.Tests.StatementsTests;

public class SqlParameterBinderTests : TestsBase
{
    [Fact]
    public void Bind_ForTypeErased_ShouldClearParameters_WhenSourceIsNull()
    {
        var command = new DbCommandMock();
        command.Parameters.Add( new DbParameterMock() );
        var dialect = new SqlDialect( "foo" );
        var @delegate = Substitute.For<Action<IDbCommand, IEnumerable<SqlParameter>>>();
        var sut = new SqlParameterBinder( dialect, @delegate );

        sut.Bind( command, source: null );

        using ( new AssertionScope() )
        {
            sut.Dialect.Should().BeSameAs( dialect );
            sut.Delegate.Should().BeSameAs( @delegate );
            @delegate.Verify().CallCount.Should().Be( 0 );
            command.Parameters.Should().BeEmpty();
        }
    }

    [Fact]
    public void Bind_ForTypeErased_ShouldInvokeDelegate_WhenSourceIsNotNull()
    {
        var command = new DbCommandMock();
        var dialect = new SqlDialect( "foo" );
        var @delegate = Substitute.For<Action<IDbCommand, IEnumerable<SqlParameter>>>();
        var source = new[] { SqlParameter.Named( "a", 0 ), SqlParameter.Named( "b", 1 ) };
        var sut = new SqlParameterBinder( dialect, @delegate );

        sut.Bind( command, source );

        using ( new AssertionScope() )
        {
            sut.Dialect.Should().BeSameAs( dialect );
            sut.Delegate.Should().BeSameAs( @delegate );
            @delegate.Verify().CallAt( 0 ).Exists().And.Arguments.Should().BeSequentiallyEqualTo( command, source );
        }
    }

    [Fact]
    public void Bind_ForGeneric_ShouldClearParameters_WhenSourceIsNull()
    {
        var command = new DbCommandMock();
        command.Parameters.Add( new DbParameterMock() );
        var dialect = new SqlDialect( "foo" );
        var @delegate = Substitute.For<Action<IDbCommand, string>>();
        var sut = new SqlParameterBinder<string>( dialect, @delegate );

        sut.Bind( command, source: null );

        using ( new AssertionScope() )
        {
            sut.Dialect.Should().BeSameAs( dialect );
            sut.Delegate.Should().BeSameAs( @delegate );
            @delegate.Verify().CallCount.Should().Be( 0 );
            command.Parameters.Should().BeEmpty();
        }
    }

    [Fact]
    public void Bind_ForGeneric_ShouldInvokeDelegate_WhenSourceIsNotNull()
    {
        var command = new DbCommandMock();
        var dialect = new SqlDialect( "foo" );
        var @delegate = Substitute.For<Action<IDbCommand, string>>();
        var source = "lorem";
        var sut = new SqlParameterBinder<string>( dialect, @delegate );

        sut.Bind( command, source );

        using ( new AssertionScope() )
        {
            sut.Dialect.Should().BeSameAs( dialect );
            sut.Delegate.Should().BeSameAs( @delegate );
            @delegate.Verify().CallAt( 0 ).Exists().And.Arguments.Should().BeSequentiallyEqualTo( command, source );
        }
    }
}
