using System.Threading.Tasks;
using LfrlAnvil.Chrono.Async;

namespace LfrlAnvil.MessageBroker.Core.Tests.Helpers;

public sealed class SharedResourceFixture : IAsyncDisposable
{
    public SharedResourceFixture()
    {
        DelaySource = ValueTaskDelaySource.Start();
    }

    public ValueTaskDelaySource DelaySource { get; }

    public ValueTask DisposeAsync()
    {
        return DelaySource.DisposeAsync();
    }
}
