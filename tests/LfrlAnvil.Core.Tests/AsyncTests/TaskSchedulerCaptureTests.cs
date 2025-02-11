using System.Threading;
using System.Threading.Tasks;
using LfrlAnvil.Async;

namespace LfrlAnvil.Tests.AsyncTests;

public class TaskSchedulerCaptureTests : TestsBase
{
    [Fact]
    public void TryGetScheduler_ShouldReturnNullForDefault()
    {
        TaskSchedulerCapture sut = default;
        var result = sut.TryGetScheduler();
        result.TestNull().Go();
    }

    [Fact]
    public void TryGetScheduler_ShouldReturnNull_WhenStrategyIsNone()
    {
        var sut = new TaskSchedulerCapture( TaskSchedulerCaptureStrategy.None );
        var result = sut.TryGetScheduler();
        result.TestNull().Go();
    }

    [Fact]
    public void
        TryGetScheduler_ShouldReturnCurrentTaskScheduler_WhenStrategyIsCurrentAndCurrentSynchronizationContextAtTheMomentOfCaptureCreationIsNull()
    {
        SynchronizationContext.SetSynchronizationContext( null );
        var sut = new TaskSchedulerCapture( TaskSchedulerCaptureStrategy.Current );

        var result = sut.TryGetScheduler();

        result.TestRefEquals( TaskScheduler.Current ).Go();
    }

    [Fact]
    public void
        TryGetScheduler_ShouldReturnSchedulerFromCurrentSynchronizationContextAtTheMomentOfCaptureCreation_WhenStrategyIsCurrent()
    {
        var context = new SynchronizationContext();
        SynchronizationContext.SetSynchronizationContext( context );
        var sut = new TaskSchedulerCapture( TaskSchedulerCaptureStrategy.Current );

        var result = sut.TryGetScheduler();

        Assertion.All( result.TestNotNull(), result.TestNotRefEquals( TaskScheduler.Current ) ).Go();
    }

    [Fact]
    public void TryGetScheduler_ShouldReturnSchedulerFromSynchronizationContextAtTheMomentOfMethodCall_WhenStrategyIsLazy()
    {
        var context = new SynchronizationContext();
        var sut = new TaskSchedulerCapture( TaskSchedulerCaptureStrategy.Lazy );

        SynchronizationContext.SetSynchronizationContext( context );
        var result = sut.TryGetScheduler();

        Assertion.All( result.TestNotNull(), result.TestNotRefEquals( TaskScheduler.Current ) ).Go();
    }

    [Fact]
    public void
        TryGetScheduler_ShouldReturnCurrentTaskScheduler_WhenStrategyIsLazyAndCurrentSynchronizationContextAtTheMomentOfMethodCallIsNull()
    {
        var sut = new TaskSchedulerCapture( TaskSchedulerCaptureStrategy.Lazy );

        SynchronizationContext.SetSynchronizationContext( null );
        var result = sut.TryGetScheduler();

        result.TestRefEquals( TaskScheduler.Current ).Go();
    }

    [Fact]
    public void TryGetScheduler_ShouldReturnProvidedScheduler_WhenNoStrategyHasBeenUsed()
    {
        var scheduler = Substitute.ForPartsOf<TaskScheduler>();
        var sut = new TaskSchedulerCapture( scheduler );

        var result = sut.TryGetScheduler();

        result.TestRefEquals( scheduler ).Go();
    }

    [Fact]
    public void GetCurrentScheduler_ShouldReturnSchedulerFromSynchronizationContext()
    {
        var context = new SynchronizationContext();
        SynchronizationContext.SetSynchronizationContext( context );

        var result = TaskSchedulerCapture.GetCurrentScheduler();

        Assertion.All( result.TestNotNull(), result.TestNotRefEquals( TaskScheduler.Current ) ).Go();
    }

    [Fact]
    public void GetCurrentScheduler_ShouldReturnCurrentTaskScheduler_WhenCurrentSynchronizationContextIsNull()
    {
        SynchronizationContext.SetSynchronizationContext( null );
        var result = TaskSchedulerCapture.GetCurrentScheduler();
        result.TestRefEquals( TaskScheduler.Current ).Go();
    }

    [Fact]
    public void FromSynchronizationContext_ShouldReturnSchedulerFromProvidedSynchronizationContextSafely()
    {
        var previousContext = SynchronizationContext.Current;
        var context = new SynchronizationContext();

        var result = TaskSchedulerCapture.FromSynchronizationContext( context );

        Assertion.All(
                result.TestNotNull(),
                result.TestNotRefEquals( TaskScheduler.Current ),
                SynchronizationContext.Current.TestRefEquals( previousContext ) )
            .Go();
    }
}
