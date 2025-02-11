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
        sut.Count.TestEquals( 0 ).Go();
    }

    [Fact]
    public void Add_ShouldAddOngoingTaskToRegistry()
    {
        var taskSource = new TaskCompletionSource();
        var sut = new TaskRegistry();

        sut.Add( taskSource.Task );

        Assertion.All(
                sut.Count.TestEquals( 1 ),
                sut.Contains( taskSource.Task ).TestTrue() )
            .Go();
    }

    [Fact]
    public void Add_ShouldDoNothing_WhenTaskIsCompletedSuccessfully()
    {
        var taskSource = new TaskCompletionSource();
        taskSource.SetResult();
        var sut = new TaskRegistry();

        sut.Add( taskSource.Task );

        Assertion.All(
                sut.Count.TestEquals( 0 ),
                sut.Contains( taskSource.Task ).TestFalse() )
            .Go();
    }

    [Fact]
    public void Add_ShouldDoNothing_WhenTaskIsCancelled()
    {
        var taskSource = new TaskCompletionSource();
        taskSource.SetCanceled();
        var sut = new TaskRegistry();

        sut.Add( taskSource.Task );

        Assertion.All(
                sut.Count.TestEquals( 0 ),
                sut.Contains( taskSource.Task ).TestFalse() )
            .Go();
    }

    [Fact]
    public void Add_ShouldDoNothing_WhenTaskIsFailed()
    {
        var taskSource = new TaskCompletionSource();
        taskSource.SetException( new Exception() );
        var sut = new TaskRegistry();

        sut.Add( taskSource.Task );

        Assertion.All(
                sut.Count.TestEquals( 0 ),
                sut.Contains( taskSource.Task ).TestFalse() )
            .Go();
    }

    [Fact]
    public void Add_ShouldThrowObjectDisposedException_WhenRegistryIsDisposed()
    {
        var taskSource = new TaskCompletionSource();
        var sut = new TaskRegistry();
        sut.Dispose();

        var action = Lambda.Of( () => sut.Add( taskSource.Task ) );

        action.Test( exc => exc.TestType().Exact<ObjectDisposedException>() ).Go();
    }

    [Fact]
    public void Add_ShouldThrowArgumentException_WhenTaskIsInCreatedState()
    {
        var task = new Task( () => { } );
        var sut = new TaskRegistry();

        var action = Lambda.Of( () => sut.Add( task ) );

        action.Test( exc => exc.TestType().Exact<ArgumentException>() ).Go();
    }

    [Fact]
    public void TaskSuccessfulCompletion_ShouldRemoveTaskFromRegistry()
    {
        var taskSource = new TaskCompletionSource();
        var sut = new TaskRegistry();
        sut.Add( taskSource.Task );

        taskSource.SetResult();

        Assertion.All(
                sut.Count.TestEquals( 0 ),
                sut.Contains( taskSource.Task ).TestFalse() )
            .Go();
    }

    [Fact]
    public void TaskFailure_ShouldRemoveTaskFromRegistry()
    {
        var taskSource = new TaskCompletionSource();
        var sut = new TaskRegistry();
        sut.Add( taskSource.Task );

        taskSource.SetException( new Exception() );

        Assertion.All(
                sut.Count.TestEquals( 0 ),
                sut.Contains( taskSource.Task ).TestFalse() )
            .Go();
    }

    [Fact]
    public void TaskCancellation_ShouldRemoveTaskFromRegistry()
    {
        var taskSource = new TaskCompletionSource();
        var sut = new TaskRegistry();
        sut.Add( taskSource.Task );

        taskSource.SetCanceled();

        Assertion.All(
                sut.Count.TestEquals( 0 ),
                sut.Contains( taskSource.Task ).TestFalse() )
            .Go();
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

        Assertion.All(
                isCompleted1.TestFalse(),
                isCompleted2.TestFalse(),
                isCompleted3.TestFalse(),
                isCompleted4.TestTrue() )
            .Go();
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

        Assertion.All(
                taskSources.TestAll( (s, _) => s.Task.IsCompleted.TestTrue() ),
                sut.Count.TestEquals( 0 ) )
            .Go();
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

        Assertion.All(
                taskSources.TestAll( (s, _) => s.Task.IsCompleted.TestTrue() ),
                sut.Count.TestEquals( 0 ) )
            .Go();
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

        action.Test(
                exc => Assertion.All(
                    exc.TestType().Exact<AggregateException>(),
                    taskSources.TestAll( (s, _) => s.Task.IsCompleted.TestTrue() ),
                    sut.Count.TestEquals( 0 ) ) )
            .Go();
    }

    [Fact]
    public void Dispose_ShouldDoNothing_WhenRegistryIsAlreadyDisposed()
    {
        var sut = new TaskRegistry();
        sut.Dispose();

        var action = Lambda.Of( () => sut.Dispose() );

        action.Test( exc => exc.TestNull() ).Go();
    }
}
