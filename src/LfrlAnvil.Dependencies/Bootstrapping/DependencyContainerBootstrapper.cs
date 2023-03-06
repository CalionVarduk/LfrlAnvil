using System;
using System.Threading;
using LfrlAnvil.Dependencies.Exceptions;

namespace LfrlAnvil.Dependencies.Bootstrapping;

public abstract class DependencyContainerBootstrapper : IDependencyContainerBootstrapper<DependencyContainerBuilder>
{
    private int _state;

    public void Bootstrap(DependencyContainerBuilder builder)
    {
        if ( Interlocked.Exchange( ref _state, 1 ) == 1 )
            throw new InvalidOperationException( Resources.BootstrapperInvokedBeforeItCouldFinish );

        BootstrapInternal( builder );
        Interlocked.Decrement( ref _state );
    }

    protected abstract void BootstrapInternal(DependencyContainerBuilder builder);
}
