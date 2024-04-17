using System.Threading.Tasks;
using LfrlAnvil.Async;
using LfrlAnvil.Functional;

namespace LfrlAnvil.Tests.AsyncTests;

public class TaskRegistryTests : TestsBase
{
    [Fact]
    public void Ctor_ShouldReturnEmptyRegistry()
    {
        var sut = new TaskRegistry();
        sut.Count.Should().Be( 0 );
    }

    [Fact]
    public void Add_ShouldAddOngoingTaskToRegistry()
    {
        var taskSource = new TaskCompletionSource();
        var sut = new TaskRegistry();

        sut.Add( taskSource.Task );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 1 );
            sut.Contains( taskSource.Task ).Should().BeTrue();
        }
    }

    [Fact]
    public void Add_ShouldDoNothing_WhenTaskIsCompletedSuccessfully()
    {
        var taskSource = new TaskCompletionSource();
        taskSource.SetResult();
        var sut = new TaskRegistry();

        sut.Add( taskSource.Task );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 0 );
            sut.Contains( taskSource.Task ).Should().BeFalse();
        }
    }

    [Fact]
    public void Add_ShouldDoNothing_WhenTaskIsCancelled()
    {
        var taskSource = new TaskCompletionSource();
        taskSource.SetCanceled();
        var sut = new TaskRegistry();

        sut.Add( taskSource.Task );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 0 );
            sut.Contains( taskSource.Task ).Should().BeFalse();
        }
    }

    [Fact]
    public void Add_ShouldDoNothing_WhenTaskIsFailed()
    {
        var taskSource = new TaskCompletionSource();
        taskSource.SetException( new Exception() );
        var sut = new TaskRegistry();

        sut.Add( taskSource.Task );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 0 );
            sut.Contains( taskSource.Task ).Should().BeFalse();
        }
    }

    [Fact]
    public void Add_ShouldThrowObjectDisposedException_WhenRegistryIsDisposed()
    {
        var taskSource = new TaskCompletionSource();
        var sut = new TaskRegistry();
        sut.Dispose();

        var action = Lambda.Of( () => sut.Add( taskSource.Task ) );

        action.Should().ThrowExactly<ObjectDisposedException>();
    }

    [Fact]
    public void Add_ShouldThrowArgumentException_WhenTaskIsInCreatedState()
    {
        var task = new Task( () => { } );
        var sut = new TaskRegistry();

        var action = Lambda.Of( () => sut.Add( task ) );

        action.Should().ThrowExactly<ArgumentException>();
    }

    [Fact]
    public void TaskSuccessfulCompletion_ShouldRemoveTaskFromRegistry()
    {
        var taskSource = new TaskCompletionSource();
        var sut = new TaskRegistry();
        sut.Add( taskSource.Task );

        taskSource.SetResult();

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 0 );
            sut.Contains( taskSource.Task ).Should().BeFalse();
        }
    }

    [Fact]
    public void TaskFailure_ShouldRemoveTaskFromRegistry()
    {
        var taskSource = new TaskCompletionSource();
        var sut = new TaskRegistry();
        sut.Add( taskSource.Task );

        taskSource.SetException( new Exception() );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 0 );
            sut.Contains( taskSource.Task ).Should().BeFalse();
        }
    }

    [Fact]
    public void TaskCancellation_ShouldRemoveTaskFromRegistry()
    {
        var taskSource = new TaskCompletionSource();
        var sut = new TaskRegistry();
        sut.Add( taskSource.Task );

        taskSource.SetCanceled();

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 0 );
            sut.Contains( taskSource.Task ).Should().BeFalse();
        }
    }

    [Fact]
    public void WaitForAll_ShouldReturnTaskThatCompletesWhenAllRegisteredTasksComplete()
    {
        var taskSources = new[] { new TaskCompletionSource(), new TaskCompletionSource(), new TaskCompletionSource() };
        var sut = new TaskRegistry();
        foreach ( var source in taskSources )
            sut.Add( source.Task );

        var result = sut.WaitForAll();
        var isCompleted1 = result.IsCompleted;
        taskSources[0].SetResult();
        var isCompleted2 = result.IsCompleted;
        taskSources[1].SetResult();
        var isCompleted3 = result.IsCompleted;
        taskSources[2].SetResult();
        var isCompleted4 = result.IsCompleted;

        using ( new AssertionScope() )
        {
            isCompleted1.Should().BeFalse();
            isCompleted2.Should().BeFalse();
            isCompleted3.Should().BeFalse();
            isCompleted4.Should().BeTrue();
        }
    }

    [Fact]
    public void Dispose_ShouldWaitForAllRegisteredTasksToComplete()
    {
        var taskSources = new[] { new TaskCompletionSource(), new TaskCompletionSource(), new TaskCompletionSource() };
        var sut = new TaskRegistry();
        foreach ( var source in taskSources )
            sut.Add( source.Task );

        Task.Run(
            async () =>
            {
                await Task.Delay( 1 );
                taskSources[0].SetResult();
                await Task.Delay( 1 );
                taskSources[1].SetResult();
                await Task.Delay( 1 );
                taskSources[2].SetResult();
            } );

        sut.Dispose();

        using ( new AssertionScope() )
        {
            taskSources.Should().OnlyContain( s => s.Task.IsCompleted );
            sut.Count.Should().Be( 0 );
        }
    }

    [Fact]
    public void Dispose_ShouldNotThrow_WhenSomeTasksGetCancelled()
    {
        var taskSources = new[] { new TaskCompletionSource(), new TaskCompletionSource(), new TaskCompletionSource() };
        var sut = new TaskRegistry();
        foreach ( var source in taskSources )
            sut.Add( source.Task );

        Task.Run(
            async () =>
            {
                await Task.Delay( 1 );
                taskSources[0].SetResult();
                await Task.Delay( 1 );
                taskSources[1].SetCanceled();
                await Task.Delay( 1 );
                taskSources[2].SetResult();
            } );

        sut.Dispose();

        using ( new AssertionScope() )
        {
            taskSources.Should().OnlyContain( s => s.Task.IsCompleted );
            sut.Count.Should().Be( 0 );
        }
    }

    [Fact]
    public void Dispose_ShouldThrow_WhenSomeTasksFail()
    {
        var taskSources = new[] { new TaskCompletionSource(), new TaskCompletionSource(), new TaskCompletionSource() };
        var sut = new TaskRegistry();
        foreach ( var source in taskSources )
            sut.Add( source.Task );

        Task.Run(
            async () =>
            {
                await Task.Delay( 1 );
                taskSources[0].SetResult();
                await Task.Delay( 1 );
                taskSources[1].SetException( new Exception() );
                await Task.Delay( 1 );
                taskSources[2].SetResult();
            } );

        var action = Lambda.Of( () => sut.Dispose() );

        using ( new AssertionScope() )
        {
            action.Should().ThrowExactly<AggregateException>();
            taskSources.Should().OnlyContain( s => s.Task.IsCompleted );
            sut.Count.Should().Be( 0 );
        }
    }

    [Fact]
    public void Dispose_ShouldDoNothing_WhenRegistryIsAlreadyDisposed()
    {
        var sut = new TaskRegistry();
        sut.Dispose();

        var action = Lambda.Of( () => sut.Dispose() );

        action.Should().NotThrow();
    }
}
