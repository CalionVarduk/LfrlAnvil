using System;
using System.Data;
using System.Diagnostics;
using System.Threading.Tasks;
using LfrlAnvil.Diagnostics;

namespace LfrlAnvil.Sql.Internal;

public readonly struct DbCommandDiagnoser<TCommand, TArgs>
    where TCommand : IDbCommand
{
    public DbCommandDiagnoser(
        Action<TCommand, TArgs>? beforeExecute = null,
        Action<TCommand, TArgs, TimeSpan, Exception?>? afterExecute = null)
    {
        BeforeExecute = beforeExecute;
        AfterExecute = afterExecute;
    }

    public Action<TCommand, TArgs>? BeforeExecute { get; }
    public Action<TCommand, TArgs, TimeSpan, Exception?>? AfterExecute { get; }

    public TResult Execute<TResult>(TCommand command, TArgs args, Func<TCommand, TResult> invoker)
    {
        TResult result;
        var start = Stopwatch.GetTimestamp();
        BeforeExecute?.Invoke( command, args );

        try
        {
            result = invoker( command );
        }
        catch ( Exception exc )
        {
            AfterExecute?.Invoke( command, args, StopwatchTimestamp.GetTimeSpan( start, Stopwatch.GetTimestamp() ), exc );
            throw;
        }

        AfterExecute?.Invoke( command, args, StopwatchTimestamp.GetTimeSpan( start, Stopwatch.GetTimestamp() ), null );
        return result;
    }

    public async ValueTask<TResult> ExecuteAsync<TResult>(TCommand command, TArgs args, Func<TCommand, ValueTask<TResult>> invoker)
    {
        TResult result;
        var start = Stopwatch.GetTimestamp();
        BeforeExecute?.Invoke( command, args );

        try
        {
            result = await invoker( command ).ConfigureAwait( false );
        }
        catch ( Exception exc )
        {
            AfterExecute?.Invoke( command, args, StopwatchTimestamp.GetTimeSpan( start, Stopwatch.GetTimestamp() ), exc );
            throw;
        }

        AfterExecute?.Invoke( command, args, StopwatchTimestamp.GetTimeSpan( start, Stopwatch.GetTimestamp() ), null );
        return result;
    }

    public async ValueTask<TResult> ExecuteAsync<TResult>(TCommand command, TArgs args, Func<TCommand, Task<TResult>> invoker)
    {
        TResult result;
        var start = Stopwatch.GetTimestamp();
        BeforeExecute?.Invoke( command, args );

        try
        {
            result = await invoker( command ).ConfigureAwait( false );
        }
        catch ( Exception exc )
        {
            AfterExecute?.Invoke( command, args, StopwatchTimestamp.GetTimeSpan( start, Stopwatch.GetTimestamp() ), exc );
            throw;
        }

        AfterExecute?.Invoke( command, args, StopwatchTimestamp.GetTimeSpan( start, Stopwatch.GetTimestamp() ), null );
        return result;
    }
}
