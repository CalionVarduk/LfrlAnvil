using System.Linq;
using System.Threading.Tasks;
using LfrlAnvil.Functional;
using LfrlAnvil.Sql.Internal;
using LfrlAnvil.TestExtensions.FluentAssertions;
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

        using ( new AssertionScope() )
        {
            result.Should().Be( expected );
            invoker.Verify().CallAt( 0 ).Exists().And.Arguments.Should().BeSequentiallyEqualTo( command );
            beforeExecute.Verify().CallAt( 0 ).Exists().And.Arguments.Should().BeSequentiallyEqualTo( command, args );
            var afterArgs = afterExecute.Verify().CallAt( 0 ).Exists().And.Arguments;
            afterArgs.ElementAtOrDefault( 0 ).Should().BeSameAs( command );
            afterArgs.ElementAtOrDefault( 1 ).Should().BeSameAs( args );
            ((TimeSpan?)afterArgs.ElementAtOrDefault( 2 )).Should().BeGreaterOrEqualTo( TimeSpan.Zero );
            afterArgs.ElementAtOrDefault( 3 ).Should().BeNull();
        }
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

        using ( new AssertionScope() )
        {
            action.Should().ThrowExactly<Exception>().And.Should().BeSameAs( exception );
            invoker.Verify().CallAt( 0 ).Exists().And.Arguments.Should().BeSequentiallyEqualTo( command );
            beforeExecute.Verify().CallAt( 0 ).Exists().And.Arguments.Should().BeSequentiallyEqualTo( command, args );
            var afterArgs = afterExecute.Verify().CallAt( 0 ).Exists().And.Arguments;
            afterArgs.ElementAtOrDefault( 0 ).Should().BeSameAs( command );
            afterArgs.ElementAtOrDefault( 1 ).Should().BeSameAs( args );
            ((TimeSpan?)afterArgs.ElementAtOrDefault( 2 )).Should().BeGreaterOrEqualTo( TimeSpan.Zero );
            afterArgs.ElementAtOrDefault( 3 ).Should().BeSameAs( exception );
        }
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

        using ( new AssertionScope() )
        {
            result.Should().Be( expected );
            invoker.Verify().CallAt( 0 ).Exists().And.Arguments.Should().BeSequentiallyEqualTo( command );
            beforeExecute.Verify().CallAt( 0 ).Exists().And.Arguments.Should().BeSequentiallyEqualTo( command, args );
            var afterArgs = afterExecute.Verify().CallAt( 0 ).Exists().And.Arguments;
            afterArgs.ElementAtOrDefault( 0 ).Should().BeSameAs( command );
            afterArgs.ElementAtOrDefault( 1 ).Should().BeSameAs( args );
            ((TimeSpan?)afterArgs.ElementAtOrDefault( 2 )).Should().BeGreaterOrEqualTo( TimeSpan.Zero );
            afterArgs.ElementAtOrDefault( 3 ).Should().BeNull();
        }
    }

    [Fact]
    public async Task ExecuteAsync_WithValueTask_ShouldInvokeCallbacksAndThrow_WhenInvokerThrows()
    {
        var exception = new Exception();
        var command = new DbCommandMock();
        var args = Fixture.Create<string>();
        var invoker = Substitute.For<Func<DbCommandMock, ValueTask<int>>>();
        invoker.Invoke( command ).Returns( ValueTask.FromException<int>( exception ) );
        var beforeExecute = Substitute.For<Action<DbCommandMock, string>>();
        var afterExecute = Substitute.For<Action<DbCommandMock, string, TimeSpan, Exception?>>();
        var sut = new DbCommandDiagnoser<DbCommandMock, string>( beforeExecute, afterExecute );

        Exception? caughtException = null;
        try
        {
            await sut.ExecuteAsync( command, args, invoker );
        }
        catch ( Exception exc )
        {
            caughtException = exc;
        }

        using ( new AssertionScope() )
        {
            caughtException.Should().BeSameAs( exception );
            invoker.Verify().CallAt( 0 ).Exists().And.Arguments.Should().BeSequentiallyEqualTo( command );
            beforeExecute.Verify().CallAt( 0 ).Exists().And.Arguments.Should().BeSequentiallyEqualTo( command, args );
            var afterArgs = afterExecute.Verify().CallAt( 0 ).Exists().And.Arguments;
            afterArgs.ElementAtOrDefault( 0 ).Should().BeSameAs( command );
            afterArgs.ElementAtOrDefault( 1 ).Should().BeSameAs( args );
            ((TimeSpan?)afterArgs.ElementAtOrDefault( 2 )).Should().BeGreaterOrEqualTo( TimeSpan.Zero );
            afterArgs.ElementAtOrDefault( 3 ).Should().BeSameAs( exception );
        }
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

        using ( new AssertionScope() )
        {
            result.Should().Be( expected );
            invoker.Verify().CallAt( 0 ).Exists().And.Arguments.Should().BeSequentiallyEqualTo( command );
            beforeExecute.Verify().CallAt( 0 ).Exists().And.Arguments.Should().BeSequentiallyEqualTo( command, args );
            var afterArgs = afterExecute.Verify().CallAt( 0 ).Exists().And.Arguments;
            afterArgs.ElementAtOrDefault( 0 ).Should().BeSameAs( command );
            afterArgs.ElementAtOrDefault( 1 ).Should().BeSameAs( args );
            ((TimeSpan?)afterArgs.ElementAtOrDefault( 2 )).Should().BeGreaterOrEqualTo( TimeSpan.Zero );
            afterArgs.ElementAtOrDefault( 3 ).Should().BeNull();
        }
    }

    [Fact]
    public async Task ExecuteAsync_WithTask_ShouldInvokeCallbacksAndThrow_WhenInvokerThrows()
    {
        var exception = new Exception();
        var command = new DbCommandMock();
        var args = Fixture.Create<string>();
        var invoker = Substitute.For<Func<DbCommandMock, Task<int>>>();
        invoker.Invoke( command ).Returns( Task.FromException<int>( exception ) );
        var beforeExecute = Substitute.For<Action<DbCommandMock, string>>();
        var afterExecute = Substitute.For<Action<DbCommandMock, string, TimeSpan, Exception?>>();
        var sut = new DbCommandDiagnoser<DbCommandMock, string>( beforeExecute, afterExecute );

        Exception? caughtException = null;
        try
        {
            await sut.ExecuteAsync( command, args, invoker );
        }
        catch ( Exception exc )
        {
            caughtException = exc;
        }

        using ( new AssertionScope() )
        {
            caughtException.Should().BeSameAs( exception );
            invoker.Verify().CallAt( 0 ).Exists().And.Arguments.Should().BeSequentiallyEqualTo( command );
            beforeExecute.Verify().CallAt( 0 ).Exists().And.Arguments.Should().BeSequentiallyEqualTo( command, args );
            var afterArgs = afterExecute.Verify().CallAt( 0 ).Exists().And.Arguments;
            afterArgs.ElementAtOrDefault( 0 ).Should().BeSameAs( command );
            afterArgs.ElementAtOrDefault( 1 ).Should().BeSameAs( args );
            ((TimeSpan?)afterArgs.ElementAtOrDefault( 2 )).Should().BeGreaterOrEqualTo( TimeSpan.Zero );
            afterArgs.ElementAtOrDefault( 3 ).Should().BeSameAs( exception );
        }
    }
}
