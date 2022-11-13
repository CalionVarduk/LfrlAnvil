using System;

namespace LfrlAnvil.Dependencies.Internal;

internal readonly struct DependencyDisposer
{
    internal readonly object Dependency;
    internal readonly Action<object>? Callback;

    internal DependencyDisposer(object dependency, Action<object>? callback)
    {
        Dependency = dependency;
        Callback = callback;
    }

    internal Exception? TryDispose()
    {
        try
        {
            if ( Callback is not null )
                Callback( Dependency );
            else
                ReinterpretCast.To<IDisposable>( Dependency ).Dispose();
        }
        catch ( Exception exc )
        {
            return exc;
        }

        return null;
    }
}
