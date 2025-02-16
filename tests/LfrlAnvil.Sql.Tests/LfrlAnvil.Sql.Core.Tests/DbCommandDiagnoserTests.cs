using System.Threading.Tasks;
using LfrlAnvil.Functional;
using LfrlAnvil.Sql.Internal;
using LfrlAnvil.TestExtensions.Sql.Mocks.System;

namespace LfrlAnvil.Sql.Tests;

public class DbCommandDiagnoserTests : TestsBase
{
    [Fact]
    public void Execute_ShouldInvokeCallbacksAndReturnInvocationResult()
    {
        var command = new DbCommandMock();
        var args = Fixture.Create<string>();
        var expected = Fixture.Create<int>();
        var invoker = Substitute.For<Func<DbCommandMock, int>>();
        invoker.Invoke( command ).Returns( expected );
        var beforeExecute = Substitute.For<Action<DbCommandMock, string>>();
        var afterExecute = Substitute.For<Action<DbCommandMock, string, TimeSpan, Exception?>>();
        var sut = new DbCommandDiagnoser<DbCommandMock, string>( beforeExecute, afterExecute );

        var result = sut.Execute( command, args, invoker );

        Assertion.All(
                result.TestEquals( expected ),
                invoker.CallAt( 0 ).Arguments.TestSequence( [ command ] ),
                beforeExecute.CallAt( 0 ).Arguments.TestSequence( [ command, args ] ),
                afterExecute.CallAt( 0 )
                    .Arguments.TestSequence(
                    [
                        (a, _) => a.TestRefEquals( command ),
                        (a, _) => a.TestRefEquals( args ),
                        (a, _) => a.TestType().Exact<TimeSpan>( t => t.TestGreaterThanOrEqualTo( TimeSpan.Zero ) ),
                        (a, _) => a.TestNull()
                    ] ) )
            .Go();
    }

    [Fact]
    public void Execute_ShouldInvokeCallbacksAndThrow_WhenInvokerThrows()
    {
        var exception = new Exception();
        var command = new DbCommandMock();
        var args = Fixture.Create<string>();
        var invoker = Substitute.For<Func<DbCommandMock, int>>();
        invoker.Invoke( command ).Returns( _ => throw exception );
        var beforeExecute = Substitute.For<Action<DbCommandMock, string>>();
        var afterExecute = Substitute.For<Action<DbCommandMock, string, TimeSpan, Exception?>>();
        var sut = new DbCommandDiagnoser<DbCommandMock, string>( beforeExecute, afterExecute );

        var action = Lambda.Of( () => sut.Execute( command, args, invoker ) );

        action.Test(
                exc => Assertion.All(
                    exc.TestRefEquals( exception ),
                    invoker.CallAt( 0 ).Arguments.TestSequence( [ command ] ),
                    beforeExecute.CallAt( 0 ).Arguments.TestSequence( [ command, args ] ),
                    afterExecute.CallAt( 0 )
                        .Arguments.TestSequence(
                        [
                            (a, _) => a.TestRefEquals( command ),
                            (a, _) => a.TestRefEquals( args ),
                            (a, _) => a.TestType().Exact<TimeSpan>( t => t.TestGreaterThanOrEqualTo( TimeSpan.Zero ) ),
                            (a, _) => a.TestRefEquals( exception )
                        ] ) ) )
            .Go();
    }

    [Fact]
    public async Task ExecuteAsync_WithValueTask_ShouldInvokeCallbacksAndReturnInvocationResult()
    {
        var command = new DbCommandMock();
        var args = Fixture.Create<string>();
        var expected = Fixture.Create<int>();
        var invoker = Substitute.For<Func<DbCommandMock, ValueTask<int>>>();
        invoker.Invoke( command ).Returns( ValueTask.FromResult( expected ) );
        var beforeExecute = Substitute.For<Action<DbCommandMock, string>>();
        var afterExecute = Substitute.For<Action<DbCommandMock, string, TimeSpan, Exception?>>();
        var sut = new DbCommandDiagnoser<DbCommandMock, string>( beforeExecute, afterExecute );

        var result = await sut.ExecuteAsync( command, args, invoker );

        Assertion.All(
                result.TestEquals( expected ),
                invoker.CallAt( 0 ).Arguments.TestSequence( [ command ] ),
                beforeExecute.CallAt( 0 ).Arguments.TestSequence( [ command, args ] ),
                afterExecute.CallAt( 0 )
                    .Arguments.TestSequence(
                    [
                        (a, _) => a.TestRefEquals( command ),
                        (a, _) => a.TestRefEquals( args ),
                        (a, _) => a.TestType().Exact<TimeSpan>( t => t.TestGreaterThanOrEqualTo( TimeSpan.Zero ) ),
                        (a, _) => a.TestNull()
                    ] ) )
            .Go();
    }

    [Fact]
    public void ExecuteAsync_WithValueTask_ShouldInvokeCallbacksAndThrow_WhenInvokerThrows()
    {
        var exception = new Exception();
        var command = new DbCommandMock();
        var args = Fixture.Create<string>();
        var invoker = Substitute.For<Func<DbCommandMock, ValueTask<int>>>();
        invoker.Invoke( command ).Returns( ValueTask.FromException<int>( exception ) );
        var beforeExecute = Substitute.For<Action<DbCommandMock, string>>();
        var afterExecute = Substitute.For<Action<DbCommandMock, string, TimeSpan, Exception?>>();
        var sut = new DbCommandDiagnoser<DbCommandMock, string>( beforeExecute, afterExecute );

        var action = Lambda.Of( async () => await sut.ExecuteAsync( command, args, invoker ) );

        action.Test(
                exc => Assertion.All(
                    exc.TestRefEquals( exception ),
                    invoker.CallAt( 0 ).Arguments.TestSequence( [ command ] ),
                    beforeExecute.CallAt( 0 ).Arguments.TestSequence( [ command, args ] ),
                    afterExecute.CallAt( 0 )
                        .Arguments.TestSequence(
                        [
                            (a, _) => a.TestRefEquals( command ),
                            (a, _) => a.TestRefEquals( args ),
                            (a, _) => a.TestType().Exact<TimeSpan>( t => t.TestGreaterThanOrEqualTo( TimeSpan.Zero ) ),
                            (a, _) => a.TestRefEquals( exception )
                        ] ) ) )
            .Go();
    }

    [Fact]
    public async Task ExecuteAsync_WithTask_ShouldInvokeCallbacksAndReturnInvocationResult()
    {
        var command = new DbCommandMock();
        var args = Fixture.Create<string>();
        var expected = Fixture.Create<int>();
        var invoker = Substitute.For<Func<DbCommandMock, Task<int>>>();
        invoker.Invoke( command ).Returns( Task.FromResult( expected ) );
        var beforeExecute = Substitute.For<Action<DbCommandMock, string>>();
        var afterExecute = Substitute.For<Action<DbCommandMock, string, TimeSpan, Exception?>>();
        var sut = new DbCommandDiagnoser<DbCommandMock, string>( beforeExecute, afterExecute );

        var result = await sut.ExecuteAsync( command, args, invoker );

        Assertion.All(
                result.TestEquals( expected ),
                invoker.CallAt( 0 ).Arguments.TestSequence( [ command ] ),
                beforeExecute.CallAt( 0 ).Arguments.TestSequence( [ command, args ] ),
                afterExecute.CallAt( 0 )
                    .Arguments.TestSequence(
                    [
                        (a, _) => a.TestRefEquals( command ),
                        (a, _) => a.TestRefEquals( args ),
                        (a, _) => a.TestType().Exact<TimeSpan>( t => t.TestGreaterThanOrEqualTo( TimeSpan.Zero ) ),
                        (a, _) => a.TestNull()
                    ] ) )
            .Go();
    }

    [Fact]
    public void ExecuteAsync_WithTask_ShouldInvokeCallbacksAndThrow_WhenInvokerThrows()
    {
        var exception = new Exception();
        var command = new DbCommandMock();
        var args = Fixture.Create<string>();
        var invoker = Substitute.For<Func<DbCommandMock, Task<int>>>();
        invoker.Invoke( command ).Returns( Task.FromException<int>( exception ) );
        var beforeExecute = Substitute.For<Action<DbCommandMock, string>>();
        var afterExecute = Substitute.For<Action<DbCommandMock, string, TimeSpan, Exception?>>();
        var sut = new DbCommandDiagnoser<DbCommandMock, string>( beforeExecute, afterExecute );

        var action = Lambda.Of( async () => await sut.ExecuteAsync( command, args, invoker ) );

        action.Test(
                exc => Assertion.All(
                    exc.TestRefEquals( exception ),
                    invoker.CallAt( 0 ).Arguments.TestSequence( [ command ] ),
                    beforeExecute.CallAt( 0 ).Arguments.TestSequence( [ command, args ] ),
                    afterExecute.CallAt( 0 )
                        .Arguments.TestSequence(
                        [
                            (a, _) => a.TestRefEquals( command ),
                            (a, _) => a.TestRefEquals( args ),
                            (a, _) => a.TestType().Exact<TimeSpan>( t => t.TestGreaterThanOrEqualTo( TimeSpan.Zero ) ),
                            (a, _) => a.TestRefEquals( exception )
                        ] ) ) )
            .Go();
    }
}
