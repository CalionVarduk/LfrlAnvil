using System.Collections.Generic;
using System.Data;
using LfrlAnvil.Sql.Statements;
using LfrlAnvil.Sql.Tests.Helpers.Data;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.Sql.Tests.StatementsTests;

public class SqlParameterBinderExecutorTests : TestsBase
{
    [Fact]
    public void Bind_Extension_ForTypeErased_ShouldCreateCorrectExecutor()
    {
        var @delegate = Substitute.For<Action<IDbCommand, IEnumerable<KeyValuePair<string, object?>>>>();
        var source = Substitute.For<IEnumerable<KeyValuePair<string, object?>>>();
        var binder = new SqlParameterBinder( new SqlDialect( "foo" ), @delegate );
        var sut = binder.Bind( source );

        using ( new AssertionScope() )
        {
            sut.Source.Should().BeSameAs( source );
            sut.Binder.Should().BeEquivalentTo( binder );
        }
    }

    [Fact]
    public void Execute_ForTypeErased_ShouldInvokeBinder()
    {
        var command = new DbCommand();
        var @delegate = Substitute.For<Action<IDbCommand, IEnumerable<KeyValuePair<string, object?>>>>();
        var source = new[] { KeyValuePair.Create( "a", (object?)0 ), KeyValuePair.Create( "b", (object?)1 ) };
        var binder = new SqlParameterBinder( new SqlDialect( "foo" ), @delegate );
        var sut = binder.Bind( source );

        sut.Execute( command );

        @delegate.Verify().CallAt( 0 ).Exists().And.Arguments.Should().BeSequentiallyEqualTo( command, source );
    }

    [Fact]
    public void Bind_Extension_ForGeneric_ShouldCreateCorrectExecutor()
    {
        var @delegate = Substitute.For<Action<IDbCommand, string>>();
        var source = Fixture.Create<string>();
        var binder = new SqlParameterBinder<string>( new SqlDialect( "foo" ), @delegate );
        var sut = binder.Bind( source );

        using ( new AssertionScope() )
        {
            sut.Source.Should().BeSameAs( source );
            sut.Binder.Should().BeEquivalentTo( binder );
        }
    }

    [Fact]
    public void Execute_ForGeneric_ShouldInvokeBinder()
    {
        var command = new DbCommand();
        var @delegate = Substitute.For<Action<IDbCommand, string>>();
        var source = Fixture.Create<string>();
        var binder = new SqlParameterBinder<string>( new SqlDialect( "foo" ), @delegate );
        var sut = binder.Bind( source );

        sut.Execute( command );

        @delegate.Verify().CallAt( 0 ).Exists().And.Arguments.Should().BeSequentiallyEqualTo( command, source );
    }
}
