using System.Threading.Tasks;
using LfrlAnvil.Chrono.Async;

namespace LfrlAnvil.MessageBroker.Client.Tests.Helpers;

public sealed class SharedClientResourceFixture : IAsyncDisposable
{
    public SharedClientResourceFixture()
    {
        DelaySource = ValueTaskDelaySource.Start();
    }

    public ValueTaskDelaySource DelaySource { get; }

    public ValueTask DisposeAsync()
    {
        return DelaySource.DisposeAsync();
    }
}
