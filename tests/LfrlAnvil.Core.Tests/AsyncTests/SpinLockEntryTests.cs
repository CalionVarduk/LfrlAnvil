using System.Threading;
using LfrlAnvil.Async;

namespace LfrlAnvil.Tests.AsyncTests;

public class SpinLockEntryTests : TestsBase
{
    [Fact]
    public void Enter_ShouldAcquireLock()
    {
        var @lock = new SpinLock();
        _ = SpinLockEntry.Enter( ref @lock );

        var hasLock = @lock.IsHeldByCurrentThread;

        hasLock.TestTrue().Go();
    }

    [Fact]
    public void Dispose_ShouldReleaseLock()
    {
        var @lock = new SpinLock();
        var sut = SpinLockEntry.Enter( ref @lock );

        sut.Dispose();
        var hasLock = @lock.IsHeldByCurrentThread;

        hasLock.TestFalse().Go();
    }

    [Fact]
    public void Dispose_ShouldNotThrowForDefault()
    {
        var sut = default( SpinLockEntry );
        Exception? exception = null;
        try
        {
            sut.Dispose();
        }
        catch ( Exception exc )
        {
            exception = exc;
        }

        exception.TestNull().Go();
    }
}
