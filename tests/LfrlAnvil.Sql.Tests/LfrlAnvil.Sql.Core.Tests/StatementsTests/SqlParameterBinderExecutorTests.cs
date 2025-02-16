using System.Collections.Generic;
using System.Data;
using LfrlAnvil.Sql.Statements;
using LfrlAnvil.TestExtensions.Sql.Mocks.System;

namespace LfrlAnvil.Sql.Tests.StatementsTests;

public class SqlParameterBinderExecutorTests : TestsBase
{
    [Fact]
    public void Bind_Extension_ForTypeErased_ShouldCreateCorrectExecutor()
    {
        var @delegate = Substitute.For<Action<IDbCommand, IEnumerable<SqlParameter>>>();
        var source = Substitute.For<IEnumerable<SqlParameter>>();
        var binder = new SqlParameterBinder( new SqlDialect( "foo" ), @delegate );
        var sut = binder.Bind( source );

        Assertion.All(
                sut.Source.TestRefEquals( source ),
                sut.Binder.TestEquals( binder ) )
            .Go();
    }

    [Fact]
    public void Execute_ForTypeErased_ShouldInvokeBinder()
    {
        var command = new DbCommandMock();
        var @delegate = Substitute.For<Action<IDbCommand, IEnumerable<SqlParameter>>>();
        var source = new[] { SqlParameter.Named( "a", 0 ), SqlParameter.Named( "b", 1 ) };
        var binder = new SqlParameterBinder( new SqlDialect( "foo" ), @delegate );
        var sut = binder.Bind( source );

        sut.Execute( command );

        @delegate.CallAt( 0 ).Arguments.TestSequence( [ command, source ] ).Go();
    }

    [Fact]
    public void Bind_Extension_ForGeneric_ShouldCreateCorrectExecutor()
    {
        var @delegate = Substitute.For<Action<IDbCommand, string>>();
        var source = Fixture.Create<string>();
        var binder = new SqlParameterBinder<string>( new SqlDialect( "foo" ), @delegate );
        var sut = binder.Bind( source );

        Assertion.All(
                sut.Source.TestRefEquals( source ),
                sut.Binder.TestEquals( binder ) )
            .Go();
    }

    [Fact]
    public void Execute_ForGeneric_ShouldInvokeBinder()
    {
        var command = new DbCommandMock();
        var @delegate = Substitute.For<Action<IDbCommand, string>>();
        var source = Fixture.Create<string>();
        var binder = new SqlParameterBinder<string>( new SqlDialect( "foo" ), @delegate );
        var sut = binder.Bind( source );

        sut.Execute( command );

        @delegate.CallAt( 0 ).Arguments.TestSequence( [ command, source ] ).Go();
    }
}
