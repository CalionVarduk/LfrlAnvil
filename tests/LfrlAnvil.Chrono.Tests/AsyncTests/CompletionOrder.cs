using System.Threading;

namespace LfrlAnvil.Chrono.Tests.AsyncTests;

public sealed class CompletionOrder
{
    private int _current;

    public int Next()
    {
        return Interlocked.Increment( ref _current );
    }
}
