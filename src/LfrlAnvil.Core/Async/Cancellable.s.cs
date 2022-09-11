using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Threading;

namespace LfrlAnvil.Async;

public static class Cancellable
{
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Cancellable<T> Create<T>(T value, CancellationToken token)
    {
        return new Cancellable<T>( value, token );
    }
}
