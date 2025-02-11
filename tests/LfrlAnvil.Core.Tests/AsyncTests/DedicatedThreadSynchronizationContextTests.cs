using System.Globalization;
using System.Threading;
using LfrlAnvil.Async;
using LfrlAnvil.Extensions;

namespace LfrlAnvil.Tests.AsyncTests;

public class DedicatedThreadSynchronizationContextTests : TestsBase
{
    [Fact]
    public void Ctor_ShouldCreateWithDefaultParameters()
    {
        var sut = new DedicatedThreadSynchronizationContext();

        Assertion.All(
                sut.IsActive.TestTrue(),
                sut.ThreadCulture.TestNotNull(),
                sut.ThreadUICulture.TestNotNull(),
                sut.ThreadName.TestNull(),
                sut.ThreadPriority.TestEquals( ThreadPriority.Normal ),
                sut.ThreadId.TestGreaterThan( 0 ) )
            .Go();
    }

    [Fact]
    public void Ctor_ShouldCreateWithCustomParameters()
    {
        var @params = new ThreadParams
        {
            Culture = CultureInfo.InvariantCulture,
            UICulture = CultureInfo.CurrentCulture,
            Name = Fixture.Create<string>(),
            Priority = Fixture.Create<ThreadPriority>()
        };

        var sut = new DedicatedThreadSynchronizationContext( @params );

        Assertion.All(
                sut.IsActive.TestTrue(),
                sut.ThreadCulture.TestRefEquals( @params.Culture ),
                sut.ThreadUICulture.TestRefEquals( @params.UICulture ),
                sut.ThreadName.TestEquals( @params.Name ),
                sut.ThreadPriority.ToNullable().TestEquals( @params.Priority ),
                sut.ThreadId.TestGreaterThan( 0 ) )
            .Go();
    }

    [Fact]
    public void Send_ShouldInvokeCallbackSynchronouslyOnTheUnderlyingThread()
    {
        object? capturedState = null;
        var capturedThreadId = -1;
        var state = new object();

        var sut = new DedicatedThreadSynchronizationContext();

        sut.Send(
            s =>
            {
                capturedState = s;
                capturedThreadId = Thread.CurrentThread.ManagedThreadId;
            },
            state );

        Assertion.All( capturedState.TestRefEquals( state ), capturedThreadId.TestEquals( sut.ThreadId ) ).Go();
    }

    [Fact]
    public void Post_ShouldInvokeCallbackAsynchronouslyOnTheUnderlyingThread()
    {
        object? capturedState = null;
        var capturedThreadId = -1;
        var state = new object();

        var sut = new DedicatedThreadSynchronizationContext();

        var reset = new ManualResetEvent( false );

        sut.Post(
            s =>
            {
                capturedState = s;
                capturedThreadId = Thread.CurrentThread.ManagedThreadId;
                reset.Set();
            },
            state );

        reset.WaitOne();

        Assertion.All( capturedState.TestRefEquals( state ), capturedThreadId.TestEquals( sut.ThreadId ) ).Go();
    }
}
