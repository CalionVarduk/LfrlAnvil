using System.Diagnostics.Contracts;
using System.Threading;

namespace LfrlAnvil.Async;

public readonly struct Cancellable<T>
{
    public Cancellable(T value, CancellationToken token)
    {
        Value = value;
        Token = token;
    }

    public T Value { get; }
    public CancellationToken Token { get; }

    [Pure]
    public override string ToString()
    {
        return $"{nameof( Value )}: {Value}, {nameof( Token.IsCancellationRequested )}: {Token.IsCancellationRequested}";
    }
}
