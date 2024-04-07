using System;
using System.Data;

namespace LfrlAnvil.Sql.Internal;

public readonly struct TemporaryCommandTimeout : IDisposable
{
    public TemporaryCommandTimeout(IDbCommand command, TimeSpan? timeout)
    {
        Command = command;
        PreviousTimeout = Command.CommandTimeout;
        if ( timeout is not null )
            Command.CommandTimeout = ( int )Math.Ceiling( timeout.Value.TotalSeconds );
    }

    public IDbCommand Command { get; }
    public int PreviousTimeout { get; }

    public void Dispose()
    {
        Command.CommandTimeout = PreviousTimeout;
    }
}
