using System;
using System.Threading.Tasks;

namespace LfrlAnvil.Async;

public sealed class CounterTask : IDisposable
{
    private readonly TaskCompletionSource _source;
    private int _count;

    public CounterTask(int limit, int count = 0)
    {
        _source = new TaskCompletionSource();
        _count = count;
        Limit = Math.Max( limit, 0 );

        if ( _count >= Limit )
            _source.SetResult();
    }

    public int Limit { get; }

    public int Count
    {
        get
        {
            using ( ExclusiveLock.Enter( _source ) )
                return _count;
        }
    }

    public Task Task => _source.Task;

    public void Dispose()
    {
        using ( ExclusiveLock.Enter( _source ) )
        {
            if ( ! _source.Task.IsCompleted )
                _source.SetCanceled();
        }
    }

    public bool Increment()
    {
        using ( ExclusiveLock.Enter( _source ) )
        {
            if ( _source.Task.IsCompleted )
                return true;

            if ( ++_count < Limit )
                return false;

            _source.SetResult();
            return true;
        }
    }

    public bool Add(int count)
    {
        if ( count <= 0 )
            return false;

        using ( ExclusiveLock.Enter( _source ) )
        {
            if ( _source.Task.IsCompleted )
                return true;

            _count = unchecked( ( int )Math.Min( unchecked( _count + ( long )count ), Limit ) );
            if ( _count < Limit )
                return false;

            _source.SetResult();
            return true;
        }
    }
}
